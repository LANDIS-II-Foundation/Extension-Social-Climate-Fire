using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Scrapple
{
    public class EventsLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "...")]
        public int Time {set; get;}

        [DataFieldAttribute(Desc = "Initiation Row")]
        public int InitRow { set; get; }

        [DataFieldAttribute(Desc = "Initiation Column")]
        public int InitColumn { set; get; }

        [DataFieldAttribute(Desc = "Initiation Percent Conifer")]
        public double InitPercentConifer { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.m_second, Desc = "Wind Speed")]
        public double WindSpeed { set; get; }

        [DataFieldAttribute(Desc = "Wind Direction")]
        public double WindDirection { set; get; }
        
        [DataFieldAttribute(Desc = "Fire Weather Index")]
        public double FireWeatherIndex { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Number of Sites in Event")]
        public int TotalSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cohorts Killed")]
        public int CohortsKilled { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Severity_Rank, Desc = "Mean Severity (1-5)", Format="0.00")]
        public double MeanSeverity { set; get; }

    }
}
