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

        public T ExtractOptional<T>(string flag, T defaultValue)
        {
            int argIndex = FindFlag(flag);

            if (argIndex == -1) 
            { 
                return defaultValue;
            } 

            _argList.RemoveAt(argIndex);
            SpecialFunctions.CheckCondition(argIndex < _argList.Count, string.Format(@"Expect an value after ""{0}""", flag));
            T t;
            SpecialFunctions.CheckCondition(SpecialFunctions.TryParse(_argList[argIndex], out t), string.Format(@"Expect value after ""{0}"" to be an {1}. Read {2}", flag, typeof(T), _argList[argIndex]));
            _argList.RemoveAt(argIndex); 
            return t; 

        } 


        public void CheckThatEmpty()
        {
            SpecialFunctions.CheckCondition(_argList.Count == 0, string.Format(@"Unknown arguments found. Check the spelling of flags. {0}", SpecialFunctions.Join(" ", _argList)));
        } 
 

 
        public T ExtractNext<T>(string argumentName)
        {
            SpecialFunctions.CheckCondition(_argList.Count > 0, string.Format(@"Expect ""{0}"" value", argumentName));
            T t;
            SpecialFunctions.CheckCondition(SpecialFunctions.TryParse(_argList[0], out t), string.Format(@"Expect value for ""{0}"" to be a {1}. Read {2}", argumentName, typeof(T), _argList[0]));
            _argList.RemoveAt(0); 
            return t; 
        }
 
        public int Count
        {
            get
            {
                return _argList.Count;
            } 
        } 

        public void CheckNoMoreOptions() 
        {
            //!!! hack. Want to find flags, but -1-9 isn't a flag, it's a range. So for now, look only for flags that don't start
            //with a number.
            Regex regex = new Regex(@"^-[\D]");
            foreach (string arg in _argList)
            { 
                //SpecialFunctions.CheckCondition(!arg.StartsWith("-"), string.Format(@"Unknown option found, {0}", arg)); 
                SpecialFunctions.CheckCondition(!arg.StartsWith("/"), string.Format(@"Unknown option found, {0}", arg));
                SpecialFunctions.CheckCondition(!regex.IsMatch(arg),  string.Format(@"Unknown option found, {0}", arg)); 
            }
        }

		private int FindFlag(string flag)
		{
			int		argIndex = SpecialFunctions.IgnoreCaseIndexOf(_argList, "/" + flag); 
			return  argIndex != -1 ? argIndex : SpecialFunctions.IgnoreCaseIndexOf(_argList, "-" + flag); 
		}
 
        public bool ContainsOptionalFlag(string flag)
        {
            return FindFlag(flag) > -1;
        }

    } 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
