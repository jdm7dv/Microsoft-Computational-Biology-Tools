using System; 
using System.Collections.Generic;
using System.Text;
using VirusCount.PhyloTree;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using System.IO; 
 
namespace Mlas.Tabulate
{ 
    public static class Tabulate
    {
        //Similar to the other tabulators, but can work with multiple sets of pValues files
        //!!!would be better if could cut off really bad pValues to save memory
        //!!! also would be nice to have filters
        public static void CreateTabulateReport(ICollection<string> inputFilePatternCollection, string outputFileName, 
            KeepTest<Dictionary<string, string>> keepTest, double maxPValue, bool auditRowIndexValues) 
        {
            //SpecialFunctions.CheckCondition(!File.Exists(outputFileName), "Output file already exists: " + outputFileName); 
            using (TextWriter textWriter = File.CreateText(outputFileName)) // Do this early so that if it fails, well know
            {
                List<Dictionary<string, string>> realRowCollectionToSort = new List<Dictionary<string, string>>();
                List<double> nullValueCollectionToBeSorted = new List<double>();

                string headerSoFar = null; 
 
                Set<int> broadRealAndNullIndexSetSoFar = null;
 
                foreach (string broadInputFilePattern in inputFilePatternCollection)
                {
                    Set<int> narrowRealAndNullIndexSetSetSoFar = Set<int>.GetInstance();

                    foreach (string narrowInputFilePattern in broadInputFilePattern.Split('+'))
                    { 
                        Set<int> realAndNullIndexSet = 
                            CreateTabulateReportInternal(narrowInputFilePattern, keepTest, maxPValue, auditRowIndexValues,
                            ref realRowCollectionToSort, ref nullValueCollectionToBeSorted, ref headerSoFar); 

                        //Instead of throwing an error, we could filter out the duplicated null indexes
                        SpecialFunctions.CheckCondition(narrowRealAndNullIndexSetSetSoFar.IntersectionIsEmpty(realAndNullIndexSet),
                            string.Format("Within inputFilePattern {0}, multiple '+'-connected parts cover the same nullIndex(s), {1}",
                            broadInputFilePattern,
                            narrowRealAndNullIndexSetSetSoFar.Intersection(realAndNullIndexSet))); 
 
                        narrowRealAndNullIndexSetSetSoFar.AddNewRange(realAndNullIndexSet);
                    } 

                    SpecialFunctions.CheckCondition(!auditRowIndexValues || narrowRealAndNullIndexSetSetSoFar.Contains(-1),
                        string.Format("The 'null' index -1 for the real data was not seen in {0}", broadInputFilePattern));


                    if (broadRealAndNullIndexSetSoFar == null) 
                    { 
                        broadRealAndNullIndexSetSoFar = narrowRealAndNullIndexSetSetSoFar;
                    } 
                    else
                    {
                        SpecialFunctions.CheckCondition(broadRealAndNullIndexSetSoFar.Equals(narrowRealAndNullIndexSetSetSoFar),
                            string.Format("The broad inputFilePattern {0} covers a different set of nullIndexes ({1}) than its predecessors ({2})",
                            broadInputFilePattern, narrowRealAndNullIndexSetSetSoFar, broadRealAndNullIndexSetSoFar));
                    } 
 
                }
 
                double numberOfRandomizationRuns = broadRealAndNullIndexSetSoFar.Count - 1;
                Console.WriteLine("Detected {0} randomized runs relative to the number of real runs.", numberOfRandomizationRuns);
                Dictionary<Dictionary<string, string>, double> qValueList = SpecialFunctions.ComputeQValues(ref realRowCollectionToSort, AccessPValueFromPhylotreeRow, ref nullValueCollectionToBeSorted, numberOfRandomizationRuns);

                //!!!this code is repeated elsewhere
                textWriter.WriteLine(SpecialFunctions.CreateTabString(headerSoFar, "qValue")); 
                foreach (Dictionary<string, string> row in realRowCollectionToSort) 
                {
                    double qValue = qValueList[row]; 
                    textWriter.WriteLine(SpecialFunctions.CreateTabString(row[""], qValue));
                }



 
            } 
        }
 
        public static double AccessPValueFromPhylotreeRow(Dictionary<string, string> row)
        {
            try
            {
                double pValue = double.Parse(row["PValue"]);
                return pValue; 
            } 
            catch (KeyNotFoundException)
            { 
                throw new Exception(@"The header must contain ""PValue""");
            }
        }
        private static Set<int> CreateTabulateReportInternal(
            string inputFilePattern,
            KeepTest<Dictionary<string, string>> keepTest, 
            double maxPValue, 
            bool auditRowIndexValues,
            ref List<Dictionary<string, string>> realRowCollectionToSort, 
            ref List<double> nullValueCollectionToBeSorted,
            ref string headerSoFar)
        {
            Set<int> nullIndexSet = Set<int>.GetInstance();

            //!!!very similar code elsewhere 
            RowIndexTabulator rowIndexTabulator = RowIndexTabulator.GetInstance(auditRowIndexValues); 
            //RangeCollection unfilteredRowIndexRangeCollection = RangeCollection.GetInstance();
 
            foreach (string fileName in Directory.GetFiles(Directory.GetCurrentDirectory(), inputFilePattern))
            {
                Debug.WriteLine(fileName);
                string headerOnFile;
                bool firstRow = true;
                foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(fileName, /*includeWholeLine*/ true, out headerOnFile)) 
                { 
                    if (firstRow)
                    { 
                        firstRow = false;
                        if (headerSoFar == null)
                        {
                            headerSoFar = headerOnFile;
                        }
                        else if (headerSoFar != headerOnFile) 
                        { 
                            Console.WriteLine("Warning: The header for file {0} is different from the 1st file read in", fileName);
                        } 
                    }

                    if (rowIndexTabulator.TryAdd(row, fileName) && keepTest.Test(row))
                    {
                        //int unfilteredRowIndex = ReadUnfilteredRowIndexButIfMissingUseRowIndex(row, rowIndex);
 
                        //unfilteredRowIndexRangeCollection.Add(unfilteredRowIndex); 

                        SpecialFunctions.CheckCondition(row.ContainsKey(NullIndexColumnName), string.Format(@"When tabulating a ""{0}"" column is required. (File ""{1}"")", NullIndexColumnName, fileName)); 

                        int nullIndex = int.Parse(row[NullIndexColumnName]);
                        nullIndexSet.AddNewOrOld(nullIndex);

                        double pValue = AccessPValueFromPhylotreeRow(row);
                        //if (double.IsNaN(pValue)) 
                        //{ 
                        //    pValue = 1;
                        //    row["PValue"] = "1"; 
                        //}
                        if (pValue <= maxPValue)
                        {
                            if (nullIndex == -1)
                            {
                                realRowCollectionToSort.Add(row); 
                            } 
                            else
                            { 
                                nullValueCollectionToBeSorted.Add(pValue);
                            }
                        }
                    }
                }
            } 
 
            rowIndexTabulator.CheckIsComplete(inputFilePattern);
 
            return nullIndexSet;
        }

        static public KeyValuePair<string, int> GetMerAndPos(string variableName)
        {
            string[] fields = variableName.Split('@'); 
            int pos; 
            string mer;
            if (int.TryParse(fields[0], out pos)) 
            {
                mer = fields[1];
            }
            else if (int.TryParse(fields[1], out pos))
            {
                mer = fields[0]; 
            } 
            else
            { 
                throw new ArgumentException("Cannot paris " + variableName + " into mer and pos.");
            }
            return new KeyValuePair<string, int>(mer, pos);
        }

        static public string NullIndexColumnName = "NullIndex"; 
        static public string PredictorVariableColumnName = "PredictorVariable"; 
        static public string PredictorTrueNameCountColumnName = "PredictorTrueCount";
        static public string PredictorFalseNameCountColumnName = "PredictorFalseCount"; 
        static public string PredictorFalseNameCountBeforeTreeColumnName = "PredictorFalseCountBeforeTree"; //!!!Used in the PhyloTreeGauss regression test
        static public string PredictorNonMissingCountColumnName = "PredictorNonMissingCount";
        static public string TargetVariableColumnName = "TargetVariable";
        static public string TargetTrueNameCountColumnName = "TargetTrueCount";
        static public string TargetFalseNameCountColumnName = "TargetFalseCount";
        static public string TargetNonMissingCountColumnName = "TargetNonMissingCount"; 
        static public string GlobalNonMissingCountColumnName = "GlobalNonMissingCount"; 
        static public string PValueColumnName = "PValue";
        static public string QValueColumnName = "QValue"; 

    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
