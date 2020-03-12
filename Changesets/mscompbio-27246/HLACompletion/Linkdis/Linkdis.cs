using System.Collections.Generic; 
using System.Diagnostics;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using System.Linq;
using System.Reflection;
using System; 
 
namespace Msr.Linkdis
{ 

    public class Linkdis
    {
        private Linkdis()
        {
        } 
 
        public static string ResourceString = "Msr.Linkdis.DataFiles.";
 
        internal static StreamReader OpenResource(string fileName)
        {
            return SpecialFunctions.OpenResource(Assembly.GetExecutingAssembly(), ResourceString, fileName); //!!!const
        }

        public ExpansionCollection ExpandOrNullIfTooMany(PidAndHlaSet pidAndHlaSet) 
        { 
            try
            { 
                ExpansionCollection expansionCollection = new ExpansionCollection();
                expansionCollection.Prenormalize(pidAndHlaSet, this);
                //expansionCollection.LogTotal = Prenormalize(pidAndHlaSet, out expansionCollection.PhaseToLogProb, out expansionCollection.UnphaseToLogProb);


                return expansionCollection; 
            } 
            catch (CombinationLimitException)
            { 
            }
            return null;
        }


        internal Dictionary<LinkedList1<HlaMsr1>, KeyValuePair<double,bool>> CreateHlaListToProb(LinkedList1<HlaMsr1> hlaListAbstractOrGround) 
        { 
            var hlaListGroundAndProbEnum = ExpandHlaList(hlaListAbstractOrGround);
            return hlaListGroundAndProbEnum.ToDictionary(hlaListGroundAndProb => hlaListGroundAndProb.Key, hlaListGroundAndProb => hlaListGroundAndProb.Value); 
        }


        private IEnumerable<KeyValuePair<LinkedList1<HlaMsr1>, KeyValuePair<double, bool>>> ExpandHlaList(LinkedList1<HlaMsr1> hlaListAbstractOrGround)
        {
            if (null == hlaListAbstractOrGround) 
            { 
                yield return new KeyValuePair<LinkedList1<HlaMsr1>, KeyValuePair<double, bool>>(null, new KeyValuePair<double,bool>(1.0,false));
            } 
            else
            {

                foreach (var restOrNullExpandedAndProb in ExpandHlaList(hlaListAbstractOrGround.RestOrNull))
                {
                    foreach (var hlaAndProb in ExpandHla(hlaListAbstractOrGround.First, restOrNullExpandedAndProb.Key)) 
                    { 
                        LinkedList1<HlaMsr1> linkedList1 = LinkedList1<HlaMsr1>.GetInstance(hlaAndProb.Key, restOrNullExpandedAndProb.Key);
                        double prob = hlaAndProb.Value.Key * restOrNullExpandedAndProb.Value.Key; 
                        bool usedBackoffModel = hlaAndProb.Value.Value || restOrNullExpandedAndProb.Value.Value;
                        yield return new KeyValuePair<LinkedList1<HlaMsr1>, KeyValuePair<double, bool>>(linkedList1, new KeyValuePair<double, bool>(prob, usedBackoffModel));
                    }
                }
            }
        } 
 
        private IEnumerable<KeyValuePair<HlaMsr1, KeyValuePair<double, bool>>> ExpandHla(HlaMsr1 hlaAbstractOrGround, LinkedList1<HlaMsr1> linkedList1)
        { 
            //Reference: http://en.wikipedia.org/wiki/Multinomial_logit#Model

            //Dictionary<string, Dictionary<HlaMsr1, double>> predictorNameToTargetHlaToWeight = Ethnicity.ClassNamePredictorNameOrInterceptToTargetToWeight[hlaAbstractOrGround.ClassName];
            EClass eclass = Ethnicity.HlaClassNameToEClass[hlaAbstractOrGround.ClassName];

 
            bool usedLowerResModel = false; 
            foreach (LinkedList1<int> hlaLengthList in eclass.HlaLengthListSorted())
            { 
                bool anyReturned = false;
                TableInfo tableInfo = eclass.HlaLengthListToTableInfo[hlaLengthList];
                foreach (var hlaAndProb in ExpandHla(tableInfo, hlaAbstractOrGround, linkedList1))
                {
                    anyReturned = true;
                    yield return new KeyValuePair<HlaMsr1, KeyValuePair<double, bool>>(hlaAndProb.Key, new KeyValuePair<double, bool>(hlaAndProb.Value, usedLowerResModel)); 
                } 
                if (anyReturned)
                { 
                    yield break; // really yield break, not yield return or return;
                }
                usedLowerResModel = true;
            }
            throw new HlaNotInModelException(hlaAbstractOrGround.Name, string.Format("Can't find {0} or any prefix in any model", hlaAbstractOrGround.ToString(/*withParen*/ true)));
        } 
 
        private IEnumerable<KeyValuePair<HlaMsr1, double>> ExpandHla(TableInfo tableInfo, HlaMsr1 hlaAbstractOrGround, LinkedList1<HlaMsr1> linkedList1)
        { 

            HashSet<HlaMsr1> groundSet = new HashSet<HlaMsr1>();
            foreach (HlaMsr1 term in hlaAbstractOrGround.TermList(tableInfo.HlaMsr1Factory))
            {
                List<HlaMsr1> groundHlaList;
                if (tableInfo.AbstractHlaToGroundHlaList.TryGetValue(term, out groundHlaList)) 
                { 
                    if (groundHlaList.Count == 1 && groundHlaList.First().Equals(term))
                    { 
                        groundSet.AddNew(term);
                    }
                    else
                    {
                        foreach (HlaMsr1 ground in groundHlaList)
                        { 
                            groundSet.AddNew(ground); 
                        }
                    } 
                }
            }

            if (groundSet.Count == 0)
            {
                yield break; 
            } 

 
            List<Dictionary<HlaMsr1, double>> rowsOfInterest = tableInfo.PullOutTheRowsOfInterest(linkedList1);
            //!!!for each list of rowsOfInterest we could cache the sum of exp's to speed things up


            //!!!This could be made faster by giving a serial number to each HLA and then doing the calcuations in arrays in which the serial number is the index.
            Dictionary<HlaMsr1, double> hlaToTotal = new Dictionary<HlaMsr1, double>(); 
            foreach (Dictionary<HlaMsr1, double> hlaToWeight in rowsOfInterest) 
            {
                foreach (KeyValuePair<HlaMsr1, double> hlaAndWeight in hlaToWeight) 
                {
                    hlaToTotal[hlaAndWeight.Key] = hlaToTotal.GetValueOrDefault(hlaAndWeight.Key) + hlaAndWeight.Value;
                }
            }
            Dictionary<HlaMsr1, double> hlaToExpTotal = new Dictionary<HlaMsr1, double>();
            double totalOfExpsPlus1 = 1; 
            foreach (KeyValuePair<HlaMsr1, double> hlaAndTotal in hlaToTotal) 
            {
                double exp = Math.Exp(hlaAndTotal.Value); 
                totalOfExpsPlus1 += Math.Exp(hlaAndTotal.Value);
                hlaToExpTotal.Add(hlaAndTotal.Key, exp);
            }


 
            foreach (HlaMsr1 hlaGround in groundSet) 
            {
                double prob = hlaToExpTotal[hlaGround] / totalOfExpsPlus1; 
                yield return new KeyValuePair<HlaMsr1, double>(hlaGround, prob);
            }

        }

 
        public static IEnumerable<string> EthnicityNameLowerList() 
        {
            return Ethnicity.NameLowerList(); 
        }
        public static IEnumerable<string> EthnicityNameMixedList()
        {
            return Ethnicity.NameMixedList();
        }
 
 
        private Ethnicity Ethnicity;
        internal int CombinationLimit; 

        public static Linkdis GetInstance(string ethnicityName, int combinationLimit)
        {
            Linkdis linkdis = new Linkdis();
            linkdis.Ethnicity = Ethnicity.GetInstance(ethnicityName);
            linkdis.CombinationLimit = combinationLimit; 
            return linkdis; 
        }
    } 

    public class CombinationLimitException : Exception
    {
        public CombinationLimitException(string message)
            : base(message)
        { 
        } 
    }
 
    public class GeneralizingTermException : Exception
    {
        public string GeneralTerm;
        public string SpecificTerm;
        public GeneralizingTermException(string generalTerm, string specificTerm, string message)
            : base(message) 
        { 
            GeneralTerm = generalTerm;
            SpecificTerm = specificTerm; 
        }
    }


    public class HlaNotInModelException : Exception
    { 
        public string HlaName; 
        public HlaNotInModelException(string hlaName, string message)
            : base(message) 
        {
            HlaName = hlaName;
        }
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
