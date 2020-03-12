using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FalseDiscoveryRateClasses;

namespace FalseDiscoveryRateCommandLine
{
    class Program
    {
        private static int findArgument(string[] args, string sArgument)
        {
            int iArgument = 0;
            for (iArgument = 0; iArgument < args.Length; iArgument++)
            {
                if (args[iArgument].ToLower().StartsWith(sArgument.ToLower()))
                    return iArgument;
            }
            return -1;
        }
        /*
        static void Main(string[] args)
        {
            DirectoryInfo dir = new DirectoryInfo( "D:\\fdr\\SyntheticMultiple" );
            FileInfo[] aFiles = dir.GetFiles();
            int iFile = 0, cFiles = aFiles.Length;
            foreach (FileInfo f in aFiles)
            {
                if (f.Name.StartsWith("synth") && f.Name.EndsWith(".txt"))
                {
                    Console.WriteLine("Processing file " + iFile + " out of " + cFiles);
                    FalseDiscoveryRate t = new FalseDiscoveryRate(1, true, double.PositiveInfinity, false, -1, 0.0, true, FalseDiscoveryRate.PiMethod.WeightedSum);
                    t.computeFDR(f.FullName, f.FullName + ".FDR");
                    iFile++;
                }
            }
        }
        */
        static void Main(string[] args)
        {
            if( args.Length < 2 )
            {
                Console.WriteLine("FisherExactTest utility for computing Fisher Exact Test and False Discovery Rate (FDR) for contingency tables.");
                Console.WriteLine("Written by Guy Shani, Microsoft Research, October 2008.");
                Console.WriteLine("Use the format: FisherExactTest inputfile outputfile <options>" );
                Console.WriteLine("inputfile - name of the text file containing the contingency table data. Each contingency table resides in a single row with the format a b c d, separateed by tabs." );
                Console.WriteLine("outputfile - name of the text file where the results should be written." );
                Console.WriteLine("Additional available options:");
                Console.WriteLine("[-UseTableNames:<column count>] - each contingency table can be perceeded by table names columns. Use this option to set the number of columns perceeding each table. The default is 0 - no table names." );
                Console.WriteLine("[-SilentMode] - execute the application without any progress messages." );
                Console.WriteLine("[-FDRCutoff:x] - Allows to avoid outputing tables with an FDR exciding the threshold x.");
                Console.WriteLine("[-Huge] - When the input data is huge (more than 100K tables) this option avoids several caching procedures to allow handling more data.");
                Console.WriteLine("[-UseSampling[:n]] - Computes FDRs using only n sampled tables. Default value for n is 100K.");
                Console.WriteLine("[-AutomatedSampling[:d]] - Activates the automated sampling algorithm, computing FDRs repeatedly with increasing sample sizes until the largest change in FDR drops below d. Default value for d is 0.1.");
                Console.WriteLine("[-ColumnHeaders] - use when the input files has column headers.");
                Console.WriteLine("[-EvaluatePi] - by default pi0=1, set this parameter to evaluate pi0=sum(observed p-values)/sum(null p-values).");
                Console.WriteLine("[-Filtering] - Computes pi0 using only relevant tables. This flag supersedes the EvaluatePi flag.");
                Console.WriteLine("[-FullOutput] - outputs all the statistics that were computed. By default only p-values and q-values are written to the output file.");
                Console.WriteLine("[-pFDR] - Compute positive FDR. The default is to compute FDR rather than pFDR.");
            }
            else
            {
                string sInputFileName = args[0];
                string sOutputFileName = args[1];
                int cTableNamesColumns = 0;
                bool bReportProgress = ( findArgument( args, "-SilentMode" ) == -1 );
                bool bHuge = (findArgument(args, "-Huge") != -1);
                double dCutoff = double.PositiveInfinity;
                int iSampleSize = -1;
                double dMinimalChangeBetweenSamples = -1.0;
                bool bHasColumnHeaders = false;
                FalseDiscoveryRate.PiMethod mPi = FalseDiscoveryRate.PiMethod.One;
                bool bFullOutput = false;
                bool bPositiveFDR = false;

                int iColumnHeaders = findArgument(args, "-ColumnHeaders");
                if (iColumnHeaders != -1)
                {
                    bHasColumnHeaders = true;
                }

                int iFullOutput = findArgument(args, "-FullOutput");
                if (iFullOutput != -1)
                {
                    bFullOutput = true;
                }

                int iPFDR = findArgument(args, "-pFDR");
                if (iPFDR != -1)
                {
                    bPositiveFDR = true;
                }

                int iEvaluatePi = findArgument(args, "-EvaluatePi");
                if (iEvaluatePi != -1)
                {
                    mPi = FalseDiscoveryRate.PiMethod.WeightedSum;
                }
                iEvaluatePi = findArgument(args, "-Filtering");
                if (iEvaluatePi != -1)
                {
                    mPi = FalseDiscoveryRate.PiMethod.Filtering;
                }

                int iCutoff = findArgument(args, "-FDRCutoff");
                if( iCutoff != -1 )
                {
                    int idx = args[iCutoff].IndexOf( ':' );
                    dCutoff = double.Parse( args[iCutoff].Substring( idx + 1 ) );
                }
                int iTableNames = findArgument(args, "-UseTableNames");
                if (iTableNames != -1)
                {
                    int idx = args[iTableNames].IndexOf(':');
                    cTableNamesColumns = int.Parse(args[iTableNames].Substring(idx + 1));
                }

                int iSampling = findArgument(args, "-UseSampling");
                if (iSampling != -1)
                {
                    int idx = args[iSampling].IndexOf(':');
                    if (idx > 0)
                        iSampleSize = int.Parse(args[iSampling].Substring(idx + 1));
                    else
                        iSampleSize = 100000;
                }

                int iAutomatedSampling = findArgument(args, "-AutomatedSampling");
                if (iAutomatedSampling != -1)
                {
                    int idx = args[iAutomatedSampling].IndexOf(':');
                    if (iSampleSize == -1)
                        iSampleSize = 100000;
                    if (idx < 0)
                        dMinimalChangeBetweenSamples = 0.01;
                    else
                        dMinimalChangeBetweenSamples = double.Parse(args[iAutomatedSampling].Substring(idx + 1));
                }

                DateTime dtBefore = DateTime.Now;
                ProgressReport pr = new ConsoleProgressReport();
                FalseDiscoveryRate t = new FalseDiscoveryRate(cTableNamesColumns, bReportProgress, dCutoff, bHuge, iSampleSize, dMinimalChangeBetweenSamples, bHasColumnHeaders, mPi, bPositiveFDR, bFullOutput, pr);
                t.computeFDR(sInputFileName, sOutputFileName);

                DateTime dtAfter = DateTime.Now;
                if (bReportProgress)
                    Console.WriteLine("Total execution time " + dtAfter.Subtract(dtBefore));
            }
        }
    }
}
