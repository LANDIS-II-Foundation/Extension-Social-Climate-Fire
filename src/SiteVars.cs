//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Scrapple
{
    public static class SiteVars
    {
        private static ISiteVar<FireEvent> eventVar;
        private static ISiteVar<int> timeOfLastFire;
        private static ISiteVar<double> lightningFireWeight;
        private static ISiteVar<double> rxFireWeight;
        private static ISiteVar<double> accidentalFireWeight;
        private static ISiteVar<byte> typeOfIginition;
        private static ISiteVar<byte> severity;
        private static ISiteVar<byte> dayOfFire;
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<ushort> groundSlope;
        private static ISiteVar<ushort> uphillSlopeAzimuth;
        private static ISiteVar<ISiteCohorts> cohorts;
        private static ISiteVar<double> fineFuels;

        //---------------------------------------------------------------------

        public static void Initialize()
        {

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.AgeCohorts");
            fineFuels = PlugIn.ModelCore.GetSiteVar<double>("Succession.FineFuels");

            eventVar = PlugIn.ModelCore.Landscape.NewSiteVar<FireEvent>(InactiveSiteMode.DistinctValues);
            timeOfLastFire       = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            severity         = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            dayOfFire = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();

            groundSlope          = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();
            uphillSlopeAzimuth   = PlugIn.ModelCore.Landscape.NewSiteVar<ushort>();
            lightningFireWeight  = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            rxFireWeight = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            accidentalFireWeight = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            typeOfIginition = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            disturbed = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();

            //Also initialize topography, will be overwritten if optional parameters provided:
            SiteVars.GroundSlope.ActiveSiteValues = 0;
            SiteVars.UphillSlopeAzimuth.ActiveSiteValues = 0;

            //Initialize TimeSinceLastFire to the maximum cohort age:
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                //ushort maxAge = GetMaxAge(site);
                timeOfLastFire[site] = 0; // PlugIn.ModelCore.StartTime - maxAge;
            }


            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Severity, "Fire.Severity");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.TimeOfLastFire, "Fire.TimeOfLastEvent");
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
        public static ISiteVar<double> FineFuels
        {
            get
            {
                return fineFuels;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<byte> TypeOfIginition
        {
            get
            {
                return typeOfIginition;
            }
        }

        //public static ISiteVar<Site> OriginSite
        //{
        //    get
        //    {
        //        return originSite;
        //    }

        //    set
        //    {
        //        originSite = value;
        //    }
        //}
        // ------------ End addition
        

        public static ISiteVar<int> TimeOfLastFire
        {
            get {
                return timeOfLastFire;
            }
        }
        //---------------------------------------------------------------------
        //public static ISiteVar<FireEvent> FireEvent
        //{
        //    get {
        //        return eventVar;
        //    }
        //}
        
        //public static ISiteVar<int> PercentConifer
        //{
        //    get {
        //        return percentConifer;
        //    }
        //}

        ////---------------------------------------------------------------------

        //public static ISiteVar<int> PercentHardwood
        //{
        //    get {
        //        return percentHardwood;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public static ISiteVar<int> PercentDeadFir
        //{
        //    get {
        //        return percentDeadFir;
        //    }
        //}

        //---------------------------------------------------------------------
        public static ISiteVar<byte> Severity
        {
            get
            {
                return severity;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<byte> DayOfFire
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

        //---------------------------------------------------------------------
        //public static ushort GetMaxAge(ActiveSite site)
        //{
        //    if (SiteVars.Cohorts[site] == null)
        //    {
        //        PlugIn.ModelCore.UI.WriteLine("Cohort are null.");
        //        return 0;
        //    }
        //    ushort max = 0;

        //    foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
        //    {
        //        foreach (ICohort cohort in speciesCohorts)
        //        {
        //            if (cohort.Age > max)
        //                max = cohort.Age;
        //        }
        //    }
        //    return max;
        //}
    }
}
