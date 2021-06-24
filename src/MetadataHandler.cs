//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Landis.Library.Metadata;
using Landis.Utilities;
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
            string[] paths = { @"social-climate-fire", "fire-intensity-{timestep}.img" };
            OutputMetadata mapOut_Intensity = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "Intensity",
                FilePath = Path.Combine(paths),
                Map_DataType = MapDataType.Ordinal,
                Map_Unit = FieldUnits.Severity_Rank,
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_Intensity);

            string[] paths2 = { @"social-climate-fire", "special-dead-wood-{timestep}.img" };
            OutputMetadata mapOut_SpecialDead = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "SpecialDeadWood",
                FilePath = Path.Combine(paths2),
                Map_DataType = MapDataType.Continuous,
                Map_Unit = FieldUnits.g_C_m2,
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_SpecialDead);

            string[] paths3 = { @"social-climate-fire", "ignitions-type-{timestep}.img" };
            OutputMetadata mapOut_IgType = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "IgnitionType",
                FilePath = Path.Combine(paths3),
                Map_DataType = MapDataType.Nominal,
                Map_Unit = FieldUnits.None,
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_IgType);

            string[] paths4 = { @"social-climate-fire", "fire-spread-probability-{timestep}.img" };
            OutputMetadata mapOut_fireSpread = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "FireSpreadProbability",
                FilePath = Path.Combine(paths4),
                Map_DataType = MapDataType.Continuous,
                Map_Unit = FieldUnits.None,
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_fireSpread);

            string[] paths5 = { @"social-climate-fire", "day-of-fire-{timestep}.img" };
            OutputMetadata mapOut_fireDay = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "DayOfFire",
                FilePath = Path.Combine(paths5),
                Map_DataType = MapDataType.Ordinal,
                Map_Unit = "Day of Year",
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_fireDay);

            string[] paths6 = { @"social-climate-fire", "event-ID-{timestep}.img" };
            OutputMetadata mapOut_eventID = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "EventID",
                FilePath = Path.Combine(paths6),
                Map_DataType = MapDataType.Nominal,
                Map_Unit = "Index",
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_eventID);

            string[] paths7 = { @"social-climate-fire", "fine-fuels-{timestep}.img" };
            OutputMetadata mapOut_fineFuels = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "FineFuels",
                FilePath = Path.Combine(paths7),
                Map_DataType = MapDataType.Continuous,
                Map_Unit = FieldUnits.g_C_m2,
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_fineFuels);

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);




        }
    }
}
