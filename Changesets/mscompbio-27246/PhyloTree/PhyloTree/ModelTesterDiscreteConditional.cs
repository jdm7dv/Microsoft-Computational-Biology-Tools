using System; 
using System.Collections.Generic;
using System.Text;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
 
namespace VirusCount.PhyloTree 
{
    public class ModelTesterDiscreteConditional : ModelTesterDiscrete 
    {
        new public const string Name = "Conditional";

        protected ModelTesterDiscreteConditional(DistributionDiscrete nullModel)
            :
            base(nullModel, nullModel) { } 
 
        public static ModelTesterDiscreteConditional GetInstance(DistributionDiscrete distribution)
        { 
            return new ModelTesterDiscreteConditional(distribution);
        }

        protected override string NullModelParametersAndLikelihoodHeaderString
        {
            get 
            { 
                return SpecialFunctions.CreateTabString(
                    NullModelDistribution.GetParameterHeaderString("0"), 
                    "logLikelihood0");
            }
        }

        protected override string AlternativeModelParametersAndLikelihoodHeaderString
        { 
            get 
            {
                return SpecialFunctions.CreateTabString( 
                    AlternativeModelDistribution.GetParameterHeaderString("1"),
                    "logLikelihood1");
            }
        }

        protected override double ComputeLLR(ModelScorer modelScorer, PhyloTree phyloTree, StringBuilder stringBuilder, double targetMarginal, double predictorMarginal, 
            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction, Converter<Leaf, SufficientStatistics> targetDistributionClassFunction) 
        {
            NullModelDistribution.EmpiricalEquilibrium = targetMarginal; 
            NullModelDistribution.InitialParamVals = null;

            MessageInitializer messageInitializer = modelScorer.CreateMessageInitializer(predictorDistributionClassFunction, targetDistributionClassFunction, NullModelDistribution);

            List<double> logLikelihoodList = new List<double>();
            foreach (bool useParameter in new bool[] { false, true }) 
            { 
                Score score = modelScorer.ScoreModel(messageInitializer, useParameter);
 
                stringBuilder.Append(SpecialFunctions.CreateTabString(score.ToString(useParameter ? AlternativeModelDistribution : NullModelDistribution), ""));
                logLikelihoodList.Add(score.Loglikelihood);
                AltModelDistribution.InitialParamVals = score.OptimizationParameters;
                Debug.WriteLine(SpecialFunctions.CreateTabString("AltModelDistribution.InitialParamVals = score.OptimizationParameters", score.OptimizationParameters));
            }
 
            double diff = logLikelihoodList[1] - logLikelihoodList[0]; 
            return diff;
        } 

        public override string ToString()
        {
            return Name + NullModelDistribution.ToString();
        }
 
        public override Converter<Leaf, SufficientStatistics> CreateAlternativeSufficientStatisticsMap( 
            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction,
            Converter<Leaf, SufficientStatistics> targetDistributionClassFunction) 
        {
            return delegate(Leaf leaf)
                {
                    DiscreteStatistics predClass = (DiscreteStatistics)predictorDistributionClassFunction(leaf);
                    DiscreteStatistics targetClass = (DiscreteStatistics)targetDistributionClassFunction(leaf);
 
 
                    // bail on missing data.
                    if (predClass.IsMissing() || targetClass.IsMissing()) 
                    {
                        return (DiscreteStatistics)(int)DistributionDiscreteBinary.DistributionClass.Missing;
                    }
                    else
                    {
                        return targetClass; 
                    } 
                };
        } 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
