using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace FalseDiscoveryRateClasses
{
    /**
     * Implements a 2X2 contingency table data structure
     * Author - Guy Shani 2008
     * */
    public class ContingencyTable : IComparable
    {
        private int m_iA, m_iB, m_iC, m_iD, m_iN;
        private int m_iMinPossibleA, m_iMaxPossibleA;
        private double m_dHypergeometricValue;
        private string m_sTableName, m_sAdditionalData;
        private int m_cTableNameColumns;
        private double m_dFisher2TailValue;
        private double m_dMinimalPValue;

        private static int g_cCacheHits = 0;
        private static ContingencyTableCache g_ftcFisherTestCache = new ContingencyTableCache();
        private static bool g_bUseCaching = false;
        private static int m_cComputedFisherScores = 0;

        public enum TestType{ TwoSided, Left, Right };

        //BUGBUG - one sided would not properly work for computing all p-values in a single iteration
        private static TestType g_ttTestType = TestType.TwoSided;


        public ContingencyTable(int a, int b, int c, int d, string sTableName)
        {
            m_iA = a;
            m_iB = b;
            m_iC = c;
            m_iD = d;
            m_iN = a + b + c + d;
            m_dHypergeometricValue = -1.0;
            m_dFisher2TailValue = -1.0;
            m_sTableName = sTableName;
            if( sTableName != null )
                m_cTableNameColumns = 1;
            m_iMinPossibleA = -1;
            m_iMaxPossibleA = -1;
            m_dMinimalPValue = 1.0;
            m_sAdditionalData = null;
        }

        public ContingencyTable(int a, int b, int c, int d)
            : this(a, b, c, d, null)
        {
        }

        /**
         * This constructor is used when reading tables from a file.
         * The line has to have tab separators.
         * We impose the following format - first you have cTableNameColumns columns of "names" or any other data that is not parsed.
         * After the names come the table counts - a \t b \t c \t d.
         * After that can come any additional information that would just be appended to the final result.
         * If the wrong format is detected the execution is aborted.
         * */
        public ContingencyTable(string sLine,int cTableNameColumns)
        {
            m_cTableNameColumns = cTableNameColumns;
            string sOriginalLine = sLine;
            try
            {
                m_sTableName = "";
                string[] asColumns = sLine.Split(new char[] { '\t' });
                int iColumn = 0;
                for( iColumn = 0 ; iColumn < cTableNameColumns ; iColumn++ )
                {
                    if (m_sTableName.Length > 0)
                        m_sTableName += "\t";
                    m_sTableName += asColumns[iColumn];
                }
                m_iA = int.Parse(asColumns[iColumn++]);
                m_iB = int.Parse(asColumns[iColumn++]);
                m_iC = int.Parse(asColumns[iColumn++]);
                m_iD = int.Parse(asColumns[iColumn++]);
                m_iN = m_iA + m_iB + m_iC + m_iD;
                m_sAdditionalData = "";
                for (; iColumn < asColumns.Length; iColumn++)
                {
                    if (m_sAdditionalData.Length > 0)
                        m_sAdditionalData += "\t";
                    m_sAdditionalData += asColumns[iColumn];
                }
                if (m_sAdditionalData.Length == 0)
                    m_sAdditionalData = null;
            }
            catch (Exception e)
            {
                string sMessage = "";
                string sTableNames = "";
                for( int i = 0 ; i < m_cTableNameColumns ; i++ )
                {
                    sTableNames += "name ";
                }
                sMessage = "Wrong format: expected " + sTableNames + "a b c d. ";
                sMessage += " Actual input was " + sOriginalLine;
                throw new Exception(sMessage);
            }
            m_dHypergeometricValue = -1.0;
            m_dFisher2TailValue = -1.0;
        }

        public string getTableName()
        {
            return m_sTableName;
        }

        public int getA()
        {
            return m_iA;
        }
        public int getB()
        {
            return m_iB;
        }
        public int getC()
        {
            return m_iC;
        }
        public int getD()
        {
            return m_iD;
        }
        public int getN()
        {
            return m_iN;
        }


        #region IComparable Members

        /**
         * We implement a comparable to allow the tables to be sorted by p-values.
         * */

        public int CompareTo(object obj)
        {
            if (obj is ContingencyTable)
            {
                ContingencyTable rOther = (ContingencyTable)obj;
                if (rOther.getFisher2TailPermutationTest() < getFisher2TailPermutationTest())
                    return 1;
                if (rOther.getFisher2TailPermutationTest() > getFisher2TailPermutationTest())
                    return -1;
            }
            else if (obj is double)
            {
                double d = (double)obj;
                if (d < getFisher2TailPermutationTest())
                    return 1;
                if (d > getFisher2TailPermutationTest())
                    return -1;
            }
            return 0;
        }

        #endregion

        public override string ToString()
        {
            string sRest = getA() + "\t" + getB() + "\t" + getC() + "\t" + getD();
            if (getAdditionalData() != null)
                sRest += "\t" + getAdditionalData();
            sRest += "\t" + FalseDiscoveryRate.floor(getFisher2TailPermutationTest());
            if (m_cTableNameColumns == 0)
                return sRest;
            return getTableName() + "\t" + sRest;
        }

        public string getTableString()
        {
            string sRest = getA() + "\t" + getB() + "\t" + getC() + "\t" + getD();
            if (m_cTableNameColumns == 0)
                return sRest;
            return getTableName() + "\t" + sRest;
        }

        public string getAdditionalData()
        {
            return m_sAdditionalData;
        }

        /*
         * Validates that the marginals are all positive.
         * */
        public bool validate()
        {
            return (m_iA + m_iB >= 0) && (m_iA + m_iC >= 0) && (m_iB + m_iD >= 0) && (m_iC + m_iD >= 0);
        }

        /* Returns 2 sided FET.  Uses "Fisher statistic" for determining
           whether counts are more extreme.
           See http://www.med.uio.no/imb/stat/two-by-two/methods.html. */
        public double getFisher2TailPermutationTest()
        {
            if ( m_dFisher2TailValue >= 0.0 )
                return m_dFisher2TailValue;
            if (!validate())
                return 1;
            if (g_bUseCaching)
            {
                double dCached = g_ftcFisherTestCache.getCachedValue(this);
                if (!double.IsNaN(dCached))
                {
                    g_cCacheHits++;
                    m_dFisher2TailValue = dCached;
                    return m_dFisher2TailValue;
                }
            }

            m_dFisher2TailValue = computeFisher2TailPermutationTest( getHypergeometricProbability(), 1.0 );
            if (m_dFisher2TailValue > 1.0)
                m_dFisher2TailValue = 1.0;

            if (g_bUseCaching)
                g_ftcFisherTestCache.setCachedValue(this, m_dFisher2TailValue);

            return m_dFisher2TailValue;
        }

        public double getFisher2TailPermutationTest(double dCutoff)
        {
            return computeFisher2TailPermutationTest(getHypergeometricProbability(), dCutoff);
        }


        public double[] computeAllFisherStatistics(double[] adResults, ref bool bApproximated)
        {
            double p0 = getHypergeometricProbability();// probability of seeing the actual data
            double p0Epsilon = p0 * 0.00001;
            double p = p0, pt = 0, ptMax = 0.0, pLeft = p0, pRight = p0;
            ContingencyTable ctMaxTable = getMaxValueTable(), ctIterator = ctMaxTable;
            int iMaxA = getMaxPossibleA(), iMinA = getMinPossibleA();
            int cPermutations = 0, cRemiaingPermutations = iMaxA - iMinA;
            int iCurrentA = 0;
            double[] adMapping = new double[iMaxA + 1];

            adResults[0] = p0;

            ptMax = ctIterator.getHypergeometricProbability();
            pt = ptMax;
            while (ctIterator != null)
            {
                cPermutations++;
                iCurrentA = ctIterator.getA();
                adMapping[iCurrentA] = pt;

                if( iCurrentA > m_iA )
                    pRight += pt;
                if (iCurrentA < m_iA)
                    pLeft += pt;

                if (p0 + p0Epsilon >= pt && iCurrentA != m_iA)
                {
                    p = p + pt;
                }
                pt = ctIterator.incrementalHypergeometricProbability(pt);
                ctIterator = ctIterator.next();

                if ((ctIterator != null) && (pt == double.Epsilon))
                {
                    pt *= (iMaxA - ctIterator.getA() + 1);
                    p += pt;
                    pRight += pt;
                    bApproximated = true;
                    for (iCurrentA = ctIterator.getA(); iCurrentA <= iMaxA; iCurrentA++)
                    {
                        adMapping[iCurrentA] = double.Epsilon;
                    }
                    ctIterator = null;
                }
            }
            //Iterate to the left side - decreasing values of a
            ctIterator = ctMaxTable.previous();
            pt = ctMaxTable.decrementalHypergeometricProbability(ptMax);
            while (ctIterator != null)
            {
                cPermutations++;
                iCurrentA = ctIterator.getA();
                adMapping[iCurrentA] = pt;

                if (iCurrentA > m_iA)
                    pRight += pt;
                if (iCurrentA < m_iA)
                    pLeft += pt;

                if (p0 + p0Epsilon >= pt && iCurrentA != m_iA)
                {
                    p = p + pt;
                }
                pt = ctIterator.decrementalHypergeometricProbability(pt);
                ctIterator = ctIterator.previous();

                if ((ctIterator != null) && (pt == double.Epsilon))
                {
                    pt *= (ctIterator.getA() - getMinPossibleA());
                    p += pt;
                    pLeft += pt;
                    bApproximated = true;
                    for (iCurrentA = ctIterator.getA(); iCurrentA >= iMinA; iCurrentA--)
                    {
                        adMapping[iCurrentA] = double.Epsilon;
                    }
                    ctIterator = null;
                }
            }
            for (iCurrentA = iMinA - 1; iCurrentA >= 0; iCurrentA--)
            {
                adMapping[iCurrentA] = 0.0;
            }

            adResults[1] = pLeft;
            adResults[2] = pRight;
            adResults[3] = p;

            return adMapping;
        }

        /**
         * Computes the Fisher 2 sided p-value.
         * We iterate over all the premutations and sum the ones that have a lower probability (more extreme).
         * We compute from scratch only a single hypergeometric probability - the probability of the "real" table.
         * Then we iterate by incrementing the table and the probability (right side) and by decrementing the table and the probability (left side).
         * The algorithm has the complexity of O(n), but usually runs much faster.
         * Adding another possible optimization - when the p-value exceeds a cutoff - return 1. This is useful when we only need to know whether one value is larger than the other.
         * When the values are too small to be represented by a double (less than 1-E302) the computation returns an upper bound on the real value.
         * */
        private double computeFisher2TailPermutationTest(double dObservedTableProbability, double dCutoff)
        {
            double p0 = dObservedTableProbability;
            double p0Epsilon = p0 * 0.00001;
            double p = p0, pt = 0, pAbove = 0.0, pLeft = p0, pRight = 0.0;
            ContingencyTable ctIterator = null;
            int cPermutations = 0, cRemiaingPermutations = getMaxPossibleA() - getMinPossibleA();

            m_dMinimalPValue = p0;

            if (p0 == double.Epsilon)
                return p0 * cRemiaingPermutations; //an upper bound estimation


            //Iterate to the right side - increasing values of a
            if (g_ttTestType == TestType.Right || g_ttTestType == TestType.TwoSided)
            {
                ctIterator = next();
                pt = incrementalHypergeometricProbability(p0);
                while (ctIterator != null)
                {
                    if (pt < m_dMinimalPValue)
                        m_dMinimalPValue = pt;
                    cPermutations++;

                    if (p0 + p0Epsilon >= pt)
                    {
                        p = p + pt;
                        pLeft += pt;
                        if (p > dCutoff)
                            return 1.0;
                    }
                    else
                    {
                        pAbove += pt;
                    }
                    pt = ctIterator.incrementalHypergeometricProbability(pt);
                    ctIterator = ctIterator.next();

                    if ((ctIterator != null) && (pt <= p0Epsilon))
                    {
                        pt *= (getMaxPossibleA() - ctIterator.getA() + 1);
                        p += pt;
                        pLeft += pt;
                        ctIterator = null;
                    }
                }
            }
            //Iterate to the left side - decreasing values of a
            if (g_ttTestType == TestType.Left || g_ttTestType == TestType.TwoSided)
            {
                ctIterator = previous();
                pt = decrementalHypergeometricProbability(p0);
                double dBackward = pt;
                while (ctIterator != null)
                {
                    if (pt < m_dMinimalPValue)
                        m_dMinimalPValue = pt;
                    cPermutations++;
                    if (p0 + p0Epsilon >= pt)
                    {
                        p = p + pt;
                        pRight += pt;
                        if (p > dCutoff)
                            return 1.0;
                    }
                    else
                    {
                        pAbove += pt;
                    }
                    double dBefore = pt;
                    pt = ctIterator.decrementalHypergeometricProbability(pt);
                    ctIterator = ctIterator.previous();

                    if ((ctIterator != null) && (pt <= p0Epsilon))
                    {
                        pt *= (ctIterator.getA() - getMinPossibleA());
                        p += pt;
                        pRight += pt;
                        ctIterator = null;
                    }
                }
            }

            m_cComputedFisherScores++;

            return p;
        }

        /**
         * Computes the Fisher scores for all the permutations in a single pass.
         * The algorithm works by starting on one side (min a) and moving to the other side (max a).
         * We compute all the probabilities that we encounter on the way incrementally.
         * Then we sort the probabilities in increasing order and sum them up in that direction.
         * The result is a mapping between the permutation probabilities and p-values.
         * TODO - we may want to cache the resulting list in case of multiple calls
         * */
        public double[,] computeAllPermutationsScores()
        {
            int cPermutations = getMaxPossibleA() - getMinPossibleA() + 1;
            List<double> alProbabilities = new List<double>();
            double[,] adScores = new double[cPermutations, 2];
            //We start from the table with the maximal value to avoid numeric computation problems
            ContingencyTable ctMaxValue = getMaxValueTable();
            ContingencyTable ctIterator = ctMaxValue;
            double pStart = ctIterator.getHypergeometricProbability();

            double pCurrent = pStart, dSum = 0.0;
            int iCurrent = 0;

            //iterate to the right side
            while (ctIterator != null)
            {
                //Add the probability of the current permutation to the list
                alProbabilities.Add(pCurrent);
                //Increment the probability
                pCurrent = ctIterator.incrementalHypergeometricProbability(pCurrent);
                //Increment the table - will return null once a exceeds the max value
                ctIterator = ctIterator.next();
            }

            //iterate to the left side
            ctIterator = ctMaxValue;
            pCurrent = ctIterator.decrementalHypergeometricProbability(pStart);
            ctIterator = ctIterator.previous();
            while (ctIterator != null)
            {
                //Add the probability of the current permutation to the list
                alProbabilities.Add(pCurrent);
                //Decrement the probability
                pCurrent = ctIterator.decrementalHypergeometricProbability(pCurrent);
                //Decrement the table - will return null once a drops below the min value
                ctIterator = ctIterator.previous();
            }

            //Sort the observed probabilities in increasing order
            alProbabilities.Sort();
            //BUGBUG - suspecting that we do not handle well identical entries. Not sure if this bug is occuring.
            dSum = 0.0;
            //Sum the probabilities in increasing order, computing two sided p-values
            //BUGBUG - Not sure how to make this work for one sided tests.
            for (iCurrent = 0; iCurrent < alProbabilities.Count; iCurrent++)
            {
                pCurrent = (double)alProbabilities[iCurrent];
                dSum += pCurrent;
                if (dSum > 1.0)
                    dSum = 1.0;
                adScores[iCurrent, 0] = pCurrent;
                adScores[iCurrent, 1] = dSum;
            }
            return adScores;
        }


        //The actual computation is implemented in the HypergeometricProbability class
        public double getHypergeometricProbability()
        {
            if (m_dHypergeometricValue < 0.0)
                m_dHypergeometricValue = HypergeometricProbability.pr(this);
            return m_dHypergeometricValue;
        }

        /*
         * Computes hypergeometrical probability of the next contingency table:
         * a b   =>   a+1 b-1
         * c d   =>   c-1 d+1
         * in an incremental manner using the current score
         */
        public double incrementalHypergeometricProbability(double dValue)
        {
            double a = m_iA, b = m_iB, c = m_iC, d = m_iD; //must work with double to avoid overflow
            double dFactor = (b * c) / ((a + 1.0) * (d + 1.0));
            double pt = dValue * dFactor;
            if (pt == 0.0)
                return double.Epsilon;
            return pt;
        }
        /*
        * Computes hypergeometrical probability of the previous contingency table:
        * a b   =>   a-1 b+1
        * c d   =>   c+1 d-1
        * in an incremental manner using the current score
        */
        public double decrementalHypergeometricProbability(double dValue)
        {
            double a = m_iA, b = m_iB, c = m_iC, d = m_iD;//must work with double to avoid overflow
            double dFactor = (a * d) / ((b + 1.0) * (c + 1.0));
            double pt = dValue * dFactor;
            if (pt == 0.0)
                return double.Epsilon;
            return pt;
        }

        //Returns the table with the minimal a - the most extreme table to the left
        public ContingencyTable getFirstTable()
        {
            int a = getMinPossibleA();
            int b = m_iA + m_iB - a;
            int c = m_iA + m_iC - a;
            int d = m_iC + m_iD - c;
            return new ContingencyTable(a, b, c, d);
        }
        //Returns the table with the maximal a - the most extreme table to the right
        public ContingencyTable getLastTable()
        {
            int a = getMaxPossibleA();
            int b = m_iA + m_iB - a;
            int c = m_iA + m_iC - a;
            int d = m_iC + m_iD - c;
            return new ContingencyTable(a, b, c, d);
        }
        //Returns the median table 
        private ContingencyTable getMedianTable()
        {
            int a = (getMinPossibleA() + getMaxPossibleA()) / 2;
            int b = m_iA + m_iB - a;
            int c = m_iA + m_iC - a;
            int d = m_iC + m_iD - c;
            return new ContingencyTable(a, b, c, d);
        }
        //Returns the table that has the maximal probability and hence a p-value of 1
        //We do this by following the incremental process, checking to see when the coefficient of the increment factor drops below 1.
        private ContingencyTable getMaxValueTable()
        {
            //can be done using binary search to speed things up
            double a = getMinPossibleA();
            double b = m_iA + m_iB - a;
            double c = m_iA + m_iC - a;
            double d = m_iC + m_iD - c;

            double dFactor = (b * c) / ((a + 1.0) * (d + 1.0));

            while( (dFactor >= 1.0) && ( a <= getMaxPossibleA() ) )
            {
                a++;
                b = m_iA + m_iB - a;
                c = m_iA + m_iC - a;
                d = m_iC + m_iD - c;
                dFactor = (b * c) / ((a + 1.0) * (d + 1.0));
            }

            return new ContingencyTable((int)a, (int)b, (int)c, (int)d);
        }

        public int getMaxPossibleA()
        {
            if( m_iMaxPossibleA <= 0 )
                m_iMaxPossibleA = Math.Min(m_iA + m_iB, m_iA + m_iC);
            return m_iMaxPossibleA;
        }
        public int getMinPossibleA()
        {
            if( m_iMinPossibleA <= 0 )
                m_iMinPossibleA = Math.Max(m_iA - m_iD, 0);
            return m_iMinPossibleA;
        }
        //Returns the next permutation with an incremented a
        public ContingencyTable next()
        {
            if (m_iA == getMaxPossibleA())
                return null;
            return new ContingencyTable(m_iA + 1, m_iB - 1, m_iC - 1, m_iD + 1);
        }
        //Returns the previous permutation with a decremented a
        public ContingencyTable previous()
        {
            if (m_iA == getMinPossibleA())
                return null;
            return new ContingencyTable(m_iA - 1, m_iB + 1, m_iC + 1, m_iD - 1);
        }
        //Can be returned only after calling computeFisher2TailPermutationTest
        public double getMinimalAchievablePValue()
        {
            return m_dMinimalPValue;
        }

        public double min(params double[] args)
        {
            double dMin = double.PositiveInfinity;
            foreach (double d in args)
            {
                if (d < dMin)
                    dMin = d;
            }
            return dMin;
        }

        public double max(params double[] args)
        {
            double dMax = double.NegativeInfinity;
            foreach (double d in args)
            {
                if (d > dMax)
                    dMax = d;
            }
            return dMax;
        }

        public double getMarginalsRatio()
        {
            return min(m_iA + m_iB, m_iA + m_iC, m_iB + m_iD, m_iC + m_iD) / max(m_iA + m_iB, m_iA + m_iC, m_iB + m_iD, m_iC + m_iD);
        }

        public ContingencyTable generateAlternativeTable()
        {
            double dPrX = (getA() + getC()) / (double)getN();
            double dPrYGivenTrue = getA() / (double)(getA() + getC());
            double dPrYGivenFalse = getB() / (double)(getB() + getD());
            double dRandFirst = 0.0, dRandSecond = 0.0;
            int i = 0, a = 0, b = 0, c = 0, d = 0;

            if (Math.Abs(dPrYGivenFalse - dPrYGivenTrue) < 0.2)
            {
                if (dPrYGivenTrue > dPrYGivenFalse)
                    dPrYGivenTrue += 0.2;
                else
                    dPrYGivenFalse += 0.2;
            }

            for (i = 0; i < getN(); i++)
            {
                dRandFirst = FalseDiscoveryRate.RandomGenerator.NextDouble();
                dRandSecond = FalseDiscoveryRate.RandomGenerator.NextDouble();
                if (dRandFirst < dPrX)
                {
                    if (dRandSecond < dPrYGivenTrue)
                    {
                        a++;
                    }
                    else
                    {
                        c++;
                    }
                }
                else
                {
                    if (dRandSecond < dPrYGivenFalse)
                    {
                        b++;
                    }
                    else
                    {
                        d++;
                    }
                }
            }
            return new ContingencyTable(a, b, c, d, "1");
        }
        public ContingencyTable generateNullTable()
        {
            double dPrX = (getA() + getC()) / (double)getN();
            double dPrY = (getA() + getB()) / (double)getN();
            double dRandX = 0.0, dRandY = 0.0;
            int i = 0, a = 0, b = 0, c = 0, d = 0;
            for (i = 0; i < getN(); i++)
            {
                dRandX = FalseDiscoveryRate.RandomGenerator.NextDouble();
                dRandY = FalseDiscoveryRate.RandomGenerator.NextDouble();
                if (dRandX < dPrX)
                {
                    if (dRandY < dPrY)
                    {
                        a++;
                    }
                    else
                    {
                        c++;
                    }
                }
                else
                {
                    if (dRandY < dPrY)
                    {
                        b++;
                    }
                    else
                    {
                        d++;
                    }
                }
            }
            return new ContingencyTable(a, b, c, d, "0");
        }

        internal double getProbabilityLessThanThreshold( double dAlpha )
        {
            return computeFisher2TailPermutationTest(dAlpha, 1.0);
        }
    }
}
