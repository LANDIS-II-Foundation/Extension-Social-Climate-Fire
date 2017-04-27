using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Climate;
using Landis.Core;

//namespace Landis.Extension.Scrapple
//{
//    class TempRetrieve// AnnualClimate, myWeatherData)
//    {
//        public static double GetTemperature(int day, IEcoregion ecoregion)
//        {
//            AnnualClimate_Daily myWeatherData;  //This gets temperature for a given julian day
//            double temperature = -9999.0;
//            double precipitation = -9999.0;
//            PlugIn.ModelCore.UI.WriteLine(" Time = {0} + {1}.", PlugIn.ModelCore.CurrentTime, Climate.Future_DailyData.First().Key);
//            int actualYear = PlugIn.ModelCore.CurrentTime + Climate.Future_DailyData.First().Key;
//            if (Climate.Future_DailyData.ContainsKey(actualYear))
//            {
//                myWeatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
//                temperature = myWeatherData.DailyMaxTemp[day];
//                precipitation = myWeatherData.DailyPrecip[day];
//            }
//            else
//            {
//                PlugIn.ModelCore.UI.WriteLine("Cannot find fire weather data for {0} for year {1}.", ecoregion.Name, PlugIn.ModelCore.CurrentTime);
//            }
//            PlugIn.ModelCore.UI.WriteLine("Temperature = {0}, JD = {1}", temperature, day);
//            PlugIn.ModelCore.UI.WriteLine("Precipitation = {0}, JD = {1}", precipitation, day);
//            return temperature;
//        }
//    }
//}
