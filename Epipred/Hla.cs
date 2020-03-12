using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;

namespace EpipredLib 
{ 
    public class Hla
    { 
        internal Hla()
        {
        }


        internal string Name; 
        virtual public bool IsGround 
        {
            get 
            {
                SpecialFunctions.CheckCondition(false, "Not implemented");
                return true;
            }
        }
 
        public override string ToString() 
        {
            return Name; 
        }

        public override int GetHashCode()
        {
            return "VirusCount.Hla".GetHashCode() ^ Name.GetHashCode();
        } 
 
        public override bool Equals(object obj)
        { 
            Hla other = obj as Hla;
            if (null == other)
            {
                return false;
            }
            else 
            { 
                return this == other;
            } 
        }

        static public bool operator ==(Hla hla1, Hla hla2)
        {
            if ((object)hla1 == null)
            { 
                return ((object) hla2) == null; 
            }
            if ((object) hla2 == null) 
            {
                return false;
            }
            return hla1.Name == hla2.Name;
        }
        static public bool operator !=(Hla hla1, Hla hla2) 
        { 
            return !(hla1 == hla2);
        } 

        public virtual bool IsMoreGeneralThan(Hla possibleCause)
        {
            throw new Exception("The method or operation is not implemented.");
        }
 
    } 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
