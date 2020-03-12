using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using AASeqToSparse;
 
namespace ProcessingPrediction 
{
    public class NEC 
    {
        private NEC()
        {
        }

        public static NEC GetInstance(string peptide, int nLength, int eLength, int cLength) 
        { 
            SpecialFunctions.CheckCondition(peptide.Length == nLength + eLength + cLength, "NEC.GetInstance lengths are wrong");
            NEC nec = GetInstance(peptide.Substring(0, nLength), peptide.Substring(nLength, eLength), peptide.Substring(nLength + eLength, cLength)); 
            Debug.Assert(nec.N + nec.E + nec.C == peptide); // real assert
            return nec;
        }

        static public NEC GetInstance(string n, string e, string c)
        { 
            NEC nec = new NEC(); 
            nec.N = n;
            nec.E = e; 
            nec.C = c;
            nec._asString = SpecialFunctions.CreateDelimitedString(",", n, e, c);
            return nec;
        }

        public string N; 
        public string E; 
        public string C;
        public string _asString; 

        public override string ToString()
        {
            return _asString;
        }
 
        public override bool Equals(object obj) 
        {
            NEC other = obj as NEC; 
            if (other == null)
            {
                return false;
            }
            else
            { 
                return other._asString.Equals(_asString); 
            }
        } 

        public override int GetHashCode()
        {
            return "NEC".GetHashCode() ^ _asString.GetHashCode();
        }
 
        public static bool TryCreateInstance(string epitopeString, int aa1PosStart, int aa1PosLast, AASeq aaSeqConsensus, out NEC nec) 
        {
            AASeq aaSeqSubConsensus = aaSeqConsensus.SubSeqAA1Pos(aa1PosStart, epitopeString.Length); 
            Debug.Assert(aaSeqSubConsensus.OriginalAA1Position(0) == aa1PosStart.ToString()); // real assert
            string consensusAsString = aaSeqConsensus.ToString();

            int flankingSize = 5;

            //Create a string builder to file from back to front 
            StringBuilder sbN = new StringBuilder(new string(' ', flankingSize)); 
            int sbNIndex = flankingSize - 1;
 
            //Look at the characters of the concensus moving to the left
            for(int aa1Pos = aa1PosStart - 1;; --aa1Pos)
            {
                if (aa1Pos < 1)
                {
                    Console.WriteLine("Warning: for epitope {0}, the epitope position is too close to the left of the protein to create a c region of length 5. Skipping epitope", epitopeString); 
                    nec = null; 
                    return false;
                } 

                char aa = consensusAsString[aa1Pos-1];
                if (aa == '-')
                {
                    continue;
                } 
                sbN[sbNIndex] = aa; 
                --sbNIndex;
                if (sbNIndex < 0) 
                {
                    break;
                }
            }

            StringBuilder sbC = new StringBuilder(); 
            //Look at the characters of the concensus moving to the right 
            for (int aa1Pos = aa1PosLast + 1; ; ++aa1Pos)
            { 
                if (aa1Pos > consensusAsString.Length)
                {
                    Console.WriteLine("Warning: for epitope {0}, the epitope position is too close to the right of the protein to create a n region of length 5. Skipping epitope", epitopeString);
                    nec = null;
                    return false;
                } 
 
                char aa = consensusAsString[aa1Pos - 1];
                if (aa == '-') 
                {
                    continue;
                }
                sbC.Append(aa);
                if (sbC.Length == flankingSize)
                { 
                    break; 
                }
            } 



            nec = NEC.GetInstance(sbN.ToString(), epitopeString, sbC.ToString());

            return true; 
        } 

        public char GetAA(string region, int aa1AwayFromE) 
        {
            //!!!switch to switch
            if (region == "N")
            {
                return N[N.Length - aa1AwayFromE];
            } 
            else if (region == "C") 
            {
                return C[aa1AwayFromE - 1]; 
            }
            else
            {
                SpecialFunctions.CheckCondition(false, string.Format("Unexpected region value of '{0}'", region));
                return char.MinValue;
            } 
        } 

        internal string GetAASeq(string region) 
        {
            //!!!switch to switch
            if (region == "N")
            {
                return N;
            } 
            else if (region == "C") 
            {
                return C; 
            }
            else if (region == "E")
            {
                return E;
            }
            else 
            { 
                SpecialFunctions.CheckCondition(false, string.Format("Unexpected region value of '{0}'", region));
                return null; 
            }
        }

        public static NEC GetInstanceWithPossibleNulls(int flankingSize, string mer, string protein, int position)
        {
            string e = protein.Substring(position, mer.Length); 
 
            string n;
            if (position - flankingSize < 0) 
            {
                //Console.WriteLine("Warning: matching position in protein too close to start to create left flanking region");
                n = null;
            }
            else
            { 
                n = protein.Substring(position - flankingSize, flankingSize); 
            }
 
            string c;
            if (position + mer.Length + flankingSize > protein.Length)
            {
                //Console.WriteLine("Warning: matching position in protein too close to end to create right flanking region");
                c = null;
            } 
            else 
            {
                c = protein.Substring(position + mer.Length, flankingSize); 
            }
            NEC nec = NEC.GetInstance(n, e, c);
            return nec;
        }

        public static NEC GetInstance(string necString) 
        { 
            string[] fieldCollection = necString.Split(',');
            SpecialFunctions.CheckCondition(fieldCollection.Length == 3); 
            for(int iField = 0; iField < fieldCollection.Length; ++iField)
            {
                if (fieldCollection[iField] == "null")
                {
                    fieldCollection[iField] = null;
                } 
            } 
            NEC nec = NEC.GetInstance(fieldCollection[0], fieldCollection[1], fieldCollection[2]);
            return nec; 
        }

        static public string GetN(NEC nec)
        {
            return nec.N;
        } 
        static public string GetE(NEC nec) 
        {
            return nec.E; 
        }
        static public string GetC(NEC nec)
        {
            return nec.C;
        }
 
        public static NEC GetConsensusInstanceWithPossibleNullFlanks(string mer, Set<NEC> necSet, ref Random random) 
        {
            DebugCheckMerSameLengthAsEInSet(mer, necSet); 
            string n = FindConsensusOrNull(necSet, GetN, ref random);
            string c = FindConsensusOrNull(necSet, GetC, ref random);
            NEC nec = NEC.GetInstance(n, mer, c);
            return nec;
        }
 
        [Conditional("DEBUG")] 
        private static void DebugCheckMerSameLengthAsEInSet(string mer, Set<NEC> necSet)
        { 
            foreach (NEC nec in necSet)
            {
                Debug.Assert(nec.E.Length == mer.Length);
            }
        }
 
        private static string FindConsensusOrNull(Set<NEC> necSet, Converter<NEC, string> GetField, ref Random random) 
        {
            string sampleFlanking = GetField(necSet.AnyElement()); 
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < sampleFlanking.Length; ++i)
            {
                char consensusAAOrMinValue = FindConsensusOrMinValue(necSet, i, sampleFlanking.Length, GetField, random);
                if (consensusAAOrMinValue == char.MinValue)
                { 
                    return null; 
                }
                sb.Append(consensusAAOrMinValue); 
            }
            return sb.ToString();
        }

        private static char FindConsensusOrMinValue(Set<NEC> necSet, int i, int c, Converter<NEC, string> GetField, Random random)
        { 
            Dictionary<char, int> charToCount = TabulateCharToCount(necSet, i, c, GetField); 

            if (charToCount.Count == 0) 
            {
                return char.MinValue;
            }

            char bestItem = PickRandomlyAmongBest(charToCount, ref random);
 
            return bestItem; 

        } 

        private static char PickRandomlyAmongBest(Dictionary<char, int> charToCount, ref Random random)
        {
            int bestCountSoFar = 0;
            int countItemsWithBestCount = 0;
            char bestItem = char.MinValue; 
            foreach (KeyValuePair<char, int> charAndCount in charToCount) 
            {
                if (bestCountSoFar < charAndCount.Value) 
                {
                    countItemsWithBestCount = 1;
                    bestCountSoFar = charAndCount.Value;
                    bestItem = charAndCount.Key;
                }
                else if (bestCountSoFar == charAndCount.Value) 
                { 
                    ++countItemsWithBestCount;
                    if (random.Next(countItemsWithBestCount) == 0) 
                    {
                        bestItem = charAndCount.Key;
                    }
                }
            }
            return bestItem; 
        } 

        private static Dictionary<char, int> TabulateCharToCount(Set<NEC> necSet, int i, int c, Converter<NEC, string> GetField) 
        {
            Dictionary<char, int> charToCount = new Dictionary<char, int>();
            foreach (NEC nec in necSet)
            {
                string flankingRegion = GetField(nec);
                SpecialFunctions.CheckCondition(flankingRegion.Length == c); 
                char aaChar = flankingRegion[i]; 
                charToCount[aaChar] = 1 + SpecialFunctions.GetValueOrDefault(charToCount, aaChar);
            } 
            if (charToCount.ContainsKey('X'))
            {
                charToCount.Remove('X');
            }
            return charToCount;
        } 
 

        public IEnumerable<string> RegionEnumeration() 
        {
            yield return N;
            yield return E;
            yield return C;
        }
 
        public static NEC GetRandomInstance(NEC sample, Dictionary<char, int> aaToCount, ref Random random) 
        {
            string n = RandomAAString(aaToCount, sample.N.Length, ref random); 
            string e = RandomAAString(aaToCount, sample.E.Length, ref random);
            string c = RandomAAString(aaToCount, sample.C.Length, ref random);
            return NEC.GetInstance(n, e, c);
        }

        private static string RandomAAString(Dictionary<char, int> aaToCount, int length, ref Random random) 
        { 
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; ++i) 
            {
                char aa = SpecialFunctions.RandomFromMultinomial(aaToCount, ref random);
                sb.Append(aa);
            }
            return sb.ToString();
        } 
 

 
        internal static NEC GetInstance(NEC input, int flankSize)
        {
            SpecialFunctions.CheckCondition(input.N.Length >= flankSize && input.C.Length >= flankSize, "The input flank size should not be larger than the new flank size");
            NEC output = NEC.GetInstance(input.N.Remove(0, input.N.Length - flankSize), input.E, input.C.Substring(0,flankSize));
            return output;
        } 
    } 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
