//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

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
