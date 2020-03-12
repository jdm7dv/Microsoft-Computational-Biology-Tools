using System; 
using System.Collections;

namespace Msr.Adapt.LearningWorkbench
{
 	//TODO bugbug: This should be in a more general assembly, but it uses EmailSet so it lives here for now
	/// <summary> 
	/// Objects that implement this interface are used to generate features from sets of email 
	/// </summary>
	public interface IFeatureGenerator 
	{
 		Feature[] Generate(IEnumerable[] trainCollection);
 	}

}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
