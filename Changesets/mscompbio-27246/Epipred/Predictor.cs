using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Adapt.LearningWorkbench;
using System.Reflection;
using Msr.Adapt.HighLevelFeatures; 
using System.IO; 
using Msr.Mlas.SpecialFunctions;
using VirusCount; 
using VirusCount.Qmrr;
using ProcessingPrediction;

namespace EpipredLib
{
    public class Predictor 
    { 
        private Predictor()
        { 
        }

        static public Predictor GetInstance(string modelName, int eLength, int ncLength, Converter<Hla, Hla> hlaForNormalization)
        {
            Predictor aBestPredictor = new Predictor();
            aBestPredictor.LoadUpPredictor(modelName, eLength, ncLength, hlaForNormalization); 
            return aBestPredictor; 
        }
        public NEC SampleNEC; 
        Converter<Hla, Hla> HlaForNormalization;

        public double Predict(List<Dictionary<string, string>> patientTable, NEC nec, bool modelOnly)
        {
            double predictedPTotal = 0.0;
 
            foreach (Dictionary<string, string> patientRow in patientTable) 
            {
                double product = 1.0; 
                foreach (KeyValuePair<string, string> columnAndValue in patientRow)
                {
                    Hla hla = HlaFactory.GetGroundInstance(columnAndValue.Key.Substring(0, 1) + columnAndValue.Value);
                    Debug.Assert(nec.N.Length == SampleNEC.N.Length && nec.E.Length == SampleNEC.E.Length && nec.C.Length == SampleNEC.C.Length); // real assert
                    string sourceIgnore;
                    double probability = Predict(nec, hla, modelOnly, out sourceIgnore); 
                    product *= 1.0 - probability; 
                }
                double noiseyOrForThisPatient = 1.0 - product; 
                predictedPTotal += noiseyOrForThisPatient;
            }
            double predictedP = predictedPTotal / (double)patientTable.Count;
            return predictedP;
        }
 
        public HlaFactory HlaFactory;// = HlaFactory.GetFactory("MixedWithB15AndA68"); 
        public string SourceDataFileName;
        public string[] NameList; 



        //static PatchPatternFactory PatchPatternFactory = PatchPatternFactory.GetFactory("strings");

        //private static PatchPattern CreateStringPatchPattern(string peptide) 
        //{ 
        //    PatchPatternBuilder aPatchPatternBuilder = PatchPatternFactory.GetBuilder();
        //    aPatchPatternBuilder.AppendGroundDisjunct(peptide); 
        //    PatchPattern patchPattern = aPatchPatternBuilder.ToPatchPattern();
        //    return patchPattern;
        //}


        public double Predict(NEC nec, Hla hla, bool modelOnly, out string source) 
        { 
            Debug.Assert(HlaFactory.IsGroundOrAbstractInstance(hla.ToString())); // real assert
            SpecialFunctions.CheckCondition(nec.N.Length == SampleNEC.N.Length && nec.E.Length == SampleNEC.E.Length && nec.C.Length == SampleNEC.C.Length, 
                string.Format("Length of peptide must be {0},{1},{2}", SampleNEC.N.Length, SampleNEC.E.Length, SampleNEC.C.Length));
            Pair<NEC, Hla> necAndHla = new Pair<NEC, Hla>(nec, hla);

            List<Pair<string, Hla>> sourceAndOriginalHlaCollection = ListAllSourcesContainingThisMerAndHlaToLength(necAndHla);
            source = SpecialFunctions.Join("+", sourceAndOriginalHlaCollection); //Will be "" if list is empty
 
            double probability = (sourceAndOriginalHlaCollection.Count == 0 || modelOnly) ? probability = (double)Logistic.EvaluateViaCache(necAndHla) : 1.0; 
            return probability;
        } 

        private List<Pair<string, Hla>> ListAllSourcesContainingThisMerAndHlaToLength(Pair<NEC, Hla> necAndHlaIn)
        {
            string peptide = necAndHlaIn.First.E;
            Hla hlaNorm = HlaForNormalization(necAndHlaIn.Second);
            List<Pair<string, Hla>> sourceAndOriginalHlaCollection = new List<Pair<string, Hla>>(); 
            foreach (EpitopeLearningDataDupHlaOK epitopeLearningData in EpitopeLearningDataList) 
            {
                if (null != epitopeLearningData && epitopeLearningData.ContainsKey(peptide)) 
                {
                    Dictionary<Hla, Dictionary<Hla, bool>> hlaNormToHlaOriToLabel = epitopeLearningData[peptide];
                    if (hlaNormToHlaOriToLabel.ContainsKey(hlaNorm))
                    {
                        foreach (KeyValuePair<Hla, bool> hlaOriAndLabel in hlaNormToHlaOriToLabel[hlaNorm])
                        { 
                            Debug.Assert(hlaOriAndLabel.Value); // real assert 
                            sourceAndOriginalHlaCollection.Add(new Pair<string, Hla>(epitopeLearningData.Name, hlaOriAndLabel.Key));
                        } 
                    }
                }
            }
            return sourceAndOriginalHlaCollection;
        }
 
        private Logistic Logistic; 
        private List<EpitopeLearningDataDupHlaOK> EpitopeLearningDataList;
 
        private void LoadUpPredictor(string modelName, int eLength, int ncLength, Converter<Hla, Hla> hlaForNormalization)
        {
            //Load up the predictor

            string featurerizerName;
 
            switch (modelName.ToLower()) 
            {
                //!!!would be better not to have multiple of these switch statements around - looks like a job for a Class 
                case "lanliedb03062007":
                    featurerizerName = "+ea+cpST@st-setteO.0"; 
                    SampleNEC = NEC.GetInstance("", new string(' ', eLength), ""); 
                    HlaFactory = HlaFactory.GetFactory("MixedWithB15AndA68");
                    SourceDataFileName = "lanlIedb03062007.pos.source.txt"; 
                    NameList = new string[]{"LANL","IEDB"};
                    break;
                default:
                    SpecialFunctions.CheckCondition(false, "Don't know what featurerizer to use for the model");
                    featurerizerName = null;
                    SourceDataFileName = null;
                    NameList = null;
                    break; 
            } 
            Converter<object, Set<IHashableFeature>> featurizer = FeatureLib.CreateFeaturizer(featurerizerName);
 
            //GeneratorType generatorType = GeneratorType.ComboAndZero6SuperType;
            //FeatureSerializer featureSerializer = PositiveNegativeExperimentCollection.GetFeatureSerializer();
            //KmerDefinition = kmerDefinition;
            //HlaResolution hlaResolution = HlaResolution.ABMixed;
            string resourceName = string.Format("maxentModel{0}{1}{2}{3}.xml", modelName.Split('.')[0], SampleNEC.N.Length, SampleNEC.E.Length, SampleNEC.C.Length);
            EpitopeLearningDataList = new List<EpitopeLearningDataDupHlaOK>(); 
            using (StreamReader streamReader = Predictor.OpenResource(resourceName)) 
            {
                Logistic = (Logistic)FeatureLib.FeatureSerializer.FromXmlStreamReader(streamReader); 
                //Logistic.FeatureGenerator = EpitopeFeatureGenerator.GetInstance(KmerDefinition, generatorType, featureSerializer).GenerateFeatureSet;
                Logistic.FeatureGenerator = FeatureLib.CreateFeaturizer(featurerizerName);
                foreach (string name in NameList)
                {
                    EpitopeLearningData epitopeLearningDataX = EpitopeLearningData.GetDbWhole(HlaFactory, SampleNEC.E.Length, name, SourceDataFileName);
                    Debug.Assert(epitopeLearningDataX.Count > 0, "Expect given data to have some data"); 
                    //!!!combine with previous step 
                    EpitopeLearningDataDupHlaOK epitopeLearningData = new EpitopeLearningDataDupHlaOK(epitopeLearningDataX.Name);
                    foreach (KeyValuePair<Pair<string, Hla>, bool> merAndHlaAndLabel in epitopeLearningDataX) 
                    {
                        Hla hlaIn = merAndHlaAndLabel.Key.Second;
                        Hla hlaOut = hlaForNormalization(hlaIn);

                        Dictionary<Hla, Dictionary<Hla, bool>> hla2ToHlaToLabel = SpecialFunctions.GetValueOrDefault(epitopeLearningData, merAndHlaAndLabel.Key.First);
                        Dictionary<Hla, bool> hlaToLabel = SpecialFunctions.GetValueOrDefault(hla2ToHlaToLabel, hlaOut); 
                        hlaToLabel.Add(hlaIn, merAndHlaAndLabel.Value); 
                    }
 
                    EpitopeLearningDataList.Add(epitopeLearningData);
                }
            }

            HlaForNormalization = hlaForNormalization;
 
        } 

        public static string ResourceString = "EpipredLib.DataFiles."; 

        internal static StreamReader OpenResource(string fileName)
        {
            return SpecialFunctions.OpenResource(Assembly.GetExecutingAssembly(), ResourceString, fileName); //!!!const
        }
        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, string header, bool includeWholeLine) 
        { 
            return TabFileTable(filename, header, includeWholeLine, '\t');
        } 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, string header, bool includeWholeLine, char separator)
        {
            return SpecialFunctions.TabFileTable(Assembly.GetExecutingAssembly(), ResourceString, filename, header, includeWholeLine, separator, /*check header match*/ true);
        }
        public static List<Dictionary<string, string>> TabFileTableAsList(string filename, string header, bool includeWholeLine)
        { 
            return SpecialFunctions.TabFileTableAsList(Assembly.GetExecutingAssembly(), ResourceString, filename, header, includeWholeLine); 
        }
        static public IEnumerable<Dictionary<string, string>> TabFileTableNoHeaderInFile(string filename, string header, bool includeWholeLine, char separator) 
        {
            return SpecialFunctions.TabFileTableNoHeaderInFile(Assembly.GetExecutingAssembly(), ResourceString, filename, header, includeWholeLine, separator);
        }
        static public IEnumerable<Dictionary<string, string>> TabFileTableNoHeaderInFile(string filename, string header, bool includeWholeLine)
        {
            return SpecialFunctions.TabFileTableNoHeaderInFile(Assembly.GetExecutingAssembly(), ResourceString, filename, header, includeWholeLine, '\t'); 
        } 

 

        public IEnumerable<Pair<string, Hla>> PositiveExampleEnumeration()
        {

            //Don't repeat if the Mer/OriHla has already been seen, but DO repeat if the mer/HlaNorm has appeared before
            Set<Pair<string, Hla>> merAndHlaOriSet = new Set<Pair<string, Hla>>(); 
 
            foreach (EpitopeLearningDataDupHlaOK epitopeLearningData in EpitopeLearningDataList)
            { 
                if (null != epitopeLearningData)
                {
                    foreach (KeyValuePair<string, Dictionary<Hla, Dictionary<Hla, bool>>> merAndHlaNormToHlaOriToLabel in epitopeLearningData)
                    {
                        string mer = merAndHlaNormToHlaOriToLabel.Key;
                        foreach (KeyValuePair<Hla, Dictionary<Hla, bool>> hlaNormAndHlaNormToLabel in merAndHlaNormToHlaOriToLabel.Value) 
                        { 
                            Pair<string, Hla> merAndHlaNorm = new Pair<string, Hla>(mer, hlaNormAndHlaNormToLabel.Key);
                            foreach (KeyValuePair<Hla, bool> HlaOriAndLabel in hlaNormAndHlaNormToLabel.Value) 
                            {
                                Pair<string, Hla> merAndHlaOri = new Pair<string, Hla>(mer, HlaOriAndLabel.Key);
                                if (!merAndHlaOriSet.Contains(merAndHlaOri))
                                {
                                    merAndHlaOriSet.AddNew(merAndHlaOri);
                                    yield return merAndHlaNorm; 
                                } 
                            }
                        } 
                    }
                }
            }
        }
    }
} 
 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
