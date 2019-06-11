//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

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
        Rx
    }

    public class FireEvent
        : IDisturbance
    {
        private static readonly bool isDebugEnabled = false; //debugLog.IsDebugEnabled;
        public static Random rnd = new Random();

        private ActiveSite initiationSite;
        private static List<ActiveSite[]> fireSites;

        public int TotalSitesDamaged;
        public int CohortsKilled;
        public double InitiationFireWeatherIndex;
        public Ignition IgnitionType;
        AnnualClimate_Daily annualWeatherData;
        public int NumberOfDays;
        public int IgnitionDay;
        public double MeanSeverity;
        public double MeanWindDirection;
        public double MeanWindSpeed;
        public double MeanEffectiveWindSpeed;
        public double MeanSuppression;
        public double MeanSpreadProbability;
        public double MeanFWI;
        public double TotalBiomassMortality;
        public ActiveSite currentSite;
        public int NumberCellsSeverity1;
        public int NumberCellsSeverity2;
        public int NumberCellsSeverity3;

        //public Dictionary<int, int> spreadArea;

        public int maxDay;
        public int siteIntensity;

        //---------------------------------------------------------------------
        static FireEvent()
        {
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
                return currentSite;
            }
        }

        // Constructor function
        public FireEvent(ActiveSite initiationSite, int day, Ignition ignitionType)
        {
            this.initiationSite = initiationSite;
            this.IgnitionDay = day;
            this.IgnitionType = ignitionType;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[initiationSite];

            int actualYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_DailyData.First().Key;
            this.annualWeatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
            SiteVars.Disturbed[initiationSite] = true;

            this.CohortsKilled = 0;
            this.TotalSitesDamaged = 1;  // minimum 1 for the ignition cell
            this.InitiationFireWeatherIndex = annualWeatherData.DailyFireWeatherIndex[day];
            this.NumberOfDays = 1;
            this.MeanSeverity = 0.0;
            this.MeanWindDirection = 0.0;
            this.MeanWindSpeed = 0.0;
            this.MeanEffectiveWindSpeed = 0.0;
            this.MeanSpreadProbability = 0.0;
            this.MeanSuppression = 0.0;
            this.MeanFWI = 0.0;
            this.TotalBiomassMortality = 0.0;
            this.NumberCellsSeverity1 = 0;
            this.NumberCellsSeverity2 = 0;
            this.NumberCellsSeverity3 = 0;
            this.currentSite = initiationSite;
            this.maxDay = day;

        }

        //---------------------------------------------------------------------
        public static FireEvent Initiate(ActiveSite initiationSite, int timestep, int day, Ignition ignitionType)
        {
            //PlugIn.ModelCore.UI.WriteLine("  Fire Event initiated.  Day = {0}, IgnitionType = {1}.", day, ignitionType);

            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("   Fire event started at {0} ...", initiationSite.Location);

            FireEvent fireEvent = new FireEvent(initiationSite, day, ignitionType);
            PlugIn.EventID++;

            ActiveSite[] initialSites = new ActiveSite[2];
            initialSites[0] = (ActiveSite)initiationSite;
            initialSites[1] = (ActiveSite)initiationSite;

            fireSites = new List<ActiveSite[]>();
            fireSites.Add(initialSites);

            // desitination and source are the same for ignition site
            fireEvent.Spread(PlugIn.ModelCore.CurrentTime, day); 

            LogEvent(PlugIn.ModelCore.CurrentTime, fireEvent, PlugIn.EventID);

            return fireEvent;
        }




        //---------------------------------------------------------------------
        private void Spread(int currentTime, int day)
        {
            float dailySpreadArea = 0.0f;
            // First, take the first site off the list, ensuring that days are sequential from the beginning.
            while (fireSites.Count() > 0)
            {
                ActiveSite site = fireSites.First()[0];
                ActiveSite sourceSite = fireSites.First()[1];

                CalculateIntensity(site, sourceSite);

                SiteVars.DayOfFire[site] = (ushort) day;
                dailySpreadArea += PlugIn.ModelCore.CellArea;

                if (day > PlugIn.DaysPerYear)
                    return;

                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                double fireWeatherIndex = 0.0;
                try
                {

                    fireWeatherIndex = Climate.Future_DailyData[PlugIn.ActualYear][ecoregion.Index].DailyFireWeatherIndex[day];
                }
                catch
                {
                    throw new UninitializedClimateData(string.Format("Fire Weather Index could not be found in Spread().  Year: {0}, Day: {1}, Ecoregion: {2}.", PlugIn.ActualYear, day, ecoregion.Name));
                }

                double effectiveWindSpeed = CalculateEffectiveWindSpeed(site, sourceSite, fireWeatherIndex, day);
                this.MeanEffectiveWindSpeed += effectiveWindSpeed;

                // DAY OF FIRE *****************************
                //      Calculate spread-area-max 
                if (this.IgnitionType == Ignition.Rx)
                {
                    if (this.TotalSitesDamaged > PlugIn.Parameters.RxTargetSize)
                        return;
                }
                else
                {
                    double spreadAreaMaxHectares = PlugIn.Parameters.MaximumSpreadAreaB0 +
                    (PlugIn.Parameters.MaximumSpreadAreaB1 * fireWeatherIndex) +
                    (PlugIn.Parameters.MaximumSpreadAreaB2 * effectiveWindSpeed);

                    //PlugIn.ModelCore.UI.WriteLine("   Day={0}, spreadAreaMaxHectares={1}, dailySpreadArea={2}, FWI={3}, WS={4}", day, spreadAreaMaxHectares, dailySpreadArea, fireWeatherIndex, effectiveWindSpeed);


                    //  if spread-area > spread-area-max, day = day + 1, assuming that spreadAreaMax units are hectares:
                    if (dailySpreadArea > spreadAreaMaxHectares)
                    {
                        day++;  // GOTO the next day.
                        dailySpreadArea = 0;
                    }

                    if (day > maxDay)
                    {
                        maxDay = day;
                        NumberOfDays++;
                    }
                }
                // DAY OF FIRE *****************************

                // SPREAD to neighbors ***********************
                if (day < PlugIn.DaysPerYear)
                {
                    List<Site> neighbors = Get4ActiveNeighbors(site);
                    neighbors.RemoveAll(neighbor => SiteVars.Disturbed[neighbor] || !neighbor.IsActive);

                    foreach (Site neighborSite in neighbors)
                    {
                        if (CanSpread((ActiveSite)neighborSite, site, day, fireWeatherIndex, effectiveWindSpeed))
                        {

                            ActiveSite[] spread = new ActiveSite[] { (ActiveSite)neighborSite, site };
                            fireSites.Add(spread);
                            //this.TotalSitesDamaged++;
                        }
                    }
                }
                // SPREAD to neighbors ***********************
                fireSites.RemoveAt(0);
            }


        }

        private void CalculateIntensity(ActiveSite site, ActiveSite sourceSite)
        {

            //PlugIn.ModelCore.UI.WriteLine("  Calculate Intensity: {0}.", site);

            double fineFuelPercent = 0.0;
            try
            {
                fineFuelPercent = Math.Min(SiteVars.FineFuels[site] / PlugIn.Parameters.MaxFineFuels, 1.0);
            }
            catch
            {
                PlugIn.ModelCore.UI.WriteLine("NOTE: FINE FUELS NOT OPERATIONAL.  DEFAULT IS ZERO.");
            }

            // LADDER FUELS ************************
            double ladderFuelBiomass = 0.0;
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                foreach (ICohort cohort in speciesCohorts)
                    if (PlugIn.Parameters.LadderFuelSpeciesList.Contains(cohort.Species) && cohort.Age <= PlugIn.Parameters.LadderFuelMaxAge)
                        ladderFuelBiomass += cohort.Biomass;
            // End LADDER FUELS ************************

            // INTENSITY calculation **************************
            // Next, determine severity (0 = none, 1 = <4', 2 = 4-8', 3 = >8'.
            // Severity a function of ladder fuels, fine fuels, source spread intensity.
            siteIntensity = 1;
            int highSeverityRiskFactors = 0;

            if (fineFuelPercent > PlugIn.Parameters.IntensityFactor_FineFuelPercent)
                highSeverityRiskFactors++;
            if (ladderFuelBiomass > PlugIn.Parameters.IntensityFactor_LadderFuelBiomass)
                highSeverityRiskFactors++;
            if (SiteVars.Intensity[sourceSite] == 3)
                highSeverityRiskFactors++;

            if (highSeverityRiskFactors == 1)
                siteIntensity = 2;
            if (highSeverityRiskFactors > 1)
                siteIntensity = 3;
            // End INTENSITY calculation **************************

            if (this.IgnitionType == Ignition.Rx)
                siteIntensity = Math.Min(siteIntensity, PlugIn.Parameters.RxMaxFireIntensity);

            int siteCohortsKilled = 0;

            if (siteIntensity > 0)
            {
                //      Cause mortality
                SiteVars.Intensity[site] = (byte) siteIntensity;
                SiteVars.TypeOfIginition[site] = (int)this.IgnitionType;

                currentSite = site;
                siteCohortsKilled = Damage(site);
                this.TotalSitesDamaged++;

                this.MeanSeverity += siteIntensity;
                if (siteIntensity == 1)
                    this.NumberCellsSeverity1++;
                if (siteIntensity == 2)
                    this.NumberCellsSeverity2++;
                if (siteIntensity == 3)
                    this.NumberCellsSeverity3++;

            }

        }

        private bool CanSpread(ActiveSite site, ActiveSite sourceSite, int day, double fireWeatherIndex, double effectiveWindSpeed)
        {
            bool spread = false;

            if (this.IgnitionType == Ignition.Rx && PlugIn.Parameters.RxZonesMap != null && SiteVars.RxZones[site] != SiteVars.RxZones[sourceSite])
            {
                //PlugIn.ModelCore.UI.WriteLine("  Fire spread zone limitation.  Spread not allowed to new site");
                return false;
            }

            //SiteVars.TypeOfIginition[site] = (int) this.IgnitionType;

            SiteVars.Disturbed[site] = true;  // set to true, regardless of whether fire burns; this prevents endless checking of the same site.

            double fineFuelPercent = 0.0;
            double fineFuelPercent_harvest = 1.0;
            try
            {
                fineFuelPercent = Math.Min(SiteVars.FineFuels[site] / PlugIn.Parameters.MaxFineFuels, 1.0);
                if (SiteVars.HarvestTime != null && SiteVars.HarvestTime[site] > PlugIn.ModelCore.CurrentTime)
                    fineFuelPercent_harvest = (System.Math.Min(1.0, (double)(PlugIn.ModelCore.CurrentTime - SiteVars.HarvestTime[site]) * 0.1));

                fineFuelPercent = Math.Min(fineFuelPercent, fineFuelPercent_harvest);
            }
            catch
            {
                PlugIn.ModelCore.UI.WriteLine("NOTE: FINE FUELS NOT OPERATIONAL.  DEFAULT IS ZERO.");
            }

            // LADDER FUELS ************************
            double ladderFuelBiomass = 0.0;
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                foreach (ICohort cohort in speciesCohorts)
                    if (PlugIn.Parameters.LadderFuelSpeciesList.Contains(cohort.Species) && cohort.Age <= PlugIn.Parameters.LadderFuelMaxAge)
                        ladderFuelBiomass += cohort.Biomass;
            // End LADDER FUELS ************************


            // SUPPRESSION ************************
            double suppressEffect = 1.0; // 1.0 = no effect

            if (this.IgnitionType == Ignition.Accidental)
            {
                switch (SiteVars.AccidentalSuppressionIndex[site])
                {
                    case 1:
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);
                        break;
                    case 2:
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break1)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessMedium / 100.0);
                        break;
                    case 3:
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break1)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessMedium / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break2)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessHigh / 100.0);
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
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);
                        break;
                    case 2:
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break1)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessMedium / 100.0);
                        break;
                    case 3:
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break1)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessMedium / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break2)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessHigh / 100.0);
                        break;
                    default:
                        suppressEffect = 1.0;  // None
                        break;

                }
            }
            if (this.IgnitionType == Ignition.Rx)
            {
                switch (SiteVars.RxSuppressionIndex[site])
                {
                    case 1:
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);
                        break;
                    case 2:
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break1)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessMedium / 100.0);
                        break;
                    case 3:
                        suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessLow / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break1)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessMedium / 100.0);

                        if (fireWeatherIndex > PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].FWI_Break2)
                            suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[(int)this.IgnitionType].EffectivenessHigh / 100.0);
                        break;
                    default:
                        suppressEffect = 1.0;  // None
                        break;

                }
            }

            // NO suppression above a given wind speed due to dangers to firefighters and aircraft.
            if (effectiveWindSpeed > PlugIn.Parameters.SuppressionMaxWindSpeed)
                suppressEffect = 1.0;

            // End SUPPRESSION ************************

            // PROBABILITY OF SPREAD calculation **************************
            // Is spread to this site allowable?
            //          Calculate P-spread based on fwi, adjusted wind speed, fine fuels, source intensity (or similar).
            //          Adjust P-spread to account for suppression
            //          Compare P-spread-adj to random number
            double spreadB0 = PlugIn.Parameters.SpreadProbabilityB0;
            double spreadB1 = PlugIn.Parameters.SpreadProbabilityB1;
            double spreadB2 = PlugIn.Parameters.SpreadProbabilityB2;
            double spreadB3 = PlugIn.Parameters.SpreadProbabilityB3;

            double Pspread = Math.Pow(Math.E, -1.0 * (spreadB0 + (spreadB1 * fireWeatherIndex) + (spreadB2 * fineFuelPercent) + (spreadB3 * effectiveWindSpeed)));
            Pspread = 1.0 / (1.0 + Pspread);

            if (this.IgnitionType == Ignition.Rx)
                Pspread = 1.0;

            // End PROBABILITY OF SPREAD calculation **************************

            double Pspread_adjusted = Pspread * suppressEffect;

            if (Pspread_adjusted > PlugIn.ModelCore.GenerateUniform())
                spread = true;

            if (spread)
            {
                this.MeanSpreadProbability += Pspread_adjusted;
                this.MeanSuppression += (1.0 - suppressEffect) * 100.0;
            }

            SiteVars.SpreadProbability[site] = Pspread_adjusted;

            return spread;

        }
        private double CalculateEffectiveWindSpeed(ActiveSite site, ActiveSite sourceSite, double fireWeatherIndex, int day)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            // EFFECTIVE WIND SPEED ************************
            double windSpeed = Climate.Future_DailyData[PlugIn.ActualYear][ecoregion.Index].DailyWindSpeed[day];
            double windDirection = Climate.Future_DailyData[PlugIn.ActualYear][ecoregion.Index].DailyWindDirection[day];// / 180 * Math.PI;
            this.MeanWindDirection += windDirection;
            this.MeanWindSpeed += windSpeed;
            this.MeanFWI += fireWeatherIndex;

            double combustionBuoyancy = 10.0;  // Cannot be zero, also very insensitive when UaUb > 5.
            if (SiteVars.Intensity[sourceSite] == 1)
                combustionBuoyancy = 10.0;
            if (SiteVars.Intensity[sourceSite] == 2)
                combustionBuoyancy = 25.0;
            if (SiteVars.Intensity[sourceSite] == 3)
                combustionBuoyancy = 50.0;

            double UaUb = windSpeed / combustionBuoyancy;
            double slopeDegrees = (double)SiteVars.GroundSlope[site] / 180.0 * Math.PI; //convert from Radians to Degrees
            double slopeAngle = (double)SiteVars.UphillSlopeAzimuth[site];
            double relativeWindDirection = (windDirection - slopeAngle) / 180.0 * Math.PI;

            // From R.M. Nelson Intl J Wildland Fire, 2002
            double effectiveWindSpeed = combustionBuoyancy * (Math.Pow(Math.Pow(UaUb, 2.0) + (2.0 * (UaUb) * Math.Sin(slopeDegrees) * Math.Cos(relativeWindDirection)) + Math.Pow(Math.Sin(slopeDegrees), 2.0), 0.5));

            return effectiveWindSpeed;
            // End EFFECTIVE WIND SPEED ************************

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
                new RelativeLocation(-1,  1),  // northwest
                new RelativeLocation( 1,  1),  // northeast
                new RelativeLocation( 1,  -1),  // southeast
                new RelativeLocation( -1, -1),  // southwest
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
            //PlugIn.ModelCore.UI.WriteLine("  Calculate Damage: {0}.", site);
            int previousCohortsKilled = this.CohortsKilled;
            SiteVars.Cohorts[site].ReduceOrKillBiomassCohorts(this); 
            return this.CohortsKilled - previousCohortsKilled;
        }

        //---------------------------------------------------------------------

        //  A filter to determine which cohorts are removed.
        int IDisturbance.ReduceOrKillMarkedCohort(ICohort cohort)
        {
            bool killCohort = false;

            List<IFireDamage> fireDamages = null;
            if (siteIntensity == 1)
                fireDamages = PlugIn.Parameters.FireDamages_Severity1;
            if (siteIntensity == 2)
                fireDamages = PlugIn.Parameters.FireDamages_Severity2;
            if (siteIntensity == 3)
                fireDamages = PlugIn.Parameters.FireDamages_Severity3;

            foreach (IFireDamage damage in fireDamages)
            {
                if(cohort.Species == damage.DamageSpecies && cohort.Age >= damage.MinAge && cohort.Age < damage.MaxAge)
                {
                    double random = PlugIn.ModelCore.GenerateUniform();
                    if (damage.ProbablityMortality > random)
                    {
                        //PlugIn.ModelCore.UI.WriteLine("damage prob={0}, Random#={1}", damage.ProbablityMortality, random);
                        killCohort = true;
                        this.TotalBiomassMortality += cohort.Biomass;  
                        foreach (IDeadWood deadwood in PlugIn.Parameters.DeadWoodList)
                        {
                            if (cohort.Species == deadwood.Species && cohort.Age >= deadwood.MinAge)
                            {
                                SiteVars.SpecialDeadWood[this.currentSite] += cohort.Biomass;
                                //PlugIn.ModelCore.UI.WriteLine("special dead = {0}, site={1}.", SiteVars.SpecialDeadWood[this.Current_damage_site], this.Current_damage_site);

                            }
                        }
                    }
                    break;  // No need to search further

                }
            }

            if (killCohort)
            {
                this.CohortsKilled++;
                return cohort.Biomass;
            }

            return 0;

        }

        //---------------------------------------------------------------------

        public static void LogEvent(int currentTime, FireEvent fireEvent, int eventID)
        {

            PlugIn.eventLog.Clear();
            EventsLog el = new EventsLog();
            el.EventID = eventID;
            el.SimulationYear = currentTime;
            el.InitRow = fireEvent.initiationSite.Location.Row;
            el.InitColumn = fireEvent.initiationSite.Location.Column;
            el.InitialFireWeatherIndex = fireEvent.InitiationFireWeatherIndex;
            el.IgnitionType = fireEvent.IgnitionType.ToString();
            el.InitialDayOfYear = fireEvent.IgnitionDay;
            el.NumberOfDays = fireEvent.NumberOfDays;
            el.MeanSpreadProbability = fireEvent.MeanSpreadProbability / (double)fireEvent.TotalSitesDamaged;
            el.MeanFWI = fireEvent.MeanFWI / (double)fireEvent.TotalSitesDamaged;
            el.TotalSitesBurned = fireEvent.TotalSitesDamaged;
            el.CohortsKilled = fireEvent.CohortsKilled;
            el.MeanSeverity = fireEvent.MeanSeverity / (double) fireEvent.TotalSitesDamaged;
            el.MeanWindDirection = fireEvent.MeanWindDirection / (double)fireEvent.TotalSitesDamaged;
            el.MeanWindSpeed = fireEvent.MeanWindSpeed / (double)fireEvent.TotalSitesDamaged;
            el.MeanEffectiveWindSpeed = fireEvent.MeanEffectiveWindSpeed / (double)fireEvent.TotalSitesDamaged;
            el.MeanSuppressionEffectiveness = fireEvent.MeanSuppression / (double)fireEvent.TotalSitesDamaged;
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
