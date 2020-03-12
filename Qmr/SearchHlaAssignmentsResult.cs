//using System; 
//using System.Collections.Generic;
//using System.Text;
//using System.Diagnostics;

//namespace VirusCount.Qmr
//{ 
//    public class SearchHlaAssignmentsResult 
//    {
//        private SearchHlaAssignmentsResult() 
//        {
//        }

//        public BestSoFar<double, HlaAssignmentWithResponses> BestLogLikelihoodSoFar = BestSoFar<double, HlaAssignmentWithResponses>.GetInstance(delegate(double champ, double challenger) { return challenger > champ; });
//        public BestSoFar<HlaAssignmentWithResponses, HlaAssignmentWithResponses> BestExplanationSoFar = BestSoFar<HlaAssignmentWithResponses, HlaAssignmentWithResponses>.GetInstance(HlaAssignmentWithResponses.BetterAtExplainingReactions);
//        //public double BaseLogLikelihood = double.NaN; 
 
//        public List<int> PatientsWhoRespond;
//        public int SizeOfSetOfHlasOfPatientsWhoRespond; 
//        public long NumberOfAllPossibleHlaSubsets;
//        public int NumberOfHlaSubsetsConsidered;
//        public int SizeOfLargestHlaSubsetConsidered;
//        public Dictionary<int, double> PatientWeightTableOrNull;
//        private List<int> AllPatients;
//        public List<int> PatientsWhoDoNotRespond; 
//        public Quickscore<string, int> Quickscore; 
//        string Peptide;
//        int Limit; 
//        Dictionary<string, List<int>> HlaToRespondingPatientsUnfiltered;
//        Dictionary<string, List<int>> HlaToRespondingPatients;
//        List<string> HlaListFromRepondingPatients;
//        EverySubsetBySize EverySubsetBySize;

 
//        private IEnumerable<HlaAssignmentWithResponses> EveryHlaAssignmentToConsiderExhaustive() 
//        {
 
//            foreach (List<int> indexCollection in EverySubsetBySize.Collection())
//            {
//                HlaAssignmentWithResponses hlaAssignment = HlaAssignmentWithResponses.GetInstance(Quickscore, HlaListFromRepondingPatients, indexCollection, HlaToRespondingPatients);
//                yield return hlaAssignment;
//            }
//        } 
 

//        internal static void ReportCollection(Dictionary<string, SearchHlaAssignmentsResult> peptideToResults) 
//        {
//            foreach (KeyValuePair<string, SearchHlaAssignmentsResult> peptideAndSearchHlaAssignmentsResult in peptideToResults)
//            {
//                string peptide = peptideAndSearchHlaAssignmentsResult.Key;
//                Debug.WriteLine(Study.CreateTabString(peptide, peptideAndSearchHlaAssignmentsResult.Value));
//            } 
//        } 

//        public override string ToString() 
//        {
//            return Study.CreateTabString(
//                Peptide,
//                PatientsWhoRespond.Count,
//                SizeOfSetOfHlasOfPatientsWhoRespond,
//                SizeOfLargestHlaSubsetConsidered, 
//                NumberOfHlaSubsetsConsidered, 
//                NumberOfAllPossibleHlaSubsets,
//                BestLogLikelihoodSoFar.ChampsScore, 
//                BestLogLikelihoodSoFar.Champ.TrueCount,
//                BestLogLikelihoodSoFar.Champ.TrueToString(),
//                BestLogLikelihoodSoFar.Champ.TrueToListString(),
//                BestLogLikelihoodSoFar.Champ.UnexplainedPatients.Count,
//                BestExplanationSoFar.ChampsScore.TrueCount,
//                BestExplanationSoFar.Champ.TrueToString(), 
//                BestExplanationSoFar.Champ.TrueToListString(), 
//                BestExplanationSoFar.Champ.UnexplainedPatients.Count);
//        } 

//        internal static object ToStringHeader()
//        {
//            return Study.CreateTabString(
//                "Peptide",
//                "patientsWhoRespond.Count", 
//                "SizeOfSetOfHlasOfPatientsWhoRespond", 
//                "SizeOfLargestHlaSubsetConsidered",
//                "NumberOfHlaSubsetsConsidered", 
//                "NumberOfAllPossibleHlaSubsets",
//                "LogLikelihood",
//                "LogLikelihood.TrueHlas.Count",
//                "LogLikelihood.TrueHlas",
//                "LogLikelihood.TrueHlasAndRespondingPatients",
//                "LogLikelihood.UnexplainedPatients.Count", 
//                "BestExplanationTrueHlas.Count", 
//                "BestExplanationTrueHlas",
//                "BestExplanationTrueHlas.TrueHlasAndRespondingPatients", 
//                "BestExplanationTrueHlas.UnexplainedPatients.Count");
//        }


//        public bool RememberIfBetter(HlaAssignmentWithResponses hlaAssignment)
//        { 
 
//            bool betterFoundCoverage = BestExplanationSoFar.Compare(hlaAssignment, hlaAssignment);
//            double logLikelihood = Quickscore.LogLikelihoodOfModelWithCompleteAssignments(PatientsWhoRespond, PatientsWhoDoNotRespond, hlaAssignment.AsDictionary, PatientWeightTableOrNull); 
//            bool betterFoundLogLikelihood = BestLogLikelihoodSoFar.Compare(logLikelihood, hlaAssignment);

//            if (betterFoundCoverage || betterFoundLogLikelihood)
//            {
//                Debug.WriteLine(ToString());
//            } 
 
//            return betterFoundLogLikelihood;
//        } 

//        internal static SearchHlaAssignmentsResult GetInstance(Dictionary<string, string> row, Quickscore<string, int> quickscore, int limit, List<int> patientsWhoRespond, Dictionary<int, double> patientWeightTableOrNull)
//        {

//            SearchHlaAssignmentsResult searchHlaAssignmentsResult = new SearchHlaAssignmentsResult();
//            searchHlaAssignmentsResult.Quickscore = quickscore; 
//            searchHlaAssignmentsResult.Peptide = row["Peptide"]; 
//            searchHlaAssignmentsResult.Limit = limit;
//            searchHlaAssignmentsResult.AllPatients = searchHlaAssignmentsResult.Quickscore.EffectList(); 
//            searchHlaAssignmentsResult.PatientsWhoRespond = patientsWhoRespond;
//            searchHlaAssignmentsResult.PatientsWhoDoNotRespond = Study.Subtract(searchHlaAssignmentsResult.AllPatients, searchHlaAssignmentsResult.PatientsWhoRespond);
//            searchHlaAssignmentsResult.SizeOfSetOfHlasOfPatientsWhoRespond = int.Parse(row["SizeOfSetOfHlasOfPatientsWhoRespond"]);
//            searchHlaAssignmentsResult.NumberOfAllPossibleHlaSubsets = (long) double.Parse(row["NumberOfAllPossibleHlaSubsets"]);
//            searchHlaAssignmentsResult.PatientWeightTableOrNull = patientWeightTableOrNull;
//            searchHlaAssignmentsResult.NumberOfHlaSubsetsConsidered = int.Parse(row["NumberOfHlaSubsetsConsidered"]); 
//            searchHlaAssignmentsResult.SizeOfLargestHlaSubsetConsidered = int.Parse(row["SizeOfLargestHlaSubsetConsidered"]); 

//            searchHlaAssignmentsResult.HlaToRespondingPatientsUnfiltered = searchHlaAssignmentsResult.Quickscore.CreateCauseToSubsetOfEffects(searchHlaAssignmentsResult.PatientsWhoRespond); 
//            searchHlaAssignmentsResult.HlaToRespondingPatients = QmrAlgorithms.Filter9xs(searchHlaAssignmentsResult.HlaToRespondingPatientsUnfiltered);
//            searchHlaAssignmentsResult.HlaListFromRepondingPatients = new List<string>(searchHlaAssignmentsResult.HlaToRespondingPatients.Keys);
//            searchHlaAssignmentsResult.EverySubsetBySize = EverySubsetBySize.GetInstance(searchHlaAssignmentsResult.HlaToRespondingPatients.Count, searchHlaAssignmentsResult.HlaToRespondingPatients.Count);

//            HlaAssignmentWithResponses likelihoodChamp = QmrAlgorithms.CreateHlaAssignment(quickscore, row["LogLikelihood.TrueHlas"], patientsWhoRespond);
//            double likelihoodScore = quickscore.LogLikelihoodOfModelWithCompleteAssignments( 
//                        searchHlaAssignmentsResult.PatientsWhoRespond, 
//                        searchHlaAssignmentsResult.PatientsWhoDoNotRespond,
//                        likelihoodChamp.AsDictionary, searchHlaAssignmentsResult.PatientWeightTableOrNull); 

//            searchHlaAssignmentsResult.BestLogLikelihoodSoFar.Compare(likelihoodScore, likelihoodChamp);

//            HlaAssignmentWithResponses explainChamp = QmrAlgorithms.CreateHlaAssignment(quickscore, row["BestExplanationTrueHlas"], patientsWhoRespond);
//            searchHlaAssignmentsResult.BestExplanationSoFar.Compare(explainChamp, explainChamp);
 
//            return searchHlaAssignmentsResult; 

//        } 



//        public static SearchHlaAssignmentsResult GetInstance(string peptide,
//            int limit, Quickscore<string, int> quickscore,
//            Dictionary<int, double> patientWeightTableOrNull, List<int> patientsWhoRespond 
//            ) 
//        {
//            SearchHlaAssignmentsResult searchHlaAssignmentsResult = new SearchHlaAssignmentsResult(); 
//            searchHlaAssignmentsResult.Quickscore = quickscore;
//            searchHlaAssignmentsResult.Peptide = peptide;
//            searchHlaAssignmentsResult.Limit = limit;
//            searchHlaAssignmentsResult.AllPatients = searchHlaAssignmentsResult.Quickscore.EffectList();
//            searchHlaAssignmentsResult.PatientsWhoRespond = patientsWhoRespond;
//            searchHlaAssignmentsResult.PatientsWhoDoNotRespond = Study.Subtract(searchHlaAssignmentsResult.AllPatients, searchHlaAssignmentsResult.PatientsWhoRespond); 
//            searchHlaAssignmentsResult.SizeOfSetOfHlasOfPatientsWhoRespond = searchHlaAssignmentsResult.Quickscore.CreateCauseToSubsetOfEffects(searchHlaAssignmentsResult.PatientsWhoRespond).Count; //CreateCauseToSubsetOfEffects called here and below 
//            searchHlaAssignmentsResult.NumberOfAllPossibleHlaSubsets = (long)Math.Pow(2, searchHlaAssignmentsResult.SizeOfSetOfHlasOfPatientsWhoRespond);
//            searchHlaAssignmentsResult.PatientWeightTableOrNull = patientWeightTableOrNull; 
//            searchHlaAssignmentsResult.NumberOfHlaSubsetsConsidered = -1;
//            searchHlaAssignmentsResult.SizeOfLargestHlaSubsetConsidered = -1;

//            searchHlaAssignmentsResult.HlaToRespondingPatientsUnfiltered = searchHlaAssignmentsResult.Quickscore.CreateCauseToSubsetOfEffects(searchHlaAssignmentsResult.PatientsWhoRespond);
//            searchHlaAssignmentsResult.HlaToRespondingPatients = QmrAlgorithms.Filter9xs(searchHlaAssignmentsResult.HlaToRespondingPatientsUnfiltered);
//            searchHlaAssignmentsResult.HlaListFromRepondingPatients = new List<string>(searchHlaAssignmentsResult.HlaToRespondingPatients.Keys); 
//            searchHlaAssignmentsResult.EverySubsetBySize = EverySubsetBySize.GetInstance(searchHlaAssignmentsResult.HlaToRespondingPatients.Count, searchHlaAssignmentsResult.HlaToRespondingPatients.Count); 

 

//            return searchHlaAssignmentsResult;

//        }

 
//        public void ExhaustiveSearch() 
//        {
//            foreach (HlaAssignmentWithResponses hlaAssignment in EveryHlaAssignmentToConsiderExhaustive()) 
//            {
//                ++NumberOfHlaSubsetsConsidered;
//                if (NumberOfHlaSubsetsConsidered >= Limit)
//                {
//                    break;
//                } 
//                SizeOfLargestHlaSubsetConsidered = hlaAssignment.TrueCount; 

//                RememberIfBetter(hlaAssignment); 
//                if (NumberOfHlaSubsetsConsidered % 100000 == 0)
//                {
//                    Debug.WriteLine(this);
//                }
//            }
 
//        } 

//        public bool RepeatBitFlipUntilNoImprovement() 
//        {
//            bool anyBetter = false;
//            {
//                bool better = true;
//                while (better)
//                { 
//                    better = BitFlip(); 
//                    anyBetter |= better;
//                } 
//            }
//            return anyBetter;
//        }

//        private bool BitFlip()
//        { 
//            bool anyBetter = false; 
//            HlaAssignmentWithResponses startingAssignment = this.BestLogLikelihoodSoFar.Champ;
//            foreach (HlaAssignmentWithResponses hlaAssignment in EveryHlaAssignmentToConsiderBitFlip(startingAssignment)) 
//            {
//                anyBetter |= RememberIfBetter(hlaAssignment);
//            }
//            return anyBetter;
//        }
 
//        private IEnumerable<HlaAssignmentWithResponses> EveryHlaAssignmentToConsiderBitFlip(HlaAssignmentWithResponses startingAssignment) 
//        {
//            Dictionary<string, int> trueCollection = CreateTrueCollection(startingAssignment); 

//            foreach (string hla in HlaListFromRepondingPatients)
//            {
//                bool setting = startingAssignment.AsDictionary[hla];
//                SetHla(trueCollection, hla, !setting);
//                HlaAssignmentWithResponses hlaAssignment = HlaAssignmentWithResponses.GetInstance(trueCollection.Keys, Quickscore, HlaListFromRepondingPatients, HlaToRespondingPatients); 
//                SetHla(trueCollection, hla, setting); 
//                Debug.Assert(trueCollection.Count == startingAssignment.TrueCount); // real assert
 
//                yield return hlaAssignment;
//            }
//        }

//        private static Dictionary<string, int> CreateTrueCollection(HlaAssignmentWithResponses startingAssignment)
//        { 
//            Dictionary<string, int> trueCollection = new Dictionary<string, int>(); 
//            foreach (string trueHla in startingAssignment.TrueCollection)
//            { 
//                trueCollection.Add(trueHla, 0);
//            }
//            return trueCollection;
//        }

//        private static void SetHla(Dictionary<string, int> trueCollection, string hla, bool setting) 
//        { 
//            if (setting)
//            { 
//                trueCollection.Add(hla, 0);
//            }
//            else
//            {
//                trueCollection.Remove(hla);
//            } 
//        } 

//        public bool RepeatReplace1sUntilNoImprovement() 
//        {
//            bool anyBetter = false;
//            {
//                bool better = true;
//                while (better)
//                { 
//                    better = Replace1sFlip(); 
//                    anyBetter |= better;
//                } 
//            }
//            return anyBetter;
//        }

//        private bool Replace1sFlip()
//        { 
//            bool anyBetter = false; 
//            HlaAssignmentWithResponses startingAssignment = this.BestLogLikelihoodSoFar.Champ;
//            foreach (HlaAssignmentWithResponses hlaAssignment in EveryHlaAssignmentToConsiderReplace1s(startingAssignment)) 
//            {
//                anyBetter |= RememberIfBetter(hlaAssignment);
//            }
//            return anyBetter;
//        }
 
//        private IEnumerable<HlaAssignmentWithResponses> EveryHlaAssignmentToConsiderReplace1s(HlaAssignmentWithResponses startingAssignment) 
//        {
//            Dictionary<string, int> trueCollection = CreateTrueCollection(startingAssignment); 

//            foreach (string originalTrueHla in startingAssignment.TrueCollection)
//            {
//                SetHla(trueCollection, originalTrueHla, false);
//                foreach (string originalFalseHla in HlaListFromRepondingPatients)
//                { 
//                    if (!startingAssignment.AsDictionary[originalFalseHla]) 
//                    {
//                        SetHla(trueCollection, originalFalseHla, true); 
//                        Debug.Assert(trueCollection.Count == startingAssignment.TrueCount); // real assert
//                        HlaAssignmentWithResponses hlaAssignment = HlaAssignmentWithResponses.GetInstance(trueCollection.Keys, Quickscore, HlaListFromRepondingPatients, HlaToRespondingPatients);
//                        SetHla(trueCollection, originalFalseHla, false);
//                        yield return hlaAssignment;
//                    }
//                } 
//                SetHla(trueCollection, originalTrueHla, true); 
//                Debug.Assert(trueCollection.Count == startingAssignment.TrueCount); // real assert
//            } 

//        }

//    }
//}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
