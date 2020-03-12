using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;

namespace Optimization 
{ 
    abstract public class OneDOptimization
    { 
        public int DebugCount = -1;

        internal OneDOptimization()
        {
        }
 
        abstract public double Run(Converter<OptimizationParameter, double> oneDRealFunction, OptimizationParameter paramToOptimize, int gridLineCount, out double bestInput); 

        public static OneDOptimization GetInstance(string optimizerName) 
        {

            //Console.WriteLine("Using {0} for 1D optimization.", optimizerName);
            if (optimizerName.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                optimizerName = "brentthengrid"; 
            } 

            optimizerName = optimizerName.ToLower(); 
            if (optimizerName == "grid")
            {
                return new OneDGrid();
            }

            string brentwithnogrid = "brentwithnogrid"; 
            if (optimizerName.StartsWith(brentwithnogrid)) 
            {
                double tol = GetTolOrDefault(optimizerName, brentwithnogrid); 
                return new Brent(tol);
            }

            string brentthengrid = "brentthengrid";
            if (optimizerName.StartsWith(brentthengrid))
            { 
                double tol = GetTolOrDefault(optimizerName, brentthengrid); 
                return new BrentThenGrid(tol);
            } 

            SpecialFunctions.CheckCondition(false, string.Format("The optimizer named {0} is no known", optimizerName));
            return null;

        }
 
        private static double GetTolOrDefault(string optimizerName, string brentwithnogrid) 
        {
            string tolAsString = optimizerName.Substring(brentwithnogrid.Length); 
            double tol = (tolAsString == "") ? .001 : double.Parse(tolAsString);
            return tol;
        }
    }
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
