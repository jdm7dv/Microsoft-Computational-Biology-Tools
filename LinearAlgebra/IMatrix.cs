using System; 

namespace Msr.Mlas.LinearAlgebra
{
    public interface IMatrix
    {
        Type ElementType { get; } 
        int[] Size { get; } 

        object GetElement(int idx1, int idx2); 
        IMatrix GetSubMatrix(int rowstart, int rowstep, int rowend, int colstart, int colstep, int colend);
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
