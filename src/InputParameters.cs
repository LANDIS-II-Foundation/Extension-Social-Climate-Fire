//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using Landis.Core;

namespace Landis.Extension.Scrapple
{


    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public interface IInputParameters
    {
        int Timestep{get;set;}
        List<IFireDamage> FireDamages_Severity1{get;}
        List<IFireDamage> FireDamages_Severity2 { get; }
        List<IFireDamage> FireDamages_Severity3 { get; }
        //string MapNamesTemplate {get;set;}

        string LighteningFireMap { get; set; }
        string RxFireMap { get; set; }
        string AccidentalFireMap { get; set; }

        string LighteningSuppressionMap { get; set; }
        string RxSuppressionMap { get; set; }
        string AccidentalSuppressionMap { get; set; }

        double LightningIgnitionB0 { get; set; }
        double LightningIgnitionB1 { get; set; }
        //double RxFireIgnitionB0 { get; set; }
        //double RxFireIgnitionB1 { get; set; }
        double AccidentalFireIgnitionB0 { get; set; }
        double AccidentalFireIgnitionB1 { get; set; }
        double MaxFineFuels { get; set; }
        double MaxRxWindSpeed { get; set; }
        double MaxRxFireWeatherIndex { get; set; }
        double MinRxFireWeatherIndex { get; set; }
        int NumberRxAnnualFires { get; set; }

        double MaximumSpreadAreaB0 { get; set; }
        double MaximumSpreadAreaB1 { get; set; }
        double MaximumSpreadAreaB2 { get; set; }

        int LadderFuelMaxAge { get; set; }
        double SeverityFactor_LadderFuelPercentage { get; set; }
        double SeverityFactor_FineFuelPercentage { get; set; }

        int LightningSuppressEffectivenss_low { get; set; }
        int LightningSuppressEffectivenss_medium { get; set; }
        int LightningSuppressEffectivenss_high { get; set; }
        int RxSuppressEffectivenss_low { get; set; }
        int RxSuppressEffectivenss_medium { get; set; }
        int RxSuppressEffectivenss_high { get; set; }
        int AccidentalSuppressEffectivenss_low { get; set; }
        int AccidentalSuppressEffectivenss_medium { get; set; }
        int AccidentalSuppressEffectivenss_high { get; set; }

        List<ISpecies> LadderFuelSpeciesList { get; }
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
        
        private List<IFireDamage> damages_severity1;
        private List<IFireDamage> damages_severity2;
        private List<IFireDamage> damages_severity3;
        //private string mapNamesTemplate;
        private string lighteningFireMap;
        private string accidentalFireMap;
        private string rxFireMap;

        private string lighteningSuppressionMap;
        private string accidentalSuppressionMap;
        private string rxSuppressionMap;

        private double lightningIgnitionB0;
        private double lightningIgnitionB1;
        //private double rxFireIgnitionB0;
        //private double rxFireIgnitionB1;
        private double accidentalFireIgnitionB0;
        private double accidentalFireIgnitionB1;
        private double maxFineFuels;
        private double maxRxWindSpeed;
        private double maxRxFireWeatherIndex;
        private double minRxFireWeatherIndex;
        private int numberRxAnnualFires;

        private double maximumSpreadAreaB0;
        private double maximumSpreadAreaB1;
        private double maximumSpreadAreaB2;

        private int ladderFuelMaxAge;
        private double severityFactor_LadderFuelPercentage;
        private double severityFactor_FineFuelPercentage;

        private int lightningSuppressEffectivenss_low;
        private int lightningSuppressEffectivenss_medium;
        private int lightningSuppressEffectivenss_high;
        private int rxSuppressEffectivenss_low;
        private int rxSuppressEffectivenss_medium;
        private int rxSuppressEffectivenss_high;
        private int accidentalSuppressEffectivenss_low;
        private int accidentalSuppressEffectivenss_medium;
        private int accidentalSuppressEffectivenss_high;

        private List<ISpecies> ladderFuelSpeciesList;



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
        public List<IFireDamage> FireDamages_Severity1
        {
            get {
                return damages_severity1;
            }
        }

        //---------------------------------------------------------------------
        public List<IFireDamage> FireDamages_Severity2
        {
            get
            {
                return damages_severity2;
            }
        }
        //---------------------------------------------------------------------
        public List<IFireDamage> FireDamages_Severity3
        {
            get
            {
                return damages_severity3;
            }
        }
        //---------------------------------------------------------------------
        public List<ISpecies> LadderFuelSpeciesList
        {
            get
            {
                return ladderFuelSpeciesList;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        //public string MapNamesTemplate
        //{
        //    get {
        //        return mapNamesTemplate;
        //    }
        //    set {
        //            MapNames.CheckTemplateVars(value);
        //        mapNamesTemplate = value;
        //    }
        //}

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
        public string LighteningSuppressionMap
        {
            get
            {
                return lighteningSuppressionMap;
            }
            set
            {
                lighteningSuppressionMap = value;
            }
        }
        //---------------------------------------------------------------------
        public string RxSuppressionMap
        {
            get
            {
                return rxSuppressionMap;
            }
            set
            {
                rxSuppressionMap = value;
            }
        }
        //---------------------------------------------------------------------
        public string AccidentalSuppressionMap
        {
            get
            {
                return accidentalSuppressionMap;
            }
            set
            {
                accidentalSuppressionMap = value;
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
        //public double RxFireIgnitionB0
        //{
        //    get
        //    {
        //        return rxFireIgnitionB0;
        //    }
        //    set
        //    {
        //        rxFireIgnitionB0 = value;
        //    }
        //}

        ////---------------------------------------------------------------------
        //public double RxFireIgnitionB1
        //{
        //    get
        //    {
        //        return rxFireIgnitionB1;
        //    }
        //    set
        //    {
        //        rxFireIgnitionB1 = value;
        //    }
        //}
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

        //---------------------------------------------------------------------
        public double MaxFineFuels
        {
            get
            {
                return maxFineFuels;
            }
            set
            {
                maxFineFuels = value;
            }
        }
        //---------------------------------------------------------------------
        public double MaxRxWindSpeed
        {
            get
            {
                return maxRxWindSpeed;
            }
            set
            {
                maxRxWindSpeed = value;
            }
        }
        //---------------------------------------------------------------------
        public double MaxRxFireWeatherIndex
        {
            get
            {
                return maxRxFireWeatherIndex;
            }
            set
            {
                maxRxFireWeatherIndex = value;
            }
        }
        //---------------------------------------------------------------------
        public double MinRxFireWeatherIndex
        {
            get
            {
                return minRxFireWeatherIndex;
            }
            set
            {
                minRxFireWeatherIndex = value;
            }
        }
        //---------------------------------------------------------------------
        public int NumberRxAnnualFires
        {
            get
            {
                return numberRxAnnualFires;
            }
            set
            {
                numberRxAnnualFires = value;
            }
        }
        //---------------------------------------------------------------------
        public double MaximumSpreadAreaB0
        {
            get
            {
                return maximumSpreadAreaB0;
            }
            set
            {
                maximumSpreadAreaB0 = value;
            }
        }
        //---------------------------------------------------------------------
        public double MaximumSpreadAreaB1
        {
            get
            {
                return maximumSpreadAreaB1;
            }
            set
            {
                maximumSpreadAreaB1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double MaximumSpreadAreaB2
        {
            get
            {
                return maximumSpreadAreaB2;
            }
            set
            {
                maximumSpreadAreaB2 = value;
            }
        }
        //---------------------------------------------------------------------
        public int LadderFuelMaxAge
        {
            get
            {
                return ladderFuelMaxAge;
            }
            set
            {
                ladderFuelMaxAge = value;
            }
        }
        //---------------------------------------------------------------------
        public double SeverityFactor_LadderFuelPercentage
        {
            get
            {
                return severityFactor_LadderFuelPercentage;
            }
            set
            {
                severityFactor_LadderFuelPercentage = value;
            }
        }
        //---------------------------------------------------------------------
        public double SeverityFactor_FineFuelPercentage
        {
            get
            {
                return severityFactor_FineFuelPercentage;
            }
            set
            {
                severityFactor_FineFuelPercentage = value;
            }
        }


        //---------------------------------------------------------------------

        public int LightningSuppressEffectivenss_low
        {
            get { return lightningSuppressEffectivenss_low; }
            set { lightningSuppressEffectivenss_low = value; }
        }
        public int LightningSuppressEffectivenss_medium
        {
            get { return lightningSuppressEffectivenss_medium; }
            set { lightningSuppressEffectivenss_medium = value; }
        }
        public int LightningSuppressEffectivenss_high
        {
            get { return lightningSuppressEffectivenss_high; }
            set { lightningSuppressEffectivenss_high = value; }
        }
        public int RxSuppressEffectivenss_low
        {
            get { return rxSuppressEffectivenss_low; }
            set { rxSuppressEffectivenss_low = value; }
        }
        public int RxSuppressEffectivenss_medium
        {
            get { return rxSuppressEffectivenss_medium; }
            set { rxSuppressEffectivenss_medium = value; }
        }
        public int RxSuppressEffectivenss_high
        {
            get { return rxSuppressEffectivenss_high; }
            set { rxSuppressEffectivenss_high = value; }
        }
        public int AccidentalSuppressEffectivenss_low
        {
            get { return accidentalSuppressEffectivenss_low; }
            set { accidentalSuppressEffectivenss_low = value; }
        }
        public int AccidentalSuppressEffectivenss_medium
        {
            get { return accidentalSuppressEffectivenss_medium; }
            set { accidentalSuppressEffectivenss_medium = value; }
        }
        public int AccidentalSuppressEffectivenss_high
        {
            get { return accidentalSuppressEffectivenss_high; }
            set { accidentalSuppressEffectivenss_high = value; }
        }
        //---------------------------------------------------------------------

        public InputParameters()
        {
            damages_severity1 = new List<IFireDamage>();
            damages_severity2 = new List<IFireDamage>();
            damages_severity3 = new List<IFireDamage>();
            ladderFuelSpeciesList = new List<ISpecies>();
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
