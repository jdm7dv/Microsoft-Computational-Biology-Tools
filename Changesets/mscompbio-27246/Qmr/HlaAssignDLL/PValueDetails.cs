using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using Msr.Mlas.Qmr;
using VirusCount.Qmr; //!!! what's the difference between Msr.Mlas.Qmr; and this? 
using Optimization; 
using EpipredLib;
 
namespace VirusCount.Qmrr
{

    //!!!PValueDetails ideas: two parts, one for each run of OptimizationParameterList. have those subparts also be used in caching. Also report causePrior.
    //!!!PreviousParams is not a good name
 
    public class PValueDetails 
    {
        private PValueDetails() 
        {
        }

        public double Diff;
        public Set<Hla> KnownHlas;
        public Set<Hla> BestHlaSetSoFar; 
        public double LeakProbability; 
        public double LinkProbability;
        public double Score1; 
        public double Score2;
        public OptimizationParameterList PreviousParams;
        public int NullIndex;
        public string Peptide;
        public Hla Hla;
        public string SelectionName; 
 
        internal static PValueDetails GetInstance(
            string selectionName, 
            int nullIndex, string peptide, Hla hla,
            double score1, double score2, Set<Hla> knownHlas, Set<Hla> bestHlaSetSoFar, double leakProbability, double linkProbability, OptimizationParameterList previousParams)
        {
            PValueDetails pValueDetails = new PValueDetails();
            pValueDetails.SelectionName = selectionName;
            pValueDetails.NullIndex = nullIndex; 
            pValueDetails.Peptide = peptide; 
            pValueDetails.Hla = hla;
            pValueDetails.Score1 = score1; 
            pValueDetails.Score2 = score2;
            pValueDetails.Diff = score1 - score2;
            pValueDetails.KnownHlas = knownHlas;
            pValueDetails.BestHlaSetSoFar = bestHlaSetSoFar;
            pValueDetails.LeakProbability = leakProbability;
            pValueDetails.LinkProbability = linkProbability; 
            pValueDetails.PreviousParams = previousParams; 
            return pValueDetails;
        } 

        static SpecialFunctions SpecialFunctions = SpecialFunctions.GetInstance();


        public double PValue()
        { 
            double pValue = SpecialFunctions.LogLikelihoodRatioTest(Math.Max(0, Diff), 1); 
            return pValue;
 
        }

        public static string Header = SpecialFunctions.CreateTabString("selection", "NullIndex", "peptide", "hla", "score1", "score2", "diff", "PValue", "knownHlas", "bestHlaSetSoFar", "leakProbability", "linkProbability");
        public override string ToString()
        {
            return SpecialFunctions.CreateTabString( 
                SelectionName, NullIndex, Peptide, Hla, Score1, Score2, 
                Diff, PValue(),
                SpecialFunctions.Join(",", KnownHlas), 
                BestHlaSetSoFar==null? null : SpecialFunctions.Join(",", BestHlaSetSoFar),
                LeakProbability, LinkProbability);
        }
    }
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
