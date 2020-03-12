using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Msr.Mlas.SpecialFunctions;
using EpipredLib; 
 
namespace VirusCount.Qmrr
{ 
    public class MixedWithB15AndA68 : HlaFactory
    {

        //A similar pattern appears in HlaResolution.cs
        static private Regex MixedWithB15AndA68GroundRegex = new Regex(@"^(([ABC][0-9][0-9])|(B15[0-9][0-9])|(A68[0-9][0-9])|(D.*))$");
        static private Regex MixedWithB15AndA68AbstractRegex = new Regex(@"^((B15\?\?)|(C\?\?)|(A68\?\?))$"); 
 
        public override bool IsGroundOrAbstractInstance(string name)
        { 
            return MixedWithB15AndA68GroundRegex.IsMatch(name) || MixedWithB15AndA68AbstractRegex.IsMatch(name);
        }



        internal MixedWithB15AndA68() 
        { 
        }
 


        override public Hla GetGroundInstance(string name)
        {
            SpecialFunctions.CheckCondition(MixedWithB15AndA68GroundRegex.IsMatch(name), string.Format("HLA {0} doesn't match the pattern ({1})", name, MixedWithB15AndA68GroundRegex.ToString()));
            SpecialFunctions.CheckCondition(name != "B15" && name != "A68", string.Format("HLA {0} should not equal 'B15' or 'A68'", name)); 
            HlaAbstractionOKType1 hla = new HlaAbstractionOKType1(); 
            hla.Name = name;
            hla._isGround = true; 
            return hla;
        }

        public Hla GetAbstractInstance(string name)
        {
            SpecialFunctions.CheckCondition(MixedWithB15AndA68AbstractRegex.IsMatch(name)); 
            HlaAbstractionOKType1 hla = new HlaAbstractionOKType1(); 
            hla.Name = name;
            hla._isGround = false; 
            return hla;
        }

        public override Hla GetGroundOrAbstractInstance(string name)
        {
            if (MixedWithB15AndA68AbstractRegex.IsMatch(name)) 
            { 
                return GetAbstractInstance(name);
            } 
            else
            {
                return GetGroundInstance(name);
            }
        }
 
    } 

    /// <summary> 
    /// The groundness property must be a function of name, so that hashing and equality can be based just on the name.
    /// </summary>
    public class HlaAbstractionOKType1 : Hla
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
            Debug.Assert(hla.Name.Length != 0 && Name.Length != 0);
            if (Name[0] != hla.Name[0])
            {
                return false;
            }
 
            string otherRest = hla.Name.Substring(1); 
            foreach(string rest in Name.Substring(1).Split('/'))
            { 
                bool thisMatch = true;
                foreach (KeyValuePair<char, char> abstractCharAndGroundChar in SpecialFunctions.EnumerateTwo(rest, otherRest))
                {
                    char abstractChar = abstractCharAndGroundChar.Key;
                    char groundChar = abstractCharAndGroundChar.Value;
                    if (abstractChar != '?') 
                    { 
                        if (abstractChar != groundChar)
                        { 
                            thisMatch = false;
                            break;
                        }
                    }
                }
 
                if (thisMatch) 
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
