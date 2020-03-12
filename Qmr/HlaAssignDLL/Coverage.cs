using System; 
using System.Collections.Generic;
using System.Text;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using Optimization;
using EpipredLib; 
 
namespace VirusCount.Qmr
{ 
    public class Coverage : ModelLikelihoodFactories
    {
        internal Coverage()
        {
        }
 
        public override PartialModelDelegate PartialModelDelegateFactory(QmrrPartialModel qmrrPartialModel) 
        {
            return delegate(TrueCollection trueCollection, OptimizationParameterList qmrrParams) 
            {
                return LogLikelihoodOfCompleteModelConditionedOnKnownHlas(qmrrPartialModel, trueCollection, qmrrParams);
            };
        }

        private double LogLikelihoodOfCompleteModelConditionedOnKnownHlas(QmrrPartialModel qmrrPartialModel, TrueCollection trueCollection, OptimizationParameterList qmrrParams) 
        { 
            Set<Hla> trueHlaSet = trueCollection.CreateHlaAssignmentAsSet();
            int reactionsCoveredCount = CountReactionsCovered(qmrrPartialModel, trueHlaSet); 
            SpecialFunctions.CheckCondition(reactionsCoveredCount < 1000);
            int trueCount = trueCollection.Count;
            SpecialFunctions.CheckCondition(trueCount < 1000);
            int falseCount = qmrrPartialModel.HlaList.Count - trueCollection.Count;
            SpecialFunctions.CheckCondition(falseCount < 1000);
            string llAsString = string.Format("{0:000}.{1:000}{2:000}", reactionsCoveredCount, falseCount, trueCount); 
            double logLikelihood = double.Parse(llAsString); 
            return logLikelihood;
        } 

        private int CountReactionsCovered(QmrrPartialModel qmrrPartialModel, Set<Hla> trueHlaSet)
        {
            int reactionsCoveredCount = 0;
            foreach (string patient in qmrrPartialModel.PatientToAnyReaction.Keys)
            { 
                if (NonEmptyIntersection(qmrrPartialModel.PatientList[patient], trueHlaSet)) 
                {
                    ++reactionsCoveredCount; 
                }
            }
            return reactionsCoveredCount;
        }

        private bool NonEmptyIntersection(Set<Hla> hlaOfPatient, Set<Hla> trueHlaSet) 
        { 
            foreach (Hla hla in hlaOfPatient)
            { 
                if (trueHlaSet.Contains(hla))
                {
                    return true;
                }
            }
            return false; 
        } 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
