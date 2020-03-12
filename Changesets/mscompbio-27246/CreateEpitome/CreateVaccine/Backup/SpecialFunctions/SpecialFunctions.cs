using System; 
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection; 
using System.Text.RegularExpressions; 
using System.Threading;
using System.Security.Cryptography;
using System.Linq.Expressions;
using System.Linq;

namespace Msr.Mlas.SpecialFunctions 
{ 
    public class SpecialFunctions
    { 


        //!!!not a special math function
        public static void CheckCondition(bool condition)
        {
            CheckCondition(condition, "A condition failed."); 
        } 

        //!!!not a special math function 

        /// <summary>
        /// Warning: The message with be evaluated even if the condition is true, so don't make it's calculation slow.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param> 
        public static void CheckCondition(bool condition, string message) 
        {
            if (!condition) 
            {
                throw new Exception(message);
            }
        }

        public static void CheckCondition(bool condition, string messageToFormat, params object[] formatValues) 
        { 
            if (!condition)
            { 
                throw new Exception(string.Format(messageToFormat, formatValues));
            }
        }


 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, bool includeWholeLine, out string header) 
        {
            header = ReadLine(filename); 
            return TabFileTable(null, null, filename, header, includeWholeLine, '\t', true);
        }

        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, bool includeWholeLine, char separator, out string header)
        {
            header = ReadLine(filename); 
            return TabFileTable(null, null, filename, header, includeWholeLine, separator, true); 
        }
 


        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, char separator)
        {
            string header = ReadLine(filename);
            return TabFileTable(null, null, filename, header, false, separator, true); 
        } 

        /// <summary> 
        /// Method to read delimited, formatted file. Need not be delimited by tabs as suggested by the name. Delimiter is implied
        ///by the way the header parameter is delimited, and by the param 'separator'. See also ReadDelimitedFile which doesn't require
        ///the output to be parsed into proper data type, but which is slower and it's output arguments cannot be passed on.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="header">e.g. @"name, start, end, region, startLocal, endLocal, startAdjusted, endAdjusted"</param> 
        /// <param name="separator">","</param> 
        /// <returns></returns>
        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, string header, char separator) 
        {
            return TabFileTable(null, null, filename, header, false, separator, true);
        }
        public static string ReadLine(string filename)
        {
            using (StreamReader streamReader = File.OpenText(filename)) 
            { 
                return streamReader.ReadLine();
            } 
        }

        //
        /// <summary>
        /// iterates through all files in a directory that match the given pattern. If checkHeader, each file must have the same header that
        /// matches the given header. 
        /// </summary> 
        static public IEnumerable<Dictionary<string, string>> TabDirectoryTable(string directoryName, string filePattern,
            string header, bool includeWholeLine, bool checkHeaderMatch) 
        {
            foreach (string filename in Directory.GetFiles(directoryName, filePattern))
            {
                //Console.WriteLine("SpecialFunction.TabDirectoryTable: opening file " + new FileInfo(filename).Name);
                foreach (Dictionary<string, string> row in TabFileTable(filename, header, includeWholeLine, checkHeaderMatch))
                { 
                    yield return row; 
                }
            } 
        }

        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, string header, bool includeWholeLine)
        {
            return TabFileTable(null, null, filename, header, includeWholeLine, '\t', true);
        } 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(string filename, string header, bool includeWholeLine, bool checkHeaderMatch) 
        {
            return TabFileTable(null, null, filename, header, includeWholeLine, '\t', checkHeaderMatch); 
        }
        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string header, bool includeWholeLine)
        {
            return TabFileTable(textReader, "STREAM", header, includeWholeLine, '\t', true);
        }
        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string header, bool includeWholeLine, bool checkHeaderMatch) 
        { 
            return TabFileTable(textReader, "STREAM", header, includeWholeLine, '\t', checkHeaderMatch, false);
        } 

        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string header, bool includeWholeLine, bool checkHeaderMatch, bool includeHeaderAsFirstLine)
        {
            return TabFileTable(textReader, "STREAM", header, includeWholeLine, '\t', checkHeaderMatch, includeHeaderAsFirstLine);
        }
 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(Assembly assembly, string resourcePrefix, string filename, 
            string header, bool includeWholeLine, char separator, bool checkHeaderMatch)
        { 
            using (TextReader textReader = File.OpenText(filename))
            {
                foreach (Dictionary<string, string> row in TabFileTable(textReader, filename, header, includeWholeLine, separator, checkHeaderMatch))
                {
                    yield return row;
                } 
            } 
        }
 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string inputName,
            string header, bool includeWholeLine, char separator, bool checkHeaderMatch)
        {
            return TabFileTable(textReader, inputName, header, includeWholeLine, separator, checkHeaderMatch, false);
        }
 
        static public IEnumerable<Dictionary<string, string>> TabFileTable(TextReader textReader, string inputName, 
            string header, bool includeWholeLine, char separator, bool checkHeaderMatch, bool includeHeaderAsFirstLine)
        { 

            string line = textReader.ReadLine();
            SpecialFunctions.CheckCondition(null != line, "Input is empty so can't read header");

            if (checkHeaderMatch)
            { 
                SpecialFunctions.CheckCondition(line.Equals(header, StringComparison.CurrentCultureIgnoreCase), string.Format("The input doesn't have the exact expected header.\nEXPECTED:\n {0}\nOBSERVED:\n{1}\nINPUT NAME:\n{2}", header, line, inputName)); //!!!raise error 
            }
            else 
            {
                header = line;
            }

            string[] headerCollection = header.Split(separator);
            //while (null != (line = textReader.ReadLine())) 
            bool firstTime = true; 

            // use do-while so we can return header as the first row, if requested. 
            do
            {
                if (firstTime && !includeHeaderAsFirstLine)
                {
                    firstTime = false;
                    continue; 
                } 

                if (line.Length == 0) continue; 

                string[] fieldCollection = line.Split(separator);
                SpecialFunctions.CheckCondition(!checkHeaderMatch || fieldCollection.Length == headerCollection.Length,
                    string.Format("The input doesn't have the expected number of columns. Header Length:{0}, LineLength:{1}\nHeader:{2}\nLine:{3}\nInputName:{4}",
                    headerCollection.Length, fieldCollection.Length, header, line, inputName)); //!!!raise error
 
                // if we're not checking for a header match, we still can't deal with lines of the wrong length. just ignore them. 
                if (fieldCollection.Length != headerCollection.Length)
                    continue; 

                Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                if (includeWholeLine)
                {
                    row.Add("", line);
                } 
                for (int iField = 0; iField < fieldCollection.Length; ++iField) 
                {
                    if (headerCollection[iField] == "") 
                    {
                        SpecialFunctions.CheckCondition(fieldCollection[iField] == "");
                    }
                    else
                    {
                        if (!row.ContainsKey(headerCollection[iField])) 
                        { 
                            row.Add(headerCollection[iField], fieldCollection[iField]);
                        } 
                        else
                        {
                            if (row[headerCollection[iField]] != fieldCollection[iField])
                            {
                                try
                                { 
                                    double r1 = double.Parse(row[headerCollection[iField]]); 
                                    double r2 = double.Parse(fieldCollection[iField]);
                                    Debug.Assert(Math.Abs(r1 - r2) < .000000001); 
                                }
                                finally
                                {
                                }
                            }
                        } 
                    } 
                }
                //Dictionary<string, string> row = new Dictionary<string, string>(rowX, StringComparer.CurrentCultureIgnoreCase); 
                yield return row;

            } while (null != (line = textReader.ReadLine()));
        }
        //General
        public static List<Dictionary<string, string>> TabFileTableAsList(Assembly assembly, string resourcePrefix, string filename, string header, bool includeWholeLine) 
        { 
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            foreach (Dictionary<string, string> row in TabFileTable(assembly, resourcePrefix, filename, header, includeWholeLine, '\t', true)) 
            {
                list.Add(row);
            }
            return list;
        }
 
 
        public static bool IgnoreCaseContains(IList<string> stringList, string item)
        { 
            return IgnoreCaseIndexOf(stringList, item) >= 0;
        }

        internal static int IgnoreCaseIndexOf(IList<string> stringList, string item)
        {
            for (int i = 0; i < stringList.Count; ++i) 
            { 
                string s = stringList[i];
                if (s.Equals(item, StringComparison.CurrentCultureIgnoreCase)) 
                {
                    return i;
                }
            }
            return -1;
        } 
 

        public static void CreateDirectoryForFileIfNeeded(string fileName) 
        {
            string outputDirectoryName = Path.GetDirectoryName(fileName);
            if ("" != outputDirectoryName)
            {
                Directory.CreateDirectory(outputDirectoryName);
            } 
        } 

        //!!!not a special math function 
        public static string CreateTabString(params object[] objectCollection)
        {
            return objectCollection.StringJoin("\t");
        }

 
    } 

    public delegate bool Predicate<T1, T2>(T1 t1, T2 t2); 


}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
