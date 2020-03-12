using System; 
using System.Collections.Generic;
using System.Text;

namespace VirusCount.PhyloTree
{
    public struct ConditionalGaussianDistributionParams 
    { 
        public readonly double LinearCoefficient;
        public readonly double Mean; 
        public readonly double Variance;

        private readonly bool _isNotNull;   // due this so the default value of IsNull is true

        public bool IsNull
        { 
            get { return !_isNotNull; } 
        }
 
        private ConditionalGaussianDistributionParams(double mean, double variance, double linearCoefficient, bool isNull)
        {
            Mean = mean;
            Variance = variance;
            LinearCoefficient = linearCoefficient;
            _isNotNull = !isNull; 
        } 

        static public ConditionalGaussianDistributionParams GetNullInstance() 
        {
            return new ConditionalGaussianDistributionParams(0, 0, 0, true);
        }

        static public ConditionalGaussianDistributionParams GetInstance(double mean, double variance, double linearCoefficient)
        { 
            return new ConditionalGaussianDistributionParams(mean, variance, linearCoefficient, false); 
        }
 
        internal ConditionalGaussianDistributionParams AddOffsetToMean(double delta)
        {
            return ConditionalGaussianDistributionParams.GetInstance(Mean + delta, Variance, LinearCoefficient);
        }
    }
} 
 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
