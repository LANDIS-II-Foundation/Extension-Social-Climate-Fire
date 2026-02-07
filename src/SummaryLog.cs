//  Authors:  Robert M. Scheller, Alec Kretchun, Vincent Schuster

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.SocialClimateFire
{
    public class SummaryLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int SimulationYear {set; get;}

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Sites Accidental Burned")]
        public int TotalBurnedSitesAccidental { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Sites Lightning Burned")]
        public int TotalBurnedSitesLightning { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Sites Rx Burned")]
        public int TotalBurnedSitesRx { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Accidental Fires")]
        public int NumberFiresAccidental { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Lightning Fires")]
        public int NumberFiresLightning { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Rx Fires")]
        public int NumberFiresRx { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.g_B_m2, Desc = "Biomass Killed Accidental")]
        public int TotalBiomassMortalityAccidental { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.g_B_m2, Desc = "Biomass Killed Lightning")]
        public int TotalBiomassMortalityLightning { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.g_B_m2, Desc = "Biomass Killed Rx")]
        public int TotalBiomassMortalityRx { set; get; }

    }
}
