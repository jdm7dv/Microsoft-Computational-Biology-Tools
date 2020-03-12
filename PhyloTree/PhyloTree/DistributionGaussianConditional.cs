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
    /// <summary>
    /// Not yet implemented
    /// </summary>
    public abstract class DistributionGaussianConditional : IDistribution
    {
        // !! may want to create another class level to distinguish conditional from otherwise.
        // but joint isn't possible, so for now, we'll leave it here
        //private LabelledLeafDistributionGaussian LabelledLeafDistributionGaussian; 
        protected readonly bool UseConditionalParameter; 

        public abstract string GetName(); 

        protected DistributionGaussianConditional(bool useConditionalParameter)
        {
            UseConditionalParameter = useConditionalParameter;
        }
 
        internal static DistributionGaussianConditional GetInstance(string distnAndLeafDistnName) 
        {
            distnAndLeafDistnName = distnAndLeafDistnName.ToLower(); 
            Console.WriteLine("This is the new version.");
            DistributionGaussianConditional aDistribution;
            if (distnAndLeafDistnName.StartsWith(DistributionGaussianConditionalReversible.Name.ToLower()))
            {
                aDistribution = DistributionGaussianConditionalReversible.GetInstance();
            } 
            else if (distnAndLeafDistnName.StartsWith(DistributionGaussianConditionalBrownianMotion.Name.ToLower())) 
            {
                aDistribution = DistributionGaussianConditionalBrownianMotion.GetInstance(); 
            }
            else if (distnAndLeafDistnName.StartsWith(DistributionGaussianConditionalIid.Name.ToLower()))
            {
                aDistribution = DistributionGaussianConditionalIid.GetInstance();
            }
            else 
            { 
                throw new ArgumentException("Cannot parse distribution name \"" + distnAndLeafDistnName + "\"");
            } 

            return aDistribution;
        }

        internal static IDistributionGaussianSingleVariable GetSingleVariableInstance(string distnAndLeafDistnName)
        { 
            distnAndLeafDistnName = distnAndLeafDistnName.ToLower(); 

            IDistributionGaussianSingleVariable aDistribution; 
            if (distnAndLeafDistnName.StartsWith(DistributionGaussianConditionalReversible.Name.ToLower()))
            {
                aDistribution = DistributionGaussianSingleVariableReversible.GetInstance();
            }
            else if (distnAndLeafDistnName.StartsWith(DistributionGaussianConditionalBrownianMotion.Name.ToLower()))
            { 
                aDistribution = DistributionGaussianSingleVariableBrownianMotion.GetInstance(); 
            }
            else if (distnAndLeafDistnName.StartsWith(DistributionGaussianConditionalIid.Name.ToLower())) 
            {
                aDistribution = DistributionGaussianSingleVariableConditionalIid.GetInstance();
            }
            else
            {
                throw new ArgumentException("Cannot parse distribution name \"" + distnAndLeafDistnName + "\""); 
            } 

            return aDistribution; 
        }

        public OptimizationParameterList InitialParamVals
        {
            get { return null; }
            set {  } // ignore for now. 
        } 

        public bool DependsOnMoreThanOneVariable 
        {
            get { return UseConditionalParameter; }
        }

        public int FreeParameterCount
        { 
            get { return 2 + (UseConditionalParameter ? 1 : 0); } 
        }
 
        public ConditionalGaussianDistributionParams CreateDistributionGaussianOrNull(
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

        public ConditionalGaussianDistributionParams CreateDistributionGaussianOrNull(Branch branch, OptimizationParameterList gaussianParameters)
        { 
            if (branch.Length == 0) 
            {
                Debug.WriteLine("Branch length of zero observed"); 
            }

            ConditionalGaussianDistributionParams plainConditionalDistribution = GetPlainConditionalGaussianDistribution(branch, gaussianParameters);
            return plainConditionalDistribution;
        }
 
        public ConditionalGaussianDistributionParams CreateDistributionGaussianOrNull( 
            Leaf leaf,
            OptimizationParameterList gaussianParameters, 
            Converter<Leaf, SufficientStatistics> predictorLeafToBoolStats)
        {
            if (leaf.Length == 0)
            {
                Debug.WriteLine("Branch length of zero observed");
            } 
 
            // TODO: make continuous
            BooleanStatistics hasPredictor = (BooleanStatistics)predictorLeafToBoolStats(leaf); 
            if (hasPredictor.IsMissing())
            {
                ConditionalGaussianDistributionParams.GetNullInstance();
                //return null; // Predictor data is missing, so skip this leaf.
            }
 
            ConditionalGaussianDistributionParams plainConditionalDistribution = GetPlainConditionalGaussianDistribution(leaf, gaussianParameters); 

            if (!hasPredictor) 
            {
                return plainConditionalDistribution;
            }
            else
            {
                double delta = GetOffset(gaussianParameters); 
 
                ConditionalGaussianDistributionParams offsetConditionalDistribution = plainConditionalDistribution.AddOffsetToMean(delta);
                return offsetConditionalDistribution; 
            }
            //return CreateDistributionGaussianOrNull(plainConditionalDistribution, hasPredictor, delta);

        }

        //public ConditionalGaussianDistributionParams CreateDistributionGaussianOrNull( 
        //    ConditionalGaussianDistributionParams plainConditionalDistribution, 
        //    bool hasPredictor, double delta)
        //{ 
        //    if ((!hasPredictor) || delta == 0)
        //    {
        //        return plainConditionalDistribution;
        //    }
        //    else
        //    { 
        //        ConditionalGaussianDistributionParams aConditionalDistribution = ConditionalGaussianDistributionParams.GetInstance(); 
        //        aConditionalDistribution.Mean = plainConditionalDistribution.Mean + delta;
        //        aConditionalDistribution.LinearCoefficent = plainConditionalDistribution.LinearCoefficent; 
        //        aConditionalDistribution.Variance = plainConditionalDistribution.Variance;
        //        return aConditionalDistribution;
        //    }

        //}
 
 
        protected abstract ConditionalGaussianDistributionParams GetPlainConditionalGaussianDistribution(
            BranchOrLeaf branchOrLeaf, OptimizationParameterList gaussianParameters); 


        public virtual OptimizationParameterList GetParameters()
        {
            return GetParameters(UseConditionalParameter, false);
        } 
 
        public virtual OptimizationParameterList GetParameters(bool zeroVariance)
        { 
            return GetParameters(UseConditionalParameter, zeroVariance);
        }

        protected virtual OptimizationParameterList GetParameters(bool useConditionalParameter, bool zeroVariance)
        {
            // ignore the initial parameter settings. 
 
            OptimizationParameterList paramStart = OptimizationParameterList.GetInstance(
                OptimizationParameter.GetPositiveFactorInstance("Alpha", 1, true), 
                //OptimizationParameter.GetProbabilityInstance("Alpha", .5, true),
                OptimizationParameter.GetPositiveFactorInstance("Variance", 10000, true),
                //OptimizationParameter.GetPositiveFactorInstance("AlphaVariance", 10000, true),
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
            paramStart["Mean"].Value = 24.71176471; 
            paramStart["Variance"].Value = 35.224; 
            //paramStart["vNoise"].Value = 35.4194535;
 
            return paramStart;
        }

        public OptimizationParameterList GetParameters(double[] parametersInPrintOrder)
        {
            OptimizationParameterList parameters = GetParameters(false, false); 
            parameters["Alpha"].Value = parametersInPrintOrder[0]; 
            parameters["Variance"].Value = parametersInPrintOrder[1];
            //parameters["AlphaVariance"].Value = parametersInPrintOrder[1]; 
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
            double variance = gaussianParameters["Variance"].Value;
            return variance;
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
            return GetName(); 
        }

        public virtual string GetParameterHeaderString(string modifier)
        {
            OptimizationParameterList parameters = GetParameters();
            StringBuilder headerString = new StringBuilder(); 
            foreach (OptimizationParameter param in parameters) 
            {
                if (param.DoSearch) 
                {
                    if (headerString.Length > 0)
                    {
                        headerString.Append("\t");
                    }
                    headerString.Append(param.Name + modifier); 
                } 
            }
            return headerString.ToString(); 
        }

        public virtual string GetParameterValueString(OptimizationParameterList parameters)
        {
            OptimizationParameterList exampleParams = GetParameters();
            StringBuilder valueString = new StringBuilder(); 
            foreach (OptimizationParameter param in exampleParams) 
            {
                if (param.DoSearch) 
                {
                    if (valueString.Length > 0)
                    {
                        valueString.Append("\t");
                    }
                    valueString.Append(parameters[param.Name].Value); 
                } 
            }
            return valueString.ToString(); 
        }

    }

    public class DistributionGaussianConditionalReversible : DistributionGaussianConditional
    { 
        public const string Name = "Reversible"; 

        public override string GetName() 
        {
            return Name;
        }

        protected DistributionGaussianConditionalReversible(bool useParameter) : base(useParameter) { }
        private readonly static DistributionGaussianConditionalReversible ConditionalInstance = new DistributionGaussianConditionalReversible(true); 
 
        public static DistributionGaussianConditionalReversible GetInstance()
        { 
            return ConditionalInstance;
        }

        protected override ConditionalGaussianDistributionParams GetPlainConditionalGaussianDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList gaussianParameters)
        {
            double variance = gaussianParameters["Variance"].Value; 
            //double alphaTimesVariance = gaussianParameters["AlphaVariance"].Value; 
            double mean = gaussianParameters["Mean"].Value;
            double alpha = gaussianParameters["Alpha"].Value; 
            double fOfBranchLength = FOfBranchLength(branchOrLeaf.Length, alpha);

            //ConditionalGaussianDistributionParams plainConditionalDistribution = ConditionalGaussianDistributionParams.GetInstance();
            //double root1MinusAlphaTimesBranchLength = Math.Sqrt(1 - alpha * branchOrLeaf.Length);
            double root1MinusFofBranchLength = Math.Sqrt(1 - FOfBranchLength(branchOrLeaf.Length, alpha));
 
            // ax + b, Mean := b 
            double meanForGaussDistParams = mean * (1.0 - root1MinusFofBranchLength);
            //double meanForGausDistParams = mean * (1.0 - root1MinusAlphaTimesBranchLength); 
            // ax + b = x, LinearCoefficent := a
            double linearCoeffForGaussDistParams = root1MinusFofBranchLength;
            //double linearCoeffForGausDistParams = root1MinusAlphaTimesBranchLength;
            //double varForGausDistParams = alphaTimesVariance * branchOrLeaf.Length;
            double varianceForGaussDistParams = fOfBranchLength * variance;
 
            ConditionalGaussianDistributionParams plainConditionalDistribution = 
                ConditionalGaussianDistributionParams.GetInstance(meanForGaussDistParams, varianceForGaussDistParams, linearCoeffForGaussDistParams);
 
            return plainConditionalDistribution;
        }

        private double FOfBranchLength(double branchLength, double alpha)
        {
            double y = double.IsInfinity(alpha) ? 1 : (alpha * branchLength) / (alpha * branchLength + 1); 
            return y; 
        }
    } 

    public class DistributionGaussianConditionalIid : DistributionGaussianConditionalReversible
    {
        new public const string Name = "Iid";

        public override string GetName() 
        { 
            return Name;
        } 

        protected DistributionGaussianConditionalIid(bool useParameter) : base(useParameter) { }
        private readonly static DistributionGaussianConditionalIid ConditionalInstance = new DistributionGaussianConditionalIid(true);
        new public static DistributionGaussianConditionalReversible GetInstance()
        {
            return ConditionalInstance; 
        } 

        protected override OptimizationParameterList GetParameters(bool useConditionalParameter, bool zeroVariance) 
        {
            OptimizationParameterList parameters = base.GetParameters(useConditionalParameter, zeroVariance);
            OptimizationParameter alpha = parameters["Alpha"];
            alpha.Value = 10000;
            //alpha.Value = double.PositiveInfinity;
            alpha.DoSearch = false; 
            return parameters; 
        }
 
        protected override ConditionalGaussianDistributionParams GetPlainConditionalGaussianDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList gaussianParameters)
        {
            double variance = gaussianParameters["Variance"].Value;

            ConditionalGaussianDistributionParams plainConditionalDistribution =
                ConditionalGaussianDistributionParams.GetInstance(0, variance, 1); 
 
            return plainConditionalDistribution;
        } 
    }

    public class DistributionGaussianConditionalBrownianMotion : DistributionGaussianConditional
    {
        public const string Name = "BrownianMotion";
 
        public override string GetName() 
        {
            return Name; 
        }

        protected DistributionGaussianConditionalBrownianMotion(bool useParameter) : base(useParameter) { }
        private readonly static DistributionGaussianConditionalBrownianMotion ConditionalInstance = new DistributionGaussianConditionalBrownianMotion(true);

        public static DistributionGaussianConditionalBrownianMotion GetInstance() 
        { 
            return ConditionalInstance;
        } 

        protected override ConditionalGaussianDistributionParams GetPlainConditionalGaussianDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList gaussianParameters)
        {
            // NOTE: For BM, interpret Alpha as AlphaTimesVariance. Alpha is only used to multiply against variance, and searching is easier we we do
            // it this way, as otherwise alphaTimesVariance is affected by both alpha and variance when we're searching.
            double alphaTimesVariance = gaussianParameters["Alpha"].Value; 
            //double alpha = gaussianParameters["Alpha"].Value; 
            //double variance = gaussianParameters["Variance"].Value;
 
            // 0 (const part of mean) ax + b, Mean := b
            double mean = 0;
            // 1 (ax + b = x), linearCoefficent := a.
            double linearCoefficient = 1;
            //double conditionalVariance = alpha * variance * branchOrLeaf.Length;
            double conditionalVariance = alphaTimesVariance * branchOrLeaf.Length; 
 
            ConditionalGaussianDistributionParams plainConditionalDistribution =
                ConditionalGaussianDistributionParams.GetInstance(mean, conditionalVariance, linearCoefficient); 

            return plainConditionalDistribution;
        }

        //protected override OptimizationParameterList GetParameters(bool useConditionalParameter, bool zeroVariance)
        //{ 
        //    // alpha is confounded with variance in this case, so we fix it at 1. 
        //    OptimizationParameterList parameters = base.GetParameters(useConditionalParameter, zeroVariance);
        //    OptimizationParameter alphaParam = parameters["Alpha"]; 
        //    alphaParam.ConvertToPositiveFactorInstance();
        //    alphaParam.Value = 1.0;
        //    //alphaParam.Value = 1.0;
        //    //alphaParam.DoSearch = false;

        //    // TEMPORARY!!!! 
        //    //parameters["Mean"].Value = 10.13; 
        //    //parameters["AlphaVariance"].Value = 2.18 * 2.18;
        //    return parameters; 
        //}

    }

    interface IDistributionGaussianSingleVariable : IDistributionSingleVariable { }
    class DistributionGaussianSingleVariableReversible : DistributionGaussianConditionalReversible, IDistributionGaussianSingleVariable 
    { 
        protected DistributionGaussianSingleVariableReversible(bool useParameter) : base(useParameter) { }
        private readonly static DistributionGaussianSingleVariableReversible SingleVariableInstance = new DistributionGaussianSingleVariableReversible(false); 

        new public static DistributionGaussianSingleVariableReversible GetInstance()
        {
            return SingleVariableInstance;
        }
    } 
 
    class DistributionGaussianSingleVariableConditionalIid : DistributionGaussianConditionalIid, IDistributionGaussianSingleVariable
    { 
        protected DistributionGaussianSingleVariableConditionalIid(bool useParameter) : base(useParameter) { }
        private readonly static DistributionGaussianSingleVariableConditionalIid SingleVariableInstance = new DistributionGaussianSingleVariableConditionalIid(false);

        new public static DistributionGaussianSingleVariableConditionalIid GetInstance()
        {
            return SingleVariableInstance; 
        } 
    }
 
    class DistributionGaussianSingleVariableBrownianMotion : DistributionGaussianConditionalBrownianMotion, IDistributionGaussianSingleVariable
    {
        protected DistributionGaussianSingleVariableBrownianMotion(bool useParameter) : base(useParameter) { }
        private readonly static DistributionGaussianSingleVariableBrownianMotion SingleVariableInstance = new DistributionGaussianSingleVariableBrownianMotion(false);

        new public static DistributionGaussianSingleVariableBrownianMotion GetInstance() 
        { 
            return SingleVariableInstance;
        } 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
