//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

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
                if (value < 0 )
                {
                    throw new InputValueException(value.ToString(), "Value must be > 0 ");
                }
                year = value;
            }
        }
        //---------------------------------------------------------------------

        public DynamicMap()
        {
        }


    }
}
