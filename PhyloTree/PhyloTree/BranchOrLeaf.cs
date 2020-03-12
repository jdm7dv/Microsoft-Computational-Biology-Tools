using System; 
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using Optimization; 
using System.Text.RegularExpressions; 
//using MSBN3Lib;
 
namespace VirusCount.PhyloTree
{
    public abstract class BranchOrLeaf
    {
        public double Length = 1;   // default value...
 
        static public BranchOrLeaf GetInstance(StreamReader streamreader, bool isRoot) 
        {
            BranchOrLeaf branchOrLeaf; 
            char peek1 = Peek(streamreader);
            if (peek1 == '(')
            {
                branchOrLeaf = Branch.GetBranchInstance(streamreader, isRoot);
            }
            else 
            { 
                branchOrLeaf = Leaf.GetLeafInstance(streamreader);
            } 

            if (isRoot)
            {
                branchOrLeaf.Length = double.NaN;
                char peek2 = Peek(streamreader);
                if (peek2 == ':') 
                { 
                    Read(streamreader);
                    //branchOrLeaf.Length = ReadLength(streamreader); 
                    //SpecialFunctions.CheckCondition(branchOrLeaf.Length == 0);
                }
            }
            else
            {
                char peek3 = Peek(streamreader); 
                if (peek3 != ':') 
                {
                    int nodeId = ReadInt(streamreader); 
                    Debug.WriteLine(SpecialFunctions.CreateTabString("NodeId", nodeId));
                }
                char colon = Read(streamreader);
                SpecialFunctions.CheckCondition(colon == ':');
                branchOrLeaf.Length = ReadLength(streamreader, true);
            } 
            return branchOrLeaf; 
        }
 
        public static char Read(StreamReader streamreader)
        {
            while (true)
            {
                char c = (char)streamreader.Read();
                if (c != '\r' && c != '\n') 
                { 
                    return c;
                } 
            }
        }


        public static int ReadInt(StreamReader streamreader)
        { 
            StringBuilder sb = new StringBuilder(); 
            while (true)
            { 
                char peek = Peek(streamreader);
                if (!"-0123456789 ".Contains(peek.ToString()))
                {
                    break;
                }
                sb.Append(Read(streamreader)); 
            } 
            int theInt = int.Parse(sb.ToString());
            return theInt; 
        }


        public static double ReadLength(StreamReader streamreader, bool changeZeroToEpislon)
        {
            StringBuilder sb = new StringBuilder(); 
            while (true) 
            {
                char peek = Peek(streamreader); 
                if (!"-0123456789.E ".Contains(peek.ToString()))
                {
                    break;
                }
                sb.Append(Read(streamreader));
            } 
            double length = double.Parse(sb.ToString()); 
            SpecialFunctions.CheckCondition(length >= 0, "Expect nonnegative lengths");
            if (changeZeroToEpislon && length == 0) 
            {
                Debug.WriteLine("Changing zero length to epsilon");
                length = double.Epsilon;
            }
            return length;
        } 
 
        public static char Peek(StreamReader streamreader)
        { 
            while (true)
            {
                int peekAsInt = streamreader.Peek();
                SpecialFunctions.CheckCondition(peekAsInt != -1);
                char peek = (char)peekAsInt;
                if (peek != '\r' && peek != '\n') 
                { 
                    return peek;
                } 
                streamreader.Read();
            }
        }


 
        internal BranchOrLeaf() 
        {
        } 


        public abstract IEnumerable<Leaf> AllLeaves();

        public abstract IEnumerable<BranchOrLeaf> AllBranchesOrLeaves();
        public abstract IEnumerable<BranchOrLeaf> AllBranchesOrLeavesExceptRoot(); 
 
        public abstract void Evolve(bool parentVal, double stationaryDistnOfTrue, double lambda, double pMissing,
            ref Random random, Dictionary<string, BooleanStatistics> caseNameToVal); 

        protected bool EvolveNextVal(bool parentVal, double stationaryDistnOfTrue, double lambda, ref Random random)
        {
            double pNotMutate = Math.Exp(-lambda * this.Length);
            if (random.NextDouble() >= pNotMutate)
            { 
                // if we mutate, mutate according to the stationaryDistn 
                return random.NextDouble() < stationaryDistnOfTrue;
            } 
            else
            {
                return parentVal;
            }
        }
    } 
 
    public class Leaf : BranchOrLeaf
    { 
        public string CaseName;
        internal Leaf()
        {
            CaseName = "";
        }
 
        public override string ToString() 
        {
            return CaseName; 
        }

        public static int SplitOnDashX(string caseName, out int strainIndexBase0)
        {
            string[] fieldCollection = caseName.Split('-');
            SpecialFunctions.CheckCondition(fieldCollection.Length == 2); 
            int patientId = int.Parse(fieldCollection[0]); 
            strainIndexBase0 = int.Parse(fieldCollection[1]) - 1;
            SpecialFunctions.CheckCondition(strainIndexBase0 >= 0); 
            return patientId;
        }

        public string PhylipFormattedName
        {
            get 
            { 
                string formattedName = CaseName.Trim();
                if (formattedName.Length > 10) 
                    formattedName = formattedName.Substring(formattedName.Length - 10);

                return formattedName;
            }
        }
 
        public override void Evolve(bool parentVal, double stationaryDistnOfTrue, double lambda, double pMissing, 
            ref Random random, Dictionary<string, BooleanStatistics> caseNameToVal)
        { 
            // with some probability, this leaf will be "missing", which means we simply ignore this method call
            if (random.NextDouble() >= pMissing)
            {
                bool newVal = EvolveNextVal(parentVal, stationaryDistnOfTrue, lambda, ref random);
                caseNameToVal.Add(this.CaseName, parentVal);
            } 
        } 

        static public int? PatientIdOrNullFromCaseName(string caseName) 
        {
            Regex regex = new Regex("[0-9]+-[0-9]+");
            //string[] nameParts = caseName.Split('.');
            //SpecialFunctions.CheckCondition(nameParts.Length == 2);
            //int pid = int.Parse(nameParts[0]);
            //return pid; 
 
            //!!!Need a table from caseName to Pid
            if (caseName.EndsWith(".1") || caseName.Contains(".1.")) 
            {
                string[] nameParts = caseName.Split('.');
                if (caseName.EndsWith(".1"))
                {
                    SpecialFunctions.CheckCondition(nameParts.Length == 3);
                } 
                else 
                {
                    SpecialFunctions.CheckCondition(nameParts.Length == 4 && nameParts[3] == "cont"); 
                }
                int pid = int.Parse(nameParts[1]);
                return pid;
            }
            else if (regex.IsMatch(caseName))//(caseName.Contains("-"))
            { 
                int strainIndexBase0; 
                int pid = SplitOnDashX(caseName, out strainIndexBase0);
                return pid; 
            }

            else
            {
                return null;
            } 
        } 

        public static Leaf GetLeafInstance(StreamReader streamreader) 
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                char peek = Peek(streamreader);
                if (peek == ':') 
                { 
                    break;
                } 
                SpecialFunctions.CheckCondition(!("()".Contains(peek.ToString())));
                sb.Append(Read(streamreader));
            }

            Leaf leaf = new Leaf();
            leaf.CaseName = sb.ToString(); 
            if (leaf.CaseName.Contains("->")) //!!!const 
            {
                int pos = leaf.CaseName.IndexOf("->"); 
                Debug.Assert(pos >= 0); // real assert
                int nodeId = int.Parse(leaf.CaseName.Substring(0, pos));
                leaf.CaseName = leaf.CaseName.Substring(pos + 2);
                Debug.WriteLine(SpecialFunctions.CreateTabString("NodeId", nodeId));
            }
            return leaf; 
        } 

 
        public override IEnumerable<Leaf> AllLeaves()
        {
            yield return this;
        }

        public override IEnumerable<BranchOrLeaf> AllBranchesOrLeaves() 
        { 
            yield return this;
        } 

        public override IEnumerable<BranchOrLeaf> AllBranchesOrLeavesExceptRoot()
        {
            SpecialFunctions.CheckCondition(false);
            return null;
        } 
 

    } 

    public class Branch : BranchOrLeaf
    {
        public bool IsRoot;
        public List<BranchOrLeaf> BranchOrLeafCollection;
 
        internal Branch() 
        {
            BranchOrLeafCollection = new List<BranchOrLeaf>(); 
        }

        public static Branch GetBranchInstance(StreamReader streamreader, bool isRoot)
        {
            Branch branch = new Branch();
            branch.IsRoot = isRoot; 
 
            //!!!fix it so that it can parse empty lists
            bool first = true; 
            while (true)
            {
                char peek = Peek(streamreader);
                if (!first && peek == ')')
                {
                    break; 
                } 

                char c = Read(streamreader); 
                if (first)
                {
                    SpecialFunctions.CheckCondition(c == '(');
                    first = false;
                }
                else 
                { 
                    SpecialFunctions.CheckCondition(c == ',');
                } 

                branch.BranchOrLeafCollection.Add(BranchOrLeaf.GetInstance(streamreader, false));

            }

 
            char rightChar = Read(streamreader); 
            SpecialFunctions.CheckCondition(rightChar == ')');
 
            return branch;
        }

        public override IEnumerable<Leaf> AllLeaves()
        {
            foreach (BranchOrLeaf child in BranchOrLeafCollection) 
            { 
                foreach (Leaf leaf in child.AllLeaves())
                { 
                    yield return leaf;
                }
            }
        }

        public override IEnumerable<BranchOrLeaf> AllBranchesOrLeaves() 
        { 
            yield return this;
            foreach (BranchOrLeaf child in BranchOrLeafCollection) 
            {
                foreach (BranchOrLeaf branchOrLeaf in child.AllBranchesOrLeaves())
                {
                    yield return branchOrLeaf;
                }
            } 
        } 

        public override IEnumerable<BranchOrLeaf> AllBranchesOrLeavesExceptRoot() 
        {
            SpecialFunctions.CheckCondition(IsRoot);
            foreach (BranchOrLeaf child in BranchOrLeafCollection)
            {
                foreach (BranchOrLeaf branchOrLeaf in child.AllBranchesOrLeaves()) //We know that children can't be the root
                { 
                    yield return branchOrLeaf; 
                }
            } 
        }

        public override void Evolve(bool parentVal, double stationaryDistnOfTrue, double lambda, double pMissing,
            ref Random random, Dictionary<string, BooleanStatistics> caseNameToVal)
        {
            bool myVal = EvolveNextVal(parentVal, stationaryDistnOfTrue, lambda, ref random); 
            foreach (BranchOrLeaf child in BranchOrLeafCollection) 
            {
                child.Evolve(myVal, stationaryDistnOfTrue, lambda, pMissing, ref random, caseNameToVal); 
            }
        }

        internal void GenerateRandomBinarySubtree(int depth, double maxBranchLength, ref Random random)
        {
            BranchOrLeafCollection.Clear(); 
            for (int i = 0; i < 2; i++) 
            {
                BranchOrLeaf child; 
                if (depth > 1)
                {
                    child = new Branch();
                    ((Branch)child).GenerateRandomBinarySubtree(depth - 1, maxBranchLength, ref random);
                }
                else 
                { 
                    child = new Leaf();
                } 
                child.Length = maxBranchLength * random.NextDouble();
                BranchOrLeafCollection.Add(child);
            }
        }
    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
