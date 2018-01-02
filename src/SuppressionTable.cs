//  Authors:  Robert M. Scheller

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Scrapple
{

    public interface ISuppressionTable
    {
        
        Ignition Type {get;set;}
        double FWI_Break1{get;set;}
        double FWI_Break2{get;set;}
        int EffectivenessLow{get;set;}
        int EffectivenessMedium{get;set;}
        int EffectivenessHigh{get;set;}
    }

    /// <summary>
    /// Definition of the probability of germination under different light levels for 5 shade classes.
    /// </summary>
    public class SuppressionTable
        : ISuppressionTable
    {
        private Ignition type;
        private double fwi_Break1;
        private double fwi_Break2;
        private int effectivenessLow;
        private int effectivenessMedium;
        private int effectivenessHigh;
        
        //---------------------------------------------------------------------

        public SuppressionTable()
        {
        }
        //---------------------------------------------------------------------


        /// <summary>
        /// The ignition type
        /// </summary>
        public Ignition Type
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
        public int EffectivenessLow
        {
            get {
                return effectivenessLow;
            }
            set {
                if (value < 0 || value > 100)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 100");
                effectivenessLow = value;
            }
        }
        //---------------------------------------------------------------------
        public int EffectivenessMedium
        {
            get {
                return effectivenessMedium;
            }
            set {
                if (value < 0 || value > 100)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 100");
                effectivenessMedium = value;
            }
        }
        //---------------------------------------------------------------------
        public int EffectivenessHigh
        {
            get {
                return effectivenessHigh;
            }
            set {
                if (value < 00 || value > 100)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 100");
                effectivenessHigh = value;
            }
        }

    }
}
