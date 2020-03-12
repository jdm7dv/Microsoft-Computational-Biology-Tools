using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using System.IO;
using Msr.Mlas.Qmr; 
using VirusCount.Qmr; //!!! what's the difference between Msr.Mlas.Qmr; and this? 
using Optimization;
 
namespace VirusCount.Qmrr
{
    //public class BackwardSelection : LrtForHla
    //{
    //    internal BackwardSelection()
    //    { 
    //        Debug.Fail("Need to update to support non ground HLAs"); 
    //    }
 
    //    internal override string SelectionName
    //    {
    //        get
    //        {
    //            return "BackwardSelection";
    //        } 
    //    } 

    //    double UnivariatePValueCutOff = .05; 

    //    override internal Set<Hla> CreateCandidateHlaSet(Dictionary<string, Set<Hla>> pidToHlaSet, string peptide)
    //    {
    //        Set<Hla> knownHlaSet = KnownTable(peptide);
    //        Set<Hla> originalHlaSet = Set<Qmrr.Hla>.GetInstance();
    //        Set<Hla> univariateHlaSet = CreateUnivariateHlaSet(UnivariatePValueCutOff, pidToHlaSet, peptide); 
    //        Set<Hla> candidateHlaSet = originalHlaSet.Union(univariateHlaSet).Subtract(knownHlaSet); 
    //        return candidateHlaSet;
    //    } 

    //    override internal Dictionary<Hla, PValueDetails> CreateHlaToPValueDetails(
    //        int nullIndex,
    //        string peptide,
    //        Dictionary<string, Set<Hla>> patientList,
    //        Set<Hla> candidateHlaSet, 
    //        StreamWriter streamWriter) 
    //    {
    //        Set<Hla> knownHlaSet = KnownTable(peptide); 
    //        //Set<string> candidateHlaSet = originalCandidateHlaSet.Clone();

    //        Set<Qmrr.Hla> hlaWithLinkZero = Set<Qmrr.Hla>.GetInstance(); //!!!don't need the linkZero's anymore because causePrior is fixed at .5
    //        double scoreAll;
    //        OptimizationParameterList previousParams = FindBestParams(peptide, candidateHlaSet, hlaWithLinkZero, patientList, out scoreAll);
 
    //        Dictionary<Qmrr.Hla, PValueDetails> hlaToPValueDetails = new Dictionary<Qmrr.Hla, PValueDetails>(); 
    //        while (hlaWithLinkZero.Count < candidateHlaSet.Count)
    //        { 
    //            Debug.WriteLine(SpecialFunctions.CreateTabString(PValueDetails.Header));
    //            BestSoFar<PValueDetails, Qmrr.Hla> hlaToRemove = BestSoFar<PValueDetails, Qmrr.Hla>.GetInstance(delegate(PValueDetails pValueDetails1, PValueDetails pValueDetails2) { return pValueDetails2.Diff.CompareTo(pValueDetails1.Diff); });

    //            Set<Qmrr.Hla> hlaWithNonZeroLinks = candidateHlaSet.Subtract(hlaWithLinkZero);
    //            foreach (Hla hla in hlaWithNonZeroLinks)
    //            { 
    //                // Set<string> setLessOne = candidateHlaSet.SubtractElement(hla); //would be faster to remove/add from one set, but this simplier and fast enough 

    //                double scoreLessOne; 
    //                OptimizationParameterList lessHlaParams = FindBestParams(peptide, candidateHlaSet, hlaWithLinkZero.Union(hla), patientList,  out scoreLessOne);

    //                PValueDetails pValueDetails = PValueDetails.GetInstance(SelectionName, nullIndex, peptide, hla, scoreAll, scoreLessOne, knownHlaSet, hlaWithNonZeroLinks, previousParams["leakProbability"].Value, previousParams["link" + hla].Value, lessHlaParams);
    //                //SpecialFunctions.CheckCondition(diff >= 0);
    //                Debug.WriteLine(SpecialFunctions.CreateTabString(pValueDetails));
    //                hlaToRemove.Compare(pValueDetails, hla); 
    //            } 

    //            PValueDetails pValueDetailsChamp = hlaToRemove.ChampsScore; 
    //            hlaToPValueDetails.Add(hlaToRemove.Champ, pValueDetailsChamp);
    //            scoreAll = pValueDetailsChamp.Score2;
    //            previousParams = pValueDetailsChamp.PreviousParams;
    //            hlaWithLinkZero.AddNew(hlaToRemove.Champ);
    //            streamWriter.WriteLine(pValueDetailsChamp);
    //            streamWriter.Flush(); 
    //        } 
    //        return hlaToPValueDetails;
    //    } 


    //}
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
