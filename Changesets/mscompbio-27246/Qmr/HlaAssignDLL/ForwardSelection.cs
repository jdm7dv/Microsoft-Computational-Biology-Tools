using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using System.IO;
using VirusCount.Qmr; //!!! what's the difference between Msr.Mlas.Qmr; and this? 
using Optimization; 
using EpipredLib;
 
namespace VirusCount.Qmrr
{
    public class ForwardSelection : LrtForHla
    {
        internal ForwardSelection(double? leakProbabilityOrNull, double pValueCutOff)
            :base(leakProbabilityOrNull) 
        { 
            PValueCutOff = pValueCutOff;
        } 

        double PValueCutOff;

        internal override string SelectionName
        {
            get 
            { 
                return "ForwardSelection";
            } 
        }

        internal override Set<Hla> CreateCandidateHlaSet(Dictionary<string, Set<Hla>> pidToHlaSet, string peptide)
        {
            return HlaSetFromReactingPatients(pidToHlaSet, peptide);
        } 
 
        internal override Dictionary<Hla, PValueDetails> CreateHlaToPValueDetails(int nullIndex, string peptide, Dictionary<string, Set<Hla>> pidToHlaSetAll, Set<Hla> setOfHlasToConsiderAdding, TextWriter writer)
        { 
            Dictionary<Hla, PValueDetails> hlaToPValueDetails = new Dictionary<Hla, PValueDetails>();
            if (setOfHlasToConsiderAdding.Count == 0)
            {
                return hlaToPValueDetails;
            }
 
            Dictionary<string, double> pidToReactValue = ReactTableUnfiltered[peptide]; 

            Set<Hla> knownHlaSet = KnownTable(peptide); 
            SpecialFunctions.CheckCondition(setOfHlasToConsiderAdding.Intersection(knownHlaSet).Count == 0);

            Set<Hla> bestHlaSetSoFar = Set<Hla>.GetInstance();

            while (setOfHlasToConsiderAdding.Count > 0)
            { 
                BestSoFar<PValueDetails, Hla> bestHlaToAddSoFar = BestSoFar<PValueDetails, Hla>.GetInstance(delegate(PValueDetails pValueDetails1, PValueDetails pValueDetails2) { return pValueDetails1.Diff.CompareTo(pValueDetails2.Diff); }); 
                foreach (Hla hla in setOfHlasToConsiderAdding) //!!!only look at hla's of patients with reactivity to this peptide (how effects nulls?)
                { 
                    PValueDetails pValueDetails = CreateAPValueDetail(nullIndex, peptide, pidToHlaSetAll, knownHlaSet, bestHlaSetSoFar, hla);
                    bestHlaToAddSoFar.Compare(pValueDetails, hla);
                }
                //Debug.WriteLine("");
                PValueDetails bestPValueDetails = bestHlaToAddSoFar.ChampsScore; //!!!weird that PValue details is the score and the object
 
                if (bestPValueDetails.PValue() > PValueCutOff) 
                {
                    break; 
                }

                Hla hlaToAdd = bestHlaToAddSoFar.Champ;

                setOfHlasToConsiderAdding.Remove(hlaToAdd);
                bestHlaSetSoFar = bestHlaSetSoFar.Union(hlaToAdd); 
 
                hlaToPValueDetails.Add(hlaToAdd, bestPValueDetails);
                writer.WriteLine(bestPValueDetails); 
                //Debug.WriteLine(bestPValueDetails);
                writer.Flush();

            }
            return hlaToPValueDetails;
        } 
 
        private PValueDetails CreateAPValueDetail(int nullIndex, string peptide, Dictionary<string, Set<Hla>> pidToHlaSetAll, Set<Hla> knownHlaSet, Set<Hla> bestHlaSetSoFar, Hla hla)
        { 

            //Dictionary<string, Dictionary<string, double>> reactTableCustom;
            Dictionary<string, Set<Hla>> pidToHlaSetCustom =
                    CreatePidToHlaSetCustom(pidToHlaSetAll, bestHlaSetSoFar, hla, knownHlaSet);
                    //out pidToHlaSetCustom, out reactTableCustom);
 
            //!!!could cache both calls to FindBestParams 
            double scoreBase;
            OptimizationParameterList baseParams = FindBestParams(peptide, bestHlaSetSoFar, Set<Hla>.GetInstance(), pidToHlaSetCustom, out scoreBase); 

            double scoreWithOneMore;
            OptimizationParameterList withMoreMoreParams = FindBestParams(peptide, bestHlaSetSoFar.Union(hla), Set<Hla>.GetInstance(), pidToHlaSetCustom,  out scoreWithOneMore);

            PValueDetails pValueDetails = PValueDetails.GetInstance(SelectionName, nullIndex, peptide, hla,
                scoreWithOneMore, scoreBase, knownHlaSet, bestHlaSetSoFar, withMoreMoreParams["leakProbability"].Value, withMoreMoreParams["link" + hla].Value, withMoreMoreParams); 
            //Debug.Write(String.Format("{0}/{1}\t", hla, scoreWithOneMore)); 
            return pValueDetails;
        } 

        //!!!this could be made faster by keeping track of patients with no abstract hlas
        private Dictionary<string, Set<Hla>> CreatePidToHlaSetCustom(Dictionary<string, Set<Hla>> pidToHlaSetAll, Set<Hla> bestHlaSetSoFar, Hla hla, Set<Hla> knownHlaSet
            //out Dictionary<string, Set<Hla>> pidToHlaSetCustom,
            //out Dictionary<string, Dictionary<string, double>> reactTableCustom
            ) 
        { 
            Set<Hla> possibleCauses = bestHlaSetSoFar.Union(knownHlaSet);
            possibleCauses.AddNewOrOld(hla); 

#if DEBUG
            foreach (Hla hlaPossibleCause in possibleCauses)
            {
                Debug.Assert(hlaPossibleCause.IsGround); // real assert
            } 
#endif 

            Dictionary<string, Set<Hla>> pidToHlaSetCustom = new Dictionary<string, Set<Hla>>(); 
            //reactTableCustom = new Dictionary<string, Dictionary<string, double>>();

            foreach (string pid in pidToHlaSetAll.Keys)
            {
                Set<Hla> patientHlaSet = pidToHlaSetAll[pid];
 
 
                //bestSoFar/known Hla PidContains ExcludePid?
                //B23   B25 B15??   No 
                //B23   B1511   B15??   Yes

                if (!ThisPatientContainsAnAbstractHlaThatGeneralizesAPossibleCause(patientHlaSet, possibleCauses))
                {
                    pidToHlaSetCustom.Add(pid, patientHlaSet);
                    // reactTableCustom.Add(pid, ReactTableUnfiltered[pid]); 
                } 
                else
                { 
                    //Debug.WriteLine(SpecialFunctions.CreateTabString(patientHlaSet, possibleCauses));
                }
            }

            return pidToHlaSetCustom;
        } 
 
        private bool ThisPatientContainsAnAbstractHlaThatGeneralizesAPossibleCause(Set<Hla> patientHlaSet, Set<Hla> possibleCauses)
        { 
            foreach (Hla patientHla in patientHlaSet)
            {
                if (!patientHla.IsGround)
                {
                    foreach (Hla possibleCause in possibleCauses)
                    { 
                        if (patientHla.IsMoreGeneralThan(possibleCause)) 
                        {
                            return true; 
                        }
                    }
                }
            }
            return false;
        } 
 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
