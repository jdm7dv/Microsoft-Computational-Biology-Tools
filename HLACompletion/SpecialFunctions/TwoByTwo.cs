using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Msr.Mlas.SpecialFunctions 
{ 
    abstract public class TwoByTwo
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

        public int this[bool var1True, bool var2True] 
        {
            get
            {
                return Counts[var1True ? 1 : 0, var2True ? 1 : 0];
            }
            set 
            { 
                Counts[var1True ? 1 : 0, var2True ? 1 : 0] = value;
                _fisherExactTest = null; 
                _bayesScore = null;
            }
        }

        public abstract int[,] Counts
        { 
            get; 
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

        public string Var1;
        public string Var2;
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


        public static List<TwoByTwo> GetSortedCollection(string inputSparseFileName1, string inputSparseFileName2, bool lowMemory, bool rememberCases)
        {
            IEnumerable<TwoByTwo> unsorted = GetUnsortedCollection(inputSparseFileName1, inputSparseFileName2, lowMemory, rememberCases); 
            List<TwoByTwo> results = new List<TwoByTwo>(unsorted); 
            results.Sort(delegate(TwoByTwo a, TwoByTwo b) { return a.FisherExactTest.CompareTo(b.FisherExactTest);});
            return results; 
        }

        public static IEnumerable<TwoByTwo> GetUnsortedCollection(string inputSparseFileName1, string inputSparseFileName2, bool lowMemory, bool rememberCases)
        {
            IEnumerable<KeyValuePair<string, Dictionary<string, int>>> varToCidToVal2 = GroupByVariable(inputSparseFileName2, lowMemory);
            IEnumerable<TwoByTwo> unsorted = ProcessCore(inputSparseFileName1, varToCidToVal2, rememberCases); 
            return unsorted; 
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

 
 
        abstract public void TabulateCounts(Dictionary<string, int> cidToVal1, Dictionary<string, int> cidToVal2);
 
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

        private static IEnumerable<TwoByTwo> ProcessCore(string inputSparseFileName1, IEnumerable<KeyValuePair<string, Dictionary<string, int>>> varToCidToVal2, bool rememberCases) 
        {
            foreach (KeyValuePair<string, Dictionary<string, int>> varAndCidToVal1 in GroupByVariableLowMemory(inputSparseFileName1))
            {
                foreach (KeyValuePair<string, Dictionary<string, int>> varAndCidToVal2 in varToCidToVal2)
                {
                    TwoByTwo twoByTwo = TwoByTwo.GetInstance(varAndCidToVal1, varAndCidToVal2, rememberCases); 
                    yield return twoByTwo; 
                }
            } 
        }

        public static TwoByTwo GetInstance(KeyValuePair<string, Dictionary<string, int>> varAndCidToVal1, KeyValuePair<string, Dictionary<string, int>> varAndCidToVal2)
        {
            return GetInstance(varAndCidToVal1, varAndCidToVal2, false);
        } 
        public static TwoByTwo GetInstance(KeyValuePair<string, Dictionary<string, int>> varAndCidToVal1, KeyValuePair<string, Dictionary<string, int>> varAndCidToVal2, bool rememberCases) 
        {
            TwoByTwo twoByTwo = rememberCases ? (TwoByTwo)new TwoByTwoCases() : (TwoByTwo)new TwoByTwoCounts(); 
            twoByTwo.Var1 = varAndCidToVal1.Key;
            twoByTwo.Var2 = varAndCidToVal2.Key;
            twoByTwo.TabulateCounts(varAndCidToVal1.Value, varAndCidToVal2.Value);
            return twoByTwo;
        }
 
        public static TwoByTwo GetInstance(Dictionary<string, int> cidToVal1, Dictionary<string, int> cidToVal2) 
        {
            return GetInstance(cidToVal1, cidToVal2, false); 
        }

        public static TwoByTwo GetInstance(Dictionary<string, int> cidToVal1, Dictionary<string, int> cidToVal2, bool rememberCases)
        {
            TwoByTwo twoByTwo = rememberCases ? (TwoByTwo)new TwoByTwoCases() : (TwoByTwo)new TwoByTwoCounts();
            twoByTwo.Var1 = null; 
            twoByTwo.Var2 = null; 
            twoByTwo.TabulateCounts(cidToVal1, cidToVal2);
            return twoByTwo; 
        }


        public static TwoByTwo GetInstance(int[] fisherCounts)
        {
            SpecialFunctions.CheckCondition(fisherCounts.Length == 4); 
            TwoByTwoCounts twoByTwo = new TwoByTwoCounts(); 
            twoByTwo.Var1 = null;
            twoByTwo.Var2 = null; 
            twoByTwo._counts = new int[2, 2]; //C# init's to 0's

            twoByTwo.Counts[1, 1] = fisherCounts[(int)TwoByTwo.ParameterIndex.TT];
            twoByTwo.Counts[1, 0] = fisherCounts[(int)TwoByTwo.ParameterIndex.TF];
            twoByTwo.Counts[0, 1] = fisherCounts[(int)TwoByTwo.ParameterIndex.FT];
            twoByTwo.Counts[0, 0] = fisherCounts[(int)TwoByTwo.ParameterIndex.FF]; 
 
            return twoByTwo;
        } 

        public static TwoByTwo GetInstance(string var1, string var2, bool rememberCases)
        {
            if (rememberCases)
            {
                TwoByTwoCases twoByTwo = new TwoByTwoCases(); 
                twoByTwo.Var1 = var1; 
                twoByTwo.Var2 = var2;
                twoByTwo.Cases = new List<string>[2, 2] { { new List<string>(), new List<string>() }, { new List<string>(), new List<string>() } }; 
                return twoByTwo;
            }
            else
            {
                TwoByTwoCounts twoByTwo = new TwoByTwoCounts();
                twoByTwo.Var1 = var1; 
                twoByTwo.Var2 = var2; 
                twoByTwo._counts = new int[2, 2]; //C# init's to 0's
                return twoByTwo; 
            }
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

        public bool PositiveCorrelation()
        {
            bool b = (Counts[0, 0] * Counts[1, 1] > Counts[0, 1] * Counts[1, 0]);
            return b; 
        } 

        public double CorrelationValue() 
        {
            double d = (Counts[0, 0] * Counts[1, 1] - Counts[0, 1] * Counts[1, 0]);
            return d;
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
            _bayesScore = null; 
            _fisherExactTest = null;
            ++Counts[var1IsTrue ? 1 : 0, var2IsTrue ? 1 : 0];
        }

        public static IEnumerable<TwoByTwo> GetCollection(bool lowMemory, bool unsorted, string inputSparseFileName1, string inputSparseFileName2)
        { 
            return GetCollection(lowMemory, unsorted, inputSparseFileName1, inputSparseFileName2, false); 
        }
 
        public static IEnumerable<TwoByTwo> GetCollection(bool lowMemory, bool unsorted, string inputSparseFileName1, string inputSparseFileName2, bool rememberCases)
        {
            if (unsorted)
            {
                return TwoByTwo.GetUnsortedCollection(inputSparseFileName1, inputSparseFileName2, lowMemory, rememberCases);
            } 
            else 
            {
                return  TwoByTwo.GetSortedCollection(inputSparseFileName1, inputSparseFileName2, lowMemory, rememberCases); 
            }
        }

        public int RowSum(bool var1IsTrue)
        {
            int row = var1IsTrue ? 1 : 0; 
            int sum = Counts[row, 0] + Counts[row, 1]; 
            return sum;
        } 

        public int ColSum(bool var2IsTrue)
        {
            int col = var2IsTrue ? 1 : 0;
            int sum = Counts[0, col] + Counts[1, col];
            return sum; 
        } 

        public int Sum() 
        {
            int sum = Counts[0, 0] + Counts[0, 1] +  Counts[1, 0] + Counts[1, 1];
            return sum;
        }

 
        public void Clear() 
        {
            Counts[0, 0] = 0; 
            Counts[0, 1] = 0;
            Counts[1, 0] = 0;
            Counts[1, 1] = 0;
            _fisherExactTest = null;
            _bayesScore = null;
        } 
 
        public void Report(TextWriter textWriter)
        { 
            textWriter.WriteLine(SpecialFunctions.CreateTabString("", "not " + Var2, Var2, "total"));
            textWriter.WriteLine(SpecialFunctions.CreateTabString("not " + Var1, Counts[0,0], Counts[0,1], RowSum(false)));
            textWriter.WriteLine(SpecialFunctions.CreateTabString(Var1, Counts[1,0], Counts[1,1], RowSum(true)));
            textWriter.WriteLine(SpecialFunctions.CreateTabString("total", ColSum(false), ColSum(true), Sum()));
        }
    } 
 
    public class TwoByTwoCounts : TwoByTwo
    { 
        internal TwoByTwoCounts()
        {
        }
        public override int[,] Counts
        {
            get 
            { 
                return _counts;
            } 
        }
        internal int[,] _counts;

        public override void TabulateCounts(Dictionary<string, int> cidToVal1, Dictionary<string, int> cidToVal2)
        {
            _counts = new int[2, 2]; //c# inits to zeros 
            foreach (KeyValuePair<string, int> cidAndVal1 in cidToVal1) 
            {
                int val2; 
                if (cidToVal2.TryGetValue(cidAndVal1.Key, out val2))
                {
                    ++_counts[cidAndVal1.Value, val2];
                }
            }
        } 
    } 
    public class TwoByTwoCases : TwoByTwo
    { 
        public override int[,] Counts
        {
            get
            {
                return new int[2, 2] { { Cases[0, 0].Count, Cases[0, 1].Count }, { Cases[1, 0].Count, Cases[1, 1].Count } };
            } 
        } 
        internal TwoByTwoCases()
        { 
        }
        public List<string>[,] Cases;

        public override void TabulateCounts(Dictionary<string, int> cidToVal1, Dictionary<string, int> cidToVal2)
        {
            Cases = new List<string>[2, 2] { { new List<string>(), new List<string>() }, { new List<string>(), new List<string>() } }; 
 
            foreach (KeyValuePair<string, int> cidAndVal1 in cidToVal1)
            { 
                string cid = cidAndVal1.Key;
                int val2;
                if (cidToVal2.TryGetValue(cid, out val2))
                {
                    Cases[cidAndVal1.Value, val2].Add(cid);
                } 
            } 
        }
 
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
