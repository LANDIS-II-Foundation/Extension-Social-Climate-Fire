//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.Scrapple
{

    //public enum SizeType {size_based, duration_based};
    public enum Distribution {gamma, lognormal, normal, Weibull};

    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public interface IInputParameters
    {
        int Timestep{get;set;}
        //string ClimateConfigFile { get; set; }    
        double RelativeHumiditySlopeAdjustment { get; set; }   //does this go in the interface or below in the input parameters?
        //SizeType FireSizeType{get;set;}

        //bool BUI{get;set;}
        double SeverityCalibrate { get;set;}
        //List<IDynamicFireRegion> DynamicFireRegions {get;}
        //List<IDynamicWeather> DynamicWeather { get;}
        //ISeasonParameters[] SeasonParameters{get;}
        //IFuelType[] FuelTypeParameters{get;}
        List<IFireDamage> FireDamages{get;}
        string MapNamesTemplate{get;set;}
//        string InitialWeatherPath{get;set;}
//        string WindInputPath { get; set; }
//        string DynamicFireRegionInputFile { get; set; }
    }
}

namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {
        private int timestep;
        
//        private SizeType fireSizeType;
        
        //private bool buildUpIndex;
        private double severityCalibrate;
        //private List<IDynamicFireRegion> dynamicFireRegions;
        //private List<IDynamicWeather> dynamicWeather;
        //private ISeasonParameters[] seasons;
        //private IFuelType[] fuelTypeParameters;
        private List<IFireDamage> damages;
        private string mapNamesTemplate;
        //private string logFileName;
        //private string summaryLogFileName;
        //private string initialWeatherPath;
        //private string windInputPath;
        //private string dynamicFireRegionInputFile;
        //private string climateConfigFile;
        private double relativeHumiditySlopeAdjust;


        //---------------------------------------------------------------------

        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                timestep = value;
            }
        }
        //---------------------------------------------------------------------
        //public string ClimateConfigFile
        //{
        //    get
        //    {
        //        return climateConfigFile;
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            ValidatePath(value);
        //        }
        //        climateConfigFile = value;
        //    }
        //}
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        public double RelativeHumiditySlopeAdjustment
        {
            get
            {
                return relativeHumiditySlopeAdjust;
            }
            set
            {
                if (value < 0.0 || value > 50.0)
                    throw new InputValueException(value.ToString(), "Relative Humidity Slope Adjustment must be > 0.0 and < 50");
                relativeHumiditySlopeAdjust = value;
            }
        }
        ////---------------------------------------------------------------------
        //public SizeType FireSizeType
        //{
        //    get {
        //        return fireSizeType;
        //    }
        //    set {
        //        fireSizeType = value;
        //    }
        //}
        ////---------------------------------------------------------------------
        
        //public bool BUI
        //{
        //    get {
        //        return buildUpIndex;
        //    }
        //    set {
        //        buildUpIndex = value;
        //    }
        //}

        ////---------------------------------------------------------------------
        ///*
        //public int WeatherRandomizer
        //{
        //    get {
        //        return weatherRandomizer;
        //    }
        //}*/
        ////---------------------------------------------------------------------
        
        //public double SeverityCalibrate
        //{
        //    get {
        //        return severityCalibrate;
        //    }
        //    set
        //    {
        //        severityCalibrate = value;
        //    }
        //}
        
        ////---------------------------------------------------------------------
        
        //public List<IDynamicFireRegion> DynamicFireRegions
        //{
        //    get {
        //        return dynamicFireRegions;
        //    }
        //}
        ////---------------------------------------------------------------------
        //public List<IDynamicWeather> DynamicWeather
        //{
        //    get
        //    {
        //        return dynamicWeather;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public ISeasonParameters[] SeasonParameters
        //{
        //    get {
        //        return seasons;
        //    }
        //    set {
        //        seasons = value;
        //    }
         
        //}
        ////---------------------------------------------------------------------
        //public IFuelType[] FuelTypeParameters
        //{
        //    get {
        //        return fuelTypeParameters;
        //    }
        //}

        //---------------------------------------------------------------------
        public List<IFireDamage> FireDamages
        {
            get {
                return damages;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        public string MapNamesTemplate
        {
            get {
                return mapNamesTemplate;
            }
            set {
                    MapNames.CheckTemplateVars(value);
                mapNamesTemplate = value;
            }
        }


        ////---------------------------------------------------------------------

        ///// <summary>
        ///// Weather input file
        ///// </summary>
        //public string InitialWeatherPath
        //{
        //    get {
        //        return initialWeatherPath;
        //    }
        //    set {
        //            // FIXME: check for null or empty path (value);
        //        initialWeatherPath = value;
        //    }
        //}
        /// <summary>
        /// Wind input file
        /// </summary>
        //public string WindInputPath
        //{
        //    get
        //    {
        //        return windInputPath;
        //    }
        //    set
        //    {
        //        // FIXME: check for null or empty path (value);
        //        windInputPath = value;
        //    }
        //}
        ////---------------------------------------------------------------------
        ///// <summary>
        ///// Input file for the dynamic inputs
        ///// </summary>
        //public string DynamicFireRegionInputFile
        //{
        //    get
        //    {
        //        return dynamicFireRegionInputFile;
        //    }
        //    set
        //    {
        //        dynamicFireRegionInputFile = value;
        //    }
        //}
        //---------------------------------------------------------------------

        public InputParameters()
        {
//            seasons = new SeasonParameters[3];
            damages = new List<IFireDamage>();
//            dynamicFireRegions = new List<IDynamicFireRegion>();
            //dynamicWeather = new List<IDynamicWeather>();
            
            //fuelTypeParameters = new FuelType[100];
            //for(int i=0; i<100; i++)
            //    fuelTypeParameters[i] = new FuelType();
        }
        //---------------------------------------------------------------------

        private void ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new InputValueException();
            if (path.Trim(null).Length == 0)
                throw new InputValueException(path,
                                              "\"{0}\" is not a valid path.",
                                              path);
        }
    }
}
