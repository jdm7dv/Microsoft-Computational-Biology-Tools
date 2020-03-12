using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{ 
    public class SingletonSet<T> 
    {
        private SingletonSet() 
        {
        }

        T element;
        string ErrorMessageFormatString;
 
        public bool IsEmpty{get; private set;} 

        static public SingletonSet<T> GetInstance(string errorMessageFormatString) 
        {
            SingletonSet<T> singletonSet = new SingletonSet<T>();
            singletonSet.IsEmpty = true;
            singletonSet.ErrorMessageFormatString = errorMessageFormatString;
            return singletonSet;
        } 
 
        public void Add(T t)
        { 
            if (IsEmpty)
            {
                element = t;
                IsEmpty = false;
            }
            else 
            { 
                SpecialFunctions.CheckCondition(element.Equals(t), string.Format(ErrorMessageFormatString, element, t));
            } 
        }

        public T First()
        {
            SpecialFunctions.CheckCondition(!IsEmpty, "Must have an elememnt");
            return element; 
        } 
    }
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
