using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using Optimization;

 
namespace VirusCount.PhyloTree 
{
    public class ModelEvaluatorDiscreteJoint : ModelEvaluatorDiscrete 
    {
        new public const string BaseName = "Joint";

        private ModelEvaluatorDiscreteJoint(List<IDistributionSingleVariable> nullDistns, DistributionDiscreteJoint jointDistn, ModelScorer scorer)
            : base(nullDistns, jointDistn, scorer)
        { } 
 
        new public static ModelEvaluatorDiscreteJoint GetInstance(string leafDistributionName, ModelScorer modelScorer)
        { 
            DistributionDiscreteSingleVariable nullDistn = DistributionDiscreteSingleVariable.GetInstance();
            DistributionDiscreteJoint jointDistn = DistributionDiscreteJoint.GetInstance(leafDistributionName);
            List<IDistributionSingleVariable> nullDistns = new List<IDistributionSingleVariable>();
            nullDistns.Add(nullDistn);
            nullDistns.Add(nullDistn);
 
            return new ModelEvaluatorDiscreteJoint(nullDistns, jointDistn, modelScorer); 
        }
 
        public override string Name
        {
            get { return ModelEvaluatorDiscrete.BaseName + BaseName + AltDistn.ToString(); }
        }

 
        public override EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> predictorMap, Converter<Leaf, SufficientStatistics> targetMap) 
        {
            int[] realFisherCounts = ModelScorer.PhyloTree.FisherCounts(predictorMap, targetMap); 

            int tt = realFisherCounts[(int)TwoByTwo.ParameterIndex.TT];
            int tf = realFisherCounts[(int)TwoByTwo.ParameterIndex.TF];
            int ft = realFisherCounts[(int)TwoByTwo.ParameterIndex.FT];
            int ff = realFisherCounts[(int)TwoByTwo.ParameterIndex.FF];
 
            int[] fisherCountsPred = new int[] { tt, ft, tf, ff };  //ModelScorer.PhyloTree.FisherCounts(targetMap, predictorMap); 
            int[] fisherCountsTarg = realFisherCounts;
 
#if NAIVE_EQUILIBRIUM
            //USE THIS FOR BACKWARDS COMPATABILITY
            int[] tempCountsPred = ModelScorer.PhyloTree.CountsOfLeaves(predictorMap);
            int[] tempCountsTarg = ModelScorer.PhyloTree.CountsOfLeaves(targetMap);
            fisherCountsPred = tempCountsPred;
            fisherCountsTarg = tempCountsTarg; 
#endif 
            bool predIsInvariant, targIsInvariant;
 
            Score nullScorePred = ComputeSingleVariableScore(targetMap, predictorMap, (DistributionDiscreteSingleVariable)NullDistns[0], fisherCountsPred, out predIsInvariant);
            Score nullScoreTarg = ComputeSingleVariableScore(predictorMap, targetMap, (DistributionDiscreteSingleVariable)NullDistns[1], fisherCountsTarg, out targIsInvariant);

            List<Score> nullScores = new List<Score>(new Score[]{nullScorePred, nullScoreTarg});
            OptimizationParameterList initParams = ((DistributionDiscreteJoint)AltDistn).GenerateInitialParams(nullScorePred.OptimizationParameters, nullScoreTarg.OptimizationParameters);
            Score jointScore; 
 
            if (predIsInvariant || targIsInvariant)  // cannot compute parameters in this case. They come directly from the single variable params
            { 
                double jointLL = nullScorePred.Loglikelihood + nullScoreTarg.Loglikelihood;
                jointScore = Score.GetInstance(jointLL, initParams, AltDistn);
            }
            else
            {
                MessageInitializerDiscrete altMessageInitializer = MessageInitializerDiscrete.GetInstance(CreateJointMap(predictorMap, targetMap), (DistributionDiscreteJoint)AltDistn, initParams, ModelScorer.PhyloTree.LeafCollection); 
                jointScore = ModelScorer.MaximizeLikelihood(altMessageInitializer); 
            }
 
            EvaluationResults evalResults = EvaluationResultsDiscrete.GetInstance(this, nullScores, jointScore, realFisherCounts, ChiSquareDegreesOfFreedom);

            return evalResults;
        }

 
 
        public override EvaluationResults EvaluateModelOnDataGivenParams(Converter<Leaf, SufficientStatistics> predictorMap, Converter<Leaf, SufficientStatistics> targetMap, EvaluationResults previousResults)
        { 
            int[] fisherCounts = ModelScorer.PhyloTree.FisherCounts(predictorMap, targetMap);

            OptimizationParameterList nullParamsTarg = previousResults.NullScores[1].OptimizationParameters;
            MessageInitializerDiscrete nullMessageInitializerTarg = MessageInitializerDiscrete.GetInstance(predictorMap, targetMap, (DistributionDiscreteSingleVariable)NullDistns[0], fisherCounts, ModelScorer.PhyloTree.LeafCollection);
            double nullLLTarg = ModelScorer.ComputeLogLikelihoodModelGivenData(nullMessageInitializerTarg, nullParamsTarg);
            Score nullScoreTarg = Score.GetInstance(nullLLTarg, nullParamsTarg, previousResults.NullScores[1].Distribution); 
 
            OptimizationParameterList nullParamsPred = previousResults.NullScores[0].OptimizationParameters;
            MessageInitializerDiscrete nullMessageInitializerPred = MessageInitializerDiscrete.GetInstance(targetMap, predictorMap, (DistributionDiscreteSingleVariable)NullDistns[1], fisherCounts, ModelScorer.PhyloTree.LeafCollection); 
            double nullLLPred = ModelScorer.ComputeLogLikelihoodModelGivenData(nullMessageInitializerPred, nullParamsPred);
            Score nullScorePred = Score.GetInstance(nullLLPred, nullParamsPred, previousResults.NullScores[0].Distribution);

            List<Score> nullScores = new List<Score>(new Score[] { nullScorePred, nullScoreTarg });

            OptimizationParameterList altParams = previousResults.AltScore.OptimizationParameters; 
 
            double altLL;
            if (((DistributionDiscreteJoint)AltDistn).ParametersCannotBeEvaluated(altParams)) 
            {
                // we'll get here only if one of the variables is always (or never) true. In this case, the variables must be independent.
                altLL = nullLLTarg + nullLLPred;
            }
            else
            { 
                MessageInitializerDiscrete altMessageInitializer = MessageInitializerDiscrete.GetInstance(CreateJointMap(predictorMap, targetMap), (DistributionDiscreteJoint)AltDistn, fisherCounts, ModelScorer.PhyloTree.LeafCollection); 
                altLL = ModelScorer.ComputeLogLikelihoodModelGivenData(altMessageInitializer, altParams);
            } 

            Score altScore = Score.GetInstance(altLL, altParams, previousResults.AltScore.Distribution);

            EvaluationResults evalResults = EvaluationResultsDiscrete.GetInstance(this, nullScores, altScore, fisherCounts, ChiSquareDegreesOfFreedom);
            return evalResults;
        } 
 

        public static Converter<Leaf, SufficientStatistics> CreateJointMap(Converter<Leaf, SufficientStatistics> predictorMap, Converter<Leaf, SufficientStatistics> targetMap) 
        {
            return delegate(Leaf leaf)
            {
                DistributionDiscreteJoint.DistributionClass jointClass;

                SufficientStatistics predStats = predictorMap(leaf); 
                SufficientStatistics targStats = targetMap(leaf); 

                if (predStats.IsMissing() || targStats.IsMissing()) 
                {
                    jointClass = DistributionDiscreteJoint.DistributionClass.Missing;
                }
                else
                {
                    DiscreteStatistics predClass = (DiscreteStatistics)predStats; 
                    DiscreteStatistics targetClass = (DiscreteStatistics)targStats; 

                    if (predClass == (int)DistributionDiscreteConditional.DistributionClass.False) 
                    {
                        if (targetClass == (int)DistributionDiscreteConditional.DistributionClass.False)
                            jointClass = DistributionDiscreteJoint.DistributionClass.FalseFalse;
                        else
                            jointClass = DistributionDiscreteJoint.DistributionClass.FalseTrue;
                    } 
                    else 
                    {
                        if (targetClass == (int)DistributionDiscreteConditional.DistributionClass.False) 
                            jointClass = DistributionDiscreteJoint.DistributionClass.TrueFalse;
                        else
                            jointClass = DistributionDiscreteJoint.DistributionClass.TrueTrue;
                    }
                }
                return (DiscreteStatistics)(int)jointClass; 
            }; 
        }
    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
