using System; 
using System.Collections.Generic;
using System.Text;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using Optimization;
using EpipredLib; 
 
namespace VirusCount.Qmr
{ 
    public class TwoCausePriors : ThreeParamSlow
    {
        internal TwoCausePriors()
        {
        }
 
 
        internal void SetPeptideToFitUniverse(string dataset)
        { 
            Qmrr.HlaFactory hlaFactory = Qmrr.HlaFactory.GetFactory("noConstraint");

            PeptideToFitUniverse = new Dictionary<string, Set<Hla>>();
            string filename = dataset + "supertypefit.txt";
            string line = null;
            //!!!would be nice to read as a tab table, to remove redundent lines, to check that HLAs are of the right form 
            using(StreamReader streamReader = File.OpenText(filename)) 
            {
                while (null != (line = streamReader.ReadLine())) 
                {
                    string[] fields = line.Split('\t');
                    SpecialFunctions.CheckCondition(fields.Length == 2);
                    string peptide = fields[0];
                    Hla hla = hlaFactory.GetGroundInstance(fields[1]);
                    Set<Hla> fitUniverse = SpecialFunctions.GetValueOrDefault(PeptideToFitUniverse, peptide); 
                    fitUniverse.AddNewOrOld(hla); 
                }
            } 

        }

        Dictionary<string, Set<Hla>> PeptideToFitUniverse;

        protected override double LogLikelihoodOfCausesConditionedOnKnownHlas(QmrrPartialModel qmrrPartialModel, TrueCollection trueCollection, OptimizationParameterList qmrrParams) 
        { 
            //!!!This could be calculated during the construction of qmrrPartialModel
            Set<Hla> fitUniverse = SpecialFunctions.GetValueOrDefault(PeptideToFitUniverse, qmrrPartialModel.Peptide); 

            //Compute with priors
            double unfitCausePrior = qmrrParams["causePrior"].Value;
            double fitFactor = qmrrParams["fitFactor"].Value;
            double fitCausePrior = unfitCausePrior * fitFactor;
            double logUnfitCausePrior = Math.Log(unfitCausePrior); 
            double logOneLessUnfitCausePrior = Math.Log(1.0 - unfitCausePrior); 
            double logFitCausePrior = Math.Log(fitCausePrior);
            double logOneLessFitCausePrior = Math.Log(1.0 - fitCausePrior); 



            //Tabulate counts
            int unfitTotalCount = qmrrPartialModel.HlaList.Count - fitUniverse.Count;
            int trueCountLessKnown = trueCollection.Count - qmrrPartialModel.KnownHlaSet.Count; 
            int falseCount = qmrrPartialModel.HlaList.Count - trueCollection.Count; 
            int knownFitCount = KnownFitCount(fitUniverse, qmrrPartialModel.KnownHlaSet); //!!!could be pretabulated
            int knownUnfitCount = qmrrPartialModel.KnownHlaSet.Count - knownFitCount; 
            int fitTrueCountLessKnown = FitTrueCount(fitUniverse, trueCollection) - knownFitCount;
            int unfitTrueCountLessKnown = trueCountLessKnown - fitTrueCountLessKnown;
            int fitFalseCount = fitUniverse.Count - fitTrueCountLessKnown - knownFitCount;
            int unfitFalseCount = unfitTotalCount - unfitTrueCountLessKnown - knownUnfitCount;

 
            //Compute logLikelihood 
            double logLikelihood =
                  (double)unfitTrueCountLessKnown * logUnfitCausePrior 
                + (double)unfitFalseCount * logOneLessUnfitCausePrior
                + (double)fitTrueCountLessKnown * logFitCausePrior
                + (double)fitFalseCount * logOneLessFitCausePrior;
            return logLikelihood;
        }
 
        private int KnownFitCount(Set<Hla> fitUniverse, Set<Hla> knownHlaSet) 
        {
            int knownFitCount = 0; 
            foreach (Hla hla in knownHlaSet)
            {
                if (fitUniverse.Contains(hla))
                {
                    ++knownFitCount;
                } 
            } 
            return knownFitCount;
        } 

        //!!!similar code is used in NumberOfPositiveHlas
        private int FitTrueCount(Set<Hla> fitUniverse, TrueCollection trueCollection)
        {
            int fitTrueCount = 0;
            foreach (Hla hla in trueCollection) 
            { 
                if (fitUniverse.Contains(hla))
                { 
                    ++fitTrueCount;
                }
            }
            return fitTrueCount;
        }
    } 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
