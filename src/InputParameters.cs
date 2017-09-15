//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.Scrapple
{


    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public interface IInputParameters
    {
        int Timestep{get;set;}
        string ClimateConfigFile { get; set; }    
        double RelativeHumiditySlopeAdjustment { get; set; }   //does this go in the interface or below in the input parameters?
        //double SeverityCalibrate { get;set;}
        List<IFireDamage> FireDamages{get;}
        string MapNamesTemplate{get;set;}
        //int Duration { get; set; }
        //int SpringStart { get; set; }
        //int WinterStart { get; set; }
        string LighteningFireMap { get; set; }
        string RxFireMap { get; set; }
        string AccidentalFireMap { get; set; }
        double LightningIgnitionB0 { get; set; }
        double LightningIgnitionB1 { get; set; }
        double RxFireIgnitionB0 { get; set; }
        double RxFireIgnitionB1 { get; set; }
        double AccidentalFireIgnitionB0 { get; set; }
        double AccidentalFireIgnitionB1 { get; set; }
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
        
        private List<IFireDamage> damages;
        private string mapNamesTemplate;
        //private string logFileName;
        //private string summaryLogFileName;
        private string climateConfigFile;
        private double relativeHumiditySlopeAdjust;
        //private int springStart;
        //private int winterStart;
        //private int duration;
        private string lighteningFireMap;
        private string accidentalFireMap;
        private string rxFireMap;
        private double lightningIgnitionB0;
        private double lightningIgnitionB1;
        private double rxFireIgnitionB0;
        private double rxFireIgnitionB1;
        private double accidentalFireIgnitionB0;
        private double accidentalFireIgnitionB1;



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
        public string ClimateConfigFile
        {
            get
            {
                return climateConfigFile;
            }
            set
            {
                if (value != null)
                {
                    ValidatePath(value);
                }
                climateConfigFile = value;
            }
        }
        //---------------------------------------------------------------------
        //public int Duration
        //{
        //    get
        //    {
        //        return duration;
        //    }
        //    set
        //    {
        //        duration = value;
        //    }
        //}
        ////---------------------------------------------------------------------
        //public int WinterStart
        //{
        //    get
        //    {
        //        return winterStart;
        //    }
        //    set
        //    {
        //        winterStart = value;
        //    }
        //}
        ////---------------------------------------------------------------------
        //public int SpringStart
        //{
        //    get
        //    {
        //        return springStart;
        //    }
        //    set
        //    {
        //        springStart = value;
        //    }
        //}
        //---------------------------------------------------------------------

        public double RelativeHumiditySlopeAdjustment
        {
            get
            {
                return relativeHumiditySlopeAdjust;
            }
            set
            {
                if (value < 0.0 || value > 100.0)
                    throw new InputValueException(value.ToString(), "Relative Humidity Slope Adjustment must be > 0.0 and < 50");
                relativeHumiditySlopeAdjust = value;
            }
        }

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

        //---------------------------------------------------------------------
        public string LighteningFireMap
        {
            get
            {
                return lighteningFireMap;
            }
            set
            {
                lighteningFireMap = value;
            }
        }
        //---------------------------------------------------------------------
        public string RxFireMap
        {
            get
            {
                return rxFireMap;
            }
            set
            {
                rxFireMap = value;
            }
        }
        //---------------------------------------------------------------------
        public string AccidentalFireMap
        {
            get
            {
                return accidentalFireMap;
            }
            set
            {
                accidentalFireMap = value;
            }
        }

        //---------------------------------------------------------------------
        public double LightningIgnitionB0
        {
            get
            {
                return lightningIgnitionB0;
            }
            set
            {
                lightningIgnitionB0 = value;
            }
        }

        //---------------------------------------------------------------------
        public double LightningIgnitionB1
        {
            get
            {
                return lightningIgnitionB1;
            }
            set
            {
                lightningIgnitionB1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double RxFireIgnitionB0
        {
            get
            {
                return rxFireIgnitionB0;
            }
            set
            {
                rxFireIgnitionB0 = value;
            }
        }

        //---------------------------------------------------------------------
        public double RxFireIgnitionB1
        {
            get
            {
                return rxFireIgnitionB1;
            }
            set
            {
                rxFireIgnitionB1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double AccidentalFireIgnitionB0
        {
            get
            {
                return accidentalFireIgnitionB0;
            }
            set
            {
                accidentalFireIgnitionB0 = value;
            }
        }

        //---------------------------------------------------------------------
        public double AccidentalFireIgnitionB1
        {
            get
            {
                return accidentalFireIgnitionB1;
            }
            set
            {
                accidentalFireIgnitionB1 = value;
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
        //---------------------------------------------------------------------

        public InputParameters()
        {
            damages = new List<IFireDamage>();
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
