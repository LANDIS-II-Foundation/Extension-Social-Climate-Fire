//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;

//RMS: An example of how to read in maps through time.  We're saving this for a late itertion.

namespace Landis.Extension.Scrapple
{

    public interface IDynamicMap
    {
        int Year {get;set;}
        string MapName{get;set;}
    }
}

namespace Landis.Extension.Scrapple
{
    public class DynamicMap
    : IDynamicMap
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

        public DynamicMap()
        {
        }


    }
}
