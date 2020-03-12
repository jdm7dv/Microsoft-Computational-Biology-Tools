using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary; 
using System.Reflection; 
using System.Collections.Generic;
using Msr.Mlas.SpecialFunctions; 

namespace VirusCount
{
 	/// <summary>
	/// Summary description for Biology.
	/// </summary> 
	[Serializable()] 
	public class Biology
	{ 
 		public Dictionary<string,GeneticCodeMapping> CodonToAminoAcid;
 		public Dictionary<char,string> Ambiguous1LetterNucCodeToChoices;
		public Dictionary<char,string> OneLetterAminoAcidAbbrevTo3Letter;
 		public Dictionary<string,char> ThreeLetterAminoAcidAbbrevTo1Letter;
		public Dictionary<string,bool> AminoAcidCollection;
		public Dictionary<char,bool> Unambiguous1LetterNucCodes; 
		public Dictionary<string,string> AminoAcidEquivalence; 
		public Dictionary<string,int> AminoAcidEquivalenceToIndex;
 
        static public Biology GetInstance()
        {
            if (Singleton == null)
            {
                Singleton = new Biology();
            } 
            return Singleton; 
        }
 
        static Biology Singleton = null;

		private Biology()
 		{
 			ReadTheGeneticCode();
			ReadAmbiguousCode(); 
 			ReadOneLetterAminoAcidAbbrev(); 

            Unambiguous1LetterNucCodes = new Dictionary<char, bool>(); 
			Unambiguous1LetterNucCodes.Add('T', true);
            Unambiguous1LetterNucCodes.Add('A', true);
            Unambiguous1LetterNucCodes.Add('G', true);
            Unambiguous1LetterNucCodes.Add('C', true);
		}
 
        private void ReadAmbiguousCode() 
        {
            SpecialFunctions.CheckCondition(Ambiguous1LetterNucCodeToChoices == null); //!!!raise error 
            Ambiguous1LetterNucCodeToChoices = new Dictionary<char,string>();

            string sLine = null;
            using (StreamReader streamreaderAmbigFile = OpenResource("AmbiguousCodes.txt"))
            {
                while (null != (sLine = streamreaderAmbigFile.ReadLine())) 
                { 
                    string[] rgFields = sLine.Split('\t');
                    SpecialFunctions.CheckCondition(rgFields.Length == 2); //!!!raise error 
                    SpecialFunctions.CheckCondition(rgFields[0].Length == 1); //!!!raise error
                    char cAmbiguousCode = rgFields[0][0];
                    Ambiguous1LetterNucCodeToChoices.Add(cAmbiguousCode, rgFields[1]);
                }
            }
        } 
 
		/// <summary>
		/// Read table from http://users.rcn.com/jkimball.ma.ultranet/BiologyPages/C/Codons.html 
		/// </summary>
 		private void ReadTheGeneticCode()
 		{
			Debug.Assert(CodonToAminoAcid == null); // real assert
 			CodonToAminoAcid = new Dictionary<string,GeneticCodeMapping>();
			AminoAcidCollection = new Dictionary<string,bool>(); 
 
			//!!!could load these into memory instead of reading them over and over again
			string sTheGenenicCodeFile = @"GeneticCodeDna.txt"; //!!!const 
            using (StreamReader streamreaderTheGenenicCodeFile = OpenResource(sTheGenenicCodeFile))
			{
				string sLine = null;
 				while (null != (sLine = streamreaderTheGenenicCodeFile.ReadLine()))
 				{
					string[] rgFields = sLine.Split('\t'); 
 					SpecialFunctions.CheckCondition(rgFields.Length == 3); //!!!raise error 
					string sCodon = rgFields[0];
					SpecialFunctions.CheckCondition(sCodon.Length == 3); //!!!raise error 
					string sAminoAcid = rgFields[1];
					bool bNormal = bool.Parse(rgFields[2]); //!!!could raise error
					GeneticCodeMapping aGeneticCodeMapping = new GeneticCodeMapping(sCodon, sAminoAcid, bNormal);
 					CodonToAminoAcid.Add(sCodon, aGeneticCodeMapping);

 					if (!AminoAcidCollection.ContainsKey(sAminoAcid)) 
					{ 
 						AminoAcidCollection.Add(sAminoAcid, true);
					} 
				}
			}
		}


        //!!!this code is repeated in PhyloTree and Biology 
		private void ReadOneLetterAminoAcidAbbrev() 
 		{
 			Debug.Assert(OneLetterAminoAcidAbbrevTo3Letter == null); // real assert 
			Debug.Assert(ThreeLetterAminoAcidAbbrevTo1Letter == null);
            OneLetterAminoAcidAbbrevTo3Letter = new Dictionary<char, string>();
            ThreeLetterAminoAcidAbbrevTo1Letter = new Dictionary<string, char>();

 			string sInputFile = @"OneLetterAAAbrev.txt"; //!!!const
            using (StreamReader streamreaderInputFile = OpenResource(sInputFile)) 
			{ 
				string sLine = null;
				while(null != (sLine = streamreaderInputFile.ReadLine())) 
				{
					string[] rgFields = sLine.Split(' ');
 					SpecialFunctions.CheckCondition(rgFields.Length == 2); //!!!raise error
 					string sOneLetter = rgFields[0];
					SpecialFunctions.CheckCondition(sOneLetter.Length == 1); //!!!raise error
 					string sAminoAcid = rgFields[1]; 
					OneLetterAminoAcidAbbrevTo3Letter.Add(sOneLetter[0], sAminoAcid); 
					ThreeLetterAminoAcidAbbrevTo1Letter.Add(sAminoAcid, sOneLetter[0]);
				} 
			}
		}

 		//!!!this could be made faster
 		public bool KnownAminoAcid(string aminoAcid)
		{ 
 			bool b = AminoAcidCollection.ContainsKey(aminoAcid); 
			return b;
		} 
        //!!!could be faster and more data driver
        public bool KnownAminoAcid(char aminoAcid)
        {
            switch (aminoAcid)
            {
                case 'B': 
                case 'X': 
                case 'Z':
                case '*': 
                case '-':
                    return false;

                default:
                    bool b = OneLetterAminoAcidAbbrevTo3Letter.ContainsKey(aminoAcid);
                    return b; 
            } 
        }
 

        /// <summary>
        /// '-' means delete and must be part of "---"
        /// ' ' means missing and is treated just like 'N'
        /// null means that it could be anything
        /// </summary> 
        public SimpleAminoAcidSet GenticCode(int ifCodeHasBeenReviewedAndWorksOKWithDeleteAndSpaceAndDashAsMissingSetTo22, string codon, bool dashAsMissing) 
        {
            SpecialFunctions.CheckCondition(ifCodeHasBeenReviewedAndWorksOKWithDeleteAndSpaceAndDashAsMissingSetTo22 == 22, "Need to review code to be sure that it is OK to treat '---' as the amino acid DELETE"); 

            if (codon.Contains("X") || codon.Contains("*"))
            {
                return null;
            }
            if (codon.Contains("-")) 
            { 
                if (dashAsMissing)
                { 
                    return null;
                }
                else
                {
                    codon = "---";
                } 
            } 
            //if (codon.Contains(" "))
            //{ 
            //    if (1==1)
            //    {
            //        Console.WriteLine("Warning: changing codon '{0}' to aa '?'", codon);
            //        return null;
            //    }
            //    string codon2 = codon.Replace(' ', 'N'); 
            //    Console.WriteLine("Warning: changing codon '{0}' to '{1}'", codon, codon2); 
            //    codon = codon2;
            //} 

			//Get the set of values
            SimpleAminoAcidSet aminoAcidCollection = SimpleAminoAcidSet.GetInstance();
			GenticCode(codon, ref aminoAcidCollection);

            if (aminoAcidCollection.PositiveCount == 21) 
            { 
                return null;
            } 
			return aminoAcidCollection;
 		}

 		public bool CanResolveToAminoAcidOrAcids(string codon)
		{
 			SpecialFunctions.CheckCondition(codon.Length == 3); //!!!raise error 
            if (codon == "---") 
            {
                return true; 
            }
			for (int i = 0; i < codon.Length; ++i)
			{
				char c = codon[i];
				bool b = (Ambiguous1LetterNucCodeToChoices.ContainsKey(c) || Unambiguous1LetterNucCodes.ContainsKey(c));
				if (!b) 
 				{ 
 					return false;
				} 
 			}
			return true;
		}

        internal static StreamReader OpenResource(string fileName)
        { 
            return SpecialFunctions.OpenResource(Assembly.GetExecutingAssembly(), "Biology.DataFiles.", fileName); //!!!const 
        }
 
        public void GenticCode(string codon, ref SimpleAminoAcidSet aminoAcidCollection)
		{
			SpecialFunctions.CheckCondition(codon.Length == 3); //!!!raise error
			Debug.Assert(aminoAcidCollection != null); //real assert

 			//If unambiguous, look it up and return it 
 			if (CodonToAminoAcid.ContainsKey(codon)) 
			{
 				GeneticCodeMapping aGeneticCodeMapping = (GeneticCodeMapping)CodonToAminoAcid[codon]; 
				string sAminoAcid = aGeneticCodeMapping.AminoAcid;
				aminoAcidCollection.AddOrCheck(sAminoAcid);
				return;
			}

			//If ambiguous, try every possiblity for this 1st ambiguity and see if the results are the same (this is recursive) 
 
 			int iFirstAmbiguity = 0;
 			char c = char.MinValue; 
			for (;iFirstAmbiguity < codon.Length; ++iFirstAmbiguity)
 			{
				c = codon[iFirstAmbiguity];
				if (Ambiguous1LetterNucCodeToChoices.ContainsKey(c))
				{
					break; 
				} 
                SpecialFunctions.CheckCondition("ATCG".Contains(c.ToString()), string.Format("Illegal nucleotide of '{0}'", c));
 			} 
 			SpecialFunctions.CheckCondition(iFirstAmbiguity < codon.Length); //!!!raise error - Is CodonToAminoAcid table missing a value?

			foreach (char cNucleotide in (string) Ambiguous1LetterNucCodeToChoices[c])
 			{
				string sNewCodon = string.Format("{0}{1}{2}", codon.Substring(0,iFirstAmbiguity),cNucleotide,codon.Substring(iFirstAmbiguity+1));
				Debug.Assert(sNewCodon.Length == 3); //real assert 
				Debug.Assert(sNewCodon != codon); // real assert 
				GenticCode(sNewCodon, ref aminoAcidCollection);
			} 
 		}

 		public string GenticCode(string codon, out bool bAcidOrStop)
		{
 			SpecialFunctions.CheckCondition(codon.Length == 3); //!!!raise error
 
			//If unambiguous, look it up and return it 
			if (CodonToAminoAcid.ContainsKey(codon))
			{ 
				GeneticCodeMapping aGeneticCodeMapping = (GeneticCodeMapping)CodonToAminoAcid[codon];
				string sAminoAcid = aGeneticCodeMapping.AminoAcid;
 				bAcidOrStop = true;
 				return sAminoAcid;
			}
 
 			//If ambiguous, try every possiblity for this 1st ambiguity and see if the results are the same (this is recursive) 
			for (int i = 0; i < codon.Length; ++i)
			{ 
				char c = codon[i];
				if (Ambiguous1LetterNucCodeToChoices.ContainsKey(c))
				{
 					string sAminoAcidAll = null;
 					bAcidOrStop = false;
					foreach (char cNucleotide in (string) Ambiguous1LetterNucCodeToChoices[c]) 
 					{ 
						string sNewCodon = string.Format("{0}{1}{2}", codon.Substring(0,i),cNucleotide,codon.Substring(i+1));
						Debug.Assert(sNewCodon.Length == 3); //real assert 
						Debug.Assert(sNewCodon != codon); // real assert
						string sAminoAcid = GenticCode(sNewCodon, out bAcidOrStop);
						if (!bAcidOrStop)
 						{
 							goto CantFixAmbiguity;
						} 
 						if (sAminoAcidAll == null) 
						{
							sAminoAcidAll = sAminoAcid; 
						}
						else
						{
 							if (sAminoAcidAll != sAminoAcid)
 							{
								//goto CantFixAmbiguity; 
 								sAminoAcidAll = "Ambiguous Amino Acid"; //!!!const 
							}
						} 
					}
					Debug.Assert(sAminoAcidAll != null); //!!!???
					return sAminoAcidAll;
 				}
 			}
 
			CantFixAmbiguity: 

 				bAcidOrStop = false; 
			return "Not an amino acid: '" + codon +"'";
		}



 
 

		public string ConvertToOneLetterSequence(string sSequence3Letter) 
		{
			string[] threeLetterArray = sSequence3Letter.Split(',');
 			return ConvertToOneLetterSequence(threeLetterArray);
 		}

		public string ConvertToOneLetterSequence(string[] threeLetterArray) 
 		{ 
			StringBuilder aStringBuilder = new StringBuilder(threeLetterArray.Length, threeLetterArray.Length);
			foreach(string threeLetterAA in threeLetterArray) 
			{
				char c = (char) ThreeLetterAminoAcidAbbrevTo1Letter[threeLetterAA];
				aStringBuilder.Append(c);
 			}

 			string sSequence1Letter = aStringBuilder.ToString(); 
			return sSequence1Letter; 
 		}
 


        public bool LegalPeptide(string peptide)
        {
            foreach (char c in peptide)
            { 
                if (!KnownAminoAcid(c)) 
                {
                    return false; 
                }
            }
            return true;
        }
    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
