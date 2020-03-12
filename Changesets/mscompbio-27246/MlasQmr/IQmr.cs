using System; 
using System.Collections.Generic;
using System.Text;

namespace Msr.Mlas.IQmr
{
    public interface IQmr<TCause, TEffect> 
    { 
        void SetCause(TCause cause, double priorProbability);
        void SetLeak(TEffect effect, double leakProbability); 
        void SetLink(TCause cause, TEffect effect, double conditionalProbability);
        double PosteriorOfCause(TCause cause, IList<TEffect> presentEffectCollection,
                                              IList<TEffect> absentEffectCollection);
        Dictionary<TCause, double>
            PosteriorOfEveryCause(IList<TEffect> presentEffectCollection,
                                    IList<TEffect> absentEffectCollection); 
    } 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
