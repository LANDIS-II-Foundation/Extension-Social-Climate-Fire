//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Srinivas S., Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.Scrapple
{
    public class IntPixel : Pixel
    {
        public Band<double> MapCode  = "The numeric code for each raster cell";
        public Band<int> SecondCode = "Who knows";
        public Band<int> ThirdCode = "more";
        public Band<int> ForthCode = "something more";

        

        public IntPixel()
        {
            PixelBand[] bands = new PixelBand[4];
            /*
            for (int i = 0; i < bands.Length; ++i)
            {
                bands[i] = MapCode;
            }
            */

            bands[0] = MapCode;
            bands[1] = SecondCode;
            bands[2] = ThirdCode;
            bands[3] = ForthCode;

            SetBands(MapCode);
        }
    }
}
