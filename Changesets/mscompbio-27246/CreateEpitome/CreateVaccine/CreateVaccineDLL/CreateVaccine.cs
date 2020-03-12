using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirusCount;
using System.IO;
using Msr.Mlas.SpecialFunctions; 
using System.Diagnostics; 

namespace CreateVaccine 
{
    public class CreateVaccine
    {
        public static void MakeGreedyEpitomes(TextReader patchTableTextReader, TextWriter streamWriterOutputFile, int stopLength)
        {
            string scorerName = "normal"; 
            PatchPatternFactory patchPatternFactory = PatchPatternFactory.GetFactory("strings"); 
            VaccineMaker vaccineMaker = VaccineMaker.GetInstance("Greedy");
 
            PatchTable patchTable = LoadPatchTable(patchTableTextReader, scorerName, patchPatternFactory);
            patchTable.Normalize();
            AssertPointsTotalOne(patchTable);

            vaccineMaker.DisplayHeader(streamWriterOutputFile);
 
            vaccineMaker.FirstVaccine(patchTable); 
            VaccineAsString vaccineAsString = null;
            while (null != (vaccineAsString = vaccineMaker.VaccineAsString)) 
            {
                double rScoreOpt = patchTable.Score(vaccineAsString);
                vaccineMaker.Display(streamWriterOutputFile, rScoreOpt);
                streamWriterOutputFile.Flush();

                if (vaccineAsString.TotalNumberOfAminoAcids > stopLength) 
                { 
                    break;
                } 

                vaccineMaker.ChangeToNext();
            }

        }

        static public string DisplayHeaderString
        {
            get
            {
                return VaccineMaker.DisplayHeaderString;
            }
        }
 
        public static void MakeGreedyEpitomes(string inputFileName, string outputFileName, int stopLength) 
        {
            using (TextReader textReader = File.OpenText(inputFileName)) 
            {
                SpecialFunctions.CreateDirectoryForFileIfNeeded(outputFileName);
                using (TextWriter textWriter = File.CreateText(outputFileName))
                {
                    MakeGreedyEpitomes(textReader, textWriter, stopLength);
                } 
            } 
        }
 

        static public PatchTable LoadPatchTable(TextReader patchTableTextReader, string scorerName, PatchPatternFactory patchPatternFactory)
        {
            PatchTable patchTable = PatchTable.GetInstanceFromFile(patchPatternFactory, patchTableTextReader, scorerName);
            return patchTable;
        } 
 

        static public void AssertPointsTotalOne(PatchTable patchTable) 
        {
            double rTotal = 0.0;
            foreach (Patch patch in patchTable.SortedPatchCollection)
            {
                rTotal += patch.Weight;
            } 
            Debug.Assert(Math.Abs(rTotal - 1.0) < 1e-10); 
        }


        public static IEnumerable<string> GreedyEpitomeEnumerable(string patchTableAsString)
        {
            //!!!Similar to other code

            string scorerName = "normal";
            PatchPatternFactory patchPatternFactory = PatchPatternFactory.GetFactory("strings");
            VaccineMaker vaccineMaker = VaccineMaker.GetInstance("Greedy");

            PatchTable patchTable = LoadPatchTable(patchTableAsString, scorerName, patchPatternFactory);
            patchTable.Normalize();
            AssertPointsTotalOne(patchTable);

            //yield return VaccineMaker.DisplayHeaderString;

            vaccineMaker.FirstVaccine(patchTable);
            VaccineAsString vaccineAsString = null;
            while (null != (vaccineAsString = vaccineMaker.VaccineAsString))
            {
                double rScoreOpt = patchTable.Score(vaccineAsString);
                yield return vaccineMaker.DisplayString(rScoreOpt);
                vaccineMaker.ChangeToNext();
            }

        }

        private static PatchTable LoadPatchTable(string patchTableAsString, string scorerName, PatchPatternFactory patchPatternFactory)
        {
            using (TextReader textReader = new StringReader(patchTableAsString))
            {
                PatchTable patchTable = PatchTable.GetInstanceFromFile(patchPatternFactory, textReader, scorerName);
                return patchTable;
            }
        }
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
