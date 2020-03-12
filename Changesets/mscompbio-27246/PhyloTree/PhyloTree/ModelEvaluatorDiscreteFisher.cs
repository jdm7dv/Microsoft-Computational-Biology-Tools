using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;
using Mlas.Tabulate;
using System.Diagnostics; 
 
namespace VirusCount.PhyloTree
{ 
    public class ModelEvaluatorDiscreteFisher : ModelEvaluatorDiscrete
    {
        new public const string BaseName = "FishersExactTest";

        private readonly IEnumerable<Leaf> _fullLeafCollection;
 
        public override string Name 
        {
            get { return ModelEvaluatorDiscrete.BaseName + BaseName; } 
        }

        protected ModelEvaluatorDiscreteFisher(IEnumerable<Leaf> fullLeafCollection)
            : base(SpecialFunctions.CreateSingletonList<IDistributionSingleVariable>(DistributionDiscreteSingleVariable.GetInstance()),
                null, null)    // null model is a hack to avoid null exceptions at load time.
        { 
            _fullLeafCollection = fullLeafCollection; 
        }
 
        public static ModelEvaluatorDiscreteFisher GetInstance(IEnumerable<Leaf> fullLeafCollection)
        {
            return new ModelEvaluatorDiscreteFisher(fullLeafCollection);
        }

 
 
        public override string ToHeaderString()
        { 
            //EvaluationResults dummyResults = EvaluationResultsFisher.GetInstance(this, TwoByTwo.GetInstance(new int[] { 1, 1, 1, 1 }));
            List<Score> nullScores;
            Score altScore;
            TwoByTwo fishers2by2 = TwoByTwo.GetInstance(new int[] { 1, 1, 1, 1 });
            ComputeIidScores(fishers2by2.ToOneDArray(), out nullScores, out altScore);
 
            EvaluationResults dummyResults = EvaluationResultsFisher.GetInstance(this, nullScores, altScore, fishers2by2); 

            return dummyResults.ToHeaderString(); 
        }


        public override EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> predMap, Converter<Leaf, SufficientStatistics> targMap)
        {
            TwoByTwo fishers2by2 = TwoByTwo.GetInstance( 
                SufficientStatisticsMapToIntDictionaryMap(predMap, _fullLeafCollection), 
                SufficientStatisticsMapToIntDictionaryMap(targMap, _fullLeafCollection));
 
            int[] fisherCounts = fishers2by2.ToOneDArray();
            List < Score > nullScores;
            Score altScore;

            ComputeIidScores(fisherCounts, out nullScores, out altScore);
 
            EvaluationResultsFisher results = EvaluationResultsFisher.GetInstance(this, nullScores, altScore, fishers2by2); 

#if DEBUG 
            EvaluationResults results2 = EvaluateModelOnDataGivenParams(predMap, targMap, results);
            double eps = 1E-14;
            Debug.Assert(ComplexNumber.ApproxEqual(results.AltLL, results2.AltLL, eps));
            Debug.Assert(ComplexNumber.ApproxEqual(results.NullLL, results2.NullLL, eps));
            Debug.Assert(ComplexNumber.ApproxEqual(results.ComputePValue(), results2.ComputePValue(), eps));
#endif 
 
            return results;
        } 


        public override EvaluationResults EvaluateModelOnDataGivenParams(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2, EvaluationResults previousResults)
        {
            TwoByTwo fishers2by2 = TwoByTwo.GetInstance(
                SufficientStatisticsMapToIntDictionaryMap(v1, _fullLeafCollection), 
                SufficientStatisticsMapToIntDictionaryMap(v2, _fullLeafCollection)); 

            int[] fisherCounts = fishers2by2.ToOneDArray(); 

            List<Score> nullScores;
            Score altScore;
            ComputeIidScoresGivenParams(fisherCounts,
                previousResults.NullScores[0].OptimizationParameters, previousResults.NullScores[1].OptimizationParameters,
                previousResults.AltScore.OptimizationParameters, 
                out nullScores, out altScore); 

            //int tt = fisherCounts[(int)TwoByTwo.ParameterIndex.TT]; 
            //int tf = fisherCounts[(int)TwoByTwo.ParameterIndex.TF];
            //int ft = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
            //int ff = fisherCounts[(int)TwoByTwo.ParameterIndex.FF];
            //int sum = SpecialFunctions.Sum(fisherCounts);

            //double pi0 = (double)(tt + tf) / sum; 
            //double pi1 = (double)(tt + ft) / sum; 
            //double ptt = (double)tt / sum;
            //double ptf = (double)tf / sum; 
            //double pft = (double)ft / sum;
            //double pff = 1 - ptt - ptf - pft;

            //double predicted_pi0 = previousResults.NullScores[0].OptimizationParameters[0].Value;
            //double predicted_pi1 = previousResults.NullScores[1].OptimizationParameters[0].Value;
            //double predicted_ptt = previousResults.AltScore.OptimizationParameters[0].Value; 
            //double predicted_ptf = previousResults.AltScore.OptimizationParameters[1].Value; 
            //double predicted_pft = previousResults.AltScore.OptimizationParameters[2].Value;
            //double predicted_pff = 1 - predicted_ptt - predicted_ptf - predicted_pft; 


            //double nullLLLeft = pi0 * Math.Log(predicted_pi0);
            //double nullLLRight = pi1 * Math.Log(predicted_pi1);
            //double altLL = predicted_ptt * Math.Log(ptt) + predicted_ptf * Math.Log(ptf) + predicted_pft * Math.Log(pft) + predicted_pff * Math.Log(pff);
 
            //Score nullScoreLeft = Score.GetInstance(nullLLLeft, previousResults.NullScores[0].OptimizationParameters, null); 
            //Score nullScoreRight = Score.GetInstance(nullLLRight, previousResults.NullScores[1].OptimizationParameters, null);
            //Score altScore = Score.GetInstance(altLL, previousResults.AltScore.OptimizationParameters, null); 

            //List<Score> nullScores = new List<Score>(2);
            //nullScores.Add(nullScoreLeft);
            //nullScores.Add(nullScoreRight);

            EvaluationResults results = EvaluationResultsFisher.GetInstance(this, nullScores, altScore, fishers2by2); 
            return results; 
        }
 
        private static void ComputeIidScores(int[] fisherCounts, out List<Score> nullScores, out Score altScore)
        {
            int tt = fisherCounts[(int)TwoByTwo.ParameterIndex.TT];
            int tf = fisherCounts[(int)TwoByTwo.ParameterIndex.TF];
            int ft = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
            int ff = fisherCounts[(int)TwoByTwo.ParameterIndex.FF]; 
            int sum = SpecialFunctions.Sum(fisherCounts); 

            double pi0 = (double)(tt + tf) / sum; 
            double pi1 = (double)(tt + ft) / sum;
            double ptt = (double)tt / sum;
            double ptf = (double)tf / sum;
            double pft = (double)ft / sum;
            double pff = (double)ff / sum;
 
            OptimizationParameterList nullParamsLeft = OptimizationParameterList.GetInstance( 
                OptimizationParameter.GetProbabilityInstance("Pi", pi0, true));
            OptimizationParameterList nullParamsRight = OptimizationParameterList.GetInstance( 
                OptimizationParameter.GetProbabilityInstance("Pi", pi1, true));

            OptimizationParameterList altParams = OptimizationParameterList.GetInstance(
                OptimizationParameter.GetProbabilityInstance("P_TT", ptt, true),
                OptimizationParameter.GetProbabilityInstance("P_TF", ptf, true),
                OptimizationParameter.GetProbabilityInstance("P_FT", pft, true)); 
 
            ComputeIidScoresGivenParams(fisherCounts, nullParamsLeft, nullParamsRight, altParams, out nullScores, out altScore);
        } 

        private static void ComputeIidScoresGivenParams(int[] fisherCounts,
            OptimizationParameterList nullParamsLeft, OptimizationParameterList nullParamsRight, OptimizationParameterList altParams,
            out List<Score> nullScores, out Score altScore)
        {
            int tt = fisherCounts[(int)TwoByTwo.ParameterIndex.TT]; 
            int tf = fisherCounts[(int)TwoByTwo.ParameterIndex.TF]; 
            int ft = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
            int ff = fisherCounts[(int)TwoByTwo.ParameterIndex.FF]; 

            int t0 = tt + tf;
            int t1 = tt + ft;
            int sum = SpecialFunctions.Sum(fisherCounts);

            double predicted_pi0 = nullParamsLeft[0].Value; 
            double predicted_pi1 = nullParamsRight[0].Value; 
            double predicted_ptt = altParams[0].Value;
            double predicted_ptf = altParams[1].Value; 
            double predicted_pft = altParams[2].Value;
            double predicted_pff = 1 - predicted_ptt - predicted_ptf - predicted_pft;

            Score nullScoreLeft = Score.GetInstance(
                (t0 == 0 ? 0 : t0 * Math.Log(predicted_pi0)) +
                (sum - t0 == 0 ? 0 : (sum - t0) * Math.Log(1 - predicted_pi0)), 
                nullParamsLeft, null); 

            Score nullScoreRight = Score.GetInstance( 
                (t1 == 0 ? 0 : t1 * Math.Log(predicted_pi1)) +
                (sum - t1 == 0 ? 0 : (sum - t1) * Math.Log(1 - predicted_pi1)),
                nullParamsRight, null);

            altScore = Score.GetInstance(
                (tt == 0 ? 0 : tt * Math.Log(predicted_ptt)) + 
                (tf == 0 ? 0 : tf * Math.Log(predicted_ptf)) + 
                (ft == 0 ? 0 : ft * Math.Log(predicted_pft)) +
                (ff == 0 ? 0 : ff * Math.Log(predicted_pff)), 
                altParams, null);

            nullScores = new List<Score>(2);
            nullScores.Add(nullScoreLeft);
            nullScores.Add(nullScoreRight);
        } 
 
        private static Dictionary<string, int> SufficientStatisticsMapToIntDictionaryMap(Converter<Leaf, SufficientStatistics> leafToStatsMap, IEnumerable<Leaf> fullLeafCollection)
        { 
            Dictionary<string, int> result = new Dictionary<string, int>(SpecialFunctions.Count(fullLeafCollection));
            foreach (Leaf leaf in fullLeafCollection)
            {
                SufficientStatistics value = leafToStatsMap(leaf);
                if (!value.IsMissing())
                { 
                    result.Add(leaf.CaseName, (int)(BooleanStatistics)value); 
                }
            } 
            return result;
        }
    }


    class EvaluationResultsFisher : EvaluationResultsDiscrete 
    { 
        private double _pValue;
 
        protected EvaluationResultsFisher(ModelEvaluator modelEval, List<Score> nullScores, Score altScore, int[] fisherCounts, int chiSquareDegreesOfFreedom, double pValue)
            :
            base(modelEval, nullScores, altScore, fisherCounts, chiSquareDegreesOfFreedom)
        {
            _pValue = pValue;
        } 
 

        public static EvaluationResultsFisher GetInstance(ModelEvaluatorDiscreteFisher modelEvaluatorDiscreteFisher, List<Score> nullScores, Score altScore, TwoByTwo fishers2by2) 
        {
            double pValue = fishers2by2.FisherExactTest;
            int[] fisherCounts = fishers2by2.ToOneDArray();
            return new EvaluationResultsFisher(modelEvaluatorDiscreteFisher, nullScores, altScore, fisherCounts, 1, pValue);
        }
 
        //public static EvaluationResultsFisher GetInstance(ModelEvaluator modelEval, TwoByTwo fishers2by2) 
        //{
        //    int[] fisherCounts = fishers2by2.ToOneDArray(); 
        //    int total = SpecialFunctions.Sum(fisherCounts);

        //    int predTrueCount = fishers2by2.Counts[1, 0] + fishers2by2.Counts[1, 1];
        //    int predFalseCount = total - predTrueCount;
        //    double logLikelihoodPred = predTrueCount * Math.Log(predTrueCount / (double)total) + predFalseCount * Math.Log(predFalseCount / (double)total);
 
        //    int targTrueCount = fishers2by2.Counts[0, 1] + fishers2by2.Counts[1, 1]; 
        //    int targFalseCount = total - targTrueCount;
        //    double logLikelihoodTarg = targTrueCount * Math.Log(targTrueCount / (double)total) + targFalseCount * Math.Log(targFalseCount / (double)total); 

        //    double logLikelihoodFisher = 0;
        //    for (int i = 0; i < 2; i++)
        //    {
        //        for (int j = 0; j < 2; j++)
        //        { 
        //            logLikelihoodFisher += fishers2by2.Counts[i, j] * Math.Log(fishers2by2.Counts[i, j] / (double)total); 
        //        }
        //    } 

        //    List<Score> nullScores = new List<Score>(2);
        //    nullScores.Add(Score.GetInstance(logLikelihoodPred, OptimizationParameterList.GetInstance(new OptimizationParameter[0]), null));
        //    nullScores.Add(Score.GetInstance(logLikelihoodTarg, OptimizationParameterList.GetInstance(new OptimizationParameter[0]), null));

        //    Score altScore = Score.GetInstance(logLikelihoodFisher, OptimizationParameterList.GetInstance(new OptimizationParameter[0]), null); 
        //    double pValue = fishers2by2.FisherExactTest; 

        //    return new EvaluationResultsFisher(modelEval, nullScores, altScore, fisherCounts, 1, pValue); 
        //}

        public double BIC
        {
            get
            { 
                return ChiSquareDegreesOfFreedom * Math.Log(GlobalNonMissingCount) / 2.0; 
            }
        } 

        public override double ComputePValue()
        {
            return _pValue;
        }
 
        public override string IidStatsHeaderString() 
        {
            return base.IidStatsHeaderString() + "\t" + "BIC"; 
        }

        public override string IidStatsString()
        {

            return base.IidStatsString() + "\t" + BIC; 
        } 

    } 

}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
