//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Core;
using System.Collections.Generic;
using System;

namespace Landis.Extension.Scrapple
{
    public class EventRegion
    {

        private static double siteWindSpeed;
        private static double siteWindDirection;

        public static List<Location> DurationBasedSize(ActiveSite site, double ROS)
        {
            List<Location> newFireList = new List<Location>(0);
            newFireList.Add(site.Location);  //Add ignition site first.

            return newFireList;
        }
        //---------------------------------------------------------------------
        //-----Edited by BRM-----
        //-----To pass fireSizeType
        public static List<Site> SizeFireCostSurface(Event fireEvent, SizeType fireSizeType, bool buildUp)
        //----------
        {

            Site initiationSite = PlugIn.ModelCore.Landscape.GetSite(fireEvent.StartLocation);

            if (!initiationSite.IsActive)
                throw new ApplicationException("fire initiation site is not active.");

            // Inflate the area or duration to give a cushion for determining optimal path:
            double AdjMaxFireParameter = fireEvent.MaxFireParameter * 2.0;


            List<Site> newFireList = new List<Site>();
            SiteVars.TravelTime[initiationSite] = 0.0;
            SiteVars.MinNeighborTravelTime[initiationSite] = 0.0;
            SiteVars.Event[initiationSite] = fireEvent;

            //Assign data to the initiation site:
            double initiationTravelTime = CalculateTravelTime(initiationSite, initiationSite, fireEvent, buildUp);
            SiteVars.TravelTime[initiationSite] = initiationTravelTime;
            SiteVars.Event[initiationSite] = fireEvent;

            //Create a list of neighboring sites to which the fire will spread:
            List<WeightedSite> sitesToConsider = new List<WeightedSite>(0);

            sitesToConsider.Add(new WeightedSite(initiationSite, initiationTravelTime));

            double totalArea = 0.0;
            double cellArea = (PlugIn.ModelCore.CellLength * PlugIn.ModelCore.CellLength) / 10000; //convert to ha
            double cnt = 1;
            double oldcnt = cnt;

            while (sitesToConsider.Count > 0)
            {

                // Sort, but not every time.  Also, only sort if the fire is growing.
                if(oldcnt < cnt &&
                    (sitesToConsider.Count < 10
                    || (sitesToConsider.Count < 15 && cnt%5 == 0)
                    || (sitesToConsider.Count < 100 && cnt%20 == 0)
                    || cnt%40 == 0))
                    sitesToConsider.Sort(new WeightComparer());

                oldcnt = cnt;

                WeightedSite fastest = sitesToConsider[0];

                sitesToConsider.Remove(fastest);

                Site srcSite = fastest.Site;
                if (!srcSite.IsActive)
                    throw new ApplicationException(" the selected source site is not active.");

                newFireList.Add(srcSite);

                //Next, add site's neighbors to the list of
                //sites to consider.  The neighbors cannot be part of
                //any other Fire event in the current timestep, and
                //cannot already be in the list.

                List<WeightedSite> destinations = Get8WeightedNeighbors(srcSite, fireEvent, buildUp);

                if (destinations.Count > 0)
                {
                    foreach (WeightedSite weighted in destinations)
                    {
                        Site destSite = weighted.Site;

                        int fuelIndex = (int)SiteVars.CFSFuelType[destSite];
                        if (fireEvent.InitiationFireRegion.MapCode > FireRegions.MaxMapCode)
                            fuelIndex = (int)SiteVars.CFSFuelType2[destSite];

                        if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Open
                            && fireEvent.FireSeason.PercentCuring == 0)
                        {
                            continue;
                        }
                        if (sitesToConsider.Contains(weighted))
                        {
                            continue;
                        }
                        else
                        {
                            //Complex Cost Surface (Scheller's algorithm)
                            SiteVars.CostTime[destSite] = weighted.Weight;
                            if (srcSite == initiationSite)  // One of the 8 first neighbors
                            {
                                SiteVars.TravelTime[destSite] = (SiteVars.CostTime[destSite] * (CellDistanceBetweenSites(destSite, srcSite) / 2))
                                                       + SiteVars.TravelTime[srcSite];
                            }
                            else
                            {
                            SiteVars.TravelTime[destSite] = (SiteVars.CostTime[destSite] / 2 + SiteVars.CostTime[srcSite] / 2) * CellDistanceBetweenSites(destSite, srcSite)
                                                           + SiteVars.TravelTime[srcSite];
                            }

                            SiteVars.MinNeighborTravelTime[destSite] = SiteVars.TravelTime[srcSite];

                            // Replace Ellipse business with a simple check for SIZE or DURATION
                            if ((fireSizeType == SizeType.size_based && totalArea <= AdjMaxFireParameter) ||
                                (fireSizeType == SizeType.duration_based && SiteVars.TravelTime[destSite] <= AdjMaxFireParameter))
                            {
                                //sitesToConsider.Enqueue(destSite);
                                weighted.Weight = SiteVars.TravelTime[destSite];
                                sitesToConsider.Add(weighted);
                                SiteVars.Event[destSite] = fireEvent;
                                totalArea += cellArea;
                                cnt++;
                            }
                        }
                    }
                }

            }

            //Finally,  check neighbor travel 2000 times to optimize the shortest travel
            //distance.

            bool finished = false;
            int count = 0;

            while (!finished)
            {
                count++;
                //PlugIn.ModelCore.Log.WriteLine("      Re-Calculating optimal travel time:  {0} times.", count);
                finished = true;

                // Loop through list and check each for a new, shorter travel time.
                foreach (Site destSite in newFireList)  //Fire destination
                {
                    Site srcSite = CalculateMinNeighbor(destSite, fireEvent);  // Where is the fire coming from?
                    if (srcSite != null && destSite.IsActive && srcSite.IsActive)
                    {
                        double oldTravelTime = SiteVars.TravelTime[destSite];
                        double newCostTime = CalculateTravelTime(destSite, srcSite, fireEvent, buildUp);
                        double newTravelTime = ((newCostTime / 2 + SiteVars.CostTime[srcSite] / 2) *
                                                    CellDistanceBetweenSites(destSite, srcSite)) + SiteVars.TravelTime[srcSite];
                        if (newTravelTime < oldTravelTime)
                        {
                            SiteVars.CostTime[destSite] = newCostTime;
                            SiteVars.TravelTime[destSite] = newTravelTime;
                            //PlugIn.ModelCore.Log.WriteLine("      OldTT = {0:0.00}, NewTT = {1:0.00}, OldMinNeighborTT = {2:0.00}, MinNeighborTT = {3:0.00}.",
                            //oldTravelTime, SiteVars.TravelTime[destSite],
                            SiteVars.MinNeighborTravelTime[destSite] = SiteVars.TravelTime[srcSite];
                            if ((oldTravelTime - newTravelTime) > 0.5)
                                finished = false;
                        }
                    }

                }

                if (count > 2000) finished = true;
            }

            return newFireList;
        }
        //-------------------------------------------------------
        private static List<WeightedSite> Get8WeightedNeighbors(Site srcSite, Event fireEvent, bool buildUp)
        {
            if (!srcSite.IsActive)
                throw new ApplicationException("Source site is not active.");

            List<WeightedSite> temp = new List<WeightedSite>(0);
            double travelTime = 0.0;

            RelativeLocation[] neighborhood = new RelativeLocation[]
            {
                new RelativeLocation(-1,  0),  // north
                new RelativeLocation(-1,  1),  // northeast
                new RelativeLocation( 0,  1),  // east
                new RelativeLocation( 1,  1),  // southeast
                new RelativeLocation( 1,  0),  // south
                new RelativeLocation( 1, -1),  // southwest
                new RelativeLocation( 0, -1),  // west
                new RelativeLocation(-1, -1),  // northwest
            };

            foreach (RelativeLocation relativeLoc in neighborhood)
            {
                Site neighbor = srcSite.GetNeighbor(relativeLoc);


                if (neighbor != null && neighbor.IsActive && SiteVars.Event[neighbor] == null)
                {
                    travelTime = CalculateTravelTime(neighbor, srcSite, fireEvent, buildUp);
                    //if (travelTime < 1)
                    //    PlugIn.ModelCore.Log.WriteLine("Travel time < 1");
                    temp.Add(new WeightedSite(neighbor, travelTime));
                }
            }

            return temp; //fastNeighbors;
        }

        //-------------------------------------------------------
        private static Site CalculateMinNeighbor(Site site, Event fireEvent)
        {
            List<Site> neighborhood = Get8Neighbors(site);

            Site lowestNeighbor = new Site(); // null;

            double minNeighborTravelTime = SiteVars.MinNeighborTravelTime[site];

            foreach (Site relneighbor in neighborhood)
            {
                if ( //relneighbor != null &&
                    SiteVars.Event[relneighbor] == fireEvent &&
                    !Double.IsPositiveInfinity(SiteVars.TravelTime[relneighbor]) &&
                    SiteVars.TravelTime[relneighbor] < minNeighborTravelTime)
                {
                    lowestNeighbor = relneighbor;
                    SiteVars.MinNeighborTravelTime[site] = SiteVars.TravelTime[relneighbor];
                    //PlugIn.ModelCore.Log.WriteLine("Location = {0},{1}.  MinTravelTime = {2}.", lowestNeighbor.Row, lowestNeighbor.Column, SiteVars.TravelTime[relneighbor]);
                }
            }

            return lowestNeighbor;

        }

        private static double CalculateTravelTime(Site site, Site firesource, Event fireEvent, bool buildUp)
        {
            if (!site.IsActive || !firesource.IsActive)
                throw new ApplicationException("Either site or fire source are not active.");

            //Calculate Fire regime size adjustment (FRUA):
            //Get local fire_region
            IDynamicInputRecord fire_region = SiteVars.FireRegion[site];
            if (fireEvent.SecondRegionMap)
                fire_region = SiteVars.FireRegion2[site];
            
            double FRUA = fire_region.MeanSize;

            //Get source fire region
            fire_region = SiteVars.FireRegion[firesource];
            if (fireEvent.SecondRegionMap)
                fire_region = SiteVars.FireRegion2[site];

            FRUA = FRUA / fire_region.MeanSize;

            int fuelIndex = SiteVars.CFSFuelType[site];
            if (fireEvent.SecondRegionMap)
                fuelIndex = SiteVars.CFSFuelType2[site];
            int percentConifer = SiteVars.PercentConifer[site];
            int percentHardwood = SiteVars.PercentHardwood[site];
            int percentDeadFir = SiteVars.PercentDeadFir[site];
            List<double> siteWindList = new List<double>(); //Alec: changed this list to double from int
            double siteWindSpeed, siteWindDirection;

            //PlugIn.ModelCore.Log.WriteLine("         Fuel Type Code = {0}.", temp.ToString());

            ISeasonParameters season = fireEvent.FireSeason;

            double f_F = Weather.CalculateFuelMoistureEffect(fireEvent.FFMC);
            double ISZ = 0.208 * f_F;  //No wind
            double RSZ = FuelEffects.InitialRateOfSpread(ISZ, season, site, fireEvent.SecondRegionMap);

            if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.ConiferPlantation)//SurfaceFuel == SurfaceFuelType.C6)
            {
                double FME = (Math.Pow((1.5 - (0.00275 * fireEvent.FMC)), 4) / (460 + (25.9 * fireEvent.FMC))) * 1000;
                double RSC = FuelEffects.CalculateRSI(60, 0.0497, 1, ISZ) * (FME / 0.778);
                double CSI = 0.001 * Math.Pow(7, 1.5) * Math.Pow((460 + 25.9 * fireEvent.FMC), 1.5);
                double SFC = FireSeverity.SurfaceFuelConsumption(fuelIndex, fireEvent.FFMC, fireEvent.BuildUpIndex, percentHardwood, percentDeadFir);
                double RSO = CSI / (300 * SFC);
                double CFB = 0;
                if (RSZ > RSO)
                    CFB = 1 - Math.Exp(-0.23 * (RSZ - RSO));
                RSZ = RSZ + (CFB * (RSC - RSZ));
            }
            double BE = 0;

            siteWindList = CalculateSlopeEffect(fireEvent.WindSpeed, fireEvent.WindDirection, site, RSZ, f_F, season, fireEvent.SecondRegionMap);
            if (siteWindList.Count > 0)
            {
                siteWindSpeed = siteWindList[0];
                siteWindDirection = siteWindList[1];
            }
            else
            {
                siteWindSpeed = fireEvent.WindSpeed;
                siteWindDirection = fireEvent.WindDirection;
            }

            double f_backW = Weather.CalculateBackWindEffect(siteWindSpeed);
            double f_W = Weather.CalculateWindEffect(siteWindSpeed);

            double ISI = 0.208 * f_F * f_W;

            //PlugIn.ModelCore.UI.WriteLine("  Debug EventRegion: FFMC={0}.", fireEvent.FFMC);
            //PlugIn.ModelCore.UI.WriteLine("  Debug EventRegion: InitialSpreadIndex={0}.", ISI);

            SiteVars.ISI[site] = ISI;

            double BISI = 0.208 * f_F * f_backW;

            double BROSi = FuelEffects.InitialRateOfSpread(BISI, season, site, fireEvent.SecondRegionMap);
            double ROSi = FuelEffects.InitialRateOfSpread(ISI, season, site,fireEvent.SecondRegionMap);

            if (ROSi == 0)
            {
                SiteVars.RateOfSpread[site] = 0;
                SiteVars.AdjROS[site] = 0;
                return Double.PositiveInfinity;
            }
            else
            {

                BROSi *= FRUA;
                ROSi *= FRUA;

                if (buildUp)
                {
                    if (SiteVars.PercentDeadFir[site] > 0) //either criteria indicates a DEAD FIR type
                    {
                        BE = Math.Exp(50 * Math.Log(0.80) * ((1 / (double)fireEvent.BuildUpIndex) - (1 / (double)50)));
                    }
                    else
                    {
                        BE = Math.Exp(50.0 * Math.Log(Event.FuelTypeParms[fuelIndex].Q) * ((1.0 / (double)fireEvent.BuildUpIndex) - (1 / (double)Event.FuelTypeParms[fuelIndex].BUI)));
                    }
                }
                else
                {
                    BE = 1;
                }
                ROSi *= BE;
                BROSi *= BE;
                SiteVars.RateOfSpread[site] = ROSi;
                if (ROSi > 1000)
                {
                    PlugIn.ModelCore.UI.WriteLine("ROSi > 1000");
                }


                double LB = CalculateLengthBreadthRatio(siteWindSpeed, fuelIndex);

                double FROSi = (ROSi + BROSi) / (LB * 2.0);//        (FBP; 89)

                double alpha = siteWindDirection * (-1);
                double A = FROSi;
                double B = A * LB; //fireEvent.LB;
                double C = B - BROSi;

                //  Equations below for e, p, b, c, a utlize the polar equation
                //  for an ellipse, given at:
                //  http://www.du.edu/~jcalvert/math/ellipse.htm
                //  Note that a and b are switched from those defined in these equations
                //  so that they will fit in Finney's equation below

                double e = Math.Sqrt((double)1 - (Math.Pow(((double)1 / LB), 2)));  //e = Eccentricity
                double dist = CellDistanceBetweenSites(site, firesource) * PlugIn.ModelCore.CellLength;  //dist = r in Calvert's equation
                double r = ROSi;
                double time = 0;
                double index = 1;

                if (dist != 0)
                {
                    double beta = BetaRand(firesource, site, alpha);       // Beta = angle between wind direction, site
                    double p = dist * (1 + e * Math.Cos(Math.PI - beta));  // p = semi-latus rectum = dist from focus to ellipse perimeter perpendicular to major axis
                    double b = p / (1 - (e * e));                          // b = half major axis (a in Calvert's equations)
                    double a = b * Math.Sqrt(1 - (e * e));                 // a = half minor axis (b in Calvert's equations)
                    double c = e * b;                                      // c = dist between focus and center

                    // New (10/22/2007) equations to get r
                    // a, b, c, and dist are real distances in m
                    time = (b + c) / ROSi;  // Calculate the time it would take to travel the distance of b+c if ROSi was the rate
                    r = dist / time;               // Calculate the ROS given the distance to the cell and the time

                    //++++++++++
                    // This is a better way to make adjustments to account for randomization of angles
                    // These equations were derived in MathCad (Beta_Adjust.xmcd)
                    // See rationale for adjustments in Randomization Summary.doc

                    index = 1;
                    if (beta < 0)
                        beta = (Math.PI * 2) + beta;
                    double L2 = LB * LB;

                    double partA = L2 * Math.Pow(((L2 - 1) / L2), 0.5);

                    double partB = (-2) * (Math.Atan(Math.Tan(((0.5) * beta) + ((0.0625) * (Math.PI))) * Math.Pow((2 * L2 + 2 * L2 * Math.Pow(((LB - 1) * ((LB + 1) / L2)), (0.5)) - 1), (0.5))));

                    double partC = (((-1) * (L2)) + (partA * Math.Cos(beta)) - (partA) + (Math.Cos(beta) * L2) - (Math.Cos(beta))) / (Math.Pow(((2 * L2) + (2 * partA) - 1), (0.5)));

                    double partD = (2) * (Math.Atan(((double)1 / (Math.Tan(((0.5) * beta) + ((0.4375) * (Math.PI))))) * Math.Pow((2 * L2 + 2 * L2 * Math.Pow(((LB - 1) * ((LB + 1) / L2)), (0.5)) - 1), (0.5))));
                    if ((((beta + 0.125 * Math.PI) >= Math.PI) && ((beta - 0.125 * Math.PI) >= Math.PI)) || (((beta + 0.125 * Math.PI) <= Math.PI) && ((beta - 0.125 * Math.PI) <= Math.PI)))
                    {
                        index = 4 * (partB * partC - partD * partC) / Math.PI;

                        //Console.WriteLine("Beta = {0}; LB = {1}; partA = {2}; partB = {3}, partC = {4}, partD = {5}; Index = {6}", Beta, LB, partA, partB, partC, partD, index);
                    }
                    else
                    {
                        double indexA = ((-1) * Math.PI * partC - partD * partC) / (Math.PI * ((double)9 / (double)8) - beta);
                        double indexB = (partB * partC - Math.PI * partC) / (beta - Math.PI * ((double)7 / (double)8));
                        double weightA = (Math.PI - (beta - 0.125 * Math.PI)) / (0.25 * Math.PI);
                        double weightB = ((beta + 0.125 * Math.PI) - Math.PI) / (0.25 * Math.PI);
                        index = indexA * weightA + indexB * weightB;
                        //Console.WriteLine("Beta = {0}; IndexA = {1}; weightA = {2}; IndexB = {3}; weightB = {4}; Index = {5}", Beta, indexA, weightA, indexB, weightB, index);

                    }
                    r = r * (1 / index);
                   //++++++++++


                }
                if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.ConiferPlantation) //(int)FuelTypeCode.C6)
                {
                    double FME = (Math.Pow((1.5 - (0.00275 * fireEvent.FMC)), 4) / (460 + (25.9 * fireEvent.FMC))) * 1000;
                    double RSC = FuelEffects.CalculateRSI(60, 0.0497, 1, ISI) * (FME / 0.778);
                    double CSI = 0.001 * Math.Pow(7, 1.5) * Math.Pow((460 + 25.9 * fireEvent.FMC), 1.5);
                    double SFC = FireSeverity.SurfaceFuelConsumption(fuelIndex, fireEvent.FFMC, fireEvent.BuildUpIndex, percentHardwood, percentDeadFir);
                    double RSO = CSI / (300 * SFC);
                    double CFB = 0;
                    if (r > RSO)
                        CFB = 1 - Math.Exp(-0.23 * (r - RSO));
                    r = r + (CFB * (RSC - r));
                }


                //-----Added by BRM-----
                SiteVars.AdjROS[site] = r;
                //----------

                //PlugIn.ModelCore.Log.WriteLine("      FROSi = {0}, BROSi = {1}.", FROSi, BROSi);
                //PlugIn.ModelCore.Log.WriteLine("      beta = {0:0.00}, theta = {1:0.00}, alpha = {2:0.00}, Travel time = {3:0.000000}.", beta, theta, alpha, 1/r);
                //PlugIn.ModelCore.Log.WriteLine("      Travel time = {0:0.000000}.  R = {1}.", 1/r, r);
                if (site == firesource)
                {
                    double rate = 1.0 / r;  //units = minutes / meter
                    double cost = rate * PlugIn.ModelCore.CellLength / 2.0;     //units = minutes
                    return cost;
                }
                else
                {
                    double rate = 1.0 / r;  //units = minutes / meter
                    double cost = rate * PlugIn.ModelCore.CellLength;     //units = minutes
                    //if (cost < 1)
                     //   PlugIn.ModelCore.Log.WriteLine("Travel time < 1 min");
                    return cost;
                }
            }
        }
        //-------------------------------------------------------
        private static double Beta(Site sourcesite, Site site, double windDirection)
        {
            double row = (double)sourcesite.Location.Row - (double)site.Location.Row;
            double column = (double)sourcesite.Location.Column - (double)site.Location.Column;

            double atan = 0.0;
            if (row == 0)
            {
                if (column == 0)
                {
                    return atan;
                }
                row = 0.0000001;
            }

            atan = Math.Atan(column / row) * 180 / Math.PI;// Convert to degrees

            if (Math.Sign(row) == -1) atan -= 180;

            double beta = atan - windDirection;

            //Randomize around 8 neighbor directions
            //double wiggle = (PlugIn.ModelCore.GenerateUniform() * 45.0) - 22.5;
            //beta = beta + wiggle;
            //
            beta = beta * Math.PI / 180;  //Convert to radians
            return beta;

        }

        private static double BetaRand(Site sourcesite, Site site, double windDirection)
        {
            double row = (double)sourcesite.Location.Row - (double)site.Location.Row;
            double column = (double)sourcesite.Location.Column - (double)site.Location.Column;

            double atan = 0.0;
            if (row == 0)
            {
                if (column == 0)
                {
                    return atan;
                }
                row = 0.0000001;
            }

            atan = Math.Atan(column / row) * 180 / Math.PI;// Convert to degrees

            if (Math.Sign(row) == -1) atan -= 180;

            double beta = atan - windDirection;

            //Randomize around 8 neighbor directions
            double wiggle = (PlugIn.ModelCore.GenerateUniform() * 45.0) - 22.5;
            beta = beta + wiggle;
            //
            beta = beta * Math.PI / 180;  //Convert to radians
            return beta;

        }

        private static double CalculateLengthBreadthRatio(double WSV, int fuelIndex)
        {
            // Use the Length to Breadth ratio to determine the FLANK rate of spread (FROS).
            // WSV = wind speed velocity (Event.WindSpeed)

            double lengthBreadthRatio = 0.0;

            //if(Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Open)
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1a || Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1b)
            {
                if (WSV < 1.0)
                    lengthBreadthRatio = 1.0; //(FBP; 81)
                else
                    lengthBreadthRatio = 1.1 + Math.Pow((double)WSV, 0.464); //(FBP; 80)
            }
            else
            lengthBreadthRatio = 1.0 + (8.729 * Math.Pow((1.0 - Math.Exp(-0.030 * (double)WSV)), 2.155));//    (FBP; 79)

            return lengthBreadthRatio;
        }

        private static bool ElipseDistanceBetweenSites(Site igSite, Site burnSite, double C, double A, double fireDirection, double ellipseArea, SizeType fireSizeType)
        {
            bool lessThan = false;

            //In the landscape, 'higher' rows have lower numbers, the inverse of what you
            //would expect if the ignition site is located at (0,0).
            double dYburn = ((double)igSite.Location.Row - (double)burnSite.Location.Row);
            double dXburn = ((double)burnSite.Location.Column - (double)igSite.Location.Column);

            C = C / (double)PlugIn.ModelCore.CellLength;
            A = A / (double)PlugIn.ModelCore.CellLength;

            double sumDist = (Math.Sqrt((A * A) + (C * C))) * 2;  // Equals B * 2 (the length of the major axis)

            //Assume that ignition site (F0) is at location 0,0.
            double LengthA = Math.Sqrt((dXburn * dXburn) + (dYburn * dYburn));

            //Where is second focal site?

            double dYf1 = C * 2 * Math.Cos((fireDirection) * Math.PI / 180);
            double dXf1 = C * 2 * Math.Sin((fireDirection) * Math.PI / 180);

            double dXburn_f1 = dXburn - dXf1;
            double dYburn_f1 = dYburn - dYf1;

            double LengthB = Math.Sqrt((dXburn_f1 * dXburn_f1) + (dYburn_f1 * dYburn_f1));

            double radius = Math.Sqrt((ellipseArea / Math.PI)) / (double)PlugIn.ModelCore.CellLength;

            if (fireSizeType == SizeType.duration_based)
            {
                radius *= 1.0;  // RMS:  Was 1.5.  Trying to limit the size of ellipses
                //sumDist *= 2;
            }
            else
            {
                //radius *= 1.5;
                sumDist *= 1.0; //5;
            }

            //if((LengthA + LengthB >= sumDist ) || (LengthA >= radius))
            //    lessThan = false;
            if ((LengthA + LengthB <= sumDist) || (LengthA <= radius))
                lessThan = true;
            //PlugIn.ModelCore.Log.WriteLine("LengthA={0:0.0}, LengthB={1:0.0}, C={2:0.0}, D={3:0.0}.", LengthA, LengthB, C,D);
            //PlugIn.ModelCore.Log.WriteLine("dXf1={0:0.0}, dYf1={1:0.0}, dXburn={2:0.0}, dYburn={3:0.0}.", dXf1, dYf1,dXburn,dYburn);
            //return true;
            return lessThan;
        }



        //-------------------------------------------------------
        //Calculate the distance (in units of cells) from a location to a center
        //point (row and column = 0).
        private static double CellDistanceBetweenSites(Site asite, Site bsite)
        {
            double row = ((double)asite.Location.Row - (double)bsite.Location.Row);
            double column = ((double)asite.Location.Column - (double)bsite.Location.Column);
            double cSq = column * column;
            double rSq = row * row;
            return Math.Sqrt(cSq + rSq);
        }

        private static List<Site> Get8Neighbors(Site site)
        {
            List<Site> neighbors = new List<Site>();

            RelativeLocation[] neighborhood = new RelativeLocation[]
            {
                new RelativeLocation(-1,  0),  // north
                new RelativeLocation(-1,  1),  // northeast
                new RelativeLocation( 0,  1),  // east
                new RelativeLocation( 1,  1),  // southeast
                new RelativeLocation( 1,  0),  // south
                new RelativeLocation( 1, -1),  // southwest
                new RelativeLocation( 0, -1),  // west
                new RelativeLocation(-1, -1),  // northwest
            };

            foreach (RelativeLocation relativeLoc in neighborhood)
            {
                Site neighbor = site.GetNeighbor(relativeLoc);
                if (neighbor != null && neighbor.IsActive)
                    neighbors.Add(neighbor);
            }
            PlugIn.ModelCore.shuffle(neighbors);

            return neighbors;
        }

        private static double CosSquared(double num)
        {
            return (Math.Cos(num) * Math.Cos(num));
        }
        private static double SinSquared(double num)
        {
            return (Math.Sin(num) * Math.Sin(num));
        }
        private static List<double> CalculateSlopeEffect(double windSpeed, double windDirection,      //Alec: changed windspeed to double
                                                Site site, double RSZ, double f_F,
                                                ISeasonParameters season, bool secondRegionMap)
        {
            List<double> siteWindList = new List<double>(); //Alec:Changed this list to double
            if ((SiteVars.GroundSlope[site] == 0) || (RSZ == 0))
            {
                siteWindSpeed = windSpeed;
                siteWindDirection = windDirection;
                //nothing is changed
            }
            else
            {
                //FuelTypeCode siteFuelType = (FuelTypeCode)SiteVars.CFSFuelType[site]; ;

                //Walk through the equations from FBP:

                double SF = 0;
                if (SiteVars.GroundSlope[site] > 60)
                {
                    SF = CalculateSF(60);  //FBP 39 has a max slope of 60%
                }
                else
                {
                    SF = CalculateSF(SiteVars.GroundSlope[site]);  //FBP 39
                }
                double RSF = RSZ * SF;  //FBP 40

                double ISF = 0.0;
                double a, b, c;
                int fuelIndex = SiteVars.CFSFuelType[site];
                if (secondRegionMap)
                    fuelIndex = SiteVars.CFSFuelType2[site];
                

                //if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Open)
                if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1a || Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1b)
                //(siteFuelType == FuelTypeCode.O1a)
                {
                    int percentCuring = season.PercentCuring;
                    double CF = (0.02 * percentCuring) - 1.0;
                    if (season.NameOfSeason == SeasonName.Spring)
                    {
                        a = 190; //Event.FuelTypeParms[(int)FuelTypeCode.O1a].A;
                        b = 0.0310; //Event.FuelTypeParms[(int)FuelTypeCode.O1a].B;
                        c = 1.4; //Event.FuelTypeParms[(int)FuelTypeCode.O1a].C;
                    }
                    else
                    {
                        a = 250; //Event.FuelTypeParms[(int)FuelTypeCode.O1b].A;
                        b = 0.0350; //Event.FuelTypeParms[(int)FuelTypeCode.O1b].B;
                        c = 1.7; //Event.FuelTypeParms[(int)FuelTypeCode.O1b].C;
                    }

                    ISF = Math.Log((1 - Math.Pow((RSF / (CF * a)), (1 / c)))) / (-1 * b); //FBP 43
                }
                else
                {
                    a = Event.FuelTypeParms[fuelIndex].A;
                    b = Event.FuelTypeParms[fuelIndex].B;
                    c = Event.FuelTypeParms[fuelIndex].C;

                    if (SiteVars.PercentDeadFir[site] > 0)
                    {
                        int PDF = SiteVars.PercentDeadFir[site];
                        if ((SiteVars.PercentHardwood[site] > 0) && (season.LeafStatus == LeafOnOff.LeafOn)) //M-4
                        {
                            a = 140 * System.Math.Exp((-1) * 35.5 / (double)PDF);
                            b = 0.0404;
                            c = 3.02 * System.Math.Exp((-1) * 0.00714 * (double)PDF);
                        }

                        else //M-3
                        {
                            a = 170 * System.Math.Exp((-1) * 35 / (double)PDF);
                            b = 0.082 * System.Math.Exp((-1) * 36 / (double)PDF);
                            c = 1.698 - (0.00303 * (double)PDF);
                        }
                        ISF = Math.Log((1 - Math.Pow((RSF / a), (1 / c)))) / (-1 * b); //FBP 41
                    }

                    else if ((SiteVars.PercentHardwood[site] > 0) && (SiteVars.PercentConifer[site] > 0))
                    {
                        int PC = SiteVars.PercentConifer[site];
                        ISF = Math.Log((1 - Math.Pow(((100 - RSF) / (PC * a)), (1 / c)))) / (-1 * b); //FBP 42
                    }
                    else
                    {
                        ISF = Math.Log((1 - Math.Pow((RSF / a), (1 / c)))) / (-1 * b); //FBP 41
                    }
                }
                double WSE = (Math.Log((ISF) / (.208 * f_F))) / (0.05039); //FBP 43: The effect the % slope would have on ROS if it were a wind speed
                double WAZ = (double)windDirection * Math.PI / 180;  //wind direction/azimuth in radians
                double SAZ = (double)SiteVars.UphillSlopeAzimuth[site] * Math.PI / 180;  //uphill slope azimuth in radians

                double WSX = ((double)windSpeed * Math.Sin(WAZ)) + (WSE * Math.Sin(SAZ));  //FBP 47
                double WSY = ((double)windSpeed * Math.Cos(WAZ)) + (WSE * Math.Cos(SAZ));  //FBP 48


                double WSV = Math.Sqrt(Math.Pow(WSX, 2) + Math.Pow(WSY, 2));  //FBP 49
                siteWindSpeed = (double)WSV;

                double RAZ = Math.Acos(WSY / WSV);  //FBP 50
                RAZ = RAZ * 180 / Math.PI; // net wind direction radians to degrees

                if (WSX < 0) RAZ = 360 - RAZ;
                siteWindDirection = (int)RAZ;
                if (siteWindSpeed < 0 || siteWindDirection < 0)
                {
                }
            }
            siteWindList.Add(siteWindSpeed);
            siteWindList.Add(siteWindDirection);
            SiteVars.SiteWindSpeed[site] = (ushort)siteWindSpeed;
            SiteVars.SiteWindDirection[site] = (ushort)siteWindDirection;

            return siteWindList;



        }


        private static double CalculateWSE(int fuelIndex, double RSF, double f_F, /*int PC,*/ ISeasonParameters season)
        {
            //FuelTypeCode siteFuelType = (FuelTypeCode) fuelIndex;
            double ISF, WSE;
            double a = Event.FuelTypeParms[fuelIndex].A;
            double b = Event.FuelTypeParms[fuelIndex].B;
            double c = Event.FuelTypeParms[fuelIndex].C;

            //if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Open)
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1a || Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1b)
            //siteFuelType == FuelTypeCode.O1a)
            {
                double CF = (0.02 * season.PercentCuring) - 1;

                ISF = Math.Log(1 - Math.Pow((RSF / (CF * a)), (1 / c))) / (-1 * b);
            }
            else
            {
                ISF = Math.Log(1 - Math.Pow((RSF / (a)), (1 / c))) / (-1 * b);
            }
            WSE = Math.Log(ISF / (0.208 * f_F)) / 0.05039;
            return WSE;
        }


        public class SiteComparer : IComparer<Site>
        {
            public int Compare(Site x, Site y)
            {
                int myCompare = SiteVars.TravelTime[x].CompareTo(SiteVars.TravelTime[y]);
                return myCompare;
            }
        }

        private static double CalculateSF(int groundSlope)
        {
            return Math.Pow(Math.E, 3.533 * Math.Pow(((double)groundSlope / 100), 1.2));  //FBP 39
        }

        public class WeightComparer : IComparer<WeightedSite>
        {
            public int Compare(WeightedSite x,
                                              WeightedSite y)
            {
                /*if (x.Weight < y.Weight)
                    return -1;
                else if (x.Weight > y.Weight)
                    return 1;
                else
                    return 0;
                 * */
                int myCompare = x.Weight.CompareTo(y.Weight);
                return myCompare;
            }

        }
    }
}
