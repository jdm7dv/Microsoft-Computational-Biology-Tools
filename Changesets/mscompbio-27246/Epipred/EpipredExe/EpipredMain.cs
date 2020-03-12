using System; 
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using VirusCount;
using Msr.Mlas.SpecialFunctions; 
using System.IO; 
using EpipredLib;
 


namespace Epipred
{
    class EpipredMain
    { 
        public static void Main(string[] args) 
        {
            try 
            {

                // Parse arguments
                ArgCollection argCollection = ArgCollection.GetInstance(args);

                if (argCollection.ExtractOptionalFlag("Help")) 
                { 
                    Console.WriteLine(HelpString);
                    return; 
                }

                string modelName = argCollection.ExtractOptional("model", "LANLIEDB03062007");
                bool modelOnly = argCollection.ExtractOptionalFlag("modelOnly");
                bool inputHasHeader = argCollection.ExtractOptionalFlag("inputHasHeader");
                MerLength merLength = argCollection.ExtractOptional<MerLength>("merLength", MerLength.scan); 
                int? dOfCenter = argCollection.ExtractOptional<int?>("withinDOfCenter", null); 
                ShowBy showBy = argCollection.ExtractOptional<ShowBy>("showBy", ShowBy.all);
                string hlaSetName = argCollection.ExtractOptional<string>("hlaSet", "singleton"); 
                HlaSetSpecification hlaSetSpecification = HlaSetSpecification.GetInstance(hlaSetName);

                argCollection.CheckNoMoreOptions();

                string inputFileName = argCollection.ExtractNext<string>("inputFile");
                string outputFileName = argCollection.ExtractNext<string>("outputFile"); 
                argCollection.CheckThatEmpty(); 

 
                ReadInputCreatingOutput(showBy, modelName, inputHasHeader, merLength, dOfCenter, hlaSetSpecification, modelOnly, inputFileName, outputFileName);

            }
            catch (Exception exception)
            {
                Console.WriteLine(""); 
                Console.WriteLine(exception.Message); 
                if (exception.InnerException != null)
                { 
                    Console.WriteLine(exception.InnerException.Message);
                }

                Console.Error.WriteLine("");
                Console.Error.WriteLine(@"For more help:
Epipred -help"); 
 
                System.Environment.Exit(-1);
            } 
        }

        static string HelpString = string.Format(@"

USAGE
 
For each line of input, reports the most likely epitope(s). 
In case of ties, combines result into one line.
 
Epipred
    {{-inputHasHeader}}
    {{-merLength k}}
    {{-withinDOfCenter d}}
    {{-hlaSet [{0}]}}
    {{-modelOnly}} 
    {{-showBy [{1}]}} 
    {{-model LANLIEDB03062007}}
    inputFileName outputFileName 

OR

Epipred -help

INPUT 
 
Each line of the input should be of the form
 
peptide <TAB> hla ...  [when ""-hlaSet singleton"" is given]
OR
peptide <TAB> supertype ...[when ""-hlaSet supertype"" is given]
OR
peptide ... [when ""-hlaSet all"" is given]
 
Additional fields will be echoed to output. If a header is given, it 
must have columns for at least peptide and, if given, hla or supertype
 
FLAGS

If ""-inputHasHeader"" is given, the 1st line of the input is ignored.

""-merLength k"", where k is ""scan"" (default), ""8"", ""9"", ""10"", ""11"", or ""given""
With ""scan"" all lengths are scanned. 
With a numeric value, only mers of that length are scanned 
With ""given"" there is no scanning; the whole input peptide is scored.
In all cases, an adjustment is made to correct for the relative frequency of the length/hla pair. 

""-withinDOfCenter d"" where d is an integer or null (default)
When an integer this limits the scan for epitopes to mers such that
at least one edge of the mer is within d amino acids of the center
of the peptide given for scanning. If the input peptide has even
length, we use left of center for center. 
 

 
""-hlaSet singleton"" (default) means to use the hla from the file
""-hlaSet supertype"" means use the supertype from the file, scanning all its hlas
""-hlaSet all"" means to scan all known hlas
Among the hlas considered, the best will be reported

""-modelOnly"" if given then uses the model's probability even if a peptide is on a source list. 
                Otherwise, peptides on a source list are given a probability of 1.0. 

""-showBy"" tells how to report the maximum probability prediction(s) 
    ""-showBy all"" (default) - for each input line, reports the maximum probability prediction(s).
    ""-showBy hla""  - for each input line and hla, reports.
    ""-showBy length""  - for each input line and peptide length, reports.
    ""-showBy hlaAndLength""  - for each input line and hla and peptide length, reports.
    ""-showBy doNotGroup""  - for each input line, reports every prediction.
 
OUTPUT 

The output is a header line followed by one output line for each input line.
Additional fields are inserted after the required inputs, the fields are:
    HLA (if hla is not given)   {2}",
                                    "singleton or supertype or all",
                                    string.Join(" or ", Enum.GetNames(typeof(ShowBy))), 
                                    Prediction.ExtraHeader(/*includeHlaInOutput*/ false) 
                    );
        private static void ReadInputCreatingOutput(ShowBy showBy, string modelName,
            bool inputHasHeader, MerLength merLength, int? dOfCenter,
            HlaSetSpecification hlaSetSpecification, bool modelOnly, string inputFileName, string outputFileName)
        {
            using (TextReader textReader = File.OpenText(inputFileName)) 
            { 
                using (TextWriter textWriter = File.CreateText(outputFileName))
                { 
                    string header = CreateHeader(hlaSetSpecification, textReader, inputHasHeader);
                    textWriter.WriteLine(header);

                    PredictorCollection predictorCollection = PredictorCollection.GetInstance(modelName);

                    foreach(string line in SpecialFunctions.ReadEachLine(textReader)) 
                    { 
                        foreach (string outputLine in ProcessLine(showBy, predictorCollection, line, merLength, dOfCenter, hlaSetSpecification, modelOnly))
                        { 
                            textWriter.WriteLine(outputLine);
                            textWriter.Flush();
                        }
                    }
                }
            } 
        } 

        private static List<string> ProcessLine(ShowBy showBy, PredictorCollection predictorCollection, string line, 
            MerLength merLength, int? dOfCenter, HlaSetSpecification hlaSetSpecification, bool modelOnly)
        {
            try
            {
                string hlaOrSupertypeOrNull;
                string inputPeptide = ExtractInputs(hlaSetSpecification, line, predictorCollection, out hlaOrSupertypeOrNull); 
 
                List<string> output = new List<string>();
                foreach (List<Prediction> predictionList in predictorCollection.MaxProbabilityPredictions(showBy, inputPeptide, merLength, dOfCenter, hlaSetSpecification, hlaOrSupertypeOrNull, modelOnly).Values) 
                {
                    string outputLine = InsertMaterial(line, hlaSetSpecification.InputHeaderCollection().Length, Prediction.CollectionToString(predictionList, false, hlaSetSpecification.IncludeHlaInOutput()));
                    output.Add(outputLine);
                }
                return output;
 
            } 
            catch (Exception exception)
            { 
                string errorString = SpecialFunctions.CreateTabString(
                            line,
                            string.Format("Error: {0}{1}", exception.Message, exception.InnerException == null ? "" : string.Format(" ({0})", exception.InnerException)));
                List<string> output = new List<string>();
                output.Add(errorString);
                return output; 
 
            }
        } 




        static public string CreateHeader(HlaSetSpecification hlaSetSpecification, TextReader textReader, bool inputHasHeader)
        { 
            if (!inputHasHeader) 
            {
                return hlaSetSpecification.Header(); 
            }

            //!!!this code is very similar to that which processes the non-header lines. Could/should they be combined?


            int splitPoint = hlaSetSpecification.InputHeaderCollection().Length; 
            string middleStuff = Prediction.ExtraHeader(hlaSetSpecification.IncludeHlaInOutput()); 
            List<string> columnCollection = SpecialFunctions.Split(textReader.ReadLine(), '\t');
 
            return InsertMaterial(columnCollection, splitPoint, middleStuff);
        }
        static public string InsertMaterial(string line, int splitPoint, string middleStuff)
        {
            List<string> columnCollection = SpecialFunctions.Split(line, '\t');
            return InsertMaterial(columnCollection, splitPoint, middleStuff); 
        } 

        static public string InsertMaterial(List<string> columnCollection, int splitPoint, string middleStuff) 
        {
            SpecialFunctions.CheckCondition(columnCollection.Count >= splitPoint, string.Format("Input requires {0} column(s)", splitPoint));
            string startOfLine = SpecialFunctions.Join("\t", SpecialFunctions.First(columnCollection, splitPoint));
            string restOfLine = SpecialFunctions.Join("\t", SpecialFunctions.Rest(columnCollection, splitPoint));
            return SpecialFunctions.CreateTabString(
                    startOfLine, 
                    middleStuff, 
                    restOfLine);
        } 


        static public string ExtractInputs(HlaSetSpecification hlaSetSpecification, string line, PredictorCollection predictorCollection, out string hlaOrSupertypeOrNull)
        {
            List<string> fieldCollection = SpecialFunctions.Split(line, '\t');
 
            int inputLength = hlaSetSpecification.InputHeaderCollection().Length; 
            SpecialFunctions.CheckCondition(inputLength <= fieldCollection.Count, string.Format("Expected input to have at least {0} columns", inputLength));
            string inputPeptide = fieldCollection[0]; 

            hlaOrSupertypeOrNull = (inputLength > 1) ? fieldCollection[1] : null;

            return inputPeptide;
        }
 
 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
