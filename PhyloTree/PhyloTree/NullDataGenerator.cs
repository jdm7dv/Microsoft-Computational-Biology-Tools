using System; 
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree 
{ 
    public abstract class NullDataGenerator //: ICloneable
    { 
        public int Preseed;

        private IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> _predictorNameAndCaseIdToNonMissingValueEnumeration;
        private IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> _targetNameAndCaseIdToNonMissingValueEnumeration;

        private Dictionary<string, Dictionary<string, SufficientStatistics>> _realPredictorVariableToCaseIdToNonMissingValue; 
        private Dictionary<string, Dictionary<string, SufficientStatistics>> _nullPredictorVariableToCaseIdToNonMissingValue; 
        private Dictionary<string, Dictionary<string, SufficientStatistics>> _realTargetVariableToCaseIdToNonMissingValue;
        private Dictionary<string, Dictionary<string, SufficientStatistics>> _nullTargetVariableToCaseIdToNonMissingValue; 

        public abstract string Name
        {
            get;
        }
 
        protected NullDataGenerator() 
        {
            _nullPredictorVariableToCaseIdToNonMissingValue = new Dictionary<string, Dictionary<string, SufficientStatistics>>(); 
            _nullTargetVariableToCaseIdToNonMissingValue = new Dictionary<string, Dictionary<string, SufficientStatistics>>();
        }

        // !! Add support for ModelTesterGaussian
        public static NullDataGenerator GetInstance(string generatorName, ModelScorer modelScorer, IDistribution distribution)
        { 
            switch (generatorName.ToLower()) 
            {
                case "predictorpermutation": 
                    return new NullDataGeneratorPredictorPermutation();
                case "targetpermutation":
                    return new NullDataGeneratorTargetPermutation();
                case "predictorparametric":
                    //!!HACK!! We can currently only evolve binary data. But even if modelTester is gaussian, the predictor is still binary.
                    // so all we need is a discrete model tester with any distribution (they all have the same null distribution, which is all 
                    // that's used. 
                    SpecialFunctions.CheckCondition(distribution is DistributionDiscrete, "Parametric data generation is currently only supported for discrete variables");
                    return new NullDataGeneratorPredictorParametric(modelScorer, (DistributionDiscrete)distribution); 
                case "targetparametric":
                    SpecialFunctions.CheckCondition(distribution is DistributionDiscrete, "Parametric data generation is currently only supported for discrete variables");
                    return new NullDataGeneratorTargetParametric(modelScorer, (DistributionDiscrete)distribution);
                default:
                    throw new ArgumentException("Cannot parse " + generatorName + " into a NullDataGenerator");
            } 
        } 

        internal void SetPreseed(int preseed) 
        {
            Preseed = preseed;
        }

        internal void SetPredictorNameAndCaseIdToNonMissingValueEnumeration(IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> realPredictorNameAndCaseIdToNonMissingValueEnumeration)
        { 
            _predictorNameAndCaseIdToNonMissingValueEnumeration = realPredictorNameAndCaseIdToNonMissingValueEnumeration; 
        }
 
        internal void SetTargetNameAndCaseIdToNonMissingValueEnumeration(IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> realTargetNameAndCaseIdToNonMissingValueEnumeration)
        {
            _targetNameAndCaseIdToNonMissingValueEnumeration = realTargetNameAndCaseIdToNonMissingValueEnumeration;
        }

        protected Dictionary<string, Dictionary<string, SufficientStatistics>> RealPredictorVariableToCaseIdToNonMissingValue 
        { 
            get
            { 
                if (_realPredictorVariableToCaseIdToNonMissingValue == null)
                {
                    _realPredictorVariableToCaseIdToNonMissingValue = SpecialFunctions.PairEnumerationToDictionary(_predictorNameAndCaseIdToNonMissingValueEnumeration);
                }
                return _realPredictorVariableToCaseIdToNonMissingValue;
            } 
        } 

        protected Dictionary<string, Dictionary<string, SufficientStatistics>> RealTargetVariableToCaseIdToNonMissingValue 
        {
            get
            {
                if (_realTargetVariableToCaseIdToNonMissingValue == null)
                {
                    _realTargetVariableToCaseIdToNonMissingValue = SpecialFunctions.PairEnumerationToDictionary(_targetNameAndCaseIdToNonMissingValueEnumeration); 
                } 
                return _realTargetVariableToCaseIdToNonMissingValue;
            } 
        }

        /// <summary>
        /// Creates a shallow copy of this object, with no null data.
        /// </summary>
        /// <returns></returns> 
        public object Clone() 
        {
            NullDataGenerator result = (NullDataGenerator)this.MemberwiseClone(); 
            result._nullPredictorVariableToCaseIdToNonMissingValue = new Dictionary<string, Dictionary<string, SufficientStatistics>>();
            result._nullTargetVariableToCaseIdToNonMissingValue = new Dictionary<string, Dictionary<string, SufficientStatistics>>();
            return result;
        }

        internal virtual Dictionary<string, SufficientStatistics> GetCaseIdToNonMissingPredictorValueOrDefault(string predictorVariable, Dictionary<string, SufficientStatistics> defaultValue, ref Random random) 
        { 
            if (!_nullPredictorVariableToCaseIdToNonMissingValue.ContainsKey(predictorVariable))
            { 
                _nullPredictorVariableToCaseIdToNonMissingValue.Add(
                    predictorVariable,
                    GenerateRandomMapping(RealPredictorVariableToCaseIdToNonMissingValue[predictorVariable], ref random));
            }

 
            return _nullPredictorVariableToCaseIdToNonMissingValue[predictorVariable]; 
        }
 
        internal virtual Dictionary<string, SufficientStatistics> GetCaseIdToNonMissingTargetValueOrDefault(string targetVariable, Dictionary<string, SufficientStatistics> defaultValue, ref Random random)
        {
            if (!_nullTargetVariableToCaseIdToNonMissingValue.ContainsKey(targetVariable))
            {
                _nullTargetVariableToCaseIdToNonMissingValue.Add(
                    targetVariable, 
                    GenerateRandomMapping(RealTargetVariableToCaseIdToNonMissingValue[targetVariable], ref random)); 
            }
 

            return _nullTargetVariableToCaseIdToNonMissingValue[targetVariable];
        }

        public abstract Dictionary<string, SufficientStatistics> GenerateRandomMapping(Dictionary<string, SufficientStatistics> realCaseIdToNonMissingValue, ref Random random);
    } 
 
    public class NullDataGeneratorPredictorPermutation : NullDataGenerator
    { 
        public override string Name
        {
            get { return "PredictorPermutation"; }
        }

        public override Dictionary<string, SufficientStatistics> GenerateRandomMapping(Dictionary<string, SufficientStatistics> realCaseIdToNonMissingValue, ref Random random) 
        { 
            return SpecialFunctions.RandomizeMapping(realCaseIdToNonMissingValue, ref random);
        } 

        internal override Dictionary<string, SufficientStatistics> GetCaseIdToNonMissingTargetValueOrDefault(string targetVariable, Dictionary<string, SufficientStatistics> defaultValue, ref Random random)
        {
            return defaultValue;
        }
    } 
 
    public class NullDataGeneratorTargetPermutation : NullDataGenerator
    { 
        public override string Name
        {
            get { return "TargetPermutation"; }
        }

        public override Dictionary<string, SufficientStatistics> GenerateRandomMapping(Dictionary<string, SufficientStatistics> realCaseIdToNonMissingValue, ref Random random) 
        { 
            return SpecialFunctions.RandomizeMapping(realCaseIdToNonMissingValue, ref random);
        } 

        internal override Dictionary<string, SufficientStatistics> GetCaseIdToNonMissingPredictorValueOrDefault(string predictorVariable, Dictionary<string, SufficientStatistics> defaultValue, ref Random random)
        {
            return defaultValue;
        }
    } 
 
    public abstract class NullDataGeneratorParametric : NullDataGenerator
    { 
        private ModelScorer _modelScorer;
        private DistributionDiscrete _discreteDistribution;

        public NullDataGeneratorParametric(ModelScorer modelScorer, DistributionDiscrete discreteDistn)
        {
            _modelScorer = modelScorer; 
            _discreteDistribution = discreteDistn; 
        }
 
        public override Dictionary<string, SufficientStatistics> GenerateRandomMapping(Dictionary<string, SufficientStatistics> realCaseIdToNonMissingValue, ref Random random)
        {
            //!!!!put check in to make sure ISufficientSTatistics is reall BooleanStatistics
            Converter<Leaf, SufficientStatistics> leafToDistnClassFunction = PhyloDDriver.CreateSufficientStatisticsMap(realCaseIdToNonMissingValue);

            PhyloTree tree = _modelScorer.PhyloTree; 
 
            MessageInitializer messageInitializer = MessageInitializerDiscrete.GetInstance(leafToDistnClassFunction, _discreteDistribution, new int[] { 1, 1 }, tree.LeafCollection);
 
            Score score = _modelScorer.MaximizeLikelihood(messageInitializer);

            double percentNonMissing = (double)tree.CountOfNonMissingLeaves(realCaseIdToNonMissingValue) /
                (double)SpecialFunctions.Count(tree.LeafCollection);
            double equilibrium = score.OptimizationParameters[(int)DistributionDiscreteConditional.ParameterIndex.Equilibrium].Value;
            double lambda = score.OptimizationParameters[(int)DistributionDiscreteConditional.ParameterIndex.Lambda].Value; 
 
            Dictionary<string, BooleanStatistics> randomCaseIdToNonMissingValue = tree.EvolveBinaryTree(equilibrium, lambda, 1 - percentNonMissing, ref random);
 
            Dictionary<string, SufficientStatistics> converted;
            SpecialFunctions.ConvertDictionaryToBaseClasses(randomCaseIdToNonMissingValue, out converted);

            return converted;
        }
    } 
 
    public class NullDataGeneratorPredictorParametric : NullDataGeneratorParametric
    { 
        public override string Name
        {
            get { return "PredictorParametric"; }
        }

        public NullDataGeneratorPredictorParametric(ModelScorer modelScorer, DistributionDiscrete distribution) 
            : 
        base(modelScorer, distribution) { }
 
        internal override Dictionary<string, SufficientStatistics> GetCaseIdToNonMissingTargetValueOrDefault(string targetVariable, Dictionary<string, SufficientStatistics> defaultValue, ref Random random)
        {
            return defaultValue;
        }
    }
 
    public class NullDataGeneratorTargetParametric : NullDataGeneratorParametric 
    {
        public override string Name 
        {
            get { return "TargetParametric"; }
        }

        public NullDataGeneratorTargetParametric(ModelScorer modelScorer, DistributionDiscrete distribution)
            : 
        base(modelScorer, distribution) { } 

        internal override Dictionary<string, SufficientStatistics> GetCaseIdToNonMissingPredictorValueOrDefault(string predictorVariable, Dictionary<string, SufficientStatistics> defaultValue, ref Random random) 
        {
            return defaultValue;
        }
    }
}
 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
