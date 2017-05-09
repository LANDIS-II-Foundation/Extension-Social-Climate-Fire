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

namespace Landis.Extension.Scrapple
{


    ///<summary>
    /// A disturbance plug-in that simulates Fire disturbance.
    /// </summary>
    public class PlugIn
        : ExtensionMain 
    {
        private static readonly bool isDebugEnabled = false; 

        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:fire");
        public static readonly string ExtensionName = "SCRAPPLE";
        public static MetadataTable<EventsLog> eventLog;
        public static MetadataTable<SummaryLog> summaryLog;

        //public static bool ClimateLibraryActive = false;
        //public static bool ReadClimateLibrary = false;
        public static int FutureClimateBaseYear;
        //public static DataTable WeatherDataTable;
        //public static DataTable WindDataTable;
        public static int WeatherRandomizer = 0;
        //public static ISeasonParameters[] SeasonParameters;
        public static double RelativeHumiditySlopeAdjust;


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
            mapNameTemplate     = parameters.MapNamesTemplate;
            severityCalibrate   = parameters.SeverityCalibrate;

            MetadataHandler.InitializeMetadata(parameters.Timestep, parameters.MapNamesTemplate, ModelCore);

            modelCore.UI.WriteLine("   Initializing Fire Events...");
            FireEvent.Initialize(parameters.FireDamages);

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

            double fireWeatherIndex = 10.0; // this not be a static number. just now for testing (AnnualFireWeather.FireWeatherIndex)
            int firesStarted;

            // do this for each day of the year (365 days)
            for (int i = 0; i < 364; ++i)
            {
                // Check to make sure FireWeatherIndex is >= 10. If not skip day
                if (fireWeatherIndex >= 10)
                {
                    // check to make at least 1 ignition happend
                    firesStarted = Ignitions(fireWeatherIndex);

                    if (firesStarted >= 1)
                    {
                        for (int i = 0; i < firesStarted)
                        {
                            // create fire event
                        }
                    }
                    // No fires started
                    else
                    {
                        modelCore.GenerateUniform();
                    }
                }
            }


            // Track the time of last fire; registered in SiteVars.cs for other extensions to access.
            if (isDebugEnabled)
                modelCore.UI.WriteLine("Assigning TimeOfLastFire SiteVar ...");
            foreach (Site site in modelCore.Landscape.AllSites)
                if(SiteVars.Disturbed[site])
                    SiteVars.TimeOfLastFire[site] = modelCore.CurrentTime;

            // Output maps here.
            //  Write Fire severity map
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, modelCore.CurrentTime);
            modelCore.UI.WriteLine("   Writing Fire severity map to {0} ...", path);
            using (IOutputRaster<BytePixel> outputRaster = modelCore.CreateRaster<BytePixel>(path, modelCore.Landscape.Dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach (Site site in modelCore.Landscape.AllSites)
                {
                    if (site.IsActive) {
                        if (SiteVars.Disturbed[site])
                            pixel.MapCode.Value = (byte) (SiteVars.Severity[site] + 2);
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            WriteSummaryLog(modelCore.CurrentTime);

            if (isDebugEnabled)
                modelCore.UI.WriteLine("Done running extension");

        
        }


        //---------------------------------------------------------------------

        private void LogEvent(int   currentTime, FireEvent fireEvent)
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
            el.FWI = fireEvent.FireWeatherIndex;
            el.CohortsKilled = fireEvent.CohortsKilled;
            el.MeanSeverity = fireEvent.EventSeverity;


            eventLog.AddObject(el);
            eventLog.WriteToFile();

        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int   currentTime)
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

        // A helper function for randomly choosing which neighbor to spread to next.
        private static List<Location> Shuffle<Location>(List<Location> list)
        {
            List<Location> shuffledList = new List<Location>();

            int randomIndex = 0;
            while (list.Count > 0)
            {
                //randomIndex = modelCore.GenerateUniform(list.Count); //Choose a random object in the list
                randomIndex = (int) (list.Count * PlugIn.ModelCore.GenerateUniform());
                shuffledList.Add(list[randomIndex]); //add it to the new, random list
                list.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return shuffledList;
        }

        private static int Ignitions(double fireWeatherIndex)
        {
            return (fireWeatherIndex ^ 2) / 500;
        }


    }
}
