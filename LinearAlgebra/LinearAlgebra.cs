using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;

namespace Msr.Mlas.LinearAlgebra 
{ 
    public class LinearAlgebra
    { 
        private static List<double> FactorialTable;

        private Dictionary<Pair<EigenPair, double>, double[][]> _matrixExpCache = new Dictionary<Pair<EigenPair, double>, double[][]>(1000);

        static LinearAlgebra()
        { 
            FactorialTable = new List<double>(); 
            FactorialTable.Insert(0, 1);
            FactorialTable.Insert(1, 1); 
        }

        /// <summary>
        /// Returns the Factorial of x. These computations are cached, so computing Factorial y less than x after Factorial(x) is
        /// guaranteed to be a const time op.
        /// </summary> 
        public static double Factorial(int x) 
        {
            if (x >= FactorialTable.Count) 
            {
                for (int i = FactorialTable.Count; i <= x; i++)
                {
                    FactorialTable.Insert(i, FactorialTable[i - 1] * i);
                }
            } 
            return FactorialTable[x]; 
        }
 
        public double[][] MatrixExpCached(EigenPair eigenPair, double t)
        {
            Pair<EigenPair, double> key = new Pair<EigenPair, double>(eigenPair, t);
            double[][] result;

            result = MatrixExp(eigenPair, t); 
            //if (_matrixExpCache.ContainsKey(key)) 
            //{
            //    result = _matrixExpCache[key]; 
            //    _hits++;
            //}
            //else
            //{
            //    result = MatrixExp(eigenPair, t);
 
            //    if (_matrixExpCache.Count >= 1000) 
            //    {
            //        Console.WriteLine("Clearing EIGENPAIR/BRANCH LENGTH cache. {0}/{1} ({2}%) were hits", _hits, _total, (double)_hits / _total); 
            //        _matrixExpCache.Clear();
            //    }

            //    _matrixExpCache.Add(key, result);
            //}
 
            return result; 
        }
 
        public static double[][] MatrixExp(EigenPair eigenPair, double t)
        {
            ComplexNumber[] values = eigenPair.EigenValues;
            ComplexNumber[] newValues = new ComplexNumber[values.Length];
            for (int i = 0; i < values.Length; i++)
                newValues[i] = ComplexNumber.Exp(values[i] * t); 
 
            return eigenPair.GetRaggedMatrixWithModifiedEigenValues(newValues);
        } 



        public static double[][] MatrixExpViaTaylorExpansion(double[][] squareMatrix)
        {
            const double eps = 1e-6; 
            DoubleArray matrix = new DoubleArray(squareMatrix); 
            DoubleArray runningMatrixPower = new DoubleArray(matrix);
            DoubleArray taylorSum = new DoubleArray(matrix) + DoubleArray.Identity(matrix.Size[0], matrix.Size[1]); 

            double maxDiff = double.MaxValue;

            int i;
            for (i = 2; maxDiff > eps; i++) // i is the factorial computation. taylor sum starts off after the 1st term is computed (ie, just matrix)
            { 
                runningMatrixPower *= matrix; 
                if (double.IsInfinity((double)runningMatrixPower.GetElement(0, 0)))
                    throw new NotFiniteNumberException("The matrix failed to converge. Try the eigen value method."); 

                double multiplier = 1.0 / Factorial(i);
                DoubleArray nextTaylorTerm = runningMatrixPower * multiplier; // operator / isn't defined for this class, but * double is.
                taylorSum += nextTaylorTerm;

                maxDiff = Math.Max(Math.Abs(nextTaylorTerm.Max()), Math.Abs(nextTaylorTerm.Min())); 
            } 
            Debug.WriteLine("MatrixExp iterations: " + i);
 
            return ShoArrayToRaggedArray(taylorSum);
        }

        //public static double[][] MatrixExpViaEigenDecomp2(double[][] squareMatrix)
        //{
        //    DoubleArray matrix = new DoubleArray(squareMatrix); 
        //    Eigen eigenSystem = new Eigen(matrix); 

        //    ComplexArray exponentiatedDiagonal = ExpOfDiagComplex(eigenSystem.D); 

        //    ComplexArray result = eigenSystem.V * exponentiatedDiagonal * eigenSystem.V.Inv();
        //    return ShoArrayToRaggedArray(result);
        //}

 
 

        private static ComplexArray ExpOfDiagComplex(ComplexArray complexArray) 
        {
            double[,] real = new double[complexArray.Size()[0], complexArray.Size()[1]];
            double[,] img = new double[complexArray.Size()[0], complexArray.Size()[1]];
            complexArray.ToArray(ref real, ref img);

            for (int i = 0; i < real.GetLength(0); i++) 
            { 
                real[i, i] = Math.Exp(real[i, i]);
                SpecialFunctions.SpecialFunctions.CheckCondition(img[i, i] == 0, "We can't deal with an imaginary number here."); 
            }

            return new ComplexArray(real, img);
        }

        /// <summary> 
        /// A Full EigenPair has the inverse of the eigen vector matrix comptued. This allows recovery of the initial matrix, but is 
        /// costly if recovery is not needed.
        /// </summary> 
        /// <param name="Q"></param>
        /// <returns></returns>
        public static EigenPair ComputeFullEigenPair(double[][] Q)
        {
            DoubleArray matrix = new DoubleArray(Q);
            Eigen eigenSystem = new Eigen(matrix); 
 
            ComplexNumber[] eigenValues = LinearAlgebra.DiagonalShoArrayToArray(eigenSystem.D);
            ComplexNumber[][] eigenVectors = LinearAlgebra.ShoArrayToRaggedArray(eigenSystem.V); 
            ComplexNumber[][] invEigenVectors = LinearAlgebra.ShoArrayToRaggedArray(eigenSystem.V.Inv());

            return new EigenPair(eigenValues, eigenVectors, invEigenVectors);
        }

        /// <summary> 
        /// A Sparse EigenPair leaves the inverse of the eigen vector matrix null. This means any attempt to reconstruct the 
        /// matrix from the EigenPair object will result in a NullPointerException. However, if this operation is not necessary,
        /// then ignoring the inverse is more efficient. 
        /// </summary>
        /// <param name="Q"></param>
        /// <returns></returns>
        public static EigenPair ComputeSparseEigenPair(double[][] Q)
        {
            DoubleArray matrix = new DoubleArray(Q); 
            Eigen eigenSystem = new Eigen(matrix); 

            ComplexNumber[] eigenValues = LinearAlgebra.DiagonalShoArrayToArray(eigenSystem.D); 
            ComplexNumber[][] eigenVectors = LinearAlgebra.ShoArrayToRaggedArray(eigenSystem.V);

            return new EigenPair(eigenValues, eigenVectors, null);
        }

        internal static double[][] ShoArrayToRaggedArray(DoubleArray shoDoubleArray) 
        { 
            double[][] result = new double[shoDoubleArray.Size[0]][];
 
            for (int i = 0; i < shoDoubleArray.Size[0]; i++)
            {
                result[i] = new double[shoDoubleArray.Size[1]];
                for (int j = 0; j < shoDoubleArray.Size[1]; j++)
                {
                    result[i][j] = (double)shoDoubleArray.GetElement(i,j); 
                } 
            }
            return result; 
        }

        internal static ComplexNumber[] DiagonalShoArrayToArray(ComplexArray shoArray)
        {
            ComplexNumber[,] asMatrix = ShoArrayToArray(shoArray);
            ComplexNumber[] result = new ComplexNumber[shoArray.Size()[0]]; 
 
            for (int i = 0; i < result.Length; i++)
            { 
                result[i] = asMatrix[i,i];
            }
            return result;
        }

        internal static ComplexNumber[][] ShoArrayToRaggedArray(ComplexArray shoComplexArray) 
        { 
            double[,] real = new double[shoComplexArray.Size()[0], shoComplexArray.Size()[1]];
            double[,] img = new double[shoComplexArray.Size()[0], shoComplexArray.Size()[1]]; 

            shoComplexArray.ToArray(ref real, ref img);

            ComplexNumber[][] result = new ComplexNumber[real.GetLength(0)][];

            for (int i = 0; i < result.Length; i++) 
            { 
                result[i] = new ComplexNumber[real.GetLength(1)];
                for (int j = 0; j < result[i].Length; j++) 
                {
                    result[i][j] = new ComplexNumber(real[i, j], img[i,j]);
                }
            }
            return result;
        } 
 
        internal static ComplexNumber[,] ShoArrayToArray(ComplexArray shoComplexArray)
        { 
            double[,] real = new double[shoComplexArray.Size()[0], shoComplexArray.Size()[1]];
            double[,] img = new double[shoComplexArray.Size()[0], shoComplexArray.Size()[1]];

            shoComplexArray.ToArray(ref real, ref img);

            ComplexNumber[,] result = new ComplexNumber[real.GetLength(0), real.GetLength(1)]; 
            for (int i = 0; i < result.GetLength(0); i++) 
                for (int j = 0; j < result.GetLength(1); j++)
                    result[i, j] = new ComplexNumber(real[i, j], img[i, j]); 

            return result;
        }


        public static string MatrixView(Array matrix) 
        { 
            StringBuilder result = new StringBuilder("[ ");
 
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < ((Array)matrix.GetValue(1)).Length; j++)
                {
                    result.Append(string.Format("{0,15:0.0####}", ((Array)matrix.GetValue(i)).GetValue(j)));
                } 
                result.Append(";\n"); 
            }
            result.Append(" ]"); 
            return result.ToString();
        }

        public static string MatrixView(double[,] matrix)
        {
            StringBuilder result = new StringBuilder(); 
            for (int i = 0; i < matrix.GetLength(0); i++) 
            {
                for (int j = 0; j < matrix.GetLength(1); j++) 
                {
                    result.Append(string.Format("{0,15:0.00000}", matrix[i,j]));
                }
                result.Append("\n");
            }
            return result.ToString(); 
        } 

        /// <summary> 
        /// Multiplies x into Q. Modifies Q, then returns it. The return is simply for convenience.
        /// </summary>
        public static double[][] MatrixMultiply(ref double[][] Q, double x)
        {
            for (int i = 0; i < Q.Length; i++)
            { 
                for (int j = 0; j < Q[i].Length; j++) 
                {
                    Q[i][j] *= x; 
                }
            }
            return Q;
        }

        public static double[][] Transpose(double[][] matrix) 
        { 
            double[][] result = new double[matrix[0].Length][];
            for (int i = 0; i < result.Length; i++) 
                result[i] = new double[matrix.Length];

            for (int i = 0; i < result.Length; i++)
                for (int j = 0; j < result[i].Length; j++)
                    result[i][j] = matrix[j][i];
            return result; 
        } 

        public static ComplexNumber[][] Transpose(ComplexNumber[][] matrix) 
        {
            ComplexNumber[][] result = new ComplexNumber[matrix[0].Length][];
            for (int i = 0; i < result.Length; i++)
                result[i] = new ComplexNumber[matrix.Length];

            for (int i = 0; i < result.Length; i++) 
                for (int j = 0; j < result[i].Length; j++) 
                    result[i][j] = matrix[j][i];
            return result; 
        }

        public static double MaxDiff(double[][] Q, double[][] p)
        {
            double maxDiff = double.MinValue;
            for (int i = 0; i < Q.Length; i++) 
                for (int j = 0; j < Q[i].Length; j++) 
                    maxDiff = Math.Max(Math.Abs(Q[i][j] - p[i][j]), maxDiff);
            return maxDiff; 
        }

        public static double[] ComplexToDouble(ComplexNumber[] complexNumber)
        {
            double[] result = new double[complexNumber.Length];
            for (int i = 0; i < complexNumber.Length; i++) 
                result[i] = (double)complexNumber[i]; 
            return result;
        } 

        public static double[] Abs(double[] dArray)
        {
            double[] result = new double[dArray.Length];
            for (int i = 0; i < dArray.Length; i++)
                result[i] = Math.Abs(dArray[i]); 
            return result; 
        }
 
        public static double[] Normalize(double[] dArray)
        {
            double[] result = new double[dArray.Length];
            double sum = 0;
            foreach (double d in dArray)
                sum += d; 
            for (int i = 0; i < dArray.Length; i++) 
                result[i] = dArray[i] / sum;
 
            return result;
        }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
