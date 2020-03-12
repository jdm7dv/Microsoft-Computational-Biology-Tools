using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{ 
 
    public static class DictionaryExtensions
    { 
        /// <summary>
        /// Returns the value associated with key in the dictionary. If not present, adds the default value to the dictionary and returns that
        /// value.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        { 
            TValue value; 
            if (!dictionary.TryGetValue(key, out value))
            { 
                value = new TValue();	// create a default value and add it to the dictionary
                dictionary.Add(key, value);
            }
            return value;
        }
 
        /// <summary> 
        /// Returns the value associated with key in the dictionary. If not present, adds the default value to the dictionary and returns that
        /// value. 
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            { 
                dictionary.Add(key, defaultValue); 
                return defaultValue;
            } 
            else
            {
                return value;
            }
        }
 
        /// <summary> 
        /// Returns the value associated with key in the dictionary. If not present, adds the default value to the dictionary and returns that
        /// value. 
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue, bool insertIfMissing)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            { 
                if (insertIfMissing) 
                {
                    dictionary.Add(key, defaultValue); 
                }
                return defaultValue;
            }
            else
            {
                return value; 
            } 
        }
 
        public static HashSet<TKey> KeyIntersection<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keyList2)
        {
            HashSet<TKey> hashSet = new HashSet<TKey>();
            foreach (TKey tkey in keyList2)
            {
                if (dictionary.ContainsKey(tkey)) 
                { 
                    hashSet.Add(tkey);
                } 
            }
            return hashSet;
        }
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
