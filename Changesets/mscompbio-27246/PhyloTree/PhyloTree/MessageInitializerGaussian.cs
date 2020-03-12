using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
 
namespace VirusCount.PhyloTree 
{
    public class MessageInitializerGaussian : MessageInitializer 
    {
        private static readonly double Log2PI = Math.Log(2 * Math.PI);
        private readonly bool _allVarianceZero;

        public DistributionGaussianConditional GaussianDistribution
        { 
            get { return (DistributionGaussianConditional)PropogationDistribution; } 
        }
 
        protected MessageInitializerGaussian(
            Converter<Leaf, SufficientStatistics> predictorClassFunction,
            Converter<Leaf, SufficientStatistics> targetClassFunction,
            DistributionGaussianConditional gaussianDistribution,
            IEnumerable<Leaf> fullLeafList)
            : base(SpecialFunctions.CreateSingletonList(predictorClassFunction), targetClassFunction, gaussianDistribution, fullLeafList) 
        { 
            _allVarianceZero = AllVarianceZero(fullLeafList, targetClassFunction);
        } 

        public static MessageInitializerGaussian GetInstance(
            Converter<Leaf, SufficientStatistics> predictorClassFunction,
            Converter<Leaf, SufficientStatistics> targetClassFunction,
            DistributionGaussianConditional gaussianDistribution,
            IEnumerable<Leaf> fullLeafList) 
        { 
            return new MessageInitializerGaussian(predictorClassFunction, targetClassFunction, gaussianDistribution, fullLeafList);
        } 


        public override IMessage InitializeMessage(Leaf leaf, OptimizationParameterList gaussianParameters)
        {
            if (IsMissing(leaf))
            { 
                return null; 
            }
 
            //ConditionalGaussianDistribution distOrNull = GaussianDistribution.CreateDistributionGaussianOrNull(leaf, gaussianParameters, _caseNameToHasPredictor);
            ConditionalGaussianDistributionParams distOrNull = GaussianDistribution.CreateDistributionGaussianOrNull(leaf, gaussianParameters, LeafToPredictorStatistics);
            if (distOrNull.IsNull)
            {
                return null;
            } 
 
            MessageGaussian message = Initalize(distOrNull, leaf, gaussianParameters);
            return message; 
        }

        public override OptimizationParameterList GetOptimizationParameters()
        {
            OptimizationParameterList paramStart = GaussianDistribution.GetParameters(_allVarianceZero);
            return paramStart; 
        } 

 

        private MessageGaussian Initalize(ConditionalGaussianDistributionParams dist, Leaf leaf, OptimizationParameterList gaussianParameters)
        {
            Debug.Assert(!LeafToTargetStatistics(leaf).IsMissing());
            GaussianStatistics gaussianStatistics = (GaussianStatistics)LeafToTargetStatistics(leaf);
            SpecialFunctions.CheckCondition(gaussianStatistics != null, "why is caseNameToTargetOrNull unknown?"); 
 
            if (gaussianStatistics.SampleSize == 1)
            { 
                double z = gaussianStatistics.Mean;
                double logK = -Math.Log(dist.LinearCoefficient);
                Debug.Assert(!double.IsNaN(logK)); // real assert - in release mode, the NaN is OK.
                double a = (z - dist.Mean) / dist.LinearCoefficient;
                double v = dist.Variance / Math.Pow(dist.LinearCoefficient, 2);
 
                MessageGaussian message = MessageGaussian.GetInstance(logK, a, v); 
                return message;
 
            }
            else
            {
                double vNoise = GaussianDistribution.GetSamplingVariance(gaussianParameters);
                double logKMult = (
                            -Math.Log(gaussianStatistics.SampleSize) 
                            - gaussianStatistics.SampleSize 
                                * (gaussianStatistics.Variance + vNoise * Log2PI + vNoise * Math.Log(vNoise))
                                / vNoise 
                            + Math.Log(2 * Math.PI * vNoise)
                    ) / 2.0;

                double aMult = gaussianStatistics.Mean;
                double vMult = vNoise / (double)gaussianStatistics.SampleSize;
 
                double logK = logKMult - Math.Log(dist.LinearCoefficient); 
                double a = (aMult - dist.Mean) / dist.LinearCoefficient;
                double v = (vMult + dist.Variance) / Math.Pow(dist.LinearCoefficient, 2); 

                MessageGaussian message = MessageGaussian.GetInstance(logK, a, v);
                return message;
            }
        }
 
        //private bool AllVarianceZero(Dictionary<string, GaussianStatistics> caseNameToTarget) 
        private static bool AllVarianceZero(IEnumerable<Leaf> LeafCollection, Converter<Leaf, SufficientStatistics> caseNameToTarget)
        { 
            bool varianceIsZero = false; //If empty input, then return false
            bool firstTime = true;
            foreach (Leaf leaf in LeafCollection)
            {
                SufficientStatistics stats = caseNameToTarget(leaf);
                if (stats.IsMissing()) 
                { 
                    continue;
                } 
                GaussianStatistics gaussianStatistics = (GaussianStatistics)stats;
                SpecialFunctions.CheckCondition((gaussianStatistics.Variance == 0) == (gaussianStatistics.SampleSize == 1), "Variance must be zero exactly when the sample size is 1");
                if (firstTime)
                {
                    firstTime = false;
                    varianceIsZero = (gaussianStatistics.Variance == 0); 
                } 
                else
                { 
                    SpecialFunctions.CheckCondition(varianceIsZero == (gaussianStatistics.Variance == 0), "If any variances are zero, then all must be zero");
                }
            }
            return varianceIsZero;
        }
 
    } 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
