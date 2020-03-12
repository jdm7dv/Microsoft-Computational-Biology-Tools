using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Msr.Mlas.SpecialFunctions
{

    public struct Ignore
    {
    }


    public class HashSet<T> : IEnumerable<T>
    {
        Dictionary<T, Ignore> Dictionary = new Dictionary<T, Ignore>();
        Ignore Ignore = new Ignore();

        public void AddNew(T t)
        {
            Dictionary.Add(t, Ignore);
        }

        public int Count
        {
            get
            {
                return Dictionary.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T t in Dictionary.Keys)
            {
                yield return t;
            }
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        internal bool Add(T t)
        {
            if (Dictionary.ContainsKey(t))
            {
                return false;
            }
            else
            {
                Dictionary.Add(t, Ignore);
                return true;
            }
        }

        public void Remove(T t)
        {
            Dictionary.Remove(t);
        }

        public void CopyTo(T[] array, int i)
        {
            foreach (T t in Dictionary.Keys)
            {
                array[i] = t;
                ++i;
            }
        }

    }
}
// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
