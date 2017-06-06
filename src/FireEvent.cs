//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.Climate;
//using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Landis.Extension.Scrapple
{

    public class FireEvent
        : ICohortDisturbance
    {
        private static readonly bool isDebugEnabled = false; //debugLog.IsDebugEnabled;

        //public static IFuelType[] FuelTypeParms;
        public static double SF;
        private static List<IFireDamage> damages;

        private ActiveSite initiationSite;
        private double maxFireParameter;
        private int sizeBin;
        private double maxDuration;
        //private IDynamicInputRecord initiationFireRegion;
        private bool secondRegionMap;
        private int initiationPercentConifer;
        private int initiationFuel;
        private int totalSitesDamaged;
        private int cohortsKilled;
        private double eventSeverity;
        private int numSitesChecked;
        private int[] sitesInEvent;

        private ActiveSite currentSite; // current site where cohorts are being damaged
        private int siteSeverity;      // used to compute maximum cohort severity at a site

        //private ISeasonParameters fireSeason;
        private double windSpeed;  
        private double windDirection;
        //private int fineFuelMoistureCode;
        //private int buildUpIndex;
        //private int foliarMC;
        //private int isi;
        //private double lengthB;
        //private double lengthA;
        //private double lengthD;
        //private double lbr;  //lenght:breadth ratio

        public double FireWeatherIndex;

        //---------------------------------------------------------------------
        static FireEvent()
        {
        }

        //---------------------------------------------------------------------

        public Location StartLocation
        {
            get {
                return initiationSite.Location;
            }
        }

        //---------------------------------------------------------------------

        public double MaxFireParameter
        {
            get {
                return maxFireParameter;
            }
        }
        //---------------------------------------------------------------------

        public double SizeBin
        {
            get
            {
                return sizeBin;
            }
        }
        //---------------------------------------------------------------------
        public double MaxDuration
        {
            get
            {
                return maxDuration;
            }
        }
        //---------------------------------------------------------------------

        //public IDynamicInputRecord InitiationFireRegion
        //{
        //    get {
        //        return initiationFireRegion;
        //    }
        //}
        //---------------------------------------------------------------------

        public bool SecondRegionMap
        {
            get
            {
                return secondRegionMap;
            }
        }
        //---------------------------------------------------------------------
        public int InitiationPercentConifer
        {
            get {
                return initiationPercentConifer;
            }
        }
        //---------------------------------------------------------------------

        public int InitiationFuel
        {
            get {
                return initiationFuel;
            }
        }
        //---------------------------------------------------------------------

        public int TotalSitesDamaged
        {
            get {
                return totalSitesDamaged;
            }
        }
        //---------------------------------------------------------------------

        public int[] SitesInEvent
        {
            get {
                return sitesInEvent;
            }
        }

        //---------------------------------------------------------------------

        public int NumSitesChecked
        {
            get {
                return numSitesChecked;
            }
        }
        //---------------------------------------------------------------------

        public int CohortsKilled
        {
            get {
                return cohortsKilled;
            }
        }

        //---------------------------------------------------------------------

        public double EventSeverity
        {
            get {
                return eventSeverity;
            }
        }

        //---------------------------------------------------------------------

        public double WindSpeed
        {
            get
            {
                return windSpeed;
            }
            set
            {
                windSpeed = value;
            }
        }
        //---------------------------------------------------------------------

        public double WindDirection
        {
            get
            {
                return windDirection;
            }
            set
            {
                windDirection = value;
            }
        }
        //---------------------------------------------------------------------

        //public int FFMC
        //{
        //    get {
        //        return fineFuelMoistureCode;
        //    }
        //}

        ////---------------------------------------------------------------------

        //public int BuildUpIndex
        //{
        //    get {
        //        return buildUpIndex;
        //    }
        //}

        ////---------------------------------------------------------------------

        //public int FMC
        //{
        //    get
        //    {
        //        return foliarMC;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public int ISI
        //{
        //    get
        //    {
        //        return isi;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public ISeasonParameters FireSeason
        //{
        //    get {
        //        return fireSeason;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public double LengthB
        //{
        //    get {
        //        return lengthB;
        //    }
        //    set {
        //        lengthB = value;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public double LengthA
        //{
        //    get {
        //        return lengthA;
        //    }
        //    set {
        //        lengthA = value;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public double LengthD
        //{
        //    get {
        //        return lengthD;
        //    }
        //    set {
        //        lengthD = value;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public double LB
        //{
        //    get {
        //        return lbr;
        //    }
        //    set {
        //        lbr = value;
        //    }
        //}
        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.ExtType;
            }
        }

        //---------------------------------------------------------------------

        ActiveSite IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }

        //---------------------------------------------------------------------
        // Constructor function

        public FireEvent(ActiveSite initiationSite, /*ISeasonParameters fireSeason, SizeType fireSizeType, IDynamicInputRecord eco,*/ int day)
        {
            this.initiationSite = initiationSite;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[initiationSite];

            int actualYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_DailyData.First().Key;
            AnnualClimate_Daily annualWeatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];

            //this.sitesInEvent = new int[FireRegions.Dataset.Length];

            //foreach (IDynamicInputRecord fire_region in FireRegions.Dataset)
            //    this.sitesInEvent[fire_region.Index] = 0;
            this.cohortsKilled = 0;
            this.eventSeverity = 0;
            this.totalSitesDamaged = 0;
            //this.lengthB = 0.0;
            //this.lengthA = 0.0;
            //this.lengthD = 0.0;
            ////IFireRegion eco = SiteVars.FireRegion[initiationSite];
            //this.initiationFireRegion = eco;
            //if (eco.MapCode > FireRegions.MaxMapCode)
            //    this.secondRegionMap = true;
            //else
            //    this.secondRegionMap = false;
            //this.maxFireParameter = ComputeSize(eco.MeanSize, eco.StandardDeviation, eco.MinSize, eco.MaxSize); 
            //this.fireSeason         = fireSeason; 
            this.FireWeatherIndex = annualWeatherData.DailyFireWeatherIndex[day];
            this.windSpeed = annualWeatherData.DailyWindSpeed[day];
            //this.fineFuelMoistureCode = (int) AnnualFireWeather.FineFuelMoistureCode;
            //this.buildUpIndex = (int) AnnualFireWeather.BuildUpIndex;
            this.windDirection = annualWeatherData.DailyWindDirection[day];
            //this.foliarMC = Weather.GenerateFMC(this.fireSeason, eco);


        }

        //---------------------------------------------------------------------

        public static void Initialize(//ISeasonParameters[] seasons,
                                      //IFuelType[] fuelTypeParameters,
                                      List<IFireDamage>    damages)
        {
            //if (isDebugEnabled)
            //    PlugIn.ModelCore.UI.WriteLine("Initializing event parameters ...");

            //if(seasons == null || fuelTypeParameters == null || damages == null)
            //{
            //    if(seasons == null)
            //        PlugIn.ModelCore.UI.WriteLine("Error:  Seasons table empty.");
            //    if(fuelTypeParameters == null)
            //        PlugIn.ModelCore.UI.WriteLine("Error:  FuelTypeParameters table empty.");
            //    if(damages == null)
            //        PlugIn.ModelCore.UI.WriteLine("Error:  Damages table empty.");
            //    throw new System.ApplicationException("Error: Event class could not be initialized.");
            //}

            //float totalSeasonFireProb = 0.0F;
            //foreach(ISeasonParameters season in seasons)
            //    totalSeasonFireProb += (float) season.FireProbability;

            //if (totalSeasonFireProb != 1.0)
            //    throw new System.ApplicationException("Error: Season Probabilities don't add to 1.0");

            //Event.FuelTypeParms = fuelTypeParameters;
            FireEvent.damages = damages;

            int tempSlope, sumSlope = 0, cellCount = 0, meanSlope = 0;
            foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
            {
                if (site.IsActive)
                {
                    tempSlope = SiteVars.GroundSlope[site];
                    sumSlope += tempSlope;
                    cellCount++;
                }
            }

            if(sumSlope > 0)
            {
                meanSlope = sumSlope / cellCount;
                if (meanSlope > 60)
                    meanSlope = 60;
                FireEvent.SF = CalculateSF(meanSlope);
            }
        }

        //---------------------------------------------------------------------
        public static FireEvent Initiate(ActiveSite site, int timestep, int day)

        {


            double randomNum = PlugIn.ModelCore.GenerateUniform();



            // Ignition moved to Run()

            //// Get probability of ignition based on Jen Beverly equation and FWI;

            //double FWIshape = FuelTypeParms[fuelIndex].IgnitionDistributionShape;//RMS: Necessary?  

            //double FWIscale = FuelTypeParms[fuelIndex].IgnitionDistributionScale;//RMS: Necessary?

            //// A. Kretchun: My equation that includes FWIshape and FWIscale and AnnualFire.FireWeatherIndex. This equation comes from Beverly et al 2007
            //double ignitionProbability = 1/(1+Math.Exp(-(FWIshape+FWIscale*AnnualFireWeather.FireWeatherIndex))); 

            /*
             * VS: These were removed to being calculated once a year. 
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            AnnualFireWeather.CalculateAnnualFireWeather(ecoregion);
            */
            FireEvent fireEvent = new FireEvent(site,/* fireSeason, fireSizeType, eco, */ day); //Must create event to determine season

            // Test that adequate weather data was retrieved:-
            /*
            if (fireEvent.windSpeed == 0)
            {
            // throw an error //RMS
                throw new Exception("Inadequate weasther data retrieved");
                //return null;
            }
            */
            return fireEvent;
        }

        //---------------------------------------------------------------------
        private bool Spread(ActiveSite initiationSite)//, IDynamicInputRecord fire_region, SizeType fireSizeType, bool BUI, double severityCalibrate)
        {
            //First, check for fire overlap:
            if(SiteVars.FireEvent[initiationSite] != null)
                return false;

            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("   Spreading fire event started at {0} ...", initiationSite.Location);

            //IDynamicInputRecord fire_region = SiteVars.FireRegion[initiationSite];

            int totalSiteSeverities = 0;
            int siteCohortsKilled    = 0;
            //int totalISI = 0;
            totalSitesDamaged = 1;

            //this.initiationFuel   = SiteVars.CFSFuelType[initiationSite];
            //if (this.secondRegionMap)
            //    this.initiationFuel = SiteVars.CFSFuelType2[initiationSite];
            this.initiationPercentConifer = SiteVars.PercentConifer[initiationSite];

            //Next, calculate the fire area:
            List<Site> FireLocations = new List<Site>();

            //if (isDebugEnabled) PlugIn.ModelCore.UI.WriteLine("  Calling SizeFireCostSurface ...");

            //FireLocations = EventRegion.SizeFireCostSurface(this, fireSizeType, BUI);

            //if (isDebugEnabled) PlugIn.ModelCore.UI.WriteLine("    FireLocations.Count = {0}", FireLocations.Count);

            //if (FireLocations.Count == 0) return false;

            ////Attach travel time weights here
            //if (isDebugEnabled)
            //    PlugIn.ModelCore.UI.WriteLine("  Computing SizeFireCostSurface ...");
            //List<WeightedSite> FireCostSurface = new List<WeightedSite>(0);
            //foreach(Site site in FireLocations)
            //{
            //    double myWeight = SiteVars.TravelTime[site];
            //    if ((Double.IsNaN(myWeight))||(Double.IsInfinity(myWeight))) { }
            //    else
            //    {
            //       FireCostSurface.Add(new WeightedSite(site, myWeight));
            //    }
            //}
            //WeightComparer weightComp = new WeightComparer();
            //FireCostSurface.Sort(weightComp);
            FireLocations = new List<Site>();

            double cellArea = (PlugIn.ModelCore.CellLength * PlugIn.ModelCore.CellLength) / 10000; //convert to ha
            //double totalArea = 0.0;
            //int cellCnt = 0;
            //double durMax = 0;

            //if (isDebugEnabled)
            //    PlugIn.ModelCore.UI.WriteLine("  Determining cells burned ...");
            //if (fireSizeType == SizeType.size_based)
            //{

            //    foreach(WeightedSite weighted in FireCostSurface)
            //    {
            //        //weightCnt++;
            //        cellCnt++;
            //        if(totalArea > this.maxFireParameter)
            //        {
            //            SiteVars.Event[weighted.Site] = null;
            //        }
            //        else
            //        {
            //            totalArea += cellArea;
            //            FireLocations.Add(weighted.Site);
            //            if (SiteVars.TravelTime[weighted.Site] > durMax)
            //                durMax = SiteVars.TravelTime[weighted.Site];
            //        }
            //    }
            //    this.maxDuration = durMax;
            //    //Debug
            //    if ((durMax < 0.01)&& (FireLocations.Count > 0))
            //        PlugIn.ModelCore.UI.WriteLine("Duration = 0");
            //    //PlugIn.ModelCore.Log.WriteLine("   Fire Summary:  Cells Checked={0}, BurnedArea={1:0.0} (ha), Target Area={2:0.0} (ha).", cellCnt, totalArea, this.maxFireParameter);
            //    //if(totalArea < this.maxFireParameter)
            //    //    PlugIn.ModelCore.Log.WriteLine("      NOTE:  Partial fire burn; fire may have spread to the edge of the active area.");
            //}
            //else if (fireSizeType == SizeType.duration_based)
            //{
            //    double durationAdj = this.maxFireParameter;
            //    if (durationAdj >= 1440)
            //        durationAdj = durationAdj * this.FireSeason.DayLengthProp;


            //    foreach(WeightedSite weighted in FireCostSurface)
            //    {
            //        cellCnt++;
            //        if (weighted.Site == this.initiationSite)
            //        {
            //            totalArea += cellArea;
            //            FireLocations.Add(weighted.Site);
            //            if (SiteVars.TravelTime[weighted.Site] > durMax)
            //                durMax = SiteVars.TravelTime[weighted.Site];
            //        }
            //        else
            //        {
            //            if (weighted.Weight > durationAdj)
            //            {
            //                SiteVars.Event[weighted.Site] = null;
            //            }
            //            else
            //            {
            //                totalArea += cellArea;
            //                FireLocations.Add(weighted.Site);
            //                //-----Added by BRM-----
            //                if (SiteVars.TravelTime[weighted.Site] > durMax)
            //                    durMax = SiteVars.TravelTime[weighted.Site];
            //                //----------
            //            }
            //        }
            //    }
            //    this.maxDuration = durMax;
            //    //Debug
            //    if ((durMax < 0.01) && (FireLocations.Count > 0))
            //        PlugIn.ModelCore.UI.WriteLine("Duration = 0");

            //    //PlugIn.ModelCore.Log.WriteLine("   Fire Summary:  Cells Checked={0}, BurnedArea={1:0.0} (ha), Target Duration={2:0.0}, Adjusted Duration = {3:0.0}.", cellCnt, totalArea, this.maxFireParameter, durationAdj);
            //    //if(durationAdj - durMax > 5.0)
            //    //    PlugIn.ModelCore.Log.WriteLine("      NOTE:  Partial fire burn; fire may have spread to the edge of the active area.");
            //}
            //if (isDebugEnabled)
            //    PlugIn.ModelCore.UI.WriteLine("  FireLocations.Count = {0}", FireLocations.Count);
            //int FMC = this.FMC;  //Foliar Moisture Content

            if (FireLocations.Count == 0) return false;

            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("  Damaging cohorts at burned sites ...");
            foreach(Site site in FireLocations)
            {
                currentSite = (ActiveSite) site;
                if(site.IsActive)
                {
                    this.numSitesChecked++;

                    this.siteSeverity = 0; // FireSeverity.CalcFireSeverity(currentSite, this); //, severityCalibrate, FMC);
                    siteCohortsKilled = Damage(currentSite);

                    this.totalSitesDamaged++;
                    totalSiteSeverities += this.siteSeverity;
                    //totalISI += (int) SiteVars.ISI[site];
                    
                    
                    //IDynamicInputRecord siteFireRegion = SiteVars.FireRegion[site];
                    //if (this.secondRegionMap)
                    //    siteFireRegion = SiteVars.FireRegion2[site];

                    //sitesInEvent[siteFireRegion.Index]++;

                    SiteVars.Disturbed[currentSite] = true;
                    SiteVars.Severity[currentSite] = (byte) siteSeverity;

                    if(siteSeverity > 0)
                        SiteVars.LastSeverity[currentSite] = (byte)siteSeverity;
                }
            }

            if (this.totalSitesDamaged == 0)
                this.eventSeverity = 0;
            else
                this.eventSeverity = ((double) totalSiteSeverities) / (double)this.totalSitesDamaged;

            //this.isi = (int) ((double) totalISI / (double) this.totalSitesDamaged);

            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("  Done spreading");
            return true;
        }
        //---------------------------------------------------------------------

        //public static double ComputeSize(double meanSize, double sd, double minSize, double maxSize)
        //{

        //    double sizeGenerated = maxSize * 2.0;
        //    //LognormalDistribution randVar = new LognormalDistribution(RandomNumberGenerator.Singleton);
        //    //double minSize = 0.0;

        //    while(sizeGenerated > maxSize || sizeGenerated <= minSize)
        //    {
        //        PlugIn.ModelCore.LognormalDistribution.Mu = meanSize;      //randVar.Mu for Lognormal //randVar.Alpha for Gamma
        //        PlugIn.ModelCore.LognormalDistribution.Sigma = sd;   //randVar.Sigma for Lognormal //randVar.Theta for Gamma
        //        sizeGenerated = PlugIn.ModelCore.LognormalDistribution.NextDouble();
        //        //PlugIn.ModelCore.Log.WriteLine(sizeGenerated.ToString());
        //    }
        //    return sizeGenerated;
        //}

        //public static int ComputeSizeBin(double meanSize, double sd, double sizeGenerated)
        //{
        //    // Percentile cutoffs from MN DNR (www.dnr.state.mn.us/forestry/fire/reports/canadian_indexes_o.html)
        //    double size5 = Math.Exp(meanSize + sd * 1.9600); // 97.5th percentil
        //    double size4 = Math.Exp(meanSize + sd * 1.2816); // 90th percentile
        //    double size3 = Math.Exp(meanSize + sd * 0.722);  // 76.5th percentile
        //    double size2 = Math.Exp(meanSize + sd * (-0.087)); // 46.5th percentile
        //    int sizeBin = 0;
        //    if (sizeGenerated >= size5)
        //        sizeBin = 5;
        //    else if (sizeGenerated >= size4)
        //        sizeBin = 4;
        //    else if (sizeGenerated >= size3)
        //        sizeBin = 3;
        //    else if (sizeGenerated >= size2)
        //        sizeBin = 2;
        //    else
        //        sizeBin = 1;
        //    return sizeBin;
        //}

        //---------------------------------------------------------------------

        private int Damage(ActiveSite site)
        {
            int previousCohortsKilled = this.cohortsKilled;
            SiteVars.Cohorts[site].RemoveMarkedCohorts(this);
            return this.cohortsKilled - previousCohortsKilled;
        }

        //---------------------------------------------------------------------

        //  A filter to determine which cohorts are removed.

        bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {
            bool killCohort = false;

            //Fire Severity 5 kills all cohorts:
            if (siteSeverity == 5)
            {
                killCohort = true;
            }
            else {
                //Otherwise, use damage table to calculate damage.
                //Read table backwards; most severe first.
                float ageAsPercent = (float) cohort.Age / (float) cohort.Species.Longevity;
                foreach(IFireDamage damage in damages)
                //for (int i = damages.Length-1; i >= 0; --i)
                {
                    //IFireDamage damage = damages[i];
                    if (siteSeverity - cohort.Species.FireTolerance >= damage.SeverTolerDifference)
                    {
                        if (damage.MaxAge >= ageAsPercent)
                        {
                            killCohort = true;

                            break;  // No need to search further in the table
                        }
                    }
                }
            }

            if (killCohort) {
                this.cohortsKilled++;
            }
            return killCohort;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Compares weights
        /// </summary>

        public class WeightComparer : IComparer<WeightedSite>
        {
            public int Compare(WeightedSite x,
                                              WeightedSite y)
            {
                int myCompare = x.Weight.CompareTo(y.Weight);
                return myCompare;
            }

        }

        private static double CalculateSF(int groundSlope)
        {
            return Math.Pow(Math.E, 3.533 * Math.Pow(((double)groundSlope / 100),1.2));  //FBP 39
        }

    }


    public class WeightedSite
    {
        private Site site;
        private double weight;

        //---------------------------------------------------------------------
        public Site Site
        {
            get {
                return site;
            }
            set {
                site = value;
            }
        }

        public double Weight
        {
            get {
                return weight;
            }
            set {
                weight = value;
            }
        }

        public WeightedSite (Site site, double weight)
        {
            this.site = site;
            this.weight = weight;
        }

    }
}
