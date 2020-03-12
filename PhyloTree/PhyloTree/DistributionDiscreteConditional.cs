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
    // TODO: Rename with "Conditional"
    public class DistributionDiscreteConditional : DistributionDiscrete
    {
        protected readonly bool Use2PredictorParameters;
        protected readonly bool PredictorParametersInSequence; 
        protected readonly bool UseParameter; 
        protected LabelledLeafDistributionDiscrete TrueLabelledLeafDistributionDiscrete;
        protected LabelledLeafDistributionDiscrete FalseLabelledLeafDistributionDiscrete; 


        ///!!! need a name for this that doesn't contain 'Class'
        public enum DistributionClass
        {
            Missing = -1, 
            False = BooleanStatistics.False, 
            True = BooleanStatistics.True
        }; 

        public enum ParameterIndex
        {
            Predictor1 = 0, Predictor2, Lambda, Equilibrium
        };
 
        public override int NonMissingClassCount 
        {
            get { return 2; } 
        }

        public override bool DependsOnMoreThanOneVariable
        {
            get { return UseParameter; }
        } 
 
        public override int FreeParameterCount
        { 
            get { return 2 + (Use2PredictorParameters ? 1 : 0) + (UseParameter ? 1 : 0); }
        }

        protected DistributionDiscreteConditional()
            : this("Escape")
        { 
            Use2PredictorParameters = false; 
            PredictorParametersInSequence = false;
            UseParameter = false; 
        }

        protected DistributionDiscreteConditional(string labelledLeafDistribution)
        {
            labelledLeafDistribution = labelledLeafDistribution.ToLower();
            UseParameter = true; 
 
            int indexOfAnd = labelledLeafDistribution.IndexOf("and");
            int indexOfThen = labelledLeafDistribution.IndexOf("then"); 

            if (indexOfThen < 0 && indexOfAnd < 0)
            {
                Use2PredictorParameters = false;
                PredictorParametersInSequence = false;
 
                TrueLabelledLeafDistributionDiscrete = LabelledLeafDistributionDiscrete.GetInstance(labelledLeafDistribution); 
                FalseLabelledLeafDistributionDiscrete = TrueLabelledLeafDistributionDiscrete;
            } 
            else
            {
                string delimiterString = "";
                int delimiterIndex;
                if (indexOfAnd > 0)
                { 
                    PredictorParametersInSequence = false; 
                    delimiterString = "and";
                    delimiterIndex = indexOfAnd; 
                }
                else
                {
                    PredictorParametersInSequence = true;
                    delimiterString = "then";
                    delimiterIndex = indexOfThen; 
                } 

                Use2PredictorParameters = true; 
                string firstLabelledLeafDistribution = labelledLeafDistribution.Substring(0, delimiterIndex);
                string secondLabelledLeafDistribution = labelledLeafDistribution.Substring(delimiterIndex + delimiterString.Length);
                TrueLabelledLeafDistributionDiscrete = LabelledLeafDistributionDiscrete.GetInstance(firstLabelledLeafDistribution);
                FalseLabelledLeafDistributionDiscrete = LabelledLeafDistributionDiscrete.GetInstance(secondLabelledLeafDistribution);
            }
        } 
 
        new internal static DistributionDiscreteConditional GetInstance(string labelledLeafDistribution)
        { 
            return new DistributionDiscreteConditional(labelledLeafDistribution);
            //if (labelledLeafDistribution.StartsWith("Continuous"))
            //{
            //    return DistributionContinuousDiscreteBinary.GetInstance(labelledLeafDistribution.Substring("Continuous".Length));
            //}
            //else 
            //{ 
            //    return new DistributionDiscreteConditional(labelledLeafDistribution);
            //} 
        }

        //public override bool NeedToRunToFindPValue(int[] fisherCounts)
        //{
        //    return TrueLabelledLeafDistributionDiscrete.NeedToRunToFindPValue(fisherCounts) && FalseLabelledLeafDistributionDiscrete.NeedToRunToFindPValue(fisherCounts);
        //} 
 
        //public override bool UsePredictorVariable(bool useParameter)
        //{ 
        //    return useParameter || PredictorParametersInSequence;
        //}

        public override string ToString()
        {
            return TrueLabelledLeafDistributionDiscrete.ToString() + 
                (Use2PredictorParameters ? (PredictorParametersInSequence? "Then" : "And") + FalseLabelledLeafDistributionDiscrete.ToString() 
                : "");
        } 

        public override double[][] CreateDistribution(Leaf leaf, OptimizationParameterList discreteParameters,
            Converter<Leaf, SufficientStatistics> predictorClassFunction)
        {
            double[][] yDist = DefaultDistribution(leaf, discreteParameters);
 
            SpecialFunctions.CheckCondition(leaf.CaseName != null, "Leaf case name is null. Should we allow this?"); 

            double[][] zDist = new double[2][]; 

            double predictor1Value = discreteParameters[(int)ParameterIndex.Predictor1].Value;
            double predictor2Value =
                Use2PredictorParameters ? discreteParameters[(int)ParameterIndex.Predictor2].Value :
                predictor1Value;
            //DistributionClass classification = (DistributionClass)predictorClassFunction(leaf); 
            DistributionClass classification = 
                predictorClassFunction == null ? DistributionClass.False : (DistributionClass)(int)(DiscreteStatistics)predictorClassFunction(leaf);
 
            switch(classification)
            {
                case DistributionClass.True:
                    for (int parentValue = 0; parentValue < 2; ++parentValue)
                    {
                        zDist[parentValue] = 
                            TrueLabelledLeafDistributionDiscrete.PredictorTrueDistribution(predictor1Value, yDist[parentValue]); 
                    }
                    break; 
                case DistributionClass.False:
                    for (int parentValue = 0; parentValue < 2; ++parentValue)
                    {
                        zDist[parentValue] =
                            FalseLabelledLeafDistributionDiscrete.PredictorFalseDistribution(predictor2Value, yDist[parentValue]);
                    } 
                    break; 
                case DistributionClass.Missing:
                    //If there is predictor value for this case, then return all 1's 
                    SpecialFunctions.CheckCondition(false, "Missing data should have been taken care of by now.");
                    break;
                default:
                    throw new ArgumentException(classification + " is not a valid classification.");
            }
 
            return zDist; 
        }
 
        protected override double[][] DefaultDistribution(BranchOrLeaf branchOrLeaf, OptimizationParameterList discreteParameters)
        {
            double lambda = discreteParameters[(int)ParameterIndex.Lambda].Value;
            double x = discreteParameters[(int)ParameterIndex.Equilibrium].Value;

            // in phylogenies, Lambda = site-specific mutation rate (in literature, "gamma"), Length = time*average mutation rate, 
            // e^(-lambda*t) = Pr[no mutation), and 1 - e^(-lambda*t) = Pr[mutation] 
            double expExp = 1.0 - Math.Exp(-lambda * branchOrLeaf.Length);
            //Debug.Assert(expExp > 0); 

            // in phylogenies, X = equilibrium frequency of this A.A.
            double[][] yDist = new double[][] { new double[2], new double[2] };
            yDist[0][1] = x * expExp;        // Pr[mutation AND mutationToTrue]
            yDist[0][0] = 1.0 - yDist[0][1];                    // that is, Pr[mutation AND mutationToFalse] + Pr[no mutation] = (1-X)*expExp + (1 - expExp) = 1 - X*expExp
            yDist[1][0] = (1 - x) * expExp;  // Pr[mutation AND mutationToFalse] 
            yDist[1][1] = 1.0 - yDist[1][0]; 

#if DEBUG 
            //foreach (double[] dArr in yDist)
            //{
            //    foreach (double d in dArr)
            //        Debug.Assert(!double.IsNaN(d) && d >= 0 && d <= 1);

            //} 
#endif 

            return yDist; 
        }

        public override double[] GetPriorProbabilities(OptimizationParameterList discreteParameters)
        {
            double[] priors = new double[2];
            priors[(int)DistributionClass.True] = discreteParameters[(int)ParameterIndex.Equilibrium].Value; 
            priors[(int)DistributionClass.False] = 1 - priors[(int)DistributionClass.True]; 
            return priors;
        } 

        public override OptimizationParameterList GetParameters()
        {
            return GetParameters(UseParameter, 0.5, null);
        }
 
        public override OptimizationParameterList GetParameters(int[] fisherCounts, OptimizationParameterList initializationParams) 
        {
            double equilibrium = -1; 
            if (initializationParams == null)
            {
                double sum = (double)SpecialFunctions.Sum(fisherCounts);
                if (fisherCounts.Length == 2)
                {
                    equilibrium = fisherCounts[(int)DistributionClass.True] / sum; 
                } 
                else
                { 
                    equilibrium = (fisherCounts[(int)TwoByTwo.ParameterIndex.TT] + fisherCounts[(int)TwoByTwo.ParameterIndex.FT]) / sum;
                }
            }
            return GetParameters(UseParameter, equilibrium, initializationParams);
        }
 
        //public override bool TryDeriveParametersFromFisherCounts(int[] fisherCounts, out OptimizationParameterList parameters) 
        //{
        //    parameters = null; 
        //    if (fisherCounts.Length != 4)
        //    {
        //        return false;
        //    }

        //    int tt = fisherCounts[(int)TwoByTwo.ParameterIndex.TT]; 
        //    int tf = fisherCounts[(int)TwoByTwo.ParameterIndex.TF]; 
        //    int ft = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
        //    int ff = fisherCounts[(int)TwoByTwo.ParameterIndex.FF]; 
        //    int total = tt+tf+ft+ff;

        //    double predP = (double)(tt + tf) / total;
        //    double targP = (double)(tt + ft) / total;

        //    if (Math.Max(predP, targP) != 1 && Math.Min(predP, targP) != 0) 
        //    { 
        //        return false;
        //    } 

        //    parameters = GetParameters();
        //    if (targP == 0 || targP == 1)
        //    {
        //        parameters[(int)ParameterIndex.Equilibrium].Value = targP;
        //        parameters[(int)ParameterIndex.Lambda].Value = 0; 
        //    } 
        //}
 
        private OptimizationParameterList GetParameters(bool useConditionalParameter, double empericalEq, OptimizationParameterList initParams)
        {
            if (initParams != null)
            {
                OptimizationParameterList cloneList = (OptimizationParameterList)initParams.Clone();
                OptimizationParameter parameter1 = cloneList[(int)ParameterIndex.Predictor1]; 
                parameter1.DoSearch = useConditionalParameter || PredictorParametersInSequence; 
                OptimizationParameter parameter2 = cloneList[(int)ParameterIndex.Predictor2];
                parameter2.DoSearch = useConditionalParameter && Use2PredictorParameters; 
                if (!useConditionalParameter)
                {
                    if (!PredictorParametersInSequence)
                        parameter1.Value = 0.0;
                    parameter2.Value = 0.0;
                } 
                return cloneList; 
            }
            else 
            {
                OptimizationParameterList optList = OptimizationParameterList.GetInstance();

                optList.Add((int)ParameterIndex.Predictor1, OptimizationParameter.GetProbabilityInstance("selection_pressure", 0, useConditionalParameter || PredictorParametersInSequence));
                optList.Add((int)ParameterIndex.Predictor2, OptimizationParameter.GetProbabilityInstance("secondary_selection_pressure", 0, useConditionalParameter && Use2PredictorParameters));
                optList.Add((int)ParameterIndex.Lambda, OptimizationParameter.GetPositiveFactorInstance("lambda", 1, true)); 
                optList.Add((int)ParameterIndex.Equilibrium, OptimizationParameter.GetProbabilityInstance("pi", empericalEq, true)); 

 
                return optList;
            }
        }

        public override OptimizationParameterList GetParameters(double[] parametersInPrintOrder)
        { 
            OptimizationParameterList parameters = GetParameters(parametersInPrintOrder.Length >= 3, 0.5, null); 

            int nullParameterOffset = parametersInPrintOrder.Length - 2; 
            parameters[(int)ParameterIndex.Predictor1].Value = parametersInPrintOrder.Length == 2 ? 0 : parametersInPrintOrder[0];
            parameters[(int)ParameterIndex.Predictor2].Value = parametersInPrintOrder.Length < 4 ? 0 : parametersInPrintOrder[1];
            parameters[(int)ParameterIndex.Equilibrium].Value = parametersInPrintOrder[nullParameterOffset];
            parameters[(int)ParameterIndex.Lambda].Value = parametersInPrintOrder[nullParameterOffset + 1];
            return parameters;
        } 
 
        public override void ReflectClassInDictionaries(DiscreteStatistics discreteStatistics, Leaf leaf, ref Dictionary<string, BooleanStatistics> predictorMapToCreate, ref Dictionary<string, BooleanStatistics> targetMapToCreate)
        { 
            BooleanStatistics leafHasTarget = (int)discreteStatistics == (int)DistributionClass.True;
            if (!targetMapToCreate.ContainsKey(leaf.CaseName))
            {
                targetMapToCreate.Add(leaf.CaseName, leafHasTarget);
            }
            else 
            { 
                targetMapToCreate[leaf.CaseName] = leafHasTarget;
            } 
        }
    }


}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
