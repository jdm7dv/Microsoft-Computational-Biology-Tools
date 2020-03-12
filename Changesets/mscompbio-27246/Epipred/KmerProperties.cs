using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Msr.Mlas.SpecialFunctions;
using EpipredLib; 
 
namespace VirusCount
{ 
    /// <summary>
    /// Summary description for KmerProperties.
    /// </summary>
    public class KmerProperties
    {
        //!!!! the code would be nicer if this B/E/A length was in its own class 
        public Dictionary<string, bool[]> AABits; 
        public Dictionary<string, int> AANumber;
        //public Dictionary<int,string> NumberToAA; 
        public string[] AAPropertyCollection;
        public Dictionary<string, int> PropertyToNumber;

        static KmerProperties Singleton = null;

        static public KmerProperties GetInstance() 
        { 
            if (Singleton == null)
            { 
                Singleton = new KmerProperties();
                Singleton.Init();
            }

            return Singleton;
        } 
 
        public Dictionary<string, Set<char>> _propertyToAACharSet = null;
        public Dictionary<string, Set<char>> PropertyToAACharSet 
        {
            get
            {
                if (null == _propertyToAACharSet)
                {
                    _propertyToAACharSet = new Dictionary<String, Set<Char>>(); 
                    foreach (string property in AAPropertyCollection) 
                    {
                        Set<char> aaSet = PropertyNameToAASet(property); 
                        _propertyToAACharSet.Add(property, aaSet);
                    }

                }
                return _propertyToAACharSet;
            } 
        } 

        public bool DoesAminoAcidHaveProperty(string aminoAcid, string property) 
        {
            bool[] rgBits = AABits[aminoAcid];
            int iProperty = PropertyToNumber[property];
            bool v = rgBits[iProperty];
            return v;
        } 
 
        private void Init()
        { 
            AANumber = new Dictionary<string, int>();
            AABits = new Dictionary<string, bool[]>();
            //NumberToAA = new Dictionary<int, string>();
            PropertyToNumber = new Dictionary<string, int>();

            string sFile = @"properties.txt"; 
            string propertyListString = "cyclic	aliphatic	aromatic	buried	hydrophobic	large	medium	small	negative	positive	charged	polar"; 

            AAPropertyCollection = propertyListString.Split('\t'); 
            for (int i = 0; i < AAPropertyCollection.Length; ++i)
            {
                PropertyToNumber.Add(AAPropertyCollection[(int)i], i);
            }

 
            string header = @"Residues:	Atom Set	" + propertyListString; 
            int iIndex = 0;
            foreach (Dictionary<string, string> row in Predictor.TabFileTable(sFile, header, false, '\t')) 
            {
                string sAminoAcid = ToMixedCase(row["Residues:"]);
                ++iIndex;
                AANumber.Add(sAminoAcid, iIndex);
                //NumberToAA.Add(iIndex, sAminoAcid);
                Debug.Assert(sAminoAcid.Length == 3); 
                bool[] rgBits = new bool[AAPropertyCollection.Length]; 
                for (int iProperty = 0; iProperty < AAPropertyCollection.Length; ++iProperty)
                { 
                    string property = AAPropertyCollection[iProperty];
                    string value = row[property];
                    SpecialFunctions.CheckCondition(value == "" || value == "*");
                    rgBits[iProperty] = (value == "*");
                }
                AABits.Add(sAminoAcid, rgBits); 
            } 

        } 

        private string ToMixedCase(string s)
        {
            if (s.Length > 1)
            {
                return s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower(); 
            } 
            else if (s.Length == 1)
            { 
                return s.ToUpper();
            }
            else
            {
                Debug.Assert(s == "");
                return s; 
            } 
        }
 


        private KmerProperties()
        {
            //
            // TODO: Add constructor logic here 
            // 
        }
 
        private bool? GoalValue(string sAminoAcid, int iProperty)
        {
            SpecialFunctions.CheckCondition(sAminoAcid != "STOP"); //!!!raise error
            if (sAminoAcid == "DELETED")
            {
                return null; 
            } 

            bool[] rgBits = AABits[sAminoAcid]; 
            bool goalValue = rgBits[iProperty];
            return goalValue;
        }

        public string Evaluate(KmerDefinition kmerDefinition,
            string featureString, bool isPositive, int rowId, int groupId, string sOperator, 
            string[] rgKmer, HlaGroup hlaGroup/*, int lengthOfOriginalLanlEpitopeOrLongMin*/) 
        {
            for (int iProperty = 0; iProperty < AAPropertyCollection.Length; ++iProperty) 
            {
                string sAAProperty = (string)AAPropertyCollection[iProperty];
                if (featureString.StartsWith(sAAProperty))
                {
                    char chBEA;
                    int iPos = GetPos(kmerDefinition, featureString, out chBEA); 
                    SpecialFunctions.CheckCondition(iPos != -1); //!!! raise error 
                    string sAminoAcid = rgKmer[iPos];
                    bool? goalValue = GoalValue(sAminoAcid, iProperty); 
                    switch (goalValue)
                    {
                        case true:
                            return "1";
                        case false:
                            return "0"; 
                        case null: 
                            return "#";
                    } 
                }
            }
            //!!!switch to switch
            if (featureString == "RowId")
            {
                return rowId.ToString(); 
            } 
            if (featureString == "Operator")
            { 
                return sOperator;
            }
            if (featureString == "GroupId")
            {
                return groupId.ToString();
            } 
            if (featureString == "Immunogenic") 
            {
                return isPositive ? "1" : "0"; 
            }
            //if (featureString == "KmerLength")
            //{
            //    Debug.Assert(lengthOfOriginalLanlEpitopeOrLongMin != int.MinValue);
            //    return lengthOfOriginalLanlEpitopeOrLongMin.ToString();
            //} 
            if (featureString.StartsWith("amino")) 
            {
                char chBEA; 
                int iPos = GetPos(kmerDefinition, featureString, out chBEA);
                SpecialFunctions.CheckCondition(iPos != -1); //!!! raise error
                string sAminoAcid = rgKmer[iPos];
                string sGoalValue = AANumberAsGoalValue(sAminoAcid);
                return sGoalValue;
            } 
            //if(featureString == "HLA") //HLA,HLA2digits,HLALetter //HLA-A*0201,HLA-A*02,A 
            //{
            //    string sGoalValue = string.Format("HLA-{0}*{1:0000}", hlaGroup.HlaClass, hlaGroup.HlaNumber); 
            //    return sGoalValue;
            //}
            //if(featureString == "HLA2digits") //HLA,HLA2digits,HLALetter //HLA-A*0201,HLA-A*02,A
            //{
            //    string sGoalValue = string.Format("HLA-{0}*{1:00}",hlaGroup.HlaClass, hlaGroup.TwoDigits());
            //    return sGoalValue; 
            //} 
            //else if(featureString == "HLALetter") //HLA,HLA2digits,HLALetter //HLA-A*0201,HLA-A*02,A
            //{ 
            //    return hlaGroup.HlaClass;
            //}
            //if (featureString == "HLAHarvard") //HLA,HLA2digits,HLALetter //HLA-A*0201,HLA-A*02,A
            //{
            //    return BMixedHlaResolution.HlaGroupString(hlaGroup);
            //} 
            //if (featureString.StartsWith(OldStyleEpitopeTrainingData.HlaHarvardFeatureString)) //!!!const 
            //{
            //    string goal = featureString.Substring(OldStyleEpitopeTrainingData.HlaHarvardFeatureString.Length); 
            //    string value = BMixedHlaResolution.HlaGroupString(hlaGroup);
            //    return (goal == value) ? "1" : "0";

            //}
            //if (featureString == "Supertype") //HLA,HLA2digits,HLALetter //HLA-A*0201,HLA-A*02,A
            //{ 
            //    return SupertypeMapping.HlaGroupString(hlaGroup); 
            //}
            Debug.Assert(false); //!!!need code 
            return null;

        }

        private string AANumberAsGoalValue(string sAminoAcid)
        { 
            SpecialFunctions.CheckCondition(sAminoAcid != "STOP"); //!!!raise error 
            if (sAminoAcid == "DELETED")
            { 
                return "#";
            }
            else
            {
                string sAANumber = AANumber[sAminoAcid].ToString();
                return sAANumber; 
            } 
        }
 

        public int GetPos(KmerDefinition kmerDefinition, string sVariable, out char chBEA)
        {
            int iLetter;
            for (iLetter = sVariable.Length - 1; iLetter >= 0; --iLetter)
            { 
                if (char.IsLetter(sVariable[(int)iLetter])) 
                {
                    break; 
                }
            }
            Debug.Assert(0 < iLetter && iLetter < sVariable.Length - 1); // real assert
            chBEA = sVariable[(int)iLetter];
            int iNum = int.Parse(sVariable.Substring((int)iLetter + 1));
 
            int iPos; 
            switch (chBEA)
            { 
                case 'B':
                    {
                        // e.g.KmerDefinition.BeforeMerCount is 6, -6 to -1 are positions 0 to 5
                        iPos = kmerDefinition.BeforeMerCount + iNum;
                        if (!(0 <= iPos && iPos < kmerDefinition.BeforeMerCount))
                        { 
                            chBEA = ' '; 
                        }
                    } 
                    break;
                case 'E':
                    {
                        // e.g.KmerDefinition.BeforeMerCount is 6, NumberOfEAt1stPosition=1, 1 to 8 are positions 6 to 13
                        // e.g.KmerDefinition.BeforeMerCount is 0, NumberOfEAt1stPosition=4, 4 to 11 are positions 0 to 7
                        iPos = iNum + kmerDefinition.BeforeMerCount - kmerDefinition.NumberOfEAt1stPosition; 
                        if (!(kmerDefinition.BeforeMerCount <= iPos && iPos < kmerDefinition.BeforeMerCount + kmerDefinition.EpitopeMerCount)) 
                        {
                            chBEA = ' '; 
                        }

                    }
                    break;
                case 'A':
                    { 
                        // e.g.KmerDefinition.BeforeMerCount is 6, NumberOfEAt1stPosition=1, KmerDefinition.EpitopeMerCount = 8, 1 to 6 are positions 14 to 19 
                        // e.g.KmerDefinition.BeforeMerCount is 0, NumberOfEAt1stPosition=4, KmerDefinition.EpitopeMerCount = 8, 1 to 6 are positions 8 to 13
                        iPos = iNum + kmerDefinition.BeforeMerCount + kmerDefinition.EpitopeMerCount - 1; 
                        if (!(kmerDefinition.BeforeMerCount + kmerDefinition.EpitopeMerCount <= iPos && iPos < kmerDefinition.FullMerCount))
                        {
                            chBEA = ' ';
                        }

                    } 
                    break; 
                default:
                    iPos = -1; 
                    SpecialFunctions.CheckCondition(false); //!!!raise error
                    break;
            }
            return iPos;
        }
 
        public static Dictionary<string, List<string>> AaToPropList = CreateAAToPropList(); 
        public static Dictionary<string, List<string>> CreateAAToPropList()
        { 
            Dictionary<string, List<string>> aaToPropList = new Dictionary<string, List<string>>();
            foreach (string aa in Biology.GetInstance().AminoAcidCollection.Keys)
            {
                if (aa == "STOP" || aa == "DELETE")
                {
                    continue; 
                } 
                bool[] rgsBit = KmerProperties.GetInstance().AABits[aa];
                List<string> probList = new List<string>(); 
                for (int iProperty = 0; iProperty < rgsBit.Length; ++iProperty)
                {
                    if (rgsBit[iProperty])
                    {
                        string property = KmerProperties.GetInstance().AAPropertyCollection[iProperty];
                        probList.Add(property); 
                    } 
                }
                aaToPropList.Add(aa, probList); 
            }
            return aaToPropList;
        }


 
 
        public static Set<char> PropertyNameToAASet(string propertyName)
        { 
            KmerProperties kmerProperties = KmerProperties.GetInstance();

            int propertyNum = kmerProperties.PropertyToNumber[propertyName];

            Set<char> aaSet = Set<char>.GetInstance();
            foreach (KeyValuePair<string, bool[]> aaAndBits in kmerProperties.AABits) 
            { 
                if (aaAndBits.Value[propertyNum])
                { 
                    char aa = Biology.GetInstance().ThreeLetterAminoAcidAbbrevTo1Letter[aaAndBits.Key];
                    aaSet.AddNew(aa);
                }
            }

            return aaSet; 
        } 

        public static IEnumerable<List<string>> EveryPropertyCombination(string aaSeq) 
        {
                List<List<string>> propertyListList = new List<List<string>>();
                foreach (char aa in aaSeq)
                {
                    string aaAsString = Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[aa];
                    propertyListList.Add(KmerProperties.AaToPropList[aaAsString]); 
                } 

                return SpecialFunctions.EveryCombination(propertyListList); 

        }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
