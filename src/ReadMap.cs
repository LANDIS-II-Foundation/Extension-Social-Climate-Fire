//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Landis.SpatialModeling;
using System.IO;
using System.Collections.Generic;


namespace Landis.Extension.Scrapple
{
    public static class FireRegions
    {
        public static void Initilize(string lightningFireMap, string rxFireMap, string accidentalFireMap)
        {
            ReadMap(lightningFireMap, SiteVars.LightningFireWeight);
            ReadMap(rxFireMap, SiteVars.RxFireWeight);
            ReadMap(accidentalFireMap, SiteVars.AccidentalFireWeight);
        }

        //---------------------------------------------------------------------

        public static void ReadMap(string path, ISiteVar<double> siteVar)
        {
            IInputRaster<IntPixel> map;

            try
            {
                map = PlugIn.ModelCore.OpenRaster<IntPixel>(path);
            }
            catch (FileNotFoundException)
            {
                string messege = string.Format("Error: The file {0} does not exist", path);
                throw new System.ApplicationException(messege);
            }

            if (map.Dimensions != PlugIn.ModelCore.Landscape.Dimensions)
            {
                string messege = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the ecoregions map", path);
                throw new System.ApplicationException(messege);
            }

            using (map) {
                IntPixel pixel = map.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    double mapCode = pixel.MapCode.Value;

                    if (site.IsActive)
                    {
                        siteVar[site] = mapCode;
                    }
                }
            }
        }


    }
}
