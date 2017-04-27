//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller

using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.SpatialModeling;

namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// Values for each fire region.
    /// </summary>
    public interface IDynamicInputRecord
    {

        //double ProbEst{get;set;}
        //int ANPP_MAX_Spp {get;set;}
        //int B_MAX_Spp {get;set;}

        int Index { get; set; }
        string Name { get; set; }
        int MapCode { get; set; }
        double MeanSize { get; set; }
        double StandardDeviation { get; set; }
        double MinSize { get; set; }
        double MaxSize { get; set; }
        int SpringFMCLo { get; set; }
        int SpringFMCHi { get; set; }
        double SpringFMCHiProp { get; set; }
        int SummerFMCLo { get; set; }
        int SummerFMCHi { get; set; }
        double SummerFMCHiProp { get; set; }
        int FallFMCLo { get; set; }
        int FallFMCHi { get; set; }
        double FallFMCHiProp { get; set; }
        int OpenFuelType { get; set; }
        double EcoIgnitionNum { get; set; }
        int FallRecords { get; set; }
        int SpringRecords { get; set; }
        int SummerRecords { get; set; }
        List<Location> FireRegionSites { get; set; }
    }

    public class DynamicInputRecord
    : IDynamicInputRecord
    {

        //private double probEst;
        //private int anpp_max_spp;
        //private int b_max_spp;

        private int index;
        private string name;
        private int mapCode;
        private double meanSize;
        private double standardDeviation;
        private double minSize;
        private double maxSize;
        private int springFMCLo;
        private int springFMCHi;
        private double springFMCHiProp;
        private int summerFMCLo;
        private int summerFMCHi;
        private double summerFMCHiProp;
        private int fallFMCLo;
        private int fallFMCHi;
        private double fallFMCHiProp;
        private int openFuelType;
        private double ecoIgnitionNum;
        private int fallRecords;
        private int springRecords;
        private int summerRecords;
        private List<Location> fireRegionSites;

        //---------------------------------------------------------------------
        /// <summary>
        /// Index
        /// </summary>
        public int Index
        {
            get
            {
                return index;
            }
            set
            {

                index = value;
            }
        }

        //---------------------------------------------------------------------  
        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {

                name = value;
            }
        }

        //---------------------------------------------------------------------  
        /// <summary>
        /// MapCode
        /// </summary>
        public int MapCode
        {
            get
            {
                return mapCode;
            }
            set
            {

                mapCode = value;
            }
        }

        //---------------------------------------------------------------------        
        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        public double MeanSize
        {
            get
            {
                return meanSize;
            }
            set
            {

                meanSize = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Standard deviation event size (hectares).
        /// </summary>
        public double StandardDeviation
        {
            get
            {
                return standardDeviation;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                standardDeviation = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Minimum event size (hectares).
        /// </summary>
        public double MinSize
        {
            get
            {
                return minSize;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0.");
                minSize = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Maximum event size (hectares).
        /// </summary>
        public double MaxSize
        {
            get
            {
                return maxSize;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0.");
                maxSize = value;
            }
        }
        //---------------------------------------------------------------------
        public int SpringFMCLo
        {
            get
            {
                return springFMCLo;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                springFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        public int SpringFMCHi
        {
            get
            {
                return springFMCHi;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0.");
                springFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        public double SpringFMCHiProp
        {
            get
            {
                return springFMCHiProp;
            }
            set
            {
                if (value < 0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0 and < or = 1.0.");
                springFMCHiProp = value;
            }
        }
        //---------------------------------------------------------------------
        public int SummerFMCLo
        {
            get
            {
                return summerFMCLo;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0.");
                summerFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        public int SummerFMCHi
        {
            get
            {
                return summerFMCHi;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0.");
                summerFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        public double SummerFMCHiProp
        {
            get
            {
                return summerFMCHiProp;
            }
            set
            {
                if (value < 0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0 and < or = 1.0.");
                summerFMCHiProp = value;
            }
        }
        //---------------------------------------------------------------------
        public int FallFMCLo
        {
            get
            {
                return fallFMCLo;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0.");
                fallFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        public int FallFMCHi
        {
            get
            {
                return fallFMCHi;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0.");
                fallFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        public double FallFMCHiProp
        {
            get
            {
                return fallFMCHiProp;
            }
            set
            {
                if (value < 0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0 and < or = 1.0.");
                fallFMCHiProp = value;
            }
        }
        //---------------------------------------------------------------------
        public int OpenFuelType
        {
            get
            {
                return openFuelType;
            }
            set
            {
                openFuelType = value;
            }
        }
        //---------------------------------------------------------------------
        public double EcoIgnitionNum
        {
            get
            {
                return ecoIgnitionNum;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be = or > 0");
                ecoIgnitionNum = value;
            }
        }
        //---------------------------------------------------------------------
        public int FallRecords
        {
            get
            {
                return fallRecords;
            }
            set
            {
                fallRecords = value;
            }
        }
        //---------------------------------------------------------------------
        public int SpringRecords
        {
            get
            {
                return springRecords;
            }
            set
            {
                springRecords = value;
            }
        }
        //---------------------------------------------------------------------
        public int SummerRecords
        {
            get
            {
                return summerRecords;
            }
            set
            {
                summerRecords = value;
            }
        }

        //---------------------------------------------------------------------
        public List<Location> FireRegionSites
        {
            get
            {
                return fireRegionSites;
            }
            set {
                fireRegionSites = value;
            }
        }
        //---------------------------------------------------------------------
        //public DynamicInputRecord(int index)
       // {
       //    fireRegionSites =   new List<Location>();
       //     this.index = index;
        //}
        //---------------------------------------------------------------------


        public DynamicInputRecord()
        {
            fireRegionSites = new List<Location>();
            this.FireRegionSites = fireRegionSites;
        }

        public static IDynamicInputRecord Clone(IDynamicInputRecord oldRecord)
        {
            IDynamicInputRecord newRecord = new DynamicInputRecord();
            newRecord.EcoIgnitionNum = oldRecord.EcoIgnitionNum;
            newRecord.FallFMCHi = oldRecord.FallFMCHi;
            newRecord.FallFMCHiProp = oldRecord.FallFMCHiProp;
            newRecord.FallFMCLo = oldRecord.FallFMCLo;
            newRecord.FireRegionSites = oldRecord.FireRegionSites;
            newRecord.Index = oldRecord.Index;
            newRecord.MapCode = oldRecord.MapCode;
            newRecord.MaxSize = oldRecord.MaxSize;
            newRecord.MeanSize = oldRecord.MeanSize;
            newRecord.MinSize = oldRecord.MinSize;
            newRecord.Name = oldRecord.Name;
            newRecord.OpenFuelType = oldRecord.OpenFuelType;
            newRecord.SpringFMCHi = oldRecord.SpringFMCHi;
            newRecord.SpringFMCHiProp = oldRecord.SpringFMCHiProp;
            newRecord.SpringFMCLo = oldRecord.SpringFMCLo;
            newRecord.StandardDeviation = oldRecord.StandardDeviation;
            newRecord.SummerFMCHi = oldRecord.SummerFMCHi;
            newRecord.SummerFMCHiProp = oldRecord.SummerFMCHiProp;
            newRecord.SummerFMCLo = oldRecord.SummerFMCLo;            
            return newRecord;
        }


    }
}
