using System; 
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Msr.Mlas.SpecialFunctions
{ 
 	//!!!should this have a finalizer that will raise an error if there are arguments left? Then it would be used in a Using statement 
	//!!!would a better arg parse return a struct or class of the arguments maybe c# 3.0 anonymous struct. Then parsing could be done in a method with just one value returned.
	public class ArgCollection 
	{
		private ArgCollection()
		{
 		}

 		static public ArgCollection GetInstance(string args) 
		{ 
 			return GetInstance(args.Split(new char[]{' ', '\t'}, StringSplitOptions.RemoveEmptyEntries));
		} 

		static public ArgCollection GetInstance(string[] args)
		{
			ArgCollection argCollection = new ArgCollection();
			argCollection._argList = new List<string>(args);
 			return argCollection; 
 		} 

		private List<string> _argList; 

 		public bool ExtractOptionalFlag(string flag)
		{
			int argIndex = FindFlag(flag);

			if (argIndex == -1) 
			{ 
				return false;
 			} 

 			_argList.RemoveAt(argIndex);
			return true;

 		}
 
		public T PeekOptional<T>(string flag, T defaultValue) 
		{
			return ExtractOptionalInternal<T>(flag, defaultValue, false); 
		}

		public T ExtractOptional<T>(string flag, T defaultValue)
 		{
 			return ExtractOptionalInternal<T>(flag, defaultValue, true);
		} 
 
 		private T ExtractOptionalInternal<T>(string flag, T defaultValue, bool removeFlagAndValue)
		{ 
			int argIndex = FindFlag(flag);

			if (argIndex == -1)
			{
				return defaultValue;
 			} 
 
 			SpecialFunctions.CheckCondition(argIndex < _argList.Count-1, string.Format(@"Expect a value after ""{0}""", flag));
			T t; 
 			SpecialFunctions.CheckCondition(Parser.TryParse(_argList[argIndex+1], out t), string.Format(@"Expect value after ""{0}"" to be an {1}. Read {2}", flag, typeof(T), _argList[argIndex]));
			
			if (removeFlagAndValue)
			{
				_argList.RemoveAt(argIndex);
				_argList.RemoveAt(argIndex); 
 			} 

 			return t; 

		}


 		public void CheckThatEmpty()
		{ 
            SpecialFunctions.CheckCondition(_argList.Count == 0, string.Format(@"Unknown arguments found. Check the spelling of flags. {0}", _argList.StringJoin(" "))); 
		}
 


		public T ExtractAt<T>(string argumentName, int argPosition)
		{
			SpecialFunctions.CheckCondition(_argList.Count > argPosition, string.Format("Expect {0} at position {1}. Only {2} arguments remain to be parsed.", argumentName, argPosition, Count));
 			T t; 
 			SpecialFunctions.CheckCondition(Parser.TryParse(_argList[argPosition], out t), string.Format(@"Expect value for ""{0}"" to be a {1}. Read {2}", argumentName, typeof(T), _argList[0])); 
			_argList.RemoveAt(argPosition);
 			return t; 
		}

		public T ExtractNext<T>(string argumentName)
		{
			SpecialFunctions.CheckCondition(_argList.Count > 0, string.Format(@"Expect ""{0}"" value", argumentName));
			return ExtractAt<T>(argumentName, 0); 
 		} 

 		public int Count 
		{
 			get
			{
				return _argList.Count;
			}
		} 
 
		public void CheckNoMoreOptions(int? numberOfRequiredArgumentsOrNull)
 		{ 
 			//!!! hack. Want to find flags, but -1-9 isn't a flag, it's a range. So for now, look only for flags that don't start
			//with a number.
 			Regex regex = new Regex(@"^-[,\D]");
			foreach (string arg in _argList)
			{
				//SpecialFunctions.CheckCondition(!arg.StartsWith("-"), string.Format(@"Unknown option found, {0}", arg)); 
				SpecialFunctions.CheckCondition(!arg.StartsWith("/"), string.Format(@"Unknown option found, {0}", arg)); 
				SpecialFunctions.CheckCondition(!regex.IsMatch(arg),  string.Format(@"Unknown option found, {0}", arg));
 			} 

 			if (null != numberOfRequiredArgumentsOrNull)
			{
 				SpecialFunctions.CheckCondition(_argList.Count == (int)numberOfRequiredArgumentsOrNull,
					string.Format("Expected {0} required arguments, but there are {1}", numberOfRequiredArgumentsOrNull, _argList.Count));
			} 
		} 

		public int FindFlag(string flag) 
		{
 			int	argIndex;


            foreach (string delmin in new string[] { "/", /*a windows dash*/ "\xFB", /*every unicode hyphen*/ "\u00AD", "\u002D", "\u2010", "\u2011", "\u2012", "\u2013", "\u2014", "\u2015", "\u2212" })
            { 
                argIndex = SpecialFunctions.IgnoreCaseIndexOf(_argList, delmin + flag); 
                if (argIndex > -1)
                { 
                    return argIndex;
                }
            }

            return -1;
 		} 
 
		public bool ContainsOptionalFlag(string flag)
 		{ 
			return FindFlag(flag) > -1;
		}

		public string[] GetUnderlyingArray()
		{
			return _argList.ToArray(); 
 		} 
 	}
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
