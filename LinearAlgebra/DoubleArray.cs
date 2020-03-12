using System; 
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Msr.Mlas.LinearAlgebra 
{ 
    public class DoubleArray
    { 
        public DoubleArray(double[][] jaggedarr) { throw new Exception("The method or operation is not implemented."); }
        public DoubleArray(DoubleArray m) { throw new Exception("The method or operation is not implemented."); }
        public static DoubleArray operator *(DoubleArray m, double v){ throw new Exception("The method or operation is not implemented."); }
        public static DoubleArray operator *(DoubleArray m1, DoubleArray m2){ throw new Exception("The method or operation is not implemented."); }
        public static DoubleArray operator +(double v, DoubleArray m) { throw new Exception("The method or operation is not implemented."); }
        public static DoubleArray operator +(DoubleArray m1, DoubleArray m2) { throw new Exception("The method or operation is not implemented."); } 
        public virtual int[] Size { get { throw new Exception("The method or operation is not implemented."); } } 
        public virtual object GetElement(int idx1, int idx2){ throw new Exception("The method or operation is not implemented."); }
        public static DoubleArray Identity(int rowsize, int colsize){ throw new Exception("The method or operation is not implemented."); } 
        public double Max(){ throw new Exception("The method or operation is not implemented."); }
        public double Min(){ throw new Exception("The method or operation is not implemented."); }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
