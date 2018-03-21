//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;


namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// Definition of a Fire Damage Class.
    /// </summary>
    public interface IDeadWood
    {
        /// <summary>
        /// The species that damage is applied to.
        /// </summary>
        ISpecies Species
        { get; set; }

        /// <summary>
        /// The minimum species ages that the damage class applies to.
        /// </summary>
        int MinAge
        { get; set; }


    }
}


namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// Definition of a Fire Damage class.
    /// </summary>
    public class DeadWood
        : IDeadWood
    {
        private int minAge;
        private ISpecies species;

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead wood species 
        /// </summary>
        public ISpecies Species
        { 
            get {
                return species;
            }
            set {
                species = value;
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
                if (value < species.Longevity)
                    minAge = value;
                else
                    throw new InputValueException(value.ToString(),
                                                  "Value must be < species longevity");
            }
        }
        //---------------------------------------------------------------------

        public DeadWood()
        {
        }
        
    }
}
