using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;

namespace Msr.Mlas.LinearAlgebra
{ 
    public class EigenPair 
    {
        private readonly ComplexNumber[] _values; 
        private readonly ComplexNumber[][] _vectors;
        private readonly ComplexNumber[][] _invVectors;

        public EigenPair(ComplexNumber[] values, ComplexNumber[][] vectors, ComplexNumber[][] invVectors)
        {
            _values = values; 
            _vectors = vectors; 
            _invVectors = invVectors;
            SpecialFunctions.SpecialFunctions.CheckCondition(_values != null && _vectors != null); 
        }

        /// <summary>
        /// Gets a deep copy of the eigen values.
        /// </summary>
        public ComplexNumber[] EigenValues 
        { 
            get
            { 
                ComplexNumber[] values = new ComplexNumber[_values.Length];
                Array.Copy(_values, values, _values.Length);
                return values;
            }
        }
 
        public ComplexNumber[][] EigenVectors 
        {
            get 
            {
                return _vectors;
            }
        }

        /// <summary> 
        /// Returns the matrix given by VLV^-1, where V is the matrix of eigen vectors and L is 
        /// the diagonal matrix of eigen values.
        /// </summary> 
        public double[][] RaggedMatrix
        {
            get
            {
                return GetRaggedMatrixWithModifiedEigenValues(_values);
            } 
        } 

        /// <summary> 
        /// Returns the matrix given by VLV^-1, where V is the matrix of eigen vectors and L is
        /// the diagonal matrix of eigen values.
        /// </summary>
        public double[,] Matrix
        {
            get 
            { 
                return GetMatrixWithModifiedEigenValues(_values);
            } 
        }

        /// <summary>
        /// Returns the matrix given by VLV^-1, where V is the matrix of eigen vectors and L is
        /// the diagonal matrix of eigen values, privided as the formal argument.
        /// </summary> 
        public double[,] GetMatrixWithModifiedEigenValues(ComplexNumber[] newEigenValues) 
        {
            // first compute VL 
            ComplexNumber[,] intermediate = new ComplexNumber[newEigenValues.Length, newEigenValues.Length];
            for (int i = 0; i < newEigenValues.Length; i++)
            {
                for (int j = 0; j < newEigenValues.Length; j++)
                {
                    intermediate[i, j] = _vectors[i][j] * newEigenValues[j]; 
                } 
            }
 
            double[,] result = new double[newEigenValues.Length, newEigenValues.Length];
            for (int i = 0; i < _vectors.Length; i++)
            {
                for (int j = 0; j < _vectors[i].Length; j++)
                {
                    for (int k = 0; k < newEigenValues.Length; k++) 
                    { 
                        result[i, j] += (double)(intermediate[i, k] * _invVectors[k][j]);
                    } 
                }
            }
            return result;
        }

        /// <summary> 
        /// Returns the matrix given by VLV^-1, where V is the matrix of eigen vectors and L is 
        /// the diagonal matrix of eigen values, privided as the formal argument.
        /// </summary> 
        public double[][] GetRaggedMatrixWithModifiedEigenValues(ComplexNumber[] newEigenValues)
        {
            // first compute VL
            ComplexNumber[,] intermediate = new ComplexNumber[newEigenValues.Length, newEigenValues.Length];
            for (int i = 0; i < newEigenValues.Length; i++)
            { 
                for (int j = 0; j < newEigenValues.Length; j++) 
                {
                    intermediate[i, j] = _vectors[i][j] * newEigenValues[j]; 
                    //if (_vectors[i][j] != 0 && newEigenValues[j] != 0 && intermediate[i, j] == 0)
                    //{
                    //    throw new NotComputableException("Apparent number underflow exception. The product of two non-zero numbers is 0.");
                    //}
                }
            } 
 
            // now multiply by inv(V)
            double[][] result = new double[newEigenValues.Length][]; 
            for (int i = 0; i < _vectors.Length; i++)
            {
                result[i] = new double[newEigenValues.Length];
                for (int j = 0; j < _vectors[i].Length; j++)
                {
                    ComplexNumber sum = new ComplexNumber(0, 0); 
                    for (int k = 0; k < newEigenValues.Length; k++) 
                    {
                         sum += intermediate[i, k] * _invVectors[k][j]; // need to sum as complex to make sure the complex components cancel. 
                         //if (sum == 0 && _invVectors[k][j] != 0 && intermediate[i, k] != 0)
                         //{
                         //    throw new NotComputableException("Apparent number underflow exception. The product of two non-zero numbers is 0.");
                         //}
                    }
                    result[i][j] = (double)sum; 
                } 
            }
            return result; 
        }

        public override string ToString()
        {
            return "V:\n" + LinearAlgebra.MatrixView(_vectors) + "\nD:\n[ " + SpecialFunctions.SpecialFunctions.CreateTabString2(_values) +
                " ]\ninv(V):\n" + LinearAlgebra.MatrixView(_invVectors); 
        } 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
