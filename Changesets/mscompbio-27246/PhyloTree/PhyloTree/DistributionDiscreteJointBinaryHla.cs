using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;
using Msr.Mlas.LinearAlgebra;
 
namespace VirusCount.PhyloTree 
{
    public class DistributionDiscreteJointBinaryHla : DistributionDiscreteJointBinary 
    {
        protected DistributionDiscreteJointBinaryHla() { }
        public static DistributionDiscreteJointBinaryHla GetInstance()
        {
            return new DistributionDiscreteJointBinaryHla();
        } 
 
        new public enum ParameterIndex
        { 
            P_AA = 0,
            P_Hla,
            RTimesLambdaA,
            ETimesLambaA,
            Lambda_Hla,
        }; 
 
        new public const string JointType = "Directed";
 
        public override string ToString()
        {
            return JointType;
        }

        public override double[] GetPriorProbabilities(OptimizationParameterList discreteParameters) 
        { 
            const double eps = 0.0001;
            double[] priors = new double[NonMissingClassCount]; 

            try
            {
                EigenPair eig = LinearAlgebra.ComputeSparseEigenPair(LinearAlgebra.Transpose(GetTransitionProbabilityMatrix(discreteParameters, 1)));
                ComplexNumber[] eigenValues = eig.EigenValues;
                for (int i = 0; i < 4; i++) 
                { 
                    if (ComplexNumber.ApproxEqual(eigenValues[i], 1, eps))
                    { 
                        priors = LinearAlgebra.Abs(LinearAlgebra.ComplexToDouble(LinearAlgebra.Transpose(eig.EigenVectors)[i]));
                        break;
                    }
                }

                priors = LinearAlgebra.Normalize(priors); 
            } 
            catch (Exception e)
            { 
                throw new NotComputableException("Problem computing the prior: " + e.Message);
            }

            return priors;

        } 
 
        public override double[][] GetTransitionProbabilityMatrix(OptimizationParameterList parameters, double t)
        { 
            double lambdaTimesE = parameters[(int)ParameterIndex.ETimesLambaA].Value;
            double lambdaHla = parameters[(int)ParameterIndex.Lambda_Hla].Value;
            double pAA = parameters[(int)ParameterIndex.P_AA].Value;
            double pHLA = parameters[(int)ParameterIndex.P_Hla].Value;
            double lambdaTimesR = parameters[(int)ParameterIndex.RTimesLambdaA].Value;
            //double pr = parameters[(int)ParameterIndex.P_r].Value; 
            //double erRatio = Math.Exp(erLogRatio); 

            //TEMPORARY TESTING!!!!!! 
            if (DateTime.Now.Date == new DateTime(2006, 07, 06).Date)
            {
                lambdaTimesR = lambdaTimesE;
            }

            double paa = 1 - pAA; 
            double phla = 1 - pHLA; 

 
            double hlaToNotHla = lambdaHla * phla;
            double notHlaToHla = lambdaHla * pHLA;

            double a = lambdaTimesE * paa;
            double b = hlaToNotHla;
            double c = lambdaTimesE * pAA; 
            double d = hlaToNotHla; 
            double e = notHlaToHla;
            double f = lambdaTimesR * paa; 
            double g = notHlaToHla;
            double h = lambdaTimesR * pAA;


            return GetTransitionProbabilityMatrix(a, b, c, d, e, f, g, h, t);
        } 
 

        /// <summary> 
        /// Sets the initial parameter values to the independent model under the two given models.
        /// </summary>
        /// <param name="independent1"></param>
        /// <param name="independent2"></param>
        public override void SetInitialParams(OptimizationParameterList hlaParams, OptimizationParameterList aaParams)
        { 
            OptimizationParameterList initParams = GetParameters(false); 
            double pHLA = hlaParams[(int)DistributionDiscreteBinary.ParameterIndex.Equilibrium].Value;
            double pAA = aaParams[(int)DistributionDiscreteBinary.ParameterIndex.Equilibrium].Value; 

            initParams[(int)ParameterIndex.P_Hla].Value = pHLA;
            initParams[(int)ParameterIndex.P_AA].Value = pAA;
            initParams[(int)ParameterIndex.Lambda_Hla].Value = hlaParams[(int)DistributionDiscreteBinary.ParameterIndex.Lambda].Value;
            initParams[(int)ParameterIndex.ETimesLambaA].Value = aaParams[(int)DistributionDiscreteBinary.ParameterIndex.Lambda].Value;
            initParams[(int)ParameterIndex.RTimesLambdaA].Value = aaParams[(int)DistributionDiscreteBinary.ParameterIndex.Lambda].Value; 
            //initParams[(int)ParameterIndex.ERLogRatio].Value = 0; 
            //initParams[(int)ParameterIndex.P_r].Value = 0;
 
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
            double pHLA = InitialParamVals == null ? 0.1 : InitialParamVals[(int)ParameterIndex.P_Hla].Value;
            double pAA = InitialParamVals == null ? EmpiricalEquilibrium : InitialParamVals[(int)ParameterIndex.P_AA].Value;
            double lAA = InitialParamVals == null ? 1 : InitialParamVals[(int)ParameterIndex.ETimesLambaA].Value;
            double lHLA = InitialParamVals == null ? 1 : InitialParamVals[(int)ParameterIndex.Lambda_Hla].Value; 
            double erRatio = InitialParamVals == null ? 0 : InitialParamVals[(int)ParameterIndex.RTimesLambdaA].Value; 
            //double r = InitialParamVals == null ? 0 : InitialParamVals[(int)ParameterIndex.P_r].Value;
 
            OptimizationParameterList optList = OptimizationParameterList.GetInstance();

            optList.Add((int)ParameterIndex.P_AA, OptimizationParameter.GetProbabilityInstance("P_AA", pAA, true));
            optList.Add((int)ParameterIndex.P_Hla, OptimizationParameter.GetProbabilityInstance("P_Hla", pHLA, true));
            optList.Add((int)ParameterIndex.ETimesLambaA, OptimizationParameter.GetPositiveFactorInstance("Lambda_AA", lAA, true));
            optList.Add((int)ParameterIndex.Lambda_Hla, OptimizationParameter.GetPositiveFactorInstance("Lambda_Hla", lHLA, true)); 
            optList.Add((int)ParameterIndex.RTimesLambdaA, OptimizationParameter.GetPositiveFactorInstance("E_R_Ratio", erRatio, true)); 
            //optList.Add((int)ParameterIndex.ERLogRatio, OptimizationParameter.GetTanInstance("E_R_Ratio", erRatio, true, -10, 10));
            //optList.Add((int)ParameterIndex.P_r, OptimizationParameter.GetProbabilityInstance("r", r, true));  // DON'T LEARN R FOR NOW 

            //optList.SetCheckConditions(CheckParameterConstraints);
            return optList;
        }

        public override OptimizationParameterList GetParameters(double[] parametersInPrintOrder) 
        { 
            OptimizationParameterList parameters = GetParameters(true);
            parameters[(int)ParameterIndex.ETimesLambaA].Value = parametersInPrintOrder[0]; 
            parameters[(int)ParameterIndex.RTimesLambdaA].Value = parametersInPrintOrder[1];
            parameters[(int)ParameterIndex.Lambda_Hla].Value = parametersInPrintOrder[2];
            parameters[(int)ParameterIndex.P_AA].Value = parametersInPrintOrder[3];
            parameters[(int)ParameterIndex.P_Hla].Value = parametersInPrintOrder[4];
            //if (parametersInPrintOrder.Length == 6)
            //    parameters[(int)ParameterIndex.P_r].Value = parametersInPrintOrder[5]; 
            return parameters; 
        }
 
        //new public bool CheckParameterConstraints(OptimizationParameterList parameters)
        //{
        //    double P_AA = parameters[(int)ParameterIndex.P_AA].Value;
        //    double P_Hla = parameters[(int)ParameterIndex.P_Hla].Value;

        //    return 0 <= P_AA && P_AA <= 1 && 0 <= P_Hla && P_Hla <= 1; 
        //} 

 
        public override string GetParameterHeaderString(string modifier)
        {
            return SpecialFunctions.CreateTabString("(1-e)*Lambda_AA", "(1-r)*Lambda_AA", "Lambda_Hla", "P_AA", "P_Hla" + modifier).Replace("\t", modifier + "\t");
            //return SpecialFunctions.CreateTabString("Lambda_AA", "Lambda_Hla", "P_AA", "P_Hla", "e", "r" + modifier).Replace("\t", modifier + "\t");
        }
 
        public override string GetParameterValueString(OptimizationParameterList parameters) 
        {
            double eTimesLambdaA = parameters[(int)ParameterIndex.ETimesLambaA].Value; 
            double Lambda_Hla = parameters[(int)ParameterIndex.Lambda_Hla].Value;
            double P_aa = parameters[(int)ParameterIndex.P_AA].Value;
            double P_Hla = parameters[(int)ParameterIndex.P_Hla].Value;
            double rTimesLambdaA = parameters[(int)ParameterIndex.RTimesLambdaA].Value;
            //double P_r = parameters[(int)ParameterIndex.P_r].Value;
 
            return SpecialFunctions.CreateTabString(eTimesLambdaA, rTimesLambdaA, Lambda_Hla, P_aa, P_Hla); 
        }
 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
