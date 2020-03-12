using System;
using System.Collections.Generic;
using System.Text;

namespace FalseDiscoveryRateClasses
{
    class ContingencyTableCache
    {
        /**
         * Implements a caching data structure for contingency tables
         * Author - Guy Shani 2008
         * */
        private Map<int[], double> m_slContingencyTables;

        public ContingencyTableCache()
        {
            m_slContingencyTables = new Map<int[], double>(new CTComparer());
        }

        public double getCachedValue(ContingencyTable ct)
        {
            int[] aKey = new int[] { ct.getA(), ct.getB(), ct.getC(), ct.getD() };
            if (m_slContingencyTables.ContainsKey(aKey))
                return m_slContingencyTables[aKey];
            return double.NaN;
        }

        public void setCachedValue(ContingencyTable ct, double dValue)
        {
            int[] aKey = new int[] { ct.getA(), ct.getB(), ct.getC(), ct.getD() };
            if (!m_slContingencyTables.ContainsKey(aKey))
            {
                m_slContingencyTables.Add(aKey, dValue);
            }
            else
            {
                m_slContingencyTables[aKey] = dValue;
            }
        }

        private class CTComparer : IComparer<int[]>, IEqualityComparer<int[]>
        {

            #region IComparer Members

            public int Compare(int[] aX, int[] aY)
            {
                int i = 0;
                for (i = 0; i < aX.Length; i++)
                {
                    if (aX[i] != aY[i])
                        return aX[i] - aY[i];
                }
                return 0;
            }

            #endregion

            #region IEqualityComparer<int[]> Members

            public bool Equals(int[] x, int[] y)
            {
                return Compare(x, y) == 0;
            }

            public int GetHashCode(int[] a)
            {
                int iHashCode = 0;
                int idx = 0;
                int[] aPrimes = new int[] { 11, 17, 23, 29 };
                for (idx = 0; idx < a.Length; idx++)
                {
                    iHashCode += a[idx] * aPrimes[idx % aPrimes.Length];
                }
                return iHashCode;
            }

            #endregion
        }
    }
}
