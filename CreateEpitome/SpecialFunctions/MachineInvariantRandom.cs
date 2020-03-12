using System; 
using System.Collections.Generic;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{
    public class MachineInvariantRandom : Random 
    { 
        private const uint Salt = unchecked((uint) -823483423);
        private const uint AfterWord = 724234833; 

        private MachineInvariantRandom()
        {
        }

        /// <summary> 
        /// Gives same result on 64-bit and 32-bit machines (unlike string's GetHashCode). Ignore's case 
        /// </summary>
        public MachineInvariantRandom(params string[] seedStringArray) : this(Salt, seedStringArray) 
        {
        }

         //<summary>
         //Gives same result on 64-bit and 32-bit machines (unlike string's GetHashCode). Ignore's case
         //</summary> 
        public MachineInvariantRandom(uint seedUInt, params string[] seedStringArray) : base((int)GetSeedUInt(seedUInt, seedStringArray)) 
        {
        } 


        public static uint GetSeedUInt(params string[] seedStringArray)
        {
            return GetSeedUInt(Salt, seedStringArray);
        } 
 

        public static uint GetSeedUInt(uint seedUInt, params string[] seedStringArray) 
        {
            foreach (string seedString in seedStringArray)
            {
                foreach (char c in seedString)
                {
                    //xor the rightrotated seed with the uppercase character 
                    seedUInt = (((seedUInt >> 1) | ((seedUInt & 1) << 31)) ^ ((uint) char.ToUpper(c).GetHashCode())); 
                }
                //After each word, do a right rotate to separate words 
                seedUInt = ((seedUInt >> 1) | ((seedUInt & 1) << 31)) ^ AfterWord;
            }
            return seedUInt;
        }

    } 
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
