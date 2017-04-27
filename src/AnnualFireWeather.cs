using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Climate;
using Landis.Core;

namespace Landis.Extension.Scrapple
{
// This class calculates fire weather variables. These calculations are based on the Canadian Fire and Fuels System.
// Refer to Lawson and Armitage 2008 "Weather Guide for the Canadian Forest Fire Danger Rating System" for further explanation
//This comment is to further test the GitHub commit process

    public class AnnualFireWeather
    {

        public static double FireWeatherIndex; 
            
        //public static double  FineFuelMoistureCode;      

        //public static double DuffMoistureCode;   

        //public static double DroughtCode;  
            
        //public static double BuildUpIndex;                
            
        public static double WindSpeedVelocity;          
            
        public static double WindAzimuth;              

        //public Season mySeason;    
            
        public int Ecoregion;                  

        //public enum Season {Winter, Spring, Summer, Fall};
	
        public static void CalculateFireWeather(int day, IEcoregion ecoregion)
        {
            //int spring_start = PlugIn.SeasonParameters[0].StartDay;
            //int summer_start = PlugIn.SeasonParameters[1].StartDay;
            //int fall_start = PlugIn.SeasonParameters[2].StartDay;
            //int winter_start = PlugIn.SeasonParameters[2].EndDay + 1;
            double RHslopeadjust =  PlugIn.RelativeHumiditySlopeAdjust;

            //ISeasonParameters season = null;

            //for (int i = 0; i <= 2; i++)
            //{
            //    if (day >= PlugIn.SeasonParameters[i].StartDay && day <= PlugIn.SeasonParameters[i].EndDay)
            //        season = PlugIn.SeasonParameters[i];
            //}

            //if (season == null)
            //    throw new System.ArgumentException("Season within AnnualFireWeather is null", "original");   
            
            for (int d = 0; d <= 365; d++) //This section loops through all the days of the fire season and retrieves various climate variables below
            {
                AnnualClimate_Daily myWeatherData;  
                double temperature = -9999.0;
                double precipitation = -9999.0;
                WindSpeedVelocity = -9999.0;
                WindAzimuth = -9999.0;
                double relative_humidity = -9999;

                int actualYear = (PlugIn.ModelCore.CurrentTime -1) + Climate.Future_DailyData.First().Key;  

                if (Climate.Future_DailyData.ContainsKey(actualYear))
                {
                    double test = Climate.Future_DailyData[actualYear][ecoregion.Index].AnnualAET;

                    myWeatherData = Climate.Future_DailyData[actualYear][ecoregion.Index];
                    temperature = (myWeatherData.DailyMaxTemp[d] + myWeatherData.DailyMinTemp[d]) / 2;  
                    precipitation = myWeatherData.DailyPrecip[d];
                    WindSpeedVelocity = myWeatherData.DailyWindSpeed[d];
                    WindAzimuth = myWeatherData.DailyWindDirection[d];
                    relative_humidity = 100 * Math.Exp((RHslopeadjust * myWeatherData.DailyMinTemp[d]) / (273.15 + myWeatherData.DailyMinTemp[d]) - (RHslopeadjust * temperature) / (273.15 + temperature));
                    //Relative humidity calculations include RHslopeadjust variable to correct for location of study.
                }
                else
                {
                    PlugIn.ModelCore.UI.WriteLine("Cannot find fire weather data for {0} for year {1}.", ecoregion.Name, PlugIn.ModelCore.CurrentTime);
                }

                //double FineFuelMoistureCode_yesterday = 85; //These are seed values for the beginning of the fire season
                //double DuffMoistureCode_yesterday = 6;
                //double DroughtCode_yesterday = 15;

                //if (d != spring_start) //for each new day, this loop assigns yesterday's fire weather variables
                //{
                //    FineFuelMoistureCode_yesterday = FineFuelMoistureCode;
                //    DuffMoistureCode_yesterday = DuffMoistureCode;
                //    DroughtCode_yesterday = DroughtCode;
                //}
 
                //double mo = Calculate_mo(d, FineFuelMoistureCode_yesterday);  
                //double rf = Calculate_rf(d, precipitation);
                //double mr = Calculate_mr(d, mo, rf);
                //double Ed = Calculate_Ed(d, relative_humidity, temperature);
                //double Ew = Calculate_Ew(d, relative_humidity, temperature);
                //double ko = Calculate_ko(d, relative_humidity, WindSpeedVelocity);
                //double kd = Calculate_kd(d, ko, temperature);
                //double kl = Calculate_kl(d, relative_humidity, WindSpeedVelocity);
                //double kw = Calculate_kw(d, kl, temperature);
                //double m = Calculate_m(d, mo, Ed, kd, Ew, kw);
                //double re = Calculate_re(d, precipitation);
                //double Mo = Calculate_Mo(d, spring_start, DuffMoistureCode_yesterday); 
                //double b = Calculate_b(d, spring_start, DuffMoistureCode_yesterday);  
                //double Mr = Calculate_Mr(d, re, b, Mo);
                //double Pr = Calculate_Pr(d, Mr);
                //int month = Calculate_month(d);
                //double Le1 = Calculate_Le1(d, month);
                //double Le2 = Calculate_Le2(d, month);
                //double Le = Calculate_Le(d, Le1, Le2);
                //double K = Calculate_K(d, temperature, relative_humidity, Le);
                //Calculate_DuffMoistureCode(d, precipitation, Pr, K, DuffMoistureCode_yesterday);
                //double rd = Calculate_rd(d, precipitation);
                //double Qo = Calculate_Qo(d, spring_start, DroughtCode_yesterday);
                //double Qr = Calculate_Qr(d, Qo, rd);
                //double Dr = Calculate_Dr(d, Qr);
                //double Lf = Calculate_Lf(d, month);
                //double V = Calculate_V(d, temperature, Lf);
                //Calculate_DroughtCode(d, precipitation, spring_start, Dr, V, DroughtCode_yesterday);
                //double WindFunction_ISI = Calculate_WindFunction_ISI(d, WindSpeedVelocity);
                //double FineFuelMoistureFunction_ISI = Calculate_FineFuelMoistureFunction_ISI(d, m);
                //double InitialSpreadIndex = Calculate_InitialSpreadIndex(d, spring_start, winter_start, WindFunction_ISI, FineFuelMoistureFunction_ISI);
                //Calculate_BuildUpIndex(d, DuffMoistureCode, DroughtCode);
                //double fD = Calculate_fD(d, BuildUpIndex);
                //double B = Calculate_B(d, InitialSpreadIndex, fD);
                //Calculate_FireWeatherIndex(d, B);  
                //double I_scale = Calculate_I_scale(d,  FireWeatherIndex); 
                //double DSR = Calculate_DSR(d, spring_start, winter_start, FireWeatherIndex);
  
                //Calculate_FineFuelMoistureCode(d, m, spring_start, winter_start);                    
                //Calculate_Season(d, spring_start, summer_start, fall_start, winter_start);
            } 

           return;
    }

    //    private static double Calculate_mo(int d, double FineFuelMoistureCode_yesterday)
    //{
    //    double mo = 0;

    //        mo = 147.2 * (101.0 - FineFuelMoistureCode_yesterday)/(59.5 + FineFuelMoistureCode_yesterday);  //This used to be an explicit seed value for FFMC

    //    return mo;
    //}

    //private static double Calculate_rf(int d, double precipitation)
    //{
    //    double rf = 0.0;

    //    rf = precipitation - 0.5; 

    //    if (rf < 0)
    //    {
    //    rf = 0;
    //    }

    //    return rf;
    //}

    //private static double Calculate_mr(int d, double mo, double rf)
    //{
    //    double mr = 0.0;        
        
    //    if (mo <= 150.0)
    //    {

    //        if (rf > 0)
    //        {
    //            mr = mo + 42.5 * rf * Math.Exp(-100.0/(251.0 - mo)) * (1 - Math.Exp(-6.93/rf));  
    //        }
    //        else
    //        {
    //            mr = mo;
    //        }
    //    }
    //    else
    //    {
    //        if (rf > 0)
    //        {
    //            mr = mo + 42.5 * rf * Math.Exp(-100.0/(251.0 - mo)) * (1 - Math.Exp(-6.93/rf)) + 0.0015 * Math.Pow((mo - 150.0), 2) * Math.Pow(rf, 0.5);  
    //        }
    //        else
    //        {
    //            mr = mo; 
    //        }
    //    }
    //    if (mr > 250)
    //    { 
    //        mr = 250;
    //    }

    //    return mr;
    //}

    //private static double Calculate_Ed(int d, double relative_humidity, double temperature)
    //{
    //    double Ed = 0.0;
		
    //    Ed = 0.942 * Math.Pow(relative_humidity, 0.679) + 11.0 * Math.Exp((relative_humidity-100.0)/10.0) + 0.18 * (21.1 - temperature) * (1.0 - Math.Exp(-0.115 * relative_humidity));

    //    return Ed;
    //}

    //private static double Calculate_Ew(int d, double relative_humidity, double temperature)
    //{
    //    double Ew = 0.0;
		
    //    Ew = 0.618 * Math.Pow(relative_humidity, 0.753) + 10.0 * Math.Exp((relative_humidity-100.0)/10.0) + 0.18 * (21.1 - temperature) * (1.0 - Math.Exp(-0.115 * relative_humidity));                          //selfs

    //    return Ew;
    //}

    //private static double Calculate_ko(int d, double relative_humidity, double WindSpeedVelocity)
    //{
    //    double ko = 0.0;
		
    //    ko = 0.424 * (1.0 - Math.Pow((relative_humidity/100.0), 1.7)) + 0.0694 * Math.Pow(WindSpeedVelocity, 0.5) * (1.0- Math.Pow((relative_humidity/100.0), 8));

    //    return ko;
    //}

    //private static double Calculate_kd(int d, double ko, double temperature)
    //{
    //    double kd = 0.0;

    //    kd = ko * 0.581 * Math.Exp(0.0365 * temperature);    

    //    return kd;
    //}

    //private static double Calculate_kl(int d, double relative_humidity, double WindSpeedVelocity)
    //{
    //    double kl = 0.0;

    //    kl =0.424 * (1.0 - Math.Pow(((100.0 - relative_humidity)/100.0), 1.7)) + 0.0694 * Math.Pow(WindSpeedVelocity, 0.5) * (1.0 - Math.Pow(((100.0 - relative_humidity)/100.0), 8));

    //    return kl;
    //}

    //private static double Calculate_kw(int d, double kl, double temperature)
    //{
    //    double kw = 0.0;
        
    //    kw = kl * 0.581 * Math.Exp(0.0365 * temperature);

    //    return kw;
    //}

    //private static double Calculate_m(int d, double mo, double Ed, double kd, double Ew, double kw)
    //{
    //    double m = 0.0;

    //    try
    //    {           
    //        if (mo > Ed)
    //        {
    //            m = Ed + (mo - Ed) * Math.Pow(10.0, (-kd));
    //        }
    //        else
    //        {
    //            if (mo < Ed)
    //            {
    //                if (mo < Ew)
    //                {
    //                    m = Ew -(Ew - mo) * Math.Pow(10.0, (-kw));
    //                }
    //                else
    //                {
    //                    m = mo; 
    //                }
    //            }
    //            else
    //            {
    //                m = mo;
    //            }
                
    //        }
    //    }                  
    //    catch
    //    {
    //        throw new System.ArgumentException("m cannot be null", "original"); 
    //    }

    //    return m;    
    //}

    //private static void Calculate_FineFuelMoistureCode(int d, double m, int spring_start, int winter_start) 
    //{
    //    FineFuelMoistureCode = 0.0;

    //    if (d >= spring_start && d < winter_start) 
    //    {
    //        FineFuelMoistureCode = 59.5 * (250.0 - m) / (147.2 + m);

    //        if (FineFuelMoistureCode > 100.0)
    //        {
    //            FineFuelMoistureCode = 100.0; 
    //        }
    //    }

    //    else
    //    {
    //        throw new System.ArgumentException("FFMC cannot be null", "original");   
    //    }

    //    return;
    //}

    //private static double Calculate_re(int d, double precipitation)
    //{
    //    double re = 0.0;

    //    if (precipitation > 1.5)
    //    {
    //        re = 0.92 * precipitation - 1.27;
    //    }
    //    else
    //    {
    //        re = 0; 
    //    }

    //    return re;
    //}

    //private static double Calculate_Mo(int d, int spring_start, double DuffMoistureCode_yesterday) 
    //{
    //    double Mo = 0.0;

    //    try
    //    { 
    //            Mo = 20.0 + Math.Exp(5.6348 - DuffMoistureCode_yesterday/43.43);
          
    //    }
    //    catch
    //    {
    //        throw new System.ArgumentException("Mo cannot be null", "original");  
    //    }

    //    return Mo;
    //}

    //private static double Calculate_b(int d, int spring_start, double DuffMoistureCode_yesterday) //, int DMC_start
    //{
    //    double b = 0.0;

    //    try
    //    {

    //        if (DuffMoistureCode_yesterday <= 33 && d > 0)
    //        {
    //            b = 100/(0.5 + 0.3 * DuffMoistureCode_yesterday);
    //        }
            
    //        else if (DuffMoistureCode_yesterday > 65 && d > 0)
    //        {
    //            b = 6.2 * Math.Log(DuffMoistureCode_yesterday)-17.2;
    //        }
            
    //        else if (d > 0)
    //        {

    //                b = 14.0 - 1.3 * Math.Log(DuffMoistureCode_yesterday);
    //        }
                
    //        else 
    //        { 
    //             throw new System.ArgumentException("b cannot be null", "original");   
    //        }
    //    }
    //    catch
    //    {
    //        throw new System.ArgumentException("b cannot be null", "original"); 
    //    }

    //    return b;
    //}

    //private static double Calculate_Mr(int d, double re, double b, double Mo)
    //{
    //    double Mr = 0.0;
		
    //    try
    //    {
    //        Mr = Mo + 1000.0 * re/(48.77 + b * re);
    //    }
    //    catch
    //    {
    //        throw new System.ArgumentException("Mr cannot be null", "original");  
    //    }

    //    return Mr;
    //}

    //private static double Calculate_Pr(int d, double Mr)
    //{
    //    double Pr = 0.0;

    //    if (d == 91)
    //    {
    //        Pr = 244.72 - 43.43 * Math.Log(Mr - 20.0); 
    //        if (Pr < 0.0)
    //        {
    //            Pr = 0.0;
    //        }
    //    }
    //    else
    //    {
    //        try
    //        {
    //            Pr = 244.72 - 43.43 * Math.Log(Mr - 20.0);
            
    //            if (Pr < 0.0)
    //            {
    //                Pr = 0.0;
    //            }
    //        }
    //        catch
    //        {
    //            throw new System.ArgumentException("pr cannot be null", "original");  
    //        } 
    //    }

    //    return Pr;
    //}

    private static int Calculate_month(int d)
    {
        int month = 0;

        if (d <= 31)
        {
            month = 1;
        }
        else if (d > 31 && d <= 60) 
        {
            month = 2;
        }
        else if (d > 60 && d <= 91) 
        {
            month = 3;
        }
        else if (d > 91 && d <= 121)
        {
            month = 4;
        }
        else if (d > 121 && d <= 152)
        {
            month = 5;
        }
        else if (d > 152 && d <= 182)
        {
            month = 6;
        }
        else if (d > 182 && d <= 213)
        {
            month = 7;
        }
        else if (d > 213 && d <= 244)
        {
            month = 8;
        }
        else if (d > 244 && d <= 274)
        {
            month = 9;
        }
        else if (d > 274 && d <= 305)
        {
            month = 10;
        }
        else if (d > 305 && d <= 335)
        {
            month = 11;
        }

        else 
        {
            month = 12;
        }

        return month;                
    }

    //private static double Calculate_Le1(int d, int month)
    //{
    //    double Le1 = 0.0;
 
    //    if (month == 1) 
    //    {
    //        Le1 = 6.5;
    //    }
    //    else if (month == 2)
    //    {
    //        Le1 = 7.5;
    //    }
    //    else if (month == 3)
    //    {
    //        Le1 = 9.0;
    //    }
    //    else if (month == 4)
    //    {
    //        Le1 = 12.8;
    //    }
    //    else if (month == 5) 
    //    {
    //        Le1 = 13.9;
    //    }
    //    else if (month == 6)
    //    {
    //       Le1 = 13.9;
    //    }
    //    else if (month == 7)
    //    {
    //        Le1 = 12.4;
    //    }
    //    else if (month == 8)
    //    {
    //        Le1 = 10.9;
    //    }
    //    else 
    //    {
    //        Le1 = 0.0;
    //    }

    //    return Le1;
    //}

    //private static double Calculate_Le2(int d, int month)
    //{
    //    double Le2 = 0.0;

    //    if (month == 9) 
    //    {
    //        Le2 = 9.2;
    //    }
    //    else if (month == 10)
    //    {
    //        Le2 = 8.0;
    //    }
    //    else if (month == 11)
    //    {
    //        Le2 = 7.0;
    //    }
    //    else if (month == 12)
    //    {
    //        Le2 = 6.0;
    //    }
		
    //    else
    //    {
    //        Le2 = 0.0;
    //    }

    //    return Le2;
    //}

    //private static double Calculate_Le(int d, double Le1, double Le2)
    //{
    //    double Le = 0.0;

    //    if (Le1 == 0.0)
    //    {
    //        Le = Le2;
    //    }
    //    else
    //    {
    //        Le = Le1;
    //    }

    //    return Le;
    //}

    //private static double Calculate_K(int d, double temperature, double relative_humidity, double Le)
    //{
    //    double K = 0.0;

    //    if (temperature < -1.1)
    //    {
    //        K = 0.0;
    //    }

    //    else
    //    {
    //        K = 1.894 * (temperature + 1.1) * (100.0 - relative_humidity) * Le * Math.Pow(10.0, -6.0);
    //    }

    //    return K;
    //}

    //private static double Calculate_DuffMoistureCode(int d, double precipitation, double Pr, double K, double DuffMoistureCode_yesterday) //int spring_start, int winter_start, double DMC_start
    //{
    //    if (precipitation > 1.5)
    //    {
    //        DuffMoistureCode = Pr + 100.0 * K;
    //    }

    //    else
    //    {
    //        DuffMoistureCode = DuffMoistureCode_yesterday + 100.0 * K;
    //    }

    //    return DuffMoistureCode;
    //}

    //private static double Calculate_rd(int d, double precipitation)
    //{
    //    double rd = 0.0;
        
    //    if (precipitation > 2.8)
    //    {
    //        rd = 0.83 * precipitation - 1.27;
    //    }

    //    else
    //    {
    //        rd = 0;
    //    }

    //    return rd;
    //}


    //private static double Calculate_Qo(int d, int spring_start, double DroughtCode_yesterday) //, int DC_start
    //{
    //    double Qo = 0.0;

    //    try
    //    {

    //        Qo = 800.0 * Math.Exp(-DroughtCode_yesterday / 400.0);
    //    }
    //    catch
    //    {
    //        throw new System.ArgumentException("Qo cannot be null", "original");
    //    }

    //    return Qo;
    //}

    //private static double Calculate_Qr(int d, double Qo, double rd)
    //{
    //    double Qr = 0.0;

    //    try
    //    {
    //        Qr = Qo + 3.937 * rd;
    //    }
    //    catch
    //    {
    //        throw new System.ArgumentException("Qr cannot be null", "original");   
    //    }

    //    return Qr;
    //}

    //private static double Calculate_Dr(int d, double Qr)
    //{
    //    double Dr = 0.0;

    //    try
    //    {
    //        Dr = 400.0 * Math.Log(800.0/Qr);

    //        if (Dr < 0)
    //        {
    //            Dr = 0;
    //        }
    //    }
    //    catch
    //    {
    //        throw new System.ArgumentException("Dr cannot be null", "original");  
    //    }

    //    return Dr;
    //}

    //private static double Calculate_Lf(int d, int month)
    //{
    //    double Lf = 0.0;

    //    if (month <= 3)
    //    {
    //        Lf = 1.6;
    //    }
    //    else if (month == 4.0) 
    //    {
    //        Lf = 0.9;
    //    }
    //    else if (month == 5.0)
    //    {    
    //        Lf = 3.8;
    //    }
    //    else if (month == 6.0) 
    //    {
    //        Lf = 5.8;
    //    }
    //    else if (month == 7.0)
    //    {
    //        Lf = 6.4;
    //    }
    //    else if (month == 8.0)
    //    {
    //        Lf = 5.0;
    //    }
    //    else if (month == 9.0)
    //    {
    //        Lf = 2.4;
    //    }
    //    else if (month == 10.0)
    //    {
    //        Lf = 0.4;
    //    }
    //    else
    //    {
    //        Lf = -1.6;
    //    }

    //    return Lf;
    //}


    //private static double Calculate_V(int d, double temperature, double Lf)
    //{
    //    double V = 0.0;

    //    if (temperature < -2.8)
    //    {
    //        V = 0.36 * (-2.8+2.8) + Lf;
    //    }
    //    else
    //    {
    //        V = 0.36 * (temperature+2.8) + Lf;
    //    }

    //    return V;
    //}

    //private static double Calculate_DroughtCode(int d, double precipitation, int spring_start, double Dr, double V, double DroughtCode_yesterday)
    //{
    //        if (d == spring_start)
    //        {

    //            if (precipitation > 2.8)
    //            {
    //                DroughtCode = Dr + 0.5 * V;
    //            }
    //            else
    //            {
    //                DroughtCode = DroughtCode_yesterday + 0.5 * V;
    //            }
    //        }
    //        else
    //        {
    //            if (precipitation > 2.8)
    //            {
    //                DroughtCode = Dr + 0.5 * V;
    //            }
    //            else
    //            {
    //                DroughtCode = DroughtCode_yesterday + 0.5 * V;
    //            }
    //        }

    //    return DroughtCode;
    //}

    private static double Calculate_WindFunction_ISI(int d, double WindSpeedVelocity)
	{
		double WindFunction_ISI = 0.0;

        WindFunction_ISI = Math.Exp(0.05039 * WindSpeedVelocity);

		return WindFunction_ISI;
	}


    //private static double Calculate_FineFuelMoistureFunction_ISI(int d, double m)
    //{
    //    double FineFuelMoistureFunction_ISI = 0.0;

    //    try
    //    {
    //        FineFuelMoistureFunction_ISI  = 91.9 * Math.Exp(-0.1386 * m)*(1.0 + Math.Pow(m, 5.31)/(4.93 * Math.Pow(10.0, 7.0)));  
    //    }
    //    catch
    //    {
    //        throw new System.ArgumentException("FFMC_ISI cannot be null", "original");  
    //    }

    //    return FineFuelMoistureFunction_ISI;
    //}

    //private static double Calculate_InitialSpreadIndex(int d, int spring_start, int winter_start, double WindFunction_ISI, double FineFuelMoistureFunction_ISI)
    //{
    //    double InitialSpreadIndex = 0.0;

    //    if (d >= spring_start && d < winter_start)
    //    {
    //        InitialSpreadIndex = 0.208 * WindFunction_ISI * FineFuelMoistureFunction_ISI;
    //    }

    //    else 
    //    {
    //        throw new System.ArgumentException("ISI cannot be null", "original");  
    //    }

    //    return InitialSpreadIndex;
    //}

    //private static double Calculate_BuildUpIndex(int d,   double DuffMoistureCode, double DroughtCode)  //int spring_start, int winter_start,
    //{
    //        if (DuffMoistureCode <= (0.4 * DroughtCode))
    //        {
    //            BuildUpIndex = 0.8 * DuffMoistureCode * DroughtCode/(DuffMoistureCode + 0.4 * DroughtCode);
    //        }
    //        else
    //        {
    //            BuildUpIndex = DuffMoistureCode-(1.0 - 0.8 * DroughtCode/(DuffMoistureCode + 0.4 * DroughtCode))*(0.92 + (0.0114 * Math.Pow(DuffMoistureCode, 1.7)));
    //        }

    //    return BuildUpIndex; 
    //}

    //private static double Calculate_fD(int d, double BuildUpIndex)
    //{
    //    double fD = 0.0;

    //    try
    //    {
    //        if (BuildUpIndex <= 80.0)
    //        {
    //            fD = 0.626 * Math.Pow(BuildUpIndex, 0.809) + 2.0;
    //        }
    //        else
    //        {
    //            fD = 1000.0/(25.0 + 108.64 * Math.Exp(-0.023 * BuildUpIndex));
    //        }
    //    }
    //    catch
    //    {
    //        throw new System.ArgumentException("fD cannot be null", "original");  
    //    }

    //    return fD;
    //}

    //private static double Calculate_B(int d, double InitialSpreadIndex, double fD)
    //{
    //    double B = 0.0;
		
    //    try
    //    {
    //        B = 0.1 * InitialSpreadIndex * fD;
    //    }
        
    //    catch
    //    {
    //        throw new System.ArgumentException("B cannot be null", "original");  
    //    }

    //    return B;
    //}

    //private static double Calculate_FireWeatherIndex(int d, double B) //int spring_start, int winter_start, 
    //{
    //    FireWeatherIndex = 0.0;

    //        if (B > 1.0)
    //        {
    //            FireWeatherIndex = Math.Exp(2.72 * Math.Pow((0.434 * Math.Log(B)), 0.647));
    //        }
    //        else
    //        {
    //            FireWeatherIndex = B;
    //        }

    //    return FireWeatherIndex;
    //}

    //private static double Calculate_I_scale(int d,  double FireWeatherIndex) 
    //{
    //    double I_scale = 0.0;

    //        try
    //        {
    //            I_scale = (1.0/0.289)* (Math.Exp(0.98 * (Math.Pow(Math.Log(FireWeatherIndex),1.546))));
    //        }
    //        catch
    //        {
    //            throw new System.ArgumentException("I_scale cannot be null", "original");   
    //        }

    //    return I_scale;
    //}

    //private static double Calculate_DSR(int d, int spring_start, int winter_start, double FireWeatherIndex)
    //{
    //    double DSR = 0.0;

    //    if (d >= spring_start && d < winter_start)
    //    {
    //        DSR = 0.0272 * Math.Pow(FireWeatherIndex, 1.77);
    //    }

    //    else
    //    {
    //        throw new System.ArgumentException("DSR cannot be null", "original");   
    //    }

    //    return DSR;
    //}

    //private static void Calculate_Season(int d, int spring_start, int summer_start, int fall_start, int winter_start)
    //{    
    //    Season mySeason;  

    //    if (d >= spring_start && d < summer_start)
    //    {    
    //        mySeason = Season.Spring;
    //    }    
    //    else if (d >= summer_start && d < fall_start)
    //    {     
    //        mySeason = Season.Summer;
    //    }    
    //    else if (d >= fall_start && d < winter_start)
    //    {    
    //        mySeason = Season.Fall;
    //    }    
    //    else 
    //    {    
    //        mySeason = Season.Winter;
    //    }

    //    return;
    //}


    }
} 

