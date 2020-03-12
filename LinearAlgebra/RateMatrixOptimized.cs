using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;

namespace Msr.Mlas.LinearAlgebra
{ 
    public class RateMatrixOptimized 
    {
        private const double maxEigenVectorValue = 100; 
        private Dictionary<string, EigenPair> _abcdToEigenPair = new Dictionary<string, EigenPair>(1000);
        private int _hits = 0;
        private int _total = 0;
        private static int _fastMethod = 0;
        private static int _slowMethod = 0;
 
        const double eps = 1e-10; 
        public EigenPair ComputeEigenPairCached(double a, double b, double c, double d, double e, double f, double g, double h)
        { 
            string key = a + "," + b + "," + c + "," + d + "," + e + "," + f + "," + g + "," + h;
            EigenPair pair;
            _total++;

            if (_abcdToEigenPair.ContainsKey(key))
            { 
                pair = _abcdToEigenPair[key]; 

                //if (DateTime.Now.Date == new DateTime(2006, 06, 30).Date) 
                //{
                //    EigenPair testPair = ComputeEigenPair(a, b, c, d, e, f, g, h);
                //    for (int i = 0; i < testPair.EigenValues.Length; i++)
                //    {
                //        SpecialFunctions.SpecialFunctions.CheckCondition(testPair.EigenValues[i].Approximates(pair.EigenValues[i], eps),
                //            String.Format("Different eigen values in cache!!!!!!!! {0} vs {1}", 
                //            testPair.EigenValues[i], pair.EigenValues[i])); 
                //        for (int j = 0; j < testPair.EigenVectors[i].Length; j++)
                //        { 
                //            SpecialFunctions.SpecialFunctions.CheckCondition(testPair.EigenVectors[i][j].Approximates(pair.EigenVectors[i][j], eps),
                //                String.Format("Different eigen vectors in cache!!!!!!!! {0} vs {1}",
                //                testPair.EigenVectors[i][j], pair.EigenVectors[i][j]));
                //        }
                //    }
                //} 
 
                _hits++;
            } 
            else
            {
                pair = ComputeEigenPair(a, b, c, d, e, f, g, h);

                if (_abcdToEigenPair.Count >= 1000)
                { 
                    //Console.WriteLine("Clearing OPTIMIZATION cache. {0}/{1} ({2}%) were hits", _hits, _total, (double)_hits / _total); 
                    _abcdToEigenPair.Clear();
                } 

                _abcdToEigenPair.Add(key, pair);
            }

            return pair;
        } 
 
        /// <summary>
        /// Forces a recompute of the Eigen Pair for the given parameters using the slow (and more robust) method. Replaces the pair in the 
        /// cache corresponding to these values.
        /// </summary>
        public EigenPair RecomputeEigenPairCachedFromSlow(double a, double b, double c, double d, double e, double f, double g, double h)
        {
            string key = a + "," + b + "," + c + "," + d + "," + e + "," + f + "," + g + "," + h;
            EigenPair pair = ComputeEigenPairSlow(a, b, c, d, e, f, g, h); 
            _abcdToEigenPair[key] = pair; 
            return pair;
        } 

        public static EigenPair ComputeEigenPair(double a, double b, double c, double d, double e, double f, double g, double h)
        {


            ComplexNumber[] eigenValues = EigenValues(a, b, c, d, e, f, g, h); 
            if (eigenValues != null) 
            {
                ComplexNumber[][] eigenVectors = EigenVectors(eigenValues, a, b, c, d, e, f, g, h); 
                if (eigenVectors != null)
                {
                    ComplexNumber[][] invEigenVectors = InvEigenVectorMatrix(eigenVectors);
                    if (invEigenVectors != null)
                    {
                        _fastMethod++; 
                        return new EigenPair(eigenValues, eigenVectors, invEigenVectors); 
                    }
                } 
            }

            // if we get here, either we failed the test, or the specific values of a,b,c,d were such that our optimized code
            // was unstable and failed. This appear to be the case, for example, if a pair of values differ by 10 orders of magnitude or more.
            // Since I don't fully understand when it fails (except in the symmetric a=b=c=d case, I'm just punting to using try/catch.
            //Console.WriteLine("UNABLE TO COMPUTE EIGEN PAIR. REVERTING TO SLOW METHOD"); 
 
            _slowMethod++;
            return ComputeEigenPairSlow(a, b, c, d, e, f, g, h); 

            //double[][] Q = new double[4][];
            //Q[0] = new double[] { -a - b, a, b, 0 };
            //Q[1] = new double[] { c, -c - d, 0, d };
            //Q[2] = new double[] { e, 0, -e - f, f };
            //Q[3] = new double[] { 0, g, h, -g - h }; 
 
            //return LinearAlgebra.ComputeEigenPair(Q);
        } 

        public static EigenPair ComputeEigenPairSlow(double a, double b, double c, double d, double e, double f, double g, double h)
        {
            double[][] Q = new double[4][];
            Q[0] = new double[] { -a - b, a, b, 0 };
            Q[1] = new double[] { c, -c - d, 0, d }; 
            Q[2] = new double[] { e, 0, -e - f, f }; 
            Q[3] = new double[] { 0, g, h, -g - h };
 
            return LinearAlgebra.ComputeFullEigenPair(Q);
        }


        public static ComplexNumber[] EigenValues(double a, double b, double c, double d, double e, double f, double g, double h)
        { 
            ComplexNumber i = new ComplexNumber(0, 1); 
            double a2 = a * a;
            double a3 = a * a2; 
            double a4 = a * a3;
            double b2 = b * b;
            double b3 = b * b2;
            double b4 = b * b3;
            double c2 = c * c;
            double c3 = c * c2; 
            double c4 = c * c3; 
            double d2 = d * d;
            double d3 = d * d2; 
            double d4 = d * d3;
            double e2 = e * e;
            double e3 = e * e2;
            double e4 = e * e3;
            double f2 = f * f;
            double f3 = f * f2; 
            double f4 = f * f3; 
            double g2 = g * g;
            double g3 = g * g2; 
            double g4 = g * g3;
            double h2 = h * h;
            double h3 = h * h2;
            double h4 = h * h3;
            double subExpression1 = 1.0 / 6.0;
            double subExpression2 = 6 * e3 * g3 + 6 * f3 * g3 + (6 * f3 + 6 * e3) * d3 + (6 * g3 + 6 * e3 + 6 * f3) * c3 + (6 * g3 + 6 * d3 + 6 * f3 + 6 * c3) * b3 + (6 * f3 + 6 * g3 + 6 * e3 + 6 * d3) * a3 + (-3 * d4 - 3 * a4 - 3 * b4 - 3 * e4 - 3 * c4) * h2 + (-3 * a4 - 3 * c4 - 3 * e4 - 3 * b4 - 3 * f4) * g2 + (-3 * d4 - 3 * a4 - 3 * g4 - 3 * g2 * h2 - 3 * c4 - 3 * b4) * f2 + (-3 * c4 - 18 * g2 * h2 - 3 * h4 - 3 * d4 - 3 * a4 - 3 * g4 + (-3 * h2 - 18 * g2) * f2) * e2 + (-3 * f4 - 3 * h4 - 3 * g2 * h2 - 3 * e4 - 3 * b4 - 3 * a4 + (-18 * g2 - 18 * h2) * f2 + (-18 * f2 + 18 * h2 - 18 * g2) * e2) * d2 + (-18 * g2 * h2 - 3 * f4 - 3 * b4 - 3 * h4 - 3 * e4 - 3 * g4 + (-18 * h2 + 18 * g2) * f2 + (-18 * f2 + 18 * g2 + 18 * h2) * e2 + (-18 * f2 - 18 * h2 - 18 * e2 - 3 * g2) * d2) * c2 + (-3 * d4 - 3 * h4 - 18 * g2 * h2 - 3 * c4 - 3 * f4 - 3 * g4 + (-18 * h2 + 18 * g2) * f2 + (-18 * h2 - 18 * g2 - 3 * f2) * e2 + (18 * h2 + 18 * f2 - 18 * g2 - 18 * e2) * d2 + (18 * g2 - 18 * e2 + 18 * f2 - 18 * d2 + 18 * h2) * c2) * b2 + (-3 * g4 - 3 * d4 - 3 * e4 - 3 * h4 - 18 * g2 * h2 - 3 * f4 + (-18 * h2 + 18 * g2) * f2 + (-18 * f2 + 18 * g2 + 18 * h2) * e2 + (-18 * g2 + 18 * e2 + 18 * f2 + 18 * h2) * d2 + (-18 * e2 - 18 * g2 - 3 * d2 - 18 * f2 - 18 * h2) * c2 + (-18 * f2 - 18 * g2 - 18 * d2 - 3 * c2 - 3 * e2 - 18 * h2) * b2) * a2 + ((-6 * f3 + 18 * a3 + 18 * c3 + 18 * e3 + 18 * b3) * g2 + (18 * a3 - 6 * g3 + 18 * c3 + 18 * d3 + 18 * b3) * f2 + (-12 * g3 - 6 * c3 - 6 * d3 - 6 * a3) * e2 + (-6 * a3 - 6 * b3 - 6 * e3 - 12 * f3) * d2 + (-6 * b3 - 12 * f3 - 6 * e3 - 12 * g3) * c2 + (-6 * c3 - 12 * f3 - 6 * d3 - 12 * g3) * b2 + (-6 * d3 - 12 * f3 - 12 * g3 - 6 * e3) * a2) * h + ((18 * a3 - 6 * d3 + 18 * e3 + 18 * b3 + 18 * c3) * h2 + (-6 * c3 - 12 * d3 - 6 * b3 - 6 * a3) * f2 + (-6 * a3 - 12 * d3 - 6 * c3) * e2 + (18 * e3 + 18 * f3 + 18 * a3 + 18 * b3) * d2 + (-6 * b3 - 6 * e3 - 6 * f3) * c2 + (-6 * f3 - 12 * d3 - 6 * c3) * b2 + (-6 * f3 - 12 * d3 - 6 * e3) * a2 + (-6 * a4 - 18 * e2 * f2 - 6 * c4 - 6 * e4 - 6 * b4 + (-24 * e2 + 30 * f2) * d2 + (-24 * f2 + 36 * e2 - 18 * d2) * c2 + (-24 * f2 - 36 * e2 + 36 * c2 - 24 * d2) * b2 + (-24 * d2 - 36 * c2 - 24 * f2 - 36 * b2 + 36 * e2) * a2) * h) * g + ((18 * b3 + 18 * d3 - 6 * e3 + 18 * c3 + 18 * a3) * h2 + (-6 * a3 - 12 * e3 - 6 * c3 - 6 * b3) * g2 + (18 * a3 + 18 * d3 + 18 * g3 + 18 * c3) * e2 + (-6 * a3 - 12 * e3 - 6 * b3) * d2 + (-6 * b3 - 12 * e3 - 6 * g3) * c2 + (-6 * d3 - 6 * g3 - 6 * c3) * b2 + (-12 * e3 - 6 * d3 - 6 * g3) * a2 + (30 * g2 * e2 - 6 * a4 - 6 * c4 - 6 * b4 - 6 * d4 + (-18 * g2 - 24 * e2) * d2 + (-24 * e2 - 36 * d2 - 24 * g2) * c2 + (-18 * e2 - 24 * g2 + 36 * c2 + 36 * d2) * b2 + (-24 * e2 - 36 * c2 - 24 * g2 + 36 * d2 - 36 * b2) * a2) * h + (6 * b4 + 6 * a4 + 6 * c4 + 6 * e2 * h2 + (54 * e2 + 6 * h2) * d2 + (-30 * h2 + 18 * d2 - 18 * e2) * c2 + (24 * c2 - 18 * d2 + 18 * e2 - 30 * h2) * b2 + (-30 * h2 + 36 * c2 + 36 * b2 - 18 * d2 - 18 * e2) * a2 + (-18 * e3 + 12 * c3 - 18 * d3 + 12 * b3 + 12 * a3) * h) * g) * f + ((-6 * c3 - 6 * a3 - 12 * b3 - 6 * d3) * h2 + (-12 * b3 - 12 * f3 - 6 * a3 - 6 * c3) * g2 + (18 * g3 - 6 * b3 + 18 * a3 + 18 * d3 + 18 * c3) * f2 + (-12 * b3 - 6 * a3 - 12 * f3) * d2 + (-12 * f3 - 12 * b3 - 6 * g3) * c2 + (18 * g3 + 18 * d3 + 18 * c3 - 6 * f3) * b2 + (-6 * d3 - 12 * f3 - 6 * g3) * a2 + (6 * g2 * f2 + 6 * a4 + 6 * c4 + 6 * d4 + (18 * g2 - 30 * f2) * d2 + (-30 * f2 + 36 * d2 - 18 * g2) * c2 + (-18 * d2 + 6 * f2 - 18 * c2 + 54 * g2) * b2 + (-30 * f2 - 18 * g2 + 24 * d2 + 18 * b2 + 36 * c2) * a2) * h + (6 * c4 + 6 * a4 - 12 * f2 * h2 + (54 * f2 + 12 * h2) * d2 + (-18 * f2 - 18 * h2 + 18 * d2) * c2 + (54 * d2 - 18 * c2 + 12 * f2 + 54 * h2) * b2 + (-18 * d2 - 18 * f2 + 36 * c2 - 18 * h2 + 18 * b2) * a2 + (-24 * b3 - 6 * f3 - 12 * a3 - 12 * c3 + 18 * d3) * h) * g + (-6 * a4 - 6 * d4 - 6 * g4 - 18 * g2 * h2 - 6 * c4 + (-24 * h2 - 36 * g2) * d2 + (-36 * d2 + 36 * g2 - 24 * h2) * c2 + (-24 * c2 + 30 * h2 - 24 * g2 - 24 * d2) * b2 + (-18 * b2 - 24 * h2 + 36 * d2 - 36 * c2 + 36 * g2) * a2 + (12 * a3 + 12 * d3 - 18 * g3 + 12 * c3 - 18 * b3) * h + (-12 * c3 - 24 * d3 - 
 
12 * a3 + 18 * b3 + (12 * c2 + 6 * b2 + 6 * d2 + 12 * a2) * h) * g) * f) * e + ((-12 * c3 - 6 * a3 - 6 * e3 - 6 * b3) * h2 + (18 * a3 + 18 * f3 + 18 * e3 - 6 * c3 + 18 * b3) * g2 + (-6 * b3 - 12 * c3 - 6 * a3 - 12 * g3) * f2 + (-12 * g3 - 12 * c3 - 6 * a3) * e2 + (18 * b3 + 18 * f3 + 18 * e3 - 6 * g3) * c2 + (-6 * f3 - 12 * c3 - 12 * g3) * b2 + (-6 * f3 - 6 * e3 - 12 * g3) * a2 + (6 * g2 * f2 + 6 * b4 + 6 * e4 + 6 * a4 + (18 * f2 - 30 * g2) * e2 + (-18 * e2 + 54 * f2 + 6 * g2) * c2 + (-30 * g2 - 18 * f2 - 18 * c2 + 36 * e2) * b2 + (-18 * f2 - 30 * g2 + 36 * b2 + 24 * e2 + 18 * c2) * a2) * h + (-6 * a4 - 18 * f2 * h2 - 6 * e4 - 6 * b4 - 6 * f4 + (-36 * f2 - 24 * h2) * e2 + (-24 * e2 + 30 * h2 - 24 * f2) * c2 + (-24 * h2 + 36 * f2 - 36 * e2 - 24 * c2) * b2 + (-24 * h2 + 36 * e2 - 36 * b2 + 36 * f2 - 18 * c2) * a2 + (12 * b3 + 12 * a3 + 12 * e3 - 18 * f3 - 18 * c3) * h) * g + (-12 * g2 * h2 + 6 * a4 + 6 * b4 + (12 * h2 + 54 * g2) * e2 + (54 * e2 + 12 * g2 + 54 * h2) * c2 + (18 * e2 - 18 * c2 - 18 * h2 - 18 * g2) * b2 + (18 * c2 - 18 * e2 - 18 * h2 + 36 * b2 - 18 * g2) * a2 + (-12 * a3 - 12 * b3 + 18 * e3 - 6 * g3 - 24 * c3) * h + (18 * c3 - 12 * b3 - 12 * a3 - 24 * e3 + (12 * b2 + 12 * a2 + 6 * c2 + 6 * e2) * h) * g) * f + (6 * h4 + 18 * g2 * h2 + 6 * a4 + (18 * h2 + 54 * g2) * f2 + (54 * f2 - 18 * h2 + 12 * g2) * c2 + (54 * c2 - 18 * h2 + 54 * g2 + 12 * f2) * b2 + (24 * h2 - 18 * g2 - 18 * f2 + 18 * c2 + 18 * b2) * a2 + (-24 * a3 + 6 * f3 + 6 * g3 + 24 * c3 + 24 * b3) * h + (18 * c3 - 12 * a3 - 24 * b3 - 24 * f3 + (-24 * f2 + 36 * b2 + 6 * a2 - 6 * c2) * h) * g + (18 * b3 - 24 * c3 - 24 * g3 - 12 * a3 + (-6 * b2 + 36 * c2 - 24 * g2 + 6 * a2) * h + (18 * h2 - 48 * b2 - 48 * c2 + 72 * a2) * g) * f) * e) * d + ((-12 * a3 - 6 * b3 - 12 * d3 - 6 * e3) * h2 + (-12 * a3 - 6 * b3 - 6 * f3 - 6 * e3) * g2 + (-6 * b3 - 12 * a3 - 12 * d3 - 6 * g3) * f2 + (-6 * g3 - 12 * a3 - 12 * d3) * e2 + (18 * b3 + 18 * e3 - 6 * a3 + 18 * f3) * d2 + (-12 * d3 - 6 * g3 - 6 * f3) * b2 + (18 * g3 - 6 * d3 + 18 * e3 + 18 * f3) * a2 + (12 * g2 * f2 + 6 * e4 + 6 * b4 + (-18 * g2 + 18 * f2) * e2 + (54 * f2 - 18 * e2 - 12 * g2) * d2 + (-18 * d2 + 36 * e2 - 18 * f2 - 18 * g2) * b2 + (54 * f2 + 12 * d2 + 54 * g2 + 18 * b2 - 18 * e2) * a2) * h + (6 * f4 + 6 * b4 + 18 * f2 * h2 + 6 * e4 + (36 * f2 - 18 * h2) * e2 + (6 * h2 - 30 * e2 - 30 * f2) * d2 + (-30 * d2 - 18 * h2 + 36 * e2 + 24 * f2) * b2 + (18 * b2 - 18 * e2 + 54 * h2 - 18 * f2 + 6 * d2) * a2 + (18 * f3 - 12 * e3 - 12 * b3 - 24 * a3 - 6 * d3) * h) * g + (18 * g2 * h2 + 6 * g4 + 6 * b4 + (12 * h2 - 18 * g2) * e2 + (54 * h2 + 18 * g2 + 54 * e2) * d2 + (-18 * d2 + 24 * g2 + 18 * e2 - 18 * h2) * b2 + (54 * h2 + 54 * e2 - 18 * g2 + 18 * b2 + 12 * d2) * a2 + (18 * e3 - 24 * a3 - 12 * b3 + 18 * g3 - 24 * d3) * h + (24 * a3 + 24 * e3 + 6 * d3 - 24 * b3 + (6 * b2 - 24 * d2 + 36 * a2 - 6 * e2) * h) * g) * f + (6 * g4 + 36 * g2 * h2 + 6 * h4 + (18 * h2 - 18 * g2) * f2 + (-18 * h2 + 18 * g2 + 54 * f2) * d2 + (-18 * g2 - 18 * h2 + 54 * d2 + 12 * f2) * b2 + (-18 * h2 + 54 * f2 - 12 * b2 - 18 * g2 + 12 * d2) * a2 + (24 * b3 + 24 * g3 + 6 * f3 + 24 * a3 + 24 * d3) * h + (24 * a3 + 6 * d3 + 24 * f3 + 24 * b3 + (-36 * b2 + 24 * d2 - 36 * a2 + 24 * f2) * h) * g + (-24 * d3 + 18 * b3 - 12 * g3 - 24 * a3 + (36 * d2 - 6 * b2 - 6 * g2 + 36 * a2) * h + (-36 * a2 - 60 * d2 + 24 * h2 - 24 * b2) * g) * f) * e + (-6 * b4 - 18 * g2 * h2 - 6 * e4 - 6 * h4 - 6 * f4 + (-24 * g2 - 36 * h2) * f2 + (-36 * f2 + 36 * h2 - 24 * g2) * e2 + (-36 * e2 + 36 * f2 - 24 * g2 + 36 * h2) * b2 + (-24 * e2 - 18 * b2 + 30 * g2 - 24 * f2 - 24 * h2) * a2 + (-12 * b3 - 12 * e3 - 6 * g3 + 18 * a3 - 24 * f3) * h + (-18 * a3 + 12 * e3 + 12 * f3 + 12 * b3 + (6 * f2 + 6 * a2 + 12 * b2 + 12 * e2) * h) * g + (18 * g3 + 18 * a3 - 24 * e3 - 12 * b3 + (-48 * e2 + 18 * g2 + 72 * b2 - 48 * a2) * h + (-6 * a2 + 36 * e2 - 24 * h2 + 6 * b2) * g) * f + (-24 * b3 + 18 * a3 + 18 * g3 - 24 * f3 + (-60 * f2 + 24 * g2 - 24 * a2 - 36 * b2) * h + (-6 * h2 + 36 * b2 - 6 * a2 + 36 * f2) * g + (-48 * h2 - 48 * g2 - 48 * a2 - 48 * b2 + 18 * h * g) * f) * e) * d) * c + ((-12 * a3
 
- 6 * c3 - 6 * d3 - 12 * e3) * h2 + (-12 * e3 - 12 * a3 - 6 * f3 - 6 * c3) * g2 + (-6 * g3 - 6 * c3 - 12 * a3 - 6 * d3) * f2 + (18 * g3 + 18 * c3 + 18 * d3 - 6 * a3) * e2 + (-6 * f3 - 12 * a3 - 12 * e3) * d2 + (-12 * e3 - 6 * f3 - 6 * g3) * c2 + (18 * f3 + 18 * d3 + 18 * g3 - 6 * e3) * a2 + (6 * d4 + 12 * g2 * f2 + 6 * c4 + (-12 * f2 + 54 * g2) * e2 + (-18 * f2 - 18 * e2 + 18 * g2) * d2 + (-18 * g2 + 36 * d2 - 18 * f2 - 18 * e2) * c2 + (54 * f2 + 12 * e2 + 18 * c2 + 54 * g2 - 18 * d2) * a2) * h + (18 * f2 * h2 + 6 * f4 + 6 * c4 + (54 * h2 + 18 * f2) * e2 + (-18 * f2 + 12 * h2 + 54 * e2) * d2 + (-18 * h2 + 24 * f2 + 18 * d2 - 18 * e2) * c2 + (18 * c2 + 12 * e2 + 54 * d2 - 18 * f2 + 54 * h2) * a2 + (18 * f3 - 24 * e3 - 12 * c3 - 24 * a3 + 18 * d3) * h) * g + (6 * g4 + 6 * d4 + 18 * g2 * h2 + 6 * c4 + (6 * h2 - 30 * g2) * e2 + (36 * g2 - 30 * e2 - 18 * h2) * d2 + (36 * d2 - 18 * h2 - 30 * e2 + 24 * g2) * c2 + (18 * c2 + 54 * h2 - 18 * g2 + 6 * e2 - 18 * d2) * a2 + (-24 * a3 - 6 * e3 + 18 * g3 - 12 * d3 - 12 * c3) * h + (24 * d3 - 24 * c3 + 6 * e3 + 24 * a3 + (6 * c2 - 6 * d2 + 36 * a2 - 24 * e2) * h) * g) * f + (-36 * g2 * h2 - 6 * c4 - 6 * d4 - 6 * h4 - 6 * g4 + (-18 * h2 - 24 * g2) * f2 + (36 * h2 - 36 * g2 - 24 * f2) * d2 + (36 * h2 - 36 * d2 - 24 * f2 + 36 * g2) * c2 + (30 * f2 - 24 * g2 - 18 * c2 - 24 * h2 - 24 * d2) * a2 + (18 * a3 - 24 * g3 - 12 * c3 - 12 * d3 - 6 * f3) * h + (-12 * c3 - 24 * d3 + 18 * a3 + 18 * f3 + (72 * c2 + 18 * f2 - 48 * d2 - 48 * a2) * h) * g + (12 * c3 - 18 * a3 + 12 * g3 + 12 * d3 + (12 * d2 + 6 * a2 + 6 * g2 + 12 * c2) * h + (-24 * h2 + 36 * d2 - 6 * a2 + 6 * c2) * g) * f) * e + (18 * g2 * h2 + 6 * h4 + 6 * f4 + (36 * h2 - 18 * g2) * f2 + (-18 * h2 + 54 * g2 + 18 * f2) * e2 + (-18 * h2 + 54 * e2 - 18 * f2 + 12 * g2) * c2 + (-12 * c2 - 18 * h2 - 18 * f2 + 12 * e2 + 54 * g2) * a2 + (6 * g3 + 24 * a3 + 24 * f3 + 24 * e3 + 24 * c3) * h + (-24 * e3 - 24 * a3 + 18 * c3 - 12 * f3 + (-6 * c2 - 6 * f2 + 36 * a2 + 36 * e2) * h) * g + (24 * c3 + 24 * a3 + 6 * e3 + 24 * g3 + (-36 * a2 + 24 * e2 - 36 * c2 + 24 * g2) * h + (-36 * a2 - 24 * c2 - 60 * e2 + 24 * h2) * g) * f + (18 * a3 - 24 * g3 + 18 * f3 - 24 * c3 + (-24 * a2 - 36 * c2 + 24 * f2 - 60 * g2) * h + (-48 * c2 - 48 * h2 - 48 * a2 - 48 * f2) * g + (36 * c2 + 18 * h * g - 6 * h2 + 36 * g2 - 6 * a2) * f) * e) * d + (6 * f4 + 6 * h4 + 6 * g4 + 36 * g2 * h2 + (24 * g2 + 36 * h2) * f2 + (-18 * g2 + 18 * f2 - 18 * h2) * e2 + (-18 * h2 - 18 * f2 + 54 * e2 + 18 * g2) * d2 + (-30 * g2 - 30 * h2 + 6 * d2 + 6 * e2 - 30 * f2) * a2 + (24 * d3 + 24 * e3 + 24 * f3 + 24 * g3 + 6 * a3) * h + (24 * e3 + 6 * d3 + 6 * a3 - 24 * f3 + (24 * d2 - 24 * f2 - 60 * a2 - 36 * e2) * h) * g + (24 * d3 + 6 * e3 - 24 * g3 + 6 * a3 + (24 * e2 - 36 * d2 - 24 * g2 - 60 * a2) * h + (24 * e2 + 24 * d2 + 24 * a2 + 24 * h2) * g) * f + (-6 * a3 - 12 * g3 - 24 * d3 + 18 * f3 + (24 * f2 + 24 * a2 - 36 * g2 - 36 * d2) * h + (-36 * h2 - 24 * f2 - 60 * d2 + 24 * a2) * g + (36 * d2 + 6 * g2 - 6 * h2 - 24 * a2) * f) * e + (-6 * a3 - 12 * f3 + 18 * g3 - 24 * e3 + (-36 * f2 + 24 * g2 + 24 * a2 - 36 * e2) * h + (36 * e2 - 24 * a2 - 6 * h2 + 6 * f2) * g + (-24 * g2 - 60 * e2 - 36 * h2 + 24 * a2) * f + (24 * h * g - 48 * g2 + 18 * a2 + 72 * h2 - 48 * f2 + (24 * h + 42 * g) * f) * e) * d) * c) * b + ((-12 * b3 - 12 * c3 - 6 * d3 - 6 * e3) * h2 + (-12 * c3 - 12 * b3 - 6 * e3 - 6 * f3) * g2 + (-12 * c3 - 12 * b3 - 6 * g3 - 6 * d3) * f2 + (-6 * g3 - 12 * c3 - 6 * d3) * e2 + (-6 * e3 - 6 * f3 - 12 * b3) * d2 + (18 * g3 + 18 * f3 - 6 * b3 + 18 * e3) * c2 + (18 * g3 + 18 * f3 + 18 * d3 - 6 * c3) * b2 + (6 * d4 + 12 * g2 * f2 + 6 * e4 + (-18 * g2 + 18 * f2) * e2 + (18 * g2 - 18 * f2 + 24 * e2) * d2 + (18 * d2 - 18 * e2 + 54 * f2 + 54 * g2) * c2 + (-18 * d2 + 18 * e2 + 54 * g2 + 12 * c2 + 54 * f2) * b2) * h + (6 * f4 + 18 * f2 * h2 + 6 * e4 + (36 * f2 - 18 * h2) * e2 + (-18 * f2 - 18 * e2 + 12 * h2) * d2 + (-12 * d2 + 54 * h2 - 18 * e2 - 18 * f2) * c2 + (-18 * f2 + 18 * e2 + 54 * d2 + 12 * c2 + 54 * h2) * b2 + (18 * d3 - 24 * c3 - 12 * e3 + 18 * f3 - 24 * b3) * h) * g + (6 * d4 + 6 * g4 + 18 * g2 * h2 + (

12 * h2 - 18 * g2) * e2 + (36 * g2 - 18 * h2 - 18 * e2) * d2 + (54 * e2 + 18 * d2 + 54 * h2 - 18 * g2) * c2 + (-18 * g2 - 18 * d2 + 12 * c2 + 54 * h2 - 12 * e2) * b2 + (18 * g3 - 24 * b3 + 18 * e3 - 24 * c3 - 12 * d3) * h + (24 * c3 + 24 * b3 + 24 * d3 + 24 * e3 + (-6 * d2 + 36 * c2 - 6 * e2 + 36 * b2) * h) * g) * f + (6 * g4 + 36 * g2 * h2 + 6 * d4 + 6 * h4 + (18 * h2 - 18 * g2) * f2 + (36 * g2 - 18 * f2 + 24 * h2) * d2 + (18 * d2 - 18 * g2 + 54 * f2 - 18 * h2) * c2 + (-30 * g2 - 30 * d2 + 6 * f2 - 30 * h2 + 6 * c2) * b2 + (24 * c3 - 24 * d3 + 24 * g3 + 6 * f3 + 6 * b3) * h + (24 * d3 + 24 * c3 + 6 * b3 + 24 * f3 + (24 * f2 - 60 * b2 - 24 * d2 - 36 * c2) * h) * g + (-24 * c3 - 12 * d3 - 12 * g3 - 6 * b3 + (-6 * g2 + 6 * d2 - 24 * b2 + 36 * c2) * h + (-36 * c2 + 24 * b2 - 36 * d2 + 24 * h2) * g) * f) * e + (18 * g2 * h2 + 6 * f4 + 6 * h4 + 6 * e4 + (36 * h2 - 18 * g2) * f2 + (36 * f2 + 24 * h2 - 18 * g2) * e2 + (-30 * f2 - 30 * e2 - 30 * h2 + 6 * g2) * c2 + (-18 * f2 - 18 * h2 + 6 * c2 + 18 * e2 + 54 * g2) * b2 + (24 * b3 - 24 * e3 + 6 * g3 + 6 * c3 + 24 * f3) * h + (-6 * c3 - 12 * f3 - 12 * e3 - 24 * b3 + (-24 * c2 - 6 * f2 + 6 * e2 + 36 * b2) * h) * g + (6 * c3 + 24 * b3 + 24 * g3 + 24 * e3 + (-24 * e2 - 60 * c2 + 24 * g2 - 36 * b2) * h + (-36 * e2 - 36 * b2 + 24 * c2 + 24 * h2) * g) * f + (24 * f3 + 6 * b3 + 6 * c3 + 24 * g3 + (24 * g2 + 24 * f2 + 24 * b2 + 24 * c2) * h + (24 * c2 - 60 * b2 - 24 * h2 - 36 * f2) * g + (-24 * h2 - 36 * g2 - 60 * c2 + 24 * b2) * f) * e) * d + (-6 * h4 - 36 * g2 * h2 - 6 * e4 - 6 * g4 - 6 * f4 + (36 * g2 - 36 * h2) * f2 + (-36 * f2 + 36 * h2 + 36 * g2) * e2 + (-24 * e2 - 24 * f2 - 18 * g2 - 24 * h2) * d2 + (-24 * g2 - 24 * f2 - 18 * e2 + 30 * d2 - 24 * h2) * b2 + (-12 * e3 - 24 * f3 + 18 * d3 - 24 * g3 + 18 * b3) * h + (-6 * d3 - 12 * e3 + 18 * b3 - 12 * f3 + (-48 * f2 + 18 * d2 + 72 * e2 - 48 * b2) * h) * g + (-24 * e3 + 18 * b3 - 12 * g3 + 18 * d3 + (-48 * b2 - 48 * d2 - 48 * g2 - 48 * e2) * h + (-36 * e2 + 24 * d2 - 24 * b2 - 60 * h2) * g) * f + (-24 * f3 - 6 * b3 + 18 * d3 - 12 * g3 + (-36 * g2 - 24 * d2 - 60 * f2 + 24 * b2) * h + (-36 * f2 + 24 * d2 - 36 * h2 + 24 * b2) * g + (72 * g2 + 18 * b2 - 48 * d2 + 24 * h * g - 48 * h2) * f) * e + (12 * e3 - 18 * b3 - 18 * g3 + 12 * f3 + (6 * e2 - 24 * g2 + 36 * f2 - 6 * b2) * h + (12 * f2 + 6 * b2 + 12 * e2 + 6 * h2) * g + (-6 * g2 + 18 * h * g + 36 * e2 - 6 * b2 + 36 * h2) * f + (-6 * g2 + 6 * h2 - 24 * b2 + 36 * f2 + (24 * g + 42 * h) * f) * e) * d) * c + (-6 * h4 - 36 * g2 * h2 - 6 * f4 - 6 * g4 - 6 * d4 + (36 * g2 - 36 * h2) * f2 + (-24 * h2 - 18 * f2 - 24 * g2) * e2 + (36 * h2 - 24 * e2 - 36 * g2 + 36 * f2) * d2 + (-18 * d2 - 24 * g2 + 30 * e2 - 24 * f2 - 24 * h2) * c2 + (-12 * d3 + 18 * e3 - 24 * g3 + 18 * c3 - 24 * f3) * h + (18 * e3 - 24 * d3 + 18 * c3 - 12 * f3 + (-48 * f2 - 48 * d2 - 48 * e2 - 48 * c2) * h) * g + (18 * c3 - 6 * e3 - 12 * d3 - 12 * g3 + (-48 * c2 + 72 * d2 - 48 * g2 + 18 * e2) * h + (-36 * d2 + 24 * e2 - 24 * c2 - 60 * h2) * g) * f + (-18 * c3 + 12 * d3 - 18 * f3 + 12 * g3 + (-24 * f2 + 36 * g2 - 6 * c2 + 6 * d2) * h + (-6 * c2 - 6 * f2 + 36 * d2 + 36 * h2) * g + (12 * d2 + 12 * g2 + 18 * h * g + 6 * c2 + 6 * h2) * f) * e + (-12 * f3 + 18 * e3 - 24 * g3 - 6 * c3 + (-24 * e2 - 36 * f2 + 24 * c2 - 60 * g2) * h + (72 * f2 - 48 * h2 - 48 * e2 + 18 * c2) * g + (-36 * h2 + 24 * e2 + 24 * c2 + 24 * h * g - 36 * g2) * f + (6 * h2 + 42 * h * g - 24 * c2 - 6 * f2 + 24 * g * f + 36 * g2) * e) * d + (12 * f3 + 12 * g3 - 18 * d3 - 18 * e3 + (36 * f2 + 36 * g2 - 6 * d2 - 6 * e2) * h + (-24 * d2 + 36 * h2 + 6 * f2 - 6 * e2) * g + (-24 * e2 + 42 * h * g - 6 * d2 + 36 * h2 + 6 * g2) * f + (6 * d2 + 12 * g2 + 24 * h * g + 18 * f * h + 12 * h2 + 6 * f2) * e + (18 * h * g + 6 * e2 + 6 * g2 + 24 * f * h + 12 * f2 + 12 * h2 + (18 * f + 18 * g) * e) * d) * c) * b) * a + (6 * a3 + 6 * b3 + 6 * c3 + 6 * d3 + 6 * e3 + (-6 * d2 - 12 * a2 - 12 * c2 - 12 * e2 - 12 * b2) * g + (-12 * a2 - 12 * b2 - 6 * e2 - 12 * d2 - 12 * c2) * f + (-6 * d2 - 6 * g * f + 18 * b2 - 6 * a2 - 6 * c2) * e + (-6 * a2 - 6 * e2 - 6 * b2 - 6 * g * f + 18 * c2 + (18 * f

 + 18 * g) * e) * d + (-6 * e2 + 6 * g * f + 18 * a2 + 18 * d2 - 6 * b2 + (18 * f + 24 * g) * e + (-24 * f - 12 * e - 18 * g) * d) * c + (18 * a2 + 18 * e2 - 6 * c2 + 6 * g * f - 6 * d2 + (-18 * f - 24 * g) * e + (-12 * e + 24 * f + 18 * g) * d + (-12 * e + 24 * g + 24 * f - 12 * d) * c) * b + (-6 * e2 - 6 * d2 + 18 * b2 + 6 * g * f + 18 * c2 + (18 * f + 24 * g) * e + (-24 * e + 24 * f + 18 * g) * d + (-12 * e - 24 * f + 12 * d - 24 * g) * c + (-12 * d + 12 * c + 12 * e - 24 * g - 24 * f) * b) * a) * h3;
            double subExpression3 = 1.0 / 2.0; 
            double subExpression3_4 = Math.Sqrt(3); //ComplexNumber.Pow(3.0, subExpression4) 
            ComplexNumber subExpressionRoot2 = SpecialFunctions.SpecialFunctions.Sqrt(subExpression2);
            //ComplexNumber subExpressionRoot2 = ComplexNumber.Pow(subExpression2, subExpression3); 
            ComplexNumber subExpression5 = -8 * d3 - 8 * a3 - 8 * b3 - 8 * c3 - 8 * h3 - 8 * e3 - 8 * f3 - 8 * g3 + 12 * subExpressionRoot2 + (12 * c2 + 12 * b2 - 24 * f2 + 12 * e2 + 12 * d2 - 24 * g2 + 12 * a2) * h + (12 * c2 + 12 * e2 + 12 * f2 + 12 * a2 - 24 * h2 + 12 * b2 - 24 * d2) * g + (-24 * e2 + 12 * d2 + 12 * c2 - 12 * h * g + 12 * b2 + 12 * g2 - 24 * h2 + 12 * a2) * f + (12 * c2 + 12 * d2 - 24 * f2 + 12 * g2 + 12 * a2 + 24 * h * g + 12 * h2 - 24 * b2 + (24 * g - 12 * h) * f) * e + (12 * a2 - 24 * g2 + 12 * f2 - 24 * c2 + 12 * h2 - 12 * h * g + 12 * b2 + 12 * e2 + (24 * g + 24 * h) * f + (24 * f + 24 * g - 48 * h) * e) * d + (12 * f2 + 12 * g2 - 24 * d2 + 12 * b2 - 24 * a2 + 12 * e2 + 24 * h * g + 12 * h2 + (24 * h - 48 * g) * f + (-48 * h - 48 * g + 24 * f) * e + (24 * f - 12 * g + 24 * h + 24 * e) * d) * c + (12 * g2 + 24 * h * g + 12 * f2 + 12 * d2 + 12 * h2 + 12 * c2 - 24 * a2 - 24 * e2 + (24 * h - 48 * g) * f + (-12 * f + 24 * h + 24 * g) * e + (24 * e - 48 * h + 24 * g - 48 * f) * d + (24 * d - 48 * g - 48 * f + 24 * e - 48 * h) * c) * b + (12 * f2 + 12 * g2 - 24 * c2 + 12 * d2 - 24 * b2 + 24 * h * g + 12 * h2 + 12 * e2 + (24 * h - 48 * g) * f + (-48 * h - 48 * g + 24 * f) * e + (-48 * h - 48 * f + 24 * g - 48 * e) * d + (-12 * d + 24 * e + 24 * g + 24 * f + 24 * h) * c + (-12 * e + 24 * d + 24 * f + 24 * h + 24 * g - 12 * c) * b) * a;
            double subExpression6 = 2.0 / 3.0;
            ComplexNumber subExpression5_6 = ComplexNumber.Pow(subExpression5, subExpression6);
            double subExpression8 = 1.0 / 3.0;
            ComplexNumber subExpression5_8 = ComplexNumber.Pow(subExpression5, subExpression8);
 
            double subSubExpression1 = -4.0*c-4.0*d; 
            double subSubExpression2 = -4.0*d+8.0*c+8.0*b;
            double subSubExpression3 = -4.0*a-4.0*d-4.0*c+8.0*b; 
            double subSubExpression4 = -4.0*c-4.0*d+8.0*e-4.0*a-4.0*b;
            double subSubExpression5 = -4.0*e+8.0*f-4.0*c-4.0*b-4.0*a-4.0*d;
            double subSubExpression6 = -4.0*e-4.0*c+8.0*h+8.0*d-4.0*b-4.0*a-4.0*f;
            double subSubExpression7 = -2.0*d-2.0*a-2.0*f-2.0*g-2.0*b-2.0*h-2.0*c-2.0*e;
            ComplexNumber subExpression10 = 4*f2+4.0*d2+4.0*c2+8.0*c*d+4.0*b2+subSubExpression1*b+subSubExpression2*a+subSubExpression3*e+4.0*e2+subSubExpression4*f+4.0*a2+4.0*g2+subSubExpression5*h+4.0*h2+subSubExpression6*g+subExpression5_6+subSubExpression7*subExpression5_8;
 
            double term1 = -e * b - a * b - f * h - g * h - g * d - c * d - a * c - e * f; 
            double term2 = -a2 - b2 - c2 - d2 - e2 - f2 - g2 - h2 + f * g + c * f + a * d + b * d + a * e + d * f + d * e + d * h + b * h + a * g + b * f + b * c + c * e + c * g + a * h + a * f + e * g + c * h + e * h + b * g;
            double subExpression11 = 1.0 / 12.0;
            double subExpression12 = 6 * f3 * g3 + 6 * e3 * g3 + (6 * f3 + 6 * e3) * d3 + (6 * e3 + 6 * g3 + 6 * f3) * c3 + (6 * f3 + 6 * c3 + 6 * g3 + 6 * d3) * b3 + (6 * d3 + 6 * e3 + 6 * f3 + 6 * g3) * a3 + (-3 * a4 - 3 * b4 - 3 * c4 - 3 * d4 - 3 * e4) * h2 + (-3 * a4 - 3 * e4 - 3 * c4 - 3 * f4 - 3 * b4) * g2 + (-3 * b4 - 3 * c4 - 3 * d4 - 3 * g2 * h2 - 3 * g4 - 3 * a4) * f2 + (-3 * a4 - 3 * d4 - 3 * c4 - 18 * g2 * h2 - 3 * g4 - 3 * h4 + (-3 * h2 - 18 * g2) * f2) * e2 + (-3 * a4 - 3 * b4 - 3 * g2 * h2 - 3 * e4 - 3 * h4 - 3 * f4 + (-18 * g2 - 18 * h2) * f2 + (18 * h2 - 18 * f2 - 18 * g2) * e2) * d2 + (-3 * f4 - 3 * e4 - 18 * g2 * h2 - 3 * h4 - 3 * g4 - 3 * b4 + (18 * g2 - 18 * h2) * f2 + (-18 * f2 + 18 * h2 + 18 * g2) * e2 + (-18 * e2 - 18 * f2 - 3 * g2 - 18 * h2) * d2) * c2 + (-3 * f4 - 3 * h4 - 3 * d4 - 3 * c4 - 3 * g4 - 18 * g2 * h2 + (18 * g2 - 18 * h2) * f2 + (-18 * h2 - 3 * f2 - 18 * g2) * e2 + (-18 * e2 + 18 * h2 + 18 * f2 - 18 * g2) * d2 + (18 * f2 + 18 * g2 + 18 * h2 - 18 * d2 - 18 * e2) * c2) * b2 + (-3 * e4 - 3 * g4 - 3 * d4 - 18 * g2 * h2 - 3 * f4 - 3 * h4 + (18 * g2 - 18 * h2) * f2 + (-18 * f2 + 18 * h2 + 18 * g2) * e2 + (-18 * g2 + 18 * e2 + 18 * f2 + 18 * h2) * d2 + (-18 * e2 - 18 * f2 - 18 * g2 - 3 * d2 - 18 * h2) * c2 + (-18 * h2 - 18 * d2 - 3 * e2 - 3 * c2 - 18 * g2 - 18 * f2) * b2) * a2 + ((-6 * f3 + 18 * c3 + 18 * e3 + 18 * a3 + 18 * b3) * g2 + (18 * b3 + 18 * c3 + 18 * a3 - 6 * g3 + 18 * d3) * f2 + (-6 * d3 - 6 * a3 - 6 * c3 - 12 * g3) * e2 + (-6 * e3 - 12 * f3 - 6 * b3 - 6 * a3) * d2 + (-12 * f3 - 12 * g3 - 6 * e3 - 6 * b3) * c2 + (-12 * f3 - 6 * d3 - 6 * c3 - 12 * g3) * b2 + (-6 * e3 - 6 * d3 - 12 * g3 - 12 * f3) * a2) * h + ((18 * a3 + 18 * b3 - 6 * d3 + 18 * c3 + 18 * e3) * h2 + (-6 * a3 - 6 * b3 - 6 * c3 - 12 * d3) * f2 + (-6 * c3 - 6 * a3 - 12 * d3) * e2 + (18 * a3 + 18 * e3 + 18 * f3 + 18 * b3) * d2 + (-6 * e3 - 6 * b3 - 6 * f3) * c2 + (-12 * d3 - 6 * f3 - 6 * c3) * b2 + (-6 * f3 - 6 * e3 - 12 * d3) * a2 + (-6 * b4 - 18 * e2 * f2 - 6 * e4 - 6 * c4 - 6 * a4 + (30 * f2 - 24 * e2) * d2 + (36 * e2 - 18 * d2 - 24 * f2) * c2 + (36 * c2 - 24 * f2 - 36 * e2 - 24 * d2) * b2 + (-36 * c2 - 36 * b2 + 36 * e2 - 24 * d2 - 24 * f2) * a2) * h) * g + ((18 * c3 + 18 * b3 - 6 * e3 + 18 * a3 + 18 * d3) * h2 + (-12 * e3 - 6 * a3 - 6 * c3 - 6 * b3) * g2 + (18 * c3 + 18 * a3 + 18 * d3 + 18 * g3) * e2 + (-6 * b3 - 6 * a3 - 12 * e3) * d2 + (-6 * g3 - 12 * e3 - 6 * b3) * c2 + (-6 * g3 - 6 * c3 - 6 * d3) * b2 + (-6 * g3 - 12 * e3 - 6 * d3) * a2 + (30 * g2 * e2 - 6 * d4 - 6 * c4 - 6 * a4 - 6 * b4 + (-24 * e2 - 18 * g2) * d2 + (-24 * e2 - 24 * g2 - 36 * d2) * c2 + (-24 * g2 - 18 * e2 + 36 * c2 + 36 * d2) * b2 + (36 * d2 - 24 * e2 - 36 * c2 - 24 * g2 - 36 * b2) * a2) * h + (6 * a4 + 6 * c4 + 6 * e2 * h2 + 6 * b4 + (54 * e2 + 6 * h2) * d2 + (-18 * e2 - 30 * h2 + 18 * d2) * c2 + (18 * e2 - 30 * h2 - 18 * d2 + 24 * c2) * b2 + (-18 * d2 - 30 * h2 - 18 * e2 + 36 * b2 + 36 * c2) * a2 + (12 * c3 - 18 * e3 + 12 * b3 + 12 * a3 - 18 * d3) * h) * g) * f + ((-12 * b3 - 6 * a3 - 6 * d3 - 6 * c3) * h2 + (-6 * a3 - 6 * c3 - 12 * b3 - 12 * f3) * g2 + (18 * c3 + 18 * g3 + 18 * d3 + 18 * a3 - 6 * b3) * f2 + (-12 * b3 - 12 * f3 - 6 * a3) * d2 + (-12 * f3 - 12 * b3 - 6 * g3) * c2 + (18 * g3 + 18 * d3 + 18 * c3 - 6 * f3) * b2 + (-12 * f3 - 6 * g3 - 6 * d3) * a2 + (6 * a4 + 6 * g2 * f2 + 6 * c4 + 6 * d4 + (18 * g2 - 30 * f2) * d2 + (-18 * g2 + 36 * d2 - 30 * f2) * c2 + (-18 * d2 + 54 * g2 - 18 * c2 + 6 * f2) * b2 + (36 * c2 + 18 * b2 + 24 * d2 - 30 * f2 - 18 * g2) * a2) * h + (-12 * f2 * h2 + 6 * a4 + 6 * c4 + (12 * h2 + 54 * f2) * d2 + (18 * d2 - 18 * f2 - 18 * h2) * c2 + (-18 * c2 + 54 * d2 + 12 * f2 + 54 * h2) * b2 + (-18 * f2 - 18 * d2 - 18 * h2 + 36 * c2 + 18 * b2) * a2 + (-6 * f3 - 12 * a3 - 24 * b3 + 18 * d3 - 12 * c3) * h) * g + (-6 * g4 - 18 * g2 * h2 - 6 * d4 - 6 * c4 - 6 * a4 + (-36 * g2 - 24 * h2) * d2 + (36 * g2 - 24 * h2 - 36 * d2) * c2 + (-24 * d2 - 24 * c2 - 24 * g2 + 30 * h2) * b2 + (36 * d2 - 36 * c2 - 18 * b2 + 36 * g2 - 24 * h2) * a2 + (12 * d3 - 18 * g3 + 12 * c3 - 18 * b3 + 12 * a3) * h + (-24 * d3 - 12 * c3 - 12 * a3 + 18 * b3 + (12 * a2 + 12 * c2 + 6 * b2 + 6 * d2) * h) * g) * f) * e + ((-6 * e3 - 6 * b3 - 6 * a3 - 12 * c3) * h2 + (18 * f3 - 6 * c3 + 18 * e3 + 18 * a3 + 18 * b3) * g2 + (-6 * b3 - 12 * c3 - 6 * a3 - 12 * g3) * f2 + (-12 * c3 - 6 * a3 - 12 * g3) * e2 + (18 * f3 + 18 * b3 + 18 * e3 - 6 * g3) * c2 + (-12 * g3 - 6 * f3 - 12 * c3) * b2 + (-6 * e3 - 12 * g3 - 6 * f3) * a2 + (6 * g2 * f2 + 6 * b4 + 6 * e4 + 6 * a4 + (-30 * g2 + 18 * f2) * e2 + (54 * f2 - 18 * e2 + 6 * g2) * c2 + (-30 * g2 - 18 * c2 + 36 * e2 - 18 * f2) * b2 + (-18 * f2 + 36 * b2 + 18 * c2 - 30 * g2 + 24 * e2) * a2) * h + (-6 * b4 - 6 * f4 - 6 * a4 - 6 * e4 - 18 * f2 * h2 + (-24 * h2 - 36 * f2) * e2 + (30 * h2 - 24 * f2 - 24 * e2) * c2 + (36 * f2 - 36 * e2 - 24 * h2 - 24 * c2) * b2 + (-18 * c2 + 36 * e2 - 24 * h2 + 36 * f2 - 36 * b2) * a2 + (-18 * c3 + 12 * b3 + 12 * e3 + 12 * a3 - 18 * f3) * h) * g + (6 * a4 + 6 * b4 - 12 * g2 * h2 + (54 * g2 + 12 * h2) * e2 + (54 * e2 + 54 * h2 + 12 * g2) * c2 + (-18 * g2 - 18 * c2 + 18 * e2 - 18 * h2) * b2 + (-18 * g2 - 18 * h2 - 18 * e2 + 36 * b2 + 18 * c2) * a2 + (-24 * c3 - 12 * b3 + 18 * e3 - 12 * a3 - 6 * g3) * h + (-24 * e3 - 12 * b3 + 18 * c3 - 12 * a3 + (6 * e2 + 12 * a2 + 6 * c2 + 12 * b2) * h) * g) * f + (6 * h4 + 18 * g2 * h2 + 6 * a4 + (18 * h2 + 54 * g2) * f2 + (12 * g2 + 54 * f2 - 18 * h2) * c2 + (-18 * h2 + 54 * c2 + 54 * g2 + 12 * f2) * b2 + (18 * c2 + 24 * h2 + 18 * b2 - 18 * g2 - 18 * f2) * a2 + (6 * f3 + 24 * b3 - 24 * a3 + 6 * g3 + 24 * c3) * h + (-24 * b3 - 24 * f3 + 18 * c3 - 12 * a3 + (6 * a2 - 24 * f2 - 6 * c2 + 36 * b2) * h) * g + (-24 * c3 + 18 * b3 - 24 * g3 - 12 * a3 + (6 * a2 - 6 * b2 + 36 * c2 - 24 * g2) * h + (72 * a2 + 18 * h2 - 48 * c2 - 48 * b2) * g) * f) * e) * d + ((-6 * e3 - 12 * a3 - 6 * b3 - 12 * d3) * h2 + (-6 * b3 - 6 * e3 - 6 * f3 - 12 * a3) * g2 + (-12 * a3 - 6 * g3 - 12 * d3 - 6 * b3) * f2 + (-12 * a3 - 6 * g3 - 12 * d3) * e2 + (18 * f3 - 6 * a3 + 18 * b3 + 18 * e3) * d2 + (-6 * g3 - 6 * f3 - 12 * d3) * b2 + (-6 * d3 + 18 * f3 + 18 * g3 + 18 * e3) * a2 + (6 * b4 + 12 * g2 * f2 + 6 * e4 + (-18 * g2 + 18 * f2) * e2 + (54 * f2 - 12 * g2 - 18 * e2) * d2 + (-18 * g2 - 18 * d2 - 18 * f2 + 36 * e2) * b2 + (12 * d2 + 18 * b2 - 18 * e2 + 54 * f2 + 54 * g2) * a2) * h + (18 * f2 * h2 + 6 * f4 + 6 * b4 + 6 * e4 + (-18 * h2 + 36 * f2) * e2 + (6 * h2 - 30 * e2 - 30 * f2) * d2 + (24 * f2 - 18 * h2 + 36 * e2 - 30 * d2) * b2 + (-18 * e2 + 6 * d2 + 18 * b2 - 18 * f2 + 54 * h2) * a2 + (-6 * d3 + 18 * f3 - 12 * b3 - 24 * a3 - 12 * e3) * h) * g + (18 * g2 * h2 + 6 * b4 + 6 * g4 + (12 * h2 - 18 * g2) * e2 + (54 * h2 + 54 * e2 + 18 * g2) * d2 + (-18 * h2 + 18 * e2 - 18 * d2 + 24 * g2) * b2 + (54 * e2 + 12 * d2 + 18 * b2 - 18 * g2 + 54 * h2) * a2 + (18 * g3 - 24 * d3 + 18 * e3 - 24 * a3 - 12 * b3) * h + (24 * e3 + 6 * d3 - 24 * b3 + 24 * a3 + (36 * a2 - 6 * e2 + 6 * b2 - 24 * d2) * h) * g) * f + (6 * g4 + 6 * h4 + 36 * g2 * h2 + (18 * h2 - 18 * g2) * f2 + (54 * f2 - 18 * h2 + 18 * g2) * d2 + (12 * f2 + 54 * d2 - 18 * g2 - 18 * h2) * b2 + (54 * f2 - 12 * b2 - 18 * h2 - 18 * g2 + 12 * d2) * a2 + (24 * b3 + 24 * d3 + 24 * a3 + 6 * f3 + 24 * g3) * h + (6 * d3 + 24 * b3 + 24 * a3 + 24 * f3 + (-36 * b2 - 36 * a2 + 24 * d2 + 24 * f2) * h) * g + (18 * b3 - 12 * g3 - 24 * a3 - 24 * d3 + (36 * d2 - 6 * g2 + 36 * a2 - 6 * b2) * h + (24 * h2 - 60 * d2 - 36 * a2 - 24 * b2) * g) * f) * e + (-18 * g2 * h2 - 6 * e4 - 6 * f4 - 6 * h4 - 6 * b4 + (-36 * h2 - 24 * g2) * f2 + (36 * h2 - 24 * g2 - 36 * f2) * e2 + (36 * h2 + 36 * f2 - 36 * e2 - 24 * g2) * b2 + (30 * g2 - 18 * b2 - 24 * h2 - 24 * f2 - 24 * e2) * a2 + (-12 * e3 - 24 * f3 - 12 * b3 - 6 * g3 + 18 * a3) * h + (12 * b3 + 12 * f3 - 18 * a3 + 12 * e3 + (12 * b2 + 12 * e2 + 6 * f2 + 6 * a2) * h) * g + (18 * g3 + 18 * a3 - 12 * b3 - 24 * e3 + (-48 * a2 + 72 * b2 + 18 * g2 - 48 * e2) * h + (6 * b2 + 36 * e2 - 6 * a2 - 24 * h2) * g) * f + (-24 * b3 - 24 * f3 + 18 * a3 + 18 * g3 + (-36 * b2 + 24 * g2 - 24 * a2 - 60 * f2) * h + (-6 * a2 - 6 * h2 + 36 * f2 + 36 * b2) * g + (-48 * a2 - 48 * h2 - 48 * g2 - 48 * b2 + 18 * h * g) * f) * e) * d) * c + ((-12 * e3 - 12 * a3 - 6 * c3 - 6 * d3) * h2 + (-6 * c3 - 12 * e3 - 6 * f3 - 12 * a3) * g2 + (-12 * a3 - 6 * c3 - 6 * g3 - 6 * d3) * f2 + (18 * d3 - 6 * a3 + 18 * c3 + 18 * g3) * e2 + (-6 * f3 - 12 * e3 - 12 * a3) * d2 + (-6 * f3 - 6 * g3 - 12 * e3) * c2 + (-6 * e3 + 18 * d3 + 18 * f3 + 18 * g3) * a2 + (6 * c4 + 6 * d4 + 12 * g2 * f2 + (-12 * f2 + 54 * g2) * e2 + (18 * g2 - 18 * f2 - 18 * e2) * d2 + (-18 * e2 + 36 * d2 - 18 * g2 - 18 * f2) * c2 + (54 * g2 + 12 * e2 - 18 * d2 + 54 * f2 + 18 * c2) * a2) * h + (18 * f2 * h2 + 6 * f4 + 6 * c4 + (54 * h2 + 18 * f2) * e2 + (-18 * f2 + 12 * h2 + 54 * e2) * d2 + (24 * f2 + 18 * d2 - 18 * e2 - 18 * h2) * c2 + (18 * c2 + 12 * e2 - 18 * f2 + 54 * d2 + 54 * h2) * a2 + (-24 * e3 + 18 * f3 + 18 * d3 - 24 * a3 - 12 * c3) * h) * g + (18 * g2 * h2 + 6 * d4 + 6 * g4 + 6 * c4 + (6 * h2 - 30 * g2) * e2 + (-30 * e2 - 18 * h2 + 36 * g2) * d2 + (-30 * e2 - 18 * h2 + 36 * d2 + 24 * g2) * c2 + (54 * h2 + 6 * e2 + 18 * c2 - 18 * g2 - 18 * d2) * a2 + (-6 * e3 - 12 * d3 - 24 * a3 - 12 * c3 + 18 * g3) * h + (6 * e3 + 24 * d3 + 24 * a3 - 24 * c3 + (6 * c2 + 36 * a2 - 6 * d2 - 24 * e2) * h) * g) * f + (-6 * g4 - 6 * h4 - 6 * c4 - 6 * d4 - 36 * g2 * h2 + (-24 * g2 - 18 * h2) * f2 + (-24 * f2 - 36 * g2 + 36 * h2) * d2 + (-36 * d2 - 24 * f2 + 36 * h2 + 36 * g2) * c2 + (-24 * g2 - 24 * d2 - 18 * c2 + 30 * f2 - 24 * h2) * a2 + (18 * a3 - 6 * f3 - 12 * c3 - 24 * g3 - 12 * d3) * h + (-24 * d3 - 12 * c3 + 18 * f3 + 18 * a3 + (18 * f2 - 48 * a2 - 48 * d2 + 72 * c2) * h) * g + (12 * c3 + 12 * g3 - 18 * a3 + 12 * d3 + (6 * g2 + 12 * d2 + 12 * c2 + 6 * a2) * h + (-6 * a2 + 36 * d2 + 6 * c2 - 24 * h2) * g) * f) * e + (6 * f4 + 18 * g2 * h2 + 6 * h4 + (36 * h2 - 18 * g2) * f2 + (18 * f2 + 54 * g2 - 18 * h2) * e2 + (54 * e2 - 18 * f2 + 12 * g2 - 18 * h2) * c2 + (12 * e2 - 18 * f2 - 12 * c2 - 18 * h2 + 54 * g2) * a2 + (6 * g3 + 24 * e3 + 24 * a3 + 24 * c3 + 24 * f3) * h + (-24 * a3 - 12 * f3 - 24 * e3 + 18 * c3 + (-6 * c2 + 36 * a2 + 36 * e2 - 6 * f2) * h) * g + (6 * e3 + 24 * c3 + 24 * g3 + 24 * a3 + (24 * g2 - 36 * a2 + 24 * e2 - 36 * c2) * h + (24 * h2 - 60 * e2 - 24 * c2 - 36 * a2) * g) * f + (18 * a3 + 18 * f3 - 24 * g3 - 24 * c3 + (-24 * a2 + 24 * f2 - 36 * c2 - 60 * g2) * h + (-48 * a2 - 48 * h2 - 48 * c2 - 48 * f2) * g + (-6 * a2 - 6 * h2 + 18 * h * g + 36 * g2 + 36 * c2) * f) * e) * d + (6 * g4 + 6 * h4 + 6 * f4 + 36 * g2 * h2 + (24 * g2 + 36 * h2) * f2 + (-18 * g2 - 18 * h2 + 18 * f2) * e2 + (54 * e2 - 18 * h2 - 18 * f2 + 18 * g2) * d2 + (-30 * f2 - 30 * h2 - 30 * g2 + 6 * e2 + 6 * d2) * a2 + (24 * d3 + 6 * a3 + 24 * g3 + 24 * e3 + 24 * f3) * h + (-24 * f3 + 24 * e3 + 6 * d3 + 6 * a3 + (-60 * a2 + 24 * d2 - 36 * e2 - 24 * f2) * h) * g + (-24 * g3 + 6 * a3 + 24 * d3 + 6 * e3 + (-36 * d2 + 24 * e2 - 24 * g2 - 60 * a2) * h + (24 * e2 + 24 * d2 + 24 * a2 + 24 * h2) * g) * f + (-12 * g3 + 18 * f3 - 24 * d3 - 6 * a3 + (-36 * g2 - 36 * d2 + 24 * f2 + 24 * a2) * h + (24 * a2 - 60 * d2 - 24 * f2 - 36 * h2) * g + (-6 * h2 - 24 * a2 + 6 * g2 + 36 * d2) * f) * e + (-24 * e3 - 12 * f3 - 6 * a3 + 18 * g3 + (-36 * f2 + 24 * g2 - 36 * e2 + 24 * a2) * h + (6 * f2 + 36 * e2 - 24 * a2 - 6 * h2) * g + (24 * a2 - 24 * g2 - 36 * h2 - 60 * e2) * f + (-48 * g2 + 24 * h * g + 18 * a2 - 48 * f2 + 72 * h2 + (42 * g + 24 * h) * f) * e) * d) * c) * b + ((-6 * d3 - 6 * e3 - 12 * b3 - 12 * c3) * h2 + (-6 * e3 - 6 * f3 - 12 * b3 - 12 * c3) * g2 + (-12 * b3 - 6 * g3 - 6 * d3 - 12 * c3) * f2 + (-6 * g3 - 6 * d3 - 12 * c3) * e2 + (-12 * b3 - 6 * f3 - 6 * e3) * d2 + (18 * f3 + 18 * g3 + 18 * e3 - 6 * b3) * c2 + (18 * g3 + 18 * d3 + 18 * f3 - 6 * c3) * b2 + (12 * g2 * f2 + 6 * e4 + 6 * d4 + (-18 * g2 + 18 * f2) * e2 + (-18 * f2 + 18 * g2 + 24 * e2) * d2 + (54 * g2 - 18 * e2 + 18 * d2 + 54 * f2) * c2 + (18 * e2 + 54 * g2 + 54 * f2 + 12 * c2 - 18 * d2) * b2) * h + (18 * f2 * h2 + 6 * e4 + 6 * f4 + (-18 * h2 + 36 * f2) * e2 + (-18 * e2 - 18 * f2 + 12 * h2) * d2 + (54 * h2 - 18 * e2 - 18 * f2 - 12 * d2) * c2 + (54 * h2 + 12 * c2 + 18 * e2 + 54 * d2 - 18 * f2) * b2 + (18 * f3 - 12 * e3 + 18 * d3 - 24 * b3 - 24 * c3) * h) * g + (6 * d4 + 6 * g4 + 18 * g2 * h2 + (12 * h2 - 18 * g2) * e2 + (-18 * h2 - 18 * e2 + 36 * g2) * d2 + (54 * h2 + 18 * d2 + 54 * e2 - 18 * g2) * c2 + (54 * h2 - 18 * g2 - 18 * d2 + 12 * c2 - 12 * e2) * b2 + (-12 * d3 - 24 * b3 + 18 * e3 - 24 * c3 + 18 * g3) * h + (24 * b3 + 24 * e3 + 24 * c3 + 24 * d3 + (36 * c2 - 6 * e2 - 6 * d2 + 36 * b2) * h) * g) * f + (6 * h4 + 36 * g2 * h2 + 6 * d4 + 6 * g4 + (18 * h2 - 18 * g2) * f2 + (-18 * f2 + 36 * g2 + 24 * h2) * d2 + (-18 * g2 + 18 * d2 + 54 * f2 - 18 * h2) * c2 + (6 * f2 - 30 * g2 - 30 * d2 + 6 * c2 - 30 * h2) * b2 + (-24 * d3 + 6 * f3 + 24 * g3 + 24 * c3 + 6 * b3) * h + (24 * d3 + 6 * b3 + 24 * f3 + 24 * c3 + (-60 * b2 + 24 * f2 - 24 * d2 - 36 * c2) * h) * g + (-12 * d3 - 12 * g3 - 24 * c3 - 6 * b3 + (36 * c2 - 24 * b2 + 6 * d2 - 6 * g2) * h + (24 * h2 - 36 * c2 + 24 * b2 - 36 * d2) * g) * f) * e + (6 * e4 + 18 * g2 * h2 + 6 * f4 + 6 * h4 + (36 * h2 - 18 * g2) * f2 + (36 * f2 + 24 * h2 - 18 * g2) * e2 + (-30 * h2 - 30 * f2 - 30 * e2 + 6 * g2) * c2 + (-18 * f2 - 18 * h2 + 18 * e2 + 6 * c2 + 54 * g2) * b2 + (24 * b3 - 24 * e3 + 6 * c3 + 6 * g3 + 24 * f3) * h + (-12 * e3 - 12 * f3 - 24 * b3 - 6 * c3 + (36 * b2 + 6 * e2 - 6 * f2 - 24 * c2) * h) * g + (24 * g3 + 24 * b3 + 24 * e3 + 6 * c3 + (24 * g2 - 36 * b2 - 60 * c2 - 24 * e2) * h + (-36 * e2 + 24 * c2 + 24 * h2 - 36 * b2) * g) * f + (24 * g3 + 24 * f3 + 6 * c3 + 6 * b3 + (24 * g2 + 24 * f2 + 24 * b2 + 24 * c2) * h + (-24 * h2 - 36 * f2 + 24 * c2 - 60 * b2) * g + (24 * b2 - 36 * g2 - 24 * h2 - 60 * c2) * f) * e) * d + (-6 * f4 - 6 * g4 - 6 * h4 - 6 * e4 - 36 * g2 * h2 + (36 * g2 - 36 * h2) * f2 + (-36 * f2 + 36 * g2 + 36 * h2) * e2 + (-24 * f2 - 18 * g2 - 24 * h2 - 24 * e2) * d2 + (-24 * h2 - 24 * g2 + 30 * d2 - 24 * f2 - 18 * e2) * b2 + (18 * b3 - 12 * e3 - 24 * g3 - 24 * f3 + 18 * d3) * h + (-12 * e3 - 6 * d3 + 18 * b3 - 12 * f3 + (-48 * b2 - 48 * f2 + 18 * d2 + 72 * e2) * h) * g + (18 * d3 + 18 * b3 - 24 * e3 - 12 * g3 + (-48 * d2 - 48 * b2 - 48 * e2 - 48 * g2) * h + (-60 * h2 - 24 * b2 - 36 * e2 + 24 * d2) * g) * f + (-24 * f3 + 18 * d3 - 6 * b3 - 12 * g3 + (-60 * f2 + 24 * b2 - 24 * d2 - 36 * g2) * h + (24 * d2 - 36 * h2 - 36 * f2 + 24 * b2) * g + (18 * b2 + 24 * h * g - 48 * h2 + 72 * g2 - 48 * d2) * f) * e + (12 * e3 - 18 * b3 + 12 * f3 - 18 * g3 + (-24 * g2 + 6 * e2 + 36 * f2 - 6 * b2) * h + (6 * b2 + 12 * f2 + 12 * e2 + 6 * h2) * g + (-6 * b2 + 36 * h2 + 18 * h * g - 6 * g2 + 36 * e2) * f + (-24 * b2 + 6 * h2 + 36 * f2 - 6 * g2 + (24 * g + 42 * h) * f) * e) * d) * c + (-6 * f4 - 6 * g4 - 6 * h4 - 6 * d4 - 36 * g2 * h2 + (36 * g2 - 36 * h2) * f2 + (-18 * f2 - 24 * h2 - 24 * g2) * e2 + (-36 * g2 - 24 * e2 + 36 * h2 + 36 * f2) * d2 + (-24 * f2 + 30 * e2 - 24 * g2 - 24 * h2 - 18 * d2) * c2 + (18 * e3 - 24 * g3 + 18 * c3 - 24 * f3 - 12 * d3) * h + (18 * e3 - 24 * d3 + 18 * c3 - 12 * f3 + (-48 * f2 - 48 * d2 - 48 * e2 - 48 * c2) * h) * g + (-12 * g3 - 12 * d3 - 6 * e3 + 18 * c3 + (-48 * c2 + 72 * d2 - 48 * g2 + 18 * e2) * h + (-60 * h2 - 36 * d2 - 24 * c2 + 24 * e2) * g) * f + (12 * g3 + 12 * d3 - 18 * c3 - 18 * f3 + (-24 * f2 - 6 * c2 + 36 * g2 + 6 * d2) * h + (36 * d2 - 6 * c2 - 6 * f2 + 36 * h2) * g + (18 * h * g + 6 * c2 + 12 * g2 + 12 * d2 + 6 * h2) * f) * e + (-12 * f3 - 24 * g3 - 6 * c3 + 18 * e3 + (-36 * f2 + 24 * c2 - 24 * e2 - 60 * g2) * h + (72 * f2 - 48 * e2 - 48 * h2 + 18 * c2) * g + (-36 * g2 + 24 * c2 + 24 * h * g + 24 * e2 - 36 * h2) * f + (-6 * f2 + 42 * h * g + 36 * g2 + 24 * g * f + 6 * h2 - 24 * c2) * e) * d + (12 * f3 + 12 * g3 - 18 * e3 - 18 * d3 + (-6 * d2 + 36 * g2 + 36 * f2 - 6 * e2) * h + (6 * f2 + 36 * h2 - 6 * e2 - 24 * d2) * g + (6 * g2 - 24 * e2 + 36 * h2 - 6 * d2 + 42 * h * g) * f + (18 * f * h + 6 * d2 + 12 * h2 + 24 * h * g + 12 * g2 + 6 * f2) * e + (12 * f2 + 6 * g2 + 24 * f * h + 12 * h2 + 6 * e2 + 18 * h * g + (18 * g + 18 * f) * e) * d) * c) * b) * a + (6 * a3 + 6 * b3 + 6 * c3 + 6 * d3 + 6 * e3 + (-12 * b2 - 12 * c2 - 12 * a2 - 6 * d2 - 12 * e2) * g + (-12 * a2 - 12 * b2 - 12 * c2 - 12 * d2 - 6 * e2) * f + (18 * b2 - 6 * g * f - 6 * a2 - 6 * d2 - 6 * c2) * e + (-6 * a2 - 6 * b2 + 18 * c2 - 6 * g * f - 6 * e2 + (18 * g + 18 * f) * e) * d + (6 * g * f - 6 * b2 + 18 * d2 + 18 * a2 - 6 * e2 + (24 * g + 18 * f) * e + (-24 * f - 12 * e - 18 * g) * d) * c + (18 * a2 + 6 * g * f - 6 * c2 - 6 * d2 + 18 * e2 + (-18 * f - 24 * g) * e + (-12 * e + 18 * g + 24 * f) * d + (24 * g + 24 * f - 12 * e - 12 * d) * c) * b + (6 * g * f - 6 * d2 + 18 * b2 + 18 * c2 - 6 * e2 + (24 * g + 18 * f) * e + (-24 * e + 24 * f + 18 * g) * d + (-24 * g - 24 * f - 12 * e + 12 * d) * c + (-12 * d - 24 * f + 12 * c - 24 * g + 12 * e) * b) * a) * h3;
            ComplexNumber subExpression12_3 = ComplexNumber.Pow(subExpression12, subExpression3); 
            ComplexNumber subExpression13 = -8 * a3 - 8 * b3 - 8 * c3 - 8 * d3 - 8 * e3 - 8 * f3 - 8 * g3 - 8 * h3 + 12 * subExpression12_3 + (-24 * f2 - 24 * g2 + 12 * d2 + 12 * c2 + 12 * b2 + 12 * a2 + 12 * e2) * h + (12 * e2 + 12 * f2 + 12 * b2 - 24 * h2 + 12 * a2 + 12 * c2 - 24 * d2) * g + (-24 * e2 - 24 * h2 + 12 * c2 + 12 * b2 + 12 * a2 + 12 * g2 - 12 * h * g + 12 * d2) * f + (-24 * f2 + 12 * c2 + 12 * g2 + 12 * a2 + 24 * h * g + 12 * h2 - 24 * b2 + 12 * d2 + (-12 * h + 24 * g) * f) * e + (12 * a2 + 12 * h2 + 12 * e2 - 24 * g2 - 24 * c2 + 12 * b2 - 12 * h * g + 12 * f2 + (24 * g + 24 * h) * f + (24 * g - 48 * h + 24 * f) * e) * d + (-24 * a2 + 12 * g2 - 24 * d2 + 12 * b2 + 12 * f2 + 24 * h * g + 12 * e2 + 12 * h2 + (24 * h - 48 * g) * f + (24 * f - 48 * g - 48 * h) * e + (24 * h + 24 * e + 24 * f - 12 * g) * d) * c + (12 * g2 - 24 * e2 + 12 * d2 + 24 * h * g + 12 * f2 - 24 * a2 + 12 * c2 + 12 * h2 + (24 * h - 48 * g) * f + (24 * g + 24 * h - 12 * f) * e + (24 * g - 48 * f - 48 * h + 24 * e) * d + (-48 * g - 48 * h - 48 * f + 24 * e + 24 * d) * c) * b + (12 * g2 + 12 * h2 + 12 * d2 + 24 * h * g - 24 * b2 + 12 * e2 + 12 * f2 - 24 * c2 + (24 * h - 48 * g) * f + (24 * f - 48 * g - 48 * h) * e + (-48 * f - 48 * h + 24 * g - 48 * e) * d + (24 * e - 12 * d + 24 * f + 24 * g + 24 * h) * c + (24 * f + 24 * d - 12 * e + 24 * h - 12 * c + 24 * g) * b) * a; 
            ComplexNumber subExpression13_8 = ComplexNumber.Pow(subExpression13, subExpression8);
            double subExpression14 = term2 * subExpression8 + term1 * subExpression6; 
            double subExpression15 = 1.0 / 6.0;
            double subExpression16 = 4.0 / 3.0;

            double subExpression17 = term2 * subExpression6 + term1 * subExpression16;
            ComplexNumber subExpression18 = subExpression15 * subExpression13_8 + subExpression17 / subExpression13_8;
            ComplexNumber subExpression19 = term1 * subExpression6 + term2 * subExpression8; 
            double subExpression20 = term2 * subExpression6 + term1 * subExpression16; 
            ComplexNumber subExpression21 = subExpression15 * subExpression13_8 + subExpression20 / subExpression13_8;
            ComplexNumber lambda23cse = (-c - d - e - f - b - g - h - a) * subExpression8; 
            ComplexNumber[] lambda = new ComplexNumber[4];
            lambda[0] = 0.0;
            lambda[1] = subExpression1 * subExpression10 / subExpression5_8;
            lambda[2] = -subExpression11 * subExpression13_8 + subExpression14 / subExpression13_8 + lambda23cse + subExpression3 * i * subExpression3_4 * subExpression18;
            lambda[3] = -subExpression11 * subExpression13_8 + subExpression19 / subExpression13_8 + lambda23cse - subExpression3 * i * subExpression3_4 * subExpression21;
 
            foreach (ComplexNumber cn in lambda) 
                if (double.IsNaN(cn.Real) || double.IsInfinity(cn.Real))
                    return null; 
                    //Console.WriteLine("MELTDOWN IN EIGEN COMPUTATION!");

            const double tooCloseForComfort = 1e-5;
            bool distinctEigenValues =
                !ComplexNumber.ApproxEqual(lambda[1], lambda[2], tooCloseForComfort) &&
                !ComplexNumber.ApproxEqual(lambda[2], lambda[3], tooCloseForComfort); 
            //if (!distinctEigenValues) 
            //    Console.WriteLine("Duplicate Eigen Values: " + SpecialFunctions.SpecialFunctions.CreateTabString2(lambda));
            return distinctEigenValues ? lambda : null;    // if non-distinct eigen values, send the message to bail. 

        }

        public static ComplexNumber[][] EigenVectors(ComplexNumber[] eigenValues, double a, double b, double c, double d, double e, double f, double g, double h)
        {
            ComplexNumber[][] eigenVectors = new ComplexNumber[4][]; 
            for (int i = 0; i < eigenValues.Length; i++) 
            {
                if (eigenValues[i] == 0) 
                    eigenVectors[i] = new ComplexNumber[] { 1, 1, 1, 1 };
                else
                {
                    if (((eigenVectors[i] = EigenVectorDropSecond(eigenValues[i], a, b, e, f, g, h)) == null) &&
                        ((eigenVectors[i] = EigenVectorDropThird(eigenValues[i], a, b, c, d, g, h)) == null) &&
                        ((eigenVectors[i] = EigenVectorDropFirst(eigenValues[i], c, d, e, f, g, h)) == null) && 
                        ((eigenVectors[i] = EigenVectorDropFourth(eigenValues[i], a, b, c, d, e, f)) == null)) 
                    {
                        //Console.WriteLine("Failed to decompose eigen vectors for eigen value {0}", eigenValues[i]); 
                        return null;
                    }
                }
            }

            return LinearAlgebra.Transpose(eigenVectors); 
        } 

        private static ComplexNumber[] EigenVectorDropFirst(ComplexNumber lambda, double c, double d, double e, double f, double g, double h) 
        {
            //Console.WriteLine("Dropping FIRST");
            ComplexNumber k2 = lambda * lambda;
            ComplexNumber k3 = lambda * k2;
            double subExpression1 = d + e + f + h + c + g;
            double subExpression2 = f + h + e; 
            double subExpression3 = g + h; 
            double subExpression4 = e + h + g + f;
            double subExpression5 = subExpression2 * d + subExpression3 * e + g * f + subExpression4 * c; 
            double subExpression6 = subExpression3 * e + g * f;
            ComplexNumber subExpression7 = k3 + subExpression1 * k2 + subExpression5 * lambda + subExpression6 * c + e * d * h;
            double subExpression8 = g * c + e * h;
            ComplexNumber subExpression9 = subExpression8 * lambda + subExpression6 * c + e * d * h;

            if (ComplexNumber.ApproxEqual(subExpression9, 0, 0.01)) 
                return null; 

            ComplexNumber subExpression10 = c * k2 + subExpression4 * c * lambda + subExpression6 * c + e * d * h; 
            double subExpression11 = e * c + e * d + subExpression3 * e;
            ComplexNumber subExpression12 = k2 * e + subExpression11 * lambda + subExpression6 * c + e * d * h;

            ComplexNumber[] vector = new ComplexNumber[4];
            vector[0] = subExpression7 / subExpression9;
            vector[1] = subExpression10 / subExpression9; 
            vector[2] = subExpression12 / subExpression9; 
            vector[3] = 1.0;
 

            foreach (ComplexNumber cn in vector)
                if (Math.Abs(cn.Real) > maxEigenVectorValue || Math.Abs(cn.Img) > maxEigenVectorValue)
                    return null;

            return vector; 
 
        }
        private static ComplexNumber[] EigenVectorDropSecond(ComplexNumber lambda, double a, double b, double e, double f, double g, double h) 
        {

            ComplexNumber k2 = lambda * lambda;
            ComplexNumber k3 = lambda * k2;
            double subExpression1 = e + f + h + g;
            double subExpression2 = g + h; 
            double subExpression3 = subExpression2 * e + g * f; 
            ComplexNumber subExpression4 = a * k2 + subExpression1 * a * lambda + g * b * f + subExpression3 * a;
            double subExpression5 = a * e + g * f; 
            ComplexNumber subExpression6 = subExpression5 * lambda + g * b * f + subExpression3 * a;

            if (ComplexNumber.ApproxEqual(subExpression6, 0, 0.01))
                return null;

            double subExpression7 = e + a + f + h + b + g; 
            double subExpression8 = f + h + g; 
            double subExpression9 = subExpression8 * b + subExpression2 * e + g * f + subExpression1 * a;
            ComplexNumber subExpression10 = k3 + subExpression7 * k2 + subExpression9 * lambda + g * b * f + subExpression3 * a; 
            double subExpression11 = g * b + g * a + g * e + g * f;
            ComplexNumber subExpression12 = g * k2 + subExpression11 * lambda + g * b * f + subExpression3 * a;

            ComplexNumber[] vector = new ComplexNumber[4];
            vector[0] = subExpression4 / subExpression6;
            vector[1] = subExpression10 / subExpression6; 
            vector[2] = 1; 
            vector[3] = subExpression12 / subExpression6;
 
            foreach (ComplexNumber cn in vector)
                if (Math.Abs(cn.Real) > maxEigenVectorValue || Math.Abs(cn.Img) > maxEigenVectorValue)
                    return null;

            //Console.WriteLine("Second Successful");
            return vector; 
        } 

        private static ComplexNumber[] EigenVectorDropThird(ComplexNumber lambda, double a, double b, double c, double d, double g, double h) 
        {
            //Console.WriteLine("Dropping THIRD");

            ComplexNumber k2 = lambda * lambda;
            ComplexNumber k3 = lambda * k2;
            double subExpression1 = g + c + h + d; 
            double subExpression2 = g + h; 
            double subExpression3 = subExpression2 * c + d * h;
            ComplexNumber subExpression4 = k2 * b + subExpression1 * b * lambda + subExpression3 * b + d * h * a; 
            double subExpression5 = d * h + c * h + a * h + h * b;
            ComplexNumber subExpression6 = k2 * h + subExpression5 * lambda + subExpression3 * b + d * h * a;

            if (ComplexNumber.ApproxEqual(subExpression6, 0, 0.01))
                return null;
 
            double subExpression7 = c * b + d * h; 
            ComplexNumber subExpression8 = subExpression7 * lambda + subExpression3 * b + d * h * a;
            double subExpression9 = b + d + a + c + g + h; 
            double subExpression10 = g + h + d;
            double subExpression11 = subExpression1 * b + subExpression2 * c + d * h + subExpression10 * a;
            ComplexNumber subExpression12 = k3 + subExpression9 * k2 + subExpression11 * lambda + subExpression3 * b + d * h * a;

            ComplexNumber[] vector = new ComplexNumber[4];
            vector[0] = subExpression4 / subExpression6; 
            vector[1] = subExpression8 / subExpression6; 
            vector[2] = subExpression12 / subExpression6;
            vector[3] = 1.0; 

            foreach (ComplexNumber cn in vector)
                if (Math.Abs(cn.Real) > maxEigenVectorValue || Math.Abs(cn.Img) > maxEigenVectorValue)
                    return null;

            return vector; 
 
        }
 
        private static ComplexNumber[] EigenVectorDropFourth(ComplexNumber lambda, double a, double b, double c, double d, double e, double f)
        {
            //Console.WriteLine("Dropping FOURTH");

            ComplexNumber k2 = lambda * lambda;
            ComplexNumber k3 = lambda * k2; 
            double subExpression1 = b * f + a * d; 
            double subExpression2 = f * d + f * c;
            double subExpression3 = e + f; 
            ComplexNumber subExpression4 = subExpression1 * lambda + subExpression2 * b + subExpression3 * d * a;
            double subExpression5 = a * f + f * d + b * f + f * c;
            ComplexNumber subExpression6 = k2 * f + subExpression5 * lambda + subExpression2 * b + subExpression3 * d * a;

            if (ComplexNumber.ApproxEqual(subExpression6, 0, 0.01))
                return null; 
 
            double subExpression7 = a * d + b * d + subExpression3 * d;
            ComplexNumber subExpression8 = k2 * d + subExpression7 * lambda + subExpression2 * b + subExpression3 * d * a; 
            double subExpression9 = c + f + b + e + a + d;
            double subExpression10 = d + c + f;
            double subExpression11 = f + d + e;
            double subExpression12 = subExpression10 * b + subExpression3 * c + subExpression3 * d + subExpression11 * a;
            ComplexNumber subExpression13 = k3 + subExpression9 * k2 + subExpression12 * lambda + subExpression2 * b + subExpression3 * d * a;
 
            ComplexNumber[] vector = new ComplexNumber[4]; 
            vector[0] = subExpression4 / subExpression6;
            vector[1] = subExpression8 / subExpression6; 
            vector[2] = 1.0;
            vector[3] = subExpression13 / subExpression6;

            foreach (ComplexNumber cn in vector)
                if (Math.Abs(cn.Real) > maxEigenVectorValue || Math.Abs(cn.Img) > maxEigenVectorValue)
                    return null; 
 
            return vector;
        } 

        public static ComplexNumber[][] InvEigenVectorMatrix(ComplexNumber[][] eigenVector)
        {
            ComplexNumber x12 = eigenVector[0][1];
            ComplexNumber x13 = eigenVector[0][2];
            ComplexNumber x14 = eigenVector[0][3]; 
            ComplexNumber x22 = eigenVector[1][1]; 
            ComplexNumber x23 = eigenVector[1][2];
            ComplexNumber x24 = eigenVector[1][3]; 
            ComplexNumber x32 = eigenVector[2][1];
            ComplexNumber x33 = eigenVector[2][2];
            ComplexNumber x34 = eigenVector[2][3];
            ComplexNumber x42 = eigenVector[3][1];
            ComplexNumber x43 = eigenVector[3][2];
            ComplexNumber x44 = eigenVector[3][3]; 
 
            ComplexNumber subExpression1 = x22 * x33 - x23 * x32;
            ComplexNumber subExpression2 = x23 * x34 - x24 * x33; 
            ComplexNumber subExpression3 = x24 * x32 - x22 * x34;
            ComplexNumber subExpression4 = subExpression1 * x44 + subExpression2 * x42 + subExpression3 * x43;
            ComplexNumber subExpression5 = x33 - x23;
            ComplexNumber subExpression6 = subExpression5 * x12 - x22 * x33 + x23 * x32;
            ComplexNumber subExpression7 = -x32 + x22;
            ComplexNumber subExpression8 = x24 * x32 - x22 * x34 + subExpression7 * x44; 
            ComplexNumber subExpression9 = -x33 + x23; 
            ComplexNumber subExpression10 = x34 - x24;
            ComplexNumber subExpression11 = -x23 * x34 + x24 * x33 + subExpression9 * x14 + subExpression10 * x13; 
            ComplexNumber subExpression12 = -x34 + x24;
            ComplexNumber subExpression13 = x32 - x22;
            ComplexNumber subExpression14 = subExpression12 * x12 + x22 * x34 - x24 * x32 + subExpression13 * x14;
            ComplexNumber subExpression15 = subExpression2 * x12 + subExpression6 * x44 + subExpression1 * x14 + subExpression8 * x13 + subExpression11 * x42 + subExpression14 * x43;

            const double theshold = 1e-5; 
            if (Math.Abs(subExpression15.Real) < theshold && Math.Abs(subExpression15.Img) < theshold) 
            {
                //Console.WriteLine("Instable inverse. Denominator = " + subExpression15); 
                return null;
            }

            ComplexNumber subExpression16 = x13 * x34 - x14 * x33;
            ComplexNumber subExpression17 = -x12 * x34 + x14 * x32;
            ComplexNumber subExpression18 = x12 * x33 * x44 - x32 * x13 * x44 + subExpression16 * x42 + subExpression17 * x43; 
            ComplexNumber subExpression19 = -x13 * x24 + x14 * x23; 
            ComplexNumber subExpression20 = x12 * x24 - x14 * x22;
            ComplexNumber subExpression21 = -x12 * x23 * x44 + x22 * x13 * x44 + subExpression19 * x42 + subExpression20 * x43; 
            ComplexNumber subExpression22 = subExpression2 * x12 + subExpression1 * x14 + subExpression3 * x13;
            ComplexNumber subExpression23 = x23 * x34 - x24 * x33 + subExpression5 * x44 + subExpression12 * x43;
            ComplexNumber subExpression24 = -x44 + x34;
            ComplexNumber subExpression25 = x14 - x34;
            ComplexNumber subExpression26 = x33 * x44 - x14 * x33 + subExpression24 * x13 + subExpression25 * x43;
            ComplexNumber subExpression27 = -x44 + x24; 
            ComplexNumber subExpression28 = x14 - x24; 
            ComplexNumber subExpression29 = x23 * x44 - x14 * x23 + subExpression27 * x13 + subExpression28 * x43;
            ComplexNumber subExpression30 = x22 * x34 - x24 * x32 + subExpression13 * x44 + subExpression12 * x42; 
            ComplexNumber subExpression31 = -x12 + x32;
            ComplexNumber subExpression32 = x12 * x34 + subExpression31 * x44 - x14 * x32 + subExpression25 * x42;
            ComplexNumber subExpression33 = -x22 + x12;
            ComplexNumber subExpression34 = -x14 + x24;
            ComplexNumber subExpression35 = -x12 * x24 + subExpression33 * x44 + x14 * x22 + subExpression34 * x42;
            ComplexNumber subExpression36 = x22 * x33 - x23 * x32 + subExpression9 * x42 + subExpression13 * x43; 
            ComplexNumber subExpression37 = x33 - x13; 
            ComplexNumber subExpression38 = -x32 + x12;
            ComplexNumber subExpression39 = -x12 * x33 + x13 * x32 + subExpression37 * x42 + subExpression38 * x43; 
            ComplexNumber subExpression40 = -x23 + x13;
            ComplexNumber subExpression41 = -x12 + x22;
            ComplexNumber subExpression42 = x12 * x23 - x13 * x22 + subExpression40 * x42 + subExpression41 * x43;
            ComplexNumber subExpression43 = subExpression5 * x12 - x22 * x33 + x23 * x32 + subExpression7 * x13;

            ComplexNumber[][] v = new ComplexNumber[4][]; 
            v[0] = new ComplexNumber[4]; 
            v[0][0] = -subExpression4 / subExpression15;
            v[0][1] = subExpression18 / subExpression15; 
            v[0][2] = subExpression21 / subExpression15;
            v[0][3] = subExpression22 / subExpression15;
            v[1] = new ComplexNumber[4];
            v[1][0] = subExpression23 / subExpression15;
            v[1][1] = -subExpression26 / subExpression15;
            v[1][2] = subExpression29 / subExpression15; 
            v[1][3] = subExpression11 / subExpression15; 
            v[2] = new ComplexNumber[4];
            v[2][0] = -subExpression30 / subExpression15; 
            v[2][1] = subExpression32 / subExpression15;
            v[2][2] = subExpression35 / subExpression15;
            v[2][3] = subExpression14 / subExpression15;
            v[3] = new ComplexNumber[4];
            v[3][0] = subExpression36 / subExpression15;
            v[3][1] = subExpression39 / subExpression15; 
            v[3][2] = subExpression42 / subExpression15; 
            v[3][3] = subExpression43 / subExpression15;
 
            foreach (ComplexNumber[] cnarr in v)
                foreach (ComplexNumber cn in cnarr)
                    if (double.IsNaN(cn.Real))
                        return null;
                        //Console.WriteLine("we got trouble in inverse land.");
            return v; 
        } 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
