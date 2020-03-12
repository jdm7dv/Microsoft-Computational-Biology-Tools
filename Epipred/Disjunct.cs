using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization; 
using System.Runtime.Serialization.Formatters.Binary; 
using Msr.Mlas.SpecialFunctions;
 
namespace VirusCount
{
 	public class Disjunct
	{
		public AASetSequence Before;
		public AASetSequence Core; 
		public AASetSequence After; 
		public string CoreRealization;
 		public string FullRealization; 
 		public string FullAsString;
		public AASetSequence FullAsAASetSequence;
 		public string RegexString;

		
		private Disjunct() 
		{ 
		}
 
		static public Disjunct GetInstance(string expression, PatchRegexFactory patchRegexFactory)
 		{
 			Disjunct disjunct = new Disjunct();
			disjunct.FullAsString = expression;
 			disjunct.PatchRegexFactory = patchRegexFactory;
			disjunct.SetLeftCoreRightRealizationAbstractionAndRegex(); 
			return disjunct; 
		}
 
		private PatchRegexFactory PatchRegexFactory;


		public bool IsLessGeneralThan(Disjunct other)
 		{
 			SpecialFunctions.CheckCondition(this != other); //!!! raise error 
			SpecialFunctions.CheckCondition(FullAsString != other.FullAsString); //!!! raise error 

 			bool falseForSure; 
			bool bAtLeastOneCharOfOtherIsMoreGeneral = false;
			CoreIsLessGeneralThan(other, out falseForSure, ref bAtLeastOneCharOfOtherIsMoreGeneral);
			if (falseForSure)
			{
				return false;
 			} 
 			 
			BeforeOrAfterIsLessGeneralThan(Before, other.Before, out falseForSure, ref bAtLeastOneCharOfOtherIsMoreGeneral);
 			if (falseForSure) 
			{
				return false;
			}
			
			BeforeOrAfterIsLessGeneralThan(After, other.After, out falseForSure, ref bAtLeastOneCharOfOtherIsMoreGeneral);
 			if (falseForSure) 
 			{ 
				return false;
 			}; 

			return bAtLeastOneCharOfOtherIsMoreGeneral;
		}

		private void CoreIsLessGeneralThan(Disjunct other, out bool falseForSure, ref bool bAtLeastOneCharOfOtherIsMoreGeneral)
		{ 
 
			SpecialFunctions.CheckCondition(Core.Count == other.Core.Count); //!!! raise error
 			for(int i = 0; i < Core.Count; ++i) 
 			{
				AASet cMe = Core[i];
 				AASet cOther = other.Core[i];
				if (CoreIsMoreGeneralThan(cMe, cOther))
				{
					falseForSure = true; 
 
					return;
				} 
 				else if (CoreIsMoreGeneralThan(cOther, cMe))
 				{
					bAtLeastOneCharOfOtherIsMoreGeneral = true;
 				}
			}
 
			falseForSure = false; 
			return;
		} 

		static private void BeforeOrAfterIsLessGeneralThan(AASetSequence flanking1, AASetSequence flanking2,out bool falseForSure, ref bool bAtLeastOneCharOfOtherIsMoreGeneral)
 		{
 			if (flanking1.Count > flanking2.Count)
			{
 				falseForSure = true; 
				return; 
			}
 
			if (flanking2.Count > flanking1.Count)
			{
				bAtLeastOneCharOfOtherIsMoreGeneral = true;
 			}
 			for(int i = 0; i < flanking1.Count; ++i)
			{ 
 				AASet c1 = flanking1[i]; 
				AASet c2 = flanking2[i];
				if (BeforeOrAfterIsMoreGeneralThan(c1, c2)) 
				{
					falseForSure = true;
					return;
 				}
 				else if (BeforeOrAfterIsMoreGeneralThan(c2, c1))
				{ 
 					bAtLeastOneCharOfOtherIsMoreGeneral = true; 
				}
			} 

			falseForSure = false;
			return;
		}

 
//		public bool IsLessGeneralThan(Disjunct other) 
//		{
//			SpecialFunctions.CheckCondition(this != other); //!!! raise error 
//			SpecialFunctions.CheckCondition(FullAsString != other.FullAsString); //!!! raise error
//
//			bool b = CoreIsLessGeneralThan(other) && BeforeOrAfterIsLessGeneralThan(Before, other.Before) && BeforeOrAfterIsLessGeneralThan(After, other.After);
//			return b;
//		}
 
//		private bool CoreIsLessGeneralThan(Disjunct other) 
//		{
//			SpecialFunctions.CheckCondition(Core.Count == other.Core.Count); //!!! raise error 
//			bool bAtLeastOneCharOfOtherIsMoreGeneral = false;
//			for(int i = 0; i < Core.Count; ++i)
//			{
//				AASet cMe = Core[i];
//				AASet cOther = other.Core[i];
//				if (CoreIsMoreGeneralThan(cMe, cOther)) 
//				{ 
//					return false;
//				} 
//				else if (CoreIsMoreGeneralThan(cOther, cMe))
//				{
//					bAtLeastOneCharOfOtherIsMoreGeneral = true;
//				}
//			}
//			return bAtLeastOneCharOfOtherIsMoreGeneral; 
//		} 

//		static private bool BeforeOrAfterIsLessGeneralThan(AASetSequence flanking1, AASetSequence flanking2) 
//		{
//			if (flanking1.Count > flanking2.Count)
//			{
//				return false;
//			}
// 
//			bool bAtLeastOneCharOfOtherIsMoreGeneral = (flanking2.Count > flanking1.Count); 
//			for(int i = 0; i < flanking1.Count; ++i)
//			{ 
//				AASet c1 = flanking1[i];
//				AASet c2 = flanking2[i];
//				if (BeforeOrAfterIsMoreGeneralThan(c1, c2))
//				{
//					return false;
//				} 
//				else if (BeforeOrAfterIsMoreGeneralThan(c2, c1)) 
//				{
//					bAtLeastOneCharOfOtherIsMoreGeneral = true; 
//				}
//			}
//			return bAtLeastOneCharOfOtherIsMoreGeneral;
//		}

 
 
 		static private bool BeforeOrAfterIsMoreGeneralThan(AASet c1, AASet c2)
 		{ 
			SpecialFunctions.CheckCondition(!IsCore(c1) && !IsCore(c2)); //!!!raise error
 			bool b  = (c1 == AASet.OptionalAny && c2 != AASet.OptionalAny);
			return b;
		}

 
		private bool CoreIsMoreGeneralThan(AASet c1, AASet c2) 
		{
			SpecialFunctions.CheckCondition(IsCore(c1) && IsCore(c2)); //!!!raise error 
 			bool b = ((c1 & (~c2)) != AASet.Empty);
 			return b;
		}

 		static private bool IsCore(AASet c)
		{ 
			bool b = AASetSequence.IsRequired(c); 
			return b;
		} 


		private void SetLeftCoreRightRealizationAbstractionAndRegex()
 		{
 			Before = AASetSequence.GetInstance();
			Core = AASetSequence.GetInstance(); 
 			After = AASetSequence.GetInstance(); 

 
			StringBuilder sbCoreRealization = new StringBuilder();
			StringBuilder sbFullRealization = new StringBuilder();
			StringBuilder sbRegex = new StringBuilder();

			CharEnumerator charEnumerator = FullAsString.GetEnumerator();
 
			// Read 1st char 
 			bool bOK = charEnumerator.MoveNext();
 			char ch = char.MinValue; 
			if (bOK)
 			{
				ch = charEnumerator.Current;
			}

			// Read before part 
			while(bOK) 
			{
 				if (!(ch == '.' || (char.IsLetter(ch) && !char.IsUpper(ch)))) 
 				{
					break;
 				}
				AASet aaSet = Before.AppendGroundOrEdge(ch);
				sbRegex.Append(AASetSequence.ToRegexString(aaSet));
 
				char fullRealizationChar = AASetSequence.ToFlankingRealizationChar(aaSet); 
				sbFullRealization.Append(fullRealizationChar);
 
				bOK = charEnumerator.MoveNext();
 				if (bOK)
 				{
					ch = charEnumerator.Current;
 				}
			} 
 
			// Read main part
			while(bOK) 
			{
				if (ch == '.' || (char.IsLetter(ch) && !char.IsUpper(ch)))
 				{
 					break;
				}
 
 				if (char.IsLetter(ch) && char.IsUpper(ch)) 
				{
					AASet aaSet = Core.AppendGround(ch); 
					char coreRealizationChar = AASetSequence.ToCoreRealizationChar(aaSet);
					Debug.Assert(ch == coreRealizationChar); //!!!real assert
					sbCoreRealization.Append(coreRealizationChar);
 					sbFullRealization.Append(coreRealizationChar);
 					sbRegex.Append(AASetSequence.ToRegexString(aaSet));
				} 
 				else 
				{
					SpecialFunctions.CheckCondition(ch == '['); //!!!raise error 
					// Read [xyz]
					StringBuilder sbCharSet = new StringBuilder();
					bOK = charEnumerator.MoveNext();
 					SpecialFunctions.CheckCondition(bOK); //!!!raise error
 					ch = charEnumerator.Current;
					while(ch != ']') 
 					{ 
						sbCharSet.Append(ch);
						bOK = charEnumerator.MoveNext(); 
						SpecialFunctions.CheckCondition(bOK); //!!!raise error
						ch = charEnumerator.Current;
					}
 					SpecialFunctions.CheckCondition(sbCharSet.Length > 0); //!!!raies error
 					AASet aaSet = Core.AppendGroundSet(sbCharSet.ToString());
					char coreRealizationChar = AASetSequence.ToCoreRealizationChar(aaSet); 
 					sbCoreRealization.Append(coreRealizationChar); 
					sbFullRealization.Append(coreRealizationChar);
					sbRegex.Append(AASetSequence.ToRegexString(aaSet)); 
				}


				bOK = charEnumerator.MoveNext();
				if (bOK)
 				{ 
 					ch = charEnumerator.Current; 
				}
 			} 

			// Read after part
			while(bOK)
			{
				if (!(ch == '.' || (char.IsLetter(ch) && !char.IsUpper(ch))))
				{ 
 					break; 
 				}
				AASet aaSet = After.AppendGroundOrEdge(ch); 
 				sbRegex.Append(AASetSequence.ToRegexString(aaSet));

				char fullRealizationChar = AASetSequence.ToFlankingRealizationChar(aaSet);
				sbFullRealization.Append(fullRealizationChar);

				bOK = charEnumerator.MoveNext(); 
				if (bOK) 
				{
 					ch = charEnumerator.Current; 
 				}
			}

 			FullAsAASetSequence = AASetSequence.Concatenate(Before, Core, After);
			CoreRealization = sbCoreRealization.ToString();
			FullRealization = sbFullRealization.ToString(); 
			RegexString = sbRegex.ToString(); 
		}
 
//		public int Core.Length
//		{
//			get
//			{
//				return Core.Count;
//			} 
//		} 

		public override string ToString() 
 		{
 			return FullAsString;
		}

//		public Disjunct SubsequenceX(int startIndex, int length)
//		{ 
//			//If the 1st core character is included then also include Left 
//			//If the last core character is included then also include Right
//			//Extra restrictions = Must have length > 0, must include 1st or last character, but not both 
//			SpecialFunctions.CheckCondition(0 <= startIndex && startIndex < Core.Length); //!!!raise error
//			string expression = Full.Substring((int) startIndex, (int) length);
//			Disjunct disjunct = Disjunct.GetInstance(expression, PatchRegexFactory);
//			return disjunct;
//		}
 
 		public Disjunct Left(int length) 
		{
			SpecialFunctions.CheckCondition(length > 0); //!!!raise error 
			AASetSequence expression = FullAsAASetSequence.Subsequence(0, Before.Count + (int) length);
			Disjunct disjunct = Disjunct.GetInstance(expression.ToString(), PatchRegexFactory);
			return disjunct;
 		}

 		public Disjunct Right(int length) 
		{ 
 			SpecialFunctions.CheckCondition(length > 0); //!!!raise error
			AASetSequence expression = FullAsAASetSequence.Subsequence(Before.Count + Core.Count - (int) length); 
			Disjunct disjunct = Disjunct.GetInstance(expression.ToString(), PatchRegexFactory);
			return disjunct;
		}


		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11T11" and the other pattern is "T1111Z" 
 		///                      then UnifyOrNull returns "T1T11Z" because that is the most general pattern 
 		///                      such that "11T11".IsMatch("T1T11Z") is true and "T1111Z".IsMatch("T1T11Z") is true.
		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11T11" and the other pattern is "A1111" 
 		///                      then UnifyOrNull returns null because there is no pattern X such that
		///                      such that "11T11".IsMatch(X) is true and "A1111".IsMatch(X) is true.
		/// </summary>
		public void AppendUnifications(Disjunct other, ref ArrayList unifiedDisjunctStringList)
		{
			SpecialFunctions.CheckCondition(Core.Count <= other.Core.Count); //!!!raise error 
 
 			for(int iPossiblePos = 0; iPossiblePos <= other.Core.Count - Core.Count; ++iPossiblePos)
 			{ 
				AASetSequence unifiedDisjunctOrNull = UnificationAtPositionOrNull(other, iPossiblePos);
 				if (unifiedDisjunctOrNull != null)
				{
					unifiedDisjunctStringList.Add(unifiedDisjunctOrNull.ToString());
				}
			} 
		} 

 		private AASetSequence UnificationAtPositionOrNull(Disjunct other, int possiblePos) 
 		{
			AASetSequence aAASetSequence = AASetSequence.GetInstance();

 			bool b =
				BeforeUnificationAtPosition(other, possiblePos, ref aAASetSequence) &&
				BeforeAndCoreUnificationAtPosition(other, possiblePos, ref aAASetSequence) && 
				CoreUnificationAtPosition(other, possiblePos, ref aAASetSequence) && 
				AfterAndCoreUnificationAtPosition(other, possiblePos, ref aAASetSequence) &&
				AfterUnificationAtPosition(other, possiblePos, ref aAASetSequence); 

 			if (b)
 			{
				Debug.Assert(aAASetSequence[0] != AASet.OptionalAny && aAASetSequence[aAASetSequence.Count-1] != AASet.OptionalAny); // real assert - can't start or end with AASet.OptionalAny
 			}
 
			return b ? aAASetSequence : null; 
		}
 
		private bool BeforeAndCoreUnificationAtPosition(Disjunct other, int possiblePos, ref AASetSequence aAASetSequence)
		{
			for(int iCommon = 0; iCommon < possiblePos; ++iCommon)
 			{
 				AASet chOther = other.Core[iCommon];
				int iInBefore = Before.Count - possiblePos + iCommon; 
 				if (iInBefore >= 0) 
				{
					AASet chThis = Before[iInBefore]; 
					AASet chUnifyOrEmpty = FlankingAndCoreUnifyOrEmpty(chThis, chOther);
					if (chUnifyOrEmpty == AASet.Empty) //!!!const
					{
 						return false;
 					}
					aAASetSequence.Append(chUnifyOrEmpty); 
 				} 
				else
				{ 
					aAASetSequence.Append(chOther);
				}
			}
 			return true;
 		}
 
		private bool AfterAndCoreUnificationAtPosition(Disjunct other, int possiblePos, ref AASetSequence aAASetSequence) 
 		{
			int iHowMuchDoesThisStickIn = Math.Max(other.Core.Count - (possiblePos + Core.Count), 0); 
			for(int iCommon = 0; iCommon < iHowMuchDoesThisStickIn; ++iCommon)
			{
				AASet chOther = other.Core[other.Core.Count - iHowMuchDoesThisStickIn + iCommon];
				if (iCommon < After.Count)
 				{
 					AASet chThis = After[(int)iCommon]; 
 
					AASet chUnifyOrEmpty = FlankingAndCoreUnifyOrEmpty(chThis, chOther);
 					if (chUnifyOrEmpty == AASet.Empty) //!!!const 
					{
						return false;
					}
					aAASetSequence.Append(chUnifyOrEmpty);
				}
 				else 
 				{ 
					aAASetSequence.Append(chOther);
 				} 
			}
			return true;
		}

		private bool BeforeUnificationAtPosition(Disjunct other, int possiblePos, ref AASetSequence aAASetSequence)
		{ 
 			//This creates the new before string. It will be the unification of other's before string 
 			//and any of This's before that sticks out beyond the core of other
			int iHowMuchDoesThisStickOut = Math.Max(0, Before.Count - possiblePos); 
 			int iHowMuchLongerDoesThisStickOutComparedToOther = Math.Max(0, iHowMuchDoesThisStickOut - other.Before.Count);
			aAASetSequence.AppendSubsequence(Before,  0, iHowMuchLongerDoesThisStickOutComparedToOther);
			int iHowMuchLongerDoesOtherStickOutComparedToThis = Math.Max(0, other.Before.Count - iHowMuchDoesThisStickOut);
			aAASetSequence.AppendSubsequence(other.Before, 0, iHowMuchLongerDoesOtherStickOutComparedToThis);

			int iHowMuchInCommon = Math.Min(iHowMuchDoesThisStickOut, other.Before.Count); 
			for(int iCommon = 0; iCommon < iHowMuchInCommon; ++iCommon) 
 			{
 				AASet chThis = Before[iHowMuchLongerDoesThisStickOutComparedToOther + iCommon]; 
				AASet chOther = other.Before[iHowMuchLongerDoesOtherStickOutComparedToThis + iCommon];

 				AASet chUnifyOrOptionalEmpty = FlankingUnifyOrOptionalEmpty(chThis, chOther);
				if (chUnifyOrOptionalEmpty == AASet.OptionalEmpty) //!!!const
				{
					return false; 
				} 
				aAASetSequence.Append(chUnifyOrOptionalEmpty);
 			} 
 			return true;
		}

 		private bool AfterUnificationAtPosition(Disjunct other, int possiblePos, ref AASetSequence aAASetSequence)
		{
			//This creates the new after string. It will be the unification of other's after string 
			//and any of This's after that sticks out beyond the core of other 
			int iHowMuchDoesThisStickOut = Math.Max(0, possiblePos + Core.Count + After.Count - other.Core.Count);
 
			int iHowMuchInCommon = Math.Min(iHowMuchDoesThisStickOut, other.After.Count);
 			for(int iCommon = 0; iCommon < iHowMuchInCommon; ++iCommon)
 			{
				AASet chThis = After[After.Count - iHowMuchDoesThisStickOut + iCommon];
 				AASet chOther = other.After[iCommon];
 
				AASet chUnifyOrOptionalEmpty = FlankingUnifyOrOptionalEmpty(chThis, chOther); 
				if (chUnifyOrOptionalEmpty == AASet.OptionalEmpty) //!!!const
				{ 
					return false;
				}
 				aAASetSequence.Append(chUnifyOrOptionalEmpty);
 			}

			int iHowMuchLongerDoesOtherStickOutComparedToThis = Math.Max(0, other.After.Count - iHowMuchDoesThisStickOut); 
 			aAASetSequence.AppendSubsequence(other.After, iHowMuchInCommon, iHowMuchLongerDoesOtherStickOutComparedToThis); 
			int iHowMuchLongerDoesThisStickOutComparedToOther = Math.Max(0, iHowMuchDoesThisStickOut - other.After.Count);
			aAASetSequence.AppendSubsequence(After, After.Count - iHowMuchLongerDoesThisStickOutComparedToOther, iHowMuchLongerDoesThisStickOutComparedToOther); 
			return true;
		}


		private AASet FlankingAndCoreUnifyOrEmpty(AASet flanking, AASet core)
 		{ 
 			if (flanking == AASet.OptionalAny) 
			{
 				return core; 
			}
			Debug.Assert(AASetSequence.IsOptional(flanking)); // real assert
			AASet chUnifyOrEmpty = AASetSequence.UnifyOrEmpty(AASetSequence.ToRequired(flanking), core);
			return chUnifyOrEmpty;
		}
 
 		static private AASet FlankingUnifyOrOptionalEmpty(AASet c1, AASet c2) 
 		{
			Debug.Assert(AASetSequence.IsOptional(c1) && AASetSequence.IsOptional(c2)); // real assert 
 			AASet result = c1 & c2;
			return result;
		}

		private bool CoreUnificationAtPosition(Disjunct other, int possiblePos, ref AASetSequence aAASetSequence)
		{ 
			for(int iThisPos = 0; iThisPos < Core.Count; ++iThisPos) 
 			{
 				AASet chThis = Core[iThisPos]; 
				int iOtherPos = possiblePos + iThisPos;
 				AASet chOther = other.Core[iOtherPos];
				AASet chUnifyOrEmpty = AASetSequence.UnifyOrEmpty(chThis, chOther);
				if (chUnifyOrEmpty == AASet.Empty) //!!!const
				{
					return false; 
				} 
 				aAASetSequence.Append(chUnifyOrEmpty);
 			} 
			return true;
 		}
	}

}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
