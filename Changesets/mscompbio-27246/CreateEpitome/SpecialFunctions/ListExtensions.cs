using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{ 
    public static class ListExtensions 
    {
        /// <summary> 
        /// Returns the value associated with index in the list. If not present, adds instances of the default value to the list up to the index and then returns that value.
        /// </summary>
        public static T GetValueOrDefault<T>(this IList<T> list, int index) where T : new()
        {
            while(list.Count < index)
            { 
                list.Add(new T());	// create a default value and add it to the list 
            }
            if (list.Count == index) 
            {
                T value = new T();
                list.Add(value);
                return value;
            }
            else 
            { 
                return list[index];
            } 
        }
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
