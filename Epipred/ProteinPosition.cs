using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount 
{ 

    [Serializable()] 
    public class ProteinPosition : IComparable
    {



        //			public static ProteinPosition GetProjection(ProteinPosition aProteinPosition, string proteinToProjectOn, Study study) 
        //			{ 
        //				if (aProteinPosition.Protein == proteinToProjectOn)
        //				{ 
        //					return aProteinPosition;
        //				}
        //
        //				CodingSequence aCodingSequenceA  =  study.Organism.ProteinToDnaEncoding[aProteinPosition.Protein];
        //				int iNucleotidePositionBase1 = aCodingSequenceA.AbsoluteNucleotideBase1FromAminoAcidIndexBase0(aProteinPosition.AminoAcidBase1 - 1);
        // 
        //				CodingSequence aCodingSequenceB  =  study.Organism.ProteinToDnaEncoding[proteinToProjectOn]; 
        //				aCodingSequenceB.AbsoluteNucleotideBase1FromNucleotideIndexBase1
        // 
        //				ProteinPosition proteinPositionOut  =  study.Organism.AllNuc1PositionToProteinPosition[iNucleotidePositionBase1];
        //				return proteinPositionOut;
        //			}

        public ProteinPosition(string protein, int aminoAcidBase1)
        { 
            Protein = protein; 
            AminoAcidBase1 = aminoAcidBase1;
        } 
        public string Protein;
        public int AminoAcidBase1;
        public int AminoAcidBase0
        {
            get
            { 
                return AminoAcidBase1 - 1; 
            }
        } 
        public override string ToString()
        {
            return string.Format("{0}+{1}", Protein, AminoAcidBase1);
        }

        //!!!could make this class a struct and then wouldn't need to clone, but could need to check all uses of the code 
        public ProteinPosition Clone() 
        {
            return new ProteinPosition(Protein, AminoAcidBase1); 
        }


        //			string sProtein = proteinAndOffset.Substring(0,proteinAndOffset.LastIndexOf("+")); //!!!could raise error

        //int iAABase1 = int.Parse(proteinPositionStart.Substring(proteinPositionStart.LastIndexOf("+"))); //!!!could raise error 
        //string.Format("{0}+{1}", sProtein, iBase1AminoAcidCount); 

        public int CompareTo(object y) 
        {
            ProteinPosition yProteinPosition = (ProteinPosition)y; //!!!raise error

            if (AminoAcidBase1 != yProteinPosition.AminoAcidBase1)
            {
                return AminoAcidBase1.CompareTo(yProteinPosition.AminoAcidBase1); 
            } 

            return Protein.CompareTo(yProteinPosition.Protein); 

        }
    }


 
    public class ProteinPositionAndLength : IComparable 
    {
        private ProteinPositionAndLength() 
        {
        }

        static public ProteinPositionAndLength GetInstance(ProteinPosition proteinPosition, int length)
        {
 
            ProteinPositionAndLength aProteinPositionAndLength = new ProteinPositionAndLength(); 
            aProteinPositionAndLength.ProteinPosition = proteinPosition;
            aProteinPositionAndLength.Length = length; 
            return aProteinPositionAndLength;
        }

        public ProteinPosition ProteinPosition;
        public int Length;
 
        public int CompareTo(object y) 
        {
            ProteinPositionAndLength yProteinPositionAndLength = (ProteinPositionAndLength)y; //!!!raise error 

            int compareProteinPosition = ProteinPosition.CompareTo(yProteinPositionAndLength.ProteinPosition);
            if (compareProteinPosition != 0)
            {
                return compareProteinPosition;
            } 
 
            return Length.CompareTo(yProteinPositionAndLength.Length);
        } 

    }

    [Serializable()]
    public class ProteinPositionPlus : IComparable
    { 
        public override string ToString() 
        {
            return string.Format("{0}/{1}@{2}", ProteinPosition, Nucleotide1Index, NucleotideIndexReference); 
        }
        public override int GetHashCode()
        {
            return Nucleotide1Index.GetHashCode();
        }
        public override bool Equals(object obj) 
        { 
            return CompareTo(obj) == 0;
        } 
        private ProteinPositionPlus()
        {
        }

        static public ProteinPositionPlus GetInstance(ProteinPosition proteinPosition, int nucleotide1Index, NucleotideIndexReference nucleotideIndexReference)
        { 
            ProteinPositionPlus aProteinPositionPlus = new ProteinPositionPlus(); 
            aProteinPositionPlus.ProteinPosition = proteinPosition;
            aProteinPositionPlus.Nucleotide1Index = nucleotide1Index; 
            aProteinPositionPlus.NucleotideIndexReference = nucleotideIndexReference;
            return aProteinPositionPlus;
        }

        public ProteinPosition ProteinPosition;
        public int Nucleotide1Index; 
        public NucleotideIndexReference NucleotideIndexReference; 

        public int CompareTo(object y) 
        {
            ProteinPositionPlus yProteinPositionPlus = (ProteinPositionPlus)y; //!!!raise error
            Debug.Assert(NucleotideIndexReference == yProteinPositionPlus.NucleotideIndexReference, "Must have the same reference organism");

            int compareNucleotideIndex = NucleotideIndexReference.CompareTo(yProteinPositionPlus.NucleotideIndexReference);
            Debug.Assert((compareNucleotideIndex == 0) == (0 == ProteinPosition.CompareTo(yProteinPositionPlus.ProteinPosition))); 
            return compareNucleotideIndex; 
        }
 

    }

    public enum NucleotideIndexReference
    {
        Hxb2, 
        DurbanGag 
    }
 

}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
