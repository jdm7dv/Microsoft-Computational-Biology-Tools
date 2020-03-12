using System; 
using System.Collections.Generic;
using System.Text;

namespace Optimization
{
    public class BrentThenGrid : OneDOptimization 
    { 
        OneDOptimization Brent;
        OneDOptimization Grid; 
        public BrentThenGrid(double tol)
        {
            Brent = OneDOptimization.GetInstance("BrentWithNoGrid" + tol.ToString());
            Grid = OneDOptimization.GetInstance("Grid");
        }
        override public double Run(Converter<OptimizationParameter, double> oneDRealFunction, 
            /*ref*/ OptimizationParameter param, int gridLineCount, out double bestInput) 
        {
            try 
            {
                return Brent.Run(oneDRealFunction, param, gridLineCount, out bestInput);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error calling Brent"); 
                Console.WriteLine(exception.Message); 
                if (exception.InnerException != null)
                { 
                    Console.WriteLine(exception.InnerException.Message);
                }
            }

            return Grid.Run(oneDRealFunction, param, gridLineCount, out bestInput);
 
 
        }
    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
