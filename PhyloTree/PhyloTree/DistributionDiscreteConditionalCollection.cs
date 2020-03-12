//using System; 
//using System.Collections.Generic;
//using System.Text;

//namespace VirusCount.PhyloTree
//{
//    public abstract class DistributionDiscreteConditionalCollection : IDistributionCollection, IDistributionDiscrete 
//    { 

//        List<IDistributionDiscrete> _distributions; 
//        IDistributionDiscrete _currentBestDistn;

//        protected DistributionDiscreteConditionalCollection(params DistributionDiscreteBinary[] distns)
//        {
//            _distributions = new List<IDistributionDiscrete>(distns);
//            _currentBestDistn = distns[0]; 
//        } 

//        public static bool TryGetInstance(string oneDirectionOrBothDirections, out DistributionDiscreteConditionalCollection distribution) 
//        {
//            switch (oneDirectionOrBothDirections)
//            {
//                case DistributionDiscreteConditionalCollectionOneDirection.Name:
//                    distribution = new DistributionDiscreteConditionalCollectionOneDirection();
//                    return true; 
//                default: 
//                    distribution = null;
//                    return false; 
//            }
//        }

//        public static IDistributionCollection GetInstance(string oneDirectionOrBothDirections)
//        {
//            switch (oneDirectionOrBothDirections) 
//            { 
//                case DistributionDiscreteConditionalCollectionOneDirection.Name:
//                    return new DistributionDiscreteConditionalCollectionOneDirection(); 
//                default:
//                    throw new ArgumentException(oneDirectionOrBothDirections + " is not a known conditional distribution collection name.");
//            }
//        }

//        #region IDistributionCollection Members 
 
//        public IDistribution BestDistribution
//        { 
//            get
//            {
//                return _currentBestDistn;
//            }
//            set
//            { 
//                _currentBestDistn = (DistributionDiscreteBinary)value; 
//            }
//        } 

//        public IDistribution SingleVariableDistribution
//        {
//            get { return _currentBestDistn; }
//        }
 
 
//        public IEnumerable<MessageInitializer> EnumerateMessageInitializers(MessageInitializer messageInitializer)
//        { 
//            foreach (DistributionDiscreteBinary distn in _distributions)
//            {
//                MessageInitializerDiscrete miWithNewDistn = (MessageInitializerDiscrete)messageInitializer.Clone();
//                miWithNewDistn.PropogationDistribution = distn;
//                yield return miWithNewDistn;
//            } 
//        } 

//        #endregion 

//        #region ICloneable Members

//        public object Clone()
//        {
//            return MemberwiseClone(); 
//        } 

//        #endregion 

//        #region IEnumerable<IDistribution> Members

//        public IEnumerator<IDistribution> GetEnumerator()
//        {
//            foreach (DistributionDiscreteBinary distn in _distributions) 
//            { 
//                yield return distn;
//            } 
//        }

//        #endregion

//        #region IEnumerable Members
 
//        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() 
//        {
//            foreach (DistributionDiscreteBinary distn in _distributions) 
//            {
//                yield return distn;
//            }
//        }

//        #endregion 
 
//        #region IDistribution Members
 
//        public Optimization.OptimizationParameterList InitialParamVals
//        {
//            get
//            {
//                return BestDistribution.InitialParamVals;
//            } 
//            set 
//            {
//                foreach (IDistribution distn in _distributions) 
//                {
//                    distn.InitialParamVals = value;
//                }
//            }
//        }
 
//        public Optimization.OptimizationParameterList GetParameters(double[] parameterValuesInPrintOrder) 
//        {
//            return BestDistribution.GetParameters(parameterValuesInPrintOrder); 
//        }

//        public Optimization.OptimizationParameterList GetParameters(bool useConditionalParameter)
//        {
//            return BestDistribution.GetParameters(useConditionalParameter);
//        } 
 
//        public Optimization.OptimizationParameterList GetParameters(bool useConditionalParameter, int[] counts)
//        { 
//            return _currentBestDistn.GetParameters(useConditionalParameter, counts);
//        }

//        public bool UsePredictorVariable(bool useParameter)
//        {
//            return BestDistribution.UsePredictorVariable(useParameter); 
//        } 

//        #endregion 

//        #region IDistributionDiscrete Members

//        public int NonMissingClassCount
//        {
//            get { return _currentBestDistn.NonMissingClassCount; } 
//        } 

//        public double EmpiricalEquilibrium 
//        {
//            get
//            {
//                return _currentBestDistn.EmpiricalEquilibrium;
//            }
//            set 
//            { 
//                foreach (IDistributionDiscrete distn in _distributions)
//                { 
//                    distn.EmpiricalEquilibrium = value;
//                }
//            }
//        }

//        public double[][] CreateDistribution(BranchOrLeaf branchOrLeaf, Optimization.OptimizationParameterList discreteParameters, Converter<Leaf, SufficientStatistics> predictorClassFunction) 
//        { 
//            return _currentBestDistn.CreateDistribution(branchOrLeaf, discreteParameters, predictorClassFunction);
//        } 

//        public double[][] CreateDistribution(Branch branch, Optimization.OptimizationParameterList discreteParameters)
//        {
//            return _currentBestDistn.CreateDistribution(branch, discreteParameters);
//        }
 
//        public double[][] CreateDistribution(Leaf leaf, Optimization.OptimizationParameterList discreteParameters, Converter<Leaf, SufficientStatistics> predictorClassFunction) 
//        {
//            return _currentBestDistn.CreateDistribution(leaf, discreteParameters, predictorClassFunction); 
//        }

//        public double[] GetPriorProbabilities(Optimization.OptimizationParameterList discreteParameters)
//        {
//            return _currentBestDistn.GetPriorProbabilities(discreteParameters);
//        } 
 
//        public string GetParameterHeaderString()
//        { 
//            return _currentBestDistn.GetParameterHeaderString();
//        }

//        public string GetParameterHeaderString(string modifier)
//        {
//            return _currentBestDistn.GetParameterHeaderString(modifier); 
//        } 

//        public string GetParameterValueString(Optimization.OptimizationParameterList parameters) 
//        {
//            return _currentBestDistn.GetParameterValueString(parameters);
//        }

//        public void ReflectClassInDictionaries(DiscreteStatistics discreteStatistics, Leaf leaf,
//            ref Dictionary<string, BooleanStatistics> predictorMapToCreate, ref Dictionary<string, BooleanStatistics> targetMapToCreate) 
//        { 
//            _currentBestDistn.ReflectClassInDictionaries(discreteStatistics, leaf, ref predictorMapToCreate, ref targetMapToCreate);
//        } 

//        #endregion

//        #region object override methods
//        public override string ToString()
//        { 
//            return _currentBestDistn.ToString(); 
//        }
 
//        public override bool Equals(object obj)
//        {
//            DistributionDiscreteConditionalCollection other = obj as DistributionDiscreteConditionalCollection;
//            if (other == null || obj.GetType() != this.GetType())
//                return false;
//            foreach (IDistributionDiscrete distn in _distributions) 
//            { 
//                if (!other._distributions.Contains(distn))
//                { 
//                    return false;
//                }
//            }
//            return true;
//        }
 
//        public override int GetHashCode() 
//        {
//            int hashCode = this.GetType().GetHashCode(); 
//            foreach (IDistribution distn in _distributions)
//            {
//                hashCode ^= distn.GetHashCode();
//            }
//            return hashCode;
//        } 
//        #endregion 
//    }
 
//    class DistributionDiscreteConditionalCollectionOneDirection : DistributionDiscreteConditionalCollection
//    {
//        public const string Name = "OneDirection";

//        public DistributionDiscreteConditionalCollectionOneDirection()
//            : base( 
//            DistributionDiscreteBinary.GetInstance("Attraction"), 
//            DistributionDiscreteBinary.GetInstance("Escape"),
//            DistributionDiscreteBinary.GetInstance("Repulsion"), 
//            DistributionDiscreteBinary.GetInstance("Reversion")) { }
//    }
//}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
