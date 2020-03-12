using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using Optimization;

namespace VirusCount.PhyloTree 
{ 
    public class Score
    { 
        private Score()
        {
        }

        //!!!make these read only to outsiders
        public readonly double Loglikelihood; 
        public readonly OptimizationParameterList OptimizationParameters; 
        public readonly IDistribution Distribution;
 
        protected Score(double loglikelihood, OptimizationParameterList optimizationParameters, IDistribution distribution)
        {
            Loglikelihood = loglikelihood;
            OptimizationParameters = optimizationParameters;
            Distribution = distribution;
        } 
 
        public static Score GetInstance(double loglikelihood, OptimizationParameterList optimizationParameters, IDistribution distribution)
        { 
            return new Score(loglikelihood, optimizationParameters, distribution);
        }

        public override string ToString()
        {
            return SpecialFunctions.CreateTabString(OptimizationParameters, Loglikelihood); 
        } 

        public string ToString(DistributionDiscrete distributionDiscrete) 
        {
            return SpecialFunctions.CreateTabString(distributionDiscrete.GetParameterValueString(OptimizationParameters), Loglikelihood);
        }

        public override int GetHashCode()
        { 
            int hashCode = Loglikelihood.GetHashCode(); 
            foreach (OptimizationParameter param in OptimizationParameters)
            { 
                hashCode ^= param.Value.GetHashCode();
            }
            return hashCode;
        }

        public override bool Equals(object obj) 
        { 
            Score other = obj as Score;
            if (other == null || Loglikelihood != other.Loglikelihood || OptimizationParameters.Count != other.OptimizationParameters.Count) 
            {
                return false;
            }
            for (int i = 0; i < OptimizationParameters.Count; i++)
            {
                if (OptimizationParameters[i] != other.OptimizationParameters[i]) 
                { 
                    return false;
                } 
            }
            return true;
        }
    }
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
