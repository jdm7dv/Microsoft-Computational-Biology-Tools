using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{ 
    public static class IEnumerableExtensions 
    {

        //public static IEnumerable<T> AsParallel<T>(this IEnumerable<T> list)
        //{
        //    return list;
        //} 

        public static string StringJoin<T>(this IEnumerable<T> list) 
        {
            StringBuilder sb = new StringBuilder();
            foreach (object obj in list)
            {
                if (obj == null)
                { 
                    sb.Append("null"); 
                }
                else 
                {
                    sb.Append(obj.ToString());
                }
            }
            return sb.ToString();
        } 
 
        public static string StringJoin<T>(this IEnumerable<T> list, string separator)
        { 
            StringBuilder aStringBuilder = new StringBuilder();
            bool isFirst = true;
            foreach (object obj in list)
            {
                if (!isFirst)
                { 
                    aStringBuilder.Append(separator); 
                }
                else 
                {
                    isFirst = false;
                }

                if (obj == null)
                { 
                    aStringBuilder.Append("null"); 
                }
                else 
                {
                    aStringBuilder.Append(obj.ToString());
                }
            }
            return aStringBuilder.ToString();
        } 
 
        public static string StringJoin<T>(this IEnumerable<T> list, string separator, int maxLength, string etcString)
        { 
            SpecialFunctions.CheckCondition(maxLength >= 2, "maxLength must be at least 2");
            StringBuilder aStringBuilder = new StringBuilder();
            int i = -1;
            foreach (object obj in list)
            {
                ++i; 
                if (i > 1) 
                {
                    aStringBuilder.Append(separator); 
                }

                if (i >= maxLength)
                {
                    aStringBuilder.Append(etcString);
                    break; // really break, not continue; 
                } 
                else if (obj == null)
                { 
                    aStringBuilder.Append("null");
                }
                else
                {
                    aStringBuilder.Append(obj.ToString());
                } 
            } 
            return aStringBuilder.ToString();
        } 


        //public static HashSet<T> ToHashSet<T>(this IEnumerable<T> list)
        //{
        //    return new HashSet<T>(list);
        //} 
 
        public static Dictionary<T1, T2> ToDictionary<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> pairList)
        { 
            return pairList.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
