//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.Scrapple
{
    public static class MetadataHandler
    {
        
        public static ExtensionMetadata Extension {get; set;}

        public static void InitializeMetadata(int Timestep, ICore mCore)
        {
            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata() {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime
            };

            Extension = new ExtensionMetadata(mCore){
                Name = PlugIn.ExtensionName,
                TimeInterval = Timestep, 
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            PlugIn.ignitionsLog = new MetadataTable<IgnitionsLog>("scrapple-ignitions-log.csv");

            OutputMetadata tblOut_igns = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "ClimateFireIgnitionsLog",
                FilePath = PlugIn.ignitionsLog.FilePath,
                Visualize = false,
            };
            tblOut_igns.RetriveFields(typeof(IgnitionsLog));
            Extension.OutputMetadatas.Add(tblOut_igns);

            PlugIn.eventLog = new MetadataTable<EventsLog>("scrapple-events-log.csv");

            OutputMetadata tblOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "ClimateFireEventsLog",
                FilePath = PlugIn.eventLog.FilePath,
                Visualize = false,
            };
            tblOut_events.RetriveFields(typeof(EventsLog));
            Extension.OutputMetadatas.Add(tblOut_events);

            PlugIn.summaryLog = new MetadataTable<SummaryLog>("scrapple-summary-log.csv");

            OutputMetadata tblSummaryOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "ClimateFireSummaryLog",
                FilePath = PlugIn.summaryLog.FilePath,
                Visualize = false,
            };
            tblSummaryOut_events.RetriveFields(typeof(SummaryLog));
            Extension.OutputMetadatas.Add(tblSummaryOut_events);

            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------
            string intensityMapFileName = "scrapple-intensity.img";
            OutputMetadata mapOut_Intensity = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "Intensity",
                FilePath = @intensityMapFileName,
                Map_DataType = MapDataType.Ordinal,
                Map_Unit = FieldUnits.Severity_Rank,
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_Intensity);

            //string specialDeadWoodMapFileName = "scrapple-dead-wood.img";
            //OutputMetadata mapOut_SpecialDead = new OutputMetadata()
            //{
            //    Type = OutputType.Map,
            //    Name = "SpecialDeadWood",
            //    FilePath = @specialDeadWoodMapFileName,
            //    Map_DataType = MapDataType.Continuous,
            //    Map_Unit = FieldUnits.g_C_m2,
            //    Visualize = true,
            //};
            //Extension.OutputMetadatas.Add(mapOut_Intensity);
            //OutputMetadata mapOut_Time = new OutputMetadata()
            //{
            //    Type = OutputType.Map,
            //    Name = "TimeLastFire",
            //    FilePath = @TimeMapFileName,
            //    Map_DataType = MapDataType.Continuous,
            //    Map_Unit = FieldUnits.Year,
            //    Visualize = true,
            //};
            //Extension.OutputMetadatas.Add(mapOut_Time);
            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);




        }
    }
}
