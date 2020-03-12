using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;
using Mlas.Tabulate;
 
namespace VirusCount.PhyloTree 
{
    /// <summary>
    /// This will change when Gaussian is implemented
    /// </summary>

    public class ModelEvaluatorGaussian : ModelEvaluator 
    {
        public const string BaseName = "GaussianConditional";

        public override string Name
        {
            get { return BaseName + ((DistributionGaussianConditional)AltDistn).GetName(); } 
        } 

        protected ModelEvaluatorGaussian(List<IDistributionSingleVariable> nullDistns, DistributionGaussianConditional altDistn, ModelScorer scorer) 
            : base(nullDistns, altDistn, scorer)
        { }

        new public static ModelEvaluatorGaussian GetInstance(string nameAndParameters, ModelScorer scorer)
        {
            IDistributionSingleVariable nullDistn = DistributionGaussianConditional.GetSingleVariableInstance(nameAndParameters); 
            DistributionGaussianConditional altDistn = DistributionGaussianConditional.GetInstance(nameAndParameters); 
            return new ModelEvaluatorGaussian(SpecialFunctions.CreateSingletonList(nullDistn), altDistn, scorer);
        } 

        protected EvaluationResults CreateDummyResults(int predNonMissing, int targNonMissing, int globalNonMissingCount)
        {
            IDistributionSingleVariable nullDistn = NullDistns[0];
            OptimizationParameterList nullParams = nullDistn.GetParameters();
            foreach (OptimizationParameter param in nullParams) 
            { 
                param.Value = double.NegativeInfinity;
            } 
            Score nullScore = Score.GetInstance(0, nullParams, nullDistn);


            OptimizationParameterList altParams = AltDistn.GetParameters();
            foreach (OptimizationParameter param in altParams)
            { 
                param.Value = double.NegativeInfinity; 
            }
            Score altScore = Score.GetInstance(0, altParams, AltDistn); 

            return EvaluationResultsGaussian.GetInstance(this, nullScore, altScore, predNonMissing, targNonMissing, globalNonMissingCount, ChiSquareDegreesOfFreedom);
        }

        public override string ToHeaderString()
        { 
            EvaluationResults dummyResults = CreateDummyResults(0, 0, 0); 
            return dummyResults.ToHeaderString();
        } 


        public override EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> predMap, Converter<Leaf, SufficientStatistics> targMap)
        {
            int predCount = ModelScorer.PhyloTree.CountOfNonMissingLeaves(predMap);
            int targCount = ModelScorer.PhyloTree.CountOfNonMissingLeaves(targMap); 
            int globalNonMissingCount = ModelScorer.PhyloTree.CountOfNonMissingLeaves(predMap, targMap); 

            MessageInitializerGaussian nullMessageInitializer = MessageInitializerGaussian.GetInstance( 
                predMap, targMap, (DistributionGaussianConditional)NullDistns[0], ModelScorer.PhyloTree.LeafCollection);
            Score nullScore = ModelScorer.MaximizeLikelihood(nullMessageInitializer);

            MessageInitializerGaussian altMessageInitializer = MessageInitializerGaussian.GetInstance(
                predMap, targMap, (DistributionGaussianConditional)AltDistn, ModelScorer.PhyloTree.LeafCollection);
            Score altScore = ModelScorer.MaximizeLikelihood(altMessageInitializer); 
 
            EvaluationResults evalResults = EvaluationResultsGaussian.GetInstance(this, nullScore, altScore, predCount, targCount, globalNonMissingCount, ChiSquareDegreesOfFreedom);
            return evalResults; 
        }

        public override EvaluationResults EvaluateModelOnDataGivenParams(Converter<Leaf, SufficientStatistics> predMap, Converter<Leaf, SufficientStatistics> targMap, EvaluationResults previousResults)
        {
            int predCount = ModelScorer.PhyloTree.CountOfNonMissingLeaves(predMap);
            int targCount = ModelScorer.PhyloTree.CountOfNonMissingLeaves(targMap); 
            int globalNonMissingCount = ModelScorer.PhyloTree.CountOfNonMissingLeaves(predMap, targMap); 

            MessageInitializerGaussian nullMessageInitializer = MessageInitializerGaussian.GetInstance( 
                predMap, targMap, (DistributionGaussianConditional)NullDistns[0], ModelScorer.PhyloTree.LeafCollection);
            double nullLL = ModelScorer.ComputeLogLikelihoodModelGivenData(nullMessageInitializer, previousResults.NullScores[0].OptimizationParameters);
            Score nullScore = Score.GetInstance(nullLL, previousResults.NullScores[0].OptimizationParameters, previousResults.NullScores[0].Distribution);

            MessageInitializerGaussian altMessageInitializer = MessageInitializerGaussian.GetInstance(
                predMap, targMap, (DistributionGaussianConditional)AltDistn, ModelScorer.PhyloTree.LeafCollection); 
            double altLL = ModelScorer.ComputeLogLikelihoodModelGivenData(altMessageInitializer, previousResults.AltScore.OptimizationParameters); 
            Score altScore = Score.GetInstance(altLL, previousResults.AltScore.OptimizationParameters, previousResults.AltScore.Distribution);
 
            EvaluationResults evalResults = EvaluationResultsGaussian.GetInstance(this, nullScore, altScore, predCount, targCount, globalNonMissingCount, ChiSquareDegreesOfFreedom);
            return evalResults;
        }
    }

    /// <summary>
    /// This will change when Gaussian is implemented
    /// </summary>
    class EvaluationResultsGaussian : EvaluationResults 
    { 
        private readonly int _predNonMissing;
        private readonly int _targNonMissing; 

        protected EvaluationResultsGaussian(ModelEvaluator modelEval, List<Score> nullScores, Score altScore,
            int predNonMissing, int targNonMissing, int globalNonMissing, int chiSquareDegreesOfFreedom)
            :
            base(modelEval, nullScores, altScore, chiSquareDegreesOfFreedom, globalNonMissing)
        { 
            _predNonMissing = predNonMissing; 
            _targNonMissing = targNonMissing;
        } 

        public static EvaluationResultsGaussian GetInstance(ModelEvaluator modelEval, Score nullScore, Score altScore,
            int predNonMissing, int targNonMissing, int globalNonMissing, int chiSquareDegreesOfFreedom)
        {
            List<Score> singletonList = SpecialFunctions.CreateSingletonList(nullScore);
            return new EvaluationResultsGaussian(modelEval, singletonList, altScore, predNonMissing, targNonMissing, globalNonMissing, chiSquareDegreesOfFreedom); 
        } 

        public override string IidStatsHeaderString() 
        {
            return SpecialFunctions.CreateTabString(Tabulate.PredictorNonMissingCountColumnName, Tabulate.TargetNonMissingCountColumnName, base.IidStatsHeaderString());
        }

        public override string IidStatsString()
        { 
            return SpecialFunctions.CreateTabString(_predNonMissing, _targNonMissing, base.IidStatsString()); 
        }
    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
