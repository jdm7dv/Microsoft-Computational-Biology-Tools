using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;

namespace VirusCount.PhyloTree
{ 
    //!!!can we use the types (or classes) to prepresent themselves instead of having an enum of them? 
    /// <summary>
    /// Returns the sufficient statistics for the leaf. In the case of Discrete distributions, this is the class the leaf resolves to. 
    /// (When implemented, in Gaussian, it's the mean, variance and sample size.)
    /// </summary>
    public interface IDistribution
    {
        //OptimizationParameterList InitialParamVals 
        //{ 
        //    get;
        //    set; 
        //}
        bool DependsOnMoreThanOneVariable
        {
            get;
        }
 
        int FreeParameterCount 
        {
            get; 
        }

        OptimizationParameterList GetParameters(double[] parameterValuesInPrintOrder);
        OptimizationParameterList GetParameters();
        //bool UsePredictorVariable(bool useParameter);
        string GetParameterHeaderString(string modifier); 
        string GetParameterValueString(OptimizationParameterList parameters); 

    } 

    public interface IDistributionSingleVariable : IDistribution { }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
