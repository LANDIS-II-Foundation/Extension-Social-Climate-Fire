//  Authors:    Robert M. Scheller

using Landis.Utilities;

namespace Landis.Extension.Scrapple
{

    public interface IDynamicSuppressionMap
    {
        int Year {get;set;}
        string MapName{get;set;}
    }
}

namespace Landis.Extension.Scrapple
{
    public class DynamicSuppressionMap
    : IDynamicSuppressionMap
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
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                        "Value must be > 0 ");
                year = value;
            }

        }
        //---------------------------------------------------------------------

        public DynamicSuppressionMap()
        {
        }
        //---------------------------------------------------------------------


    }
}
