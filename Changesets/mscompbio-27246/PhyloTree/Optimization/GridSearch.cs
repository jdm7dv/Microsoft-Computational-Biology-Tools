using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using Optimization;
using VirusCount; 
 
namespace Optimization
{ 
    public class GridSearch
    {
        OneDOptimization OneDOptimization;

        internal GridSearch()
        { 
        } 

        public static GridSearch GetInstance(string optimizerName) 
        {
            GridSearch gridSearch = new GridSearch();
            gridSearch.OneDOptimization = OneDOptimization.GetInstance(optimizerName);
            return gridSearch;
        }
 
        public double Optimize(Converter<OptimizationParameterList,double> functionToOptimize, 
            //ref List<double> pointList, List<double> lowList, List<double> highList,
            /*ref*/ OptimizationParameterList paramList, int numberOfIterationsOverParameters, int gridLineCount) 
        {
            double eps = 1e-9;

            List<double> pointList = paramList.ExtractParameterValueListForSearch();
            List<double> lowList = paramList.ExtractParameterLowListForSearch();
            List<double> highList = paramList.ExtractParameterHighListForSearch(); 
 
            double oldScore = double.NaN;
            double tempOldScore = functionToOptimize(paramList); 
            //Console.WriteLine(tempOldScore);
            Dictionary<int, double> paramNumberToInitValue = new Dictionary<int, double>();
            for (int iterationOverParameters = 0; iterationOverParameters < numberOfIterationsOverParameters; ++iterationOverParameters)
            {
                double newScore = double.NaN;
                int doSearchParameterCount = 0; 
                foreach(OptimizationParameter paramToOptimize in paramList) 
                {
                    //if (paramList.DoSearch(iParameter)) 
                    if (paramToOptimize.DoSearch)
                    {
                        ++doSearchParameterCount;

                        //double initValue = SpecialFunctions.GetValueOrDefault(paramNumberToInitValue, doSearchParameterCount, paramToOptimize.Value);
                        Debug.WriteLine("Search Param " + doSearchParameterCount.ToString()); 
 
                        double bestInput;
                        newScore = OneDOptimization.Run( 
                            delegate(OptimizationParameter param)
                            {
                                if (!double.IsNaN(param.Value) && paramList.SatisfiesConditions())
                                {
                                    return functionToOptimize(paramList);
                                } 
                                else 
                                {
                                    return double.NaN; 
                                }
                            },
                            /*ref*/ paramToOptimize, gridLineCount, out bestInput);

                        Debug.WriteLine("bestInput " + bestInput.ToString());
                        if (newScore > 0) 
                        { 
                            Debug.Write("stop.");
                        } 
                        paramToOptimize.ValueForSearch = bestInput;

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
 



        public static void Plot(Converter<double, double> oneDRealFunctionInDoubleSpace, double start, double last, double inc)
        {
            for (double r1 = start; r1 < last; r1 += inc) 
            { 
                Debug.WriteLine(SpecialFunctions.CreateTabString(r1, oneDRealFunctionInDoubleSpace(r1)));
            } 
        }


        public int DebugCount
        {
            get 
            { 
                return OneDOptimization.DebugCount;
            } 
        }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
