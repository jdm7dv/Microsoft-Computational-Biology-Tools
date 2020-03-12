using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree 
{ 
    abstract public class RowIndexTabulator
    { 
        internal RowIndexTabulator()
        {
        }

        static public RowIndexTabulator GetInstance(bool audit)
        { 
            if (audit) 
            {
                return new Auditor(); 
            }
            else
            {
                return new NoAudit();
            }
        } 
 
        abstract public bool TryAdd(Dictionary<string, string> row, string fileName);
        abstract public void CheckIsComplete(string inputFilePattern); 
    }

    public class Auditor : RowIndexTabulator
    {
        internal Auditor()
        { 
        } 

        RangeCollection RowIndexRangeCollection = RangeCollection.GetInstance(); 
        int RowCountSoFar = int.MinValue;

        override public bool TryAdd(Dictionary<string, string> row, string fileName)
        {
            SpecialFunctions.CheckCondition(row.ContainsKey("rowIndex"), string.Format(@"When auditing tabulation a ""rowIndex"" column is required. (File ""{0}"")", fileName));
            SpecialFunctions.CheckCondition(row.ContainsKey("rowCount"), string.Format(@"When auditing tabulation a ""rowCount"" column is required. (File ""{0}"")", fileName)); 
 
            int rowIndex = int.Parse(row["rowIndex"]);
            int rowCount = int.Parse(row["rowCount"]); 

            SpecialFunctions.CheckCondition(0 <= rowIndex && rowIndex < rowCount, string.Format(@"rowIndex must be at least zero and less than rowCount (File ""{0}"")", fileName));
            if (RowCountSoFar == int.MinValue)
            {
                RowCountSoFar = rowCount;
            } 
            else 
            {
                SpecialFunctions.CheckCondition(RowCountSoFar == rowCount, string.Format("A different row count was at rowIndex {0} in file {1}", rowIndex, fileName)); 
            }

            bool tryAdd = RowIndexRangeCollection.TryAdd(rowIndex);
            return tryAdd;
        }
 
        override public void CheckIsComplete(string inputFilePattern) 
        {
            SpecialFunctions.CheckCondition(RowIndexRangeCollection.IsComplete(RowCountSoFar), 
                string.Format("Not all needed rows were found. Here are the indexes of the found rows:\n{0}\n{1}", RowIndexRangeCollection, inputFilePattern));
        }
    }

    public class NoAudit : RowIndexTabulator
    { 
        internal NoAudit() 
        {
        } 

        override public bool TryAdd(Dictionary<string, string> row, string fileName)
        {
            return true;
        }
 
        override public void CheckIsComplete(string inputFilePattern) 
        {
            //Do nothing 
            return;
        }
    }

}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
