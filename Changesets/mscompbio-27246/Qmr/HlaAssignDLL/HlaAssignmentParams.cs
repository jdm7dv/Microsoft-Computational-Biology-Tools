using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using EpipredLib; 
 

namespace VirusCount.Qmr 
{
 	public class HlaAssignmentParams
	{
		private HlaAssignmentParams()
		{
		} 
 
		string FileName;
 		//string Header; 
 		public double CausePrior;
		public double LinkProbability;
 		public double LeakProbability;

		HlaResolution HlaResolution;
		public int Limit; 
		bool DoPlus; 
		string OutputFileOrNull;
 


		public static HlaAssignmentParams GetInstance(string nameForNumbers, string prefix, int limit, bool doPlus, bool doOutput)
 		{
 			HlaAssignmentParams aHlaAssignmentParams = new HlaAssignmentParams();
			aHlaAssignmentParams.Limit = limit; 
 			aHlaAssignmentParams.DoPlus = doPlus; 
			aHlaAssignmentParams.FileName = string.Format(@"{0}-model.txt", prefix);
			//aHlaAssignmentParams.Header = @"peptide	did	a1	a2	b1	b2	c1	c2"; 
			aHlaAssignmentParams.OutputFileOrNull = doOutput ? string.Format(@"exhaustivePlus{0}-abinitio.{1}.{2}.new.txt", prefix, limit, nameForNumbers) : null; //!!!const

			//string solutionFileName = string.Format(@"{0}-solution-abinitio-leak0.003.txt", prefix);
			//string solutionHeader = @"peptide	HLA	p(assignment)	isKnown	knownHLAs";

 
 

 			if (nameForNumbers == "") 
 			{
				if (prefix == "HIVOptimals")
 				{
					aHlaAssignmentParams.CausePrior = 0.011498;
					aHlaAssignmentParams.LinkProbability = 0.48795;
					aHlaAssignmentParams.LeakProbability = 0.051818; 
				} 
				else
 				{ 
 					SpecialFunctions.CheckCondition(prefix == "EBVOptimals");
					aHlaAssignmentParams.CausePrior = 0.0068892;
 					aHlaAssignmentParams.LinkProbability = 0.4597;
					aHlaAssignmentParams.LeakProbability = 0.042766;

				} 
			} 
			//else if (nameForNumbers == "1in300")
			//{ 

 			//    if (prefix == "HIVOptimals")
 			//    {
			//        linkProbability = 0.30816;
 			//        causePrior = 0.040854;
			//        leakProbability = 0.0033333; 
			//    } 
			//    else
			//    { 

			//        CheckCondition(prefix == "EBVOptimals");
 			//        linkProbability = 0.30056;
 			//        causePrior = 0.028322;
			//        leakProbability = 0.0033333;
 			//    } 
			//} 
			else if (nameForNumbers == "leak0")
			{ 
				if (prefix == "HIVOptimals")
				{
 					aHlaAssignmentParams.CausePrior = 0.041551;
 					aHlaAssignmentParams.LinkProbability = 0.33478;
					aHlaAssignmentParams.LeakProbability = 1.0 / 300.0;
 				} 
				else 
				{
 
					SpecialFunctions.CheckCondition(prefix == "EBVOptimals");
					aHlaAssignmentParams.CausePrior = 0.028628;
					aHlaAssignmentParams.LinkProbability = 0.29874;
 					aHlaAssignmentParams.LeakProbability = 1.0 / 300.0;
 				}
			} 
 			else 
			{
				aHlaAssignmentParams.CausePrior = double.NaN; 
				aHlaAssignmentParams.LinkProbability = double.NaN;
				aHlaAssignmentParams.LeakProbability = double.NaN;
			}

 			aHlaAssignmentParams.HlaResolution = HlaResolution.ABMixed;
 
 			return aHlaAssignmentParams; 
		}
 

        //internal double ScoreSolution(string prefix, string solutionFile)
        //{
        //    Dictionary<string, SearchHlaAssignmentsResult> peptideToResults = new Dictionary<string, SearchHlaAssignmentsResult>();
        //    string header = "CausePrior	LinkProbability	LeakProbability	Peptide	patientsWhoRespond.Count	SizeOfSetOfHlasOfPatientsWhoRespond	SizeOfLargestHlaSubsetConsidered	NumberOfHlaSubsetsConsidered	NumberOfAllPossibleHlaSubsets	LogLikelihood	LogLikelihood.TrueHlas.Count	LogLikelihood.TrueHlas	LogLikelihood.TrueHlasAndRespondingPatients	LogLikelihood.UnexplainedPatients.Count	BestExplanationTrueHlas.Count	BestExplanationTrueHlas	BestExplanationTrueHlas.TrueHlasAndRespondingPatients	BestExplanationTrueHlas.UnexplainedPatients.Count";
 
 
        //    //!!!these three lines appear elsewhere
        //    Quickscore<string, int> quickscore = HlaAssignmentParams.CreateQuickscore(FileName, Header, CausePrior, LinkProbability, LeakProbability, HlaResolution); 
        //    Dictionary<int, double> patientWeightTable = HlaAssignmentParams.CreatePatientWeightTable(FileName, Header);
        //    Dictionary<string, List<int>> peptideToPatientsWhoRespond = HlaAssignmentParams.CreatePeptideToPatientsWhoRespond(FileName, Header);

        //    double loglikelihoodTotal = 0.0;

        //    foreach (Dictionary<string, string> row in Study.TabFileTable(solutionFile, header, false)) 
        //    { 
        //        string peptide = row["Peptide"];
        //        SearchHlaAssignmentsResult aSearchHlaAssignmentsResult = SearchHlaAssignmentsResult.GetInstance(row, quickscore, Limit, peptideToPatientsWhoRespond[peptide], patientWeightTable); 
        //        peptideToResults.Add(peptide, aSearchHlaAssignmentsResult);
        //        loglikelihoodTotal += aSearchHlaAssignmentsResult.BestLogLikelihoodSoFar.ChampsScore;
        //    }

        //    //double loglikelihoodTotal = ScoreModelAgainstPeptideSolutions(peptideToResults, quickscore
 
        //    return loglikelihoodTotal; 
        //}
 



        //internal Dictionary<string, SearchHlaAssignmentsResult> SearchHlaAssignmentsExhaustivePlus()
        //{
        //    Debug.Assert(Math.Exp(double.NegativeInfinity) == 0); // Real assert 
 
        //    //Create the structure of patients and their HLAs
        //    Quickscore<string, int> quickscore = CreateQuickscore(FileName, Header, CausePrior, LinkProbability, LeakProbability, HlaResolution); 
        //    Dictionary<int, double> patientWeightTable = CreatePatientWeightTable(FileName, Header);
        //    Dictionary<string, List<int>> peptideToPatientsWhoRespond = CreatePeptideToPatientsWhoRespond(FileName, Header);

        //    Dictionary<string, SearchHlaAssignmentsResult> peptideToResults;

        //    using (StreamWriter streamwriterOutputFileOrNull = CreateTextOrNull(OutputFileOrNull)) 
        //    { 
        //        peptideToResults = FindBestAssignmentsExhaustive(streamwriterOutputFileOrNull, Limit, quickscore, patientWeightTable, peptideToPatientsWhoRespond, DoPlus);
        //    } 

        //    return peptideToResults;
        //    //SearchHlaAssignmentsResult.ReportCollection(peptideToResults);
        //}

 		static private StreamWriter CreateTextOrNull(string outputFileOrNull) 
		{ 
			if (outputFileOrNull == null)
			{ 
				return null;
			}
 			else
 			{
				return File.CreateText(outputFileOrNull);
 			} 
		} 

 
		static public IEnumerable<List<Dictionary<string, string>>> QuickScoreOptimalsGroupByPeptide(string fileName, string header)
		{

			Dictionary<string, List<Dictionary<string, string>>> tableByPeptideCollection = new Dictionary<string, List<Dictionary<string, string>>>();
			//!!!would be faster to not read the file from disk here and in another function
 			foreach (Dictionary<string, string> row in QuickScoreOptimalsTable(fileName, header)) 
 			{ 

 
				string peptide = row["peptide"];
 				if (!tableByPeptideCollection.ContainsKey(peptide))
				{
					List<Dictionary<string, string>> tableByPeptide = new List<Dictionary<string, string>>();
					tableByPeptide.Add(row);
					tableByPeptideCollection.Add(peptide, tableByPeptide); 
				} 
 				else
 				{ 
					tableByPeptideCollection[peptide].Add(row);
 				}
			}
			return tableByPeptideCollection.Values;
		}
 
		private static Dictionary<string, List<int>> CreatePeptideToPatientsWhoRespond(string fileName, string header) 
		{
 			Dictionary<string, List<int>> peptideToPatientsWhoRespond = new Dictionary<string, List<int>>(); 

 			foreach (List<Dictionary<string, string>> tableByPeptide in QuickScoreOptimalsGroupByPeptide(fileName, header))
			{
 				Debug.Assert(tableByPeptide.Count > 0); // real assert
				string peptide = tableByPeptide[0]["peptide"];
 
				Dictionary<int, bool> patientsWhoRespondDictionary = new Dictionary<int, bool>(); 
				foreach (Dictionary<string, string> row in tableByPeptide)
				{ 
					int patient = GetPatient(row);
 					patientsWhoRespondDictionary[patient] = true;
 				}

				List<int> patientsWhoRespond = new List<int>(patientsWhoRespondDictionary.Keys);
 				peptideToPatientsWhoRespond.Add(peptide, patientsWhoRespond); 
			} 

			return peptideToPatientsWhoRespond; 
		}

        public static Quickscore<Hla, int> CreateQuickscore(string fileName, string header, double causePrior, double linkProbability, double leakProbability, HlaResolution hlaResolution)
		{
            Qmrr.HlaFactory hlaFactory = Qmrr.HlaFactory.GetFactory("noConstraint");
 
            Quickscore<Hla, int> quickscore = Quickscore<Hla, int>.GetInstance(hlaFactory.GetGroundInstance("")); 

 
			foreach (Dictionary<string, string> row in QuickScoreOptimalsTable(fileName, header))
 			{
 				foreach (string column in CreateHlaColumns(header))
				{
                    Hla hla = GetHlaValue(row, column, hlaResolution);
 					int patient = GetPatient(row); 
 
					quickscore.SetCause(hla, causePrior);
					quickscore.SetLeak(patient, leakProbability); 
					quickscore.SetLink(hla, patient, linkProbability);
				}
			}
 			return quickscore;
 		}
 
		//!!!this file gets read from 3 times. Also, condidtional doens't need to be in the loop 
 		public static Dictionary<int, double> CreatePatientWeightTable(string fileName, string header)
		{ 
			string weightColumnName = "weight";
			Dictionary<int, double> patientWeightTable = new Dictionary<int, double>();
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(fileName, header, /*includeWholeLine*/ false))
			{
				double weight = 1.0;
 				if (row.ContainsKey(weightColumnName)) 
 				{ 
					weight = double.Parse(row[weightColumnName]);
 				} 
				int did = int.Parse(row["did"]);
				if (patientWeightTable.ContainsKey(did))
				{
					SpecialFunctions.CheckCondition(patientWeightTable[did] == weight);
				}
 				else 
 				{ 
					patientWeightTable.Add(did, weight);
 				} 
			}
			return patientWeightTable;
		}

		public static int GetPatient(Dictionary<string, string> row)
		{ 
 			int patient = int.Parse(row["did"]); 
 			return patient;
		} 

        //private static Dictionary<string, SearchHlaAssignmentsResult> FindBestAssignmentsExhaustive(StreamWriter streamwriterOutputFileOrNull,
        //    int limit, Quickscore<string, int> quickscore, Dictionary<int, double> patientWeightTable,
        //    Dictionary<string, List<int>> peptideToPatientsWhoRespond, bool doPlus)
        //{
        //    Dictionary<string, SearchHlaAssignmentsResult> peptideToResults = new Dictionary<string, SearchHlaAssignmentsResult>(); 
 
        //    if (streamwriterOutputFileOrNull != null)
        //    { 
        //        streamwriterOutputFileOrNull.WriteLine(SearchHlaAssignmentsResult.ToStringHeader());
        //    }


        //    foreach (string peptide in peptideToPatientsWhoRespond.Keys)
        //    { 
        //        SearchHlaAssignmentsResult searchHlaAssignmentsResult = SearchHlaAssignmentsResult.GetInstance(peptide, limit, quickscore, patientWeightTable, peptideToPatientsWhoRespond[peptide]); 
        //        searchHlaAssignmentsResult.ExhaustiveSearch();
 
        //        while (doPlus)
        //        {
        //            bool anyBetter1 = searchHlaAssignmentsResult.RepeatBitFlipUntilNoImprovement();
        //            bool anyBetter2 = searchHlaAssignmentsResult.RepeatReplace1sUntilNoImprovement();
        //            if (!(anyBetter1 || anyBetter2))
        //            { 
        //                break; 
        //            }
        //        } 


        //        peptideToResults.Add(peptide, searchHlaAssignmentsResult);

        //        Debug.WriteLine(searchHlaAssignmentsResult);
        //        if (streamwriterOutputFileOrNull != null) 
        //        { 
        //            streamwriterOutputFileOrNull.WriteLine(searchHlaAssignmentsResult);
        //            streamwriterOutputFileOrNull.Flush(); 
        //        }
        //    }

        //    return peptideToResults;
        //}
 
 
 		static public IEnumerable<Dictionary<string, string>> QuickScoreOptimalsTable(string filename, string header)
		{ 

            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(filename, header, true))
			{
				int zeroCount = 0;
				foreach (string column in CreateHlaColumns(header))
				{ 
 					if (int.Parse(row[column]) == 0) 
 					{
						++zeroCount; 
 					}
				}
				//Debug.Assert(zeroCount == 0 || zeroCount == 6);
				if (zeroCount > 0)
				{
					continue; 
 				} 
 				yield return row;
			} 
 		}

		public static string[] CreateHlaColumns(string header)
		{
			string[] hlaList = (header.Contains("A1\t")) ? new string[] { "A1", "A2", "B1", "B2", "C1", "C2" } : new string[] { "a1", "a2", "b1", "b2", "c1", "c2" };
			return hlaList; 
		} 

        static Qmrr.HlaFactory HlaFactory = Qmrr.HlaFactory.GetFactory("noConstraint"); 


        public static Hla GetHlaValue(Dictionary<string, string> row, string column, HlaResolution hlaResolution)
 		{
 			HlaToLength hlaToLength = GetHlaToLengthValueOrNull(row, column, hlaResolution);
			SpecialFunctions.CheckCondition(hlaToLength != null); 
            return HlaFactory.GetGroundInstance(hlaToLength.ToString()); 
 		}
        internal static HlaToLength GetHlaToLengthValueOrNull(Dictionary<string, string> row, string column, HlaResolution hlaResolution) 
		{
			string hlaValue = row[column].ToString();
			if (hlaValue.StartsWith("A") || hlaValue.StartsWith("B") || hlaValue.StartsWith("C"))
			{
				HlaToLength hlaToLength = hlaResolution.GetHlaLengthInstance(hlaValue);
 				return hlaToLength; 
 			} 
			else
 			{ 
				if (hlaValue.Length == 3 || hlaValue.Length == 1)
				{
					hlaValue = "0" + hlaValue;
				}
				string hla = (column[0].ToString() + hlaValue).ToUpper();
 				HlaToLength hlaToLength = hlaResolution.GetHlaLengthInstance(hla); 
 				return hlaToLength; 
			}
 		} 


        //double ScoreWithNewParameters(Dictionary<string, SearchHlaAssignmentsResult> peptideToResults, int iParameter, double parameter)
        //{
        //    double totalLogLikelihood = 0.0;
 
        //    Quickscore<string, int> quickScore = null; 
        //    foreach (SearchHlaAssignmentsResult searchHlaAssignmentsResult in peptideToResults.Values)
        //    { 
        //        //!!!don't create these structures from scratch on every call
        //        //!!!don't create a whole new quickscore
        //        if (quickScore == null)
        //        {
        //            quickScore = CloneQuickScoreWithNewParameter(searchHlaAssignmentsResult.Quickscore, iParameter, parameter);
        //        } 
        //        HlaAssignmentWithResponses hlaAssignment = searchHlaAssignmentsResult.BestLogLikelihoodSoFar.Champ; 
        //        totalLogLikelihood +=
        //            //!!!speed up: don't compute whole loglikelihood - only part that changes with the parameters 
        //            quickScore.LogLikelihoodOfModelWithCompleteAssignments(
        //                searchHlaAssignmentsResult.PatientsWhoRespond,
        //                searchHlaAssignmentsResult.PatientsWhoDoNotRespond,
        //                hlaAssignment.AsDictionary, searchHlaAssignmentsResult.PatientWeightTableOrNull);
        //    }
 
        //    return totalLogLikelihood; 
        //}
 

		//double ScoreWithNewParameters(Dictionary<string, SearchHlaAssignmentsResult> peptideToResults, int iParameter, double parameter)
		//{
		//    Quickscore<string, int> quickScore = CloneQuickScoreWithNewParameter(searchHlaAssignmentsResult.Quickscore, iParameter, parameter);

		//    double totalLogLiklihood = ScoreModelAgainstPeptideSolutions(peptideToResults, quickScore); 
 
		//    return totalLogLiklihood;
 		//} 

 		//private static double ScoreModelAgainstPeptideSolutions(Dictionary<string, SearchHlaAssignmentsResult> peptideToResults, Quickscore<string, int> quickScore)
		//{
 		//    double totalLogLiklihood = 0.0;
		//    foreach (SearchHlaAssignmentsResult searchHlaAssignmentsResult in peptideToResults.Values)
		//    { 
		//        //!!!don't create these structures from scratch on every call 
		//        //!!!don't create a whole new quickscore
		//        HlaAssignment hlaAssignment = searchHlaAssignmentsResult.BestLogLikelihoodSoFar.Champ; 
 		//        totalLogLiklihood +=
 		//            //!!!speed up: don't compute whole loglikelihood - only part that changes with the parameters
		//            quickScore.LogLikelihoodOfModelWithCompleteAssignments(
 		//                searchHlaAssignmentsResult.PatientsWhoRespond,
		//                searchHlaAssignmentsResult.PatientsWhoDoNotRespond,
		//                hlaAssignment.AsDictionary, searchHlaAssignmentsResult.PatientWeightTableOrNull); 
		//    } 
		//    return totalLogLiklihood;
		//} 

 		private Quickscore<string, int> CloneQuickScoreWithNewParameter(Quickscore<string, int> quickscoreIn, int iParameter, double parameter)
 		{
			double causePrior = CausePrior;
 			double linkProbability = LinkProbability;
			double leakProbability = LeakProbability; 
			switch(iParameter) 
			{
				case 0: 
					causePrior = parameter;
 					break;
 				case 1:
					linkProbability = parameter;
 					break;
				case 2: 
					leakProbability =parameter; 
					break;
				default: 
					SpecialFunctions.CheckCondition(false);
 					break;
 			}

			Quickscore<string, int> quickscoreOut = Quickscore<string, int>.GetInstance("");
 
 			foreach(string cause in quickscoreIn.CauseList()) 
			{
				quickscoreOut.SetCause(cause, causePrior); 
				foreach(int effect in quickscoreIn.EffectListForCause(cause))
				{
					quickscoreOut.SetLink(cause, effect, linkProbability);
 					quickscoreOut.SetLeak(effect, leakProbability);
 				}
			} 
 
 			return quickscoreOut;
		} 

        //internal double CoordinateDecent(Dictionary<string, SearchHlaAssignmentsResult> peptideToResults, int numberOfIterationsOverParameters)
        //{
        //    double eps = 1e-9;

        //    double oldScore = double.NaN; 
        //    Debug.WriteLine(SpecialFunctions.CreateTabString("iterationOverParameters", "iParameter", "Probability", "LogOdds", "Score")); 
        //    for (int iterationOverParameters = 0; iterationOverParameters < numberOfIterationsOverParameters; ++iterationOverParameters)
        //    { 
        //        double newScore = double.NaN;
        //        for(int iParameter = 0; iParameter < ParameterCollectionSize; ++iParameter)
        //        {
        //            BestSoFar<double, double> bestParamSoFar = OneDOptimization(GetParameterCollection(iParameter), delegate(double parameter) { return ScoreWithNewParameters(peptideToResults, iParameter, parameter); });
        //            SetParameterCollection(iParameter, SpecialFunctions.Probability(bestParamSoFar.Champ));
        //            Debug.WriteLine(SpecialFunctions.CreateTabString(iterationOverParameters, iParameter, SpecialFunctions.Probability(bestParamSoFar.Champ), bestParamSoFar.Champ, bestParamSoFar.ChampsScore)); 
        //            newScore = bestParamSoFar.ChampsScore; 
        //        }
        //        if (!double.IsNaN(oldScore) && Math.Abs(oldScore - newScore) < eps) 
        //        {
        //            break;
        //        }
        //        oldScore = newScore;
        //    }
        //    return oldScore; 
        //} 

		int ParameterCollectionSize 
		{
			get
			{
 				return 3;
 			}
		} 
 		double GetParameterCollection(int iParameter) 
		{
			switch (iParameter) 
			{
				case 0:
					return CausePrior;
 				case 1:
 					return LinkProbability;
				case 2: 
 					return LeakProbability; 
				default:
					SpecialFunctions.CheckCondition(false); 
					return double.NaN;
			}

		}

 		void SetParameterCollection(int iParameter, double newParameter) 
 		{ 
			switch (iParameter)
 			{ 
				case 0:
					CausePrior = newParameter;
					break;
				case 1:
					LinkProbability = newParameter;
 					break; 
 				case 2: 
					LeakProbability = newParameter;
 					break; 
				default:
					SpecialFunctions.CheckCondition(false);
					break;
			}

		} 
 

 	} 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
