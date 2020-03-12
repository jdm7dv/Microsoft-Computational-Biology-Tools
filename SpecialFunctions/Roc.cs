using System; 
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Msr.Mlas.SpecialFunctions 
{ 
    public class Roc<T>
    { 
        private Roc()
        {
        }

        private string OtherInfoHeader;
        public List<RocRow<T>> RocRowCollection; 
        private string PositiveClassName; 
        private string NegativeClassName;
 
        public static Roc<T> GetInstance(string otherInfoHeaderWithNoTabs, string positiveClassName, string negativeClassName, string fileName)
        {
            Roc<T> roc = GetInstance(otherInfoHeaderWithNoTabs, positiveClassName, negativeClassName);
            roc.Append(fileName);
            return roc;
        } 
 

        public static Roc<T> GetInstance(string otherInfoHeaderThatCanContainTabs, string positiveClassName, string negativeClassName) 
        {
            Roc<T> aRoc = new Roc<T>();
            aRoc.RocRowCollection = new List<RocRow<T>>();
            aRoc.PositiveClassName = positiveClassName;
            aRoc.NegativeClassName = negativeClassName;
            aRoc.OtherInfoHeader = otherInfoHeaderThatCanContainTabs; 
            return aRoc; 
        }
 
        public void Add(T otherInfoWithToStringThatCanContainTabs, bool label, double prediction)
        {
            //SpecialFunctions.CheckCondition(0.0 <= prediction && prediction <= 1.0);
            RocRow<T> rocRow = RocRow<T>.GetInstance(otherInfoWithToStringThatCanContainTabs, label, prediction);
            RocRowCollection.Add(rocRow);
        } 
 
        public Dictionary<Pair<double, double>, double> ReportRocCurve(TextWriter curveTextWriter)
        { 
            double threshold;
            double resultXFraction;
            return ReportRocCurve(curveTextWriter, DefaultAucBoundPair, 0, out threshold, out resultXFraction);
        }

        public static Set<Pair<double, double>> DefaultAucBoundPair = Set<Pair<double, double>>.GetInstance(new Pair<double, double>(0, 1)); 
 
        public Dictionary<Pair<double, double>, double> ReportRocCurve(TextWriter curveTextWriter, Set<Pair<double, double>> aucBoundPairSet, double goalYFraction, out double threshold,out double resultXFraction)
        { 
            threshold = 1;
            resultXFraction = 1;

            double eps = .000001;
            Dictionary<Pair<double, double>, double> boundsToMeanY = CreateBoundsToAuc(aucBoundPairSet);
            List<RocRow<T>> rocRowCollection = RocRowCollection; 
 

            rocRowCollection.Sort( 
                delegate(RocRow<T> rocRow0, RocRow<T> rocRow1)
                {
                    return rocRow1.Prediction.CompareTo(rocRow0.Prediction);
                });

            int positiveTotal = CountPositives(rocRowCollection); 
            int negativeTotal = rocRowCollection.Count - positiveTotal; 

            int positiveSoFar = positiveTotal; 
            int negativeSoFar = 0;

            double prevThresholdToMeet = 2.0;

            string goodLabeledBad = string.Format("{0}Labeled{1}", PositiveClassName, NegativeClassName);
            string badLabeledGood = string.Format("{0}Labeled{1}", NegativeClassName, PositiveClassName); 
 

            curveTextWriter.WriteLine(SpecialFunctions.CreateTabString( 
                OtherInfoHeader,
                "Is" + PositiveClassName,
                "ThresholdToMeetToBeLabeled" + PositiveClassName,
                goodLabeledBad + "Count", badLabeledGood + "Count",
                goodLabeledBad + "Fraction",
                badLabeledGood + "Fraction")); 
 
            if (rocRowCollection.Count > 0 && rocRowCollection[0].Prediction != 1.0)
            { 
                prevThresholdToMeet = 1.0;
                curveTextWriter.WriteLine(SpecialFunctions.CreateTabString(
                    SameNumberOfTabs(OtherInfoHeader),
                    "",
                    prevThresholdToMeet,
                    positiveSoFar, negativeSoFar, 
                    (double)positiveSoFar / (double)positiveTotal, 
                    (double)negativeSoFar / (double)negativeTotal));
 
            }

            double previousXFraction = 1;

            for (int iRocRow = 0; iRocRow < rocRowCollection.Count; )
            { 
                Debug.Assert(rocRowCollection[iRocRow].Prediction < prevThresholdToMeet); // real assert 
                prevThresholdToMeet = rocRowCollection[iRocRow].Prediction;
 
                int endIRocRowWithSamePrediction = SearchAheadToFindARowWithADifferentPrediction(rocRowCollection, iRocRow);

                //Find the counts with the current prediction as the threshold
                for (int i2 = iRocRow; i2 < endIRocRowWithSamePrediction; ++i2)
                {
                    if (rocRowCollection[i2].Label) 
                    { 
                        --positiveSoFar;
                    } 
                    else
                    {
                        ++negativeSoFar;

                    }
                } 
 

                //Find output the current set of rows with the errors based on these counts 
                for (int i2 = iRocRow; i2 < endIRocRowWithSamePrediction; ++i2)
                {
                    double xFraction = (double)positiveSoFar / (double)positiveTotal;
                    double yFraction = (double)negativeSoFar / (double)negativeTotal;
                    curveTextWriter.WriteLine(SpecialFunctions.CreateTabString(
                        rocRowCollection[i2].OtherInfo, 
                        rocRowCollection[i2].Label, 
                        rocRowCollection[i2].Prediction,
                        positiveSoFar, negativeSoFar, 
                        xFraction,
                        yFraction));

                    if (yFraction <= goalYFraction)
                    {
                        threshold = rocRowCollection[i2].Prediction; 
                        resultXFraction = xFraction; 
                    }
 
                    foreach (Pair<double, double> bounds in aucBoundPairSet)
                    {
                        double low = (bounds.First != bounds.Second) ? bounds.First : bounds.First - eps;
                        double high = (bounds.First != bounds.Second) ? bounds.Second : bounds.Second + eps;
                        double width = high - low;
                        SpecialFunctions.CheckCondition(width > 0.0, "The lower AUC bound must be less than higher one"); 
 
                        double previousXFractionInBound = SpecialFunctions.Bound(low, high, previousXFraction);
                        double xFractionInBound = SpecialFunctions.Bound(low, high, xFraction); 
                        double increment = (previousXFractionInBound - xFractionInBound) * yFraction / width;
                        boundsToMeanY[bounds] = increment + boundsToMeanY[bounds];
                    }
                    previousXFraction = xFraction;
                }
 
                iRocRow = endIRocRowWithSamePrediction; 
            }
 

            curveTextWriter.Flush();

            Debug.Assert(positiveSoFar == 0); // real assert
            Debug.Assert(negativeSoFar == negativeTotal);
 
            return boundsToMeanY; 
        }
 
        private static Dictionary<Pair<double, double>, double> CreateBoundsToAuc(Set<Pair<double, double>> aucBoundPairSet)
        {
            Dictionary<Pair<double, double>, double> boundsToAuc = new Dictionary<Pair<double, double>, double>();
            foreach (Pair<double, double> bounds in aucBoundPairSet)
            {
                boundsToAuc.Add(bounds, 0.0); 
            } 
            return boundsToAuc;
        } 

        private static int SearchAheadToFindARowWithADifferentPrediction(List<RocRow<T>> rocRowCollection, int iRocRow)
        {
            int endIRocRowWithSamePrediction = iRocRow + 1;
            while (true)
            { 
                if (endIRocRowWithSamePrediction == rocRowCollection.Count) 
                {
                    break; 
                }
                if (rocRowCollection[endIRocRowWithSamePrediction].Prediction != rocRowCollection[iRocRow].Prediction)
                {
                    break;
                }
                ++endIRocRowWithSamePrediction; 
            } 
            return endIRocRowWithSamePrediction;
        } 

        private string SameNumberOfTabs(string OtherInfoHeader)
        {
            string output = new string('\t', OtherInfoHeader.Split('\t').Length - 1);
            return output;
        } 
 
        //static public List<double> RankPositives(List<RocRow<T>> rocRowCollection)
        //{ 
        //    rocRowCollection.Sort(
        //        delegate(RocRow x, RocRow y)
        //        {
        //            return y.Prediction.CompareTo(x.Prediction); //opposite of the report
        //        });
 
        //    List<double> rankPositives = new List<double>(); 
        //    for (int iRocRow = 0; iRocRow < rocRowCollection.Count; ++iRocRow)
        //    { 
        //        RocRow rocRow = rocRowCollection[(int)iRocRow];
        //        if (rocRow.Label)
        //        {
        //            double rank = Rank(rocRowCollection, rocRow);
        //            rankPositives.Add(rank);
        //        } 
        //    } 
        //    return rankPositives;
        //} 

        ///// <summary>
        ///// !!!could be done much faster
        ///// </summary>
        ///// <param name="rocRowCollection"></param>
        ///// <param name="rocRow"></param> 
        ///// <returns></returns> 
        //private static double Rank(List<RocRow<T>> rocRowCollection, RocRow<T> rocRowTarget)
        //{ 
        //    int rowsBetterThanTarget = 0;
        //    int rowsSameThanTarget = 0;
        //    foreach (RocRow rocRow in rocRowCollection)
        //    {
        //        if (rocRow.Prediction > rocRowTarget.Prediction)
        //        { 
        //            ++rowsBetterThanTarget; 
        //        }
        //        else if (rocRow.Prediction == rocRowTarget.Prediction) 
        //        {
        //            ++rowsSameThanTarget;
        //        }
        //    }
        //    int bestRank = rowsBetterThanTarget + 1;
        //    int worseRank = rowsBetterThanTarget + rowsSameThanTarget; 
        //    double rank = (double)(bestRank + worseRank) / 2.0; 
        //    return rank;
        //} 


        private static bool DoOutput(int iRocRow, int cRocRow, int lineLimit)
        {
            if (lineLimit == int.MaxValue)
            { 
                return true; 
            }
 
            //I worked out this formula with Excel and Mathematica
            int prevSlot = (lineLimit + 2 * (iRocRow - 1) * lineLimit - cRocRow) / (2 * cRocRow);
            int thisSlot = (lineLimit + 2 * iRocRow * lineLimit - cRocRow) / (2 * cRocRow);
            return thisSlot != prevSlot;
        }
 
 
        private static int CountPositives(List<RocRow<T>> rocRowCollection)
        { 
            int positiveCount = 0;
            rocRowCollection.ForEach(
                delegate(RocRow<T> rocRow)
                {
                    if (rocRow.Label)
                    { 
                        ++positiveCount; 
                    }
                }); 
            return positiveCount;
        }

        static public void UnitTest()
        {
            //!!!still need to test linelimit, filtering, and ties 
 
            Roc<int> roc = Roc<int>.GetInstance("the index", "GoodEmail", "Spam");
            Random random = new Random("Roc.UnitTest".GetHashCode()); 

            for (int i = 0; i < 1000; ++i)
            {
                double prediction = random.NextDouble();
                bool isTrue = Math.Sqrt(prediction) > random.NextDouble();
                roc.Add(i, isTrue, prediction); 
            } 

            TextWriter textWriter = new StreamWriter(Console.OpenStandardOutput()); 
            roc.ReportRocCurve(textWriter);
        }

        public Dictionary<Pair<double, double>, double> ReportRocCurve(string outputCurveFileName)
        {
            double threshold; 
            double resultXFraction; 
            return ReportRocCurve(outputCurveFileName, DefaultAucBoundPair, 1.0, out threshold, out resultXFraction);
        } 

        public Dictionary<Pair<double, double>, double> ReportRocCurve(string outputCurveFileName, Set<Pair<double, double>> aucBoundPairSetOrNull, double yFractionGoal, out double threshold, out double resultXFraction)
        {
            using (TextWriter curveTextWriter = File.CreateText(outputCurveFileName))
            {
                return ReportRocCurve(curveTextWriter, aucBoundPairSetOrNull, yFractionGoal, out threshold, out resultXFraction); 
            } 
        }
 
        public void Append(string inputROCFileName)
        {
            SpecialFunctions.CheckCondition(!OtherInfoHeader.Contains("\t"), "'Append' doesn't support otherInfoHeader with tabs");
            foreach (Dictionary<string, string> row in SpecialFunctions.Rest(SpecialFunctions.TabFileTable(inputROCFileName, "NecAndHla	Isgood	ThresholdToMeetToBeLabeledgood	goodLabeledbadCount	badLabeledgoodCount	goodLabeledbadFraction	badLabeledgoodFraction", false)))
            {
                T otherInfo; 
                bool isOK = SpecialFunctions.TryParse<T>(row[OtherInfoHeader], out otherInfo); 
                SpecialFunctions.CheckCondition(isOK, "Could not parse " + OtherInfoHeader);
                bool label = bool.Parse(row["Is" + PositiveClassName]); 
                double probability = double.Parse(row["ThresholdToMeetToBeLabeled" + PositiveClassName]);
                Add(otherInfo, label, probability);
            }
        }

        public void Add(RocRow<T> rocRow) 
        { 
            Add(rocRow.OtherInfo, rocRow.Label, rocRow.Prediction);
        } 

        public double MeanMinusLog2POutcome()
        {
            double mean = SpecialFunctions.Mean(MinusLog2OutcomeEnumeration());
            return mean;
        } 
 
        public double MedianMinusLog2POutcome()
        { 
            double median = SpecialFunctions.Median(MinusLog2OutcomeEnumeration());
            return median;
        }

        private IEnumerable<double> MinusLog2OutcomeEnumeration()
        { 
            foreach (RocRow<T> rocRow in RocRowCollection) 
            {
                double minusLog2Outcome = -Math.Log(rocRow.POfOutcome(), 2); 
                yield return minusLog2Outcome;
            }
        }


        public double FindThreshold(double goalYFraction, out double resultXFraction) 
        { 
            double threshold;
            using (TextWriter ignore = new StringWriter()) 
            {
                ReportRocCurve(ignore, DefaultAucBoundPair, goalYFraction, out threshold, out resultXFraction);
            }
            return threshold;
        }
 
        public List<bool> LabelsSortedByPrediction() 
        {
            List<RocRow<T>> copy = new List<RocRow<T>>(RocRowCollection); 
            copy.Sort(BiggerPrediction);
            List<bool> sortedLabels = new List<bool>();
            foreach (RocRow<T> rocRow in copy)
            {
                sortedLabels.Add(rocRow.Label);
            } 
            return sortedLabels; 
        }
 

        static int BiggerPrediction(RocRow<T> rocRow0, RocRow<T> rocRow1)
        {
            return rocRow1.Prediction.CompareTo(rocRow0.Prediction);
        }
    } 
 

    public class RocRow<T> 

    {
        private RocRow()
        {
        }
 
        public bool Label; 
        public double Prediction;
        public double Confidence = 1.0; 
        public T OtherInfo;

        internal static RocRow<T> GetInstance(T otherInfo, bool label, double prediction)
        {
            RocRow<T> rocRow = new RocRow<T>();
            rocRow.OtherInfo = otherInfo; 
            rocRow.Label = label; 
            rocRow.Prediction = prediction;
            return rocRow; 
        }



        public double POfOutcome()
        { 
             return Label ? Prediction : (1.0 - Prediction); 
        }
    } 
}



// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
