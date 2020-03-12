using System;
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.IO;
using System.Diagnostics;
using Optimization;
using Mlas.Tabulate;

namespace VirusCount.PhyloTree
{

    public class PhyloDDriver //where T:ISufficientStatistics
    {
        public SpecialFunctions SpecialFunctions = SpecialFunctions.GetInstance();


        protected PhyloDDriver() { }

        public static PhyloDDriver GetInstance()
        {
            return new PhyloDDriver();

        }

        public static string GetHeaderString(ModelEvaluator eval)
        {
            string result = SpecialFunctions.CreateTabString(
                "LeafDistribution",
                "rowIndex",
                "rowCount",
                "pieceIndex",
                Tabulate.NullIndexColumnName,
                Tabulate.PredictorVariableColumnName,
                Tabulate.TargetVariableColumnName,
                //Tabulate.GlobalNonMissingCountColumnName, //TODO: probably want this in somewhere 
                eval.ToHeaderString());
            return result;
        }

        public string Run(
                ModelEvaluator modelEvaluator,
            //PhyloTree phyloTree,
                string predictorSparseFileName,
                string targetSparseFileName,
                string leafDistributionName,
                string nullDataGeneratorName,
                KeepTest<Dictionary<string, string>> keepTest,
                RangeCollection skipRowIndexRangeCollectionOrNull,
                string shortName,
                string outputDirectoryName,
                RangeCollection pieceIndexRangeCollection, int pieceCount,
                RangeCollection nullIndexRangeCollection,
                string optimizerName)
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Directory.CreateDirectory(outputDirectoryName);




            #region from PhyloTree refactor
            //Dictionary<string, Dictionary<string, bool>> predictorVariableToCaseIdToRealNonMissingValue = LoadSparseFileInMemory<bool>(predictorSparseFileName); 
            //IEnumerable<Pair<string, Dictionary<string, T>>> targetNameAndCaseIdToNonMissingValueEnumeration = LoadSparseFileEnumeration<T>(targetSparseFileName);

            //NullDataCollection nullDataGenerator =
            //    NullDataCollection.GetInstance(this, modelTester, nullIndexRangeCollection, predictorVariableToCaseIdToRealNonMissingValue);

            //UniversalWorkList<T> workList = UniversalWorkList<T>.GetInstance( 
            //    predictorVariableToCaseIdToRealNonMissingValue, 
            //    targetNameAndCaseIdToNonMissingValueEnumeration,
            //    nullDataGenerator, nullIndexRangeCollection, keepTest); 
            #endregion
            bool speedOverMemory = true;

            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>>
                    predictorNameAndCaseIdToNonMissingValueEnumeration = CreateNameAndCaseIdToNonMissingValueEnumeration(predictorSparseFileName, speedOverMemory);
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>>
                    targetNameAndCaseIdToNonMissingValueEnumeration = CreateNameAndCaseIdToNonMissingValueEnumeration(targetSparseFileName, speedOverMemory);

            NullDataCollection nullDataGenerator =
                NullDataCollection.GetInstance(modelEvaluator.CreateNullDataGenerator(nullDataGeneratorName), nullIndexRangeCollection,
                    predictorNameAndCaseIdToNonMissingValueEnumeration, targetNameAndCaseIdToNonMissingValueEnumeration);


            UniversalWorkList workList = UniversalWorkList.GetInstance(
                predictorNameAndCaseIdToNonMissingValueEnumeration,
                targetNameAndCaseIdToNonMissingValueEnumeration,
                nullDataGenerator, nullIndexRangeCollection, keepTest);

            int workListCount = SpecialFunctions.Count(workList.List());

            int effectiveWorkListCount;
            if (skipRowIndexRangeCollectionOrNull == null)
            {
                effectiveWorkListCount = workListCount;
            }
            else
            {
                effectiveWorkListCount = 0;
                for (int iRowIndex = 0; iRowIndex < workListCount; iRowIndex++)
                {
                    if (!skipRowIndexRangeCollectionOrNull.Contains(iRowIndex))
                    {
                        effectiveWorkListCount++;
                    }
                }
            }
            Console.WriteLine("{0} Total rows. Skipping {1} of them.", workListCount, workListCount - effectiveWorkListCount);

            string outputFileName = string.Format(@"{0}\{1}.{2}.{3}.{4}.{5}.{6}{7}.txt",
                            outputDirectoryName, shortName,
                            modelEvaluator.Name, nullDataGenerator.Name,
                            nullIndexRangeCollection,
                            pieceCount,
                            pieceIndexRangeCollection,
                            skipRowIndexRangeCollectionOrNull == null ? "" : ".Skip" + skipRowIndexRangeCollectionOrNull.Count().ToString()
                        );

            using (TextWriter textWriter = File.CreateText(outputFileName))
            {
                textWriter.WriteLine(GetHeaderString(modelEvaluator));
                textWriter.Flush();
                int rowIndex = -1;
                int effectiveRowIndex = -1;

                foreach (RowData rowAndTargetData in workList.List())
                {
                    //TODOmake all these parameters and the calculation a class
                    ++rowIndex;
                    Debug.Assert(rowIndex < workListCount); // real assert

                    if (skipRowIndexRangeCollectionOrNull == null || !skipRowIndexRangeCollectionOrNull.Contains(rowIndex))
                    {
                        ++effectiveRowIndex;

                        int workIndex = ExtractWorkIndex(effectiveRowIndex, pieceCount, effectiveWorkListCount);

                        if (pieceIndexRangeCollection.Contains(workIndex))
                        {
                            Debug.WriteLine("WorkItemIndex " + rowIndex.ToString());
                            string reportLine;
                            try
                            {
                                reportLine =
                                    CreateReportLine(modelEvaluator, rowAndTargetData, workList, rowIndex, workListCount, workIndex);
                            }
                            catch (OutOfMemoryException)
                            {
                                Console.WriteLine("OUT OF MEMORY!! Clearing cache and trying to recover where we left off.");
                                modelEvaluator.ModelScorer.ClearCache();
                                reportLine =
                                    CreateReportLine(modelEvaluator, rowAndTargetData, workList, rowIndex, workListCount, workIndex);
                            }

                            textWriter.WriteLine(reportLine);
                            textWriter.Flush();
                        }
                    }
                }
            }
            stopwatch.Stop();
            Console.WriteLine("Running time: {0}", stopwatch.Elapsed);
            if (modelEvaluator.ModelScorer != null)
            {
                Console.WriteLine("Function calls (per ML call): {0} ({1:f4})", modelEvaluator.ModelScorer.FuncCalls, modelEvaluator.ModelScorer.FuncCalls / (double)(modelEvaluator.ModelScorer.CacheMisses));
                Console.WriteLine("Cache hits (%): {0} ({1:f4})", modelEvaluator.ModelScorer.CacheHits, 100 * modelEvaluator.ModelScorer.CacheHits / (double)(modelEvaluator.ModelScorer.CacheHits + modelEvaluator.ModelScorer.CacheMisses));
                Console.WriteLine("Cache clears: {0}", modelEvaluator.ModelScorer.CacheClears);
            }

            return outputFileName;
        }




        private string CreateReportLine(
            ModelEvaluator modelEvaluator,
            RowData rowAndTargetData,
            UniversalWorkList workList,
            int rowIndex, int workListCount, int workIndex)
        {
            Dictionary<string, string> row = rowAndTargetData.Row;
            string predictorVariable = row[Tabulate.PredictorVariableColumnName];
            string targetVariable = row[Tabulate.TargetVariableColumnName];
            int nullIndex = int.Parse(row[Tabulate.NullIndexColumnName]);

            Dictionary<string, SufficientStatistics> caseIdToNonMissingPredictorValue = rowAndTargetData.PredictorData;
            Dictionary<string, SufficientStatistics> caseIdToNonMissingTargetValue = rowAndTargetData.TargetData;

            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction = CreateSufficientStatisticsMap(caseIdToNonMissingPredictorValue);
            Converter<Leaf, SufficientStatistics> targetDistributionClassFunction = CreateSufficientStatisticsMap(caseIdToNonMissingTargetValue);

            EvaluationResults results = modelEvaluator.EvaluateModelOnData(predictorDistributionClassFunction, targetDistributionClassFunction);

            string reportLine = SpecialFunctions.CreateTabString(
                results.ModelEvaluator.Name, rowIndex, workListCount, workIndex, nullIndex, predictorVariable, targetVariable, results.ToString());

            return reportLine;
        }


        //TODO strange name...
        public void ScoreTree(
            ModelScorer modelScorer,
            PhyloTree phyloTree,
            string predictorSparseFileName,
            string targetSparseFileName,
            string predictorVariableName,
            string targetVariableName,
            double[] nullModelArgs,
            double[] altModelArgs)
        {
            throw new NotImplementedException();
            ////Dictionary<string, Dictionary<string, SufficientStatistics>> predictorVariableToCaseIdToRealNonMissingValue = LoadSparseFileInMemory(predictorSparseFileName); 
            //IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration = LoadSparseFileEnumeration(predictorSparseFileName); 
            //IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration = LoadSparseFileEnumeration(targetSparseFileName);

            //RangeCollection nullIndexRangeCollection = RangeCollection.GetInstance(-1, -1);
            //NullDataCollection nullDataGenerator = null;
            //    //CreateNullDataGenerator("PredictorPermutation", modelScorer, phyloTree, nullIndexRangeCollection,
            //    //predictorNameAndCaseIdToNonMissingValueEnumeration, targetNameAndCaseIdToNonMissingValueEnumeration);

            //UniversalWorkList workList = UniversalWorkList.GetInstance( 
            //    predictorNameAndCaseIdToNonMissingValueEnumeration, 
            //    targetNameAndCaseIdToNonMissingValueEnumeration,
            //    //targetNameAndCaseIdToNonMissingValueEnumeration, 
            //    nullDataGenerator, nullIndexRangeCollection, AlwaysKeep<Dictionary<string, string>>.GetInstance());


            //foreach (RowData rowAndTargetData in workList.List())
            //{
            //    if (rowAndTargetData.Row[Tabulate.PredictorVariableColumnName] == predictorVariableName && 
            //        rowAndTargetData.Row[Tabulate.TargetVariableColumnName] == targetVariableName) 
            //    {
            //        Dictionary<string, SufficientStatistics> caseIdToNonNullPredictorValue = rowAndTargetData.PredictorData;//workList.GetCaseIdToNonMissingValueForNullIndexAndPredictorVariable(-1, predictorVariableName); 
            //        Dictionary<string, SufficientStatistics> caseIdToNonMissingTargetValue = rowAndTargetData.TargetData;

            //        Converter<Leaf, SufficientStatistics> targetDistributionMap = CreateSufficientStatisticsMap(caseIdToNonMissingTargetValue);
            //        Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction = CreateSufficientStatisticsMap(caseIdToNonNullPredictorValue);
            //        Converter<Leaf, SufficientStatistics> altDistributionMap = CreateAlternativeSufficientStatisticsMap(predictorDistributionClassFunction, targetDistributionMap);
            //        double logLikelihood; 
            //        Score scoreIndTarget, scoreIndPredictor, scoreAlt; 
            //        MessageInitializer messageInitializer;
            //        OptimizationParameterList nullParams = SingleVarModelDistribution.GetParameters(nullModelArgs); 
            //        OptimizationParameterList altParams = TwoVarModelDistribution.GetParameters(altModelArgs);

            //        Console.WriteLine(SpecialFunctions.CreateTabString("Variable", nullParams.ToStringHeader(), "LogL"));
            //        messageInitializer = modelScorer.CreateMessageInitializer(predictorDistributionClassFunction, targetDistributionMap, SingleVarModelDistribution);
            //        logLikelihood = modelScorer.ComputeLogLikelihoodModelGivenData(messageInitializer, nullParams);
            //        scoreIndTarget = Score.GetInstance(logLikelihood, nullParams); 
            //        Console.WriteLine("Target\t" + scoreIndTarget); 

            //        messageInitializer = modelScorer.CreateMessageInitializer(targetDistributionMap, predictorDistributionClassFunction, SingleVarModelDistribution); 
            //        logLikelihood = modelScorer.ComputeLogLikelihoodModelGivenData(messageInitializer, nullParams);
            //        modelScorer.ComputeLogLikelihoodModelGivenData(messageInitializer, nullParams);
            //        scoreIndPredictor = Score.GetInstance(logLikelihood, nullParams);
            //        Console.WriteLine("Predictor\t" + scoreIndPredictor);

            //        Console.WriteLine("\n" + SpecialFunctions.CreateTabString("Variable", altParams.ToStringHeader(), "LogL")); 
            //        messageInitializer = modelScorer.CreateMessageInitializer(null, altDistributionMap, TwoVarModelDistribution); 
            //        logLikelihood = modelScorer.ComputeLogLikelihoodModelGivenData(messageInitializer, altParams);
            //        scoreAlt = Score.GetInstance(logLikelihood, altParams); 
            //        Console.WriteLine(SpecialFunctions.CreateTabString(TwoVarModelDistribution, scoreAlt));
            //    }
            //}
        }

        public static Converter<Leaf, SufficientStatistics> CreateSufficientStatisticsMap(Dictionary<string, SufficientStatistics> caseIdToNonMissingValue)
        {
            return SufficientStatistics.DictionaryToLeafMap(caseIdToNonMissingValue);
        }

        #region Helper methods
        private IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> CreateNameAndCaseIdToNonMissingValueEnumeration(string predictorSparseFileName, bool speedOverMemory)
        {
            //Dictionary<string, Dictionary<string, SufficientStatistics>> predictorVariableToCaseIdToRealNonMissingValue = LoadSparseFileInMemory(predictorSparseFileName);
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration;
            if (speedOverMemory)
            {
                Dictionary<string, Dictionary<string, SufficientStatistics>> predictorVariableToCaseIdToRealNonMissingValue = LoadSparseFileInMemory(predictorSparseFileName);
                predictorNameAndCaseIdToNonMissingValueEnumeration = SpecialFunctions.DictionaryToPairEnumeration(predictorVariableToCaseIdToRealNonMissingValue);
            }
            else
            {
                predictorNameAndCaseIdToNonMissingValueEnumeration = LoadSparseFileEnumeration(predictorSparseFileName);
            }
            return predictorNameAndCaseIdToNonMissingValueEnumeration;
        }

        private static int ExtractWorkIndex(int rowIndex, int pieceCount, int workListCount)
        {
            int workIndex = (int)((long)rowIndex * (long)pieceCount / workListCount); //note: Integer divide, long to avoid overflow
            return workIndex;
        }

        private static bool ProcessRow(RangeCollection skipRowIndexRangeCollectionOrNull, RangeCollection pieceIndexRangeCollection, int rowIndex, int workIndex)
        {
            bool doTheWork = true;
            if (skipRowIndexRangeCollectionOrNull != null && skipRowIndexRangeCollectionOrNull.Contains(rowIndex))
            {
                doTheWork = false;
            }

            if (!pieceIndexRangeCollection.Contains(workIndex))
            {
                doTheWork = false;
            }
            return doTheWork;
        }
        #endregion

        #region Static methods
        //public static IEnumerable<List<string>> GetDistributionNames() 
        //{ 
        //    string[][] distributions = {
        //        new string[]{ "Discrete" , "Conditional" , "Attraction" }, 
        //        new string[]{ "Discrete" , "Conditional" , "Escape" },
        //        new string[]{ "Discrete" , "Conditional" , "Reversion" },
        //        new string[]{ "Discrete" , "Conditional" , "Repulsion" },
        //        new string[]{ "Discrete" , "Joint" , "Undirected" },
        //        new string[]{ "Discrete" , "FishersExactTest" },
        //        new string[]{ "Gaussian" , "Conditional" , "Reversible" }, 
        //        new string[]{ "Gaussian" , "Conditional" , "BrownianMotion"} 
        //    };

        //    foreach (string[] distn in distributions)
        //    {
        //        yield return new List<string>(distn);
        //    }
        //}

        /** 
         * Get's the names of distributions that we wish to "publish" to the outside world.
         * These (may) differ from GetDistributionNames() by omitting distributions that are 
         * for internal and/or testing use only.
         */
        public static IEnumerable<string> GetPublishedDistributionNames()
        {
            string[] distributions = {
                "DiscreteConditionalAttraction", 
                "DiscreteConditionalEscape", 
                "DiscreteConditionalReversion",
                "DiscreteConditionalRepulsion", 
                //"DiscreteJointUndirected",
                "DiscreteFishersExactTest",
                "GaussianConditionalReversible",
                "GaussianConditionalBrownianMotion"
            };

            foreach (string distn in distributions)
            {
                yield return distn;
            }
        }

        /**
         * Get's all currently supported distribution names.
         * Note that "CrossValidate" can be prepended to any of these. 
         */
        public static IEnumerable<string> GetDistributionNames()
        {
            string[] distributions = {
                "GaussianConditionalIid",
                "DiscreteConditionalCollectionOneDirection",
                "DiscreteConditionalCollectionBothDirections"
            };

            foreach (string distn in GetPublishedDistributionNames())
            {
                yield return distn;
            }
            foreach (string distn in distributions)
            {
                yield return distn;
            }
        }

        /**
         * Returns true iff distributionName is enumerated by GetDistributionNames() (ignoring a CrossValidate prefix) 
         */
        public static bool ValidateDistribution(string distributionName)
        {
            distributionName = distributionName.ToLower();

            if (distributionName.StartsWith(ModelEvaluatorCrossValidate.BaseName.ToLower()))
            {
                distributionName = distributionName.Substring(ModelEvaluatorCrossValidate.BaseName.Length);
            }
            foreach (string distn in GetDistributionNames())
            {
                if (distn.Equals(distributionName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        static public Dictionary<string, Dictionary<string, SufficientStatistics>> LoadSparseFileInMemory(string sparseFileName) //where TStat:ISufficientStatistics
        {
            Dictionary<string, Dictionary<string, SufficientStatistics>> variableToCaseIdToNonMissingValue =
                new Dictionary<string, Dictionary<string, SufficientStatistics>>();
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(sparseFileName, "var\tcid\tval", false))
            {
                string variable = row["var"];
                string caseId = row["cid"];
                SufficientStatistics val = SufficientStatistics.Parse(row["val"]);

                Dictionary<string, SufficientStatistics> caseIdToNonMissingValue = SpecialFunctions.GetValueOrDefault(variableToCaseIdToNonMissingValue, variable);
                SpecialFunctions.CheckCondition(!caseIdToNonMissingValue.ContainsKey(caseId), string.Format("Input file ({0}) for var {1} contains multiple entries for caseId {2}", sparseFileName, variable, caseId));
                caseIdToNonMissingValue.Add(caseId, val);
            }
            return variableToCaseIdToNonMissingValue;
        }

        static public IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> LoadSparseFileEnumeration(string sparseFileName) //where T1:ISufficientStatistics
        {
            Set<string> variablesAlreadySeenSet = new Set<string>();

            Pair<string, Dictionary<string, SufficientStatistics>> variableAndCaseIdToNonMissingValue = null;
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(sparseFileName, "var\tcid\tval", false))
            {
                string variable = row["var"];
                if (variableAndCaseIdToNonMissingValue != null && variableAndCaseIdToNonMissingValue.First != variable)
                {
                    yield return variableAndCaseIdToNonMissingValue;
                    variableAndCaseIdToNonMissingValue = null;
                }
                if (variableAndCaseIdToNonMissingValue == null)
                {
                    SpecialFunctions.CheckCondition(!variablesAlreadySeenSet.Contains(variable), string.Format("Input file ({0}) is not grouped by variable. Variable {1} appears in multiple places", sparseFileName, variable));
                    variablesAlreadySeenSet.AddNew(variable);
                    variableAndCaseIdToNonMissingValue =
                        new Pair<string, Dictionary<string, SufficientStatistics>>(variable, new Dictionary<string, SufficientStatistics>());
                }
                string caseId = row["cid"];
                SufficientStatistics val = SufficientStatistics.Parse(row["val"]);

                SpecialFunctions.CheckCondition(!variableAndCaseIdToNonMissingValue.Second.ContainsKey(caseId), string.Format("Input file ({0}) for var {1} contains multiple entries for caseId {2}", sparseFileName, variable, caseId));
                variableAndCaseIdToNonMissingValue.Second.Add(caseId, val);
            }

            if (variableAndCaseIdToNonMissingValue != null)
            {
                yield return variableAndCaseIdToNonMissingValue;
            }
        }
        #endregion
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
