    using System; 
    using System.Collections.Generic;
    using System.Text;

    namespace VirusCount.PhyloTree
    {
     public class ConditionalGaussianDistribution 
 
        {
         private ConditionalGaussianDistribution() 

            {
            }

         static public ConditionalGaussianDistribution GetInstance()
 
            { 
                ConditionalGaussianDistribution aLinearGaussian = new ConditionalGaussianDistribution();
                return aLinearGaussian; 
            }

            public double LinearCoefficent;
            public double Mean;
            public double Variance;
        } 
    } 

 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
