//  Authors:  Robert M. Scheller

using Landis.SpatialModeling;
using System.IO;
using System.Collections.Generic;


namespace Landis.Extension.Scrapple
{
    public class RxFireRegions
    {
        // FINISH LATER AFTER DYNAMIC RX IGNITIONS
        /*public static List<IDynamicIgnitionMap> Dataset;

        //---------------------------------------------------------------------

        public static void ReadMap(string path)
        {
            IInputRaster<IntPixel> map;

            try {
                map = PlugIn.ModelCore.OpenRaster<IntPixel>(path);
            }
            catch (FileNotFoundException) {
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
                    int mapCode = (int) pixel.MapCode.Value;
                    if (site.IsActive)
                    {
                        if (Dataset == null)
                            PlugIn.ModelCore.UI.WriteLine("FireRegion.Dataset not set correctly.");
                        IDynamicIgnitionMap ecoregion = Find(mapCode);

                        if (ecoregion == null)
                        {
                            string mesg = string.Format("mapCode = {0}, dimensions.rows = {1}", mapCode, map.Dimensions.Rows);
                            throw new System.ApplicationException(mesg);
                        }

                        SiteVars.RxRegion[site] = ecoregion;
                    }
                }
            }
        }

        private static IDynamicIgnitionMap Find(uint mapCode)
        {
            foreach(IDynamicIgnitionMap fireregion in Dataset)
                if(fireregion.MapCode == mapCode)
                    return fireregion;

            return null;
        }

        public static IDynamicIgnitionMap FindName(string name)
        {
            foreach(IDynamicIgnitionMap fireregion in Dataset)
                if(fireregion.Name == name)
                    return fireregion;

            return null;
        }
        */
    }
}
