using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FalseDiscoveryRateClasses;

namespace FalseDiscoveryRateCommandLine
{
    class ConsoleProgressReport : ProgressReport
    {
        private string m_sPhase;
        public ConsoleProgressReport()
        {
            m_sPhase = "";
        }

        #region ProgressReport Members

        public bool reportProcessedTables(int cProccessedTables, int cAllTables)
        {
            return true;
        }

        public bool reportPhase(string sPhase)
        {
            m_sPhase = sPhase;
            return true;
        }

        public bool reportMessage(string sMessage, bool bNewLine)
        {
            if (bNewLine)
                Console.WriteLine(sMessage);
            else
                Console.Write(sMessage);
            return true;
        }

        public bool reportError(string sError)
        {
            Console.WriteLine(sError);
            return false;
        }

        #endregion
    }
}
