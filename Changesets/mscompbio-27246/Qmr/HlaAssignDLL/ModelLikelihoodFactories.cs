using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using Optimization;
using EpipredLib;
 
namespace VirusCount.Qmr 
{
    public delegate double PartialModelDelegate(TrueCollection trueCollection, OptimizationParameterList qmrrParams); 
    public delegate double MissingAssignmentDelegate(TrueCollection trueCollection);
    public delegate double MissingParametersDelegate(OptimizationParameterList parameterList);


    abstract public class ModelLikelihoodFactories
    { 
        protected ModelLikelihoodFactories() 
        {
        } 

        static public ModelLikelihoodFactories GetInstanceThreeParamSlow(OptimizationParameterList qmrrParams)
        {
            SpecialFunctions.CheckCondition(qmrrParams.Count == 4);
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("causePrior"));
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("link")); 
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("leakProbability")); 

            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("useKnownList")); 
            SpecialFunctions.CheckCondition(!qmrrParams["useKnownList"].DoSearch);
            SpecialFunctions.CheckCondition(qmrrParams["useKnownList"].Value == 0.0 || qmrrParams["useKnownList"].Value == 1.0);


            ThreeParamSlow aThreeParamSlow = new ThreeParamSlow();
            return aThreeParamSlow; 
        } 

        static public ModelLikelihoodFactories GetInstanceTwoCausePriors(OptimizationParameterList qmrrParams, string dataset) 
        {
            SpecialFunctions.CheckCondition(qmrrParams.Count == 5);
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("causePrior"));
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("fitFactor"));
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("link"));
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("leakProbability")); 
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("useKnownList")); 
            SpecialFunctions.CheckCondition(!qmrrParams["useKnownList"].DoSearch);
            SpecialFunctions.CheckCondition(qmrrParams["useKnownList"].Value == 0.0 || qmrrParams["useKnownList"].Value == 1.0); 

            TwoCausePriors aTwoCausePriors = new TwoCausePriors();
            aTwoCausePriors.SetPeptideToFitUniverse(dataset);
            return aTwoCausePriors;
        }
 
 
        abstract public PartialModelDelegate PartialModelDelegateFactory(QmrrPartialModel qmrrPartialModel);
 
        virtual public MissingAssignmentDelegate MissingAssignmentDelegateFactory(QmrrPartialModel qmrrPartialModel, OptimizationParameterList qmrrParams)
        {
            PartialModelDelegate LogLikelihoodOfCompleteModelConditionedOnKnownHlas = PartialModelDelegateFactory(qmrrPartialModel);

            return delegate(TrueCollection trueCollection)
            { 
                return LogLikelihoodOfCompleteModelConditionedOnKnownHlas(trueCollection, qmrrParams); 
            };
        } 

        virtual public MissingParametersDelegate MissingParametersDelegateFactory(QmrrPartialModel qmrrPartialModel, TrueCollection trueCollection)
        {
            PartialModelDelegate LogLikelihoodOfCompleteModelConditionedOnKnownHlas = PartialModelDelegateFactory(qmrrPartialModel);

            return delegate(OptimizationParameterList parameterList) 
            { 
                return LogLikelihoodOfCompleteModelConditionedOnKnownHlas(trueCollection, parameterList);
            }; 
        }

        public static ModelLikelihoodFactories GetInstanceCoverage(OptimizationParameterList qmrrParamsStart, string dataset)
        {
            SpecialFunctions.CheckCondition(qmrrParamsStart.Count == 1);
            SpecialFunctions.CheckCondition(qmrrParamsStart.ContainsKey("useKnownList")); 
            SpecialFunctions.CheckCondition(!qmrrParamsStart["useKnownList"].DoSearch); 
            SpecialFunctions.CheckCondition(qmrrParamsStart["useKnownList"].Value == 0.0 || qmrrParamsStart["useKnownList"].Value == 1.0);
 
            Coverage aCoverage = new Coverage();
            return aCoverage;
        }

        internal static ModelLikelihoodFactories GetInstanceLinkPerHla(OptimizationParameterList qmrrParams, Set<Hla> candidateHlaSet)
        { 
            SpecialFunctions.CheckCondition(qmrrParams.Count == 3 + candidateHlaSet.Count); 
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("causePrior"));
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("leakProbability")); 
            SpecialFunctions.CheckCondition(qmrrParams.ContainsKey("useKnownList"));
            SpecialFunctions.CheckCondition(!qmrrParams["useKnownList"].DoSearch);
            SpecialFunctions.CheckCondition(qmrrParams["useKnownList"].Value == 1.0);
            foreach (Hla hla in candidateHlaSet)
            {
                string paramName = "link" + hla.ToString(); 
                SpecialFunctions.CheckCondition(qmrrParams.ContainsKey(paramName)); 
            }
 
            LinkPerHla aLinkPerHla = new LinkPerHla();
            return aLinkPerHla;
        }
    }

} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
