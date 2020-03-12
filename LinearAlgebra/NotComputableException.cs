using System; 
using System.Collections.Generic;
using System.Text;

namespace Msr.Mlas.LinearAlgebra
{
    public class NotComputableException : Exception 
    { 
        public NotComputableException() : base() { }
        public NotComputableException(string message) : base(message) { } 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
