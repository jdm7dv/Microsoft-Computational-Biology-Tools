using System; 
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;

namespace Msr.Adapt.LearningWorkbench 
{ 
 	public class Csv
	{ 
		public Csv()
		{
		}

		public static string[,] Parse(Stream aStream)
 		{ 
 			ArrayList rgOutput = new ArrayList(); 
			TextReader aTextReader = new StreamReader(aStream);
 			string sLine; 
			while ((sLine = aTextReader.ReadLine()) != null)
			{
				//TODO this doesn't work if there are commas in the string
				string[] rgsParts = sLine.Split(',');
				//TODO remove the dependence on 3 columns, but what rows with differernt numbers of columns?
 				Debug.Assert(rgsParts.Length==3,"Expected exactly 3 comma-delimited fields (is there a comma in the quotes?)"); //TODO raise an error 
 				rgOutput.Add(rgsParts); 
			}
 
 			string[,] rgrgOut = new string[3,rgOutput.Count];
			for (int iRow = 0; iRow < rgOutput.Count; ++iRow)
			{
				string[]rgsParts = (string[])rgOutput[iRow];
				for (int iCol = 0; iCol < 3; ++iCol)
				{ 
 					rgrgOut[iRow,iCol] = RemoveQuotes(rgsParts[iCol]); 
 				}
			} 

 			return rgrgOut;
		}


		private static string RemoveQuotes(string s) 
		{ 
			StringBuilder aSB = new StringBuilder(s);
 
			if (aSB.Length > 0 && aSB[0] == '"')
 			{
 				aSB.Remove(0,1);
			}
 			if (aSB.Length > 0 && aSB[aSB.Length - 1] == '"')
			{ 
				aSB.Remove(aSB.Length - 1,1); 
			}
			aSB.Replace("\"\"","\""); 
			return aSB.ToString();			
 		}

 	}
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
