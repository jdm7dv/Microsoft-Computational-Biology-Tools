using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree
{ 
    public class NullDataCollection 
    {
        private Dictionary<int, NullDataGenerator> _nullIndexToNullDataGenerator; 
        public readonly string Name;

        protected NullDataCollection(
            NullDataGenerator nullDataGenerator,
            RangeCollection nullIndexRange,
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration, 
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration 
            //Dictionary<string, Dictionary<string, SufficientStatistics>> predictorVariableToCaseIdToRealNonMissingValue,
            //Dictionary<string, Dictionary<string, SufficientStatistics>> targetVariableToCaseIdToRealNonMissingValue 
            )
        {
            Console.WriteLine(nullDataGenerator);
            Name = nullDataGenerator.Name;
            _nullIndexToNullDataGenerator = new Dictionary<int, NullDataGenerator>();
            foreach (int nullIndex in nullIndexRange.Elements) 
            { 
                int predCount = SpecialFunctions.Count(predictorNameAndCaseIdToNonMissingValueEnumeration);
                int preseed = ~nullIndex.GetHashCode() ^ predCount.GetHashCode() 
                     ^ "NullDataCollection".GetHashCode();

                NullDataGenerator newNullDataGenerator = (NullDataGenerator)nullDataGenerator.Clone();
                newNullDataGenerator.SetPreseed(preseed);
                newNullDataGenerator.SetPredictorNameAndCaseIdToNonMissingValueEnumeration(predictorNameAndCaseIdToNonMissingValueEnumeration);
                newNullDataGenerator.SetTargetNameAndCaseIdToNonMissingValueEnumeration(targetNameAndCaseIdToNonMissingValueEnumeration); 
                //newNullDataGenerator.RealPredictorVariableToCaseIdToNonMissingValue = predictorVariableToCaseIdToRealNonMissingValue; 
                //newNullDataGenerator.RealTargetVariableToCaseIdToNonMissingValue = targetVariableToCaseIdToRealNonMissingValue;
                _nullIndexToNullDataGenerator.Add(nullIndex, newNullDataGenerator); 
            }
        }

        public static NullDataCollection GetInstance(
            NullDataGenerator nullDataGenerator,
            RangeCollection nullIndexRange, 
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration, 
            IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration)
        { 
            return new NullDataCollection(nullDataGenerator, nullIndexRange, predictorNameAndCaseIdToNonMissingValueEnumeration, targetNameAndCaseIdToNonMissingValueEnumeration);
        }

        public Dictionary<string, SufficientStatistics> GetCaseIdToNonMissingValueForNullIndexAndPredictorVariableOrReal(
            int nullIndex, string predictorVariable, Dictionary<string, SufficientStatistics> realValue)
        { 
            if (nullIndex == -1) 
            {
                return realValue; 
            }
            NullDataGenerator nullDataGenerator = _nullIndexToNullDataGenerator[nullIndex];
            Random random = new Random(predictorVariable.GetHashCode() ^ nullDataGenerator.Preseed);
            return nullDataGenerator.GetCaseIdToNonMissingPredictorValueOrDefault(predictorVariable, realValue, ref random);
        }
 
        public Dictionary<string, SufficientStatistics> GetCaseIdToNonMissingValueForNullIndexAndTargetVariableOrReal( 
            int nullIndex, string targetVariable, Dictionary<string, SufficientStatistics> realValue)
        { 
            if (nullIndex == -1)
            {
                return realValue;
            }
            NullDataGenerator nullDataGenerator = _nullIndexToNullDataGenerator[nullIndex];
            Random random = new Random(targetVariable.GetHashCode() ^ nullDataGenerator.Preseed); 
            return nullDataGenerator.GetCaseIdToNonMissingTargetValueOrDefault(targetVariable, realValue, ref random); 
        }
    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
