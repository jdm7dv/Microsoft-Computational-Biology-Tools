using System; 
using System.Collections.Generic;
using System.Text;
using VirusCount;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using VirusCount.Qmrr; 
using ProcessingPrediction; 

namespace EpipredLib 
{
    public class PredictorCollection
    {
        private PredictorCollection()
        {
        } 
 
        static public string Version
        { 
            get
            {
                //UnitTest();
                return "Aplus+LANL+IEDB08232006 or Aplus08182006 or Aplus+LANL+IEDBFlank510242006";
            }
        } 
 
        private Dictionary<int, Dictionary<Hla, double>> KToHlaToPriorLogOdds;
        private Set<Hla> _hlaSet; 
        private Dictionary<string, Set<Hla>> _supertypeMap;
        public Dictionary<int, Predictor> KToPredictor;
        public int NCLength;
        public SupertypeSpec HasBlanks;
        public Converter<Hla, Hla> HlaForNormalization;
        public bool RaiseErrorIfNotFoundInNormalizationTable; 
        public double RatioOfTrueToFalseTrainingExample; 

 
        static public Hla Identity(Hla hla)
        {
            return hla;
        }

 
        static public Hla TrimToHla2(Hla hlaIn) 
        {
            string hlaName = hlaIn.ToString(); 
            SpecialFunctions.CheckCondition(hlaName.Length >= 3, string.Format("Expected hla name to have length of at least 3 (a class and two digits), but it is '{0}'.", hlaName));
            Hla hlaOut = SingletonSpecification.HlaFactoryNoConstraints.GetGroundInstance(hlaName.Substring(0, 3));
            return hlaOut;
        }

 
        public static PredictorCollection GetInstance(string modelName) 
        {
            PredictorCollection bestPredictorCollection = new PredictorCollection(); 

            switch (modelName.ToLower())
            {
                case "lanliedb03062007": 
                    bestPredictorCollection.NCLength = 0; 
                    bestPredictorCollection.HasBlanks = SupertypeSpec.None; //!!!Might want to create supertype set later
                    bestPredictorCollection.HlaForNormalization = Identity; 
                    bestPredictorCollection.RaiseErrorIfNotFoundInNormalizationTable = true;
                    bestPredictorCollection.RatioOfTrueToFalseTrainingExample = .1;
                    break;
                default:
                    SpecialFunctions.CheckCondition(false, "Don't know of model");
                    bestPredictorCollection.NCLength = int.MinValue; 
                    bestPredictorCollection.HasBlanks = SupertypeSpec.ImpossibleValue; 
                    bestPredictorCollection.HlaForNormalization = null;
                    bestPredictorCollection.RaiseErrorIfNotFoundInNormalizationTable = true; 
                    bestPredictorCollection.RatioOfTrueToFalseTrainingExample = double.NaN;
                    break;
            }

            bestPredictorCollection.KToPredictor = new Dictionary<int, Predictor>();
 
            for (int k = (int)MerLength.firstLength; k <= (int)MerLength.lastLength; ++k) 
            {
                Predictor bestPredictor = Predictor.GetInstance(modelName, k, bestPredictorCollection.NCLength, bestPredictorCollection.HlaForNormalization); 
                bestPredictorCollection.KToPredictor.Add(k, bestPredictor);
            }

            bestPredictorCollection.CreateKToHlaToPriorLogOdds();

 
            return bestPredictorCollection; 
        }
 
        /// <summary>
        /// Returns a list with one item per unique "GroupBy" key (e.g. each hla,  each length, etc). Each item is a list of the predictions
        /// that tie for maximum probability.
        ///
        /// Most of the inputs are the same as for PredictionEnumeration.
        /// </summary> 
        /// <param name="showBy"></param> 
        /// <param name="inputPeptide"></param>
        /// <param name="merLength"></param> 
        /// <param name="hlaSetSpecification"></param>
        /// <param name="hlaOrSupertypeOrNull"></param>
        /// <param name="modelOnly"></param>
        /// <returns></returns>
        public Dictionary<object, List<Prediction>> MaxProbabilityPredictions(ShowBy showBy, string inputPeptide,
                MerLength merLength, int? dOfCenter, HlaSetSpecification hlaSetSpecification, string hlaOrSupertypeOrNull, bool modelOnly) 
        { 
            Dictionary<object, BestSoFarWithTies<double, Prediction>> keyToBest =
                    ForEachKeyFindTheBestResults(showBy, inputPeptide, merLength, dOfCenter, hlaSetSpecification, hlaOrSupertypeOrNull, modelOnly); 

            Dictionary<object, List<Prediction>> maxProbabilityPredictionsPerKey = FindResultsPerKey(keyToBest);

            return maxProbabilityPredictionsPerKey;
        }
 
 
        private Dictionary<object, BestSoFarWithTies<double, Prediction>> ForEachKeyFindTheBestResults(
            ShowBy showBy, 
            string inputPeptide, MerLength merLength, int? dOfCenter, HlaSetSpecification hlaSetSpecification, string hlaOrSupertypeOrNull, bool modelOnly)
        {
            Dictionary<object, BestSoFarWithTies<double, Prediction>> keyToBest = new Dictionary<object, BestSoFarWithTies<double, Prediction>>();
            foreach (Prediction prediction in PredictionEnumeration(inputPeptide, merLength, dOfCenter, hlaSetSpecification, hlaOrSupertypeOrNull, modelOnly))
            {
                object key = prediction.GroupByKey(showBy); //!!!would be nice if this were typed, rather than just using 'string' 
                BestSoFarWithTies<double, Prediction> bestSoFarWithTies = SpecialFunctions.GetValueOrDefault(keyToBest, key, BestSoFarWithTies<double, Prediction>.GetInstance(SpecialFunctions.DoubleGreaterThan)); 

                bestSoFarWithTies.Compare(prediction.WeightOfEvidence, prediction); 
            }
            return keyToBest;
        }

        private static Dictionary<object, List<Prediction>> FindResultsPerKey(Dictionary<object, BestSoFarWithTies<double, Prediction>> keyToBest)
        { 
            SpecialFunctions.CheckCondition(keyToBest.Count > 0, "No predictions possible. (Was input peptide too short?)"); 
            Dictionary<object, List<Prediction>> keyToMaxProbabilityPredictionList = new Dictionary<object, List<Prediction>>();
            foreach (KeyValuePair<object, BestSoFarWithTies<double, Prediction>> keyAndBest in keyToBest) 
            {
                keyToMaxProbabilityPredictionList.Add(keyAndBest.Key, keyAndBest.Value.ChampList);
            }
            return keyToMaxProbabilityPredictionList;
        }
 
        ////!!! this could be moved into a class 
        //private object CreateKey(Prediction prediction, Best display)
        //{ 
        //    switch (display)
        //    {
        //        case Best.overall:
        //            return "best";
        //        case Best.perHla:
        //            return prediction.Hla; 
        //        case Best.perPrediction: 
        //            return prediction;
        //        case  Best.perLength: 
        //            return prediction.K;
        //        case Best.perHlaAndLength:
        //            return new Pair<Hla, int>(prediction.Hla, prediction.K);
        //        default:
        //            SpecialFunctions.CheckCondition(false, "Don't know how to display " + display.ToString());
        //            return null; 
        //    } 
        //}
 
        /// <summary>
        ///  HlaSetSpecification class choices:
        ///        HlaSetSpecification.Singleton – Means that an Hla will be given and it is the only hla to be considered
        ///        HlaSetSpecification.Supertype – Means that a supertype will be given and it’s hlas should be considered
        ///        HlaSetSpecification.All – Means to consider all known hlas
        /// </summary> 
        /// <param name="inputPeptide">a string of amino acids</param> 
        /// <param name="merLength">A value from the MerLength enum, which includes MerLength.scan, MerLength.given, MerLength.Eight, etc</param>
        /// <param name="hlaSetSpecification">A predefined HlaSetSpecification class.</param> 
        /// <param name="hlaOrSupertypeOrNull">The hla or supertype required by HlaSetSpecification, or null for HlaSetSpecification.All</param>
        /// <param name="modelOnly">If should report the probability from the model, even when the epitope is on a source list.</param>
        /// <returns></returns>
        public IEnumerable<Prediction> PredictionEnumeration(string inputPeptide, MerLength merLength, int? dOfCenter, HlaSetSpecification hlaSetSpecification, string hlaOrSupertypeOrNull, bool modelOnly)
        {
            Set<Hla> hlaSet = HlaSet(hlaSetSpecification, hlaOrSupertypeOrNull); 
            foreach (int eLength in KEnumeration(merLength, inputPeptide.Length)) 
            {
                Predictor predictor = KToPredictor[eLength]; 
                Dictionary<Hla, double> hlaToPriorLogOdds = KToHlaToPriorLogOdds[eLength];

                int necLength = NCLength + eLength + NCLength;
                foreach (int startIndex in StartIndexEnumeration(inputPeptide.Length, necLength, dOfCenter))
                {
                    string peptide = inputPeptide.Substring(startIndex, necLength); 
                    NEC nec = NEC.GetInstance(peptide, NCLength, eLength, NCLength); 
                    foreach (Hla hla in hlaSet)
                    { 
                        Hla hlaForNormalization = HlaForNormalization(hla);
                        double priorLogOddsOfThisLengthAndHla;
                        if (!hlaToPriorLogOdds.TryGetValue(hlaForNormalization, out priorLogOddsOfThisLengthAndHla))
                        {
                            SpecialFunctions.CheckCondition(!RaiseErrorIfNotFoundInNormalizationTable, string.Format("Hla '{0}' (which is '{1}' for the purposes of normalization) and is not found in the normalization table", hla, hlaForNormalization));
                            priorLogOddsOfThisLengthAndHla = SpecialFunctions.LogOdds(RatioOfTrueToFalseTrainingExample); 
                        } 

 
                        string source;
                        double originalP = predictor.Predict(nec, hla, modelOnly, out source);
                        double originalLogOdds = SpecialFunctions.LogOdds(originalP);

                        double correctedLogOdds = originalLogOdds + priorLogOddsOfThisLengthAndHla;
                        double posteriorProbability = SpecialFunctions.InverseLogOdds(correctedLogOdds); 
                        double weightOfEvidence = correctedLogOdds - SpecialFunctions.LogOdds(RatioOfTrueToFalseTrainingExample); 
                        Prediction prediction = Prediction.GetInstance(inputPeptide, hla, posteriorProbability, weightOfEvidence, nec, startIndex + NCLength + 1, startIndex + NCLength + eLength, source);
                        yield return prediction; 
                    }
                }
            }
        }

        private IEnumerable<int> StartIndexEnumeration(int inputPeptideLength, int necLength, int? dOfCenter) 
        { 
            if (null == dOfCenter)
            { 
                for (int startIndex = 0; startIndex <= inputPeptideLength - necLength; ++startIndex)
                {
                    yield return startIndex;
                }
            }
            else 
            { 
                //SpecialFunctions.CheckCondition(NCLength == 0, "Need to code for dOfCenter and flanking");
                int center0 = (inputPeptideLength - 1) / 2; //int division 
                int first = Math.Max(0, center0 - (int)dOfCenter - necLength + 1);
                int last = Math.Min(inputPeptideLength - necLength, center0 + (int)dOfCenter);

                for (int startIndex = first; startIndex <= last; ++startIndex)
                {
                    yield return startIndex; 
                } 
            }
        } 


        private Set<Hla> HlaSet(HlaSetSpecification hlaSetSpecification, string hlaOrSupertypeOrNull)
        {
            Set<Hla> hlaSet = hlaSetSpecification.HlaSet(hlaOrSupertypeOrNull, _hlaSet, _supertypeMap);
            return hlaSet; 
        } 

        public Set<Hla> HlaSet() 
        {
            return _hlaSet;
        }


 
        public IEnumerable<int> KEnumeration() 
        {
            return KEnumeration(MerLength.scan, int.MinValue); 
        }
        private IEnumerable<int> KEnumeration(MerLength merLength, int inputPeptideLength)
        {
            switch (merLength)
            {
                case MerLength.scan: 
                    for (int k = (int)MerLength.firstLength; k <= (int)MerLength.lastLength; ++k) 
                    {
                        yield return k; 
                    }
                    break;
                case MerLength.given:
                    int inputEpitopeLength = inputPeptideLength - NCLength - NCLength;
                    SpecialFunctions.CheckCondition((int)MerLength.firstLength <= inputEpitopeLength && inputEpitopeLength <= (int)MerLength.lastLength,
                            string.Format("Given peptide length {0} is out of range", inputPeptideLength)); 
                    yield return inputEpitopeLength; 
                    break;
                default: 
                    SpecialFunctions.CheckCondition(MerLength.firstLength <= merLength && merLength <= MerLength.lastLength,
                            string.Format("Given peptide length {0} is out of range", (int)merLength));
                    yield return (int)merLength;
                    break;
            }
        } 
 
        /* From [Microsoft Research]:
 
            - I’ve changed two things wrt prior corrections.  First, I’m computing relative frequencies
              across length per HLA rather than per supertype (there was too much variation within
              supertype).  Second, the formula that I gave you last was not quite right in that it did not
              take into account the denominator of the prior odds term.  Given p_kh, the uncorrected
              probability of being an epitope according to the classifier for peptide of length k and
              HLA h, the correction is as follows: 
 
            log odds  := ln (p_kh/(1-p_kh))
            log odds := log odds + ln(  [relFreq_kh/0.25 * (1/100)] / [1 – relFreq_kh/0.25 * (1/100)] ) 
            pk_corrected = exp(log odds) / (1 + exp(log odds))

            (Technical notes: In training, we are assuming a prior of 1/100 for each hla and k.
             In the data, the prior over hla is not uniform (e.g., there is lots of A02), but we think
             this is sampling bias.  That is, we think the prior on being an epitope is roughly'
             uniform for each hla.  But, the data is fairly unbiased wrt prior on epitope of length 
             k reacting, given HLA.  That is, biologists were looking at particular HLAs, but they 
             then found the optimal length for the epitope, giving an unbiased view of which lengths
             react with which HLAs.  Thus, for every HLA, we should correct the prior as a function 
             of length.  We used to correct by supertype, but I’m seeing too much variation within
             a given supertype.  To help with smoothing, I’m using a Dirichlet(1,1,1,1) prior.
             Dividing each relFreq by 0.25 in the above formula guarantees that the overall prior is
             still 1/100.)
         *
 
         *  From: [Microsoft Research] 
            Sent: Thursday, July 27, 2006 4:25 PM
 

         *      As we discussed, I would like to write out the weight of evidence for the epitope rather
         *      than its posterior probability.  This is logOdds minus the prior (which is implicitly 1/100
         *      in our training data).

                The formula for weight of evidence is (assuming 4 values of K, and 99 negatives per positive) 
 
                    priorLogOddsOfThisLengthAndHla = LogOdds((relFreq/.25) * .01);
                    originalLogOdds = LogOdds(originalP); 
                    correctedLogOdds = originalLogOdds + priorLogOddsOfThisLengthAndHla;
                    weightofEvidence = correctedLogOdds – LogOdds(0.01);

         */
        private void CreateKToHlaToPriorLogOdds()
        { 
            KToHlaToPriorLogOdds = new Dictionary<int, Dictionary<Hla, double>>(); 
            _hlaSet = new Set<Hla>();
            HlaFactory hlaFactory = HlaFactory.GetFactory("MixedWithB15AndA68"); 
            _supertypeMap = new Dictionary<string, Set<Hla>>();


            Dictionary<Hla, Dictionary<int, int>> hlaToLengthToLengthToSmoothedCount = CreateHlaToLengthToLengthToSmoothedCount();

            foreach (Hla hla in hlaToLengthToLengthToSmoothedCount.Keys) 
            { 
                _hlaSet.AddNewOrOld(hla);
 
                Dictionary<int, int> lengthToSmoothedCount = hlaToLengthToLengthToSmoothedCount[hla];
                int smoothedTotal = ComputeSmoothedTotal(lengthToSmoothedCount);

                for (int k = (int)MerLength.firstLength; k <= (int)MerLength.lastLength; ++k)
                {
                    AddToHlaToPriorLogOdds(hla, lengthToSmoothedCount, smoothedTotal, k); 
                } 

                AddToSupertypeMap(hla); 
            }

            AssertThatEveryKHasEveryHla();

        }
 
        private void AddToSupertypeMap(Hla hla) 
        {
            string supertypeAny = SetSupertypeAny(hla, HasBlanks); 
            if (supertypeAny != "unknown" && supertypeAny != "none") //!!!"unknown" is a misnomer. Should be "none" or null, but don't want to change it because it is already in useful models.
            {
                Set<Hla> hlaSet = SpecialFunctions.GetValueOrDefault(_supertypeMap, supertypeAny);
                hlaSet.AddNewOrOld(hla);
            }
        } 
 
        private void AddToHlaToPriorLogOdds(Hla hla, Dictionary<int, int> lengthToSmoothedCount, int smoothedTotal, int k)
        { 
            double relFreq = (double)lengthToSmoothedCount[k] / (double)smoothedTotal;
            Dictionary<Hla, double> hlaToPriorLogOdds = SpecialFunctions.GetValueOrDefault(KToHlaToPriorLogOdds, k);
            hlaToPriorLogOdds.Add(hla, SpecialFunctions.LogOdds((relFreq / .25) * RatioOfTrueToFalseTrainingExample));
        }

 
        private static int ComputeSmoothedTotal(Dictionary<int, int> lengthToSmoothedCount) 
        {
            int smoothedTotal = 0; 
            for (int k = (int)MerLength.firstLength; k <= (int)MerLength.lastLength; ++k)
            {
                smoothedTotal += SpecialFunctions.GetValueOrDefault(lengthToSmoothedCount, k, 1);
            }
            return smoothedTotal;
        } 
 
        private Dictionary<Hla, Dictionary<int, int>> CreateHlaToLengthToLengthToSmoothedCount()
        { 
            Dictionary<Hla, Dictionary<int, int>> hlaToLengthToLengthToSmoothedCount = new Dictionary<Hla, Dictionary<int, int>>();
            for (int k = (int)MerLength.firstLength; k <= (int)MerLength.lastLength; ++k)
            {
                Predictor predictor = KToPredictor[k];
                foreach (Pair<string, Hla> merAndHlaToLength in predictor.PositiveExampleEnumeration())
                { 
                    Dictionary<int, int> lengthToSmoothedCount = SpecialFunctions.GetValueOrDefault(hlaToLengthToLengthToSmoothedCount, merAndHlaToLength.Second); 
                    int length = merAndHlaToLength.First.Length;
                    //Debug.Assert(length == merAndHlaToLength.Mer.Length); // real assert 
                    lengthToSmoothedCount[length] = 1 + SpecialFunctions.GetValueOrDefault(lengthToSmoothedCount, length, 1);
                }
            }
            return hlaToLengthToLengthToSmoothedCount;
        }
 
        private static string SetSupertypeAny(Hla hla, SupertypeSpec hasBlanks) 
        {
            switch (hasBlanks) 
            {
                case SupertypeSpec.HasBlanksTrue:
                    {
                        HlaToLength hlaToLength = HlaToLength.GetInstanceABMixed(hla.Name);
                        string supertypeAny = hlaToLength.ToZero6SupertypeBlanksString();
                        return supertypeAny; 
                    } 
                case SupertypeSpec.HasBlanksFalse:
                    { 
                        HlaToLength hlaToLength = HlaToLength.GetInstanceABMixed(hla.Name);
                        string supertypeAny = hlaToLength.ToZero6SupertypeNoBlanksString();
                        return supertypeAny;
                    }
                case SupertypeSpec.None:
                    return "none"; //!!!const 
                default: 
                    SpecialFunctions.CheckCondition(false, "unknown SupertypeSpec: " + hasBlanks.ToString());
                    return null; 
            }
        }



        private void AssertThatEveryKHasEveryHla() 
        { 
            foreach (Dictionary<Hla, double> hlaToPriorLogOdds in KToHlaToPriorLogOdds.Values)
            { 
                Debug.Assert(_hlaSet.Equals(Set<Hla>.GetInstance(hlaToPriorLogOdds.Keys))); // real assert
            }
        }


        //public static void UnitTest() 
        //{ 
        //    // Create an instance of the main object
        //    PredictorCollection predictorCollection = PredictorCollection.GetInstance("lanliedb3062007"); 

        //    //A supertype is a set of Hla’s.
        //    //Output all known HLA's
        //    Console.WriteLine(predictorCollection.HlaSet());

        //    //Output all known supertypes and their hla's 
        //    foreach (string supertype in predictorCollection.SupertypeMap.Keys) 
        //    {
        //        Console.WriteLine("{0} -> {1}", supertype, predictorCollection.SupertypeMap[supertype]); 
        //    }


        //    //Output all known merlengths. This will output 8, 9, 10, 11
        //    foreach (int k in predictorCollection.KEnumeration())
        //    { 
        //        Console.WriteLine(k); 
        //    }
 

        //    // Find the predicted weightOfEvidence for peptide AAAAAAAAA for known k, every hla, and every possible position.
        //    foreach (Prediction prediction in predictorCollection.PredictionEnumeration("AAAAAAAAA", MerLength.scan, null, HlaSetSpecification.All, null, /*modelOnly*/ false))
        //    {
        //        Console.WriteLine(prediction.WeightOfEvidence); //Output the WOE, is one of many fields in a prediction
        //        Console.WriteLine(prediction); //Output all the fields of the prediction 
        //    } 

        //    // Find the predicted weightOfEvidence for peptide AAAAAAAAA for k=9, hla=A01, and every possible position. 
        //    // Report the probability from the model and not the source list.
        //    Console.WriteLine(HlaSetSpecification.All.Header()); //Output the header for all the fields of a prediction
        //    foreach (Prediction prediction in predictorCollection.PredictionEnumeration("AAAAAAAAA", (MerLength)9, null, HlaSetSpecification.Singleton, "A01", /*modelOnly*/ true))
        //    {
        //        Console.WriteLine(prediction); //Output all the fields of the prediction
        //    } 
 

        //    //For each HLA, find the prediction(s) with the max probability 
        //    // where 1. the peptide contains is AAAAAAAAA,
        //    //       2. we only consider HLAs with supertype B58
        //    //       3. only consider peptides of length 9 (the length of AAAAAAAAA),
        //    //       4. If AAAAAAAAA is on a source list, assign it probability 1.

        //    Dictionary<object, List<Prediction>> keyToMaxProbabilityPredictionList = 
        //            predictorCollection.MaxProbabilityPredictions( 
        //                    ShowBy.hla,
        //                    "AAAAAAAAAA", MerLength.given, null, HlaSetSpecification.Supertype, "B58", /*modelOnly*/ false); 

        //    foreach (object key in keyToMaxProbabilityPredictionList.Keys)
        //    {
        //        Console.WriteLine("For key {0} these predictions are tied for max probability", key);
        //        List<Prediction> maxProbabilityPredictionList = keyToMaxProbabilityPredictionList[key];
        //        foreach (Prediction prediction in maxProbabilityPredictionList) 
        //        { 
        //            Console.WriteLine("\t{0}", prediction);
        //        } 
        //    }

        //}


        public Dictionary<string, Set<Hla>> SupertypeMap 
        { 
            get
            { 
                return _supertypeMap;
            }
        }

    }
 
    public enum MerLength 
    {
        scan = int.MaxValue, 
        given = -1,
        eight = 8,
        nine = 9,
        ten = 10,
        eleven = 11,
        firstLength = eight, 
        lastLength = eleven, 

 
    }

    public enum ShowBy
    {
        all,
        hla, 
        length, 
        hlaAndLength,
        doNotGroup, 
    }

    public enum SupertypeSpec
    {
        ImpossibleValue,
        HasBlanksFalse, 
        HasBlanksTrue, 
        None
    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
