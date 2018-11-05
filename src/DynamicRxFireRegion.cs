//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Utilities;

namespace Landis.Extension.BaseFire
{

    public interface IDynamicRxRegions
    {
        int Year {get;set;}
        string MapName{get;set;}
    }
}

namespace Landis.Extension.BaseFire
{
    public class DynamicRxFireRegion
    : IDynamicRxRegions
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

        public DynamicRxFireRegion()
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
