using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using EpipredLib;
using Msr.Mlas.SpecialFunctions;
 
namespace Msr.Linkdis 
{
    public class ExpansionCollection 
    {
        internal Dictionary<UOPair<LinkedList1<HlaMsr1>>, double> PhaseToLogProb;
        internal Dictionary<LinkedList1<UOPair<HlaMsr1>>, double> UnphaseToLogProb;
        internal double LogTotal;
        internal bool UsedLowerResModel;
        internal string BadHlaMsr1NameOrNull; 
 
        public PidAndHlaSet PidAndHlaSet { get; internal set; }
 
        public IEnumerable<PhasedExpansion> Phased()
        {
            foreach (var phaseAndLogProb in PhaseToLogProb)
            {
                yield return PhasedExpansion.GetInstance(phaseAndLogProb.Key, Math.Exp(phaseAndLogProb.Value - LogTotal), UsedLowerResModel, BadHlaMsr1NameOrNull);
            } 
        } 

        public IEnumerable<UnphasedExpansion> Unphased() 
        {
            foreach (var unphaseAndLogProb in UnphaseToLogProb)
            {
                yield return UnphasedExpansion.GetInstance(unphaseAndLogProb.Key, Math.Exp(unphaseAndLogProb.Value - LogTotal), UsedLowerResModel, BadHlaMsr1NameOrNull);
            }
        } 
 
        internal void Prenormalize(PidAndHlaSet pidAndHlaSet, Linkdis linkdis)
        { 
            PhaseToLogProb = new Dictionary<UOPair<LinkedList1<HlaMsr1>>, double>();
            UnphaseToLogProb = new Dictionary<LinkedList1<UOPair<HlaMsr1>>, double>();
            LogTotal = double.NegativeInfinity;
            BadHlaMsr1NameOrNull = null;
            UsedLowerResModel = false;
 
            //CounterWithMessages abstractPhaseCounter = CounterWithMessages.GetInstance("\tabstract phase index = {0}", 1, null); 

            try 
            {
                foreach (var phaseAbstract in pidAndHlaSet.GetPhasedEnumeration())
                {
                    //abstractPhaseCounter.Increment();

                    var firstHlaListToProb = linkdis.CreateHlaListToProb(phaseAbstract.First); 
                    var secondHlaListToProb = linkdis.CreateHlaListToProb(phaseAbstract.Second); 
                    if (firstHlaListToProb.Count * secondHlaListToProb.Count > linkdis.CombinationLimit)
                    { 
                        throw new CombinationLimitException("The combinationLimit was exceeded. " + linkdis.CombinationLimit.ToString());
                    }

                    CounterWithMessages groundPhaseCounter = CounterWithMessages.GetInstance("\t\tground phase index = {0}", 1000, null);
                    foreach (var firstHlaListAndProb in firstHlaListToProb)
                    { 
                        foreach (var secondHlaListAndProb in secondHlaListToProb) 
                        {
                            groundPhaseCounter.Increment(); 

                            var phaseGrounded = UOPair<LinkedList1<HlaMsr1>>.GetInstance(firstHlaListAndProb.Key, secondHlaListAndProb.Key);
                            var unphasedGrounded = MakeUnphased(phaseGrounded);

                            double prob = firstHlaListAndProb.Value.Key * secondHlaListAndProb.Value.Key;
                            UsedLowerResModel |= firstHlaListAndProb.Value.Value || secondHlaListAndProb.Value.Value; 
                            double logProb = Math.Log(prob); 

 
                            LogSum(PhaseToLogProb, phaseGrounded, logProb);
                            LogSum(UnphaseToLogProb, unphasedGrounded, logProb);
                            LogTotal = SpecialFunctions.LogSum(LogTotal, logProb);
                        }
                    }
                } 
            } 
            catch (HlaNotInModelException e)
            { 
                CreateNoAnswerAnswer(pidAndHlaSet, e);
            }

        }

        private void CreateNoAnswerAnswer(PidAndHlaSet pidAndHlaSet, HlaNotInModelException e) 
        { 
            PhaseToLogProb = new Dictionary<UOPair<LinkedList1<HlaMsr1>>, double>();
            UnphaseToLogProb = new Dictionary<LinkedList1<UOPair<HlaMsr1>>, double>(); 

            BadHlaMsr1NameOrNull = e.HlaName;
            UsedLowerResModel = true;

            var phaseGrounded = UOPair<LinkedList1<HlaMsr1>>.GetInstance(
                LinkedList1<HlaMsr1>.GetInstanceFromList(pidAndHlaSet.HlaUopairList.Select(pair => pair.First).ToList()), 
                LinkedList1<HlaMsr1>.GetInstanceFromList(pidAndHlaSet.HlaUopairList.Select(pair => pair.Second).ToList()) 
                );
            var unphasedGrounded = pidAndHlaSet.HlaUopairList; 

            double logProb = double.NaN;
            LogSum(PhaseToLogProb, phaseGrounded, logProb);
            LogSum(UnphaseToLogProb, unphasedGrounded, logProb);
        }
 
        private static void LogSum<T>(Dictionary<T, double> keyToLogSum, T key, double logProb) 
        {
            double runningLogTotal; 
            if (keyToLogSum.TryGetValue(key, out runningLogTotal))
            {
                keyToLogSum[key] = SpecialFunctions.LogSum(runningLogTotal, logProb);
            }
            else
            { 
                keyToLogSum[key] = logProb; 
            }
        } 

        private static LinkedList1<UOPair<HlaMsr1>> MakeUnphased(UOPair<LinkedList1<HlaMsr1>> phaseGrounded)
        {
            LinkedList1<UOPair<HlaMsr1>> unphased = null;
            foreach (var pair in SpecialFunctions.EnumerateTwo(phaseGrounded.First, phaseGrounded.Second))
            { 
                unphased = LinkedList1<UOPair<HlaMsr1>>.GetInstance(UOPair<HlaMsr1>.GetInstance(pair.Key, pair.Value), unphased); 
            }
            return unphased; 
        }


    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
