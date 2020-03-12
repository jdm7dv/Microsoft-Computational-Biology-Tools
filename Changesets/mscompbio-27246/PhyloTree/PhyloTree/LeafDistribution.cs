using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using Optimization;
//using Msr.Mlas.LinearAlgebra; 
using System.IO; 

namespace VirusCount.PhyloTree 
{



    public abstract class LabelledLeafDistributionDiscrete
    { 
        internal LabelledLeafDistributionDiscrete() 
        {
        } 

        public static LabelledLeafDistributionDiscrete GetInstance(string name)
        {
            switch (name.ToLower())
            {
                case "escape": 
                    return Escape.GetInstance(); 
                case "reversion":
                    return Reversion.GetInstance(); 
                case "escapereversion":
                    return EscapeReversion.GetInstance();
                case "attraction":
                    return Attraction.GetInstance();
                case "repulsion":
                    return Repulsion.GetInstance(); 
                case "attractionrepulsion": 
                    return AttractionRepulsion.GetInstance();
                case "null": 
                    return null;
                default:
                    throw new ArgumentException("Don't know leaf distribution " + name);
            }
        }
 
        abstract internal double[] PredictorTrueDistribution(double leafParameter, double[] yDist); 

        abstract internal double[] PredictorFalseDistribution(double leafParameter, double[] yDist); 

        //abstract public bool NeedToRunToFindPValue(int[] fisherCounts);
    }

    public class Escape : LabelledLeafDistributionDiscrete
    { 
        private Escape() 
        {
        } 

        internal static LabelledLeafDistributionDiscrete GetInstance()
        {
            return new Escape();
        }
 
        public override string ToString() 
        {
            return "Escape"; 
        }

        internal override double[] PredictorTrueDistribution(double leafParameter, double[] yDist)
        {
            double[] zDist = new double[2];
            zDist[0] = yDist[0] + leafParameter * yDist[1]; 
            zDist[1] = 1.0 - zDist[0]; 
            return zDist;
        } 

        internal override double[] PredictorFalseDistribution(double leafParameter, double[] yDist)
        {
            return yDist;
        }
 
 

        //public override bool NeedToRunToFindPValue(int[] fisherCounts) 
        //{
        //    return SpecialFunctions.FisherCountsAreNegativelyCorrelated(fisherCounts);
        //}
    }

    public class Reversion : LabelledLeafDistributionDiscrete 
    { 
        private Reversion()
        { 
        }

        internal static LabelledLeafDistributionDiscrete GetInstance()
        {
            return new Reversion();
        } 
 
        public override string ToString()
        { 
            return "Reversion";
        }

        internal override double[] PredictorTrueDistribution(double leafParameter, double[] yDist)
        {
            return yDist; 
        } 

        internal override double[] PredictorFalseDistribution(double leafParameter, double[] yDist) 
        {
            double[] zDist = new double[2];
            zDist[1] = yDist[1] + leafParameter * yDist[0];
            zDist[0] = 1.0 - zDist[1];
            return zDist;
        } 
 
        //public override bool NeedToRunToFindPValue(int[] fisherCounts)
        //{ 
        //    return SpecialFunctions.FisherCountsAreNegativelyCorrelated(fisherCounts);
        //}
    }

    public class EscapeReversion : LabelledLeafDistributionDiscrete
    { 
        private EscapeReversion() 
        {
        } 

        internal static LabelledLeafDistributionDiscrete GetInstance()
        {
            EscapeReversion aEscapeReversion = new EscapeReversion();
            aEscapeReversion._escape = Escape.GetInstance();
            aEscapeReversion._reversion = Reversion.GetInstance(); 
            return aEscapeReversion; 
        }
 
        private LabelledLeafDistributionDiscrete _escape;
        private LabelledLeafDistributionDiscrete _reversion;

        public override string ToString()
        {
            return "EscapeReversion"; 
        } 

        internal override double[] PredictorTrueDistribution(double leafParameter, double[] yDist) 
        {
            return _escape.PredictorTrueDistribution(leafParameter, yDist);
        }

        internal override double[] PredictorFalseDistribution(double leafParameter, double[] yDist)
        { 
            return _reversion.PredictorFalseDistribution(leafParameter, yDist); 
        }
 
        //public override bool NeedToRunToFindPValue(int[] fisherCounts)
        //{
        //    return SpecialFunctions.FisherCountsAreNegativelyCorrelated(fisherCounts);
        //}
    }
 
    public class Attraction : LabelledLeafDistributionDiscrete 
    {
        private Attraction() 
        {
        }

        internal static LabelledLeafDistributionDiscrete GetInstance()
        {
            return new Attraction(); 
        } 

        public override string ToString() 
        {
            return "Attraction";
        }

        internal override double[] PredictorTrueDistribution(double leafParameter, double[] yDist)
        { 
            double[] zDist = new double[2]; 
            zDist[1] = yDist[1] + leafParameter * yDist[0]; // Pr[true already] + Pr[would be false AND predictor pulls you to true]
            zDist[0] = 1.0 - zDist[1]; 
            return zDist;
        }

        internal override double[] PredictorFalseDistribution(double leafParameter, double[] yDist)
        {
            return yDist; 
        } 

        //public override bool NeedToRunToFindPValue(int[] fisherCounts) 
        //{
        //    return SpecialFunctions.FisherCountsArePositivelyCorrelated(fisherCounts);
        //}
    }

    public class Repulsion : LabelledLeafDistributionDiscrete 
    { 
        private Repulsion()
        { 
        }

        internal static LabelledLeafDistributionDiscrete GetInstance()
        {
            return new Repulsion();
        } 
 
        public override string ToString()
        { 
            return "Repulsion";
        }

        internal override double[] PredictorTrueDistribution(double leafParameter, double[] yDist)
        {
            return yDist; 
        } 

        internal override double[] PredictorFalseDistribution(double leafParameter, double[] yDist) 
        {
            double[] zDist = new double[2];
            zDist[0] = yDist[0] + leafParameter * yDist[1];
            zDist[1] = 1.0 - zDist[0];
            return zDist;
        } 
 
        //public override bool NeedToRunToFindPValue(int[] fisherCounts)
        //{ 
        //    return SpecialFunctions.FisherCountsArePositivelyCorrelated(fisherCounts);
        //}
    }

    public class AttractionRepulsion : LabelledLeafDistributionDiscrete
    { 
        private AttractionRepulsion() 
        {
        } 

        internal static LabelledLeafDistributionDiscrete GetInstance()
        {
            AttractionRepulsion aAttractionRepulsion = new AttractionRepulsion();
            aAttractionRepulsion._attraction = Attraction.GetInstance();
            aAttractionRepulsion._repulsion = Repulsion.GetInstance(); 
            return aAttractionRepulsion; 
        }
 
        private LabelledLeafDistributionDiscrete _attraction;
        private LabelledLeafDistributionDiscrete _repulsion;

        public override string ToString()
        {
            return "AttractionRepulsion"; 
        } 

        internal override double[] PredictorTrueDistribution(double leafParameter, double[] yDist) 
        {
            return _attraction.PredictorTrueDistribution(leafParameter, yDist);
        }

        internal override double[] PredictorFalseDistribution(double leafParameter, double[] yDist)
        { 
            return _repulsion.PredictorFalseDistribution(leafParameter, yDist); 
        }
 
    }
 

}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
