using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msr.Mlas.SpecialFunctions;
//using EpipredLib;
 
namespace Msr.Linkdis 
{
    public class PhasedExpansion 
    {
        private PhasedExpansion()
        {
        }

        //!!should have A,B,C as input 
        public static string Header = SpecialFunctions.CreateTabString("A1", "B1", "C1", "A2", "B2", "C2", "probability of completion", "lower-resolution model used"); 

        public static string TooManyCombinationsMessage() 
        {
            return SpecialFunctions.CreateTabString("ERROR: Too many combinations. Case skipped", "", "", "", "", "", "", "1");
        }

        UOPair<LinkedList1<HlaMsr1>> Phase;
        double Prob; 
        bool UsedLowerResModel; 
        public string BadHlaNameOrNull;
 
        internal static PhasedExpansion GetInstance(UOPair<LinkedList1<HlaMsr1>> phase, double prob, bool usedLowerResModel, string badHlaNameOrNull)
        {
            PhasedExpansion phasedExpansion = new PhasedExpansion();
            phasedExpansion.Phase = phase;
            phasedExpansion.Prob = prob;
            phasedExpansion.UsedLowerResModel = usedLowerResModel; 
            phasedExpansion.BadHlaNameOrNull = badHlaNameOrNull; 
            return phasedExpansion;
        } 

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var half in Phase)
            { 
                foreach (var hla in half.Reverse()) 
                {
                    sb.AppendFormat("{0}\t", hla.ToString(/*withParen*/ true)); 
                }
            }
            sb.AppendFormat("{0}\t", Prob);
            if (null == BadHlaNameOrNull)
            {
                sb.Append(UsedLowerResModel ? 1 : 0); 
            } 
            else
            { 
                sb.AppendFormat("Error: Allele '{0}' not found in main or lower-resolution models", BadHlaNameOrNull);
            }
            return sb.ToString();

        }
 
 

    } 
}


// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
