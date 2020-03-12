using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using Optimization;

namespace VirusCount.Qmr 
{ 
    public class OptimizationParameterList
    { 
        private OptimizationParameterList()
        {
        }

        public OptimizationParameter this[string name]
        { 
            get 
            {
                return AsSortedDictionary[name]; 
            }
        }

        public SortedDictionary<string, OptimizationParameter> AsSortedDictionary;
        private OptimizationParameter[] AsParameterArray;
        public bool DoSearch(int parameterIndex) 
        { 
            return AsParameterArray[parameterIndex].DoSearch;
        } 

        public override string ToString()
        {
            StringBuilder aStringBuilder = new StringBuilder();
            foreach (OptimizationParameter p in AsParameterArray)
            { 
                if (aStringBuilder.Length > 0) 
                {
                    aStringBuilder.Append('\t'); 
                }
                aStringBuilder.Append(p.Value.ToString());
            }
            return aStringBuilder.ToString();
        }
 
        public string ToStringHeader() 
        {
            return SpecialFunctions.CreateTabString2(AsSortedDictionary.Keys); 
        }

        public int Count
        {
            get
            { 
                return AsParameterArray.Length; 
            }
        } 

        public bool Close(OptimizationParameterList other, double eps)
        {
            if (other == null)
            {
                return false; 
            } 

            SpecialFunctions.CheckCondition(other.Count == Count); 

            for (int iParam = 0; iParam < Count; ++iParam)
            {
                if (!Close(AsParameterArray[iParam], other.AsParameterArray[iParam], eps))
                {
                    return false; 
                } 
            }
            return true; 
        }

        private bool Close(OptimizationParameter myValue, OptimizationParameter otherValue, double eps)
        {
            return Math.Abs(myValue.Value - otherValue.Value) < eps;
        } 
 
        public static OptimizationParameterList GetInstance(params OptimizationParameter[] parameterCollection)
        { 
            return GetInstance2(parameterCollection);
        }

        public static OptimizationParameterList GetInstance2(IEnumerable<OptimizationParameter> parameterCollection)
        {
            OptimizationParameterList aQmrrParams = new OptimizationParameterList(); 
            aQmrrParams.AsSortedDictionary = new SortedDictionary<string, OptimizationParameter>(); 
            foreach (OptimizationParameter parameter in parameterCollection)
            { 
                aQmrrParams.AsSortedDictionary.Add(parameter.Name, parameter);
            }

            //Must do this after because the parameters may get re-ordered.
            aQmrrParams.AsParameterArray = new OptimizationParameter[aQmrrParams.AsSortedDictionary.Count];
            int iParam = -1; 
            foreach (OptimizationParameter parameter in aQmrrParams.AsSortedDictionary.Values) 
            {
                ++iParam; 
                aQmrrParams.AsParameterArray[iParam] = parameter;
            }
            return aQmrrParams;
        }

        public OptimizationParameterList Clone() 
        { 
            List<OptimizationParameter> parameterCollection = new List<OptimizationParameter>();
            foreach (OptimizationParameter parameter in AsParameterArray) 
            {
                parameterCollection.Add(parameter.Clone());
            }
            return GetInstance2(parameterCollection);
        }
 
        public List<double> ExtractParameterValueListForSearch() 
        {
            List<double> parameterValueListForSearch = new List<double>(Count); 
            for (int iParam = 0; iParam < Count; ++iParam)
            {
                parameterValueListForSearch.Add(AsParameterArray[iParam].ValueForSearch);
            }
            return parameterValueListForSearch;
        } 
        public List<double> ExtractParameterLowListForSearch() 
        {
            List<double> parameterLowListForSearch = new List<double>(Count); 
            for (int iParam = 0; iParam < Count; ++iParam)
            {
                parameterLowListForSearch.Add(AsParameterArray[iParam].LowForSearch);
            }
            return parameterLowListForSearch;
        } 
        public List<double> ExtractParameterHighListForSearch() 
        {
            List<double> parameterHighListForSearch = new List<double>(Count); 
            for (int iParam = 0; iParam < Count; ++iParam)
            {
                parameterHighListForSearch.Add(AsParameterArray[iParam].HighForSearch);
            }
            return parameterHighListForSearch;
        } 
 
        public OptimizationParameterList CloneWithNewValuesForSearch(List<double> point)
        { 
            List<OptimizationParameter> parameterCollection = new List<OptimizationParameter>();
            for (int iParam = 0; iParam < Count; ++iParam)
            {
                OptimizationParameter clone = AsParameterArray[iParam].Clone();
                clone.ValueForSearch = point[iParam];
                parameterCollection.Add(clone); 
            } 
            return GetInstance2(parameterCollection);
        } 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
