using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using Msr.Mlas.SpecialFunctions; 
 
namespace VirusCount
{ 
 	abstract public class AASimilarity
	{
		public string Name;
		static public AASimilarity GetInstance(string similarity)
		{
			if (similarity == "Eq") 
 			{ 
 				EqClassDefinitions aEqClassDefinitions = new EqClassDefinitions();
				aEqClassDefinitions.Name = similarity; 
 				aEqClassDefinitions.EqClassCollection = EqClassDefinitions.GetEqClassCollection();
				return aEqClassDefinitions;
			}
			else
			{
				HowConsevered howConsevered; 
 				if (similarity == "Con") 
 				{
					howConsevered = HowConsevered.Conserved; 
 				}
				else
				{
					Debug.Assert(similarity == "Semi");
					howConsevered = HowConsevered.SemiConserved;
				} 
 				AASimilarity aaSimilarity = TangriEtAl.GetInstance(howConsevered); 
 				return aaSimilarity;
			} 
 		}
		abstract public string CanComeFromSet(char c);
        abstract public string CanGoToSet(char c);
    }

	public class EqClassDefinitions : AASimilarity 
	{ 
		internal Hashtable EqClassCollection;
		override public string CanComeFromSet(char c) 
 		{
 			SpecialFunctions.CheckCondition(char.IsLetter(c) && char.IsUpper(c)); //!!!raise error
			string eqClassString = (string) EqClassCollection[c];
 			Debug.Assert(eqClassString.Length > 1); // real assert
			return eqClassString;
 
		} 

        override public string CanGoToSet(char c) 
        {
            return CanComeFromSet(c);
        }


		static internal Hashtable GetEqClassCollection() 
		{ 

			SortedList rgClassToOneLetterAA = new SortedList(); 
            foreach (string sThreeLetter in Biology.GetInstance().AminoAcidEquivalence.Keys)
 			{
                char cAminoAcid = Biology.GetInstance().ThreeLetterAminoAcidAbbrevTo1Letter[sThreeLetter];
                string sClass = Biology.GetInstance().AminoAcidEquivalence[sThreeLetter];
 				if (!rgClassToOneLetterAA.ContainsKey(sClass))
				{ 
 					rgClassToOneLetterAA.Add(sClass, new StringBuilder()); 
				}
				StringBuilder aaList = (StringBuilder) rgClassToOneLetterAA[sClass]; 
				aaList.Append(cAminoAcid);
			}

			Hashtable eqClassCollection = new Hashtable();
 			foreach(StringBuilder sb in rgClassToOneLetterAA.Values)
 			{ 
				string s = sb.ToString(); 
 				foreach(char c in s)
				{ 
					eqClassCollection.Add(c, s);
				}
			}
			return eqClassCollection;
 		}
 
 	} 
				
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
