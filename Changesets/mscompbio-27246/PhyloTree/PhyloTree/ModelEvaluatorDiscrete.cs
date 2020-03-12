using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree 
{ 
    public abstract class ModelEvaluatorDiscrete : ModelEvaluator
    { 
        public const string BaseName = "Discrete";

        protected ModelEvaluatorDiscrete(List<IDistributionSingleVariable> nullDistns, DistributionDiscrete altDistn, ModelScorer scorer)
            : base(nullDistns, altDistn, scorer)
        { }
 
        new public static ModelEvaluatorDiscrete GetInstance(string nameAndParameters, ModelScorer scorer) 
        {
            nameAndParameters = nameAndParameters.ToLower(); 

            if (nameAndParameters.StartsWith(ModelEvaluatorDiscreteConditional.BaseName.ToLower()))
            {
                return ModelEvaluatorDiscreteConditional.GetInstance(nameAndParameters.Substring(ModelEvaluatorDiscreteConditional.BaseName.Length), scorer);
            }
            else if (nameAndParameters.StartsWith(ModelEvaluatorDiscreteJoint.BaseName.ToLower())) 
            { 
                return ModelEvaluatorDiscreteJoint.GetInstance(nameAndParameters.Substring(ModelEvaluatorDiscreteJoint.BaseName.Length), scorer);
            } 
            else if (nameAndParameters.Equals(ModelEvaluatorDiscreteFisher.BaseName.ToLower()))
            {
                return ModelEvaluatorDiscreteFisher.GetInstance(scorer.PhyloTree.LeafCollection);
            }
            throw new ArgumentException("Cold not parse " + nameAndParameters + " into a ModelEvaluatorDiscrete.");
        } 
 
        protected virtual EvaluationResults CreateDummyResults(int[] fisherCounts)
        { 
            List<Score> nullScores = new List<Score>(NullDistns.Count);

            foreach(DistributionDiscreteSingleVariable nullDistn in NullDistns)
            {
                OptimizationParameterList nullParams = nullDistn.GetParameters();
                foreach (OptimizationParameter param in nullParams) 
                { 
                    param.Value = double.NegativeInfinity;
                } 
                Score nullScore = Score.GetInstance(0, nullParams, nullDistn);
                nullScores.Add(nullScore);
            }

            OptimizationParameterList altParams =  this.AltDistn.GetParameters();
            foreach (OptimizationParameter param in altParams) 
            { 
                param.Value = double.NegativeInfinity;
            } 
            Score altScore = Score.GetInstance(0, altParams, AltDistn);

            return EvaluationResultsDiscrete.GetInstance(this, nullScores, altScore, fisherCounts, ChiSquareDegreesOfFreedom);
        }

        protected bool UninformativeVariable(int[] fisherCounts) 
        { 
            int tt = (int)TwoByTwo.ParameterIndex.TT;
            int tf = (int)TwoByTwo.ParameterIndex.TF; 
            int ft = (int)TwoByTwo.ParameterIndex.FT;
            int ff = (int)TwoByTwo.ParameterIndex.FF;

            return (fisherCounts[tt] + fisherCounts[tf]) == 0 ||
                (fisherCounts[tt] + fisherCounts[ft]) == 0 ||
                (fisherCounts[ft] + fisherCounts[ff]) == 0 || 
                (fisherCounts[tf] + fisherCounts[ff]) == 0; 
        }
 
        protected Score ComputeSingleVariableScore(
            Converter<Leaf, SufficientStatistics> predictorMap,
            Converter<Leaf, SufficientStatistics> targetMap,
            DistributionDiscreteSingleVariable nullDistn,
            int[] fisherCounts,
            out bool variableIsInvariant) 
        { 
            MessageInitializerDiscrete nullMessageInitializer =
                MessageInitializerDiscrete.GetInstance(predictorMap, targetMap, nullDistn, fisherCounts, ModelScorer.PhyloTree.LeafCollection); 

            double p = (double)TwoByTwo.GetRightSum(fisherCounts) / SpecialFunctions.Sum(fisherCounts);
            Score nullScore;
            if (TryGetSingleVariableScoreFromCounts(nullMessageInitializer, p, out nullScore))
            {
                variableIsInvariant = true; 
            } 
            else
            { 
                variableIsInvariant = false;
                nullScore = ModelScorer.MaximizeLikelihood(nullMessageInitializer);
            }
            return nullScore;
        }
 
        private bool TryGetSingleVariableScoreFromCounts(MessageInitializerDiscrete singleVariableMessageInitializer, double pVar, out Score score) 
        {
            if (pVar == 1 || pVar == 0) 
            {

                OptimizationParameterList nullParamList = singleVariableMessageInitializer.DiscreteDistribution.GetParameters();
                nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Lambda].Value = 0;
                nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Equilibrium].Value = pVar;
                score = Score.GetInstance(0, nullParamList, singleVariableMessageInitializer.DiscreteDistribution); 
                return true; 
            }
            else if (double.IsNaN(pVar)) 
            {
                OptimizationParameterList nullParamList = singleVariableMessageInitializer.DiscreteDistribution.GetParameters();
                nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Lambda].Value = double.NaN;
                nullParamList[(int)DistributionDiscreteSingleVariable.ParameterIndex.Equilibrium].Value = double.NaN;
                score = Score.GetInstance(double.NaN, nullParamList, singleVariableMessageInitializer.DiscreteDistribution);
                return true; 
            } 
            else
            { 
                score = null;
                return false;
            }
        }

        //public override NullDataGenerator CreateNullDataGenerator(string nullDataGeneratorName) 
        //{ 
        //    return NullDataGenerator.GetInstance(nullDataGeneratorName, ModelScorer, NullDistns[0]);
        //} 

        public override string ToHeaderString()
        {
            EvaluationResults dummyResults = CreateDummyResults(new int[4]);
            return dummyResults.ToHeaderString();
        } 
    } 

    class EvaluationResultsDiscrete : EvaluationResults 
    {
        private readonly int[] _fisherCounts;

        protected EvaluationResultsDiscrete(ModelEvaluator modelEval, List<Score> nullScores, Score altScore, int[] fisherCounts, int chiSquareDegreesOfFreedom)
            :
            base(modelEval, nullScores, altScore, chiSquareDegreesOfFreedom, SpecialFunctions.Sum(fisherCounts)) 
        { 
            _fisherCounts = fisherCounts;
        } 

        public static EvaluationResultsDiscrete GetInstance(ModelEvaluator modelEval, Score nullScore, Score altScore, int[] fisherCounts, int chiSquareDegreesOfFreedom)
        {
            List<Score> singletonList = SpecialFunctions.CreateSingletonList(nullScore);
            return new EvaluationResultsDiscrete(modelEval, singletonList, altScore, fisherCounts, chiSquareDegreesOfFreedom);
        } 
 
        public static EvaluationResultsDiscrete GetInstance(ModelEvaluator modelEval, List<Score> nullScores, Score altScore, int[] fisherCounts, int chiSquareDegreesOfFreedom)
        { 
            return new EvaluationResultsDiscrete(modelEval, nullScores, altScore, fisherCounts, chiSquareDegreesOfFreedom);
        }

        public override string IidStatsHeaderString()
        {
            return TwoByTwo.CountsHeader + "\t" + base.IidStatsHeaderString(); 
        } 

        public override string IidStatsString() 
        {
            return SpecialFunctions.CreateTabString2(_fisherCounts) + "\t" + base.IidStatsString();
        }
    }
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
