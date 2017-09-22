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

        [DataFieldAttribute(Desc = "Initiation Row")]
        public int InitRow { set; get; }

        [DataFieldAttribute(Desc = "Initiation Column")]
        public int InitColumn { set; get; }

        [DataFieldAttribute(Desc = "Day of Year")]
        public int DayOfYear { set; get; }

        [DataFieldAttribute(Desc = "Fire Weather Index")]
        public double FireWeatherIndex { set; get; }

        [DataFieldAttribute(Desc = "Ignition Type")]
        public string IgnitionType { set; get; }


    }
}
