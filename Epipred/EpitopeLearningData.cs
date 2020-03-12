using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using EpipredLib; 
using ProcessingPrediction; 
using VirusCount.Qmrr;
 
namespace VirusCount
{
    public class EpitopeLearningData : Dictionary<Pair<string, Hla>, bool>
    {
        internal EpitopeLearningData(string name)
            : base() 
        { 
            Name = name;
        } 

        public string Name;

        public static EpitopeLearningData Empty = new EpitopeLearningData("Empty");

        //internal static EpitopeLearningData GetSyfpeithiCollection(HlaResolution hlaResolution, KmerDefinition kmerDefinition) 
        //{ 
        //    Debug.Assert(kmerDefinition.FullMerCount == kmerDefinition.EpitopeMerCount); //!!!this is all we have code for
        //    Debug.Assert(8 <= kmerDefinition.EpitopeMerCount && kmerDefinition.EpitopeMerCount <= 11); 
        //    Debug.Assert(hlaResolution is ABMixedHlaResolution); //!!!this is all we have code for

        //    EpitopeLearningData rg = new EpitopeLearningData("Syfpeithi");

        //    string fileInput = @"W_2005-04-05_09_57_55_syfpeithiClean.txt";//!!!const
 
        //    using (StreamReader streamreader = Predictor.OpenResource(fileInput)) 
        //    {
        //        string line = streamreader.ReadLine(); 
        //        Debug.Assert(line == "Epitope ID	Epitope Sequence	HLA	kMerLength	Anchor 1	Anchor 2	Anchor 3");
        //        while (null != (line = streamreader.ReadLine()))
        //        {
        //            string[] fieldCollection = line.Split('\t');
        //            SpecialFunctions.CheckCondition(fieldCollection.Length >= 4); //!!!raise error
 
 
        //            string aaSequence = fieldCollection[1].Trim();
        //            if (aaSequence.Length != kmerDefinition.FullMerCount) 
        //            {
        //                continue;
        //            }
        //            if (aaSequence.Contains("X"))
        //            {
        //                continue; 
        //            } 

 
        //            string hlaAsString = fieldCollection[2].Trim();

        //            HlaToLength hlaToLength = HlaToLength.GetInstance(hlaAsString, hlaResolution);

        //            MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(aaSequence, hlaToLength, kmerDefinition);
 
        //            rg[aMerAndHlaToLength] = true; 
        //        }
        //    } 
        //    return rg;
        //}

        //internal static EpitopeLearningData GetMhcPepCollection(HlaResolution hlaResolution, KmerDefinition kmerDefinition)
        //{
        //    Debug.Fail("The MHC Pep data needs it's A68's expanded before it can be used"); 
        //    Debug.Assert(kmerDefinition.EpitopeMerCount == 9 && kmerDefinition.FullMerCount == 9); //!!!this is the case we have code for 
        //    EpitopeLearningData rg = new EpitopeLearningData("MhcPep");
        //    string fileInput = @"W_2005-04-05_11_43_49_MHCPEPcleanDuplicatesRemoved.txt"; //!!!const 

        //    using (StreamReader streamreader = Predictor.OpenResource(fileInput))
        //    {
        //        string line = streamreader.ReadLine();
        //        Debug.Assert(line == "Epitopes	Original HLA (4 digit where possible)	Our Canonical HLA	Length");
        //        while (null != (line = streamreader.ReadLine())) 
        //        { 
        //            string[] fieldCollection = line.Split('\t');
        //            SpecialFunctions.CheckCondition(fieldCollection.Length == 4); //!!!raise error 


        //            string aaSequence = fieldCollection[0].Trim();
        //            if (aaSequence.Length != kmerDefinition.FullMerCount)
        //            {
        //                continue; 
        //            } 

 
        //            string hlaAsString = fieldCollection[2].Trim();
        //            if (hlaAsString == "B15" || hlaAsString == "A68")
        //            {
        //                Debug.WriteLine("Skipping 2-digit B15 and A68's");
        //                continue;
        //            } 
        //            HlaToLength hlaToLength = HlaToLength.GetInstanceABMixed(hlaAsString); 

        //            int length = int.Parse(fieldCollection[3]); 
        //            SpecialFunctions.CheckCondition(length == aaSequence.Length); //!!!raise error

        //            MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(aaSequence, hlaToLength, kmerDefinition);
        //            rg[aMerAndHlaToLength] = true;
        //        }
        //    } 
        //    return rg; 
        //}
 
        public static EpitopeLearningData Union(params EpitopeLearningData[] rgrg)
        {
            EpitopeLearningData rgOut = new EpitopeLearningData(null);
            StringBuilder sb = new StringBuilder();
            foreach (EpitopeLearningData rgIn in rgrg)
            { 
                if (sb.Length > 0) 
                {
                    sb.Append('+'); 
                }
                sb.Append(rgIn.Name);
                foreach (KeyValuePair<Pair<string, Hla>, bool> aMerAndHlaToLengthWithLabel in rgIn)
                {
                    rgOut[aMerAndHlaToLengthWithLabel.Key] = aMerAndHlaToLengthWithLabel.Value;
                } 
            } 
            rgOut.Name = sb.ToString();
            return rgOut; 
        }

        internal EpitopeLearningData Union(EpitopeLearningData other)
        {
            return Union(this, other);
        } 
 
        //internal static EpitopeLearningData GetDurbanBostonNegativeCollection(string fileSuffix, KmerDefinition kmerDefinition, EpitopeLearningData positiveExampleNotToInclude)
        //{ 

        //    string fileInput = GetNegativeFileName(fileSuffix);


        //    EpitopeLearningData rg = new EpitopeLearningData(fileSuffix);
 
        //    using (StreamReader streamreader = Predictor.OpenResource(fileInput)) 
        //    {
        //        string line = streamreader.ReadLine(); 
        //        Debug.Assert(line == "Turned On	 Peptide #	 Peptide Sequence	 HLA	 Confidence Score	 In A-list	 Log Weight of Anchor Evidence"
        //            || line == "Turned On\tPeptide #\tPeptide Sequence\tHLA\tConfidence Score\tIn A-List\tHLA In A-list\tLog Weight of Anchor Evidence");
        //        while (null != (line = streamreader.ReadLine()))
        //        {
        //            string[] fieldCollection = line.Split('\t');
        //            SpecialFunctions.CheckCondition(fieldCollection.Length == 7 || fieldCollection.Length == 8); //!!!raise error 
 
        //            SpecialFunctions.CheckCondition(fieldCollection[0] == "0" || fieldCollection[0] == "1"); //!!!raise error
        //            if (fieldCollection[0] == "1") 
        //            {
        //                continue;
        //            }

        //            string aaSequence = fieldCollection[2].Trim();
 
        //            string hlaAsString = fieldCollection[3].Trim(); 

        //            if (hlaAsString == "B15" || hlaAsString == "A68") 
        //            {
        //                continue;
        //            }

        //            HlaToLength hlaToLength = HlaToLength.GetInstanceABMixed(hlaAsString);
 
 
        //            foreach (string sMer in SubstringCollection(aaSequence, kmerDefinition.FullMerCount))
        //            { 
        //                MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(sMer, hlaToLength, kmerDefinition);
        //                if (!positiveExampleNotToInclude.ContainsKey(aMerAndHlaToLength))
        //                {
        //                    rg[aMerAndHlaToLength] = false;
        //                }
        //            } 
        //        } 
        //    }
        //    return rg; 
        //}
        //internal static EpitopeLearningData GetNegativeCollection(string fileSuffix, KmerDefinition kmerDefinition, EpitopeLearningData positiveExampleNotToInclude)
        //{

        //    string fileInput = GetNegativeFileName(fileSuffix);
 
 
        //    EpitopeLearningData rg = new EpitopeLearningData(fileSuffix);
 
        //    using (StreamReader streamreader = Predictor.OpenResource(fileInput))
        //    {
        //        string line = streamreader.ReadLine();
        //        Debug.Assert(line == "Rank by Confidence	 Peptide #	 Peptide Sequence	 HLA	 Confidence Score	 In A-list	 Log Weight of Anchor Evidence");
        //        while (null != (line = streamreader.ReadLine()))
        //        { 
        //            string[] fieldCollection = line.Split('\t'); 
        //            SpecialFunctions.CheckCondition(fieldCollection.Length == 7); //!!!raise error
 

        //            string aaSequence = fieldCollection[2].Trim();

        //            string hlaAsString = fieldCollection[3].Trim();

        //            HlaToLength hlaToLength = HlaToLength.GetInstanceABMixed(hlaAsString); 
 
        //            foreach (string sMer in SubstringCollection(aaSequence, kmerDefinition.FullMerCount))
        //            { 
        //                MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(sMer, hlaToLength, kmerDefinition);
        //                if (!positiveExampleNotToInclude.ContainsKey(aMerAndHlaToLength))
        //                {
        //                    rg[aMerAndHlaToLength] = false;
        //                }
        //            } 
        //        } 
        //    }
        //    return rg; 
        //}

        static public IEnumerable<string> SubstringCollection(string s, int length)
        {
            for (int start = 0; start <= s.Length - length; ++start)
            { 
                yield return s.Substring(start, (int)length); 
            }
        } 


        private static string GetNegativeFileName(string fileSuffix)
        {
            string fileInput;
            //!!!switch to switch 
            if (fileSuffix == "BostonOlp") 
            {
                fileInput = @"W_2005-04-22_10_31_02_BostonOLP_F1_D3_C1_M3_A1_FinalSolution.txt"; 
            }
            else if (fileSuffix == "DurbanOlp")
            {
                fileInput = @"W_2005-04-23_13_50_50_DurbanOLP_F1_D4_C1_M3_A1_FinalSolutions.txt";

            } 
            else if (fileSuffix == "HivOptimals") 
            {
                fileInput = @"W_2005-04-15_16_22_41_offOnly_HarvardHIVOptimals_Flip_F1rankList.txt"; 
            }
            else
            {
                Debug.Fail("unknown " + fileSuffix);
                fileInput = null;
            } 
            return fileInput; 
        }
 

        internal EpitopeLearningData Subtract(EpitopeLearningData other)
        {
            EpitopeLearningData rgOut = new EpitopeLearningData(string.Format("{0}-{1}", Name, other.Name));
            foreach (Pair<string, Hla> aMerAndHlaToLength in Keys)
            { 
                if (!other.ContainsKey(aMerAndHlaToLength)) 
                {
                    rgOut[aMerAndHlaToLength] = this[aMerAndHlaToLength]; 
                }
            }
            return rgOut;
        }

        //internal EpitopeLearningData[] Split(int cCrossValPart, Random aRandom) 
        //{ 
        //    List<KeyValuePair<MerAndHlaToLength, bool>> shuffleList = new List<KeyValuePair<MerAndHlaToLength, bool>>();
        //    foreach (KeyValuePair<Pair<string, Hla>, bool> merAndHlaToLengthWithLabel in this) 
        //    {
        //        shuffleList.Add(merAndHlaToLengthWithLabel);
        //        int iRandomPos = aRandom.Next(shuffleList.Count);
        //        shuffleList[shuffleList.Count - 1] = shuffleList[iRandomPos];
        //        shuffleList[iRandomPos] = merAndHlaToLengthWithLabel;
        //    } 
 
        //    EpitopeLearningData[] rgrg = new EpitopeLearningData[cCrossValPart];
        //    for (int irgrg = 0; irgrg < rgrg.Length; ++irgrg) 
        //    {
        //        rgrg[irgrg] = new EpitopeLearningData(string.Format("{0}{1}", Name, irgrg));
        //    }
        //    for (int iShuffleList = 0; iShuffleList < shuffleList.Count; ++iShuffleList)
        //    {
        //        KeyValuePair<MerAndHlaToLength, bool> merAndHlaToLengthWithLabel = shuffleList[iShuffleList]; 
        //        int iSet = iShuffleList * cCrossValPart / shuffleList.Count; 
        //        rgrg[iSet].Add(merAndHlaToLengthWithLabel.Key, merAndHlaToLengthWithLabel.Value);
        //    } 
        //    return rgrg;
        //}


        public static EpitopeLearningData GetDbWhole(HlaFactory hlaFactory, int eLength, string datasetName, string fileOrResourceName)
        { 
            Set<string> wantedSet = CreateSourceSet(datasetName); 
            EpitopeLearningData rg = new EpitopeLearningData(datasetName);
 
            //SpecialFunctions.CheckCondition(hlaResolution.Equals(HlaResolution.ABMixed));
            foreach (Dictionary<string, string> row in Predictor.TabFileTable(fileOrResourceName, "peptide	hla	source	label", false))
            {
                string peptide = row["peptide"];
                SpecialFunctions.CheckCondition(Biology.GetInstance().LegalPeptide(peptide), string.Format("Peptide, '{0}', contains illegal char.", peptide));
 
                if (peptide.Length != eLength) //!!!const 
                {
                    continue; 
                }

                string source = row["source"];
                Set<string> providedSet = CreateSourceSet(source);
                //Debug.Assert(providedSet.IsSubsetOf(Set<string>.GetInstance(new string[] { "Aplus", "LANL", "IEDB" }))); // real assert
                if (providedSet.IntersectionIsEmpty(wantedSet)) 
                { 
                    continue;
                } 


                Hla hla = hlaFactory.GetGroundInstance(row["hla"]);
                //HlaToLength hlaToLength = HlaToLength.GetInstance(hla, hlaResolution);
                Pair<string, Hla> peptideAndHla = new Pair<string, Hla>(peptide, hla);
                //MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(peptide, hlaToLength, kmerDefinition); 
 
                string label = row["label"];
                SpecialFunctions.CheckCondition(label == "0" || label == "1", string.Format("Warning: Epitope example {0} has unknown label {1} and will be ignored.", peptideAndHla, label)); 
                rg[peptideAndHla] = (label == "1");
            }

            return rg;

        } 
 
        private static Set<string> CreateSourceSet(string datasetName)
        { 
            Set<string> sourceSet = Set<string>.GetInstance();
            foreach (string source in datasetName.Split('+',','))
            {
                sourceSet.AddNew(source.ToLower());
            }
            return sourceSet; 
        } 

        private static System.Collections.Generic.IEnumerable<HlaToLength> HlaCollection(string hlaCollection, HlaResolution hlaResolution) 
        {
            foreach (string sHlaPattern in hlaCollection.Split('/'))
            {
                HlaToLength hlaToLength = HlaToLength.GetInstanceOrNull(sHlaPattern, hlaResolution);
                SpecialFunctions.CheckCondition(hlaToLength != null); //!!!raise error
                yield return hlaToLength; 
            } 
        }
 
        static private string TrimToMaxLengthFromRight(string aaSequence, int maxLength)
        {
            if (aaSequence.Length <= maxLength)
            {
                return aaSequence;
            } 
            else 
            {
                string s = aaSequence.Substring(aaSequence.Length - (int)maxLength); 
                return s;
            }
        }


        private static string FillInKmerOrNull(string aaSequence, string sProtein, int aa1, KmerDefinition kmerDefinition) 
        { 
            SpecialFunctions.CheckCondition(kmerDefinition.BeforeMerCount == 0 && kmerDefinition.AfterMerCount == 0, "look at old code to see how to handle other cases");
            if (aaSequence.Length == kmerDefinition.FullMerCount) 
            {
                return aaSequence;
            }
            else
            {
                return null; 
            } 
        }
 
        ////!!!shares much code with GetDurbanBostonNegativeCollection
        //internal static Dictionary<MerAndHlaToLength,double> GetDurbanBostonNegativeConfidenceCollection(string fileSuffix, KmerDefinition kmerDefinition, EpitopeLearningData positiveExampleNotToInclude)
        //{

        //    string fileInput = GetNegativeFileName(fileSuffix);
 
 
        //    Dictionary<MerAndHlaToLength, double> rg = new Dictionary<MerAndHlaToLength, double>();
 
        //    using (StreamReader streamreader = Predictor.OpenResource(fileInput))
        //    {
        //        string line = streamreader.ReadLine();
        //        Debug.Assert(line == "Turned On	 Peptide #	 Peptide Sequence	 HLA	 Confidence Score	 In A-list	 Log Weight of Anchor Evidence"
        //            || line == "Turned On\tPeptide #\tPeptide Sequence\tHLA\tConfidence Score\tIn A-List\tHLA In A-list\tLog Weight of Anchor Evidence");
        //        while (null != (line = streamreader.ReadLine())) 
        //        { 
        //            string[] fieldCollection = line.Split('\t');
        //            SpecialFunctions.CheckCondition(fieldCollection.Length == 7 || fieldCollection.Length == 8); //!!!raise error 

        //            SpecialFunctions.CheckCondition(fieldCollection[0] == "0" || fieldCollection[0] == "1"); //!!!raise error
        //            if (fieldCollection[0] == "1")
        //            {
        //                continue;
        //            } 
 
        //            string aaSequence = fieldCollection[2].Trim();
 
        //            string hlaAsString = fieldCollection[3].Trim();

        //            double confidence = double.Parse(fieldCollection[4].Trim());

        //            if (hlaAsString == "B15")
        //            { 
        //                continue; 
        //            }
 
        //            HlaToLength hlaToLength = HlaToLength.GetInstanceABMixed(hlaAsString);


        //            foreach (string sMer in SubstringCollection(aaSequence, kmerDefinition.FullMerCount))
        //            {
        //                MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(sMer, hlaToLength, kmerDefinition); 
        //                if (!positiveExampleNotToInclude.ContainsKey(aMerAndHlaToLength)) 
        //                {
        //                    if (rg.ContainsKey(aMerAndHlaToLength)) 
        //                    {
        //                        rg[aMerAndHlaToLength] = Math.Max(rg[aMerAndHlaToLength], confidence);
        //                    }
        //                    else
        //                    {
        //                        rg.Add(aMerAndHlaToLength, confidence); 
        //                    } 
        //                }
        //            } 
        //        }
        //    }
        //    return rg;
        //}
        //internal static Dictionary<MerAndHlaToLength, double> GetNegativeConfidenceCollection(string fileSuffix, KmerDefinition kmerDefinition, EpitopeLearningData positiveExampleNotToInclude)
        //{ 
 
        //    string fileInput = GetNegativeFileName(fileSuffix);
 

        //    Dictionary<MerAndHlaToLength, double> rg = new Dictionary<MerAndHlaToLength, double>();

        //    using (StreamReader streamreader = Predictor.OpenResource(fileInput))
        //    {
        //        string line = streamreader.ReadLine(); 
        //        Debug.Assert(line == "Rank by Confidence	 Peptide #	 Peptide Sequence	 HLA	 Confidence Score	 In A-list	 Log Weight of Anchor Evidence"); 
        //        while (null != (line = streamreader.ReadLine()))
        //        { 
        //            string[] fieldCollection = line.Split('\t');
        //            SpecialFunctions.CheckCondition(fieldCollection.Length == 7); //!!!raise error


        //            string aaSequence = fieldCollection[2].Trim();
 
        //            string hlaAsString = fieldCollection[3].Trim(); 

        //            double confidence = double.Parse(fieldCollection[4].Trim()); 


        //            HlaToLength hlaToLength = HlaToLength.GetInstanceABMixed(hlaAsString);

        //            foreach (string sMer in SubstringCollection(aaSequence, kmerDefinition.FullMerCount))
        //            { 
        //                MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(sMer, hlaToLength, kmerDefinition); 
        //                if (!positiveExampleNotToInclude.ContainsKey(aMerAndHlaToLength))
        //                { 
        //                    if (rg.ContainsKey(aMerAndHlaToLength))
        //                    {
        //                        rg[aMerAndHlaToLength] = Math.Max(rg[aMerAndHlaToLength], confidence);
        //                    }
        //                    else
        //                    { 
        //                        rg.Add(aMerAndHlaToLength, confidence); 
        //                    }
        //                } 
        //            }
        //        }
        //    }
        //    return rg;
        //}
 
 

 
        //internal EpitopeLearningData CreateRandomSet(Random random, EpitopeLearningData examplesToAvoid, double factor, bool label)
        //{
        //    EpitopeLearningData randomEpitopeLearningData = new EpitopeLearningData(string.Format("TrainOnRandomNeg{0}/{1}", factor,Name));

        //    MerAndHlaToLength[] originalTrainingKeysAsArray = new MerAndHlaToLength[this.Count];
        //    this.Keys.CopyTo(originalTrainingKeysAsArray, 0); 
        //    //bool[] originalTrainingValuesAsArray = new bool[this.Count]; 
        //    //this.Values.CopyTo(originalTrainingValuesAsArray, 0);
 
        //    for(int iExample = 0; iExample < (double) Count * factor; ++iExample)
        //    {
        //        KeyValuePair<MerAndHlaToLength,bool> randomMerAndHlaToLengthWithLabel;
        //        while(true)
        //        {
        //            randomMerAndHlaToLengthWithLabel = MerAndHlaToLength.GetRandomInstance(originalTrainingKeysAsArray, label, random); 
        //            if (!examplesToAvoid.ContainsKey(randomMerAndHlaToLengthWithLabel.Key) && !randomEpitopeLearningData.ContainsKey(randomMerAndHlaToLengthWithLabel.Key)) 
        //            {
        //                break; 
        //            }
        //        }
        //        randomEpitopeLearningData.Add(randomMerAndHlaToLengthWithLabel.Key, randomMerAndHlaToLengthWithLabel.Value);
        //    }
        //    return randomEpitopeLearningData;
        //} 
 

        ////!!!Reads the file twice, once for positive and once for negative 
        ////!!! it seems that "hlaResolution" is not used by this or similar methods
        //internal static EpitopeLearningData GetMhcbnCollection(HlaResolution hlaResolution, KmerDefinition kmerDefinition, bool wantPositives, bool includeCs)
        //{

        //    Debug.Assert(kmerDefinition.EpitopeMerCount == 9 && kmerDefinition.FullMerCount == 9); //!!!this is the case we have code for
        //    EpitopeLearningData rg = new EpitopeLearningData("Mhcbn" + (wantPositives ? "Positives" : "Negatives")); 
        //    string fileInput = @"mhcbn.txt"; //!!!const 

 
        //    string sample = "ID                          ";
        //    using (StreamReader mhcbnStream = Predictor.OpenResource(fileInput))
        //    {
        //        string aaSequence = null;
        //        string hlaAsString = null;
        //        string bindingAsString = null; 
        //        string line = null; 
        //        while(null != (line = mhcbnStream.ReadLine()))
        //        { 
        //            if (line.StartsWith("  MHCBN:"))
        //            {
        //                continue;
        //            }
        //            if (line == "")
        //            { 
        //                ProcessRecord(hlaResolution, kmerDefinition, wantPositives, includeCs, rg, ref aaSequence, ref hlaAsString, ref bindingAsString); 
        //                continue;
        //            } 

        //            string key = line.Substring(0, sample.Length).TrimEnd();
        //            string value = line.Substring(sample.Length);

        //            //!!!switch to switch
        //            if (key == "MHC") 
        //            { 
        //                hlaAsString = value.Trim();
        //            } 
        //            else if (key == "Sequence")
        //            {
        //                aaSequence = value.Trim().ToUpper();
        //            }
        //            else if (key == "Binding")
        //            { 
        //                bindingAsString = value.Trim().ToUpper(); 
        //            }
        //        } 
        //        if (aaSequence != null)
        //        {
        //            ProcessRecord(hlaResolution, kmerDefinition, wantPositives, includeCs, rg, ref aaSequence, ref hlaAsString, ref bindingAsString);
        //        }
        //        SpecialFunctions.CheckCondition(aaSequence == null);
        //        SpecialFunctions.CheckCondition(hlaAsString == null); 
        //        SpecialFunctions.CheckCondition(bindingAsString == null); 
        //    }
 
        //    return rg;
        //}

        //private static void ProcessRecord(HlaResolution hlaResolution, KmerDefinition kmerDefinition, bool wantPositives, bool includeCs, EpitopeLearningData rg, ref string aaSequence, ref string hlaAsString, ref string bindingAsString)
        //{
        //    SpecialFunctions.CheckCondition(aaSequence != null); 
        //    SpecialFunctions.CheckCondition(hlaAsString != null); 
        //    SpecialFunctions.CheckCondition(bindingAsString != null);
        //    Debug.Assert(bindingAsString == bindingAsString.ToUpper()); // real assert 
        //    SpecialFunctions.CheckCondition(bindingAsString == "NOT DETERMINED" || bindingAsString == "UNDETERMINED,YES" || bindingAsString.StartsWith("YES") || bindingAsString.StartsWith("NO"));


        //    if (aaSequence.Length == kmerDefinition.FullMerCount
        //        && bindingAsString.StartsWith("YES") == wantPositives
        //        && (includeCs || !hlaAsString.Contains("C"))) 
        //    { 
        //        if (aaSequence.Contains("X") || aaSequence.Contains("B") || aaSequence.Contains("Z") || aaSequence.Contains("U"))
        //        { 
        //            Debug.WriteLine("Warning: Skipping sequence with ambiguous amino acid " + aaSequence);
        //        }
        //        else
        //        {
        //            HlaToLength hlaToLength = hlaResolution.GetHlaLengthInstanceWithFixup(hlaAsString);
        //            MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(aaSequence, hlaToLength, kmerDefinition); 
        //            rg[aMerAndHlaToLength] = wantPositives; 
        //        }
        //    } 


        //    aaSequence = null;
        //    hlaAsString = null;
        //    bindingAsString = null;
        //} 
 
        //internal EpitopeLearningData RandomSubset(int goalNegatives, int goalPositives, ref Random random)
        //{ 
        //    //!!!could shuffle in one pass
        //    List<MerAndHlaToLength> negatives = new List<MerAndHlaToLength>();
        //    List<MerAndHlaToLength> positives = new List<MerAndHlaToLength>();
        //    foreach(KeyValuePair<MerAndHlaToLength,bool> merAndHlaToLengthAndLabel in this)
        //    {
        //        if (merAndHlaToLengthAndLabel.Value) 
        //        { 
        //            positives.Add(merAndHlaToLengthAndLabel.Key);
        //        } 
        //        else
        //        {
        //            negatives.Add(merAndHlaToLengthAndLabel.Key);
        //        }
        //    }
        //    SpecialFunctions.CheckCondition(negatives.Count >= goalNegatives); 
        //    SpecialFunctions.CheckCondition(positives.Count >= goalPositives); 

        //    List<MerAndHlaToLength> shuffledNegatives = SpecialFunctions.Shuffle<MerAndHlaToLength>(negatives, ref random); 
        //    List<MerAndHlaToLength> shuffledPositives = SpecialFunctions.Shuffle<MerAndHlaToLength>(positives, ref random);


        //    EpitopeLearningData aEpitopeLearningData = new EpitopeLearningData(string.Format("{0}RandomSubset({1},{2})", Name, goalNegatives, goalPositives));
        //    for (int i = 0; i < goalNegatives; ++i)
        //    { 
        //        aEpitopeLearningData.Add(shuffledNegatives[i], false); 
        //    }
        //    for (int i = 0; i < goalPositives; ++i) 
        //    {
        //        aEpitopeLearningData.Add(shuffledPositives[i], true);
        //    }

        //    return aEpitopeLearningData;
        //} 
 
        //internal static EpitopeLearningData GetAplusCollection(HlaResolution hlaResolution, KmerDefinition kmerDefinition)
        //{ 
        //    EpitopeLearningData rg = new EpitopeLearningData("Aplus");

        //    SpecialFunctions.CheckCondition(hlaResolution.Equals(HlaResolution.ABMixed));
        //    foreach (Dictionary<string, string> row in Predictor.TabFileTable("Aplus.txt", "HLA	peptide", false))
        //    {
        //        string peptide = row["peptide"]; 
        //        SpecialFunctions.CheckCondition(peptide.IndexOf('?') < 0); //!!!raise error 

        //        if (peptide.Length != kmerDefinition.FullMerCount) //!!!const 
        //        {
        //            continue;
        //        }

        //        string hla = row["HLA"];
 
        //        HlaToLength hlaToLength = HlaToLength.GetInstance(hla, hlaResolution); 
        //        MerAndHlaToLength aMerAndHlaToLength = MerAndHlaToLength.GetInstance(peptide, hlaToLength, kmerDefinition);
        //        rg[aMerAndHlaToLength] = true; 
        //    }

        //    return rg;

        //}
 
 	} 

    public class EpitopeLearningDataDupHlaOK : Dictionary<string, Dictionary<Hla, Dictionary<Hla, bool>>> 
    {
        internal EpitopeLearningDataDupHlaOK(string name)
            : base()
        {
            Name = name;
        } 
 
        public string Name;
 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
