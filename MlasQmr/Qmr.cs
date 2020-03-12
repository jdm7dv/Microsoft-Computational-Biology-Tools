using System; 
using System.Collections.Generic;
using System.Text;

namespace Msr.Mlas.Qmr
{
    public abstract class Qmr<TCause, TEffect> 
    { 
 		public abstract void SetCause(TCause cause, double priorProbability);
		public abstract void SetLeak(TEffect effect, double leakProbability); 
		public abstract void SetLink(TCause cause, TEffect effect, double conditionalProbability);
		public abstract double PosteriorOfCause(TCause cause, IList<TEffect> presentEffectCollection,
                                              IList<TEffect> absentEffectCollection);
		public abstract Dictionary<TCause, double>
            PosteriorOfEveryCause(IList<TEffect> presentEffectCollection,
                                    IList<TEffect> absentEffectCollection); 
 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
