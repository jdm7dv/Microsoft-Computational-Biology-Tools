using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using VirusCount;
 
namespace Optimization 
{
    public class OneDGrid : OneDOptimization 
    {
        static double Precision = .01;

        internal OneDGrid()
        {
        } 
        override public double Run(Converter<OptimizationParameter, double> oneDRealFunction, 
                OptimizationParameter param, int gridLineCount, out double bestInput)
        { 
            double low = param.LowForSearch;
            double high = param.HighForSearch;
            Converter<double, double> oneDRealFunctionInDoubleSpace = delegate(double d)
            {
                param.ValueForSearch = d;
                return oneDRealFunction(param); 
            }; 

            double initParamValue = param.ValueForSearch; 
            BestSoFar<double, double> bestFound = OneDOptimizationX(oneDRealFunctionInDoubleSpace, low, high, gridLineCount);
            bestFound.Compare(oneDRealFunctionInDoubleSpace(initParamValue), initParamValue); // make sure we didn't get any worse

            bestInput = bestFound.Champ;
            param.ValueForSearch = bestFound.Champ;
            return bestFound.ChampsScore; 
        } 

        static public BestSoFar<double, double> OneDOptimizationX(Converter<double, double> oneDRealFunction, 
                double low, double high, int gridLineCount)
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

            if (high - low < Precision)
            {
                return bestParameterSoFar;
            } 
 
            return OneDOptimizationX(oneDRealFunction, bestParameterSoFar.Champ - rangeIncrement, bestParameterSoFar.Champ + rangeIncrement,
                gridLineCount); 
        }

        private BestSoFar<double, double> OneDOptimizationX(double parameterInit, Converter<double, double> oneDRealFunction)
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
                double parameter = SpecialFunctions.InverseLogOdds(parameterLogOdds);
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

 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
