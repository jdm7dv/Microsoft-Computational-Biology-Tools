using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.IO;
using System.Diagnostics;
using Optimization; 
 
namespace VirusCount.PhyloTree
{ 
    abstract public class ModelTesterGaussian : ModelTester
    {
        public const string Name = "Gaussian";

        //protected DistributionGaussian GaussianDistribution
        //{ 
        //    get { return (DistributionGaussian)NullModelDistribution; } 
        //}
 
        internal ModelTesterGaussian(IDistribution gaussianDistn) : base(gaussianDistn, gaussianDistn)
        { }

        new public static ModelTesterGaussian GetInstance(string name)
        {
            return ModelTesterGaussianConditional.GetInstance(name); 
        } 

        public override string ToString() 
        {
            return NullModelDistribution.ToString();
        }
    }

    public class ModelTesterGaussianConditional : ModelTesterGaussian 
    { 

        protected ModelTesterGaussianConditional(IDistribution gaussianDistn) 
            :
            base(gaussianDistn) { }

        new public static ModelTesterGaussianConditional GetInstance(string gaussianDistnName)
        {
            IDistribution distn; 
            if (gaussianDistnName =="Discrete") 
            {
                distn = DistributionDiscreteGaussianBinary.GetInstance(); 
            }
            else
            {
                distn = DistributionGaussian.GetInstance(gaussianDistnName);
            }
            return new ModelTesterGaussianConditional(distn); 
        } 

        public override string Header 
        {
            get
            {
                OptimizationParameterList exampleParameters;
                if (NullModelDistribution is DistributionGaussian)
                    exampleParameters = ((DistributionGaussian)NullModelDistribution).GetParameters(false, false); 
                else 
                    exampleParameters = NullModelDistribution.GetParameters(false);
 
                return SpecialFunctions.CreateTabString(
                        "LeafDistribution",
                        "rowIndex",
                        "rowCount",
                        "pieceIndex",
                        PhyloTree.NullIndexColumnName, 
                        PhyloTree.PredictorVariableColumnName, 
                        PhyloTree.PredictorFalseNameCountColumnName,
                        PhyloTree.PredictorTrueNameCountColumnName, 
                        PhyloTree.PredictorNonMissingCountColumnName,
                        PhyloTree.TargetVariableColumnName,
                        PhyloTree.TargetNonMissingCountColumnName,
                        PhyloTree.GlobalNonMissingCountColumnName,
                        exampleParameters.ToStringHeader().Replace("\t", "0\t"),
                        "logLikelihood0", 
                        exampleParameters.ToStringHeader().Replace("\t", "1\t"), 
                        "logLikelihood1",
                        "diff", 
                        "PValue");
            }
        }

        //public override Converter<Leaf, SufficientStatistics> CreateTargetSufficientStatisticsMap(Dictionary<string, ISufficientStatistics> caseIdToNonMissingValue)
        //{ 
        //    return ISufficientStatistics.DictionaryToLeafMap(caseIdToNonMissingValue); 
        //}
 
        public override Converter<Leaf, SufficientStatistics> CreateAlternativeSufficientStatisticsMap(
            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction,
            Converter<Leaf, SufficientStatistics> targetDistributionClassFunction)
        {
            return delegate(Leaf leaf)
                { 
                    SufficientStatistics predStats = predictorDistributionClassFunction(leaf); 
                    SufficientStatistics targetStats = targetDistributionClassFunction(leaf);
 

                    // bail on missing data.
                    if (predStats.IsMissing() || targetStats.IsMissing())
                    {
                        return GaussianStatistics.GetMissingInstance();
                    } 
                    else 
                    {
                        return targetStats; 
                    }
                };
        }

        protected override string CreateReportLine(
            ModelScorer modelScorer, 
            PhyloTree phyloTree, 
            RowData rowAndTargetData,
            UniversalWorkList workList, 
            int rowIndex, int workListCount, int workIndex)
        {
            //!!!there is very similar code in ModelTesterDiscrete.cs

            Dictionary<string, string> row = rowAndTargetData.Row;
            string predictorVariable = row[PhyloTree.PredictorVariableColumnName]; 
            string targetVariable = row[PhyloTree.TargetVariableColumnName]; // e.g. A@182 (amino acid "A" at position 182) 
            int nullIndex = int.Parse(row[PhyloTree.NullIndexColumnName]);
 
            //Dictionary<string, bool> caseIdToNonNullPredictorValue = workList.NullIndexToPredictorToCaseIdToNonMissingValue[nullIndex][predictorVariable];
            Dictionary<string, SufficientStatistics> caseIdToNonNullPredictorValue = rowAndTargetData.PredictorData; //workList.GetCaseIdToNonMissingValueForNullIndexAndPredictorVariable(nullIndex, predictorVariable);
            Dictionary<string, SufficientStatistics> caseIdToNonMissingTargetValue = rowAndTargetData.TargetData;

            Converter<Leaf, SufficientStatistics> targetDistributionMap = CreateSufficientStatisticsMap(caseIdToNonMissingTargetValue);
            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction = CreateSufficientStatisticsMap(caseIdToNonNullPredictorValue); 
 
            int[] predictorCounts = phyloTree.CountsOfLeaves(predictorDistributionClassFunction);
 
            int predictorFalseNameCount = predictorCounts[(int)DistributionDiscreteBinary.DistributionClass.False];
            int predictorTrueNameCount = predictorCounts[(int)DistributionDiscreteBinary.DistributionClass.True];
            int targetNonMissingCount = phyloTree.CountOfNonMissingLeaves(caseIdToNonMissingTargetValue);
            int globalNonMissingCount = phyloTree.GlobalNonMissingCount(predictorDistributionClassFunction, targetDistributionMap);

            StringBuilder stringBuilder = new StringBuilder( 
                SpecialFunctions.CreateTabString( 
                    this, rowIndex, workListCount, workIndex, nullIndex, predictorVariable,
                    predictorFalseNameCount, 
                    predictorTrueNameCount,
                    predictorTrueNameCount + predictorFalseNameCount,
                    targetVariable,
                    targetNonMissingCount,
                    globalNonMissingCount,
                    "")); 
 
            bool ignoreRow = false;
            foreach (int count in predictorCounts) 
            {
                if (count == 0)
                    ignoreRow = true;
            }

            if (ignoreRow) 
            { 
                CompleteRowWithNaN(stringBuilder);
            } 
            else
            {
                List<double> logLikelihoodList = new List<double>();
                MessageInitializer messageInitializer =
                    modelScorer.CreateMessageInitializer(predictorDistributionClassFunction, targetDistributionMap, NullModelDistribution);
                NullModelDistribution.InitialParamVals = null; 
                foreach (bool useParameter in new bool[] { false, true }) 
                {
                    Score score = modelScorer.ScoreModel(messageInitializer, useParameter); 
                    stringBuilder.Append(SpecialFunctions.CreateTabString(score, ""));
                    Debug.Write(SpecialFunctions.CreateTabString(score, ""));
                    logLikelihoodList.Add(score.Loglikelihood);
                    AltModelDistribution.InitialParamVals = score.OptimizationParameters;
                }
 
                double diff = logLikelihoodList[1] - logLikelihoodList[0]; 
                double pValue = SpecialFunctions.LogLikelihoodRatioTest(Math.Max(diff, 0), ChiSquareDegreesOfFreedom);
 
                stringBuilder.Append(SpecialFunctions.CreateTabString(diff, pValue));
                Debug.WriteLine(SpecialFunctions.CreateTabString(diff, pValue));
            }
            return stringBuilder.ToString();
        }
 
 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
