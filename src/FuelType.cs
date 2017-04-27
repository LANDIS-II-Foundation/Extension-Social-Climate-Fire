//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Core;
using System.Collections.Generic;


namespace Landis.Extension.Scrapple
{

    public enum SurfaceFuelType {C1, C2, C3, C4, C5, C6, C7, D1, S1, S2, S3, M1, M2, M3, M4, O1a, O1b, NoFuel};
    public enum BaseFuelType {Conifer, ConiferPlantation, Deciduous, NoFuel, Open, Slash};

    public interface IFuelType
    {
        int FuelIndex {get;set;}
        BaseFuelType BaseFuel{get;set;}
        SurfaceFuelType SurfaceFuel {get;set;}
        double InitiationProbability{get;set;}
        int A{get;set;}
        double B{get;set;}
        double C{get;set;}
        double Q{get;set;}
        int BUI{get;set;}
        double MaxBE{get;set;}
        int CBH{get;set;}  //Crown base height
        double IgnitionDistributionScale { get; set; }
        double IgnitionDistributionShape { get; set; }
    }
}

namespace Landis.Extension.Scrapple
{
    public class FuelType
    : IFuelType
    {
        private int fuelIndex;
        private BaseFuelType baseFuel;
        private SurfaceFuelType surfaceFuel;
        private double initiationProbability;
        private int a;
        private double b;
        private double c;
        private double q;
        private int bui;
        private double maxBE;
        private int cbh;
        private double ignitionDistributionShape;
        private double ignitionDistributionScale;
        
        //---------------------------------------------------------------------
        public int FuelIndex
        {
            get {
                return fuelIndex;
            }
            set {
                    if (value < 1 || value > 100)
                        throw new InputValueException(value.ToString(),
                            "Fuel Index must be between 1 and 100");
                fuelIndex = value;
            }
        }
        //---------------------------------------------------------------------
        /*public string FuelName
        {
            get {
                return fuelName;
            }
        }*/
        //---------------------------------------------------------------------
        public BaseFuelType BaseFuel
        {
            get {
                return baseFuel;
            }
            set {
                baseFuel = value;
            }
        }
        //---------------------------------------------------------------------
        public SurfaceFuelType SurfaceFuel
        {
            get {
                return surfaceFuel;
            }
            set {
                surfaceFuel = value;
            }
        }
        //---------------------------------------------------------------------
        public double InitiationProbability
        {
            get {
                return initiationProbability;
            }
            set {
                    if (value < 0.0 || value > 1.0)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 1.0");
                initiationProbability = value;
            }
        }

        //---------------------------------------------------------------------
        public int A
        {
            get {
                return a;
            }
            set {
                    if (value < 0 || value > 1000)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 1000");
                a = value;
            }
        }
        //---------------------------------------------------------------------
        public double B
        {
            get {
                return b;
            }
            set {
                    if (value < 0.0 || value > 1.0)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 1.0");
                b = value;
            }
        }
        //---------------------------------------------------------------------
        public double C
        {
            get {
                return c;
            }
            set {
                    if (value < 0.0 || value > 10.0)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 10.0");
                c = value;
            }
        }
        //---------------------------------------------------------------------
        public double Q
        {
            get {
                return q;
            }
            set {
                    if (value < 0.0 || value > 1.0)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 1.0");
                q = value;
            }
        }
        //---------------------------------------------------------------------
        public int BUI
        {
            get {
                return bui;
            }
            set {
                    if (value < 1 || value > 700)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 1 and 500");
                bui = value;
            }
        }
        //---------------------------------------------------------------------
        public double MaxBE
        {
            get {
                return maxBE;
            }
            set {
                    if (value < 0.9 || value > 2.0)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 1 and 2.0");
                maxBE = value;
            }
        }
        //---------------------------------------------------------------------
        public int CBH
        {
            get {
                return cbh;
            }
            set {
                    if (value < 0 || value > 100)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 100");
                cbh = value;
            }
        }
        //---------------------------------------------------------------------
        public double IgnitionDistributionScale
        {
            get
            {
                return ignitionDistributionScale;
            }
            set
            {
                if (value < 0.0 || value > 10.0)
                    throw new InputValueException(value.ToString(),
                        "Value must be between 0.0 and 10.0");
                ignitionDistributionScale = value;
            }
        }
        //---------------------------------------------------------------------
        public double IgnitionDistributionShape
        {
            get
            {
                return ignitionDistributionShape;
            }
            set
            {
                if (value < -10.0 || value > 10.0)
                    throw new InputValueException(value.ToString(),
                        "Value must be between -10.0 and 10.0");
                ignitionDistributionShape = value;
            }
        }
        //---------------------------------------------------------------------

        public FuelType()
        {
            this.baseFuel = BaseFuelType.NoFuel;
            this.surfaceFuel = SurfaceFuelType.NoFuel;
            this.initiationProbability = 0.0;
            this.a = 0;
            this.b = 0.0;
            this.c = 0.0;
            this.q = 0.0;
            this.bui = 0;
            this.maxBE = 0.0;
            this.cbh = 0;
            this.ignitionDistributionScale = 0.0;
            this.ignitionDistributionShape = 0.0;
        }
        //---------------------------------------------------------------------


    }
}
