using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Scrapple
{
    public class SummaryLog
    {
        //summaryLog.Write("TimeStep, TotalSitesBurned, NumberFires");

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time {set; get;}

        [DataFieldAttribute(Desc = "Fire Region")]
        public string FireRegion { set; get; }

        //[DataFieldAttribute(Desc = "Prescription Name")]
        //public string Prescription { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Sites Burned")]
        public int TotalBurnedSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Fires")]
        public int NumberFires { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Cohorts Partial Harvest")]
        //public int TotalCohortsPartialHarvest { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Species Cohorts Harvested by Species", SppList = true)]
        //public int[] CohortsHarvested { set; get; }


    }
}
