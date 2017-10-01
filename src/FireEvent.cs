//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

//using Landis.Library.AgeOnlyCohorts;
using Landis.Library.BiomassCohorts;
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

    public enum Ignition : int
    {
        Accidental,
        Lightning,
        Rx,
        Spread
    }

    public class FireEvent
        : IDisturbance//ICohortDisturbance
    {
        private static readonly bool isDebugEnabled = false; //debugLog.IsDebugEnabled;
        public static Random rnd = new Random();

        private ActiveSite initiationSite;
        private int totalSitesDamaged;

        private int cohortsKilled;
//        private double eventSeverity;
        
        public double InitiationFireWeatherIndex;
        public Ignition IgnitionType;
        AnnualClimate_Daily annualWeatherData;
        public int NumberOfDays;
        public double MeanSeverity;
        public double MeanWindDirection;
        public double MeanWindSpeed;
        public double MeanSuppression;
        public double TotalBiomassMortality;
        public int NumberCellsSeverity1;
        public int NumberCellsSeverity2;
        public int NumberCellsSeverity3;

        public Dictionary<int, int> spreadArea;

        public int maxDay;
        public int siteSeverity;

        //---------------------------------------------------------------------
        static FireEvent()
        {
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

        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.ExtType;
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
            this.IgnitionType = ignitionType;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[initiationSite];

            int actualYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_DailyData.First().Key;
            this.annualWeatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
            //SiteVars.TypeOfIginition[initiationSite] = (ushort) ignitionType;
            //SiteVars.Disturbed[initiationSite] = true;

            this.cohortsKilled = 0;
            this.totalSitesDamaged = 0;
            this.InitiationFireWeatherIndex = annualWeatherData.DailyFireWeatherIndex[day];
            this.spreadArea = new Dictionary<int, int>();
            this.NumberOfDays = 1;
            this.MeanSeverity = 0.0;
            this.MeanWindDirection = 0.0;
            this.MeanWindSpeed = 0.0;
            this.MeanSuppression = 0.0;
            this.TotalBiomassMortality = 0.0;
            this.NumberCellsSeverity1 = 0;
            this.NumberCellsSeverity2 = 0;
            this.NumberCellsSeverity3 = 0;
            this.maxDay = day;

        //this.windSpeed = annualWeatherData.DailyWindSpeed[day];
        //this.windDirection = annualWeatherData.DailyWindDirection[day];
        //this.originLocation = initiationSite.Location;
    }


    //---------------------------------------------------------------------
    public static FireEvent Initiate(ActiveSite initiationSite, int timestep, int day, Ignition ignitionType)

        {
            //PlugIn.ModelCore.UI.WriteLine("  Fire Event initiated.  Day = {0}, IgnitionType = {1}.", day, ignitionType);
            
            //First, check for fire overlap (NECESSARY??):
            if (!SiteVars.Disturbed[initiationSite])
            {
                // Randomly select neighbor to spread to
                if (isDebugEnabled)
                    PlugIn.ModelCore.UI.WriteLine("   Fire event started at {0} ...", initiationSite.Location);

                FireEvent fireEvent = new FireEvent(initiationSite, day, ignitionType);

                fireEvent.Spread(PlugIn.ModelCore.CurrentTime, day, (ActiveSite) initiationSite);
                //if(fireEvent.CohortsKilled > 0)
                    LogEvent(PlugIn.ModelCore.CurrentTime, fireEvent);

                return fireEvent;


            }
            else
            {
                return null;
            }
        }

        

        //---------------------------------------------------------------------
        public void Spread(int currentTime, int day, ActiveSite site)
        {
            // First, load necessary parameters
            // Refer to design doc on Google Drive for questions or explanations

            if (day > maxDay)
            {
                maxDay = day;
                NumberOfDays++;
            }

            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            double fireWeatherIndex = 0.0;
            try
            {
                fireWeatherIndex = this.annualWeatherData.DailyFireWeatherIndex[day]; 
            }
            catch
            {
                throw new UninitializedClimateData(string.Format("Fire Weather Index could not be found \t year: {0}, day: {1} in ecoregion: {2} not found", currentTime, day, ecoregion.Name));
            }
            double windSpeed = this.annualWeatherData.DailyWindSpeed[day];
            double windDirection = this.annualWeatherData.DailyWindDirection[day];
            this.MeanWindDirection += windDirection;
            this.MeanWindSpeed += windSpeed;
            double fineFuels = 10.0; //SiteVars.FineFuels[site];  // NEED TO FIX NECN-Hydro installer
            double ladderFuelBiomass = 0.0;
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                foreach (ICohort cohort in speciesCohorts)
                    if (PlugIn.Parameters.LadderFuelSpeciesList.Contains(cohort.Species) && cohort.Age <= PlugIn.Parameters.LadderFuelMaxAge)
                        ladderFuelBiomass += cohort.Biomass;
            
            //PlugIn.ModelCore.UI.WriteLine("  Fire spreading.  Day = {0}, FWI = {1}, windSpeed = {2}, windDirection = {3}.", day, fireWeatherIndex, windSpeed, windDirection);

            // Is spread to this site allowable?
            //          Calculate P-spread based on fwi, adjusted wind speed, fine fuels, source intensity (or similar). (AK)
            //          Adjust P-spread to account for suppression (RMS)
            //          Compare P-spread-adj to random number

            // ********* TEMP ****************************************
            double Pspread_adjusted = 0.05;
            // ********* TEMP ****************************************
            double suppressEffect = 1.0;
            if (this.IgnitionType == Ignition.Accidental)
            {
                switch (SiteVars.AccidentalSuppressionIndex[site])
                {
                    case 1:
                        suppressEffect = (double)PlugIn.Parameters.AccidentalSuppressEffectivenss_low / 100.0;
                        break;
                    case 2:
                        suppressEffect = (double)PlugIn.Parameters.AccidentalSuppressEffectivenss_medium / 100.0;
                        break;
                    case 3:
                        suppressEffect = (double)PlugIn.Parameters.AccidentalSuppressEffectivenss_high / 100.0;
                        break;
                    default:
                        suppressEffect = 1.0;  // None
                        break;

                }
            }
            if (this.IgnitionType == Ignition.Lightning)
            {
                switch (SiteVars.LightningSuppressionIndex[site])
                {
                    case 1:
                        suppressEffect = (double) PlugIn.Parameters.LightningSuppressEffectivenss_low / 100.0;
                        break;
                    case 2:
                        suppressEffect = (double)PlugIn.Parameters.LightningSuppressEffectivenss_medium / 100.0;
                        break;
                    case 3:
                        suppressEffect = (double)PlugIn.Parameters.LightningSuppressEffectivenss_high / 100.0;
                        break;
                    default:
                        suppressEffect = 1.0;
                        break;

                }
            }
            if (this.IgnitionType == Ignition.Rx)
            {
                switch (SiteVars.RxSuppressionIndex[site])
                {
                    case 1:
                        suppressEffect = (double)PlugIn.Parameters.RxSuppressEffectivenss_low / 100.0;
                        break;
                    case 2:
                        suppressEffect = (double)PlugIn.Parameters.RxSuppressEffectivenss_medium / 100.0;
                        break;
                    case 3:
                        suppressEffect = (double)PlugIn.Parameters.RxSuppressEffectivenss_high / 100.0;
                        break;
                    default:
                        suppressEffect = 1.0;
                        break;

                }
            }
            Pspread_adjusted *= suppressEffect;
                //SiteVars.LightningSuppressionIndex[site] == 1)

            if (Pspread_adjusted > PlugIn.ModelCore.GenerateUniform())
            {
                SiteVars.Disturbed[site] = true;  // set to true, regardless of severity


                // Next, determine severity (0 = none, 1 = <4', 2 = 4-8', 3 = >8'.
                // Severity a function of ladder fuels, fine fuels, source spread intensity.
                siteSeverity = 1;
                int highSeverityRiskFactors = 0;
                if (fineFuels > PlugIn.Parameters.SeverityFactor_FineFuelPercentage)
                    highSeverityRiskFactors++;
                if (ladderFuelBiomass > PlugIn.Parameters.SeverityFactor_LadderFuelPercentage)
                    highSeverityRiskFactors++;
                if(SiteVars.Severity[initiationSite] > 2)
                    highSeverityRiskFactors++;

                if (highSeverityRiskFactors == 1)
                    siteSeverity = 2;
                if (highSeverityRiskFactors > 1)
                    siteSeverity = 3;
                // End SEVERITY calculation

                int siteCohortsKilled = 0;

                if (siteSeverity > 0)
                {
                    //      Cause mortality
                    siteCohortsKilled = Damage(site);
                    if (siteCohortsKilled > 0)
                    {
                        this.totalSitesDamaged++;
                    }

                    // Log information
                    SiteVars.TypeOfIginition[site] = (ushort) this.IgnitionType;
                    SiteVars.Severity[site] = (byte)siteSeverity;
                    SiteVars.DayOfFire[site] = (ushort) day;
                    this.MeanSeverity += siteSeverity;
                    if (siteSeverity == 1)
                        this.NumberCellsSeverity1++;
                    if (siteSeverity == 2)
                        this.NumberCellsSeverity2++;
                    if (siteSeverity == 3)
                        this.NumberCellsSeverity3++;

                }

                //      Calculate spread-area-max 
                double spreadAreaMaxHectares = PlugIn.Parameters.MaximumSpreadAreaB0 + 
                    PlugIn.Parameters.MaximumSpreadAreaB1*fireWeatherIndex + 
                    PlugIn.Parameters.MaximumSpreadAreaB2*windSpeed;
                
                if (!spreadArea.ContainsKey(day))
                {
                    spreadArea.Add(day, 1);  // second int is the cell count, later turned into area
                }
                else
                {
                    spreadArea[day]++;
                }

                //      Spread to neighbors
                List<Site> neighbors = Get4ActiveNeighbors(initiationSite);
                neighbors.RemoveAll(neighbor => SiteVars.Disturbed[neighbor] || !neighbor.IsActive);
                int neighborDay = day;


                foreach (Site neighborSite in neighbors)
                {
                    //  if spread-area > spread-area-max, day = day + 1
                    // Assuming that spreadAreaMax units are hectares:
                    double dailySpreadAreaHectares = spreadArea[day] * PlugIn.ModelCore.CellArea / 10000; // convert to Ha


                    if (dailySpreadAreaHectares > spreadAreaMaxHectares)
                        neighborDay = day+1;
                    this.Spread(PlugIn.ModelCore.CurrentTime, neighborDay, (ActiveSite)initiationSite);
                }


            }



        }

        //---------------------------------------------------------------------
        private static List<Site> Get4ActiveNeighbors(Site srcSite)
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
            SiteVars.Cohorts[site].ReduceOrKillBiomassCohorts(this); //.RemoveMarkedCohorts(this);
            return this.cohortsKilled - previousCohortsKilled;
        }

        //---------------------------------------------------------------------

        //  A filter to determine which cohorts are removed.
        int IDisturbance.ReduceOrKillMarkedCohort(ICohort cohort)
        //bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {
            bool killCohort = false;
            //int siteSeverity = 1;

            List<IFireDamage> fireDamages = null;
            if (siteSeverity == 1)
                fireDamages = PlugIn.Parameters.FireDamages_Severity1;
            if (siteSeverity == 2)
                fireDamages = PlugIn.Parameters.FireDamages_Severity2;
            if (siteSeverity == 3)
                fireDamages = PlugIn.Parameters.FireDamages_Severity3;

            foreach (IFireDamage damage in fireDamages)
            {
                if(cohort.Species == damage.DamageSpecies && cohort.Age >= damage.MinAge && cohort.Age < damage.MaxAge)
                {
                    if (damage.ProbablityMortality > PlugIn.ModelCore.GenerateUniform())
                    {
                        killCohort = true;
                        this.TotalBiomassMortality += cohort.Biomass;  
                    }
                    break;  // No need to search further

                }
            }

            if (killCohort) {
                this.cohortsKilled++;
            }
            return cohort.Biomass; // killCohort;
        }

        //---------------------------------------------------------------------

        public static void LogEvent(int currentTime, FireEvent fireEvent)
        {

            PlugIn.eventLog.Clear();
            EventsLog el = new EventsLog();
            el.SimulationYear = currentTime;
            el.InitRow = fireEvent.initiationSite.Location.Row;
            el.InitColumn = fireEvent.initiationSite.Location.Column;
            el.InitialFireWeatherIndex = fireEvent.InitiationFireWeatherIndex;
            el.IgnitionType = fireEvent.IgnitionType.ToString();
            el.NumberOfDays = fireEvent.NumberOfDays;
            el.TotalSitesBurned = fireEvent.TotalSitesDamaged;
            el.CohortsKilled = fireEvent.CohortsKilled;
            el.MeanSeverity = fireEvent.MeanSeverity / (double) fireEvent.TotalSitesDamaged;
            el.MeanWindDirection = fireEvent.MeanWindDirection / (double)fireEvent.TotalSitesDamaged;
            el.MeanWindSpeed = fireEvent.MeanWindSpeed / (double)fireEvent.TotalSitesDamaged;
            el.MeanSuppression = fireEvent.MeanSuppression / (double)fireEvent.TotalSitesDamaged;
            el.TotalBiomassMortality = fireEvent.TotalBiomassMortality;
            el.NumberCellsSeverity1 = fireEvent.NumberCellsSeverity1;
            el.NumberCellsSeverity2 = fireEvent.NumberCellsSeverity2;
            el.NumberCellsSeverity3 = fireEvent.NumberCellsSeverity3;

            PlugIn.eventLog.AddObject(el);
            PlugIn.eventLog.WriteToFile();

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
