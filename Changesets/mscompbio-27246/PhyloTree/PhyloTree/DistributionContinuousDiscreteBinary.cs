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

    //public class DistributionContinuousDiscreteBinary : DistributionDiscreteConditional
    //{
    //    protected DistributionContinuousDiscreteBinary(string labelledLeafDistribution) : base(labelledLeafDistribution) { }
 
    //    new internal static DistributionContinuousDiscreteBinary GetInstance(string labelledLeafDistribution) 
    //    {
    //        return new DistributionContinuousDiscreteBinary(labelledLeafDistribution); 
    //    }

    //    public override string ToString()
    //    {
    //        return "Continuous" + TrueLabelledLeafDistributionDiscrete.ToString();
    //    } 
 
    //    public override double[][] CreateDistribution(Leaf leaf, OptimizationParameterList discreteParameters,
    //        Converter<Leaf, SufficientStatistics> predictorClassFunction) 
    //    {
    //        double[][] yDist = DefaultDistribution(leaf, discreteParameters);

    //        SpecialFunctions.CheckCondition(leaf.CaseName != null, "Leaf case name is null. Should we allow this?");

    //        double[][] zDist = new double[2][]; 
 
    //        double predictorValue = discreteParameters[(int)ParameterIndex.Predictor1].Value;
    //        SufficientStatistics stats = predictorClassFunction(leaf); 
    //        SpecialFunctions.CheckCondition(!stats.IsMissing(), "Missing data should have been taken care of by now.");

    //        double value = (ContinuousStatistics)stats;
    //        double weight = discreteParameters[(int)ParameterIndex.Predictor1].Value;
    //        double p = SpecialFunctions.Sigmoid(weight + value);
 
    //        //if (weight > 0) 
    //        //{
    //        //    Console.WriteLine(weight); 
    //        //}
    //        for (int parentValue = 0; parentValue < 2; ++parentValue)
    //        {
    //            zDist[parentValue] =
    //                TrueLabelledLeafDistributionDiscrete.PredictorTrueDistribution(p, yDist[parentValue]);
    //        } 
    //        return zDist; 
    //    }
 
    //    public override OptimizationParameterList GetParameters()
    //    {
    //        OptimizationParameterList optList = base.GetParameters();
    //        OptimizationParameter parameter = optList[(int)ParameterIndex.Predictor1];
    //        parameter.ConvertToTanInstance(-100, 100);
    //        parameter.Value = double.MinValue; 
 
    //        return optList;
    //    } 
    //}
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
