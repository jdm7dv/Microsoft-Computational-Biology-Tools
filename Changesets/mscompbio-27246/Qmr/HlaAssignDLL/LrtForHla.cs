using System; 
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using System.IO; 
using Msr.Mlas.Qmr; 
using VirusCount.Qmr; //!!! what's the difference between Msr.Mlas.Qmr; and this?
using Optimization; 
using EpipredLib;

namespace VirusCount.Qmrr
{
    abstract public class LrtForHla
    { 
        internal LrtForHla(double? leakProbabilityOrNull) 
        {
            LeakProbabilityOrNull = leakProbabilityOrNull; 
        }

        double? LeakProbabilityOrNull;
        protected HlaFactory HlaFactory;
        protected Dictionary<string, Set<Hla>> PidToHlaSetReal;
        protected Set<Hla> HlaUniverse; 
        protected Dictionary<string, Dictionary<string, double>> ReactTableUnfiltered; 
        protected Dictionary<string, Set<Hla>> _knownTable;
        public Set<Hla> KnownTable(string peptide) 
        {
            if (_knownTable.ContainsKey(peptide))
            {
                return _knownTable[peptide];
            }
            else 
            { 
                return Set<Hla>.GetInstance();
            } 
        }
        protected string CaseName;

        static public LrtForHla GetInstance(string selectionName, string inputDirectory, string caseName, string hlaFactoryName, double? leakProbabilityOrNull, double pValue)
        {
            string patientFileName = string.Format(@"{0}\{1}patient.txt", inputDirectory, caseName);//!!!const 
            string patientFileNameReact = string.Format(@"{0}\{1}react.txt", inputDirectory, caseName);//!!!const 
            string patientFileNameKnown = string.Format(@"{0}\{1}known.txt", inputDirectory, caseName);//!!!const
 
            using (DbDataReader datareaderHLA = new StreamDataReader(patientFileName))
            using (DbDataReader datareaderReact = new StreamDataReader(patientFileNameReact))
            using (DbDataReader datareaderKnown = new StreamDataReader(patientFileNameKnown))
            {
                return GetInstance(selectionName, datareaderHLA, datareaderReact, datareaderKnown, caseName, hlaFactoryName, leakProbabilityOrNull, pValue);
            } 
        } 

        private static DbDataReader CreateDataReader(string patientFileName) 
        {
            throw new Exception("The method or operation is not implemented.");
        }

        static public LrtForHla GetInstance(string selectionName, DbDataReader datareaderHLA, DbDataReader datareaderReact, DbDataReader datareaderKnown, string caseName, string hlaFactoryName, double? leakProbabilityOrNull, double pValue)
        { 
            LrtForHla aLrtForHla; 
            //if (selectionName == "BackwardSelection")
            //{ 
            //    aLrtForHla = new BackwardSelection();
            //} else
            if (selectionName == "ForwardSelection")
            {
                aLrtForHla = new ForwardSelection(leakProbabilityOrNull, pValue);
            } 
            //else if (selectionName == "AddToKnown") 
            //{
            //    aLrtForHla = new AddToKnown(); 
            //}
            else
            {
                Debug.Fail("unknown selection name");
                aLrtForHla = null;
            } 
 
            aLrtForHla.HlaFactory = HlaFactory.GetFactory(hlaFactoryName);
            aLrtForHla.PidToHlaSetReal = aLrtForHla.LoadCidToHlaSet(datareaderHLA); 
            aLrtForHla.HlaUniverse = CreateHlaUniverse(aLrtForHla.PidToHlaSetReal);

            Set<string> pidsInReactTable;
            aLrtForHla.ReactTableUnfiltered = LoadReactTableUnfiltered(datareaderReact, out pidsInReactTable);

            aLrtForHla._knownTable = aLrtForHla.LoadKnownTable(datareaderKnown, caseName); 
            aLrtForHla.CaseName = caseName; 

            Set<string> patientsInHlaFile = Set<string>.GetInstance(aLrtForHla.PidToHlaSetReal.Keys); 
            ReportOnExtraPatients(patientsInHlaFile, "HLA", pidsInReactTable, "React");
            ReportOnExtraPatients(pidsInReactTable, "React", patientsInHlaFile, "HLA");


            return aLrtForHla;
        } 
 
        internal abstract string SelectionName
        { 
            get;
        }

        public void Run(RangeCollection pieceIndexRange, int pieceCount, RangeCollection nullIndexRange, string directory, string fileName)
        {
            string filePath = directory + "\\" + fileName + ".txt"; 
            string doneFilePath = directory + "\\Done." + fileName + ".txt"; 

            using (StreamWriter streamWriter = File.CreateText(filePath)) 
            {
                Run(pieceIndexRange, pieceCount, nullIndexRange, streamWriter);
            }
            using (StreamWriter streamWriter = File.CreateText(doneFilePath))
            {
                streamWriter.WriteLine(DateTime.Now.ToString()); 
            } 
        }
 
        public void Run(RangeCollection pieceIndexRange, int pieceCount, RangeCollection nullIndexRange, TextWriter writer)
        {
            writer.WriteLine(PValueDetails.Header);

            foreach (int nullIndex in nullIndexRange.Elements)
            { 
                Dictionary<string, Set<Hla>> pidToHlaSet = PidToHlaSetForThisNullIndex(nullIndex); 

                CreatePeptideToHlaToPValueDetails(pieceIndexRange, pieceCount, nullIndex, pidToHlaSet, writer); 
            }
        }

        private Dictionary<string, Set<Hla>> LoadCidToHlaSet(DbDataReader datareader)
        {
            Dictionary<string, Set<Hla>> cidToHlaSet = new Dictionary<string, Set<Hla>>(); 
 
            Set<Hla> hlaSet;
            int fieldCount = datareader.FieldCount; 
            int indexHLA = -1;

            for (int field = 0; field < fieldCount; ++field)
            {
                string fieldName = datareader.GetName(field).ToLower();
 
                if (fieldName == "hla") 
                {
                    indexHLA = field; 
                    break;
                }

                if (fieldName == "var")
                {
                    indexHLA = field; 
                    //	don't break out of loop, so prefer column "HLA" over "VAR" 
                }
            } 

            int indexCID = datareader.GetOrdinal("cid");
            int indexPresent = datareader.GetOrdinal("present");
            int irecord = 0;

            while (datareader.Read()) 
            { 
                ++irecord;
 
                string cid = datareader.GetString(indexCID).Trim();

                if (cid.Length == 0)
                    continue;

                if (!cidToHlaSet.TryGetValue(cid, out hlaSet)) 
                { 
                    hlaSet = Set<Hla>.GetInstance();
                    cidToHlaSet.Add(cid, hlaSet); 
                }

                Hla hla = HlaFactory.GetGroundOrAbstractInstance(datareader.GetString(indexHLA).Trim());

                hlaSet.AddNewOrOld(hla);
 
                if (datareader.GetDouble(indexPresent) != 1.0) 
                {
                    SpecialFunctions.CheckCondition(false, string.Format("{0} record {1}: field 'present' is not unity", "HLA table", irecord)); 
                }
            }

            Console.WriteLine("{0}: number of records read: {1}", "HLA table", irecord);

            return cidToHlaSet; 
        } 

        //private Dictionary<string, Set<Hla>> LoadCidToHlaSet(TextReader patientReader) 
        //{
        //    Dictionary<string, Set<Hla>> cidToHlaSet = new Dictionary<string, Set<Hla>>();
        //    string header = "hla	cid	present";
        //    Set<Hla> hlaSet;
        //    int	irow = 0;
 
        //    foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(patientReader, header, false)) 
        //    {
        //        ++irow; 

        //        string cid = row["cid"];

        //        if (!cidToHlaSet.TryGetValue(cid, out hlaSet))
        //        {
        //            hlaSet = Set<Hla>.GetInstance(); 
        //            cidToHlaSet.Add(cid, hlaSet); 
        //        }
 
        //        Hla hla = HlaFactory.GetGroundOrAbstractInstance(row["hla"]);

        //        SpecialFunctions.CheckCondition(row["present"] == "1", string.Format("{0} record {1}: field present is not unity", "HLA file", irow));

        //        hlaSet.AddNewOrOld(hla);
        //    } 
 
        //    return cidToHlaSet;
        //} 

        //private Dictionary<string, Set<Hla>> LoadPidToHlaSet(TextReader patientReader)
        //{
        //    Dictionary<string, Set<Hla>> pidToHlaSet = new Dictionary<string, Set<Hla>>();
        //    string header = "pid	a1	a2	b1	b2	c1	c2";
 
        //    foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(patientReader, header, false)) 
        //    {
        //        string pid = row["pid"]; 
        //        Set<Hla> hlaSet = Set<Hla>.GetInstance();
        //        foreach (string column in "a1	a2	b1	b2	c1	c2".Split('\t'))
        //        {
        //            string hlaName = row[column];
        //            Hla hla = HlaFactory.GetGroundOrAbstractInstance(hlaName);
 
        //            hlaSet.AddNewOrOld(hla); 
        //        }
        //        pidToHlaSet.Add(pid, hlaSet); 
        //    }
        //    return pidToHlaSet;
        //}

        private static Set<Hla> CreateHlaUniverse(Dictionary<string, Set<Hla>> pidToHlaSet)
        { 
            Set<Hla> hlaUniverse = Set<Hla>.GetInstance(); 
            foreach (Set<Hla> hlaSet in pidToHlaSet.Values)
            { 
                hlaUniverse.AddNewOrOldRange(hlaSet);
            }
            return hlaUniverse;
        }

        //!!!same logic is elseware. Look for common heading 
        private static Dictionary<string, Dictionary<string, double>> LoadReactTableUnfiltered(DbDataReader datareader, out Set<string> cidsInReactTable) 
        {
            cidsInReactTable = Set<string>.GetInstance(); 

            Dictionary<string, Dictionary<string, double>> reactTable = new Dictionary<string, Dictionary<string, double>>();

            int indexPeptide = datareader.GetOrdinal("peptide");
            int indexCID = datareader.GetOrdinal("cid");
            int indexMagnitude = datareader.GetOrdinal("magnitude"); 
 
            int irecord = 0;
 
            while (datareader.Read())
            {
                ++irecord;

                string cid = datareader.GetString(indexCID).Trim();
 
                if (cid.Length == 0) 
                    continue;
 
                cidsInReactTable.AddNewOrOld(cid);
                string peptide = datareader.GetString(indexPeptide).Trim();
                double amount = datareader.GetDouble(indexMagnitude);

                Dictionary<string, double> peptideToAmount = SpecialFunctions.GetValueOrDefault(reactTable, peptide);
                peptideToAmount.Add(cid, amount); 
            } 

            Console.WriteLine("{0}: number of records read: {1}", "React table", irecord); 

            return reactTable;
        }

        ////!!!same logic is elseware. Look for common heading
        //private static Dictionary<string, Dictionary<string, double>> LoadReactTableUnfiltered(TextReader patientReader, out Set<string> cidsInReactTable) 
        //{ 
        //    cidsInReactTable = Set<string>.GetInstance();
 
        //    Dictionary<string, Dictionary<string, double>> reactTable = new Dictionary<string, Dictionary<string, double>>();
        //    string header = "peptide	cid	magnitude";
        //    foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(patientReader, header, false))
        //    {
        //        string cid = row["cid"];
        //        cidsInReactTable.AddNewOrOld(cid); 
        //        string peptide = row["peptide"]; 
        //        double amount = double.Parse(row["magnitude"]);
 
        //        Dictionary<string, double> peptideToAmount = SpecialFunctions.GetValueOrDefault(reactTable, peptide);
        //        peptideToAmount.Add(cid, amount);
        //    }
        //    return reactTable;
        //}
 
        private Dictionary<string, Set<Hla>> LoadKnownTable(DbDataReader datareader, string caseName) 
        {
            //!!!code appears elsewhere. Look for common header 
            Dictionary<string, Set<Hla>> knownTable = new Dictionary<string, Set<Hla>>();

            int indexPeptide = datareader.GetOrdinal("peptide");
            int indexHLA = datareader.GetOrdinal("knownHLA");

            int irecord = 0; 
 
            while (datareader.Read())
            { 
                ++irecord;

                string peptide = datareader.GetString(indexPeptide).Trim();
                string hlaName = datareader.GetString(indexHLA).Trim();

                if (peptide.Length == 0 || hlaName.Length == 0) 
                    continue; 

                Set<Hla> knownHlaSet = SpecialFunctions.GetValueOrDefault(knownTable, peptide); 
                Hla hla = HlaFactory.GetGroundInstance(hlaName);
                SpecialFunctions.CheckCondition(!knownHlaSet.Contains(hla), string.Format("Hla {0} is duplicated in {1}known.txt, for peptide {2}", hla, caseName, peptide));
                knownHlaSet.AddNew(hla); //!!!const
            }

            Console.WriteLine("{0}: number of records read: {1}", "Known table", irecord); 
 
            return knownTable;
        } 

        //private Dictionary<string, Set<Hla>> LoadKnownTable(TextReader patientReader, string caseName)
        //{
        //    //!!!code appears elsewhere. Look for common header
        //    Dictionary<string, Set<Hla>> knownTable = new Dictionary<string, Set<Hla>>();
        //    foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(patientReader, "peptide	knownHLA", false))//!!!const 
        //    { 
        //        string peptide = row["peptide"];//!!!const
        //        Set<Hla> knownHlaSet = SpecialFunctions.GetValueOrDefault(knownTable, peptide); 
        //        string hlaName = row["knownHLA"];
        //        Hla hla = HlaFactory.GetGroundInstance(hlaName);
        //        SpecialFunctions.CheckCondition(!knownHlaSet.Contains(hla), string.Format("Hla {0} is duplicated in {1}known.txt, for peptide {2}", hla, caseName, peptide));
        //        knownHlaSet.AddNew(hla); //!!!const
        //    }
 
        //    return knownTable; 
        //}
 
        private Dictionary<string, Set<Hla>> PidToHlaSetForThisNullIndex(int nullIndex)
        {
            Dictionary<string, Set<Hla>> pidToHlaSet;
            if (nullIndex == -1)
            {
                pidToHlaSet = PidToHlaSetReal; 
            } 
            else
            { 
                Random random = new Random("FindAndReport".GetHashCode() ^ nullIndex.GetHashCode());
                pidToHlaSet = SpecialFunctions.RandomizeMapping(PidToHlaSetReal, ref random);

            }
            return pidToHlaSet;
        } 
 
        private void CreatePeptideToHlaToPValueDetails(
            RangeCollection pieceIndexRange, int pieceCount, //!!!might be faster to move these out of the null loop 
            int nullIndex,
            Dictionary<string, Set<Hla>> pidToHlaSet,
            TextWriter writer
            )
        {
            foreach (string peptide in SelectedPeptides(pieceIndexRange, pieceCount)) 
            { 
                Set<Hla> candidateHlaSet = CreateCandidateHlaSet(pidToHlaSet, peptide);
 
                Debug.Assert(candidateHlaSet.Intersection(KnownTable(peptide)).Count == 0); // real assert
                if (candidateHlaSet.Count > 0)
                {
                    CreateHlaToPValueDetails(nullIndex, peptide, pidToHlaSet, candidateHlaSet, writer);
                }
            } 
        } 

        private IEnumerable<string> SelectedPeptides(RangeCollection pieceIndexRange, int pieceCount) 
        {
            int iPeptide = -1;
            foreach (string peptide in ReactTableUnfiltered.Keys)
            {
                ++iPeptide;
                int pieceIndex = (int)(((long)iPeptide * (long)pieceCount) / (long)ReactTableUnfiltered.Count); 
                if (pieceIndexRange.Contains(pieceIndex)) 
                {
                    yield return peptide; 
                }
            }
        }

        public void Tabulate(string directory, string caseName, List<KeyValuePair<int, int>> firstNullAndLastNullList)
        { 
            List<Dictionary<string, string>> realRowCollectionToSort = new List<Dictionary<string, string>>(); 
            List<double> nullDiffCollectionToBeSorted = new List<double>();
 
            //!!!move VirusCount.PhyloTree.RangeCollection to a more general place
            RangeCollection rangeCollection = RangeCollection.GetInstance();
            int minFirstNull = int.MaxValue;
            int maxLastNull = int.MinValue;

 
            foreach (KeyValuePair<int, int> firstNullAndLastNull in firstNullAndLastNullList) 
            {
                int firstNull = firstNullAndLastNull.Key; 
                minFirstNull = Math.Min(minFirstNull, firstNull);
                int lastNull = firstNullAndLastNull.Value;
                maxLastNull = Math.Max(maxLastNull, lastNull);
                //!!!string repeated elsewere
                //!!!what "new" or not?
                string inputFileName = string.Format(@"{0}\{1}.{2}.{3}-{4}.pValues.new.txt", directory, SelectionName, CaseName, firstNull, lastNull); //!!!const 
                int maxNullSeen = int.MinValue; 

                //foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(inputFileName, true)) 
                foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(inputFileName, PValueDetails.Header, true))
                {
                    int nullIndex = int.Parse(row["NullIndex"]);
                    rangeCollection.TryAdd(nullIndex);
                    if (nullIndex == -1)
                    { 
                        realRowCollectionToSort.Add(row); 
                    }
                    else 
                    {
                        double value = AccessPValueFromRow(row);
                        nullDiffCollectionToBeSorted.Add(value);
                        maxNullSeen = Math.Max(maxNullSeen, nullIndex);
                    }
                } 
            } 
            SpecialFunctions.CheckCondition(realRowCollectionToSort.Count != 0);
            SpecialFunctions.CheckCondition(rangeCollection.IsComplete(minFirstNull, maxLastNull)); 
            int nullCount = maxLastNull - Math.Max(0, minFirstNull) + 1;


            Dictionary<Dictionary<string, string>, double>
                qValueList = SpecialFunctions.ComputeQValues(ref realRowCollectionToSort, AccessPValueFromRow, ref nullDiffCollectionToBeSorted, nullCount);
 
            string outputFile = string.Format(@"{0}\{1}.{2}.{3}.qValues.new.txt", directory, SelectionName, CaseName, nullCount); //!!!const 

            using (StreamWriter outputStream = File.CreateText(outputFile)) 
            {
                outputStream.WriteLine(SpecialFunctions.CreateTabString(PValueDetails.Header, "qValue"));
                foreach (Dictionary<string, string> row in realRowCollectionToSort)
                {
                    double qValue = qValueList[row];
                    outputStream.WriteLine(SpecialFunctions.CreateTabString(row[""], qValue)); 
                } 
            }
 
        }


        //!!!!somehow combine with Tabulate
        static public void TabulateForTwo(LrtForHla lrtForHlaA, string directoryA, string caseNameA, List<KeyValuePair<int, int>> firstNullAndLastNullListA,
                                     LrtForHla lrtForHlaB, string directoryB, string caseNameB, List<KeyValuePair<int, int>> firstNullAndLastNullListB) 
        { 
            SpecialFunctions.CheckCondition(lrtForHlaA.SelectionName == lrtForHlaB.SelectionName);
            List<Dictionary<string, string>> realRowCollectionToSort = new List<Dictionary<string, string>>(); 
            List<double> nullDiffCollectionToBeSorted = new List<double>();

            //!!!move VirusCount.PhyloTree.RangeCollection to a more general place
            RangeCollection rangeCollection = RangeCollection.GetInstance();
            int minFirstNullx = int.MaxValue;
            int maxLastNullx = int.MinValue; 
 
            foreach (char which in new char[] { 'A', 'B' })
            { 
                List<KeyValuePair<int, int>> firstNullAndLastNullList;
                string directory;
                LrtForHla lrtForHla;
                if (which == 'A')
                {
                    firstNullAndLastNullList = firstNullAndLastNullListA; 
                    directory = directoryA; 
                    lrtForHla = lrtForHlaA;
                } 
                else
                {
                    Debug.Assert(which == 'B');
                    firstNullAndLastNullList = firstNullAndLastNullListB;
                    directory = directoryB;
                    lrtForHla = lrtForHlaB; 
                } 

                int minFirstNull = int.MaxValue; 
                int maxLastNull = int.MinValue;
                foreach (KeyValuePair<int, int> firstNullAndLastNull in firstNullAndLastNullList)
                {
                    int firstNull = firstNullAndLastNull.Key;
                    minFirstNull = Math.Min(minFirstNull, firstNull);
                    int lastNull = firstNullAndLastNull.Value; 
                    maxLastNull = Math.Max(maxLastNull, lastNull); 
                    //!!!string repeated elsewere
                    //!!!what "new" or not? 

                    string inputFileName = string.Format(@"{0}\{1}.{2}.{3}-{4}.pValues.new.txt", directory, lrtForHla.SelectionName, lrtForHla.CaseName, firstNull, lastNull); //!!!const
                    int maxNullSeen = int.MinValue;

                    //foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(inputFileName, true))
                    foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(inputFileName, PValueDetails.Header, true)) 
                    { 
                        int nullIndex = int.Parse(row["NullIndex"]);
                        rangeCollection.TryAdd(nullIndex); 
                        if (nullIndex == -1)
                        {
                            realRowCollectionToSort.Add(row);
                        }
                        else
                        { 
                            double value = AccessPValueFromRow(row); 
                            nullDiffCollectionToBeSorted.Add(value);
                            maxNullSeen = Math.Max(maxNullSeen, nullIndex); 
                        }
                    }
                }

                if (minFirstNullx == int.MaxValue)
                { 
                    minFirstNullx = minFirstNull; 
                    Debug.Assert(maxLastNullx == int.MinValue);
                    maxLastNullx = maxLastNull; 
                }
                else
                {
                    SpecialFunctions.CheckCondition(minFirstNullx == minFirstNull);
                    SpecialFunctions.CheckCondition(maxLastNullx == maxLastNull);
                } 
            } 
            SpecialFunctions.CheckCondition(realRowCollectionToSort.Count != 0);
            SpecialFunctions.CheckCondition(rangeCollection.IsComplete(minFirstNullx, maxLastNullx)); 
            int nullCount = maxLastNullx - Math.Max(0, minFirstNullx) + 1;

            //Dictionary<string,string> realRowCollectionToSortNAAs0 = new List<double>();
            //foreach (Dictionary<string,string> row in realRowCollectionToSort)
            //{
            //    double r = AccessPValueFromRow(row); 
            //    if (double.IsNaN(r)) 
            //    {
            //        Dictionary<string, string> row2 = new Dictionary<string, string>(); 
            //        foreach (KeyValuePair<string, string> keyAndValue in row)
            //        {
            //        }
            //        realRowCollectionToSortNAAs0.Add(0.0);
            //    }
            //    else 
            //    { 
            //        realRowCollectionToSortNAAs0.Add(row);
            //    } 
            //}

            Dictionary<Dictionary<string, string>, double>
                qValueList = SpecialFunctions.ComputeQValues(ref realRowCollectionToSort, AccessPValueFromRow, ref nullDiffCollectionToBeSorted, nullCount);

            string outputFile = string.Format(@"{0}\{1}.{2}-{3}.{4}.qValues.new.txt", directoryB, lrtForHlaA.SelectionName, lrtForHlaA.CaseName, lrtForHlaB.CaseName, nullCount); //!!!const 
 
            using (StreamWriter outputStream = File.CreateText(outputFile))
            { 
                outputStream.WriteLine(SpecialFunctions.CreateTabString(PValueDetails.Header, "qValue"));
                foreach (Dictionary<string, string> row in realRowCollectionToSort)
                {
                    double qValue = qValueList[row];
                    outputStream.WriteLine(SpecialFunctions.CreateTabString(row[""], qValue));
                } 
            } 

        } 


        internal IEnumerable<string> PeptideCollection()
        {
            return ReactTableUnfiltered.Keys;
        } 
 
        abstract internal Set<Hla> CreateCandidateHlaSet(Dictionary<string, Set<Hla>> pidToHlaSet, string peptide);
 
        abstract internal Dictionary<Hla, PValueDetails> CreateHlaToPValueDetails(
            int nullIndex,
            string peptide,
            Dictionary<string, Set<Hla>> patientList,
            Set<Hla> candidateHlaSet,
            TextWriter writer); 
 
        static double AccessPValueFromRow(Dictionary<string, string> row)
        { 
            double pValue = double.Parse(row["PValue"]);
            if (double.IsNaN(pValue))
            {
                return 0;
            }
            else 
            { 
                return pValue;
            } 
        }

        protected OptimizationParameterList FindBestParams(string peptide,
            Set<Hla> candidateHlaSet,
            Set<Hla> hlaWithLinkZero,
            Dictionary<string, Set<Hla>> patientList, 
            //Dictionary<string, Dictionary<string,double>> reactTable, 
            out double score)
        { 
            //SpecialFunctions.CheckCondition(false, "Regression test this to be sure that switch to new optimization method didn't change anything important - cmk 5/1/2006");
            Set<Hla> knownHlaSet = KnownTable(peptide);

            TrueCollection trueCollection = TrueCollection.GetInstance(candidateHlaSet, knownHlaSet);

            OptimizationParameterList qmrrParamsThisPeptide = CreateQmrrParamsStartForTheseCandidateHlas(trueCollection.CreateHlaAssignmentAsSet(), hlaWithLinkZero); 
            //OptimizationParameterList qmrrParamsThisPeptide = CreateQmrrParamsX(causePrior, leakProbability, trueCollection.CreateHlaAssignmentAsSet()); 

 
            ModelLikelihoodFactories modelLikelihoodFactories = ModelLikelihoodFactories.GetInstanceLinkPerHla(qmrrParamsThisPeptide, trueCollection.CreateHlaAssignmentAsSet());
            QmrrPartialModelCollection singletonQmrrPartialModelCollection =
                    QmrrPartialModelCollection.GetInstance(
                            peptide,
                            modelLikelihoodFactories,
                            qmrrParamsThisPeptide, 
                            patientList, 
                            ReactTableUnfiltered,
                            _knownTable, 
                            HlaFactory.Name //!!!would it be better to pass the actual factory???
                            );
            BestSoFar<double, TrueCollection> bestSoFar = BestSoFar<double, TrueCollection>.GetInstance(SpecialFunctions.DoubleGreaterThan);
            bestSoFar.Compare(double.NegativeInfinity, trueCollection);
            Dictionary<string, BestSoFar<double, TrueCollection>> peptideToBestHlaAssignmentSoFar = new Dictionary<string, BestSoFar<double, TrueCollection>>();
            peptideToBestHlaAssignmentSoFar.Add(peptide, bestSoFar); 
 
            QmrrlModelMissingParametersCollection aQmrrlModelMissingParametersCollection
                 = QmrrlModelMissingParametersCollection.GetInstance(modelLikelihoodFactories, singletonQmrrPartialModelCollection, peptideToBestHlaAssignmentSoFar); 


            OptimizationParameterList qmrrParamsEnd = aQmrrlModelMissingParametersCollection.FindBestParams(qmrrParamsThisPeptide, out score);
            return qmrrParamsEnd;
        }
 
 
        internal Set<Hla> CreateUnivariateHlaSet(double pValueCutOff, Dictionary<string, Set<Hla>> pidToHlaSet, string peptide)
        { 
            Set<Hla> univariateHlaSet = Set<Hla>.GetInstance();
            foreach (Hla hla in HlaUniverse)
            {
                int[,] fourCounts = new int[2, 2]; //C# init's to 0's
                foreach (string pid in pidToHlaSet.Keys)
                { 
                    bool hasHla = pidToHlaSet[pid].Contains(hla); 
                    bool doesReact = ReactTableUnfiltered[peptide].ContainsKey(pid);
                    ++fourCounts[hasHla ? 1 : 0, doesReact ? 1 : 0]; 
                }

                double pValue = SpecialFunctions.FisherExactTest(fourCounts);

                if (pValue <= pValueCutOff)
                { 
                    univariateHlaSet.AddNewOrOld(hla); 
                }
            } 
            return univariateHlaSet;
        }

        protected OptimizationParameterList CreateQmrrParamsStartForTheseCandidateHlas(Set<Hla> trueHlaSet, Set<Hla> hlaWithLinkZero)
        {
            List<OptimizationParameter> parameterCollection = new List<OptimizationParameter>(); 
            parameterCollection.Add(OptimizationParameter.GetProbabilityInstance("causePrior", .5, false)); 
            foreach (Hla hla in trueHlaSet)
            { 
                bool fixAtZero = hlaWithLinkZero.Contains(hla);
                OptimizationParameter aParameter = OptimizationParameter.GetProbabilityInstance("link" + hla, fixAtZero ? 0 : .5, !fixAtZero);
                parameterCollection.Add(aParameter);
            }
            if (LeakProbabilityOrNull == null)
            { 
                parameterCollection.Add(OptimizationParameter.GetProbabilityInstance("leakProbability", .01, true)); 
            }
            else 
            {
                parameterCollection.Add(OptimizationParameter.GetProbabilityInstance("leakProbability", (double)LeakProbabilityOrNull, false));
            }
            parameterCollection.Add(OptimizationParameter.GetPositiveFactorInstance("useKnownList", 1, false));
            OptimizationParameterList qmrrParamsStart = OptimizationParameterList.GetInstance2(parameterCollection);
            return qmrrParamsStart; 
        } 

 
        protected Set<Hla> HlaSetFromReactingPatients(Dictionary<string, Set<Hla>> pidToHlaSet, string peptide)
        {
            Dictionary<string, double> pidToReactValue = ReactTableUnfiltered[peptide];

            Set<string> patientsInHlaFile = Set<string>.GetInstance(pidToHlaSet.Keys);
            Set<string> patientsInReactFile = Set<string>.GetInstance(pidToReactValue.Keys); 
            Set<string> commonPatients = patientsInReactFile.Intersection(patientsInHlaFile); 

            Set<Hla> reactingPatientsHlas = Set<Hla>.GetInstance(); 
            foreach (string pid in commonPatients)
            {
                foreach (Hla hla in pidToHlaSet[pid])
                {
                    if (hla.IsGround)
                    { 
                        reactingPatientsHlas.AddNewOrOld(hla); 
                    }
                } 
            }
            return reactingPatientsHlas.Subtract(KnownTable(peptide));
        }

        private static void ReportOnExtraPatients(Set<string> patientsInFile1, string fileName1, Set<string> patientsInFile2, string fileName2)
        { 
            Set<string> patientsInHlaButNotReactFile = patientsInFile1.Subtract(patientsInFile2); 
            if (patientsInHlaButNotReactFile.Count > 0)
            { 
                Console.WriteLine("Warning: These patient(s) are in {0} file, but not {1} file: {2}", fileName1, fileName2, SpecialFunctions.Join(",", patientsInHlaButNotReactFile));
            }
        }

    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
