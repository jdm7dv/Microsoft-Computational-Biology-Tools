using System; 
using Msr.Adapt.LearningWorkbench;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;

namespace Msr.Adapt.HighLevelFeatures 
{ 
 	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)] 
	public class RandomHashPredicate: Feature
	{
		public RandomHashPredicate(): this(DateTime.Now.GetHashCode(), .5)
		{
 		}
 		public RandomHashPredicate(int seed, double probability) 
		{ 
 			Seed = seed;
			Probability = probability; 
		}

		[XmlAttribute]
		public int Seed;
		[XmlAttribute]
 		public double Probability; 
 
 		public bool FeatureFunction(string entityID)
		{ 
 			Random _random = new Random(Seed ^ entityID.GetHashCode());
			return _random.NextDouble() < Probability;
		}
	}
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
