using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.IO;
using System.Diagnostics;
using Optimization; 
 
namespace VirusCount.PhyloTree
{ 

    public abstract class ModelTester //where T:ISufficientStatistics
    {
        public SpecialFunctions SpecialFunctions = SpecialFunctions.GetInstance();

 
 
        IDistribution _nullModelDistn, _altModelDistn;
        private readonly int _chiSquareDegreesOfFreedom; 

        protected ModelTester(IDistribution nullModel, IDistribution altModel)
        {
            _nullModelDistn = nullModel;
            _altModelDistn = altModel;
 
            if (_nullModelDistn != null) 
            {
                int altFreeParams = _altModelDistn.GetParameters(true).CountFreeParameters(); 
                int nullFreeParams = _nullModelDistn.GetParameters(false).CountFreeParameters();
                _chiSquareDegreesOfFreedom = altFreeParams - nullFreeParams;
            }
        }

        public static ModelTester GetInstance(String modelTesterNameAndArguments) 
        { 
            if (modelTesterNameAndArguments.StartsWith(ModelTesterDiscrete.Name))
            { 
                return ModelTesterDiscrete.GetInstance(modelTesterNameAndArguments.Substring(ModelTesterDiscrete.Name.Length));
            }
            else if (modelTesterNameAndArguments.StartsWith(ModelTesterGaussian.Name))
            {
                return ModelTesterGaussian.GetInstance(modelTesterNameAndArguments.Substring(ModelTesterGaussian.Name.Length));
            } 
            else 
            {
                throw new ArgumentException("Cannot parse " + modelTesterNameAndArguments + " into a model tester."); 
            }
        }

        public IDistribution NullModelDistribution
        {
            get { return _nullModelDistn; } 
        } 
        public IDistribution AltModelDistribution
        { 
            get { return _altModelDistn; }
        }

        protected virtual int ChiSquareDegreesOfFreedom
        {
            get { return _chiSquareDegreesOfFreedom; } 
        } 

        public abstract string Header 
        {
            get;
        }

        public void Run(
                ModelScorer modelScorer, 
                PhyloTree phyloTree, 
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
 
 
            string outputFileName = string.Format(@"{0}\{1}.{2}.{3}.{4}.{5}.{6}{7}.txt",
                                        outputDirectoryName, shortName, 
                                        leafDistributionName, nullDataGeneratorName,
                                        nullIndexRangeCollection,
                                        pieceCount,
                                        pieceIndexRangeCollection,
                                        skipRowIndexRangeCollectionOrNull == null ? "" : ".Skip" + skipRowIndexRangeCollectionOrNull.Count().ToString()
                                    ); 
 
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

            NullDataCollection nullDataGenerator= 
                CreateNullDataGenerator(nullDataGeneratorName, modelScorer, phyloTree, nullIndexRangeCollection,
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

            using (TextWriter textWriter = File.CreateText(outputFileName))
            {
                textWriter.WriteLine(Header);
                int rowIndex = -1;
                int effectiveRowIndex = -1; 
 
                foreach (RowData rowAndTargetData in workList.List())
                { 
                    //!!!make all these parameters and the calculation a class
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
                                    CreateReportLine(modelScorer, phyloTree, rowAndTargetData, workList, rowIndex, workListCount, workIndex); 
                            }
                            catch (OutOfMemoryException)
                            {
                                Console.WriteLine("OUT OF MEMORY!! Clearing cache and trying to recover where we left off.");
                                modelScorer.ClearCache();
                                reportLine = 
                                    CreateReportLine(modelScorer, phyloTree, rowAndTargetData, workList, rowIndex, workListCount, workIndex); 
                            }
 
                            textWriter.WriteLine(reportLine);
                            textWriter.Flush();
                        }
                    }
                }
            } 
            stopwatch.Stop(); 
            Console.WriteLine("Running time: " + stopwatch.Elapsed);
        } 

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

        protected void CompleteRowWithNaN(StringBuilder stringBuilder)
        {
            int columns = Header.Split('\t').Length;
            string[] stringBuilderColumns = stringBuilder.ToString().Split('\t'); 
            int columnsThusFar = stringBuilderColumns.Length; 
            if (stringBuilderColumns[columnsThusFar - 1] == "")
                columnsThusFar--;   // arises if we have a trailing tab. 

            int columnsRemaining = columns - columnsThusFar;

            for (int i = 0; i < columnsRemaining; i++)
            {
                if (i > 0) 
                    stringBuilder.Append('\t'); 
                stringBuilder.Append(double.NaN);
            } 
        }

        protected abstract string CreateReportLine(
            ModelScorer modelScorer,
            PhyloTree phyloTree,
            RowData rowAndTargetData, 
            UniversalWorkList workList, 
            int rowIndex, int workListCount, int workIndex);
 


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
            //Dictionary<string, Dictionary<string, SufficientStatistics>> predictorVariableToCaseIdToRealNonMissingValue = LoadSparseFileInMemory(predictorSparseFileName);
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration = LoadSparseFileEnumeration(predictorSparseFileName);
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration = LoadSparseFileEnumeration(targetSparseFileName); 
 
            RangeCollection nullIndexRangeCollection = RangeCollection.GetInstance(-1, -1);
            NullDataCollection nullDataGenerator = 
                CreateNullDataGenerator("PredictorPermutation", modelScorer, phyloTree, nullIndexRangeCollection,
                predictorNameAndCaseIdToNonMissingValueEnumeration, targetNameAndCaseIdToNonMissingValueEnumeration);

            UniversalWorkList workList = UniversalWorkList.GetInstance(
                predictorNameAndCaseIdToNonMissingValueEnumeration,
                targetNameAndCaseIdToNonMissingValueEnumeration, 
                //targetNameAndCaseIdToNonMissingValueEnumeration, 
                nullDataGenerator, nullIndexRangeCollection, AlwaysKeep<Dictionary<string, string>>.GetInstance());
 

            foreach (RowData rowAndTargetData in workList.List())
            {
                if (rowAndTargetData.Row[PhyloTree.PredictorVariableColumnName] == predictorVariableName &&
                    rowAndTargetData.Row[PhyloTree.TargetVariableColumnName] == targetVariableName)
                { 
                    Dictionary<string, SufficientStatistics> caseIdToNonNullPredictorValue = rowAndTargetData.PredictorData;//workList.GetCaseIdToNonMissingValueForNullIndexAndPredictorVariable(-1, predictorVariableName); 
                    Dictionary<string, SufficientStatistics> caseIdToNonMissingTargetValue = rowAndTargetData.TargetData;
 
                    Converter<Leaf, SufficientStatistics> targetDistributionMap = CreateSufficientStatisticsMap(caseIdToNonMissingTargetValue);
                    Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction = CreateSufficientStatisticsMap(caseIdToNonNullPredictorValue);
                    Converter<Leaf, SufficientStatistics> altDistributionMap = CreateAlternativeSufficientStatisticsMap(predictorDistributionClassFunction, targetDistributionMap);
                    double logLikelihood;
                    Score scoreIndTarget, scoreIndPredictor, scoreAlt;
                    MessageInitializer messageInitializer; 
                    OptimizationParameterList nullParams = NullModelDistribution.GetParameters(nullModelArgs); 
                    OptimizationParameterList altParams = AltModelDistribution.GetParameters(altModelArgs);
 
                    Console.WriteLine(SpecialFunctions.CreateTabString("Variable", nullParams.ToStringHeader(), "LogL"));
                    messageInitializer = modelScorer.CreateMessageInitializer(predictorDistributionClassFunction, targetDistributionMap, NullModelDistribution);
                    logLikelihood = modelScorer.ComputeLogLikelihoodModelGivenData(messageInitializer, nullParams);
                    scoreIndTarget = Score.GetInstance(logLikelihood, nullParams);
                    Console.WriteLine("Target\t" + scoreIndTarget);
 
                    messageInitializer = modelScorer.CreateMessageInitializer(targetDistributionMap, predictorDistributionClassFunction, NullModelDistribution); 
                    logLikelihood = modelScorer.ComputeLogLikelihoodModelGivenData(messageInitializer, nullParams);
                    modelScorer.ComputeLogLikelihoodModelGivenData(messageInitializer, nullParams); 
                    scoreIndPredictor = Score.GetInstance(logLikelihood, nullParams);
                    Console.WriteLine("Predictor\t" + scoreIndPredictor);

                    Console.WriteLine("\n" + SpecialFunctions.CreateTabString("Variable", altParams.ToStringHeader(), "LogL"));
                    messageInitializer = modelScorer.CreateMessageInitializer(null, altDistributionMap, AltModelDistribution);
                    logLikelihood = modelScorer.ComputeLogLikelihoodModelGivenData(messageInitializer, altParams); 
                    scoreAlt = Score.GetInstance(logLikelihood, altParams); 
                    Console.WriteLine(SpecialFunctions.CreateTabString(AltModelDistribution, scoreAlt));
                } 
            }
        }

        public Converter<Leaf, SufficientStatistics> CreateSufficientStatisticsMap(Dictionary<string, SufficientStatistics> caseIdToNonMissingValue)
        {
            return SufficientStatistics.DictionaryToLeafMap(caseIdToNonMissingValue); 
        } 

        //public Converter<Leaf, SufficientStatistics> CreateTargetSufficientStatisticsMap(Dictionary<string, ISufficientStatistics> caseIdToNonMissingValue); 

        public abstract Converter<Leaf, SufficientStatistics> CreateAlternativeSufficientStatisticsMap(
            Converter<Leaf, SufficientStatistics> predictorDistnClassFunction,
            Converter<Leaf, SufficientStatistics> targetDistnClassFunction);

 
        protected NullDataCollection CreateNullDataGenerator( 
            string nullDataGeneratorName,
            ModelScorer modelScorer, 
            PhyloTree phyloTree,
            RangeCollection nullIndexRangeCollection,
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration,
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration)
        {
            return NullDataCollection.GetInstance( 
                NullDataGenerator.GetInstance(nullDataGeneratorName, modelScorer, phyloTree, this), 
                nullIndexRangeCollection,
                predictorNameAndCaseIdToNonMissingValueEnumeration, 
                targetNameAndCaseIdToNonMissingValueEnumeration);
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
 
        //static private ISufficientStatistics ParseValue(string val)
        //{
        //    if (val == "0" || val == "1")
        //    {
        //        BooleanStatistics.GetInstance(val == "1");
        //    } 
        //    else 
        //    {
        //        GaussianStatistics.Parse(val); 
        //    }
        //    if (type == typeof(BooleanStatistics))
        //    {
        //        SpecialFunctions.CheckCondition(val == "0" || val == "1", "Expect value of 0 or 1");
        //        return BooleanStatistics.GetInstance(val == "1");
        //    } 
        //    else if (type == typeof(GaussianStatistics)) 
        //    {
        //        return GaussianStatistics.Parse(val); 
        //    }
        //    else
        //    {
        //        SpecialFunctions.CheckCondition(false, "Don't know how to parse " + type.ToString());
        //        return null;
        //    } 
        //} 

 
    }




 
 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
