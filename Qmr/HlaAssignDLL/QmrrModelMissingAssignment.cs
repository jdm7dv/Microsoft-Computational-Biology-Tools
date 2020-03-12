using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using Optimization;
using EpipredLib; 
 
namespace VirusCount.Qmr
{ 
 	public class QmrrModelMissingAssignment
	{
        private QmrrModelMissingAssignment()
        {
        }
 
        private ModelLikelihoodFactories ModelLikelihoodFactories; 
        internal QmrrPartialModel QmrrPartialModel;
        internal OptimizationParameterList OptimizationParameterList; 
        public static QmrrModelMissingAssignment GetInstance(ModelLikelihoodFactories modelLikelihoodFactories, QmrrPartialModel qmrrPartialModel, OptimizationParameterList qmrrParams)
		{
            QmrrModelMissingAssignment aQmrrModelMissingAssignment = new QmrrModelMissingAssignment();
            aQmrrModelMissingAssignment.QmrrPartialModel = qmrrPartialModel;
            aQmrrModelMissingAssignment.OptimizationParameterList = qmrrParams;
            aQmrrModelMissingAssignment.KnownHlaSet = qmrrPartialModel.KnownHlaSet; 
            aQmrrModelMissingAssignment.SwitchableHlasOfRespondingPatients = qmrrPartialModel.SwitchableHlasOfRespondingPatients; 
            aQmrrModelMissingAssignment.ModelLikelihoodFactories = modelLikelihoodFactories;
            aQmrrModelMissingAssignment.LogLikelihoodOfCompleteModelConditionedOnKnownHlas = modelLikelihoodFactories.MissingAssignmentDelegateFactory(qmrrPartialModel, qmrrParams); 
            return aQmrrModelMissingAssignment;
            //SetOfAllHlasCount = qmrrPartialModel.HlaList.Count;
		}

        public MissingAssignmentDelegate LogLikelihoodOfCompleteModelConditionedOnKnownHlas;
 
        public List<Hla> SwitchableHlasOfRespondingPatients; 
        public Set<Hla> KnownHlaSet;
        //private double InitialLogLikelihood; 
        //private double LogHlaPrior;
        //private double Log1LessHlaPrior;
        //private Dictionary<string, double> EffectToLogPiPOfNoEffectGivenCause = new Dictionary<string,double>();
        //private Dictionary<string, double> EffectToLogPiPOfEffectGivenCause = new Dictionary<string, double>();
        //public int SetOfAllHlasCount;
        //private List<string>[] EffectCollection; 
        //private Dictionary<string,Dictionary<string, bool>> PatientToHlaSet; 

 
        //private void SetInitialLogLikelihood(QmrrPartialModel qmrrPartialModel, OptimizationParameterList qmrrParams)
        //{
        //    HlasOfRespondingPatients = qmrrPartialModel.HlasOfRespondingPatients;
			
        //    EffectCollection = new List<string>[2];
        //    EffectCollection[1] = new List<string>(qmrrPartialModel.PatientToAnyReaction.Keys); 
        //    EffectCollection[0] = Study.Subtract<string>(qmrrPartialModel.PatientList.Keys, EffectCollection[1]); 

        //    PatientToHlaSet = new Dictionary<string,Dictionary<string, bool>>(); 
        //    foreach (string effect in qmrrPartialModel.PatientList.Keys)
        //    {
        //        Dictionary<string, bool> hlaSet = new Dictionary<string, bool>();
        //        PatientToHlaSet.Add(effect, hlaSet);
        //        foreach (string cause in qmrrPartialModel.PatientList[effect])
        //        { 
        //            hlaSet.Add(cause, true); 
        //        }
        //    } 


        //    SetOfAllHlasCount = qmrrPartialModel.HlaList.Count;

        //    Debug.Fail("Need code");
        //    InitialLogLikelihood = Math.Log(qmrrParams["leak"].Value); //!!!10/25/05 +LogLikelihoodOfAbsentEffectsWhoShareNoHlasWithPresentEffects(); 
        //    LogHlaPrior = Math.Log(qmrrParams["causePrior"].Value); 
        //    Log1LessHlaPrior = Math.Log(1.0 - qmrrParams["causePrior"].Value);
 
        //}

		//abstract public double LogLikelihoodOfCompleteModelConditionedOnKnownHlas(TrueCollection trueCollection);
        //{
        //    double logLikelihood = InitialLogLikelihood;
        //    logLikelihood += LogLikelihoodOfCauses(trueCollection); 
        //    logLikelihood += LogLikelihoodOfEffects(trueCollection); 
        //    return logLikelihood;
        //} 

 		//internal double LogLikelihoodOfCompleteModelConditionedOnKnownHlas(TrueCollection trueCollection, OptimizationParameterList qmrrParams)
 		//{
		//    Dictionary<string, bool> hlaAssignmentAsDict = trueCollection.CreateHlaAssignmentAsDict(this.HlaList);

 		//EffectCollection 
		//    List<string> patientsWhoRespond = new List<string>(PatientToAnyReaction.Keys); 
		//    List<string> patientWhoDoNotRespond = Study.Subtract<string>(PatientList.Keys, patientsWhoRespond);
 


		//}

        //static bool[] FalseAndTrue = new bool[] { false, true };
        //private double LogLikelihoodOfEffects(TrueCollection trueCollection) 
        //{ 
        //    double logLikelihood = 0.0;
        //    for(int presentAsInt = 0; presentAsInt < 2; ++presentAsInt) 
        //    {
        //        Debug.Fail("Need code");
        //        double[] tallyToLogLikelihoodOfEffect = null; //!!!10/25/05 PresentToTallyToLogLikelihoodOfEffect[presentAsInt];
        //        foreach (string effect in EffectCollection[presentAsInt])
        //        {
        //            int hlaTally = HlaTally(effect, trueCollection); 
        //            logLikelihood += tallyToLogLikelihoodOfEffect[hlaTally]; 
        //        }
        //    } 
        //    return logLikelihood;
        //}

        //// e.g. 41 means 4 of the patient's hlas are on and 1 is not
        //private int HlaTally(string effect, TrueCollection trueCollection)
        //{ 
        //    Dictionary<string, bool> hlaSet = PatientToHlaSet[effect]; 
        //    int hlaTally = hlaSet.Count;
        //    foreach (string hla in trueCollection) 
        //    {
        //        if (hlaSet.ContainsKey(hla))
        //        {
        //            hlaTally += 9; // subtract 1 and add 10
        //        }
        //    } 
        //    return hlaTally; 
        //}
 
        ////private double LogLikelihoodOfAbsent(ref double logLikelihood, ref Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause)
        ////{
        ////    double logLikelihood = 0.0;
        ////    foreach (string effect in PatientsWhoRespond)
        ////    {
        ////        double logPiPOfNoEffectGivenCause = GetLogPiPOfNoEffectGivenCause(effect); 
        ////        logLikelihood += Math.Log(logPiPOfNoEffectGivenCause); 
        ////    }
        ////    return logLikelihood; 
        ////}

        ////private static void TallyLogLikelihoodOfAbsentEffects(List<string> patientsWhoDoNotRespond, ref double logLikelihood, ref Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause)
        ////{
        ////    foreach (string effect in patientsWhoDoNotRespond)
        ////    { 
        ////        double logPiPOfNoEffectGivenCause = GetLogPiPOfNoEffectGivenCause(effectToLogPiPOfNoEffectGivenCause, effect); 
        ////        logLikelihood += logPiPOfNoEffectGivenCause;
        ////    } 

        ////}

        //private static double GetLogPiPOfNoEffectGivenCause(ref Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause, string effect)
        //{
        //    double logPiPOfNoEffectGivenCause; 
        //    if (effectToLogPiPOfNoEffectGivenCause.ContainsKey(effect)) 
        //    {
        //        logPiPOfNoEffectGivenCause = effectToLogPiPOfNoEffectGivenCause[effect]; 
        //        effectToLogPiPOfNoEffectGivenCause.Remove(effect);
        //    }
        //    else
        //    {
        //        logPiPOfNoEffectGivenCause = 0.0;
        //    } 
        //    return logPiPOfNoEffectGivenCause; 
        //}
 

        ////private Dictionary<string, double> CreateEffectToLogPiPOfNoEffectGivenCause(Dictionary<string, bool> hlaAssignment)
        ////{
        ////    Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause = new Dictionary<string, double>();
        ////    foreach (KeyValuePair<string, double> causeAndPrior in CauseCollection)
        ////    { 
        ////        TCause cause = causeAndPrior.Key; 
        ////        double prior = causeAndPrior.Value;
 
        ////        if (hlaAssignment[cause] || cause.Equals(Leak))
        ////        {
        ////            foreach (KeyValuePair<TEffect, double> effectAndConditionalProbability in CauseEffectCollection[cause])
        ////            {
        ////                TEffect effect = effectAndConditionalProbability.Key;
        ////                double conditionalProbability = effectAndConditionalProbability.Value; 
 
        ////                double logPiPOfNoEffectGivenCause = (effectToLogPiPOfNoEffectGivenCause.ContainsKey(effect)) ? effectToLogPiPOfNoEffectGivenCause[effect] : 0.0;
        ////                effectToLogPiPOfNoEffectGivenCause[effect] = logPiPOfNoEffectGivenCause + Math.Log(1.0 - conditionalProbability); 
        ////            }
        ////        }
        ////    }
        ////}

 
 
        //private double LogLikelihoodOfCauses(TrueCollection trueCollection) //, out Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause)
        //{ 
        //    double logLikelihood = LogHlaPrior * trueCollection.Count + Log1LessHlaPrior * (SetOfAllHlasCount - trueCollection.Count);
        //    return logLikelihood;

        //        //if (causeValue)
        //        //{
        //        //    foreach (KeyValuePair<string, double> effectAndConditionalProbability in CauseEffectCollection[cause]) 
        //        //    { 
        //        //        string effect = effectAndConditionalProbability.Key;
        //        //        double conditionalProbability = effectAndConditionalProbability.Value; 

        //        //        double logPiPOfNoEffectGivenCause = (effectToLogPiPOfNoEffectGivenCause.ContainsKey(effect)) ? effectToLogPiPOfNoEffectGivenCause[effect] : 0.0;
        //        //        effectToLogPiPOfNoEffectGivenCause[effect] = logPiPOfNoEffectGivenCause + Math.Log(1.0 - conditionalProbability);
        //        //    }
        //        //}
        //} 
 

        ////private Quickscore<string, string> CreateQuickScore(OptimizationParameterList qmrrParams) 
        ////{
        ////    Quickscore<string, string> quickScore = Quickscore<string, string>.GetInstance("");
        ////    SetCauses(qmrrParams, quickScore);
        ////    SetLink(qmrrParams, quickScore);
        ////    SetLeak(qmrrParams, quickScore);
 
        ////    return quickScore; 
        ////}
 
        ////private void SetLink(OptimizationParameterList qmrrParams, Quickscore<string, string> quickScore)
        ////{
        ////    foreach (KeyValuePair<string, List<string>> patientAndHlaList in  PatientList)
        ////    {
        ////        string patient = patientAndHlaList.Key;
        ////        foreach (string hla in patientAndHlaList.Value) 
        ////        { 
        ////            double linkProbability;
        ////            if (LogMedianMagOfReactionsToThisHlaLessLogGlobalMedian.ContainsKey(hla)) 
        ////            {
        ////                linkProbability = SpecialFunctions.Bound(0,1,qmrrParams.LinkA + qmrrParams.LinkB * LogMedianMagOfReactionsToThisHlaLessLogGlobalMedian[hla]);
        ////            }
        ////            else
        ////            {
        ////                linkProbability = qmrrParams.LinkA; 
        ////            } 
        ////            quickScore.SetLink(hla, patient, linkProbability);
        ////        } 
        ////    }
        ////}

        ////private void SetLeak(OptimizationParameterList qmrrParams, Quickscore<string, string> quickScore)
		//{
 
		//    foreach (string patient in this.PatientList.Keys) 
 		//    {
 		//        quickScore.SetLeak(patient, qmrrParams.LeakProbability); 
		//    }
 		//}

		//private void SetCauses(OptimizationParameterList qmrrParams, Quickscore<string, string> quickScore)
		//{
		//    foreach (string hla in this.HlaList) 
		//    { 
		//        quickScore.SetCause(hla, qmrrParams.CausePrior);
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
