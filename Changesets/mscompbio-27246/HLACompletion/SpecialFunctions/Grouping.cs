using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{ 
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement> 
    {
        private Grouping() 
        {
        }

        static public Grouping<TKey, TElement> GetInstance(TKey key, IEnumerator<TElement> enumerator)
        {
            Grouping<TKey, TElement> grouping = new Grouping<TKey, TElement>(); 
            grouping.Key = key; 
            grouping._enumerator = enumerator;
            return grouping; 
        }

        public TKey Key { get; private set; }

        private IEnumerator<TElement> _enumerator;
        public IEnumerator<TElement> GetEnumerator() 
        { 
            return _enumerator;
        } 


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _enumerator;
        } 
 
    }
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
