using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Msr.Mlas.SpecialFunctions 
{ 
    public class TwoByTwo
    { 
        public static string Header = SpecialFunctions.CreateTabString("var1", "var2", "TT", "TF", "FT", "FF");
        public static string CountsHeader = SpecialFunctions.CreateTabString("TT", "TF", "FT", "FF");

        public enum ParameterIndex
        {
            TT = 0, TF, FT, FF 
        } 

        public override string ToString() 
        {
            return SpecialFunctions.CreateTabString(Var1, Var2, Counts[1, 1], Counts[1, 0], Counts[0, 1], Counts[0, 0]);
        }
        public string CountsString()
        {
            return SpecialFunctions.CreateTabString(Counts[1, 1], Counts[1, 0], Counts[0, 1], Counts[0, 0]); 
        } 

        public int[] ToOneDArray() 
        {
            int[] fisherCounts = new int[4];
            fisherCounts[(int)TwoByTwo.ParameterIndex.TT] = Counts[1, 1];
            fisherCounts[(int)TwoByTwo.ParameterIndex.TF] = Counts[1, 0];
            fisherCounts[(int)TwoByTwo.ParameterIndex.FT] = Counts[0, 1];
            fisherCounts[(int)TwoByTwo.ParameterIndex.FF] = Counts[0, 0]; 
            return fisherCounts; 
        }
 
        string Var1;
        string Var2;
        public int[,] Counts;
        double? _fisherExactTest = null;
        public double FisherExactTest
        { 
            get 
            {
                if (null == _fisherExactTest) 
                {
                    _fisherExactTest = SpecialFunctions.FisherExactTest(Counts);
                }
                return (double)_fisherExactTest;
            }
        } 
 
        double? _bayesScore = null;
        public double BayesScore 
        {
            get
            {
                if (null == _bayesScore)
                {
                    _bayesScore = SpecialFunctions.BayesScore(Counts); 
                } 
                return (double)_bayesScore;
            } 
        }


        public static List<TwoByTwo> GetSortedCollection(string inputSparseFileName1, string inputSparseFileName2, bool lowMemory)
        {
            IEnumerable<KeyValuePair<string, Dictionary<string, int>>> varToCidToVal2 = GroupByVariable(inputSparseFileName2, lowMemory); 
            List<TwoByTwo> results = ProcessCore(inputSparseFileName1, varToCidToVal2); 
            results.Sort(delegate(TwoByTwo a, TwoByTwo b) { return a.FisherExactTest.CompareTo(b.FisherExactTest);});
            return results; 
        }

        public static IEnumerable<KeyValuePair<string, Dictionary<string, int>>> GroupByVariable(string inputSparseFileName2, bool lowMemory)
        {
            IEnumerable<KeyValuePair<string, Dictionary<string, int>>> varToCidToVal;
            if (lowMemory) 
            { 
                varToCidToVal = GroupByVariableLowMemory(inputSparseFileName2);
            } 
            else
            {
                varToCidToVal = GroupByVariableInMemory(inputSparseFileName2);
            }
            return varToCidToVal;
        } 
 

 
        public void TabulateCounts(Dictionary<string, int> cidToVal1, Dictionary<string, int> cidToVal2)
        {
            Counts = new int[2, 2]; //c# inits to zeros
            foreach (KeyValuePair<string, int> cidAndVal1 in cidToVal1)
            {
                int val2; 
                if (cidToVal2.TryGetValue(cidAndVal1.Key, out val2)) 
                {
                    ++Counts[cidAndVal1.Value, val2]; 
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, Dictionary<string, int>>> GroupByVariableLowMemory(string inputSparseFileName)
        { 
            string header = @"var	cid	val"; 
            //Not using SpecialFunctions.TabFileTable because this will be an inner loop and we want it very fast
            Set<string> varSet = Set<string>.GetInstance(); 
            string var = null;
            Dictionary<string, int> cidToVal = null;
            using (TextReader textReader = File.OpenText(inputSparseFileName))
            {
                string line = textReader.ReadLine();
                SpecialFunctions.CheckCondition(line == header, string.Format(@"Expected header to be ""{0}"". File is ""{1}"".", header, inputSparseFileName)); 
                while (null != (line = textReader.ReadLine())) 
                {
                    Debug.Assert((var == null) == (cidToVal == null)); // real assert 
                    string[] fieldCollection = line.Split('\t');
                    SpecialFunctions.CheckCondition(fieldCollection.Length == 3, string.Format(@"Input lines should have three tab-delimited columns. File is ""{0}"". Line is ""{1}"".", inputSparseFileName, line));
                    if (fieldCollection[0] != var)
                    {
                        if (var != null)
                        { 
                            yield return new KeyValuePair<string, Dictionary<string, int>>(var, cidToVal); 
                        }
                        var = fieldCollection[0]; 
                        SpecialFunctions.CheckCondition(!varSet.Contains(var), string.Format(@"Input file should be grouped by variable. Variable ""{0}"" appears as more than one group. File is ""{1}"".", var, inputSparseFileName));
                        varSet.AddNew(var);
                        cidToVal = new Dictionary<string, int>();
                    }
                    string cid = fieldCollection[1];
                    SpecialFunctions.CheckCondition(!cidToVal.ContainsKey(cid), string.Format(@"cid ""{0}"" appears twice in variable ""{1}"". File is ""{2}"".", cid, var, inputSparseFileName)); 
                    int val; 
                    SpecialFunctions.CheckCondition(int.TryParse(fieldCollection[2], out val) && (val == 0 || val == 1), string.Format(@"Expected 0 or 1 values. Variable is ""{0}"". Cid is ""{1}"". File is ""{2}"".", var, cid, inputSparseFileName));
                    cidToVal.Add(cid, val); 
                }
                Debug.Assert((var == null) == (cidToVal == null)); // real assert
                if (var != null)
                {
                    yield return new KeyValuePair<string, Dictionary<string, int>>(var, cidToVal);
                } 
 
            }
        } 


        private static Dictionary<string, Dictionary<string, int>> GroupByVariableInMemory(string inputSparseFileName2)
        {
            Dictionary<string, Dictionary<string, int>> varToCidToVal2 = new Dictionary<string, Dictionary<string, int>>();
            foreach (KeyValuePair<string, Dictionary<string, int>> varAndCidToVal2 in GroupByVariableLowMemory(inputSparseFileName2)) 
            { 
                varToCidToVal2.Add(varAndCidToVal2.Key, varAndCidToVal2.Value);
            } 
            return varToCidToVal2;
        }

        private static List<TwoByTwo> ProcessCore(string inputSparseFileName1, IEnumerable<KeyValuePair<string, Dictionary<string, int>>> varToCidToVal2)
        {
            List<TwoByTwo> twoByTwoCollection = new List<TwoByTwo>(); 
            foreach (KeyValuePair<string, Dictionary<string, int>> varAndCidToVal1 in GroupByVariableLowMemory(inputSparseFileName1)) 
            {
                foreach (KeyValuePair<string, Dictionary<string, int>> varAndCidToVal2 in varToCidToVal2) 
                {
                    TwoByTwo twoByTwo = TwoByTwo.GetInstance(varAndCidToVal1, varAndCidToVal2);
                    twoByTwoCollection.Add(twoByTwo);
                }
            }
            return twoByTwoCollection; 
        } 

        public static TwoByTwo GetInstance(KeyValuePair<string, Dictionary<string, int>> varAndCidToVal1, KeyValuePair<string, Dictionary<string, int>> varAndCidToVal2) 
        {
            TwoByTwo twoByTwo = new TwoByTwo();
            twoByTwo.Var1 = varAndCidToVal1.Key;
            twoByTwo.Var2 = varAndCidToVal2.Key;
            twoByTwo.TabulateCounts(varAndCidToVal1.Value, varAndCidToVal2.Value);
            return twoByTwo; 
        } 

        public static TwoByTwo GetInstance(Dictionary<string, int> cidToVal1, Dictionary<string, int> cidToVal2) 
        {
            TwoByTwo twoByTwo = new TwoByTwo();
            twoByTwo.Var1 = null;
            twoByTwo.Var2 = null;
            twoByTwo.TabulateCounts(cidToVal1, cidToVal2);
            return twoByTwo; 
        } 

 
        public static TwoByTwo GetInstance(int[] fisherCounts)
        {
            SpecialFunctions.CheckCondition(fisherCounts.Length == 4);
            TwoByTwo twoByTwo = new TwoByTwo();
            twoByTwo.Var1 = null;
            twoByTwo.Var2 = null; 
            twoByTwo.Counts = new int[2, 2]; 

            twoByTwo.Counts[1, 1] = fisherCounts[(int)TwoByTwo.ParameterIndex.TT]; 
            twoByTwo.Counts[1, 0] = fisherCounts[(int)TwoByTwo.ParameterIndex.TF];
            twoByTwo.Counts[0, 1] = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
            twoByTwo.Counts[0, 0] = fisherCounts[(int)TwoByTwo.ParameterIndex.FF];

            return twoByTwo;
        } 
 
        public static TwoByTwo GetInstance(string var1, string var2)
        { 
            TwoByTwo twoByTwo = new TwoByTwo();
            twoByTwo.Var1 = var1;
            twoByTwo.Var2 = var2;
            twoByTwo.Counts = new int[2, 2]; //C# init's to 0's
            return twoByTwo;
        } 
 

        //Should we define a class like FilterForTwoByTwos (or Converter<TwoByTwo,bool>) and apply the list of them to make this more extensible? 
        public bool PassTests(bool positiveDirectionOnly, double fishersExactTestMaximum, double countOrExpCountMinimum)
        {
            if (FisherExactTest > fishersExactTestMaximum)
            {
                return false;
            } 
 
            if (MinOfCountOfExpCount() < countOrExpCountMinimum)
            { 
                return false;
            }

            if (positiveDirectionOnly && !RightDirection())
            {
                return false; 
            } 

 
            return true;
        }

        private double MinOfCountOfExpCount()
        {
            double minOfCountOrExpCount = SpecialFunctions.MinOfCountOrExpCount(Counts[1, 1], Counts[1, 0], Counts[0, 1], Counts[0, 0]); 
            return minOfCountOrExpCount; 
        }
 
        public bool RightDirection()
        {
            bool b = (Counts[0, 0] * Counts[1, 1] < Counts[0, 1] * Counts[1, 0]);
            return b;
        }
 
 
        public static int GetRightSum(int[] fisherCounts){
            int tt = fisherCounts[(int)TwoByTwo.ParameterIndex.TT]; 
            int ft = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];

            int sum = tt + ft;
            return sum;

        } 
 
        public static int GetLeftSum(int[] fisherCounts)
        { 
            int tt = fisherCounts[(int)TwoByTwo.ParameterIndex.TT];
            int tf = fisherCounts[(int)TwoByTwo.ParameterIndex.TF];

            int sum = tt + tf;
            return sum;
        } 
 
        public void Increment(bool var1IsTrue, bool var2IsTrue)
        { 
            ++Counts[var1IsTrue ? 1 : 0, var2IsTrue ? 1 : 0];
        }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
