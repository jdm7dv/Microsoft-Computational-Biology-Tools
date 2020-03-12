using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using Msr.Mlas.SpecialFunctions; 
 
namespace VirusCount
{ 
 	abstract public class PatchPatternFactory
	{
		protected PatchPatternFactory()
		{
		}
 
		public abstract PatchPatternBuilder GetBuilder(); 

 		static public PatchPatternFactory GetFactory(string language /*AASimilarity aAASimilarity*/) 
 		{
            //!!!switch to switch
            if (language == "strings")
			{   Debug.WriteLine("Using the strings language");
 				PatchPatternFactory patchPatternFactory = new PatchStringFactory();
				return patchPatternFactory; 
			} 
			else if (language == "regexs")
			{	Debug.WriteLine("Using the regexs language"); 
				PatchRegexFactory patchRegexFactory = new PatchRegexFactory();
 				return patchRegexFactory;
 			}
			else
 			{
				Debug.Fail("Don't know how to create a PatchPatternFactory for language " + language); 
				return null; 
			}
		} 

		public abstract PatchPattern GetInstance(string expression);
 		public abstract PatchPattern GetInstance(ICollection disjunctStringCollection);
 	}

	public class PatchStringFactory : PatchPatternFactory 
 	{ 
		public PatchStringFactory(/*AASimilarity aAASimilarity*/)
		{ 
			//AASimilarity = aAASimilarity;
		}

		private Hashtable Hashtable = new Hashtable();

 		public override PatchPattern GetInstance(ICollection disjunctStringCollection) 
 		{ 
			SpecialFunctions.CheckCondition(disjunctStringCollection.Count == 1); //!!!raise error
 			foreach(string disjunct in disjunctStringCollection) 
			{
				return GetInstance(disjunct);
			}
			Debug.Assert(false); //real assert
			return null;
 		} 
 		public override PatchPattern GetInstance(string expression) 
		{
 			if (Hashtable.ContainsKey(expression)) 
			{
				PatchPattern patchPattern = (PatchPattern) Hashtable[expression];
				return patchPattern;
			}
			else
 			{ 
 				PatchPattern patchPattern = new PatchString(expression, this); 
				SpecialFunctions.CheckCondition(patchPattern.ToString() == expression, "PatchPattern expression is not in standard form."); //!!!raise error
 				Hashtable.Add(expression, patchPattern); 
				return patchPattern;
			}
		}

		public override PatchPatternBuilder GetBuilder()
		{ 
 			PatchPatternBuilder aPatchPatternBuilder = new PatchPatternBuilder(); 
 			//aPatchPatternBuilder.AASimilarity = AASimilarity;
			aPatchPatternBuilder.PatchPatternFactory = this; 
 			return aPatchPatternBuilder;
		}

		//AASimilarity AASimilarity;

 
	} 
				
 

	public class PatchRegexFactory : PatchPatternFactory
 	{
 		public PatchRegexFactory(/*AASimilarity aAASimilarity*/)
		{
 			//AASimilarity = aAASimilarity; 
		} 

		public Hashtable Hashtable = new Hashtable(); 

		public override PatchPattern GetInstance(ICollection disjunctStringCollection)
		{
			PatchRegex patchRegex = PatchRegex.GetInstance(disjunctStringCollection, this);
 			if (Hashtable.ContainsKey(patchRegex.ToString()))
 			{ 
				PatchPattern patchPattern = (PatchPattern) Hashtable[patchRegex.ToString()]; 
 				return patchPattern;
			} 
			else
			{
				PatchPattern patchPattern = patchRegex;
				Hashtable.Add(patchPattern.ToString(), patchPattern);
 				return patchPattern;
 			} 
		} 

 		public override PatchPattern GetInstance(string expression) 
		{
			if (Hashtable.ContainsKey(expression))
			{
				PatchPattern patchPattern = (PatchPattern) Hashtable[expression];
				return patchPattern;
 			} 
 			else 
			{
 				PatchPattern patchPattern = PatchRegex.GetInstance(expression, this); 
				SpecialFunctions.CheckCondition(patchPattern.ToString() == expression, "PatchPattern expression is not in standard form."); //!!!raise error
				Hashtable.Add(expression, patchPattern);
				return patchPattern;
			}
		}
 
 		public override PatchPatternBuilder GetBuilder() 
 		{
			PatchPatternBuilder aPatchPatternBuilder = new PatchPatternBuilder(); 
 			//aPatchPatternBuilder.AASimilarity = AASimilarity;
			aPatchPatternBuilder.PatchPatternFactory = this;
			return aPatchPatternBuilder;
		}

		//public AASimilarity AASimilarity; 
	} 

 	abstract public class PatchPattern 
 	{
		abstract public string CoreRealization();
 		abstract public string FullRealization();
		//abstract public string Abstraction();
		abstract public int CoreLength {get;}
		abstract public int FullLength {get;} 
		abstract public int MaxLength {get;} 
		//abstract public string ToString();
 
 		/// <summary>
 		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11A11" and the vaccine is "ZZZZTTATTTYYYY"
		///                      then the patchPattern matches the vaccine.
 		/// </summary>
		abstract public bool IsMatch(string vaccineString);
 
		/// <summary> 
		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11T11" and the other pattern is "T1111Z"
		///                      then UnifyOrNull returns "T1T11Z" because that is the most general pattern 
		///                      such that "11T11".IsMatch("T1T11Z") is true and "T1111Z".IsMatch("T1T11Z") is true.
 		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11T11" and the other pattern is "A1111"
 		///                      then UnifyOrNull returns null because there is no pattern X such that
		///                      such that "11T11".IsMatch(X) is true and "A1111".IsMatch(X) is true.
 		/// </summary>
		abstract public PatchPattern UnifyOrNull(PatchPattern other); 
		//		{ 
		//			SpecialFunctions.CheckCondition(Length <= other.Length); //!!!raise error
		// 
		//			PatchPattern patchPattern = DoUnifyOrNull();
 		//			return patchPattern;
 		//		}

		abstract public PatchPattern UnifyOnRightOrNull(int overlap, PatchPattern other);
 		//		{ 
		//			SpecialFunctions.CheckCondition(overlap < Length && overlap < other.Length); //!!!raise error 
		//			PatchPattern patchPattern = DoUnifyOnLeftOrNull();
		//			return patchPattern; 
		////			for(int iSmall = 1; iSmall < small.Length; ++iSmall)
		////			{
 		////				PatchPattern rightPartOfSmall = small.Subpattern(iSmall);
 		////				PatchPattern leftPartOfBig = big.Subpattern(0, rightPartOfSmall.Length);
		////				PatchPattern unification = rightPartOfSmall.UnifyOrNull(leftPartOfBig);
 		////				if (unification != null) 
		////				{ 
		////					Move aMove = combinable.GetAddToLeft(component, iSmall, unification);
		////					return aMove; 
		////				}
		////			}
 		////
 		//		}

 
		protected PatchPattern() 
 		{
		} 

	}


	public class PatchString : PatchPattern
	{ 
		PatchStringFactory PatchStringFactory; 

 		public PatchString(string expression, PatchStringFactory patchStringFactory) //!!!protected??? 
 		{
			String = expression;
 			PatchStringFactory = patchStringFactory;
		}

		private string String; 
 
		override public string CoreRealization()
		{ 
			return String;
 		}

 		override public string FullRealization()
		{
 			return String; 
		} 
		
		//		override public string Abstraction() 
//		{
//			return String;
//		}
		override public int CoreLength
		{
 			get 
 			{ 
				return String.Length;
 			} 
		}
		override public int FullLength
		{
			get
			{
 				return String.Length; 
 			} 
		}
 		override public int MaxLength 
		{
			get
			{
				return String.Length;
			}
 		} 
 
 		override public string ToString()
		{ 
 			return String;
		}

		/// <summary>
		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11A11" and the vaccine is "ZZZZTTATTTYYYY"
		///                      then the patchPattern matches the vaccine. 
		/// </summary> 
 		override public bool IsMatch(string vaccineString)
 		{ 
			bool b = (vaccineString.IndexOf(String) >= 0);
 			return b;
		}


		/// <summary> 
		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11T11" and the other pattern is "T1111Z" 
		///                      then UnifyOrNull returns "T1T11Z" because that is the most general pattern
		///                      such that "11T11".IsMatch("T1T11Z") is true and "T1111Z".IsMatch("T1T11Z") is true. 
 		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11T11" and the other pattern is "A1111"
 		///                      then UnifyOrNull returns null because there is no pattern X such that
		///                      such that "11T11".IsMatch(X) is true and "A1111".IsMatch(X) is true.
 		/// </summary>
		override public PatchPattern UnifyOrNull(PatchPattern other)
		{ 
			SpecialFunctions.CheckCondition(CoreLength <= other.CoreLength); //!!!raise error 
			SpecialFunctions.CheckCondition(other is PatchString); //!!!raise error
			if (other.ToString().IndexOf(String) >= 0) 
 			{
 				return other;
			}
 			else
			{
				return null; 
			} 
		}
 
		override public PatchPattern UnifyOnRightOrNull(int overlap, PatchPattern other)
 		{
 			SpecialFunctions.CheckCondition(overlap < CoreLength && overlap < other.CoreLength); //!!!raise error
			SpecialFunctions.CheckCondition(other is PatchString); //!!!raise error

 			string[] rgsOther = SplitRight(other.ToString(), overlap); 
			string[] rgsMe = SplitLeft(String, overlap); 

			if (rgsOther[1] != rgsMe[0]) 
			{
				return null;
			}

 			string sUnification = rgsOther[0] + String;
 
 			PatchPattern patchPattern = PatchStringFactory.GetInstance(sUnification); 
			return patchPattern;
 		} 

		static private string[] SplitRight(string s, int overlap)
		{
			SpecialFunctions.CheckCondition(0 < overlap && overlap < s.Length); //!!!raise error
			string[] rgs = new string[2];
			rgs[0] = s.Substring(0, s.Length - (int) overlap); 
 			rgs[1] = s.Substring(s.Length - (int) overlap); 
 			return rgs;
		} 

 		static private string[] SplitLeft(string s, int overlap)
		{
			SpecialFunctions.CheckCondition(0 < overlap && overlap < s.Length); //!!!raise error
			string[] rgs = new string[2];
			rgs[0] = s.Substring(0, (int) overlap); 
			rgs[1] = s.Substring((int) overlap); 
 			return rgs;
 		} 

	}

 	public class PatchRegex : PatchPattern
	{
 
		private PatchRegex() 
		{
		} 


		static public PatchRegex GetInstance(string expression, PatchRegexFactory patchRegexFactory)
 		{
 			return GetInstance(expression.Split(DisjunctJoiner), patchRegexFactory);
		} 
 
 		static public PatchRegex GetInstance(ICollection disjunctStringCollection, PatchRegexFactory patchRegexFactory)
		{ 
			Debug.Assert(!(disjunctStringCollection is AASetSequence)); // real assert

			// e.g. "ABC123", "ABC123|1BCD23"
			SortedList rgDisjunct = new SortedList();
			foreach(string disjunctAsString in disjunctStringCollection)
 			{ 
 				Disjunct disjunct = Disjunct.GetInstance(disjunctAsString, patchRegexFactory); 
				rgDisjunct[disjunct.FullAsString] = disjunct; //Remove idenitical disjuncts
 			} 

			PatchRegex patchRegex = FinishUp(rgDisjunct, patchRegexFactory);
			return patchRegex;
		}

 
		private static PatchRegex FinishUp(SortedList rgDisjunct1, PatchRegexFactory patchRegexFactory) 
		{
 			SortedList rgDisjunct2 = RemoveSubsumedDisjuncts(rgDisjunct1); 
 			PatchRegex patchRegex = new PatchRegex();
			patchRegex.DisjunctCollection = new Disjunct[rgDisjunct2.Count];
 			rgDisjunct2.Values.CopyTo(patchRegex.DisjunctCollection, 0);

			SpecialFunctions.CheckCondition(rgDisjunct2.Count > 0); //!!!raise error - just have at least one disjunct
			patchRegex.SetStringFromDisjunctCollection(); 
			patchRegex.SetEverythingElse(patchRegexFactory); 

			if (patchRegexFactory.Hashtable.ContainsKey(patchRegex.ToString())) 
			{
 				return (PatchRegex) patchRegexFactory.Hashtable[patchRegex.ToString()];
 			}
			else
 			{
				return patchRegex; 
			} 
		}
 


		static private PatchRegex Concatenate(params PatchRegex[] patchRegexParams)
		{
 			SpecialFunctions.CheckCondition(patchRegexParams.Length > 0); //!!!raise error
 			PatchRegexFactory patchRegexFactory = null; 
			//ArrayList rgSegment = new ArrayList(); 
 			double combinations = 1;
			AASetSequence sbLuckyOne = AASetSequence.GetInstance(); 

			foreach(PatchRegex patchRegex in patchRegexParams)
			{
				//rgSegment.AddRange(patchRegex.SegmentCollection);
				combinations *= patchRegex.DisjunctCollection.Length;
 				if (combinations == 1) 
 				{ 
					sbLuckyOne.Append(patchRegex.DisjunctCollection[0].FullAsAASetSequence);
 				} 
				if (patchRegexFactory == null)
				{
					patchRegexFactory = patchRegex.PatchRegexFactory;
				}
				else
 				{ 
 					Debug.Assert(patchRegexFactory == patchRegex.PatchRegexFactory);//this says that all PatchRegex's must come from the same Hashtable 
				}
 			} 

			if (combinations == 1)
			{
				Debug.Assert(sbLuckyOne[0] != AASet.OptionalAny && sbLuckyOne[sbLuckyOne.Count-1] != AASet.OptionalAny); // real assert - disjuncts can't start or end with AASet.OptionalAny
				string[] rgDisjuncts = new string[]{sbLuckyOne.ToString()};
				PatchRegex patchRegex = PatchRegex.GetInstance(rgDisjuncts, patchRegexFactory); 
 				return patchRegex; 
 			}
			else 
 			{
				Debug.Fail("how may combinations?");
				Debug.WriteLine("place of interest");
				return null;
			}
			//			else if (rgSegment.Count > 1 && combinations == 2) 
 			//			{ 
 			//				Segment segment = TurnToOneSegment(rgSegment, patchRegexFactory);
			//				rgSegment.Clear(); 
 			//				rgSegment.Add(segment);
			//			}

		}

		static private void	MultipleDisjunctStrings(int factor, ref ArrayList rgDisjunctStrings) 
		{ 
			Debug.Assert(factor > 0); //real assert
 			int originalLength = rgDisjunctStrings.Count; 
 			for(int iFactor = 1 /*not 0*/; iFactor < factor; ++ iFactor)
			{
 				for(int iEntry = 0; iEntry < originalLength; ++iEntry)
				{
					StringBuilder sb = (StringBuilder) rgDisjunctStrings[(int)iEntry];
					rgDisjunctStrings.Add(new StringBuilder(sb.ToString())); 
				} 
			}
 		} 

//		static private Segment TurnToOneSegment(ArrayList rgSegment, PatchRegexFactory patchRegexFactory)
//		{
//			Debug.Assert(rgSegment.Count >= 1); // real assert
//
//			ArrayList rgDisjunctStrings = new ArrayList(); 
//			rgDisjunctStrings.Add(new StringBuilder()); 
//
//			foreach(Segment segment in rgSegment) 
//			{
//				int originalLength = rgDisjunctStrings.Count;
//				MultipleDisjunctStrings(segment.DisjunctCollection.Length, ref rgDisjunctStrings);
//
//				for(int iDisjunct = 0; iDisjunct < segment.DisjunctCollection.Length; ++ iDisjunct)
//				{ 
//					Disjunct disjunct = segment.DisjunctCollection[iDisjunct]; 
//					for(int iEntry = 0; iEntry < originalLength; ++iEntry)
//					{ 
//						int i = iDisjunct * originalLength + iEntry;
//						StringBuilder sb = (StringBuilder) rgDisjunctStrings[(int) i];
//						sb.Append(disjunct.Full);
//					}
//				}
//			} 
// 
//			Segment segmentNew = Segment.GetInstance(rgDisjunctStrings, patchRegexFactory);
//			return segmentNew; 
//			
//		}



 		PatchRegexFactory PatchRegexFactory; 
 
//		private void SetToStringFromSegmentCollection()
//		{ 
//			StringBuilder toStringStringBuilder = new StringBuilder();
//			foreach(Segment segment in SegmentCollection)
//			{
//				if (toStringStringBuilder.Length != 0)
//				{
//					toStringStringBuilder.Append(SegmentJoiner); 
//				} 
//				toStringStringBuilder.Append(segment.ToString());
//			} 
//
//			String = toStringStringBuilder.ToString();
//		}


		private void SetEverythingElse(PatchRegexFactory patchRegexFactory) 
 		{ 
			PatchRegexFactory = patchRegexFactory;
			_MaxLength = int.MinValue; 
			_coreRealization = null;
			StringBuilder regexStringBuilder = new StringBuilder();
			foreach(Disjunct Disjunct in DisjunctCollection)
 			{
 				_MaxLength = Math.Max(Disjunct.FullAsAASetSequence.Count, MaxLength);
				if (_coreRealization == null) 
 				{ 
					_coreRealization = Disjunct.CoreRealization;
					_fullRealization = Disjunct.FullRealization; 
				}
				else
				{
 					SpecialFunctions.CheckCondition(CoreLength == Disjunct.Core.Count); //!!!raise error - disjuncts lengths vary
 					if (_fullRealization.Length > Disjunct.FullRealization.Length)
					{ 
 						_fullRealization = Disjunct.FullRealization; 
					}
				} 
				if(regexStringBuilder.Length != 0)
				{
					regexStringBuilder.Append('|');
 				}
 				regexStringBuilder.Append(Disjunct.RegexString);
			} 
 
 			RegexString = regexStringBuilder.ToString();
			Regex = new Regex(regexStringBuilder.ToString() /*, RegexOptions.Compiled*/); 
			Debug.Assert(CoreLength <= MaxLength);

		}


 
		private int _MaxLength; 
		private string _coreRealization;
 		private string _fullRealization; 
 		private string String;
		private Regex Regex;
 		public string RegexString;

		override public string CoreRealization()
		{ 
			return _coreRealization; 
		}
 
		override public string FullRealization()
 		{
 			return _fullRealization;
		}

 		override public int CoreLength 
		{ 
			get
			{ 
				return _coreRealization.Length;
			}
 		}

 		override public int FullLength
		{ 
 			get 
			{
				return _fullRealization.Length; 
			}
		}


		override public int MaxLength
 		{ 
 			get 
			{
 				return _MaxLength; 
			}
		}

		override public string ToString()
		{
			return String; 
 		} 

 		/// <summary> 
		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11A11" and the vaccine is "ZZZZTTATTTYYYY"
 		///                      then the patchPattern matches the vaccine.
		/// </summary>
		override public bool IsMatch(string vaccineString)
		{
			bool b = (Regex.IsMatch(vaccineString)); 
			return b; 
 		}
 

 		/// <summary>
		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11T11" and the other pattern is "T1111Z"
 		///                      then UnifyOrNull returns "T1T11Z" because that is the most general pattern
		///                      such that "11T11".IsMatch("T1T11Z") is true and "T1111Z".IsMatch("T1T11Z") is true.
		/// Example:  Suppose "1" is "[TNMSQC]" and the patchPattern is "11T11" and the other pattern is "A1111" 
		///                      then UnifyOrNull returns null because there is no pattern X such that 
		///                      such that "11T11".IsMatch(X) is true and "A1111".IsMatch(X) is true.
		/// </summary> 
 		override public PatchPattern UnifyOrNull(PatchPattern other)
 		{
			SpecialFunctions.CheckCondition(CoreLength <= other.CoreLength); //!!!raise error
 			SpecialFunctions.CheckCondition(other is PatchRegex); //!!!raise error

			ArrayList newDisjuncts = new ArrayList(); 
			foreach(Disjunct disjunct in DisjunctCollection) 
			{
				foreach(Disjunct disjunctOther in (other as PatchRegex).DisjunctCollection) 
				{
 					disjunct.AppendUnifications(disjunctOther, ref newDisjuncts);
 				}
			}

 			if (newDisjuncts.Count == 0) 
			{ 
				string sMiniVaccine = string.Format("{0}{1}{0}", new string('_', (int) Math.Max(MaxLength, other.MaxLength)), other.CoreRealization());
				if (IsMatch(sMiniVaccine)) 
				{
					Debug.WriteLine(string.Format("Warning: two patterns would unify except for flanking regions that could be ignored by requiring that nothing be appending to the component. {0}, {1}", this, other));
 				}
 				return null;
			}
 			else 
			{ 
				PatchPattern patchPatternUnified = PatchRegex.GetInstance(newDisjuncts, PatchRegexFactory);
				string sMiniVaccine = string.Format("{0}{1}{0}", new string('_', (int) Math.Max(MaxLength, other.MaxLength)), patchPatternUnified.CoreRealization()); 
				Debug.Assert(IsMatch(sMiniVaccine)); // real assert
				Debug.Assert(other.IsMatch(sMiniVaccine)); // real assert
 				return patchPatternUnified;
 			}
		}
 
 		static int DisjunctLimit = 100; 

		override public PatchPattern UnifyOnRightOrNull(int overlap, PatchPattern other) 
		{
			SpecialFunctions.CheckCondition(overlap < CoreLength && overlap < other.CoreLength); //!!!raise error
			SpecialFunctions.CheckCondition(other is PatchRegex); //!!!raise error

			ArrayList rgrgOther = SplitRight((PatchRegex) other, overlap);
 			ArrayList rgrgMe = SplitLeft(overlap); 
 

 			ArrayList rgOuts = new ArrayList(); 
			foreach(PatchRegex[] rgOther in rgrgOther)
 			{
				foreach(PatchRegex[] rgMe in rgrgMe)
				{
					PatchRegex unification = (PatchRegex) rgMe[0].UnifyOrNull(rgOther[1]);
 
					 
					if (unification != null)
 					{ 
 						PatchPattern patchPattern;
						if (!unification.AnyFlanking())
 						{
							patchPattern = Concatenate(rgOther[0], unification, rgMe[1]);
							Debug.Assert(patchPattern != null);
						} 
						else 
						{
 							patchPattern = ReunifyWithFlanking(rgOther[0], unification, rgMe[1]); 
 						}
						if (patchPattern != null)
 						{
							rgOuts.Add(patchPattern);
						}
						if (rgOuts.Count >= DisjunctLimit) 
						{ 
							goto SkipOutEarly;
 						} 
 					}
				}
 			}
			SkipOutEarly: ;
			if (rgOuts.Count == 0)
			{ 
				return null; 
			}
 			else if (rgOuts.Count == 1) 
 			{
				return (PatchPattern) rgOuts[0];
 			}
			else
			{
				return MergePatchPatterns(rgOuts, PatchRegexFactory); 
			} 
		}
 
 		PatchRegex ReunifyWithFlanking(PatchRegex left, PatchRegex unification, PatchRegex right)
 		{
			ArrayList rgStringDisjuncts = new ArrayList();

 			foreach(Disjunct disjunctUnification in unification.DisjunctCollection)
			{ 
				foreach(Disjunct disjunctLeft in left.DisjunctCollection) 
				{
					AASetSequence newLeftDisjunctOrNull = UnifyWithBeforeOrNull(disjunctLeft.Core.Count, disjunctLeft.FullAsAASetSequence, disjunctUnification.Before); 
					if (newLeftDisjunctOrNull != null)
 					{
 						Debug.Assert(newLeftDisjunctOrNull[0] != AASet.OptionalAny && newLeftDisjunctOrNull[newLeftDisjunctOrNull.Count-1] != AASet.OptionalAny); // real assert - disjuncts never start or end with AASet.OptionalAny
						foreach(Disjunct disjunctRight in right.DisjunctCollection)
 						{
 
							AASetSequence newRightDisjunctOrNull = UnifyWithAfterOrNull(disjunctRight.Core.Count, disjunctRight.FullAsAASetSequence, disjunctUnification.After); 
							if (newRightDisjunctOrNull != null)
							{ 
								Debug.Assert(newRightDisjunctOrNull[0] != AASet.OptionalAny && newRightDisjunctOrNull[newRightDisjunctOrNull.Count-1] != AASet.OptionalAny); // real assert - disjuncts never start or end with AASet.OptionalAny
								Concatenate(newLeftDisjunctOrNull, disjunctUnification, newRightDisjunctOrNull, ref rgStringDisjuncts);
 							}
 						}
					}
 				} 
			} 

			if (rgStringDisjuncts.Count == 0) 
			{
				return null;
			}
 			else
 			{
				PatchRegex newRegex = PatchRegex.GetInstance(rgStringDisjuncts, PatchRegexFactory); 
 				return newRegex; 
			}
		} 

		private void Concatenate(AASetSequence disjunctLeftString, Disjunct disjunctMiddle, AASetSequence disjunctRightString, ref ArrayList rgStringDisjuncts)
		{
			string newDisjunctString = string.Format("{0}{1}{2}", disjunctLeftString, disjunctMiddle.Core, disjunctRightString);
 			rgStringDisjuncts.Add(newDisjunctString);
 		} 
 

 
		//		private void Concatenate(string disjunctLeftString, Disjunct disjunctMiddle, PatchRegex patchRegexRight, ref ArrayList rgStringDisjuncts)
 		//		{
		//			Debug.Assert(disjunctMiddle.After == ""); // real assert
		//			Debug.Assert(disjunctMiddle.Before != ""); // real assert
		//
		//			Debug.Assert(patchRegexRight.SegmentCollection.Length == 1); //!!!need code for other cases 
		//			Segment segmentRight = patchRegexRight.SegmentCollection[0]; 
 		//			foreach(Disjunct disjunctRight in segmentRight.DisjunctCollection)
 		//			{ 
		//				Debug.Assert(disjunctRight.Before == ""); // real asert
 		//				string newDisjunctString = string.Format("{0}{1}{2}", disjunctLeftString, disjunctMiddle.Core, disjunctRight.Full);
		//				rgStringDisjuncts.Add(newDisjunctString);
		//			}
		//		}
		// 
		//		private void Concatenate(PatchRegex patchRegexLeft, Disjunct disjunctMiddle, string disjunctRightString, ref ArrayList rgStringDisjuncts) 
 		//		{
 		//			Debug.Assert(disjunctMiddle.Before == ""); // real assert 
		//			Debug.Assert(disjunctMiddle.After != ""); // real assert
 		//
		//			Debug.Assert(patchRegexLeft.SegmentCollection.Length == 1); //!!!need code for other cases
		//			Segment segmentLeft = patchRegexLeft.SegmentCollection[0];
		//			foreach(Disjunct disjunctLeft in segmentLeft.DisjunctCollection)
		//			{ 
		//				Debug.Assert(disjunctLeft.After == ""); // real asert 
 		//				string newDisjunctString = string.Format("{0}{1}{2}", disjunctLeft.Full, disjunctMiddle.Core, disjunctRightString);
 		//				rgStringDisjuncts.Add(newDisjunctString); 
		//			}
 		//		}


		private AASetSequence UnifyWithBeforeOrNull(int coreLength, AASetSequence disjunctRightFull, AASetSequence disjunctUnificationAfter)
		{ 
			AASetSequence s = Reverse(UnifyWithAfterOrNull(coreLength, Reverse(disjunctRightFull), Reverse(disjunctUnificationAfter))); 
			return s;
		} 

 		static private AASetSequence Reverse(AASetSequence s)
 		{
			if (s == null)
 			{
				return null; 
			} 
			else
			{ 
				AASetSequence r = s.ReverseClone();
 				return r;
 			}
		}

 		private AASetSequence UnifyWithAfterOrNull(int coreLength, AASetSequence disjunctRightFull, AASetSequence disjunctUnificationAfter) 
		{ 
			AASetSequence sb;
			if (disjunctRightFull.Count >= disjunctUnificationAfter.Count) 
			{
				sb = UnifyWithAfterOrNullLongerShorter(disjunctRightFull, disjunctUnificationAfter);
 			}
 			else
			{
 				sb = UnifyWithAfterOrNullLongerShorter(disjunctUnificationAfter, disjunctRightFull); 
			} 

			if (sb == null) 
			{
				return null;
			}

 			//make everything after the core region, lower case to shows that it is a flanking region
 			for(int i = (int) coreLength + 1; i < sb.Count; ++i) 
			{ 
 				sb[i] = AASetSequence.ToOptional(sb[i]);
			} 
			return sb;
		}
		private AASetSequence UnifyWithAfterOrNullLongerShorter(AASetSequence longer, AASetSequence shorter)
		{
 			Debug.Assert(longer.Count >= shorter.Count); // real assert
 			AASetSequence sb = AASetSequence.GetInstance(longer); 
			for(int i = 0; i < shorter.Count; ++i) 
 			{
				AASet chShorter = shorter[i]; 
				AASet chLonger = longer[i];
				AASet chUnifyOrEmpty = UnifyOrBoundWithFlanking(chShorter, chLonger);
				if (chUnifyOrEmpty == AASet.Empty)
				{
 					return null;
 				} 
				sb[i] = chUnifyOrEmpty; 
 			}
			return sb; 
		}

		private AASet UnifyOrBoundWithFlanking(AASet c1, AASet c2)
		{
			AASet result = AASetSequence.ToRequired(c1 & c2);
 			return result; 
 		} 

		private bool AnyFlanking() 
 		{
			foreach(Disjunct disjunct in DisjunctCollection)
			{
				if (disjunct.Before.Count != 0 || disjunct.After.Count != 0)
				{
					return true; 
 				} 
 			}
			return false; 
 		}


		static private PatchPattern MergePatchPatterns(ArrayList rgOut, PatchRegexFactory patchRegexFactory)
		{
			ArrayList rgStringDisjuncts = new ArrayList(); 
			foreach(PatchRegex aPatchRegex in rgOut) 
			{
 				Debug.Assert(aPatchRegex != null); // real assert 
 				foreach(Disjunct aDisjunct in aPatchRegex.DisjunctCollection)
				{
 					rgStringDisjuncts.Add(aDisjunct.FullAsAASetSequence.ToString());
				}
			}
			PatchRegex newRegex = PatchRegex.GetInstance(rgStringDisjuncts, patchRegexFactory); 
			return newRegex; 
		}
 

 		//Return an ArrayList of PatchRegex[] pairs
 		static private ArrayList SplitRight(PatchRegex s, int overlap)
		{
 			SpecialFunctions.CheckCondition(0 < overlap && overlap < s.CoreLength); //!!!raise error
			ArrayList rgrg = s.SplitLeft(s.CoreLength - (int) overlap); 
			return rgrg; 
		}
 
		public Disjunct[][] SplitDisjuncts(int leftLength)
		{
 			Debug.Assert(0 < leftLength && leftLength < CoreLength );

 			Disjunct[][] rgrg = new Disjunct[DisjunctCollection.Length][];
			for(int iDisjunct = 0; iDisjunct < DisjunctCollection.Length; ++iDisjunct) 
 			{ 
				Disjunct disjunct = DisjunctCollection[iDisjunct];
 
				rgrg[iDisjunct] =
					new Disjunct[]{
									  disjunct.Left(leftLength),
									  disjunct.Right(CoreLength - leftLength)};
 			}
 			return rgrg; 
		} 

 
 		//Return an ArrayList of PatchRegex[] pairs
		private ArrayList SplitLeft(int overlap)
		{
			SpecialFunctions.CheckCondition(0 < overlap && overlap < CoreLength); //!!!raise error

			ArrayList rgrg = new ArrayList(); 
 
			Disjunct[][] rgrgDisjuncts = SplitDisjuncts(overlap);
 			foreach(Disjunct[] splitDisjunct in rgrgDisjuncts) 
 			{
				PatchRegex patchRegexLeft = PatchRegex.GetInstance(splitDisjunct[0].FullAsAASetSequence.ToString(), PatchRegexFactory);
 				PatchRegex patchRegexRight = PatchRegex.GetInstance(splitDisjunct[1].FullAsAASetSequence.ToString(), PatchRegexFactory);
				rgrg.Add(new PatchRegex[]{patchRegexLeft, patchRegexRight});
			}
 
			Debug.Assert(rgrg.Count == rgrgDisjuncts.Length); // real assert 

			return rgrg; 
		}

//		private void FindTheIndexOfTheSegmentThatWillSplit(int overlap,
//			out ArrayList beforeSegments,
//			out Disjunct[][] splitSegmentSplit,
//			out ArrayList afterSegments) 
//		{ 
//			SpecialFunctions.CheckCondition(0 < overlap && overlap < Core.Length); //!!!raise error
// 
//			//Any of these can stay empty
//			beforeSegments = new ArrayList();
//			splitSegmentSplit = new Disjunct[][]{null};
//			afterSegments = new ArrayList();
//
//			int iLengthSoFar = 0; 
//			foreach(Segment segment in SegmentCollection) 
//			{
//				if (iLengthSoFar + segment.Core.Length <= overlap) 
//				{
//					beforeSegments.Add(segment);
//				}
//				else if (iLengthSoFar >= overlap)
//				{
//					afterSegments.Add(segment); 
//				} 
//				else
//				{ 
//					splitSegmentSplit = segment.SplitDisjuncts(overlap - iLengthSoFar);
//				}
//
//				iLengthSoFar += segment.Core.Length;
//			}
//		} 
 
 		////			private PatchRegex Subsequence(int startIndex, int lengthOfSubsequence)
 		//				SpecialFunctions.CheckCondition(0 <= startIndex && startIndex < Length); //!!!raise error 
		//				SpecialFunctions.CheckCondition(0 < startIndex + lengthOfSubsequence && startIndex + lengthOfSubsequence <= Length); //!!!raise error
 		//				ArrayList rgSegment = new ArrayList();
		//			foreach(Segment segmentOld in SegmentCollection)
		//			{
		//				Debug.Assert(lengthOfSubsequence > 0); // real assert
		//				if (segmentOld.Length <= startIndex) 
		//				{ 
 		//					startIndex -= segmentOld.Length;
 		//				} 
		//				else
 		//				{
		//					int lengthForThisSegment = Math.Min(segmentOld.Length - startIndex, lengthOfSubsequence);
		//					Segment segmentNew = segmentOld.Subsequence(startIndex, lengthForThisSegment);
		//					lengthOfSubsequence -= segmentNew.Length;
		//					startIndex = 0; 
		//					rgSegment.Add(segmentNew); 
 		//
 		//					if (lengthOfSubsequence == 0) 
		//					{
 		//						break;
		//					}
		//				}
		//
		//				PatchRegex patchRegex = FinishUpBasedOnSegments(rgSegment, PatchRegexFactory); 
		//				return patchRegex; 
 		//			}
 

 		//			return rgrg;
		//		}

 		static char DisjunctJoiner = '|';
 
		public Disjunct[] DisjunctCollection; //!!!make this private after debugging 

		private static SortedList RemoveSubsumedDisjuncts(SortedList rgDisjunct1) 
		{
			SortedList rgDisjunct2 = new SortedList();
			foreach(Disjunct disjuncti in rgDisjunct1.Values)
 			{
 				bool bIncludeDisjuncti = true;
				foreach(Disjunct disjunctj in rgDisjunct1.Values) 
 				{ 
					if (disjuncti == disjunctj)
					{ 
						continue;
					}
					Debug.Assert(disjuncti.ToString() != disjunctj.ToString()); // real assert
 					if (disjuncti.IsLessGeneralThan(disjunctj))
 					{
						bIncludeDisjuncti = false; 
 					} 
				}
				if (bIncludeDisjuncti) 
				{
					rgDisjunct2.Add(disjuncti.ToString(), disjuncti);
				}
 			}
 			return rgDisjunct2;
		} 
 
 		private void SetStringFromDisjunctCollection()
		{ 
			StringBuilder toStringStringBuilder = new StringBuilder();
			foreach(Disjunct Disjunct in DisjunctCollection)
			{
				if (toStringStringBuilder.Length != 0)
 				{
 					toStringStringBuilder.Append(DisjunctJoiner); 
				} 
 				toStringStringBuilder.Append(Disjunct.FullAsString);
			} 

			String = toStringStringBuilder.ToString();
		}


 
 
	}
 


	public class PatchPatternBuilder
 	{
 		//public AASimilarity AASimilarity;
		public PatchPatternFactory PatchPatternFactory; 
 
 		SortedList DisjunctAASetSequenceCollection = new SortedList();
		int CoreLength = int.MinValue; 
		int CoreLengthSoFar = 0;
		AASetSequence sbCurrentDisjunct = AASetSequence.GetInstance();

		private void SetAndCheckCoreLength()
		{
 			SpecialFunctions.CheckCondition(sbCurrentDisjunct.Count > 0); //!!!raise error 
 			if (CoreLength == int.MinValue) 
			{
 				CoreLength = CoreLengthSoFar; 
			}
			else
			{
				Debug.Assert(CoreLength == CoreLengthSoFar);
			}
 		} 
 
 		public void EndDisjunct()
		{ 
 			SetAndCheckCoreLength();
			DisjunctAASetSequenceCollection[sbCurrentDisjunct.ToString()] = null;
			sbCurrentDisjunct = AASetSequence.GetInstance();
			CoreLengthSoFar = 0;
		}
 
		public PatchPattern ToPatchPattern() 
 		{
 			SpecialFunctions.CheckCondition(DisjunctAASetSequenceCollection.Count > 0); //!!!raise error 
			SpecialFunctions.CheckCondition(sbCurrentDisjunct.Count == 0); //!!!raise error
 			PatchPattern patchPattern = PatchPatternFactory.GetInstance(DisjunctAASetSequenceCollection.Keys);
			return patchPattern;
		}

		public void AppendGroundDisjunct(string s) 
		{ 
			SpecialFunctions.CheckCondition(sbCurrentDisjunct.Count == 0); //!!!raise error
 			foreach(char ch in s) 
 			{
				sbCurrentDisjunct.AppendGround(ch);
 			}
			CoreLengthSoFar += s.Length;
			EndDisjunct();
		} 
 
		public void AppendGround(char c)
		{ 
 			sbCurrentDisjunct.AppendGround(c);
 			++CoreLengthSoFar;
		}
 		public void AppendCanComeFromSet(char c, AASimilarity aaSimilarity)
		{
			SpecialFunctions.CheckCondition(char.IsUpper(c)); //!!!raise error 
			string sCanComeFromSet = aaSimilarity.CanComeFromSet(c); 
			sbCurrentDisjunct.AppendGroundSet(sCanComeFromSet);
			++CoreLengthSoFar; 
 		}
        public void AppendCanGoToSet(char c, AASimilarity aaSimilarity)
        {
            SpecialFunctions.CheckCondition(char.IsUpper(c)); //!!!raise error
            string sCanGoToSet = aaSimilarity.CanGoToSet(c);
            sbCurrentDisjunct.AppendGroundSet(sCanGoToSet); 
            ++CoreLengthSoFar; 
        }
 
 		public void AppendDontCareOrEdge()
		{
 			sbCurrentDisjunct.Append(AASet.OptionalAny);
		}
		public void AppendGroundOrEdge(char ch)
		{ 
			sbCurrentDisjunct.AppendGroundOrEdge(ch); 
		}
 	} 
}


// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
