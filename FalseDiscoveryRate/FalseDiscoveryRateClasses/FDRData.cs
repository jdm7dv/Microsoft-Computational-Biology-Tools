using System;
using System.Collections.Generic;
using System.Text;

namespace FalseDiscoveryRateClasses
{
    public class FDRData
    {
        private double m_dPValue;
        private double m_dPooledPValue;
        private double m_dFilteringPi;
        private double m_dRejectionAreaProb;
        private double m_dFDR;
        private double m_dQValue;

        public FDRData(double dPValue)
        {
            m_dPValue = dPValue;
            m_dPooledPValue = 0.0;
            m_dFilteringPi = 0.0;
            m_dRejectionAreaProb = 0.0;
            m_dFDR = 0.0;
            m_dQValue = 0.0;
        }
        public double PValue
        {
            get
            {
                return m_dPValue;
            }
            set
            {
                m_dPValue = value;
            }
        }
        public double PooledPValue
        {
            get
            {
                return m_dPooledPValue;
            }
            set
            {
                m_dPooledPValue = value;
            }
        }
        public double FilteringPi
        {
            get
            {
                return m_dFilteringPi;
            }
            set
            {
                m_dFilteringPi = value;
            }
        }
        public double RejectionAreaProb
        {
            get
            {
                return m_dRejectionAreaProb;
            }
            set
            {
                m_dRejectionAreaProb = value;
            }
        }
        public double FDR
        {
            get
            {
                return m_dFDR;
            }
            set
            {
                m_dFDR = value;
            }
        }
        public double QValue
        {
            get
            {
                return m_dQValue;
            }
            set
            {
                m_dQValue = value;
            }
        }

        public string getData(bool bAll, bool bFiltering, bool bPositiveFDR)
        {
            string sData = "";// +PValue + "\t";
            if (bAll)
            {
                sData += FalseDiscoveryRate.floor( PooledPValue ) + "\t";
                if( bFiltering )
                    sData += FalseDiscoveryRate.floor(FilteringPi) + "\t";
                if( bPositiveFDR )
                    sData += FalseDiscoveryRate.floor(RejectionAreaProb) + "\t";
                sData += FalseDiscoveryRate.floor(FDR) + "\t";
            }
            sData += FalseDiscoveryRate.floor(QValue);
            return sData;
        }
    }
}
