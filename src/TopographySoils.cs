//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using Landis.SpatialModeling;
using System.IO;


namespace Landis.Extension.Scrapple
{
    internal static class TopographySoils
    {
        //---------------------------------------------------------------------

        internal static void ReadGroundSlopeMap(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("   Reading in {0}", path);
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

            using (map)
            {
                IntPixel pixel = map.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    double mapCode = pixel.MapCode.Value;
                    if (site.IsActive)
                    {
                        if (mapCode < 0)
                        {
                            string mesg = string.Format("Ground Slope invalid map code: {0}", mapCode);
                            throw new System.ApplicationException(mesg);
                        }
                        SiteVars.GroundSlope[site] = (ushort) mapCode;
                    }
                }
            }
        }
        //---------------------------------------------------------------------

        internal static void ReadUphillSlopeAzimuthMap(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("   Reading in {0}", path);
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
                        if (mapCode < 0 || mapCode > 360)
                        {
                            string mesg = string.Format("Uphill slope azimuth invalid map code (<0 or >360): {0}", mapCode);
                            throw new System.ApplicationException(mesg);
                        }
                        SiteVars.UphillSlopeAzimuth[site] = (ushort) mapCode;
                    }
                }
            }
        }
        //---------------------------------------------------------------------

        internal static void ReadClayMap(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("   Reading in {0}", path);
            IInputRaster<DoublePixel> map;
            try
            {
                map = PlugIn.ModelCore.OpenRaster<DoublePixel>(path);
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

            using (map)
            {
                DoublePixel pixel = map.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    double mapCode = pixel.MapCode.Value;
                    if (site.IsActive)
                    {
                        if (mapCode < 0)
                        {
                            string mesg = string.Format("Ground Slope invalid map code: {0}", mapCode);
                            throw new System.ApplicationException(mesg);
                        }
                        SiteVars.Clay[site] = mapCode;
                    }
                }
            }
        }

    }
}
