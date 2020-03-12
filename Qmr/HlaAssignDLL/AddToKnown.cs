using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using System.IO;
using Msr.Mlas.Qmr; 
using VirusCount.Qmr; //!!! what's the difference between Msr.Mlas.Qmr; and this? 
using Optimization;
using EpipredLib; 

namespace VirusCount.Qmrr
{
    public class AddToKnown : LrtForHla
    {
        internal AddToKnown(double? leakProbabilityOrNull) 
            :base(leakProbabilityOrNull) 
        {
            Debug.Fail("Need to update to support non ground HLAs"); 
        }

        internal override string SelectionName
        {
            get
            { 
                return "AddToKnown"; 
            }
        } 



        internal override Dictionary<Hla, PValueDetails> CreateHlaToPValueDetails(int nullIndex, string peptide, Dictionary<string, Set<Hla>> pidToHlaSet, Set<Hla> candidateHlaSet, TextWriter writer)
        {
            Dictionary<string, double> pidToReactValue = ReactTableUnfiltered[peptide]; 
            Set<Hla> knownHlaSet = KnownTable(peptide); 

            double scoreKnown; 
            OptimizationParameterList knownParams = FindBestParams(peptide, Set<Hla>.GetInstance(), Set<Hla>.GetInstance(), pidToHlaSet, out scoreKnown);


            Dictionary<Hla, PValueDetails> hlaToPValueDetails = new Dictionary<Hla, PValueDetails>();
            foreach (Hla hla in candidateHlaSet.Subtract(knownHlaSet)) //!!!only look at hla's of patients with reactivity to this peptide (how effects nulls?)
            { 
                double scoreWithOne; 
                OptimizationParameterList withHlaParams = FindBestParams(peptide, Set<Hla>.GetInstance(hla), Set<Hla>.GetInstance(), pidToHlaSet, out scoreWithOne);
                PValueDetails pValueDetails = PValueDetails.GetInstance(SelectionName, nullIndex, peptide, hla, scoreWithOne, scoreKnown, knownHlaSet, null, withHlaParams["leakProbability"].Value, withHlaParams["link" + hla].Value, withHlaParams); 
                hlaToPValueDetails.Add(hla, pValueDetails);
                writer.WriteLine(pValueDetails);
                Debug.WriteLine(pValueDetails);
                writer.Flush();
            }
            return hlaToPValueDetails; 
        } 

        internal override Set<Hla> CreateCandidateHlaSet(Dictionary<string, Set<Hla>> pidToHlaSet, string peptide) 
        {
            return HlaSetFromReactingPatients(pidToHlaSet, peptide);
        }


    } 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
