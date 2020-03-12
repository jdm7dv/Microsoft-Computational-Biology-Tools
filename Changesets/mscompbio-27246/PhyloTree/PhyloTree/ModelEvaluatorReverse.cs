using System; 
using System.Collections.Generic;
using System.Text;

namespace VirusCount.PhyloTree
{
    public class ModelEvaluatorReverse : ModelEvaluator 
    { 
        public const string BaseName = "Reverse";
        ModelEvaluator _internalEvaluator; 

        protected ModelEvaluatorReverse(ModelEvaluator evaluator):
            base(evaluator.NullDistns, evaluator.AltDistn, evaluator.ModelScorer)
        {
            _internalEvaluator = evaluator;
        } 
 
        public static ModelEvaluatorReverse GetInstance(ModelEvaluator modelEvaluator)
        { 
            return new ModelEvaluatorReverse(modelEvaluator);
        }

        public override string Name
        {
            get { return BaseName + _internalEvaluator.Name; } 
        } 

        public override EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2) 
        {
            EvaluationResults internalResults = _internalEvaluator.EvaluateModelOnData(v2, v1);
            internalResults.ModelEvaluator = this;
            return internalResults;
        }
 
        public override EvaluationResults EvaluateModelOnDataGivenParams(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2, EvaluationResults previousResults) 
        {
            EvaluationResults internalResults = _internalEvaluator.EvaluateModelOnDataGivenParams(v2, v1, previousResults); 
            internalResults.ModelEvaluator = this;
            return internalResults;
        }

        public override string ToHeaderString()
        { 
            return _internalEvaluator.ToHeaderString(); 
        }
    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
