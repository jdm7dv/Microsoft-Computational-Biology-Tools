using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FalseDiscoveryRateClasses
{
    public interface ProgressReport
    {
        bool reportProcessedTables(int cProccessedTables, int cAllTables);
        bool reportPhase(string sPhase);
        bool reportMessage(string sMessage, bool bNewLine);
        bool reportError(string sError);
    }
}
