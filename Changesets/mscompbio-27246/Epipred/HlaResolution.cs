using System; 
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Msr.Mlas.SpecialFunctions; 
 
namespace VirusCount
{ 
 	public abstract class HlaResolution
	{
		internal HlaResolution()
		{
		}
 
		public static HlaResolution Two = new TwoDigitHlaResolution(); 
 		public static HlaResolution Four = new FourDigitHlaResolution();
 		public static HlaResolution BMixed = new BMixedHlaResolution(); 
		public static HlaResolution ABMixed = new ABMixedHlaResolution();


 		abstract public string ShortName { get;     }

		public abstract string HlaToLengthString(HlaToLength hlaToLength); 
 

		public abstract string HlaToLengthStringWithLeading0(HlaToLength hlaToLength); 

		public abstract HlaGroup PossibleHlaGroup(HlaToLength hlaToLength);

		public abstract HlaToLength GetHlaLengthInstance(string hlaPattern);

 
		public abstract bool HlaPatternIsOK(string hlaPattern); 

 		//!!!would be nice to use regular expressions 


 		static private Regex BMixedRegex = new Regex("^(([ABC][0-9][0-9])|(B15[0-9][0-9]))$");
		public static bool BMixedHlaPatternIsOK(string hlaPattern)
 		{
			if (hlaPattern == "B15") 
			{ 
				return false;
			} 
			return BMixedRegex.IsMatch(hlaPattern);
 		}

        static private Regex ABMixedRegex = new Regex("^(([ABCM][0-9][0-9])|(B15[0-9][0-9])|(A68[0-9][0-9]))$");
 		public static bool ABMixedHlaPatternIsOK(string hlaPattern)
		{ 
 			if (hlaPattern == "B15" || hlaPattern == "A68") 
			{
				return false; 
			}
			return ABMixedRegex.IsMatch(hlaPattern);
		}

 		internal static bool FourDigitHlaPatternIsOK(string hlaPattern)
 		{ 
			bool b; 
 			if (hlaPattern.Length == 5)
			{ 
				b = "ABC".IndexOf(hlaPattern[0]) >= 0 && char.IsDigit(hlaPattern[1]) && char.IsDigit(hlaPattern[2]) && char.IsDigit(hlaPattern[3]) && char.IsDigit(hlaPattern[4]); //!!!const
			}
			else
			{
 				b = hlaPattern.Length == 6 && "ABC".IndexOf(hlaPattern[0]) >= 0 && hlaPattern[1] == '*' && char.IsDigit(hlaPattern[2]) && char.IsDigit(hlaPattern[3]) && char.IsDigit(hlaPattern[4]) && char.IsDigit(hlaPattern[5]); //!!!const
 			} 
			return b; 
 		}
 
		internal static bool TwoDigitHlaPatternIsOK(string hlaPattern)
		{
			if (hlaPattern.Length == 2)
			{
				return "ABCM".IndexOf(hlaPattern[0]) >= 0 && char.IsDigit(hlaPattern[1]);
 			} 
 			else if (hlaPattern.Length == 3) 
			{
 				return "ABCM".IndexOf(hlaPattern[0]) >= 0 && char.IsDigit(hlaPattern[1]) && char.IsDigit(hlaPattern[2]); 
			}
			else
			{
				return false;
			}
 		} 
 

 		internal abstract int TwoDigits(int hlaNumberToLength); 

		abstract public System.Collections.ArrayList HlaToLength4(LanlEpitope aLanlEpitope);



 
 		public abstract HlaToLength GetHlaLengthInstanceWithFixup(string name); 
	}
 
	public class TwoDigitHlaResolution : HlaResolution
	{
		internal TwoDigitHlaResolution()
		{
 		}
 		public override string ShortName 
		{ 
 			get
			{ 
				return "2";
			}
		}

		public override string HlaToLengthString(HlaToLength hlaToLength)
 		{ 
 			return string.Format("{0}{1:00}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength); 
		}
 
 		public override string HlaToLengthStringWithLeading0(HlaToLength hlaToLength)
		{
			return string.Format("{0}{1}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
		}

		public override HlaGroup PossibleHlaGroup(HlaToLength hlaToLength) 
		{ 
 			return HlaGroup.GetInstance(hlaToLength.HlaClass, hlaToLength.HlaNumberToLength * 100);
 		} 

		public override HlaToLength GetHlaLengthInstance(string hlaPattern)
 		{
			if (!HlaPatternIsOK(hlaPattern))
			{
				return null; 
			} 

			return HlaToLength.GetInstanceTwo(hlaPattern); 
 			//return GetInstanceFour(hlaPattern);
 		}

		public override bool HlaPatternIsOK(string hlaPattern)
 		{
			return FourDigitHlaPatternIsOK(hlaPattern) || TwoDigitHlaPatternIsOK(hlaPattern); 
		} 

		internal override int TwoDigits(int hlaNumberToLength) 
		{
			return hlaNumberToLength;
 		}


 		public override ArrayList HlaToLength4(LanlEpitope aLanlEpitope) 
		{ 
            throw new Exception("The commented out version of this code requires an implict set of HLAs");
            //ArrayList rg = new ArrayList(); 
            //foreach (HlaToLength hlaToLength4 in FourDigitPossiblities(aLanlEpitope.HlaToLength))
            //{
            //    rg.Add(hlaToLength4);
            //}
            //return rg;
 		} 
 

        //// This could be precomputed to thus made faster 
        //static private ArrayList FourDigitPossiblities(HlaToLength hlaToLength2)
        //{
        //    ArrayList rg = new ArrayList();

        //    Debug.Assert(hlaToLength2.HlaResolution == HlaResolution.Two); // real assert
        //    foreach (HlaGroup hlaGroup in study.HlaGroupCollection.Values) 
        //    { 
        //        if (hlaToLength2.Consistant(hlaGroup))
        //        { 
        //            HlaToLength hlaToLength4 = HlaToLength.GetInstanceOrNull(hlaGroup, HlaResolution.Four);
        //            Debug.Assert(hlaToLength4 != null); // real assert
        //            rg.Add(hlaToLength4);
        //        }
        //    }
        //    return rg; 
        //} 

 

		public override HlaToLength GetHlaLengthInstanceWithFixup(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	} 
 
 	public class FourDigitHlaResolution : HlaResolution
 	{ 
		internal FourDigitHlaResolution()
 		{
		}
		public override string ShortName
		{
			get 
			{ 
 				return "4";
 			} 
		}
 		public override string HlaToLengthString(HlaToLength hlaToLength)
		{
			return string.Format("{0}{1:0000}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
		}
		public override string HlaToLengthStringWithLeading0(HlaToLength hlaToLength) 
		{ 
 			return string.Format("{0}{1}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
 		} 
		public override HlaGroup PossibleHlaGroup(HlaToLength hlaToLength)
 		{
			return HlaGroup.GetInstance(hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
		}

 
		public override HlaToLength GetHlaLengthInstance(string hlaPattern) 
		{
			if (!HlaPatternIsOK(hlaPattern)) 
 			{
 				return null;
			}

 			return HlaToLength.GetInstanceFour(hlaPattern);
		} 
 
		public override bool HlaPatternIsOK(string hlaPattern)
		{ 
			return FourDigitHlaPatternIsOK(hlaPattern);
		}

 		internal override int TwoDigits(int hlaNumberToLength)
 		{
			string sFourDigits = string.Format("{0:0000}", hlaNumberToLength); 
 			string s1st2Digits = sFourDigits.Substring(0, 2); 
			int iAsNumber = int.Parse(s1st2Digits);
			return iAsNumber; 
		}


		public override System.Collections.ArrayList HlaToLength4(LanlEpitope aLanlEpitope)
		{
 			ArrayList rg = new ArrayList(); 
 			rg.Add(aLanlEpitope.HlaToLength); 
			return rg;
 		} 

		static Regex HlaParseRegex = new Regex(@"HLA-(?<class>A|B|C[Ww])\*{0,1}(?<number>[0-9]{1,4})");
		public override HlaToLength GetHlaLengthInstanceWithFixup(string name)
		{
			Match aMatch = HlaParseRegex.Match(name);
			SpecialFunctions.CheckCondition(aMatch.Success); 
 			string theClass = aMatch.Groups["class"].Value; 
 			string num = aMatch.Groups["number"].Value;
			string s = null; 
 			if (num.Length == 4)
			{
				s = string.Format("{0}{1}", theClass[0], num);
			}
			else
			{ 
 				SpecialFunctions.CheckCondition(false); 
 			}
			return GetHlaLengthInstance(s); 
 			/*
			HLA-A*0201
			HLA-A*1101
			HLA-B*0801
			HLA-B*2705
			HLA-B*2709 
 			HLA-B*3501 
 			HLA-B*4402
			HLA-B*4403 
 			HLA-B*5101
			HLA-B*5301
			HLA-CW*3
			HLA-CW*4
			*/
 
		} 
 	}
 
 	public class BMixedHlaResolution : HlaResolution
	{
 		internal BMixedHlaResolution()
		{
		}
		public override string ShortName 
		{ 
			get
 			{ 
 				return "B";
			}
 		}

		internal bool IsB15(HlaToLength hlaToLength)
		{ 
			return hlaToLength.HlaClass == "B" && TwoDigits(hlaToLength.HlaNumberToLength) == 15; 
		}
		public override string HlaToLengthString(HlaToLength hlaToLength) 
 		{
 			if (IsB15(hlaToLength))
			{
 				return string.Format("{0}{1:0000}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
			}
			else 
			{ 
				return string.Format("{0}{1:00}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
			} 

 		}
 		public override string HlaToLengthStringWithLeading0(HlaToLength hlaToLength)
		{
 			return string.Format("{0}{1}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
		} 
		public override HlaGroup PossibleHlaGroup(HlaToLength hlaToLength) 
		{
			if (IsB15(hlaToLength)) 
			{
                //return HlaGroup.GetInstance(hlaToLength.HlaClass, hlaToLength.HlaNumberToLength * 100);
 				return HlaGroup.GetInstance(hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
 			}
			else
 			{ 
				return HlaGroup.GetInstance(hlaToLength.HlaClass, hlaToLength.HlaNumberToLength * 100); 
			}
		} 


		public override HlaToLength GetHlaLengthInstance(string hlaPattern)
        {
			if (!HlaPatternIsOK(hlaPattern))
 			{ 
 				Debug.Fail("fix up"); 
				return null;
 			} 

			return HlaToLength.GetInstanceBMixed(hlaPattern);
		}


 
		public override bool HlaPatternIsOK(string hlaPattern) 
		{
			return BMixedHlaPatternIsOK(hlaPattern); 
 		}

 		internal override int TwoDigits(int hlaNumberToLength)
		{
 			string sFourDigit = string.Format("{0:0000}", hlaNumberToLength);
			string s1st2Digits = sFourDigit.Substring(0, 2); 
			int iAsNumber = int.Parse(s1st2Digits); 
			return iAsNumber;
		} 


		public override ArrayList HlaToLength4(LanlEpitope aLanlEpitope)
 		{
 			Debug.Fail("Need code");
			ArrayList rg = new ArrayList(); 
 			rg.Add(aLanlEpitope.HlaToLength); 
			return rg;
		} 

		internal static string HlaGroupString(HlaGroup hlaGroup)
		{
			int twoDigits = hlaGroup.TwoDigits();
 			if (twoDigits == 15)
 			{ 
				return hlaGroup.ToString(); 
 			}
			else 
			{
				return string.Format("{0}{1:00}", hlaGroup.HlaClass, twoDigits);
			}
		}

 		static Regex HlaParseRegex = new Regex(@"HLA-(?<class>A|B|CW)\*(?<number>[0-9]{1,4})"); 
 		public override HlaToLength GetHlaLengthInstanceWithFixup(string name) 
		{
 			Match aMatch = HlaParseRegex.Match(name); 
			SpecialFunctions.CheckCondition(aMatch.Success);
			string theClass = aMatch.Groups["class"].Value;
			string num = aMatch.Groups["number"].Value;
			string s = null;
			if (theClass == "B" && num.StartsWith("15"))
 			{ 
 				SpecialFunctions.CheckCondition(num.Length == 4); 
				s = theClass + num;
 			} 
			else if (num.Length == 1)
			{
				s = string.Format("{0}0{1}", theClass[0], num);
			}
			else if (num.Length == 2)
 			{ 
 				s = string.Format("{0}{1}", theClass[0], num); 
			}
 			else if (num.Length == 4) 
			{
				s = string.Format("{0}{1}", theClass[0], num.Substring(0, 2));
			}
			else
			{
 				SpecialFunctions.CheckCondition(false); 
 			} 
			return GetHlaLengthInstance(s);
 			/* 
			HLA-A*0201
			HLA-A*1101
			HLA-B*0801
			HLA-B*2705
			HLA-B*2709
 			HLA-B*3501 
 			HLA-B*4402 
			HLA-B*4403
 			HLA-B*5101 
			HLA-B*5301
			HLA-CW*3
			HLA-CW*4
			*/

		} 
 	} 

 	public class ABMixedHlaResolution : HlaResolution 
	{
 		internal ABMixedHlaResolution()
		{
		}
		public override string ShortName
		{ 
			get 
 			{
 				return "AB"; 
			}
 		}

		private bool IsA68OrB15(HlaToLength hlaToLength)
		{
			return IsA68(hlaToLength) || IsB15(hlaToLength); 
		} 
		//!!!some code in common with BMixed
 		internal bool IsB15(HlaToLength hlaToLength) 
 		{
			return hlaToLength.HlaClass == "B" && TwoDigits(hlaToLength.HlaNumberToLength) == 15;
 		}
		private bool IsA68(HlaToLength hlaToLength)
		{
			return hlaToLength.HlaClass == "A" && TwoDigits(hlaToLength.HlaNumberToLength) == 68; 
		} 

		public override string HlaToLengthString(HlaToLength hlaToLength) 
 		{
 			if (IsA68OrB15(hlaToLength))
			{
 				return string.Format("{0}{1:0000}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
			}
			else 
			{ 
				return string.Format("{0}{1:00}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
			} 

 		}
 		public override string HlaToLengthStringWithLeading0(HlaToLength hlaToLength)
		{
 			return string.Format("{0}{1}", hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
		} 
		public override HlaGroup PossibleHlaGroup(HlaToLength hlaToLength) 
		{
			if (IsA68OrB15(hlaToLength)) 
			{
 				return HlaGroup.GetInstance(hlaToLength.HlaClass, hlaToLength.HlaNumberToLength);
 			}
			else
 			{
				return HlaGroup.GetInstance(hlaToLength.HlaClass, hlaToLength.HlaNumberToLength * 100); 
			} 
		}
 

		public override HlaToLength GetHlaLengthInstance(string hlaPattern)
		{
 			if (!HlaPatternIsOK(hlaPattern))
 			{
				Debug.Fail("fix up"); 
 				return null; 
			}
 
			return HlaToLength.GetInstanceABMixed(hlaPattern);
		}



		public override bool HlaPatternIsOK(string hlaPattern) 
		{ 
 			return ABMixedHlaPatternIsOK(hlaPattern);
 		} 

		internal override int TwoDigits(int hlaNumberToLength)
 		{
			string sFourDigit = string.Format("{0:0000}", hlaNumberToLength);
			string s1st2Digits = sFourDigit.Substring(0, 2);
			int iAsNumber = int.Parse(s1st2Digits); 
			return iAsNumber; 
		}
 

 		public override ArrayList HlaToLength4(LanlEpitope aLanlEpitope)
 		{
			Debug.Fail("Need code");
 			ArrayList rg = new ArrayList();
			rg.Add(aLanlEpitope.HlaToLength); 
			return rg; 
		}
 
		internal static string XHlaGroupString(HlaGroup hlaGroup)
		{
 			int twoDigits = hlaGroup.TwoDigits();
 			if (twoDigits == 15)
			{
 				return hlaGroup.ToString(); 
			} 
			else
			{ 
				return string.Format("{0}{1:00}", hlaGroup.HlaClass, twoDigits);
			}
 		}

 		static Regex HlaParseRegex = new Regex(@"HLA-(?<class>A|B|CW)\*(?<number>[0-9]{1,4})");
		public override HlaToLength GetHlaLengthInstanceWithFixup(string name) 
 		{ 
			Match aMatch = HlaParseRegex.Match(name);
			SpecialFunctions.CheckCondition(aMatch.Success); 
			string theClass = aMatch.Groups["class"].Value;
			string num = aMatch.Groups["number"].Value;
			string s = null;
 			if ((theClass == "B" && num.StartsWith("15")) || (theClass == "A" && num.StartsWith("68")))
 			{
				SpecialFunctions.CheckCondition(num.Length == 4); 
 				s = theClass + num; 
			}
			else if (num.Length == 1) 
			{
				s = string.Format("{0}0{1}", theClass[0], num);
			}
 			else if (num.Length == 2)
 			{
				s = string.Format("{0}{1}", theClass[0], num); 
 			} 
			else if (num.Length == 4)
			{ 
				s = string.Format("{0}{1}", theClass[0], num.Substring(0, 2));
			}
			else
 			{
 				SpecialFunctions.CheckCondition(false);
			} 
 			return GetHlaLengthInstance(s); 
			/*
			HLA-A*0201 
			HLA-A*1101
			HLA-B*0801
			HLA-B*2705
 			HLA-B*2709
 			HLA-B*3501
			HLA-B*4402 
 			HLA-B*4403 
			HLA-B*5101
			HLA-B*5301 
			HLA-CW*3
			HLA-CW*4
			*/

 		}
 	} 
 
}
 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
