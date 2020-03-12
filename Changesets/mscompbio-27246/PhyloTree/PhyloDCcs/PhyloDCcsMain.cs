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

namespace PhyloDCcs
{
    class PhyloDCcsMain
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

                string keepTestName = argCollection.ExtractOptional<string>("keepTest", "AlwaysKeep");
                string skipRowIndexFileNameOrNull = argCollection.ExtractOptional<string>("skipRowIndexFile", null);
                string optimizerName = argCollection.ExtractOptional<string>("optimizer", "BrentThenGrid");
                string clusterName = argCollection.ExtractOptional<string>("cluster", null);
                string internalRemoteDirectoryName = argCollection.ExtractOptional<string>("internalRemoteDirectory", null); 
 
                argCollection.CheckNoMoreOptions();
 
                string externalRemoteDirectoryName = argCollection.ExtractNext<string>("externalRemoteDirectory");
                if (null == internalRemoteDirectoryName)
                {
                    internalRemoteDirectoryName = externalRemoteDirectoryName;
                }
                int pieceCount = argCollection.ExtractNext<int>("pieceCount"); 
                int taskPerJobCount = argCollection.ExtractNext<int>("taskPerJobCount"); 
                string treeFileName = argCollection.ExtractNext<string>("treeFile");
                string predictorFileName = argCollection.ExtractNext<string>("predictorFile"); 
                string targetFileName = argCollection.ExtractNext<string>("targetFile");
                string leafDistributionName = argCollection.ExtractNext<string>("leafDistribution");
                string nullDataGeneratorName = argCollection.ExtractNext<string>("nullDataGenerator");
                string niceName = argCollection.ExtractNext<string>("niceName");
                RangeCollection nullIndexRangeCollection = argCollection.ExtractNext<RangeCollection>("nullIndexRange");
 
                argCollection.CheckThatEmpty(); 

                if (null == clusterName) 
                {
                    clusterName = CCSLib.CCSLib.GetMachineNameFromUNCName(externalRemoteDirectoryName);
                }

                string exeNewRelativeDirectoryName = CCSLib.CCSLib.CopyExesToCluster(externalRemoteDirectoryName, niceName);
 
                string id = null; 
                string password = null;
 
                CreateTasks(externalRemoteDirectoryName, internalRemoteDirectoryName, pieceCount, taskPerJobCount, treeFileName, predictorFileName, targetFileName, leafDistributionName, nullDataGeneratorName, keepTestName, skipRowIndexFileNameOrNull, niceName, nullIndexRangeCollection, exeNewRelativeDirectoryName, clusterName, id, password, optimizerName);

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
 
        static string UsageMessage = @"Usage:
PhyloDCcs -help
PhyloDCcs [many arguments, see -help for details]
";
        static string HelpMessage = @"
Example: 
PhyloDCcs EXTERNALREMOTEDIRECTORY 4 2 input\gag1000.txt_phyml_tree.txt input\hla.sparse.txt input\gag1000.sparse.txt DiscreteConditionalEscape PredictorPermutation Gag1000 -1--1 

Required Arguments: 
    externalRemoteDirectory
    pieceCount
    taskPerJobCount
    treeFile
    predictorFile
    targetFile 
    leafDistribution (e.g. DiscreteConditionalEscape) 
    nullDataGenerator (e.g. PredictorPermutation or TargetParametric)
    niceName (e.g. Gag) 
    nullIndexSet (e.g. 0-0 1 -1-9)

Optional Argument:
    -keepTest, default AlwaysKeep
    -skipRowIndexFile, (default null, e.g. a file with the lines 100-200,300-400)
    -optimizer Brent (default) OR BrentWithNoGrid (default) OR Grid 
    -cluster, default is to extract from ""externalRemoteDirectory"" 
    -internalRemoteDirectory
              default is to be the same as the externalRemoteDirectory 
";



        private static void CreateTasks(string externalRemoteDirectoryName, string internalRemoteDirectoryName, int pieceCount, int taskPerJobCount, string treeFileName, string predictorFileName, string targetFileName, string leafDistributionName, string nullDataGeneratorName, string keepTestName, string skipRowIndexFileNameOrNull, string niceName, RangeCollection nullIndexRangeCollection, string exeNewRelativeDirectoryName, string clusterName, string id, string password, string optimizerName)
        { 
            ICluster cluster = new Cluster(); 
            cluster.Connect(clusterName);
 
            Queue<ITask> taskQueue = new Queue<ITask>();
            for (int pieceIndex = 0; pieceIndex < pieceCount; ++pieceIndex)
            {
                ITask task = CreateTask(externalRemoteDirectoryName, internalRemoteDirectoryName, pieceCount, treeFileName, predictorFileName, targetFileName, leafDistributionName, nullDataGeneratorName, keepTestName, skipRowIndexFileNameOrNull, niceName, nullIndexRangeCollection, exeNewRelativeDirectoryName, pieceIndex, optimizerName);
                taskQueue.Enqueue(task);
            } 
 
            while (taskQueue.Count != 0)
            { 
                IJob job = CreateJob(taskPerJobCount, leafDistributionName, niceName, nullIndexRangeCollection, cluster, taskQueue);
                cluster.AddJob(job);
                cluster.SubmitJob(job.Id, id, password, true, 0);
                //cluster.SubmitJob(job.Id, null, null, true, 0);
            }
        } 
 
        private static IJob CreateJob(int taskPerJobCount, string leafDistributionName, string niceName, RangeCollection nullIndexRangeCollection, ICluster cluster, Queue<ITask> taskQueue)
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
            job.Name = SpecialFunctions.CreateTabString(niceName, firstPieceIndex + "-" + lastPieceIndex, 
                leafDistributionName, nullIndexRangeCollection).Replace("\t", " ");  //!!!create a nicer way to make a space delimited list
            if (job.Name.Length > 80)
            {
                job.Name = job.Name.Substring(0, 80);
            }
            Console.WriteLine(job.Name);
            job.IsExclusive = false;

            return job;
        }

        private static ITask CreateTask(string externalRemoteDirectoryName, string internalRemoteDirectoryName, int pieceCount, string treeFileName, string predictorFileName, string targetFileName, string leafDistributionName, string nullDataGeneratorName, string keepTestName, string skipRowIndexFileNameOrNull, string niceName, RangeCollection nullIndexRangeCollection, string exeNewRelativeDirectoryName, int pieceIndex, string optimizerName) 
        { 
            /*                <Task MaximumNumberOfProcessors="1"
             MinimumNumberOfProcessors="1" 
             * Depend=""
             * Stdout="FILLIN.txt"
             * Stderr="FILLIN2.txt"
             * Name="FILL4"
             * CommandLine="FILLIN3"
             * IsCheckpointable="false" 
             * IsExclusive="false" 
             * IsRerunnable="false" Runtime="Infinite">
            //    <EnvironmentVariables /> 
            //</Task>
             */

            ITask task = new Task();
            task.WorkDirectory = internalRemoteDirectoryName;
 
            string taskCommandLine = SpecialFunctions.CreateDelimitedString(" ", 
                    string.Format("{0}\\PhyloD.exe", exeNewRelativeDirectoryName),
                    string.Format("-keepTest {0}", keepTestName), 
                    (null == skipRowIndexFileNameOrNull) ? "" : string.Format("-skipRowIndexFile {0}", skipRowIndexFileNameOrNull),
                    string.Format("-optimizer {0}", optimizerName),
                    treeFileName, predictorFileName, targetFileName, leafDistributionName, nullDataGeneratorName, niceName,
                    "raw",
                    string.Format("{0}-{0}", pieceIndex),
                    pieceCount, nullIndexRangeCollection); //!!!create a nicer way to make a space delimited list 
 

            SpecialFunctions.CheckCondition(taskCommandLine.Length <= 480, "Command line length " + taskCommandLine.Length + " exceeds max of 480"); 
            task.CommandLine = taskCommandLine;

            task.Name = SpecialFunctions.CreateDelimitedString(" ", niceName, pieceIndex, leafDistributionName, nullIndexRangeCollection);
            task.IsExclusive = false;
            task.Stderr = string.Format(@"{0}\Stderr\{1}.{2}.{3}.txt", internalRemoteDirectoryName, niceName, leafDistributionName, pieceIndex);
            Directory.CreateDirectory(externalRemoteDirectoryName + @"\Stderr"); 
            task.Stdout = string.Format(@"{0}\Stdout\{1}.{2}.{3}.txt", internalRemoteDirectoryName, niceName, leafDistributionName, pieceIndex); 
            Directory.CreateDirectory(externalRemoteDirectoryName + @"\Stdout");
            task.Runtime = "Infinite"; 
            return task;
        }



 
 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
