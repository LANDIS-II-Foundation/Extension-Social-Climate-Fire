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

        public static void InitializeMetadata(int Timestep, string MapFileName, ICore mCore)
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

             PlugIn.eventLog = new MetadataTable<EventsLog>("dynamic-fire-events-log.csv");

            OutputMetadata tblOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "DynamicFireLog",
                FilePath = PlugIn.eventLog.FilePath,
                Visualize = false,
            };
            tblOut_events.RetriveFields(typeof(EventsLog));
            Extension.OutputMetadatas.Add(tblOut_events);

            PlugIn.summaryLog = new MetadataTable<SummaryLog>("dynamic-fire-summary-log.csv");

            OutputMetadata tblSummaryOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "DynamicFireSummaryLog",
                FilePath = PlugIn.summaryLog.FilePath,
                Visualize = false,
            };
            tblSummaryOut_events.RetriveFields(typeof(SummaryLog));
            Extension.OutputMetadatas.Add(tblSummaryOut_events);

            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            OutputMetadata mapOut_Severity = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "Severity",
                FilePath = @MapFileName,
                Map_DataType = MapDataType.Ordinal,
                Map_Unit = FieldUnits.Severity_Rank,
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_Severity);

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
