using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{ 
    public static class RandomExtensions 
    {
        public static T NextListItem<T>(this Random random, IList<T> list) 
        {
            T t = list[random.Next(list.Count)];
            return t;
        }
    }
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
