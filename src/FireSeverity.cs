//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.SpatialModeling;
using Landis.Core;
using System.Collections.Generic;
using System;

namespace Landis.Extension.Scrapple
{
    public class FireSeverity
    {

        public static int CalcFireSeverity(ActiveSite site, Event fireEvent, double severityCalibrate, int FMC)
        {

            int PH = SiteVars.PercentHardwood[site];  //Percent Hardwood
            int PDF = SiteVars.PercentDeadFir[site];
            int PC = SiteVars.PercentConifer[site];

            int fuelIndex = SiteVars.CFSFuelType[site];
            if (fireEvent.InitiationFireRegion.MapCode > FireRegions.MaxMapCode)
                fuelIndex = SiteVars.CFSFuelType2[site];
            int CBH = Event.FuelTypeParms[fuelIndex].CBH;
            //If M3 or M4 type (PDF >0) assign appropriate fuel index
            if (PDF > 0)
            {

                    if (fireEvent.FireSeason.LeafStatus == LeafOnOff.LeafOff)
                    {
                        foreach (FuelType listFuel in Event.FuelTypeParms)
                        {
                            if (listFuel.SurfaceFuel == SurfaceFuelType.M3)
                                fuelIndex = listFuel.FuelIndex;
                            if (Event.FuelTypeParms[fuelIndex].CBH < CBH)
                                CBH = Event.FuelTypeParms[fuelIndex].CBH;
                        }
                    }
                    else
                    {
                        foreach (FuelType listFuel in Event.FuelTypeParms)
                        {
                            if (listFuel.SurfaceFuel == SurfaceFuelType.M4)
                                fuelIndex = listFuel.FuelIndex;
                            if (Event.FuelTypeParms[fuelIndex].CBH < CBH)
                                CBH = Event.FuelTypeParms[fuelIndex].CBH;
                        }
                    }
                }


            //If mixed, use weighted average CBH
            if(PH > 0 && PC > 0 && PDF <= 0)
            {
                int decidIndex = SiteVars.DecidFuelType[site];
                if (decidIndex > 0)
                {
                    int decidCBH = Event.FuelTypeParms[decidIndex].CBH;
                    CBH = ((CBH * PC) + (decidCBH * PH)) / 100;
                }

            }
            int FFMC = fireEvent.FFMC;
            int BUI = fireEvent.BuildUpIndex;
            double SFC = SurfaceFuelConsumption(fuelIndex, FFMC, BUI, PH, PDF);
            if (PH > 0 && PC > 0 && PDF <= 0)
            {
                int decidIndex = SiteVars.DecidFuelType[site];
                double decidSFC = SurfaceFuelConsumption(decidIndex, FFMC, BUI, PH, PDF);
                SFC = ((SFC * PC) + (decidSFC * PH)) / 100;
            }
            int severity = 0;

            //-----Edited by BRM-----
            //double ROS = SiteVars.RateOfSpread[site];
            //double ROS = SiteVars.AdjROS[site];

            double ROS = (SiteVars.AdjROS[site] + (SiteVars.RateOfSpread[site] * severityCalibrate)) / (1 + severityCalibrate);
            //----------
            double CSI = 0.001 * Math.Pow(CBH,1.5) * Math.Pow((460 + 25.9 * FMC),1.5);
            double RSO = CSI / (300 * SFC);
            double CFB = 1 - Math.Exp(-0.23 * (ROS - RSO));


            // Finally, calculate SEVERITY:
            double lowThreshold = (RSO + 0.458)/2.0;

            // TEMPORARY?  This seems to help adjust severities 1 and 2 so that there isn't so much of 1 and little of 2.
            lowThreshold *= severityCalibrate;

            if (CFB >= 0.9) severity = 5;
            if (CFB < 0.9 && CFB >= 0.495) severity = 4;
            if (CFB < 0.495 && CFB >= 0.1) severity = 3;
            if (CFB < 0.1 && ROS >= lowThreshold) severity = 2;
            if (CFB < 0.1 && ROS < lowThreshold) severity = 1;

            //PlugIn.ModelCore.Log.WriteLine("      Severity = {0}.  CSI={1}, RSO={2}, ROS={3}, CFB={4}.", severity, CSI, RSO, ROS, CFB);

            return severity;
        }

        ///<summary>
        /// This method calculates the surface fuel consumption.
        ///</summary>
        public static double SurfaceFuelConsumption(int fuelIndex, int FFMC, int BUI, int PH, int PDF)
        {
            double SFC = 0.0;

            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C1)
            {
                //SFC = 1.5 * (1.0 - Math.Exp(-0.223 * ((double) FFMC - 81)));
                SFC = CalculateFC(1.5, 0.223, FFMC - 81, 1.0);
                if (SFC < 0) SFC = 0;

            }

            if ((Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C2) ||
                (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.M3) ||
                (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.M4))
            {
                //SFC = 5.0 * (1.0 - Math.Exp(-0.0115 * (double) BUI));
                SFC = CalculateFC(5.0, 0.0115, BUI, 1.0);
            }
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C3 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C4)
            {
                //SFC = 5.0 * Math.Pow((1.0 - Math.Exp(-0.0164 * (double) BUI)), 2.24);
                SFC = CalculateFC(5.0, 0.0164, BUI, 2.24);
            }
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C5 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C6)
            {
                //SFC = 5.0 * Math.Pow((1.0 - Math.Exp(-0.0149 * (double)BUI)), 2.48);
                SFC = CalculateFC(5.0, 0.0149, BUI, 2.48);
            }
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C7)
            {
                //double FFC = 2.0 * (1.0 - Math.Exp(-0.104 * ((double)FFMC - 70)));
                double FFC = CalculateFC(2.0, 0.104, FFMC - 70, 1.0);
                if (FFC < 0) FFC = 0;

                //double WFC = 1.5 * (1.0 - Math.Exp(-0.0201 * (double)BUI));
                double WFC = CalculateFC(1.5, 0.0201, BUI, 1.0);
                SFC = FFC + WFC;

            }
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.D1)
            {
                //SFC = 1.5 * (1.0 - Math.Exp(-0.0183 * (double)BUI));
                SFC = CalculateFC(1.5, 0.0183, BUI, 1.0);
            }
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1a ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1b)
            {
                SFC = 0.3;
            }
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.S1)
            {
                //double FFC = 4.0 * (1.0 - Math.Exp(-0.025 * (double)BUI));
                double FFC = CalculateFC(4.0, 0.025, BUI, 1.0);

                //double WFC = 4.0 * (1.0 - Math.Exp(-0.034 * (double)BUI));
                double WFC = CalculateFC(4.0, 0.034, BUI, 1.0);
                SFC = FFC + WFC;
            }
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.S2)
            {
                //double FFC = 10.0 * (1.0 - Math.Exp(-0.013 * (double)BUI));
                double FFC = CalculateFC(10.0, 0.013, BUI, 1.0);

                //double WFC = 6.0 * (1.0 - Math.Exp(-0.060 * (double)BUI));
                double WFC = CalculateFC(6.0, 0.060, BUI, 1.0);
                SFC = FFC + WFC;
            }
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.S3)
            {
                //double FFC = 12.0 * (1.0 - Math.Exp(-0.0166 * (double)BUI));
                double FFC = CalculateFC(12.0, 0.0166, BUI, 1.0);

                //double WFC = 20.0 * (1.0 - Math.Exp(-0.021 * (double)BUI));
                double WFC = CalculateFC(20.0, 0.021, BUI, 1.0);
                SFC = FFC + WFC;
            }

            return SFC;
        }

        private static double CalculateFC(double multiplier, double exp1, int exp2, double power)
        {
            double FC = multiplier * Math.Pow(1.0 - Math.Exp(-1 * exp1 * (double) exp2), power);
            return FC;
        }

    }
}
