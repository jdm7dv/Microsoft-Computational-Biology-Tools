using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic; 
using System.Text.RegularExpressions; 
using Msr.Mlas.SpecialFunctions;
 
namespace VirusCount
{
 	[Serializable()]
	public class HlaGroup
	{
		//!!!rename Class to Locus 
		//!!!rename HLa to HlaAllele 
        static public HlaGroup GetInstance(string hlaClass, int hlaNumber)
        { 
            string name = CreateName(hlaClass, hlaNumber);
            if (SingletonHlaCollection.ContainsKey(name))
            {
                return SingletonHlaCollection[name];
            }
            HlaGroup hlaGroup = new HlaGroup(hlaClass, hlaNumber); 
            SingletonHlaCollection.Add(hlaGroup.ToString(),hlaGroup); 
            return hlaGroup;
        } 

        static public HlaGroup GetInstance(string name)
        {
            Match aMatch = Pattern.Match(name);
            Debug.Assert(aMatch.Success); // real assert
            string hlaClass = aMatch.Groups["class"].Value; 
            int hlaNumber = int.Parse(aMatch.Groups["number"].Value); 
            return GetInstance(hlaClass, hlaNumber);
        } 

		private HlaGroup(string hlaClass, int hlaNumber)
 		{
            string name = CreateName(hlaClass, hlaNumber);
            SpecialFunctions.CheckCondition(Pattern.Match(name).Success); //!!!raise error
 			_hlaClass = hlaClass; 
			_hlaNumber = hlaNumber; 
 		}
 
        static Regex Pattern = new Regex("^(?<class>[ABC]|(DRB1)|(DQB1))(?<number>[0-9]{4,4})$");

        static Dictionary<string, HlaGroup> SingletonHlaCollection = new Dictionary<string, HlaGroup>();


 
        private string _hlaClass; 

        public string HlaClass 
        {
            get { return _hlaClass; }
        }
        private int _hlaNumber;

        public int HlaNumber 
        { 
            get { return _hlaNumber; }
        } 
		public int TwoDigits()
		{
			int iTopTwo = _hlaNumber / 100;
			return iTopTwo;
		}
 		public Hashtable PatientList = new Hashtable(); 
 
 		override public string ToString()
		{ 
            return CreateName(_hlaClass, _hlaNumber);
 		}

        private static string CreateName(string hlaClass, int hlaNumber)
        {
            string sKey = string.Format("{0}{1:0000}", hlaClass, hlaNumber); 
            return sKey; 
        }
 
		
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
