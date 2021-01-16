//  Authors:  Robert M. Scheller

using Landis.Utilities;

namespace Landis.Extension.Scrapple
{

    public interface ISuppressionTable
    {
        
        IgnitionType Type {get;set;}
        int MapCode { get; set; }
        double FWI_Break1{get;set;}
        double FWI_Break2{get;set;}
        int Suppression0{get;set;}
        int Suppression1{get;set;}
        int Suppression2{get;set;}
    }

    /// <summary>
    /// </summary>
    public class SuppressionTable
        : ISuppressionTable
    {
        private IgnitionType type;
        private double fwi_Break1;
        private double fwi_Break2;
        private int suppression0;
        private int suppression1;
        private int suppression2;
        private int mapCode;
        
        //---------------------------------------------------------------------

        public SuppressionTable()
        {
            fwi_Break1 = 0.0;
            fwi_Break2 = 0.0;
            suppression0 = 0;
            suppression1 = 0;
            suppression2 = 0;
            mapCode = 0;
        }
        //---------------------------------------------------------------------


        /// <summary>
        /// The ignition type
        /// </summary>
        public IgnitionType Type
        {
            get {
                return type;
            }
            set {
                type = value;
            }
        }

        //---------------------------------------------------------------------
        public double FWI_Break1
        {
            get {
                return fwi_Break1;
            }
            set {
                    if (value < 0.0)
                        throw new InputValueException(value.ToString(), "Value must be > 0");
                fwi_Break1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double FWI_Break2
        {
            get {
                return fwi_Break2;
            }
            set {
                if (value < 0.0)
                    throw new InputValueException(value.ToString(), "Value must be > 0");
                fwi_Break2 = value;
            }
        }
        //---------------------------------------------------------------------
        public int Suppression0
        {
            get {
                return suppression0;
            }
            set {
                if (value < 0 || value > 100)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 100");
                suppression0 = value;
            }
        }
        //---------------------------------------------------------------------
        public int Suppression1
        {
            get {
                return suppression1;
            }
            set {
                if (value < 0 || value > 100)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 100");
                suppression1 = value;
            }
        }
        //---------------------------------------------------------------------
        public int Suppression2
        {
            get {
                return suppression2;
            }
            set {
                if (value < 00 || value > 100)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 100");
                suppression2 = value;
            }
        }

        //---------------------------------------------------------------------
        public int MapCode
        {
            get
            {
                return mapCode;
            }
            set
            {
                if (value < 0 || value > 3)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 3");
                mapCode = value;
            }
        }


    }
}
