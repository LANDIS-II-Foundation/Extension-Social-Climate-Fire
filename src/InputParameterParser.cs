//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System;

namespace Landis.Extension.Scrapple
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class InputParameterParser
        : TextParser<IInputParameters>
    {
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
            Edu.Wisc.Forest.Flel.Util.Percentage p = new Edu.Wisc.Forest.Flel.Util.Percentage();
            RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            ReadLandisDataVar();

            InputParameters parameters = new InputParameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            //InputVar<string> climateConfigFile = new InputVar<string>("ClimateConfigFile"); 
            //if (ReadOptionalVar(climateConfigFile))
            //{
            //    parameters.ClimateConfigFile = climateConfigFile.Value;
            //    PlugIn.ReadClimateLibrary = true;
            //}

            InputVar<double> rha = new InputVar<double>("RelativeHumiditySlopeAdjust");
            if (ReadOptionalVar(rha))
                parameters.RelativeHumiditySlopeAdjustment = rha.Value;
            else
                parameters.RelativeHumiditySlopeAdjustment = 1.0;




            const string GroundSlopeFile = "GroundSlopeFile";
            InputVar<string> groundSlopeFile = new InputVar<string>("GroundSlopeFile");
            ReadVar(groundSlopeFile);
            
                PlugIn.ModelCore.UI.WriteLine("   Loading Slope data...");
                
                Topography.ReadGroundSlopeMap(groundSlopeFile.Value);

                InputVar<string> uphillSlopeMap = new InputVar<string>("UphillSlopeAzimuthMap");
                ReadVar(uphillSlopeMap);

                PlugIn.ModelCore.UI.WriteLine("   Loading Azimuth data...");

                Topography.ReadUphillSlopeAzimuthMap(uphillSlopeMap.Value);

            
            //-------------------------------------------------------------------
            //  Read table of Fire Damage classes.
            //  Damages are in increasing order.
             PlugIn.ModelCore.UI.WriteLine("   Loading Fire Damage data...");
            
            ReadName(FireDamage);

            InputVar<Percentage> maxAge = new InputVar<Percentage>("Max Survival Age");
            InputVar<int> severTolerDifference = new InputVar<int>("Severity Tolerance Diff");

            const string MapNames = "MapNames";
            int previousNumber = -5;
            double previousMaxAge = 0.0;

            while (! AtEndOfInput && CurrentName != MapNames
                                  && previousNumber != 4) {
                StringReader currentLine = new StringReader(CurrentLine);

                IFireDamage damage = new FireDamage();
                parameters.FireDamages.Add(damage);

                ReadValue(maxAge, currentLine);
                damage.MaxAge = maxAge.Value;
                if (maxAge.Value.Actual <= 0)
                {
                //  Maximum age for damage must be > 0%
                    throw new InputValueException(maxAge.Value.String,
                                      "Must be > 0% for the all damage classes");
                }
                if (maxAge.Value.Actual > 1)
                {
                //  Maximum age for damage must be <= 100%
                    throw new InputValueException(maxAge.Value.String,
                                      "Must be <= 100% for the all damage classes");
                }
                //  Maximum age for every damage must be > 
                //  maximum age of previous damage.
                if (maxAge.Value.Actual <= previousMaxAge)
                {
                    throw new InputValueException(maxAge.Value.String,
                        "MaxAge must > the maximum age ({0}) of the preceeding damage class",
                        previousMaxAge);
                }

                previousMaxAge = (double) maxAge.Value.Actual;

                ReadValue(severTolerDifference, currentLine);
                damage.SeverTolerDifference = severTolerDifference.Value;

                //Check that the current damage number is > than
                //the previous number (numbers are must be in increasing
                //order).
                if (severTolerDifference.Value.Actual <= previousNumber)
                    throw new InputValueException(severTolerDifference.Value.String,
                                                  "Expected the damage number {0} to be greater than previous {1}",
                                                  damage.SeverTolerDifference, previousNumber);
                if (severTolerDifference.Value.Actual > 4)
                    throw new InputValueException(severTolerDifference.Value.String,
                                                  "Expected the damage number {0} to be less than 5",
                                                  damage.SeverTolerDifference);
                                                  
                previousNumber = severTolerDifference.Value.Actual;

                CheckNoDataAfter("the " + severTolerDifference.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            
            if (parameters.FireDamages.Count == 0)
                throw NewParseException("No damage classes defined.");

            InputVar<string> mapNames = new InputVar<string>(MapNames);
            ReadVar(mapNames);
            parameters.MapNamesTemplate = mapNames.Value;


            return parameters; //.GetComplete();
        }
        //---------------------------------------------------------------------

        public static Distribution DistParse(string word)
        {
            if (word == "gamma")
                return Distribution.gamma;
            else if (word == "lognormal")
                return Distribution.lognormal;
            else if (word == "normal")
                return Distribution.normal;
            else if (word == "Weibull")
                return Distribution.Weibull;
            throw new System.FormatException("Valid Distributions: gamma, lognormal, normal, Weibull");
        }


        //---------------------------------------------------------------------

        /// <summary>
        /// Registers the appropriate method for reading input values.
        /// </summary>
        public static void RegisterForInputValues()
        {

            Edu.Wisc.Forest.Flel.Util.Type.SetDescription<Distribution>("Random Number Distribution");
            InputValues.Register<Distribution>(DistParse);
        }
        //---------------------------------------------------------------------

        private void ValidatePath(InputValue<string> path)
        {
            if (path.Actual.Trim(null) == "")
                throw new InputValueException(path.String,
                                              "Invalid file path: {0}",
                                              path.String);
        }

        
    }
}
