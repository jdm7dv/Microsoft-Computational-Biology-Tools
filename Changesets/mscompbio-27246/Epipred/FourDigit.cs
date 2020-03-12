using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Msr.Mlas.SpecialFunctions;
using EpipredLib; 
 
namespace VirusCount.Qmrr
{ 
    public class FourDigit : HlaFactory
    {

        //A similar pattern appears in HlaResolution.cs
        static private Regex FourDigitGroundRegex = new Regex(@"^[ABC][0-9@][0-9@][0-9@][0-9@]$");
        static private Regex FourDigitAbstractOrGroundRegex = new Regex(@"^[ABC](([0-9@][0-9@][0-9@][0-9@])|([0-9@][0-9@][?][?])|([?][?][?][?]))(/(([0-9@][0-9@][0-9@][0-9@])|([0-9@][0-9@][?][?])|([?][?][?][?])))*$"); 
        internal FourDigit() 
        {
        } 

        //!!!some of the methods are very similar to MixedWithB15AndA68
        override public Hla GetGroundInstance(string name)
        {
            SpecialFunctions.CheckCondition(FourDigitGroundRegex.IsMatch(name), string.Format("Hla ({0}) is not of legal form", name));
            HlaAbstractionOKType1 hla = new HlaAbstractionOKType1(); 
            hla.Name = name; 
            hla._isGround = true;
            return hla; 
        }

        public Hla GetAbstractInstance(string name)
        {
            SpecialFunctions.CheckCondition(FourDigitAbstractOrGroundRegex.IsMatch(name) && !FourDigitGroundRegex.IsMatch(name), string.Format("Hla ({0}) is not of legal form", name));
            HlaAbstractionOKType1 hla = new HlaAbstractionOKType1(); 
            hla.Name = name; 
            hla._isGround = false;
            return hla; 
        }

        public override Hla GetGroundOrAbstractInstance(string name)
        {
            if (FourDigitGroundRegex.IsMatch(name))
            { 
                return GetGroundInstance(name); 
            }
            else 
            {
                return GetAbstractInstance(name);
            }
        }

 
        public override bool IsGroundOrAbstractInstance(string name) 
        {
            return FourDigitAbstractOrGroundRegex.IsMatch(name); 
        }
    }

}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
