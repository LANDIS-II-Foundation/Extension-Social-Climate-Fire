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
        Landis.Library.Parameters.Species.AuxParm<double> AgeDBH { get; }
        Landis.Library.Parameters.Species.AuxParm<double> MaximumBarkThickness { get; }
        string LighteningFireMap { get; set; }
        string RxFireMap { get; set; }
        string AccidentalFireMap { get; set; }
        string LighteningSuppressionMap { get; set; }
        string RxSuppressionMap { get; set; }
        string AccidentalSuppressionMap { get; set; }
        string ClayMap { get; set; }

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

        double SiteMortalityB0 { get; set; }
        double SiteMortalityB1 { get; set; }
        double SiteMortalityB2 { get; set; }
        double SiteMortalityB3 { get; set; }
        double SiteMortalityB4 { get; set; }
        double SiteMortalityB5 { get; set; }
        double SiteMortalityB6 { get; set; }
        double CohortMortalityB0 { get; set; }
        double CohortMortalityB1 { get; set; }
        double CohortMortalityB2 { get; set; }
        int LadderFuelMaxAge { get; set; }

        int SuppressionMaxWindSpeed { get; set; }
        Dictionary<int, ISuppressionTable> SuppressionFWI_Table { get; }
        List<ISpecies> LadderFuelSpeciesList { get; }
        List<IDeadWood> DeadWoodList { get; }

        double TimeZeroPET { get; set; }
        double TimeZeroCWD { get; set; }


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
        private ISpeciesDataset speciesDataset;

        private Landis.Library.Parameters.Species.AuxParm<double> ageDBH;
        private Landis.Library.Parameters.Species.AuxParm<double> maximumBarkThickness;
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
        private string clayMap;

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

        private double siteMortalityB0;
        private double siteMortalityB1;
        private double siteMortalityB2;
        private double siteMortalityB3;
        private double siteMortalityB4;
        private double siteMortalityB5;
        private double siteMortalityB6;

        private double cohortMortalityB0;
        private double cohortMortalityB1;
        private double cohortMortalityB2;

        private int ladderFuelMaxAge;

        private int suppressionMaxWindSpeed;
        private Dictionary<int, ISuppressionTable> suppressionFWI_Table;
        private List<ISpecies> ladderFuelSpeciesList;
        private List<IDeadWood> deadWoodList;

        private double timeZeroPET;
        private double timeZeroCWD;




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
        public Landis.Library.Parameters.Species.AuxParm<double> AgeDBH
        {
            get { return ageDBH; }
            set { ageDBH = value; }
        }
        public Landis.Library.Parameters.Species.AuxParm<double> MaximumBarkThickness
        {
            get { return maximumBarkThickness; }
            set { maximumBarkThickness = value; }
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
        public string ClayMap
        {
            get
            {
                return clayMap;
            }
            set
            {
                clayMap = value;
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
        public double SiteMortalityB0
        {
            get { return siteMortalityB0;}
            set { siteMortalityB0 = value;}
        }
        //---------------------------------------------------------------------
        public double SiteMortalityB1
        {
            get {return siteMortalityB1;}
            set {siteMortalityB1 = value;}
        }
        //---------------------------------------------------------------------
        public double SiteMortalityB2
        {
            get {return siteMortalityB2;}
            set {siteMortalityB2 = value;}
        }
        //---------------------------------------------------------------------
        public double SiteMortalityB3
        {
            get
            {
                return siteMortalityB3;
            }
            set
            {
                siteMortalityB3 = value;
            }
        }
        //---------------------------------------------------------------------
        public double SiteMortalityB4
        {
            get
            {
                return siteMortalityB4;
            }
            set
            {
                siteMortalityB4 = value;
            }
        }
        //---------------------------------------------------------------------
        public double SiteMortalityB5
        {
            get
            {
                return siteMortalityB5;
            }
            set
            {
                siteMortalityB5 = value;
            }
        }
        //---------------------------------------------------------------------
        public double SiteMortalityB6
        {
            get
            {
                return siteMortalityB6;
            }
            set
            {
                siteMortalityB6 = value;
            }
        }
        //---------------------------------------------------------------------
        public double CohortMortalityB0
        {
            get { return cohortMortalityB0; }
            set { cohortMortalityB0 = value; }
        }
        //---------------------------------------------------------------------
        public double CohortMortalityB1
        {
            get { return cohortMortalityB1; }
            set { cohortMortalityB1 = value; }
        }
        //---------------------------------------------------------------------
        public double CohortMortalityB2
        {
            get { return cohortMortalityB2; }
            set { cohortMortalityB2 = value; }
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
        //public List<ISuppressionTable> SuppressionFWI_Table
        public Dictionary<int, ISuppressionTable> SuppressionFWI_Table
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
        public double TimeZeroPET
        {
            get { return timeZeroPET; }
            set { timeZeroPET = value; }
        }
        //---------------------------------------------------------------------
        public double TimeZeroCWD
        {
            get { return timeZeroCWD; }
            set { timeZeroCWD = value; }
        }



        //---------------------------------------------------------------------

        public InputParameters(ISpeciesDataset speciesDataset)
        {
            this.speciesDataset = speciesDataset;

            ladderFuelSpeciesList = new List<ISpecies>();
            deadWoodList = new List<IDeadWood>();
            suppressionFWI_Table = new Dictionary<int, ISuppressionTable>();
            dynamicRxIgnitions = new List<IDynamicIgnitionMap>();
            dynamicLightningIgnitions = new List<IDynamicIgnitionMap>();
            dynamicAccidentalIgnitions = new List<IDynamicIgnitionMap>();
            dynamicSuppression = new List<IDynamicSuppressionMap>();
            ageDBH = new Landis.Library.Parameters.Species.AuxParm<double>(speciesDataset);
            maximumBarkThickness = new Landis.Library.Parameters.Species.AuxParm<double>(speciesDataset);

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
