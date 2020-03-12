using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using Mlas.Tabulate;
 
 
namespace VirusCount.PhyloTree
{ 

    public class UniversalWorkList
    {
        protected UniversalWorkList()
        {
        } 
 
        //public Dictionary<int, Dictionary<string, Dictionary<string, bool>>> NullIndexToPredictorToCaseIdToNonMissingValue;
 
        protected IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> _predictorNameAndCaseIdToNonMissingValueEnumeration;
        protected IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> _targetNameAndCaseIdToNonMissingValueEnumeration;
        //private Dictionary<string, Dictionary<string, T>> TargetVariableToCaseIdToNonMissingValue;
        //private Dictionary<string, Dictionary<string, ISufficientStatistics>> TargetVariableToCaseIdToNonMissingValue;
        // private IEnumerable<string> _targetVariables;
        //private Dictionary<string, Dictionary<string, SufficientStatistics>> _predictorVariableToCaseIdToNonMissingValue; 
        protected NullDataCollection _nullDataCollection; 
        protected KeepTest<Dictionary<string, string>> _keepTest;
        protected RangeCollection _nullIndexRange; 

        protected UniversalWorkList(
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration,
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration,
            NullDataCollection nullDataCollection,
            RangeCollection nullIndexRange, 
            KeepTest<Dictionary<string, string>> keepTest 
            )
        { 
            _predictorNameAndCaseIdToNonMissingValueEnumeration = predictorNameAndCaseIdToNonMissingValueEnumeration;
            _targetNameAndCaseIdToNonMissingValueEnumeration = targetNameAndCaseIdToNonMissingValueEnumeration;
            _keepTest = keepTest;
            _nullDataCollection = nullDataCollection;
            _nullIndexRange = nullIndexRange;
 
            //Console.WriteLine("In UniversalWorkList constructor."); 
        }
 
        public static UniversalWorkList GetInstance(
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration,
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration,
            //Dictionary<string, Dictionary<string, SufficientStatistics>> predictorVariableToCaseIdToRealNonMissingValue,
            NullDataCollection nullDataCollection,
            RangeCollection nullIndexRange, 
            KeepTest<Dictionary<string, string>> keepTest 
            )
        { 
            //SpecialFunctions.CheckCondition(-1 <= nullIndexStart && nullIndexStart <= nullIndexLast);

            bool enumeratePairs = keepTest is KeepPredictorTargetPairs;
            if (keepTest is KeepCollection<Dictionary<string, string>>)
            {
                foreach (KeepTest<Dictionary<string, string>> keepTestInCollection in ((KeepCollection<Dictionary<string, string>>)keepTest).KeepTestCollection) 
                { 
                    if (keepTestInCollection is KeepPredictorTargetPairs)
                    { 
                        enumeratePairs = true;
                    }
                }
            }
            UniversalWorkList aUniversalWorkList;
            if (enumeratePairs) 
            { 
                aUniversalWorkList = UniversalWorkListPredTargPairs.GetInstance(
                                predictorNameAndCaseIdToNonMissingValueEnumeration, 
                                targetNameAndCaseIdToNonMissingValueEnumeration,
                                nullDataCollection,
                                nullIndexRange,
                                keepTest
                                );
            } 
            else 
            {
                aUniversalWorkList = new UniversalWorkList( 
                                predictorNameAndCaseIdToNonMissingValueEnumeration,
                                targetNameAndCaseIdToNonMissingValueEnumeration,
                                nullDataCollection,
                                nullIndexRange,
                                keepTest
                                ); 
            } 
//            aUniversalWorkList._predictorNameAndCaseIdToNonMissingValueEnumeration = predictorNameAndCaseIdToNonMissingValueEnumeration;
//            aUniversalWorkList._targetNameAndCaseIdToNonMissingValueEnumeration = targetNameAndCaseIdToNonMissingValueEnumeration; 
////          aUniversalWorkList._targetVariables = targetVariables;
////          aUniversalWorkList._predictorVariableToCaseIdToNonMissingValue = predictorVariableToCaseIdToRealNonMissingValue;
//            aUniversalWorkList._keepTest = keepTest;
//            aUniversalWorkList._nullDataCollection = nullDataCollection;
//            aUniversalWorkList._nullIndexRange = nullIndexRange;
 
            return aUniversalWorkList; 
        }
 
        // iterates over all possible targets, predictors and nullIndex values. That is, performs
        // an exhaustive combination analysis.

        public virtual IEnumerable<RowData> List()
        {
            //int index = -1; 
            foreach (Pair<string, Dictionary<string, SufficientStatistics>> targetNameAndCaseIdToNonMissingValue in _targetNameAndCaseIdToNonMissingValueEnumeration) 
 			 {
                string targetVariable = targetNameAndCaseIdToNonMissingValue.First; 

                foreach(int nullIndex in _nullIndexRange.Elements)
                {
                    foreach (Pair<string, Dictionary<string, SufficientStatistics>> predictorNameAndCaseIdToNonMissingValue in _predictorNameAndCaseIdToNonMissingValueEnumeration)
                    //foreach (string predictorVariable in _predictorVariableToCaseIdToNonMissingValue.Keys)
                    { 
                        //++index; 

                        string predictorVariable = predictorNameAndCaseIdToNonMissingValue.First; 

                        Dictionary<string, string> row = new Dictionary<string, string>();

                        row.Add(Tabulate.PredictorVariableColumnName, predictorVariable);
                        row.Add(Tabulate.TargetVariableColumnName, targetVariable);
 
                        row.Add(Tabulate.NullIndexColumnName, nullIndex.ToString()); 
                        if (_keepTest.Test(row))
                        { 
                            //RowAndTargetData rowAndTargetData = RowAndTargetData.GetInstance(row, targetNameAndCaseIdToNonMissingValue.Second);
                            RowData rowData =
                                RowData.GetInstance(
                                    row,
                                    delegate()
                                    { 
                                        return _nullDataCollection.GetCaseIdToNonMissingValueForNullIndexAndPredictorVariableOrReal( 
                                            nullIndex, predictorVariable, predictorNameAndCaseIdToNonMissingValue.Second);
                                    }, 
                                    delegate()
                                    {
                                        return _nullDataCollection.GetCaseIdToNonMissingValueForNullIndexAndTargetVariableOrReal(
                                        nullIndex, targetVariable, targetNameAndCaseIdToNonMissingValue.Second);
                                    });
 
                            yield return rowData; 
                        }
                    } 
                }
            }
            _keepTest.Reset();   // some KeepTests need to be reset before they run through the set again.
        }
    }
 
    public class UniversalWorkListPredTargPairs : UniversalWorkList 
    {
        protected UniversalWorkListPredTargPairs( 
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration,
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration,
            NullDataCollection nullDataCollection,
            RangeCollection nullIndexRange,
            KeepTest<Dictionary<string, string>> keepTest
            ) 
            : 
            base(
            predictorNameAndCaseIdToNonMissingValueEnumeration, 
            targetNameAndCaseIdToNonMissingValueEnumeration,
            nullDataCollection, nullIndexRange,
            keepTest
            )
        {
            //Console.WriteLine("In UniversalWorkListPredTargPairs constructor."); 
        } 

        new public static UniversalWorkList GetInstance( 
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration,
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration,
            //Dictionary<string, Dictionary<string, SufficientStatistics>> predictorVariableToCaseIdToRealNonMissingValue,
            NullDataCollection nullDataCollection,
            RangeCollection nullIndexRange,
            KeepTest<Dictionary<string, string>> keepTest 
            ) 
        {
            //SpecialFunctions.CheckCondition(-1 <= nullIndexStart && nullIndexStart <= nullIndexLast); 
            UniversalWorkList aUniversalWorkList = new UniversalWorkListPredTargPairs(
                predictorNameAndCaseIdToNonMissingValueEnumeration,
                targetNameAndCaseIdToNonMissingValueEnumeration,
                nullDataCollection,
                nullIndexRange,
                keepTest 
                ); 

            return aUniversalWorkList; 
        }

        public override IEnumerable<RowData> List()
        {
            //Console.WriteLine("Enumerating pairs");
            foreach (int nullIndex in _nullIndexRange.Elements) 
            { 
                foreach (
                    KeyValuePair<Pair<string, Dictionary<string, SufficientStatistics>>, Pair<string, Dictionary<string, SufficientStatistics>>> 
                        predictorAndTarget
                    in SpecialFunctions.EnumerateTwo(_predictorNameAndCaseIdToNonMissingValueEnumeration, _targetNameAndCaseIdToNonMissingValueEnumeration))
                {
                    Pair<string, Dictionary<string, SufficientStatistics>> predictorNameAndCaseIdToNonMissingValue = predictorAndTarget.Key;
                    Pair<string, Dictionary<string, SufficientStatistics>> targetNameAndCaseIdToNonMissingValue = predictorAndTarget.Value;
 
                    string targetVariable = targetNameAndCaseIdToNonMissingValue.First; 
                    string predictorVariable = predictorNameAndCaseIdToNonMissingValue.First;
 
                    Dictionary<string, string> row = new Dictionary<string, string>();

                    row.Add(Tabulate.PredictorVariableColumnName, predictorVariable);
                    row.Add(Tabulate.TargetVariableColumnName, targetVariable);

                    row.Add(Tabulate.NullIndexColumnName, nullIndex.ToString()); 
                    if (_keepTest.Test(row)) 
                    {
                        //RowAndTargetData rowAndTargetData = RowAndTargetData.GetInstance(row, targetNameAndCaseIdToNonMissingValue.Second); 
                        RowData rowData =
                            RowData.GetInstance(
                            row,
                            delegate()
                            {
                                return _nullDataCollection.GetCaseIdToNonMissingValueForNullIndexAndPredictorVariableOrReal( 
                                    nullIndex, predictorVariable, predictorNameAndCaseIdToNonMissingValue.Second); 
                            },
                            delegate() 
                            {
                                return _nullDataCollection.GetCaseIdToNonMissingValueForNullIndexAndTargetVariableOrReal(
                                nullIndex, targetVariable, targetNameAndCaseIdToNonMissingValue.Second);
                            });

                        yield return rowData; 
                    } 
                }
 
                _keepTest.Reset();   // some KeepTests need to be reset before they run through the set again.
            }
        }
    }

    public class RowData 
    { 
        public delegate Dictionary<string, SufficientStatistics> DataGenerator();
 
        private RowData()
        {
        }

        public readonly Dictionary<string, string> Row;
        private readonly DataGenerator _predictorGenerator; 
        private readonly DataGenerator _targetGenerator; 

        private Dictionary<string, SufficientStatistics> _predictorData; 
        private Dictionary<string, SufficientStatistics> _targetData;

        public Dictionary<string, SufficientStatistics> PredictorData
        {
            get
            { 
                if (_predictorData == null) 
                {
                    _predictorData = _predictorGenerator(); 
                }
                return _predictorData;
            }
        }

        public Dictionary<string, SufficientStatistics>TargetData 
        { 
            get
            { 
                if (_targetData == null)
                {
                    _targetData = _targetGenerator();
                }
                return _targetData;
            } 
        } 

        public RowData ( 
            Dictionary<string, string> row,
            DataGenerator predictorGenerator,
            DataGenerator targetGenerator
            )
        {
            Row = row; 
            _predictorGenerator = predictorGenerator; 
            _targetGenerator = targetGenerator;
        } 

        static public RowData GetInstance(
            Dictionary<string, string> row,
            DataGenerator predictorGenerator,
            DataGenerator targetGenerator
            ) 
        { 
            return new RowData(row, predictorGenerator, targetGenerator);
        } 
    }

}


// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
