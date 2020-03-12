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
    //TODO Rename
    public class DistributionDiscreteJointUndirected : DistributionDiscreteJoint
    {

        //public enum DistributionClass 
        //{ 
        //    Missing = -1,
        //    TrueTrue = 0, 
        //    TrueFalse,
        //    FalseTrue,
        //    FalseFalse
        //};

        public enum ParameterIndex 
        { 
            P_AB = 0,
            P_Ab, 
            P_aB,
            Lambda_A,
            Lambda_B,
        };

        public const string JointType = "Undirected"; 
 
        public override string BaseName
        { 
            get { return JointType; }
        }

        protected DistributionDiscreteJointUndirected() { }

        public static DistributionDiscreteJointUndirected GetInstance() 
        { 
            return new DistributionDiscreteJointUndirected();
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

        //protected override double[][] DefaultDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList parameters)
        //{ 
 
        //    double[][] P = GetTransitionProbabilityMatrix(parameters, branchOrLeaf.Length);
 
        //    return P;
        //}

        public override bool ParametersCannotBeEvaluated(OptimizationParameterList parameters)
        {
            double lambdaA = parameters[(int)ParameterIndex.Lambda_A].Value; 
            double lambdaB = parameters[(int)ParameterIndex.Lambda_B].Value; 
            double pAB = parameters[(int)ParameterIndex.P_AB].Value;
            double pAb = parameters[(int)ParameterIndex.P_Ab].Value; 
            double paB = parameters[(int)ParameterIndex.P_aB].Value;

            double pA = pAB + pAb;
            double pB = pAB + paB;
            double pb = 1 - pB;
            double pa = 1 - pA; 
 
            return (pA * pa * pB * pb == 0);
        } 

        public override double[][] GetTransitionProbabilityMatrix(OptimizationParameterList parameters, double t)
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

            double[][] pr = GetTransitionProbabilityMatrix(a, b, c, d, e, f, g, h, t); 
            return pr; 

        } 

        public override OptimizationParameterList GenerateInitialParams(OptimizationParameterList var1Params, OptimizationParameterList var2Params)
        {
            OptimizationParameterList initParams = GetParameters();
            double pA = var1Params[(int)DistributionDiscreteConditional.ParameterIndex.Equilibrium].Value;
            double pB = var2Params[(int)DistributionDiscreteConditional.ParameterIndex.Equilibrium].Value; 
 
            initParams[(int)ParameterIndex.Lambda_A].Value = var1Params[(int)DistributionDiscreteConditional.ParameterIndex.Lambda].Value;
            initParams[(int)ParameterIndex.Lambda_B].Value = var2Params[(int)DistributionDiscreteConditional.ParameterIndex.Lambda].Value; 
            initParams[(int)ParameterIndex.P_AB].Value = pA * pB;
            initParams[(int)ParameterIndex.P_Ab].Value = pA * (1 - pB);
            initParams[(int)ParameterIndex.P_aB].Value = (1 - pA) * pB;

            return initParams;
        } 
 

 
        public override OptimizationParameterList GetParameters(int[] fisherCounts, OptimizationParameterList initParams)
        {
            double pAB, pAb, paB, lA, lB;

            if (initParams != null)
            { 
                pAB = initParams[(int)ParameterIndex.P_AB].Value; 
                pAb = initParams[(int)ParameterIndex.P_Ab].Value;
                paB = initParams[(int)ParameterIndex.P_aB].Value; 
                lA = initParams[(int)ParameterIndex.Lambda_A].Value;
                lB = initParams[(int)ParameterIndex.Lambda_B].Value;
            }
            else if (fisherCounts.Length == 4)
            {
                double sum = (double)SpecialFunctions.Sum(fisherCounts); 
                lA = lB = 1; 
                pAB = fisherCounts[(int)TwoByTwo.ParameterIndex.TT];
                pAb = fisherCounts[(int)TwoByTwo.ParameterIndex.TF]; 
                paB = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
            }
            else if (fisherCounts.Length == 2) // primarily for backward compatability testing
            {
                double empiricalEquilibrium = (double)fisherCounts[(int)DistributionDiscreteConditional.DistributionClass.True] / (fisherCounts[0] + fisherCounts[1]);
                lA = lB = 1; 
                pAB = empiricalEquilibrium / 2; 
                pAb = empiricalEquilibrium / 2;
                paB = (1 - empiricalEquilibrium) / 2; 
            }
            else
            {
                throw new ArgumentException("Cannot parse fisher counts of length " + fisherCounts.Length);
            }
 
 
            //double pAB = initParams == null ? empiricalEquilibrium / 2 : initParams[(int)ParameterIndex.P_AB].Value;
            //double pAb = initParams == null ? empiricalEquilibrium / 2 : initParams[(int)ParameterIndex.P_Ab].Value; 
            //double paB = initParams == null ? (1 - empiricalEquilibrium) / 2 : initParams[(int)ParameterIndex.P_aB].Value;
            //double lA = initParams == null ? 1 : initParams[(int)ParameterIndex.Lambda_A].Value;
            //double lB = initParams == null ? 1 : initParams[(int)ParameterIndex.Lambda_B].Value;

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
            OptimizationParameterList parameters = GetParameters();
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
            return SpecialFunctions.CreateTabString("Lambda_A", "Lambda_B", "P_TT", "P_TF", "P_FT", "P_FF", "P_A", "P_B");
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

        public static OptimizationParameterList MakeIndependenceParams(OptimizationParameterList parameters)
        {
            double P_AB = parameters[(int)ParameterIndex.P_AB].Value;
            double P_Ab = parameters[(int)ParameterIndex.P_Ab].Value;
            double P_aB = parameters[(int)ParameterIndex.P_aB].Value; 
            double P_A = P_AB + P_Ab; 
            double P_B = P_AB + P_aB;
 
            OptimizationParameterList result = (OptimizationParameterList)parameters.Clone();
            result[(int)ParameterIndex.P_AB].Value = P_A * P_B;
            result[(int)ParameterIndex.P_Ab].Value = P_A * (1 - P_B);
            result[(int)ParameterIndex.P_aB].Value = (1 - P_A) * P_B;
            return result;
        } 
 
    }
 

}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
