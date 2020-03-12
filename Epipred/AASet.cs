using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic; 
using Msr.Mlas.SpecialFunctions; 

namespace VirusCount 
{
 	[Flags]
	public enum AASet : uint
	{
		Empty = 0,
		Ala = 1, 
		FirstAminoAcid = Ala, 
 		Cys = 2,
 		Asp = 4, 
		Glu = 8,
 		Phe = 16,
		Gly = 32,
		His = 64,
		Ile = 128,
		Lys = 256, 
		Leu = 512, 
 		Met = 1024,
 		Asn = 2048, 
		Pro = 2048 * 2,
 		Gln = 2048 * 4,
		Arg = 2048 * 8,
		Ser = 2048 * 16,
		Thr = 2048 * 32,
		Val = 2048 * 64, 
		Trp = 2048 * 128, 
 		Tyr = 2048 * 256,
 		LastAminoAcid = Tyr, 
		Optional = 2048 * 512,
 		OptionalEmpty = Optional,
		OptionalAny = Optional | Ala | Cys | Asp | Glu | Phe | Gly | His | Ile | Lys | Leu | Met | Asn | Pro | Gln | Arg | Ser | Thr | Val | Trp | Tyr
	}

	public class AASetSequence : List<AASet> 
	{ 
		public AASetSequence ReverseClone()
 		{ 
 			AASetSequence rc = AASetSequence.GetInstance();
			for(int i = this.Count - 1; i >= 0; --i)
 			{
				rc.Add(this[i]);
			}
			return rc; 
		} 

 
		static public AASet UnifyOrEmpty(AASet c1, AASet c2)
 		{
 			SpecialFunctions.CheckCondition(!AASetSequence.IsOptional(c1) && !AASetSequence.IsOptional(c2)); //!!!raise error
			AASet result = c1 & c2;
 			return result;
		} 
 
		static public AASetSequence GetInstance()
		{ 
			return new AASetSequence();
		}
 		private AASetSequence()
 		{
		}
 
 		static HybridDictionary OneLetterTable = CreateOneLetterTable(); 
		static Dictionary<char,AASet> InverseOneLetterTable = CreateInverseOneLetterTable();
        static Dictionary<char, AASet> CreateInverseOneLetterTable() 
		{
			HybridDictionary rg = CreateOneLetterTable();
            Dictionary<char, AASet> rgInv = new Dictionary<char, AASet>();
			foreach(AASet aaSet in rg.Keys)
			{
 				char ch = (char) rg[aaSet]; 
 				rgInv.Add(ch, aaSet); 
			}
 			return rgInv; 
		}

		static HybridDictionary CreateOneLetterTable()
		{
			HybridDictionary aHybridDictionary = new HybridDictionary();
			aHybridDictionary.Add(AASet.Ala, 'A'); 
 			aHybridDictionary.Add(AASet.Cys, 'C'); 
 			aHybridDictionary.Add(AASet.Asp, 'D');
			aHybridDictionary.Add(AASet.Glu, 'E'); 
 			aHybridDictionary.Add(AASet.Phe, 'F');
			aHybridDictionary.Add(AASet.Gly, 'G');
			aHybridDictionary.Add(AASet.His, 'H');
			aHybridDictionary.Add(AASet.Ile, 'I');
			aHybridDictionary.Add(AASet.Lys, 'K');
			aHybridDictionary.Add(AASet.Leu, 'L'); 
 			aHybridDictionary.Add(AASet.Met, 'M'); 
 			aHybridDictionary.Add(AASet.Asn, 'N');
			aHybridDictionary.Add(AASet.Pro, 'P'); 
 			aHybridDictionary.Add(AASet.Gln, 'Q');
			aHybridDictionary.Add(AASet.Arg, 'R');
			aHybridDictionary.Add(AASet.Ser, 'S');
			aHybridDictionary.Add(AASet.Thr, 'T');
			aHybridDictionary.Add(AASet.Val, 'V');
			aHybridDictionary.Add(AASet.Trp, 'W'); 
 			aHybridDictionary.Add(AASet.Tyr, 'Y'); 
 			return aHybridDictionary;
		} 

 		static public bool IsGround(AASet aaSetX)
		{
			for(AASet aaSet = AASet.FirstAminoAcid; aaSet <= AASet.LastAminoAcid; aaSet=(AASet)((int)aaSet*2))
			{
				if ((aaSetX & aaSet) != AASet.Empty) 
				{ 
 					return true;
 				} 
			}
 			return false;
		}

		public override string ToString()
		{ 
			StringBuilder sb = new StringBuilder(); 
			foreach(AASet aaSet in this)
 			{ 
 				sb.Append(ToString(aaSet));
			}
 			return sb.ToString();
		}

 
		static public string ToString(AASet aaSetX) 
		{
			SpecialFunctions.CheckCondition(aaSetX != AASet.Empty && aaSetX != AASet.OptionalEmpty); //!!!raise error - can't print nothing 
			if (aaSetX ==AASet.OptionalAny)
 			{
 				return ".";
			}

 			StringBuilder aStringBuilder = new StringBuilder(); 
			if (IsRequired(aaSetX)) 
			{
				for(AASet aaSet = AASet.FirstAminoAcid; aaSet <= AASet.LastAminoAcid; aaSet=(AASet)((int)aaSet*2)) 
				{
					if ((aaSetX & aaSet) != AASet.Empty)
 					{
 						aStringBuilder.Append(OneLetterTable[aaSet]);
					}
 				} 
				if (aStringBuilder.Length == 1) 
				{
					return aStringBuilder.ToString(); 
				}
				else
 				{
 					return string.Format("[{0}]", aStringBuilder);
				}
 			} 
			else 
			{
				for(AASet aaSet = AASet.FirstAminoAcid; aaSet <= AASet.LastAminoAcid; aaSet=(AASet)((int)aaSet*2)) 
				{
					if ((aaSetX & aaSet) != AASet.Empty)
 					{
 						aStringBuilder.Append(char.ToLower((char) OneLetterTable[aaSet]));
					}
 				} 
				Debug.Assert(aStringBuilder.Length == 1); // real assert 
			}
			Debug.Assert(aStringBuilder.Length > 0); // real assert 
			return aStringBuilder.ToString();
		}

 		public string ToRegexString()
 		{
			StringBuilder sb = new StringBuilder(); 
 			foreach(AASet aaSet in this) 
			{
				sb.Append(ToRegexString(aaSet)); 
			}
			return sb.ToString();
		}

 		static public string ToRegexString(AASet aaSetX)
 		{ 
			SpecialFunctions.CheckCondition(aaSetX != AASet.Empty && aaSetX != AASet.OptionalEmpty); //!!!raise error - can't print nothing 
 			if (aaSetX ==AASet.OptionalAny)
			{ 
				return ".";
			}

			StringBuilder aStringBuilder = new StringBuilder();
			if (IsRequired(aaSetX))
 			{ 
 				for(AASet aaSet = AASet.FirstAminoAcid; aaSet <= AASet.LastAminoAcid; aaSet=(AASet)((int)aaSet*2)) 
				{
 					if ((aaSetX & aaSet) != AASet.Empty) 
					{
						aStringBuilder.Append(OneLetterTable[aaSet]);
					}
				}
				if (aStringBuilder.Length == 1)
 				{ 
 					return aStringBuilder.ToString(); 
				}
 				else 
				{
					return string.Format("[{0}]", aStringBuilder);
				}
			}
			else
 			{ 
 				aStringBuilder.Append("[_"); 
				for(AASet aaSet = AASet.FirstAminoAcid; aaSet <= AASet.LastAminoAcid; aaSet=(AASet)((int)aaSet*2))
 				{ 
					if ((aaSetX & aaSet) != AASet.Empty)
					{
						aStringBuilder.Append((char) OneLetterTable[aaSet]);
					}
				}
 				Debug.Assert(aStringBuilder.Length == 3); // real assert 
 				aStringBuilder.Append("]"); 
			}
 			Debug.Assert(aStringBuilder.Length > 0); // real assert 
			return aStringBuilder.ToString();
		}



		static public AASetSequence GetInstance(AASetSequence aAASetSequence) 
		{ 
			AASetSequence r = AASetSequence.GetInstance();
 			foreach(AASet aaSet in aAASetSequence) 
 			{
				r.Add(aaSet);
 			}
			return r;
		}
 
//		private AASetSequence(int size) : base(size) 
//		{
//			for(int i = 0; i < size; ++i) 
//			{
//				Add(null);
//			}
//		}
		public static AASetSequence Concatenate(params AASetSequence[] aaSetSequenceParams)
		{ 
			AASetSequence result = AASetSequence.GetInstance(); 
 			foreach(AASetSequence aAASetSequence in aaSetSequenceParams)
 			{ 
				result.Append(aAASetSequence);
 			}
			return result;
		}

		public static bool IsOptional(AASet aaSet) 
		{ 
			bool b = (aaSet & AASet.Optional) != AASet.Empty;
 			return b; 
 		}

		public static bool IsRequired(AASet aaSet)
 		{
			bool b = (aaSet & AASet.Optional) == AASet.Empty;
			return b; 
		} 

		public static AASet ToRequired(AASet aaSet) 
		{
 			AASet result = (aaSet & (~AASet.Optional));
 			return result;
		}

 		public static AASet ToOptional(AASet aaSet) 
		{ 
			AASet result = (aaSet | AASet.Optional);
			return result; 
		}

		public void Append(AASet aaSet)
 		{
 			Add(aaSet);
		} 
 		public void Append(AASetSequence aAASetSequence) 
		{
			AddRange(aAASetSequence); 
		}
		public void AppendSubsequence(AASetSequence from, int start, int length)
		{
 			for(int i = start; i < start + length; ++i)
 			{
				Append(from[i]); 
 			} 
		}
		public AASetSequence Subsequence(int start, int length) 
		{
			AASetSequence aAASetSequence = AASetSequence.GetInstance();
			for(int i = start; i < start + length; ++i)
 			{
 				aAASetSequence.Append(this[i]);
			} 
 			return aAASetSequence; 
		}
		public AASetSequence Subsequence(int start) 
		{
			AASetSequence aAASetSequence = AASetSequence.GetInstance();
			for(int i = start; i < Count; ++i)
 			{
 				aAASetSequence.Append(this[i]);
			} 
 			return aAASetSequence; 
		}
 
		public AASet AppendGround(char ch)
		{
			AASet aaSet = InverseOneLetterTable[ch];
			Append(aaSet);
 			return aaSet;
 		} 
 

		public AASet AppendGroundOrEdge(char ch) 
 		{
			AASet aaSet;
			if (ch == '.')
			{
				aaSet = AASet.OptionalAny;
			} 
 			else 
 			{
				SpecialFunctions.CheckCondition(char.IsLetter(ch) && !char.IsUpper(ch)); //!!!raise error 
 				aaSet = AASet.Optional | ((AASet) InverseOneLetterTable[char.ToUpper(ch)]);
			}

			Append(aaSet);
			return aaSet;
		} 
 
		public AASet AppendGroundSet(string s)
 		{ 
 			//SpecialFunctions.CheckCondition(s.Length > 1); //!!!raise error
			AASet aaSetTotal = AASet.Empty;
 			foreach(char ch in s)
			{
				AASet aaSet = InverseOneLetterTable[ch];
				Debug.Assert((aaSetTotal & aaSet) == AASet.Empty); //Assert that haven't see this amino acid before in the set 
				aaSetTotal |= aaSet; 
			}
 			Append(aaSetTotal); 
 			return aaSetTotal;
		}

 		static public char ToFlankingRealizationChar(AASet aaSetX)
		{
			Debug.Assert(!IsRequired(aaSetX)); 
			for(AASet aaSet = AASet.FirstAminoAcid; aaSet <= AASet.LastAminoAcid; aaSet=(AASet)((int)aaSet*2)) 
			{
				if ((aaSetX & aaSet) != AASet.Empty) 
 				{
 					return (char) OneLetterTable[aaSet];
				}
 			}
			SpecialFunctions.CheckCondition(false, "Empty set not allowed"); //!!!raise error
			return char.MinValue; 
		} 

 
		static public char ToCoreRealizationChar(AASet aaSetX)
		{
 			Debug.Assert(IsRequired(aaSetX));
 			for(AASet aaSet = AASet.FirstAminoAcid; aaSet <= AASet.LastAminoAcid; aaSet=(AASet)((int)aaSet*2))
			{
 				if ((aaSetX & aaSet) != AASet.Empty) 
				{ 
					return (char) OneLetterTable[aaSet];
				} 
			}
			SpecialFunctions.CheckCondition(false,"Empty set not allowed"); //!!!raise error
 			return char.MinValue;
 		}
	}
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
