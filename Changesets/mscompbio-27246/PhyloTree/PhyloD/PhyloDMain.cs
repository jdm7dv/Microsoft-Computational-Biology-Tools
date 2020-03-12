using System; 
using System.Collections.Generic;
using System.Text;
using VirusCount.PhyloTree;
using Msr.Mlas.SpecialFunctions;
using System.IO;
 
namespace PhyloD 
{
    class PhyloDMain 
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
 
                string optimizerName = argCollection.ExtractOptional<string>("optimizer", "BrentThenGrid");
                string keepTestName = argCollection.ExtractOptional<string>("keepTest", "AlwaysKeep"); 
                string skipRowIndexFileNameOrNull = argCollection.ExtractOptional<string>("skipRowIndexFile", null);

                argCollection.CheckNoMoreOptions();

                string treeFileName = argCollection.ExtractNext<string>("treeFile");
                string predictorFileName = argCollection.ExtractNext<string>("predictorFile"); 
                string targetFileName = argCollection.ExtractNext<string>("targetFile"); 
                string leafDistributionName = argCollection.ExtractNext<string>("leafDistribution");
                string nullDataGeneratorName = argCollection.ExtractNext<string>("nullDataGenerator"); 
                string niceName = argCollection.ExtractNext<string>("niceName");
                string outputDirectory = argCollection.ExtractNext<string>("outputDirectory");
                RangeCollection pieceIndexRangeCollection = argCollection.ExtractNext<RangeCollection>("pieceIndexRange");
                int pieceCount = argCollection.ExtractNext<int>("pieceCount");
                RangeCollection nullIndexRangeCollection = argCollection.ExtractNext<RangeCollection>("nullIndexRange");
 
                argCollection.CheckThatEmpty(); 

                if (!PhyloDDriver.ValidateDistribution(leafDistributionName)) 
                {
                    Console.WriteLine("{0} is not a recognized distribution name. Please choose a name from the following list:", leafDistributionName);
                    foreach (string name in PhyloDDriver.GetDistributionNames())
                    {
                        Console.WriteLine("\t{0}", name);
                    } 
                    throw new ArgumentException("Invalid distribution name."); 
                }
                RangeCollection skipRowIndexRangeCollectionOrNull = (null == skipRowIndexFileNameOrNull) || skipRowIndexFileNameOrNull == "null" ? null : RangeCollection.Parse(File.ReadAllText(skipRowIndexFileNameOrNull)); 
                KeepTest<Dictionary<string, string>> keepTest = KeepTest<Dictionary<string, string>>.GetInstance(null, keepTestName);

                SpecialFunctions.CheckCondition(pieceIndexRangeCollection.IsBetween(0, pieceCount - 1), "pieceIndex must be at least 0 and less than pieceCount");
                SpecialFunctions.CheckCondition(nullIndexRangeCollection.IsBetween(-1, int.MaxValue), "nullIndex must be at least -1");

                PhyloTree aPhyloTree = PhyloTree.GetInstance(treeFileName, null); 
 
                ModelScorer modelScorer = ModelScorer.GetInstance(aPhyloTree, leafDistributionName, optimizerName);
                ModelEvaluator modelEvaluator = ModelEvaluator.GetInstance(leafDistributionName, modelScorer); 
                PhyloDDriver driver = PhyloDDriver.GetInstance();

                driver.Run(
                    modelEvaluator,
                    predictorFileName, targetFileName,
                    leafDistributionName, nullDataGeneratorName, 
                    keepTest, skipRowIndexRangeCollectionOrNull, 
                    niceName,
                    outputDirectory, 
                    pieceIndexRangeCollection, pieceCount,
                    nullIndexRangeCollection,
                    optimizerName);

                //Console.Write("Press enter to exist.");
                //Console.Read(); 
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
PhyloD -help 
PhyloD [many arguments, see -help for details]
"; 
        static string HelpMessage = @"

Required Arguments:
    treeFile
    predictorFile
    targetFile 
    leafDistribution (e.g. DiscreteConditionalEscape) 
    nullDataGenerator (e.g. PredictorPermutation or TargetParametric)
    niceName (e.g. Gag) 
    outputDirectory
    pieceIndexSet pieceCount nullIndexSet (e.g. 0-0 1 -1-9)

Optional Argument:
    -keepTest, default AlwaysKeep
    -skipRowIndexFile, (default null, e.g. a file with the lines 100-200,300-400) 
    -optimizer BrentThenGrid (default) OR BrentWithNoGrid OR Grid 
                 You can append a tolerance to the Brent methods: e.g. BrentThenGrid.001.
                 The default Brent tolerance is .001. 
";

    }
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
