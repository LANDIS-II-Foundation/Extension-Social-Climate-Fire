//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.Scrapple
{
    public class BytePixel : Pixel
    {
        public Band<byte> MapCode  = "The numeric code for each raster cell";

        public BytePixel()
        {
            SetBands(MapCode);
        }
    }
}
