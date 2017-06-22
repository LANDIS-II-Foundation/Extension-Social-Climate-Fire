//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.SpatialModeling;
using System.IO;
using System.Collections.Generic;


namespace Landis.Extension.Scrapple
{
    public static class FireRegions
    {
        private static List<FireRegion> fireRegionsSites;
        public static List<FireRegion> FireRegionsSites { get => fireRegionsSites; set => fireRegionsSites = value; }

        //RMS: This class is derelict but it does provide an example of how to read a map.
        

        public static void Initilize(string lighteningFireMap/*, string rxFireMap, string accidentalFireMap */)
        {
            ReadMap(lighteningFireMap, SiteVars.LightningFireWeight);
            //ReadMap(rxFireMap);
            //ReadMap(accidentalFireMap);
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
                string mesg = string.Format("Error: The file {0} does not exist", path);
                throw new System.ApplicationException(mesg);
            }

            if (map.Dimensions != PlugIn.ModelCore.Landscape.Dimensions)
            {
                string mesg = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the ecoregions map", path);
                throw new System.ApplicationException(mesg);
            }

            using (map) {
                IntPixel pixel = map.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    double mapCode = pixel.MapCode.Value;

                    if (site.IsActive)
                    {
                        // RMS: SiteVars.MY-FAVORITE-SITEVAR-HERE[site] = mapCode; 
                        SiteVars.AccidentalFireWeight[site] = mapCode;
                        //PlugIn.ModelCore.UI.WriteLine(string.Format("site name: {0},    map code: {1},    2nd code: {2},    3rd code: {3},   4th code: {4}", site.ToString(), mapCode, secondCode, thirdCode, forthCode));
                    }
                }
            }
        }


    }
}
