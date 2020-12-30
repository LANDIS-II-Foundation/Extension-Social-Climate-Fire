//  Author: Robert Scheller, Melissa Lucash

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Utilities;
//using Landis.Library.Climate;
using Landis.Library.Parameters;

using System.Collections.Generic;
using System;
using System.IO;

namespace Landis.Extension.Scrapple
{
    public class SpeciesData 
    {
        public static Landis.Library.Parameters.Species.AuxParm<double> AgeDBH;
        public static Landis.Library.Parameters.Species.AuxParm<double> MaximumBarkThickness;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            AgeDBH          = parameters.AgeDBH;
            MaximumBarkThickness = parameters.MaximumBarkThickness;

        }
    }
}
