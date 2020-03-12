using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using Optimization;
//using Msr.Mlas.LinearAlgebra; 
using System.IO; 

namespace VirusCount.PhyloTree 
{
    public abstract class DistributionGaussian : IDistribution
    {
        // !! may want to create another class level to distinguish conditional from otherwise.
        // but joint isn't possible, so for now, we'll leave it here
        private LabelledLeafDistributionGaussian LabelledLeafDistributionGaussian; 
 
        public abstract string GetName();
 
        protected DistributionGaussian()
        {
        }

        internal static DistributionGaussian GetInstance(string distnAndLeafDistnName)
        { 
            // !!! parse to BrownianMotion and Reversible. 
            DistributionGaussian aDistribution;
            string leafDistnName; 
            if (distnAndLeafDistnName.StartsWith(DistributionGaussianReversible.Name))
            {
                aDistribution = new DistributionGaussianReversible();
                leafDistnName = distnAndLeafDistnName.Substring(DistributionGaussianReversible.Name.Length);
            }
            else if (distnAndLeafDistnName.StartsWith(DistributionGaussianBrownianMotion.Name)) 
            { 
                aDistribution = new DistributionGaussianBrownianMotion();
                leafDistnName = distnAndLeafDistnName.Substring(DistributionGaussianBrownianMotion.Name.Length); 
            }
            else
            {
                throw new ArgumentException("Cannont parse distribution name \"" + distnAndLeafDistnName + "\"");
            }
 
            aDistribution.LabelledLeafDistributionGaussian = LabelledLeafDistributionGaussian.GetInstance(leafDistnName); 
            return aDistribution;
        } 

        public OptimizationParameterList InitialParamVals
        {
            get { return null; }
            set {  } // ignore for now.
        } 
 
        public virtual bool UsePredictorVariable(bool useParameter)
        { 
            return useParameter;
        }

        public ConditionalGaussianDistribution CreateDistributionGaussianOrNull(
             BranchOrLeaf branchOrLeaf, OptimizationParameterList gaussianParameters, Converter<Leaf, SufficientStatistics> predictorLeafToBoolStats)
        { 
            if (branchOrLeaf is Branch) 
            {
                return CreateDistributionGaussianOrNull((Branch)branchOrLeaf, gaussianParameters); 
            }
            else
            {
                return CreateDistributionGaussianOrNull((Leaf)branchOrLeaf, gaussianParameters, predictorLeafToBoolStats);
            }
        } 
 
        public ConditionalGaussianDistribution CreateDistributionGaussianOrNull(Branch branch, OptimizationParameterList gaussianParameters)
        { 
            if (branch.Length == 0)
            {
                Debug.WriteLine("Branch length of zero observed");
            }

            ConditionalGaussianDistribution plainConditionalDistribution = GetPlainConditionalGaussianDistribution(branch, gaussianParameters); 
            return plainConditionalDistribution; 
        }
 
        public ConditionalGaussianDistribution CreateDistributionGaussianOrNull(
            Leaf leaf,
            OptimizationParameterList gaussianParameters,
            Converter<Leaf, SufficientStatistics> predictorLeafToBoolStats)
        {
            if (leaf.Length == 0) 
            { 
                Debug.WriteLine("Branch length of zero observed");
            } 

            ConditionalGaussianDistribution plainConditionalDistribution = GetPlainConditionalGaussianDistribution(leaf, gaussianParameters);

            BooleanStatistics hasPredictor = (BooleanStatistics)predictorLeafToBoolStats(leaf);
            if (hasPredictor.IsMissing())
            { 
                return null; // Predictor data is missing, so skip this leaf. 
            }
 
            double delta = GetOffset(gaussianParameters);
            return LabelledLeafDistributionGaussian.CreateDistributionGaussianOrNull(plainConditionalDistribution, hasPredictor, delta);

        }

        protected abstract ConditionalGaussianDistribution GetPlainConditionalGaussianDistribution( 
            BranchOrLeaf branchOrLeaf, OptimizationParameterList gaussianParameters); 

 
        public virtual OptimizationParameterList GetParameters(bool useConditionalParameter)
        {
            return GetParameters(useConditionalParameter, false);
        }

        public virtual OptimizationParameterList GetParameters(bool useConditionalParameter, bool zeroVariance) 
        { 
            // ignore the initial parameter settings.
 
            OptimizationParameterList paramStart = OptimizationParameterList.GetInstance(
                OptimizationParameter.GetProbabilityInstance("Alpha", .5, true),
                OptimizationParameter.GetPositiveFactorInstance("AlphaVariance", 10000, true),
                OptimizationParameter.GetTanInstance("Delta", 0, useConditionalParameter, -1000, 1000),
                OptimizationParameter.GetTanInstance("Mean", 0, true, -1000, 1000),
                zeroVariance ? OptimizationParameter.GetPositiveFactorInstance("vNoise", 0, false) 
                             : OptimizationParameter.GetPositiveFactorInstance("vNoise", 75, true) 
                );
 
            //TEMPORARY TESTING!!!!!!!
            //paramStart["Alpha"].Value = 0.165267457;
            //paramStart["AlphaVariance"].Value = 6952.429217;
            //paramStart["Delta"].Value = -23.71002634;
            //paramStart["Delta"].DoSearch = true;
            //paramStart["Mean"].Value = 79.52629868; 
            //paramStart["vNoise"].Value = 35.4194535; 

            return paramStart; 
        }

        public OptimizationParameterList GetParameters(double[] parametersInPrintOrder)
        {
            OptimizationParameterList parameters = GetParameters(false, false);
            parameters["Alpha"].Value = parametersInPrintOrder[0]; 
            parameters["AlphaVariance"].Value = parametersInPrintOrder[1]; 
            parameters["Delta"].Value = parametersInPrintOrder[2];
            parameters["Mean"].Value = parametersInPrintOrder[3]; 
            parameters["vNoise"].Value = parametersInPrintOrder[4];
            return parameters;
        }

        public double GetEquilibriumMean(OptimizationParameterList gaussianParameters)
        { 
            return gaussianParameters["Mean"].Value; 
        }
        public double GetEquilibriumVariance(OptimizationParameterList gaussianParameters) 
        {
            double AlphaVariance = gaussianParameters["AlphaVariance"].Value;
            double alpha = gaussianParameters["Alpha"].Value;
            return AlphaVariance / alpha;
        }
        public double GetSamplingVariance(OptimizationParameterList gaussianParameters) 
        { 
            return gaussianParameters["vNoise"].Value;
        } 
        public double GetOffset(OptimizationParameterList gaussianParameters)
        {
            return gaussianParameters["Delta"].Value;
        }

        public override string ToString() 
        { 
            return GetName() + LabelledLeafDistributionGaussian.ToString();
        } 
    }

    public class DistributionGaussianReversible : DistributionGaussian
    {
        public const string Name = "Reversible";
 
        public override string GetName() 
        {
            return Name; 
        }

        protected override ConditionalGaussianDistribution GetPlainConditionalGaussianDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList gaussianParameters)
        {
            double alphaTimesVariance = gaussianParameters["AlphaVariance"].Value;
            double mean = gaussianParameters["Mean"].Value; 
            double alpha = gaussianParameters["Alpha"].Value; 

            ConditionalGaussianDistribution plainConditionalDistribution = ConditionalGaussianDistribution.GetInstance(); 
            double root1MinusAlphaTimesBranchLength = Math.Sqrt(1 - alpha * branchOrLeaf.Length);
            // ax + b, Mean := b
            plainConditionalDistribution.Mean = mean * (1.0 - root1MinusAlphaTimesBranchLength);
            // ax + b = x, LinearCoefficent := a
            plainConditionalDistribution.LinearCoefficent = root1MinusAlphaTimesBranchLength;
            plainConditionalDistribution.Variance = alphaTimesVariance * branchOrLeaf.Length; 
            return plainConditionalDistribution; 
        }
    } 

    public class DistributionGaussianBrownianMotion : DistributionGaussian
    {
        public const string Name = "BrownianMotion";

        public override string GetName() 
        { 
            return Name;
        } 

        protected override ConditionalGaussianDistribution GetPlainConditionalGaussianDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList gaussianParameters)
        {
            double alphaTimesVariance = gaussianParameters["AlphaVariance"].Value;

            ConditionalGaussianDistribution plainConditionalDistribution = ConditionalGaussianDistribution.GetInstance(); 
            // 0 (const part of mean) ax + b, Mean := b 
            plainConditionalDistribution.Mean = 0;
            // 1 (ax + b = x), linearCoefficent := a. 
            plainConditionalDistribution.LinearCoefficent = 1;
            plainConditionalDistribution.Variance = alphaTimesVariance * branchOrLeaf.Length;
            return plainConditionalDistribution;
        }

        public override OptimizationParameterList GetParameters(bool useConditionalParameter, bool zeroVariance) 
        { 
            // alpha is confounded with variance in this case, so we fix it at 1.
            OptimizationParameterList parameters = base.GetParameters(useConditionalParameter, zeroVariance); 
            OptimizationParameter alphaParam = parameters["Alpha"];
            alphaParam.ConvertToPositiveFactorInstance();
            alphaParam.Value = 1.0;
            //alphaParam.Value = 1.0;
            //alphaParam.DoSearch = false;
 
            // TEMPORARY!!!! 
            //parameters["Mean"].Value = 10.13;
            //parameters["AlphaVariance"].Value = 2.18 * 2.18; 
            return parameters;
        }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
