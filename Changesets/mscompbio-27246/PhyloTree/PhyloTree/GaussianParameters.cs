using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree 
{ 
    //public class OptimizationParameterList
    //{ 
    //    private OptimizationParameterList()
    //    {
    //    }

    //    public static OptimizationParameterList GetInstance(double alpha, double alphaVariance, double delta, double mean, double vNoise)
    //    { 
    //        OptimizationParameterList aGaussianParameters = new OptimizationParameterList(); 
    //        aGaussianParameters.Alpha = alpha;
    //        aGaussianParameters.AlphaTimesVariance = alphaVariance; 
    //        aGaussianParameters.Delta = delta;
    //        aGaussianParameters.Mean = mean;
    //        aGaussianParameters.VNoise = vNoise;
    //        return aGaussianParameters;
    //    }
 
    //    public double Alpha; 
    //    public double AlphaTimesVariance;
    //    public double Delta;    //only used by leaf. ignored by internal nodes (in conditional case) 
    //    public double Mean;
    //    public double VNoise;   // variance in the observations at the tips

    //    public override string ToString()
    //    {
    //        return SpecialFunctions.CreateTabString(Alpha, AlphaTimesVariance, Delta, Mean, VNoise); 
    //    } 
    //}
 
    //public class DiscreteParameters
    //{
    //    private DiscreteParameters()
    //    {
    //    }
 
    //    public static DiscreteParameters GetInstance(double lambda, double parameter, double x) 
    //    {
    //        DiscreteParameters aArabDiscreteParameters = new DiscreteParameters(); 
    //        aArabDiscreteParameters.Lambda = lambda;
    //        aArabDiscreteParameters.Parameter = parameter;
    //        aArabDiscreteParameters.X = x;
    //        return aArabDiscreteParameters;
    //    }
 
    //    public double X; 
    //    public double Lambda;
 
    //    //!!!give this a better, less generic, name
    //    public double Parameter;

    //    public override string ToString()
    //    {
    //        return SpecialFunctions.CreateTabString(X, Lambda, Math.Log(Lambda), Parameter); 
    //    } 
    //}
 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
