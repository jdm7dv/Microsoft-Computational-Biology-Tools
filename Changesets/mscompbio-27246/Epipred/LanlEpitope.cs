using System; 
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
//using Msr.Mlas.LRWrap;
using System.Collections.Generic; 
using Msr.Mlas.SpecialFunctions; 
using EpipredLib;
 
namespace VirusCount
{
    public class LanlEpitope
    {

        private static System.Collections.Generic.IEnumerable<HlaToLength> HlaCollection(string hlaCollection, HlaResolution hlaResolution) 
        { 
            foreach (string sHlaPattern in hlaCollection.Split('/'))
            { 
                HlaToLength hlaToLength = HlaToLength.GetInstanceOrNull(sHlaPattern, hlaResolution);
                SpecialFunctions.CheckCondition(hlaToLength != null); //!!!raise error
                yield return hlaToLength;
            }
        }
 
 
        private LanlEpitope()
        { 
            Source = this;
        }

        public double CaseWeight = 1.0;
        public LanlEpitope Source;
        public string Operator = ""; 
        public string Epitope; 
        public HlaToLength HlaToLength;
        public ProteinPosition ProteinPositionNotCanonical; 


        //		public ProteinPosition ProteinPositionProjectionOrNull(Study study, string proteinToProjectOn)
        //		{
        //			ProteinPosition aProteinPosition = ProteinPosition.Project(ProteinPositionNotCanonical, proteinToProjectOn, study);
        //			return aProteinPosition; 
        //		} 

        private string SpeciesPattern; 
        //		private int ExtraLength(int eLength)
        //		{
        //			return Epitope.Length - eLength;
        //		}

 
        public string TrimToMaxLengthFromRight(int maxLength) 
        {
            if (Epitope.Length <= maxLength) 
            {
                return Epitope;
            }
            else
            {
                string s = Epitope.Substring(Epitope.Length - (int)maxLength); 
                return s; 
            }
        } 
        public string TrimmedEpitopeOrNull(int EpitopeMerCount)
        {
            if (Epitope.Length > EpitopeMerCount)
            {
                string s = Epitope.Substring((int)(Epitope.Length - EpitopeMerCount), (int)EpitopeMerCount);
                return s; 
            } 
            else if (Epitope.Length == EpitopeMerCount)
            { 
                return Epitope;
            }
            else
            {
                return null;
            } 
        } 

        private static string LanlFileNormalizedOutput(string fileSuffix, HlaResolution hlaResolution) 
        {
            SpecialFunctions.CheckCondition(fileSuffix == "Lanl"); //!!!raise error
            string sInputFileName = string.Format(@"lanl-{0}digit.Normalized.new.txt", hlaResolution.ShortName); //!!!const
            return sInputFileName;
        }
 
        static private string LanlFile(string fileSuffix, HlaResolution hlaResolution) 
        {
 
            SpecialFunctions.CheckCondition(fileSuffix == "ignore" || fileSuffix == "" || fileSuffix == "Old" || fileSuffix == "Lanl" || fileSuffix == "8910" || fileSuffix == "8910NoExpand" || fileSuffix == "8910SomeExpand" || fileSuffix == "8910Both"); //!!!raise error

            string sInputFileName;
            if (fileSuffix == "Old")
            {
                sInputFileName = string.Format(@"lanlOld.csv"); //!!!const 
            } 
            else
            { 
                sInputFileName = string.Format(@"lanl-{0}digit.csv", hlaResolution.ShortName); //!!!const
            }


            return sInputFileName;
        } 
 

 

        static private ArrayList FilterAndDuplicateAccordingToSuffix(ArrayList lanlEpitopeCollection, string fileSuffix)
        {
            //!!!switch to switch
            if (fileSuffix == "" || fileSuffix == "Old")
            { 
                return lanlEpitopeCollection; 
            }
 
            if (fileSuffix == "Lanl")
            {
                ArrayList rgOut = new ArrayList();
                foreach (LanlEpitope aLanlEpitope in lanlEpitopeCollection)
                {
                    if (9 == aLanlEpitope.Epitope.Length) 
                    { 
                        rgOut.Add(aLanlEpitope);
                    } 
                }
                return rgOut;
            }
            if (fileSuffix == "8910NoExpand")
            {
                ArrayList rgOut = new ArrayList(); 
                foreach (LanlEpitope aLanlEpitope in lanlEpitopeCollection) 
                {
                    if (8 <= aLanlEpitope.Epitope.Length && aLanlEpitope.Epitope.Length <= 10) 
                    {
                        rgOut.Add(aLanlEpitope);
                    }
                }
                return rgOut;
            } 
            if (fileSuffix == "8910SomeExpand") 
            {
                ArrayList rgOut = new ArrayList(); 
                foreach (LanlEpitope aLanlEpitope in lanlEpitopeCollection)
                {
                    switch (aLanlEpitope.Epitope.Length)
                    {
                        case 8:
                            { 
                                ArrayList rgExpanded = MakeOneLongerAtE1(aLanlEpitope); 
                                rgOut.AddRange(rgExpanded);
                            } 
                            break;
                        case 9:
                            {
                                Debug.Assert(aLanlEpitope.CaseWeight == 1.0); // real assert
                                rgOut.Add(aLanlEpitope);
                            } 
                            break; 
                        case 10:
                            { 
                                ArrayList rgExpanded = MakeOneShorterAt4thTo8th(aLanlEpitope);
                                rgOut.AddRange(rgExpanded);
                            }
                            break;
                        default:
                            { 
                                //Skip 
                            }
                            break; 
                    }
                }
                return rgOut;
            }
            if (fileSuffix == "8910Both")
            { 
 
                ArrayList rgOut = new ArrayList();
                foreach (LanlEpitope aLanlEpitope in lanlEpitopeCollection) 
                {
                    switch (aLanlEpitope.Epitope.Length)
                    {
                        case 8:
                            {
                                ArrayList rgExpanded = MakeOneLonger(aLanlEpitope); 
                                rgOut.AddRange(rgExpanded); 
                                aLanlEpitope.Operator = "X8";
                                Debug.Assert(aLanlEpitope.CaseWeight == 1.0); // real assert 
                                rgOut.Add(aLanlEpitope);
                            }
                            break;
                        case 9:
                            {
                                Debug.Assert(aLanlEpitope.CaseWeight == 1.0); // real assert 
                                rgOut.Add(aLanlEpitope); 
                            }
                            break; 
                        case 10:
                            {
                                ArrayList rgExpanded = MakeOneShorter(aLanlEpitope);
                                rgOut.AddRange(rgExpanded);
                                aLanlEpitope.Operator = "X10";
                                Debug.Assert(aLanlEpitope.CaseWeight == 1.0); // real assert 
                                rgOut.Add(aLanlEpitope); 
                            }
                            break; 
                        default:
                            {
                                //Skip
                            }
                            break;
                    } 
                } 
                return rgOut;
            } 

            if (fileSuffix == "8910")
            {

                ArrayList rgOut = new ArrayList();
                foreach (LanlEpitope aLanlEpitope in lanlEpitopeCollection) 
                { 
                    switch (aLanlEpitope.Epitope.Length)
                    { 
                        case 8:
                            {
                                ArrayList rgExpanded = MakeOneLonger(aLanlEpitope);
                                rgOut.AddRange(rgExpanded);
                            }
                            break; 
                        case 9: 
                            {
                                Debug.Assert(aLanlEpitope.CaseWeight == 1.0); // real assert 
                                rgOut.Add(aLanlEpitope);
                            }
                            break;
                        case 10:
                            {
                                ArrayList rgExpanded = MakeOneShorter(aLanlEpitope); 
                                rgOut.AddRange(rgExpanded); 
                            }
                            break; 
                        default:
                            {
                                //Skip
                            }
                            break;
                    } 
                } 
                return rgOut;
            } 
            else
            {
                Debug.Fail("Unknown suffix");
                return null;
            }
        } 
 

        static private string RemoveCharFromString(string s, int iPos) 
        {
            string sOut = s.Remove((int)iPos, 1);
            return sOut;
        }

        static private string AddCharToString(string s, int iPos, string ch) 
        { 
            string sOut = s.Insert((int)iPos, ch);
            return sOut; 
        }



        static private ArrayList MakeOneShorterAt4thTo8th(LanlEpitope modelLanlEpitope)
        { 
            Debug.Assert(modelLanlEpitope.Epitope.Length == 10); 

            ArrayList rgExpanded = new ArrayList(); 
            for (int iPos = 3; iPos <= 8; ++iPos)
            {
                LanlEpitope aLanlEpitope = new LanlEpitope();
                aLanlEpitope.Epitope = RemoveCharFromString(modelLanlEpitope.Epitope, iPos);
                aLanlEpitope.ProteinPositionNotCanonical = modelLanlEpitope.ProteinPositionNotCanonical;
                aLanlEpitope.SpeciesPattern = modelLanlEpitope.SpeciesPattern; 
                aLanlEpitope.HlaToLength = modelLanlEpitope.HlaToLength; 
                aLanlEpitope.CaseWeight = 1.0 / 6.0;
                aLanlEpitope.Source = modelLanlEpitope; 
                aLanlEpitope.Operator = string.Format("D{0}", iPos + 1);
                rgExpanded.Add(aLanlEpitope);
            }

            return rgExpanded;
        } 
 
        static private ArrayList MakeOneShorter(LanlEpitope modelLanlEpitope)
        { 
            ArrayList rgExpanded = new ArrayList();


            for (int iPos = 0; iPos < modelLanlEpitope.Epitope.Length; ++iPos)
            {
                LanlEpitope aLanlEpitope = new LanlEpitope(); 
                aLanlEpitope.Epitope = RemoveCharFromString(modelLanlEpitope.Epitope, iPos); 
                aLanlEpitope.ProteinPositionNotCanonical = modelLanlEpitope.ProteinPositionNotCanonical;
                aLanlEpitope.SpeciesPattern = modelLanlEpitope.SpeciesPattern; 
                aLanlEpitope.HlaToLength = modelLanlEpitope.HlaToLength;
                aLanlEpitope.CaseWeight = 1.0 / (double)modelLanlEpitope.Epitope.Length;
                aLanlEpitope.Operator = string.Format("D{0}", iPos + 1);
                aLanlEpitope.Source = modelLanlEpitope;
                rgExpanded.Add(aLanlEpitope);
            } 
 
            return rgExpanded;
        } 

        //		static private void AddToHashtable(LanlEpitope aLanlEpitope, ref Hashtable rgExpanded)
        //		{
        //			if (rgExpanded.ContainsKey(aLanlEpitope.Epitope))
        //			{
        //				LanlEpitope previous = (LanlEpitope) rgExpanded[aLanlEpitope.Epitope]; 
        //				previous.CaseWeight += aLanlEpitope.CaseWeight; 
        //
        //			} 
        //			else
        //			{
        //				rgExpanded.Add(aLanlEpitope.Epitope, aLanlEpitope);
        //			}
        //	}
 
        static private ArrayList MakeOneLongerAtE1(LanlEpitope modelLanlEpitope) 
        {
            Debug.Assert(modelLanlEpitope.Epitope.Length == 8); 

            ArrayList rgExpanded = new ArrayList();

            int iPos = 0;
            LanlEpitope aLanlEpitope = new LanlEpitope();
            aLanlEpitope.Epitope = AddCharToString(modelLanlEpitope.Epitope, iPos, "#"); 
            aLanlEpitope.ProteinPositionNotCanonical = modelLanlEpitope.ProteinPositionNotCanonical; 
            aLanlEpitope.SpeciesPattern = modelLanlEpitope.SpeciesPattern;
            aLanlEpitope.HlaToLength = modelLanlEpitope.HlaToLength; 
            aLanlEpitope.CaseWeight = 1.0;
            aLanlEpitope.Source = modelLanlEpitope;
            aLanlEpitope.Operator = string.Format("I{0}", iPos + 1);
            rgExpanded.Add(aLanlEpitope);

            return rgExpanded; 
        } 

        static private ArrayList MakeOneLonger(LanlEpitope modelLanlEpitope) 
        {
            ArrayList rgExpanded = new ArrayList();

            for (int iPos = 0; iPos <= modelLanlEpitope.Epitope.Length; ++iPos)
            {
                LanlEpitope aLanlEpitope = new LanlEpitope(); 
                aLanlEpitope.Epitope = AddCharToString(modelLanlEpitope.Epitope, iPos, "#"); 
                aLanlEpitope.ProteinPositionNotCanonical = modelLanlEpitope.ProteinPositionNotCanonical;
                aLanlEpitope.SpeciesPattern = modelLanlEpitope.SpeciesPattern; 
                aLanlEpitope.HlaToLength = modelLanlEpitope.HlaToLength;
                aLanlEpitope.CaseWeight = 1.0 / ((double)modelLanlEpitope.Epitope.Length + 1.0);
                aLanlEpitope.Source = modelLanlEpitope;
                aLanlEpitope.Operator = string.Format("I{0}", iPos + 1);
                rgExpanded.Add(aLanlEpitope);
            } 
 
            return rgExpanded;
        } 

        static string PreferedHeader = "Epitope\tProtein\tHxb2locstart\tHxb2locend\tSpecies\tHla\tlength";

        internal static void ReportNormalizedInputWithNoDup(ArrayList lanlEpitopeCollection, string fileSuffix, HlaResolution hlaResolution)
        {
            string outputFileName = LanlFileNormalizedOutput(fileSuffix, hlaResolution); 
            using (StreamWriter streamwriterOutputFile = File.CreateText(outputFileName)) 
            {
                streamwriterOutputFile.WriteLine(PreferedHeader); 
                foreach (LanlEpitope lanlEpitope in lanlEpitopeCollection)
                {
                    string line = lanlEpitope.Key();
                    streamwriterOutputFile.WriteLine(line);
                }
            } 
        } 

        //!!!could make a fast key 
        private string Key()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                        Epitope, ProteinPositionNotCanonical.Protein, ProteinPositionNotCanonical.AminoAcidBase1,
                        ProteinPositionNotCanonical.AminoAcidBase1 + Epitope.Length - 1, SpeciesPattern, HlaToLength,
                        Epitope.Length); 
        } 

        static public ArrayList GetLanlCollection(string fileSuffix, HlaResolution hlaResolution) 
        {

            //Load up known epitopes
            ArrayList lanlEpitopeCollection = new ArrayList();

            if (fileSuffix == "ignore") 
            { 
                return lanlEpitopeCollection;
             } 

            string sInputFileName = LanlFile(fileSuffix, hlaResolution);

            Dictionary<string, bool> rgSeenIt = new Dictionary<string, bool>();

            using (StreamReader streamreaderInputFile = Predictor.OpenResource(sInputFileName)) 
            { 
                string sLine = streamreaderInputFile.ReadLine();
                SpecialFunctions.CheckCondition(sLine == PreferedHeader || sLine == @"Epitope,Protein,Hxb2locstart,Hxb2locend,Species,HLA" || sLine == @"Epitope,Protein,Hxb2locstart,Hxb2locend,Species,Hla,length"); //!!!raise error 
                while ((sLine = streamreaderInputFile.ReadLine()) != null)
                {
                    if (sLine == "")
                    {
                        continue;
                    } 
 
                    string[] rgField = sLine.Split(',', '\t');
                    SpecialFunctions.CheckCondition(rgField.Length == 7 || rgField.Length == 6); //!!!raise error 

                    string aaSequence = rgField[0];
                    SpecialFunctions.CheckCondition(aaSequence.IndexOf('?') < 0); //!!!raise error

                    if (rgField.Length == 7)
                    { 
                        int iLength = int.Parse(rgField[6]); 
                        SpecialFunctions.CheckCondition(aaSequence.Length == iLength); //!!!raise error
                    } 

                    string hlaCollection = rgField[5];
                    SpecialFunctions.CheckCondition(hlaCollection != ""); //!!!raise error

                    string species = rgField[4];
                    if (species.IndexOf("human") < 0) 
                    { 
                        Debug.WriteLine("Skipping non-human epitope: " + species);
                        continue; 
                    }

                    foreach (HlaToLength aHlaToLength in HlaCollection(hlaCollection, hlaResolution))
                    {
                        LanlEpitope aLanlEpitope = new LanlEpitope();
                        aLanlEpitope.Epitope = aaSequence; //!!!const 
                        string sProtein = rgField[1]; //!!!const 
                        int aa1 = int.Parse(rgField[2]); //!!!could raise error //!!!const
                        aLanlEpitope.ProteinPositionNotCanonical = new ProteinPosition(sProtein, aa1); 
                        //!!!add an assert about the species pattern being one that we can work with
                        aLanlEpitope.SpeciesPattern = "human";
                        //!!!would be good to assert that this matches pattern - no "HLA*" or " "
                        aLanlEpitope.HlaToLength = aHlaToLength;

                        string key = aLanlEpitope.Key(); 
                        if (!rgSeenIt.ContainsKey(key)) 
                        {
                            rgSeenIt.Add(key, true); 
                            lanlEpitopeCollection.Add(aLanlEpitope);
                        }
                    }
                }
            }
 
            LanlEpitope.ReportNormalizedInputWithNoDup(lanlEpitopeCollection, fileSuffix, hlaResolution); 

 
            lanlEpitopeCollection = FilterAndDuplicateAccordingToSuffix(lanlEpitopeCollection, fileSuffix);

            DumpCollection(fileSuffix, hlaResolution, lanlEpitopeCollection);

            return lanlEpitopeCollection;
        } 
 

        static public void DumpCollection(string fileSuffix, HlaResolution hlaResolution, ArrayList lanlEpitopeCollection) 
        {
            string sOutputFile = string.Format(@"LanlEpitopeCollection{0}{1}.New.txt", fileSuffix, hlaResolution.ShortName);
            using (StreamWriter streamwriterOutputFile = File.CreateText(sOutputFile))
            {
                LanlEpitope.WriteHeaderLine(streamwriterOutputFile);
                foreach (LanlEpitope aLanlEpitope in lanlEpitopeCollection) 
                { 
                    aLanlEpitope.WriteLine(streamwriterOutputFile);
                } 
            }
        }

        private static void WriteHeaderLine(StreamWriter streamwriterOutputFile)
        {
            streamwriterOutputFile.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", 
                "CaseWeight", "Epitope", "HlaPatternCollection", "Operator", "ProteinPositionNotCanonical", "Source!=null", "SpeciesPattern"); 
        }
 
        private void WriteLine(StreamWriter streamwriterOutputFile)
        {
            streamwriterOutputFile.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                CaseWeight, Epitope, HlaToLength, Operator, ProteinPositionNotCanonical, Source != null ? "True" : "False", SpeciesPattern);
        }
 
 
        //		public bool LengthIsOK(int EpitopeMerCount)
        //		{ 
        //			bool b = Epitope.Length > EpitopeMerCount;
        //			return b;
        //		}


    } 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
