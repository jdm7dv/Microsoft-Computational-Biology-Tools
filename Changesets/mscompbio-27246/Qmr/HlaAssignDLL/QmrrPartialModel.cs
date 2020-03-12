using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using Optimization;
using EpipredLib; 
 
namespace VirusCount.Qmr
{ 
 	public class QmrrPartialModel
	{
        private QmrrPartialModel()
        {
        }
 
        static public QmrrPartialModel GetInstance(ModelLikelihoodFactories modelLikelihoodFactories, 
            string peptide,
            Dictionary<string, double> patientToAnyReaction, 
            Set<Hla> knownHlaSet,
            Dictionary<string, Set<Hla>> patientList,
            OptimizationParameterList qmrrParamsStart
            )
		{
            QmrrPartialModel aQmrrPartialModel = new QmrrPartialModel(); 
            aQmrrPartialModel.QmrrParamsStart = qmrrParamsStart; 
            aQmrrPartialModel.Peptide = peptide;
            aQmrrPartialModel.PatientToAnyReaction = patientToAnyReaction; 
            aQmrrPartialModel.PatientList = patientList;
            aQmrrPartialModel.KnownHlaSet = knownHlaSet;
            aQmrrPartialModel.CreateHlaList();
            aQmrrPartialModel.CreateSwitchableHlasWithRespondingPatients();
            aQmrrPartialModel.ModelLikelihoodFactories = modelLikelihoodFactories;
            if (modelLikelihoodFactories != null) 
            { 
                aQmrrPartialModel.LogLikelihoodOfCompleteModelConditionedOnKnownHlas = modelLikelihoodFactories.PartialModelDelegateFactory(aQmrrPartialModel);
            } 
            else
            {
                aQmrrPartialModel.LogLikelihoodOfCompleteModelConditionedOnKnownHlas = null;
            }
            return aQmrrPartialModel;
        } 
 
        public PartialModelDelegate LogLikelihoodOfCompleteModelConditionedOnKnownHlas;
 
        private ModelLikelihoodFactories ModelLikelihoodFactories;
        internal OptimizationParameterList QmrrParamsStart;
		public string Peptide;
		internal Dictionary<string, double> PatientToAnyReaction;
        internal Dictionary<string, Set<Hla>> PatientList;
        public Set<Hla> HlaList; 
        public List<Hla> SwitchableHlasOfRespondingPatients; 
        public Set<Hla> KnownHlaSet;
 

		private void CreateSwitchableHlasWithRespondingPatients()
 		{
            Set<Hla> hlaSet = Set<Hla>.GetInstance();
 			foreach (string patient in PatientToAnyReaction.Keys)
			{ 
                if (PatientList.ContainsKey(patient)) 
                {
                    foreach (Hla hla in PatientList[patient]) 
                    {
                        if (!hlaSet.Contains(hla))
                        {
                            hlaSet.AddNewOrOld(hla);
                        }
                    } 
                } 
 			}
            SwitchableHlasOfRespondingPatients = new List<Hla>(hlaSet); 
		}

		private void CreateHlaList()
		{
            HlaList = Set<Hla>.GetInstance();
            foreach (Set<Hla> hlaSubList in PatientList.Values) 
			{ 
                HlaList.AddNewOrOldRange(hlaSubList);
			} 
            if (KnownHlaSet != null)
            {
                HlaList.AddNewOrOldRange(KnownHlaSet);
            }
 		}
 
 
 		//internal double LogLikelihoodOfCompleteModelConditionedOnKnownHlas(TrueCollection trueCollection, OptimizationParameterList qmrrParams)
        //{ 
        //    //!!!10/24/2005
        //    Quickscore<string, string> quickScore = CreateQuickScore(qmrrParams);
        //    double logLikelihood = ScoreQuickScore(quickScore, trueCollection, qmrrParams);
        //    return logLikelihood;
        //}
 
        //private Quickscore<string, string> CreateQuickScore(OptimizationParameterList qmrrParams) 
        //{
        //    Quickscore<string, string> quickScore = Quickscore<string, string>.GetInstance(""); 
        //    SetCauses(qmrrParams, quickScore);
        //    SetLink(qmrrParams, quickScore);
        //    SetLeak(qmrrParams, quickScore);

        //    return quickScore;
        //} 
 
        //private void SetLink(OptimizationParameterList qmrrParams, Quickscore<string, string> quickScore)
        //{ 
        //    foreach (KeyValuePair<string, List<string>> patientAndHlaList in  PatientList)
        //    {
        //        string patient = patientAndHlaList.Key;
        //        foreach (string hla in patientAndHlaList.Value)
        //        {
        //            double linkProbability; 
        //            if (LogMedianMagOfReactionsToThisHlaLessLogGlobalMedian.ContainsKey(hla)) 
        //            {
        //                linkProbability = SpecialFunctions.Bound(0,1, qmrrParams["linkA"].Value + qmrrParams["linkB"].Value * LogMedianMagOfReactionsToThisHlaLessLogGlobalMedian[hla]); 
        //            }
        //            else
        //            {
        //                linkProbability = qmrrParams["linkA"].Value;
        //            }
        //            quickScore.SetLink(hla, patient, linkProbability); 
        //        } 
        //    }
        //} 

        //private void SetLeak(OptimizationParameterList qmrrParams, Quickscore<string, string> quickScore)
        //{

        //    foreach (string patient in this.PatientList.Keys)
        //    { 
        //        quickScore.SetLeak(patient, qmrrParams["leakProbability"].Value); 
        //    }
        //} 

        //private void SetCauses(OptimizationParameterList qmrrParams, Quickscore<string, string> quickScore)
        //{
        //    foreach (string hla in this.HlaList)
        //    {
        //        quickScore.SetCause(hla, qmrrParams["causePrior"].Value); 
        //    } 
        //}
 

        //private double ScoreQuickScore(Quickscore<string, string> quickscore, TrueCollection trueCollection, OptimizationParameterList qmrrParams)
        //{
        //    Dictionary<string, bool> hlaAssignmentAsDict = trueCollection.CreateHlaAssignmentAsDict(this.HlaList);

        //    List<string> patientsWhoRespond = new List<string>(PatientToAnyReaction.Keys); 
        //    List<string> patientWhoDoNotRespond = Study.Subtract<string>(PatientList.Keys, patientsWhoRespond); 

        //    return quickscore.LogLikelihoodOfModelWithCompleteAssignments(patientsWhoRespond, patientWhoDoNotRespond, hlaAssignmentAsDict, null); 
        //}




	} 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
