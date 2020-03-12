using System;
using System.Collections.Generic;
using System.Text;

namespace FalseDiscoveryRateClasses
{
    class HypergeometricProbability
    {
        /**
         * Computes the hypergeometric probablity using the following factorization:
         * (a+b)!(a+c)!(b+d)!(c+d)!     (a+b)!    (a+c)!    (c+d)!     (b+d)!
         * ------------------------ =  ------- * ------- * -------- * --------
         *       a!b!c!d!n!              a!b!       c!        d!         n!
         *  The assumption is that (a+b) is the smallest marginal. 
         *  A better implementation would check for the smallest marginal and factor according to it, but the current implementation seems fast enough.
         * */
        public static double pr(ContingencyTable ct)
        {
            double pt = 1;
            double iFactorial = 0;
            int a = ct.getA(), b = ct.getB(), c = ct.getC(), d = ct.getD();
            double iDenominator = a + b + c + d;
            double iMinDenominator = b + d;
            for (iFactorial = a + 1; iFactorial <= a + b; iFactorial++) // (a+b)!/a!b!
            {
                pt *= iFactorial / ( iFactorial - a );
                while ((pt > 1) && (iDenominator > iMinDenominator))
                {
                    pt /= iDenominator;
                    iDenominator--;
                }
            }
            for (iFactorial = c + 1; iFactorial <= a + c; iFactorial++) // (a+c)!/c! 
            {
                pt *= iFactorial;
                while ((pt > 1) && (iDenominator > iMinDenominator))
                {
                    pt /= iDenominator;
                    iDenominator--;
                }
            }
            for (iFactorial = d + 1; iFactorial <= c + d; iFactorial++) // (c+d)!/d! 
            {
                pt *= iFactorial;
                while( (pt > 1) && (iDenominator > iMinDenominator) )
                {
                    pt /= iDenominator;
                    iDenominator--;
                }
            }

            if (pt == 0.0) //underflow
                return double.Epsilon;

            while ((iDenominator > iMinDenominator) && (pt > 0.0))
            {
                pt /= iDenominator;
                if (pt == 0.0) //underflow
                    return double.Epsilon;
                iDenominator--;
            }
            if (pt > 1.0) //numerical error
                pt = 1.0;

            return pt;
        }

    }
}
