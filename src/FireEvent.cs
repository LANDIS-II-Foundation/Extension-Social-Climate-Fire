//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.Climate;
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
        public static Random rnd = new Random();

        private ActiveSite initiationSite;
        private Location originLocation;
        private int totalSitesDamaged;
        private int spreadDistance;
        

        private int cohortsKilled;
        private double eventSeverity;
        
        private double windSpeed;  
        private double windDirection;

        private double fireWeatherIndex;
        public Ignition IgnitionType;
        //private int numSpread;

        //---------------------------------------------------------------------
        static FireEvent()
        {
        }

        //---------------------------------------------------------------------
        // Spreadlength is the distance that a fire can spread during a given day
        public int SpreadDistance
        {
            get
            {
                return spreadDistance;
            }

            set
            {
                spreadDistance = value;
            }
        }
        //---------------------------------------------------------------------

        public double FireWeatherIndex
        {
            get
            {
                return fireWeatherIndex;
            }
        }

        public Location StartLocation
        {
            get {
                return initiationSite.Location;
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

        //public double WindSpeed
        //{
        //    get
        //    {
        //        return windSpeed;
        //    }
        //    set
        //    {
        //        windSpeed = value;
        //    }
        //}
        ////---------------------------------------------------------------------

        //public double WindDirection
        //{
        //    get
        //    {
        //        return windDirection;
        //    }
        //    set
        //    {
        //        windDirection = value;
        //    }
        //}
        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.ExtType;
            }
        }

        //---------------------------------------------------------------------

        public Location OriginLocation
        {
            get {
                return originLocation;
            }
            set
            {
                value = originLocation;
            }
        }



        //---------------------------------------------------------------------
        ActiveSite IDisturbance.CurrentSite
        {
            get
            {
                return initiationSite;
            }
        }
        // Constructor function

        public FireEvent(ActiveSite initiationSite, int day, Ignition ignitionType)
        {
            this.initiationSite = initiationSite;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[initiationSite];

            int actualYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_DailyData.First().Key;
            AnnualClimate_Daily annualWeatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
            SiteVars.TypeOfIginition[initiationSite] = (byte)ignitionType;
            SiteVars.Disturbed[initiationSite] = true;
            
            this.cohortsKilled = 0;
            this.eventSeverity = 0;
            this.totalSitesDamaged = 0;
            
            this.fireWeatherIndex = annualWeatherData.DailyFireWeatherIndex[day];
            this.windSpeed = annualWeatherData.DailyWindSpeed[day];
            this.windDirection = annualWeatherData.DailyWindDirection[day];
            this.originLocation = initiationSite.Location;
            this.initiationSite = initiationSite;
        }
        

        //---------------------------------------------------------------------
        public static FireEvent Initiate(ActiveSite initiationSite, int timestep, int day, Ignition ignitionType, int spreadLength)

        {
            double randomNum = PlugIn.ModelCore.GenerateUniform();

            //First, check for fire overlap:

            if (!SiteVars.Disturbed[initiationSite])
            {
                // Randomly select neighbor to spread to
                if (isDebugEnabled)
                    PlugIn.ModelCore.UI.WriteLine("   Fire event started at {0} ...", initiationSite.Location);

                FireEvent fireEvent = new FireEvent(initiationSite, day, ignitionType);
                fireEvent.OriginLocation = fireEvent.initiationSite.Location;
                fireEvent.IgnitionType = ignitionType;

                // RMS TODO:  ADD OTHER FIRE EVENT PARAMETERS

                PlugIn.LogEvent(PlugIn.ModelCore.CurrentTime, fireEvent);


                List<Site> neighbors = Get4WeightedNeighbors(initiationSite);
                neighbors.RemoveAll(neighbor => SiteVars.Disturbed[neighbor] || !neighbor.IsActive);

                // if there are no neighbors already disturbed then nothing to do since it can't spread
                if (neighbors.Count > 0)
                {
                    //VS: for now pick random site to spread to
                    int r = rnd.Next(neighbors.Count);
                    Site nextSite = neighbors[r];

                    //Initiate a fireevent at that site
                    //FireEvent spreadEvent = Initiate((ActiveSite)nextSite, currentTime, day, Ignition.Spread, (this.SpreadLength - 1));
                    if (fireEvent.SpreadDistance > 0)
                    {
                        fireEvent.Spread(PlugIn.ModelCore.CurrentTime, day, (ActiveSite) nextSite);
                    }
                }
                return fireEvent;
            }
            else
            {
                return null;
            }
            /*
             * VS: These were removed to being calculated once a year. 
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            AnnualFireWeather.CalculateAnnualFireWeather(ecoregion);
            */
            // FireEvent fireEvent = new FireEvent(site,/* fireSeason, fireSizeType, eco, */ day); 
            //FireEvent fireEvent = new FireEvent(site, day, ignitionType);
            //fireEvent.SpreadLength = spreadLength;


            // Test that adequate weather data was retrieved:-
            /*
            if (fireEvent.windSpeed == 0)
            {
            // throw an error //RMS
                throw new Exception("Inadequate weasther data retrieved");
                //return null;
            }
            */
        }

        

        //---------------------------------------------------------------------
        public void Spread(int currentTime, int day, ActiveSite site)
        {
            // First, load necessary parameters
            //      load fwi
            //      load wind speed velocity (in which case, NOT a fire event parameter)
            //      load wind direction (in which case, NOT a fire event parameter)
            //      load fine fuels
            //      load uphill slope azimuth
            //      wind speed = wind speed adjusted
            //AMK equations for wind speed/direction factor conversions from raw data 
            //Refer to design doc on Google Drive for questions or explanations
            //wsx = (wind_speed_velocity * sin(fire_azimuth)) + (wind_speed_velocity * sin(uphill_azimuth))
            //wsy = (wind_speed_velocity * cos(fire_azimuth)) + (wind_speed_velocity * cos(uphill_azimuth))
            //ws.factor = sqrt(wsx^2 + wsy^2) //wind speed factor
            //wd.factor = acos(wsy/ws.factor) //wind directior factor

            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            double fireWeatherIndex = 0.0;
            try
            {
                fireWeatherIndex = Climate.Future_DailyData[currentTime][ecoregion.Index].DailyFireWeatherIndex[day];
            }
            catch
            {
                throw new UninitializedClimateData(string.Format("Fire Weather Index could not be found \t year: {0}, day: {1} in ecoregion: {2} not found", currentTime, day, ecoregion.Name));
            }
            double windSpeed = Climate.Future_DailyData[currentTime][ecoregion.Index].DailyWindSpeed[day];
            double windDirection = Climate.Future_DailyData[currentTime][ecoregion.Index].DailyWindDirection[day];
            // FINISH.  double fineFuels = SiteVars.FineFuels[site];

            //      Is spread to this site allowable?
            //          Calculate P-spread based on fwi, adjusted wind speed, fine fuels, source intensity (or similar). (AK)
            //          Adjust P-spread to account for suppression (RMS)


            // Next, determine severity (0 = none, 1 = <4', 2 = 4-8', 3 = >8'.
            //      Severity a function of fwi, ladder fuels, other? (AK)

            // If severity > 0
            //      Calculate spread-distance-max (AK)
            //      if spread-distance > spread-distance-max, day = day + 1

            //      Cause mortality
            //      map daily spread (doy) (add SiteVar)
            //      map severity

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
            /*
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

                            break;  // No need to search further in th
                    }
                }
            }
            */

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
