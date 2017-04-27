//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System.Text;

namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// A parser that reads the tool parameters from text input.
    /// </summary>
    public class DynamicInputsParser
        : TextParser<Dictionary<int, IDynamicInputRecord[]>>
    {

        private string FileName = "Dynamic Fire Region Table";

        //---------------------------------------------------------------------
        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }
        //---------------------------------------------------------------------

        public DynamicInputsParser()
        {
        }

        //---------------------------------------------------------------------

        protected override Dictionary<int, IDynamicInputRecord[]> Parse()
        {

            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != FileName)
                throw new InputValueException(landisData.Value.String, "The value is not \"{0}\"", FileName);
            
            Dictionary<int, IDynamicInputRecord[]> allData = new Dictionary<int, IDynamicInputRecord[]>();

            //---------------------------------------------------------------------
            //Read in fire region data:
            InputVar<int>    year       = new InputVar<int>("Time step for updating values");
            //InputVar<string> ecoregionName = new InputVar<string>("Ecoregion Name");
            //InputVar<string> speciesName = new InputVar<string>("Species Name");
            //InputVar<double> pest = new InputVar<double>("Probability of Establishment");
            //InputVar<int> anpp = new InputVar<int>("ANPP");
            //InputVar<int> bmax = new InputVar<int>("Maximum Biomass");

            InputVar<string> name = new InputVar<string>("Name");
            InputVar<int> mapCode = new InputVar<int>("Map Code");
            InputVar<double> meanSize = new InputVar<double>("Mean Size");
            InputVar<double> standardDeviation = new InputVar<double>("Standard Deviation");
            InputVar<double> minSize = new InputVar<double>("Min Size");
            InputVar<double> maxSize = new InputVar<double>("Max Size");
            InputVar<int> spLo = new InputVar<int>("Spring Low FMC");
            InputVar<int> spHi = new InputVar<int>("Spring High FMC");
            InputVar<double> spHiPro = new InputVar<double>("Spring High FMC proportion");
            InputVar<int> suLo = new InputVar<int>("Summer Low FMC");
            InputVar<int> suHi = new InputVar<int>("Summer High FMC");
            InputVar<double> suHiPro = new InputVar<double>("Summer High FMC proportion");
            InputVar<int> faLo = new InputVar<int>("Fall Low FMC");
            InputVar<int> faHi = new InputVar<int>("Fall High FMC");
            InputVar<double> faHiPro = new InputVar<double>("Fall High FMC proportion");
            InputVar<int> ftc = new InputVar<int>("Open Type Fuel");
            InputVar<double> ein = new InputVar<double>("FireRegion Ignition Number");

            int fireRegionIndex = 0;
            int yr = 0;
            while (! AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(year, currentLine);
                int lastYear = yr;
                yr = year.Value.Actual;
                if (yr > lastYear)
                {
                    fireRegionIndex = 0;
                    FireRegions.AllData = allData;
                }
                
                   

                if(!allData.ContainsKey(yr))
                {
                    IDynamicInputRecord[] inputTable = new IDynamicInputRecord[1];
                    if (yr > 0)
                    {
                        inputTable = new IDynamicInputRecord[allData[lastYear].Length];
                        int newIndex = 0;
                        foreach (IDynamicInputRecord record in inputTable)
                        {
                            IDynamicInputRecord oldInputRecord = new DynamicInputRecord();
                            IDynamicInputRecord newInputRecord = new DynamicInputRecord();
                            oldInputRecord = allData[lastYear][newIndex];
                            newInputRecord = DynamicInputRecord.Clone(oldInputRecord);
                            inputTable[newIndex] = newInputRecord;
                            newIndex += 1;
                        }
                    }

                    //IDynamicInputRecord[,] inputTable = new IDynamicInputRecord[PlugIn.ModelCore.Species.Count, PlugIn.ModelCore.Ecoregions.Count];
                    allData.Add(yr, inputTable);
                    //PlugIn.ModelCore.UI.WriteLine("  Dynamic Input Parser:  Add new year = {0}.", yr);
                }

                ReadValue(mapCode, currentLine);
                ReadValue(name, currentLine);

                //IFireRegion fire_region = new FireRegion(fireRegionIndex);
                IDynamicInputRecord dynamicInputRecord = new DynamicInputRecord();

                if (yr == 0)
                {
                    dynamicInputRecord.Name = name.Value;
                    dynamicInputRecord.MapCode = mapCode.Value;
                    dynamicInputRecord.Index = fireRegionIndex;
                }

                if (yr > 0)
                {
                    IDynamicInputRecord oldInputRecord = new DynamicInputRecord();
                    IDynamicInputRecord newInputRecord = new DynamicInputRecord();
                    oldInputRecord = GetFireRegionRecord(name.Value);
                    int oldIndex = new int();
                    oldIndex = oldInputRecord.Index;
                    newInputRecord = DynamicInputRecord.Clone(oldInputRecord);
                    dynamicInputRecord = newInputRecord;
                    dynamicInputRecord.Index = oldIndex;
                }


                ReadValue(meanSize, currentLine);
                dynamicInputRecord.MeanSize = meanSize.Value;

                ReadValue(standardDeviation, currentLine);
                dynamicInputRecord.StandardDeviation = standardDeviation.Value;

                ReadValue(minSize, currentLine);
                dynamicInputRecord.MinSize = minSize.Value;

                ReadValue(maxSize, currentLine);
                dynamicInputRecord.MaxSize = maxSize.Value;

                ReadValue(spLo, currentLine);
                dynamicInputRecord.SpringFMCLo = spLo.Value;

                ReadValue(spHi, currentLine);
                dynamicInputRecord.SpringFMCHi = spHi.Value;

                ReadValue(spHiPro, currentLine);
                dynamicInputRecord.SpringFMCHiProp = spHiPro.Value;

                ReadValue(suLo, currentLine);
                dynamicInputRecord.SummerFMCLo = suLo.Value;

                ReadValue(suHi, currentLine);
                dynamicInputRecord.SummerFMCHi = suHi.Value;

                ReadValue(suHiPro, currentLine);
                dynamicInputRecord.SummerFMCHiProp = suHiPro.Value;

                ReadValue(faLo, currentLine);
                dynamicInputRecord.FallFMCLo = faLo.Value;

                ReadValue(faHi, currentLine);
                dynamicInputRecord.FallFMCHi = faHi.Value;

                ReadValue(faHiPro, currentLine);
                dynamicInputRecord.FallFMCHiProp = faHiPro.Value;

                ReadValue(ftc, currentLine);
                dynamicInputRecord.OpenFuelType = ftc.Value;

                ReadValue(ein, currentLine);
                dynamicInputRecord.EcoIgnitionNum = ein.Value;

                /*ReadValue(pest, currentLine);
                dynamicInputRecord.ProbEst = pest.Value;

                ReadValue(anpp, currentLine);
                dynamicInputRecord.ANPP_MAX_Spp = anpp.Value;

                ReadValue(bmax, currentLine);
                dynamicInputRecord.B_MAX_Spp = bmax.Value;
                */
                //bool check = allData[yr].Length > fire_region.Index;
                if ((yr > 0) || (allData[yr].Length > fireRegionIndex))
                    allData[yr][dynamicInputRecord.Index] = dynamicInputRecord;
                else
                {
                   //Need to add record to allData[yr]
                    IDynamicInputRecord[] newInputTable = new IDynamicInputRecord[fireRegionIndex + 1];

                    int tempIndex = 0;
                    foreach (IDynamicInputRecord tempRecord in allData[yr])
                    {
                        newInputTable[tempIndex] = tempRecord;
                        tempIndex += 1;
                    }
                    allData.Remove(yr);
                    allData.Add(yr, newInputTable);
                    allData[yr][fireRegionIndex] = dynamicInputRecord;
                }

                CheckNoDataAfter("the " + ein.Name + " column",
                                 currentLine);
                fireRegionIndex += 1;
                GetNextLine();

            }
            FireRegions.AllData = allData;
            return allData;
        }

        //---------------------------------------------------------------------
        /*
        private IFireRegion GetFireRegion(InputValue<string> fireRegionName)
        {
            IFireRegion fire_region = FireRegions.FindName(fireRegionName.Actual);
            if (fire_region == null)
                throw new InputValueException(fireRegionName.String,
                                              "{0} is not a fire region name.",
                                              fireRegionName.String);
             return fire_region;
        }
         * */
        //---------------------------------------------------------------------

        private IDynamicInputRecord GetFireRegionRecord(InputValue<string> fireRegionName)
        {
            IDynamicInputRecord fire_region = FireRegions.FindName(fireRegionName.Actual);
            if (fire_region == null)
                throw new InputValueException(fireRegionName.String,
                                              "{0} is not a fire region name.",
                                              fireRegionName.String);
            return fire_region;
        }
        //---------------------------------------------------------------------
        private IEcoregion GetEcoregion(InputValue<string>      ecoregionName)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregions[ecoregionName.Actual];
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String,
                                              "{0} is not an ecoregion name.",
                                              ecoregionName.String);
            //if (!ecoregion.Active)
            //    throw new InputValueException(ecoregionName.String,
            //                                  "{0} is not an active ecoregion.",
            //                                  ecoregionName.String);

            return ecoregion;
        }

        //---------------------------------------------------------------------

        private ISpecies GetSpecies(InputValue<string> speciesName)
        {
            ISpecies species = PlugIn.ModelCore.Species[speciesName.Actual];
            if (species == null)
                throw new InputValueException(speciesName.String,
                                              "{0} is not a recognized species name.",
                                              speciesName.String);

            return species;
        }


    }
}
