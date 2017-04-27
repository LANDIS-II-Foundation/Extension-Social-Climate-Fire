//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using System.Data;
using System;
using System.IO;
using System.Collections.Generic;

namespace Landis.Extension.DynamicFire
{

    public class InputWindData
    {

        //---------------------------------------------------------------------

        public static ISeasonParameters GetSeason(ISeasonParameters[] seasons, int day)
        {
            ISeasonParameters theSeason = seasons[0];
            foreach (ISeasonParameters season in seasons)
            {
                if (season.NameOfSeason == SeasonName.Spring && day < season.StartDay)
                {
                    string mesg = string.Format("Error: The fire day {0} is before the beginning of spring", day);
                    throw new System.ApplicationException(mesg);
                }

                if (day < season.EndDay)
                    theSeason = season;
            }
            return theSeason;
        }

        //---------------------------------------------------------------------

        public static int GenerateWindSpeed(System.Data.DataSet weatherDS)
        {
            int windSpeed = 0;

            foreach (DataRow myDataRow in weatherDS.Tables["Table"].Rows)
            {

                windSpeed = (int) Math.Round(Convert.ToDouble(myDataRow["WindSpeedVelocity"]));  //Alec: I changed this to "WindSpeedVelocity" to match column names below
                //PlugIn.ModelCore.Log.WriteLine("   New Event WSV:  {0}", windSpeed.ToString());
            }

            return windSpeed;
        }
         //---------------------------------------------------------------------

        public static int GenerateWindSpeed(System.Data.DataRow weatherRow)
        {
            int windSpeed = (int) Math.Round(Convert.ToDouble(weatherRow["WindSpeedVelocity"]));

            return windSpeed;
        }

        //---------------------------------------------------------------------

        public static int GenerateWindDirection(System.Data.DataRow weatherRow)
        {
            int windDir = (int) weatherRow["WindAzimuth"];

            return windDir;
        }
        //---------------------------------------------------------------------

        //public static int GenerateFineFuelMoistureCode(System.Data.DataSet weatherDS)
        //{
        //    int FFMC = 0;

        //    foreach (DataRow myDataRow in weatherDS.Tables["Table"].Rows)
        //    {
        //        FFMC = (int) Math.Round(Convert.ToDouble(myDataRow["FFMC"]));
        //        //PlugIn.ModelCore.Log.WriteLine("   New Event FFMC:  {0}", FFMC.ToString());
        //    }

        //    return FFMC;
        //}
        //---------------------------------------------------------------------

        //public static int GenerateFineFuelMoistureCode(System.Data.DataRow weatherRow)
        //{
        //    int FFMC = (int) Math.Round(Convert.ToDouble(weatherRow["FFMC"]));

        //    return FFMC;
        //}
        ////---------------------------------------------------------------------

        //public static int GenerateBuildUpIndex(System.Data.DataSet weatherDS)
        //{
        //    int BUI = 0;

        //    foreach (DataRow myDataRow in weatherDS.Tables["Table"].Rows)
        //    {
        //        BUI = (int)Math.Round(Convert.ToDouble(myDataRow["BUI"]));
        //        //PlugIn.ModelCore.Log.WriteLine("   New Event BUI:  {0}", BPlugIn.ModelCore.Log.ToString());
        //    }

        //    return  BUI;
        //}
        // //---------------------------------------------------------------------

        //public static int GenerateBuildUpIndex(System.Data.DataRow weatherRow)
        //{
        //    int BUI = (int) Math.Round(Convert.ToDouble(weatherRow["BUI"]));

        //    return  BUI;
        //}
        //---------------------------------------------------------------------

        //public static double CalculateFuelMoistureEffect(double FFMC)
        //{
        //    // FBP 45 and 46:
        //    double f_F = 0.0;
        //    double m = (147.2 * (101 - FFMC)) / (59.5 + FFMC);

        //    f_F = 91.9 * System.Math.Exp(-0.1386 * m) *
        //                    (1 + (System.Math.Pow(m, 5.31) / 49300000));
        //    return f_F;
        //}
        //---------------------------------------------------------------------
        public static double CalculateWindEffect(double WSV)
        {
            // FBP 53 and 53a:
            double f_W = 0.0;
            if(WSV  <= 40)  //0.06 = convert back to km/hr
                f_W = System.Math.Exp(0.05039 * WSV);
            else
                f_W = 12 * (1 - System.Math.Exp(-0.0818 * (WSV - 28)));
            return f_W;
        }
        //---------------------------------------------------------------------
        public static double CalculateBackWindEffect(double WSV)
        {
            // FBP 75:
            double f_W = System.Math.Exp(-1 * 0.05039 * WSV);
            return f_W;
        }
        //---------------------------------------------------------------------

        private static double GenerateRandomNum(Distribution dist, double parameter1, double parameter2)
        {
            double randomNum = 0.0;
            if(dist == Distribution.normal)
            {
                //NormalDistribution randVar = new NormalDistribution(RandomNumberGenerator.Singleton);
                PlugIn.ModelCore.NormalDistribution.Mu = parameter1;      // mean
                PlugIn.ModelCore.NormalDistribution.Sigma = parameter2;   // std dev
                PlugIn.ModelCore.NormalDistribution.NextDouble();
            }
            if(dist == Distribution.lognormal)
            {
                //LognormalDistribution randVar = new LognormalDistribution(RandomNumberGenerator.Singleton);
                PlugIn.ModelCore.LognormalDistribution.Mu = parameter1;      // mean
                PlugIn.ModelCore.LognormalDistribution.Sigma = parameter2;   // std dev
                PlugIn.ModelCore.LognormalDistribution.NextDouble();
            }
            if(dist == Distribution.gamma)
            {
                //GammaDistribution randVar = new GammaDistribution(RandomNumberGenerator.Singleton);
                PlugIn.ModelCore.GammaDistribution.Alpha = parameter1;      // mean
                PlugIn.ModelCore.GammaDistribution.Theta = parameter2;   // std dev
                PlugIn.ModelCore.GammaDistribution.NextDouble();
            }
            if(dist == Distribution.Weibull)
            {
                //WeibullDistribution randVar = new WeibullDistribution(RandomNumberGenerator.Singleton);
                PlugIn.ModelCore.WeibullDistribution.Alpha = parameter1;      // mean
                PlugIn.ModelCore.WeibullDistribution.Lambda = parameter2;   // std dev
                PlugIn.ModelCore.WeibullDistribution.NextDouble();
            }
            return randomNum;
        }
        //---------------------------------------------------------------------

        public static DataRow GenerateSeasonWindData(ISeasonParameters season) 
        {

            int weatherRandomizer = PlugIn.WeatherRandomizer;
            //string seasonName = season.NameOfSeason.ToString();
            //string ecoName = fire_region.Name;
            //int weatherBin = 0;

            int seasonStart = season.StartDay;
            int seasonEnd = season.EndDay;

            string selectString = "Day >= " + seasonStart + " AND Day <= " + seasonEnd;
            DataRow[] rows = PlugIn.WindDataTable.Select(selectString);

            if (rows.Length > 0)
            {
                int newRandNum = (int)(Math.Round(PlugIn.ModelCore.GenerateUniform() * (rows.Length - 1)));
                DataRow weatherRow = rows[newRandNum];

                return weatherRow;
            }
            else
            {
                return null;
            }
        }
        //---------------------------------------------------------------------

        //public static int GenerateFMC(ISeasonParameters season, IDynamicInputRecord fire_region)
        //{

        //    int FMC = 0;
        //    if (season.NameOfSeason == SeasonName.Spring)
        //    {
        //        if (PlugIn.ModelCore.GenerateUniform() < fire_region.SpringFMCHiProp)
        //            FMC = fire_region.SpringFMCHi;
        //        else
        //            FMC = fire_region.SpringFMCLo;
        //    }
        //    if (season.NameOfSeason == SeasonName.Summer)
        //    {
        //        if (PlugIn.ModelCore.GenerateUniform() < fire_region.SummerFMCHiProp)
        //            FMC = fire_region.SummerFMCHi;
        //        else
        //            FMC = fire_region.SummerFMCLo;
        //    }
        //    if (season.NameOfSeason == SeasonName.Fall)
        //    {
        //        if (PlugIn.ModelCore.GenerateUniform() < fire_region.FallFMCHiProp)
        //            FMC = fire_region.FallFMCHi;
        //        else
        //            FMC = fire_region.FallFMCLo;
        //    }
        //    return FMC;
        //}
        //---------------------------------------------------------------------

        public static DataTable ReadWindFile(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("   Loading Wind Input Data...");

            CSVParser windParser = new CSVParser();

            DataTable windTable = windParser.ParseToDataTable(path);

            string selectText = ("Day > 0 AND Day < 365");
            DataRow[] foundRows = windTable.Select(selectText);

            if (foundRows.Length > 0)
            {
                //Input validation
                double WSV;
                int WINDDIR;
                for (int j = 0; j < foundRows.Length; j++)
                {
                    DataRow myDataRow = foundRows[j];

                    WSV = Convert.ToDouble(myDataRow["WindSpeedVelocity"]);
                    if (WSV < 0.0)
                    {
                        throw new System.ApplicationException("Error: Wind Speed < 0:  Day = " + myDataRow["Day"]);
                    }
                    WINDDIR = (int)myDataRow["WindAzimuth"];
                    if (WINDDIR < 0)
                    {
                        throw new System.ApplicationException("Error: WINDDIR < 0:  Day = " + myDataRow["Day"]);
                    }
                    else if (WINDDIR > 360)
                    {
                        throw new System.ApplicationException("Error: WINDDIR > 360:  Day = " + myDataRow["Day"]);
                    }
                }

            }

            return windTable;
        }
    }
}
