using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Msr.Mlas.SpecialFunctions; 
using System.Xml; 
using System.Text;
using System.Text.RegularExpressions; 
using System.Collections.Generic;
using EpipredLib;

namespace VirusCount
{
 
 
 	public class HlaToLength
	{ 
		static public bool operator == (HlaToLength a, HlaToLength b)
		{
			if (null == (a as object))
			{
 				return null == (b as object);
 			} 
 
			return a.Equals(b);
 		} 

		static public bool operator != (HlaToLength a, HlaToLength b)
		{
			return !(a == b);
		}
 
 

		static public HlaToLength GetInstanceOrNull(HlaGroup hlaGroup, HlaResolution hlaResolution) 
 		{
 			HlaToLength hlaToLength = HlaToLength.GetInstanceOrNull(hlaGroup.ToString(), hlaResolution);
			return hlaToLength;

 		}
		public override string ToString() 
		{ 
            return HlaResolution.HlaToLengthString(this);
		} 

        public string ToStringWithoutLeading0()
        {
            return HlaResolution.HlaToLengthStringWithLeading0(this);
        }
		 
 
		public override int GetHashCode()
 		{ 
 			return _hlaNumberToLength.GetHashCode() ^ _hlaClass.GetHashCode() ^ HlaResolution.GetHashCode();
		}

 		public override bool Equals(object obj)
		{
			HlaToLength other = obj as HlaToLength; 
			if ((other as object) == null) 
			{
				return false; 
 			}
 			else
			{
 				bool b = other._hlaNumberToLength == _hlaNumberToLength
					&& other._hlaClass == _hlaClass
					&& other.HlaResolution == HlaResolution; 
				return b; 
			}
		} 

        public HlaGroup PossibleHlaGroup()
        {
            return HlaResolution.PossibleHlaGroup(this);
        }
 
 		private HlaToLength() 
 		{
		} 
 		static public HlaToLength GetInstance(string hlaPattern, HlaResolution hlaResolution)
		{
			HlaToLength aHlaToLength = hlaResolution.GetHlaLengthInstance(hlaPattern);
			SpecialFunctions.CheckCondition(aHlaToLength != null);
			return aHlaToLength;
		} 
 
        static public HlaToLength GetInstanceOrNull(string hlaPattern, HlaResolution hlaResolution)
        { 
            return hlaResolution.GetHlaLengthInstance(hlaPattern);
        }

 		static internal HlaToLength GetInstanceTwo(string hlaPattern)
 		{
			HlaToLength aHlaToLength = new HlaToLength(); 
 			aHlaToLength.HlaResolution = HlaResolution.Two; 
            if (HlaResolution.TwoDigitHlaPatternIsOK(hlaPattern))
			{ 
				aHlaToLength._hlaClass = hlaPattern.Substring(0,1);
				aHlaToLength._hlaNumberToLength = int.Parse(hlaPattern.Substring(1));
			}
			else
 			{
 				HlaToLength hlaToLengthFour = GetInstanceFour(hlaPattern); 
				aHlaToLength._hlaClass = hlaToLengthFour.HlaClass; 
 				aHlaToLength._hlaNumberToLength = hlaToLengthFour.TwoDigits();
			} 
			return aHlaToLength;
		}

        static internal HlaToLength GetInstanceFour(string hlaPattern)
		{
			HlaToLength aHlaToLength = new HlaToLength(); 
 			aHlaToLength.HlaResolution = HlaResolution.Four; 
 			aHlaToLength._hlaClass = hlaPattern.Substring(0,1);
            SpecialFunctions.CheckCondition(hlaPattern.Length == 5, String.Format("Expected 4-digit HLA (not {0})", hlaPattern)); 
			aHlaToLength._hlaNumberToLength = int.Parse(hlaPattern.Substring(hlaPattern.Length-4));
 			return aHlaToLength;
		}


 
        static internal HlaToLength GetInstanceBMixed(string hlaPattern) 
        {
            HlaToLength aHlaToLength = new HlaToLength(); 
            aHlaToLength.HlaResolution = HlaResolution.BMixed;

            if (HlaResolution.TwoDigitHlaPatternIsOK(hlaPattern))
            {
                aHlaToLength._hlaClass = hlaPattern.Substring(0, 1);
                aHlaToLength._hlaNumberToLength = int.Parse(hlaPattern.Substring(1)); 
                SpecialFunctions.CheckCondition(aHlaToLength._hlaClass != "B" || aHlaToLength._hlaNumberToLength != 15); //!!!raise error 
            }
            else 
            {
                HlaToLength hlaToLengthFour = GetInstanceFour(hlaPattern);
                int twoDigits = hlaToLengthFour.TwoDigits();
                if (twoDigits == 15 && hlaToLengthFour.HlaClass == "B")
                {
                    aHlaToLength._hlaClass = hlaToLengthFour.HlaClass; 
                    aHlaToLength._hlaNumberToLength = hlaToLengthFour._hlaNumberToLength; 
                }
                else 
                {
                    aHlaToLength._hlaClass = hlaToLengthFour.HlaClass;
                    aHlaToLength._hlaNumberToLength = twoDigits;
                }
            }
            return aHlaToLength; 
 
        }
 
		static public HlaToLength GetInstanceABMixed(string hlaPattern)
		{
			HlaToLength aHlaToLength = new HlaToLength();
			aHlaToLength.HlaResolution = HlaResolution.ABMixed;

 			if (HlaResolution.TwoDigitHlaPatternIsOK(hlaPattern)) 
 			{ 
				aHlaToLength._hlaClass = hlaPattern.Substring(0, 1);
 				aHlaToLength._hlaNumberToLength = int.Parse(hlaPattern.Substring(1)); 
				SpecialFunctions.CheckCondition(aHlaToLength._hlaClass != "B" || aHlaToLength._hlaNumberToLength != 15); //!!!raise error
				SpecialFunctions.CheckCondition(aHlaToLength._hlaClass != "A" || aHlaToLength._hlaNumberToLength != 68); //!!!raise error
			}
			else
			{
 				HlaToLength hlaToLengthFour = GetInstanceFour(hlaPattern); 
 				int twoDigits = hlaToLengthFour.TwoDigits(); 
				if ((twoDigits == 15 && hlaToLengthFour.HlaClass == "B") || (twoDigits == 68 && hlaToLengthFour.HlaClass == "A"))
 				{ 
					aHlaToLength._hlaClass = hlaToLengthFour.HlaClass;
					aHlaToLength._hlaNumberToLength = hlaToLengthFour._hlaNumberToLength;
				}
				else
				{
 					aHlaToLength._hlaClass = hlaToLengthFour.HlaClass; 
 					aHlaToLength._hlaNumberToLength = twoDigits; 
				}
 			} 
			return aHlaToLength;

		}

		public bool Consistant(HlaGroup hlaGroup)
		{ 
			bool b = (this == HlaToLength.GetInstanceOrNull(hlaGroup, HlaResolution)); 
 			return b;
 		} 




		private string _hlaClass;
 		private int _hlaNumberToLength; 
        public HlaResolution HlaResolution; 

		public int TwoDigits() 
		{
            return HlaResolution.TwoDigits(_hlaNumberToLength);
		}

		public int HlaNumberToLength
		{ 
 			get 
 			{
				return _hlaNumberToLength; 
 			}
		}
		public string HlaClass
		{
			get
			{ 
 				return _hlaClass; 
 			}
		} 


        static Dictionary<string, string> ABMixedToZero6SupertypeBlanksTable = CreateABMixedToZero6SupertypeTable("Zero6Supertypes.txt", true);
        public static Dictionary<string, string> ABMixedToZero6SupertypeNoBlanksTable = CreateABMixedToZero6SupertypeTable("Zero6SupertypesNoBlanks.txt", false);
        public static Dictionary<string, string> SupertypeTableFromWorkingDirectory = null;
 
 

        static public Dictionary<string, string> CreateABMixedToZero6SupertypeTable(string fileName, bool checkABMixed) 
        {

            Dictionary<string, string> aBMixedToZero6SupertypeTable = new Dictionary<string, string>();
            foreach (Dictionary<string, string> row in Predictor.TabFileTableAsList(fileName, "hla	supertype", false))
            {
                string abMixedHlaToLengthString = row["hla"]; 
                SpecialFunctions.CheckCondition(!checkABMixed || HlaResolution.ABMixedHlaPatternIsOK(abMixedHlaToLengthString)); 
                if (row["supertype"] != "")
                { 
                    aBMixedToZero6SupertypeTable.Add(abMixedHlaToLengthString, row["supertype"]);
                }
            }
            return aBMixedToZero6SupertypeTable;
        }
 
 

        public string ToZero6SupertypeBlanksString() 
        {
            SpecialFunctions.CheckCondition(HlaResolution == HlaResolution.ABMixed, "Currently, supertype is only defined for ABMixed");
            string asString = ToString();
            return ToZero6SupertypeBlanksString(asString);
        }
 
        public string ToZero6SupertypeNoBlanksString() 
        {
            SpecialFunctions.CheckCondition(HlaResolution == HlaResolution.ABMixed, "Currently, supertype is only defined for ABMixed"); 
            string asString = ToString();
            return ToZero6SupertypeNoBlanksString(asString);
        }


        public static string ToZero6SupertypeNoBlanksString(string hlaName) 
        { 
            if (ABMixedToZero6SupertypeNoBlanksTable.ContainsKey(hlaName))
            { 
                return ABMixedToZero6SupertypeNoBlanksTable[hlaName];
            }
            else
            {
                return "none";
            } 
        } 

 
        public static string ToZero6SupertypeBlanksString(string hlaName)
        {
            if (ABMixedToZero6SupertypeBlanksTable.ContainsKey(hlaName))
 			{
                return ABMixedToZero6SupertypeBlanksTable[hlaName];
			} 
			else 
			{
                return "unknown"; //!!!really a misnomer should be "none" or null, but don't want to change it because it is part of useful models 
			}
       }

       internal static string ToSupertypeFromWorkingDirectoryString(string hlaName)
       {
           if (null == SupertypeTableFromWorkingDirectory) 
           { 
               SupertypeTableFromWorkingDirectory = CreateSupertypeTableFromWorkingDirectory("HlaToSupertype.txt");
           } 
           SpecialFunctions.CheckCondition(SupertypeTableFromWorkingDirectory.ContainsKey(hlaName), string.Format("Hla ({0}) not found in supertype table from working directory.", hlaName));
           return SupertypeTableFromWorkingDirectory[hlaName];

       }

        private static Dictionary<string, string> CreateSupertypeTableFromWorkingDirectory(string fileName) 
        { 

            Dictionary<string, string> hlaToSupertype = new Dictionary<string, string>(); 
            SpecialFunctions.CheckCondition(File.Exists(fileName), string.Format("The file '{0}' was not found in the working directory, {1}", fileName, Directory.GetCurrentDirectory()));
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(fileName, "hla	supertype", false))
            {
                string hla = row["hla"];
                hlaToSupertype.Add(hla, row["supertype"]);
            } 
            return hlaToSupertype; 
        }
   } 

}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
