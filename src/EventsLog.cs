//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Scrapple
{
    public class EventsLog
    {

        [DataFieldAttribute(Desc = "EventIndex")]
        public int EventID { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Year")]
        public int SimulationYear {set; get;}

        [DataFieldAttribute(Desc = "Initiation Row")]
        public int InitRow { set; get; }

        [DataFieldAttribute(Desc = "Initiation Column")]
        public int InitColumn { set; get; }

        [DataFieldAttribute(Desc = "Fire Weather Index")]
        public double InitialFireWeatherIndex { set; get; }

        [DataFieldAttribute(Desc = "Day of Year")]
        public int InitialDayOfYear { set; get; }

        [DataFieldAttribute(Desc = "Maximum Spread Area")]
        public int MaximumSpreadArea { set; get; }

        [DataFieldAttribute(Desc = "Ignition Type")]
        public string IgnitionType { set; get; }

        [DataFieldAttribute(Desc = "Number of Days Burning")]
        public int NumberOfDays { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Number of Sites in Event")]
        public int TotalSitesBurned { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cohorts Killed")]
        public int CohortsKilled { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Available Cohorts")]
        public int AvailableCohorts { set; get; }

        [DataFieldAttribute(Desc = "Average Wind Speed")]
        public double MeanWindSpeed { set; get; }

        [DataFieldAttribute(Desc = "Average Effective Wind Speed")]
        public double MeanEffectiveWindSpeed { set; get; }

        [DataFieldAttribute(Desc = "Average Wind Direction")]
        public double MeanWindDirection { set; get; }

        [DataFieldAttribute(Desc = "Average Suppression Effectiveness")]
        public double MeanSuppressionEffectiveness { set; get; }

        [DataFieldAttribute(Desc = "Mean FWI", Format = "0.00")]
        public double MeanFWI { set; get; }

        [DataFieldAttribute(Desc = "Mean Spread Probability")]
        public double MeanSpreadProbability { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Severity_Rank, Desc = "Mean Severity (1-5)", Format = "0.00")]
        public double MeanDNBR { set; get; }

        [DataFieldAttribute(Desc = "Mean DNBR", Format = "0.0")]
        public double MeanSeverity { set; get; }
        [DataFieldAttribute(Desc = "Mortality Biomass")]
        public double TotalBiomassMortality { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cells Intensity 1")]
        //public double NumberCellsSeverity1 { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cells Intensity 2")]
        //public double NumberCellsSeverity2 { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cells Intensity 3")]
        //public double NumberCellsSeverity3 { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cells Intensity 4")]
        //public double NumberCellsSeverity4 { set; get; }
        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cells Intensity 5")]
        //public double NumberCellsSeverity5 { set; get; }
        //[DataFieldAttribute(Unit = FieldUnits.Percentage, Desc = "Percent of Cells Intensity Factor 1")]
        //public double PercentsCellsIntensityFactor1 { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Percentage, Desc = "Percent of Cells Intensity Factor 2")]
        //public double PercentsCellsIntensityFactor2 { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Percentage, Desc = "Percent of Cells Intensity Factor 3")]
        //public double PercentsCellsIntensityFactor3 { set; get; }
    }
}
