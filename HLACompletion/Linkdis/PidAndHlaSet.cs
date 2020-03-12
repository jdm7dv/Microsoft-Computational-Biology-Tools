using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions; 
using System.IO;
 
namespace Msr.Linkdis
{
    public class PidAndHlaSet
    {
        private PidAndHlaSet()
        { 
        } 

        public string Pid { get; private set; } 
        public LinkedList1<UOPair<HlaMsr1>> HlaUopairList { get; private set; }
        public List<string> ClassList { get; private set; } //assumed to be same length as HlaUopairList
        public HashSet<string> WarningSet;

        public static IEnumerable<PidAndHlaSet> GetEnumerationFromString(string inputString)
        { 
            Exception e1 = null; 
            try
            { 
                GetEnumerationDense(new StringReader(inputString)).First();
            }
            catch (Exception e)
            {
                e1 = e;
            } 
 
            if (e1 == null)
            { 
                return GetEnumerationDense(new StringReader(inputString));
            }

            Exception e2 = null;
            try
            { 
                GetEnumerationSparse(new StringReader(inputString)).First(); 
            }
            catch (Exception e) 
            {
                e2 = e;
            }

            if (e2 == null)
            { 
                return GetEnumerationSparse(new StringReader(inputString)); 
            }
 
            throw new Exception("Can't read inputfile as either dense or sparse. \nDense Error Message: " + e1.Message + "\nSparse Error Message: " + e2.Message, e1);
        }

        private static HlaMsr1Factory HlaMsr1Factory444 = HlaMsr1Factory.GetFactory(new int[] { 4, 4, 4 });
        public static IEnumerable<PidAndHlaSet> GetEnumerationSparse(TextReader inputTextReader)
        { 
            string previousPid = null; 
            Dictionary<string, List<HlaMsr1>> classToHlaList = null;
            HashSet<string> warningSet = null; 
            foreach (var pidAndHlaRecord in SpecialFunctions.ReadDelimitedFile(inputTextReader, new { pid = "", hla = "" }, new char[]{'\t'}, true))
            {
                string pid = pidAndHlaRecord.pid;
                if (previousPid != pid)
                {
                    if (previousPid != null) 
                    { 
                        PidAndHlaSet pidAndHlaSet = CreatePidAndHlaSet(previousPid, classToHlaList, warningSet);
                        yield return pidAndHlaSet; 
                    }
                    previousPid = pid;
                    classToHlaList = new Dictionary<string, List<HlaMsr1>>();
                    warningSet = new HashSet<string>();

                } 
                HlaMsr1 hlaMsr1 = (HlaMsr1)HlaMsr1Factory444.GetGroundOrAbstractInstance(pidAndHlaRecord.hla, ref warningSet); 
                List<HlaMsr1> hlaList = classToHlaList.GetValueOrDefault(hlaMsr1.ClassName);
                hlaList.Add(hlaMsr1); 

            }
            if (previousPid != null)
            {
                PidAndHlaSet pidAndHlaSet = CreatePidAndHlaSet(previousPid, classToHlaList, warningSet);
                yield return pidAndHlaSet; 
            } 

 

        }

        public static IEnumerable<PidAndHlaSet> GetEnumerationDense(TextReader inputTextReader)
        {
            foreach (var pidAndHlaRecord in SpecialFunctions.ReadDelimitedFile(inputTextReader, new { pid = "", A1 = "", A2 = "", B1 = "", B2 = "", C1 = "", C2 = "" }, new char[] { '\t' }, true)) 
            { 

                PidAndHlaSet pidAndHlaSet = new PidAndHlaSet(); 
                pidAndHlaSet.Pid = pidAndHlaRecord.pid;
                pidAndHlaSet.WarningSet = new HashSet<string>();
                pidAndHlaSet.HlaUopairList = LinkedList1<UOPair<HlaMsr1>>.GetInstance(
                    UOPair<HlaMsr1>.GetInstance(CreateHla(pidAndHlaRecord.C1, ref pidAndHlaSet.WarningSet), CreateHla(pidAndHlaRecord.C2, ref pidAndHlaSet.WarningSet)),
                    UOPair<HlaMsr1>.GetInstance(CreateHla(pidAndHlaRecord.B1, ref pidAndHlaSet.WarningSet), CreateHla(pidAndHlaRecord.B2, ref pidAndHlaSet.WarningSet)),
                    UOPair<HlaMsr1>.GetInstance(CreateHla(pidAndHlaRecord.A1, ref pidAndHlaSet.WarningSet), CreateHla(pidAndHlaRecord.A2, ref pidAndHlaSet.WarningSet))); 
                pidAndHlaSet.ClassList = new List<string> { "C", "B", "A" }; 
                yield return pidAndHlaSet;
            } 
        }



        private static PidAndHlaSet CreatePidAndHlaSet(string previousPid, Dictionary<string, List<HlaMsr1>> classToHlaList, HashSet<string> warningSet)
        { 
            SpecialFunctions.CheckCondition(new HashSet<string>(classToHlaList.Keys).SetEquals(new HashSet<string> { "A", "B", "C" }), "Expect Hla's for exactly classes A,B, & C. " + previousPid); 
            SpecialFunctions.CheckCondition(classToHlaList.Values.All(list => list.Count == 2), "Expect two hla lines for each Hla class. " + previousPid);
            PidAndHlaSet pidAndHlaSet = new PidAndHlaSet(); 
            pidAndHlaSet.Pid = previousPid;
            pidAndHlaSet.WarningSet = warningSet;
            pidAndHlaSet.HlaUopairList = LinkedList1<UOPair<HlaMsr1>>.GetInstance(
                UOPair<HlaMsr1>.GetInstance(classToHlaList["C"][0], classToHlaList["C"][1]),
                UOPair<HlaMsr1>.GetInstance(classToHlaList["B"][0], classToHlaList["B"][1]),
                UOPair<HlaMsr1>.GetInstance(classToHlaList["A"][0], classToHlaList["A"][1])); 
            pidAndHlaSet.ClassList = new List<string> { "C", "B", "A" }; 
            return pidAndHlaSet;
        } 

        private static HlaMsr1 CreateHla(string name, ref HashSet<string> warningSet)
        {
            return (HlaMsr1)HlaMsr1Factory444.GetGroundOrAbstractInstance(name, ref warningSet);
        }
 
 
        public IEnumerable<UOPair<LinkedList1<HlaMsr1>>> GetPhasedEnumeration()
        { 
            return UOPair<HlaMsr1>.PhaseEnumeration(HlaUopairList);
        }
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
