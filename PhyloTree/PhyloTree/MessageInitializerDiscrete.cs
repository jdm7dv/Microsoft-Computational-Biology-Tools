using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree 
{ 
    public class MessageInitializerDiscrete : MessageInitializer
    { 
        private readonly int[] _fisherCounts;
        private readonly OptimizationParameterList _initializationParams;

        public DistributionDiscrete DiscreteDistribution
        {
            get { return (DistributionDiscrete)PropogationDistribution; } 
        } 

        protected MessageInitializerDiscrete( 
            List<Converter<Leaf, SufficientStatistics>> predictorClassFunctionList,
            Converter<Leaf, SufficientStatistics> targetClassFunction,
            DistributionDiscrete discreteDistribution,
            int[] fisherCounts,
            OptimizationParameterList initParams,
            IEnumerable<Leaf> fullLeafCollection) 
            : base(predictorClassFunctionList, targetClassFunction, discreteDistribution, fullLeafCollection) 
        {
            _fisherCounts = fisherCounts; 
            _initializationParams = initParams;
        }

        public static MessageInitializerDiscrete GetInstance(
            Converter<Leaf, SufficientStatistics> singleVariableClassFunction,
            DistributionDiscrete discreteDistribution, 
            int[] fisherCounts, 
            IEnumerable<Leaf> fullLeafCollection)
        { 
            List<Converter<Leaf, SufficientStatistics>> emptyList = new List<Converter<Leaf, SufficientStatistics>>();
            return new MessageInitializerDiscrete(emptyList, singleVariableClassFunction, discreteDistribution, fisherCounts, null, fullLeafCollection);
        }

        public static MessageInitializerDiscrete GetInstance(
            Converter<Leaf, SufficientStatistics> singleVariableClassFunction, 
            DistributionDiscrete discreteDistribution, 
            OptimizationParameterList initParams,
            IEnumerable<Leaf> fullLeafCollection) 
        {
            List<Converter<Leaf, SufficientStatistics>> emptyList = new List<Converter<Leaf, SufficientStatistics>>();
            return new MessageInitializerDiscrete(emptyList, singleVariableClassFunction, discreteDistribution, null, initParams, fullLeafCollection);
        }

        public static MessageInitializerDiscrete GetInstance( 
            Converter<Leaf, SufficientStatistics> predictorClassFunction, 
            Converter<Leaf, SufficientStatistics> targetClassFunction,
            DistributionDiscrete discreteDistribution, 
            int[] fisherCounts,
            IEnumerable<Leaf> fullLeafCollection)
        {
            return new MessageInitializerDiscrete(SpecialFunctions.CreateSingletonList(predictorClassFunction), targetClassFunction, discreteDistribution, fisherCounts, null, fullLeafCollection);
        }
 
        public static MessageInitializerDiscrete GetInstance( 
            Converter<Leaf, SufficientStatistics> predictorClassFunction,
            Converter<Leaf, SufficientStatistics> targetClassFunction, 
            DistributionDiscrete discreteDistribution,
            OptimizationParameterList initParams,
            IEnumerable<Leaf> fullLeafCollection)
        {
            return new MessageInitializerDiscrete(SpecialFunctions.CreateSingletonList(predictorClassFunction), targetClassFunction, discreteDistribution, null, initParams, fullLeafCollection);
        } 
 
        public override IMessage InitializeMessage(Leaf leaf, OptimizationParameterList discreteParameters)
        { 
            int stateCount = DiscreteDistribution.NonMissingClassCount;
            double[] p = new double[stateCount];
            if (IsMissing(leaf))
            {
                //If missing data, return 1 to ignore this branch.
                for (int iParentState = 0; iParentState < stateCount; ++iParentState) 
                { 
                    p[iParentState] = 1.0;
                } 
            }
            else
            {
                double[][] dist = DiscreteDistribution.CreateDistribution(leaf, discreteParameters, LeafToPredictorStatistics);
                int distnClass = (DiscreteStatistics)LeafToTargetStatistics(leaf);
                for (int iParentState = 0; iParentState < stateCount; ++iParentState) 
                { 
                    p[iParentState] = dist[iParentState][distnClass];
                } 
            }



            MessageDiscrete message = MessageDiscrete.GetInstance(p);
            return message; 
        } 

        public override OptimizationParameterList GetOptimizationParameters() 
        {
            return DiscreteDistribution.GetParameters(_fisherCounts, _initializationParams);
        }
    }
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
