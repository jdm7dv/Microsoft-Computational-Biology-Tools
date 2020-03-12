using System; 
using System.Collections.Generic;
using System.Text;

namespace VirusCount.PhyloTree
{
    public class DistributionDiscreteSingleVariable : DistributionDiscreteConditional, IDistributionSingleVariable 
    { 
        private static DistributionDiscreteSingleVariable Instance;
        private DistributionDiscreteSingleVariable() { } 

        public static DistributionDiscreteSingleVariable GetInstance()
        {
            if (Instance == null)
            {
                Instance = new DistributionDiscreteSingleVariable(); 
            } 
            return Instance;
        } 

        public override string ToString()
        {
            return "SingleVariable";
        }
    } 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
