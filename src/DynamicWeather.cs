//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Scrapple
{

    public interface IDynamicWeather
    {
        int Year {get;set;}
        string FileName{get;set;}
    }
}

namespace Landis.Extension.Scrapple
{
    public class DynamicWeather
    : IDynamicWeather
    {
        private string fileName;
        private int year;
        
        //---------------------------------------------------------------------
        public string FileName
        {
            get {
                return fileName;
            }
            set {
                if (value.Trim(null) == "")
                throw new InputValueException(value,
                                              "Invalid file path: {0}",
                                              value);
                fileName = value;
            }
        }

        //---------------------------------------------------------------------
        public int Year
        {
            get {
                return year;
            }
            set {
                    if (value < 0 )
                        throw new InputValueException(value.ToString(),
                            "Value must be > 0 ");
                year = value;
            }
        }
        //---------------------------------------------------------------------

        public DynamicWeather()
        {
        }

/*        //---------------------------------------------------------------------
        public DynamicWeatherTable(
            string fileName,
            int year
            )
        {
            this.fileName = fileName;
            this.year = year;
        }

        //---------------------------------------------------------------------

        public DynamicWeatherTable()
        {
            this.fileName = "";
            this.year = 0;
        }
*/

    }
}
