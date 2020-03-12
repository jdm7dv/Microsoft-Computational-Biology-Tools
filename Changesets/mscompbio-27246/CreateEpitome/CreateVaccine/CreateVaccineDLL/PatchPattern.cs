using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using Msr.Mlas.SpecialFunctions; 
using System.Collections.Generic; 

namespace VirusCount 
{
 	abstract public class PatchPatternFactory
	{
		protected PatchPatternFactory()
		{
		} 
 
		static public PatchPatternFactory GetFactory(string language)
 		{ 
            SpecialFunctions.CheckCondition(language == "strings", "patch language must be 'strings'");
 			PatchPatternFactory patchPatternFactory = new PatchStringFactory();
			return patchPatternFactory;
 		}

		public abstract PatchPattern GetInstance(string expression); 
		public abstract PatchPattern GetInstance(ICollection disjunctStringCollection); 
	}
 
	public class PatchStringFactory : PatchPatternFactory
	{
 		public PatchStringFactory()
 		{
		}
 
        private Dictionary<string, PatchPattern> ExpressionToPatchPattern = new Dictionary<string, PatchPattern>(); 

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
			if (ExpressionToPatchPattern.ContainsKey(expression))
			{
				PatchPattern patchPattern = (PatchPattern) ExpressionToPatchPattern[expression];
 				return patchPattern; 
 			} 
			else
 			{ 
				PatchPattern patchPattern = new PatchString(expression, this);
				SpecialFunctions.CheckCondition(patchPattern.ToString() == expression, "PatchPattern expression is not in standard form."); //!!!raise error
				ExpressionToPatchPattern.Add(expression, patchPattern);
				return patchPattern;
			}
 		} 
 
 	}
				 
 	abstract public class PatchPattern
	{
		abstract public string CoreRealization();
		abstract public string FullRealization();
		abstract public int CoreLength {get;}
		abstract public int FullLength {get;} 
 		abstract public int MaxLength {get;} 

 		abstract public bool IsMatch(string vaccineString); 
		abstract public PatchPattern UnifyOrNull(PatchPattern other);
 		abstract public PatchPattern UnifyOnRightOrNull(int overlap, PatchPattern other);

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
 
} 


// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
