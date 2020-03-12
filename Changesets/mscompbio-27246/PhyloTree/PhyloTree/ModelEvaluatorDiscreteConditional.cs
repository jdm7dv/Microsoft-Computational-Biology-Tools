using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using Optimization;
using System.Diagnostics;
 
 
namespace VirusCount.PhyloTree
{ 
    public class ModelEvaluatorDiscreteConditional : ModelEvaluatorDiscrete
    {
        new public const string BaseName = "Conditional";
        private readonly bool _includePredictorInScore = false;

        protected ModelEvaluatorDiscreteConditional( 
            DistributionDiscreteSingleVariable nullDistn, 
            DistributionDiscreteConditional conditionalDistn,
            ModelScorer scorer, 
            bool includePredictorInScore)
            : base(SpecialFunctions.CreateSingletonList<IDistributionSingleVariable>(nullDistn), conditionalDistn, scorer)
        {
            _includePredictorInScore = includePredictorInScore;
        }
 
        new public static ModelEvaluatorDiscreteConditional GetInstance(string leafDistributionName, ModelScorer modelScorer) 
        {
            return GetInstance(leafDistributionName, modelScorer, false); 
        }

        public static ModelEvaluatorDiscreteConditional GetInstance(string leafDistributionName, ModelScorer modelScorer, bool includePredictorInScore)
        {
            DistributionDiscreteSingleVariable nullDistn = DistributionDiscreteSingleVariable.GetInstance();
            DistributionDiscreteConditional condDistn = DistributionDiscreteConditional.GetInstance(leafDistributionName); 
            return new ModelEvaluatorDiscreteConditional(nullDistn, condDistn, modelScorer, includePredictorInScore); 
        }
 
        protected DistributionDiscreteSingleVariable NullDistn
        {
            get { return (DistributionDiscreteSingleVariable)NullDistns[0]; }
        }

        public override string Name 
        { 
            get { return ModelEvaluatorDiscrete.BaseName + BaseName + AltDistn.ToString(); }
        } 

        public override EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> predictorMap, Converter<Leaf, SufficientStatistics> targetMap)
        {
            EvaluationResults evalResults;
            int[] fisherCounts = ModelScorer.PhyloTree.FisherCounts(predictorMap, targetMap);
            int[] realFisherCounts = fisherCounts;  // for compatability when NAIVE_EQUILIBRIUM is set 
 
#if NAIVE_EQUILIBRIUM
            //USE THIS FOR BACKWARDS COMPATABILITY 
            int[] tempCounts = ModelScorer.PhyloTree.CountsOfLeaves(targetMap);
            fisherCounts = tempCounts;
#endif

            //MessageInitializerDiscrete nullMessageInitializer = MessageInitializerDiscrete.GetInstance(predictorMap, targetMap, NullDistn, fisherCounts, ModelScorer.PhyloTree.LeafCollection);
            //if (TryShortCutFromCounts(realFisherCounts, nullMessageInitializer, out evalResults)) 
            //{ 
            //    return evalResults;
            //} 

            //Score nullScore = ModelScorer.MaximizeLikelihood(nullMessageInitializer);
            bool isInvariant;

            Score nullScoreTarg = ComputeSingleVariableScore(predictorMap, targetMap, NullDistn, fisherCounts, out isInvariant);
            Score altScore = ComputeConditionalVariableScore(predictorMap, targetMap, nullScoreTarg, fisherCounts); 
 
            //(realFisherCounts, nullScoreTarg, out evalResults))
            //{ 
            //    return evalResults;
            //}

            //MessageInitializerDiscrete altMessageInitializer = MessageInitializerDiscrete.GetInstance(predictorMap, targetMap, (DistributionDiscreteConditional)AltDistn, nullScore.OptimizationParameters, ModelScorer.PhyloTree.LeafCollection);
            //Score condScore = ModelScorer.MaximizeLikelihood(altMessageInitializer);
 
            List<Score> nullScores = new List<Score>(); 
            if (_includePredictorInScore)
            { 
                int[] predFisherCounts = new int[] { realFisherCounts[0], realFisherCounts[2], realFisherCounts[1], realFisherCounts[3] };
                Score predNullScore = ComputeSingleVariableScore(targetMap, predictorMap, NullDistn, predFisherCounts, out isInvariant);
                nullScores.Add(predNullScore);
                // conditional model altScore doesn't include predLL. If we're here, we want to add it to make it comparable to joint or reverseConditional
                altScore = Score.GetInstance(altScore.Loglikelihood + predNullScore.Loglikelihood, altScore.OptimizationParameters, altScore.Distribution);
            } 
            nullScores.Add(nullScoreTarg); 

            evalResults = EvaluationResultsDiscrete.GetInstance(this, nullScores, altScore, realFisherCounts, ChiSquareDegreesOfFreedom); 



#if DEBUG
            MessageInitializerDiscrete nullMessageInitializer = MessageInitializerDiscrete.GetInstance(predictorMap, targetMap, NullDistn, fisherCounts, ModelScorer.PhyloTree.LeafCollection);
            MessageInitializerDiscrete altMessageInitializer = MessageInitializerDiscrete.GetInstance(predictorMap, targetMap, (DistributionDiscreteConditional)AltDistn, nullScoreTarg.OptimizationParameters, ModelScorer.PhyloTree.LeafCollection); 
            double nullLL = ModelScorer.ComputeLogLikelihoodModelGivenData(nullMessageInitializer, nullScoreTarg.OptimizationParameters); 
            double altLL = ModelScorer.ComputeLogLikelihoodModelGivenData(altMessageInitializer, altScore.OptimizationParameters);
 
            if (_includePredictorInScore)
            {
                int[] predFisherCounts = new int[] { realFisherCounts[0], realFisherCounts[2], realFisherCounts[1], realFisherCounts[3] };
                MessageInitializerDiscrete nullMessageInitializerPred = MessageInitializerDiscrete.GetInstance(targetMap, predictorMap, NullDistn, predFisherCounts, ModelScorer.PhyloTree.LeafCollection);
                double nullLLPred = ModelScorer.ComputeLogLikelihoodModelGivenData(nullMessageInitializerPred, nullScores[0].OptimizationParameters);
                altLL += nullLLPred; 
            } 

            EvaluationResults evalResults2 = EvaluateModelOnDataGivenParams(predictorMap, targetMap, evalResults); 

            double eps = 1E-10;
            Debug.Assert(ComplexNumber.ApproxEqual(nullLL, nullScoreTarg.Loglikelihood, eps));
            Debug.Assert(ComplexNumber.ApproxEqual(altLL, altScore.Loglikelihood, eps));
            Debug.Assert(ComplexNumber.ApproxEqual(evalResults.NullLL, evalResults2.NullLL, eps) && ComplexNumber.ApproxEqual(evalResults.AltLL, evalResults2.AltLL, eps), "In ModelEvaluatorCond, results of maximizing LL and computing LL from same params are not the same.");
#endif 
 
            return evalResults;
        } 



        //public static Converter<Leaf, SufficientStatistics> TESTPRED, TESTTARG;
        //public static EvaluationResults TESTEVALRESULTS;
 
        public override EvaluationResults EvaluateModelOnDataGivenParams( 
            Converter<Leaf, SufficientStatistics> predictorMap, Converter<Leaf, SufficientStatistics> targetMap, EvaluationResults previousResults)
        { 
            int[] fisherCounts = ModelScorer.PhyloTree.FisherCounts(predictorMap, targetMap);

            int targNullIdx = _includePredictorInScore ? 1 : 0;

            OptimizationParameterList nullParamsTarg = previousResults.NullScores[targNullIdx].OptimizationParameters;
            MessageInitializerDiscrete nullMessageInitializerTarg = MessageInitializerDiscrete.GetInstance(predictorMap, targetMap, 
                NullDistn, new int[0], ModelScorer.PhyloTree.LeafCollection); 
            double nullLLTarg = ModelScorer.ComputeLogLikelihoodModelGivenData(nullMessageInitializerTarg, nullParamsTarg);
            Score nullScoreTarg = Score.GetInstance(nullLLTarg, nullParamsTarg, NullDistn); 


            OptimizationParameterList altParams = previousResults.AltScore.OptimizationParameters;
            MessageInitializerDiscrete altMessageInitializer = MessageInitializerDiscrete.GetInstance(predictorMap, targetMap,
                (DistributionDiscreteConditional)AltDistn, new int[0], ModelScorer.PhyloTree.LeafCollection);
            double condLL = ModelScorer.ComputeLogLikelihoodModelGivenData(altMessageInitializer, altParams); 
            Score altScore = Score.GetInstance(condLL, altParams, AltDistn); 

            List<Score> nullScores = new List<Score>(); 
            if (_includePredictorInScore)
            {
                OptimizationParameterList nullParamsPred = previousResults.NullScores[0].OptimizationParameters;
                MessageInitializerDiscrete nullMessageInitializerPred = MessageInitializerDiscrete.GetInstance(targetMap, predictorMap,
                    NullDistn, new int[0], ModelScorer.PhyloTree.LeafCollection);
                double nullLLPred = ModelScorer.ComputeLogLikelihoodModelGivenData(nullMessageInitializerPred, nullParamsPred); 
                Score nullScorePred = Score.GetInstance(nullLLPred, nullParamsPred, NullDistn); 
                nullScores.Add(nullScorePred);
                // conditional model altScore doesn't include predLL. If we're here, we want to add it to make it comparable to joint or reverseConditional 
                altScore = Score.GetInstance(altScore.Loglikelihood + nullScorePred.Loglikelihood, altScore.OptimizationParameters, altScore.Distribution);
            }
            nullScores.Add(nullScoreTarg);


            EvaluationResults evalResults = EvaluationResultsDiscrete.GetInstance(this, nullScores, altScore, fisherCounts, ChiSquareDegreesOfFreedom); 
            return evalResults; 
        }
 

        private Score ComputeConditionalVariableScore(
            Converter<Leaf, SufficientStatistics> predictorMap,
            Converter<Leaf, SufficientStatistics> targetMap,
            Score nullScore,
            int[] fisherCounts) 
        { 
            int tt = fisherCounts[(int)TwoByTwo.ParameterIndex.TT];
            int tf = fisherCounts[(int)TwoByTwo.ParameterIndex.TF]; 
            int ft = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
            int sum = SpecialFunctions.Sum(fisherCounts);

            Score altScore;
            if (tt + ft == sum || tt + ft == 0) // target is always true or false
            { 
                bool isNaN = sum == 0; 
                OptimizationParameterList altParamList = AltDistn.GetParameters();
                altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor1].Value = 0; 
                altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor2].Value = 0;
                altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Lambda].Value = isNaN ? double.NaN : 0;
                altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Equilibrium].Value = (double)(tt + ft) / sum;
                altScore = Score.GetInstance(isNaN ? double.NaN : 0, altParamList, AltDistn);
            }
            else if (tt + tf == 0 || tt + tf == sum) // predictor is always true or false 
            { 
                OptimizationParameterList nullParamList = nullScore.OptimizationParameters;
                OptimizationParameterList altParamList = AltDistn.GetParameters(); 
                altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor1].Value = nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor1].Value;
                altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor2].Value = nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor2].Value;
                altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Lambda].Value = nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Lambda].Value;
                altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Equilibrium].Value = nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Equilibrium].Value;

                altScore = Score.GetInstance(nullScore.Loglikelihood, altParamList, AltDistn); 
            } 
            else // compute ML using ModelScorer
            { 
                MessageInitializerDiscrete altMessageInitializer = MessageInitializerDiscrete.GetInstance(predictorMap, targetMap, (DistributionDiscreteConditional)AltDistn, nullScore.OptimizationParameters, ModelScorer.PhyloTree.LeafCollection);
                altScore = ModelScorer.MaximizeLikelihood(altMessageInitializer);

            }
            return altScore;
        } 
 
        protected override EvaluationResults CreateDummyResults(int[] fisherCounts)
        { 
            EvaluationResults result = base.CreateDummyResults(fisherCounts);
            if (_includePredictorInScore)
            {
                result.NullScores.Add(Score.GetInstance(result.NullScores[0].Loglikelihood, result.NullScores[0].OptimizationParameters, result.NullScores[0].Distribution));
            }
            return result; 
        } 
        //private bool TryShortCutFromCounts(int[] fisherCounts, MessageInitializerDiscrete nullMessageInitializer, out EvaluationResults evalResults)
        //private bool TryShortCutFromCounts(int[] fisherCounts, Score nullScore, out EvaluationResults evalResults) 
        //{
        //    int tt = fisherCounts[(int)TwoByTwo.ParameterIndex.TT];
        //    int tf = fisherCounts[(int)TwoByTwo.ParameterIndex.TF];
        //    int ft = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
        //    int ff = fisherCounts[(int)TwoByTwo.ParameterIndex.FF];
 
        //    int sum = SpecialFunctions.Sum(fisherCounts); 

        //    double pTarg = (double)(tt + ft) / sum; 
        //    Score nullScore;


        //    if (TryGetSingleVariableScoreFromCounts(nullMessageInitializer, pTarg, out nullScore)) // target is always true or false
        //    {
        //        OptimizationParameterList altParamList = AltDistn.GetParameters(); 
        //        altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor1].Value = 0; 
        //        altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor2].Value = 0;
        //        altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Lambda].Value = 0; 
        //        altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Equilibrium].Value = pTarg;
        //        Score altScore = Score.GetInstance(0, altParamList, AltDistn);

        //        evalResults = EvaluationResultsDiscrete.GetInstance(this, SpecialFunctions.CreateSingletonList(nullScore), altScore, fisherCounts, ChiSquareDegreesOfFreedom);
        //        return true;
        //    } 
        //    else if (tt + tf == 0 || tt + tf == sum) // predictor is always true 
        //    {
        //        nullScore = ModelScorer.MaximizeLikelihood(nullMessageInitializer); 

        //        OptimizationParameterList nullParamList = nullScore.OptimizationParameters;
        //        OptimizationParameterList altParamList = AltDistn.GetParameters();
        //        altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor1].Value = nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor1].Value;
        //        altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor2].Value = nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Predictor2].Value;
        //        altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Lambda].Value = nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Lambda].Value; 
        //        altParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Equilibrium].Value = nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Equilibrium].Value; 

        //        Score altScore = Score.GetInstance(nullScore.Loglikelihood, altParamList, AltDistn); 
        //        evalResults = EvaluationResultsDiscrete.GetInstance(this, SpecialFunctions.CreateSingletonList(nullScore), altScore, fisherCounts, ChiSquareDegreesOfFreedom);
        //        return true;
        //    }
        //    else
        //    {
        //        evalResults = null; 
        //        return false; 
        //    }
        //} 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
