using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using VirusCount.Qmr;
using Optimization; 
 
namespace VirusCount.Qmr
{ 
    public class BestParamsAndHlaAssignments
    {
        private BestParamsAndHlaAssignments()
        {
        }
 
        public BestSoFar<double, OptimizationParameterList> BestParamsSoFar; 
        public Dictionary<string, BestSoFar<double, TrueCollection>> PeptideToBestHlaAssignmentSoFar;
        public OptimizationParameterList QmrrParamsStart; 
        private ModelLikelihoodFactories ModelLikelihoodFactories;

        //private BestParamsAndHlaAssignments(OptimizationParameterList qmrrParamsStart)
        //{
        //    QmrrParamsStart = qmrrParamsStart;
        //    CreateBestParamsSoFar(); 
        //    PeptideToBestHlaAssignmentSoFar = null; 
        //}
 
        public static BestParamsAndHlaAssignments GetInstance(ModelLikelihoodFactories modelLikelihoodFactories, OptimizationParameterList qmrrParamsStart, QmrrPartialModelCollection qmrrPartialModelCollection, double depth)
        {
            BestParamsAndHlaAssignments aBestParamsAndHlaAssignments = new BestParamsAndHlaAssignments();
            aBestParamsAndHlaAssignments.QmrrParamsStart = qmrrParamsStart;
            aBestParamsAndHlaAssignments.CreateBestParamsSoFar();
            aBestParamsAndHlaAssignments.ModelLikelihoodFactories = modelLikelihoodFactories; 
            aBestParamsAndHlaAssignments.SearchForBestParamsAndHlaAssignments(qmrrPartialModelCollection, depth); 
            return aBestParamsAndHlaAssignments;
        } 


        private void CreateBestParamsSoFar()
        {
            BestParamsSoFar = BestSoFar<double, OptimizationParameterList>.GetInstance(SpecialFunctions.DoubleGreaterThan);
            BestParamsSoFar.Compare(double.NegativeInfinity, QmrrParamsStart); 
        } 

 
        private void SearchForBestParamsAndHlaAssignments(QmrrPartialModelCollection qmrrPartialModelCollection, double depth)
        {
            int cStep = 100;
            double eps = 1e-7;
            OptimizationParameterList oldQmrrParams = null;
            Debug.WriteLine(SpecialFunctions.CreateTabString("depth", "dataset", QmrrParamsStart.ToStringHeader(), "Step", "After", QmrrParamsStart.ToStringHeader(), "Score")); 
            for (int iStep = 0; iStep < cStep && !(BestParamsSoFar.Champ.IsClose(oldQmrrParams, eps)); ++iStep) 
            {
                oldQmrrParams = BestParamsSoFar.Champ.Clone(); 

                double hlaAssignmentSumScore = FindBestHlaAssignmentSet(qmrrPartialModelCollection, depth);

                Debug.WriteLine(SpecialFunctions.CreateTabString(depth, qmrrPartialModelCollection.DatasetName,
                    QmrrParamsStart, iStep + 1, "AfterHla", BestParamsSoFar.Champ,
                    hlaAssignmentSumScore)); 
 
                FindBestQmrrParams(qmrrPartialModelCollection);
 
                Debug.WriteLine(SpecialFunctions.CreateTabString(depth, qmrrPartialModelCollection.DatasetName,
                    QmrrParamsStart, iStep + 1, "AfterParam", BestParamsSoFar.Champ, BestParamsSoFar.ChampsScore));
            }
        }

 
        private double FindBestHlaAssignmentSet(QmrrPartialModelCollection qmrrPartialModelCollection, double depth) 
        {
            double sumScore = 0.0; 
            PeptideToBestHlaAssignmentSoFar = new Dictionary<string, BestSoFar<double, TrueCollection>>();
            foreach (QmrrPartialModel qmrrPartialModel in qmrrPartialModelCollection)
            {
                QmmrModelOnePeptide aQmmrModelOnePeptide = QmmrModelOnePeptideGetInstance(qmrrPartialModel, BestParamsSoFar.Champ, depth);
                aQmmrModelOnePeptide.FindBestHlaAssignment();
                sumScore += aQmmrModelOnePeptide.BestHlaAssignmentSoFar.ChampsScore; 
                PeptideToBestHlaAssignmentSoFar.Add(qmrrPartialModel.Peptide, aQmmrModelOnePeptide.BestHlaAssignmentSoFar); 
            }
 
            return sumScore;
        }

        protected QmmrModelOnePeptide QmmrModelOnePeptideGetInstance(QmrrPartialModel qmrrPartialModel, OptimizationParameterList qmrrParams, double depth)
        {
            QmmrModelOnePeptide aQmmrModelOnePeptide = new QmmrModelOnePeptide(); 
            aQmmrModelOnePeptide.QmrrModelMissingAssignment = QmrrModelMissingAssignment.GetInstance(ModelLikelihoodFactories, qmrrPartialModel, qmrrParams); 
            aQmmrModelOnePeptide.CreateNoSwitchablesHlaAssignment();
            if (depth == 0) 
            {
                // do nothing
            }
            else if (depth == Math.Floor(depth))
            {
                SpecialFunctions.CheckCondition(depth > 0); 
                aQmmrModelOnePeptide.SetForDepthSearch((int)Math.Floor(depth)); 
            }
            else 
            {
                SpecialFunctions.CheckCondition(depth == 1.5);
                aQmmrModelOnePeptide.SetForBitFlipsAnd1Replacement();
            }
            return aQmmrModelOnePeptide;
        } 
 

 
        internal void FindBestQmrrParams(QmrrPartialModelCollection qmrrPartialModelCollection)
        {
            SpecialFunctions.CheckCondition(false, "Regression test this to be sure that switch to new optimization method didn't change anything important - cmk 5/1/2006");

            QmrrlModelMissingParametersCollection aQmrrlModelMissingParametersCollection =
                QmrrlModelMissingParametersCollection.GetInstance(ModelLikelihoodFactories, qmrrPartialModelCollection, PeptideToBestHlaAssignmentSoFar); 
 
            double score;
            OptimizationParameterList qmrrParamsEnd = aQmrrlModelMissingParametersCollection.FindBestParams(BestParamsSoFar.Champ, out score); 

            BestParamsSoFar.Compare(score, qmrrParamsEnd);
        }


 
        //private double ScoreParameterList(QmrrPartialModelCollection qmrrPartialModelCollection, List<double> parameterList) 
        //{
        //    OptimizationParameterList qmrrParams = QmrrParamsStart.CloneWithNewValuesForSearch(parameterList); 


        //    double sum = 0.0;
        //    foreach (QmrrPartialModel qmrrPartialModel in qmrrPartialModelCollection)
        //    {
        //        double loglikelihood = qmrrPartialModel.LogLikelihoodOfCompleteModelConditionedOnKnownHlas(trueCollection, qmrrParams); 
        //        sum += loglikelihood; 
        //    }
        //    return sum; 
        //}

        //private OptimizationParameterList CreateQmrrParamsFromParameterList(List<double> parameterList)
        //{
        //    SpecialFunctions.CheckCondition(parameterList.Count == 4);
 
        //    OptimizationParameterList qmrrParams = OptimizationParameterList.GetInstance( 
        //        SpecialFunctions.Probability(parameterList[0]),
        //        SpecialFunctions.Probability(parameterList[1]), 
        //        Math.Exp(parameterList[2]),
        //        SpecialFunctions.Probability(parameterList[3]));
        //    return qmrrParams;
        //}

        //private static List<double> ExtractListOfParameters(OptimizationParameterList qmrrParams) 
        //{ 
        //    List<double> point = new List<double>();
        //    point.Add(SpecialFunctions.LogOdds(qmrrParams.CausePrior)); 
        //    point.Add(SpecialFunctions.LogOdds(qmrrParams.LinkA));
        //    point.Add(Math.Log(qmrrParams.LinkB));
        //    point.Add(SpecialFunctions.LogOdds(qmrrParams.LeakProbability));
        //    return point;
        //}
 
 

    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
