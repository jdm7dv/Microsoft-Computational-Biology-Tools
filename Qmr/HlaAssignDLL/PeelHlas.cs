using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using System.IO;
using Msr.Mlas.Qmr; 
using VirusCount.Qmr; //!!! what's the difference between Msr.Mlas.Qmr; and this? 
using Optimization;
using EpipredLib; 

namespace VirusCount.Qmrr
{
    public static class PeelHlas
    {
 		//public static void FindAndReport(string selectionName, string directory, string caseName, RangeCollection nullIndexRange, double pValue) 
		//{ 
		//    Debug.Fail("Need to expose leakProbabilityOrNull?");
		//    LrtForHla aLrtForHla = LrtForHla.GetInstance(selectionName, directory, caseName, "MixedWithB15AndA68", null, pValue); 
		//    aLrtForHla.Run(RangeCollection.GetInstance(0,0), 1, nullIndexRange, directory);
		//}


        public static void Tabulate(string selectionName, string directory, string caseName, List<KeyValuePair<int, int>> firstNullAndLastNullList, double pValue)
        { 
            Debug.Fail("Need to expose leakProbabilityOrNull?"); 
            LrtForHla aLrtForHla = LrtForHla.GetInstance(selectionName, directory, caseName, "MixedWithB15AndA68", null, pValue);
            aLrtForHla.Tabulate(directory, caseName, firstNullAndLastNullList); 
        }


        private static Dictionary<string, Set<Hla>> ExtractPeptideToHlaSet(QmrrModelAllPeptides qmrrModel)
        {
            Dictionary<string, Set<Hla>> realPeptideToHlaSet = new Dictionary<string, Set<Hla>>(); 
            foreach (string peptide in qmrrModel.BestParamsAndHlaAssignments.PeptideToBestHlaAssignmentSoFar.Keys) 
            {
                realPeptideToHlaSet.Add(peptide, qmrrModel.BestParamsAndHlaAssignments.PeptideToBestHlaAssignmentSoFar[peptide].Champ.CreateHlaAssignmentAsSet()); 
            }
            return realPeptideToHlaSet;
        }


 
 
        //private static OptimizationParameterList CreateQmrrParamsX(double causePrior, double leakProbability, Set<string> candidateHlaSet)
        //{ 
        //    List<Parameter> parameterCollection = new List<Parameter>();
        //    parameterCollection.Add(Parameter.GetProbabilityInstance("causePrior", causePrior, false));
        //    foreach (string hla in candidateHlaSet)
        //    {
        //        if (hla == "B1599")
        //        { 
        //            Parameter aParameter = Parameter.GetProbabilityInstance("link" + hla, 0, false); 
        //            parameterCollection.Add(aParameter);
        //        } 
        //        else
        //        {
        //            Parameter aParameter = Parameter.GetProbabilityInstance("link" + hla, .5, true);
        //            parameterCollection.Add(aParameter);
        //        }
        //    } 
        //    parameterCollection.Add(Parameter.GetProbabilityInstance("leakProbability", leakProbability, false)); 
        //    parameterCollection.Add(Parameter.GetPositiveFactorInstance("useKnownList", 1, false));
        //    OptimizationParameterList qmrrParamsStart = OptimizationParameterList.GetInstance2(parameterCollection); 
        //    return qmrrParamsStart;
        //}



 
        //private static Dictionary<string, string> LoadOriginalParameters(string directory) 
        //{
        //    string parameterFileName = string.Format(@"{0}\{1}", directory, "NoisyOr.Parameters.TwoCause.UseKnown.txt");//!!!const 
        //    string header = "dataset	varyFitFactor	score	causePrior	fitFactor	leakProbability	link	useKnownList";
        //    foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(parameterFileName, header, false))
        //    {
        //        bool varyFitFactor = bool.Parse(row["varyFitFactor"]);
        //        if (!varyFitFactor)
        //        { 
        //            return row; 
        //        }
        //    } 
        //    Debug.Fail("Didn't find expected line");
        //    return null;
        //}


 
 
        //static Dictionary<string, Set<string>> LoadOriginalPeptideToHlaSet(string directory)
        //{ 
        //    string fileName = string.Format(@"{0}\{1}", directory, @"NoisyOr.HlasPerPeptide.hivmodel.TwoCauseFalse.UseKnownTrue.txt");


        //    Dictionary<string, Set<string>> originalPeptideToHlaSet = new Dictionary<string, Set<string>>();
        //    string header = "Peptide	HLAAssignment	LogLikelihood";
        //    foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(fileName, header, false)) 
        //    { 
        //        string peptide = row["Peptide"];
        //        Set<string> hlaSet = Set<string>.GetInstance(row["HLAAssignment"].Split(';')); 
        //        originalPeptideToHlaSet.Add(peptide, hlaSet);
        //    }
        //    return originalPeptideToHlaSet;
        //}

        public static QmrrModelAllPeptides CreateSimpleModel(string directory, string casename, bool useKnownList) 
        { 
            OptimizationParameterList qmrrParamsStart = OptimizationParameterList.GetInstance(
                                OptimizationParameter.GetProbabilityInstance("causePrior", .01, true), 
                                OptimizationParameter.GetProbabilityInstance("link", .5, true),
                                OptimizationParameter.GetProbabilityInstance("leakProbability", .003, true),
                                OptimizationParameter.GetPositiveFactorInstance("useKnownList", useKnownList ? 1 : 0, false)
                                );

 
            ModelLikelihoodFactories modelLikelihoodFactories = ModelLikelihoodFactories.GetInstanceThreeParamSlow(qmrrParamsStart); 

            double depth = 1.5; 
            string dataset = string.Format(@"{0}\{1}", directory, casename);
            QmrrModelAllPeptides qmrrModel = QmrrModelAllPeptides.GetInstance(modelLikelihoodFactories, dataset, qmrrParamsStart, depth, "noConstraints");
            return qmrrModel;
        }

 
 

 
        ////internal static void HlaAndKnownVsKnown(string directory, string caseName, int firstNullIndex, int lastNullIndex)
        ////{
        ////    //!!!this code is similar to code both in this object and in one the the partialmodel classes
        ////    Dictionary<string, Set<string>> pidToHlaSetReal = LoadPidToHlaSet(directory, caseName);
        ////    Set<string> hlaUniverse = CreateHlaUniverse(pidToHlaSetReal);
        ////    Dictionary<string, Dictionary<string, double>> reactTable = LoadReactTable(directory, caseName); 
        ////    Dictionary<string, Set<string>> knownTable = LoadKnownTable(caseName, directory); 

 
        ////    string fileName = string.Format(@"{0}\{1}.{2}.{3}-{4}.pValues.new.txt", directory, "HlaAndKnownVsKnown", caseName, firstNullIndex, lastNullIndex); //!!!const
        ////    using (StreamWriter streamWriter = File.CreateText(fileName))
        ////    {
        ////        streamWriter.WriteLine(PValueDetails.Header);

        ////        for (int nullIndex = firstNullIndex; nullIndex <= lastNullIndex; ++nullIndex) 
        ////        { 
        ////            Dictionary<string, Set<string>> pidToHlaSet = PidToHlaSetForThisNullIndex(pidToHlaSetReal, nullIndex);
 
        ////            Dictionary<string, Dictionary<string, PValueDetails>> peptideToHlaToPValueDetails =
        ////                    HlaAndKnownVsKnownPValueDetailDictionaries(pidToHlaSet, hlaUniverse, reactTable, knownTable);

        ////            ReportPeptideToHlaToPValueDetails(peptideToHlaToPValueDetails, nullIndex, streamWriter);
        ////            streamWriter.Flush();
        ////        } 
        ////    } 
        ////}
 

        ////internal static void LRTPass2(string directory, string caseName, int firstNullIndex, int lastNullIndex)
        ////{
        ////    double pValueFilter = .005;

        ////    //!!!this code is similar to code both in this object and in one the the partialmodel classes 
        ////    Dictionary<string, Set<string>> pidToHlaSetReal = LoadPidToHlaSet(directory, caseName); 
        ////    Set<string> hlaUniverse = CreateHlaUniverse(pidToHlaSetReal);
        ////    Dictionary<string, Dictionary<string, double>> reactTable = LoadReactTable(directory, caseName); 
        ////    Dictionary<string, Set<string>> knownTable = LoadKnownTable(caseName, directory);


        ////    Dictionary<string, Set<string>> peptideToCandidateHlaSet = CreatePeptideToCandidateHlaSet(directory, caseName, firstNullIndex, lastNullIndex, pValueFilter);

        ////    foreach (string peptide in peptideToCandidateHlaSet.Keys) 
        ////    { 
        ////        Set<string> candidateHlaSet = peptideToCandidateHlaSet[peptide];
 
        ////    }
        ////}

        ////private static Dictionary<string, Set<string>> CreatePeptideToCandidateHlaSet(string directory, string caseName, int firstNullIndex, int lastNullIndex, double pValueFilter)
        ////{
        ////    Dictionary<string, Set<string>> peptideToCandidateHlaSet = new Dictionary<string, Set<string>>(); 
        ////    string pass1FileName = string.Format(@"{0}\{1}.{2}.{3}-{4}.pValues.new.txt", directory, "HlaAndKnownVsKnown", caseName, firstNullIndex, lastNullIndex); //!!!const 
        ////    System.Windows.Forms.MessageBox.Show(string.Format("Confirm this is up-to-date.\n{0}", pass1FileName));
        ////    foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(pass1FileName, PValueDetails.Header, false)) 
        ////    {
        ////        double pValue = AccessPValueFromRow(row);
        ////        if (pValue <= pValueFilter)
        ////        {
        ////            string peptide = row["peptide"];
        ////            string hla = row["hla"]; 
        ////            Set<string> candidateHlaSet = SpecialFunctions.GetValueOrDefault(peptideToCandidateHlaSet, peptide); 
        ////            candidateHlaSet.AddNew(hla);
        ////        } 
        ////    }
        ////    return peptideToCandidateHlaSet;
        ////}

    }
 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
