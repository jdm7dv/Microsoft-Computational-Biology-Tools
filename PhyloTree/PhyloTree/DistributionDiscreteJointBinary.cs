using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using Optimization;
using Msr.Mlas.LinearAlgebra; 
using System.IO; 

namespace VirusCount.PhyloTree 
{
    public class DistributionDiscreteJointBinary : DistributionDiscrete
    {
        /// <summary>
        /// By convention, the first bool corresponds to the predictor, and the second to the target. Make sure your code reflects
        /// this or you may get surprising results. 
        /// </summary> 
        public enum DistributionClass
        { 
            Missing = -1,
            TrueTrue = 0,
            TrueFalse,
            FalseTrue,
            FalseFalse
        }; 
 
        public enum ParameterIndex
        { 
            P_AB = 0,
            P_Ab,
            P_aB,
            Lambda_A,
            Lambda_B,
        }; 
 
        public const string JointType = "Undirected";
 
        private LinearAlgebra LinearAlgebra = new LinearAlgebra();
        private RateMatrixOptimized RateMatrixOptimized = new RateMatrixOptimized();

        public override int NonMissingClassCount
        {
            get { return 4; } 
        } 
        public override string ToString()
        { 
            return JointType;
        }
        protected DistributionDiscreteJointBinary() { }
        new public static DistributionDiscreteJointBinary GetInstance(string jointDistnType)
        {
            switch (jointDistnType) 
            { 
                case DistributionDiscreteJointBinary.JointType:
                    return new DistributionDiscreteJointBinary(); 
                case DistributionDiscreteJointBinaryHla.JointType:
                    return DistributionDiscreteJointBinaryHla.GetInstance();
                default:
                    throw new ArgumentException("Unknown joint distribution type: " + jointDistnType);
            }
            //return new DistributionDiscreteJointBinary(); 
        } 

        public override double[][] CreateDistribution(Leaf leaf, OptimizationParameterList discreteParameters, Converter<Leaf, SufficientStatistics> predClass) 
        {
            //SpecialFunctions.CheckCondition(predClass == null, "The predictorToDistnClass closure is not null. Did you call the right method?");
            return DefaultDistribution(leaf, discreteParameters);
        }

        public override double[] GetPriorProbabilities(OptimizationParameterList discreteParameters) 
        { 
            //const double eps = 0.0001;
            double[] priors = new double[4]; 
            priors[(int)DistributionClass.TrueTrue] = discreteParameters[(int)ParameterIndex.P_AB].Value;
            priors[(int)DistributionClass.TrueFalse] = discreteParameters[(int)ParameterIndex.P_Ab].Value;
            priors[(int)DistributionClass.FalseTrue] = discreteParameters[(int)ParameterIndex.P_aB].Value;
            priors[(int)DistributionClass.FalseFalse] = 1 - priors[0] - priors[1] - priors[2];

            return priors; 
        } 

        protected override double[][] DefaultDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList parameters) 
        {

            double[][] P = GetTransitionProbabilityMatrix(parameters, branchOrLeaf.Length);

            return P;
        } 
 
        public virtual double[][] GetTransitionProbabilityMatrix(OptimizationParameterList parameters, double t)
        { 
            double lambdaA = parameters[(int)ParameterIndex.Lambda_A].Value;
            //lambdaA = Math.Min(1e4, lambdaA);
            //lambdaA = Math.Max(1e-4, lambdaA);
            double lambdaB = parameters[(int)ParameterIndex.Lambda_B].Value;
            //lambdaB = Math.Min(1e4, lambdaB);
            //lambdaB = Math.Max(1e-4, lambdaB); 
            double pAB = parameters[(int)ParameterIndex.P_AB].Value; 
            double pAb = parameters[(int)ParameterIndex.P_Ab].Value;
            double paB = parameters[(int)ParameterIndex.P_aB].Value; 

            double pab = 1 - pAB - pAb - paB;
            double pA = pAB + pAb;
            double pB = pAB + paB;
            double pb = 1 - pB;
            double pa = 1 - pA; 
 
            if (pA * pa * pB * pb == 0)
                throw new NotComputableException("Joint distribution breaks down when one of the variables is always (or never) true."); 
            //if (double.IsInfinity(lambdaA * lambdaB))
            //    throw new NotComputableException("Joint distribution breaks down when on of the rate parameters are infinite.");

            const double minValue = 0.0001; // much lower than this and the eigen value code becomes unstable. Values lower than this give the same results to at least 4 decimals.
            const double maxValue = 10000;
            lambdaA = Math.Min(maxValue, Math.Max(minValue, lambdaA)); 
            lambdaB = Math.Min(maxValue, Math.Max(minValue, lambdaB)); 
            //double a = Math.Max(pA == 0 ? 0 : lambdaB * pAb / pA, minValue);
            //double b = Math.Max(pB == 0 ? 0 : lambdaA * paB / pB, minValue); 
            //double c = Math.Max(pB == 0 ? 0 : lambdaA * pab / pb, minValue);
            //double d = Math.Max(pa == 0 ? 0 : lambdaB * pab / pa, minValue);
            double a = lambdaB * pAb / pA;
            double b = lambdaA * paB / pB;
            double c = lambdaB * pAB / pA;
            double d = lambdaA * pab / pb; 
            double e = lambdaA * pAB / pB; 
            double f = lambdaB * pab / pa;
            double g = lambdaA * pAb / pb; 
            double h = lambdaB * paB / pa;

            return GetTransitionProbabilityMatrix(a, b, c, d, e, f, g, h, t);

        }
 
        protected double[][] GetTransitionProbabilityMatrix(double a, double b, double c, double d, double e, double f, double g, double h, double t) 
        {
            try 
            {
                return LinearAlgebra.MatrixExpCached(RateMatrixOptimized.ComputeEigenPairCached(a, b, c, d, e, f, g, h), t);
            }
            catch(Exception exception)// (InvalidCastException exception)
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

        /// <summary>
        /// Sets the initial parameter values to the independent model under the two given models.
        /// </summary>
        /// <param name="independent1"></param>
        /// <param name="independent2"></param> 
        public virtual void SetInitialParams(OptimizationParameterList predictorParams, OptimizationParameterList targetParams) 
        {
            OptimizationParameterList initParams = GetParameters(false); 
            double pA = predictorParams[(int)DistributionDiscreteBinary.ParameterIndex.Equilibrium].Value;
            double pB = targetParams[(int)DistributionDiscreteBinary.ParameterIndex.Equilibrium].Value;

            initParams[(int)ParameterIndex.Lambda_A].Value = predictorParams[(int)DistributionDiscreteBinary.ParameterIndex.Lambda].Value;
            initParams[(int)ParameterIndex.Lambda_B].Value = targetParams[(int)DistributionDiscreteBinary.ParameterIndex.Lambda].Value;
            initParams[(int)ParameterIndex.P_AB].Value = pA * pB; 
            initParams[(int)ParameterIndex.P_Ab].Value = pA * (1 - pB); 
            initParams[(int)ParameterIndex.P_aB].Value = (1 - pA) * pB;
 
            InitialParamVals = initParams;
        }

        /// <summary>
        /// Initialization assumptions: LambdaA and LambdaB are 1. ie, both bases are evolving at the average rate.
        /// P_A = (1-P_B) = empiricalLeafMargin. Note however that the empiricalLeafMargin is measured for base a; base B could be entirely different. 
        /// However, we need to keep the interface the same as for the other DiscreteDistributions, so it's not clear how to pass that information 
        /// in. For now we assume symmetry.
        /// </summary> 
        public override OptimizationParameterList GetParameters(bool useConditionalParameter)
        {
            //Random random = new Random();
            //double pAB = rand.NextDouble();
            //double pAb = (1 - pAB) * rand.NextDouble();
            //double paB = (1 - pAB - pAb) * rand.NextDouble(); 
            //double lA = 3000 * rand.NextDouble(); 
            //double lB = 3000 * rand.NextDouble();
 
            double pAB = InitialParamVals == null ? EmpiricalEquilibrium / 2 : InitialParamVals[(int)ParameterIndex.P_AB].Value;
            double pAb = InitialParamVals == null ? EmpiricalEquilibrium / 2 : InitialParamVals[(int)ParameterIndex.P_Ab].Value;
            double paB = InitialParamVals == null ? (1 - EmpiricalEquilibrium) / 2 : InitialParamVals[(int)ParameterIndex.P_aB].Value;
            double lA = InitialParamVals == null ? 1 : InitialParamVals[(int)ParameterIndex.Lambda_A].Value;
            double lB = InitialParamVals == null ? 1 : InitialParamVals[(int)ParameterIndex.Lambda_B].Value;
 
            OptimizationParameterList optList = OptimizationParameterList.GetInstance(); 

            optList.Add((int)ParameterIndex.P_AB, OptimizationParameter.GetProbabilityInstance("P_TT", pAB, true)); 
            optList.Add((int)ParameterIndex.P_Ab, OptimizationParameter.GetProbabilityInstance("P_TF", pAb, true));
            optList.Add((int)ParameterIndex.P_aB, OptimizationParameter.GetProbabilityInstance("P_FT", paB, true));
            optList.Add((int)ParameterIndex.Lambda_A, OptimizationParameter.GetPositiveFactorInstance("Lambda_A", lA, true));
            optList.Add((int)ParameterIndex.Lambda_B, OptimizationParameter.GetPositiveFactorInstance("Lambda_B", lB, true));

            optList.SetCheckConditions(CheckParameterConstraints); 
            return optList; 
        }
 
        public override OptimizationParameterList GetParameters(double[] parametersInPrintOrder)
        {
            OptimizationParameterList parameters = GetParameters(true);
            parameters[(int)ParameterIndex.Lambda_A].Value = parametersInPrintOrder[0];
            parameters[(int)ParameterIndex.Lambda_B].Value = parametersInPrintOrder[1];
            parameters[(int)ParameterIndex.P_AB].Value = parametersInPrintOrder[2]; 
            parameters[(int)ParameterIndex.P_Ab].Value = parametersInPrintOrder[3]; 
            parameters[(int)ParameterIndex.P_aB].Value = parametersInPrintOrder[4];
            return parameters; 
        }

        public bool CheckParameterConstraints(OptimizationParameterList parameters)
        {
            double P_AB = parameters[(int)ParameterIndex.P_AB].Value;
            double P_Ab = parameters[(int)ParameterIndex.P_Ab].Value; 
            double P_aB = parameters[(int)ParameterIndex.P_aB].Value; 

            return P_AB + P_Ab + P_aB <= 1; 
        }

        public override string GetParameterHeaderString(string modifier)
        {
            return SpecialFunctions.CreateTabString("Lambda_A", "Lambda_B", "P_TT", "P_TF", "P_FT", "P_FF", "P_A", "P_B" + modifier).Replace("\t", modifier + "\t");
        } 
 
        public override string GetParameterValueString(OptimizationParameterList parameters)
        { 
            double lambdaA = parameters[(int)ParameterIndex.Lambda_A].Value;
            double lambdaB = parameters[(int)ParameterIndex.Lambda_B].Value;
            double P_AB = parameters[(int)ParameterIndex.P_AB].Value;
            double P_Ab = parameters[(int)ParameterIndex.P_Ab].Value;
            double P_aB = parameters[(int)ParameterIndex.P_aB].Value;
            double P_ab = 1 - P_AB - P_Ab - P_aB; 
            double P_A = P_AB + P_Ab; 
            double P_B = P_AB + P_aB;
 
            return SpecialFunctions.CreateTabString(lambdaA, lambdaB, P_AB, P_Ab, P_aB, P_ab, P_A, P_B);
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
 
        public static OptimizationParameterList MakeIndependenceParams(OptimizationParameterList parameters)
        { 
            double P_AB = parameters[(int)ParameterIndex.P_AB].Value;
            double P_Ab = parameters[(int)ParameterIndex.P_Ab].Value;
            double P_aB = parameters[(int)ParameterIndex.P_aB].Value;
            double P_A = P_AB + P_Ab;
            double P_B = P_AB + P_aB;
 
            OptimizationParameterList result = parameters.Clone(); 
            result[(int)ParameterIndex.P_AB].Value = P_A * P_B;
            result[(int)ParameterIndex.P_Ab].Value = P_A * (1 - P_B); 
            result[(int)ParameterIndex.P_aB].Value = (1 - P_A) * P_B;
            return result;
        }

    }
 
 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
