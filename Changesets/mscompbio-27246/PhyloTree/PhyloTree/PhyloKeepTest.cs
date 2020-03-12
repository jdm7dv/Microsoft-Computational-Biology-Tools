using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree 
{ 
    public abstract class PhyloKeepTest<TRow> :  KeepTest<TRow>
    { 
        new public static KeepTest<Dictionary<string, string>> GetInstance(string inputDirectory, string keepTestName)
        {
            return GetInstance(inputDirectory, null, null, keepTestName, 1, null);
        }
        new public static KeepTest<Dictionary<string, string>> GetInstance(string inputDirectory,
            string binarySeqFileName, string keepTestName, int merSize, Dictionary<int, string> pidToCaseName) 
        { 
            return GetInstance(inputDirectory, binarySeqFileName, null, keepTestName, merSize, pidToCaseName);
        } 

        new public static KeepTest<Dictionary<string, string>> GetInstance(string inputDirectory,
            string binarySeqFileName, string hlaFileName, string keepTestName, int merSize, Dictionary<int, string> pidToCaseName)
        {
            //!!!would be nice of classes could parse themselves
            if (keepTestName.StartsWith(KeepEndOfGag.Prefix)) 
            { 
                bool keepIt = bool.Parse(keepTestName.Substring(KeepEndOfGag.Prefix.Length));
                return KeepEndOfGag.GetInstance(keepIt); 
            }
            else if (keepTestName.StartsWith(K1.Prefix))
            {
                int k1 = int.Parse(keepTestName.Substring(K1.Prefix.Length));
                return K1.GetInstance(k1);
            } 
            //else if (keepTestName.StartsWith(K2.Prefix)) 
            //{
            //    int k2 = int.Parse(keepTestName.Substring(K2.Prefix.Length)); 
            //    return K2.GetInstance(k2);
            //}
            else if (keepTestName.StartsWith(KeepNonOverlappingAA.Prefix))
            {
                return KeepNonOverlappingAA.GetInstance();
            } 
            else if (keepTestName.StartsWith(KeepGene.Prefix)) 
            {
                string geneRange = keepTestName.Substring(KeepGene.Prefix.Length); 
                return KeepGene.GetInstance(geneRange);
            }
            else if (keepTestName.StartsWith(KeepSpecificRows.Prefix))
            {
                return KeepSpecificRows.GetInstance(keepTestName.Substring(KeepSpecificRows.Prefix.Length));
            } 
            else if (keepTestName.StartsWith(KeepSpecificRow.Prefix)) 
            {
                return KeepSpecificRow.GetInstance(keepTestName.Substring(KeepSpecificRow.Prefix.Length)); 
            }
            else if (keepTestName.StartsWith(KeepSpecificGenes.Prefix))
            {
                return KeepSpecificGenes.GetInstance(keepTestName.Substring(KeepSpecificGenes.Prefix.Length));
            }
            else if (keepTestName.StartsWith(KeepOneOfAAPair.Prefix)) 
            { 
                return KeepOneOfAAPair.GetInstance();
            } 
            else if (keepTestName.StartsWith(KeepAllButSamePosition.Prefix))
            {
                return KeepAllButSamePosition.GetInstance();
            }
            else if (keepTestName.StartsWith(KeepAllButSameDeletion.Prefix))
            { 
                return KeepAllButSameDeletion.GetInstance(); 
            }
            else if (keepTestName.StartsWith(KeepNonTrivialRows.Prefix)) 
            {
                return new KeepNonTrivialRows();
            }
            else if (keepTestName.StartsWith(KeepTestTemp.Prefix))
            {
                return KeepTestTemp.GetInstance(); 
            } 
            //else if (keepTestName.StartsWith(KeepPollockOneDirection.Prefix))
            //{ 
            //    return KeepPollockOneDirection.GetInstance(keepTestName.Substring(KeepPollockOneDirection.Prefix.Length));
            //}
            //else if (keepTestName.StartsWith(KeepFisherOneDirection.Prefix))
            //{
            //    return KeepFisherOneDirection.GetInstance(keepTestName.Substring(KeepFisherOneDirection.Prefix.Length));
            //} 
            else if (keepTestName.StartsWith(KeepNonRare.Prefix)) 
            {
                return KeepNonRare.GetInstance(keepTestName.Substring(KeepNonRare.Prefix.Length)); 
            }
            else if (keepTestName.StartsWith(KeepPredictorTargetPairs.Prefix))
            {
                return KeepPredictorTargetPairs.GetInstance(keepTestName.Substring(KeepPredictorTargetPairs.Prefix.Length));
            }
            else if (keepTestName.StartsWith("JointGagPolTest")) 
            { 
                return And<Dictionary<string, string>>.GetInstance(
                    //KeepRandom<Dictionary<string,string>>.GetInstance(0, 0.001), // how do we make it the same when we count and when we really run through it? 
                    KeepOneOfAAPair.GetInstance(),
                    KeepNonOverlappingAA.GetInstance(),
                    KeepSpecificGenes.GetInstance(keepTestName.Substring("JointGagPolTest".Length)));
            }
            else
            { 
                return KeepTest<TRow>.GetInstance(inputDirectory, binarySeqFileName, null, keepTestName, merSize, pidToCaseName); 
            }
        } 
    }

    //// In the null models, we don't want the target and predictor to be the same positions or we'll skew our results.
    //public class KeepAllButSameOnNull : KeepTest
    //{
    //    private KeepAllButSameOnNull() 
    //    { 
    //    }
 
    //    bool KeepIt;
    //    internal static KeepTest<Dictionary<string, string>>GetInstance(bool keepIt)
    //    {
    //        if (keepIt)
    //        {
    //            return AlwaysKeep.GetInstance(); 
    //        } 

    //        KeepAllButSameOnNull aKeepAllButSameOnNull = new KeepAllButSameOnNull(); 
    //        aKeepAllButSameOnNull.KeepIt = keepIt;
    //        return aKeepAllButSameOnNull;
    //    }

    //    internal static string Prefix = "KeepAllButSameOnNull";
 
    //    public override string ToString() 
    //    {
    //        return string.Format("KeepAllButSameOnNull{0}", KeepIt); 
    //    }

    //    public override bool Test(Dictionary<string, string> row)
    //    {
    //        Debug.Assert(!KeepIt); // real assert
    //        int nullIndex = int.Parse(row[Tabulate.NullIndexColumnName]); 
    //        int posTarget = int.Parse(row[PhyloTree.Nuc1TargetPositionColumnName]); 
    //        int posPredictor = int.Parse(row[PhyloTree.Nuc2PredictorPositionColumnName]);
 
    //        return nullIndex == -1 || posTarget != posPredictor;
    //    }

    //    public override bool IsCompatibleWithNewKeepTest(KeepTest keepTestNew)
    //    {
    //        return false; //!!!could be made tighter 
    //    } 
    //}
 
    public class KeepEndOfGag : PhyloKeepTest<Dictionary<string, string>>
    {
        private KeepEndOfGag()
        {
        }
 
        bool KeepIt; 
        internal static KeepTest<Dictionary<string, string>> GetInstance(bool keepIt)
        { 
            if (keepIt)
            {
                return AlwaysKeep<Dictionary<string, string>>.GetInstance();
            }

            KeepEndOfGag aKeepEndOfGag = new KeepEndOfGag(); 
            aKeepEndOfGag.KeepIt = keepIt; 
            return aKeepEndOfGag;
        } 

        internal static string Prefix = "KeepEndOfGag";

        public override string ToString()
        {
            return string.Format("KeepEndOfGag{0}", KeepIt); 
        } 

        public override bool Test(Dictionary<string, string> row) 
        {
            Debug.Assert(!KeepIt); // real assert

            int nuc1Position = int.Parse(row[PhyloTree.Nuc1TargetPositionColumnName]);
            SpecialFunctions.CheckCondition((nuc1Position % 3) != 2, "nuc1Position is in neither the Gag nor the Pol frame");
            bool gagFrame = (nuc1Position % 3) == 1; 
            if (gagFrame) 
            {
                return nuc1Position < 2085; 
            }
            else
            {
                return true;
            }
 
        } 

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        {
            return false; //!!!could be made tighter
        }
    }

    public class K1 : PhyloKeepTest<Dictionary<string, string>> 
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
            int trueCount = int.Parse(row[PhyloTree.PredictorTrueNameCountColumnName]); 
            int falseCount = int.Parse(row[PhyloTree.PredictorFalseNameCountColumnName]);
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

    public class KeepNonOverlappingAA : PhyloKeepTest<Dictionary<string, string>>
    {
        internal static readonly String Prefix = "KeepNonOverlappingAA";

        private KeepNonOverlappingAA() { } 
 
        public static KeepNonOverlappingAA GetInstance()
        { 
            return new KeepNonOverlappingAA();
        }

        public override bool Test(Dictionary<string, string> row)
        {
            string predVar = row[PhyloTree.PredictorVariableColumnName]; 
            int predPos = int.Parse(predVar.Split('@')[1]); 
            string targVar = row[PhyloTree.TargetVariableColumnName];
            int targPos = int.Parse(targVar.Split('@')[1]); 

            return Math.Abs(targPos - predPos) > 2;
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        { 
            // haven't done anything here. 
            return false;
        } 


    }

    public class KeepGene : PhyloKeepTest<Dictionary<string, string>>
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
            int targetPos = int.Parse(row[PhyloTree.Nuc1TargetPositionColumnName]);
            int predPos = int.Parse(row[PhyloTree.Nuc1PredictorPositionColumnName]);

 
            return 
                targetPos >= _start && targetPos <= _stop && (targetPos - _start) % 3 == 0 &&
                predPos >= _start && predPos <= _stop && (predPos - _start) % 3 == 0; 
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            // haven't done anything here.
            return false; 
        } 

        public override string ToString() 
        {
            return string.Format("Keep{0}-{1}", _start, _stop);
        }


    } 
 
    public class KeepSpecificGenes : Or<Dictionary<string, string>>
    { 
        public const string Prefix = "KeepSpecificGenes";

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
    public class KeepOneOfAAPair : PhyloKeepTest<Dictionary<string, string>>
    { 
        public static readonly string Prefix = "KeepOneOfAAPair"; 

        private KeepOneOfAAPair() { } 

        public static PhyloKeepTest<Dictionary<string, string>> GetInstance()
        {
            return new KeepOneOfAAPair();
        }
 
        public override bool Test(Dictionary<string, string> row) 
        {
            string predictor = row[PhyloTree.PredictorVariableColumnName]; 
            string target = row[PhyloTree.TargetVariableColumnName];
            int pos1 = int.Parse(predictor.Split('@')[1]);
            int pos2 = int.Parse(target.Split('@')[1]);

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
    public class KeepAllButSamePosition : PhyloKeepTest<Dictionary<string, string>> 
    {
        public static readonly string Prefix = "KeepAllButSamePosition";

        private KeepAllButSamePosition() { }

        public static PhyloKeepTest<Dictionary<string, string>> GetInstance() 
        { 
            return new KeepAllButSamePosition();
        } 

        public override bool Test(Dictionary<string, string> row)
        {
            string predictor = row[PhyloTree.PredictorVariableColumnName];
            string target = row[PhyloTree.TargetVariableColumnName];
            int pos1 = int.Parse(predictor.Split('@')[1]); 
            int pos2 = int.Parse(target.Split('@')[1]); 

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

    public class KeepAllButSameDeletion : PhyloKeepTest<Dictionary<string, string>>
    { 
        public static readonly string Prefix = "KeepAllButSameDeletion"; 

        private KeepAllButSameDeletion() { } 

        public static PhyloKeepTest<Dictionary<string, string>> GetInstance()
        {
            return new KeepAllButSameDeletion();
        }
 
        public override bool Test(Dictionary<string, string> row) 
        {
            string predictor = row[PhyloTree.PredictorVariableColumnName]; 
            string target = row[PhyloTree.TargetVariableColumnName];
            string[] predParts = predictor.Split('@');
            string[] targParts = target.Split('@');

            string predAA = predParts[0];
            string targAA = targParts[0]; 
            int pos1 = int.Parse(predParts[1]); 
            int pos2 = int.Parse(targParts[1]);
 
            //return pos1 >= pos2;
            return predAA != targAA ||
                predAA != "-" ||
                targAA != "-" ||
                Math.Abs(pos1 - pos2) > 5;    // reject anything that's part of the same deletion. 5 is totally arbitrary.
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
    public class KeepSpecificRow : PhyloKeepTest<Dictionary<string, string>>
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
            return row[PhyloTree.PredictorVariableColumnName] == _predictor &&
                row[PhyloTree.TargetVariableColumnName] == _target; 
        }

        //public static KeepSpecificRow GetInstance(int predPos, string predMer, int targPos, string targMer)
        //{
        //    KeepSpecificRow aKeepTest = new KeepSpecificRow();
        //    aKeepTest._testRow = new Dictionary<string, string>(); 
 
        //    aKeepTest._testRow.Add(PhyloTree.Nuc1TargetPositionColumnName, targPos.ToString());
        //    aKeepTest._testRow.Add(PhyloTree.Nuc2PredictorPositionColumnName, predPos.ToString()); 
        //    aKeepTest._testRow.Add(PhyloTree.MerTargetColumnName, targMer.ToString());
        //    aKeepTest._testRow.Add(PhyloTree.MerPredictorColumnName, predMer.ToString());

        //    return aKeepTest;
        //}
 
        //public override bool Test(Dictionary<string, string> row) 
        //{
        //    return 
        //        row[PhyloTree.Nuc2PredictorPositionColumnName] == _testRow[PhyloTree.Nuc2PredictorPositionColumnName] &&
        //        row[PhyloTree.MerPredictorColumnName] == _testRow[PhyloTree.MerPredictorColumnName] &&
        //        row[PhyloTree.Nuc1TargetPositionColumnName] == _testRow[PhyloTree.Nuc1TargetPositionColumnName] &&
        //        row[PhyloTree.MerTargetColumnName] == _testRow[PhyloTree.MerTargetColumnName];
        //}
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        {
            return false; 
        }
    }
    public class KeepSpecificRows : PhyloKeepTest<Dictionary<string, string>>
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
    public class KeepNonTrivialRows : PhyloKeepTest<Dictionary<string, string>>
    { 
        public const string Prefix = "KeepNonTrivial";

        public override bool Test(Dictionary<string, string> row)
        {
            if (int.Parse(row[PhyloTree.PredictorFalseNameCountColumnName]) == 0 ||
                int.Parse(row[PhyloTree.PredictorTrueNameCountColumnName]) == 0) 
            { 
                return false;
            } 
            if (row.ContainsKey(PhyloTree.TargetFalseNameCountColumnName) &&
                (int.Parse(row[PhyloTree.TargetFalseNameCountColumnName]) == 0 ||
                int.Parse(row[PhyloTree.TargetTrueNameCountColumnName]) == 0))
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
    public class KeepTestTemp : PhyloKeepTest<Dictionary<string, string>> 
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
            string hla = row[PhyloTree.PredictorVariableColumnName];
            return Array.IndexOf(_badList, hla) < 0; // only keep if not in the bad list.
        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        { 
            throw new Exception("The method or operation is not implemented.");
        } 
    }

    public class KeepNonRare : PhyloKeepTest<Dictionary<string, string>>
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
            bool keepPred = true; 
            bool keepTarg = true;
            if (row.ContainsKey(PhyloTree.PredictorTrueNameCountColumnName))
            {
                keepPred = int.Parse(row[PhyloTree.PredictorTrueNameCountColumnName]) >= _minTrueCount;
            }
            if (row.ContainsKey(PhyloTree.TargetTrueNameCountColumnName)) 
            { 
                keepTarg = int.Parse(row[PhyloTree.TargetTrueNameCountColumnName]) >= _minTrueCount;
            } 

            return keepPred && keepTarg;

        }

        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew) 
        { 
            throw new Exception("The method or operation is not implemented.");
        } 
    }

    public class KeepPredictorTargetPairs : PhyloKeepTest<Dictionary<string, string>>
    {
        public const string Prefix = "KeepPredictorTargetPairs";
 
        private Dictionary<string, string> _predictorToTarget = new Dictionary<string, string>(); 

        public static KeepPredictorTargetPairs GetInstance(string predSparseCommaTargSparseOrEmpty) 
        {
            KeepPredictorTargetPairs keepTest = new KeepPredictorTargetPairs();

            if (predSparseCommaTargSparseOrEmpty == "")
            {
                keepTest._predictorToTarget = null; 
            } 
            else
            { 
                string[] predSparseAndTargSparse = predSparseCommaTargSparseOrEmpty.Split(',');
                Dictionary<string, Dictionary<string, SufficientStatistics>> predSparseAsDict = ModelTester.LoadSparseFileInMemory(predSparseAndTargSparse[0]);
                Dictionary<string, Dictionary<string, SufficientStatistics>> targSparseAsDict = ModelTester.LoadSparseFileInMemory(predSparseAndTargSparse[1]);

                Dictionary<string, string> predictorToTarget = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> predAndTarget in SpecialFunctions.EnumerateTwo(predSparseAsDict.Keys, targSparseAsDict.Keys)) 
                { 
                    predictorToTarget.Add(predAndTarget.Key, predAndTarget.Value);
                } 
                keepTest._predictorToTarget = predictorToTarget;
            }

            return keepTest;
        }
        public override bool Test(Dictionary<string, string> row) 
        { 
            if (_predictorToTarget == null)
            { 
                return true;
            }

            string predictorVariable = row[PhyloTree.PredictorVariableColumnName];
            string targetVariable = row[PhyloTree.TargetVariableColumnName];
 
            return _predictorToTarget[predictorVariable] == targetVariable; 
        }
 
        public override bool IsCompatibleWithNewKeepTest(KeepTest<Dictionary<string, string>> keepTestNew)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
