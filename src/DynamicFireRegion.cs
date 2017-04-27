//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Scrapple
{

    public interface IDynamicFireRegion
    {
        int Year {get;set;}
        string MapName{get;set;}
    }
}

namespace Landis.Extension.Scrapple
{
    public class DynamicFireRegion
    : IDynamicFireRegion
    {
        private string mapName;
        private int year;
        
        //---------------------------------------------------------------------
        public string MapName
        {
            get {
                return mapName;
            }

            set {
                mapName = value;
            }
        }

        //---------------------------------------------------------------------
        public int Year
        {
            get {
                return year;
            }

            set {
                //if (value != null) {
                    if (value < 0 )
                        throw new InputValueException(value.ToString(),
                            "Value must be > 0 ");
                //}
                year = value;
            }
        }
        //---------------------------------------------------------------------

        public DynamicFireRegion()
        {
        }
        //---------------------------------------------------------------------
/*
        public DynamicFireRegion(
            string mapName,
            int year
            )
        {
            this.mapName = mapName;
            this.year = year;
        }

        //---------------------------------------------------------------------

        public DynamicFireRegion()
        {
            this.mapName = "";
            this.year = 0;
        }*/


    }
}
