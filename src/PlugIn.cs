//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

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

            modelCore.UI.WriteLine("   Initializing Fire Climate Data...");
            FireRegions.Initilize(parameters.LighteningFireMap);
            
            Climate.Initialize(parameters.ClimateConfigFile, false, modelCore);
            FutureClimateBaseYear = Climate.Future_MonthlyData.Keys.Min();


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
            SiteVars.Severity.ActiveSiteValues = 0;
            SiteVars.Disturbed.ActiveSiteValues = false;
            //SiteVars.TravelTime.ActiveSiteValues = Double.PositiveInfinity;
            //SiteVars.MinNeighborTravelTime.ActiveSiteValues = Double.PositiveInfinity;
            //SiteVars.RateOfSpread.ActiveSiteValues = 0.0;
            List<FireEvent> fireEvents = new List<FireEvent>();

            
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
                throw new UninitializedClimateData(string.Format("No climate data for year:", actualYear));
            }
            
           
            int daysPerYear = 366;
            //daysPerYear = (AnnualClimate.IsLeapYear(actualYear) ? true : false) ? 366 : 365;

            // number of fires get initilized to 0 every timestep
            int numFiresStarted = 0;
            double numFires = 0.0;
            double fireWeatherIndex = 0.0;
            bool fireWeatherDataExists = true;
            
            // Get the active sites from the landscape and shuffle them according to 
            List<ActiveSite> activeSites = PlugIn.ModelCore.Landscape.ToList();
            List<ActiveSite> shuffledActiveSites = Shuffle(activeSites, SiteVars.AccidentalFireWeight);
            ActiveSite site = shuffledActiveSites.First();
            foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                AnnualClimate_Daily weatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
                // do this for each day of the year
                for (int day = 0; day < daysPerYear; ++day)
                {
                    // Check to make sure FireWeatherIndex is >= .10. If not skip day
                    try
                    {
                        fireWeatherIndex = weatherData.DailyFireWeatherIndex[day];
                    }
                    catch
                    {
                        fireWeatherDataExists = false;
                    }
                    if (fireWeatherIndex >= .10 && fireWeatherDataExists)
                    {
                        // check to make at least 1 ignition happend
                        numFires = Ignitions(fireWeatherIndex);

                        if (numFires >= 1)
                        {
                            numFiresStarted = (numFires > 3.0) ? 3 : (int)Math.Round(numFires);
                        }
                        else
                        {
                            numFiresStarted = (modelCore.GenerateUniform() >= numFires) ? 1 : 0;
                        }

                        // create a fire event for each fire
                        for (int i = 0; i < numFiresStarted; ++i)
                        {
                            // create fire Event. VS: How do i determine type? <== dealing with it at a later phase
                            FireEvent fireEvent = FireEvent.Initiate(shuffledActiveSites.First(), modelCore.CurrentTime, day);
                            
                            LogEvent(modelCore.CurrentTime, fireEvent);
                            shuffledActiveSites.Remove(shuffledActiveSites.First());
                            // shuffledLightningSites.Remove(shuffledActiveSites.First());
                        }
                    }
                }
           
            }

            WriteYearlyIgnitionsMap(PlugIn.ModelCore.CurrentTime);

            WriteSummaryLog(modelCore.CurrentTime);

            if (isDebugEnabled)
                modelCore.UI.WriteLine("Done running extension");
        }

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

        private void LogEvent(int currentTime, FireEvent fireEvent)
        {

            eventLog.Clear();
            EventsLog el = new EventsLog();
            el.Time = currentTime;
            el.InitRow = fireEvent.StartLocation.Row;
            el.InitColumn = fireEvent.StartLocation.Column;
            //el.InitFuel = fireEvent.InitiationFuel;
            el.InitPercentConifer = fireEvent.InitiationPercentConifer;
            //el.SizeOrDuration = fireEvent.MaxFireParameter;
            //el.SizeBin = fireEvent.SizeBin;
            //el.Duration = fireEvent.MaxDuration;
            el.WindSpeed = fireEvent.WindSpeed;
            el.WindDirection = fireEvent.WindDirection;
            el.TotalSites = fireEvent.NumSitesChecked;
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

        public static ActiveSite SelectRandomSite(List<ActiveSite> list, ISiteVar<double> weightedSiteVar, double totalWeight)
        {
            ActiveSite selectedSite = list.FirstOrDefault(); // currently selected element
            double randomNum = PlugIn.ModelCore.GenerateUniform();
            
            //check to make sure it is 
            foreach (ActiveSite site in list)
            {
                if (randomNum < weightedSiteVar[site] )
                {
                    selectedSite = site;
                    break;
                }

                randomNum -= weightedSiteVar[site];
            }

            return selectedSite; // when iterations end, selected is some element of sequence. 
        }

        private static double Ignitions(double fireWeatherIndex)
        {
            double numIgnitions = (fireWeatherIndex * fireWeatherIndex) / 500;
            return numIgnitions;
        }


    }
}
