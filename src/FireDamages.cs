//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;


namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// Definition of a Fire Damage Class.
    /// </summary>
    public interface IFireDamage
    {
        /// <summary>
        /// The species that damage is applied to.
        /// </summary>
        ISpecies DamageSpecies
        { get; set; }

        /// <summary>
        /// The minimum species ages that the damage class applies to.
        /// </summary>
        int MinAge
        { get; set; }

        /// <summary>
        /// The maximum species ages that the damage class applies to.
        /// </summary>
        int MaxAge
        {get;set;}

        //---------------------------------------------------------------------

        /// <summary>
        /// The probability of mortality for the spp-age range.
        /// </summary>
        double ProbablityMortality
        {get;set;}

    }
}


namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// Definition of a Fire Damage class.
    /// </summary>
    public class FireDamage
        : IFireDamage
    {
        private int minAge;
        private int maxAge;
        private double probabilityMortality;
        private ISpecies damageSpecies;

        //---------------------------------------------------------------------

        /// <summary>
        /// The species that damage is applied to.
        /// </summary>
        public ISpecies DamageSpecies
        { 
            get {
                return damageSpecies;
            }
            set {
                damageSpecies = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// The minimum species ages that the damage class applies to.
        /// </summary>
        public int MinAge
        {
            get
            {
                return minAge;
            }

            set
            {
                if (value < damageSpecies.Longevity)
                    minAge = value;
                else
                    throw new InputValueException(value.ToString(),
                                                  "Value must be < species longevity");
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// The maximum species ages that the damage class applies to.
        /// </summary>
        public int MaxAge
        {
            get {
                return maxAge;
            }

            set {
                if(value <= damageSpecies.Longevity)
                    maxAge = value;
                else
                    throw new InputValueException(value.ToString(),
                                                  "Value must be < species longevity");
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// The probability of mortality for a given severity
        /// </summary>
        public double ProbablityMortality
        {
            get {
                return probabilityMortality;
            }

            set {
                if (value < 0.0 || value > 1.0)
                    throw new InputValueException(value.ToString(),
                                                  "Value must be between 0.0 and 1.0");
                probabilityMortality = value;
            }
        }

        //---------------------------------------------------------------------

        public FireDamage()
        {
        }
        
    }
}
