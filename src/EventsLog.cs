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

        //[DataFieldAttribute(Desc = "Initiation Fuel")]
        //public int InitFuel { set; get; }

        [DataFieldAttribute(Desc = "Initiation Percent Conifer")]
        public double InitPercentConifer { set; get; }

        //[DataFieldAttribute(Desc = "Size or Duration")]
        //public double SizeOrDuration { set; get; }

        //[DataFieldAttribute(Desc = "Size Bin")]
        //public double SizeBin { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.minutes, Desc = "Duration")]
        //public double Duration { set; get; }

        //[DataFieldAttribute(Desc = "Fire Season")]
        //public string FireSeason { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.m_second, Desc = "Wind Speed")]
        public double WindSpeed { set; get; }

        [DataFieldAttribute(Desc = "Wind Direction")]
        public double WindDirection { set; get; }

        //[DataFieldAttribute(Desc = "Fine Fuel Moisture Code")]
        //public double FFMC { set; get; }

        //[DataFieldAttribute(Desc = "Build Up Index")]
        //public double BUI { set; get; }

        //[DataFieldAttribute(Desc = "Percent Curing")]
        //public double PercentCuring { set; get; }

        //[DataFieldAttribute(Desc = "Initial Spread Index")]
        //public double ISI { set; get; }

        [DataFieldAttribute(Desc = "Fire Weather Index")]
        public double FWI { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Number of Sites in Event")]
        public int TotalSites { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Damaged Sites in Event")]
        //public int DamagedSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cohorts Killed")]
        public int CohortsKilled { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Severity_Rank, Desc = "Mean Severity (1-5)", Format="0.00")]
        public double MeanSeverity { set; get; }

    }
}
