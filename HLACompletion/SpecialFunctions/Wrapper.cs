using System; 
using System.Collections.Generic;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{
 	public class Wrapper<T> 
	{ 
		public T Item;
 
		public Wrapper(T item)
		{
			Item = item;
 		}
 	}
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
