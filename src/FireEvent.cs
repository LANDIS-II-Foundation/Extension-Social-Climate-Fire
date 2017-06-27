//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.Climate;
//using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Landis.Extension.Scrapple
{

    public enum Ignition
    {
        Accidental,
        Lightning,
        Rx,
        Spread
    }

    public class FireEvent
        : ICohortDisturbance
    {
        private static readonly bool isDebugEnabled = false; //debugLog.IsDebugEnabled;

        //public static IFuelType[] FuelTypeParms;
        public static double SF;
        private static List<IFireDamage> damages;

        private ActiveSite initiationSite;
        private double maxFireParameter;
        private int sizeBin;
        private double maxDuration;
        //private IDynamicInputRecord initiationFireRegion;
        private bool secondRegionMap;
        private int initiationPercentConifer;
        private int initiationFuel;
        private int totalSitesDamaged;
        private int cohortsKilled;
        private double eventSeverity;
        private int numSitesChecked;
        private int[] sitesInEvent;

        private ActiveSite currentSite; // current site where cohorts are being damaged
        private int siteSeverity;      // used to compute maximum cohort severity at a site

        //private ISeasonParameters fireSeason;
        private double windSpeed;  
        private double windDirection;

        public double fireWeatherIndex;
        private Ignition ignitionType;

        //---------------------------------------------------------------------
        static FireEvent()
        {
        }

        //---------------------------------------------------------------------

        public Location StartLocation
        {
            get {
                return initiationSite.Location;
            }
        }

        //---------------------------------------------------------------------

        public double MaxFireParameter
        {
            get {
                return maxFireParameter;
            }
        }
        //---------------------------------------------------------------------

        public double SizeBin
        {
            get
            {
                return sizeBin;
            }
        }
        //---------------------------------------------------------------------
        public double MaxDuration
        {
            get
            {
                return maxDuration;
            }
        }
        //---------------------------------------------------------------------

        //public IDynamicInputRecord InitiationFireRegion
        //---------------------------------------------------------------------

        public bool SecondRegionMap
        {
            get
            {
                return secondRegionMap;
            }
        }
        //---------------------------------------------------------------------
        public int InitiationPercentConifer
        {
            get {
                return initiationPercentConifer;
            }
        }
        //---------------------------------------------------------------------

        public int InitiationFuel
        {
            get {
                return initiationFuel;
            }
        }
        //---------------------------------------------------------------------

        public int TotalSitesDamaged
        {
            get {
                return totalSitesDamaged;
            }
        }
        //---------------------------------------------------------------------

        public int[] SitesInEvent
        {
            get {
                return sitesInEvent;
            }
        }

        //---------------------------------------------------------------------

        public int NumSitesChecked
        {
            get {
                return numSitesChecked;
            }
        }
        //---------------------------------------------------------------------

        public int CohortsKilled
        {
            get {
                return cohortsKilled;
            }
        }

        //---------------------------------------------------------------------

        public double EventSeverity
        {
            get {
                return eventSeverity;
            }
        }

        //---------------------------------------------------------------------

        public double WindSpeed
        {
            get
            {
                return windSpeed;
            }
            set
            {
                windSpeed = value;
            }
        }
        //---------------------------------------------------------------------

        public double WindDirection
        {
            get
            {
                return windDirection;
            }
            set
            {
                windDirection = value;
            }
        }
        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.ExtType;
            }
        }

        //---------------------------------------------------------------------

        ActiveSite IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }

        //---------------------------------------------------------------------
        // Constructor function

        public FireEvent(ActiveSite initiationSite, int day, Ignition ignitionType)
        {
            this.initiationSite = initiationSite;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[initiationSite];

            int actualYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_DailyData.First().Key;
            AnnualClimate_Daily annualWeatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
            SiteVars.TypeOfIginition[initiationSite] = 1;
            SiteVars.Disturbed[initiationSite] = true;
            
            this.cohortsKilled = 0;
            this.eventSeverity = 0;
            this.totalSitesDamaged = 0;
            
            this.fireWeatherIndex = annualWeatherData.DailyFireWeatherIndex[day];
            this.windSpeed = annualWeatherData.DailyWindSpeed[day];
            this.windDirection = annualWeatherData.DailyWindDirection[day];
        }

        //---------------------------------------------------------------------

        public static void Initialize(//ISeasonParameters[] seasons,
                                      //IFuelType[] fuelTypeParameters,
                                      List<IFireDamage>    damages)
        {
            //if (isDebugEnabled)
            //    PlugIn.ModelCore.UI.WriteLine("Initializing event parameters ...");

            //if(seasons == null || fuelTypeParameters == null || damages == null)
            //{
            //    if(seasons == null)
            //        PlugIn.ModelCore.UI.WriteLine("Error:  Seasons table empty.");
            //    if(fuelTypeParameters == null)
            //        PlugIn.ModelCore.UI.WriteLine("Error:  FuelTypeParameters table empty.");
            //    if(damages == null)
            //        PlugIn.ModelCore.UI.WriteLine("Error:  Damages table empty.");
            //    throw new System.ApplicationException("Error: Event class could not be initialized.");
            //}

            //float totalSeasonFireProb = 0.0F;
            //foreach(ISeasonParameters season in seasons)
            //    totalSeasonFireProb += (float) season.FireProbability;

            //if (totalSeasonFireProb != 1.0)
            //    throw new System.ApplicationException("Error: Season Probabilities don't add to 1.0");

            //Event.FuelTypeParms = fuelTypeParameters;
            FireEvent.damages = damages;

            int tempSlope, sumSlope = 0, cellCount = 0, meanSlope = 0;
            foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
            {
                if (site.IsActive)
                {
                    tempSlope = SiteVars.GroundSlope[site];
                    sumSlope += tempSlope;
                    cellCount++;
                }
            }

            if(sumSlope > 0)
            {
                meanSlope = sumSlope / cellCount;
                if (meanSlope > 60)
                    meanSlope = 60;
                FireEvent.SF = CalculateSF(meanSlope);
            }
        }

        //---------------------------------------------------------------------
        public static FireEvent Initiate(ActiveSite site, int timestep, int day, Ignition ignitionType)

        {


            double randomNum = PlugIn.ModelCore.GenerateUniform();

            /*
             * VS: These were removed to being calculated once a year. 
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            AnnualFireWeather.CalculateAnnualFireWeather(ecoregion);
            */
            // FireEvent fireEvent = new FireEvent(site,/* fireSeason, fireSizeType, eco, */ day); 
            FireEvent fireEvent = new FireEvent(site, day, ignitionType);

            // Test that adequate weather data was retrieved:-
            /*
            if (fireEvent.windSpeed == 0)
            {
            // throw an error //RMS
                throw new Exception("Inadequate weasther data retrieved");
                //return null;
            }
            */
            return fireEvent;
        }

        

        //---------------------------------------------------------------------
        public void Spread(ActiveSite initiationSite)
        {
            //First, check for fire overlap:
            if(SiteVars.Disturbed[initiationSite])
            {
                // Randomly select neighbor to spread to
                if (isDebugEnabled)
                    PlugIn.ModelCore.UI.WriteLine("   Spreading fire event started at {0} ...", initiationSite.Location);

                List<Site> neighbors = Get4WeightedNeighbors(initiationSite);
                // remove any neighbors that are already disturbed by fire. This avoids overlap
                foreach (Site neighbor in neighbors)
                {
                    if(SiteVars.Disturbed[neighbor])
                    {
                        neighbors.Remove(neighbor);
                    }
                }

                // if there are no neighbors already disturbed then nothing to do since it can't spread
                if (neighbors.Count > 0)
                {
                    // Randomly select neighbor from remaining list and create a new fire event
                }
                

                /*
                int totalSiteSeverities = 0;
                int siteCohortsKilled = 0;
                //int totalISI = 0;
                totalSitesDamaged = 1;

                //this.initiationFuel   = SiteVars.CFSFuelType[initiationSite];
                //if (this.secondRegionMap)
                //    this.initiationFuel = SiteVars.CFSFuelType2[initiationSite];
                this.initiationPercentConifer = SiteVars.PercentConifer[initiationSite];

                //Next, calculate the fire area:
                List<Site> FireLocations = new List<Site>();
                FireLocations = new List<Site>();

                double cellArea = (PlugIn.ModelCore.CellLength * PlugIn.ModelCore.CellLength) / 10000; 

                if (FireLocations.Count == 0) return false;

                if (isDebugEnabled)
                    PlugIn.ModelCore.UI.WriteLine("  Damaging cohorts at burned sites ...");
                foreach (Site site in FireLocations)
                {
                    currentSite = (ActiveSite)site;
                    if (site.IsActive)
                    {
                        this.numSitesChecked++;

                        this.siteSeverity = 0; 
                        siteCohortsKilled = Damage(currentSite);

                        this.totalSitesDamaged++;
                        totalSiteSeverities += this.siteSeverity;
                        //totalISI += (int) SiteVars.ISI[site];


                        //IDynamicInputRecord siteFireRegion = SiteVars.FireRegion[site];
                        //if (this.secondRegionMap)
                        //    siteFireRegion = SiteVars.FireRegion2[site];

                        //sitesInEvent[siteFireRegion.Index]++;

                        SiteVars.Disturbed[currentSite] = true;
                        SiteVars.Severity[currentSite] = (byte)siteSeverity;

                        if (siteSeverity > 0)
                            SiteVars.LastSeverity[currentSite] = (byte)siteSeverity;
                    }
                }

                if (this.totalSitesDamaged == 0)
                    this.eventSeverity = 0;
                else
                    this.eventSeverity = ((double)totalSiteSeverities) / (double)this.totalSitesDamaged;

                //this.isi = (int) ((double) totalISI / (double) this.totalSitesDamaged);

                if (isDebugEnabled)
                    PlugIn.ModelCore.UI.WriteLine("  Done spreading");
                return true;
                */
            }

                

        }

        //---------------------------------------------------------------------
        private static List<Site> Get4WeightedNeighbors(Site srcSite)
        {
            if (!srcSite.IsActive)
                throw new ApplicationException("Source site is not active.");

            List<Site> neighbors = new List<Site>();

            RelativeLocation[] neighborhood = new RelativeLocation[]
            {
                new RelativeLocation(-1,  0),  // north
                new RelativeLocation( 0,  1),  // east
                new RelativeLocation( 1,  0),  // south
                new RelativeLocation( 0, -1),  // west
            };

            foreach (RelativeLocation relativeLoc in neighborhood)
            {
                Site neighbor = srcSite.GetNeighbor(relativeLoc);

                if (neighbor != null && neighbor.IsActive)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors; //fastNeighbors;
        }
        //---------------------------------------------------------------------

        private int Damage(ActiveSite site)
        {
            int previousCohortsKilled = this.cohortsKilled;
            SiteVars.Cohorts[site].RemoveMarkedCohorts(this);
            return this.cohortsKilled - previousCohortsKilled;
        }

        //---------------------------------------------------------------------

        //  A filter to determine which cohorts are removed.

        bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {
            bool killCohort = false;

            //Fire Severity 5 kills all cohorts:
            if (siteSeverity == 5)
            {
                killCohort = true;
            }
            else {
                //Otherwise, use damage table to calculate damage.
                //Read table backwards; most severe first.
                float ageAsPercent = (float) cohort.Age / (float) cohort.Species.Longevity;
                foreach(IFireDamage damage in damages)
                //for (int i = damages.Length-1; i >= 0; --i)
                {
                    //IFireDamage damage = damages[i];
                    if (siteSeverity - cohort.Species.FireTolerance >= damage.SeverTolerDifference)
                    {
                        if (damage.MaxAge >= ageAsPercent)
                        {
                            killCohort = true;

                            break;  // No need to search further in the table
                        }
                    }
                }
            }

            if (killCohort) {
                this.cohortsKilled++;
            }
            return killCohort;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Compares weights
        /// </summary>

        public class WeightComparer : IComparer<WeightedSite>
        {
            public int Compare(WeightedSite x,
                                              WeightedSite y)
            {
                int myCompare = x.Weight.CompareTo(y.Weight);
                return myCompare;
            }

        }

        private static double CalculateSF(int groundSlope)
        {
            return Math.Pow(Math.E, 3.533 * Math.Pow(((double)groundSlope / 100),1.2));  //FBP 39
        }

    }


    public class WeightedSite
    {
        private Site site;
        private double weight;

        //---------------------------------------------------------------------
        public Site Site
        {
            get {
                return site;
            }
            set {
                site = value;
            }
        }

        public double Weight
        {
            get {
                return weight;
            }
            set {
                weight = value;
            }
        }

        public WeightedSite (Site site, double weight)
        {
            this.site = site;
            this.weight = weight;
        }

    }
}
