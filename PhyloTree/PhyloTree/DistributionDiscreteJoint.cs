using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.LinearAlgebra;
using Optimization;
using Msr.Mlas.SpecialFunctions;
 
namespace VirusCount.PhyloTree 
{
    public abstract class DistributionDiscreteJoint : DistributionDiscrete 
    {
        public enum DistributionClass
        {
            Missing = -1,
            TrueTrue = 0,
            TrueFalse, 
            FalseTrue, 
            FalseFalse
        }; 

        private LinearAlgebra LinearAlgebra = new LinearAlgebra();
        private RateMatrixOptimized RateMatrixOptimized = new RateMatrixOptimized();

        public override int NonMissingClassCount
        { 
            get { return 4; } 
        }
        public override bool DependsOnMoreThanOneVariable 
        {
            get { return true; }
        }

        public override int FreeParameterCount
        { 
            get { return 5; } 
        }
 

        public abstract string BaseName { get;}

        protected DistributionDiscreteJoint() { }

        public override string ToString() 
        { 
            return BaseName;
        } 

        new public static DistributionDiscreteJoint GetInstance(string jointDistnType)
        {
            SpecialFunctions.CheckCondition(false, "Joint distributions not supported because need code/libraries for matrix operations.");
            switch (jointDistnType.ToLower())
            { 
                case "undirected": 
                    return DistributionDiscreteJointUndirected.GetInstance();
                case "directed": 
                    return DistributionDiscreteJointDirected.GetInstance();
                default:
                    throw new ArgumentException("Unknown joint distribution type: " + jointDistnType);
            }
        }
 
        public override double[][] CreateDistribution(Leaf leaf, OptimizationParameterList discreteParameters, Converter<Leaf, SufficientStatistics> predClass) 
        {
            return DefaultDistribution(leaf, discreteParameters); 
        }

        protected override double[][] DefaultDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList parameters)
        {

            double[][] P = GetTransitionProbabilityMatrix(parameters, branchOrLeaf.Length); 
 
            return P;
        } 

        public abstract double[][] GetTransitionProbabilityMatrix(OptimizationParameterList parameters, double t);

        protected double[][] GetTransitionProbabilityMatrix(double a, double b, double c, double d, double e, double f, double g, double h, double t)
        {
            try 
            { 
                return LinearAlgebra.MatrixExpCached(RateMatrixOptimized.ComputeEigenPairCached(a, b, c, d, e, f, g, h), t);
            } 
            catch (Exception exception)// (InvalidCastException exception)
            {
                // if it failed, it did because we had bogus eigen values.
                Console.WriteLine(exception.Message + "\nRecomputing eigen pairs from slow method.");
                try
                { 
                    return LinearAlgebra.MatrixExpCached(RateMatrixOptimized.RecomputeEigenPairCachedFromSlow(a, b, c, d, e, f, g, h), t); 
                }
                catch (Exception exception2) // Sho could also fail to converge, throwing it's own exception. 
                {
                    Console.WriteLine(exception2.Message + "\nPassing null message.");
                    throw new NotComputableException("Could not comput matrix exponentiation. The matrix values are too unstable for our methods.");
                }
            }
        } 
 
        public override OptimizationParameterList GetParameters()
        { 
            return GetParameters(new int[] { 1, 1 }, null);
        }

        public override void ReflectClassInDictionaries(DiscreteStatistics discreteStatistics, Leaf leaf,
            ref Dictionary<string, BooleanStatistics> predictorMapToCreate, ref Dictionary<string, BooleanStatistics> targetMapToCreate)
        { 
            string caseName = leaf.CaseName; 
            if (!predictorMapToCreate.ContainsKey(caseName))
            { 
                predictorMapToCreate.Add(caseName, false);
            }
            if (!targetMapToCreate.ContainsKey(caseName))
            {
                targetMapToCreate.Add(caseName, false);
            } 
 
            int discreteClass = (int)discreteStatistics;
            switch ((DistributionClass)discreteClass) 
            {
                case DistributionClass.FalseFalse:
                    predictorMapToCreate[caseName] = false;
                    targetMapToCreate[caseName] = false;
                    break;
                case DistributionClass.FalseTrue: 
                    predictorMapToCreate[caseName] = false; 
                    targetMapToCreate[caseName] = true;
                    break; 
                case DistributionClass.TrueFalse:
                    predictorMapToCreate[caseName] = true;
                    targetMapToCreate[caseName] = false;
                    break;
                case DistributionClass.TrueTrue:
                    predictorMapToCreate[caseName] = true; 
                    targetMapToCreate[caseName] = true; 
                    break;
                default: 
                    SpecialFunctions.CheckCondition(false, "Shouldn't be here.");
                    break;
            }
        }

        /// <summary> 
        /// Sets the initial parameter values to the independent model under the two given models. 
        /// </summary>
        /// <param name="independent1"></param> 
        /// <param name="independent2"></param>
        public abstract OptimizationParameterList GenerateInitialParams(OptimizationParameterList var1Params, OptimizationParameterList var2Params);

        public abstract bool ParametersCannotBeEvaluated(OptimizationParameterList parameters);

 
    } 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
