//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Landis.SpatialModeling;
using Landis.Library.Climate;
using Landis.Core;
using Landis.Library.Metadata;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Landis.Extension.Scrapple
{
    ///<summary>
    /// A disturbance plug-in that simulates Fire disturbance.
    /// </summary>
    public class PlugIn
        : ExtensionMain 
    {
        private static readonly bool isDebugEnabled = true; 

        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:fire");
        public static readonly string ExtensionName = "SCRAPPLE";
        public static MetadataTable<EventsLog> eventLog;
        public static MetadataTable<SummaryLog> summaryLog;
        public static MetadataTable<IgnitionsLog> ignitionsLog;
        
        // Get the active sites from the landscape and shuffle them 
        public List<ActiveSite> activeRxSites; 
        public List<ActiveSite> activeAccidentalSites;
        public List<ActiveSite> activeLightningSites;
        public double rxTotalWeight;
        public double accidentalTotalWeight;
        public double lightningTotalWeight;

        public static int FutureClimateBaseYear;
        public static Dictionary<int, int> sitesPerEcoregions;
        public static Dictionary<int, double> fractionSitesPerEcoregions;
        public static int ActualYear;
        public static int EventID = 0;

        private static int[] totalBurnedSites;
        private static int[] numberOfFire;
        private static int[] totalBiomassMortality;
        private static int numCellsSeverity1;
        private static int numCellsSeverity2;
        private static int numCellsSeverity3;

        public static IInputParameters Parameters;
        private static ICore modelCore;

        public static double MaximumSpreadAreaB0;
        public static double MaximumSpreadAreaB1;
        public static double MaximumSpreadAreaB2;

        public static int DaysPerYear = 364;

        private List<IDynamicIgnitionMap> dynamicRxIgns;
        // VS: hasn't been properly integrated into Climate Library.
        //int daysPerYear = (AnnualClimate.IsLeapYear(actualYear) ? true : false) ? 366 : 365;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile,
                                            ICore mCore)
        {
            modelCore = mCore;
            SiteVars.Initialize();
            InputParameterParser parser = new InputParameterParser();
            Parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            Timestep = 1;  // RMS:  Initially we will force annual time step. parameters.Timestep;


            ///******************** DEBUGGER LAUNCH *********************
            /// 
            /*
            if (Debugger.Launch())
            {
                modelCore.UI.WriteLine("Debugger is attached");
                if (Debugger.IsLogging())
                {
                    modelCore.UI.WriteLine("Debugging is logging");
                }
                Debugger.Break();
            }
            else
            { 
                modelCore.UI.WriteLine("Debugger not attached");
            }
            */
            ///******************** DEBUGGER END *********************

            // Initilize the FireRegions Maps
            modelCore.UI.WriteLine("Initializing SCRAPPLE Fire...");

            dynamicRxIgns = Parameters.DynamicRxIgnitionMaps;
            MapUtility.Initilize(Parameters.LighteningFireMap, Parameters.AccidentalFireMap, Parameters.RxFireMap,
                                 Parameters.LighteningSuppressionMap, Parameters.AccidentalSuppressionMap, Parameters.RxSuppressionMap);
            if (Parameters.RxZonesMap != null)
                MapUtility.ReadMap(Parameters.RxZonesMap, SiteVars.RxZones);
            MetadataHandler.InitializeMetadata(Parameters.Timestep, ModelCore);

            sitesPerEcoregions = new Dictionary<int, int>();

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                if (!sitesPerEcoregions.ContainsKey(ecoregion.Index))
                    sitesPerEcoregions.Add(ecoregion.Index, 1);
                else
                    sitesPerEcoregions[ecoregion.Index]++;

            }

            fractionSitesPerEcoregions = new Dictionary<int, double>();
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (sitesPerEcoregions.ContainsKey(ecoregion.Index))
                {
                    fractionSitesPerEcoregions.Add(ecoregion.Index, ((double)sitesPerEcoregions[ecoregion.Index] / (double)modelCore.Landscape.ActiveSiteCount));
                }
            }

            double totalWeight = 0.0;
            activeRxSites = PreShuffle(SiteVars.RxFireWeight, out totalWeight);
            rxTotalWeight = totalWeight;
            activeAccidentalSites = PreShuffle(SiteVars.AccidentalFireWeight, out totalWeight);
            accidentalTotalWeight = totalWeight;
            activeLightningSites = PreShuffle(SiteVars.LightningFireWeight, out totalWeight);
            lightningTotalWeight = totalWeight;
        }

        //---------------------------------------------------------------------

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {

            if (PlugIn.ModelCore.CurrentTime > 0)
                SiteVars.InitializeDisturbances();

            SiteVars.Disturbed.ActiveSiteValues = false;
            SiteVars.Intensity.ActiveSiteValues = 0;
            SiteVars.SpreadProbability.ActiveSiteValues = 0.0;
            SiteVars.DayOfFire.ActiveSiteValues = 0;
            SiteVars.TypeOfIginition.ActiveSiteValues = 0;
            SiteVars.SpecialDeadWood.ActiveSiteValues = 0;
            SiteVars.EventID.ActiveSiteValues = 0;

            foreach (IDynamicIgnitionMap dynamicIgnitions in dynamicRxIgns)
            {
                if (dynamicIgnitions.Year == PlugIn.modelCore.CurrentTime)
                {
                    PlugIn.ModelCore.UI.WriteLine("   Reading in new Fire Regions Map {0}.", dynamicIgnitions.MapName);
                    MapUtility.ReadMap(dynamicIgnitions.MapName, SiteVars.RxFireWeight);

                    double totalWeight = 0.0;
                    activeRxSites = PreShuffle(SiteVars.RxFireWeight, out totalWeight);
                    rxTotalWeight = totalWeight;

                }

            }

            AnnualClimate_Daily weatherData = null;
            totalBurnedSites = new int[3];
            numberOfFire = new int[3];
            totalBiomassMortality = new int[3];
            numCellsSeverity1 = 0;
            numCellsSeverity2 = 0;
            numCellsSeverity3 = 0;

            modelCore.UI.WriteLine("   Processing landscape for Fire events ...");

            ActualYear = 0;
            try
            {
                ActualYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_AllData.First().Key;
            }
            catch
            {
                throw new UninitializedClimateData(string.Format("Could not initilize the actual year {0} from climate data", ActualYear));
            }

            modelCore.UI.WriteLine("   Next, shuffle ignition sites...");
            // Get the active sites from the landscape and shuffle them 
            // Sites are weighted for ignition in the Shuffle method, based on the respective inputs maps.
            List<ActiveSite> shuffledLightningFireSites = Shuffle(activeLightningSites, SiteVars.LightningFireWeight, lightningTotalWeight);
            List<ActiveSite> shuffledRxFireSites = Shuffle(activeRxSites, SiteVars.RxFireWeight, rxTotalWeight);
            List<ActiveSite> shuffledAccidentalFireSites = Shuffle(activeAccidentalSites, SiteVars.AccidentalFireWeight, accidentalTotalWeight);

            modelCore.UI.WriteLine("   Next, loop through each day to start fires...");

            int numRxFires = Parameters.RxNumberAnnualFires;
            for (int day = 0; day < DaysPerYear; ++day)
            {
                //double ecoregionNumSites = 0.0;
                //double ecoregionAverageFireWeatherIndex = 0.0;
                //double ecoregionAverageTemperature = 0.0;
                //double ecoregionAverageRelativeHumidity = 0.0;
                double landscapeAverageFireWeatherIndex = 0.0;
                double landscapeAverageTemperature = 0.0;
                double landscapeAverageRelHumidity = 0.0;
                // number of fires get initilized to 0 every timestep

                foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                {
                    if (sitesPerEcoregions.ContainsKey(ecoregion.Index))
                    {
                        //if (sitesPerEcoregions.ContainsKey(ecoregion.Index))
                        double ecoregionFractionSites = (double) fractionSitesPerEcoregions[ecoregion.Index];

                        try
                        {
                            weatherData = Climate.Future_DailyData[ActualYear][ecoregion.Index];
                        }
                        catch
                        {
                            throw new UninitializedClimateData(string.Format("Climate data could not be found in Run(). Year: {0} in ecoregion: {1}", ActualYear, ecoregion.Name));
                        }

                        try
                        {
                            landscapeAverageFireWeatherIndex += weatherData.DailyFireWeatherIndex[day] * ecoregionFractionSites;
                            landscapeAverageTemperature += weatherData.DailyMaxTemp[day] * ecoregionFractionSites;
                            landscapeAverageRelHumidity += weatherData.DailyMinRH[day] * ecoregionFractionSites;
                            //ecoregionAverageFireWeatherIndex += weatherData.DailyFireWeatherIndex[day] * ecoregionNumSites;
                            //ecoregionAverageTemperature += weatherData.DailyMaxTemp[day] * ecoregionNumSites;
                            //ecoregionAverageRelativeHumidity += weatherData.DailyMinRH[day] * ecoregionNumSites;
                        }
                        catch
                        {
                            throw new UninitializedClimateData(string.Format("Fire Weather Index could not be found in Run(). Year: {0}, day: {1}, ecoregion: {2}, NumSites={3}", ActualYear, day, ecoregion.Name, sitesPerEcoregions[ecoregion.Index]));
                        }
                    }
                }

                //double landscapeAverageFireWeatherIndex = ecoregionAverageFireWeatherIndex / (double) modelCore.Landscape.ActiveSiteCount;
                //double landscapeAverageTemperature = ecoregionAverageTemperature / (double)modelCore.Landscape.ActiveSiteCount;
                //double landscapeAverageRelHumidity = ecoregionAverageRelativeHumidity / (double)modelCore.Landscape.ActiveSiteCount;

                //modelCore.UI.WriteLine("   Processing landscape for Fire events.  Day={0}, FWI={1}", day, landscapeAverageFireWeatherIndex);

                //if (landscapeAverageFireWeatherIndex > Parameters.RxMinFireWeatherIndex || landscapeAverageFireWeatherIndex >= 10.0)
                //{

                    // Ignite Accidental Fires. FWI must be > .10 
                    //PlugIn.ModelCore.UI.WriteLine("   Generating accidental fires...");
                    if (shuffledAccidentalFireSites.Count > 0 && landscapeAverageFireWeatherIndex >= 10.0)
                    {
                        int numLFires = NumberOfIgnitions(Ignition.Accidental, landscapeAverageFireWeatherIndex);
                        for (int i = 0; i < numLFires; ++i)
                        {
                            Ignite(Ignition.Accidental, shuffledAccidentalFireSites, day, landscapeAverageFireWeatherIndex);
                            LogIgnition(ModelCore.CurrentTime, landscapeAverageFireWeatherIndex, Ignition.Accidental.ToString(), numLFires, day);
                        }
                    }

                    // Ignite Lightning Fires FWI must be > .10 
                    //PlugIn.ModelCore.UI.WriteLine("   Generating lightning fires...");
                    if (shuffledLightningFireSites.Count > 0 && landscapeAverageFireWeatherIndex >= 10.0)
                    {
                        int numAFires = NumberOfIgnitions(Ignition.Lightning, landscapeAverageFireWeatherIndex);
                        for (int i = 0; i < numAFires; ++i)
                        {
                            Ignite(Ignition.Lightning, shuffledLightningFireSites, day, landscapeAverageFireWeatherIndex);
                            LogIgnition(ModelCore.CurrentTime, landscapeAverageFireWeatherIndex, Ignition.Lightning.ToString(), numAFires, day);
                        }
                    }

                    // Ignite a single Rx fire per day
                    //PlugIn.ModelCore.UI.WriteLine("   Generating prescribed fire...");
                    if (shuffledRxFireSites.Count > 0 &&
                        numRxFires > 0 &&
                        landscapeAverageFireWeatherIndex > Parameters.RxMinFireWeatherIndex &&
                        landscapeAverageFireWeatherIndex < Parameters.RxMaxFireWeatherIndex &&
                        landscapeAverageTemperature < Parameters.RxMaxTemperature &&
                        landscapeAverageRelHumidity > Parameters.RxMinRelativeHumidity &&
                        weatherData.DailyWindSpeed[day] < Parameters.RxMaxWindSpeed &&
                        day >= Parameters.RxFirstDayFire &&
                        day < Parameters.RxLastDayFire)
                    {
                        for (int i = 0; i < Parameters.RxNumberDailyFires; ++i)
                        {
                            Ignite(Ignition.Rx, shuffledRxFireSites, day, landscapeAverageFireWeatherIndex);
                            LogIgnition(ModelCore.CurrentTime, landscapeAverageFireWeatherIndex, Ignition.Rx.ToString(), Parameters.RxNumberDailyFires, day);
                            numRxFires--;
                        }
                    }
                }
            //}

            modelCore.UI.WriteLine("   Done processing days.  Next, write maps and summary log files. ...");

            WriteMaps(PlugIn.ModelCore.CurrentTime);

            WriteSummaryLog(modelCore.CurrentTime);

            if (isDebugEnabled)
                modelCore.UI.WriteLine("Done running extension");
        }


        //---------------------------------------------------------------------

        private void WriteMaps(int currentTime)
        {
            string[] paths = { "scrapple-fire", "special-dead-wood-{timestep}.img" };
            //string path = MapNames.ReplaceTemplateVars("scrapple-fire/special-dead-wood-{timestep}.img", currentTime);
            string path = MapNames.ReplaceTemplateVars(Path.Combine(paths), currentTime);

            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site] && SiteVars.Intensity[site] > 0)
                        {
                            pixel.MapCode.Value = (int)(SiteVars.SpecialDeadWood[site]);
                        }
                        else
                            pixel.MapCode.Value = 0;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }
            string[] paths2 = { "scrapple-fire", "ignition-type-{timestep}.img" };
            //path = MapNames.ReplaceTemplateVars("scrapple-fire/ignition-type-{timestep}.img", currentTime);
            path = MapNames.ReplaceTemplateVars(Path.Combine(paths2), currentTime);


            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site] && SiteVars.Intensity[site] > 0)
                        {
                            pixel.MapCode.Value = (SiteVars.TypeOfIginition[site] + 2);
                        }
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            string[] paths3 = { "scrapple-fire", "fire-intensity-{timestep}.img" };
            path = MapNames.ReplaceTemplateVars(Path.Combine(paths3), currentTime);
            //path = MapNames.ReplaceTemplateVars("scrapple-fire/fire-intensity-{timestep}.img", currentTime);
            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site] && SiteVars.Intensity[site] > 0)
                            pixel.MapCode.Value = (int) (SiteVars.Intensity[site] + 1);
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            string[] paths4 = { "scrapple-fire", "fire-spread-probability-{timestep}.img" };
            path = MapNames.ReplaceTemplateVars(Path.Combine(paths4), currentTime);
            //path = MapNames.ReplaceTemplateVars("scrapple-fire/fire-spread-probability-{timestep}.img", currentTime);
            using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path, modelCore.Landscape.Dimensions))
            {
                ShortPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site] && SiteVars.Intensity[site] > 0)
                            pixel.MapCode.Value = (short)(SiteVars.SpreadProbability[site] * 100);
                        else
                            pixel.MapCode.Value = 0;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            string[] paths5 = { "scrapple-fire", "day-of-fire-{timestep}.img" };
            path = MapNames.ReplaceTemplateVars(Path.Combine(paths5), currentTime);
            //path = MapNames.ReplaceTemplateVars("scrapple-fire/day-of-fire-{timestep}.img", currentTime);
            using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path, modelCore.Landscape.Dimensions))
            {
                ShortPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site] && SiteVars.Intensity[site] > 0)
                            pixel.MapCode.Value = (short) (SiteVars.DayOfFire[site] + 1);
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            string[] paths6 = { "scrapple-fire", "smolder-consumption-{timestep}.img" };
            path = MapNames.ReplaceTemplateVars(Path.Combine(paths6), currentTime);
            //path = MapNames.ReplaceTemplateVars("scrapple-fire/smolder-consumption-{timestep}.img", currentTime);
            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site] && SiteVars.Intensity[site] > 0)
                            pixel.MapCode.Value = SiteVars.SmolderConsumption[site];
                        else
                            pixel.MapCode.Value = 0;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            string[] paths7 = { "scrapple-fire", "flaming-consumptions-{timestep}.img" };
            path = MapNames.ReplaceTemplateVars(Path.Combine(paths7), currentTime);
            //path = MapNames.ReplaceTemplateVars("scrapple-fire/flaming-consumption-{timestep}.img", currentTime);
            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site] && SiteVars.Intensity[site] > 0)
                            pixel.MapCode.Value = SiteVars.FlamingConsumption[site];
                        else
                            pixel.MapCode.Value = 0;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            string[] paths8 = { "scrapple-fire", "event-ID-{timestep}.img" };
            path = MapNames.ReplaceTemplateVars(Path.Combine(paths8), currentTime);
            //path = MapNames.ReplaceTemplateVars("scrapple-fire/event-ID-{timestep}.img", currentTime);
            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site] && SiteVars.Intensity[site] > 0)
                            pixel.MapCode.Value = SiteVars.EventID[site];
                        else
                            pixel.MapCode.Value = 0;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }
        }


        //---------------------------------------------------------------------

        //  Determines the number of Ignitions per day
        //  Returns: 0 <= numIgnitons <= 3
        private static int NumberOfIgnitions(Ignition ignitionType, double fireWeatherIndex)
        {
            double b0 = 0.0;
            double b1 = 0.0;
            if(ignitionType == Ignition.Lightning)
            {
                b0 = Parameters.LightningIgnitionB0;
                b1 = Parameters.LightningIgnitionB1;
            }
            //if (ignitionType == Ignition.Rx)
            //{
            //    b0 = parameters.RxFireIgnitionB0;
            //    b1 = parameters.RxFireIgnitionB1;
            //}
            if (ignitionType == Ignition.Accidental)
            {
                b0 = Parameters.AccidentalFireIgnitionB0;
                b1 = Parameters.AccidentalFireIgnitionB1;
            }

            int numIgnitions = 0;
            double possibleIgnitions = Math.Pow(Math.E, (b0 + (b1* fireWeatherIndex)));
            int floorPossibleIginitions = (int)Math.Floor(possibleIgnitions);
            numIgnitions +=  floorPossibleIginitions;
            numIgnitions += (modelCore.GenerateUniform() <= (possibleIgnitions - (double)floorPossibleIginitions) ? 1 : 0);
            return numIgnitions;
        }

        //---------------------------------------------------------------------

        // Ignites and Spreads a fire
        private static void Ignite(Ignition ignitionType, List<ActiveSite> shuffledFireSites, int day, double fireWeatherIndex)
        {
            while (shuffledFireSites.Count() > 0 && SiteVars.Disturbed[shuffledFireSites.First()] == true)
            {
                shuffledFireSites.Remove(shuffledFireSites.First());
            }
            if (shuffledFireSites.Count() > 0)
            {
                FireEvent fireEvent = FireEvent.Initiate(shuffledFireSites.First(), modelCore.CurrentTime, day, ignitionType);

                totalBurnedSites[(int) ignitionType] += fireEvent.TotalSitesBurned;
                numberOfFire[(int)ignitionType]++;
                totalBiomassMortality[(int)ignitionType] += (int)fireEvent.TotalBiomassMortality;
                numCellsSeverity1 += fireEvent.NumberCellsSeverity1;
                numCellsSeverity2 += fireEvent.NumberCellsSeverity2;
                numCellsSeverity3 += fireEvent.NumberCellsSeverity3;
            }
        }

        //---------------------------------------------------------------------

        // Determines if an Rx Fire will start on the given day
        // RMS TODO
        private bool AllowRxFire(int day, double fireWeatherIndex)
        {
            return false;
        }

        //---------------------------------------------------------------------

        public static void LogIgnition(int currentTime, double fwi, string type, int numIgns, int doy)
        {

            ignitionsLog.Clear();
            IgnitionsLog ign = new IgnitionsLog();
            ign.SimulationYear = currentTime;
            ign.AttemptedNumberIgnitions = numIgns;
            ign.DayOfYear = doy;
            ign.FireWeatherIndex = fwi;
            ign.IgnitionType = type;

            ignitionsLog.AddObject(ign);
            ignitionsLog.WriteToFile();

        }


        //---------------------------------------------------------------------
        // A helper function to shuffle a list of ActiveSties: Algorithm may be improved.
        // Sites are weighted for ignition in the Shuffle method, based on the respective inputs maps.



        private static List<ActiveSite> PreShuffle(ISiteVar<double> weightedSiteVar, out double totalWeight)
        {
            List<ActiveSite> list = new List<ActiveSite>(); ;
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape.ActiveSites)
                if (weightedSiteVar[site] > 0.0)
                    list.Add(site);

            //List<ActiveSite> shuffledList = new List<ActiveSite>();
            totalWeight = 0.0;
            foreach (ActiveSite site in list)
            {
                totalWeight += weightedSiteVar[site];
            }

            return list;
        }


        private static List<ActiveSite> Shuffle(List<ActiveSite> list, ISiteVar<double> weightedSiteVar, double totalWeight)
        {
            //List<ActiveSite> list = new List<ActiveSite>(); ;
            //foreach (ActiveSite site in PlugIn.ModelCore.Landscape.ActiveSites)
            //    if (weightedSiteVar[site] > 0.0)
            //        list.Add(site);

            //double totalWeight = 0;
            //foreach (ActiveSite site in list)
            //{
            //    totalWeight += weightedSiteVar[site];
            //}

            List<ActiveSite> shuffledList = new List<ActiveSite>();
            while (list.Count > 0)
            {
                ActiveSite toAdd;
                toAdd = SelectRandomSite(list, weightedSiteVar, totalWeight);
                shuffledList.Add(toAdd);
                totalWeight -= weightedSiteVar[toAdd];
                list.Remove(toAdd);
            }

            return shuffledList;
        }

        //---------------------------------------------------------------------
        // The random selection based on input map weights
        public static ActiveSite SelectRandomSite(List<ActiveSite> list, ISiteVar<double> weightedSiteVar, double totalWeight)
        {
            ActiveSite selectedSite = list.FirstOrDefault(); // currently selected element

            int randomNum = FireEvent.rnd.Next((int)totalWeight);
            while (randomNum > totalWeight)
            {
                randomNum = FireEvent.rnd.Next(list.Count);
            }

            //check to make sure it is 
            foreach (ActiveSite site in list)
            {
                if (randomNum < weightedSiteVar[site])
                {
                    selectedSite = site;
                    break;
                }

                randomNum -= (int)weightedSiteVar[site];
            }

            return selectedSite; // when iterations end, selected is some element of sequence. 
        }
        //---------------------------------------------------------------------

        private void WriteSummaryLog(int currentTime)
        {
            summaryLog.Clear();
            SummaryLog sl = new SummaryLog();
            sl.SimulationYear = currentTime;
            sl.TotalBurnedSitesAccidental = totalBurnedSites[0];
            sl.TotalBurnedSitesLightning = totalBurnedSites[1];
            sl.TotalBurnedSitesRx = totalBurnedSites[2];
            sl.NumberFiresAccidental = numberOfFire[0];
            sl.NumberFiresLightning = numberOfFire[1];
            sl.NumberFiresRx = numberOfFire[2];
            sl.TotalBiomassMortalityAccidental = totalBiomassMortality[0];
            sl.TotalBiomassMortalityLightning = totalBiomassMortality[1];
            sl.TotalBiomassMortalityRx = totalBiomassMortality[2];
            sl.NumberCellsSeverity1 = numCellsSeverity1;
            sl.NumberCellsSeverity2 = numCellsSeverity2;
            sl.NumberCellsSeverity3 = numCellsSeverity3;


            summaryLog.AddObject(sl);
            summaryLog.WriteToFile();
        }

        /*
         * VS:: THIS MAY BE NEEDED
         * 
        private static List<ActiveSite> Sort<T>(List<ActiveSite> list, ISiteVar<T> type)
        {
            List<ActiveSite> sortedList = new List<ActiveSite>();
            foreach(ActiveSite site in list)
            {
                type[site] 
            }
        }
        */

    }
}
