using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Msr.Mlas.SpecialFunctions;
using VirusCount.Qmrr; 
 
namespace EpipredLib
{ 
    public class JustSlash : HlaFactory
    {

        //A similar pattern appears in HlaResolution.cs
        static private Regex JustSlashGroundRegex = new Regex(@"^[^/]+$");
        static private Regex JustSlashAbstractOrGroundRegex = new Regex(@"^.+$"); 
        internal JustSlash() 
        {
        } 

        public override bool IsGroundOrAbstractInstance(string name)
        {
            return JustSlashAbstractOrGroundRegex.IsMatch(name);
        }
 
 
        //!!!some of the methods are very similar to MixedWithB15AndA68 and FourDigit
        override public Hla GetGroundInstance(string name) 
        {
            SpecialFunctions.CheckCondition(JustSlashGroundRegex.IsMatch(name), string.Format("Hla ({0}) is not of legal form", name));
            HlaAbstractionOKTypeJustSlash hla = new HlaAbstractionOKTypeJustSlash();
            hla.Name = name;
            hla._isGround = true;
            return hla; 
        } 

        public Hla GetAbstractInstance(string name) 
        {
            SpecialFunctions.CheckCondition(JustSlashAbstractOrGroundRegex.IsMatch(name) && !JustSlashGroundRegex.IsMatch(name), string.Format("Hla ({0}) is not of legal form", name));
            HlaAbstractionOKTypeJustSlash hla = new HlaAbstractionOKTypeJustSlash();
            hla.Name = name;
            hla._isGround = false;
            return hla; 
        } 

        public override Hla GetGroundOrAbstractInstance(string name) 
        {
            if (JustSlashGroundRegex.IsMatch(name))
            {
                return GetGroundInstance(name);
            }
            else 
            { 
                return GetAbstractInstance(name);
            } 
        }

    }


    /// <summary> 
    /// The groundness property must be a function of name, so that hashing and equality can be based just on the name. 
    /// </summary>
    public class HlaAbstractionOKTypeJustSlash : Hla 
    {
        internal bool _isGround;
        public override bool IsGround
        {
            get
            { 
                return _isGround; 
            }
        } 

        public override bool IsMoreGeneralThan(Hla hla)
        {
            Debug.Assert(!IsGround); // real assert
            SpecialFunctions.CheckCondition(hla.IsGround); //We only have code for this
 
            foreach (string subHlaName in Name.Split('/')) 
            {
                if (subHlaName == hla.Name) 
                {
                    return true;
                }
            }
            return false;
        } 
    } 

 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
