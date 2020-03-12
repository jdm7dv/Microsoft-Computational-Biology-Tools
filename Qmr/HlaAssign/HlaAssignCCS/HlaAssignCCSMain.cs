using System; 
using System.Collections.Generic;
using System.Text;
using Microsoft.ComputeCluster;
using Msr.Mlas.SpecialFunctions;
using System.IO;
using System.Diagnostics; 
using System.Reflection; 
using System.Windows.Forms;
using CCSLib; 

namespace HlaAssignCCS
{
    class HlaAssignCCSMain
    {
        static void Main(string[] args) 
        { 
            try
            { 
                ArgCollection argCollection = ArgCollection.GetInstance(args);
                if (argCollection.ExtractOptionalFlag("help"))
                {
                    Console.WriteLine("");
                    Console.WriteLine(UsageMessage);
                    Console.WriteLine(HelpMessage); 
                    return; 
                }
 
                string clusterName = argCollection.ExtractOptional<string>("cluster", null);
                string specFileNameOrNull = argCollection.ExtractOptional<string>("specFile", null);
                string internalRemoteDirectoryName = argCollection.ExtractOptional<string>("internalRemoteDirectory", null);
                SpecialFunctions.CheckCondition(specFileNameOrNull == null || internalRemoteDirectoryName == null, "specFile and internalRemoteDirectory should not both be given"); 

                argCollection.CheckNoMoreOptions(); 

                string externalRemoteDirectoryName = argCollection.ExtractNext<string>("externalRemoteDirectory");
                SpecialFunctions.CheckCondition(specFileNameOrNull == null || externalRemoteDirectoryName == "null", "When the specFile is given externalRemoteDirectory should be 'null'");

                if (null == internalRemoteDirectoryName) 
                { 
                    internalRemoteDirectoryName = externalRemoteDirectoryName;
                } 
                int pieceCount = argCollection.ExtractNext<int>("pieceCount");
                int taskPerJobCount = argCollection.ExtractNext<int>("taskPerJobCount");

                //!!!much like HlaAssign's main
                double? leakProbabilityOrNull = argCollection.ExtractNext<double?>("leakProbabilityOrNull");
                double pValue = argCollection.ExtractNext<double>("pValue"); 
                string inputDirectoryName = argCollection.ExtractNext<string>("inputDirectory"); 
                SpecialFunctions.CheckCondition(specFileNameOrNull == null || inputDirectoryName == "null", "When the specFile is given inputDirectory should be 'null'");
                string caseName = argCollection.ExtractNext<string>("caseName"); 
                SpecialFunctions.CheckCondition(specFileNameOrNull == null || caseName == "null", "When the specFile is given caseName should be 'null'");

                string hlaFactoryName = argCollection.ExtractNext<string>("hlaFactory");
                RangeCollection nullIndexRangeCollection = argCollection.ExtractNext<RangeCollection>("nullIndexRange");
                argCollection.CheckThatEmpty();
 
 
                ICluster cluster = new Cluster();
                SpecialFunctions.CheckCondition(specFileNameOrNull == null || clusterName != null, "When the specFile is given, the clusterName must be given explicitly"); 
                if (null == clusterName)
                {
                    clusterName = CCSLib.CCSLib.GetMachineNameFromUNCName(externalRemoteDirectoryName);
                }
                cluster.Connect(clusterName);
 
 

                string id = null; 
                string password = null;


                Queue<ITask> taskQueue = new Queue<ITask>();
                if (null == specFileNameOrNull)
                { 
                    FillQueueBySplitingPeptideList(pieceCount, leakProbabilityOrNull, pValue, caseName, hlaFactoryName, nullIndexRangeCollection, internalRemoteDirectoryName, externalRemoteDirectoryName, inputDirectoryName, taskQueue); 
                }
                else 
                {
                    FillQueueBySplitingDirNameLists(pieceCount, leakProbabilityOrNull, pValue, hlaFactoryName, nullIndexRangeCollection, specFileNameOrNull, taskQueue, ref caseName);
                }
                SubmitQueue(taskPerJobCount, caseName, nullIndexRangeCollection, cluster, id, password, taskQueue);

                //Console.WriteLine("\nPress enter to exit..."); 
                //Console.ReadLine(); 
            }
            catch (Exception exception) 
            {
                Console.WriteLine("");
                Console.WriteLine(exception.Message);
                if (exception.InnerException != null)
                {
                    Console.WriteLine(exception.InnerException.Message); 
                } 

                Console.WriteLine(""); 
                Console.WriteLine(UsageMessage);
                throw;
            }
        }

        private static void FillQueueBySplitingDirNameLists( 
            int pieceCountX, 
            double? leakProbabilityOrNull, double pValue, string hlaFactoryName,
            RangeCollection nullIndexRangeCollection, 
            string specFileNameOrNull,
             Queue<ITask> taskQueue, ref string caseNameTotal)
        {
            SpecialFunctions.CheckCondition(pieceCountX == 0, "When a list of directories is given, pieceCount must be 0");
            List<string> caseNameCollection = new List<string>();
            List<Dictionary<string, string>> rowCollection = SpecialFunctions.TabFileTableAsList(specFileNameOrNull, "case\tinputDirectory\texternalRemoteDirectory\tinternalRemoteDirectory", false); 
 
            int dirCount = rowCollection.Count;
 
            for (int dirIndex = 0; dirIndex < dirCount; ++dirIndex)
            {
                Dictionary<string, string> row = rowCollection[dirIndex];
                string caseName = row["case"];
                caseNameCollection.Add(caseName);
                string newExeDirectoryName; 
                string exeNewRelativeDirectoryName; 
                string internalRemoteInputDirectoryName;
                SetupDirectories(row["internalRemoteDirectory"], row["externalRemoteDirectory"], row["inputDirectory"], caseName, out newExeDirectoryName, out exeNewRelativeDirectoryName, out internalRemoteInputDirectoryName); 


                string taskCommandLine = SpecialFunctions.CreateDelimitedString(" ",
                    //"HlaAssign.exe",
                        string.Format("{0}\\HlaAssign.exe", exeNewRelativeDirectoryName),
                        leakProbabilityOrNull, pValue, internalRemoteInputDirectoryName, caseName, hlaFactoryName, 
                        string.Format("{0}-{0}", 0), 
                        1, nullIndexRangeCollection); //!!!create a nicer way to make a space delimited list
 

                ITask task = new Task();

                task.WorkDirectory = row["internalRemoteDirectory"];
                SpecialFunctions.CheckCondition(taskCommandLine.Length <= 480);
                task.CommandLine = taskCommandLine; 
 
                task.Name = SpecialFunctions.CreateDelimitedString(" ", caseName, 1, nullIndexRangeCollection);
                task.IsExclusive = false; 
                task.Stderr = string.Format(@"{0}\Stderr\{1}.txt", exeNewRelativeDirectoryName, caseName);
                Directory.CreateDirectory(newExeDirectoryName + @"\Stderr");
                task.Stdout = string.Format(@"{0}\Stdout\{1}.txt", exeNewRelativeDirectoryName, caseName);
                Directory.CreateDirectory(newExeDirectoryName + @"\Stdout");
                task.Runtime = "Infinite";
 
                taskQueue.Enqueue(task); 
            }
            if (caseNameCollection.Count == 0) 
            {
                caseNameTotal = "empty";
            }
            else if (caseNameCollection.Count == 1)
            {
                caseNameTotal = caseNameCollection[0]; 
            } 
            else
            { 
                caseNameTotal = caseNameCollection[0] + "..." + caseNameCollection[caseNameCollection.Count - 1];
            }
        }

        private static void FillQueueBySplitingPeptideList(
            int pieceCount, double? leakProbabilityOrNull, double pValue, string caseName, string hlaFactoryName, 
            RangeCollection nullIndexRangeCollection, 
            string internalRemoteDirectoryName,
            string externalRemoteDirectoryName, 
            string inputDirectoryName,
             Queue<ITask> taskQueue)
        {
            string newExeDirectoryName;
            string exeNewRelativeDirectoryName;
            string internalRemoteInputDirectoryName; 
            SetupDirectories(internalRemoteDirectoryName, externalRemoteDirectoryName, inputDirectoryName, caseName, out newExeDirectoryName, out exeNewRelativeDirectoryName, out internalRemoteInputDirectoryName); 

            for (int pieceIndex = 0; pieceIndex < pieceCount; ++pieceIndex) 
            {

                string taskCommandLine = SpecialFunctions.CreateDelimitedString(" ",
                    //"HlaAssign.exe",
                        string.Format("{0}\\HlaAssign.exe", exeNewRelativeDirectoryName),
                        leakProbabilityOrNull, pValue, internalRemoteInputDirectoryName, caseName, hlaFactoryName, 
                        string.Format("{0}-{0}", pieceIndex), 
                        pieceCount, nullIndexRangeCollection); //!!!create a nicer way to make a space delimited list
 

                ITask task = new Task();

                task.WorkDirectory = internalRemoteDirectoryName;
                SpecialFunctions.CheckCondition(taskCommandLine.Length <= 480);
                task.CommandLine = taskCommandLine; 
 
                task.Name = SpecialFunctions.CreateDelimitedString(" ", caseName, pieceIndex, nullIndexRangeCollection);
                task.IsExclusive = false; 
                task.Stderr = string.Format(@"{0}\Stderr\{1}.{2}.txt", exeNewRelativeDirectoryName, caseName, pieceIndex);
                Directory.CreateDirectory(newExeDirectoryName + @"\Stderr");
                task.Stdout = string.Format(@"{0}\Stdout\{1}.{2}.txt", exeNewRelativeDirectoryName, caseName, pieceIndex);
                Directory.CreateDirectory(newExeDirectoryName + @"\Stdout");
                task.Runtime = "Infinite";
 
                taskQueue.Enqueue(task); 
            }
        } 

        private static void SubmitQueue(int taskPerJobCount, string caseName, RangeCollection nullIndexRangeCollection, ICluster cluster, string id, string password, Queue<ITask> taskQueue)
        {
            while (taskQueue.Count != 0)
            {
 
                IJob job = cluster.CreateJob(); 
                string firstPieceIndex = null, lastPieceIndex = null;
                for (int iTaskPerJob = 0; iTaskPerJob < taskPerJobCount; ++iTaskPerJob) 
                {
                    ITask task = taskQueue.Dequeue();
                    string taskPieceIndex = task.Name.Split(' ')[1];
                    if (iTaskPerJob == 0)
                        firstPieceIndex = taskPieceIndex.ToString();
                    lastPieceIndex = taskPieceIndex; 
                    job.AddTask(task); 
                    job.Runtime = task.Runtime;
 
                    if (taskQueue.Count == 0)
                    {
                        break;
                    }
                }
                string jobName =  SpecialFunctions.CreateTabString(caseName, firstPieceIndex + "-" + lastPieceIndex, 
                    nullIndexRangeCollection).Replace("\t", " ");  //!!!create a nicer way to make a space delimited list 
                job.Name = jobName.Substring(0, Math.Min(jobName.Length,70));
                Console.WriteLine(job.Name);
                job.IsExclusive = false;
                cluster.AddJob(job);
                cluster.SubmitJob(job.Id, id, password, true, 0);
            }
        }

        private static void SetupDirectories(string internalRemoteDirectoryName, string externalRemoteDirectoryName, string inputDirectoryName, string caseName, out string newExeDirectoryName, out string exeNewRelativeDirectoryName, out string internalRemoteInputDirectoryName) 
        { 
            Directory.CreateDirectory(externalRemoteDirectoryName);
 


            CCSLib.CCSLib.CreateNewExeDirectory(caseName, externalRemoteDirectoryName, out newExeDirectoryName, out exeNewRelativeDirectoryName);

            string oldExeDirectoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            SpecialFunctions.CopyDirectory(oldExeDirectoryName, newExeDirectoryName, true); 
 
            string externalRemoteInputDirectoryName = string.Format(@"{0}\Raw", externalRemoteDirectoryName);
            internalRemoteInputDirectoryName = string.Format(@"{0}\Raw", internalRemoteDirectoryName); 
            Directory.CreateDirectory(externalRemoteInputDirectoryName);
            SpecialFunctions.CopyDirectory(inputDirectoryName, externalRemoteInputDirectoryName, false);
        }

        static string UsageMessage = @"Usage:
HlaAssignCCS [many parmaters]
   OR 
HlaAssignCCS -help
"; 
        static string HelpMessage = @"
where

    ""externalRemoteDirectory"" 
    ""pieceCount""  is the total number of tasks (for example, the # of processors on the cluster) 
    ""taskPerJobCount"", for example, 1
 
    ""leakProbabilityOrNull"" is either ""null"" (to have it learned) or given
    ""pValue"" is the max pValue.
    ""inputDirectory"" is the local location of the input files
    ""caseName"" is the prefix of the input files
        The input files themselves are
            CASENAMEKnown.txt (header: ""peptide knownHLA"") 
            CASENAMEPatient.txt (header: ""pid     a1      a2      b1      b2      c1      c2"") 
            CASENAMEReact.txt (header: ""pid     peptide mag"")
    ""hlaFactoryName"" is either ""MixedWithB15AndA68"" or ""FourDigit"" or ""JustSlash"" or ""noConstraints"" 
    nullIndexRange, for example, -1-9

Optional:

    -cluster, default is to extract from ""externalRemoteDirectory""
    -internalRemoteDirectory, the compute clusters' name for the externalRemoteDirectory

    After the runs, the output files will be in REMOTEDIRECTORY\raw
";

 
 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
