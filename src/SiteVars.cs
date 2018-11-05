//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;

namespace Landis.Extension.Scrapple
{
    public static class SiteVars
    {
        private static ISiteVar<FireEvent> eventVar;
        private static ISiteVar<int> timeOfLastFire;
        private static ISiteVar<double> lightningFireWeight;
        private static ISiteVar<double> rxFireWeight;
        private static ISiteVar<double> accidentalFireWeight;
        private static ISiteVar<short> typeOfIginition;
        private static ISiteVar<byte> intensity;
        private static ISiteVar<ushort> dayOfFire;
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<ushort> groundSlope;
        private static ISiteVar<ushort> uphillSlopeAzimuth;
        private static ISiteVar<ISiteCohorts> cohorts;
        private static ISiteVar<double> fineFuels;
        private static ISiteVar<int> specialDeadWood;  // potential snags, specifically
        private static ISiteVar<double> spreadProbablity;

        private static ISiteVar<double> lightningSuppressionIndex;
        private static ISiteVar<double> rxSuppressionIndex;
        private static ISiteVar<double> accidentalSuppressionIndex;

        public static ISiteVar<double> SmolderConsumption;
        public static ISiteVar<double> FlamingConsumption;
        public static ISiteVar<int> HarvestTime;
        public static ISiteVar<int> EventID;
        public static ISiteVar<int> RxFireRegion;

        //---------------------------------------------------------------------

        public static void Initialize()
        {

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");
            fineFuels = PlugIn.ModelCore.GetSiteVar<double>("Succession.FineFuels");

            eventVar = PlugIn.ModelCore.Landscape.NewSiteVar<FireEvent>(InactiveSiteMode.DistinctValues);
            timeOfLastFire       = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            intensity         = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            spreadProbablity = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            dayOfFire = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();

            groundSlope          = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();
            uphillSlopeAzimuth   = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();
            lightningFireWeight  = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            rxFireWeight = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            accidentalFireWeight = PlugIn.ModelCore.Landscape.NewSiteVar<double>();

            lightningSuppressionIndex = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            rxSuppressionIndex = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            accidentalSuppressionIndex = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            typeOfIginition = PlugIn.ModelCore.Landscape.NewSiteVar<short>();
            disturbed = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
            specialDeadWood = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            EventID = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            RxFireRegion = PlugIn.ModelCore.Landscape.NewSiteVar<int>();

            SmolderConsumption = PlugIn.ModelCore.GetSiteVar<double>("Succession.SmolderConsumption");
            FlamingConsumption = PlugIn.ModelCore.GetSiteVar<double>("Succession.FlamingConsumption");
            HarvestTime = PlugIn.ModelCore.GetSiteVar<int>("Harvest.TimeOfLastEvent");

            //Initialize TimeSinceLastFire to the maximum cohort age:
            //foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            //{
            //    timeOfLastFire[site] = 0; 
            //}


            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Intensity, "Fire.Severity");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.TimeOfLastFire, "Fire.TimeOfLastEvent");
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes for disturbances.
        /// </summary>
        public static void InitializeDisturbances()
        {
            fineFuels = PlugIn.ModelCore.GetSiteVar<double>("Succession.FineFuels");

        }

        //---------------------------------------------------------------------
        public static ISiteVar<double> LightningFireWeight
        {
            get
            {
                return lightningFireWeight;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> RxFireWeight
        {
            get
            {
                return rxFireWeight;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> AccidentalFireWeight
        {
            get
            {
                return accidentalFireWeight;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<double> LightningSuppressionIndex
        {
            get
            {
                return lightningSuppressionIndex;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> RxSuppressionIndex
        {
            get
            {
                return rxSuppressionIndex;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> AccidentalSuppressionIndex
        {
            get
            {
                return accidentalSuppressionIndex;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> SpreadProbability
        {
            get
            {
                return spreadProbablity;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> FineFuels
        {
            get
            {
                return fineFuels;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<short> TypeOfIginition
        {
            get
            {
                return typeOfIginition;
            }
        }
        //---------------------------------------------------------------------


        public static ISiteVar<int> TimeOfLastFire
        {
            get {
                return timeOfLastFire;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<byte> Intensity
        {
            get
            {
                return intensity;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<ushort> DayOfFire
        {
            get
            {
                return dayOfFire;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<bool> Disturbed
        {
            get {
                return disturbed;
            }
        }
        //---------------------------------------------------------------------
        
        public static ISiteVar<ushort> GroundSlope
        {
            get {
                return groundSlope;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<ushort> UphillSlopeAzimuth
        {
            get {
                return uphillSlopeAzimuth;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> SpecialDeadWood
        {
            get
            {
                return specialDeadWood;
            }
        }
        //---------------------------------------------------------------------
        //public static ISiteVar<ushort> SiteWindSpeed
        //{
        //    get
        //    {
        //        return siteWindSpeed;
        //    }
        //}

        ////---------------------------------------------------------------------
        //public static ISiteVar<ushort> SiteWindDirection
        //{
        //    get
        //    {
        //        return siteWindDirection;
        //    }
        //}

        //---------------------------------------------------------------------

        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }

    }
}
