using System; 
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection; 
using System.Text.RegularExpressions; 
using System.Threading;
using System.IO.Compression; 
using System.Security.Cryptography;
using System.Drawing;
using System.Linq.Expressions;
using System.Linq;

namespace Msr.Mlas.SpecialFunctions 
{ 
    /// <summary>
    /// Summary description for SpecialFunctions. 
    /// </summary>
    public class SpecialFunctions
    {
        public static double Log2Pi = Math.Log(2 * Math.PI);
        /*
         Given a contingency table 
                X=0	   X=1 
        Y=0	  a		  b
        Y=1	  c		  d 
        the Bayes score for a dependency between X and Y is
            gammaln(a+1) + gammaln(b+1) + gammaln(c+1) + gammaln(d+1) +gammln(a+b+c+d+4)
            - gammaln(a+b+2) - gammaln(c+d+2) - gammaln(a+c+2) -gammaln(b+c+2) - gammaln(4).

         */
        static public double BayesScore(double a, double b, double c, double d) 
        { 
            double rBayesScore =
                        SpecialFunctions.LogGamma(a + 1) 
                    + SpecialFunctions.LogGamma(b + 1)
                    + SpecialFunctions.LogGamma(c + 1)
                    + SpecialFunctions.LogGamma(d + 1)
                    + SpecialFunctions.LogGamma(a + b + c + d + 4)
                    - SpecialFunctions.LogGamma(a + b + 2)
                    - SpecialFunctions.LogGamma(c + d + 2) 
                    - SpecialFunctions.LogGamma(a + c + 2) 
                    - SpecialFunctions.LogGamma(b + d + 2)
                    - SpecialFunctions.LogGamma(4); 

            return rBayesScore;

        }

        static public double PData(double a, double b, double c, double d) 
        { 
            double logr =
                      SpecialFunctions.LogGamma(a + 1) 
                    + SpecialFunctions.LogGamma(b + 1)
                    + SpecialFunctions.LogGamma(c + 1)
                    + SpecialFunctions.LogGamma(d + 1)
                    + SpecialFunctions.LogGamma(a + b + c + d + 1)
                    - SpecialFunctions.LogGamma(a + b + 1)
                    - SpecialFunctions.LogGamma(c + d + 1) 
                    - SpecialFunctions.LogGamma(a + c + 1) 
                    - SpecialFunctions.LogGamma(b + d + 1);
            double r = Math.Exp(-logr); 
            return r;
        }

        static public double LogGamma(double x)
        {
            const double c1 = 0.918938533204673; 
            const double c2 = 0.000595238095238; 
            const double c3 = 0.000793650793651;
            const double c4 = 0.002777777777778; 
            const double c5 = 0.083333333333333;

            double f, z;

            if (x < 7.0)
            { 
                f = 1.0; 
                for (z = x; z < 7.0; z += 1.0)
                { 
                    x = z;
                    f *= z;
                }
                x += 1.0;
                f = -Math.Log(f);
            } 
            else 
            {
                f = 0.0; 
            }
            z = 1.0 / (x * x);

            return f + Math.Log(x) * (x - 0.5) - x + c1 +
                (((-c2 * z + c3) * z - c4) * z + c5) / x;
        } 
        static public void UnitTest() 
        {
            double p0 = .99999999; 
            double p1 = .000000001;

            Debug.Assert(Math.Abs(SpecialFunctions.LogSum(Math.Log(p0), Math.Log(p1)) - Math.Log(p0 + p1)) < .000001);


 
            Debug.Assert(Math.Abs(SpecialFunctions.BayesScore(5, 131, 3, 7) - 3.330435168) < .000001); // real Debug.Assert 
            Debug.Assert(Math.Abs(SpecialFunctions.BayesScore(new int[,] { { 5, 131 }, { 3, 7 } }) - 3.330435168) < .000001); // real Debug.Assert
 
            Debug.WriteLine(SpecialFunctions.FisherExactTest(0, 113, 1, 0));
            Debug.WriteLine(SpecialFunctions.FisherExactTest(0, 1, 113, 0));


            Debug.Assert(Math.Abs(Math.Exp(SpecialFunctions.LogGamma(4)) - 6.0) < .0000001); // real Debug.Assert
            Debug.Assert(Math.Abs(SpecialFunctions.BayesScore(5, 131, 3, 7) - 3.330435168) < .000001); // real Debug.Assert 
            Debug.Assert(Math.Abs(SpecialFunctions.FisherExactTest(5, 131, 3, 7) - 0.011) < .005); 
            Debug.Assert(Math.Abs(SpecialFunctions.FisherExactTest(5, 50, 10, 100) - 1) < .005);
            Debug.Assert(Math.Abs(SpecialFunctions.FisherExactTest(100, 200, 3000, 5000) - 0.145153831) < 1e-6); 

            //			Debug.WriteLine(SpecialFunctions.GaussTail(0, false)); //0.5
            //			Debug.WriteLine(SpecialFunctions.Normal(0)); //0.5
            //			Debug.WriteLine(SpecialFunctions.GaussTail(10000.0, false)); //1
            //			Debug.WriteLine(SpecialFunctions.Normal(10000.0)); //1
            //			Debug.WriteLine(SpecialFunctions.GaussTail(-10000.0, false)); //1 
            //			Debug.WriteLine(SpecialFunctions.Normal(-10000.0)); //1 
            //			Debug.WriteLine(SpecialFunctions.GaussTail(1, false)); //0.841344746068515
            //			Debug.WriteLine(SpecialFunctions.Normal(1)); //0.841344746068515 
            //			Debug.WriteLine(SpecialFunctions.GaussTail(-1, false)); //0.841344746068515
            //			Debug.WriteLine(SpecialFunctions.Normal(-1)); //0.841344746068515

            //			Debug.WriteLine(SpecialFunctions.InverseCumulativeGaussian(.5));
            //			Debug.WriteLine(SpecialFunctions.InverseCumulativeGaussian(0.841344746068515));
 
        } 

        static private void Swap(ref int I, ref int J) 
        {
            int K;
            K = I;
            I = J;
            J = K;
        } 
        static public double FisherExactTest(int[,] counts) 
        {
            CheckCondition(counts.GetLength(0) == 2 && counts.GetLength(1) == 2); 
            return FisherExactTest(counts[1, 1], counts[1, 0], counts[0, 1], counts[0, 0]);
        }

        static private double PDataDeleteMe(int a, int b, int c, int d)
        {
            double pt = 1.0; 
 
            //(a+b)!/a!
            int aPlusB = a + b; 
            for (int z = a + 1; z <= aPlusB; ++z)
            {
                pt = pt * z;
            }

            // (a+c)!/c! 
            int aPlusC = a + c; 
            for (int z = c + 1; z <= aPlusC; ++z)
            { 
                pt = pt * z;
            }

            //(c+d)!/d!
            int cPlusD = c + d;
            for (int z = d + 1; z <= cPlusD; ++z) 
            { 
                pt = pt * z;
            } 

            //(b+d)!/b!
            int bPlusD = b + d;
            for (int z = b + 1; z <= bPlusD; ++z)
            {
                pt = pt * z; 
            } 

            CheckCondition(!double.IsInfinity(pt), string.Format("The Fisher Exact Test calculation overflowed on [[{0},{1}],[{2},{3}]]", a, b, c, d)); 

            //(a+b+c+d)!
            int sumABCD = a + b + c + d;
            for (int z = 1; z <= sumABCD; ++z)
            {
                pt = pt / z; 
            } 

            return pt; 
        }



        /// <summary>
        /// Returns 2 sided FET.  Uses "Fisher statistic" for determining 
        /// whether counts are more extreme. 
        /// See http://www.med.uio.no/imb/stat/two-by-two/methods.html.
        /// </summary> 
        static public double FisherExactTest(int inA, int inB, int inC, int inD)
        {
            //Based on TurboPascal code

            if ((inA + inB == 0) || (inA + inC == 0) || (inB + inD == 0) || (inC + inD == 0))
            { 
                return 1.0; 
            }
 
            double p0 = PData(inA, inB, inC, inD); // the prob of seeing the actual data
            double p = 0.0;
            int minA = Math.Min(inA + inB, inA + inC);
            for (int j = 0; j <= minA; ++j)
            {
                int a = j; 
                int b = inA + inB - j; 
                int c = inA + inC - j;
                int d = inD - inA + j; 
                if ((a >= 0) && (b >= 0) && (c >= 0) && (d >= 0))
                {
                    // using Fisher stat -- the probability of the data --
                    //   to determine which cell counts are "more extreme" *)
                    double pt = PData(a, b, c, d);
                    if (pt <= p0) 
                    { 
                        p = p + pt;
                    } 
                }
            }

            return Math.Min(p, 1.0);
        }
 
 
        static public double OneWayPower(
            int HLANotMut, int HLAMut, int NotHLANotMut, int NotHLAMut, 
            double OddsRatio, double PValue)
        {

            double PCPosOR = OddsRatio;
            double PCNegOR = 1.0 / PCPosOR;
            double PCpvalue = PValue; 
            double TestStat = SpecialFunctions.NormalDensityInverse(PCpvalue); 
            int HLACount = HLANotMut + HLAMut;
            int MutCount = HLAMut + NotHLAMut; 
            int Count = HLANotMut + HLAMut + NotHLANotMut + NotHLAMut;
            double MaxPower = double.NegativeInfinity;
            for (int I = 1; I <= 2; ++I)
            {
                double n1 = HLACount;
                double n2 = Count - HLACount; 
                double p = (double)MutCount / (double)Count; 
                double RR;
                if (I == 1) 
                {
                    RR = PCPosOR;
                }
                else
                {
                    RR = PCNegOR; 
                } 
                double p1 = (RR * p * (n1 + n2)) / (RR * n1 + n2);
                double p2 = p1 / RR; 

                if ((p1 <= 1) && (p1 >= 0) && (p2 <= 1) && (p2 >= 0))
                {
                    double q = 1 - p;
                    double q1 = 1 - p1;
                    double q2 = 1 - p2; 
 
                    if (p1 * q1 + p2 * q2 > 0)
                    { 
                        double X1 = (-TestStat * Math.Sqrt((p * q) / n1 + (p * q) / n2) - (p1 - p2)) / (Math.Sqrt(p1 * q1 / n1 + p2 * q2 / n2));
                        double Power1;
                        if (X1 < 0)
                        {
                            Power1 = SpecialFunctions.NormalDensity(X1) / 2;
                        } 
                        else 
                        {
                            Power1 = 1 - SpecialFunctions.NormalDensity(X1) / 2; 
                        }
                        double X2 = (TestStat * Math.Sqrt((p * q) / n1 + (p * q) / n2) - (p1 - p2)) / (Math.Sqrt(p1 * q1 / n1 + p2 * q2 / n2));
                        double Power2;
                        if (X2 < 0)
                        {
                            Power2 = 1 - SpecialFunctions.NormalDensity(X2) / 2.0; 
                        } 
                        else
                        { 
                            Power2 = SpecialFunctions.NormalDensity(X2) / 2.0;
                        }
                        double Power_ = Power1 + Power2;

                        if (Power_ >= MaxPower)
                        { 
                            MaxPower = Power_; 
                        }
                    } 
                }
            }
            return MaxPower;
        }

 
        public static double NormalDensity(double Z) 
        {
            if (Z == double.NegativeInfinity || Z == double.PositiveInfinity) 
            {
                return double.NaN;
            }

            if (Z < 0)
            { 
                Z = -Z; 
            }
 
            double Result = Math.Pow((1 + (Z * (0.0498673470 + Z * (0.0211410061 + Z *
                (0.0032776263 + Z * (0.0000380036 + Z *
                (0.0000488906 + Z *
                0.0000053830))))))), -16);
            return Result;
        } 
 

 

        //		// GaussianIntegral.cxx
        //
        //		// Gaussian tail area - Algorithm 304 in Collected Algorithms From CACM
        //		// Calculates tail area from dblX to infinity (if bUpper is true), or
        //		// from -infinity to dblX (if bUpper is false). 
        // 
        public static double GaussTail(double dblX, bool bUpper)
        { 
            const double dblOneOverSqrt2Pi = 0.3989422804014;		// one over the square root of two pi

            if (dblX == 0)
            {
                return 0.5;
            } 
            else if (dblX < 0) 
            {
                bUpper = !bUpper; 
                dblX = -dblX;
            }

            double dblXSquared = dblX * dblX;
            double dblY = dblOneOverSqrt2Pi * Math.Exp(dblXSquared * -0.5);
 
            double dblN = dblY / dblX; 

            if (0.0 == dblN) 
            {
                return (bUpper ? 0.0 : 1.0);
            }

            double dblS;
            double dblT; 
 
            if (dblX > (bUpper ? 2.32 : 3.5))
            { 
                double dblP1, dblP2, dblQ1, dblQ2, dblM;

                dblQ1 = dblX;
                dblP2 = dblX * dblY;
                dblP1 = dblY;
                dblQ2 = dblXSquared + 1.0; 
                if (bUpper) 
                {
                    dblM = dblP1 / dblQ1; 
                    dblS = dblM;
                    dblT = dblP2 / dblQ2;
                }
                else
                {
                    dblM = 1.0 - dblP1 / dblQ1; 
                    dblS = dblM; 
                    dblT = 1.0 - dblP2 / dblQ2;
                } 
                for (dblN = 2.0; dblM != dblT && dblS != dblT; dblN += 1.0)
                {
                    dblS = dblX * dblP2 + dblN * dblP1;
                    dblP1 = dblP2;
                    dblP2 = dblS;
                    dblS = dblX * dblQ2 + dblN * dblQ1; 
                    dblQ1 = dblQ2; 
                    dblQ2 = dblS;
                    dblS = dblM; 
                    dblM = dblT;
                    dblT = dblP2 / dblQ2;

                    if (!bUpper)
                    {
                        dblT = 1.0 - dblT; 
                    } 
                }
                return dblT; 
            }
            else
            {
                dblX *= dblY;
                dblS = dblX;
                dblT = 0.0; 
 
                for (dblN = 3.0; dblS != dblT; dblN += 2.0)
                { 
                    dblT = dblS;
                    dblX *= dblXSquared / dblN;
                    dblS += dblX;
                }

                return 0.5 + (bUpper ? -dblS : dblS); 
            } 
        }
 

        public static double InverseCumulativeGaussian(double pvalue, double mean, double stdDev)
        {
            double result = InverseCumulativeGaussian(pvalue) * stdDev + mean;
            return result;
        } 
 
        // From C++ code
        // 
        // Odeh, R. E. & Evans, J. O. (1974) Algorithm AS 70: Percentage points
        //  of the normal distribution. Applied Statistics, 23, 96-97.
        //  also, Kennedy, W. J. & Gentle, J. E. (1980) Statistical Computing
        // returns # of std devs from mean for probability desired
        public static double InverseCumulativeGaussian(double dblProb)
        { 
            const double RTINY = 1.0e-20;			//  A number very close to zero (from Numerical Recipies) 

 
            // P's are used in numerator of fraction, Q's are used in the denominator
            const double dblP0 = -0.322232431088;
            const double dblP1 = -1.0;
            const double dblP2 = -0.342242088547;
            const double dblP3 = -0.0204231210245;
            const double dblP4 = -0.453642210148E-4; 
 
            const double dblQ0 = 0.0993484626060;
            const double dblQ1 = 0.588581570495; 
            const double dblQ2 = 0.531103462366;
            const double dblQ3 = 0.103537752850;
            const double dblQ4 = 0.38560700634E-2;

            double dblPosResult;					// a positive result
            double dblPLeft;						// always <= 0.5 
 
            // this is exactly the mean, so it's a special case
            if (dblProb == 0.5) 
            {
                return (0.0);
            }

            // we always do our calculation for one side of the gaussian
            // and negate at the end if we were actually on the other side 
            if (dblProb > 0.5) 
            {
                dblPLeft = 1.0 - dblProb; 
            }
            else
            {
                dblPLeft = dblProb;
            }
 
            if (dblPLeft < RTINY) 
            {
                dblPLeft = RTINY;					// minimum value to prevent underflow */ 
            }

            double dblY;							// intermediate value
            dblY = Math.Sqrt(Math.Log(1.0 / (dblPLeft * dblPLeft)));

            dblPosResult = dblY + ((((dblP4 * dblY + dblP3) * dblY + dblP2) * dblY + dblP1) * dblY + dblP0) / 
                        ((((dblQ4 * dblY + dblQ3) * dblY + dblQ2) * dblY + dblQ1) * dblY + dblQ0); 

            return (dblProb < 0.5 ? -dblPosResult : dblPosResult); 

        }

        public static double NormalDensityInverse(double pvalue)
        {
            if (pvalue >= 1) 
            { 
                return 100;
            } 
            else
            {
                double ts1 = 0;
                double ts2 = 100;

                while (true) 
                { 
                    double ts = ts1 + (ts2 - ts1) / 2;
                    double p = NormalDensity(ts); 
                    if (p < pvalue)
                    {
                        ts2 = ts;
                    }
                    else
                    { 
                        ts1 = ts; 
                    }
 
                    if (Math.Abs(p - pvalue) < 0.0005)
                    {
                        return ts;
                    }
                }
            } 
        } 

 

        /// <summary>
        ///		We also exclude those covariates that have a value in the 2x2 table
        ///		that is less than 5 and have an expected value < 5. This, together
        ///		with the power elimination rule, tries minimise the chance of the
        ///		logistics models going wacky with covariates with small amounts of 
        ///		data. 
        /// </summary>
        /// <returns>True, if these counts should be excluded</returns> 
        static public double MinOfCountOrExpCount(
            int HLANotMut, int HLAMut, int NotHLANotMut, int NotHLAMut
            )
        {
            //		N stands for "not HLA"
            //		and NS for "non-synonymous mutation (i.e. polymorphism from 
            //		consensus)". Hence; 

            double NSumNS = NotHLAMut; // NSumNS = the number of patients without the HLA and with an amino acid (or ambiguity) different to the consensus 
            double SumS = HLANotMut;	 // SumS = the number of patients with the HLA and with the consensus  amino acid
            double NSumS = NotHLANotMut;  // NSumS = the number of patients without the HLA and with the consensus amino acid
            double SumNS = HLAMut;  // SumNS = the number of patients with the HLA and with an amino acid (or ambiguity) different to the consensus

            double ExpNSumS = (double)(NSumS + NSumNS) * (NSumS + SumS) / (double)(SumNS + SumS + NSumNS + NSumS);
            double ExpSumNS = (double)(SumS + SumNS) * (NSumNS + SumNS) / (double)(SumNS + SumS + NSumNS + NSumS); 
            double ExpNSumNS = (double)(NSumS + NSumNS) * (NSumNS + SumNS) / (double)(SumNS + SumS + NSumNS + NSumS); 
            double ExpSumS = (double)(SumS + SumNS) * (NSumS + SumS) / (double)(SumNS + SumS + NSumNS + NSumS);
 
            double rMin =
                Min(Math.Max(NSumNS, ExpNSumNS),
                    Math.Max(SumS, ExpSumS),
                    Math.Max(NSumS, ExpNSumS),
                    Math.Max(SumNS, ExpSumNS));
            return rMin; 
        } 

        //!!!not a special math function 
        public static void CheckCondition(bool condition)
        {
            CheckCondition(condition, "A condition failed.");
        }

        //!!!not a special math function 
 
        /// <summary>
        /// Warning: The message with be evaluated even if the condition is true, so don't make it's calculation slow. 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void CheckCondition(bool condition, string message)
        {
            if (!condition) 
            { 
                throw new Exception(message);
            } 
        }

        public static void CheckCondition(bool condition, string messageToFormat, params object[] formatValues)
        {
            if (!condition)
            { 
                throw new Exception(string.Format(messageToFormat, formatValues)); 
            }
        } 


        public static string CreateDelimitedString(string delimiter, params object[] objectCollection)
        {
            return objectCollection.StringJoin(delimiter);
        } 
 
        [Obsolete("This has be superseded  by 'ienum.StringJoin(separator)'")]
        public static string CreateDelimitedString2<T>(string delimiter, IEnumerable<T> objectCollection) 
        {
            return objectCollection.StringJoin(delimiter);
        }

        //!!!not a special math function
        public static string CreateTabString(params object[] objectCollection) 
        { 
            return objectCollection.StringJoin("\t");
        } 


        //!!!not a special math function
        [Obsolete("This has be superseded  by 'ienum.StringJoin(separator)'")]
        public static string CreateTabString2<T>(IEnumerable<T> objectCollection)
        { 
            return objectCollection.StringJoin("\t"); 

        } 



        static public int Min(params int[] collection)
        {
            CheckCondition(collection.Length > 0); //!!!raise error 
            int iMin = collection[0]; 
            for (int i = 1; i < collection.Length; ++i)
            { 
                iMin = Math.Min(iMin, collection[i]);
            }
            return iMin;
        }

        static public double Min(params double[] collection) 
        { 
            CheckCondition(collection.Length > 0); //!!!raise error
            double rMin = collection[0]; 
            for (int i = 1; i < collection.Length; ++i)
            {
                rMin = Math.Min(rMin, collection[i]);
            }
            return rMin;
        } 
 
        static public int Max(params int[] collection)
        { 
            CheckCondition(collection.Length > 0); //!!!raise error
            int iMax = collection[0];
            for (int i = 1; i < collection.Length; ++i)
            {
                iMax = Math.Max(iMax, collection[i]);
            } 
            return iMax; 
        }
 
        static public double Max(params double[] collection)
        {
            CheckCondition(collection.Length > 0); //!!!raise error
            double rMax = collection[0];
            for (int i = 1; i < collection.Length; ++i)
            { 
                rMax = Math.Max(rMax, collection[i]); 
            }
            return rMax; 
        }

        public static double BayesScore(int[,] counts)
        {
            // Returns log p(dependent)/p(independent), assuming a K2-like
            // parameter prior and assuming that initially we're 50/50. 
 
            // counts[iStateVar1][iStateVar2] contains the number of rows in which
            // variable1 occurs in state iStateVar1 AND variable2 occurs in state iStateVar2. 

            // The score is computed as log p(variable2 data | variable1 data) / p(variable2 data)
            // In the numerator, we use a Dirichlet prior where each state has an ESS of 1, and in
            // the denominator, we use a Dirichlet prior where each state has an ESS of cStateVar1.

            // See ftp://ftp.research.microsoft.com/pub/tr/tr-95-06.pdf, page 9. For the dependant 
            // model, we split up the data and apply Equation (13) for each state of variable 1. 
            // For the independent model, we sum over all states of variable 1 to get the marginal
            // counts for variable 2. 

            int cStateVar1 = counts.GetLength(0);
            int cStateVar2 = counts.GetLength(1);

            double dblScoreDep = 0.0;	// Score for the dependent model
            double dblScoreIndep = 0.0;	// Score for the independent model 
 
            // Calculate the dependence score
 
            for (int iStateVar1 = 0; iStateVar1 < cStateVar1; iStateVar1++)
            {
                // Add in the terms in the product of Equation (13) for this
                // particular state of variable 1.

                int ccaseStateVar1 = 0; // Accumulate the marginal count for this state of var 1 
 
                for (int iStateVar2 = 0; iStateVar2 < cStateVar2; iStateVar2++)
                { 
                    int ccaseCoOccur = counts[iStateVar1, iStateVar2];

                    ccaseStateVar1 += ccaseCoOccur;
                    dblScoreDep += LogGamma(1 + ccaseCoOccur);

                    // Note that Gamma(1) = 1, so no division is necessary 
                } 

                // Now add in the first term in Equation (13) for this particular state. 
                // The \alpha is the number of states of variable 2, and the N is the number
                // of cases where variable 1 is in state iStateVar1.

                dblScoreDep += LogGamma(cStateVar2) - LogGamma(cStateVar2 + ccaseStateVar1);
            }
 
            // For the independence model, we need the histogram for variable 2. This should 
            // probably be passed in, as it will most likely be available to the caller.
            // Alternatively, we can construct the histogram within the loop above, after 
            // allocating memory in which to store it. Below we're simply swapping the loops
            // from above and thus accomplishing an O(n) calculation in O(n^2). Because we're
            // calling LogGamma() n^2 times above anyway, this isn't going to slow things
            // down much, but its somewhat distasteful.

            int ccaseTotal = 0;	// Sum of the elements in counts 
 
            for (int iStateVar2 = 0; iStateVar2 < cStateVar2; iStateVar2++)
            { 
                // Get the number of marginal counts for this state. (i.e., get the
                // height of the histogram for state iStateVar2)

                int ccaseStateVar2 = 0;

                for (int iStateVar1 = 0; iStateVar1 < cStateVar1; iStateVar1++) 
                { 
                    ccaseStateVar2 += counts[iStateVar1, iStateVar2];
                } 

                // Put in the corresponding product term from Equation 13, using
                // \alpha_k = cStateVar1

                dblScoreIndep += LogGamma(cStateVar1 + ccaseStateVar2) - LogGamma(cStateVar1);
 
                ccaseTotal += ccaseStateVar2; 

            } 

            // Finally, add in the first term of Equation (13) for the independence model.
            // Because the ESS for each state of variable 2 is cStateVar1, \apha is equal
            // to cStateVar1 * cStateVar2

            double dblAlpha = cStateVar2 * cStateVar1; 
 
            dblScoreIndep += LogGamma(dblAlpha) - LogGamma(dblAlpha + ccaseTotal);
 
            return dblScoreDep - dblScoreIndep;
        }



 
        public double ChiSquareInvOneSidedValue(double sig, int dependentVariableValueCount) 
        {
            //double rSigBF = sig / (double) independentVariableCount; 
            int iDF = dependentVariableValueCount - 1;
            if (iDF == 0)
            {
                return double.PositiveInfinity;
            }
            CheckCondition(0 < iDF && iDF < _rgDegreeOfFreedomToSigToValue.Length); //!!!raise error 
            SortedList rgSigToValue = (SortedList)_rgDegreeOfFreedomToSigToValue[iDF]; 
            CheckCondition(rgSigToValue.ContainsKey(sig)); //!!!raise error
            double rValue = (double)rgSigToValue[sig]; 
            return rValue;
        }


        private SortedList[] _rgDegreeOfFreedomToSigToValue = null;
 
        private SpecialFunctions() 
        {
            LoadLogLikelihoodRatioTestTable(); 
        }

        static public SpecialFunctions GetInstance()
        {
            return SingletonCreator.Singleton;
        } 
 
        // Thread-safe (lazy) Singleton Pattern ala "C# 3.0 Design Patterns" by Judith Bishop, p. 118
        class SingletonCreator 
        {
            static SingletonCreator() { }
            internal static readonly SpecialFunctions Singleton = new SpecialFunctions();
        }

 
        double[] LogLikelihoodRatioTestKeyTable; 
        double[] LogLikelihoodRatioTestValueTable;
 
        private void LoadLogLikelihoodRatioTestTable()
        {
            string header = "DOF\tRowCount\tRowFraction\tColumn3\tlogLikelihoodRatio\tPvalue\tln(Pvalue)";
            List<Dictionary<string, string>> rowList = //SpecialFunctions.TabFileTableAsList(@"DataFiles\ChiSquareIsLogLogLinear.txt", header, false);
                SpecialFunctions.TabFileTableAsList(Assembly.GetAssembly(typeof(SpecialFunctions)),
                "Msr.Mlas.SpecialFunctions.DataFiles.", "ChiSquareIsLogLogLinear.txt", header, false); 
 
            LogLikelihoodRatioTestKeyTable = new double[rowList.Count + 1];
            LogLikelihoodRatioTestValueTable = new double[rowList.Count + 1]; 
            for (int i = 0; i < rowList.Count; ++i)
            {
                Dictionary<string, string> row = rowList[i];
                double logLikelihoodRatio = double.Parse(row["logLikelihoodRatio"]);
                double logPValue = double.Parse(row["ln(Pvalue)"]);
                LogLikelihoodRatioTestKeyTable[i] = logLikelihoodRatio; 
                LogLikelihoodRatioTestValueTable[i] = logPValue; 
            }
            LogLikelihoodRatioTestKeyTable[rowList.Count] = double.PositiveInfinity; 
            LogLikelihoodRatioTestValueTable[rowList.Count] = double.NegativeInfinity;
            Array.Sort(LogLikelihoodRatioTestKeyTable, LogLikelihoodRatioTestValueTable);
        }

        //!!!make into generic linear interpolation code
        public double LogLikelihoodRatioTest(double logLikelihoodRatio, int degreesOfFreedom, bool boundaryCase) 
        { 
            double p;
 
            if (double.IsNaN(logLikelihoodRatio))
            {
                return double.NaN;
            }
            CheckCondition(degreesOfFreedom == 1, "Only defined for degreesOfFreedom=1, currently");
            CheckCondition(logLikelihoodRatio >= 0, "log likelihood ratio must be at least zero"); 
 
            int indexOrNot = Array.BinarySearch<double>(LogLikelihoodRatioTestKeyTable, logLikelihoodRatio);
            if (indexOrNot >= 0) 
            {
                p = Math.Exp(LogLikelihoodRatioTestValueTable[indexOrNot]);
            }

            else
            { 
                int indexHigh = ~indexOrNot; 

                int indexLow = indexHigh - 1; 
                CheckCondition(0 <= indexLow && indexHigh < LogLikelihoodRatioTestValueTable.Length);

                //If the difference is very large, then return the smallest non-zero pValue in the charts
                if (indexHigh == LogLikelihoodRatioTestValueTable.Length - 1)
                {
                    p = Math.Exp(LogLikelihoodRatioTestValueTable[indexLow]); 
                } 
                else
                { 
                    double keyLow = LogLikelihoodRatioTestKeyTable[indexLow];
                    double keyHigh = LogLikelihoodRatioTestKeyTable[indexHigh];

                    double fraction = (logLikelihoodRatio - keyLow) / (keyHigh - keyLow);
                    Debug.Assert(0 <= fraction && fraction <= 1); // real assert
 
                    double valueLow = LogLikelihoodRatioTestValueTable[indexLow]; 
                    double valueHigh = LogLikelihoodRatioTestValueTable[indexHigh];
 
                    double logPValue = (valueHigh - valueLow) * fraction + valueLow;
                    p = Math.Exp(logPValue);
                }
            }
            if (boundaryCase)
            { 
                p /= 2; 
            }
            return p; 

        }

        static public double LogSumParams(params double[] logProbabilityCollection)
        {
            //Debug.Assert(Math.Exp(double.NegativeInfinity) == 0); //real assert 
 
            double logMax = double.NegativeInfinity;
            int celem = logProbabilityCollection.Length; 

            for (int ielem = 0; ielem < celem; ielem++)
                if (logProbabilityCollection[ielem] > logMax)
                    logMax = logProbabilityCollection[ielem];

            if (double.IsNegativeInfinity(logMax)) 
            { 
                return logMax;
            } 
            //Debug.Assert(logMax > double.NegativeInfinity); //!!!raise exception

            double logK = Math.Log(double.MaxValue) - Math.Log((double)celem + 1.0) - logMax;

            double sum = 0;
            for (int ielem = 0; ielem < celem; ielem++) 
            { 
                sum += Math.Exp(logProbabilityCollection[ielem] + logK);
            } 

            double dbl = -logK + Math.Log(sum);

            //Debug.Assert(double.NegativeInfinity < dbl && dbl < double.PositiveInfinity);

            return -logK + Math.Log(sum); 
        } 

        static double LogSumConst = Math.Log(double.MaxValue) - Math.Log(3.0); 

        static public double LogSum(double logP1, double logP2)
        {
            if (double.IsNegativeInfinity(logP1))
            {
                if (double.IsNegativeInfinity(logP2)) 
                { 
                    return double.NegativeInfinity;
                } 
                else
                {
                    return logP2;
                }
            }
            if (double.IsNegativeInfinity(logP2)) 
            { 
                return logP1;
            } 

            double logK = LogSumConst - Math.Max(logP1, logP2);
            double sum = Math.Exp(logP1 + logK) + Math.Exp(logP2 + logK);
            double r = Math.Log(sum) - logK;
            //Debug.Assert(double.NegativeInfinity < r && r < double.PositiveInfinity);
            return r; 
        } 

        static public double LogSubtract(double logP1, double logP2) 
        {
            if (double.IsNegativeInfinity(logP1) && double.IsNegativeInfinity(logP2))
            {
                return double.NegativeInfinity;
            }
            double logK = LogSumConst - Math.Max(logP1, logP2); 
            double sum = Math.Exp(logP1 + logK) - Math.Exp(logP2 + logK); 
            double r = Math.Log(sum) - logK;
            //Debug.Assert(double.NegativeInfinity < r && r < double.PositiveInfinity); 
            return r;
        }

        static public double LogSubtractParams(params double[] logProbabilityCollection)
        {
            //Debug.Assert(Math.Exp(double.NegativeInfinity) == 0); //real assert 
 
            double logMax = double.NegativeInfinity;
            int celem = logProbabilityCollection.Length; 

            for (int ielem = 0; ielem < celem; ielem++)
                if (logProbabilityCollection[ielem] > logMax)
                    logMax = logProbabilityCollection[ielem];

            if (double.IsNegativeInfinity(logMax)) 
            { 
                return logMax;
            } 
            //Debug.Assert(logMax > double.NegativeInfinity); //!!!raise exception

            double logK = Math.Log(double.MaxValue) - Math.Log((double)celem + 1.0) - logMax;

            double sum = 0;
            for (int ielem = 0; ielem < celem; ielem++) 
            { 
                sum -= Math.Exp(logProbabilityCollection[ielem] + logK);
            } 

            double dbl = -logK + Math.Log(sum);

            //Debug.Assert(double.NegativeInfinity < dbl && dbl < double.PositiveInfinity);

            return -logK + Math.Log(sum); 
        } 

        public static double LogOdds(double probability) 
        {
            return Math.Log(probability / (1.0 - probability));
        }

        public static double InverseLogOdds(double logOdds)
        { 
            if (double.IsInfinity(logOdds)) 
            {
                if (logOdds > 0) 
                {
                    return 1.0;
                }
                else
                {
                    return 0.0; 
                } 
            }
            else 
            {
                double odds = Math.Exp(logOdds);
                return odds / (1.0 + odds);
            }
        }
 
        //This could be replaced with the quick-sort-like linear expect time algorithm. See, for example, http://www2.toki.or.id/book/AlgDesignManual/BOOK/BOOK4/NODE150.HTM 
        // OR http://en.wikipedia.org/wiki/Median#Efficient_computation
        // There was also a discussion in http://codecoop around 9/19/2007 
        //   Return NaN if the list is empty and return the mean of the middle two values if the list has even length.

        public static double Median(ref List<double> list)
        {
            if (list.Count == 0)
            { 
                return double.NaN; 
            }
 
            list.Sort();

            //Examples of indexs of interest
            //0 1 2 -> 1
            //0 1 2 3 -> 1,2
 
            int lowIndex = (list.Count - 1) / 2; 
            if (list.Count % 2 == 1)
            { 
                return list[lowIndex];
            }
            else
            {
                return (list[lowIndex] + list[lowIndex + 1]) / 2.0;
            } 
        } 

        public static double Median(ref List<int> list) 
        {
            if (list.Count == 0)
            {
                return double.NaN;
            }
 
            list.Sort(); 

            //Examples of indexs of interest 
            //0 1 2 -> 1
            //0 1 2 3 -> 1,2

            int lowIndex = (list.Count - 1) / 2;
            if (list.Count % 2 == 1)
            { 
                return (double)list[lowIndex]; 
            }
            else 
            {
                return (double)(list[lowIndex] + list[lowIndex + 1]) / 2.0;
            }
        }

        public static double Median(IEnumerable<int> listIn) 
        { 

            List<int> list = new List<int>(listIn); 
            if (list.Count == 0)
            {
                return double.NaN;
            }
            list.Sort();
 
            //Examples of indexs of interest 
            //0 1 2 -> 1
            //0 1 2 3 -> 1,2 

            int lowIndex = (list.Count - 1) / 2;
            if (list.Count % 2 == 1)
            {
                return (double)list[lowIndex];
            } 
            else 
            {
                return (double)(list[lowIndex] + list[lowIndex + 1]) / 2.0; 
            }
        }

        public static double Median(IEnumerable<double> listIn)
        {
 
            List<double> list = new List<double>(listIn); 
            if (list.Count == 0)
            { 
                return double.NaN;
            }
            list.Sort();

            //Examples of indexs of doubleerest
            //0 1 2 -> 1 
            //0 1 2 3 -> 1,2 

            int lowIndex = (list.Count - 1) / 2; 
            if (list.Count % 2 == 1)
            {
                return list[lowIndex];
            }
            else
            { 
                return (list[lowIndex] + list[lowIndex + 1]) / 2.0; 
            }
        } 


        internal static double Bound(double low, double high, double value)
        {
            CheckCondition(low <= high);
            if (value < low) 
            { 
                return low;
            } 
            else if (value > high)
            {
                return high;
            }
            else
            { 
                return value; 
            }
        } 



        public static double BIC(double logLikelihood, int degreesOfFreedom, int caseCount)
        {
            return logLikelihood - ((double)degreesOfFreedom / 2.0) * Math.Log(caseCount); 
        } 

        //General 
        //!!!could use this in many, many places
        //!!!It is not super fast, but probabily faster than a real database or reading from the file.

        //!!!instead of opening the file once to get the header and again to the lines, could only open it once
        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, bool includeWholeLine)
        { 
            string header = ReadLine(filename); 
            return TabFileTable(filename, header, includeWholeLine);
        } 


        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, bool includeWholeLine, out string header)
        {
            header = ReadLine(filename);
            return TabFileTable(null, null, filename, header, includeWholeLine, '\t', true); 
        } 

        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, bool includeWholeLine, char separator, out string header) 
        {
            header = ReadLine(filename);
            return TabFileTable(null, null, filename, header, includeWholeLine, separator, true);
        }

 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, char separator) 
        {
            string header = ReadLine(filename); 
            return TabFileTable(null, null, filename, header, false, separator, true);
        }

        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, string header, char separator)
        {
            return TabFileTable(null, null, filename, header, false, separator, true); 
        } 

        static public IEnumerable<Dictionary<string, string>> TabFileTableFixHeader(string filename, char separator) 
        {
            string header = ReadLine(filename);
            string header2 = FixupHeader(separator, header);
            return TabFileTable(null, null, filename, header2, false, separator, false);
        }
 
        private static string FixupHeader(char separator, string header) 
        {
            string[] headerCollection = header.Split(separator); 
            Set<string> seenIt = Set<string>.GetInstance();
            List<string> headerCollection2 = new List<string>();
            foreach (string head in headerCollection)
            {
                if (seenIt.Contains(head))
                { 
                    for (int i = 2; ; ++i) 
                    {
                        string head2 = head + "#" + i.ToString(); 
                        if (!seenIt.Contains(head2))
                        {
                            headerCollection2.Add(head2);
                            seenIt.AddNew(head2);
                            break;
                        } 
                    } 
                }
                else 
                {
                    headerCollection2.Add(head);
                    seenIt.AddNew(head);
                }

            } 
            string header2 = headerCollection2.StringJoin(separator.ToString()); 
            return header2;
        } 


        public static string ReadLine(string filename)
        {
            using (StreamReader streamReader = File.OpenText(filename))
            { 
                return streamReader.ReadLine(); 
            }
        } 

        //
        /// <summary>
        /// iterates through all files in a directory that match the given pattern. If checkHeader, each file must have the same header that
        /// matches the given header.
        /// </summary> 
        static public IEnumerable<Dictionary<string, string>> TabDirectoryTable(string directoryName, string filePattern, 
            string header, bool includeWholeLine, bool checkHeaderMatch)
        { 
            foreach (string filename in Directory.GetFiles(directoryName, filePattern))
            {
                //Console.WriteLine("SpecialFunction.TabDirectoryTable: opening file " + new FileInfo(filename).Name);
                foreach (Dictionary<string, string> row in TabFileTable(filename, header, includeWholeLine, checkHeaderMatch))
                {
                    yield return row; 
                } 
            }
        } 

        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, string header, bool includeWholeLine)
        {
            return TabFileTable(null, null, filename, header, includeWholeLine, '\t', true);
        }
        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, string header, bool includeWholeLine, bool checkHeaderMatch) 
        { 
            return TabFileTable(null, null, filename, header, includeWholeLine, '\t', checkHeaderMatch);
        } 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string header, bool includeWholeLine)
        {
            return TabFileTable(textReader, "STREAM", header, includeWholeLine, '\t', true);
        }
        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string header, bool includeWholeLine, bool checkHeaderMatch)
        { 
            return TabFileTable(textReader, "STREAM", header, includeWholeLine, '\t', checkHeaderMatch, false); 
        }
 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string header, bool includeWholeLine, bool checkHeaderMatch, bool includeHeaderAsFirstLine)
        {
            return TabFileTable(textReader, "STREAM", header, includeWholeLine, '\t', checkHeaderMatch, includeHeaderAsFirstLine);
        }

        static public IEnumerable<Dictionary<string, string>> TabFileTable(Assembly assembly, string resourcePrefix, string filename, 
            string header, bool includeWholeLine, char separator, bool checkHeaderMatch) 
        {
            using (TextReader textReader = OpenResourceOrFile(assembly, resourcePrefix, filename)) 
            {
                foreach (Dictionary<string, string> row in TabFileTable(textReader, filename, header, includeWholeLine, separator, checkHeaderMatch))
                {
                    yield return row;
                }
            } 
        } 

        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string inputName, 
            string header, bool includeWholeLine, char separator, bool checkHeaderMatch)
        {
            return TabFileTable(textReader, inputName, header, includeWholeLine, separator, checkHeaderMatch, false);
        }

        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string inputName, 
            string header, bool includeWholeLine, char separator, bool checkHeaderMatch, bool includeHeaderAsFirstLine) 
        {
 
            string line = textReader.ReadLine();
            SpecialFunctions.CheckCondition(null != line, "Input is empty so can't read header");

            if (checkHeaderMatch)
            {
                SpecialFunctions.CheckCondition(line.Equals(header, StringComparison.CurrentCultureIgnoreCase), string.Format("The input doesn't have the exact expected header.\nEXPECTED:\n {0}\nOBSERVED:\n{1}\nINPUT NAME:\n{2}", header, line, inputName)); //!!!raise error 
            } 
            else
            { 
                header = line;
            }

            string[] headerCollection = header.Split(separator);
            //while (null != (line = textReader.ReadLine()))
            bool firstTime = true; 
 
            // use do-while so we can return header as the first row, if requested.
            do 
            {
                if (firstTime && !includeHeaderAsFirstLine)
                {
                    firstTime = false;
                    continue;
                } 
 
                if (line.Length == 0) continue;
 
                string[] fieldCollection = line.Split(separator);
                SpecialFunctions.CheckCondition(!checkHeaderMatch || fieldCollection.Length == headerCollection.Length,
                    string.Format("The input doesn't have the expected number of columns. Header Length:{0}, LineLength:{1}\nHeader:{2}\nLine:{3}\nInputName:{4}",
                    headerCollection.Length, fieldCollection.Length, header, line, inputName)); //!!!raise error

                // if we're not checking for a header match, we still can't deal with lines of the wrong length. just ignore them. 
                if (fieldCollection.Length != headerCollection.Length) 
                    continue;
 
                Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                if (includeWholeLine)
                {
                    row.Add("", line);
                }
                for (int iField = 0; iField < fieldCollection.Length; ++iField) 
                { 
                    if (headerCollection[iField] == "")
                    { 
                        SpecialFunctions.CheckCondition(fieldCollection[iField] == "");
                    }
                    else
                    {
                        if (!row.ContainsKey(headerCollection[iField]))
                        { 
                            row.Add(headerCollection[iField], fieldCollection[iField]); 
                        }
                        else 
                        {
                            if (row[headerCollection[iField]] != fieldCollection[iField])
                            {
                                try
                                {
                                    double r1 = double.Parse(row[headerCollection[iField]]); 
                                    double r2 = double.Parse(fieldCollection[iField]); 
                                    Debug.Assert(Math.Abs(r1 - r2) < .000000001);
                                } 
                                finally
                                {
                                }
                            }
                        }
                    } 
                } 
                //Dictionary<string, string> row = new Dictionary<string, string>(rowX, StringComparer.CurrentCultureIgnoreCase);
                yield return row; 

            } while (null != (line = textReader.ReadLine()));
        }
        //General
        public static List<Dictionary<string, string>> TabFileTableAsList(Assembly assembly, string resourcePrefix, string filename, string header, bool includeWholeLine)
        { 
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>(); 
            foreach (Dictionary<string, string> row in TabFileTable(assembly, resourcePrefix, filename, header, includeWholeLine, '\t', true))
            { 
                list.Add(row);
            }
            return list;
        }

        /// <summary> 
        /// Returns the value associated with key in the dictionary. If not present, adds the default value to the dictionary and returns that 
        /// value.
        /// </summary> 
        [Obsolete("This has be superseded  by 'dictionary.GetValueOrDefault(key)'")]
        public static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            return dictionary.GetValueOrDefault(key);
        }
 
        /// <summary> 
        /// Returns the value associated with key in the dictionary. If not present, adds the default value to the dictionary and returns that
        /// value. 
        /// </summary>
        [Obsolete("This has be superseded  by 'dictionary.GetValueOrDefault(key, defaultValue)'")]
        public static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return dictionary.GetValueOrDefault(key, defaultValue);
        } 
 
        public static StreamReader OpenResource(Assembly assembly, string prefix, string fileName)
        { 
            string s = prefix + fileName;
            Stream stream = assembly.GetManifestResourceStream(s);
            SpecialFunctions.CheckCondition(stream != null, "Couldn't find input resource " + s);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader;
        } 
 
        public static StreamReader OpenResourceOrFile(Assembly assembly, string resourcePrefix, string filename)
        { 
            if (File.Exists(filename) || resourcePrefix == null)
            {
                //return File.OpenText(filename);
                return OpenTextOrZippedText(filename);
            }
            else 
            { 
                return OpenResource(assembly, resourcePrefix, filename);
            } 
        }

        private static StreamReader OpenTextOrZippedText(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            if (fileInfo.Extension.Equals(".gz", StringComparison.CurrentCultureIgnoreCase) || 
                fileInfo.Extension.Equals(".gzip", StringComparison.CurrentCultureIgnoreCase)) 
            {
                FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read); 
                GZipStream zipStream = new GZipStream(infile, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(zipStream);
                return reader;
            }
            else
            { 
                return fileInfo.OpenText(); 
            }
        } 


        /// <summary>
        /// Does not reuse or modify the input or output itemCollection
        /// </summary>
        public static IEnumerable<List<T>> EveryPermuation<T>(List<T> itemCollection) 
        { 
            CheckCondition(itemCollection.Count >= 0);
            if (itemCollection.Count == 0) 
            {
                yield return new List<T>();
            }
            else
            {
                for (int i = 0; i < itemCollection.Count; ++i) 
                { 
                    T first = itemCollection[i];
                    List<T> rest = new List<T>(itemCollection); 
                    rest.RemoveAt(i);
                    foreach (List<T> restPerm in EveryPermuation(rest))
                    {
                        List<T> returnThis = new List<T>(itemCollection.Count);
                        returnThis.Add(first);
                        returnThis.AddRange(restPerm); 
                        yield return returnThis; 
                    }
                } 
            }
        }


        public static IEnumerable<List<T>> EveryCombination<T>(IEnumerable<IEnumerable<T>> listList)
        { 
            IEnumerable<T> choicesFor1stPosition; 
            if (TryFirst<IEnumerable<T>>(listList, out choicesFor1stPosition))
            { 
                foreach (T t in choicesFor1stPosition)
                {
                    foreach (List<T> comboFromRest in EveryCombination(SpecialFunctions.Rest(listList)))
                    {
                        List<T> combination = new List<T>();
                        combination.Add(t); 
                        combination.AddRange(comboFromRest); 
                        yield return combination;
                    } 
                }
            }
            else
            {
                yield return new List<T>();
            } 
        } 

 
        public static IEnumerable<List<T>> EveryCombinationOfListEnum<T>(IEnumerable<List<T>> listList)
        {
            List<T> choicesFor1stPosition;
            if (TryFirst<List<T>>(listList, out choicesFor1stPosition))
            {
                foreach (T t in choicesFor1stPosition) 
                { 
                    foreach (List<T> comboFromRest in EveryCombinationOfListEnum(SpecialFunctions.Rest(listList)))
                    { 
                        List<T> combination = new List<T>();
                        combination.Add(t);
                        combination.AddRange(comboFromRest);
                        yield return combination;
                    }
                } 
            } 
            else
            { 
                yield return new List<T>();
            }
        }


        //Why can't this be combined with the previous one? 
        public static IEnumerable<List<T>> EveryCombination<T>(IEnumerable<Set<T>> listSet) 
        {
            Set<T> choicesFor1stPosition; 
            if (TryFirst<Set<T>>(listSet, out choicesFor1stPosition))
            {
                foreach (T t in choicesFor1stPosition)
                {
                    foreach (List<T> comboFromRest in EveryCombination(SpecialFunctions.Rest(listSet)))
                    { 
                        List<T> combination = new List<T>(); 
                        combination.Add(t);
                        combination.AddRange(comboFromRest); 
                        yield return combination;
                    }
                }
            }
            else
            { 
                yield return new List<T>(); 
            }
        } 

        //Why can't this be combined with the previous one?
        public static IEnumerable<List<T>> EveryCombination<T>(IEnumerable<HashSet<T>> listSet)
        {
            HashSet<T> choicesFor1stPosition;
            if (TryFirst<HashSet<T>>(listSet, out choicesFor1stPosition)) 
            { 
                foreach (T t in choicesFor1stPosition)
                { 
                    foreach (List<T> comboFromRest in EveryCombination(SpecialFunctions.Rest(listSet)))
                    {
                        List<T> combination = new List<T>();
                        combination.Add(t);
                        combination.AddRange(comboFromRest);
                        yield return combination; 
                    } 
                }
            } 
            else
            {
                yield return new List<T>();
            }
        }
 
        //!!!Why have both Join and CreateDelimitedString And CreateTabString? 
        [Obsolete("This has be superseded  by 'ienum.StringJoin(separator)'")]
        public static string Join<T>(string separator, IEnumerable<T> list) 
        {
            return CreateDelimitedString2(separator, list);
        }

        public static void AddWithCheck<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        { 
            if (dictionary.ContainsKey(key)) 
            {
                CheckCondition(dictionary[key].Equals(value), string.Format("Key '{0}' added to dictionary with both value '{1}' and value '{2}'", key, dictionary[key], value)); 
            }
            else
            {
                dictionary.Add(key, value);
            }
        } 
 
        [Obsolete("This has be superseded  by the System.Linq Count<T> extension method")]
        public static int Count<T>(IEnumerable<T> enumerable)
        {
            // no sense in the long version if there's a shortcut.
            if (enumerable is ICollection<T>)
            { 
                return ((ICollection<T>)enumerable).Count; 
            }
 
            int count = 0;
            foreach (T item in enumerable)
            {
                ++count;
            }
            return count; 
        } 

        //!!!should make sure that T is sortable? 
        static public IList<T> GetRandomSortedCollection<T>(int goalCount, IList<T> list, ref Random random)
        {
            //!!!could make a version that is efficient even if all are wanted
            SpecialFunctions.CheckCondition(goalCount * 2 <= list.Count, "Number of items desired must be less than half the number available.");
            SortedList<T, bool> results = new SortedList<T, bool>();
            GetRandomSortedCollectionInternal(goalCount, list, ref random, ref results); 
            return results.Keys; 
        }
 
        static private void GetRandomSortedCollectionInternal<T>(int goalCount, IList<T> list, ref Random random, ref SortedList<T, bool> results)
        {
            if (goalCount == 0)
            {
                return;
            } 
 
            while (true)
            { 
                T randomItem = list[random.Next(list.Count)];
                if (!results.ContainsKey(randomItem))
                {
                    results.Add(randomItem, true);
                    GetRandomSortedCollectionInternal(goalCount - 1, list, ref random, ref results);
                    return; 
                } 
            }
        } 


        static public IEnumerable<Dictionary<string, string>> TabFileTableNoHeaderInFile(string filename, string header, bool includeWholeLine)
        {
            return TabFileTableNoHeaderInFile(null, null, filename, header, includeWholeLine, '\t');
        } 
 
        static public IEnumerable<Dictionary<string, string>> TabFileTableNoHeaderInFile(string filename, string header, char separator)
        { 
            return TabFileTableNoHeaderInFile(null, null, filename, header, false, separator);
        }

        public static IEnumerable<Dictionary<string, string>> TabFileTableNoHeaderInFile(Assembly assembly, string resourcePrefix, string filename, string header, bool includeWholeLine, char separator)
        {
            using (StreamReader streamReader = OpenResourceOrFile(assembly, resourcePrefix, filename)) 
            { 
                string line = null;
                string[] headerCollection = header.Split(separator); 
                while (null != (line = streamReader.ReadLine()))
                {
                    string[] fieldCollection = line.Split(separator);
                    if (fieldCollection.Length <= 1 && headerCollection.Length > 1)
                        continue;
 
                    SpecialFunctions.CheckCondition(fieldCollection.Length == headerCollection.Length); //!!!raise error 

                    Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase); 
                    if (includeWholeLine)
                    {
                        row.Add("", line);
                    }
                    for (int iField = 0; iField < fieldCollection.Length; ++iField)
                    { 
                        if (headerCollection[iField] == "") 
                        {
                            SpecialFunctions.CheckCondition(fieldCollection[iField] == ""); 
                        }
                        else
                        {
                            if (!row.ContainsKey(headerCollection[iField]))
                            {
                                row.Add(headerCollection[iField], fieldCollection[iField]); 
                            } 
                            else
                            { 
                                if (row[headerCollection[iField]] != fieldCollection[iField])
                                {
                                    try
                                    {
                                        double r1 = double.Parse(row[headerCollection[iField]]);
                                        double r2 = double.Parse(fieldCollection[iField]); 
                                        Debug.Assert(Math.Abs(r1 - r2) < .000000001); 
                                    }
                                    finally 
                                    {
                                    }
                                }
                            }
                        }
                    } 
                    //Dictionary<string, string> row = new Dictionary<string, string>(rowX, StringComparer.CurrentCultureIgnoreCase); 
                    yield return row;
                } 
            }
        }

        public static double Sum(IEnumerable<double> doubleList)
        {
            double total = 0.0; 
            foreach (double r in doubleList) 
            {
                total += r; 
            }
            return total;
        }

        //!!!If C# allowed generic types to be constrained numbers the versions of Sum could be combined.
        public static int Sum(IEnumerable<int> intList) 
        { 
            int total = 0;
            foreach (int r in intList) 
            {
                total += r;
            }
            return total;
        }
 
        public static double Mean(IEnumerable<double> doubleList) 
        {
            int count = 0; 
            double total = 0.0;
            foreach (double r in doubleList)
            {
                total += r;
                ++count;
            } 
 
            CheckCondition(count > 0, "Mean of zero items is not defined.");
            double mean = total / (double)count; 
            return mean;
        }

        public static List<T> Shuffle<T>(IEnumerable<T> input, ref Random random)
        {
            List<T> list = new List<T>(); 
            foreach (T t in input) 
            {
                list.Add(t); //We put the value here to get the new space allocated 
                int oldIndex = random.Next(list.Count);
                list[list.Count - 1] = list[oldIndex];
                list[oldIndex] = t;
            }
            return list;
        } 
 
        public static void ShuffleInPlace<T>(ref List<T> listToShuffle, ref Random random)
        { 
            for (int i = 0; i < listToShuffle.Count; i++)
            {
                int j = random.Next(listToShuffle.Count);
                Swap(listToShuffle, i, j);
            }
        } 
 
        private static void Swap<T>(List<T> list, int i, int j)
        { 
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }

        public static double ComputeFDR(double pValue, 
            List<double> sortedPooledPValueCollectionFromData, 
            List<double> sortedPooledPValueCollectionFromRandomization, // "potentially significant" randomized pValues
            double numberOfRandomizationRuns   // number of null runs (eg, if ran -1-50, number of null runs is 50) 
            )
        {
            int dataAssociationsBelowOrAt = CountItemsBelowOrAt(sortedPooledPValueCollectionFromData, pValue);
            int randomizedAssociationsBelowOrAt = CountItemsBelowOrAt(sortedPooledPValueCollectionFromRandomization, pValue);

            // if all of pValue is no better than any of the randomization runs 
            // return a flag value that says we know nothing except that this is a bad pValue. 
            // It may be that the correct answer is 1,
            // but in order to allow data compression, we need to be agnostic. 
            if (randomizedAssociationsBelowOrAt == sortedPooledPValueCollectionFromRandomization.Count)
            {
                return 10.0;
            }

            // do logs to prevent over/under flow. 
            double fdr = randomizedAssociationsBelowOrAt / 
                 (double)(dataAssociationsBelowOrAt * numberOfRandomizationRuns);
 
            return fdr;
        }

        static public int CountItemsBelowOrAt(List<double> sortedPooledPValueCollection, double pValue)
        {
            int index = sortedPooledPValueCollection.BinarySearch(pValue); 
            if (index < 0) 
            {
                index = ~index; 
            }
            else
            {
                // if pValue is in the collection, BinarySearch returns one of the instances, with no guarantee
                // of which one. thus, we keep incrementing index until it's equal to the size of the collection
                // or it points to the first element in the collection that is strictly greater than pValue. 
                while (++index < sortedPooledPValueCollection.Count && 
                    sortedPooledPValueCollection[index] <= pValue) { }
            } 


            //Check that sorted
            if (index < sortedPooledPValueCollection.Count - 1)
            {
                SpecialFunctions.CheckCondition(sortedPooledPValueCollection[index] <= sortedPooledPValueCollection[index + 1], 
                    "sortedPooledPValueCollection isn't sorted: " + 
                    String.Format("sortedPooled...[{0}] = {1}; sortedPolled...[{2}] = {3}",
                        index, sortedPooledPValueCollection[index], index + 1, sortedPooledPValueCollection[index + 1]) 
                    );
            }

            return index;
        }
 
        public static int DoubleGreaterThan(double r1, double r2) 
        {
            return r1.CompareTo(r2); 
        }

        public static int DoubleLessThan(double r1, double r2)
        {
            return r2.CompareTo(r1);
        } 
 
        public static int IntGreaterThan(int i1, int i2)
        { 
            return i1.CompareTo(i2);
        }

        public static int IntLessThan(int i1, int i2)
        {
            return i2.CompareTo(i1); 
        } 

        public static Dictionary<TRow, double> ComputeQValues<TRow>( 
                ref List<TRow> realRowCollectionToSort,
                Converter<TRow, double> AccessValueFromRow,
                ref List<double> nullValueCollectionToBeSorted, // only those pValues that are potentially of some interest
                double numberOfRandomizationRuns) // should be around 0..50. if way more, you don't understand what this is. it's the number of runs, not trials.
        {
 
            if (numberOfRandomizationRuns > 0) 
            {
                return ComputeQValuesUseNulls(ref realRowCollectionToSort, AccessValueFromRow, ref nullValueCollectionToBeSorted, numberOfRandomizationRuns); 
            }
            else
            {
                return ComputeQValuesUseStoreyTibsharani(ref realRowCollectionToSort, AccessValueFromRow, ref nullValueCollectionToBeSorted);
            }
        } 
 
        public static Dictionary<TRow, double> ComputeQValuesUseNulls<TRow>(
                ref List<TRow> realRowCollectionToSort, 
                Converter<TRow, double> AccessValueFromRow,
                ref List<double> nullValueCollectionToBeSorted, // only those pValues that are potentially of some interest
                double numberOfRandomizationRuns) // should be around 1..50. if way more, you don't understand what this is. it's the number of runs, not trials.
        {
            realRowCollectionToSort.Sort(delegate(TRow row1, TRow row2)
                { 
                    double pValue1 = AccessValueFromRow(row1); 
                    double pValue2 = AccessValueFromRow(row2);
                    return pValue1.CompareTo(pValue2); 
                });
            nullValueCollectionToBeSorted.Sort();
            List<double> realPValueCollectionSorted = realRowCollectionToSort.ConvertAll<double>(AccessValueFromRow);

            double qValueRunning = double.PositiveInfinity;
            Dictionary<double, double> pValueToFdr = new Dictionary<double, double>();  //Fdr = false discovery rate 
            Dictionary<double, double> pValueToQValue = new Dictionary<double, double>(); 
            realRowCollectionToSort.Reverse(); //Go through backwards so can compute qValue in one pass
            foreach (TRow row in realRowCollectionToSort) 
            {
                double pValue = AccessValueFromRow(row);
                double fdr = ComputeFDR(pValue, realPValueCollectionSorted, nullValueCollectionToBeSorted, numberOfRandomizationRuns);
                pValueToFdr[pValue] = fdr; // OK to override value because will be the same
                qValueRunning = Math.Min(qValueRunning, fdr);
                pValueToQValue[pValue] = qValueRunning; // OK to override value because will be the same 
            } 
            realRowCollectionToSort.Reverse();
 
            Dictionary<TRow, double> results = new Dictionary<TRow, double>();
            foreach (TRow row in realRowCollectionToSort)
            {
                double pValue = AccessValueFromRow(row);
                double fdr = pValueToFdr[pValue];
                double qValueFinal = pValueToQValue[pValue]; 
                results[row] = qValueFinal; // OK to override value because will be the same 
            }
            return results; 
        }

        public static Dictionary<TRow, double> ComputeQValuesUseStoreyTibsharani<TRow>(
            ref List<TRow> realRowCollectionToSort,
            Converter<TRow, double> AccessValueFromRow,
            ref List<double> allPValues) 
        { 

            Console.WriteLine("Computing Storey-Tibshirani q's over m={0}", allPValues.Count); 

            realRowCollectionToSort.Sort((row1, row2) => AccessValueFromRow(row1).CompareTo(AccessValueFromRow(row2)));

            Dictionary<TRow, double> results = new Dictionary<TRow, double>();
            double currentQ = double.MaxValue;
            for (int j = realRowCollectionToSort.Count - 1; j >= 0; j--) 
            { 
                double pvalue = AccessValueFromRow(realRowCollectionToSort[j]);
                double eNumNull = pvalue * allPValues.Count; 
                double numReal = j + 1;
                double fdr = eNumNull / numReal;
                currentQ = Math.Min(fdr, currentQ);

                results.Add(realRowCollectionToSort[j], currentQ);
            } 
 
            return results;
        } 


        public static IEnumerable<KeyValuePair<T1, T2>> EnumerateTwo<T1, T2>(IEnumerable<T1> enum1, IEnumerable<T2> enum2, bool checkSameLength)
        {
            IEnumerator<T1> eor1 = enum1.GetEnumerator();
            IEnumerator<T2> eor2 = enum2.GetEnumerator(); 
            while (true) 
            {
                bool b1 = eor1.MoveNext(); 
                bool b2 = eor2.MoveNext();
                if (!b1 || !b2)
                {
                    SpecialFunctions.CheckCondition(!checkSameLength || b1 == b2, "Enumerations must be the same length");
                    break;
                } 
                yield return new KeyValuePair<T1, T2>(eor1.Current, eor2.Current); 
            }
        } 

        public static IEnumerable<KeyValuePair<T1, T2>> EnumerateTwoLonger<T1, T2>(IEnumerable<T1> enum1, IEnumerable<T2> enum2)
        {
            IEnumerator<T1> eor1 = enum1.GetEnumerator();
            IEnumerator<T2> eor2 = enum2.GetEnumerator();
            while (true) 
            { 
                bool b1 = eor1.MoveNext();
                bool b2 = eor2.MoveNext(); 
                if (!b1 && !b2)
                {
                    break;
                }

                yield return new KeyValuePair<T1, T2>(b1 ? eor1.Current : default(T1), b2 ? eor2.Current : default(T2)); 
            } 
        }
 

        /// <summary>
        /// Enumerates two enumerables.
        /// Checks that enumerations are the same length
        /// </summary>
        public static IEnumerable<KeyValuePair<T1, T2>> EnumerateTwo<T1, T2>(IEnumerable<T1> enum1, IEnumerable<T2> enum2) 
        { 
            return EnumerateTwo(enum1, enum2, true);
        } 

        /// <summary>
        /// Return each item in each enumerable.
        /// </summary>
        public static IEnumerable<T> EnumerateAll<T>(params IEnumerable<T>[] enumerables)
        { 
            foreach (IEnumerable<T> enumerable in enumerables) 
            {
                foreach (T item in enumerable) 
                {
                    yield return item;
                }
            }
        }
 
 

        public static IEnumerable<string> ReadEachLine(string fileName) 
        {
            using (TextReader textReader = File.OpenText(fileName))
            {
                string line;
                while (null != (line = textReader.ReadLine()))
                { 
                    yield return line; 
                }
            } 
        }
        public static IEnumerable<string> ReadEachLine(TextReader textReader)
        {
            string line;
            while (null != (line = textReader.ReadLine()))
            { 
                yield return line; 
            }
        } 

        public static IEnumerable<int> CreateRange(int c)
        {
            for (int i = 0; i < c; ++i)
            {
                yield return i; 
            } 
        }
 
        public static Dictionary<T2, T1> ReverseOneToOneDictionary<T1, T2>(IDictionary<T1, T2> dictionary)
        {
            Dictionary<T2, T1> result = new Dictionary<T2, T1>();
            foreach (KeyValuePair<T1, T2> t1AndT2 in dictionary)
            {
                result.Add(t1AndT2.Value, t1AndT2.Key); 
            } 
            return result;
        } 

        public static Dictionary<T2, Set<T1>> ReverseOneToManyDictionary<T1, T2>(IDictionary<T1, T2> dictionary)
        {
            Dictionary<T2, Set<T1>> result = new Dictionary<T2, Set<T1>>();
            foreach (KeyValuePair<T1, T2> t1AndT2 in dictionary)
            { 
                T1 t1 = t1AndT2.Key; 
                T2 t2 = t1AndT2.Value;
 
                Set<T1> set = result.GetValueOrDefault(t2);
                set.AddNew(t1);
            }
            return result;
        }
 
 
        public static int WrapAroundLeftShift(int someInt, int count)
        { 
            //Tip: Use "?Convert.ToString(WrapAroundLeftShift(someInt,count),2)" to see this work
            int result = (someInt << count) | ((~(-1 << count)) & (someInt >> (8 * sizeof(int) - count)));
            return result;
        }

        public static int CountIf<T>(IEnumerable<T> collection, Predicate<T> testDelegate) 
        { 
            int total = 0;
            foreach (T t in collection) 
            {
                if (testDelegate(t))
                {
                    ++total;
                }
            } 
            return total; 
        }
 
        public static IEnumerable<T> Rest<T>(IEnumerable<T> args)
        {
            return Rest(args, 1);
        }

        public static IEnumerable<T> Rest<T>(IEnumerable<T> enumeration, int skipCount) 
        { 
            foreach (T t in enumeration)
            { 
                if (skipCount > 0)
                {
                    --skipCount;
                }
                else
                { 
                    yield return t; 
                }
            } 
        }

        [Obsolete("This has be superseded  by the System.Linq First<T> extension method")]
        public static T First<T>(IEnumerable<T> enumeration)
        {
            T t; 
            bool isOK = TryFirst(enumeration, out t); 
            CheckCondition(isOK, "Can't get 1st item from an empty enumeration");
            return t; 
        }


        public static IEnumerable<T> SubEnumeration<T>(IEnumerable<T> enumeration, int startIndex, int count)
        {
            foreach (T t in enumeration) 
            { 
                if (startIndex > 0)
                { 
                    --startIndex;
                }
                else if (count > 0)
                {
                    --count;
                    yield return t; 
                } 
                else
                { 
                    yield break;
                }
            }
        }

        public static IEnumerable<T> First<T>(IEnumerable<T> enumeration, int keepCount) 
        { 
            return SubEnumeration(enumeration, 0, keepCount);
        } 

        public static T FirstAndOnly<T>(IEnumerable<T> enumeration)
        {
            IEnumerator<T> enumor = enumeration.GetEnumerator();
            CheckCondition(enumor.MoveNext(), "Can't get first item");
            T t = enumor.Current; 
            CheckCondition(!enumor.MoveNext(), "More than one item available"); 
            return t;
        } 



        public static IEnumerable<KeyValuePair<T, T>> Neighbors<T>(IEnumerable<T> collection)
        {
            T previous = default(T); 
            bool first = true; 
            foreach (T item in collection)
            { 
                if (!first)
                {
                    yield return new KeyValuePair<T, T>(previous, item);
                }
                else
                { 
                    first = false; 
                }
                previous = item; 
            }
        }

        public static bool KeysEqual<T, Tv1, Tv2>(IDictionary<T, Tv1> set1, IDictionary<T, Tv2> set2)
        {
            if (set1.Count != set2.Count) 
            { 
                return false;
            } 

            foreach (T key in set1.Keys)
            {
                //Debug.Assert(set1[key]); // real assert - all values must be "true"
                if (!set2.ContainsKey(key))
                { 
                    return false; 
                }
                else 
                {
                    // Debug.Assert(set2[key]); // real assert - all values must be "true"
                }
            }
            return true;
        } 
 

 

        public static Set<Tkey> ExistenceMapToSet<Tkey>(Dictionary<Tkey, bool?> dictionary)
        {
            Set<Tkey> result = new Set<Tkey>();
            //Console.WriteLine("dictionary null ? " + (dictionary == null));
            foreach (Tkey key in dictionary.Keys) 
            { 
                bool? value = dictionary[key];
                if (value != null && (bool)value) 
                {
                    result.AddNewOrOld(key);
                }
            }
            return result;
        } 
 
        // randomizes the given mapping. returns a new mapping without changing that formal.
        public static Dictionary<Tkey, Tvalue> RandomizeMapping<Tkey, Tvalue>(IDictionary<Tkey, Tvalue> mapping, ref Random random) 
        {
            IEnumerable<Tvalue> shuffledValues = Shuffle<Tvalue>(mapping.Values, ref random);
            Dictionary<Tkey, Tvalue> result = new Dictionary<Tkey, Tvalue>(mapping.Count);
            foreach (KeyValuePair<Tkey, Tvalue> keyAndValue in EnumerateTwo(mapping.Keys, shuffledValues))
            {
                result.Add(keyAndValue.Key, keyAndValue.Value); 
            } 
            return result;
        } 

        /// <summary>
        /// Given two disjoint mappings, returns a new mapping that is the union of the two.
        /// </summary>
        public static Dictionary<Tkey, Tvalue> CombineMappings<Tkey, Tvalue>(IDictionary<Tkey, Tvalue> mapping1, IDictionary<Tkey, Tvalue> mapping2)
        { 
            Dictionary<Tkey, Tvalue> result = new Dictionary<Tkey, Tvalue>(mapping1.Count + mapping2.Count); 
            foreach (KeyValuePair<Tkey, Tvalue> keyAndValue in mapping1)
            { 
                result.Add(keyAndValue.Key, keyAndValue.Value);
            }
            foreach (KeyValuePair<Tkey, Tvalue> keyAndValue in mapping2)
            {
                result.Add(keyAndValue.Key, keyAndValue.Value);
            } 
            return result; 
        }
 

        // creates a random mapping between the elements in keys and those in values.
        // For now, we require that keys and values be the same size, though this could be generalized in the future.
        // Since we're making a dictionary, if there are duplicate values in keys, collisions will cause the first values to be
        // erased. It'd be best to make this a set, but there's no generic set interface in C# and it's difficult to use our own, since
        // this method is often used with the keySet of a map, which of course is not of type Set. 
        public static Dictionary<Tkey, Tvalue> CreateRandomMapping<Tkey, Tvalue>(IEnumerable<Tkey> keys, IEnumerable<Tvalue> values, ref Random random) 
        {
            Dictionary<Tkey, Tvalue> result = new Dictionary<Tkey, Tvalue>(); 

            IEnumerable<Tvalue> shuffledValues = Shuffle<Tvalue>(values, ref random);
            IEnumerator<Tvalue> shuffledValsEnum = shuffledValues.GetEnumerator();

            foreach (Tkey key in keys)
            { 
                // for now, require that values and keys are the same size. In general, we could make this wrap around 
                SpecialFunctions.CheckCondition(shuffledValsEnum.MoveNext(), "There are more keys than values in the random mapping");
                result.Add(key, shuffledValsEnum.Current); 
            }

            // for now, require that values and keys are the same size. In general, we could make this wrap around
            // At this point, if we can move next, then there were more values than keys.
            SpecialFunctions.CheckCondition(!shuffledValsEnum.MoveNext(), "There are more values than keys in the random mapping");  // for now, require that values and keys are the same size.
 
            return result; 
        }
 


        public static List<T> RemoveDuplicatesFromSortedList<T>(List<T> list)
        {
            List<T> result = new List<T>(list.Count);
            if (list.Count == 0) 
                return result; 

            T last = list[0]; 
            result.Add(last);

            foreach (T item in list)
            {
                if (!item.Equals(last))
                    result.Add(item); 
                last = item; 
            }
            return result; 
        }

        /// <summary>
        /// Returns the cube root of x. Is smart about the fact that cuberoot of a negative number is possible.
        /// </summary>
        public static double CubeRoot(double x) 
        { 
            bool isNeg = x < 0;
            if (isNeg) 
                x = -x;

            double y = Math.Pow(x, 1.0 / 3.0);
            if (isNeg)
                y = -y;
 
            return y; 
        }
 


        public static string ExpandParentheticalCode(string line, string varRoot)
        {
            StringBuilder newVariables = new StringBuilder();
            int numVariables = 0; 
            int endParen; 
            int matchingBeginParen;
            string subexpression; 
            string variableName;

            while ((endParen = line.IndexOf(')')) >= 0)
            {
                matchingBeginParen = line.LastIndexOf('(', endParen);
 
                subexpression = line.Substring(matchingBeginParen, endParen - matchingBeginParen + 1); 
                variableName = varRoot + (++numVariables);
 
                newVariables.AppendLine(string.Format("{0} = {1};", variableName, subexpression.Substring(1, subexpression.Length - 2)));   // take of the parens.
                line = line.Replace(subexpression, variableName);
            }

            newVariables.AppendLine(line.Replace(";", ";\n"));
 
            return newVariables.ToString(); 
        }
 
        public static string NormalizeCode(string line, string varRoot)
        {
            // regular expression to fully parenthesize rational numbers
            Regex rationalNumberRegEx = new Regex(@"[\d\.\s]+\/[\d\.\s]+");
            MatchEvaluator rationalNumberMatchEvaluator = delegate(Match m)
            { 
                return "(" + m.Value + ")"; 
            };
 
            // regex to convert ints to doubles
            Regex intToFloatRegEx = new Regex(@"\W\d+");
            MatchEvaluator intToFloatMatchEvaluator = delegate(Match m)
            {
                return m.Value + ".0";
            }; 
 
            string powerDefinitions;
            string lineWithNewPowerDefinitions = SimplifyPowerTerms(line, out powerDefinitions); 

            string fullyParenthesizedLines = rationalNumberRegEx.Replace(lineWithNewPowerDefinitions, rationalNumberMatchEvaluator);
            string doubleizedLines = intToFloatRegEx.Replace(fullyParenthesizedLines, intToFloatMatchEvaluator);
            string transformedLines = SpecialFunctions.ExpandParentheticalCode(doubleizedLines, varRoot);

            return powerDefinitions + transformedLines; 
        } 

        private static string SimplifyPowerTerms(string line, out string powerDefinitions) 
        {
            StringBuilder powerDefs = new StringBuilder();
            SortedDictionary<string, string> knownPowersToDefinition = new SortedDictionary<string, string>();

            Regex powerRegEx = new Regex(@"(\w+)\s*\^\s*(\d+)");
            MatchEvaluator powerReplacer = delegate(Match m) 
            { 
                string baseLabel = m.Groups[1].Value;
                int pow = int.Parse(m.Groups[2].Value); 

                string replacement = baseLabel + pow;
                if (!knownPowersToDefinition.ContainsKey(replacement))
                {
                    knownPowersToDefinition.Add(replacement, string.Format("{0} * {1};", baseLabel, pow == 2 ? baseLabel : baseLabel + (pow - 1)));
                } 
                return replacement; 
            };
 
            string result = powerRegEx.Replace(line, powerReplacer);

            foreach (KeyValuePair<string, string> replacementAndDefinition in knownPowersToDefinition)
            {
                powerDefs.AppendLine(string.Format("{0} = {1}", replacementAndDefinition.Key, replacementAndDefinition.Value));
            } 
 
            powerDefinitions = powerDefs.ToString();
            return result; 
        }

        public static string MatlabToCSharp(string normalizedCode)
        {
            Regex powerReplacerRegEx = new Regex(@"([\w\d\s\.]+)\^([\w\d\s\.]+)");
            MatchEvaluator powerReplacer = delegate(Match m) 
            { 
                return string.Format("ComplexNumber.Pow({0}, {1})", m.Groups[1], m.Groups[2]);
            }; 

            string cSharpCode = powerReplacerRegEx.Replace(normalizedCode, powerReplacer);
            cSharpCode = "i = new ComplexNumber(0, 1);\n" + cSharpCode;

            return cSharpCode;
        } 
 
        //!!!could the FileInfo() class be used??
        public static string BaseFileName(string filename) 
        {
            int fileDelim = filename.LastIndexOf('\\');
            if (fileDelim >= 0)
                filename = filename.Substring(fileDelim + 1);

            int suffixIdx = filename.LastIndexOf('.'); 
            if (suffixIdx == filename.Length - 4) //!!!where does "4" come from??? does it assume 3 letter suffix? 
                filename = filename.Substring(0, suffixIdx);
 
            return filename;
        }


        public static Dictionary<T, int> ValueToCount<T>(IEnumerable<T> valueCollection)
        { 
            Dictionary<T, int> valueToCount = new Dictionary<T, int>(); 
            foreach (T t in valueCollection)
            { 
                valueToCount[t] = 1 + valueToCount.GetValueOrDefault(t);
            }
            return valueToCount;
        }

        public static void ConvertBinarySeqToSparse(string inputFileName, string outputFileName) 
        { 
            using (TextWriter textWriter = File.CreateText(outputFileName))
            { 
                textWriter.WriteLine(SpecialFunctions.CreateTabString("var", "cid", "val"));
                foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(inputFileName, "n1pos	aa	pid	val", false))
                {
                    if (row["val"] != "M")
                    {
                        SpecialFunctions.CheckCondition(row["val"] == "T" || row["val"] == "F", "Expect val of 'T', 'F', or 'M'"); 
                        string val = (row["val"] == "T") ? "1" : "0"; 
                        string cid = row["pid"];
                        string var = string.Format("{0}@{1}", row["aa"], row["n1pos"]); 
                        textWriter.WriteLine(SpecialFunctions.CreateTabString(var, cid, val));
                    }
                }
            }
        }
 
        public static List<T> SubList<T>(IList<T> list, int startIndex, int length) 
        {
            List<T> result = new List<T>(length); 
            for (int i = 0; i < length; ++i)
            {
                result.Add(list[startIndex + i]);
            }
            return result;
        } 
 
        public static IEnumerable<string> SubstringEnumeration(string s, int length)
        { 
            for (int startIndex = 0; startIndex <= s.Length - length; ++startIndex)
            {
                yield return s.Substring(startIndex, length);
            }
        }
 
 
        public static T[] SubArray<T>(T[] args, int startingIndex)
        { 
            return SubArray(args, startingIndex, args.Length - startingIndex);
        }

        public static T[] SubArray<T>(T[] args, int startingIndex, int length)
        {
            T[] result = new T[length]; 
            for (int i = 0; i < length; i++) 
            {
                result[i] = args[i + startingIndex]; 
            }
            return result;
        }

        public static void CopyDirectory(string oldDirectoryName, string newDirectoryName, bool recursive)
        { 
            CopyDirectory(oldDirectoryName, newDirectoryName, recursive, false); 
        }
        public static void CopyDirectory(string oldDirectoryName, string newDirectoryName, bool recursive, bool laterDateOnly) 
        {
            Directory.CreateDirectory(newDirectoryName);

            DirectoryInfo oldDirectory = new DirectoryInfo(oldDirectoryName);
            foreach (FileInfo fileInfo in oldDirectory.GetFiles())
            { 
                string targetFileName = newDirectoryName + @"\" + fileInfo.Name; 
                if (laterDateOnly && File.Exists(targetFileName))
                { 
                    DateTime sourceTime = fileInfo.LastWriteTime;
                    DateTime targetTime = File.GetLastAccessTime(targetFileName);
                    if (targetTime >= sourceTime)
                    {
                        continue;
                    } 
                } 

                fileInfo.CopyTo(targetFileName, true); 
                if (((File.GetAttributes(targetFileName) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly))
                {
                    File.SetLastAccessTime(targetFileName, fileInfo.LastWriteTime);
                }
            }
 
            if (recursive) 
            {
                foreach (DirectoryInfo subdirectory in oldDirectory.GetDirectories()) 
                {
                    CopyDirectory(subdirectory.FullName, newDirectoryName + @"\" + subdirectory.Name, recursive, laterDateOnly);
                }
            }
        }
 
        public static double Sigmoid(double x) 
        {
            return 1.0 / (1.0 + Math.Exp(-x)); 
        }

        public static double GaussianProbability(double x, double mean, double variance)
        {
            double k = 1.0 / Math.Sqrt(variance * 2 * Math.PI);
            double p = k * Math.Exp(-Math.Pow(x - mean, 2) / (2 * variance)); 
            return p; 
        }
 
        public static int PickRandomBin(double[] probabilityDistribution, ref Random rand)
        {
            int classIndex = 0;
            double randomValue = rand.NextDouble();
            double sum = 0;
            while ((sum += probabilityDistribution[classIndex]) < randomValue) 
            { 
                classIndex++;
            } 
            return classIndex;
        }

        public static double[] ParseDoubleArrayArgs(string commaDelimitedDoubles, char delim)
        {
            string[] args = commaDelimitedDoubles.Split(delim); 
            double[] result = new double[args.Length]; 
            for (int i = 0; i < args.Length; i++)
            { 
                result[i] = double.Parse(args[i]);
            }
            return result;
        }

        public static void Die(string p) 
        { 
            throw new Exception(string.Format("Forced Exit:\n{0}", p));
        } 

        public static void ConvertDictionaryToDerivedClasses<T1, T2, T3, T4>(Dictionary<T1, T2> originalDict, out Dictionary<T3, T4> resultDict)
            where T3 : T1
            where T4 : T2
        {
            resultDict = new Dictionary<T3, T4>(originalDict.Count); 
            foreach (KeyValuePair<T1, T2> keyValuePair in originalDict) 
            {
                resultDict.Add((T3)keyValuePair.Key, (T4)keyValuePair.Value); 
            }
        }

        public static void ConvertDictionaryToBaseClasses<T1, T2, T3, T4>(Dictionary<T1, T2> originalDict, out Dictionary<T3, T4> resultDict)
            where T1 : T3
            where T2 : T4 
        { 
            resultDict = new Dictionary<T3, T4>(originalDict.Count);
            foreach (KeyValuePair<T1, T2> keyValuePair in originalDict) 
            {
                resultDict.Add((T3)keyValuePair.Key, (T4)keyValuePair.Value);
            }
        }

        public static List<TItem> CreateSortedList<TItem, TSortKey>( 
            IEnumerable<TItem> inputCollection, 
            Converter<TItem, TSortKey> accessor,
            Comparison<TSortKey> isBetter) 
        {
            CheckCondition(false, "need to test this");
            List<TItem> list = new List<TItem>(inputCollection);
            list.Sort(
                    delegate(TItem item1, TItem item2)
                    { 
                        TSortKey sortKey1 = accessor(item1); 
                        TSortKey sortKey2 = accessor(item2);
                        return isBetter(sortKey1, sortKey2); 
                    }
            );
            return list;
        }

        //public static TItem FindBest<TItem, TSortKey>(IEnumerable<TItem> originalAA0PositionToCount, 
        //				Converter<TItem, TSortKey> accessor, 
        //				Comparison<TSortKey> isBetter)
        //{ 

        //}

        public static bool IgnoreCaseContains(IList<string> stringList, string item)
        {
            return IgnoreCaseIndexOf(stringList, item) >= 0; 
        } 

        internal static int IgnoreCaseIndexOf(IList<string> stringList, string item) 
        {
            for (int i = 0; i < stringList.Count; ++i)
            {
                string s = stringList[i];
                if (s.Equals(item, StringComparison.CurrentCultureIgnoreCase))
                { 
                    return i; 
                }
            } 
            return -1;
        }


        public static List<string> Split(string line, params char[] separator)
        { 
            List<string> stringList = new List<string>(line.Split(separator)); 
            return stringList;
 
        }



        public static bool TryParse<T>(string s, out T t)
        { 
            return Parser.TryParse<T>(s, out t); 
        }
 
        internal static bool TryParseOld<T>(string s, out T t)
        {
            if (typeof(T).Equals(typeof(int)))
            {
                int i;
                bool b = int.TryParse(s, out i); 
                t = (T)(object)i; 
                return b;
            } 

            if (typeof(T).Equals(typeof(int?)))
            {
                if (s.ToLower() == "null")
                {
                    t = (T)(object)null; 
                    return true; 
                }
                int i; 
                bool b = int.TryParse(s, out i);
                t = (T)(object)i;
                return b;
            }

 
            if (typeof(T).Equals(typeof(string))) 
            {
                t = (T)(object)s; 
                return true;
            }

            if (typeof(T).Equals(typeof(double)))
            {
                double r; 
                bool b = double.TryParse(s, out r); 
                t = (T)(object)r;
                return b; 
            }

            if (typeof(T).Equals(typeof(double?)))
            {
                if (s.ToLower() == "null")
                { 
                    t = (T)(object)null; 
                    return true;
                } 

                double r;
                bool b = double.TryParse(s, out r);
                t = (T)(object)r;
                return b;
            } 
 
            if (typeof(T).Equals(typeof(bool)))
            { 
                bool r;
                bool b = bool.TryParse(s, out r);
                t = (T)(object)r;
                return b;
            }
 
            if (typeof(T).Equals(typeof(bool?))) 
            {
                if (s.ToLower() == "null") 
                {
                    t = (T)(object)null;
                    return true;
                }

                bool r; 
                bool b = bool.TryParse(s, out r); 
                t = (T)(object)r;
                return b; 
            }

            if (typeof(T).IsEnum)
            {
                int i;
                if (int.TryParse(s, out i)) 
                { 
                    //return Enum.GetName(typeof(T), i);
                    t = (T)(object)i; 
                    return true;
                }

                try
                {
                    t = (T)Enum.Parse(typeof(T), s); 
                    return true; 
                }
                catch (ArgumentException) 
                {
                }
                t = default(T);
                return false;

            } 
 
            //!!!could define a ITryParse and IParse interfact
            if (typeof(T).Equals(typeof(RangeCollection))) 
            {
                try
                {
                    t = (T)(object)RangeCollection.Parse(s);
                    return true;
                } 
                catch 
                {
                } 
                t = default(T);
                return false;
            }
            //!!!could use more reflection to make this work for a list of any type
            if (typeof(T).Equals(typeof(List<double>)))
            { 
                try 
                {
                    List<double> list = new List<double>(); 
                    foreach (string itemAsString in s.Split(','))
                    {
                        double item = double.Parse(itemAsString);
                        list.Add(item);
                    }
                    t = (T)(object)list; 
                    return true; 
                }
                catch 
                {
                }
                t = default(T);
                return false;
            }
            if (typeof(T).Equals(typeof(List<int>))) 
            { 
                try
                { 
                    List<int> list = new List<int>();
                    foreach (string itemAsString in s.Split(','))
                    {
                        int item = int.Parse(itemAsString);
                        list.Add(item);
                    } 
                    t = (T)(object)list; 
                    return true;
                } 
                catch
                {
                }
                t = default(T);
                return false;
            } 
            if (typeof(T).Equals(typeof(List<string>))) 
            {
                try 
                {
                    List<string> list = new List<string>();
                    foreach (string itemAsString in s.Split(','))
                    {
                        //string item = string.Parse(itemAsString);
                        list.Add(itemAsString); 
                    } 
                    t = (T)(object)list;
                    return true; 
                }
                catch
                {
                }
                t = default(T);
                return false; 
            } 

            if (typeof(T).Equals(typeof(TimeSpan))) 
            {
                TimeSpan r;
                bool b = TimeSpan.TryParse(s, out r);
                t = (T)(object)r;
                return b;
            } 
 
            //!!!could use more reflection to make this work for a Pair of any type
            if (typeof(T).Equals(typeof(Pair<double, double>))) 
            {
                try
                {
                    string[] itemCollection = s.Split(',');
                    CheckCondition(itemCollection.Length == 2, "Expected two items for pair");
                    Pair<double, double> pair = new Pair<double, double>(double.Parse(itemCollection[0]), double.Parse(itemCollection[1])); 
                    t = (T)(object)pair; 
                    return true;
                } 
                catch
                {
                }
                t = default(T);
                return false;
            } 
            if (typeof(T).Equals(typeof(Pair<int, int>))) 
            {
                try 
                {
                    string[] itemCollection = s.Split(',');
                    CheckCondition(itemCollection.Length == 2, "Expected two items for pair");
                    Pair<int, int> pair = new Pair<int, int>(int.Parse(itemCollection[0]), int.Parse(itemCollection[1]));
                    t = (T)(object)pair;
                    return true; 
                } 
                catch
                { 
                }
                t = default(T);
                return false;
            }

 
 
            CheckCondition(false, "Don't know how to parse " + typeof(T));
            t = default(T); 
            return false;

        }

        public static Dictionary<TKey, TValue> PairEnumerationToDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumeration)
        { 
            Dictionary<TKey, TValue> result = enumeration as Dictionary<TKey, TValue>; 
            if (null != enumeration)
            { 
                // you gave me a Dictionary stupid.
                return result;
            }

            result = new Dictionary<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValue> pair in enumeration) 
            { 
                result.Add(pair.Key, pair.Value);
            } 
            return result;
        }


        public static IEnumerable<List<T>> Transpose<T>(IEnumerable<List<T>> rowList)
        { 
            List<T> any; 
            bool isAny = TryFirst(rowList, out any);
            CheckCondition(isAny, "Can't transpose empty input"); 
            for (int iColumn = 0; iColumn < any.Count; ++iColumn)
            {
                List<T> columnList = new List<T>();
                foreach (List<T> row in rowList)
                {
                    CheckCondition(row.Count == any.Count, "All rows must have the same length"); 
                    columnList.Add(row[iColumn]); 
                }
                yield return columnList; 
            }
        }

        /// <summary>
        /// Must be rectangular (not ragged).
        /// </summary> 
        /// <typeparam name="T"></typeparam> 
        /// <param name="input"></param>
        /// <returns></returns> 
        public static T[][] TransposeArray<T>(T[][] input)
        {
            if (input.Length == 0)
                return input;

            int nRow = input.Length; 
            int nCol = input[0].Length; 

            T[][] result = new T[nCol][]; 
            for (int i = 0; i < nCol; i++)
            {
                result[i] = new T[nRow];
                for (int j = 0; j < nRow; j++)
                {
                    result[i][j] = input[j][i]; 
                } 
            }
            return result; 
        }

        public static void NormalizeRows(double[][] input)
        {
            for (int ii = 0; ii < input.Length; ii++)
            { 
                double sum = Sum(input[ii]); 
                for (int jj = 0; jj < input[ii].Length; jj++)
                    input[ii][jj] = input[ii][jj] / sum; 
            }
        }

        private static bool TryFirst<T>(IEnumerable<T> enumeration, out T any)
        {
            IEnumerator<T> enumerator = enumeration.GetEnumerator(); 
            if (enumerator.MoveNext()) 
            {
                any = enumerator.Current; 
                return true;
            }
            else
            {
                any = default(T);
                return false; 
            } 
        }
 
        public static T RandomFromMultinomial<T>(Dictionary<T, int> itemToCount, ref Random random)
        {
            T t = default(T);
            int previousItemCount = 0;
            foreach (KeyValuePair<T, int> itemAndCount in itemToCount)
            { 
                int count = itemAndCount.Value; 
                CheckCondition(count >= 0, "Multinomial distribution must not have a negative count");
                if (random.Next(previousItemCount + count) >= previousItemCount) 
                {
                    t = itemAndCount.Key;
                }
                previousItemCount += count;
            }
            CheckCondition(previousItemCount > 0, "Multinomial distribution must have a positive count"); 
            return t; 
        }
 

        public static List<T> CreateAllocatedList<T>(int count)
        {
            List<T> list = new List<T>(count);
            for (int i = 0; i < count; ++i)
            { 
                list.Add(default(T)); 
            }
            return list; 
        }


        public static bool DictionaryEqual<TKey, TValue>(IDictionary<TKey, TValue> a, IDictionary<TKey, TValue> b)
        {
            foreach (KeyValuePair<TKey, TValue> aKeyAndValue in a) 
            { 
                TValue bValue;
                if (!b.TryGetValue(aKeyAndValue.Key, out bValue)) 
                {
                    return false;
                }
                if (!aKeyAndValue.Value.Equals(bValue))
                {
                    return false; 
                } 
            }
 
            foreach (TKey bKey in b.Keys)
            {
                if (!a.ContainsKey(bKey))
                {
                    return false;
                } 
            } 

            return true; 
        }


        public static long RandomLong(ref Random random, long n)
        {
            if (n <= int.MaxValue) 
            { 
                return random.Next((int)n);
            } 


            // random.Next returns a number in the range 0 ... int.MaxValue-1 (inclusive)
            while (true)
            {
                int partCount = (int)Ceiling(n, int.MaxValue); 
                int part = random.Next(partCount); 
                long r = part * int.MaxValue + random.Next(int.MaxValue);
                if (r < n) 
                {
                    return r;
                }
            }
        }
 
        private static long Ceiling(long n, int p) 
        {
            long partCount = n / p; 
            if (p * partCount != n)
            {
                ++partCount;
            }
            return partCount;
        } 
 
        public static Dictionary<TGroup, Dictionary<TKey, TValue>> GroupBy<TGroup, TKey, TValue>(Dictionary<TKey, TValue> inputDictionary, Converter<KeyValuePair<TKey, TValue>, TGroup> groupExtractor)
        { 
            Dictionary<TGroup, Dictionary<TKey, TValue>> outputDictionary = new Dictionary<TGroup, Dictionary<TKey, TValue>>();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in inputDictionary)
            {
                TGroup group = groupExtractor(keyValuePair);
                Dictionary<TKey, TValue> littleDictonary = outputDictionary.GetValueOrDefault(group);
                littleDictonary.Add(keyValuePair.Key, keyValuePair.Value); 
            } 
            return outputDictionary;
        } 

        public static List<T> CreateSingletonList<T>(T item)
        {
            List<T> result = new List<T>();
            result.Add(item);
            return result; 
        } 

 
        public static double AreaUnderFdrFnrCurve(IList<bool> rankedListOfTrueClassifications)
        {
            int countTrue = 0;
            int countFalse = 0;
            double sum = 0;
 
            for (int i = 0; i < rankedListOfTrueClassifications.Count; i++) 
            {
                if (rankedListOfTrueClassifications[i]) 
                {
                    countTrue++;
                    if (countFalse > 0) // if 0, the term should be 0. This avoids div 0 errors.
                    {
                        sum += (double)countFalse / (i + 1) + (double)countFalse / i;
                    } 
                } 
                else
                { 
                    countFalse++;
                }
            }
            double area = sum / (2.0 * countTrue);
            if (double.IsNaN(area))
            { 
                Console.WriteLine("Found nan"); 
            }
            return area; 
        }



        /// <summary>
        /// Compute the area under the ROC curve. **Assumes there are no ties. 
        /// </summary> 
        /// <param name="rankedListOfTrueClassifications">Total ordered list of booleans. Values should be ordered in increasing order
        /// of significance. True indicates the value at that rank is really true (true positive), false indicates it's really false 
        /// (false positive).</param>
        /// <returns></returns>
        public static double RocAreaUnderCurve(IEnumerable<bool> rankedListOfTrueClassifications)
        {
            int countTrue = 0;
            int countFalse = 0; 
            int sumOfPositiveRanks = 0; 
            int i = -1;
            foreach (bool trueClassification in rankedListOfTrueClassifications) 
            {
                ++i;
                if (trueClassification)
                {
                    countTrue++;
                    sumOfPositiveRanks += i + 1; 
                } 
                else
                { 
                    countFalse++;
                }
            }

            double F = sumOfPositiveRanks - (countTrue * (countTrue + 1)) / 2.0;
 
            double areaUnderCurve = 1 - F / (countTrue * countFalse); 
            return areaUnderCurve;
        } 

        /// <summary>
        /// Returns the unnormalized AUC. That is, count the area under the TP vs Rank curve, where
        /// each TP adds height of unit 1.
        /// </summary>
        /// <param name="rankedListOfTrueClassifications"></param> 
        /// <returns></returns> 
        public static double RocUnNormalizedAreaUnderCurve(IList<bool> rankedListOfTrueClassifications)
        { 
            int auc = 0;
            int currentHeight = 0;
            for (int i = 0; i < rankedListOfTrueClassifications.Count; i++)
            {
                if (rankedListOfTrueClassifications[i])
                { 
                    currentHeight++; 
                }
                else 
                {
                    auc += currentHeight;
                }
            }
            return auc;
        } 
 
        /// <summary>
        /// Runs a permutation test and the predictions of two classifiers. 
        /// </summary>
        /// <param name="classifier1">True classifications of events, ordered by classifier1.</param>
        /// <param name="classifier2">True classifications of events, ordered by classifier2.</param>
        /// <param name="scoringFunction">Function that computes a score for each vector.</param>
        /// <param name="numPermutations"></param>
        /// <param name="simulatedScores">The distribution of classification differences under the null hypothesis will be dumped here.</param> 
        /// <returns>Probability that the discriminatory power of the two classifiers is different</returns> 
        public static double PermutationTestOfBooleanVectors(IList<bool> classifier1, IList<bool> classifier2, Converter<IList<bool>, double> scoringFunction,
            int numPermutations, bool twoTailed, out double[] simulatedScores) 
        {
            Random rand = new MachineInvariantRandom("PermutationTestOfBooleanVectors");
            return PermutationTestOfBooleanVectors(classifier1, classifier2, scoringFunction, numPermutations, twoTailed, ref rand, out simulatedScores);


        } 
 
        /// <summary>
        /// Runs a permutation test on the predictions of two classifiers. 
        /// </summary>
        /// <param name="classifier1">True classifications of events, ordered by classifier1.</param>
        /// <param name="classifier2">True classifications of events, ordered by classifier2.</param>
        /// <param name="scoringFunction">Function that computes a score for each vector.</param>
        /// <param name="numPermutations">The distribution of classification differences under the null hypothesis will be dumped here.</param>
        /// <param name="simulatedScores"></param> 
        /// <returns>Probability that the discriminatory power of the two classifiers is different</returns> 
        public static double PermutationTestOfBooleanVectors(IList<bool> classifier1, IList<bool> classifier2, Converter<IList<bool>, double> scoringFunction,
            int numPermutations, bool twoTailed, ref Random rand, out double[] simulatedScores) 
        {
            CheckCondition(classifier1.Count == classifier2.Count);

            int n = classifier1.Count;
            double observedScore = scoringFunction(classifier1) - scoringFunction(classifier2);
            if (twoTailed) 
            { 
                observedScore = Math.Abs(observedScore);
            } 
            //double observedDifference = Math.Abs(RocAreaUnderCurve(classifier1) - RocAreaUnderCurve(classifier2));
            //double observedDifference = RocAreaUnderCurve(classifier1) - RocAreaUnderCurve(classifier2);

            simulatedScores = new double[numPermutations];
            bool[] dummyClassifier1 = new bool[n];
            bool[] dummyClassifier2 = new bool[n]; 
 
            for (int iteration = 0; iteration < numPermutations; iteration++)
            { 
                for (int i = 0; i < n; i++)
                {
                    if (rand.NextDouble() < 0.5)
                    {
                        dummyClassifier1[i] = classifier1[i];
                        dummyClassifier2[i] = classifier2[i]; 
                    } 
                    else
                    { 
                        dummyClassifier1[i] = classifier2[i];
                        dummyClassifier2[i] = classifier1[i];
                    }
                }
                double fakeDiff = scoringFunction(dummyClassifier1) - scoringFunction(dummyClassifier2);
                if (twoTailed) 
                { 
                    fakeDiff = Math.Abs(fakeDiff);
                } 

                //double fakeDiff = Math.Abs(RocAreaUnderCurve(dummyClassifier1) - RocAreaUnderCurve(dummyClassifier2));
                //double fakeDiff = RocAreaUnderCurve(dummyClassifier1) - RocAreaUnderCurve(dummyClassifier2);
                simulatedScores[iteration] = fakeDiff;
            }
 
            double p = ComputePValueFromSimulationData(observedScore, simulatedScores, true); 
            return p;
        } 

        public static double ComputePValueFromSimulationData(double observedScore, double[] simulatedScores, bool higherIsBetter)
        {
            int numPermutations = simulatedScores.Length;
            // sort in descending order
 
            if (higherIsBetter) 
            {
                Array.Sort(simulatedScores, DoubleLessThan); 
            }
            else
            {
                Array.Sort(simulatedScores, DoubleGreaterThan);
            }
 
            int rankOfObsDiff = 0; 
            while (rankOfObsDiff < numPermutations &&
                (higherIsBetter ? 
                    observedScore <= simulatedScores[rankOfObsDiff] :
                    observedScore >= simulatedScores[rankOfObsDiff]))
            {
                rankOfObsDiff++;
            }
 
            double p = (double)rankOfObsDiff / numPermutations; 
            return p;
        } 

        public static double ZScoreToPValue(double z, double eps)
        {
            z = -Math.Abs(z);

            double p = 0.5 * (1 + Erf(z / Math.Sqrt(2), eps)); 
            if (p > 0.5) 
            {
                p = 1 - p; 
            }
            p *= 2;

            return p;
        }
 
        public static double Erf(double x, double eps) 
        {
            SpecialFunctions.CheckCondition(!double.IsNaN(x) && !double.IsInfinity(x)); 
            int erfSign = x < 0 ? -1 : 1;
            x = Math.Abs(x);

            double sum = 0;
            double lastSum = double.MinValue;
            double lnX = Math.Log(x); 
            double lnFactorial = 0; 

            for (int i = 0; i < 10000; i++) 
            {
                int sign = i % 2 == 0 ? 1 : -1;
                lnFactorial += i == 0 ? 0 : Math.Log(i);
                double logNumerator = Math.Log(x) * (2 * i + 1);
                double logDenominator = Math.Log(2 * i + 1) + lnFactorial;
 
                double term = sign * Math.Exp(logNumerator - logDenominator); 
                sum += term;
                if (Math.Abs(sum - lastSum) < eps) 
                {
                    break;
                }
                lastSum = sum;
            }
 
            double erf = erfSign * 2.0 * sum / Math.Sqrt(Math.PI); 
            return erf;
        } 

        public static void ConvertToLog(double[] dArr)
        {
            for (int i = 0; i < dArr.Length; i++)
            {
                dArr[i] = Math.Log(dArr[i]); 
            } 
        }
        public static void ConvertToLog(double[][] twoDArr) 
        {
            for (int i = 0; i < twoDArr.Length; i++)
            {
                ConvertToLog(twoDArr[i]);
            }
        } 
 
        public static void ConvertFromLog(double[] dArr)
        { 
            for (int i = 0; i < dArr.Length; i++)
            {
                dArr[i] = Math.Exp(dArr[i]);
            }
        }
        public static void ConvertFromLog(double[][] twoDArr) 
        { 
            for (int i = 0; i < twoDArr.Length; i++)
            { 
                ConvertFromLog(twoDArr[i]);
            }
        }

        public static IEnumerable<TItem> GroupByEnumeration<TItem, TGroup>(IEnumerable<TItem> inputList, Converter<TItem, TGroup> accessor, Random random)
        { 
            Dictionary<TGroup, Set<TItem>> dictionary = new Dictionary<TGroup, Set<TItem>>(); 
            foreach (TItem item in inputList)
            { 
                TGroup group = accessor(item);
                Set<TItem> set = dictionary.GetValueOrDefault(group);
                set.AddNew(item);
            }

            foreach (TGroup group in Shuffle(dictionary.Keys, ref random)) 
            { 
                foreach (TItem item in dictionary[group])
                { 
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> EnumerateDefault<T>(int count) where T : new() 
        { 
            for (int i = 0; i < count; ++i)
            { 
                yield return new T();
            }
        }

        public static IEnumerable<TItem> GroupByEnumerationTwoLevel<TItem, TGroup1, TGroup2>(IEnumerable<TItem> inputList, Converter<TItem, TGroup1> accessor1, Converter<TItem, TGroup2> accessor2, Random random)
        { 
            Dictionary<TGroup1, Set<TItem>> dictionary1 = new Dictionary<TGroup1, Set<TItem>>(); 
            foreach (TItem item in inputList)
            { 
                TGroup1 group1 = accessor1(item);
                Set<TItem> set = dictionary1.GetValueOrDefault(group1);
                set.AddNew(item);
            }

            foreach (TGroup1 group1 in Shuffle(dictionary1.Keys, ref random)) 
            { 
                foreach (TItem item in GroupByEnumeration(dictionary1[group1], accessor2, random))
                { 
                    yield return item;
                }
            }
        }

        public static void XCopyED(string localDirectoryName, string externalRemoteDirectoryName) 
        { 

        } 




        public static IEnumerable<KeyValuePair<T, T>> EveryPair<T>(IEnumerable<T> pairedFeatures)
        { 
            int i = 0; 
            foreach (T t1 in Rest(pairedFeatures)) // 2nd item, 3rd, 4th, ... last
            { 
                ++i;
                int j = -1;
                foreach (T t2 in pairedFeatures) // 1st item, 2nd item, ... one Less than item above
                {
                    ++j;
                    yield return new KeyValuePair<T, T>(t2, t1); 
                    // 1,2,	1,3, 2,3,	1,4, 2,4, 3,4, .... 
                }
            } 

        }

        public static FileStream GetExclusiveReadWriteFileStream(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename); 
            return GetExclusiveReadWriteFileStream(filename); 
        }
 
        static int sleepTimer = 200;
        public static FileStream GetExclusiveReadWriteFileStream(FileInfo fileInfo)
        {
            Random rand = new MachineInvariantRandom(Environment.CommandLine);//for PhyloD, need something that is uniq to each process that will be competing for this file
            FileStream fileStream;
            //int sleepTimer = 1000; 
            for (; ; ) 
            {
                try 
                {
                    fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    fileStream.Lock(0, int.MaxValue);
                    //sleepTimer = Math.Max(1000, sleepTimer - 1000);
                    return fileStream;
                } 
                catch (IOException) 
                {
                    Thread.Sleep(rand.Next(sleepTimer)); 
                    //sleepTimer *= 2;
                }
            }
        }

        public static IEnumerable<T2> Cast<T1, T2>(IEnumerable<T1> list) where T2 : T1 
        { 
            foreach (T1 t1 in list)
            { 
                yield return (T2)t1;
            }
        }

        public static IEnumerable<T2> Cast<T1, T2>(IEnumerable<T1> list, Converter<T1, T2> castMethod)
        { 
            foreach (T1 t1 in list) 
            {
                yield return castMethod(t1); 
            }
        }

        public static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> enumeration)
        {
            Set<T> seenIt = Set<T>.GetInstance(); 
            foreach (T t in enumeration) 
            {
                if (!seenIt.Contains(t)) 
                {
                    seenIt.AddNew(t);
                    yield return t;
                }
            }
        } 
 
        public static void WriteEachLine<T>(IEnumerable<T> list, string fileName)
        { 
            using (TextWriter textWriter = File.CreateText(fileName))
            {
                foreach (T t in list)
                {
                    textWriter.WriteLine(t.ToString());
                } 
            } 
        }
 
        public static void WriteToFile(object obj, string fileName)
        {
            using (TextWriter textWriter = File.CreateText(fileName))
            {
                textWriter.WriteLine(obj.ToString());
            } 
        } 

 
        public static IEnumerable<T> DivideWork<T>(IEnumerable<T> enumerable, RangeCollection pieceIndexRangeCollection, int pieceCount, int batchCount, RangeCollection skipList)
        {
            int pieceIndex = 0;
            int batchIndex = 0;
            bool inRange = pieceIndexRangeCollection.Contains(pieceIndex);
 
            foreach (T t in UseSkipList(enumerable, skipList)) 
            {
                if (inRange) 
                {
                    yield return t;
                }

                ++batchIndex;
                if (batchIndex == batchCount) 
                { 
                    batchIndex = 0;
                    pieceIndex = (pieceIndex + 1) % pieceCount; 
                    inRange = pieceIndexRangeCollection.Contains(pieceIndex);
                }
            }
        }

        private static IEnumerable<T> UseSkipList<T>(IEnumerable<T> enumerable, RangeCollection skipList) 
        { 
            int itemIndex = -1;
            foreach (T t in enumerable) 
            {
                ++itemIndex;
                if (skipList.Contains(itemIndex))
                {
                    continue;
                } 
                yield return t; 
            }
        } 

        /// <summary>
        /// Defines 0/0 := 0.
        /// </summary>
        public static double SafeDivide(double numerator, double denominator)
        { 
            return SafeDivide(numerator, denominator, double.Epsilon); 
        }
        public static double SafeDivide(double numerator, double denominator, double eps) 
        {
            double result = denominator == 0 && Math.Abs(numerator) < eps ? 0 : numerator / denominator;

            if (double.IsNaN(result))
            {
                throw new ArgumentException(string.Format("divide by zero error. {0}/{1} = {2}", numerator, denominator, result)); 
            } 
            return result;
        } 


        /// <summary>
        /// Converts p(A,B|C) to p(A|B,C) using p(B|C).
        /// </summary>
        /// <param name="jointABGivenC"></param> 
        /// <param name="marginalBGivenC"></param> 
        public static void JointToConditionalProb(ref double[][] jointABGivenC, double[] marginalBGivenC, bool useLogTransform)
        { 
            for (int iParentState = 0; iParentState < jointABGivenC.GetLength(0); iParentState++)
            {
                for (int iChildState = 0; iChildState < jointABGivenC[iParentState].Length; iChildState++)
                {
                    jointABGivenC[iParentState][iChildState] =
                        useLogTransform ? 
                            jointABGivenC[iParentState][iChildState] - marginalBGivenC[iParentState] : 
                            jointABGivenC[iParentState][iChildState] == 0 && marginalBGivenC[iParentState] == 0 ?
                                0 : 
                                jointABGivenC[iParentState][iChildState] / marginalBGivenC[iParentState];
                }
            }
        }

        public static void ConditionalToJointProb(ref double[][] pAGivenB, double[] pB, bool useLogTransform) 
        { 
            for (int iParentState = 0; iParentState < pAGivenB.GetLength(0); iParentState++)
            { 
                for (int iChildState = 0; iChildState < pAGivenB[iParentState].Length; iChildState++)
                {
                    pAGivenB[iParentState][iChildState] =
                        useLogTransform ?
                            pAGivenB[iParentState][iChildState] + pB[iParentState] :
                            pAGivenB[iParentState][iChildState] * pB[iParentState]; 
                } 
            }
        } 

        public static void InitializeDoubleArray(ref double[] dArr, int length, bool logspace)
        {
            if (dArr == null)
            {
                dArr = new double[length]; 
            } 
            else if (!logspace)
            { 
                Array.Clear(dArr, 0, dArr.Length);
            }
            if (logspace)
            {
                SetAllValues(ref dArr, double.NegativeInfinity);
            } 
        } 

 

        public static void CreateArrayIfNull<T>(ref T[][] array, int size1, int size2)
        {
            if (array == null || array.Length != size1)
            {
                array = new T[size1][]; 
                for (int i = 0; i < size1; i++) 
                {
                    array[i] = new T[size2]; 
                }
            }
        }

        public static void CreateArrayIfNull<T>(ref T[] array, int size1)
        { 
            if (array == null) 
            {
                array = new T[size1]; 
            }
        }


        public static double[][] CreateIdentityMatrix(int stateCount)
        { 
            double[][] id = new double[stateCount][]; 
            for (int i = 0; i < stateCount; i++)
            { 
                id[i] = new double[stateCount];
                id[i][i] = 1;
            }
            return id;
        }
 
        public static void CreateEqualProbabilityMatrix(double[][] id, int stateCount) 
        {
            //double[][] id = new double[stateCount][]; 
            for (int i = 0; i < stateCount; i++)
            {
                id[i] = new double[stateCount];
                for (int j = 0; j < stateCount; j++)
                {
                    id[i][j] = 1.0 / stateCount; 
                } 
            }
            //return id; 
        }

        public static void SetAllValues(ref double[] array, double value)
        {
            for (int i = 0; i < array.Length; i++)
            { 
                array[i] = value; 
            }
        } 


        public static void ComputeMeanAndVar(double[] values, out double mean, out double variance)
        {
            int n = values.Length;
            double sum = 0; 
            double ss = 0; 
            foreach (double d in values)
            { 
                sum += d;
                ss += d * d;
            }

            mean = sum / n;
            variance = (ss - (sum * sum / n)) / (n - 1); 
        } 

        public static void ComputeMeansAndVariances(double[][] setOfValues, out double[] means, out double[] variances) 
        {
            double[][] transposed = TransposeArray(setOfValues);

            means = new double[transposed.Length];
            variances = new double[transposed.Length];
 
            for (int i = 0; i < transposed.Length; i++) 
            {
                ComputeMeanAndVar(transposed[i], out means[i], out variances[i]); 
            }
        }



        public static T SelectRandom<T>(List<T> list, ref Random rand) 
        { 
            int index = rand.Next(list.Count);
            return list[index]; 
        }

        /// <summary>
        /// This could be slow, so only use it where speed is not important.
        /// </summary>
        public static void AppendLine(string fileName, string line) 
        { 
            using (StreamWriter streamWriter = File.AppendText(fileName))
            { 
                streamWriter.WriteLine(line);
            }
        }

        public static IEnumerable<int> EnumerateRange(int start, int last, int increment)
        { 
            SpecialFunctions.CheckCondition(increment == 1 || increment == -1, "increment must be 1 or -1"); 
            for (int i = start; ; i += increment)
            { 
                yield return i;
                if (i == last)
                {
                    break;
                }
            } 
        } 

        public static IEnumerable<T> EnumerateInterleave<T>(IEnumerable<T> enum1, IEnumerable<T> enum2) 
        {
            IEnumerator<T> eor1 = enum1.GetEnumerator();
            IEnumerator<T> eor2 = enum2.GetEnumerator();
            while (true)
            {
                if (!eor1.MoveNext()) 
                { 
                    while (eor2.MoveNext())
                    { 
                        yield return eor2.Current;
                    }
                    yield break;

                }
                if (!eor2.MoveNext()) 
                { 
                    while (true)
                    { 
                        yield return eor1.Current;
                        if (!eor1.MoveNext())
                        {
                            yield break;
                        }
                    } 
                } 

                yield return eor1.Current; 
                yield return eor2.Current;
            }
        }

        public static void SampleFromList<T>(List<T> srcList, int count, ref List<T> destList, ref Random rand)
        { 
            for (int i = 0; i < count; i++) 
            {
                int idx = rand.Next(0, srcList.Count); 
                destList.Add(srcList[idx]);
            }
        }


        public static T Next<T>(IEnumerator<T> lineCollection) 
        { 
            SpecialFunctions.CheckCondition(lineCollection.MoveNext(), "Can't get next");
            return lineCollection.Current; 
        }

        public static IEnumerable<T> EnumerateFilter<T>(IEnumerable<T> iEnumerable, Predicate<T> predicate)
        {
            foreach (T t in iEnumerable)
            { 
                if (!predicate(t)) 
                {
                    yield return t; 
                }
            }
        }

        public static StreamWriter GetTextWriterWithExternalReadAccess(string filename)
        { 
            return new StreamWriter(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read)); 
        }
 
        public static StreamReader GetTextReaderWithExternalReadWriteAccess(string filename)
        {
            return new StreamReader(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite));
        }

        public static void DeleteFilesPatiently(Queue<FileInfo> filesToDelete, int numberOfTimesToTry, TimeSpan timeToWaitBetweenRetries) 
        { 
            Queue<FileInfo> filesNotDeleted = new Queue<FileInfo>();
 
            foreach (FileInfo fileInfo in filesToDelete)
            {
                try
                {
                    fileInfo.Delete();
                } 
                catch (System.IO.IOException) 
                {
                    filesNotDeleted.Enqueue(fileInfo); 
                }
            }

            if (filesNotDeleted.Count > 0)
            {
                if (numberOfTimesToTry > 0) 
                { 
                    Console.WriteLine("Could not delete all files. Will retry after a short pause.");
                    System.Threading.Thread.Sleep(timeToWaitBetweenRetries); 
                    DeleteFilesPatiently(filesNotDeleted, numberOfTimesToTry - 1, timeToWaitBetweenRetries);
                }
                else
                {
                    StringBuilder sb = new StringBuilder("Could not delete the following files: ");
                    sb.Append(filesNotDeleted.StringJoin(",")); 
                    throw new Exception(sb.ToString()); 
                }
            } 
        }

        public static double FishersMinimax(int tt, int tf, int ft, int ff)
        {
            double sum = tt + tf + ft + ff;
            double v1TrueP = (tt + tf); 
            double v2TrueP = (tt + ft); 
            double v1FalseP = (ff + ft);
            double v2FalseP = (ff + tf); 

            double ett = v1TrueP * v2TrueP / sum;
            double etf = v1TrueP * v2FalseP / sum;
            double eft = v1FalseP * v2TrueP / sum;
            double eff = v1FalseP * v2FalseP / sum;
 
            double minCount = Math.Min(Math.Max(tt, ett), 
                              Math.Min(Math.Max(tf, etf),
                              Math.Min(Math.Max(ft, eft), 
                                       Math.Max(ff, eff))));

            return minCount;
        }

        // Generalization of the above method to more than 2 by 2 tables 
        public static double FishersMinimaxGeneralized(int[][] counts) 
        {
            double sum = 0; 
            double[] sumOfRows = new double[counts.Length];
            double[] sumOfCols = new double[counts[0].Length];
            // Calculate row/column sums
            for (int ii = 0; ii < counts.Length; ii++)
            {
                for (int jj = 0; jj < counts[ii].Length; jj++) 
                { 
                    sum += counts[ii][jj];
                    sumOfRows[ii] += counts[ii][jj]; 
                    sumOfCols[jj] += counts[ii][jj];
                }
            }
            // Calculates max of observed counts and expected counts
            // and keep track of smallest such value seen so far
            double min = double.PositiveInfinity; 
            for (int ii = 0; ii < counts.Length; ii++) 
                for (int jj = 0; jj < counts[ii].Length; jj++)
                { 
                    min = Math.Min(min, Math.Max(counts[ii][jj], sumOfRows[ii] * sumOfCols[jj] / sum));
                }
            return min;
        }

        public static double Product(double[] pValues) 
        { 
            double product = 0;
            foreach (double d in pValues) 
            {
                product += Math.Log(d);
            }
            return Math.Exp(product);
        }
 
        public static void IncrementBitArray(BitArray bitArr) 
        {
            IncrementBitArray(bitArr, 0); 
        }

        private static void IncrementBitArray(BitArray bitArr, int idx)
        {
            bitArr[idx] = !bitArr[idx];
            if (!bitArr[idx]) 
                IncrementBitArray(bitArr, (idx + 1) % bitArr.Count); 
        }
 
        internal static double NoisyOr(List<double> independentPTrueValues)
        {
            double pNot = 1;
            foreach (double pTrue_i in independentPTrueValues)
            {
                pNot *= 1 - pTrue_i; 
            } 
            double pTrue = 1 - pNot;
            return pTrue; 
        }

        public static void ProbabilityChildGivenGrandparent(double[][] pParentGivenGrandParent, double[][] pChildGivenParent, double[][] result, bool useLog)
        {
            int stateCount = pChildGivenParent.GetLength(0);
 
            //double[][] result = new double[stateCount][]; 
            for (int i = 0; i < stateCount; i++)
            { 
                //result[i] = new double[stateCount];
                SetAllValues(ref result[i], useLog ? double.NegativeInfinity : 0);

                for (int j = 0; j < stateCount; j++)
                {
                    for (int k = 0; k < stateCount; k++) 
                    { 
                        if (useLog)
                        { 
                            result[i][j] += pParentGivenGrandParent[i][k] + pChildGivenParent[k][j];

                        }
                        else
                        {
                            result[i][j] += pParentGivenGrandParent[i][k] * pChildGivenParent[k][j]; 
                        } 
                    }
                } 
            }
            //return result;
        }

        public static double Deg2Rad(double angleInDegrees)
        { 
            return angleInDegrees * Math.PI / 180; 
        }
 
        public static double Rad2Deg(double angleInRadians)
        {
            return angleInRadians * 180 / Math.PI;
        }

 
        public static double DistanceOfPointToLine(PointF queryPoint, PointF p1, PointF p2) 
        {
            double x2MinusX1 = p2.X - p1.X; 
            double y2MinusY1 = p2.Y - p1.Y;
            double dist = Math.Abs(x2MinusX1 * (p1.Y - queryPoint.Y) - (p1.X - queryPoint.X) * y2MinusY1);
            dist /= Math.Sqrt(x2MinusX1 * x2MinusX1 + y2MinusY1 * y2MinusY1);
            return dist;
        }
 
        public static void MergeFiles(DirectoryInfo dirinfo, IEnumerable<string> inputFilePatternCollection, string[] columnNamesToAdd, string outputFileName, bool deleteFilesOnSuccessfullMerge, bool printProgress) 
        {
            string tmpFile = outputFileName + new Random().Next(int.MaxValue); 
            List<Pair<string, string[]>> filePatternsAndColumnNames = new List<Pair<string, string[]>>();
            foreach (string filePattern in inputFilePatternCollection)
            {
                string[] filePatternAndColValues = filePattern.Split(':');
                SpecialFunctions.CheckCondition(filePatternAndColValues.Length == 1 + columnNamesToAdd.Length, string.Format("{0} does not have the right number of added columns specified. Expected {1}; found {2}.", filePattern, columnNamesToAdd.Length, filePatternAndColValues.Length - 1));
                string[] colVals = columnNamesToAdd.Length == 0 ? new string[0] : SpecialFunctions.SubArray(filePatternAndColValues, 1); 
                filePatternsAndColumnNames.Add(new Pair<string, string[]>(filePatternAndColValues[0], colVals)); 
            }
 
            List<FileInfo> filesInMerge = new List<FileInfo>();
            //FileInfo outputFileInfo = new FileInfo(outputFileName);
            using (TextWriter textWriter = outputFileName == "-" ? Console.Out : File.CreateText(tmpFile)) // Do this early so that if it fails, well know
            {
                string universalHeader = null;
 
                foreach (Pair<string, string[]> filePatternAndColVals in filePatternsAndColumnNames) 
                {
                    if (printProgress) 
                        Console.WriteLine();
                    foreach (FileInfo fileinfo in dirinfo.GetFiles(filePatternAndColVals.First)) //EnumerateFiles(dirinfo, inputFilePatternCollection))
                    {
                        if (printProgress)
                            Console.Write("\rMerging {0}", fileinfo.Name);
                        filesInMerge.Add(fileinfo); 
                        string headerOnFile; 
                        using (TextReader reader = SpecialFunctions.GetTextReaderWithExternalReadWriteAccess(fileinfo.FullName))
                        { 
                            headerOnFile = reader.ReadLine();
                            if (universalHeader == null)
                            {
                                universalHeader = headerOnFile;
                                if (columnNamesToAdd.Length > 0)
                                { 
                                    textWriter.WriteLine(CreateTabString(columnNamesToAdd) + '\t' + universalHeader); 
                                }
                                else 
                                {
                                    textWriter.WriteLine(universalHeader);
                                }
                            }
                            else if (universalHeader != headerOnFile)
                            { 
                                File.Delete(tmpFile); 
                                throw new ArgumentException(string.Format("ERROR: The header for file {0} is different from the 1st file read in. \nCurrent header: {1}\nFirst header: {2}\nAborting file merge.",
                                    fileinfo.Name, headerOnFile, universalHeader), fileinfo.Name); 
                            }
                        }

                        using (TextReader reader = SpecialFunctions.GetTextReaderWithExternalReadWriteAccess(fileinfo.FullName))
                        {
                            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(reader, headerOnFile, /*includeWholeLine*/ true)) 
                            { 
                                if (columnNamesToAdd.Length > 0)
                                { 
                                    textWriter.WriteLine(CreateTabString(filePatternAndColVals.Second) + '\t' + row[""]);  // write the whole line
                                }
                                else
                                {
                                    textWriter.WriteLine(row[""]);  // write the whole line
                                } 
                            } 
                        }
                    } 
                }
                if (printProgress)
                    Console.WriteLine();
            }

            if (outputFileName != "-") 
            { 
                SpecialFunctions.MoveAndReplace(tmpFile, outputFileName);
            } 

            if (deleteFilesOnSuccessfullMerge)
            {
                FileInfo outputFileInfo = new FileInfo(outputFileName);
                // if we get here, we were successful. Delete the files.
                Queue<FileInfo> filesToDelete = new Queue<FileInfo>(); 
                foreach (FileInfo fileinfo in filesInMerge) 
                {
                    if (!fileinfo.FullName.Equals(outputFileInfo.FullName)) 
                    {
                        filesToDelete.Enqueue(fileinfo);
                    }
                }
                DeleteFilesPatiently(filesToDelete, 5, new TimeSpan(0, 1, 0)); // will try deleting for up to ~5 min before bailing.
            } 
        } 

 
        /// <summary>
        /// A wrapper for File.Move that will first delete dest if it exists.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void MoveAndReplace(string src, string dest) 
        { 
            if (File.Exists(dest))
            { 
                File.Delete(dest);
            }
            File.Move(src, dest);
        }

 
 

        public static Dictionary<T1, T2> MergeDictionaries<T1, T2>(params Dictionary<T1, T2>[] dictionaryArray) 
        {
            Dictionary<T1, T2> result = new Dictionary<T1, T2>();
            foreach (Dictionary<T1, T2> input in dictionaryArray)
            {
                foreach (KeyValuePair<T1, T2> keyAndValue in input)
                { 
                    result.Add(keyAndValue.Key, keyAndValue.Value); 
                }
            } 
            return result;

        }


        private static double Mean(params double[] doubleArray) 
        { 
            return Mean(doubleArray);
        } 

        public static void CreateDirectoryForFileIfNeeded(string fileName)
        {
            string outputDirectoryName = Path.GetDirectoryName(fileName);
            if ("" != outputDirectoryName)
            { 
                Directory.CreateDirectory(outputDirectoryName); 
            }
        } 


        public static void Copy2DSquareArray(double[][] src, ref double[][] dest)
        {
            if (dest == null)
            { 
                CreateArrayIfNull(ref dest, src.Length, src.Length); 
            }
            for (int i = 0; i < src.Length; i++) 
            {
                Array.Copy(src[i], dest[i], src[i].Length);
            }
        }

        [Obsolete("This has be superseded  by 'x=>x'")] 
        public static T Identity<T>(T t) 
        {
            return t; 
        }

        /// <summary>
        /// See http://en.wikipedia.org/wiki/Pearson_r
        /// AKA CORREL, PEARSON, corr, related to RSQ in Excel
        /// </summary> 
        /// <returns></returns> 
        public static double PearsonR(IEnumerable<double> vector1, IEnumerable<double> vector2)
        { 
            //One pass forumla from page 298, eq #20 in _Theory and Problems of Statistics 2/ed_, 1992 by Murray R. Spiegel (Schaum's Outline Series)

            double sumXY = 0;
            double sumXX = 0;
            double sumX = 0;
            double sumY = 0; 
            double sumYY = 0; 
            double N = 0;
            foreach (KeyValuePair<double, double> xiAndyi in EnumerateTwo(vector1, vector2)) 
            {
                ++N;
                double xi = xiAndyi.Key;
                double yi = xiAndyi.Value;
                sumXY += xi * yi;
                sumX += xi; 
                sumY += yi; 
                sumXX += xi * xi;
                sumYY += yi * yi; 
            }
            double num = N * sumXY - sumX * sumY;
            double den = Math.Sqrt((N * sumXX - sumX * sumX) * (N * sumYY - sumY * sumY));
            double r = num / den;
            return r;
        } 
 
        //!!!Other nice options: Be case-insensative to header.
        public static IEnumerable<T> ReadDelimitedFile<T>(string fileName, T sample, char[] separatorList, bool hasHeader) 
        {
            using (TextReader textReader = File.OpenText(fileName))
            {
                foreach (T t in ReadDelimitedFile(textReader, sample, separatorList, hasHeader))
                {
                    yield return t; 
                } 
            }
        } 

        public static IEnumerable<T> ReadDelimitedFile<T>(TextReader textReader, T sample, char[] separatorList, bool hasHeader)
        {
            int columnCount = sample.GetType().GetProperties().Count();

 
            if (hasHeader) 
            {
                string header = textReader.ReadLine(); 
                SpecialFunctions.CheckCondition(header != null, "File is empty so can't read header.");
                string[] columns = header.Split(separatorList);
                SpecialFunctions.CheckCondition(columns.Length == columnCount,
                    string.Format("Expected {0} {3}columns, but found {1}. '{2}'", columnCount, columns.Length, header,
                    (separatorList.Length == 1 && separatorList[0] == '\t') ? "tab-delimited " : ""
                    )); 
 

                for (int iColumn = 0; iColumn < columnCount; ++iColumn) 
                {
                    string inputColumn = columns[iColumn].ToLowerInvariant();
                    string sampleColumn = sample.GetType().GetProperties()[iColumn].Name.ToLowerInvariant();
                    SpecialFunctions.CheckCondition(sampleColumn == inputColumn, string.Format("Expected header not found. Expected column #{0} to be '{1}', but instead found '{2}'", iColumn + 1, sampleColumn, inputColumn));
                }
            } 
 

            //Check that the columns match the fields in the sample 
            object[] parameters = new object[columnCount];
            Type[] types = new Type[columnCount];

            string line;
            while (null != (line = textReader.ReadLine()))
            { 
                string[] fields = line.Split(separatorList); 
                SpecialFunctions.CheckCondition(fields.Length == columnCount, string.Format("Expected {0} {3}fields, but found {1}. '{2}'", columnCount, fields.Length, line,
                    (separatorList.Length == 1 && separatorList[0] == '\t') ? "tab-delimited " : "")); 
                for (int fieldIndex = 0; fieldIndex < fields.Length; ++fieldIndex)
                {
                    //string column = columns[fieldIndex].ToLowerInvariant();
                    string field = fields[fieldIndex];
                    PropertyInfo propertyInfo = sample.GetType().GetProperties()[fieldIndex]; //!!! could move propertyInfo look up out of the loop
                    types[fieldIndex] = propertyInfo.PropertyType; 
                    parameters[fieldIndex] = Parser.Parse(field, propertyInfo.PropertyType); 
                }
 
                T t = (T)sample.GetType().GetConstructor(types).Invoke(parameters);

                yield return t;
            }
        }
 
        //!!!Is there a better way to do this? 
        [Obsolete("This has be superseded  by 'Parser.Parse<T>(string field)', though this method still needs work")]
        public static object Parse(string field, Type type) 
        {
            if (type.Equals(typeof(string)))
            {
                return field;
            }
            if (type.Equals(typeof(int))) 
            { 
                return int.Parse(field);
            } 
            if (type.Equals(typeof(double)))
            {
                return double.Parse(field);
            }
            if (type.Equals(typeof(char)))
            { 
                return char.Parse(field); 
            }
            Debug.Assert(false, "Don't know how to parse type " + type.Name); 
            return null;
        }



 
        public static IEnumerable<string> GetFiles(string inputPattern, bool zeroIsOK) 
        {
            bool isZero = true; 
            foreach (string inputSubPattern in inputPattern.Split('+'))
            {
                string directoryName = Path.GetDirectoryName(inputSubPattern);
                if (directoryName == "")
                {
                    directoryName = "."; 
                } 

                foreach (string fileName in Directory.GetFiles(directoryName, Path.GetFileName(inputSubPattern))) 
                {
                    yield return fileName;
                    isZero = false;
                }
            }
            CheckCondition(!isZero || zeroIsOK, "No files of the given pattern found. " + inputPattern); 
        } 

 

        public static List<string> ParenProtectedSplit(string s, char delimiter)
        {
            List<string> result = new List<string>();
            Stack<int> openParens = new Stack<int>();
 
            int start = 0; 
            for (int stop = 0; stop < s.Length; stop++)
            { 
                if (s[stop] == '(')
                {
                    openParens.Push(stop);
                }
                else if (s[stop] == ')')
                { 
                    SpecialFunctions.CheckCondition(openParens.Count > 0, "Unmatched close parenthesis at position " + stop); 
                    openParens.Pop();
                } 
                else if (s[stop] == delimiter && openParens.Count == 0)
                {
                    string itemString = s.Substring(start, stop - start);
                    result.Add(itemString);
                    start = stop + 1;
                } 
            } 
            if (openParens.Count > 0)
            { 
                throw new ArgumentException("Unmatched open parenthesis at position " + openParens.Pop());
            }
            result.Add(s.Substring(start));

            return result;
        } 
 
        public static void Transform<T>(List<T> values, Converter<T, T> transformer)
        { 
            for (int i = 0; i < values.Count; i++)
            {
                values[i] = transformer(values[i]);
            }
        }
 
 
        public static IEnumerable<string[]> EachSparseLine(string filePattern, bool zeroIsOK, string fileMessageOrNull)
        { 
            foreach (string fileName in GetFiles(filePattern, zeroIsOK))
            {
                if (null != fileMessageOrNull)
                {
                    Console.WriteLine(fileMessageOrNull, fileName);
                } 
                using (TextReader textReader = File.OpenText(fileName)) 
                {
                    string header = textReader.ReadLine(); 
                    CheckCondition(header != null, "Expect header");
                    CheckCondition(header.Equals("var\tcid\tval", StringComparison.InvariantCultureIgnoreCase), "Expected header of 'var<tab>cid<tab>val'. Not " + header);
                    string line;
                    while (null != (line = textReader.ReadLine()))
                    {
                        string[] fields = line.Split('\t'); 
                        CheckCondition(fields.Length == 3, "Expect 3 fields on each line. Not " + line); 
                        yield return fields;
                    } 
                }
            }
        }

        /// <summary>
        /// Assumes, but doesn't check that variables are together and that they don't span files 
        /// </summary> 
        public static IEnumerable<List<string[]>> SparseGroupedByVar(string filePattern, bool zeroIsOK, string fileMessageOrNull)
        { 

            List<string[]> group = null;
            string currentVar = null;
            foreach (string[] varCidVal in EachSparseLine(filePattern, zeroIsOK, fileMessageOrNull))
            {
                string var = varCidVal[0]; 
                if (currentVar != var) 
                {
                    if (currentVar != null) 
                    {
                        yield return group;
                    }
                    currentVar = var;
                    group = new List<string[]>();
                } 
                group.Add(varCidVal); 
            }
            if (currentVar != null) 
            {
                yield return group;
            }
        }

 
        public static Dictionary<string, bool> ReadSparseTargetFile(string targetVarOrNull, string targetSparseFileName) 
        {
            Dictionary<string, bool> cidToIsCase = new Dictionary<string, bool>(); 
            SingletonSet<string> varSet = SingletonSet<string>.GetInstance("Target file should contain only one distinct variable. " + targetSparseFileName);
            foreach (var row in SpecialFunctions.ReadDelimitedFile(targetSparseFileName, new { var = "", cid = "", val = 0 }, new char[] { '\t' }, true))
            {
                if (targetVarOrNull != null && 0 != string.Compare(targetVarOrNull, row.var, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue; // not break; 
                } 
                cidToIsCase.Add(row.cid, ZeroOneToBool(row.val));
                varSet.Add(row.var); 
            }
            SpecialFunctions.CheckCondition(!varSet.IsEmpty, "target file should not be empty. " + targetSparseFileName);
            return cidToIsCase;
        }

        public static bool ZeroOneToBool(int val) 
        { 
            switch (val)
            { 
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    SpecialFunctions.CheckCondition(false, "Unknown target value. " + val.ToString()); 
                    return false; 
            }
 
        }


        public static Dictionary<T, int> IndexList<T>(IList<T> list)
        {
            Dictionary<T, int> indexCollection = new Dictionary<T, int>(list.Count); 
            for (int i = 0; i < list.Count; ++i) 
            {
                indexCollection.Add(list[i], i); 
            }
            return indexCollection;
        }

        public static bool EqualIfNewElseSet<T>(ref Nullable<T> oldValueOrNull, T newValue) where T : struct
        { 
            if (oldValueOrNull.HasValue) 
            {
                return newValue.Equals(oldValueOrNull.Value); 
            }
            else
            {
                oldValueOrNull = newValue;
                return true;
            } 
        } 

        public static IEnumerable<IEnumerable<T>> GroupByCount<T>(IList<T> list, int subLength, bool errorIfUneven) 
        {
            for (int i = 0; i < list.Count; i += subLength)
            {
                yield return NextN(list, i, subLength, errorIfUneven);
            }
        } 
 
        private static IEnumerable<T> NextN<T>(IList<T> list, int start, int subLength, bool errorIfUneven)
        { 
            for (int i = start; i < start + subLength; ++i)
            {
                if (i >= list.Count)
                {
                    CheckCondition(!errorIfUneven, string.Format("List length of the list {0} is not divisible by the subLength {1}", list.Count, subLength));
                    yield break; 
                } 
                yield return list[i];
            } 
        }

        public static IEnumerable<KeyValuePair<T, int>> Tally<T>(IEnumerable<T> list)
        {
            Dictionary<T, int> itemToCount = new Dictionary<T, int>();
            foreach (T item in list) 
            { 
                itemToCount[item] = 1 + itemToCount.GetValueOrDefault(item);
            } 

            var itemToSortedCount = itemToCount.OrderBy(itemAndCount => itemToCount.Values);
            return itemToCount;
        }

        /// <summary> 
        /// Efficently returns a random sample (without replacement) of iEnumerable of size sampleCount. 
        /// If sampleCount == int.MaxValue then returns all
        /// If sampleCount > iEnumerable's count, then returns all 
        /// </summary>
        public static IEnumerable<T> SelectRandom<T>(IEnumerable<T> iEnumerable, int sampleCount, ref Random random)
        {
            if (int.MaxValue == sampleCount)
            {
                return iEnumerable; 
            } 
            else
            { 
                List<T> buffer = new List<T>();
                int itemCount = 0;
                foreach (T item in iEnumerable)
                {
                    ++itemCount;
                    if (buffer.Count < sampleCount) 
                    { 
                        buffer.Add(item);
                    } 
                    else
                    {
                        int randomPlace = random.Next(itemCount);
                        if (randomPlace < buffer.Count)
                        {
                            buffer[randomPlace] = item; 
                        } 
                    }
                } 
                return buffer;
            }
        }

    }
 
    public delegate bool Predicate<T1, T2>(T1 t1, T2 t2); 

    public class CharEqualityIgnoreCase : IEqualityComparer<char> 
    {
        static int dist = Math.Abs((int)'A' - (int)'a');

        public CharEqualityIgnoreCase() { }

        public bool Equals(char x, char y) 
        { 
            return x == y || (char.IsLetter(x) && char.IsLetter(y) && Math.Abs((int)x - (int)y) == dist);
        } 

        public int GetHashCode(char obj)
        {
            return char.IsLetter(obj) ? char.ToLower(obj).GetHashCode() : obj.GetHashCode();
        }
    } 
 

} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
