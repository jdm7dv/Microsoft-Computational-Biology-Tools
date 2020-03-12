using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using Optimization; 
using EpipredLib; 

namespace VirusCount.Qmr 
{
    public class QmrrModelAllPeptides
    {
        private QmrrModelAllPeptides()
        {
        } 
 
        public static QmrrModelAllPeptides GetInstance(ModelLikelihoodFactories modelLikelihoodFactories, string datasetName, OptimizationParameterList qmrrParamsStart, double depth, string hlaFactoryName)
        { 
            QmrrModelAllPeptides aQmrrModelAllPeptides = new QmrrModelAllPeptides();
            aQmrrModelAllPeptides.QmrrPartialModelCollection = QmrrPartialModelCollection.GetInstance(modelLikelihoodFactories, datasetName, qmrrParamsStart, hlaFactoryName);
            aQmrrModelAllPeptides.BestParamsAndHlaAssignments = BestParamsAndHlaAssignments.GetInstance(modelLikelihoodFactories, qmrrParamsStart, aQmrrModelAllPeptides.QmrrPartialModelCollection, depth);
            aQmrrModelAllPeptides.ModelLikelihoodFactories = modelLikelihoodFactories;
            return aQmrrModelAllPeptides;
        } 
 
        internal QmrrPartialModelCollection QmrrPartialModelCollection;
        public BestParamsAndHlaAssignments BestParamsAndHlaAssignments; 
        private ModelLikelihoodFactories ModelLikelihoodFactories;

        public void Report(string directory, string name)
        {
            ReportPerHlaAssignment(BestParamsAndHlaAssignments.PeptideToBestHlaAssignmentSoFar, directory, name);
            ReportPerHla(BestParamsAndHlaAssignments.BestParamsSoFar.Champ, BestParamsAndHlaAssignments.PeptideToBestHlaAssignmentSoFar, directory, name); 
        } 

        private void ReportPerHlaAssignment(Dictionary<string, BestSoFar<double, TrueCollection>> peptideToBestHlaAssignmentSoFar, 
            string directory, string name)
        {
            string fileName = string.Format(@"{0}\NoisyOr.HlasPerPeptide.{1}.new.txt", directory, name);
            using (StreamWriter output = File.CreateText(fileName))
            {
                output.WriteLine(SpecialFunctions.CreateTabString("Peptide", "HLAAssignment", "LogLikelihood")); 
                foreach (QmrrPartialModel qmrrPartialModel in QmrrPartialModelCollection) 
                {
                    BestSoFar<double, TrueCollection> bestHlaAssignment = peptideToBestHlaAssignmentSoFar[qmrrPartialModel.Peptide]; 
                    TrueCollection trueCollectionFull = bestHlaAssignment.Champ;
                    double loglikelihoodFull = bestHlaAssignment.ChampsScore;
                    output.WriteLine(SpecialFunctions.CreateTabString(qmrrPartialModel.Peptide, trueCollectionFull, loglikelihoodFull));
                }
            }
        } 
 
        private void ReportPerHla(OptimizationParameterList qmrrParams, Dictionary<string, BestSoFar<double, TrueCollection>> peptideToBestHlaAssignmentSoFar,
            string directory, string name) 
        {
            string fileName = string.Format(@"{0}\NoisyOr.PeptideHlaProbability.{1}.new.txt", directory, name);
            using (StreamWriter output = File.CreateText(fileName))
            {
                output.WriteLine(SpecialFunctions.CreateTabString("Peptide", "HLA", "LogOdds", "Probability"));
                foreach (QmrrPartialModel qmrrPartialModel in QmrrPartialModelCollection) 
                { 
                    QmrrModelMissingAssignment aQmrrModelMissingAssignment = QmrrModelMissingAssignment.GetInstance(ModelLikelihoodFactories, qmrrPartialModel, qmrrParams);
                    BestSoFar<double, TrueCollection> bestHlaAssignment = peptideToBestHlaAssignmentSoFar[qmrrPartialModel.Peptide]; 
                    Set<Hla> trueCollectionFullAsSet = new Set<Hla>(bestHlaAssignment.Champ);

                    double loglikelihoodFull = bestHlaAssignment.ChampsScore;
                    foreach (Hla hla in trueCollectionFullAsSet)
                    {
                        bool known = qmrrPartialModel.KnownHlaSet.Contains(hla); 
                        if (!known) 
                        {
                            Set<Hla> allLessOne = trueCollectionFullAsSet.SubtractElement(hla); 
                            TrueCollection trueCollectionWithout = TrueCollection.GetInstance(allLessOne);
                            double loglikelihoodWithout = aQmrrModelMissingAssignment.LogLikelihoodOfCompleteModelConditionedOnKnownHlas(trueCollectionWithout);
                            double logOdds = loglikelihoodFull - loglikelihoodWithout;
                            double probabilityFull = Math.Exp(loglikelihoodFull);
                            double probabilityWithout = Math.Exp(loglikelihoodWithout);
                            double probability = probabilityFull / (probabilityFull + probabilityWithout); 
                            Debug.Assert(logOdds >= 0); // real assert 
                            output.WriteLine(SpecialFunctions.CreateTabString(qmrrPartialModel.Peptide, hla, logOdds, probability));
                        } 
                    }
                }
            }
        }
    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
