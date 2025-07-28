//  Authors:  Robert M. Scheller, Vincent Schuster, Alec Kretchun

using Landis.Utilities;
using Landis.Core;
using System.Collections.Generic;
using System.Data;

namespace Landis.Extension.SocialClimateFire
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class InputParameterParser
        : TextParser<IInputParameters>
    {
        private InputVar<string> speciesName;
        private ISpeciesDataset speciesDataset;
        private Dictionary<string, int> speciesLineNums;
        //---------------------------------------------------------------------
        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }
        //---------------------------------------------------------------------

        public InputParameterParser()
        {
            this.speciesDataset = PlugIn.ModelCore.Species;
            this.speciesLineNums = new Dictionary<string, int>();
            this.speciesName = new InputVar<string>("Species");
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            ReadLandisDataVar();
            RegisterForInputValues();

            InputParameters parameters = new InputParameters(speciesDataset);

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<double> tzpet = new InputVar<double>("TimeZeroPET");
            if(ReadOptionalVar(tzpet))
                parameters.TimeZeroPET = tzpet.Value;
            else
                parameters.TimeZeroPET = 0.0;

            InputVar<double> tzcwd = new InputVar<double>("TimeZeroCWD");
            if (ReadOptionalVar(tzcwd)) 
                parameters.TimeZeroCWD = tzcwd.Value;
            else
                parameters.TimeZeroCWD = 0.0;

            //-------------------------
            //  Read Species Parameters table
            PlugIn.ModelCore.UI.WriteLine("   Begin parsing NECN SPECIES table.");

            InputVar<string> csv = new InputVar<string>("Species_CSV_File");
            ReadVar(csv);

            CSVParser speciesParser = new CSVParser();
            DataTable speciesTable = speciesParser.ParseToDataTable(csv.Value);
            foreach (DataRow row in speciesTable.Rows)
            {
                ISpecies species = ReadSpecies(System.Convert.ToString(row["SpeciesCode"]));
                parameters.AgeDBH[species] = System.Convert.ToDouble(row["AgeDBH"]);
                parameters.MaximumBarkThickness[species] = System.Convert.ToDouble(row["MaximumBarkThickness"]);
            }

            InputVar<string> humanIgnitionsMapFile = new InputVar<string>("AccidentalIgnitionsMap");
            ReadVar(humanIgnitionsMapFile);
            parameters.AccidentalFireMap = humanIgnitionsMapFile.Value;

            //----------------------------------------------------------
            // Read in the table of dynamic ignitions:

            if (ReadOptionalName("DynamicAccidentalIgnitionMaps"))
            {

                InputVar<string> mapName = new InputVar<string>("Dynamic Accidental Ignition Map Name");
                InputVar<int> year = new InputVar<int>("Year to read in new ignition map");

                double previousYear = 0;

                while (!AtEndOfInput && CurrentName != "LightningIgnitionsMap")
                {
                    StringReader currentLine = new StringReader(CurrentLine);

                    IDynamicIgnitionMap dynAxIgnMap = new DynamicIgnitionMap();
                    parameters.DynamicAccidentalIgnitionMaps.Add(dynAxIgnMap);

                    ReadValue(year, currentLine);
                    dynAxIgnMap.Year = year.Value;

                    if (year.Value.Actual <= previousYear)
                    {
                        throw new InputValueException(year.Value.String,
                            "Year must > the year ({0}) of the preceeding ecoregion map",
                            previousYear);
                    }

                    previousYear = year.Value.Actual;

                    ReadValue(mapName, currentLine);
                    dynAxIgnMap.MapName = mapName.Value;

                    CheckNoDataAfter("the " + mapName.Name + " column",
                                     currentLine);
                    GetNextLine();
                }
            }

            InputVar<string> lighteningIgnitionsMapFile = new InputVar<string>("LightningIgnitionsMap");
            ReadVar(lighteningIgnitionsMapFile);
            parameters.LighteningFireMap = lighteningIgnitionsMapFile.Value;

            //----------------------------------------------------------
            // Read in the table of dynamic ignitions:

            if (ReadOptionalName("DynamicLightningIgnitionMaps"))
            {

                InputVar<string> mapName = new InputVar<string>("Dynamic Lightning Ignition Map Name");
                InputVar<int> year = new InputVar<int>("Year to read in new ignition map");

                double previousYear = 0;

                while (!AtEndOfInput && CurrentName != "RxIgnitionsMap")
                {
                    StringReader currentLine = new StringReader(CurrentLine);

                    IDynamicIgnitionMap dynLxIgnMap = new DynamicIgnitionMap();
                    parameters.DynamicLightningIgnitionMaps.Add(dynLxIgnMap);

                    ReadValue(year, currentLine);
                    dynLxIgnMap.Year = year.Value;

                    if (year.Value.Actual <= previousYear)
                    {
                        throw new InputValueException(year.Value.String,
                            "Year must > the year ({0}) of the preceeding ecoregion map",
                            previousYear);
                    }

                    previousYear = year.Value.Actual;

                    ReadValue(mapName, currentLine);
                    dynLxIgnMap.MapName = mapName.Value;

                    CheckNoDataAfter("the " + mapName.Name + " column",
                                     currentLine);
                    GetNextLine();
                }
            }

            InputVar<string> rxIgnitionsMapFile = new InputVar<string>("RxIgnitionsMap");
            ReadVar(rxIgnitionsMapFile);
            parameters.RxFireMap = rxIgnitionsMapFile.Value;

            //----------------------------------------------------------
            // Read in the table of dynamic Rx ignitions:

            if (ReadOptionalName("DynamicRxIgnitionMaps"))
            {

                InputVar<string> mapName = new InputVar<string>("Dynamic Rx Ignition Map Name");
                InputVar<int> year = new InputVar<int>("Year to read in new ignitions map");

                double previousYear = 0;

                while (!AtEndOfInput && CurrentName != "AccidentalSuppressionMap")
                {
                    StringReader currentLine = new StringReader(CurrentLine);

                    IDynamicIgnitionMap dynRxIgnMap = new DynamicIgnitionMap();
                    parameters.DynamicRxIgnitionMaps.Add(dynRxIgnMap);

                    ReadValue(year, currentLine);
                    dynRxIgnMap.Year = year.Value;

                    if (year.Value.Actual <= previousYear)
                    {
                        throw new InputValueException(year.Value.String,
                            "Year must > the year ({0}) of the preceeding ecoregion map",
                            previousYear);
                    }

                    previousYear = year.Value.Actual;

                    ReadValue(mapName, currentLine);
                    dynRxIgnMap.MapName = mapName.Value;

                    CheckNoDataAfter("the " + mapName.Name + " column",
                                     currentLine);
                    GetNextLine();
                }
            }

            InputVar<string> humanSuppressionMapFile = new InputVar<string>("AccidentalSuppressionMap");
            ReadVar(humanSuppressionMapFile);
            parameters.AccidentalSuppressionMap = humanSuppressionMapFile.Value;

            InputVar<string> lighteningSuppressionMapFile = new InputVar<string>("LightningSuppressionMap");
            ReadVar(lighteningSuppressionMapFile);
            parameters.LighteningSuppressionMap = lighteningSuppressionMapFile.Value;

            InputVar<string> rxSuppressionMapFile = new InputVar<string>("RxSuppressionMap");
            ReadVar(rxSuppressionMapFile);
            parameters.RxSuppressionMap = rxSuppressionMapFile.Value;

            //----------------------------------------------------------
            // Read in the table of dynamic suppression maps:

            if (ReadOptionalName("DynamicAccidentalSuppressionMaps"))
            {

                InputVar<string> mapName = new InputVar<string>("Dynamic Suppression Map Name");
                InputVar<int> year = new InputVar<int>("Year to read in new Suppression Map");

                double previousYear = 0;

                while (!AtEndOfInput && CurrentName != "GroundSlopeMap")
                {
                    StringReader currentLine = new StringReader(CurrentLine);

                    IDynamicSuppressionMap dynSuppMap = new DynamicSuppressionMap();
                    parameters.DynamicSuppressionMaps.Add(dynSuppMap);

                    ReadValue(year, currentLine);
                    dynSuppMap.Year = year.Value;

                    if (year.Value.Actual <= previousYear)
                    {
                        throw new InputValueException(year.Value.String,
                            "Year must > the year ({0}) of the preceeding ecoregion map",
                            previousYear);
                    }

                    previousYear = year.Value.Actual;

                    ReadValue(mapName, currentLine);
                    dynSuppMap.MapName = mapName.Value;

                    CheckNoDataAfter("the " + mapName.Name + " column",
                                     currentLine);
                    GetNextLine();
                }
            }


            // Load Ground Slope Data
            InputVar<string> groundSlopeFile = new InputVar<string>("GroundSlopeMap");
            ReadVar(groundSlopeFile);
            TopographySoils.ReadGroundSlopeMap(groundSlopeFile.Value);

            // Load Uphill Slope Azimuth Data
            InputVar<string> uphillSlopeMap = new InputVar<string>("UphillSlopeAzimuthMap");
            ReadVar(uphillSlopeMap);
            TopographySoils.ReadUphillSlopeAzimuthMap(uphillSlopeMap.Value);

            // Load Clay Data
            InputVar<string> clayMap = new InputVar<string>("ClayMap");
            ReadVar(clayMap);
            TopographySoils.ReadClayMap(clayMap.Value);

            InputVar<double> lightningB0 = new InputVar<double>("LightningIgnitionsB0");
            ReadVar(lightningB0);
            parameters.LightningIgnitionB0 = lightningB0.Value;

            InputVar<double> lightningB1 = new InputVar<double>("LightningIgnitionsB1");
            ReadVar(lightningB1);
            parameters.LightningIgnitionB1 = lightningB1.Value;

            InputVar<double> accidentalB0 = new InputVar<double>("AccidentalIgnitionsB0");
            ReadVar(accidentalB0);
            parameters.AccidentalFireIgnitionB0 = accidentalB0.Value;

            InputVar<double> accidentalB1 = new InputVar<double>("AccidentalIgnitionsB1");
            ReadVar(accidentalB1);
            parameters.AccidentalFireIgnitionB1 = accidentalB1.Value;

            InputVar<IgnitionDistribution> igniteDist = new InputVar<IgnitionDistribution>("IgnitionDistribution");
            if (ReadOptionalVar(igniteDist) && igniteDist.Value == IgnitionDistribution.ZeroInflatedPoisson)
            {
                PlugIn.IgnitionDist = IgnitionDistribution.ZeroInflatedPoisson;

                InputVar<double> lightningBinomialB0 = new InputVar<double>("LightningIgnitionsBinomialB0");
                ReadVar(lightningBinomialB0);
                parameters.LightningIgnitionBinomialB0 = lightningBinomialB0.Value;

                InputVar<double> lightningBinomialB1 = new InputVar<double>("LightningIgnitionsBinomialB1");
                ReadVar(lightningBinomialB1);
                parameters.LightningIgnitionBinomialB1 = lightningBinomialB1.Value;

                InputVar<double> accidentalBinomialB0 = new InputVar<double>("AccidentalIgnitionsBinomialB0");
                ReadVar(accidentalBinomialB0);
                parameters.AccidentalFireIgnitionBinomialB0 = accidentalBinomialB0.Value;

                InputVar<double> accidentalBinomialB1 = new InputVar<double>("AccidentalIgnitionsBinomialB1");
                ReadVar(accidentalBinomialB1);
                parameters.AccidentalFireIgnitionBinomialB1 = accidentalBinomialB1.Value;

            }


            InputVar<double> maxFF = new InputVar<double>("MaximumFineFuels");
            ReadVar(maxFF);
            parameters.MaxFineFuels = maxFF.Value;

            InputVar<double> maxRxWS = new InputVar<double>("MaximumRxWindSpeed");
            ReadVar(maxRxWS);
            parameters.RxMaxWindSpeed = maxRxWS.Value;

            InputVar<double> maxRxFWI = new InputVar<double>("MaximumRxFireWeatherIndex");
            if(ReadOptionalVar(maxRxFWI))
                parameters.RxMaxFireWeatherIndex = maxRxFWI.Value;
            else
                parameters.RxMaxFireWeatherIndex = 300;  // the maximum imaginable

            InputVar<double> minRxFWI = new InputVar<double>("MinimumRxFireWeatherIndex");
            if (ReadOptionalVar(minRxFWI))
                parameters.RxMinFireWeatherIndex = minRxFWI.Value;
            else
                parameters.RxMinFireWeatherIndex = 0;

            InputVar<double> maxRxT = new InputVar<double>("MaximumRxTemperature");
            if (ReadOptionalVar(maxRxT))
                parameters.RxMaxTemperature = maxRxT.Value;
            else
                parameters.RxMaxTemperature = 50;  // the maximum imaginable

            InputVar<double> minRxRH = new InputVar<double>("MinimumRxRelativeHumidity");
            if (ReadOptionalVar(minRxRH))
                parameters.RxMinRelativeHumidity = minRxRH.Value;
            else
                parameters.RxMinRelativeHumidity = -500.0;

            InputVar<int> maxRxFI = new InputVar<int>("MaximumRxFireIntensity");
            ReadVar(maxRxFI);
            parameters.RxMaxFireIntensity = maxRxFI.Value;

            InputVar<int> nrxf_a = new InputVar<int>("NumberRxAnnualFires");
            ReadVar(nrxf_a);
            parameters.RxNumberAnnualFires = nrxf_a.Value;

            InputVar<int> nrxf_d = new InputVar<int>("NumberRxDailyFires");
            ReadVar(nrxf_d);
            parameters.RxNumberDailyFires = nrxf_d.Value;

            InputVar<int> first_rx_day = new InputVar<int>("FirstDayRxFires");
            ReadVar(first_rx_day);
            parameters.RxFirstDayFire = first_rx_day.Value;

            InputVar<int> last_rx_day = new InputVar<int>("LastDayRxFires");
            ReadVar(last_rx_day);
            parameters.RxLastDayFire = last_rx_day.Value;

            InputVar<int> trxs = new InputVar<int>("TargetRxSize");
            ReadVar(trxs);
            parameters.RxTargetSize = trxs.Value;

            InputVar<string> rzn = new InputVar<string>("RxZonesMap");
            if (ReadOptionalVar(rzn))
                parameters.RxZonesMap = rzn.Value;
            else
                parameters.RxZonesMap = null;

            InputVar<double> maxSA0 = new InputVar<double>("MaximumSpreadAreaB0");
            ReadVar(maxSA0);
            parameters.MaximumSpreadAreaB0 = maxSA0.Value;

            InputVar<double> maxSA1 = new InputVar<double>("MaximumSpreadAreaB1");
            ReadVar(maxSA1);
            parameters.MaximumSpreadAreaB1 = maxSA1.Value;

            InputVar<double> maxSA2 = new InputVar<double>("MaximumSpreadAreaB2");
            ReadVar(maxSA2);
            parameters.MaximumSpreadAreaB2 = maxSA2.Value;

            InputVar<double> sp0 = new InputVar<double>("SpreadProbabilityB0");
            ReadVar(sp0);
            parameters.SpreadProbabilityB0 = sp0.Value;

            InputVar<double> sp1 = new InputVar<double>("SpreadProbabilityB1");
            ReadVar(sp1);
            parameters.SpreadProbabilityB1 = sp1.Value;

            InputVar<double> sp2 = new InputVar<double>("SpreadProbabilityB2");
            ReadVar(sp2);
            parameters.SpreadProbabilityB2 = sp2.Value;

            InputVar<double> sp3 = new InputVar<double>("SpreadProbabilityB3");
            ReadVar(sp3);
            parameters.SpreadProbabilityB3 = sp3.Value;

            InputVar<double> sm0 = new InputVar<double>("SiteMortalityB0");
            ReadVar(sm0);
            parameters.SiteMortalityB0 = sm0.Value;

            InputVar<double> sm1 = new InputVar<double>("SiteMortalityB1");
            ReadVar(sm1);
            parameters.SiteMortalityB1 = sm1.Value;

            InputVar<double> sm2 = new InputVar<double>("SiteMortalityB2");
            ReadVar(sm2);
            parameters.SiteMortalityB2 = sm2.Value;

            InputVar<double> sm3 = new InputVar<double>("SiteMortalityB3");
            ReadVar(sm3);
            parameters.SiteMortalityB3 = sm3.Value;

            InputVar<double> sm4 = new InputVar<double>("SiteMortalityB4");
            ReadVar(sm4);
            parameters.SiteMortalityB4 = sm4.Value;

            InputVar<double> sm5 = new InputVar<double>("SiteMortalityB5");
            ReadVar(sm5);
            parameters.SiteMortalityB5 = sm5.Value;

            InputVar<double> sm6 = new InputVar<double>("SiteMortalityB6");
            ReadVar(sm6);
            parameters.SiteMortalityB6 = sm6.Value;

            InputVar<double> cm0 = new InputVar<double>("CohortMortalityB0");
            ReadVar(cm0);
            parameters.CohortMortalityB0 = cm0.Value;

            InputVar<double> cm1 = new InputVar<double>("CohortMortalityB1");
            ReadVar(cm1);
            parameters.CohortMortalityB1 = cm1.Value;

            InputVar<double> cm2 = new InputVar<double>("CohortMortalityB2");
            ReadVar(cm2);
            parameters.CohortMortalityB2 = cm2.Value;

            InputVar<int> lfma = new InputVar<int>("LadderFuelMaxAge");
            ReadVar(lfma);
            parameters.LadderFuelMaxAge = lfma.Value;

            //  Read the species list for ladderfuels:
            List<string> speciesNames = new List<string>();

            const string LadderFuelSpeciesList = "LadderFuelSpeciesList";
            ReadName(LadderFuelSpeciesList);

            while (!AtEndOfInput && CurrentName != "SuppressionMaxWindSpeed")
            {
                StringReader currentLine = new StringReader(CurrentLine);
                TextReader.SkipWhitespace(currentLine);
                while (currentLine.Peek() != -1)
                {
                    ReadValue(speciesName, currentLine);
                    string name = speciesName.Value.Actual;

                    if (speciesNames.Contains(name))
                        throw NewParseException("The species {0} appears more than once.", name);
                    speciesNames.Add(name);

                    ISpecies species = GetSpecies(new InputValue<string>(name, speciesName.Value.String));
                    parameters.LadderFuelSpeciesList.Add(species);

                    TextReader.SkipWhitespace(currentLine);
                }
                GetNextLine();
            }
            //foreach (ISpecies ladder_spp in parameters.LadderFuelSpeciesList)
            //    PlugIn.ModelCore.UI.WriteLine("    Ladder fuel species: {0}", ladder_spp.Name);

            InputVar<int> smws = new InputVar<int>("SuppressionMaxWindSpeed");
            ReadVar(smws);
            parameters.SuppressionMaxWindSpeed = smws.Value;

            InputVar<string> csv_suppress = new InputVar<string>("Suppression_CSV_File");
            ReadVar(csv_suppress);

            CSVParser suppressionParser = new CSVParser();
            DataTable suppressionTable = suppressionParser.ParseToDataTable(csv_suppress.Value);
            foreach (DataRow row in suppressionTable.Rows)
            {
                ISuppressionTable suppressTable = new SuppressionTable();

                suppressTable.Type = IgnitionTypeParse(System.Convert.ToString(row["IgnitionType"]));
                suppressTable.MapCode = System.Convert.ToInt32(row["MapCode"]);
                suppressTable.FWI_Break1 = System.Convert.ToDouble(row["FWI_Break_1"]);
                suppressTable.FWI_Break2 = System.Convert.ToDouble(row["FWI_Break_2"]);
                suppressTable.Suppression0 = System.Convert.ToInt32(row["Suppress_Category_0"]);
                suppressTable.Suppression1 = System.Convert.ToInt32(row["Suppress_Category_1"]);
                suppressTable.Suppression2 = System.Convert.ToInt32(row["Suppress_Category_2"]);

                int index = suppressTable.MapCode + ((int)suppressTable.Type * 10);

                parameters.SuppressionFWI_Table.Add(index, suppressTable);

            }

            InputVar<bool> writednbrpreds = new InputVar<bool>("WriteDNBRPredictorMaps");
            if (ReadOptionalVar(writednbrpreds))
            {
                parameters.WriteDNBRPredictorMaps = writednbrpreds.Value;
                PlugIn.ModelCore.UI.WriteLine("Extra dNBR predictor maps = TRUE");
            }
            else
            {
                parameters.WriteDNBRPredictorMaps = false;
                PlugIn.ModelCore.UI.WriteLine("Extra dNBR predictor maps = FALSE");
            }


            //-------------------------------------------------------------------
            //  Read table of Fire Damage classes.
            //  Damages are in increasing order.
            PlugIn.ModelCore.UI.WriteLine("   Loading Dead Wood table...");


            InputVar<string> dw_spp = new InputVar<string>("Species Name");
            InputVar<int> dw_minAge = new InputVar<int>("Min Interval Age");

            ReadName("DeadWoodTable");
            while (!AtEndOfInput)// && CurrentName != FireIntensityClass_1_DamageTable)
            {
                StringReader currentLine = new StringReader(CurrentLine);
                IDeadWood dead_wood_list = new DeadWood();
                parameters.DeadWoodList.Add(dead_wood_list);

                ReadValue(dw_spp, currentLine);
                ISpecies species = PlugIn.ModelCore.Species[dw_spp.Value.Actual];
                if (species == null)
                    throw new InputValueException(dw_spp.Value.String,
                                                  "{0} is not a species name.",
                                                  dw_spp.Value.String);
                dead_wood_list.Species = species;

                ReadValue(dw_minAge, currentLine);
                dead_wood_list.MinAge = dw_minAge.Value;

                GetNextLine();
            }


                

            //-------------------------------------------------------------------
                //  Read table of Fire Damage classes.
                //  Damages are in increasing order.
                //PlugIn.ModelCore.UI.WriteLine("   Loading Fire mortality tables...");

                ////InputVar<string> spp = new InputVar<string>("Species Name");
                //InputVar<int> maxAge = new InputVar<int>("Max Interval Age");
                //InputVar<int> minAge = new InputVar<int>("Min Interval Age");
                //InputVar<double> probMortality = new InputVar<double>("Probability of Mortality");

                //ReadName(FireIntensityClass_1_DamageTable);
                //while (!AtEndOfInput && CurrentName != FireIntensityClass_2_DamageTable)
                //{
                //    StringReader currentLine = new StringReader(CurrentLine);
                //    IFireDamage damage = new FireDamage();
                //    parameters.FireDamages_Severity1.Add(damage);

                //    ReadValue(speciesName, currentLine);
                //    ISpecies species = PlugIn.ModelCore.Species[speciesName.Value.Actual];
                //    if (species == null)
                //        throw new InputValueException(speciesName.Value.String,
                //                                      "{0} is not a species name.",
                //                                      speciesName.Value.String);
                //    damage.DamageSpecies = species;

                //    ReadValue(minAge, currentLine);
                //    damage.MinAge = minAge.Value;

                //    ReadValue(maxAge, currentLine);
                //    damage.MaxAge = maxAge.Value;

                //    ReadValue(probMortality, currentLine);
                //    damage.ProbablityMortality = probMortality.Value;

                //    GetNextLine();
                //}

                //ReadName(FireIntensityClass_2_DamageTable);
                //while (!AtEndOfInput && CurrentName != FireIntensityClass_3_DamageTable)
                //{
                //    //int previousMaxAge = 0;

                //    StringReader currentLine = new StringReader(CurrentLine);

                //    //ISpecies species = ReadSpecies(currentLine);
                //    ReadValue(speciesName, currentLine);
                //    ISpecies species = PlugIn.ModelCore.Species[speciesName.Value.Actual];
                //    if (species == null)
                //        throw new InputValueException(speciesName.Value.String,
                //                                      "{0} is not a species name.",
                //                                      speciesName.Value.String);

                //    IFireDamage damage = new FireDamage();
                //    parameters.FireDamages_Severity2.Add(damage);
                //    damage.DamageSpecies = species;

                //    ReadValue(minAge, currentLine);
                //    damage.MinAge = minAge.Value;

                //    ReadValue(maxAge, currentLine);
                //    damage.MaxAge = maxAge.Value;

                //    ReadValue(probMortality, currentLine);
                //    damage.ProbablityMortality = probMortality.Value;

                //    GetNextLine();
                //}

                //ReadName(FireIntensityClass_3_DamageTable);
                //while (!AtEndOfInput) // && CurrentName != LadderFuelSpeciesList) 
                //{

                //    StringReader currentLine = new StringReader(CurrentLine);

                //    //ISpecies species = ReadSpecies(currentLine);
                //    ReadValue(speciesName, currentLine);
                //    ISpecies species = PlugIn.ModelCore.Species[speciesName.Value.Actual];
                //    if (species == null)
                //        throw new InputValueException(speciesName.Value.String,
                //                                      "{0} is not a species name.",
                //                                      speciesName.Value.String);
                //    IFireDamage damage = new FireDamage();
                //    parameters.FireDamages_Severity3.Add(damage);
                //    damage.DamageSpecies = species;

                //    ReadValue(minAge, currentLine);
                //    damage.MinAge = minAge.Value;

                //    ReadValue(maxAge, currentLine);
                //    damage.MaxAge = maxAge.Value;

                //    ReadValue(probMortality, currentLine);
                //    damage.ProbablityMortality = probMortality.Value;

                //    GetNextLine();
                //}

                // Next, read out the data to verify:
                //PlugIn.ModelCore.UI.WriteLine("   Fire mortality data for severity class 1:");
                //foreach (FireDamage damage in parameters.FireDamages_Severity1)
                //    PlugIn.ModelCore.UI.WriteLine("      {0} : {1} : {2}", damage.DamageSpecies.Name, damage.MaxAge, damage.ProbablityMortality);

                //PlugIn.ModelCore.UI.WriteLine("   Fire mortality data for severity class 2:");
                //foreach (FireDamage damage in parameters.FireDamages_Severity2)
                //    PlugIn.ModelCore.UI.WriteLine("      {0} : {1} : {2}", damage.DamageSpecies.Name, damage.MaxAge, damage.ProbablityMortality);

                //PlugIn.ModelCore.UI.WriteLine("   Fire mortality data for severity class 3:");
                //foreach (FireDamage damage in parameters.FireDamages_Severity3)
                //    PlugIn.ModelCore.UI.WriteLine("      {0} : {1} : {2}", damage.DamageSpecies.Name, damage.MaxAge, damage.ProbablityMortality);


                return parameters; 
        }
        //---------------------------------------------------------------------

        protected ISpecies GetSpecies(InputValue<string> name)
        {
            ISpecies species = PlugIn.ModelCore.Species[name.Actual];
            if (species == null)
                throw new InputValueException(name.String,
                                              "{0} is not a species name.",
                                              name.String);
            return species;
        }
        //---------------------------------------------------------------------

        public static IgnitionType IgnitionTypeParse(string word)
        {
            switch (word.ToLower())
            {
                case "prescribed": return IgnitionType.Rx;
                case "rx": return IgnitionType.Rx;
                case "lightning": return IgnitionType.Lightning;
                case "accidental": return IgnitionType.Accidental;
                default: throw new System.FormatException("Valid Ignition Types: Prescribed, Rx, Lightening, Accidental");
            }
        }

        //---------------------------------------------------------------------

        public static IgnitionDistribution IgnitionDistributionParse(string word)
        {
            switch (word.ToLower())
            {
                case "poisson": return IgnitionDistribution.Poisson;
                case "zeroinflatedpoisson": return IgnitionDistribution.ZeroInflatedPoisson;
                default: throw new System.FormatException("Valid Ignition Distributions: Poisson, ZeroInflatedPoisson");
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Registers the appropriate method for reading input values.
        /// </summary>
        public static void RegisterForInputValues()
        {

            InputValues.Register<IgnitionDistribution>(IgnitionDistributionParse);
        }
        //---------------------------------------------------------------------

        private void ValidatePath(InputValue<string> path)
        {
            if (path.Actual.Trim(null) == "")
                throw new InputValueException(path.String,
                                              "Invalid file path: {0}",
                                              path.String);
        }

        //---------------------------------------------------------------------
        private ISpecies ReadSpecies(string speciesName)
        {
            ISpecies species = speciesDataset[speciesName];
            if (species == null)
                throw new InputValueException(speciesName,
                                              "{0} is not a species name.",
                                              speciesName);
            int lineNumber;
            if (speciesLineNums.TryGetValue(species.Name, out lineNumber))
                throw new InputValueException(speciesName,
                                              "The species {0} was previously used on line {1}",
                                              speciesName, lineNumber);
            else
                speciesLineNums[species.Name] = LineNumber;
            return species;
        }

    }
}
