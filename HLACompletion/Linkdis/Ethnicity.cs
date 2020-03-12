using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Msr.Mlas.SpecialFunctions; 
using System.Diagnostics;
 
namespace Msr.Linkdis
{
    public class Ethnicity
    {

        public Ethnicity() 
        { 
        }
 

        internal static Ethnicity GetInstance(string ethnicityNameX)
        {
            string ethnicityName = ethnicityNameX.ToLowerInvariant();
            SpecialFunctions.CheckCondition(EthnicityNameToEthnicity.ContainsKey(ethnicityName), string.Format("Ethnicity {0} is not one of the known values. {1}", ethnicityName, NameMixedList()));
 
            return EthnicityNameToEthnicity[ethnicityName]; 
        }
 
        static Dictionary<string, Ethnicity> EthnicityNameToEthnicity = Init();


        public Dictionary<string, EClass> HlaClassNameToEClass = new Dictionary<string, EClass>();

        static private Dictionary<string, Ethnicity> Init() 
        { 
            Dictionary<string, Ethnicity> ethnicityNameToEthnicity = new Dictionary<string, Ethnicity>();
            using (TextReader textReader = Linkdis.OpenResource("datafileList.txt")) 
            {
                foreach (var row in SpecialFunctions.ReadDelimitedFile(textReader, new { Ethnicity = "", Class = "", HlaLengthList = "", FileName = "" }, new char[] { '\t' }, true))
                {
                    Ethnicity ethnicity = ethnicityNameToEthnicity.GetValueOrDefault(row.Ethnicity.ToLowerInvariant());
                    EClass eclass = ethnicity.HlaClassNameToEClass.GetValueOrDefault(row.Class);
                    var hlaLengthListQuery = 
                                                from hlaLengthAsString in row.HlaLengthList.Split(' ') 
                                                select int.Parse(hlaLengthAsString);
                    LinkedList1<int> hlaLengthList = LinkedList1<int>.GetInstanceFromList(hlaLengthListQuery.ToList()); 


                    TableInfo tableInfo = eclass.HlaLengthListToTableInfo.GetValueOrDefault(hlaLengthList);
                    tableInfo.HlaMsr1Factory = HlaMsr1Factory.GetFactory(hlaLengthList);
                    tableInfo.LoadTable(row.FileName, row.Class);
                } 
            } 
            return ethnicityNameToEthnicity;
        } 

//Ethnicity	Class	HLAFactories	FileName
//African	A	A4	haplotypeModel.eth_African.locus_A.2008-01-31_16_51_04.csv
//African	B	A4,B4	haplotypeModel.eth_African.locus_B.2008-01-31_16_51_04.csv
//African	C	A4,B4,C4	haplotypeModel.eth_African.locus_C.2008-01-31_16_51_04.csv
//Amerindian	A	A4	haplotypeModel.eth_Amerindian.locus_A.2008-01-31_16_51_04.csv 
 

 

        static internal IEnumerable<string> NameLowerList()
        {
            return EthnicityNameToEthnicity.Keys;
        }
        static internal IEnumerable<string> NameMixedList() 
        { 
            return EthnicityNameToEthnicity.Keys.Select(key => key.ToMixedInvariant());
        } 





 
 

        private string RemoveStar(string hlaNameStar) 
        {
            string hlaName = hlaNameStar.Replace("*", "").Replace("Cw", "C");
            return hlaName;
        }

 
    } 

    public class EClass 
    {
        internal Dictionary<LinkedList1<int>, TableInfo> HlaLengthListToTableInfo = new Dictionary<LinkedList1<int>, TableInfo>();

        List<LinkedList1<int>> _hlaLengthListSorted = null;
        internal IEnumerable<LinkedList1<int>> HlaLengthListSorted()
        { 
            if (null == _hlaLengthListSorted) 
            {
                _hlaLengthListSorted = HlaLengthListToTableInfo.Keys.ToList(); 
                _hlaLengthListSorted.Sort((lla, llb) => (llb.Sum()).CompareTo(lla.Sum()));
            }
            return _hlaLengthListSorted;
        }
    }
 
    public class TableInfo 
    {
        public Dictionary<string, Dictionary<HlaMsr1, double>> PredictorNameOrInterceptToTargetToWeight; 
        public Dictionary<HlaMsr1, List<HlaMsr1>> AbstractHlaToGroundHlaList = new Dictionary<HlaMsr1, List<HlaMsr1>>();
        public HlaMsr1Factory HlaMsr1Factory;


        public List<Dictionary<HlaMsr1, double>> PullOutTheRowsOfInterest(LinkedList1<HlaMsr1> linkedList1)
        { 
            List<Dictionary<HlaMsr1, double>> rowsOfInterst = new List<Dictionary<HlaMsr1, double>>(); 
            rowsOfInterst.Add(PredictorNameOrInterceptToTargetToWeight[""]); //The intercept
            if (null != linkedList1) 
            {
                foreach (HlaMsr1 predictorHla in linkedList1)
                {
                    Dictionary<HlaMsr1, double> rowOfInterest;
                    if (PredictorNameOrInterceptToTargetToWeight.TryGetValue(predictorHla.ToString(/*withParen*/ false), out rowOfInterest))
                    { 
                        rowsOfInterst.Add(rowOfInterest); 
                    }
                } 
            }
            return rowsOfInterst;
        }


        internal void LoadTable(string resourceName, string targetClass) 
        { 
            using (TextReader textReader = Linkdis.OpenResource(resourceName))
            { 
                List<HlaMsr1> targetHlaList = CreateTargetHlaList(textReader, targetClass, resourceName);

                AddToAbstractHlaToGroundHlaList(targetHlaList);

                CreatePredictorNameOrInterceptToTargetToWeight(textReader, targetHlaList, resourceName);
            } 
        } 

        List<HlaMsr1> CreateTargetHlaList(TextReader textReader, string targetClass, string errorSuffix) 
        {
            string header = textReader.ReadLine();
            SpecialFunctions.CheckCondition(header != null, "data file in bad format. (a) " + errorSuffix);
            string[] blankStarList = header.Split(',');
            SpecialFunctions.CheckCondition(blankStarList.Length > 0 && blankStarList[0] == "", "data file in bad format. (b) " + errorSuffix);
            HashSet<string> warningSetIgnore = new HashSet<string>(); 
            List<HlaMsr1> targetHlaList = 
                (from hlaName in blankStarList.Skip(1)
                 select (HlaMsr1)HlaMsr1Factory.GetGroundInstance(hlaName, ref warningSetIgnore)) 
                 .ToList();
            SpecialFunctions.CheckCondition(targetHlaList.All(hla => hla.ClassName == targetClass), "data file in bad format. (c) " + errorSuffix);
            return targetHlaList;
        }

        private void AddToAbstractHlaToGroundHlaList(List<HlaMsr1> targetHlaList) 
        { 
            foreach (HlaMsr1 groundHla in targetHlaList)
            { 

                foreach (HlaMsr1 groundOrAbstractHla in GeneralizationList(groundHla))
                {
                    List<HlaMsr1> groundHlaList = AbstractHlaToGroundHlaList.GetValueOrDefault(groundOrAbstractHla);
                    groundHlaList.Add(groundHla);
                } 
            } 
        }
 
        private IEnumerable<HlaMsr1> GeneralizationList(HlaMsr1 groundHla)
        {
            return groundHla.GeneralizationList(HlaMsr1Factory);
        }

 
        private void CreatePredictorNameOrInterceptToTargetToWeight(TextReader textReader, List<HlaMsr1> targetHlaList, string errorSuffix) 
        {
            PredictorNameOrInterceptToTargetToWeight = new Dictionary<string, Dictionary<HlaMsr1, double>>(); 
            string line;
            while (null != (line = textReader.ReadLine()))
            {
                string[] starOrInterceptNumberList = line.Split(',');
                SpecialFunctions.CheckCondition(starOrInterceptNumberList.Length - 1 == targetHlaList.Count, "data file in bad format. (d) " + errorSuffix);
 
                string hlaNameOrBlankForIntercept = CreateHlaNameOrBlankForIntercept(starOrInterceptNumberList); 
                var predictorNameOrInterceptToWeight = new Dictionary<HlaMsr1, double>();
                PredictorNameOrInterceptToTargetToWeight.Add(hlaNameOrBlankForIntercept, predictorNameOrInterceptToWeight); 

                foreach (var targetAndWeight in CreateTargetToWeight(targetHlaList, starOrInterceptNumberList))
                {
                    predictorNameOrInterceptToWeight.Add(targetAndWeight.Key, targetAndWeight.Value);
                }
            } 
            SpecialFunctions.CheckCondition(PredictorNameOrInterceptToTargetToWeight.Count > 0, "data file in bad format. (e) " + errorSuffix); 
            SpecialFunctions.CheckCondition(PredictorNameOrInterceptToTargetToWeight.ContainsKey(""), "data file in bad format. (f) " + errorSuffix);
        } 

        private string CreateHlaNameOrBlankForIntercept(string[] starOrInterceptNumberList)
        {
            string hlaOrInterceptName = starOrInterceptNumberList[0];
            string hlaNameOrBlankForIntercept = CreateHlaNameOrBlankForIntercept2(hlaOrInterceptName);
            return hlaNameOrBlankForIntercept; 
        } 

        private string CreateHlaNameOrBlankForIntercept2(string hlaOrInterceptName) 
        {
            string hlaNameOrBlankForIntercept;
            if (hlaOrInterceptName == "INTERCEPT")
            {
                hlaNameOrBlankForIntercept = "";
            } 
            else 
            {
                HashSet<string> warningSetIgnore = new HashSet<string>(); 
                hlaNameOrBlankForIntercept = HlaMsr1Factory.GetGroundInstance(hlaOrInterceptName, ref warningSetIgnore).ToString(/*withParens*/ false);
            }
            return hlaNameOrBlankForIntercept;
        }

        private static Dictionary<HlaMsr1, double> CreateTargetToWeight(List<HlaMsr1> targetHlaList, string[] starOrInterceptNumberList) 
        { 
            Dictionary<HlaMsr1, double> targetToWeight = new Dictionary<HlaMsr1, double>();
            foreach (var targetAndDoubleString in SpecialFunctions.EnumerateTwo(targetHlaList, starOrInterceptNumberList.Skip(1))) 
            {
                targetToWeight.Add(targetAndDoubleString.Key, double.Parse(targetAndDoubleString.Value));
            }
            return targetToWeight;
        }
 
    } 

} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
