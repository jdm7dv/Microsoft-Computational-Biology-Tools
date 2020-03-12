using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using VirusCount.PhyloTree;
 
namespace Mlas.Tabulate 
{
    class TabulateMain 
    {
        static void Main(string[] argsx)
        {
            try
            {
                List<string> argumentCollection = new List<string>(argsx); 
 
                bool auditRowIndexValues = true;
                string noAuditFlag = "-NoAudit"; 
                if (argumentCollection.Contains(noAuditFlag))
                {
                    argumentCollection.Remove(noAuditFlag);
                    auditRowIndexValues = false;
                }
 
                double maxPValue = 1.0; // Ignore pValues greater than this 
                string maxPValueFlag = "-MaxPValue";
                int maxPValuePosition = argumentCollection.IndexOf(maxPValueFlag); 
                if (maxPValuePosition >= 0)
                {
                    argumentCollection.RemoveAt(maxPValuePosition);
                    SpecialFunctions.CheckCondition(maxPValuePosition < argumentCollection.Count, "pValue expected after -MaxPValue");
                    maxPValue = double.Parse(argumentCollection[maxPValuePosition]);
                    argumentCollection.RemoveAt(maxPValuePosition); 
                } 

                KeepTest<Dictionary<string,string>> keepTest; // Ignore pValues greater than this 
                string keepTestFlag = "-KeepTest";
                int keepTestPosition = argumentCollection.IndexOf(keepTestFlag);
                if (keepTestPosition >= 0)
                {
                    argumentCollection.RemoveAt(keepTestPosition);
                    SpecialFunctions.CheckCondition(keepTestPosition < argumentCollection.Count, "KeepTest expected after -MaxPValue"); 
                    keepTest = KeepTest<Dictionary<string, string>>.GetInstance(null, argumentCollection[keepTestPosition]); 
                    argumentCollection.RemoveAt(keepTestPosition);
                } 
                else
                {
                    keepTest = new AlwaysKeep<Dictionary<string, string>>();
                }

 
                SpecialFunctions.CheckCondition(argumentCollection.Count > 1, "Expect 2 or more parameters"); 
                string outputFileName = argumentCollection[argumentCollection.Count - 1];
                argumentCollection.RemoveAt(argumentCollection.Count - 1); 

                Tabulate.CreateTabulateReport(argumentCollection, outputFileName, keepTest, maxPValue, auditRowIndexValues);
            }
            catch (Exception e)
            {
                Console.WriteLine(""); 
                Console.WriteLine(e.Message); 
                if (e.InnerException != null)
                { 
                    Console.WriteLine(e.InnerException.Message);
                }
                Console.WriteLine(@"
Usage:
Tabulate {-NoAudit} {-MaxPValue maxPValue} {-KeepTest keeptest} broadInputFileNamePattern1 {broadInputFileNamePattern2 ...} outputFileName
 
Each broadInputFileNamePattern1 is of the form 
narrowInputFileNamePattern1{+narrowInputFileNamePattern2...}
 
Each broadInputFileNamePattern must cover the same range of nullIndexes (including -1, the real index).

Each narrowInputFileNamePattern within a broadInputFileNamePattern must cover a disjoint
set of nullIndexes.

For example 
Tabulate -MaxPValue .05  raw\GagEscape0606*-1-19*.txt raw\GagReversion0606*-1-9*.txt+raw\GagReversion0606*10-19*.txt AllGag.qValue.txt 
Notice that broad pattern
    raw\GagEscape0606*-1-19*.txt 
has one narrow pattern and covers nullIndex's -1 to 19

While broad pattern
    raw\GagReversion0606*-1-9*.txt+raw\GagReversion0606*10-19*.txt
has two narrow patterns:
   raw\GagReversion0606*-1-9*.txt, which covers nullIndexes -1 to 9 
   raw\GagReversion0606*10-19*.txt which covers nullIndexes 10 to 19 

 

By default, ""Tabulate"" will audit the ""rowIndex"" and ""rowCount"" values
in the input to remove duplicates and check that all rows are present.
Use ""-NoAudit"" when this is not desired.

Use ""-MaxPValue maxPValue"", where maxPValue is a double, to ignore rows with obviously bad rows 
"); 
                throw;
            } 

        }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
