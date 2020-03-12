using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msr.Mlas.SpecialFunctions;

namespace CreateEpitome 
{ 
    class CreateEpitomeMain
    { 
        static void Main(string[] args)
        {
            try
            {
                ArgCollection argCollection = ArgCollection.GetInstance(args);
 
                if (argCollection.ExtractOptionalFlag("Help")) 
                {
                    Console.WriteLine(HelpString); 
                    return;
                }

                int stopLength = argCollection.ExtractOptional<int>("stopLength", 10000);

                argCollection.CheckNoMoreOptions(2); 
                string inputFileName = argCollection.ExtractNext<string>("inputFile"); 
                string outputFileName = argCollection.ExtractNext<string>("outputFile");
                argCollection.CheckThatEmpty(); 

                CreateVaccine.CreateVaccine.MakeGreedyEpitomes(inputFileName, outputFileName, stopLength);


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
CreateEpitome -help");
 
                System.Environment.Exit(-1);
            }
        }


        static string HelpString = @" 
 
USAGE: CreateEpitome {-stopLength 10000} inputFile outputFile
 
The input is a tab-delimited file with two columns: Patch & Weight
A patch is a peptide, for example, NKIVRMYSP
A weight is a number, for example, 167
" ;

 
 
    }
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
