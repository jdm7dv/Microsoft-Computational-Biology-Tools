using System; 
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using EpipredLib; 
 
namespace VirusCount
{ 
 	/// <summary>
	/// Summary description for AASimilarity.
	/// </summary>
	public class TangriEtAl : AASimilarity
	{
 
		static public TangriEtAl GetInstance(HowConsevered howConsevered) 
 		{
 			TangriEtAl aTangriEtAl = new TangriEtAl(); 
			aTangriEtAl.ReadFile();
 			aTangriEtAl.HowConsevered = howConsevered;

			if (howConsevered == HowConsevered.Conserved)
			{
				aTangriEtAl.Name = string.Format("Con"); 
				//A little unit testing 
				Debug.Assert(aTangriEtAl.CanGoToSet('G') == "GSATDP");
 			} 
 			else
			{
 				aTangriEtAl.Name = string.Format("Semi");
				//A little unit testing
				Debug.Assert(aTangriEtAl.CanComeFromSet('P') == "ACDEGHIKLMNPQRSTVWY");
			} 
 
			return aTangriEtAl;
		} 

 		private void ReadFile()
 		{

			for(HowConsevered howConsevered = HowConsevered.Conserved; howConsevered <= HowConsevered.SemiConserved; ++howConsevered)
 			{ 
				HowConseveredToForward[(int)howConsevered] = new SortedList(); 
				HowConseveredToBackward[(int)howConsevered] = new SortedList();
			} 



			string inputFileName = @"SimilarityOfAminoAcids.txt";

			SortedList rgHeadings = new SortedList(); 
            using (StreamReader streamreaderInputFile = Predictor.OpenResource(inputFileName)) 
 			{
 				string sLine; 
				char cPrevHeading = '\0';
 				while(null != (sLine = streamreaderInputFile.ReadLine()))
				{
					if (sLine.StartsWith("//"))
					{
						continue; 
					} 

 					//There must be a line for every amino acid and they must be in alpha order 
 					string[] tableParts = sLine.Split(' '); //!!!const
					SpecialFunctions.CheckCondition(tableParts.Length == 3); //!!!raise error
 					Debug.Assert(tableParts[0].Length > 0);
					char cHeading = tableParts[0][0];
					SpecialFunctions.CheckCondition(cPrevHeading < cHeading);//!!!raise error
					cPrevHeading = cHeading; 
					SpecialFunctions.CheckCondition(Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter.ContainsKey(cHeading)); //!!!raise error 
					SpecialFunctions.CheckCondition(!rgHeadings.ContainsKey(cHeading)); //!!!raise error
 					rgHeadings.Add(cHeading, null); 

 					//We must see every amino acid in the line;
					SortedList rgInLine = new SortedList();
 					foreach(string part in tableParts)
					{
						foreach(char cAA in part) 
						{ 
                            SpecialFunctions.CheckCondition(Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter.ContainsKey(cAA)); //!!!raise error
							SpecialFunctions.CheckCondition(!rgInLine.ContainsKey(cAA)); //!!!raise error 
							rgInLine.Add(cAA, null);
 						}
 					}
					SpecialFunctions.CheckCondition(rgInLine.Count == 20); //!!!raise error

 					foreach(char cAA in tableParts[(int) HowConsevered.Conserved]) 
					{ 
						AddPair(cHeading, cAA, HowConsevered.Conserved);
						AddPair(cHeading, cAA, HowConsevered.SemiConserved); 
					}

					foreach(char cAA in tableParts[(int) HowConsevered.SemiConserved])
 					{
 						AddPair(cHeading, cAA, HowConsevered.SemiConserved);
					} 
 
 				}
			} 
			SpecialFunctions.CheckCondition(rgHeadings.Count == 20);//!!!raise error

		}

		private SortedList[] HowConseveredToForward = new SortedList[2];
		private SortedList[] HowConseveredToBackward = new SortedList[2]; 
 		private HowConsevered HowConsevered; 

 		override public string CanComeFromSet(char aminoAcid) 
		{
 			SortedList backward = HowConseveredToBackward[(int) HowConsevered];
			StringBuilder backwardSB = (StringBuilder) backward[aminoAcid];
			return backwardSB.ToString();

		} 
 
		override public string CanGoToSet(char aminoAcid)
		{ 
 			SortedList forward = HowConseveredToForward[(int) HowConsevered];
 			StringBuilder forwardSB = (StringBuilder) forward[aminoAcid];
			return forwardSB.ToString();

 		}
 
 
		private void AddPair(char from, char to, HowConsevered howConsevered)
		{ 
			SortedList forward = HowConseveredToForward[(int)howConsevered];
			if (!forward.ContainsKey(from))
			{
 				forward.Add(from, new StringBuilder());
 			}
			StringBuilder forwardSB = (StringBuilder) forward[from]; 
 			forwardSB.Append(to); 

 
			SortedList backward = HowConseveredToBackward[(int)howConsevered];
			if (!backward.ContainsKey(to))
			{
				backward.Add(to, new StringBuilder());
			}
 			StringBuilder backwardSB = (StringBuilder) backward[to]; 
 			backwardSB.Append(from); 

 
		}

 		private TangriEtAl()
		{
			//
			// TODO: Add constructor logic here 
			// 
		}
 	} 

 	public enum HowConsevered
	{
 		Conserved = 0,
		SemiConserved = 1,
		NonConserved = 2 
	} 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
