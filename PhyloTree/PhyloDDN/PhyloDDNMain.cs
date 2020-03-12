using System;
using System.IO;
using System.Reflection;
using Digipede.Framework;
using Digipede.Framework.Api;
using Msr.Mlas.SpecialFunctions;

namespace PhyloDDN
{
    /// <summary>
    /// Represents a command-line implementation of PhyloD for the Digipede Network.
    /// </summary>
    class PhyloDDNMain
    {
        static void Main(string[] args)
        {
            try {
                // before using the ArgCollection class, let the DigipedeClient grab its args.
                DigipedeClient client = new DigipedeClient();
                args = client.ProcessArguments(args, true);
                ArgCollection argCollection = ArgCollection.GetInstance(args);

                // show help, if requested
                if (argCollection.ExtractOptionalFlag("help"))
                {
                    Console.WriteLine("");
                    Console.WriteLine(UsageMessage); 
                    Console.WriteLine(HelpMessage);
                    return;
                }             
                
                // fail if the DigipedeClient doesn't have the args
                SpecialFunctions.CheckCondition(client.IsInitialized, "Digipede Client didn't initialize.");



                string keepTestName = argCollection.ExtractOptional("keepTest", "AlwaysKeep");
                string skipRowIndexFileNameOrNull = argCollection.ExtractOptional<string>("skipRowIndexFile", null);
                string optimizerName = argCollection.ExtractOptional("optimizer", "BrentThenGrid");
 
                argCollection.CheckNoMoreOptions();
 
                int pieceCount = argCollection.ExtractNext<int>("pieceCount"); 
                //int taskPerJobCount = argCollection.ExtractNext<int>("taskPerJobCount");  -- we're not using this -- keep it in case we want to put it back
                string treeFileName = argCollection.ExtractNext<string>("treeFile");
                string predictorFileName = argCollection.ExtractNext<string>("predictorFile"); 
                string targetFileName = argCollection.ExtractNext<string>("targetFile");
                string leafDistributionName = argCollection.ExtractNext<string>("leafDistribution");
                string nullDataGeneratorName = argCollection.ExtractNext<string>("nullDataGenerator");
                string niceName = argCollection.ExtractNext<string>("niceName");
                string outputDirectoryName = argCollection.ExtractNext<string>("outputDirectory");
                RangeCollection nullIndexRangeCollection = argCollection.ExtractNext<RangeCollection>("nullIndexRange");
                SpecialFunctions.CheckCondition(nullIndexRangeCollection.IsBetween(-1, int.MaxValue), "nullIndex must be at least -1");

                argCollection.CheckThatEmpty();

                Directory.CreateDirectory(outputDirectoryName);

                // Define a JobTemplate for PhyloD.  
                JobTemplate jobTemplate = CreateJobTemplate();
                // Require 32 bit (ensures we use WOW64 on 64-bit machines) since SpecialFunctions.dll built for x86.
                jobTemplate.Control.UseWow64On64Bit = true;
                // allow task failures (all but one failure will result in job success)
                jobTemplate.JobDefaults.MaxFailures = pieceCount - 1;
                // allow multiple concurrent tasks (one for each core); each isolated in its own process.
                jobTemplate.Control.Concurrency = ApplicationConcurrency.MultiplePerCore;
                jobTemplate.Control.ProcessHostingOptions = HostingOptions.ManySingleUse;
                

                // create a Job based on that JobTemplate
                Job job = jobTemplate.NewJob();

                // add job-specific data / files
                FileDefCollection fileDefs = job.FileDefs;
                // files
                Utility.GetNamedFileDef(fileDefs, Constants.TreeFileDefName).RemoteName = treeFileName;
                Utility.GetNamedFileDef(fileDefs, Constants.PredictorFileDefName).RemoteName = predictorFileName;
                Utility.GetNamedFileDef(fileDefs, Constants.TargetFileDefName).RemoteName = targetFileName;
                // skipRowIndex file is more complicated because it may not exist, but the JobTemplate requires it.
                FileDef fileDef = Utility.GetNamedFileDef(fileDefs, Constants.SkipRowIndexFileDefName);
                if (skipRowIndexFileNameOrNull == null || skipRowIndexFileNameOrNull == "null") {
                    // stream an empty file
                    fileDef.Stream = new MemoryStream(0);
                } else {
                    // stream the actual file
                    fileDef.LocalName = skipRowIndexFileNameOrNull;
                }

                // Create the tasks for the template
                for (int pieceIndex = 0; pieceIndex < pieceCount; pieceIndex++) {
                    // Create a Task for this piece
                    Task task = job.NewTask();
                    // Create an InputData object to encapsulate all input data in one place
                    InputData inputData = new InputData(optimizerName, keepTestName, leafDistributionName, nullDataGeneratorName, 
                        niceName, outputDirectoryName, pieceIndex, pieceCount, nullIndexRangeCollection.ToString());
                    // create a Worker for this task
                    task.Worker = new PhyloDWorker(inputData);
                }

                // Wire events to catch result data.  Note that retrieving data isn't necessary here -- 
                // data can be requested in a server call from another process.
                job.TaskCompleted += job_TaskCompleted;

                // Write an event to catch any monitoring errors
                client.MonitoringError += client_MonitoringError;

                // submit the job
                SubmissionResult sr = client.SubmitJob(jobTemplate, job);
                Console.WriteLine("Submitted job {0} with {1} tasks.", sr.JobId, job.Tasks.Count);

                // wait for the result
                JobStatusSummary jss = client.WaitForJobWithStatus(sr.JobId);
                Console.WriteLine("Job finished with status of {0}", jss.Status);

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

        /// <summary>
        /// Handles the MonitoringError event of the <see cref="DigipedeClient"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Digipede.Framework.Api.MonitoringErrorEventArgs"/> instance containing the event data.</param>
        static void client_MonitoringError(object sender, MonitoringErrorEventArgs e) {
            Console.WriteLine("Monitoring error raised by the DigipedeClient:{0}", e.Message);
        }


        /// <summary>
        /// Handles the TaskCompleted event of the <see cref="Job"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Digipede.Framework.Api.TaskStatusEventArgs"/> instance containing the event data.</param>
        private static void job_TaskCompleted(object sender, TaskStatusEventArgs e) {
            PhyloDWorker worker = (PhyloDWorker)e.TaskResult.Worker;
            using (StreamWriter writer = new StreamWriter(worker.LocalOutputFileName)) {
                writer.Write(worker.OutputData);
            }
            Console.WriteLine("Wrote the result of task {0} to output file '{1}'.", e.TaskId, worker.OutputFileName);
        }

        /// <summary>
        /// Creates a job template.
        /// </summary>
        /// <returns></returns>
        public static JobTemplate CreateJobTemplate() {
            JobTemplate jobTemplate = JobTemplate.NewWorkerJobTemplate(Assembly.GetExecutingAssembly());
            // add job-specific input files to the template.  These will get streamed to the Server and downloaded to the Agents.
            // tree file
            FileDef fd = jobTemplate.FileDefs.Add();
            fd.Name = Constants.TreeFileDefName;
            fd.TransferType = TransferType.Streamed;
            fd.Relevance = Relevance.JobPlaceholder;
            fd.LocalName = "tree$(JobId).txt";
            // predictor file
            fd = jobTemplate.FileDefs.Add();
            fd.Name = Constants.PredictorFileDefName;
            fd.TransferType = TransferType.Streamed;
            fd.Relevance = Relevance.JobPlaceholder;
            fd.LocalName = "predictor$(JobId).txt";
            // target file (not an output file!)
            fd = jobTemplate.FileDefs.Add();
            fd.Name = Constants.TargetFileDefName;
            fd.TransferType = TransferType.Streamed;
            fd.Relevance = Relevance.JobPlaceholder;
            fd.LocalName = "target$(JobId).txt";            
            // skip row index file (this requires special handling below because it may be null)
            fd = jobTemplate.FileDefs.Add();
            fd.Name = Constants.SkipRowIndexFileDefName;
            fd.TransferType = TransferType.Streamed;
            fd.Relevance = Relevance.JobPlaceholder;
            fd.LocalName = "skipRowIndex$(JobId).txt";

            return jobTemplate;
        }

        const string UsageMessage = @"Usage:
PhyloDDN -help
PhyloDDN [many arguments, see -help for details]
";
        const string HelpMessage = @"
Example: 
PhyloDDDN -u user -p password -host localhost 2 input\gag1000.txt_phyml_tree.txt input\hla.sparse.txt input\gag1000.sparse.txt DiscreteConditionalEscape PredictorPermutation Gag1000 -1--1 

Required Arguments: 
    -u user
    -p password
    -host | -services | -url
    pieceCount
    treeFile
    predictorFile
    targetFile 
    leafDistribution (e.g. DiscreteConditionalEscape) 
    nullDataGenerator (e.g. PredictorPermutation or TargetParametric)
    niceName (e.g. Gag) 
    outputDirectory
    nullIndexSet (e.g. 0-0 1 -1-9)

Optional Argument:
    -keepTest, default AlwaysKeep
    -skipRowIndexFile, (default null, e.g. a file with the lines 100-200,300-400)
    -optimizer Brent (default) OR BrentWithNoGrid (default) OR Grid 
    -poolid #, The ID of the pool on which to run this job.
    -host hostnamee, The host name of the Digipede Server (e.g., MACHINE).
    -services services, The base services address of the Digipede Server (e.g., 'http://MACHINE/DigipedeWS').
    -url url, The full endpoint of the services address of the Digipede Server (e.g., 'http://MACHINE/DigipedeWS/DigipedeWS.asmx').
";


            }
        }
