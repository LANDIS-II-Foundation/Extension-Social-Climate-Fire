//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Srinivas S., Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.Scrapple
{
    public class IntPixel : Pixel
    {
        public Band<double> MapCode  = "The numeric code for each raster cell";

        public IntPixel()
        {
            SetBands(MapCode);
        }
    }
}
