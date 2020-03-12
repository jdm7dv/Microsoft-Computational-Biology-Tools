using System; 
using System.Collections.Generic;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{
    public class Pair<T1, T2> 
    { 
        public T1 First;
        public T2 Second; 

        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        } 
 
        //Parameterless constructor needed to use by GetValueOrDefault
        public Pair() 
        {
            First = default(T1);
            Second = default(T2);
        }

 
        public override bool Equals(object obj) 
        {
            Pair<T1,T2> other = obj as Pair<T1,T2>; 
            if(null == other)
                return false;

            return First.Equals(other.First) && Second.Equals(other.Second);
        }
 
        public override int GetHashCode() 
        {
            return First.GetHashCode() ^ Second.GetHashCode() ^ "Pair".GetHashCode() ^ typeof(T1).GetHashCode() ^ typeof(T2).GetHashCode(); 
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", First.ToString(), Second.ToString());
        } 
 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
