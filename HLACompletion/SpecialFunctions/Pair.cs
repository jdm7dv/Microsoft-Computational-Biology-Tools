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
 
        static int pairStringHashCode = (int) MachineInvariantRandom.GetSeedUInt("Pair"); 

        /// <summary> 
        /// Depending on the subtypes, the hash code may be different on 32-bit and 64-bit machines
        /// </summary>
        public override int GetHashCode()
        {
            return pairStringHashCode
                ^ First.GetHashCode() ^ typeof(T1).GetHashCode() 
                ^ SpecialFunctions.WrapAroundLeftShift(Second.GetHashCode(),1) ^ typeof(T2).GetHashCode(); //Do the shift so that <"A","B"> hashs different than <"B","A"> 
        }
 
        public override string ToString()
        {
            return string.Format("({0}, {1})", First, Second);
        }

 
    } 
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
