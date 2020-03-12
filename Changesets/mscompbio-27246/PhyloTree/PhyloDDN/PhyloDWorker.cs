using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Digipede.Framework;
using Digipede.Framework.Api;
using Msr.Mlas.SpecialFunctions;
using VirusCount.PhyloTree;

namespace PhyloDDN
{
    [Serializable]
    public class PhyloDWorker : Worker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhyloDWorker"/> class.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        /// <remarks>This constructor is used before Task submission from the master application.</remarks>
        public PhyloDWorker(InputData inputData)
        {
            mInputData = inputData;
        }

        // serialized input data
        private InputData mInputData;
        // serialized output data
        private string mOutputFileName;
        private string mOutputData;
        private string mLocalOutputFileName;

        /// <summary>
        /// Gets the output data.
        /// </summary>
        /// <value>The output data.</value>
        public string OutputData
        {
            get { return mOutputData; }
        }

        /// <summary>
        /// Gets the name of the output file.
        /// </summary>
        /// <value>The name of the output file.</value>
        public string OutputFileName
        {
            get { return mOutputFileName; }
        }

        public string LocalOutputFileName
        {
            get { return mLocalOutputFileName; }
        }

        /// <summary>
        /// Does the work.
        /// </summary>
        public override void DoWork()
        {
            // get our input data and null the field to make sure we don't serialize it back
            InputData inputData = mInputData;
            mInputData = null;

            // get the job-specific names of input files
            FileDefCollection fileDefs = Job.FileDefs;
            string treeFileName = Utility.GetNamedFileDef(fileDefs, Constants.TreeFileDefName).LocalName;
            string predictorFileName = Utility.GetNamedFileDef(fileDefs, Constants.PredictorFileDefName).LocalName;
            string targetFileName = Utility.GetNamedFileDef(fileDefs, Constants.TargetFileDefName).LocalName;
            string skipRowIndexFileName = Utility.GetNamedFileDef(fileDefs, Constants.SkipRowIndexFileDefName).LocalName;

            // construct RangeCollections
            RangeCollection pieceIndexRangeCollection = RangeCollection.Parse(inputData.PieceIndexRange);
            RangeCollection nullIndexRangeCollection = RangeCollection.Parse(inputData.NullIndexRange);
            RangeCollection skipRowIndexRangeCollection;
            FileInfo fileInfo = new FileInfo(skipRowIndexFileName);
            if (fileInfo.Length > 0)
            {
                skipRowIndexRangeCollection = RangeCollection.Parse(File.ReadAllText(skipRowIndexFileName));
            }
            else
            {
                skipRowIndexRangeCollection = null;
            }

            // do the rest
            PhyloTree aPhyloTree = PhyloTree.GetInstance(treeFileName, null);
            ModelScorer modelScorer =
                ModelScorer.GetInstance(aPhyloTree, inputData.LeafDistributionName, inputData.OptimizerName);
            ModelEvaluator modelEvaluator = ModelEvaluator.GetInstance(inputData.LeafDistributionName, modelScorer);
            KeepTest<Dictionary<string, string>> keepTest =
                KeepTest<Dictionary<string, string>>.GetInstance(null, inputData.KeepTestName);
            PhyloDDriver driver = PhyloDDriver.GetInstance();

            // create a name for the temporary job sandbox.  This directory gets created by driver.Run(...)
            string agentOutputDirectoryName =
                Path.Combine(Environment.CurrentDirectory, String.Format(CultureInfo.InvariantCulture, "{0}.{1}", Job.JobId, Task.TaskId));

            // save the standard out and standard error in memory streams
            using (MemoryStream streamOut = new MemoryStream(), streamError = new MemoryStream())
            {
                try
                {
                    // redirect the outputs
                    using (
                        StreamWriter writerOut = new StreamWriter(streamOut),
                                     writerError = new StreamWriter(streamError))
                    {
                        Console.SetOut(writerOut);
                        Console.SetError(writerError);

                        try
                        {
                            // run the model
                            string outputFileName = driver.Run(
                                modelEvaluator,
                                predictorFileName, targetFileName,
                                inputData.LeafDistributionName, inputData.NullDataGeneratorName,
                                keepTest, skipRowIndexRangeCollection,
                                inputData.NiceName,
                                agentOutputDirectoryName,
                                pieceIndexRangeCollection, inputData.PieceCount,
                                nullIndexRangeCollection,
                                inputData.OptimizerName);

                            // this is the expected output file name -- save this so it can be written on the master side with the same name.
                            mOutputFileName = Path.GetFileName(outputFileName);


                            mLocalOutputFileName = Path.Combine(inputData.LocalOutputDirectoryName, mOutputFileName);

                            // get the output data
                            string fullOutputPath = Path.Combine(agentOutputDirectoryName, mOutputFileName);
                            if (!File.Exists(fullOutputPath))
                            {
                                TaskResult.FailureReason = TaskFailureReason.MissingOutput;
                                TaskResult.FailureMessage = String.Format(CultureInfo.CurrentCulture, "Cannot find output file '{0}'", targetFileName);
                                TaskResult.Status = TaskAssignmentStatus.Failed;
                            }
                            using (StreamReader outputData = new StreamReader(fullOutputPath))
                            {
                                mOutputData = outputData.ReadToEnd();
                            }
                        }
                        finally
                        {
                            // this finally is to make sure we delete the folder
                            // get rid of the sandbox
                           Directory.Delete(agentOutputDirectoryName, true);
                        }
                    }
                }
                finally
                {
                    // this finally is to make sure we get console output
                    Encoding encoding = Encoding.Default;
                    TaskResult.StandardOutput = encoding.GetString(streamOut.GetBuffer());
                    TaskResult.StandardError = encoding.GetString(streamError.GetBuffer());
                }
            }
        }
    }
}
