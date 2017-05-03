using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Scrapple
{
    public class SummaryLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time {set; get;}

        [DataFieldAttribute(Desc = "Fire Region")]
        public string FireRegion { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Sites Burned")]
        public int TotalBurnedSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Fires")]
        public int NumberFires { set; get; }


    }
}
