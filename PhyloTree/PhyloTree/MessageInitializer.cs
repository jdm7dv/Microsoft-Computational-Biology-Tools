using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
 
namespace VirusCount.PhyloTree 
{
 
    public abstract class MessageInitializer
    {
        private readonly List<Converter<Leaf, SufficientStatistics>> _leafToPredictorClassList;
        private readonly Converter<Leaf, SufficientStatistics> _leafToTargetClass;
        private readonly IDistribution _distribution;
        private readonly int _hashCode; 
        private readonly IEnumerable<Leaf> _fullLeafCollection; 

        protected MessageInitializer( 
            List<Converter<Leaf, SufficientStatistics>> predictorClassFunctionList,
            Converter<Leaf, SufficientStatistics> targetClassFunction,
            IDistribution distribution,
            IEnumerable<Leaf> fullLeafCollection)
        {
            _leafToPredictorClassList = predictorClassFunctionList; 
            _leafToTargetClass = targetClassFunction; 
            _distribution = distribution;
            _fullLeafCollection = fullLeafCollection; 
            _hashCode = ComputeHashCode();
        }

        protected List<Converter<Leaf, SufficientStatistics>> LeafToPredictorStatisticsList
        {
            get { return _leafToPredictorClassList; } 
        } 
        protected Converter<Leaf, SufficientStatistics> LeafToPredictorStatistics
        { 
            get { return _leafToPredictorClassList.Count == 0 ? null : _leafToPredictorClassList[0]; }
        }
        protected Converter<Leaf, SufficientStatistics> LeafToTargetStatistics
        {
            get { return _leafToTargetClass; }
        } 
 
        public IDistribution PropogationDistribution
        { 
            get { return _distribution; }
        }

        public bool IsMissing(Leaf leaf)
        {
            bool isMissing = false; 
            foreach (Converter<Leaf, SufficientStatistics> map in LeafToPredictorStatisticsList) 
            {
                isMissing = isMissing || map(leaf).IsMissing(); 
            }

            return LeafToTargetStatistics(leaf).IsMissing() || isMissing;
        }

        public abstract IMessage InitializeMessage(Leaf leaf, OptimizationParameterList optimizableParameters); 
 
        public abstract OptimizationParameterList GetOptimizationParameters();
 

        public override int GetHashCode()
        {
            return _hashCode;
        }
 
        public override bool Equals(object obj) 
        {
            MessageInitializer other = obj as MessageInitializer; 
            if (other == null ||
                _hashCode != other._hashCode ||
                _distribution.DependsOnMoreThanOneVariable != other._distribution.DependsOnMoreThanOneVariable ||
                _distribution.ToString() != other._distribution.ToString()
                )
            { 
                return false; 
            }
 
            foreach (Leaf leaf in _fullLeafCollection)
            {
                if (IsMissing(leaf) != other.IsMissing(leaf) ||
                    LeafToTargetStatistics(leaf) != other.LeafToTargetStatistics(leaf))
                {
                    return false; 
                } 
                // if these distributions depend on the predictor variables, then make sure they all match up.
                if (_distribution.DependsOnMoreThanOneVariable) 
                {
                    foreach (KeyValuePair<Converter<Leaf, SufficientStatistics>, Converter<Leaf, SufficientStatistics>> predMapPair in SpecialFunctions.EnumerateTwo(LeafToPredictorStatisticsList, other.LeafToPredictorStatisticsList))
                    {
                        if (predMapPair.Key(leaf) != predMapPair.Value(leaf))
                        {
                            return false; 
                        } 
                    }
                } 
            }

            return true;
        }

 
        private int ComputeHashCode() 
        {
            int hashCode = "seed".GetHashCode(); 

            hashCode ^= _distribution.ToString().GetHashCode();


            foreach (Leaf leaf in _fullLeafCollection)
            { 
                string classString = IsMissing(leaf) ? "Missing" : 
                    _leafToTargetClass(leaf).ToString();
 
                // only record the specifics of the predictor variables if this distribution actually depends on them (eg, single variable only cares if missing)
                if (_distribution.DependsOnMoreThanOneVariable)
                {
                    foreach (Converter<Leaf, SufficientStatistics> predMap in _leafToPredictorClassList)
                    {
                        classString += predMap(leaf).ToString(); 
                    } 
                }
 
                string name = leaf.CaseName;
                Debug.Assert(name != null); // real assert
                hashCode ^= (leaf.CaseName + classString).GetHashCode();
            }
            return hashCode;
        } 
    } 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
