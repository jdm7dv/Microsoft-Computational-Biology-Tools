using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using VirusCount;
using System.IO; 
using System.Text.RegularExpressions; 

namespace AASeqToSparse 
{
    /// <summary>
    /// Warning: Hashcode and Equals ignores (by design) the information about original aa sequence.
    /// </summary>
    public class AASeq
    { 
        private AASeq(bool mixture) 
        {
            Mixture = mixture; 
        }

        public bool Mixture;
        private List<Set<char>> Sequence;
        private List<string> _originalAA1PositionTableOrNull = null;
        public string OriginalAA1Position(int currentAA0Position) 
        { 
            if (_originalAA1PositionTableOrNull == null)
            { 
                return (currentAA0Position + 1).ToString();
            }
            else
            {
                return _originalAA1PositionTableOrNull[currentAA0Position];
            } 
        } 

        private string AASeqAsString = null; 

        public Set<char> this[int index]
        {
            get
            {
                return Sequence[index]; 
            } 
        }
 
        public static Set<char> Delete = Set<char>.GetInstance('-');
        public static Set<char> Stop = Set<char>.GetInstance('*');
        public static Set<char> Any = Set<char>.GetInstance('?');

        static public AASeq GetCompressedInstance(string caseId, AASeq aaSeqIn, TextWriter errorStream)
        { 
            AASeq aaSeqOut = new AASeq(aaSeqIn.Mixture); 
            aaSeqOut.Sequence = new List<Set<char>>();
            aaSeqOut._originalAA1PositionTableOrNull = new List<string>(); 

            for (int iChar = 0; iChar < aaSeqIn.Count; ++iChar)
            {
                Set<char> set = aaSeqIn[iChar];
                string originalAA1Position = aaSeqIn.OriginalAA1Position(iChar);
                if (set.Equals(Delete)) //!!!const 
                { 
                    continue;
                } 
                if (set.Equals(Stop)) //!!!const
                {
                    if (iChar != aaSeqIn.Count - 1)
                    {
                        errorStream.WriteLine("Warning: The sequence for case id '{0}' contains a '*' before the last position", caseId);
                    } 
                    break; 
                }
                aaSeqOut.Sequence.Add(set); 
                aaSeqOut._originalAA1PositionTableOrNull.Add(originalAA1Position);
            }
            return aaSeqOut;
        }

        static public AASeq GetInstance(string aaSeqAsString, bool mixture) 
        { 
            AASeq aaSeq = new AASeq(mixture);
            aaSeq.Sequence = CreateSequence(aaSeqAsString); 
            return aaSeq;
        }

        static public AASeq GetInstance(string aaSeqAsString, List<string> originalAA1PositionTable, bool mixture)
        {
            AASeq aaSeq = new AASeq(mixture); 
            aaSeq.Sequence = CreateSequence(aaSeqAsString); 
            SpecialFunctions.CheckCondition(aaSeq.Count == originalAA1PositionTable.Count, "aaSeq and position table must be same length");
            aaSeq._originalAA1PositionTableOrNull = originalAA1PositionTable; 
            return aaSeq;
        }


        private static List<Set<char>> CreateSequence(string aaSeqAsString)
        { 
 
            List<Set<char>> sequence = new List<Set<char>>();
 
            Set<char> set = null;

            foreach (char ch in aaSeqAsString)
            {
                switch (ch)
                { 
                    case '{': 
                        {
                            SpecialFunctions.CheckCondition(set == null, "Nested '{''s are not allowed in aaSeq strings"); 
                            set = new Set<char>();
                            sequence.Add(set);
                            break;
                        }
                    case '}':
                        { 
                            SpecialFunctions.CheckCondition(set != null, "'}' must follow a '{' in aaSeq strings"); 
                            SpecialFunctions.CheckCondition(set.Count > 0, "Empty sets not allow in aaSeq strings");
                            set = null; 
                            break;
                        }
                    case ' ':
                        {
                            SpecialFunctions.CheckCondition(false, "Sequences should not contain blanks. Use '?' for missing.");
                            break; 
                        } 
                    case '?':
                    case '-': 
                        {
                            SpecialFunctions.CheckCondition(set == null, string.Format("'{0}' must not appear in sets", ch));
                            sequence.Add(Set<char>.GetInstance(ch));
                            break;
                        }
                    default: 
                        { 
                            //!!!most this to Biology?
                            SpecialFunctions.CheckCondition(Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter.ContainsKey(ch), 
                                string.Format("The character {0} is not an amino acid", ch));
                            string aminoAcid = Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[ch];
                            SpecialFunctions.CheckCondition(Biology.GetInstance().KnownAminoAcid(aminoAcid),
                                string.Format("The character {0} is not a standard amino acid", ch));
                            if (set == null)
                            { 
                                sequence.Add(Set<char>.GetInstance(ch)); 
                            }
                            else 
                            {
                                set.AddNew(ch);
                            }
                            break;
                        }
                } 
            } 
            SpecialFunctions.CheckCondition(set == null, "Missing '}' in aaSeq string");
            return sequence; 

        }

        private static string SequenceToString(List<Set<char>> sequence)
        {
            StringBuilder sb = new StringBuilder(); 
            foreach (Set<char> set in sequence) 
            {
                if (set.Count != 1) 
                {
                    sb.Append('{');
                }

                foreach(char ch in set)
                { 
                    sb.Append(ch); 
                }
 
                if (set.Count != 1)
                {
                    sb.Append('}');
                }
            }
            return sb.ToString(); 
        } 

        public override string ToString() 
        {
            if (AASeqAsString == null)
            {
                AASeqAsString = SequenceToString(Sequence);
            }
            return AASeqAsString; 
        } 

        public override int GetHashCode() 
        {
            return ToString().GetHashCode() ^ "AASeq".GetHashCode();
        }

        public override bool Equals(object obj)
        { 
            AASeq other = obj as AASeq; 
            if (other == null)
            { 
                return false;
            }
            else
            {
                return Sequence == other.Sequence;
            } 
        } 

        internal int Count 
        {
            get
            {
                return Sequence.Count;
            }
        } 
 
        internal IEnumerable<AASeq> SubSeqEnumeration(int merLength)
        { 
            for (int startIndex = 0; startIndex <= Sequence.Count - merLength; ++startIndex)
            {
                AASeq aaSeqOut = SubSeqAA0Pos(startIndex, merLength);
                yield return aaSeqOut;
            }
        } 
        public AASeq SubSeqAA1Pos(int aa1Pos, int merLength) 
        {
            return SubSeqAA0Pos(aa1Pos - 1, merLength); 
        }
        public bool TrySubSeqAA1Pos(int aa1Pos, int merLength, out AASeq aaSeq)
        {
            return TrySubSeqAA0Pos(aa1Pos - 1, merLength, out aaSeq);
        }
 
        public bool TrySubSeqAA0Pos(int aa0Pos, int merLength, out AASeq aaSeq) 
        {
            if (aa0Pos < 0 || aa0Pos + merLength > this.Sequence.Count) 
            {
                aaSeq = null;
                return false;
            }
            aaSeq = SubSeqAA0Pos(aa0Pos, merLength);
            return true; 
        } 

        public AASeq SubSeqAA0Pos(int aa0Pos, int merLength) 
        {
            List<Set<char>> subSequence = SpecialFunctions.SubList(Sequence, aa0Pos, merLength);
            AASeq aaSeqOut = new AASeq(Mixture);
            aaSeqOut.Sequence = subSequence;
            aaSeqOut._originalAA1PositionTableOrNull = new List<string>();
            for (int aa0 = aa0Pos; aa0 < aa0Pos + merLength; ++aa0) 
            { 
                string originalAA1Position = OriginalAA1Position(aa0);
                aaSeqOut._originalAA1PositionTableOrNull.Add(originalAA1Position); 
            }
            return aaSeqOut;
        }


        private bool? _ambiguious = null; 
 
        internal bool Ambiguious
        { 
            get
            {
                if (_ambiguious == null)
                {
                    SetAmbiguious();
                } 
                return (bool)_ambiguious; 
            }
        } 

        private void SetAmbiguious()
        {
            _ambiguious = false;
            foreach (Set<char> set in Sequence)
            { 
                if (set.Count > 1 || set.Equals(Any)) 
                {
                    _ambiguious = true; 
                    break;
                }
            }
        }

        internal bool? ContainsMer(string merAsString, Regex merAsRegex) 
        { 
            // e.g. As strings:  "AB{CD}E" contains "AB", but not "BC"
            if (ToString().Contains(merAsString)) 
            {
                return true;
            }

            // e.g. as pattern:  "AB{CD}E" does not contain "BF", but might contain BC
            // e.g. as pattern:  "AB?E" does not contain "BFG", but might contain "BFE" 
 
            else if (merAsRegex.IsMatch(ToString()))
            { 
                return null;
            }
            else
            {
                return false;
            } 
 
        }
 

        static public Regex CreateMerRegex(string mer)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char aaChar in mer)
            { 
                sb.AppendFormat(@"([{0}?]|({{[^}}]*{0}[^}}]*}}))", aaChar); 
            }
            return new Regex(sb.ToString()); 
        }


        //public bool IsUsingOriginalPositions()
        //{
        //    return _originalAA0PositionTableOrNull == null; 
        //} 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
