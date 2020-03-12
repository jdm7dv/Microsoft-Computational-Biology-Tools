using System; 
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Msr.Adapt.HighLevelFeatures;
using Msr.Adapt.LearningWorkbench;
using System.IO; 
using System.Diagnostics; 
using Msr.Mlas.SpecialFunctions;
using EpipredLib; 

namespace VirusCount
{
    public class PositiveNegativeExperiment
    {
 
        static private List<WeightIf> GetWeights(Dictionary<IHashableFeature, int> featureTable, string weightsFile) 
        {
            List<IHashableFeature> featureSet = CreateFeatureSet(featureTable); 
            return GetWeights(featureSet, weightsFile);
        }
        static public List<WeightIf> GetWeights(List<IHashableFeature> featureSet, string weightsFile)
        {
            List<WeightIf> weightIfCollection = new List<WeightIf>();
            bool seenOffset = false; 
 
            using (StreamReader streamreaderWeights = File.OpenText(weightsFile))
            { 
                string sExpectedStart = "*Added feature "; //!!!const
                string sLineWeightsStart = streamreaderWeights.ReadLine();
                SpecialFunctions.CheckCondition(sLineWeightsStart.StartsWith(sExpectedStart)); //!!!raise error
                int cFeature = 1 + int.Parse(sLineWeightsStart.Substring(sExpectedStart.Length)); //!!!could raise error
                SpecialFunctions.CheckCondition(cFeature == 1 + featureSet.Count, string.Format("Expected contant feature to be feature #{0} features in weight file, but it is {1}. File: {2}", featureSet.Count+1, cFeature, weightsFile)); //!!!raise error
                for (int iFeature = 0; iFeature < cFeature; ++iFeature) 
                { 
                    string sLineWeight = streamreaderWeights.ReadLine();
                    SpecialFunctions.CheckCondition(sLineWeight != null); //!!! raise error 
                    double rWeight = ParseWeight(sLineWeight, iFeature);

                    if (iFeature + 1 < cFeature)
                    {
                        if (rWeight != 0)
                        { 
                            IHashableFeature aFeature = featureSet[iFeature]; 
                            WeightIf aWeightIf = new WeightIf(rWeight, (Feature)aFeature);
                            weightIfCollection.Add(aWeightIf); 
                        }
                    }
                    else
                    {
                        seenOffset = true;
                        IHashableFeature aFeature = Logistic.AlwaysTrue; 
                        WeightIf aWeightIf = new WeightIf(rWeight, (Feature)aFeature); 
                        weightIfCollection.Add(aWeightIf);
                    } 
                }
                SpecialFunctions.CheckCondition(null == streamreaderWeights.ReadLine()); //!!! raise error
            }

            SpecialFunctions.CheckCondition(seenOffset); //!!!raise error
            return weightIfCollection; 
 
        }
 
        private static List<IHashableFeature> CreateFeatureSet(Dictionary<IHashableFeature, int> featureTable)
        {
            List<IHashableFeature> featureSet = SpecialFunctions.CreateAllocatedList<IHashableFeature>(featureTable.Count);
            foreach (KeyValuePair<IHashableFeature, int> featureAndNumber in featureTable)
            {
                featureSet.Insert(featureAndNumber.Value, featureAndNumber.Key); 
            } 
            return featureSet;
        } 

        private static double ParseWeight(string sLineWeight, int iFeature)
        {

            //1	   -0.176747
            string[] rgsFields = sLineWeight.Split('\t'); 
            SpecialFunctions.CheckCondition(rgsFields.Length == 2); //!!! raise error 
            int iFeatureAgain = int.Parse(rgsFields[0]);
            SpecialFunctions.CheckCondition(iFeature == iFeatureAgain); //!!!raise error 
            double r = double.Parse(rgsFields[1]);
            return r;
        }

//        public void GenerateMaxEntTrainingFile()
//        { 
 
//            //!!!multithreading!!!!
 
//            Stopwatch stopwatchFeatureGeneration = new Stopwatch();
//            stopwatchFeatureGeneration.Start();
//            EpitopeFeatureGenerator.GenerateFeaturesAndMebFile(Train, MaxEntTrainingFileName, FeatureFileName);
//            stopwatchFeatureGeneration.Stop();
//            Debug.WriteLine(string.Format("Train set: {0}", Train.Name));
//            Debug.WriteLine(string.Format("Test set: {0}", Test.Name)); 
//        } 

//        static public Dictionary<IHashableFeature, int> CreateFeatureTableFile(string featureFileName, FeatureSerializer featureSerializer) 
//        {
//            Dictionary<IHashableFeature, int> featureTable = new Dictionary<IHashableFeature, int>();

//            using (StreamReader streamReader = File.OpenText(featureFileName))
//            {
//                string line; 
//                while (null != (line = streamReader.ReadLine())) 
//                {
//                    string[] rgField = line.Split('\t'); 
//                    SpecialFunctions.CheckCondition(rgField.Length == 2); //!!!raise error
//                    int id = int.Parse(rgField[0]);
//                    string sFeatureXml = rgField[1];
//                    IHashableFeature aFeature = (IHashableFeature)featureSerializer.FromXml(sFeatureXml);
//                    featureTable.Add(aFeature, id);
//                } 
//            } 

//            return featureTable; 
//        }


//        public List<RocRow> Eval(int lineLimit, HlaFilter hlaFilterOrNull)
//        {
//            Logistic aLogistic = LoadModel(); 
//            VerifyTestFile(); 
//            List<RocRow> rocRowCollection = EvaluateLogistic(aLogistic);
//            RocRow.ReportRocCurve(rocRowCollection, EvalFileName, true, lineLimit, hlaFilterOrNull); 
//            return rocRowCollection;
//        }

//        static public Logistic LoadModel(string weightsFileName, EpitopeFeatureGenerator epitopeFeatureGenerator, string featureFileName, FeatureSerializer featureSerializer)
//        {
//            Dictionary<IHashableFeature, int> featureSet = CreateFeatureTableFile(featureFileName, featureSerializer); 
 
//            List<WeightIf> weightIfCollection = GetWeights(featureSet, weightsFileName);
//            Logistic aLogistic = new Logistic(weightIfCollection.ToArray()); 

//            aLogistic.FeatureGenerator = epitopeFeatureGenerator.GenerateFeatureSet; //This is used for faster evalaution
//            //WriteOutModel(featureSerializer, aLogistic);

//            return aLogistic;
//        } 
 

//        public Logistic LoadModel() 
//        {
//            Dictionary<IHashableFeature, int> featureSet = CreateFeatureTableFile(FeatureFileName, FeatureSerializer);

//            List<WeightIf> weightIfCollection = GetWeights(featureSet, WeightsFileName);
//            Logistic aLogistic = new Logistic(weightIfCollection.ToArray());
 
//            aLogistic.FeatureGenerator = EpitopeFeatureGenerator.GenerateFeatureSet; //This is used for faster evalaution 
//            //WriteOutModel(featureSerializer, aLogistic);
 
//            aLogistic.Report(ModelFileName, CrossValIndex);
//            return aLogistic;
//        }

//        private void VerifyTestFile()
//        { 
//            Dictionary<string, bool> expectedLineCollections = new Dictionary<string, bool>(); 

//            foreach (KeyValuePair<MerAndHlaToLength, bool> merAndHlaToLengthWithLabel in Test) 
//            {
//                expectedLineCollections.Add(LabeledDataAsString(merAndHlaToLengthWithLabel), true);
//            }

//            using (StreamReader streamReader = File.OpenText(TestingFileName))
//            { 
//                string line; 
//                while (null != (line = streamReader.ReadLine()))
//                { 
//                    expectedLineCollections.Remove(line); //!!!an error here means the testing data doesn't match
//                }
//            }
//            Debug.Assert(expectedLineCollections.Count == 0); //!!!an error here means the testing data doesn't match
//        }
 
//        private string LabeledDataAsString(KeyValuePair<MerAndHlaToLength, bool> merAndHlaToLengthWithLabel) 
//        {
//            string s = string.Format("{0}\t{1}\t{2}", merAndHlaToLengthWithLabel.Value, merAndHlaToLengthWithLabel.Key.Mer, merAndHlaToLengthWithLabel.Key.HlaToLength); 
//            return s;
//        }

//        public void WriteOutTrainingAndTestFile()
//        {
//            WriteOutTrainingOrTestFile(TrainingFileName, Train); 
//            WriteOutTrainingOrTestFile(TestingFileName, Test); 
//        }
 

//        private void WriteOutTrainingOrTestFile(string fileName, EpitopeLearningData aEpitopeLearningData)
//        {
//            using (TextWriter streamWriter = File.CreateText(fileName))
//            {
//                foreach (KeyValuePair<MerAndHlaToLength, bool> merAndHlaToLengthWithLabel in aEpitopeLearningData) 
//                { 
//                    streamWriter.WriteLine(LabeledDataAsString(merAndHlaToLengthWithLabel));
//                } 
//            }
//        }

    }
    public class HlaFilter : List<HlaToLength>
    { 
        bool direction = true; 
        public string Name;
        private HlaFilter() 
        {
        }

        public static HlaFilter GetInstanceOrNull(string hlaFilterLine)
        {
            HlaFilter hlaFilter = new HlaFilter(); 
            if (hlaFilterLine == null || hlaFilterLine == "") 
            {
                return null; 
            }
            string supertypeKeyword = "Supertype";
            if (hlaFilterLine.StartsWith(supertypeKeyword))
            {
                SpecialFunctions.CheckCondition(false, "Need code");
                //string supertype = hlaFilterLine.Substring(supertypeKeyword.Length); 
                //hlaFilter.Name = hlaFilterLine; 
                //if (supertype == "Undefined")
                //{ 
                //    hlaFilterLine = HlaToLength.SupertypeToListString("Not" + supertype);
                //    hlaFilter.direction = false;
                //}
                //else
                //{
                //    hlaFilterLine = HlaToLength.SupertypeToListString(supertype); 
                //} 
            }
            else 
            {
                hlaFilter.Name = hlaFilterLine.Replace(",", "");
            }

            foreach (string hlaString in hlaFilterLine.Split(','))
            { 
                hlaFilter.Add(HlaToLength.GetInstanceABMixed(hlaString)); 
            }
            return hlaFilter; 
        }


        internal List<RocRow> Filter(List<RocRow> rocRowCollection)
        {
            List<RocRow> output = new List<RocRow>(); 
            foreach(RocRow rocRow in rocRowCollection) 
            {
                if (MatchWithFilter(rocRow.MerAndHlaToLength.HlaToLength)) 
                {
                    output.Add(rocRow);
                }
            }
            return output;
        } 
 
        //!!!could replace linear search
        public bool MatchWithFilter(HlaToLength hlaToLengthInput) 
        {
            string hlaVal = hlaToLengthInput.ToString();
            foreach (HlaToLength hlaToLength in this)
            {
                if (hlaVal.StartsWith(hlaToLength.ToString()))
                { 
                    return direction; 
                }
            } 
            return !direction;
        }
    }
}

 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
