//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.SpatialModeling;
using System.IO;
using System.Collections.Generic;


namespace Landis.Extension.Scrapple
{
    public class FireRegions
    {
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
                        //if (Dataset == null)
                         //   PlugIn.ModelCore.Log.WriteLine("FireRegion.Dataset not set correctly.");
                        //IDynamicInputRecord fire_region = Find(mapCode);

                        //if (fire_region == null)
                        //{
                        //    string mesg = string.Format("Unknown map code = {0}, Row/Column = {1}/{2}", mapCode, site.Location.Row, site.Location.Column);
                        //    throw new System.ApplicationException(mesg);
                        //}
                        SiteVars.FireRegion[site] = mapCode; // fire_region;
                        //fire_region.FireRegionSites.Add(site.Location);
                        //if (mapCode > MaxMapCode)
                        //    MaxMapCode = mapCode;
                    }
                }
            }
        }

        //public static void ReadMap2(string path)
        //{
        //    IInputRaster<IntPixel> map;

        //    try
        //    {
        //        map = PlugIn.ModelCore.OpenRaster<IntPixel>(path);
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        string mesg = string.Format("Error: The file {0} does not exist", path);
        //        throw new System.ApplicationException(mesg);
        //    }

        //    if (map.Dimensions != PlugIn.ModelCore.Landscape.Dimensions)
        //    {
        //        string mesg = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the ecoregions map", path);
        //        throw new System.ApplicationException(mesg);
        //    }

        //    using (map)
        //    {
        //        IntPixel pixel = map.BufferPixel;
        //        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
        //        {
        //            map.ReadBufferPixel();
        //            int mapCode = pixel.MapCode.Value;
        //            if (site.IsActive)
        //            {
        //                //if (Dataset == null)
        //                //    PlugIn.ModelCore.Log.WriteLine("FireRegion.Dataset not set correctly.");
        //                IDynamicInputRecord fire_region = Find(mapCode);

        //                if (fire_region == null)
        //                {
        //                    string mesg = string.Format("Unknown map code = {0}, Row/Column = {1}/{2}", mapCode, site.Location.Row, site.Location.Column);
        //                    throw new System.ApplicationException(mesg);
        //                }
        //                SiteVars.FireRegion2[site] = fire_region;
        //                fire_region.FireRegionSites.Add(site.Location);
        //            }
        //        }
        //    }
        //}

        //public static IDynamicInputRecord Find(int mapCode)
        //{            
        //    foreach (IDynamicInputRecord regionRecord in FireRegions.AllData[0])
        //    {
        //        if (regionRecord.MapCode == mapCode)
        //        {
        //            //PlugIn.ModelCore.Log.WriteLine("FireRegion mapCode {0}.  Find {1}", fireregion.MapCode, mapCode);
        //            return regionRecord;
        //        }
        //    }
        //    return null;
        //}

        //public static IDynamicInputRecord FindName(string name)
        //{
        //    foreach(IDynamicInputRecord regionRecord in FireRegions.AllData[0])
        //        if(regionRecord.Name == name)
        //            return regionRecord;

        //    return null;
        //}

    }
}
