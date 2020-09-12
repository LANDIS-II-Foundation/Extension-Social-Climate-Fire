//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Scrapple
{
    public class IgnitionsLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int SimulationYear {set; get;}

        [DataFieldAttribute(Desc = "Day of Year")]
        public int DayOfYear { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Attempted Number of Ignitions")]
        public int AttemptedNumberIgnitions { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Actual Number of Ignitions")]
        public int ActualNumberIgnitions { set; get; }

        [DataFieldAttribute(Desc = "Daily Landscape Fire Weather Index")]
        public double FireWeatherIndex { set; get; }

        [DataFieldAttribute(Desc = "Ignition Type")]
        public string IgnitionType { set; get; }


    }
}
