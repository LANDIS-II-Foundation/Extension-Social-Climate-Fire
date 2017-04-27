//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.Scrapple
{

    public enum SeasonName {Spring, Summer, Fall};
    public enum LeafOnOff  {LeafOn, LeafOff};

    public interface ISeasonParameters
    {
        SeasonName NameOfSeason{get;set;}
        LeafOnOff LeafStatus{get;set;}
        double FireProbability{get;set;}
        int PercentCuring{get;set;}
        double DayLengthProp{get;set;}
        int RecordCount {get; set;}
        int StartDay { get; set; }
        int EndDay { get; set; }
    }
}

namespace Landis.Extension.Scrapple
{
    public class SeasonParameters
    : ISeasonParameters
    {
        private SeasonName nameOfSeason;
        private LeafOnOff leafStatus;
        private double fireProbability;
        private int percentCuring;
        private double dayLengthProp;
        private int recordCount;
        private int startDay;
        private int endDay;

        
        //---------------------------------------------------------------------
        public SeasonName NameOfSeason
        {
            get {
                return nameOfSeason;
            }
            set {
                nameOfSeason = value;
            }
        }
        //---------------------------------------------------------------------
        public double FireProbability
        {
            get {
                return fireProbability;
            }
            set {
                    if (value < 0.0 || value > 1.0)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 1.0");
                fireProbability = value;
            }
        }
     
        //---------------------------------------------------------------------
        public int PercentCuring
        {
            get {
                return percentCuring;
            }
            set {
                    if (value < 0 || value > 100)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 100");
                percentCuring = value;
            }
        }
        //---------------------------------------------------------------------
        public double DayLengthProp
        {
            get
            {
                return dayLengthProp;
            }
            set
            {
                    if (value < 0 || value > 1)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0.0 and 1.0");
                dayLengthProp = value;
            }
        }
        //---------------------------------------------------------------------
        public int RecordCount
        {
            get
            {
                return recordCount;
            }
            //New
            set
            {
                recordCount = value;
            }
        }
        //---------------------------------------------------------------------
        public LeafOnOff LeafStatus
        {
            get {
                return leafStatus;
            }
            set {
                leafStatus = value;
            }
        }
        //---------------------------------------------------------------------
        public int StartDay
        {
            get
            {
                return startDay;
            }
            set
            {
                if (value < 1 || value > 365)
                    throw new InputValueException(value.ToString(),
                        "Value must be between 1 and 365");
                startDay = value;
            }
        }
        //---------------------------------------------------------------------
        public int EndDay
        {
            get
            {
                return endDay;
            }
            set
            {
                if (value < 1 || value > 365)
                    throw new InputValueException(value.ToString(),
                        "Value must be between 1 and 365");
                endDay = value;
            }
        }
        //---------------------------------------------------------------------
        public SeasonParameters()
        {
        }
        //---------------------------------------------------------------------


    }
}
