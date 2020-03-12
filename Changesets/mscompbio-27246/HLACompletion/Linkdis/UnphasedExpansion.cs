using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msr.Mlas.SpecialFunctions;
//using EpipredLib;
 
namespace Msr.Linkdis 
{
    public class UnphasedExpansion 
    {
        //!!!Add a field to tell of any filters?
        //!!should have A,B,C as input
        //!!!OK to have multiword headers?
        public static string Header = SpecialFunctions.CreateTabString("A", "A", "B", "B", "C", "C", "probability of completion", "lower-resolution model used");
        //!!should have A,B,C as input 
        public static string TooManyCombinationsMessage() 
        {
            return SpecialFunctions.CreateTabString("ERROR: Too many combinations. Case skipped", "", "", "", "", "", "", "1"); 
        }


        LinkedList1<UOPair<HlaMsr1>> Unphrase;
        double Prob;
        bool UsedLowerResModel; 
        public string BadHlaNameOrNull; 

 
        internal static UnphasedExpansion GetInstance(LinkedList1<UOPair<HlaMsr1>> unphase, double prob, bool usedLowerResModel, string badHlaNameOrNull)
        {
            UnphasedExpansion unphasedExpansion = new UnphasedExpansion();
            unphasedExpansion.Unphrase = unphase;
            unphasedExpansion.Prob = prob;
            unphasedExpansion.UsedLowerResModel = usedLowerResModel; 
            unphasedExpansion.BadHlaNameOrNull = badHlaNameOrNull; 
            return unphasedExpansion;
        } 

        //!!Extracting class and number from HlaMsr1 should be member functions and not ad hoc
        //!!!really remove the A,B,C prefix?
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(); 
            foreach (var pair in Unphrase) 
            {
                List<string> items = new List<string> { pair.First.ToString(/*withParen*/ true), pair.Second.ToString(/*withParen*/ true) }; 
                items.Sort();
                sb.AppendFormat("{0}\t{1}\t", items[0], items[1]);
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
