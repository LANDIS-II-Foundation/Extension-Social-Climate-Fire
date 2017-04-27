//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Scrapple
{
    public static class SiteVars
    {
        private static ISiteVar<Event> eventVar;
        private static ISiteVar<int> timeOfLastFire;
        private static ISiteVar<IDynamicInputRecord> fire_regions;
        private static ISiteVar<IDynamicInputRecord> fire_regions2;
        private static ISiteVar<int> cfsFuelType;
        private static ISiteVar<int> cfsFuelType2;
        private static ISiteVar<int> decidFuelType;
        private static ISiteVar<int> percentConifer;
        private static ISiteVar<int> percentHardwood;
        private static ISiteVar<int> percentDeadFir;
        private static ISiteVar<byte> severity;
        private static ISiteVar<byte> lastSeverity;
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<double> travelTime;
        private static ISiteVar<double> minNeighborTravelTime;
        //private static ISiteVar<double> simpleTravelTime;
        private static ISiteVar<double> rateOfSpread;
        private static ISiteVar<double> isi;
        private static ISiteVar<double> adjROS;
        private static ISiteVar<double> costTime;
        private static ISiteVar<ushort> groundSlope;
        private static ISiteVar<ushort> uphillSlopeAzimuth;

        private static ISiteVar<ushort> siteWindSpeed;
        private static ISiteVar<ushort> siteWindDirection;

        private static ISiteVar<ISiteCohorts> cohorts;

        //---------------------------------------------------------------------

        public static void Initialize()
        {

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.AgeCohorts");
            
            eventVar            = PlugIn.ModelCore.Landscape.NewSiteVar<Event>(InactiveSiteMode.DistinctValues);
            timeOfLastFire      = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            fire_regions = PlugIn.ModelCore.Landscape.NewSiteVar<IDynamicInputRecord>();
            fire_regions2 = PlugIn.ModelCore.Landscape.NewSiteVar<IDynamicInputRecord>();
            percentDeadFir      = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            severity            = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            lastSeverity        = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            disturbed           = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
            travelTime          = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            minNeighborTravelTime     = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            rateOfSpread        = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            adjROS              = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            isi                 = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            costTime            = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            groundSlope         = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();
            uphillSlopeAzimuth  = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();
            siteWindSpeed       = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();
            siteWindDirection   = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();

            //Also initialize topography, will be overwritten if optional parameters provided:
            SiteVars.GroundSlope.ActiveSiteValues = 0;
            SiteVars.UphillSlopeAzimuth.ActiveSiteValues = 0;

            //Initialize TimeSinceLastFire to the maximum cohort age:
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                ushort maxAge = GetMaxAge(site);
                timeOfLastFire[site] = PlugIn.ModelCore.StartTime - maxAge;
            }


            PlugIn.ModelCore.RegisterSiteVar(SiteVars.FireRegion, "Fire.FireRegion");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.FireRegion2, "Fire.FireRegion2");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Severity, "Fire.Severity");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.LastSeverity, "Fire.LastSeverity");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.TimeOfLastFire, "Fire.TimeOfLastEvent");
        }

        //---------------------------------------------------------------------

        public static void InitializeFuelType()
        {
            PlugIn.ModelCore.UI.WriteLine("   Initializing Fuel Type.");

            cfsFuelType     = PlugIn.ModelCore.GetSiteVar<int>("Fuels.CFSFuelType");
            cfsFuelType2    = PlugIn.ModelCore.GetSiteVar<int>("Fuels.CFSFuelType");
            decidFuelType   = PlugIn.ModelCore.GetSiteVar<int>("Fuels.DecidFuelType");
            percentConifer  = PlugIn.ModelCore.GetSiteVar<int>("Fuels.PercentConifer");
            percentHardwood = PlugIn.ModelCore.GetSiteVar<int>("Fuels.PercentHardwood");
            percentDeadFir  = PlugIn.ModelCore.GetSiteVar<int>("Fuels.PercentDeadFir");

            if (SiteVars.CFSFuelType == null)
                throw new System.ApplicationException("Error: CFS Fuel Type NOT Initialized.  Fuel extension MUST be active.");

            //SiteVars.PercentDeadFir.ActiveSiteValues = 0;

        }

        //---------------------------------------------------------------------

        public static ISiteVar<IDynamicInputRecord> FireRegion
        {
            get {
                return fire_regions;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<IDynamicInputRecord> FireRegion2
        {
            get
            {
                return fire_regions2;
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
        public static ISiteVar<Event> Event
        {
            get {
                return eventVar;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> CFSFuelType
        {
            get {
                return cfsFuelType;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<int> CFSFuelType2
        {
            get
            {
                return cfsFuelType2;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<int> DecidFuelType
        {
            get {
                return decidFuelType;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<int> PercentConifer
        {
            get {
                return percentConifer;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> PercentHardwood
        {
            get {
                return percentHardwood;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<int> PercentDeadFir
        {
            get {
                return percentDeadFir;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<byte> Severity
        {
            get {
                return severity;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<byte> LastSeverity
        {
            get
            {
                return lastSeverity;
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

        public static ISiteVar<double> TravelTime
        {
            get {
                return travelTime;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<double> MinNeighborTravelTime
        {
            get {
                return minNeighborTravelTime;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<double> RateOfSpread
        {
            get {
                return rateOfSpread;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<double> ISI
        {
            get {
                return isi;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> AdjROS
        {
            get
            {
                return adjROS;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> CostTime
        {
            get
            {
                return costTime;
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
        public static ISiteVar<ushort> SiteWindSpeed
        {
            get
            {
                return siteWindSpeed;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<ushort> SiteWindDirection
        {
            get
            {
                return siteWindDirection;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }

        //---------------------------------------------------------------------
        public static ushort GetMaxAge(ActiveSite site)
        {
            if (SiteVars.Cohorts[site] == null)
            {
                PlugIn.ModelCore.UI.WriteLine("Cohort are null.");
                return 0;
            }
            ushort max = 0;

            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
            {
                foreach (ICohort cohort in speciesCohorts)
                {
                    if (cohort.Age > max)
                        max = cohort.Age;
                }
            }
            return max;
        }
    }
}
