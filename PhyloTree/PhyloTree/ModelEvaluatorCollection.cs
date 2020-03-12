using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree
{ 
    public abstract class ModelEvaluatorCollection : ModelEvaluator 
    {
        private readonly List<ModelEvaluatorCrossValidate> _modelsToEvaluate; 

        protected ModelEvaluatorCollection(List<ModelEvaluator> modelsToEvaluate)
            :
            base(modelsToEvaluate[0].NullDistns, modelsToEvaluate[0].AltDistn, modelsToEvaluate[0].ModelScorer)
        {
            _modelsToEvaluate = new List<ModelEvaluatorCrossValidate>(modelsToEvaluate.Count); 
 
            foreach (ModelEvaluator modelEvaluator in modelsToEvaluate)
            { 
                ModelEvaluatorCrossValidate modelToAdd;
                if (modelEvaluator is ModelEvaluatorCrossValidate)
                {
                    modelToAdd = (ModelEvaluatorCrossValidate)modelEvaluator;
                }
                else 
                { 
                    modelToAdd = ModelEvaluatorCrossValidate.GetInstance(modelEvaluator);
                } 
                _modelsToEvaluate.Add(modelToAdd);
            }
        }

        public override EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2)
        { 
            EvaluationResults bestResults = null; 

            foreach (ModelEvaluatorCrossValidate model in _modelsToEvaluate) 
            {
                EvaluationResults results = model.EvaluateModelOnData(v1, v2);
                if (bestResults == null || results.AltScore.Loglikelihood > bestResults.AltScore.Loglikelihood)
                {
                    bestResults = results;
                } 
            } 
            //EvaluationResults resultsFromFullDataset = bestResults.ModelEvaluator.EvaluateModelOnData(v1, v2);
            EvaluationResults resultsFromFullDataset = ((ModelEvaluatorCrossValidate)(bestResults.ModelEvaluator)).InternalEvaluator.EvaluateModelOnData(v1, v2); 

            return resultsFromFullDataset;
        }

        public override EvaluationResults EvaluateModelOnDataGivenParams(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2, EvaluationResults previousResults)
        { 
 
            EvaluationResults newResults = previousResults.ModelEvaluator.EvaluateModelOnDataGivenParams(v1, v2, previousResults);
 
            return newResults;
        }

        public override string ToHeaderString()
        {
            return _modelsToEvaluate[0].ToHeaderString(); 
        } 
    }
 
    //class EvaluationResultsCollection : EvaluationResults
    //{
    //    public readonly ModelEvaluator ModelEvaluator;
    //    public readonly EvaluationResults EvaluationResults;

    //    private EvaluationResultsCollection(ModelEvaluator evaluator, EvaluationResults results) 
    //        : 
    //        base(results.NullScores, results.AltScore, results.ChiSquareDegreesOfFreedom, results.GlobalNonMissingCount)
    //    { 
    //        ModelEvaluator = evaluator;
    //        EvaluationResults = results;
    //    }

    //    public static EvaluationResultsCollection GetInstance(ModelEvaluator evaluator, EvaluationResults results)
    //    { 
    //        return new EvaluationResultsCollection(evaluator, results); 
    //    }
 
    //    public override double ComputePValue()
    //    {
    //        return EvaluationResults.ComputePValue();
    //    }

    //    public override string IidStatsHeaderString() 
    //    { 
    //        return EvaluationResults.IidStatsHeaderString();
    //    } 

    //    public override string IidStatsString()
    //    {
    //        return EvaluationResults.IidStatsString();
    //    }
    //} 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
