using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Msr.Mlas.SpecialFunctions;
 
namespace VirusCount 
{
    public class MerAndHlaToLength 
    {
        public string Mer;
        public HlaToLength HlaToLength;
        //internal Study Study;
        internal KmerDefinition KmerDefinition;
 
        public override int GetHashCode() 
        {
            return Mer.GetHashCode() 
                ^ HlaToLength.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            MerAndHlaToLength other = obj as MerAndHlaToLength; 
            if (other == null) 
            {
                return false; 
            }
            else
            {
                //SpecialFunctions.CheckCondition(Study == other.Study); //!!!raise error
                SpecialFunctions.CheckCondition(KmerDefinition.ToString() == other.KmerDefinition.ToString()); //!!!raise error
                bool b = other.Mer == Mer 
                    && other.HlaToLength == HlaToLength; 
                return b;
            } 
        }




        public static MerAndHlaToLength GetInstance(string aaSequence, HlaToLength aHlaToLength, KmerDefinition kmerDefinition) 
        { 
 			SpecialFunctions.CheckCondition(aaSequence.Length > 0 && char.IsUpper(aaSequence[0])); //Spot check that all upper
            MerAndHlaToLength aMerAndHlaToLength = new MerAndHlaToLength(); 
            aMerAndHlaToLength.Mer = aaSequence;
            aMerAndHlaToLength.HlaToLength = aHlaToLength;
            //aMerAndHlaToLength.Study = study;
            aMerAndHlaToLength.KmerDefinition = kmerDefinition;
            return aMerAndHlaToLength;
        } 
 
        private MerAndHlaToLength()
        { 
        }


        internal static KeyValuePair<MerAndHlaToLength, bool> GetRandomInstance(MerAndHlaToLength[] originalTrainingKeysAsArray, bool label, Random random)
        {
 
            MerAndHlaToLength hlaModel = originalTrainingKeysAsArray[random.Next(originalTrainingKeysAsArray.Length)]; 

            MerAndHlaToLength aMerAndHlaToLength = new MerAndHlaToLength(); 
            aMerAndHlaToLength.HlaToLength = hlaModel.HlaToLength;
            //aMerAndHlaToLength.Study = hlaModel.Study;
            aMerAndHlaToLength.KmerDefinition = hlaModel.KmerDefinition;
            int merLength = hlaModel.Mer.Length;

            char[] rgchMer = new char[hlaModel.Mer.Length]; 
            for (int iMer = 0; iMer < rgchMer.Length; ++iMer) 
            {
                MerAndHlaToLength merModel = originalTrainingKeysAsArray[random.Next(originalTrainingKeysAsArray.Length)]; 
                SpecialFunctions.CheckCondition(merLength == merModel.Mer.Length); //!!!raise error - the selection will not be uniform unless all are off the same length
                rgchMer[iMer] = merModel.Mer[random.Next(merLength)];
            }
            aMerAndHlaToLength.Mer = new string(rgchMer);

            KeyValuePair<MerAndHlaToLength, bool> aMerAndHlaToLengthWithLabel 
                = new KeyValuePair<MerAndHlaToLength, bool>(aMerAndHlaToLength, label); 
            return aMerAndHlaToLengthWithLabel;
        } 

        public override string ToString()
        {
            string s = string.Format("{0},{1}", Mer, HlaToLength);
            return s;
        } 
    } 

 

}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
