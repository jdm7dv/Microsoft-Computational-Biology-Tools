using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions; 
 
namespace AASeqToSparse
{ 
    public class CaseIdToAASeq
    {
        private CaseIdToAASeq()
        {
        }
 
        private Dictionary<string, AASeq> _caseIdToAASeq; 
        private int? SequenceLength = null;
 
        static public CaseIdToAASeq GetInstance()
        {
            CaseIdToAASeq caseId = new CaseIdToAASeq();
            caseId._caseIdToAASeq = new Dictionary<string, AASeq>();
            return caseId;
        } 
 
        //    /*
        //    1189MB    MEPVDPNLEPWNHPGSQPKTPCTNCYCKHCSYHCLVCFQTKGLGISYGRK 
        //    J112MA    MEPVDPNLEPWNHPGSQPITACNKCYCKYCSYHCLVCFQTKGLGISYGRK
        //    1157M3M   MEPVDPNLEPWNHPGSQPKTPCNKCYCKHCSYHCLVCFQTKGLGISYGRK
        //    1195MB    MEPVDPNLEPWNHPGSQPKTPCNKCYCKYCSYHCLVCFQTKGLGISYGRK
        //     */
        static public CaseIdToAASeq GetInstance(TextReader textReader, bool mixture)
        { 
            CaseIdToAASeq caseIdToAASeq = CaseIdToAASeq.GetInstance(); 
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(textReader, "cid\taaSeq", false))
            { 
                string caseId = row["cid"]; //!!!const
                string aaSeqAsString = row["aaSeq"];//!!!const
                AASeq aaSeq = AASeq.GetInstance(aaSeqAsString, mixture);
                caseIdToAASeq.Add(caseId, aaSeq);
            }
 
            return caseIdToAASeq; 
        }
 

        static public CaseIdToAASeq GetInstance(string aaSeqFileName, bool mixture)
        {
            using (TextReader textReader = File.OpenText(aaSeqFileName))
            {
                return GetInstance(textReader, mixture); 
            } 
        }
 
        public void Add(string caseId, AASeq aaSeq)
        {
            SpecialFunctions.CheckCondition(!_caseIdToAASeq.ContainsKey(caseId), string.Format("caseId {0} appears more than once", caseId));
            if (null == SequenceLength)
            {
                SequenceLength = aaSeq.Count; 
            } 
            if (SequenceLength != aaSeq.Count)
            { 
                Console.WriteLine("Warning: Not all amino acid sequences are of the same length");
            }
            _caseIdToAASeq.Add(caseId, aaSeq);
        }

        //private static char AnyAminoAcid = '?'; 
 
        //public static string AA1PosToAtAA1(int aa1Pos)
        //{ 
        //}

        public static string SparseHeader = "var\tcid\tval";
        public void CreateSparseStream(TextWriter textWriter, bool keepOneValueVariables)
        {
            textWriter.WriteLine(SparseHeader); 
            foreach(string line in SparseLineEnumeration(keepOneValueVariables)) 
            {
                textWriter.WriteLine(line); 
            }
        }

        public IEnumerable<string> SparseLineEnumeration(bool keepOneValueVariables)
        {
            if (_caseIdToAASeq.Count == 0) 
            { 
                Debug.Assert(SequenceLength == null); // real assert
                yield break; 
            }
            SpecialFunctions.CheckCondition(SequenceLength != null, "This converter to sparse assumes all sequences have the same length");

            /*
            n1pos	aa	pid	val
            880	A	3	F 
            880	A	5	F 
            880	A	9	F
            880	A	13	F 
            880	A	14	F
            880	A	15	T
            ...
                */

 
            for (int aa0Pos = 0; aa0Pos < (int) SequenceLength; ++aa0Pos) 
            {
                Set<char> everyAminoAcid = EveryAminoAcid(aa0Pos); 
                if (!keepOneValueVariables && everyAminoAcid.Count == 1)
                {
                    continue;
                }

                string posName = null; 
                foreach (char aa in everyAminoAcid) 
                {
                    Set<bool> valueSet = Set<bool>.GetInstance(); 
                    Dictionary<string, bool> caseToVal = new Dictionary<string, bool>();
                    foreach (string caseId in _caseIdToAASeq.Keys)
                    {
                        AASeq aaSeq = _caseIdToAASeq[caseId];
                        //SpecialFunctions.CheckCondition(aaSeq.IsUsingOriginalPositions(), "This converter to sparse assumes all sequences are using their original positions");
                        Set<char> strainAASet = aaSeq[aa0Pos]; 
                        if (posName == null) 
                        {
                            posName = aaSeq.OriginalAA1Position(aa0Pos); 
                        }
                        else
                        {
                            SpecialFunctions.CheckCondition(posName == aaSeq.OriginalAA1Position(aa0Pos));
                        }
                        // missing: e.g.  A/Any   or   A/AB 
                        // 1: e.g. A/A 
                        // 0: e.g. A/B    or  A/BCD
                        if (strainAASet.Equals(AASeq.Any)) 
                        {
                            //Do nothing - missing
                        }
                        else if (strainAASet.Contains(aa))
                        {
                            if (strainAASet.Count > 1) 
                            { 
                                if (aaSeq.Mixture)
                                { 
                                    caseToVal.Add(caseId, false);
                                    valueSet.AddNewOrOld(false);
                                }
                                else
                                {
                                    // Do nothing = missing 
                                } 
                            }
                            else 
                            {
                                caseToVal.Add(caseId, true);
                                valueSet.AddNewOrOld(true);
                            }
                        }
                        else 
                        { 
                            caseToVal.Add(caseId, false);
                            valueSet.AddNewOrOld(false); 
                        }
                    }
                    SpecialFunctions.CheckCondition(posName != null);
                    if (keepOneValueVariables || valueSet.Count == 2)
                    {
                        foreach (KeyValuePair<string, bool> caseIdAndVal in caseToVal) 
                        { 
                            //string variableName = string.Format("{0}@{1}", posName, aa);
                            string variableName = string.Format("{1}@{0}", posName, aa); 
                            yield return SpecialFunctions.CreateTabString(
                                variableName, caseIdAndVal.Key, caseIdAndVal.Value ? 1 : 0);
                        }
                    }
                }
            } 
        } 

        private Set<char> EveryAminoAcid(int aa0Pos) 
        {
            Set<char> every = Set<char>.GetInstance();
            foreach (AASeq aaSequence in _caseIdToAASeq.Values)
            {
                every.AddNewOrOldRange(aaSequence[aa0Pos]);
            } 
            every.RemoveIfPresent('?'); 
            return every;
        } 

        public void CreateAASeqStream(TextWriter textWriter)
        {

            //    /*
            //    caseId    aaSeq 
            //    1189MB    MEPVDPNLEPWNHPGSQPKTPCTNCYCKHCSYHCLVCFQTKGLGISYGRK 
            //    J112MA    MEPVDPNLEPWNHPGSQPITACNKCYCKYCSYHCLVCFQTKGLGISYGRK
            //    1157M3M   MEPVDPNLEPWNHPGSQPKTPCNKCYCKHCSYHCLVCFQTKGLGISYGRK 
            //    1195MB    MEPVDPNLEPWNHPGSQPKTPCNKCYCKYCSYHCLVCFQTKGLGISYGRK
            //     */

            textWriter.WriteLine("cid\taaSeq");
            foreach (KeyValuePair<string, AASeq> caseIdAndAASeq in _caseIdToAASeq)
            { 
                textWriter.WriteLine("{0}\t{1}", caseIdAndAASeq.Key, caseIdAndAASeq.Value); 
            }
        } 

        public void CreateAASeqFile(string outputFileName)
        {
            using (TextWriter textWriter = File.CreateText(outputFileName))
            {
                CreateAASeqStream(textWriter); 
            } 
        }
 
        public void CreateMerSparseStream(TextWriter textWriter, int merLength, bool keepOneValueVariables)
        {
            Dictionary<string, AASeq> caseToCompressedAASeq = RemoveDeletesAndStopsFromData(textWriter);
            Dictionary<string, string> merStringToBestOriginalAA0Position = FindMerStringToBestOriginalAA0Position(merLength, textWriter, caseToCompressedAASeq);
            Debug.Assert(Set<string>.GetInstance(merStringToBestOriginalAA0Position.Keys).Equals(
                Set<string>.GetInstance(EveryUnambiguousMer(merLength, caseToCompressedAASeq)))); 
 

            textWriter.WriteLine("var\tcid\tval"); 

            foreach (string mer in merStringToBestOriginalAA0Position.Keys)
            {
                Dictionary<bool, int> valueToNonZeroCount;
                Dictionary<string, bool> caseIdToValue = FindMerValues(mer, caseToCompressedAASeq, out valueToNonZeroCount);
                Debug.Assert(valueToNonZeroCount.Count != 0); 
 
                if (valueToNonZeroCount.Count == 1 && !keepOneValueVariables)
                { 
                    continue;
                }

                foreach (string caseId in caseIdToValue.Keys)
                {
                    int val = (caseIdToValue[caseId]) ? 1 : 0; 
                    string variableName = string.Format("{0}@{1}", mer, merStringToBestOriginalAA0Position[mer]); 
                    textWriter.WriteLine(SpecialFunctions.CreateTabString(variableName, caseId, val));
                } 
            }

        }

        private Dictionary<string, bool> FindMerValues(string merAsString, Dictionary<string, AASeq> caseToCompressedAASeq, out Dictionary<bool, int> valueToNonZeroCount)
        { 
            Regex merAsRegex = AASeq.CreateMerRegex(merAsString); 

            Dictionary<string, bool> merValues = new Dictionary<string, bool>(); 
            valueToNonZeroCount = new Dictionary<bool, int>();
            foreach (KeyValuePair<string, AASeq> caseIdAndCompressedAASeq in caseToCompressedAASeq)
            {
                string caseId = caseIdAndCompressedAASeq.Key;
                AASeq compressedAASeq = caseIdAndCompressedAASeq.Value;
 
                bool? containsMer = compressedAASeq.ContainsMer(merAsString, merAsRegex); 

                if (null != containsMer) 
                {
                    merValues.Add(caseId, (bool)containsMer);
                    valueToNonZeroCount[(bool)containsMer] = 1 + SpecialFunctions.GetValueOrDefault(valueToNonZeroCount, (bool)containsMer);
                }

            } 
            return merValues; 
        }
 

        //internal void CreateSparseStream(TextWriter textWriter, bool keepOneValueVariables)
        //{
        //    CreateSparseStream(textWriter, keepOneValueVariables);
        //}
 
        public void CreateSparseFile(string outputFileName, bool keepOneValueVariables) 
        {
            using (TextWriter textWriter = File.CreateText(outputFileName)) 
            {
                CreateSparseStream(textWriter, keepOneValueVariables);
            }
        }

        //public void CreateSparseFile(string outputFileName, bool keepOneValueVariables) 
        //{ 
        //    CreateSparseFile(outputFileName, keepOneValueVariables);
        //} 

        private Dictionary<string, AASeq> RemoveDeletesAndStopsFromData(TextWriter textWriter)
        {
            Dictionary<string, AASeq> compressedDictionary = new Dictionary<string, AASeq>();
            foreach (KeyValuePair<string, AASeq> caseIdAndAASeq in _caseIdToAASeq)
            { 
                AASeq compressedAASeq = AASeq.GetCompressedInstance(caseIdAndAASeq.Key, caseIdAndAASeq.Value, textWriter); 
                compressedDictionary.Add(caseIdAndAASeq.Key, compressedAASeq);
            } 
            return compressedDictionary;
        }

        private IEnumerable<string> EveryUnambiguousMer(int merLength, Dictionary<string, AASeq> caseToCompressedAASeq)
        {
            Set<string> nonMissingMerSet = Set<string>.GetInstance(); 
            foreach (AASeq aaSeq in caseToCompressedAASeq.Values) 
            {
                foreach (AASeq mer in aaSeq.SubSeqEnumeration(merLength)) 
                {
                    if (mer.Ambiguious)
                    {
                        continue;
                    }
                    string merAsString = mer.ToString(); 
 
                    Debug.Assert(!merAsString.Contains("*") && !merAsString.Contains("-") && !merAsString.Contains("?")); // real assert
                    if (nonMissingMerSet.Contains(merAsString)) 
                    {
                        continue;
                    }
                    nonMissingMerSet.AddNew(merAsString);
                    yield return merAsString;
                } 
            } 
        }
 

        public void CreateMainKmerPositionsStream(int merLength, TextWriter textWriter)
        {
            Dictionary<string, AASeq> caseToCompressedAASeq = RemoveDeletesAndStopsFromData(textWriter);
            Dictionary<string, string> merStringToBestOriginalAA0Position = FindMerStringToBestOriginalAA0Position(merLength, textWriter, caseToCompressedAASeq);
 
            textWriter.WriteLine(SpecialFunctions.CreateTabString("kMer", "MostCommonOriginalAA1Position")); 
            foreach (string merString in merStringToBestOriginalAA0Position.Keys)
            { 
                string bestOriginalAA0Position = merStringToBestOriginalAA0Position[merString];
                textWriter.WriteLine(SpecialFunctions.CreateTabString(merString, bestOriginalAA0Position));
            }
        }

        private static Dictionary<string, string> FindMerStringToBestOriginalAA0Position(int merLength, TextWriter textWriterForWarnings, Dictionary<string, AASeq> caseToCompressedAASeq) 
        { 
            Dictionary<string, Dictionary<string, int>> merStringToOriginalAA0PositionToCount = CreateMerStringToOriginalAA0PositionToCount(merLength, textWriterForWarnings, caseToCompressedAASeq);
            Dictionary<string, string> merStringToBestOriginalAA0Position = new Dictionary<string, string>(); 
            foreach (string merString in merStringToOriginalAA0PositionToCount.Keys)
            {
                Dictionary<string, int> originalAA0PositionToCount = merStringToOriginalAA0PositionToCount[merString];
                VirusCount.BestSoFar<int, string> best = FindBest(originalAA0PositionToCount);
                merStringToBestOriginalAA0Position.Add(merString, best.Champ);
            } 
            return merStringToBestOriginalAA0Position; 
        }
 
        private static VirusCount.BestSoFar<int, string> FindBest(Dictionary<string, int> originalAA0PositionToCount)
        {
            VirusCount.BestSoFar<int, string> bestSoFar = VirusCount.BestSoFar<int, string>.GetInstance(SpecialFunctions.IntGreaterThan);
            foreach (KeyValuePair<string, int> originalAA0PositionAndCount in originalAA0PositionToCount)
            {
                bestSoFar.Compare(originalAA0PositionAndCount.Value, originalAA0PositionAndCount.Key); 
            } 
            return bestSoFar;
        } 

        private static Dictionary<string, Dictionary<string, int>> CreateMerStringToOriginalAA0PositionToCount(int merLength, TextWriter textWriterForWarnings, Dictionary<string, AASeq> caseToCompressedAASeq)
        {
            Dictionary<string, Dictionary<string, int>> merStringToOriginalAA0PositionToCount = new Dictionary<string, Dictionary<string, int>>();
            foreach (string caseId in caseToCompressedAASeq.Keys)
            { 
                AASeq aaSeq = caseToCompressedAASeq[caseId]; 

                Set<string> SeenIt = new Set<string>(); 
                foreach (AASeq mer in aaSeq.SubSeqEnumeration(merLength))
                {
                    if (mer.Ambiguious)
                    {
                        continue;
                    } 
 
                    string merString = mer.ToString();
                    if (SeenIt.Contains(merString)) 
                    {
                        textWriterForWarnings.WriteLine("Warning: Mer '{0}' appears again in case '{1}'", merString, caseId);
                    }
                    SeenIt.AddNewOrOld(merString);

                    string originalAA1Position = mer.OriginalAA1Position(0); 
 
                    Dictionary<string, int> originalAA0PositionToCount = SpecialFunctions.GetValueOrDefault(merStringToOriginalAA0PositionToCount, merString);
                    originalAA0PositionToCount[originalAA1Position] = 1 + SpecialFunctions.GetValueOrDefault(originalAA0PositionToCount, originalAA1Position); 
                }
            }
            return merStringToOriginalAA0PositionToCount;
        }

 
    } 

} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
