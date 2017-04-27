//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;
using System.Collections.Generic;

namespace Landis.Extension.Scrapple
{
	/// <summary>
	/// Methods for working with the template for map filenames.
	/// </summary>
	public static class MapNames
	{
		public const string TimestepVar = "timestep";

		private static IDictionary<string, bool> knownVars;
		private static IDictionary<string, string> varValues;

		//---------------------------------------------------------------------

		static MapNames()
		{
			knownVars = new Dictionary<string, bool>();
			knownVars[TimestepVar] = true;

			varValues = new Dictionary<string, string>();
		}

		//---------------------------------------------------------------------

		public static void CheckTemplateVars(string template)
		{
			OutputPath.CheckTemplateVars(template, knownVars);
		}

		//---------------------------------------------------------------------

		public static string ReplaceTemplateVars(string template,
		                                         int    timestep)
		{
			varValues[TimestepVar] = timestep.ToString();
			return OutputPath.ReplaceTemplateVars(template, varValues);
		}
	}
}
