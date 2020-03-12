using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree 
{ 
    public enum DiscreteDistributionType
    {
        BinaryPhylogeny, 
        JointBinaryPhylogeny 
    }
 


    /// <summary>
    /// Returns an index in a Distribution. Should return values from your distribution's DistributionClassEnum.
    /// These enum's have -1 for missing data or data that cannot be placed in a distribution class.
    /// </summary> 
    /// <param name="leaf"></param> 
    /// <returns></returns>
    //!!!Should this be called "LeafToDistributionDelegate"? Should the return type be "bool?" ? 

    /// <summary>
    /// Describes a generatic discrete distribution over a phylogenetic tree.
    /// </summary>
    public abstract class DistributionDiscrete : IDistribution
    { 
        //private double _empiricalEquilibrium = 0.5; 
        //private OptimizationParameterList _initialParamVals = null;
 
        public abstract int NonMissingClassCount
        {
            get;
        }

        public abstract bool DependsOnMoreThanOneVariable 
        { 
            get;
        } 

        public abstract int FreeParameterCount
        {
            get;
        }
 
        //public double EmpiricalEquilibrium 
        //{
        //    get { return _empiricalEquilibrium; } 
        //    set { _empiricalEquilibrium = value; }
        //}
        //public OptimizationParameterList InitialParamVals
        //{
        //    get { return _initialParamVals; }
        //    set { _initialParamVals = value; } 
        //} 

        protected DistributionDiscrete() 
        {
            // TODO: Make the following dynamic checks:
            // 1) Check that DistributionClass enum exists
            // 2) Check that Missing value exists and has value < 0
            // 3) Check that the other values are in the range 0..length-1, where length is the length returned by DefaultDistribution
        } 
 
        public static DistributionDiscrete GetInstance(string distributionAndLeafName)
        { 
            if (distributionAndLeafName.StartsWith("Conditional"))
            {
                return DistributionDiscreteConditional.GetInstance(distributionAndLeafName.Substring("Conditional".Length));
            }
            else if (distributionAndLeafName.StartsWith("Joint"))
            { 
                return DistributionDiscreteJointUndirected.GetInstance(distributionAndLeafName.Substring("Joint".Length)); 
            }
            throw new ArgumentException("Cannot parse DistributionDiscrete name " + distributionAndLeafName); 
        }


        public override string ToString()
        {
            return "DERIVED CLASSES MUST OVERRIDE TOSTRING METHOD"; 
        } 

        //public virtual bool UsePredictorVariable(bool useParameter) 
        //{
        //    return useParameter;
        //}

        public double[][] CreateDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList discreteParameters,
            Converter<Leaf, SufficientStatistics> predictorClassFunction) 
        { 
            if (branchOrLeaf is Branch)
                return CreateDistribution((Branch)branchOrLeaf, discreteParameters); 
            else
                return CreateDistribution((Leaf)branchOrLeaf, discreteParameters, predictorClassFunction);
        }

        public virtual double[][] CreateDistribution(Branch branch, OptimizationParameterList discreteParameters)
        { 
            return DefaultDistribution(branch, discreteParameters); 
        }
 
        public abstract double[][] CreateDistribution(Leaf leaf, OptimizationParameterList discreteParameters,
            Converter<Leaf, SufficientStatistics> predictorClassFunction);

        protected abstract double[][] DefaultDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList discreteParameters);

        public abstract double[] GetPriorProbabilities(OptimizationParameterList discreteParameters); 
 
        public abstract OptimizationParameterList GetParameters();
 
        public abstract OptimizationParameterList GetParameters(int[] fisherCounts, OptimizationParameterList initializationParams);

        public abstract OptimizationParameterList GetParameters(double[] parameterValuesInPrintOrder);


        public string GetParameterHeaderString() 
        { 
            return GetParameterHeaderString("");
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



         public abstract void ReflectClassInDictionaries(DiscreteStatistics discreteStatistics, Leaf leaf,
            ref Dictionary<string, BooleanStatistics> predictorMapToCreate, ref Dictionary<string, BooleanStatistics> targetMapToCreate);
 
        public virtual bool NeedToRunToFindPValue(int[] fisherCounts) 
        {
            return false; 
        }


    }
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
