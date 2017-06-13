//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// The parameters for an ecoregion.
    /// </summary>
    public interface IFireRegion
    {
        string Name { get; set; }
        ushort MapCode { get; set; }
        int Index { get; set; }
        double RxFire { get; set; }
        double LighteningFire { get; set; }
        double AccidentalFire { get; set; }

        List<Location> FireRegionSites {get;}

    }
}

namespace Landis.Extension.Scrapple
{
    public class FireRegion
        : IFireRegion
    {
        private string name;
        private ushort mapCode;
        private int index;
        private double rxFire;
        private double lighteningFire;
        private double accidentalFire;
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
               if (value.Trim() == "")
                   throw new InputValueException(value, "Missing name");
             
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
        public List<Location> FireRegionSites
        {
            get
            {
                return fireRegionSites;
            }
        }

        public double LighteningFire
        {
            get
            {
                return lighteningFire;
            }
            set
            {
                lighteningFire = value;
            }
        }
        public double AccidentalFire { get => accidentalFire; set => accidentalFire = value; }
        public double RxFire { get => rxFire; set => rxFire = value; }

        //---------------------------------------------------------------------

        public FireRegion(int index)
        {
            fireRegionSites =   new List<Location>();
            this.index = index;
        }
        //---------------------------------------------------------------------
        
    }
}
