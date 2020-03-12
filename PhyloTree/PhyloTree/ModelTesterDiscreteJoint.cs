using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.IO;

namespace VirusCount.PhyloTree 
{ 
    public class ModelTesterDiscreteJoint : ModelTesterDiscrete
    { 
        new public const string Name = "Joint";

        private readonly int _chiSquareDegreesOfFreedom;

        protected ModelTesterDiscreteJoint(DistributionDiscrete nullModel, DistributionDiscrete altModel)
            : 
            base(nullModel, altModel) 
        {
            _chiSquareDegreesOfFreedom = altModel.GetParameters(true).CountFreeParameters() - 2 * nullModel.GetParameters(false).CountFreeParameters(); 
        }

        public static ModelTesterDiscreteJoint GetInstance(DistributionDiscrete nullDistn, DistributionDiscrete altDistn)
        {
            return new ModelTesterDiscreteJoint(nullDistn, altDistn);
        } 
 
        protected override int ChiSquareDegreesOfFreedom
        { 
            get
            {
                return _chiSquareDegreesOfFreedom;
            }
        }
        protected override string NullModelParametersAndLikelihoodHeaderString 
        { 
            get
            { 
                return SpecialFunctions.CreateTabString(
                    NullModelDistribution.GetParameterHeaderString("Pred"),
                    "logLikelihoodPred",
                    NullModelDistribution.GetParameterHeaderString("Targ"),
                    "logLikelihoodTarg",
                    "logLikelihoodPredAndTarg" 
                    ); 
            }
        } 

        protected override string AlternativeModelParametersAndLikelihoodHeaderString
        {
            get
            {
                return SpecialFunctions.CreateTabString( 
                    AlternativeModelDistribution.GetParameterHeaderString(), 
                    "logLikelihoodAB");
            } 
        }

        protected override double ComputeLLR(ModelScorer modelScorer, PhyloTree phyloTree, StringBuilder stringBuilder, double targetMarginal, double predictorMarginal,
            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction, Converter<Leaf, SufficientStatistics> targetDistributionClassFunction)
        {
            Converter<Leaf, SufficientStatistics> LeafToJointDistributionClass = 
                CreateAlternativeSufficientStatisticsMap(predictorDistributionClassFunction, targetDistributionClassFunction); 

            double logLikelihoodIndependentModel, logLikelihoodJointModel; 
            Score scoreIndTarget, scoreIndPredictor, scoreJoint;
            MessageInitializer messageInitializer;

            // first score the target.
            NullModelDistribution.EmpiricalEquilibrium = targetMarginal;
            messageInitializer = modelScorer.CreateMessageInitializer(predictorDistributionClassFunction, targetDistributionClassFunction, NullModelDistribution); 
            scoreIndTarget = modelScorer.ScoreModel(messageInitializer, false); 

            NullModelDistribution.EmpiricalEquilibrium = predictorMarginal; 
            messageInitializer = modelScorer.CreateMessageInitializer(targetDistributionClassFunction, predictorDistributionClassFunction, NullModelDistribution);
            scoreIndPredictor = modelScorer.ScoreModel(messageInitializer, false);

            DistributionDiscreteJointBinary jointDistn = (DistributionDiscreteJointBinary)AlternativeModelDistribution;
            jointDistn.SetInitialParams(scoreIndPredictor.OptimizationParameters, scoreIndTarget.OptimizationParameters);
            messageInitializer = modelScorer.CreateMessageInitializer(null, LeafToJointDistributionClass, jointDistn); 
            scoreJoint = modelScorer.ScoreModel(messageInitializer, false); 

            logLikelihoodIndependentModel = scoreIndTarget.Loglikelihood + scoreIndPredictor.Loglikelihood; 
            logLikelihoodJointModel = scoreJoint.Loglikelihood;

            stringBuilder.Append(SpecialFunctions.CreateTabString(scoreIndPredictor.ToString(NullModelDistribution), scoreIndTarget.ToString(NullModelDistribution),
                logLikelihoodIndependentModel, scoreJoint.ToString(jointDistn), ""));

            double diff = logLikelihoodJointModel - logLikelihoodIndependentModel; 
            return diff; 
        }
 
        //protected override NullDataCollection CreateNullDataGenerator(
        //    ModelScorer modelScorer,
        //    PhyloTree phyloTree,
        //    RangeCollection nullIndexRangeCollection,
        //    IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration,
        //    IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration) 
        ////Dictionary<string, Dictionary<string, SufficientStatistics>> predictorVariableToCaseIdToRealNonMissingValue, 
        ////Dictionary<string, Dictionary<string, SufficientStatistics>> targetVariableToCaseIdToRealNonMissingValue)
        //{ 
        //    return NullDataCollection.GetInstance(
        //        new NullDataGeneratorPredictorParametric(modelScorer, phyloTree, this),
        //        nullIndexRangeCollection,
        //        predictorNameAndCaseIdToNonMissingValueEnumeration,
        //        targetNameAndCaseIdToNonMissingValueEnumeration);
        //} 
 
        public override string ToString()
        { 
            return Name + AlternativeModelDistribution.ToString();
        }

        public override Converter<Leaf, SufficientStatistics> CreateAlternativeSufficientStatisticsMap(
            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction,
            Converter<Leaf, SufficientStatistics> targetDistributionClassFunction) 
        { 
            return delegate(Leaf leaf)
                { 
                    DistributionDiscreteJointBinary.DistributionClass jointClass;

                    SufficientStatistics predStats = predictorDistributionClassFunction(leaf);
                    SufficientStatistics targStats = targetDistributionClassFunction(leaf);

                    if (predStats.IsMissing() || targStats.IsMissing()) 
                    { 
                        jointClass = DistributionDiscreteJointBinary.DistributionClass.Missing;
                    } 
                    else
                    {
                        DiscreteStatistics predClass = (DiscreteStatistics)predStats;
                        DiscreteStatistics targetClass = (DiscreteStatistics)targStats;

                        if (predClass == (int)DistributionDiscreteBinary.DistributionClass.False) 
                        { 
                            if (targetClass == (int)DistributionDiscreteBinary.DistributionClass.False)
                                jointClass = DistributionDiscreteJointBinary.DistributionClass.FalseFalse; 
                            else
                                jointClass = DistributionDiscreteJointBinary.DistributionClass.FalseTrue;
                        }
                        else
                        {
                            if (targetClass == (int)DistributionDiscreteBinary.DistributionClass.False) 
                                jointClass = DistributionDiscreteJointBinary.DistributionClass.TrueFalse; 
                            else
                                jointClass = DistributionDiscreteJointBinary.DistributionClass.TrueTrue; 
                        }
                    }
                    return (DiscreteStatistics)(int)jointClass;
                };
        }
    } 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
