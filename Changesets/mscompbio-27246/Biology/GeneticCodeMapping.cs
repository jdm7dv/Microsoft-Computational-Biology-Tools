using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Msr.Mlas.SpecialFunctions; 
 
namespace VirusCount
{ 
 	[Serializable()]
	public class GeneticCodeMapping
	{
		public GeneticCodeMapping(string codon, string aminoAcid, bool normal)
		{
			SpecialFunctions.CheckCondition(codon.Length == 3); //!!!raise error 
 			Codon = codon; 
 			AminoAcid = aminoAcid;
			Normal = normal; 
 		}
		public string Codon;
		public string AminoAcid;
		public bool Normal;
	}
	 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
