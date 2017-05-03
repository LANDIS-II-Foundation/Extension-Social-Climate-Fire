//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.SpatialModeling;
using System.IO;
using System.Collections.Generic;


namespace Landis.Extension.Scrapple
{
    public class FireRegions
    {
        //RMS: This class is derelict but it does provide an example of how to read a map.

        //public static IDynamicInputRecord[] Dataset;
        //public static int MaxMapCode;
        //public static Dictionary<int, IDynamicInputRecord[]> AllData;

        //---------------------------------------------------------------------

        public static void ReadMap(string path)
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
                //MaxMapCode = 0;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    int mapCode = pixel.MapCode.Value;
                    if (site.IsActive)
                    {
                        // RMS: SiteVars.MY-FAVORITE-SITEVAR HERE[site] = mapCode; 
                    }
                }
            }
        }


    }
}
