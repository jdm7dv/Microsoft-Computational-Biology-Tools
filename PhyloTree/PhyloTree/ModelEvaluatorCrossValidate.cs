using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using Optimization;
using System.Diagnostics;
 
namespace VirusCount.PhyloTree 
{
    public class ModelEvaluatorCrossValidate : ModelEvaluator 
    {
        public const int DefaultCrossValidateCount = 10;
        public const string BaseName = "CrossValidate";

        private readonly int _crossValidateCount = 10;
        public readonly ModelEvaluator InternalEvaluator; 
 
        protected ModelEvaluatorCrossValidate(ModelEvaluator modelToCrossValidate, int crossValidateCount)
            : 
            base(modelToCrossValidate.NullDistns, modelToCrossValidate.AltDistn, modelToCrossValidate.ModelScorer)
        {
            InternalEvaluator = modelToCrossValidate;
            _crossValidateCount = crossValidateCount;
        }
 
        public static ModelEvaluatorCrossValidate GetInstance(ModelEvaluator modelToCrossValidate) 
        {
            return new ModelEvaluatorCrossValidate(modelToCrossValidate, DefaultCrossValidateCount); 
        }

        new public static ModelEvaluatorCrossValidate GetInstance(string nameAndParametersOfEvaluatorToCrossValidate, ModelScorer scorer)
        {
            ModelEvaluator evaluator = ModelEvaluator.GetInstance(nameAndParametersOfEvaluatorToCrossValidate, scorer);
            return GetInstance(evaluator); 
        } 

        public override string Name 
        {
            get { return BaseName + InternalEvaluator.Name; }
        }

        public override EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2)
        { 
            List<Leaf> nonMissingLeaves = new List<Leaf>(100); 
            int seed = 0;
            foreach (Leaf leaf in ModelScorer.PhyloTree.LeafCollection) 
            {
                SufficientStatistics class1 = v1(leaf);
                SufficientStatistics class2 = v2(leaf);
                if (!class1.IsMissing() && !class2.IsMissing())
                {
                    nonMissingLeaves.Add(leaf); 
                    seed ^= (leaf.CaseName + class1.ToString() + class2.ToString()).GetHashCode(); 
                }
            } 

            Random rand = new Random(seed);
            nonMissingLeaves = SpecialFunctions.Shuffle(nonMissingLeaves, ref rand);

            int groupSize = nonMissingLeaves.Count / _crossValidateCount;
 
            EvaluationResultsCrossValidate combinedResults = null; 
            double testAltLLSum = 0;    // for debugging
            double testNullLLSum = 0;   // for debugging 
            for (int i = 0; i < _crossValidateCount; i++)
            {
                int testStart = i * groupSize;
                int trainStart = testStart + groupSize;
                Set<Leaf> trainSet = new Set<Leaf>(SpecialFunctions.SubList(nonMissingLeaves, trainStart, nonMissingLeaves.Count - trainStart));
                trainSet.AddNewRange(SpecialFunctions.SubList(nonMissingLeaves, 0, testStart)); 
 
                Converter<Leaf, SufficientStatistics> v1Train = CreateFilteredMap(v1, trainSet);
                Converter<Leaf, SufficientStatistics> v2Train = CreateFilteredMap(v2, trainSet); 

                EvaluationResults trainingResults = InternalEvaluator.EvaluateModelOnData(v1Train, v2Train);
                EvaluationResults testAndTrainResult = InternalEvaluator.EvaluateModelOnDataGivenParams(v1, v2, trainingResults);
                EvaluationResultsTestGivenTrain testGivenTrainResult = EvaluationResultsTestGivenTrain.GetInstance(this, trainingResults, testAndTrainResult);

                if (combinedResults == null) 
                { 
                    combinedResults = EvaluationResultsCrossValidate.GetInstance(this, testGivenTrainResult);
                } 
                else
                {
                    combinedResults = combinedResults.AddNewResults(testGivenTrainResult);
                }

                if (double.IsInfinity(combinedResults.AltLL))   // no point in continuing...infinity will kill everything. 
                { 
                    break;
                } 
#if DEBUG
                double eps = 1E-10;
                EvaluationResults testTrainingResults = InternalEvaluator.EvaluateModelOnDataGivenParams(v1Train, v2Train, trainingResults);
                Debug.Assert(ComplexNumber.ApproxEqual(testTrainingResults.AltLL, trainingResults.AltLL, eps) &&
                             ComplexNumber.ApproxEqual(testTrainingResults.NullLL, trainingResults.NullLL, eps));
                //Debug.Assert(testTrainingResults.Equals(trainingResults)); 
 
                double newNullLL = testAndTrainResult.NullLL - trainingResults.NullLL;
                double newAltLL = testAndTrainResult.AltLL - trainingResults.AltLL; 

                Debug.Assert(ComplexNumber.ApproxEqual(newNullLL, testGivenTrainResult.NullLL, eps));
                Debug.Assert(ComplexNumber.ApproxEqual(newAltLL, testGivenTrainResult.AltLL, eps));

                testNullLLSum += newNullLL;
                testAltLLSum += newAltLL; 
 
                Debug.Assert(ComplexNumber.ApproxEqual(testNullLLSum, combinedResults.NullLL, eps), "Combined result has wrong NullLL");
                Debug.Assert(ComplexNumber.ApproxEqual(testAltLLSum, combinedResults.AltLL, eps), "Combined result has wrong AltLL"); 
#endif

            }
            return combinedResults;
        }
 
 

        public override EvaluationResults EvaluateModelOnDataGivenParams(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2, EvaluationResults previousResults) 
        {
            return InternalEvaluator.EvaluateModelOnDataGivenParams(v1, v2, previousResults);
        }

        public override string ToHeaderString()
        { 
            return InternalEvaluator.ToHeaderString(); 
        }
 
        private Converter<Leaf, SufficientStatistics> CreateFilteredMap(Converter<Leaf, SufficientStatistics> map, Set<Leaf> keepOnlyTheseLeaves)
        {
            return delegate(Leaf leaf)
            {
                if (keepOnlyTheseLeaves.Contains(leaf))
                { 
                    return map(leaf); 
                }
                else 
                {
                    return MissingStatistics.Singleton;
                }
            };
        }
    } 
 
    class EvaluationResultsTestGivenTrain : EvaluationResults
    { 
        EvaluationResults _testAndTrain;

        protected EvaluationResultsTestGivenTrain(ModelEvaluator modelEval, EvaluationResults testAndTrain, List<Score> testGiveTrainNullScores, Score testGivenTrainAltScore)
            :
            base(modelEval, testGiveTrainNullScores, testGivenTrainAltScore, testAndTrain.ChiSquareDegreesOfFreedom, testAndTrain.GlobalNonMissingCount)
        { 
            _testAndTrain = testAndTrain; 
        }
 
        public static EvaluationResultsTestGivenTrain GetInstance(ModelEvaluator modelEval, EvaluationResults train, EvaluationResults testAndTrain)
        {
            List<Score> nullScores = new List<Score>(train.NullScores.Count);
            for (int i = 0; i < train.NullScores.Count; i++)
            {
                Score score = Score.GetInstance( 
                    testAndTrain.NullScores[i].Loglikelihood - train.NullScores[i].Loglikelihood, 
                    testAndTrain.NullScores[i].OptimizationParameters,
                    testAndTrain.NullScores[i].Distribution); 
                nullScores.Add(score);
            }
            Score altScore = Score.GetInstance(
                testAndTrain.AltScore.Loglikelihood - train.AltScore.Loglikelihood,
                testAndTrain.AltScore.OptimizationParameters,
                testAndTrain.AltScore.Distribution); 
 
            return new EvaluationResultsTestGivenTrain(modelEval, testAndTrain, nullScores, altScore);
        } 

        public override string IidStatsHeaderString()
        {
            return _testAndTrain.IidStatsHeaderString();
        }
 
        public override string IidStatsString() 
        {
            return _testAndTrain.IidStatsString(); 
        }
    }

    class EvaluationResultsCrossValidate : EvaluationResults
    {
        readonly EvaluationResults _representativeResults; 
        readonly int _numberOfResults; 

        protected EvaluationResultsCrossValidate(ModelEvaluator modelEval, List<Score> nullScores, Score altScore, EvaluationResults representativeResults, int numberOfResults) 
            :
            base(modelEval, nullScores, altScore, representativeResults.ChiSquareDegreesOfFreedom, representativeResults.GlobalNonMissingCount)
        {
            _representativeResults = representativeResults;
            _numberOfResults = numberOfResults;
        } 
 
        public static EvaluationResultsCrossValidate GetInstance(ModelEvaluator modelEval, EvaluationResults representativeResults)
        { 
            return new EvaluationResultsCrossValidate(modelEval, representativeResults.NullScores, representativeResults.AltScore, representativeResults, 1);
        }

        public EvaluationResultsCrossValidate AddNewResults(EvaluationResults newResults)
        {
            SpecialFunctions.CheckCondition(newResults.GetType() == _representativeResults.GetType(), "Evaluation results can only be combined if their types are identical."); 
 
            // first average the parameters.
            double newParamWeight = 1.0 / (_numberOfResults + 1); 
            List<Score> newNullScores = new List<Score>(NullScores.Count);

            for (int i = 0; i < NullScores.Count; i++)
            {
                Score newScore = CombineScores(NullScores[i], newResults.NullScores[i], newParamWeight);
                newNullScores.Add(newScore); 
            } 
            Score newAltScore = CombineScores(AltScore, newResults.AltScore, newParamWeight);
 
            EvaluationResultsCrossValidate result = new EvaluationResultsCrossValidate(ModelEvaluator, newNullScores, newAltScore, newResults, _numberOfResults + 1);
            return result;
        }

        private Score CombineScores(Score existingScore, Score newScoreToAdd, double pNewScoreToAdd)
        { 
            OptimizationParameterList newAverageParamList = AverageParamLists(existingScore.OptimizationParameters, newScoreToAdd.OptimizationParameters, pNewScoreToAdd); 
            double newLL = existingScore.Loglikelihood + newScoreToAdd.Loglikelihood;
            Score newScore = Score.GetInstance(newLL, newAverageParamList, existingScore.Distribution); 
            return newScore;
        }

        private OptimizationParameterList AverageParamLists(OptimizationParameterList existingParamList, OptimizationParameterList newParamList, double pNewScoreToAdd)
        {
            OptimizationParameterList resultParamList = existingParamList.Clone(); 
            foreach (OptimizationParameter param in newParamList) 
            {
                param.Value = (1 - pNewScoreToAdd) * param.Value + pNewScoreToAdd * newParamList[param.Name].Value; 
            }
            return resultParamList;
        }

        public override string IidStatsHeaderString()
        { 
            return _representativeResults.IidStatsHeaderString(); 
        }
 
        public override string IidStatsString()
        {
            return _representativeResults.IidStatsString();
        }
    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
