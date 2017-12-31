//  Authors:  Robert M. Scheller

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Scrapple
{

    public interface ISuppressionTable
    {
        
        Ignition Type {get;set;}
        double FWI_Break1{get;set;}
        double FWI_Break2{get;set;}
        double EffectivenessLow{get;set;}
        double EffectivenessMedium{get;set;}
        double EffectivenessHigh{get;set;}
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
        private double effectivenessLow;
        private double effectivenessMedium;
        private double effectivenessHigh;
        
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
        public double EffectivenessLow
        {
            get {
                return effectivenessLow;
            }
            set {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 1");
                effectivenessLow = value;
            }
        }
        //---------------------------------------------------------------------
        public double EffectivenessMedium
        {
            get {
                return effectivenessMedium;
            }
            set {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 1");
                effectivenessMedium = value;
            }
        }
        //---------------------------------------------------------------------
        public double EffectivenessHigh
        {
            get {
                return effectivenessHigh;
            }
            set {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(), "Value must be between 0 and 1");
                effectivenessHigh = value;
            }
        }

    }
}
