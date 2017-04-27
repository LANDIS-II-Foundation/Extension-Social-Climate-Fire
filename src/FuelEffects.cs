//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Core;
using System.Collections.Generic;
using System;

namespace Landis.Extension.Scrapple
{
    public class FuelEffects
    {

        // ---------------------------------------------------------------------
        // This method calculates the initial rate of spread for a specific site.
        // See below for the method that estimates the initial rate of spread for a broad area.
        
        public static double InitialRateOfSpread(double ISI, ISeasonParameters season, Site site, bool secondRegionMap)
        {   

            
            int fuelIndex = SiteVars.CFSFuelType[site];
            if (secondRegionMap)
                fuelIndex = SiteVars.CFSFuelType2[site];
            int PC = SiteVars.PercentConifer[site];
            int PH = SiteVars.PercentHardwood[site];
            int PDF = SiteVars.PercentDeadFir[site];
            
            //PlugIn.ModelCore.Log.WriteLine("Fuel Type Code = {0}.", siteFuelType.ToString());
            
            double RSI = 0.0;  
            
            //if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Conifer ||
                //Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.ConiferPlantation)
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C1 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C2 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C3 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C4 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C5 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C6 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.C7)
            {
                double a = Event.FuelTypeParms[fuelIndex].A;
                double b = Event.FuelTypeParms[fuelIndex].B;
                double c = Event.FuelTypeParms[fuelIndex].C;
                
                double percentHard = (double) PH / 100.0;
                double percentConi = (double) PC / 100.0;
                
                RSI = CalculateRSI(a, b, c, ISI);
                

                if (PDF > 0)
                {
                    if (PH > 0 && season.LeafStatus == LeafOnOff.LeafOn) //M-4
                    {
                        a = 140 * Math.Exp((-1) * 35.5 / (double) PDF);
                        b = 0.0404;
                        c = 3.02 * Math.Exp((-1) * 0.00714 * (double) PDF);
                    }
                    else  //M-3
                    {
                        a = 170 * Math.Exp((-1) * 35 / (double) PDF);
                        b = 0.082 * Math.Exp((-1) * 36 / (double)PDF);
                        c = 1.698 - (0.00303 * PDF);
                    }
                    double MRSI = CalculateRSI(a, b, c, ISI);
                    if (MRSI > RSI)
                        RSI = MRSI;
                }
                
                // These are the classic MIXED CONIFER + DECIDUOUS
                else if (PH > 0)
                {
                    double RSIconifer = RSI;
                    int dIndex = SiteVars.DecidFuelType[site]; //(int)FuelTypeCode.D1;
                    double RSIdecid = CalculateRSI(Event.FuelTypeParms[dIndex].A, Event.FuelTypeParms[dIndex].B, Event.FuelTypeParms[dIndex].C, ISI);

                    if (season.LeafStatus == LeafOnOff.LeafOn)  //M-2
                    {
                        RSI = ((1 - percentHard) * RSIconifer) + (0.2 * percentHard * RSIdecid);
                    }
                    else  //M-1
                    {
                        RSI = ((1 - percentHard) * RSIconifer) + (percentHard * RSIdecid);
                    }

                    //PlugIn.ModelCore.Log.WriteLine("Calculating ROSi for a MIXED type. PH={0}, PC={1}, LeafStatus={2}.", PH, PC, season.LeafStatus);
                    //PlugIn.ModelCore.Log.WriteLine("  RSIcon={0:0.0}, RSIdecid={1:0.0}, RSImix={2:0.000}.", RSIconifer, RSIdecid, RSI);

                }
            }
                

            //if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Open)
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1a || Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.O1b)
            //siteFuelType == FuelTypeCode.O1a)
            {
                double a, b, c;
                int percentCuring = season.PercentCuring;
                
                if(season.NameOfSeason == SeasonName.Spring)  //O1a
                {
                    a = 190; //Event.FuelTypeParms[(int)FuelTypeCode.O1a].A;
                    b = 0.0310; //Event.FuelTypeParms[(int)FuelTypeCode.O1a].B;
                    c = 1.4; //Event.FuelTypeParms[(int)FuelTypeCode.O1a].C;
                }
                else   //O1b
                {
                    a = 250; //Event.FuelTypeParms[(int)FuelTypeCode.O1b].A;
                    b = 0.0350; //Event.FuelTypeParms[(int)FuelTypeCode.O1b].B;
                    c = 1.7; //Event.FuelTypeParms[(int)FuelTypeCode.O1b].C;
                }
                
                double CF = (0.02 * percentCuring) - 1.0;
                RSI = CalculateRSI(a, b, c, ISI);
                if(percentCuring > 50) 
                    RSI *= CF;
                else
                    RSI = 0;
                if (PDF > 0)
                {
                    if (season.LeafStatus == LeafOnOff.LeafOn) //M-4
                    {
                        a = 140 * Math.Exp((-1) * 35.5 / (double)PDF);
                        b = 0.0404;
                        c = 3.02 * Math.Exp((-1) * 0.00714 * (double)PDF);
                    }
                    else //M-3
                    {
                        a = 170 * (Math.Exp(((-1) * 35) / (double)PDF));
                        b = 0.082 * (Math.Exp(((-1) * 36) / (double)PDF));
                        c = 1.698 - (0.00303 * (double)PDF);
                    }
                    double MRSI = CalculateRSI(a, b, c, ISI);
                    if (MRSI > RSI)
                        RSI = MRSI;
                }
            }
            
            //if(Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.NoFuel || fuelIndex == 0)
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.NoFuel || fuelIndex == 0)
            //siteFuelType == FuelTypeCode.NoFuel)
            {
                if (PDF > 0)
                {
                    double a=0, b=0, c=0;
                    if (season.LeafStatus == LeafOnOff.LeafOn) //M-4
                    {
                        a = 140 * Math.Exp((-1) * 35.5 / (double)PDF);
                        b = 0.0404;
                        c = 3.02 * Math.Exp((-1) * 0.00714 * (double)PDF);
                    }
                    else //M-3
                    {
                        a = 170 * (Math.Exp(((-1) * 35) / (double)PDF));
                        b = 0.082 * (Math.Exp(((-1) * 36) / (double)PDF));
                        c = 1.698 - (0.00303 * (double)PDF);
                    }
                    RSI = CalculateRSI(a, b, c, ISI);
                }

                else
                {
                    return 0;
                }
            }

            //if( Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Slash || 
                //Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Deciduous)
            if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.S1 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.S2 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.S3 ||
                Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.D1)
            {
                //PlugIn.ModelCore.Log.WriteLine("Calculating ROSi for a DECIDUOUS or SLASH type.");

                double a = Event.FuelTypeParms[fuelIndex].A;
                double b = Event.FuelTypeParms[fuelIndex].B;
                double c = Event.FuelTypeParms[fuelIndex].C;
                    
                RSI = CalculateRSI(a, b, c, ISI);

                //if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Deciduous 
                if(Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.D1 
                    && season.LeafStatus == LeafOnOff.LeafOn)
                //if(siteFuelType == FuelTypeCode.D1 && season.LeafStatus == LeafOnOff.LeafOn)
                    RSI *= 0.2;
                
                if (PDF > 0)
                {
                    //if (Event.FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Deciduous
                    if (Event.FuelTypeParms[fuelIndex].SurfaceFuel == SurfaceFuelType.D1 
                        && season.LeafStatus == LeafOnOff.LeafOn)  //M-4
                    //siteFuelType == FuelTypeCode.D1) 
                    {
                        a = 140 * Math.Exp((-1) * 35.5 / (double) PDF);
                        b = 0.0404;
                        c = 3.02 * Math.Exp((-1) * 0.00714 * (double) PDF);
                    }
                    else //M-3
                    {
                        a = 170 * (Math.Exp(((-1) * 35) / (double) PDF));
                        b = 0.082 * (Math.Exp(((-1) * 36 )/ (double) PDF));
                        c = 1.698 - (0.00303 * (double) PDF);
                    }
                    double MRSI = CalculateRSI(a, b, c, ISI);
                    if (MRSI > RSI)
                        RSI = MRSI;
                }
            }

            
            return RSI;
        }


        public static double CalculateRSI(double a, double b, double c, double ISI)
        {
            double RSI = a * System.Math.Pow((1 - System.Math.Exp(-1 * b * ISI)), c);
            return RSI;
            
        }

        //---------------------------------------------------------------------
        
        public static double FuelMoistureBuildupIndex(double bui, double q, double bui_0)
        {

            double buildupEffect = 1.0;  
            buildupEffect = System.Math.Exp(50 * System.Math.Log(q) * ((1/bui)-(1/bui_0)));
            return buildupEffect;
            
        }
        
    }
}
