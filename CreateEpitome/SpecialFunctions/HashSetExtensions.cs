using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Msr.Mlas.SpecialFunctions
{



    public static class HashSetExtensions
    {
        public static void AddNew<T>(this HashSet<T> hashSet, T element)
        {
            bool didAdd = hashSet.Add(element);
            if (!didAdd)
            {
                throw new ArgumentException("Set already contains element: " + element.ToString());
            }
        }

        public static void AddNewRange<T>(this HashSet<T> hashSet, IEnumerable<T> iEnumerable)
        {
            foreach (T t in iEnumerable)
            {
                hashSet.AddNew(t);
            }
        }

        public static void AddNewOrOldRange<T>(this HashSet<T> hashSet, IEnumerable<T> iEnumerable)
        {
            foreach (T t in iEnumerable)
            {
                hashSet.Add(t);
            }
        }


        ////!!!Is SetEquals, for which the 2nd argument is IEnumerable, really as fast as an algorithm that knows they don't have duplicate values, checks length, and then checks that each element of one is in the other?
        //[Obsolete("Use 'SetEquals' instead")]
        //public static bool IsEqualTo<T>(this HashSet<T> hashSet1, HashSet<T> hashSet2)
        //{
        //    return hashSet1.SetEquals(hashSet2);
        //}

        public static HashSet<T> GetInstanceFromNewRange<T>(IEnumerable<T> iEnumerable)
        {
            HashSet<T> hashSet = new HashSet<T>();
            hashSet.AddNewRange(iEnumerable);
            return hashSet;
        }
    }
} 

//// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
//// Copyright (c) Microsoft Corporation. All rights reserved.
