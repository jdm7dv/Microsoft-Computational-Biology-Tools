using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using Optimization;
 
namespace VirusCount.Qmr 
{
    public delegate double FunctionToOptimizeDelegate(List<double> parameterList); 

    public class GridSearch
    {
        static public double Optimize(FunctionToOptimizeDelegate functionToOptimize,
            ref List<double> point, List<double> low, List<double> high,
            OptimizationParameterList qmrrParamsStart, int numberOfIterationsOverParameters, double precision, int gridLineCount) 
        { 
            double eps = 1e-9;
 


            //Debug.WriteLine(qmrrParamsStart.ToStringHeader());

            double oldScore = double.NaN;
            //Debug.WriteLine(SpecialFunctions.CreateTabString("iterationOverParameters", "iParameter", "Probability", "LogOdds", "Score")); 
            for (int iterationOverParameters = 0; iterationOverParameters < numberOfIterationsOverParameters; ++iterationOverParameters) 
            {
                double newScore = double.NaN; 
                int doSearchParameterCount = 0;
                for (int iParameter = 0; iParameter < point.Count; ++iParameter)
                {
                    if (qmrrParamsStart.DoSearch(iParameter))
                    {
                        ++doSearchParameterCount; 
                        List<double> pointClone = new List<double>(point); 

                        BestSoFar<double, double> bestParamSoFar = 
                            OneDOptimization(
                                delegate(double parameter) { pointClone[iParameter] = parameter; return functionToOptimize(pointClone); },
                                low[iParameter], high[iParameter], precision, gridLineCount);
                        point[iParameter] = bestParamSoFar.Champ;
                        //Debug.WriteLine(SpecialFunctions.CreateTabString(iterationOverParameters, iParameter, SpecialFunctions.Probability(bestParamSoFar.Champ), bestParamSoFar.Champ, bestParamSoFar.ChampsScore));
                        newScore = bestParamSoFar.ChampsScore; 
 
                        //Debug.WriteLine("END ITER:" + SpecialFunctions.CreateTabString2(point) + SpecialFunctions.CreateTabString("", newScore));
                    } 
                }
                if ((!double.IsNaN(oldScore) && Math.Abs(oldScore - newScore) < eps)
                    || doSearchParameterCount < 2) //If only 0 or 1 searchable params, then one pass is enough
                {
                    oldScore = newScore;
                    break; 
                } 
                oldScore = newScore;
            } 
            return oldScore;
        }

        private BestSoFar<double, double> OneDOptimization(double parameterInit, Converter<double, double> oneDRealFunction)
        {
            return OneDOptimizationInternal(oneDRealFunction, SpecialFunctions.LogOdds(.001), SpecialFunctions.LogOdds(.999), .001); 
        } 

        private BestSoFar<double, double> OneDOptimizationInternal(Converter<double, double> oneDRealFunction, 
                double logOddsLow, double logOddsHigh, double logOddsPrecision)
        {
            int gridLineCount = 10;

            double logOddsRangeIncrement = (logOddsHigh - logOddsLow) / (double)gridLineCount;
 
            BestSoFar<double, double> bestParameterSoFar = BestSoFar<double, double>.GetInstance(SpecialFunctions.DoubleGreaterThan); 

            Debug.WriteLine(SpecialFunctions.CreateTabString("parameter", "parameterLogOdds", "score")); 
            for (int gridLine = 0; gridLine <= gridLineCount; ++gridLine)
            {
                double parameterLogOdds = logOddsLow + gridLine * logOddsRangeIncrement;
                double parameter = SpecialFunctions.Probability(parameterLogOdds);
                double score = oneDRealFunction(parameter);
                Debug.WriteLine(SpecialFunctions.CreateTabString(parameter, parameterLogOdds, score)); 
                bestParameterSoFar.Compare(score, parameterLogOdds); 
            }
            Debug.WriteLine(""); 

            if (logOddsHigh - logOddsLow < logOddsPrecision)
            {
                return bestParameterSoFar;
            }
 
            return OneDOptimizationInternal(oneDRealFunction, bestParameterSoFar.Champ - logOddsRangeIncrement, bestParameterSoFar.Champ + logOddsRangeIncrement, logOddsPrecision); 
        }
 

        //private BestSoFar<double, double> OneDOptimizationInternal(oneDRealFunctionDelegate oneDRealFunction,
        //        double start, double increment, int gridStart, int gridLines)
        //{

        //    BestSoFar<double, double> bestParameterSoFar = BestSoFar<double, double>.GetInstance(delegate(double champScore, double challengerScore) { return challengerScore > champScore; }); 
        //    for(int gridLine = gridStart; gridLine < gridLines; ++gridLine) 
        //    {
        //        double parameter = start + gridLine * increment; 
        //        double score = oneDRealFunction(parameter);
        //        bestParameterSoFar.Compare(score, parameter);
        //    }
        //    if (Math.Abs(bestParameterSoFar.Champ - increment) < increment * .1)
        //    {
        //        return OneDOptimizationInternal(oneDRealFunction, 0.0, increment * .1, 1, 9); 
        //    } 
        //    else if (Math.Abs((1.0 - bestParameterSoFar.Champ) - increment) < increment * .1)
        //    { 
        //        return OneDOptimizationInternal(oneDRealFunction, bestParameterSoFar.Champ, increment * .1, 1, 9);
        //    }
        //    else
        //    {
        //        return bestParameterSoFar;
        //    } 
        //} 

        static public BestSoFar<double, double> OneDOptimization(Converter<double, double> oneDRealFunction, 
                double low, double high, double precision, int gridLineCount)
        {


            double rangeIncrement = (high - low) / (double)gridLineCount;
 
            BestSoFar<double, double> bestParameterSoFar = BestSoFar<double, double>.GetInstance(SpecialFunctions.DoubleGreaterThan); 

            //Debug.WriteLine(SpecialFunctions.CreateTabString("parameter", "score")); 
            //Debug.WriteLine(SpecialFunctions.CreateTabString(low, high, rangeIncrement));
            for (int gridLine = 0; gridLine <= gridLineCount; ++gridLine)
            {
                double parameter = low + gridLine * rangeIncrement;
                //double parameter = SpecialFunctions.Probability(parameterLogOdds);
                double score = oneDRealFunction(parameter); 
                //Debug.WriteLine(SpecialFunctions.CreateTabString(parameter, score)); 
                bestParameterSoFar.Compare(score, parameter);
            } 
            //Debug.WriteLine("");

            if (high - low < precision)
            {
                return bestParameterSoFar;
            } 
 
            return OneDOptimization(oneDRealFunction, bestParameterSoFar.Champ - rangeIncrement, bestParameterSoFar.Champ + rangeIncrement,
                precision, gridLineCount); 
        }




        //static private string ToString(bool[] paramToSearch) 
        //{ 
        //    StringBuilder sb = new StringBuilder();
        //    foreach (bool b in paramToSearch) 
        //    {
        //        if (sb.Length > 0)
        //        {
        //            sb.Append('\t');
        //        }
        //        sb.Append(b.ToString()); 
        //    } 
        //    return sb.ToString();
        //} 

    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
