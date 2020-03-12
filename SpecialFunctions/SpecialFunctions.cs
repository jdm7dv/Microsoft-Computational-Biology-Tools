using System; 
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection; 
using System.Text.RegularExpressions; 

namespace Msr.Mlas.SpecialFunctions 
{
    /// <summary>
    /// Summary description for SpecialFunctions.
    /// </summary>
    public class SpecialFunctions
    { 
        /* 
         Given a contingency table
                X=0       X=1 
        Y=0      a          b
        Y=1      c          d
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

        public static double InverseCumulativeGaussian(double pvalue, double mean, double stdDev) 
        {
            double result = InverseCumulativeGaussian(pvalue) * stdDev + mean;
            return result;
        }

        // From Max's C++ code 
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
 
        //		static public FisherExactTest2(int Cxy0, int Cx, int Cy, int N) 
        //		{
        //			//# This version works up from observed joint count, using log 
        //			//# probabilities. Probabilities are summed from largest to smallest,
        //			//# until the sum reaches a fixed point.
        //			//
        //			//# Initial probability is computed using factorials approximated by a
        //			//# numerical approximation to log of the gamma function.
        // 
        //			// constants used in gamma function numerical approximation 
        //
        //			double c1 = 0.918938533204673; 
        //			double c2 = 0.000595238095238;
        //			double c3 = 0.000793650793651;
        //			double c4 = 0.002777777777778;
        //			double c5 = 0.083333333333333;
        //
        //			int max_Cxy = Math.Max(Cx,Cy); 
        //			double Exy = ((double) Cx* (double) Cy/(double) N); 
        //			Debug.WriteLine(string.Format("expected joint count: {0}", Exy));
        //			Exy = Math.Floor(Exy); 
        //
        //			if (Cxy0 > max_Cxy)
        //			{
        //				throw(new Exception("prob = 0.0"));
        //			}
        // 
        //			if (Cxy0 <= (Cx + Cy - N)) 
        //			{
        //				throw(new Exception("prob = 1.0")); 
        //			}
        //
        //			if (Cxy0 <= Exy)
        //			{
        //				throw(new Exception("prob > .5"));
        //			} 
        // 
        //			int Cnx = N - Cx;
        //			int Cny = N - Cy; 
        //			int Cxy = Cxy0;
        //			int Cxny = Cx - Cxy;
        //			int Cnxy = Cy - Cxy;
        //			int Cnxny = Cnx - Cnxy;
        //			ArrayList multipliers = new ArrayList(new int[]{Cnx,Cny,Cx,Cy});
        //			//	log_fact_term = &log_factorial(num); 
        //			//	print "log factorial term added = log_fact_term\n"; 
        //
        //			multipliers.Sort(); 
        //
        //			ArrayList divisors = new ArrayList
        //			foreach num (N,Cnxny,Cxny,Cnxy,Cxy) {
        //			push(@divisors,&log_factorial(num));
        //		//	log_fact_term = &log_factorial(num);
        //		//	print "log factorial term subtracted = log_fact_term\n"; 
        //			} 
        //		//    print "\n";
        //			@divisors = (sort {b <=> a} @divisors); 
        //			current_log_prob = 0;
        //			while (@multipliers || @divisors) {
        //			if ((current_log_prob < 0) && @multipliers) {
        //				current_log_prob += shift(@multipliers);
        //			}
        //			else { 
        //				current_log_prob -= shift(@divisors); 
        //			}
        //		//	print "base log prob = current_log_prob\n"; 
        //			}
        //			cum_log_prob = current_log_prob;
        //			prev_cum_log_prob = 0;
        //			while ((Cxy < max_Cxy) && (cum_log_prob != prev_cum_log_prob)) {
        //			Cxy++;
        //			Cnxny++; 
        //			current_log_prob += log((Cnxy * Cxny)/(Cxy * Cnxny)); 
        //			prev_cum_log_prob = cum_log_prob;
        //			// The following is safe, because cum_log_prob must be greater than current_log_prob. 
        //			cum_log_prob = log(exp(current_log_prob-cum_log_prob) + 1) + cum_log_prob;
        //			Cxny--;
        //			Cnxy--;
        //			}
        //			neg_log_prob = -cum_log_prob;
        // 
        //		print "\nneg log prob = neg_log_prob\n"; 
        //
        //		time = (times)[0]; 
        //
        //		print "\ntime seconds\n";
        //
        //sub log_factorial {
        //    my (x) = @_;
        //    my (f,z); 
        //    x++; 
        //    if( x < 7.0 ) {
        //	f = 1.0; 
        //	for( z = x; z < 7.0; z += 1.0 ) {
        //	    x = z;
        //	    f *= z;
        //	}
        //	x += 1.0;
        //	f = -log(f); 
        //    } 
        //    else {
        //	f = 0.0; 
        //    }
        //    z = 1.0 / (x * x);
        //    return(f + log(x) * (x - 0.5) - x + c1 +
        //	   (((-c2 * z + c3) * z - c4) * z + c5) / x);
        //}
        // 
        //// NOTE: This subroutine is not called, but it is here because it shows 
        //// how to handle the general case.
        // 
        //// log_add(log_x,log_y) = log(exp(log_x) + exp(log_y)).
        //
        //// That is, log_add computes the logarithm of the sum of the numbers
        //// which its arguments of the logarithms of.  Its intended use is
        //// summing probability estimates when operating in the log domain.
        // 
        //// Note that it depends on the specific representation that Perl uses 
        //// for representing an arithmetic overflow: "1.//INF".  If this ever
        //// changes in Perl, a corresponding change would need to be made in the 
        //// code.
        //
        //sub log_add {
        //    my (log_x,log_y) = @_;
        //    my (x_div_y);
        //    if (!defined(log_x)) { 
        //	if (!defined(log_y)) { 
        //	    return(undef);
        //	} 
        //	else {
        //	    return(log_y);
        //	}
        //    }
        //    elsif (!defined(log_y)) {
        //	return(log_x); 
        //    } 
        //    x_div_y = exp(log_x-log_y);
        //    if (x_div_y eq "1.//INF") { 
        //	return(log_x);
        //    }
        //    elsif (x_div_y == 0) {
        //	return(log_y);
        //    }
        //    else { 
        //	return(log(x_div_y + 1) + log_y); 
        //    }
        //} 
        //
        //		}

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
 
        public static string CreateDelimitedString(string delimiter, params object[] objectCollection) 
        {
            return CreateDelimitedString2(delimiter, objectCollection); 
        }

        private static string CreateDelimitedString2(string delimiter, IEnumerable objectCollection)
        {
            StringBuilder aStringBuilder = new StringBuilder();
            bool isFirst = true; 
            foreach (object obj in objectCollection) 
            {
                if (!isFirst) 
                {
                    aStringBuilder.Append(delimiter);
                }
                else
                {
                    isFirst = false; 
                } 

                if (obj == null) 
                {
                    aStringBuilder.Append("null");
                }
                else
                {
                    aStringBuilder.Append(obj.ToString()); 
                } 
            }
            return aStringBuilder.ToString(); 
        }

        //!!!not a special math function
        public static string CreateTabString(params object[] objectCollection)
        {
            return CreateDelimitedString2("\t", objectCollection);//CreateTabString2(objectCollection); 
        } 

 
        //!!!not a special math function
        public static string CreateTabString2(IEnumerable objectCollection)
        {
            return CreateDelimitedString2("\t", objectCollection);

            //StringBuilder aStringBuilder = new StringBuilder(); 
            //bool isFirst = true; 
            //foreach (object obj in objectCollection)
            //{ 
            //    if (!isFirst)
            //    {
            //        aStringBuilder.Append('\t');
            //    }
            //    else
            //    { 
            //        isFirst = false; 
            //    }
 
            //    if (obj == null)
            //    {
            //        aStringBuilder.Append("null");
            //    }
            //    else
            //    { 
            //        aStringBuilder.Append(obj.ToString()); 
            //    }
            //} 
            //return aStringBuilder.ToString();
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
        }

        static private SpecialFunctions Singleton = null;

        public static SpecialFunctions GetInstance() 
        { 
            if (null == Singleton)
            { 
                Singleton = new SpecialFunctions();
                Singleton.LoadLogLikelihoodRatioTestTable();

            }
            return Singleton;
 
        } 

        double[] LogLikelihoodRatioTestKeyTable; 
        double[] LogLikelihoodRatioTestValueTable;

        private void LoadLogLikelihoodRatioTestTable()
        {
            string header = @"DOF	RowCount	RowFraction	Column3	logLikelihoodRatio	Pvalue	ln(Pvalue)";
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
        public double LogLikelihoodRatioTest(double logLikelihoodRatio, int degreesOfFreedom)
        { 
            if (double.IsNaN(logLikelihoodRatio))
            {
                return double.NaN;
            }
            CheckCondition(degreesOfFreedom == 1, "Only defined for degreesOfFreedom=1, currently");
            CheckCondition(logLikelihoodRatio >= 0, "log likelihood ratio must be at least zero"); 
 
            int indexOrNot = Array.BinarySearch<double>(LogLikelihoodRatioTestKeyTable, logLikelihoodRatio);
            if (indexOrNot >= 0) 
            {
                return Math.Exp(LogLikelihoodRatioTestValueTable[indexOrNot]);
            }

            int indexHigh = ~indexOrNot;
            int indexLow = indexHigh - 1; 
            CheckCondition(0 <= indexLow && indexHigh < LogLikelihoodRatioTestValueTable.Length); 

            double keyLow = LogLikelihoodRatioTestKeyTable[indexLow]; 
            double keyHigh = LogLikelihoodRatioTestKeyTable[indexHigh];

            double fraction = (logLikelihoodRatio - keyLow) / (keyHigh - keyLow);
            Debug.Assert(0 <= fraction && fraction <= 1); // real assert

            double valueLow = LogLikelihoodRatioTestValueTable[indexLow]; 
            double valueHigh = LogLikelihoodRatioTestValueTable[indexHigh]; 

 
            double logPValue = (valueHigh - valueLow) * fraction + valueLow;
            return Math.Exp(logPValue);


        }
 
        static public double LogSumParams(params double[] logProbabilityCollection) 
        {
            //Debug.Assert(Math.Exp(double.NegativeInfinity) == 0); //real assert 

            double logMax = double.NegativeInfinity;
            int celem = logProbabilityCollection.Length;

            for (int ielem = 0; ielem < celem; ielem++)
                if (logProbabilityCollection[ielem] > logMax) 
                    logMax = logProbabilityCollection[ielem]; 

            Debug.Assert(logMax > double.NegativeInfinity); //!!!raise exception 

            double logK = Math.Log(double.MaxValue) - Math.Log((double)celem + 1.0) - logMax;

            double sum = 0;
            for (int ielem = 0; ielem < celem; ielem++)
            { 
                sum += Math.Exp(logProbabilityCollection[ielem] + logK); 
            }
 
            double dbl = -logK + Math.Log(sum);

            Debug.Assert(double.NegativeInfinity < dbl && dbl < double.PositiveInfinity);

            return -logK + Math.Log(sum);
        } 
 
        static double LogSumConst = Math.Log(double.MaxValue) - Math.Log(3.0);
 
        static public double LogSum(double logP1, double logP2)
        {
            if (double.IsNegativeInfinity(logP1) && double.IsNegativeInfinity(logP2))
            {
                return double.NegativeInfinity;
            } 
            double logK = LogSumConst - Math.Max(logP1, logP2); 
            double sum = Math.Exp(logP1 + logK) + Math.Exp(logP2 + logK);
            double r = Math.Log(sum) - logK; 
            Debug.Assert(double.NegativeInfinity < r && r < double.PositiveInfinity);
            return r;
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
        internal static double Median(ref List<double> list) 
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

        internal static double Median(ref List<int> list)
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

        internal static double Median(IEnumerable<int> listIn) 
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
 
        internal static double Median(IEnumerable<double> listIn) 
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


 
        ///// <summary> 
        /////  This is based on formula (1) of page 42 of _Optimal Statistical Decisions_ by DeGroot, 1970
        /////   The LogGamma form is derived in FDistInTermsOfLogGamma.nb, a Mathematica notebook. 
        ///// </summary>
        //internal static double FDist(double x, double degreeOfFreedom1, double degreeOfFreedom2)
        //{
        //    double logDegreeOfFreedom2PlusDegreeOfFreedom1X = Math.Log(degreeOfFreedom2 + degreeOfFreedom1 * x);
        //    double logX = Math.Log(x);
        //    double fdist = 
        //    Math.Exp( 
        //        .5 *
        //        ( 
        //            degreeOfFreedom1 * Math.Log(degreeOfFreedom1)
        //        + degreeOfFreedom2 * Math.Log(degreeOfFreedom2)
        //        - 2 * logX
        //        + degreeOfFreedom1 * logX
        //        - degreeOfFreedom1 * logDegreeOfFreedom2PlusDegreeOfFreedom1X
        //        - degreeOfFreedom2 * logDegreeOfFreedom2PlusDegreeOfFreedom1X 
        //        - 2 * LogGamma(degreeOfFreedom1 / 2.0) 
        //        - 2 * LogGamma(degreeOfFreedom2 / 2.0)
        //        + 2 * LogGamma((degreeOfFreedom1 + degreeOfFreedom2) / 2) 
        //    ));
        //    return fdist;
        //}

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
            string header2 = SpecialFunctions.Join(separator.ToString(), headerCollection2);
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
                Console.WriteLine("SpecialFunction.TabDirectoryTable: opening file " + new FileInfo(filename).Name); 
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
            return TabFileTable(textReader, "STREAM", header, includeWholeLine, '\t', checkHeaderMatch);
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

            string line = textReader.ReadLine();

            if (checkHeaderMatch)
            {
                SpecialFunctions.CheckCondition(line == header, string.Format("The input doesn't have the exact expected header. {0},{1},{2}", header, line, inputName)); //!!!raise error 
            } 
            else
            { 
                header = line;
            }

            string[] headerCollection = header.Split(separator);
            while (null != (line = textReader.ReadLine()))
            { 
                if (line.Length == 0) continue; 

                string[] fieldCollection = line.Split(separator); 
                SpecialFunctions.CheckCondition(!checkHeaderMatch || fieldCollection.Length == headerCollection.Length,
                    string.Format("The input doesn't have the expected number of columns. Header Length:{0}, LineLength:{1}\nLine:{2}\nInputName:{3}",
                    headerCollection.Length, fieldCollection.Length, line, inputName)); //!!!raise error

                // if we're not checking for a header match, we still can't deal with lines of the wrong length. just ignore them.
                if (fieldCollection.Length != headerCollection.Length) 
                    continue; 

                Dictionary<string, string> rowX = new Dictionary<string, string>(); 
                if (includeWholeLine)
                {
                    rowX.Add("", line);
                }
                for (int iField = 0; iField < fieldCollection.Length; ++iField)
                { 
                    if (headerCollection[iField] == "") 
                    {
                        SpecialFunctions.CheckCondition(fieldCollection[iField] == ""); 
                    }
                    else
                    {
                        if (!rowX.ContainsKey(headerCollection[iField]))
                        {
                            rowX.Add(headerCollection[iField], fieldCollection[iField]); 
                        } 
                        else
                        { 
                            if (rowX[headerCollection[iField]] != fieldCollection[iField])
                            {
                                try
                                {
                                    double r1 = double.Parse(rowX[headerCollection[iField]]);
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
                Dictionary<string, string> row = new Dictionary<string, string>(rowX, StringComparer.CurrentCultureIgnoreCase); 
                yield return row;
            } 
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
        public static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        { 
            if (!dictionary.ContainsKey(key))
            {
                TValue value = new TValue();    // create a default value and add it to the dictionary
                dictionary.Add(key, value);
                return value;
            } 
            else 
            {
                return dictionary[key]; 
            }
        }

        /// <summary>
        /// Returns the value associated with key in the dictionary. If not present, adds the default value to the dictionary and returns that
        /// value. 
        /// </summary> 
        public static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        { 
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, defaultValue);
                return defaultValue;
            }
            else 
            { 
                return dictionary[key];
            } 
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
                return File.OpenText(filename); 
            }
            else 
            {
                return OpenResource(assembly, resourcePrefix, filename);
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
 
        public static IEnumerable<List<T>> EveryCombination<T>(IEnumerable<List<T>> listList)
        { 
            List<T> choicesFor1stPosition;
            if (TryFirst<List<T>>(listList, out choicesFor1stPosition))
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


        //!!!Why have both Join and CreateDelimitedString And CreateTabString?
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

        //public static IEnumerable<Dictionary<string, string>> TabFileTable(string fileInput, string header, bool includeWholeLine)
        //{
        //    return SpecialFunctions.TabFileTable(Assembly.GetExecutingAssembly(), "VirusCount.DataFiles.", fileInput, header, includeWholeLine); //!!!const 
        //} 
        static public List<Dictionary<string, string>> TabFileTableAsList(string fileInput, string header, bool includeWholeLine)
        { 
            return SpecialFunctions.TabFileTableAsList(Assembly.GetExecutingAssembly(), "VirusCount.DataFiles.", fileInput, header, includeWholeLine); //!!!const
        }



 
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

        public static IEnumerable<Dictionary<string, string>> TabFileTableNoHeaderInFile(Assembly assembly, string resourcePrefix, string filename, string header, bool includeWholeLine, char separator)
        {
            using (StreamReader streamReader = OpenResourceOrFile(assembly, resourcePrefix, filename))
            { 
                string line = null; 
                string[] headerCollection = header.Split(separator);
                while (null != (line = streamReader.ReadLine())) 
                {
                    string[] fieldCollection = line.Split(separator);
                    SpecialFunctions.CheckCondition(fieldCollection.Length == headerCollection.Length); //!!!raise error

                    Dictionary<string, string> rowX = new Dictionary<string, string>();
                    if (includeWholeLine) 
                    { 
                        rowX.Add("", line);
                    } 
                    for (int iField = 0; iField < fieldCollection.Length; ++iField)
                    {
                        if (headerCollection[iField] == "")
                        {
                            SpecialFunctions.CheckCondition(fieldCollection[iField] == "");
                        } 
                        else 
                        {
                            if (!rowX.ContainsKey(headerCollection[iField])) 
                            {
                                rowX.Add(headerCollection[iField], fieldCollection[iField]);
                            }
                            else
                            {
                                if (rowX[headerCollection[iField]] != fieldCollection[iField]) 
                                { 
                                    try
                                    { 
                                        double r1 = double.Parse(rowX[headerCollection[iField]]);
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
                    Dictionary<string, string> row = new Dictionary<string, string>(rowX, StringComparer.CurrentCultureIgnoreCase);
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


            realRowCollectionToSort.Sort(delegate(TRow row1, TRow row2) 
                { 
                    double pValue1 = AccessValueFromRow(row1);
                    double pValue2 = AccessValueFromRow(row2); 
                    return pValue1.CompareTo(pValue2);
                });
            nullValueCollectionToBeSorted.Sort();
            List<double> realPValueCollectionSorted = realRowCollectionToSort.ConvertAll<double>(AccessValueFromRow);

            //// TEMPORARY!!! 
            //for (int i = 0; i < 50; i++) 
            //{
            //    Console.WriteLine("Real: {0} Null: {1}", realPValueCollectionSorted[i], nullValueCollectionToBeSorted[i]); 
            //}

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
 
        //public static bool ListEqual<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        //{
        //    foreach(KeyValuePair<T,T> itemAndItem in DoubleEnum(list1, list2))
        //    {
        //        if (!itemAndItem.Key.Equals(itemAndItem.Value))
        //        { 
        //            return false; 
        //        }
        //    } 
        //    return true;
        //}

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

                Set<T1> set = GetValueOrDefault(result, t2);
                set.AddNew(t1); 
            } 
            return result;
        } 


        public static int WrapAroundLeftShift(int someInt, int count)
        {
            int result = (someInt << count) | (someInt >> (8 * sizeof(int) - count));
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

        public static T First<T>(IEnumerable<T> enumeration) 
        { 
            T t;
            bool isOK = TryFirst(enumeration, out t); 
            CheckCondition(isOK, "Can't get 1st item from an empty enumeration");
            return t;
        }

        public static IEnumerable<T> First<T>(IEnumerable<T> enumeration, int keepCount)
        { 
            foreach (T t in enumeration) 
            {
                if (keepCount > 0) 
                {
                    --keepCount;
                    yield return t;
                }
                else
                { 
                    yield break; 
                }
            } 
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

        //This works, but it is not used anywhere, so delete it after a while
        //// randomizes the given mapping. returns a new mapping without changing that formal.
        //public static IEnumerable<KeyValuePair<Tkey, Tvalue>> RandomizeMapping<Tkey, Tvalue>(IEnumerable<KeyValuePair<Tkey, Tvalue>> mapping, Random random)
        //{
        //    IEnumerable<KeyValuePair<Tkey, Tvalue>> shuffledMapping = Shuffle<KeyValuePair<Tkey, Tvalue>>(mapping, ref random); 
        //    //Dictionary<Tkey, Tvalue> result = new Dictionary<Tkey, Tvalue>(); 
        //    foreach (KeyValuePair<KeyValuePair<Tkey, Tvalue>, KeyValuePair<Tkey, Tvalue>> keyAndValueAndShuffledKeyAndValue in EnumerateTwo(mapping, shuffledMapping))
        //    { 
        //        yield return new KeyValuePair<Tkey, Tvalue>(keyAndValueAndShuffledKeyAndValue.Key.Key, keyAndValueAndShuffledKeyAndValue.Value.Value);
        //    }
        //}


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

        /// <summary>
        /// For now, just cast to double. Will fail if c has a complex component. 
        /// </summary> 
        public static ComplexNumber CubeRoot(ComplexNumber c)
        { 
            return CubeRoot((double)c);
        }

        public static ComplexNumber Sqrt(double d)
        {
            if (d >= 0) 
                return Math.Sqrt(d); 
            else
                return new ComplexNumber(0, Math.Sqrt(-d)); 
        }

        //!!!how about having this be a static on ComplexNumber
        /// <summary>
        /// For now, just cast to double. Will fail if c has a complex component.
        /// </summary> 
        public static ComplexNumber Sqrt(ComplexNumber c) 
        {
            return SpecialFunctions.Sqrt((double)c); 
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
                valueToCount[t] = 1 + SpecialFunctions.GetValueOrDefault(valueToCount, t);
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
 
 
        public static IEnumerable<IList<T>> SubListEnumeration<T>(IList<T> list, int length)
        { 
            for (int startIndex = 0; startIndex <= list.Count - length; ++startIndex)
            {
                yield return SubList(list, startIndex, length);
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
 
 
        public static TArray[] SubArray<TArray>(TArray[] args, int startingIndex)
        { 
            return SubArray(args, startingIndex, args.Length - startingIndex);
        }

        public static TArray[] SubArray<TArray>(TArray[] args, int startingIndex, int length)
        {
            TArray[] result = new TArray[length]; 
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
                File.SetLastAccessTime(targetFileName, fileInfo.LastWriteTime);
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

        public static double GaussianProbability(double x, double mean0, double variance) 
        {
            double k = 1.0 / Math.Sqrt(variance * 2 * Math.PI);
            double p = k * Math.Exp(-Math.Pow(x - mean0, 2) / (2 * variance));
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
        //                Converter<TItem, TSortKey> accessor,
        //                Comparison<TSortKey> isBetter)
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

 
        internal static bool TryParse<T>(string s, out T t)
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

        public static Dictionary<TKey, TValue> PairEnumerationToDictionary<TKey, TValue>(IEnumerable<Pair<TKey, TValue>> enumeration)
        { 
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>(); 
            foreach (Pair<TKey, TValue> pair in enumeration)
            { 
                result.Add(pair.First, pair.Second);
            }
            return result;
        }

        public static IEnumerable<Pair<TKey, TValue>> DictionaryToPairEnumeration<TKey, TValue>(Dictionary<TKey, TValue> dict) 
        { 
            foreach (KeyValuePair<TKey, TValue> keyAndValue in dict)
            { 
                yield return new Pair<TKey, TValue>(keyAndValue.Key, keyAndValue.Value);
            }
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

        public static Pair<ComplexNumber, ComplexNumber> QuadraticRoots(int a, double b, double c)
        {
            ComplexNumber rootTerm = SpecialFunctions.Sqrt(b * b - 4 * a * c); 
 
            ComplexNumber root1 = (-b + rootTerm) / (2 * a);
            ComplexNumber root2 = (-b - rootTerm) / (2 * a); 

            return new Pair<ComplexNumber, ComplexNumber>(root1, root2);
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
                Dictionary<TKey, TValue> littleDictonary = GetValueOrDefault(outputDictionary, group); 
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
        public static double RocAreaUnderCurve(IList<bool> rankedListOfTrueClassifications)
        {
            int countTrue = 0;
            int countFalse = 0;
            int sumOfPositiveRanks = 0; 
            for (int i = 0; i < rankedListOfTrueClassifications.Count; i++) 
            {
                if (rankedListOfTrueClassifications[i]) 
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

            //double areaUnderCurve = 1 - (double)sumOfPositiveRanks / (countTrue * countFalse);
            double areaUnderCurve = 1 - F / (countTrue * countFalse);
            return areaUnderCurve;
        } 
 

        /// <summary> 
        /// Runs a permutation test and the predictions of two classifiers.
        /// </summary>
        /// <param name="classifier1">True classifications of events, ordered by classifier1.</param>
        /// <param name="classifier2">True classifications of events, ordered by classifier2.</param>
        /// <param name="scoringFunction">Function that computes a score for each vector.</param>
        /// <param name="numPermutations">The distribution of classification differences under the null hypothesis will be dumped here.</param> 
        /// <param name="simulatedScores"></param> 
        /// <returns>Probability that the discriminatory power of the two classifiers is different</returns>
        public static double PermutationTestOfBooleanVectors(IList<bool> classifier1, IList<bool> classifier2, Converter<IList<bool>, double> scoringFunction, 
            int numPermutations, out double[] simulatedScores)
        {
            CheckCondition(classifier1.Count == classifier2.Count);

            int n = classifier1.Count;
            double observedScore = Math.Abs(scoringFunction(classifier1) - scoringFunction(classifier2)); 
            //double observedDifference = Math.Abs(RocAreaUnderCurve(classifier1) - RocAreaUnderCurve(classifier2)); 
            //double observedDifference = RocAreaUnderCurve(classifier1) - RocAreaUnderCurve(classifier2);
 
            simulatedScores = new double[numPermutations];
            bool[] dummyClassifier1 = new bool[n];
            bool[] dummyClassifier2 = new bool[n];
            Random rand = new Random();

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
                double fakeDiff = Math.Abs(scoringFunction(dummyClassifier1) - scoringFunction(dummyClassifier2));
                //double fakeDiff = Math.Abs(RocAreaUnderCurve(dummyClassifier1) - RocAreaUnderCurve(dummyClassifier2));
                //double fakeDiff = RocAreaUnderCurve(dummyClassifier1) - RocAreaUnderCurve(dummyClassifier2);
                simulatedScores[iteration] = fakeDiff; 
            } 

            // sort in descending order 
            Array.Sort(simulatedScores, delegate(double x, double y)
            {
                return
                    x > y ? -1 :
                    x == y ? 0 : 1;
            }); 
 
            int rankOfObsDiff = 0;
            while (rankOfObsDiff < numPermutations && observedScore <= simulatedScores[rankOfObsDiff]) 
            {
                rankOfObsDiff++;
            }

            double p = (double)rankOfObsDiff / numPermutations;
            return p; 
        } 

        public static IEnumerable<TItem> GroupByEnumeration<TItem, TGroup>(IEnumerable<TItem> inputList, Converter<TItem, TGroup> accessor, Random random) 
        {
            Dictionary<TGroup, Set<TItem>> dictionary = new Dictionary<TGroup, Set<TItem>>();
            foreach (TItem item in inputList)
            {
                TGroup group = accessor(item);
                Set<TItem> set = GetValueOrDefault(dictionary, group); 
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
                Set<TItem> set = GetValueOrDefault(dictionary1, group1); 
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
            int numPermutations, ref Random rand, out double[] simulatedScores)
        { 
            CheckCondition(classifier1.Count == classifier2.Count);

            int n = classifier1.Count;
            double observedScore = Math.Abs(scoringFunction(classifier1) - scoringFunction(classifier2));
            //double observedDifference = Math.Abs(RocAreaUnderCurve(classifier1) - RocAreaUnderCurve(classifier2));
            //double observedDifference = RocAreaUnderCurve(classifier1) - RocAreaUnderCurve(classifier2); 
 
            simulatedScores = new double[numPermutations];
            bool[] dummyClassifier1 = new bool[n]; 
            bool[] dummyClassifier2 = new bool[n];

            for (int iteration = 0; iteration < numPermutations; iteration++)
            {
                if (iteration % 100 == 0)
                { 
                    Console.WriteLine(iteration); 
                }
 
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
                double fakeDiff = Math.Abs(scoringFunction(dummyClassifier1) - scoringFunction(dummyClassifier2));
                //double fakeDiff = Math.Abs(RocAreaUnderCurve(dummyClassifier1) - RocAreaUnderCurve(dummyClassifier2)); 
                //double fakeDiff = RocAreaUnderCurve(dummyClassifier1) - RocAreaUnderCurve(dummyClassifier2); 
                simulatedScores[iteration] = fakeDiff;
            } 

            // sort in descending order
            Array.Sort(simulatedScores, DoubleLessThan);

            int rankOfObsDiff = 0;
            while (rankOfObsDiff < numPermutations && observedScore <= simulatedScores[rankOfObsDiff]) 
            { 
                rankOfObsDiff++;
            } 

            double p = (double)rankOfObsDiff / numPermutations;
            return p;
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


    }
 
    public delegate bool Predicate<T1, T2>(T1 t1, T2 t2); 

} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
