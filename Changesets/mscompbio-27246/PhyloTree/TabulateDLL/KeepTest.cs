using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using Mlas.Tabulate;
 
namespace VirusCount.PhyloTree 
{
    public abstract class KeepTest<TRow> 
    {
        protected KeepTest()
        {
        }

        public static KeepTest<Dictionary<string, string>> GetInstance(string inputDirectory, string keepTestName) 
        { 
            return GetInstance(inputDirectory, null, null, keepTestName, 1, null);
        } 
        public static KeepTest<Dictionary<string, string>> GetInstance(string inputDirectory,
            string binarySeqFileName, string keepTestName, int merSize, Dictionary<int, string> pidToCaseName)
        {
            return GetInstance(inputDirectory, binarySeqFileName, null, keepTestName, merSize, pidToCaseName);
        }
 
        public static KeepTest<Dictionary<string, string>> GetInstance(string inputDirectory, 
            string binarySeqFileName, string hlaFileName, string keepTestName, int merSize, Dictionary<int, string> pidToCaseName)
        { 
            keepTestName = keepTestName.ToLower();

            //!!!would be nice of classes could parse themselves
            if (keepTestName == "alwayskeep")
            {
                return AlwaysKeep<Dictionary<string, string>>.GetInstance(); 
            } 
            else if (keepTestName.StartsWith(Row.Prefix.ToLower()))
            { 
                //Row["TargetName"]="Value"
                string[] stringList = keepTestName.Split('"');
                string errorString = string.Format(@"Row filter should be of the form 'Row[""TargetName""]=""Value""' (not '{0}')", keepTestName);
                SpecialFunctions.CheckCondition(stringList.Length == 5, errorString);
                SpecialFunctions.CheckCondition(stringList[2] == "]=" && stringList[4] == "", errorString);
                return Row.GetInstance(stringList[1], stringList[3]); 
 
            }
            else if (keepTestName.StartsWith(KeepRandom<Dictionary<string, string>>.Prefix.ToLower())) 
            {
                string pString = keepTestName.Substring(KeepRandom<Dictionary<string, string>>.Prefix.Length);
                return KeepRandom<Dictionary<string, string>>.GetInstance(pString);
            }

            if (keepTestName.StartsWith(K1.Prefix.ToLower()))
            { 
                int k1 = int.Parse(keepTestName.Substring(K1.Prefix.Length)); 
                return K1.GetInstance(k1);
            } 
            else if (keepTestName.StartsWith(KeepNonOverlappingAA.Prefix.ToLower())) 
            { 
                return KeepNonOverlappingAA.GetInstance();
            } 
            else if (keepTestName.StartsWith(KeepGene.Prefix.ToLower()))
            {
                string geneRange = keepTestName.Substring(KeepGene.Prefix.Length);
                return KeepGene.GetInstance(geneRange);
            }
            else if (keepTestName.StartsWith(KeepPeptide.Prefix.ToLower())) 
            { 
                string geneRange = keepTestName.Substring(KeepPeptide.Prefix.Length);
                return KeepPeptide.GetInstance(geneRange); 
            }
            else if (keepTestName.StartsWith(KeepSpecificRows.Prefix.ToLower()))
            {
                return KeepSpecificRows.GetInstance(keepTestName.Substring(KeepSpecificRows.Prefix.Length));
            }
            else if (keepTestName.StartsWith(KeepSpecificRow.Prefix.ToLower())) 
            { 
                return KeepSpecificRow.GetInstance(keepTestName.Substring(KeepSpecificRow.Prefix.Length));
            } 
            else if (keepTestName.StartsWith(KeepSpecificGenes.Prefix.ToLower()))
            {
                return KeepSpecificGenes.GetInstance(keepTestName.Substring(KeepSpecificGenes.Prefix.Length));
            }
            else if (keepTestName.StartsWith(KeepOneOfAAPair.Prefix.ToLower()))
            { 
                return KeepOneOfAAPair.GetInstance(); 
            }
            else if (keepTestName.StartsWith(KeepAllButSamePosition.Prefix.ToLower())) 
            {
                return KeepAllButSamePosition.GetInstance();
            }
            else if (keepTestName.StartsWith(KeepAllButSameDeletion.Prefix.ToLower()))
            {
                return KeepAllButSameDeletion.GetInstance(); 
            } 
            else if (keepTestName.StartsWith(KeepAllBut.Prefix.ToLower()))
            { 
                return KeepAllBut.GetInstance(keepTestName.Substring(KeepAllBut.Prefix.Length));
            }
            else if (keepTestName.StartsWith(KeepOnly.Prefix.ToLower()))
            {
                return KeepOnly.GetInstance(keepTestName.Substring(KeepOnly.Prefix.Length));
            } 
            else if (keepTestName.StartsWith(KeepNonTrivialRows.Prefix.ToLower())) 
            {
                return new KeepNonTrivialRows(); 
            }
            else if (keepTestName.StartsWith(KeepTestTemp.Prefix.ToLower()))
            {
                return KeepTestTemp.GetInstance();
            }
            else if (keepTestName.StartsWith(KeepNonRare.Prefix.ToLower()))
            { 
                return KeepNonRare.GetInstance(keepTestName.Substring(KeepNonRare.Prefix.Length)); 
            }
            else if (keepTestName.StartsWith("JointGagPolTest")) 
            {
                return And<Dictionary<string, string>>.GetInstance(
                    //KeepRandom<Dictionary<string,string>>.GetInstance(0, 0.001), // how do we make it the same when we count and when we really run through it?
                    KeepOneOfAAPair.GetInstance(),
                    KeepNonOverlappingAA.GetInstance(),
                    KeepSpecificGenes.GetInstance(keepTestName.Substring("JointGagPolTest".Length))); 
            } 
            else if (keepTestName.StartsWith(KeepPollockOneDirection.Prefix.ToLower()))
            { 
                return KeepPollockOneDirection.GetInstance(keepTestName.Substring(KeepPollockOneDirection.Prefix.Length));
            }
            else if (keepTestName.StartsWith(KeepFisherOneDirection.Prefix.ToLower()))
            {
                return KeepFisherOneDirection.GetInstance(keepTestName.Substring(KeepFisherOneDirection.Prefix.Length));
            } 
            else if (keepTestName.StartsWith(KeepPredictorTargetPairs.Prefix.ToLower())) 
            {
                return KeepPredictorTargetPairs.GetInstance(keepTestName.Substring(KeepPredictorTargetPairs.Prefix.Length)); 
            }
            else if (keepTestName.StartsWith(And<Dictionary<string, string>>.Prefix.ToLower()))
            {
                string[] args;
                // '-' is used for ranges too. if ; exists, split on that so we can And with range keep tests.
                if (keepTestName.Contains(";")) 
                    args = keepTestName.Split(';'); 
                else
                    args = keepTestName.Split('-'); 

                SpecialFunctions.CheckCondition(args[0] == And<Dictionary<string, string>>.Prefix.ToLower());
                List<KeepTest<Dictionary<string, string>>> conjuncts = new List<KeepTest<Dictionary<string, string>>>();
                foreach (string arg in SpecialFunctions.Rest(args))
                {
                    conjuncts.Add(KeepTest<Dictionary<string, string>>.GetInstance(inputDirectory, binarySeqFileName, hlaFileName, arg, merSize, pidToCaseName)); 
                } 

                return And<Dictionary<string, string>>.GetInstance(conjuncts); 
            }
            else
            {
                SpecialFunctions.CheckCondition(false, "Don't know KeepTest " + keepTestName);
                return null;
            } 
        } 

        public abstract bool Test(TRow row); 

        public virtual void Reset() { } // gives KeepTests a change to reset after a run through the entire set.

        public static KeepTest<TRow> GetGenericInstance(string inputDirectory, string keepTestName)
        {
            if (keepTestName == "AlwaysKeep") 
            { 
                return AlwaysKeep<TRow>.GetInstance();
            } 

            Debug.Fail("Don't know KeepTest " + keepTestName);

            return null;
        }
 
        public abstract bool IsCompatibleWithNewKeepTest(KeepTest<TRow> keepTestNew); 
    }
 
    public class AlwaysKeep<TRow> : KeepTest<TRow>
    {
        public AlwaysKeep()
        {
        }
 
        public static KeepTest<TRow> GetInstance() 
        {
            return new AlwaysKeep<TRow>(); 
        }

        public override string ToString()
        {
            return "AlwaysKeep";
        } 
 
        public override bool Test(TRow row)
        { 
            return true;
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<TRow> keepTestNew)
        {
            return true; 
        } 
    }
 
    /// <summary>
    /// Does everything but the Test() method for Collection-based tests, such as And and Or.
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    public abstract class KeepCollection<TRow> : KeepTest<TRow>
    { 
        protected abstract string prefix 
        {
            get; 
        }

        public List<KeepTest<TRow>> KeepTestCollection;

        protected KeepCollection(params KeepTest<TRow>[] keepTests)
            : 
            this((IEnumerable<KeepTest<TRow>>)keepTests) { } 

        protected KeepCollection(IEnumerable<KeepTest<TRow>> keepTestCollection) 
        {
            KeepTestCollection = new List<KeepTest<TRow>>();
            foreach (KeepTest<TRow> keepTest in keepTestCollection)
            {
                if (!(keepTest is AlwaysKeep<TRow>))
                { 
                    KeepTestCollection.Add(keepTest); 
                }
            } 

            if (KeepTestCollection.Count == 0)
            {
                KeepTestCollection.Add(AlwaysKeep<TRow>.GetInstance());
            }
        } 
 
        public override string ToString()
        { 
            return string.Format("{0}-{1}", prefix, SpecialFunctions.Join("-", KeepTestCollection));
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<TRow> keepTestNew)
        {
            KeepCollection<TRow> newCollection = (KeepCollection<TRow>)keepTestNew; 
            if (newCollection == null || this.KeepTestCollection.Count != newCollection.KeepTestCollection.Count) 
            {
                return false; 
            }
            //!!!could try every permuation, but for now require them to be in the same order

            for (int i = 0; i < KeepTestCollection.Count; ++i)
            {
                if (!KeepTestCollection[i].IsCompatibleWithNewKeepTest(newCollection.KeepTestCollection[i])) 
                { 
                    return false;
                } 
            }
            return true;
        }
    }

    public class And<TRow> : KeepCollection<TRow> 
    { 
        internal const string Prefix = "And";
 
        protected override string prefix
        {
            get { return Prefix; }
        }

 
        private And(params KeepTest<TRow>[] keepTests) 
            :
            this((IEnumerable<KeepTest<TRow>>)keepTests) { } 

        private And(IEnumerable<KeepTest<TRow>> keepTests)
            :
            base(keepTests) { }

        public static KeepTest<TRow> GetInstance(params KeepTest<TRow>[] keepTests) 
        { 
            return GetInstance((IEnumerable<KeepTest<TRow>>)keepTests);
        } 

        public static KeepTest<TRow> GetInstance(IEnumerable<KeepTest<TRow>> keepTests)
        {
            And<TRow> andTest = new And<TRow>(keepTests);
            if (andTest.KeepTestCollection.Count == 1)
            { 
                return andTest.KeepTestCollection[0]; 
            }
            else 
            {
                return andTest;
            }
        }

        public override bool Test(TRow row) 
        { 
            foreach (KeepTest<TRow>keepTest in KeepTestCollection)
            { 
                if (!keepTest.Test(row))
                {
                    return false;
                }
            }
            return true; 
        } 

    } 

    public class Or<TRow> : KeepCollection<TRow>
    {
        internal const string Prefix = "Or";

        protected override string prefix 
        { 
            get { return Prefix; }
        } 


        protected Or(params KeepTest<TRow>[] keepTests)
            :
            this((IEnumerable<KeepTest<TRow>>)keepTests) { }
 
        protected Or(IEnumerable<KeepTest<TRow>> keepTests) 
            :
            base(keepTests) { } 

        public static KeepTest<TRow> GetInstance(params KeepTest<TRow>[] keepTests)
        {
            return GetInstance((IEnumerable<KeepTest<TRow>>)keepTests);
        }
 
        public static KeepTest<TRow> GetInstance(IEnumerable<KeepTest<TRow>> keepTests) 
        {
            Or<TRow> orTest = new Or<TRow>(keepTests); 
            if (orTest.KeepTestCollection.Count == 1)
            {
                return orTest.KeepTestCollection[0];
            }
            else
            { 
                return orTest; 
            }
        } 

        public override bool Test(TRow row)
        {
            foreach (KeepTest<TRow> keepTest in KeepTestCollection)
            {
                if (keepTest.Test(row)) 
                { 
                    return true;
                } 
            }
            return false;
        }

    }
 
    /// <summary> 
    /// Returns true if exactly 1 of the collection of KeepTests returns true.
    /// </summary> 
    /// <typeparam name="TRow"></typeparam>
    public class Xor<TRow> : KeepCollection<TRow>
    {
        internal const string Prefix = "Xor";

        protected override string prefix 
        { 
            get { return Prefix; }
        } 


        private Xor(params KeepTest<TRow>[] keepTests)
            :
            this((IEnumerable<KeepTest<TRow>>)keepTests) { }
 
        private Xor(IEnumerable<KeepTest<TRow>> keepTests) 
            :
            base(keepTests) { } 

        public static KeepTest<TRow> GetInstance(params KeepTest<TRow>[] keepTests)
        {
            return GetInstance((IEnumerable<KeepTest<TRow>>)keepTests);
        }
 
        public static KeepTest<TRow> GetInstance(IEnumerable<KeepTest<TRow>> keepTests) 
        {
            Xor<TRow> xOrTest = new Xor<TRow>(keepTests); 
            if (xOrTest.KeepTestCollection.Count == 1)
            {
                return xOrTest.KeepTestCollection[0];
            }
            else
            { 
                return xOrTest; 
            }
        } 

        public override bool Test(TRow row)
        {
            bool returnVal = false;
            foreach (KeepTest<TRow> keepTest in KeepTestCollection)
            { 
                if (keepTest.Test(row)) 
                {
                    if (returnVal)  // someone else has already returned true, so we're done. 
                        return false;
                    else
                        returnVal = true;
                }
            }
            return returnVal; 
        } 
    }
 
    public class KeepRandom<TRow> : KeepTest<TRow>
    {
        public const string Prefix = "KeepRandom";
        private Random _rand;
        private double p;
        private int _seed; 
        private KeepRandom(double p, int seed) 
        {
            SpecialFunctions.CheckCondition(p >= 0 && p <= 1, p + " is not a probability"); 
            _rand = new Random(seed);
            _seed = seed;
            this.p = p;
        }

        public static KeepRandom<TRow> GetInstance(string pString) 
        { 
            return new KeepRandom<TRow>(double.Parse(pString), 0);
        } 
        public static KeepRandom<TRow> GetInstance(double p)
        {
            return new KeepRandom<TRow>(p, 0);
        }

        public override bool Test(TRow row) 
        { 
            return _rand.NextDouble() < p;
        } 

        public override bool IsCompatibleWithNewKeepTest(KeepTest<TRow> keepTestNew)
        {
            return false;   // random can't be compatible will anything
        }
 
        public override string ToString() 
        {
            return Prefix + p; 
        }

        /// <summary>
        /// Reset random
        /// </summary>
        public override void Reset() 
        { 
            base.Reset();
            _rand = new Random(_seed); 
        }
    }



    //public class KeepPopularHlas : KeepTest<Dictionary<string, string>> 
    //{ 
    //    private KeepPopularHlas()
    //    { 
    //    }

    //    private int PidsPerHlaRequired;
    //    private CaseNamesAndHlas PidsAndHlas;
    //    private PatientsAndSequences PatientsAndSequences;
 
    //    internal static KeepTest<Dictionary<string, string>>GetInstance(string inputDirectory, string binarySeqFileName, string hlaFileName, int pidsPerHlaRequired, int merSize, Dictionary<int, string> pidToCaseName) 
    //    {
    //        KeepPopularHlas aKeepPopularHlas = new KeepPopularHlas(); 
    //        aKeepPopularHlas.PidsPerHlaRequired = pidsPerHlaRequired;
    //        aKeepPopularHlas.PatientsAndSequences = PatientsAndSequences.GetInstance(inputDirectory, binarySeqFileName, merSize, pidToCaseName);
    //        aKeepPopularHlas.PidsAndHlas = CaseNamesAndHlas.GetInstance(inputDirectory, aKeepPopularHlas.PatientsAndSequences, pidToCaseName, hlaFileName);

    //        //aKeepPopularHlas.PidsAndHlas.Test();
 
    //        return aKeepPopularHlas; 
    //    }
 
    //    internal static string Prefix = "KeepPopularHlas";

    //    public override string ToString()
    //    {
    //        return Prefix + PidsPerHlaRequired.ToString();
    //    } 
 
    //    public override bool Test(Dictionary<string, string> row)
    //    { 
    //        string hla = row[Tabulate.HlaColumnName];
    //        if (!PidsAndHlas.HlaToCaseNameSet.ContainsKey(hla))
    //        {
    //            return false;
    //        }
    //        Set<string> pidSet = PidsAndHlas.HlaToCaseNameSet[hla]; 
    //        return pidSet.Count >= PidsPerHlaRequired; 
    //    }
 
    //    //!!!would be nice if class didn't have to know all these classes it was compatible with
    //    public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>>keepTestNew)
    //    {
    //        if (keepTestNew is KeepPopularHlas)
    //        {
    //            return PidsPerHlaRequired <= ((KeepPopularHlas)keepTestNew).PidsPerHlaRequired; 
    //        } 

    //        if (keepTestNew is And<Dictionary<string, string>>) 
    //        {
    //            And<Dictionary<string, string>> aAnd = (And<Dictionary<string, string>>)keepTestNew;
    //            foreach (KeepTest<Dictionary<string, string>>conjunct in aAnd.KeepTestCollection)
    //            {
    //                if (!IsCompatibleWithNewKeepTest(conjunct))
    //                { 
    //                    return false; 
    //                }
    //            } 
    //            return true;
    //        }

    //        return false;
    //    }
    //} 
 
    //public class KeepPopularMers : KeepTest<Dictionary<string, string>>
    //{ 
    //    private KeepPopularMers()
    //    {
    //    }

    //    private int CasesPerMerRequired;
    //    private PatientsAndSequences PatientsAndSequences; 
    //    private int MerSize; 

    //    internal static KeepTest<Dictionary<string, string>>GetInstance(string inputDirectory, string binarySeqFileName, int casesPerMerRequired, int merSize, Dictionary<int, string> pidToCaseName) 
    //    {
    //        KeepPopularMers aKeepPopularMers = new KeepPopularMers();
    //        aKeepPopularMers.CasesPerMerRequired = casesPerMerRequired;
    //        SpecialFunctions.CheckCondition(merSize > 1, "This test only makes sense for mers larger than 1");
    //        aKeepPopularMers.MerSize = merSize;
    //        aKeepPopularMers.PatientsAndSequences = PatientsAndSequences.GetInstance(inputDirectory, binarySeqFileName, merSize, pidToCaseName); 
 
    //        //aKeepPopularMers.PidsAndMers.Test();
 
    //        return aKeepPopularMers;
    //    }

    //    internal static string Prefix = "KeepPopularMers";

    //    public override string ToString() 
    //    { 
    //        return Prefix + CasesPerMerRequired.ToString();
    //    } 

    //    public override bool Test(Dictionary<string, string> row)
    //    {
    //        Mer mer = Mer.GetInstance(MerSize, row[Tabulate.MerTargetColumnName]);
    //        SpecialFunctions.CheckCondition(mer.ToString().Length != 1, "This test doesn't make sense to apply to 1mers");
 
    //        if (!PatientsAndSequences.MerToCaseNameSet.ContainsKey(mer)) 
    //        {
    //            return false; 
    //        }
    //        Set<string> pidSet = PatientsAndSequences.MerToCaseNameSet[mer];
    //        //Debug.WriteLine(pidSet.Count);
    //        return pidSet.Count >= CasesPerMerRequired;
    //    }
 
    //    //!!!would be nice if class didn't have to know all these classes it was compatible with 
    //    public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>>keepTestNew)
    //    { 
    //        if (keepTestNew is KeepPopularMers)
    //        {
    //            return CasesPerMerRequired <= ((KeepPopularMers)keepTestNew).CasesPerMerRequired;
    //        }

    //        if (keepTestNew is And<Dictionary<string, string>>) 
    //        { 
    //            And<Dictionary<string, string>> aAnd = (And<Dictionary<string, string>>)keepTestNew;
    //            foreach (KeepTest<Dictionary<string, string>>conjunct in aAnd.KeepTestCollection) 
    //            {
    //                if (!IsCompatibleWithNewKeepTest(conjunct))
    //                {
    //                    return false;
    //                }
    //            } 
    //            return true; 
    //        }
 
    //        return false;
    //    }
    //}

    //public class KeepMersAtPosition : KeepTest<Dictionary<string, string>>
    //{ 
    //    private KeepMersAtPosition() 
    //    {
    //    } 

    //    private int CasesPerMerRequired;
    //    private PatientsAndSequences PatientsAndSequences;
    //    private int MerSize;

    //    internal static KeepTest<Dictionary<string, string>>GetInstance(string inputDirectory, string binarySeqFileName, int casesRequired, int merSize, Dictionary<int, string> pidToCaseName) 
    //    { 
    //        KeepMersAtPosition aKeepMersAtPosition = new KeepMersAtPosition();
    //        aKeepMersAtPosition.CasesPerMerRequired = casesRequired; 
    //        aKeepMersAtPosition.MerSize = merSize;
    //        aKeepMersAtPosition.PatientsAndSequences = PatientsAndSequences.GetInstance(inputDirectory, binarySeqFileName, merSize, pidToCaseName);

    //        //aKeepMersAtPosition.PidsAndMers.Test();

    //        return aKeepMersAtPosition; 
    //    } 

    //    internal static string Prefix = "KeepMersAtPosition"; 

    //    public override string ToString()
    //    {
    //        return Prefix + CasesPerMerRequired.ToString();
    //    }
 
    //    public override bool Test(Dictionary<string, string> row) 
    //    {
    //        Mer mer = Mer.GetInstance(MerSize, row[Tabulate.MerTargetColumnName]); 
    //        int n1Pos = int.Parse(row[Tabulate.Nuc1TargetPositionColumnName]);

    //        Dictionary<string,bool?> caseNameToTargetValue = PatientsAndSequences.N1PosToMerToCaseNameToBool[n1Pos][mer];
    //        int trueCount = SpecialFunctions.CountIf(caseNameToTargetValue.Values, delegate(bool? value) { return value != null && (bool)value; });
    //        return trueCount >= CasesPerMerRequired;
    //    } 
 
    //    //!!!would be nice if class didn't have to know all these classes it was compatible with
    //    public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
    //    {
    //        if (keepTestNew is KeepMersAtPosition)
    //        {
    //            return CasesPerMerRequired <= ((KeepMersAtPosition)keepTestNew).CasesPerMerRequired;
    //        }
 
    //        //!!!this code is duplicate many times 
    //        if (keepTestNew is And<Dictionary<string, string>>)
    //        { 
    //            And<Dictionary<string, string>> aAnd = (And<Dictionary<string, string>>)keepTestNew;
    //            foreach (KeepTest<Dictionary<string, string>> conjunct in aAnd.KeepTestCollection)
    //            {
    //                if (!IsCompatibleWithNewKeepTest(conjunct))
    //                {
    //                    return false; 
    //                } 
    //            }
    //            return true; 
    //        }

    //        return false;
    //    }
    //}
 
 
    //public class K2 : KeepTest<Dictionary<string, string>>
    //{ 
    //    private K2()
    //    {
    //    }

    //    int k2;
    //    internal static KeepTest<Dictionary<string, string>> GetInstance(int k2) 
    //    { 
    //        if (k2 == int.MaxValue)
    //        { 
    //            return AlwaysKeep<Dictionary<string, string>>.GetInstance();
    //        }
    //        K2 aK2 = new K2();
    //        aK2.k2 = k2;
    //        return aK2;
    //    } 
 
    //    internal static string Prefix = "K2=";
 
    //    public override string ToString()
    //    {
    //        return string.Format("{0}{1}", Prefix, k2);
    //    }

    //    public override bool Test(Dictionary<string, string> row) 
    //    { 
    //        int nullCount = int.Parse(row[Tabulate.PredictorNullNameCountColumnName]);
    //        return (nullCount <= k2); 
    //    }

    //    //!!!would be nice if class didn't have to know all these classes it was compatible with
    //    public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
    //    {
    //        if (keepTestNew is K2) 
    //        { 
    //            return k2 >= ((K2)keepTestNew).k2;
    //        } 


    //        //!!!this code is duplicate many times
    //        if (keepTestNew is And<Dictionary<string, string>>)
    //        {
    //            And<Dictionary<string, string>> aAnd = (And<Dictionary<string, string>>)keepTestNew; 
    //            foreach (KeepTest<Dictionary<string, string>> conjunct in aAnd.KeepTestCollection) 
    //            {
    //                if (!IsCompatibleWithNewKeepTest(conjunct)) 
    //                {
    //                    return false;
    //                }
    //            }
    //            return true;
    //        } 
 
    //        return false;
    //    } 
    //}

    public class Row : KeepTest<Dictionary<string, string>>
    {
        private Row()
        { 
        } 

        private string ColumnName; 
        private string GoalValue;

        new internal static KeepTest<Dictionary<string, string>> GetInstance(string columnName, string goalValue)
        {
            Row aRow = new Row();
            aRow.ColumnName = columnName; 
            aRow.GoalValue = goalValue; 
            return aRow;
        } 

        internal static string Prefix = "Row[";

        public override string ToString()
        {
            return string.Format(@"{0}""{1}""]=""{2}""", Prefix, ColumnName, GoalValue); 
        } 

        public override bool Test(Dictionary<string, string> row) 
        {
            bool test = row[ColumnName].Equals(GoalValue, StringComparison.CurrentCultureIgnoreCase);
            return test;
        }

        //!!!would be nice if class didn't have to know all these classes it was compatible with 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        {
            if (keepTestNew is Row) 
            {
                return ColumnName == ((Row)keepTestNew).ColumnName && GoalValue == ((Row)keepTestNew).GoalValue;
            }

            if (keepTestNew is And<Dictionary<string, string>>)
            { 
                And<Dictionary<string, string>> aAnd = (And<Dictionary<string, string>>)keepTestNew; 
                foreach (KeepTest<Dictionary<string, string>> conjunct in aAnd.KeepTestCollection)
                { 
                    if (!IsCompatibleWithNewKeepTest(conjunct))
                    {
                        return false;
                    }
                }
                return true; 
            } 

            return false; 
        }
    }

    public class KeepPollockOneDirection : KeepTest<Dictionary<string, string>>
    {
        bool keepPositiveCorrelation; 
 
        public const string Prefix = "KeepPollockOneDirection";
 
        private KeepPollockOneDirection() { }

        public static KeepPollockOneDirection GetInstance(string directionAttractionOrEscape)
        {
            KeepPollockOneDirection keepTest = new KeepPollockOneDirection();
            if (directionAttractionOrEscape == "attraction") 
            { 
                keepTest.keepPositiveCorrelation = true;
            } 
            else if (directionAttractionOrEscape == "escape")
            {
                keepTest.keepPositiveCorrelation = false;
            }
            else
            { 
                throw new ArgumentException(directionAttractionOrEscape + " must be either Attraction or Escape."); 
            }
            return keepTest; 
        }

        public override bool Test(Dictionary<string, string> row)
        {
            double pAB, pAb, paB, pab;
            if (row.ContainsKey("P_TT")) 
            { 
                pAB = double.Parse(row["P_TT"]);
                pAb = double.Parse(row["P_TF"]); 
                paB = double.Parse(row["P_FT"]);
                pab = double.Parse(row["P_FF"]);
            }
            else
            {
                pAB = double.Parse(row["P_AB"]); 
                pAb = double.Parse(row["P_Ab"]); 
                paB = double.Parse(row["P_aB"]);
                pab = double.Parse(row["P_ab"]); 
            }

            double directionCoeff = pAB*pab - pAb*paB;
            if (keepPositiveCorrelation)
            {
                return directionCoeff >= 0; 
            } 
            else
            { 
                return directionCoeff <= 0;
            }
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        { 
            throw new Exception("The method or operation is not implemented."); 
        }
    } 

    public class KeepFisherOneDirection : KeepTest<Dictionary<string, string>>
    {
        bool keepPositiveCorrelation;

        public const string Prefix = "KeepFisherOneDirection"; 
 
        private KeepFisherOneDirection() { }
 
        public static KeepFisherOneDirection GetInstance(string directionAttractionOrEscape)
        {
            KeepFisherOneDirection keepTest = new KeepFisherOneDirection();
            if (directionAttractionOrEscape == "attraction")
            {
                keepTest.keepPositiveCorrelation = true; 
            } 
            else if (directionAttractionOrEscape == "escape")
            { 
                keepTest.keepPositiveCorrelation = false;
            }
            else
            {
                throw new ArgumentException(directionAttractionOrEscape + " must be either Attraction or Escape.");
            } 
            return keepTest; 
        }
 
        public override bool Test(Dictionary<string, string> row)
        {
            double pAB = double.Parse(row["TT"]);
            double pAb = double.Parse(row["TF"]);
            double paB = double.Parse(row["FT"]);
            double pab = double.Parse(row["FF"]); 
 
            double directionCoeff = pAB * pab - pAb * paB;
            if (keepPositiveCorrelation) 
            {
                return directionCoeff >= 0;
            }
            else
            {
                return directionCoeff <= 0; 
            } 
        }
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
 
    //public class KeepEndOfGag : KeepTest<Dictionary<string, string>> 
    //{
    //    private KeepEndOfGag() 
    //    {
    //    }

    //    bool KeepIt;
    //    internal static KeepTest<Dictionary<string, string>> GetInstance(bool keepIt)
    //    { 
    //        if (keepIt) 
    //        {
    //            return AlwaysKeep<Dictionary<string, string>>.GetInstance(); 
    //        }

    //        KeepEndOfGag aKeepEndOfGag = new KeepEndOfGag();
    //        aKeepEndOfGag.KeepIt = keepIt;
    //        return aKeepEndOfGag;
    //    } 
 
    //    internal static string Prefix = "KeepEndOfGag";
 
    //    public override string ToString()
    //    {
    //        return string.Format("KeepEndOfGag{0}", KeepIt);
    //    }

    //    public override bool Test(Dictionary<string, string> row) 
    //    { 
    //        Debug.Assert(!KeepIt); // real assert
 
    //        int nuc1Position = int.Parse(row[Tabulate.Nuc1TargetPositionColumnName]);
    //        SpecialFunctions.CheckCondition((nuc1Position % 3) != 2, "nuc1Position is in neither the Gag nor the Pol frame");
    //        bool gagFrame = (nuc1Position % 3) == 1;
    //        if (gagFrame)
    //        {
    //            return nuc1Position < 2085; 
    //        } 
    //        else
    //        { 
    //            return true;
    //        }

    //    }

    //    public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
    //    { 
    //        return false; //!!!could be made tighter
    //    } 
    //}

    public class K1 : KeepTest<Dictionary<string, string>>
    {
        private K1()
        { 
        } 

        int k1; 
        internal static KeepTest<Dictionary<string, string>> GetInstance(int k1)
        {
            if (k1 == 0)
            {
                return AlwaysKeep<Dictionary<string, string>>.GetInstance();
            } 
 
            K1 aK1 = new K1();
            aK1.k1 = k1; 
            return aK1;
        }

        internal static string Prefix = "K1=";

        public override string ToString() 
        { 
            return string.Format("K1={0}", k1);
        } 

        public override bool Test(Dictionary<string, string> row)
        {
            int trueCount = int.Parse(row[Tabulate.PredictorTrueNameCountColumnName]);
            int falseCount = int.Parse(row[Tabulate.PredictorFalseNameCountColumnName]);
            return (trueCount >= k1 && falseCount >= k1); 
        } 

        //!!!would be nice if class didn't have to know all these classes it was compatible with 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            if (keepTestNew is K1)
            {
                return k1 <= ((K1)keepTestNew).k1;
            } 
 
            //!!!This code is duplicate many times
            if (keepTestNew is And<Dictionary<string, string>>) 
            {
                And<Dictionary<string, string>> aAnd = (And<Dictionary<string, string>>)keepTestNew;
                foreach (KeepTest<Dictionary<string, string>> conjunct in aAnd.KeepTestCollection)
                {
                    if (!IsCompatibleWithNewKeepTest(conjunct))
                    { 
                        return false; 
                    }
                } 
                return true;
            }

            return false;
        }
    } 
 
    public class KeepNonOverlappingAA : KeepTest<Dictionary<string, string>>
    { 
        internal static readonly String Prefix = "KeepNonOverlappingAA";

        private KeepNonOverlappingAA() { }

        public static KeepNonOverlappingAA GetInstance()
        { 
            return new KeepNonOverlappingAA(); 
        }
 
        public override bool Test(Dictionary<string, string> row)
        {
            //string predVar = row[Tabulate.PredictorVariableColumnName];
            //int predPos = int.Parse(predVar.Split('@')[1]);
            //string targVar = row[Tabulate.TargetVariableColumnName];
            //int targPos = int.Parse(targVar.Split('@')[1]); 
 
            KeyValuePair<string, int> pred = Tabulate.GetMerAndPos(row[Tabulate.PredictorVariableColumnName]);
            KeyValuePair<string, int> targ = Tabulate.GetMerAndPos(row[Tabulate.TargetVariableColumnName]); 
            return Math.Abs(targ.Value - pred.Value) > 2;
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            // haven't done anything here. 
            return false; 
        }
 

    }

    public class KeepGene : KeepTest<Dictionary<string, string>>
    {
        internal static readonly String Prefix = "KeepGene"; 
 
        private int _start, _stop;
        private KeepGene() { } 

        public static KeepGene GetInstance(string geneRange)
        {
            KeepGene keepGene = new KeepGene();
            string[] fields = geneRange.Split('-');
            try 
            { 
                keepGene._start = int.Parse(fields[0]);
                keepGene._stop = int.Parse(fields[1]); 
            }
            catch
            {
                throw new FormatException(string.Format("Could not parse range {0}.", geneRange));
            }
            return keepGene; 
        } 

        public override bool Test(Dictionary<string, string> row) 
        {
            bool keepTarget = true;
            bool keepPredictor = true;

            string targVar = row[Tabulate.TargetVariableColumnName];
            string predVar = row[Tabulate.PredictorVariableColumnName]; 
 
            if (targVar.Contains("@"))
            { 
                //int pos = int.Parse(targVar.Split('@')[1]);
                int pos = Tabulate.GetMerAndPos(targVar).Value;

                keepTarget = pos >= _start && pos <= _stop && (pos - _start) % 3 == 0;
            }
            if (predVar.Contains("@")) 
            { 
                int pos = Tabulate.GetMerAndPos(predVar).Value;
                //int pos = int.Parse(predVar.Split('@')[1]); 
                keepPredictor = pos >= _start && pos <= _stop && (pos - _start) % 3 == 0;
            }

            return keepTarget && keepPredictor;

        } 
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        { 
            // haven't done anything here.
            return false;
        }

        public override string ToString()
        { 
            return string.Format("KeepGene{0}-{1}", _start, _stop); 
        }
 

    }

    public class KeepPeptide : KeepTest<Dictionary<string, string>>
    {
        internal static readonly String Prefix = "KeepPeptide"; 
 
        private int _start, _stop;
        private KeepPeptide() { } 

        public static KeepPeptide GetInstance(string peptideRangeInAminoAcidSpace)
        {
            KeepPeptide keepGene = new KeepPeptide();
            string[] fields = peptideRangeInAminoAcidSpace.Split('-');
            try 
            { 
                keepGene._start = int.Parse(fields[0]);
                keepGene._stop = int.Parse(fields[1]); 
            }
            catch
            {
                throw new FormatException(string.Format("Could not parse range {0}.", peptideRangeInAminoAcidSpace));
            }
            return keepGene; 
        } 

        public override bool Test(Dictionary<string, string> row) 
        {
            bool keepTarget = true;
            bool keepPredictor = true;

            string targVar = row[Tabulate.TargetVariableColumnName];
            string predVar = row[Tabulate.PredictorVariableColumnName]; 
 
            if (targVar.Contains("@"))
            { 
                int pos = Tabulate.GetMerAndPos(targVar).Value;
                //int pos = int.Parse(targVar.Split('@')[1]);
                keepTarget = pos >= _start && pos <= _stop;
            }
            if (predVar.Contains("@"))
            { 
                int pos = Tabulate.GetMerAndPos(predVar).Value; 
                //int pos = int.Parse(predVar.Split('@')[1]);
                keepPredictor = pos >= _start && pos <= _stop; 
            }

            return keepTarget && keepPredictor;

        }
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        {
            // haven't done anything here. 
            return false;
        }

        public override string ToString()
        {
            return string.Format("KeepPeptide{0}-{1}", _start, _stop); 
        } 

 
    }

    public class KeepSpecificGenes : Or<Dictionary<string, string>>
    {
        new public const string Prefix = "KeepSpecificGenes";
 
        public static KeepTest<Dictionary<string, string>> GetInstance(string geneRanges) 
        {
            List<KeepTest<Dictionary<string, string>>> geneList = new List<KeepTest<Dictionary<string, string>>>(); 

            string[] genes = geneRanges.Split(',');
            foreach (string gene in genes)
            {
                geneList.Add(KeepGene.GetInstance(gene));
            } 
 
            return Or<Dictionary<string, string>>.GetInstance(geneList);
 
        }

        public override bool Test(Dictionary<string, string> row)
        {
            throw new Exception("The method or operation is not implemented.");
        } 
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        { 
            throw new Exception("The method or operation is not implemented.");
        }

        public override string ToString()
        {
            return Prefix; 
        } 
    }
    public class KeepOneOfAAPair : KeepTest<Dictionary<string, string>> 
    {
        public static readonly string Prefix = "KeepOneOfAAPair";

        private KeepOneOfAAPair() { }

        public static KeepTest<Dictionary<string, string>> GetInstance() 
        { 
            return new KeepOneOfAAPair();
        } 

        public override bool Test(Dictionary<string, string> row)
        {
            string predictor = row[Tabulate.PredictorVariableColumnName];
            string target = row[Tabulate.TargetVariableColumnName];
            //int pos1 = int.Parse(predictor.Split('@')[1]); 
            //int pos2 = int.Parse(target.Split('@')[1]); 

            int pos1 = Tabulate.GetMerAndPos(predictor).Value; 
            int pos2 = Tabulate.GetMerAndPos(target).Value;

            //return pos1 >= pos2;
            return pos1 <= pos2;
        }
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        {
            throw new Exception("The method or operation is not implemented."); 
        }
        public override string ToString()
        {
            return Prefix;
        }
    } 
    public class KeepAllButSamePosition : KeepTest<Dictionary<string, string>> 
    {
        public static readonly string Prefix = "KeepAllButSamePosition"; 

        private KeepAllButSamePosition() { }

        public static KeepTest<Dictionary<string, string>> GetInstance()
        {
            return new KeepAllButSamePosition(); 
        } 

        public override bool Test(Dictionary<string, string> row) 
        {
            string predictor = row[Tabulate.PredictorVariableColumnName];
            string target = row[Tabulate.TargetVariableColumnName];
            //int pos1 = int.Parse(predictor.Split('@')[1]);
            //int pos2 = int.Parse(target.Split('@')[1]);
            int pos1 = Tabulate.GetMerAndPos(predictor).Value; 
            int pos2 = Tabulate.GetMerAndPos(target).Value; 

 
            //return pos1 >= pos2;
            return pos1 != pos2;
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        { 
            throw new Exception("The method or operation is not implemented."); 
        }
        public override string ToString() 
        {
            return Prefix;
        }
    }

    public class KeepAllButSameDeletion : KeepTest<Dictionary<string, string>> 
    { 
        public static readonly string Prefix = "KeepAllButSameDeletion";
 
        private KeepAllButSameDeletion() { }

        public static KeepTest<Dictionary<string, string>> GetInstance()
        {
            return new KeepAllButSameDeletion();
        } 
 
        public override bool Test(Dictionary<string, string> row)
        { 
            string predictor = row[Tabulate.PredictorVariableColumnName];
            string target = row[Tabulate.TargetVariableColumnName];
            //string[] predParts = predictor.Split('@');
            //string[] targParts = target.Split('@');

            //string predAA = predParts[0]; 
            //string targAA = targParts[0]; 
            //int pos1 = int.Parse(predParts[1]);
            //int pos2 = int.Parse(targParts[1]); 

            KeyValuePair<string, int> pred = Tabulate.GetMerAndPos(predictor);
            KeyValuePair<string, int> targ = Tabulate.GetMerAndPos(target);

            //return pos1 >= pos2;
            return pred.Key != targ.Key || 
                pred.Key != "-" || 
                targ.Key != "-" ||
                Math.Abs(pred.Value - targ.Value) > 5;    // reject anything that's part of the same deletion. 5 is totally arbitrary. 
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            throw new Exception("The method or operation is not implemented.");
        } 
        public override string ToString() 
        {
            return Prefix; 
        }
    }
    public class KeepSpecificRow : KeepTest<Dictionary<string, string>>
    {
        internal static readonly String Prefix = "KeepSpecificRow";
 
        //private Dictionary<string, string> _testRow; 
        private string _predictor, _target;
 
        private KeepSpecificRow(string predictor, string target)
        {
            _predictor = predictor;
            _target = target;
        }
 
        /// <summary> 
        /// Useful for quickly hard coding the row you want to keep.
        /// </summary> 
        /// <returns></returns>
        //public static KeepSpecificRow GetInstance()
        //{
        //    return GetInstance(895, "W", 886, "H");
        //}
 
        public static KeepSpecificRow GetInstance(string commaDelimitedRowDefn) 
        {
            string[] fields = commaDelimitedRowDefn.Split(','); 
            SpecialFunctions.CheckCondition(fields.Length == 2);
            return GetInstance(fields[0], fields[1]);
        }

        new public static KeepSpecificRow GetInstance(string predictor, string target)
        { 
            return new KeepSpecificRow(predictor, target); 
        }
 
        public override bool Test(Dictionary<string, string> row)
        {
            return row[Tabulate.PredictorVariableColumnName].Equals(_predictor, StringComparison.CurrentCultureIgnoreCase) &&
                row[Tabulate.TargetVariableColumnName].Equals(_target, StringComparison.CurrentCultureIgnoreCase);
        }
 
        //public static KeepSpecificRow GetInstance(int predPos, string predMer, int targPos, string targMer) 
        //{
        //    KeepSpecificRow aKeepTest = new KeepSpecificRow(); 
        //    aKeepTest._testRow = new Dictionary<string, string>();

        //    aKeepTest._testRow.Add(Tabulate.Nuc1TargetPositionColumnName, targPos.ToString());
        //    aKeepTest._testRow.Add(Tabulate.Nuc2PredictorPositionColumnName, predPos.ToString());
        //    aKeepTest._testRow.Add(Tabulate.MerTargetColumnName, targMer.ToString());
        //    aKeepTest._testRow.Add(Tabulate.MerPredictorColumnName, predMer.ToString()); 
 
        //    return aKeepTest;
        //} 

        //public override bool Test(Dictionary<string, string> row)
        //{
        //    return
        //        row[Tabulate.Nuc2PredictorPositionColumnName] == _testRow[Tabulate.Nuc2PredictorPositionColumnName] &&
        //        row[Tabulate.MerPredictorColumnName] == _testRow[Tabulate.MerPredictorColumnName] && 
        //        row[Tabulate.Nuc1TargetPositionColumnName] == _testRow[Tabulate.Nuc1TargetPositionColumnName] && 
        //        row[Tabulate.MerTargetColumnName] == _testRow[Tabulate.MerTargetColumnName];
        //} 

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            return false;
        }
    } 
    public class KeepSpecificRows : KeepTest<Dictionary<string, string>> 
    {
        internal static readonly String Prefix = "KeepSpecificRows"; 

        private List<KeepSpecificRow> _testRows;

        private KeepSpecificRows() { }

        //public static KeepSpecificRows GetInstance() 
        //{ 
        //    List<KeepSpecificRow> keepList = new List<KeepSpecificRow>();
        //    //// this is the list of associations that have q < 0.01 in Gag Within gene. 
        //    //keepList.Add(KeepSpecificRow.GetInstance(2134, "P", 2146, "L"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2134, "L", 2146, "P"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2146, "P", 2134, "L"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2146, "L", 2134, "P"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(1957, "I", 1990, "L"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(1990, "L", 1957, "I")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(2215, "K", 2227, "D")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(1579, "K", 1306, "A"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(1957, "V", 1990, "I")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(1306, "A", 1579, "K"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(1591, "M", 1579, "K"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(1579, "K", 1591, "M"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(1591, "M", 1306, "A"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2206, "P", 2182, "F"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(1591, "L", 1579, "R")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(2227, "D", 2215, "K")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(1306, "A", 1591, "M"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(1471, "I", 1531, "T")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(1579, "R", 1591, "L"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2206, "P", 2176, "F"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2206, "P", 2215, "K"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2239, "E", 2224, "E"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2224, "E", 2239, "E"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2215, "G", 2206, "G")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(2206, "G", 2215, "G")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(1531, "T", 1471, "I"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2176, "Q", 2170, "Q")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(2170, "Q", 2176, "Q"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2212, "P", 2185, "E"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(2185, "E", 2212, "P"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(895, "W", 889, "I"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(904, "R", 889, "I"));
        //    //keepList.Add(KeepSpecificRow.GetInstance(904, "R", 895, "W")); 
        //    //keepList.Add(KeepSpecificRow.GetInstance(901, "C", 889, "I")); 

        //    return GetInstance(keepList); 
        //}

        public static KeepSpecificRows GetInstance(string semiColonDelimitedRows)
        {
            string[] rowDefs = semiColonDelimitedRows.Split(';');
            SpecialFunctions.CheckCondition(rowDefs.Length > 0); 
 
            List<KeepSpecificRow> rows = new List<KeepSpecificRow>(rowDefs.Length);
            foreach (string row in rowDefs) 
            {
                rows.Add(KeepSpecificRow.GetInstance(row));
            }
            return GetInstance(rows);
        }
 
        public static KeepSpecificRows GetInstance(List<KeepSpecificRow> rows) 
        {
            KeepSpecificRows aKeepTest = new KeepSpecificRows(); 
            aKeepTest._testRows = rows;
            return aKeepTest;
        }


        public override bool Test(Dictionary<string, string> row) 
        { 
            foreach (KeepSpecificRow keepRowTest in _testRows)
            { 
                if (keepRowTest.Test(row))
                    return true;
            }
            return false;
        }
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        {
            return false; 
        }
    }
    public class KeepNonTrivialRows : KeepTest<Dictionary<string, string>>
    {
        public const string Prefix = "KeepNonTrivial";
 
        public override bool Test(Dictionary<string, string> row) 
        {
            if (int.Parse(row[Tabulate.PredictorFalseNameCountColumnName]) == 0 || 
                int.Parse(row[Tabulate.PredictorTrueNameCountColumnName]) == 0)
            {
                return false;
            }
            if (row.ContainsKey(Tabulate.TargetFalseNameCountColumnName) &&
                (int.Parse(row[Tabulate.TargetFalseNameCountColumnName]) == 0 || 
                int.Parse(row[Tabulate.TargetTrueNameCountColumnName]) == 0)) 
            {
                return false; 
            }
            return true;
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        { 
            throw new Exception("The method or operation is not implemented."); 
        }
    } 
    public class KeepTestTemp : KeepTest<Dictionary<string, string>>
    {
        public const string Prefix = "KeepTestTemp";

        private string[] _badList;
 
        public static KeepTestTemp GetInstance() 
        {
            KeepTestTemp keepTest = new KeepTestTemp(); 
            keepTest._badList = new string[]{
            "hla1",
            "hla93",
            "hla85",
            "hla41",
            "hla7", 
            "hla17", 
            "hla2",
            "hla58", 
            "hla57",
            "hla94",
            "hla47",
            "hla83",
            "hla31",
            "hla43", 
            "hla37"}; 

            Array.Sort(keepTest._badList); 
            return keepTest;
        }

        public override bool Test(Dictionary<string, string> row)
        {
            string hla = row[Tabulate.PredictorVariableColumnName]; 
            return Array.IndexOf(_badList, hla) < 0; // only keep if not in the bad list. 
        }
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
 
    public class KeepNonRare : KeepTest<Dictionary<string, string>> 
    {
        public const string Prefix = "KeepNonRare"; 

        private int _minTrueCount;

        public static KeepNonRare GetInstance(string minCount)
        {
            KeepNonRare keepTest = new KeepNonRare(); 
            keepTest._minTrueCount = int.Parse(minCount); 
            return keepTest;
        } 

        public override bool Test(Dictionary<string, string> row)
        {
            if (row.ContainsKey("TT"))
            {
                int[] fisherCounts = new int[4]; 
                fisherCounts[0] = int.Parse(row["TT"]); 
                fisherCounts[1] = int.Parse(row["TF"]);
                fisherCounts[2] = int.Parse(row["FT"]); 
                fisherCounts[3] = int.Parse(row["FF"]);

                //test that each variable's false and true counts are at least _minTrueCount. Use
                // fisher count to account for missing data in the other variable.
                if (fisherCounts[0] + fisherCounts[1] < _minTrueCount) return false; // is first true enough?
                if (fisherCounts[0] + fisherCounts[2] < _minTrueCount) return false; // is second true enough? 
                if (fisherCounts[1] + fisherCounts[3] < _minTrueCount) return false; // is second false enough? 
                if (fisherCounts[2] + fisherCounts[3] < _minTrueCount) return false; // is first false enough?
                return true; 
            }
            else
            {
                bool keepPred = true;
                bool keepTarg = true;
                bool keepGlobal = !row.ContainsKey(Tabulate.GlobalNonMissingCountColumnName) ? true : int.Parse(row[Tabulate.GlobalNonMissingCountColumnName]) >= _minTrueCount; 
                if (row.ContainsKey(Tabulate.PredictorTrueNameCountColumnName)) 
                {
                    keepPred = 
                        int.Parse(row[Tabulate.PredictorFalseNameCountColumnName]) >= _minTrueCount &&
                        int.Parse(row[Tabulate.PredictorTrueNameCountColumnName]) >= _minTrueCount;
                }
                if (row.ContainsKey(Tabulate.TargetTrueNameCountColumnName))
                {
                    keepTarg = 
                        int.Parse(row[Tabulate.TargetFalseNameCountColumnName]) >= _minTrueCount && 
                        int.Parse(row[Tabulate.TargetTrueNameCountColumnName]) >= _minTrueCount;
                } 

                return keepGlobal && keepPred && keepTarg;
            }
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        { 
            throw new Exception("The method or operation is not implemented.");
        } 
    }

    public class KeepAllBut : KeepTest<Dictionary<string, string>>
    {
        public const string Prefix = "KeepAllBut";
 
        private KeepOnly _keepOnly; 

        public static KeepAllBut GetInstance(string commaDelimitedRejectList) 
        {
            KeepAllBut keepTest = new KeepAllBut();
            keepTest._keepOnly = KeepOnly.GetInstance(commaDelimitedRejectList);
            return keepTest;
        }
 
        public override bool Test(Dictionary<string, string> row) 
        {
            return !_keepOnly.Test(row); 
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            throw new Exception("The method or operation is not implemented.");
        } 
    } 

    public class KeepOnly : KeepTest<Dictionary<string, string>> 
    {
        public const string Prefix = "KeepOnly";

        private List<string> _keepList = new List<string>();

        public static KeepOnly GetInstance(string commaDelimiteKeepList) 
        { 
            KeepOnly keepTest = new KeepOnly();
            keepTest._keepList = new List<string>(commaDelimiteKeepList.ToLower().Split(',')); 
            return keepTest;
        }

        public override bool Test(Dictionary<string, string> row)
        {
            string predictorVariable = row[Tabulate.PredictorVariableColumnName].ToLower(); 
            string targetVariable = row[Tabulate.TargetVariableColumnName].ToLower(); 

            return (_keepList.Contains(predictorVariable) || _keepList.Contains(targetVariable)); 
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            throw new Exception("The method or operation is not implemented.");
        } 
    } 

    //public class KeepPredictorTargetPairs : KeepTest<Dictionary<string, string>> 
    //{
    //    public const string Prefix = "KeepPredictorTargetPairs";

    //    private Dictionary<string, string> _predictorToTarget = new Dictionary<string, string>();

    //    public static KeepPredictorTargetPairs GetInstance(string predSparseCommaTargSparseOrEmpty) 
    //    { 
    //        KeepPredictorTargetPairs keepTest = new KeepPredictorTargetPairs();
 
    //        if (predSparseCommaTargSparseOrEmpty == "")
    //        {
    //            keepTest._predictorToTarget = null;
    //        }
    //        else
    //        { 
    //            string[] predSparseAndTargSparse = predSparseCommaTargSparseOrEmpty.Split(','); 
    //            Dictionary<string, Dictionary<string, SufficientStatistics>> predSparseAsDict = ModelTester.LoadSparseFileInMemory(predSparseAndTargSparse[0]);
    //            Dictionary<string, Dictionary<string, SufficientStatistics>> targSparseAsDict = ModelTester.LoadSparseFileInMemory(predSparseAndTargSparse[1]); 

    //            Dictionary<string, string> predictorToTarget = new Dictionary<string, string>();
    //            foreach (KeyValuePair<string, string> predAndTarget in SpecialFunctions.EnumerateTwo(predSparseAsDict.Keys, targSparseAsDict.Keys))
    //            {
    //                predictorToTarget.Add(predAndTarget.Key, predAndTarget.Value);
    //            } 
    //            keepTest._predictorToTarget = predictorToTarget; 
    //        }
 
    //        return keepTest;
    //    }
    //    public override bool Test(Dictionary<string, string> row)
    //    {
    //        if (_predictorToTarget == null)
    //        { 
    //            return true; 
    //        }
 
    //        string predictorVariable = row[Tabulate.PredictorVariableColumnName];
    //        string targetVariable = row[Tabulate.TargetVariableColumnName];

    //        return _predictorToTarget[predictorVariable] == targetVariable;
    //    }
 
    //    public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
    //    {
    //        throw new Exception("The method or operation is not implemented."); 
    //    }
    //}

    public class KeepPredictorTargetPairs : KeepTest<Dictionary<string, string>>
    {
        public const string Prefix = "KeepPredictorTargetPairs"; 
 
        private Dictionary<string, string> _predictorToTarget = new Dictionary<string, string>();
 
        public static KeepPredictorTargetPairs GetInstance(string predSparseCommaTargSparseOrEmpty)
        {
            KeepPredictorTargetPairs keepTest = new KeepPredictorTargetPairs();

            return keepTest;
        } 
        public override bool Test(Dictionary<string, string> row) 
        {
            // this class serves as a flag for the WorkList. It always returns true, it just signals to the WorkList that 
            // enumeration should be done differently than it otherwise would be.
            return true;
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        { 
            throw new Exception("The method or operation is not implemented."); 
        }
    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
