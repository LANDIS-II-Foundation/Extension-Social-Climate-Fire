//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

//// NECN will take in carbon dead to calculate FineFuels

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Library.Climate;
using Landis.Core;
using Landis.Library.Metadata;

using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

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
        
        public static int FutureClimateBaseYear;
        public static int WeatherRandomizer = 0;
        private static double RelativeHumiditySlopeAdjust;
        private static int springStart;
        private static int winterStart;
        private int duration;
        private string climateConfigFile;
        

        private string mapNameTemplate;
        private double severityCalibrate;
        private static IInputParameters parameters;
        private static ICore modelCore;

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
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            Timestep = 1;  // RMS:  Initially we will force annual time step. parameters.Timestep;
            RelativeHumiditySlopeAdjust = parameters.RelativeHumiditySlopeAdjustment;
            mapNameTemplate = parameters.MapNamesTemplate;
            climateConfigFile = parameters.ClimateConfigFile;
            //severityCalibrate = parameters.SeverityCalibrate;
            duration = parameters.Duration;
            springStart = parameters.SpringStart;
            winterStart = parameters.WinterStart;

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
            modelCore.UI.WriteLine("   Initializing Fire Region Maps...");
            FireRegions.Initilize(parameters.LighteningFireMap, parameters.AccidentalFireMap, parameters.RxFireMap);
            MetadataHandler.InitializeMetadata(parameters.Timestep, parameters.MapNamesTemplate, ModelCore);

            modelCore.UI.WriteLine("   Initializing Fire Events...");
            //FireEvent.Initialize(parameters.FireDamages);

            if (isDebugEnabled)
                modelCore.UI.WriteLine("Initialization done");
        }

        


        //---------------------------------------------------------------------

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {
            SiteVars.InitializeFuelType();
            SiteVars.FireEvent.SiteValues = null;
            SiteVars.Disturbed.ActiveSiteValues = false;
            List<FireEvent> fireEvents = new List<FireEvent>();
            AnnualClimate_Daily weatherData = null;



            modelCore.UI.WriteLine("   Processing landscape for Fire events ...");

            // RMS:  foreach day-of-year loop
            // {

            // Estimate number of successful ignitions per day based on FWI using algorithm from AK
            // double FWI = AnnualFireWeather.FireWeatherIndex
            // Add minimum:  If < 0.10, skip it.
            // if (AnnualFireWeather.FireWeatherIndex < 10)
            // skip this day

            // double numFires = fancy equation from AK
            // if numFires > 1, then that's the number of fires to start (numFiresStarted)
            // if numFires < 1, then:
            // if (modelCore.GenerateUniform() <= numFires)
            // numFiresStarted = 1;

            // Next create a FireEvent and burn baby burn!
            //}

            int actualYear = 0;
            try
            {
                actualYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_AllData.First().Key;
            }
            catch
            {
                throw new UninitializedClimateData("Could not initilize the actual year from climate data");
            }
            
           
            int daysPerYear = 366;
            // VS: hasn't been properly integrated into Climate Library.
            //int daysPerYear = (AnnualClimate.IsLeapYear(actualYear) ? true : false) ? 366 : 365;

            // number of fires get initilized to 0 every timestep
            int numFires = 0;
            double fireWeatherIndex = 0.0;
            
            // Get the active sites from the landscape and shuffle them 
            List<ActiveSite> activeSites = PlugIn.ModelCore.Landscape.ToList();
            List<ActiveSite> shuffledAccidentalFireSites = Shuffle(activeSites, SiteVars.AccidentalFireWeight);
            
            activeSites = PlugIn.ModelCore.Landscape.ToList();
            List<ActiveSite> shuffledLightningFireSites = Shuffle(activeSites, SiteVars.LightningFireWeight);
            
            activeSites = PlugIn.ModelCore.Landscape.ToList();
            List<ActiveSite> shuffledRxFireSites = Shuffle(activeSites, SiteVars.RxFireWeight);

            foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                try
                {
                    weatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
                }
                catch
                {
                    throw new UninitializedClimateData(string.Format("Climate data could not be found \t year: {0} in ecoregion: {1}", actualYear, ecoregion.Name));
                }
                for (int day = 0; day < daysPerYear; ++day)
                {
                    try
                    {
                        fireWeatherIndex = weatherData.DailyFireWeatherIndex[day];
                    }
                    catch
                    {
                        throw new UninitializedClimateData(string.Format("Fire Weather Index could not be found \t year: {0}, day: {1} in ecoregion: {2} not found", actualYear, day, ecoregion.Name));
                    }
                    // FWI must be > .10 
                    if (fireWeatherIndex >= .10)
                    {
                        numFires = Ignitions(fireWeatherIndex);
                        // Ignite Accidental Fires
                        for (int i = 0; i < numFires; ++i)
                        {
                            Ignite(Ignition.Accidental, shuffledAccidentalFireSites, day, fireWeatherIndex);
                        }
                        // Ignite Lightning Fires
                        for (int i = 0; i < numFires; ++i)
                        {
                            Ignite(Ignition.Lightning, shuffledLightningFireSites, day, fireWeatherIndex);
                        }
                    }
                    if ( AllowRxFire(day, fireWeatherIndex) )
                    {
                        Ignite(Ignition.Rx, shuffledRxFireSites, day, fireWeatherIndex);
                    }
                }
            }
            // VS: not sure why i created this.. Useful? what is it to do?
            //foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            //{
                
            //}

            WriteYearlyIgnitionsMap(PlugIn.ModelCore.CurrentTime);

            WriteSummaryLog(modelCore.CurrentTime);

            if (isDebugEnabled)
                modelCore.UI.WriteLine("Done running extension");
        }


        //---------------------------------------------------------------------

        private void WriteYearlyIgnitionsMap(int currentTime)
        {
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, currentTime);

            using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path, modelCore.Landscape.Dimensions))
            {
                ShortPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        if (SiteVars.Disturbed[site])
                            pixel.MapCode.Value = (short)(SiteVars.TypeOfIginition[site]);
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

        public static void LogEvent(int currentTime, FireEvent fireEvent)
        {

            eventLog.Clear();
            EventsLog el = new EventsLog();
            el.Time = currentTime;
            el.InitRow = fireEvent.StartLocation.Row;
            el.InitColumn = fireEvent.StartLocation.Column;
            el.WindSpeed = fireEvent.WindSpeed;
            el.WindDirection = fireEvent.WindDirection;
            el.FireWeatherIndex = fireEvent.FireWeatherIndex;
            el.CohortsKilled = fireEvent.CohortsKilled;
            el.MeanSeverity = fireEvent.EventSeverity;


            eventLog.AddObject(el);
            eventLog.WriteToFile();

        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int currentTime)
        {
            //foreach (IDynamicInputRecord fire_region in FireRegions.Dataset)
            //{
                summaryLog.Clear();
                SummaryLog sl = new SummaryLog();
                sl.Time = currentTime;
                //sl.FireRegion = fire_region.Name;
                //sl.TotalBurnedSites = summaryFireRegionSiteCount[fire_region.Index];
                //sl.NumberFires = summaryFireRegionEventCount[fire_region.Index];

                summaryLog.AddObject(sl);
                summaryLog.WriteToFile();

            //}
        }

        //---------------------------------------------------------------------

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

        // A helper function to shuffle a list of ActiveSties: Algorithm may be improved.
        private static List<ActiveSite> Shuffle(List<ActiveSite> list, ISiteVar<double> weightedSiteVar)
        {
            List<ActiveSite> shuffledList = new List<ActiveSite>();
            double totalWeight = 0;
            foreach(ActiveSite site in list)
            {
                totalWeight += weightedSiteVar[site];
            }
            
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
                if (randomNum < weightedSiteVar[site] )
                {
                    selectedSite = site;
                    break;
                }

                randomNum -= (int)weightedSiteVar[site];
            }

            return selectedSite; // when iterations end, selected is some element of sequence. 
        }

        //---------------------------------------------------------------------

        //  Determines the number of Ignitions per day
        //  Returns: 0 <= numIgnitons <= 3
        private static int Ignitions(double fireWeatherIndex)
        {
            int numIgnitions = 0;
            double possibleIgnitions = (fireWeatherIndex * fireWeatherIndex) / 500;
            if (possibleIgnitions >= 3.0)
            {
                numIgnitions = 3;
            }
            else
            {
                int floorPossibleIginitions = (int)Math.Floor(possibleIgnitions);
                numIgnitions +=  floorPossibleIginitions;
                numIgnitions += (modelCore.GenerateUniform() >= (possibleIgnitions - (double)floorPossibleIginitions) ? 1 : 0);
            }
            return numIgnitions;
        }

        //---------------------------------------------------------------------

        // Detemines the number of cells a given ignition can spread to
        // Windspeed, wind direction, fireweatherindex, fuels
        private static int SpreadLength(double fireWeatherIndex)
        {
            return 5;
        }

        //---------------------------------------------------------------------

        // Ignites and Spreads a fire
        private static void Ignite(Ignition ignitionType, List<ActiveSite> shuffledFireSites, int day, double fireWeatherIndex)
        {

            
            while ( shuffledFireSites.Count() > 0 && SiteVars.Disturbed[shuffledFireSites.First()] == true )
            {
                shuffledFireSites.Remove(shuffledFireSites.First());
            }
            if (shuffledFireSites.Count() > 0)
            {
                FireEvent fireEvent = FireEvent.Initiate(shuffledFireSites.First(), modelCore.CurrentTime, day, ignitionType, SpreadLength(fireWeatherIndex));
                LogEvent(modelCore.CurrentTime, fireEvent);
                fireEvent.Spread(modelCore.CurrentTime, day);
            }
        }

        //---------------------------------------------------------------------

        // Determines if an Rx Fire will start on the given day
        private bool AllowRxFire(int day, double fireWeatherIndex)
        {
            return false;
        }

    }
}
