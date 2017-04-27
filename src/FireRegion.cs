//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.DynamicFire
{
    /// <summary>
    /// The parameters for an ecoregion.
    /// </summary>
    public interface IFireRegion
    {
        string Name {get;set;}
        ushort MapCode {get;set;}
        int Index {get; set;}
        
        double MeanSize  {get;set;}
        double StandardDeviation  {get;set;}
        int MinSize { get; set; }
        int MaxSize {get;set;}
        int SpringFMCLo  {get;set;}
        int SpringFMCHi  {get;set;}
        double SpringFMCHiProp  {get;set;}
        int SummerFMCLo  {get;set;}
        int SummerFMCHi {get;set;}
        double SummerFMCHiProp {get;set;}
        int FallFMCLo {get;set;}
        int FallFMCHi {get;set;}
        double FallFMCHiProp {get;set;}
        int OpenFuelType {get;set;}
        double EcoIgnitionNum {get;set;}
        int FallRecords { get;set;}
        int SpringRecords { get;set;}
        int SummerRecords { get;set;}
        List<Location> FireRegionSites {get;}

    }
}

namespace Landis.Extension.DynamicFire
{
    public class FireRegion
        : IFireRegion
    {
        private string name;
        private ushort mapCode;
        private int index;
        
        private double meanSize;
        private double standardDeviation;
        private int minSize;
        private int maxSize;
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
        
        public int Index
        {
            get {
                return index;
            }
            set {
                index = value;
            }
        }

        //---------------------------------------------------------------------

        public string Name
        {
            get {
                return name;
            }
            set {
                //if (value != null) {
                    if (value.Trim() == "")
                        throw new InputValueException(value, "Missing name");
                //}
                name = value;
            }
        }


        //---------------------------------------------------------------------

        public ushort MapCode
        {
            get {
                return mapCode;
            }
            set {
                mapCode = value;
            }
        }


        //---------------------------------------------------------------------
        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        public double MeanSize
        {
            get {
                return meanSize;
            }
            set {

                meanSize = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Standard deviation event size (hectares).
        /// </summary>
        public double StandardDeviation
        {
            get {
                return standardDeviation;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),"Value must be = or > 0.");
                standardDeviation = value;
            }
        }
        
        //---------------------------------------------------------------------
        /// <summary>
        /// Minimum event size (hectares).
        /// </summary>
        public int MinSize
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
        public int MaxSize
        {
            get {
                return maxSize;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                maxSize = value;
            }
        }
        //---------------------------------------------------------------------
        public int SpringFMCLo
        {
            get {
                return springFMCLo;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                springFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        public int SpringFMCHi
        {
            get {
                return springFMCHi;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                springFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        public double SpringFMCHiProp
        {
            get {
                return springFMCHiProp;
            }
            set {
                    if (value < 0 || value > 1.0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0 and < or = 1.0.");
                springFMCHiProp = value;
            }
        }
        //---------------------------------------------------------------------
        public int SummerFMCLo
        {
            get {
                return summerFMCLo;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                summerFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        public int SummerFMCHi
        {
            get {
                return summerFMCHi;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                summerFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        public double SummerFMCHiProp
        {
            get {
                return summerFMCHiProp;
            }
            set {
                    if (value < 0 || value > 1.0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0 and < or = 1.0.");
                summerFMCHiProp = value;
            }
        }
        //---------------------------------------------------------------------
        public int FallFMCLo
        {
            get {
                return fallFMCLo;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                fallFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        public int FallFMCHi
        {
            get {
                return fallFMCHi;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                fallFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        public double FallFMCHiProp
        {
            get {
                return fallFMCHiProp;
            }
            set {
                    if (value < 0 || value > 1.0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0 and < or = 1.0.");
                fallFMCHiProp = value;
            }
        }
        //---------------------------------------------------------------------
        public int OpenFuelType
        {
            get {
                return openFuelType;
            }
            set {
                openFuelType = value;
            }
        }
        //---------------------------------------------------------------------
        public double EcoIgnitionNum
        {
            get {
                return ecoIgnitionNum;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0");
                ecoIgnitionNum = value;
            }
        }
        //---------------------------------------------------------------------
        public List<Location> FireRegionSites
        {
            get
            {
                return fireRegionSites;
            }
            //set {
            //    fireRegionSites = value;
            //}
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

        public FireRegion(int index)
        {
            fireRegionSites =   new List<Location>();
            this.index = index;
        }
        //---------------------------------------------------------------------
        
    }
}
