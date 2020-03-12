using System; 
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
 
namespace VirusCount 
{
    public class RocRow 
    {
        private RocRow()
        {
        }

 
        long CrossValIndex; 
        public MerAndHlaToLength MerAndHlaToLength;
        public bool Label; 
        public double Prediction;
        public double Confidence = 1.0;

        public static RocRow GetInstance(long crossValIndex, KeyValuePair<MerAndHlaToLength, bool> merAndHlaToLengthWithLabel, double prediction)
        {
            RocRow rocRow = new RocRow(); 
            rocRow.CrossValIndex = crossValIndex; 
            rocRow.MerAndHlaToLength = merAndHlaToLengthWithLabel.Key;
            rocRow.Label = merAndHlaToLengthWithLabel.Value; 
            rocRow.Prediction = prediction;
            return rocRow;
        }

        public static void ReportRocCurve(List<RocRow> rocRowCollection, string fileName, bool isComplete, long lineLimit, HlaFilter hlaFilterOrNull)
 		{ 
			if (hlaFilterOrNull != null) 
			{
				fileName = fileName + "FilteredOn" + hlaFilterOrNull.Name; 
				rocRowCollection = hlaFilterOrNull.Filter(rocRowCollection);
			}


 			Debug.WriteLine(fileName);
 
 
 			using (TextWriter evalStream = File.CreateText(fileName))
            { 

                rocRowCollection.Sort(
                    delegate(RocRow x, RocRow y)
                    {
                        return y.Prediction.CompareTo(x.Prediction);
                    }); 
 
                long positiveTotal = CountPositives(rocRowCollection);
                long negativeTotal = rocRowCollection.Count - positiveTotal; 

                long positiveSoFar = positiveTotal;
                long negativeSoFar = 0;

                double prevPrediction = 1.0;
 
                if (!isComplete) 
                {
                    evalStream.WriteLine("WARNING: The weights files were incomplete"); 
                }

                evalStream.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                    "CrossValIndex", "Label",
                    "Mer", "Hla",
                    "Prediction", 
                    "FalsePositiveCount", "FalseNegativeCount", 
                    rocRowCollection.Count > lineLimit ? "SampledFalsePositiveFraction" : "FalsePositiveFraction",
                    rocRowCollection.Count > lineLimit ? "SampledFalseNegativeFraction" : "FalseNegativeFraction"); 

                evalStream.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                    "", "",
                    "", "",
                    1.0,
                    positiveSoFar, negativeSoFar, 
                    (double)positiveSoFar / (double)positiveTotal, 
                    (double)negativeSoFar / (double)negativeTotal);
 

                for(long iRocRow = 0; iRocRow < rocRowCollection.Count; ++iRocRow)
                {

                    RocRow rocRow = rocRowCollection[(int)iRocRow];
                    Debug.Assert(prevPrediction >= rocRow.Prediction); // real assert 
                    //if (prevPrediction > rocRow.Prediction) 
                    //{
                    //    Console.WriteLine("WARNING: There needs to be a code change for when there are predictions with ties. Consider switching to SpecialFunctions.Roc (but it doesn't currently have sampling or filtering)"); 
                    //}

                    prevPrediction = rocRow.Prediction;
                    if (rocRow.Label)
                    {
                        --positiveSoFar; 
                    } 
                    else
                    { 
                        ++negativeSoFar;
                    }

                    if (DoOutput(iRocRow, rocRowCollection.Count, lineLimit))
                    {
                        evalStream.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", 
                            rocRow.CrossValIndex, rocRow.Label, 
                            rocRow.MerAndHlaToLength.Mer, rocRow.MerAndHlaToLength.HlaToLength,
                            rocRow.Prediction, 
                            positiveSoFar, negativeSoFar,
                            (double)positiveSoFar / (double)positiveTotal,
                            (double)negativeSoFar / (double)negativeTotal);
                    }
                }
                Debug.Assert(positiveSoFar == 0); // real assert 
                Debug.Assert(negativeSoFar == negativeTotal); 
            }
        } 

		static public List<double> RankPositives(List<RocRow> rocRowCollection)
 		{
			rocRowCollection.Sort(
				delegate(RocRow x, RocRow y)
				{ 
					return y.Prediction.CompareTo(x.Prediction); //opposite of the report 
				});
 
 			List<double> rankPositives = new List<double>();
 			for (long iRocRow = 0; iRocRow < rocRowCollection.Count; ++iRocRow)
			{
 				RocRow rocRow = rocRowCollection[(int)iRocRow];
				if (rocRow.Label)
				{ 
					double rank = Rank(rocRowCollection, rocRow); 
					rankPositives.Add(rank);
				} 
 			}
 			return rankPositives;
		}

 		/// <summary>
		/// !!!could be done much faster 
		/// </summary> 
		/// <param name="rocRowCollection"></param>
		/// <param name="rocRow"></param> 
		/// <returns></returns>
 		private static double Rank(List<RocRow> rocRowCollection, RocRow rocRowTarget)
 		{
			int rowsBetterThanTarget = 0;
 			int rowsSameThanTarget = 0;
			foreach(RocRow rocRow in rocRowCollection) 
			{ 
				if (rocRow.Prediction > rocRowTarget.Prediction)
				{ 
					++rowsBetterThanTarget;
 				}
 				else if (rocRow.Prediction == rocRowTarget.Prediction)
				{
 					++rowsSameThanTarget;
				} 
			} 
			int bestRank = rowsBetterThanTarget + 1;
			int worseRank = rowsBetterThanTarget + rowsSameThanTarget; 
			double rank = (double)(bestRank + worseRank) / 2.0;
 			return rank;
 		}


        private static bool DoOutput(long iRocRow, int cRocRow, long lineLimit) 
        { 
            //I worked out this formula with Excel and Mathematica
            long prevSlot = (lineLimit + 2 * (iRocRow-1) * lineLimit - cRocRow) / (2 * cRocRow); 
            long thisSlot = (lineLimit + 2 * iRocRow * lineLimit - cRocRow) / (2 * cRocRow);
            return thisSlot != prevSlot;
        }


        public static long CountPositives(List<RocRow> rocRowCollection) 
        { 
            long positiveCount = 0;
            rocRowCollection.ForEach( 
                delegate(RocRow rocRow)
                {
                    if (rocRow.Label)
                    {
                        ++positiveCount;
                    } 
                }); 
            return positiveCount;
        } 
    }


}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
