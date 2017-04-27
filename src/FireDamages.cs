//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// Definition of a Fire Damage Class.
    /// </summary>
    public interface IFireDamage
    {
        /// <summary>
        /// The maximum cohort ages (as % of species longevity) that the
        /// damage class applies to.
        /// </summary>
        Percentage MaxAge
        {get;set;}

        //---------------------------------------------------------------------

        /// <summary>
        /// The difference between fire severity and species fire tolerance.
        /// </summary>
        int SeverTolerDifference
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
        private Percentage maxAge;
        private int severTolerDifference;

        //---------------------------------------------------------------------

        /// <summary>
        /// The maximum cohort ages (as % of species longevity) that the
        /// damage class applies to.
        /// </summary>
        public Percentage MaxAge
        {
            get {
                return maxAge;
            }

            set {
                //if (value != null) {
                ValidateAge(value);
                //}
                maxAge = value;
            }
        }

        /// <summary>
        /// The difference between fire severity and species fire tolerance.
        /// </summary>
        public int SeverTolerDifference
        {
            get {
                return severTolerDifference;
            }

            set {
                //if (value != null) {
                    if (value < -4 || value > 4)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be between -4 and 4");
                //}
                severTolerDifference = value;
            }
        }

        //---------------------------------------------------------------------

        public FireDamage()
        {
        }
        //---------------------------------------------------------------------
/*
        public FireDamage(
                        double maxAge,
                        int  severTolerDifference)
        {
            this.maxAge = maxAge;
            this.severTolerDifference = severTolerDifference;
        }*/
        //---------------------------------------------------------------------

        private void ValidateAge(Percentage age)
        {
            if (age < 0.0 || age > 1.0)
                throw new InputValueException(age.ToString(),
                                              "Value must be between 0% and 100%");
        }
    }
}
