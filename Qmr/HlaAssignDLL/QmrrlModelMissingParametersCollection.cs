using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Adapt.Tabulate;
using Optimization;

namespace VirusCount.Qmr 
{ 
    public class QmrrlModelMissingParametersCollection
    { 
        private QmrrlModelMissingParametersCollection()
        {
        }

        public static QmrrlModelMissingParametersCollection GetInstance(ModelLikelihoodFactories modelLikelihoodFactories, QmrrPartialModelCollection qmrrPartialModelCollection, Dictionary<string, BestSoFar<double, TrueCollection>> PeptideToBestHlaAssignmentSoFar)
        { 
 
            QmrrlModelMissingParametersCollection aQmrrlModelMissingParametersCollection = new QmrrlModelMissingParametersCollection();
            foreach (QmrrPartialModel qmrrPartialModel in qmrrPartialModelCollection) 
            {
                QmrrlModelMissingParameters aQmrrlModelMissingParameters = QmrrlModelMissingParameters.GetInstance(modelLikelihoodFactories, qmrrPartialModel, PeptideToBestHlaAssignmentSoFar[qmrrPartialModel.Peptide]);
                aQmrrlModelMissingParametersCollection.Collection.Add(aQmrrlModelMissingParameters);
            }
            aQmrrlModelMissingParametersCollection.GridSearch = GridSearch.GetInstance("Grid");
            return aQmrrlModelMissingParametersCollection; 
        } 

        private List<QmrrlModelMissingParameters> Collection = new List<QmrrlModelMissingParameters>(); 

        public double ScoreParameterList(OptimizationParameterList parameterList)
        {
            double sum = 0.0;
            foreach (QmrrlModelMissingParameters qmrrlModelMissingParameters in Collection)
            { 
                double loglikelihood = qmrrlModelMissingParameters.LogLikelihoodOfCompleteModelConditionedOnKnownHlas(parameterList); 
                sum += loglikelihood;
            } 
            return sum;
        }



 
        internal OptimizationParameterList FindBestParams(OptimizationParameterList qmrrParamsStart, out double logLikelihood) 
        {
 
            OptimizationParameterList qmrrParamsEnd = qmrrParamsStart.Clone();
            logLikelihood = GridSearch.Optimize(ScoreParameterList, qmrrParamsEnd, 10, 10);
            return qmrrParamsEnd;
        }

        GridSearch GridSearch; 
    } 

    public class QmrrlModelMissingParameters 
    {
        private QmrrlModelMissingParameters()
        {
        }

        internal static QmrrlModelMissingParameters GetInstance(ModelLikelihoodFactories modelLikelihoodFactories, QmrrPartialModel qmrrPartialModel, BestSoFar<double, TrueCollection> bestSoFar) 
        { 
            QmrrlModelMissingParameters aQmrrlModelMissingParameters = new QmrrlModelMissingParameters();
            aQmrrlModelMissingParameters.QmrrPartialModel = qmrrPartialModel; 
            aQmrrlModelMissingParameters.TrueCollection = bestSoFar.Champ;
            aQmrrlModelMissingParameters.ModelLikelihoodFactories = modelLikelihoodFactories;
            aQmrrlModelMissingParameters.LogLikelihoodOfCompleteModelConditionedOnKnownHlas = modelLikelihoodFactories.MissingParametersDelegateFactory(qmrrPartialModel, bestSoFar.Champ);
            return aQmrrlModelMissingParameters;
        }
 
        public MissingParametersDelegate LogLikelihoodOfCompleteModelConditionedOnKnownHlas; 
        internal QmrrPartialModel QmrrPartialModel;
        internal TrueCollection TrueCollection; 
        internal ModelLikelihoodFactories ModelLikelihoodFactories;



        //public double LogLikelihoodOfCompleteModelConditionedOnKnownHlas(Dictionary<string, bool> hlaAssignmentAsDict)
        //{ 
        //    //TrueCollection trueCollection 
        //    //             = trueCollection.CreateHlaAssignmentAsDict(HlasOfRespondingPatients);
 
        //    double logLikelihood = 0;
        //    Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause;
        //    TallyLogLikelihoodOfCauses(hlaAssignmentAsDict, ref logLikelihood, out effectToLogPiPOfNoEffectGivenCause);

        //    //
        //    //    TallyLogLikelihoodOfPresentEffects(patientsWhoRespond, ref logLikelihood, ref effectToLogPiPOfNoEffectGivenCause); 
        //    //    TallyLogLikelihoodOfAbsentEffects(patientsWhoDoNotRespond, ref logLikelihood, ref effectToLogPiPOfNoEffectGivenCause); 

        //    //    SpecialFunctions.CheckCondition(effectToLogPiPOfNoEffectGivenCause.Count == 0); //!!!raise error 
        //}

        ////internal double LogLikelihoodOfCompleteModelConditionedOnKnownHlas(TrueCollection trueCollection, OptimizationParameterList qmrrParams)
        ////{
        ////    Dictionary<string, bool> hlaAssignmentAsDict = trueCollection.CreateHlaAssignmentAsDict(this.HlaList);
 
        ////    List<string> patientsWhoRespond = new List<string>(PatientToAnyReaction.Keys); 
        ////    List<string> patientWhoDoNotRespond = Study.Subtract<string>(PatientList.Keys, patientsWhoRespond);
 


        ////}

        //private static void TallyLogLikelihoodOfAbsentEffects(List<string> patientsWhoDoNotRespond, ref double logLikelihood, ref Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause)
        //{ 
        //    foreach (string effect in patientsWhoDoNotRespond) 
        //    {
        //        double logPiPOfNoEffectGivenCause = GetLogPiPOfNoEffectGivenCause(effectToLogPiPOfNoEffectGivenCause, effect); 
        //        logLikelihood += logPiPOfNoEffectGivenCause;
        //    }

        //}

        //private static void TallyLogLikelihoodOfPresentEffects(List<string> patientsWhoRespond, ref double logLikelihood, ref Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause) 
        //{ 
        //    foreach (string effect in patientsWhoRespond)
        //    { 
        //        double logPiPOfNoEffectGivenCause = GetLogPiPOfNoEffectGivenCause(effectToLogPiPOfNoEffectGivenCause, effect);
        //        double probabilityOfOutcome = 1.0 - Math.Exp(logPiPOfNoEffectGivenCause);
        //        logLikelihood += Math.Log(probabilityOfOutcome);
        //    }
        //}
 
 

        //private void TallyLogLikelihoodOfCauses(Dictionary<string, bool> hlaAssignment, ref double logLikelihood, out Dictionary<string, double> effectToLogPiPOfNoEffectGivenCause) 
        //{
        //    effectToLogPiPOfNoEffectGivenCause = new Dictionary<string, double>();
        //    foreach (KeyValuePair<string, double> causeAndPrior in CauseCollection)
        //    {
        //        string cause = causeAndPrior.Key;
        //        double prior = causeAndPrior.Value; 
 
        //        bool causeValue = (cause.Equals(Leak)) ? true : hlaAssignment[cause];
 
        //        logLikelihood += Math.Log(causeValue ? prior : (1.0 - prior));

        //        if (causeValue)
        //        {
        //            foreach (KeyValuePair<string, double> effectAndConditionalProbability in CauseEffectCollection[cause])
        //            { 
        //                string effect = effectAndConditionalProbability.Key; 
        //                double conditionalProbability = effectAndConditionalProbability.Value;
 
        //                double logPiPOfNoEffectGivenCause = (effectToLogPiPOfNoEffectGivenCause.ContainsKey(effect)) ? effectToLogPiPOfNoEffectGivenCause[effect] : 0.0;
        //                effectToLogPiPOfNoEffectGivenCause[effect] = logPiPOfNoEffectGivenCause + Math.Log(1.0 - conditionalProbability);
        //            }
        //        }
        //    }
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
        //                linkProbability = SpecialFunctions.Bound(0,1,qmrrParams.LinkA + qmrrParams.LinkB * LogMedianMagOfReactionsToThisHlaLessLogGlobalMedian[hla]);
        //            }
        //            else
        //            {
        //                linkProbability = qmrrParams.LinkA; 
        //            } 
        //            quickScore.SetLink(hla, patient, linkProbability);
        //        } 
        //    }
        //}

        //private void SetLeak(OptimizationParameterList qmrrParams, Quickscore<string, string> quickScore)
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
