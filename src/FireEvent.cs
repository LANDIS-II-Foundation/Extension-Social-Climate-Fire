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

    public enum IgnitionType : int
    {
        Accidental,
        Lightning,
        Rx
    }

    public enum IgnitionDistribution : int
    {
        Poisson,
        ZeroInflatedPoisson
    }

    public class FireEvent
        : IDisturbance
    {
        private static readonly bool isDebugEnabled = false; //debugLog.IsDebugEnabled;
        public static Random rnd = new Random();

        private ActiveSite initiationSite;
        private static List<ActiveSite[]> fireSites;  //an array to handle source and target sites.

        public int TotalSitesSpread;
        public int CohortsKilled;
        public int AvailableCohorts;
        public double InitiationFireWeatherIndex;
        public IgnitionType IgnitionType;
        AnnualClimate_Daily annualWeatherData;
        public int NumberOfDays;
        public int IgnitionDay;
        public double MeanIntensity;
        public double MeanWindDirection;
        public double MeanWindSpeed;
        public double MeanEffectiveWindSpeed;
        public double MeanSuppression;
        public double MeanSpreadProbability;
        public double MeanDNBR;
        public double MeanFWI;
        public double TotalBiomassMortality;
        public ActiveSite currentSite;
        public int NumberCellsSeverity1;
        public int NumberCellsSeverity2;
        public int NumberCellsSeverity3;
        public int NumberCellsSeverity4;
        public int NumberCellsSeverity5;
        //public int NumberCellsIntensityFactor1;
        //public int NumberCellsIntensityFactor2;
        //public int NumberCellsIntensityFactor3;
        public int TotalSitesBurned;
        public int MaxSpreadArea;

        //public Dictionary<int, int> spreadArea;

        public int maxDay;
        public int siteIntensity = 1;  //default is low intensity
        public int SiteMortality = 0;
        private double siteWindDirection = -999;
        private double siteWindSpeed = 0;
        private double siteFireWeatherIndex = 0;
        private double siteEffectiveWindSpeed = 0;


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
        public FireEvent(ActiveSite initiationSite, int day, IgnitionType ignitionType)
        {
            this.initiationSite = initiationSite;
            this.IgnitionDay = day;
            this.IgnitionType = ignitionType;
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[initiationSite];

            int actualYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_DailyData.First().Key;
            this.annualWeatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
            SiteVars.Disturbed[initiationSite] = true;

            this.CohortsKilled = 0;
            this.AvailableCohorts = 0;
            this.TotalSitesSpread = 0;
            this.TotalSitesBurned = 0;
            this.InitiationFireWeatherIndex = annualWeatherData.DailyFireWeatherIndex[day];
            this.NumberOfDays = 1;
            this.MeanIntensity = 0.0;
            this.MeanWindDirection = 0.0;
            this.MeanWindSpeed = 0.0;
            this.MeanEffectiveWindSpeed = 0.0;
            this.MeanSpreadProbability = 0.0;
            this.MeanDNBR = 0.0;
            this.MeanSuppression = 0.0;
            this.MeanFWI = 0.0;
            this.TotalBiomassMortality = 0.0;
            //this.NumberCellsSeverity1 = 0;
            //this.NumberCellsSeverity2 = 0;
            //this.NumberCellsSeverity3 = 0;
            //this.NumberCellsSeverity4 = 0;
            //this.NumberCellsSeverity5 = 0;
            this.currentSite = initiationSite;
            this.maxDay = day;
            //this.NumberCellsIntensityFactor1 = 0;
            //this.NumberCellsIntensityFactor2 = 0;
            //this.NumberCellsIntensityFactor3 = 0;

        }

        //---------------------------------------------------------------------
        public static FireEvent Initiate(ActiveSite initiationSite, int timestep, int day, IgnitionType ignitionType)
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
            //PlugIn.ModelCore.UI.WriteLine("   Fire spread function...");
            float dailySpreadArea = 0.0f;
            // First, take the first site off the list, ensuring that days are sequential from the beginning.
            while (fireSites.Count() > 0)
            {
                ActiveSite targetSite = fireSites.First()[0];
                ActiveSite sourceSite = fireSites.First()[1];

                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[targetSite];
                double fireWeatherIndex = 0.0;
                try
                {

                    fireWeatherIndex = Climate.Future_DailyData[PlugIn.ActualYear][ecoregion.Index].DailyFireWeatherIndex[day];
                }
                catch
                {
                    throw new UninitializedClimateData(string.Format("Fire Weather Index could not be found in Spread().  Year: {0}, Day: {1}, Ecoregion: {2}.", PlugIn.ActualYear, day, ecoregion.Name));
                }

                double effectiveWindSpeed = CalculateEffectiveWindSpeed(targetSite, sourceSite, fireWeatherIndex, day);

                //CalculateIntensity(targetSite, sourceSite);
                CalculateDNBR(targetSite);
                fireSites.RemoveAt(0);

                SiteVars.DayOfFire[targetSite] = (ushort) day;
                SiteVars.TimeOfLastFire[targetSite] = PlugIn.ModelCore.CurrentTime;
                dailySpreadArea += PlugIn.ModelCore.CellArea;

                if (day > PlugIn.DaysPerYear)
                    return;

                // DAY OF FIRE *****************************
                //      Calculate spread-area-max 
                if (this.IgnitionType == IgnitionType.Rx)
                {
                    if ((this.TotalSitesBurned * PlugIn.ModelCore.CellArea) > PlugIn.Parameters.RxTargetSize)
                        return;
                }
                else
                {
                    this.MaxSpreadArea = (int) (PlugIn.Parameters.MaximumSpreadAreaB0 +
                    (PlugIn.Parameters.MaximumSpreadAreaB1 * fireWeatherIndex) +
                    (PlugIn.Parameters.MaximumSpreadAreaB2 * effectiveWindSpeed));

                    //PlugIn.ModelCore.UI.WriteLine("   Day={0}, spreadAreaMaxHectares={1}, dailySpreadArea={2}, FWI={3}, WS={4}", day, spreadAreaMaxHectares, dailySpreadArea, fireWeatherIndex, effectiveWindSpeed);


                    //  if spread-area > spread-area-max, day = day + 1, assuming that spreadAreaMax units are hectares:
                    if (dailySpreadArea > this.MaxSpreadArea)
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

                // SPREAD from target to new neighbors ***********************
                if (day < PlugIn.DaysPerYear)
                {
                    sourceSite = targetSite;  // the target becomes the source
                    List<ActiveSite> neighbors = Get4CardinalActiveNeighbors(targetSite);
                    //neighbors.RemoveAll(neighbor => SiteVars.Disturbed[neighbor]);

                    foreach (ActiveSite neighborSite in neighbors)
                    {
                        if (CanSpread(neighborSite, sourceSite, day, fireWeatherIndex, effectiveWindSpeed, 1.0))
                        {

                            ActiveSite[] spread = new ActiveSite[] { neighborSite, sourceSite };
                            fireSites.Add(spread);
                            this.TotalSitesSpread++;
                        }
                    }
                    neighbors = Get4DiagonalNeighbors(targetSite);
                    //neighbors.RemoveAll(neighbor => SiteVars.Disturbed[neighbor]);

                    foreach (ActiveSite neighborSite in neighbors)
                    {
                        if (CanSpread(neighborSite, sourceSite, day, fireWeatherIndex, effectiveWindSpeed, 0.71))
                        {

                            ActiveSite[] spread = new ActiveSite[] { neighborSite, sourceSite };
                            fireSites.Add(spread);
                            this.TotalSitesSpread++;
                        }
                    }
                }
                // SPREAD to neighbors ***********************
            }


        }

        private void CalculateDNBR(ActiveSite site)
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

            // dNBR / DRdNBR calculation SITE scale
            // New mortality sub model for Scrpple used to model site level mortality to site level variables 
            // (Clay%, ET, Windspeed, Water Deficit, and Fuel)
            // The function for the site level mortality is generalized linear model utilizing a gamma distribution with an inverse link.

            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            // Establish the variables 
            double Clay = SiteVars.Clay[site];
            //double Previous_Year_ET = 0.0;
            //try
            //{
            //    Previous_Year_ET = Climate.Future_DailyData[PlugIn.ActualYear - 1][ecoregion.Index].AnnualAET;
            //}
            //catch
            //{
            //    // Indicating that we're at the first year, without a prior year.
            //    Previous_Year_ET = Climate.Future_DailyData[PlugIn.ActualYear][ecoregion.Index].AnnualAET;
            //}

            double Previous_Year_ET = SiteVars.PotentialEvapotranspiration[site];
            double WaterDeficit = SiteVars.ClimaticWaterDeficit[site];
            //double TotalFuels = SiteVars.FineFuels[site] + ladderFuelBiomass;

            /// For delayed relative delta normalized burn ratio (DRdNBR) calculation 
            double intercept = PlugIn.Parameters.SiteMortalityB0; //The parameter fit for the intercept 
            double Beta_Clay = PlugIn.Parameters.SiteMortalityB1; //The parameter fit for site level clay % in Soil.
            double Beta_ET = PlugIn.Parameters.SiteMortalityB2; //The parameter fit for site level previous years annual ET
            double Beta_Windspeed = PlugIn.Parameters.SiteMortalityB3;// The parameter fit for site level Effective Windspeed 
            double Beta_Water_Deficit = PlugIn.Parameters.SiteMortalityB4;//The parameter fit for site level PET-AET
            double Beta_Fuel = PlugIn.Parameters.SiteMortalityB5; //The parameter fit for site level fuels, here combining fine fuels and ladder fuels
            double Beta_LadderFuels = PlugIn.Parameters.SiteMortalityB6;

            double siteMortality = Math.Pow(Math.Max((intercept + (Clay * Beta_Clay)
                + (Previous_Year_ET * Beta_ET)
                + (siteEffectiveWindSpeed * Beta_Windspeed)
                + (WaterDeficit * Beta_Water_Deficit)
                + (ladderFuelBiomass * Beta_LadderFuels)
                + (fineFuelPercent * Beta_Fuel)), .0005), -1.0);

            siteMortality = Math.Max(siteMortality, 0.0);  // In the long-run, this shouldn't be necessary.  But useful for testing.

            int siteCohortsKilled = 0;
            this.MeanDNBR += (int) siteMortality;
            this.SiteMortality = (int) siteMortality;

            int standardSeverityIndex = Math.Max((int) siteMortality / 100, 1);
            SiteVars.Intensity[site] = (byte) Math.Min(standardSeverityIndex, 10);  // must range from 1-10.

            SiteVars.Mortality[site] = (int) siteMortality;
            SiteVars.TypeOfIginition[site] = (int)this.IgnitionType;
            //PlugIn.ModelCore.UI.WriteLine("  dNBR: {0}, severity={1}.", siteMortality, standardSeverityIndex);


            currentSite = site;
            siteCohortsKilled = Damage(site);


            this.TotalSitesBurned++;
            this.MeanWindDirection += siteWindDirection;
            this.MeanWindSpeed += siteWindSpeed;
            this.MeanFWI += siteFireWeatherIndex;
            this.MeanEffectiveWindSpeed += siteEffectiveWindSpeed;

            SiteVars.EventID[site] = PlugIn.EventID;

        }
        
        //---------------------------------------------------------------------
        //  A filter to determine which cohorts are removed.
        //  Use species level variables for bark thickness accumulation with age to calculate cohort level mortality. 
        // the cohort level mortality is a binomial distribution  

        int IDisturbance.ReduceOrKillMarkedCohort(ICohort cohort)
        {
            this.AvailableCohorts++;

            bool killCohort = false;

            // User Inputs
            double Beta_naught_m = PlugIn.Parameters.CohortMortalityB0; // Intercept parameter for mortality curve 
            double Beta_Bark = PlugIn.Parameters.CohortMortalityB1; // The parameter fit for the relationship between bark thickness and mortality. 
            double Beta_Site_Mortality = PlugIn.Parameters.CohortMortalityB2; // The parameter fit for the relationship between site level and individual level mortality. 

            // From the input file each species will need 
            // AgeDBH _Parameter is a parameter to scale Age and DBH estimated from a function in the form of 
            // It is essentially the half-life of the MaxBarkThickness *Age relationship. 
            // This is a logistic survival code with the MaxBarkThickness being asymptote. 
            // As age increase DBH approaches MaxBarkThickness
            double AgeDBH = SpeciesData.AgeDBH[cohort.Species];

            // The maximum measured Bark thickness. The asymptote of the logistic survival curve. 
            // This was calculated by using a species-specific bark DBH Coefficient described in Cansler 2020 and the maximum measured DBH form FIA. 
            //  Cansler, C. A., Hood, S. M., Varner, J. M., van Mantgem, P. J., Agne, M. C., Andrus, R. A., ... & 
            //  Bentz, B. J. (2020). The Fire and Tree Mortality Database, for empirical modeling of individual tree 
            //  mortality after fire. Scientific data, 7(1), 1-14.
            double MaxBarkThickness = SpeciesData.MaximumBarkThickness[cohort.Species];

            //// CohortAge  The age of the cohort
            double BarkThickness = (MaxBarkThickness * cohort.Age) / (cohort.Age + AgeDBH);

            double Pm = Math.Exp(Beta_naught_m + (Beta_Bark * BarkThickness) + (Beta_Site_Mortality * SiteMortality));

            double probabilityMortality = Pm / (1.0 + Pm);


            double random = PlugIn.ModelCore.GenerateUniform();
            if (probabilityMortality > random)
            {
                //PlugIn.ModelCore.UI.WriteLine("damage prob={0}, Random#={1}", ProbablityMortality, random);
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

            if (killCohort)
            {
                this.CohortsKilled++;
                return cohort.Biomass;
            }

            return 0;

        }


        private bool CanSpread(ActiveSite site, ActiveSite sourceSite, int day, double fireWeatherIndex, double effectiveWindSpeed, double distanceWeight)
        {
            bool spread = false;

            if (this.IgnitionType == IgnitionType.Rx && PlugIn.Parameters.RxZonesMap != null && SiteVars.RxZones[site] != SiteVars.RxZones[sourceSite])
            {
                //PlugIn.ModelCore.UI.WriteLine("  Fire spread zone limitation.  Spread not allowed to new site");
                return false;
            }

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
            double fwi1 = 0.0;
            double fwi2 = 0.0;
            int index = 0;

            if (this.IgnitionType == IgnitionType.Accidental)
            {
                try
                {
                    index = SiteVars.AccidentalSuppressionIndex[site] + ((int)this.IgnitionType * 10);
                }
                catch
                {
                    PlugIn.ModelCore.UI.WriteLine("NOTE: No table entry for Suppression MapCode {0}, Ignition Type {1}.  DEFAULT NO SUPPRESSION.", SiteVars.AccidentalSuppressionIndex[site], this.IgnitionType.ToString());
                }
            }
            if (this.IgnitionType == IgnitionType.Lightning)
                try
                {
                    index = SiteVars.LightningSuppressionIndex[site] + ((int)this.IgnitionType * 10);
                }
                catch
                {
                    PlugIn.ModelCore.UI.WriteLine("NOTE: No table entry for Suppression MapCode {0}, Ignition Type {1}.  DEFAULT NO SUPPRESSION.", SiteVars.AccidentalSuppressionIndex[site], this.IgnitionType.ToString());
                }
            if (this.IgnitionType == IgnitionType.Rx)
                try
                {
                    index = SiteVars.RxSuppressionIndex[site] + ((int)this.IgnitionType * 10);
                }
                catch
                {
                    PlugIn.ModelCore.UI.WriteLine("NOTE: No table entry for Suppression MapCode {0}, Ignition Type {1}.  DEFAULT NO SUPPRESSION.", SiteVars.AccidentalSuppressionIndex[site], this.IgnitionType.ToString());
                }

            if (index > 0)
            {
                fwi1 = PlugIn.Parameters.SuppressionFWI_Table[index].FWI_Break1;
                fwi2 = PlugIn.Parameters.SuppressionFWI_Table[index].FWI_Break2;

                if (fireWeatherIndex < fwi1)
                    suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[index].Suppression0 / 100.0);
                else if (fireWeatherIndex >= fwi1 && fireWeatherIndex < fwi2)
                    suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[index].Suppression1 / 100.0);
                else if (fireWeatherIndex >= fwi2)
                    suppressEffect = 1.0 - ((double)PlugIn.Parameters.SuppressionFWI_Table[index].Suppression2 / 100.0);
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
            //The distance weight accounts for the longer centroid distance between diagonal spread 
            // as compared to cardinal spread.  This is intended to correct the 'square effect' when Pspread is relatively uniform. 
            Pspread *= distanceWeight;

            if (this.IgnitionType == IgnitionType.Rx)
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
            siteWindDirection = windDirection;
            siteWindSpeed = windSpeed;
            siteFireWeatherIndex = fireWeatherIndex;

            double combustionBuoyancy = 10.0;  // Cannot be zero, also very insensitive when UaUb > 5.
            //if (SiteVars.Intensity[sourceSite] <= 3)
            //    combustionBuoyancy = 10.0;
            if (SiteVars.Intensity[sourceSite] > 3)
                combustionBuoyancy = 25.0;
            if (SiteVars.Intensity[sourceSite] > 6)
                combustionBuoyancy = 50.0;

            double UaUb = windSpeed / combustionBuoyancy;
            double slopeRadians = (double)SiteVars.GroundSlope[site] / 180.0 * Math.PI; //convert from Degrees to Radians
            double slopeAngle = (double)SiteVars.UphillSlopeAzimuth[site];
            double relativeWindDirection = (windDirection - slopeAngle) / 180.0 * Math.PI;

            // From R.M. Nelson Intl J Wildland Fire, 2002
            double effectiveWindSpeed = combustionBuoyancy * (Math.Pow(Math.Pow(UaUb, 2.0) + (2.0 * (UaUb) * Math.Sin(slopeRadians) * Math.Cos(relativeWindDirection)) + Math.Pow(Math.Sin(slopeRadians), 2.0), 0.5));

            siteEffectiveWindSpeed = effectiveWindSpeed;

            return effectiveWindSpeed;
            // End EFFECTIVE WIND SPEED ************************

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
            el.MaximumSpreadArea = fireEvent.MaxSpreadArea;
            el.MeanSpreadProbability = fireEvent.MeanSpreadProbability / (double)fireEvent.TotalSitesSpread;
            el.MeanFWI = fireEvent.MeanFWI / (double)fireEvent.TotalSitesBurned;
            el.TotalSitesBurned = fireEvent.TotalSitesBurned;
            el.CohortsKilled = fireEvent.CohortsKilled;
            el.AvailableCohorts = fireEvent.AvailableCohorts;
            el.MeanSeverity = fireEvent.MeanIntensity / (double) fireEvent.TotalSitesBurned;
            el.MeanDNBR = fireEvent.MeanDNBR / (double)fireEvent.TotalSitesBurned;
            el.MeanWindDirection = fireEvent.MeanWindDirection / (double)fireEvent.TotalSitesBurned;
            el.MeanWindSpeed = fireEvent.MeanWindSpeed / (double)fireEvent.TotalSitesBurned;
            el.MeanEffectiveWindSpeed = fireEvent.MeanEffectiveWindSpeed / (double)fireEvent.TotalSitesBurned;
            el.MeanSuppressionEffectiveness = fireEvent.MeanSuppression / (double)fireEvent.TotalSitesSpread;
            el.TotalBiomassMortality = fireEvent.TotalBiomassMortality;
            //el.NumberCellsSeverity1 = fireEvent.NumberCellsSeverity1;
            //el.NumberCellsSeverity2 = fireEvent.NumberCellsSeverity2;
            //el.NumberCellsSeverity3 = fireEvent.NumberCellsSeverity3;
            //el.NumberCellsSeverity4 = fireEvent.NumberCellsSeverity4;
            //el.NumberCellsSeverity5 = fireEvent.NumberCellsSeverity5;
            //el.PercentsCellsIntensityFactor1 = (double) fireEvent.NumberCellsIntensityFactor1 / (double)fireEvent.TotalSitesBurned;
            //el.PercentsCellsIntensityFactor2 = (double)fireEvent.NumberCellsIntensityFactor2 / (double)fireEvent.TotalSitesBurned;
            //el.PercentsCellsIntensityFactor3 = (double)fireEvent.NumberCellsIntensityFactor3 / (double)fireEvent.TotalSitesBurned;

            PlugIn.eventLog.AddObject(el);
            PlugIn.eventLog.WriteToFile();

        }

        //---------------------------------------------------------------------
        private static List<ActiveSite> Get8ActiveNeighbors(Site srcSite)
        {
            if (!srcSite.IsActive)
                throw new ApplicationException("Source site is not active.");

            List<ActiveSite> neighbors = new List<ActiveSite>();

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

                if (neighbor != null && neighbor.IsActive && !SiteVars.Disturbed[neighbor])
                {
                    neighbors.Add((ActiveSite)neighbor);
                }
            }

            return neighbors;
        }
        //---------------------------------------------------------------------
        private static List<ActiveSite> Get4CardinalActiveNeighbors(Site srcSite)
        {
            if (!srcSite.IsActive)
                throw new ApplicationException("Source site is not active.");

            List<ActiveSite> neighbors = new List<ActiveSite>();

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

                if (neighbor != null && neighbor.IsActive && !SiteVars.Disturbed[neighbor])
                {
                    neighbors.Add((ActiveSite)neighbor);
                }
            }

            return neighbors;
        }
        //---------------------------------------------------------------------
        private static List<ActiveSite> Get4DiagonalNeighbors(Site srcSite)
        {
            if (!srcSite.IsActive)
                throw new ApplicationException("Source site is not active.");

            List<ActiveSite> neighbors = new List<ActiveSite>();

            RelativeLocation[] neighborhood = new RelativeLocation[]
            {
                new RelativeLocation(-1,  1),  // northwest
                new RelativeLocation( 1,  1),  // northeast
                new RelativeLocation( 1,  -1),  // southeast
                new RelativeLocation( -1, -1),  // southwest
            };

            foreach (RelativeLocation relativeLoc in neighborhood)
            {
                Site neighbor = srcSite.GetNeighbor(relativeLoc);

                if (neighbor != null && neighbor.IsActive && !SiteVars.Disturbed[neighbor])
                {
                    neighbors.Add((ActiveSite)neighbor);
                }
            }

            return neighbors;
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
