using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using VirusCount.Qmrr;
using ProcessingPrediction; 
 
namespace EpipredLib
{ 
    public class Prediction
    {
        private Prediction()
        {
        }
 
        public string InputPeptide; 
        public Hla Hla;
        public double PosteriorProbability; 
        public double WeightOfEvidence;
        public NEC NEC;
        public int EStartPosition;
        public int ELastPosition;
        public string Source;
 
        public static Prediction GetInstance(string inputPeptide, Hla hla, double posteriorProbability, double weightOfEvidence, NEC nec, int eStartPosition, int eLastPosition, string source) 
        {
            Prediction prediction = new Prediction(); 
            prediction.InputPeptide = inputPeptide;
            prediction.Hla = hla;
            prediction.PosteriorProbability = posteriorProbability;
            prediction.WeightOfEvidence = weightOfEvidence;
            prediction.NEC = nec;
            prediction.EStartPosition = eStartPosition; 
            prediction.ELastPosition = eLastPosition; 
            prediction.Source = source;
            return prediction; 
        }

        public override string ToString()
        {
            return ToString(true, true);
        } 
 
        public string ToString(bool includeInputPeptide, bool includeHlaInOutput)
        { 
            StringBuilder sb = new StringBuilder();
            bool needTab = false;
            if (includeInputPeptide)
            {
                sb.Append(InputPeptide);
                needTab = true; 
            } 

            if (includeHlaInOutput) 
            {
                if (needTab)
                {
                    sb.Append('\t');
                }
                sb.Append(Hla); 
                needTab = true; 
            }
 
            if (needTab)
            {
                sb.Append('\t');
            }

            Debug.Assert(NEC.N.Length == NEC.C.Length); // real assert 
            sb.Append(SpecialFunctions.CreateTabString(PosteriorProbability, WeightOfEvidence, NEC.N, NEC.E, NEC.C, NEC.E.Length, NEC.N.Length, EStartPosition, ELastPosition, Source)); 
            return sb.ToString();
        } 

        static string RestOfExtra = SpecialFunctions.CreateTabString("PosteriorProbability", "WeightOfEvidence", "BestNFlank", "BestEpitope", "BestCFlank", "EpitopeLength", "FlankingLength", "EpitiopeStartPosition", "EpitopeLastPosition", "Source");
        public static string ExtraHeader(bool includeHlaInOutput)
        {
            if (includeHlaInOutput)
            { 
                return SpecialFunctions.CreateTabString("BestHla", RestOfExtra); 
            }
            else 
            {
                return RestOfExtra;
            }
        }

        public static string CollectionToString(List<Prediction> predictionList, bool includeInputPeptide, bool includeHlaInOutput) 
        { 
            if (predictionList.Count == 1)
            { 
                return predictionList[0].ToString(includeInputPeptide, includeHlaInOutput);
            }

            SpecialFunctions.CheckCondition(predictionList.Count > 1);

            List<List<string>> rowList = CreateRowList(predictionList, includeInputPeptide, includeHlaInOutput); 
            IEnumerable<string> outputList = CreateRowWithVariations(rowList); 
            return SpecialFunctions.CreateTabString2(outputList);
 
        }

        private static List<List<string>> CreateRowList(List<Prediction> predictionList, bool includeInputPeptide, bool includeHlaInOutput)
        {
            List<List<string>> rowList = new List<List<string>>();
            foreach (Prediction prediction in predictionList) 
            { 
                string s = prediction.ToString(includeInputPeptide, includeHlaInOutput);
                List<string> fieldCollection = SpecialFunctions.Split(s, '\t'); 
                rowList.Add(fieldCollection);
            }
            return rowList;
        }

        private static IEnumerable<string> CreateRowWithVariations(List<List<string>> rowList) 
        { 
            foreach (List<string> columnList in SpecialFunctions.Transpose(rowList))
            { 
                Debug.Assert(columnList.Count > 0); // real assert
                bool valueIsUnanimous = IsValueUnanimous(columnList);

                if (valueIsUnanimous)
                {
                    yield return columnList[0]; 
                } 
                else
                { 
                    yield return SpecialFunctions.Join(",", columnList);
                }
            }
        }

        private static bool IsValueUnanimous(List<string> columnList) 
        { 
            foreach (string other in SpecialFunctions.Rest(columnList))
            { 
                if (other != columnList[0])
                {
                    return false;
                }
            }
            return true; 
        } 

        internal object GroupByKey(ShowBy showBy) 
        {
            switch (showBy)
            {
                case ShowBy.all:
                    return "all";
                case ShowBy.hla: 
                    return Hla; 
                case ShowBy.length:
                    return NEC.E.Length; 
                case ShowBy.hlaAndLength:
                    return new Pair<Hla, int>(Hla, NEC.E.Length);
                case ShowBy.doNotGroup:
                    return this;
                default:
                    SpecialFunctions.CheckCondition(false, "Don't know how to showBy " + showBy.ToString()); 
                    return null; 

            } 
        }

    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
