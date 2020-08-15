//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Landis.Utilities;
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
        string LighteningFireMap { get; set; }
        string RxFireMap { get; set; }
        string AccidentalFireMap { get; set; }

        string LighteningSuppressionMap { get; set; }
        string RxSuppressionMap { get; set; }
        string AccidentalSuppressionMap { get; set; }

        double LightningIgnitionB0 { get; set; }
        double LightningIgnitionB1 { get; set; }
        double AccidentalFireIgnitionB0 { get; set; }
        double AccidentalFireIgnitionB1 { get; set; }
        double LightningIgnitionBinomialB0 { get; set; }
        double LightningIgnitionBinomialB1 { get; set; }
        double AccidentalFireIgnitionBinomialB0 { get; set; }
        double AccidentalFireIgnitionBinomialB1 { get; set; }
        double MaxFineFuels { get; set; }
        List<IDynamicIgnitionMap> DynamicRxIgnitionMaps { get; }
        List<IDynamicIgnitionMap> DynamicLightningIgnitionMaps { get; }
        List<IDynamicIgnitionMap> DynamicAccidentalIgnitionMaps { get; }
        List<IDynamicSuppressionMap> DynamicSuppressionMaps { get; }

        double RxMaxWindSpeed { get; set; }
        double RxMaxFireWeatherIndex { get; set; }
        double RxMinFireWeatherIndex { get; set; }
        double RxMaxTemperature { get; set; }
        double RxMinRelativeHumidity { get; set; }
        int RxMaxFireIntensity { get; set; }
        int RxNumberAnnualFires { get; set; }
        int RxNumberDailyFires { get; set; }
        int RxFirstDayFire { get; set; }
        int RxLastDayFire { get; set; }
        int RxTargetSize { get; set; }
        string RxZonesMap { get; set; }

        double MaximumSpreadAreaB0 { get; set; }
        double MaximumSpreadAreaB1 { get; set; }
        double MaximumSpreadAreaB2 { get; set; }

        double SpreadProbabilityB0 { get; set; }
        double SpreadProbabilityB1 { get; set; }
        double SpreadProbabilityB2 { get; set; }
        double SpreadProbabilityB3 { get; set; }

        int LadderFuelMaxAge { get; set; }
        double IntensityFactor_LadderFuelBiomass { get; set; }
        double IntensityFactor_FineFuelPercent { get; set; }

        int SuppressionMaxWindSpeed { get; set; }
        List<ISuppressionTable> SuppressionFWI_Table { get; }
        List<ISpecies> LadderFuelSpeciesList { get; }
        List<IDeadWood> DeadWoodList { get; }

        List<IFireDamage> FireDamages_Severity1 { get; }
        List<IFireDamage> FireDamages_Severity2 { get; }
        List<IFireDamage> FireDamages_Severity3 { get; }

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
        
        private string lighteningFireMap;
        private string accidentalFireMap;
        private string rxFireMap;
        private List<IDynamicIgnitionMap> dynamicRxIgnitions;
        private List<IDynamicIgnitionMap> dynamicLightningIgnitions;
        private List<IDynamicIgnitionMap> dynamicAccidentalIgnitions;
        private List<IDynamicSuppressionMap> dynamicSuppression;

        private string lighteningSuppressionMap;
        private string accidentalSuppressionMap;
        private string rxSuppressionMap;

        private double lightningIgnitionB0;
        private double lightningIgnitionB1;
        private double accidentalFireIgnitionB0;
        private double accidentalFireIgnitionB1;
        private double lightningIgnitionBinomialB0;
        private double lightningIgnitionBinomialB1;
        private double accidentalFireIgnitionBinomialB0;
        private double accidentalFireIgnitionBinomialB1;
        private double maxFineFuels;

        private double maxRxWindSpeed;
        private double maxRxFireWeatherIndex;
        private double minRxFireWeatherIndex;
        private double maxRxTemperature;
        private double minRxRelativeHumidity;
        private int maxRxFireIntensity;
        private int numberRxAnnualFires;
        private int numberRxDailyFires;
        private int firstDayRx;
        private int lastDayRx;
        private int targetRxSize;
        private string rxZoneMap;

        private double maximumSpreadAreaB0;
        private double maximumSpreadAreaB1;
        private double maximumSpreadAreaB2;

        private double spreadProbabilityB0;
        private double spreadProbabilityB1;
        private double spreadProbabilityB2;
        private double spreadProbabilityB3;

        private int ladderFuelMaxAge;
        private double intensityFactor_LadderFuelBiomass;
        private double intensityFactor_FineFuelPercentage;

        private int suppressionMaxWindSpeed;
        private List<ISuppressionTable> suppressionFWI_Table;
        private List<ISpecies> ladderFuelSpeciesList;
        private List<IDeadWood> deadWoodList;

        private List<IFireDamage> damages_severity1;
        private List<IFireDamage> damages_severity2;
        private List<IFireDamage> damages_severity3;



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
        public List<ISpecies> LadderFuelSpeciesList
        {
            get
            {
                return ladderFuelSpeciesList;
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

        public List<IDynamicIgnitionMap> DynamicRxIgnitionMaps
        {
            get
            {
                return dynamicRxIgnitions;
            }
        }
        //---------------------------------------------------------------------

        public List<IDynamicIgnitionMap> DynamicLightningIgnitionMaps
        {
            get
            {
                return dynamicLightningIgnitions;
            }
        }
        //---------------------------------------------------------------------

        public List<IDynamicIgnitionMap> DynamicAccidentalIgnitionMaps
        {
            get
            {
                return dynamicAccidentalIgnitions;
            }
        }
        //---------------------------------------------------------------------

        public List<IDynamicSuppressionMap> DynamicSuppressionMaps
        {
            get
            {
                return dynamicSuppression;
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
        public double LightningIgnitionBinomialB0
        {
            get
            {
                return lightningIgnitionBinomialB0;
            }
            set
            {
                lightningIgnitionBinomialB0 = value;
            }
        }

        //---------------------------------------------------------------------
        public double LightningIgnitionBinomialB1
        {
            get
            {
                return lightningIgnitionBinomialB1;
            }
            set
            {
                lightningIgnitionBinomialB1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double AccidentalFireIgnitionBinomialB0
        {
            get
            {
                return accidentalFireIgnitionBinomialB0;
            }
            set
            {
                accidentalFireIgnitionBinomialB0 = value;
            }
        }

        //---------------------------------------------------------------------
        public double AccidentalFireIgnitionBinomialB1
        {
            get
            {
                return accidentalFireIgnitionBinomialB1;
            }
            set
            {
                accidentalFireIgnitionBinomialB1 = value;
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
        public double RxMaxWindSpeed
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
        public double RxMaxFireWeatherIndex
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
        public double RxMinFireWeatherIndex
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
        public double RxMaxTemperature
        {
            get
            {
                return maxRxTemperature;
            }
            set
            {
                maxRxTemperature = value;
            }
        }
        //---------------------------------------------------------------------
        public double RxMinRelativeHumidity
        {
            get
            {
                return minRxRelativeHumidity;
            }
            set
            {
                minRxRelativeHumidity = value;
            }
        }
        //---------------------------------------------------------------------
        public int RxMaxFireIntensity
        {
            get
            {
                return maxRxFireIntensity;
            }
            set
            {
                maxRxFireIntensity = value;
            }
        }

        //---------------------------------------------------------------------
        public int RxNumberAnnualFires
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
        public int RxNumberDailyFires
        {
            get
            {
                return numberRxDailyFires;
            }
            set
            {
                numberRxDailyFires = value;
            }
        }
        //---------------------------------------------------------------------
        public int RxFirstDayFire
        {
            get
            {
                return firstDayRx;
            }
            set
            {
                firstDayRx = value;
            }
        }
        //---------------------------------------------------------------------
        public int RxLastDayFire
        {
            get
            {
                return lastDayRx;
            }
            set
            {
                lastDayRx = value;
            }
        }
        //---------------------------------------------------------------------
        public int RxTargetSize
        {
            get
            {
                return targetRxSize;
            }
            set
            {
                targetRxSize = value;
            }
        }
        //---------------------------------------------------------------------
        public string RxZonesMap
        {
            get
            {
                return rxZoneMap;
            }
            set
            {
                rxZoneMap = value;
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
        public double SpreadProbabilityB0
        {
            get
            {
                return spreadProbabilityB0;
            }
            set
            {
                spreadProbabilityB0 = value;
            }
        }
        //---------------------------------------------------------------------
        public double SpreadProbabilityB1
        {
            get
            {
                return spreadProbabilityB1;
            }
            set
            {
                spreadProbabilityB1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double SpreadProbabilityB2
        {
            get
            {
                return spreadProbabilityB2;
            }
            set
            {
                spreadProbabilityB2 = value;
            }
        }
        //---------------------------------------------------------------------
        public double SpreadProbabilityB3
        {
            get
            {
                return spreadProbabilityB3;
            }
            set
            {
                spreadProbabilityB3 = value;
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
        public double IntensityFactor_LadderFuelBiomass
        {
            get
            {
                return intensityFactor_LadderFuelBiomass;
            }
            set
            {
                intensityFactor_LadderFuelBiomass = value;
            }
        }
        //---------------------------------------------------------------------
        public double IntensityFactor_FineFuelPercent
        {
            get
            {
                return intensityFactor_FineFuelPercentage;
            }
            set
            {
                intensityFactor_FineFuelPercentage = value;
            }
        }


        //---------------------------------------------------------------------
        public int SuppressionMaxWindSpeed
        {
            get
            {
                return suppressionMaxWindSpeed;
            }
            set
            {
                suppressionMaxWindSpeed = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISuppressionTable> SuppressionFWI_Table
        {
            get
            {
                return suppressionFWI_Table;
            }
            //set
            //{
            //    suppressionFWI_Table = value;
            //}
        }
        //---------------------------------------------------------------------
        public List<IDeadWood> DeadWoodList
        {
            get
            {
                return deadWoodList;
            }
        }
        //---------------------------------------------------------------------
        public List<IFireDamage> FireDamages_Severity1
        {
            get
            {
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

        public InputParameters()
        {
            ladderFuelSpeciesList = new List<ISpecies>();
            deadWoodList = new List<IDeadWood>();
            suppressionFWI_Table = new List<ISuppressionTable>();
            damages_severity1 = new List<IFireDamage>();
            damages_severity2 = new List<IFireDamage>();
            damages_severity3 = new List<IFireDamage>();
            dynamicRxIgnitions = new List<IDynamicIgnitionMap>();
            dynamicLightningIgnitions = new List<IDynamicIgnitionMap>();
            dynamicAccidentalIgnitions = new List<IDynamicIgnitionMap>();
            dynamicSuppression = new List<IDynamicSuppressionMap>();
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
