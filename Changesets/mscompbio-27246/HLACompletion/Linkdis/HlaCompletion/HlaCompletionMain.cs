using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using Msr.Linkdis;
using VirusCount; 
using System.IO; 
using System.Linq;
using System.Reflection; 


namespace Msr.Linkdis
{
    class LinkdisMain
    { 
        static void Main(string[] args) 
        {
            //HlaMsr1Factory.UnitTest(); 

            try
            {
                ArgCollection argCollection = ArgCollection.GetInstance(args);

                string ethnicityName = argCollection.ExtractOptional<string>("ethnicity", "").ToLowerInvariant(); 
                SpecialFunctions.CheckCondition(Linkdis.EthnicityNameLowerList().Contains(ethnicityName), string.Format("'-ethnicity ETHNICITY' is required, where ETHNICITY is " + Linkdis.EthnicityNameMixedList().StringJoin(", "))); 
                int outputLineLimit = argCollection.ExtractOptional<int>("outputLineLimit", 100000);
                int combinationLimit = argCollection.ExtractOptional<int>("combinationLimit", 10000); 
                bool isSparse = argCollection.ExtractOptionalFlag("sparse");

                argCollection.CheckNoMoreOptions(3);

                string inputFileName = argCollection.ExtractNext<string>("inputFile");
                string phasedOutputFileName = argCollection.ExtractNext<string>("phasedOutputFile"); 
                string unphasedOutputFileName = argCollection.ExtractNext<string>("unphasedOutputFile"); 
                argCollection.CheckThatEmpty();
 
                Linkdis linkdis = Linkdis.GetInstance(ethnicityName, combinationLimit);

                string versionName = string.Format("MSCompBio HLA Completion v. {0}", GetVersionString());


                CounterWithMessages pidCounter = CounterWithMessages.GetInstance("Pid index = {0}", 1, null); 
 
                int outputLineIndex = -1;
                using (TextWriter phasedTextWriter = File.CreateText(phasedOutputFileName), 
                                    unphasedTextWriter = File.CreateText(unphasedOutputFileName))
                {
                    phasedTextWriter.WriteLine(versionName + "\n");
                    unphasedTextWriter.WriteLine(versionName + "\n");

                    phasedTextWriter.WriteLine("pid" + "\t" + PhasedExpansion.Header); 
                    unphasedTextWriter.WriteLine("pid" + "\t" + UnphasedExpansion.Header); 
                    outputLineIndex += 6;
 
                    HashSet<string> warningSet = new HashSet<string>();
                    using (TextReader textReader = File.OpenText(inputFileName))
                    {
                        foreach (PidAndHlaSet pidAndHlaSet in isSparse ? PidAndHlaSet.GetEnumerationSparse(textReader) : PidAndHlaSet.GetEnumerationDense(textReader))
                        {
                            pidCounter.Increment(); 
                            warningSet.UnionWith(pidAndHlaSet.WarningSet); 

                            ExpansionCollection expansionCollectionOrNull = linkdis.ExpandOrNullIfTooMany(pidAndHlaSet); 

                            if (null == expansionCollectionOrNull)
                            {
                                phasedTextWriter.WriteLine(pidAndHlaSet.Pid + "\t" + PhasedExpansion.TooManyCombinationsMessage());
                                unphasedTextWriter.WriteLine(pidAndHlaSet.Pid + "\t" + UnphasedExpansion.TooManyCombinationsMessage());
                                warningSet.Add(string.Format("Error: Too many combinations, case {0} skipped", pidAndHlaSet.Pid)); 
                                outputLineIndex += 2; 
                                if (outputLineIndex > outputLineLimit)
                                { 
                                    goto TOOMANYLINES;
                                }
                            }
                            else
                            {
                                foreach (PhasedExpansion phasedExpansion in expansionCollectionOrNull.Phased()) 
                                { 
                                    string phasedLine = pidAndHlaSet.Pid + "\t" + phasedExpansion.ToString();
                                    phasedTextWriter.WriteLine(phasedLine); 
                                    if (phasedExpansion.BadHlaNameOrNull != null)
                                    {
                                        warningSet.Add(phasedLine);
                                    }
                                    ++outputLineIndex;
                                    if (outputLineIndex > outputLineLimit) 
                                    { 
                                        goto TOOMANYLINES;
                                    } 
                                }

                                foreach (UnphasedExpansion unphasedExpansion in expansionCollectionOrNull.Unphased())
                                {
                                    string unphasedLine = pidAndHlaSet.Pid + "\t" + unphasedExpansion.ToString();
                                    unphasedTextWriter.WriteLine(unphasedLine); 
                                    if (unphasedExpansion.BadHlaNameOrNull != null) 
                                    {
                                        warningSet.Add(unphasedLine); 
                                    }

                                    ++outputLineIndex;
                                    if (outputLineIndex > outputLineLimit)
                                    {
                                        goto TOOMANYLINES; 
                                    } 
                                }
                            } 

                        }
                    }

                    goto INANYCASE;
                TOOMANYLINES: 
                    string tooManyLinesMessage = string.Format("ERROR: The line limit of {0} was reached and output was ended early", outputLineLimit); 
                    phasedTextWriter.WriteLine(tooManyLinesMessage);
                    unphasedTextWriter.WriteLine(tooManyLinesMessage); 
                    warningSet.Add(tooManyLinesMessage);
                INANYCASE:
                    Console.Error.WriteLine(warningSet.StringJoin("\n"));
                }
            }
            catch (Exception exception) 
            { 
                Console.WriteLine(exception.Message);
                if (exception.InnerException != null) 
                {
                    Console.WriteLine(exception.InnerException.Message);
                }

                Console.Error.WriteLine(@"
 
USAGE 

HlaCompletion -ethnicity ETHNICITY [-outputLineLimit 100000] [-sparse] [-combinationLimit 10000] inputFile phaseFile unphaseFile 
where ETHNICITY is {0}
'outputLineLimit' limits the total lines of output. If it is reached, a warning message is written as the last line of the output.
'combinationLimit' limits the number of combinations of HLAs consider in one phase for one case.
        It is is reached, an error message is output for that case in place of results.
'-sparse' reads files in sparse format
 
", Linkdis.EthnicityNameMixedList().StringJoin(", ")); 

                System.Environment.Exit(-1); 
            }
        }

        private static string GetVersionString()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Msr.Linkdis.Linkdis)); 
            string verString = assembly.GetName().Version.ToString(); 
            return verString;
        } 

    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
