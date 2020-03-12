using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using Optimization;

namespace Optimization 
{ 

    public class OptimizationParameterList : IEnumerable<OptimizationParameter> 
    {
        private Dictionary<string, OptimizationParameter> _asDictionary = new Dictionary<string, OptimizationParameter>();
        private List<OptimizationParameter> _asParameterList = new List<OptimizationParameter>();
        private List<int> _enumerationOrder;
        private Predicate<OptimizationParameterList> CheckConditions = null;
 
        private OptimizationParameterList() 
        {
        } 

        public OptimizationParameter this[string name]
        {
            get
            {
                return _asDictionary[name]; 
            } 
        }
 
        public OptimizationParameter this[int index]
        {
            get
            {
                return _asParameterList[index];
            } 
        } 

 
        public IEnumerable<string> Names
        {
            get
            {
                foreach (OptimizationParameter param in this)
                { 
                    yield return param.Name; 
                }
            } 
        }

        public IEnumerable<double> Values
        {
            get
            { 
                foreach (OptimizationParameter param in this) 
                {
                    yield return param.Value; 
                }
            }
        }

        public int Count
        { 
            get 
            {
                return _asParameterList.Count; 
            }
        }



        public static OptimizationParameterList GetInstance(params OptimizationParameter[] parameterCollection) 
        { 
            return GetInstance2(parameterCollection);
        } 


        public static OptimizationParameterList GetInstance2(IEnumerable<OptimizationParameter> parameterCollection)
        {
            OptimizationParameterList aParamsList = new OptimizationParameterList();
            foreach (OptimizationParameter parameter in parameterCollection) 
            { 
                aParamsList._asDictionary.Add(parameter.Name, parameter);
                aParamsList._asParameterList.Add(parameter); 
            }

            // no longer requires sort.
            ////Must do this after because the parameters may get re-ordered.
            //aQmrrParams._asParameterArray = new OptimizationParameter[aQmrrParams._asSortedDictionary.Count];
            //int iParam = -1; 
            //foreach (OptimizationParameter parameter in aQmrrParams._asSortedDictionary.Values) 
            //{
            //    ++iParam; 
            //    aQmrrParams._asParameterArray[iParam] = parameter;
            //}

            return aParamsList;
        }
 
        public void SetCheckConditions(Predicate<OptimizationParameterList> checkConditionsFunction) 
        {
            CheckConditions = checkConditionsFunction; 
        }

        /// <summary>
        /// Returns true iff the current settings of this parameter list satisfies the conditions given by CheckConditions.
        /// </summary>
        public bool SatisfiesConditions() 
        { 
            return CheckConditions == null || CheckConditions(this);
        } 

        public bool DoSearch(int parameterIndex)
        {
            return _asParameterList[parameterIndex].DoSearch;
        }
 
        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder(); 
            foreach (OptimizationParameter param in this)
            {
                if (sb.Length > 0)
                    sb.Append("\t");
                sb.Append(param.Value);
                //if (param.Low >= 0 && param.High > 1) 
                //    sb.Append("\t" + Math.Log(param.Value)); 
            }
            return sb.ToString(); 
        }

        public int CountFreeParameters()
        {
            int i = 0;
            foreach (OptimizationParameter param in _asParameterList) 
            { 
                if (param.DoSearch)
                { 
                    i++;
                }
            }
            return i;
        }
 
        public string ToStringHeader() 
        {
            StringBuilder sb = new StringBuilder(); 
            foreach (OptimizationParameter param in this)
            {
                if (sb.Length > 0)
                    sb.Append("\t");
                sb.Append(param.Name);
                //if (param.Low >= 0 && param.High > 1) 
                //    sb.Append("\tLog" + param.Name); 
            }
            return sb.ToString(); 
        }

        public bool IsClose(OptimizationParameterList other, double eps)
        {
            if (other == null)
            { 
                return false; 
            }
 
            SpecialFunctions.CheckCondition(other.Count == Count);

            for (int iParam = 0; iParam < Count; ++iParam)
            {
                if (!IsClose(_asParameterList[iParam], other._asParameterList[iParam], eps))
                { 
                    return false; 
                }
            } 
            return true;
        }

        private bool IsClose(OptimizationParameter myValue, OptimizationParameter otherValue, double eps)
        {
            return Math.Abs(myValue.Value - otherValue.Value) < eps; 
        } 

 
        public OptimizationParameterList Clone()
        {
            List<OptimizationParameter> parameterCollection = new List<OptimizationParameter>();
            foreach (OptimizationParameter parameter in _asParameterList)
            {
                parameterCollection.Add(parameter.Clone()); 
            } 
            OptimizationParameterList result = GetInstance2(parameterCollection);
            if (_enumerationOrder != null) 
            {
                result.SetEnumerationOrder(_enumerationOrder);
            }
            return result;
        }
 
        public List<double> ExtractParameterValueListForSearch() 
        {
            List<double> parameterValueListForSearch = new List<double>(Count); 
            foreach(OptimizationParameter param in this)
            {
                parameterValueListForSearch.Add(param.ValueForSearch);
            }
            return parameterValueListForSearch;
        } 
        public List<double> ExtractParameterLowListForSearch() 
        {
            List<double> parameterLowListForSearch = new List<double>(Count); 
            foreach (OptimizationParameter param in this)
            {
                parameterLowListForSearch.Add(param.LowForSearch);
            }
            return parameterLowListForSearch;
        } 
        public List<double> ExtractParameterHighListForSearch() 
        {
            List<double> parameterHighListForSearch = new List<double>(Count); 
            foreach (OptimizationParameter param in this)
            {
                parameterHighListForSearch.Add(param.HighForSearch);
            }
            return parameterHighListForSearch;
        } 
 
        public OptimizationParameterList CloneWithNewValuesForSearch(List<double> point)
        { 
            List<OptimizationParameter> parameterCollection = new List<OptimizationParameter>();
            for (int iParam = 0; iParam < Count; ++iParam)
            {
                OptimizationParameter clone = _asParameterList[iParam].Clone();
                clone.ValueForSearch = point[iParam];
                parameterCollection.Add(clone); 
            } 
            return GetInstance2(parameterCollection);
        } 

        #region IEnumerable<OptimizationParameter> Members

        public IEnumerator<OptimizationParameter> GetEnumerator()
        {
            if (_enumerationOrder == null) 
            { 
                foreach (OptimizationParameter param in _asParameterList)
                { 
                    yield return param;
                }
            }
            else
            {
                foreach (int index in _enumerationOrder) 
                { 
                    yield return _asParameterList[index];
                } 
            }
        }

        #endregion

        #region IEnumerable Members 
 
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        { 
            if (_enumerationOrder == null)
            {
                foreach (OptimizationParameter param in _asParameterList)
                {
                    yield return param;
                } 
            } 
            else
            { 
                foreach (int index in _enumerationOrder)
                {
                    yield return _asParameterList[index];
                }
            }
        } 
 
        #endregion
 
        /// <summary>
        /// Inserts optimizedParameter at the specified index.
        /// </summary>
        public void Add(int idx, OptimizationParameter optimizationParameter)
        {
            while (_asParameterList.Count <= idx) 
                _asParameterList.Add(null); 
            _asParameterList[idx] = optimizationParameter;
            _asDictionary.Add(optimizationParameter.Name, optimizationParameter); 
        }

        public void SetEnumerationOrder(IEnumerable<int> searchOrder)
        {
            _enumerationOrder = new List<int>(_asParameterList.Count);
            foreach (int i in searchOrder) 
            { 
                SpecialFunctions.CheckCondition(i >= 0 && i < _asParameterList.Count);
                _enumerationOrder.Add(i); 
            }
        }

        public bool ContainsKey(string parameterName)
        {
            return _asDictionary.ContainsKey(parameterName); 
        } 

        /// <summary> 
        /// Sets the values of the this ParameterList's parameters to the given values.
        /// The values must be in the same order as returned by the indexer.
        /// </summary>
        /// <param name="parameters"></param>
        public void SetParameterValues(double[] parameters)
        { 
            SpecialFunctions.CheckCondition(parameters.Length == _asParameterList.Count, "Wrong number of parameters."); 

            for (int i = 0; i < parameters.Length; i++) 
            {
                this[i].Value = parameters[i];
            }
        }
    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
