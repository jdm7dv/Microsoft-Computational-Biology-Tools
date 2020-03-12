using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.IO;
using System.Reflection;
using System.Windows.Forms; 
 
namespace CCSLib
{ 
    public static class CCSLib
    {
        public static string GetMachineNameFromUNCName(string directoryName)
        {
            //!!!be nicer to use Regex
            SpecialFunctions.CheckCondition(directoryName.StartsWith(@"\\"), "The directory name should start with '\\'"); 
            string[] partCollecton = directoryName.Substring(2).Split(new char[] { '\\' }, 2); 
            SpecialFunctions.CheckCondition(partCollecton.Length == 2, "Expected the directory name to have a machine part and a file part");
            return partCollecton[0]; 
        }



        public static void CreateNewExeDirectory(string niceName, string directoryName, out string newExeDirectoryName, out string exeRelativeDirectoryName)
        { 
            string exeRelativeDirectoryNameMostly = string.Format(@"exes\{0}{1}", niceName, DateTime.Now.ToShortDateString().Replace("/", "")); 
            for (int suffixIndex = 0; ; ++suffixIndex)
            { 
                string suffix = suffixIndex == 0 ? "" : suffixIndex.ToString();
                newExeDirectoryName = string.Format(@"{0}\{1}{2}", directoryName, exeRelativeDirectoryNameMostly, suffix);
                //!!!Two instances of this program could (it is possible) create the same directory.
                if (!Directory.Exists(newExeDirectoryName))
                {
                    Directory.CreateDirectory(newExeDirectoryName); 
                    exeRelativeDirectoryName = exeRelativeDirectoryNameMostly + suffix; 
                    break;
                } 
            }
        }



 
        public static string CopyExesToCluster(string directoryName, string niceName) 
        {
            Directory.CreateDirectory(directoryName); 
            string newExeDirectoryName;
            string exeNewRelativeDirectoryName;
            CreateNewExeDirectory(niceName, directoryName, out newExeDirectoryName, out exeNewRelativeDirectoryName);
            string oldExeDirectoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            SpecialFunctions.CopyDirectory(oldExeDirectoryName, newExeDirectoryName, true);
            return "\"" + exeNewRelativeDirectoryName + "\""; 
        } 

 

    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
