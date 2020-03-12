using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using Optimization;
using EpipredLib; 
 
namespace VirusCount.Qmr
{ 
    public class ThreeParamSlow : ModelLikelihoodFactories
    {
        internal ThreeParamSlow()
        {
        }
 
        public override PartialModelDelegate PartialModelDelegateFactory(QmrrPartialModel qmrrPartialModel) 
        {
            return delegate(TrueCollection trueCollection, OptimizationParameterList qmrrParams) 
            {
                return LogLikelihoodOfCompleteModelConditionedOnKnownHlas(qmrrPartialModel, trueCollection, qmrrParams);
            };
        }

        virtual protected double LogLikelihoodOfCompleteModelConditionedOnKnownHlas(QmrrPartialModel qmrrPartialModel, TrueCollection trueCollection, OptimizationParameterList qmrrParams) 
        { 
            double logLikelihood =
                  LogLikelihoodOfCausesConditionedOnKnownHlas(qmrrPartialModel, trueCollection, qmrrParams) 
                + LogLikelihoodOfEffects(qmrrPartialModel, trueCollection, qmrrParams);
            return logLikelihood;
        }

        virtual protected double LogLikelihoodOfCausesConditionedOnKnownHlas(QmrrPartialModel qmrrPartialModel, TrueCollection trueCollection, OptimizationParameterList qmrrParams)
        { 
            double logCausePrior = Math.Log(qmrrParams["causePrior"].Value); 
            double logOneLessCausePrior = Math.Log(1.0 - qmrrParams["causePrior"].Value);
 
            int trueCountLessKnown = trueCollection.Count - qmrrPartialModel.KnownHlaSet.Count;
            Debug.Assert(trueCountLessKnown >= 0); // real assert
            int falseCount = qmrrPartialModel.HlaList.Count - trueCollection.Count;

            double logLikelihood =
                  (double)trueCountLessKnown * logCausePrior 
                + (double)falseCount * logOneLessCausePrior; 
            return logLikelihood;
 
        }

        virtual protected double LogLikelihoodOfEffects(QmrrPartialModel qmrrPartialModel, TrueCollection trueCollection, OptimizationParameterList qmrrParams)
        {
            double logOneLessLink = Math.Log(1.0 - qmrrParams["link"].Value);
            double logOneLessLeakProbability = Math.Log(1.0 - qmrrParams["leakProbability"].Value); 
 
            double logLikelihood = 0.0;
 
            Set<Hla> trueCollectionAsSet = trueCollection.CreateHlaAssignmentAsSet();

            foreach (KeyValuePair<string, Set<Hla>> patientAndHlaList in qmrrPartialModel.PatientList)
            {
                double logLikelihoodNoReactionInThisPatient =
                        logOneLessLeakProbability 
                        + NumberOfPositiveHlas(patientAndHlaList.Value, trueCollectionAsSet) * logOneLessLink; 
                bool didReact = qmrrPartialModel.PatientToAnyReaction.ContainsKey(patientAndHlaList.Key);
                logLikelihood += LogLikelihoodOfThisPatient(logLikelihoodNoReactionInThisPatient, didReact); 
            }

            return logLikelihood;

        }
 
        internal static double LogLikelihoodOfThisPatient(double logLikelihoodNoReactionInThisPatient, bool didReact) 
        {
            double logLikelihoodOfThisPatient; 
            if (didReact)
            {
                logLikelihoodOfThisPatient = Math.Log(1.0 - Math.Exp(logLikelihoodNoReactionInThisPatient));
            }
            else
            { 
                logLikelihoodOfThisPatient = logLikelihoodNoReactionInThisPatient; 
            }
            return logLikelihoodOfThisPatient; 
        }

        private int NumberOfPositiveHlas(Set<Hla> patientHlaList, Set<Hla> hlaSet)
        {
            int numberOfPositiveHlas = 0;
            foreach (Hla hla in patientHlaList) 
            { 
                if (hlaSet.Contains(hla))
                { 
                    ++numberOfPositiveHlas;
                }
            }
            return numberOfPositiveHlas;
        }
 
    } 

 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
