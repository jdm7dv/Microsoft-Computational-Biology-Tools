using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;

namespace Msr.Mlas.SpecialFunctions 
{ 
    public class Hla
    { 
        protected Hla()
        {
        }


        public string Name; 
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

        static int hlaStringHashCode = (int)MachineInvariantRandom.GetSeedUInt("Hla");

        public override int GetHashCode()
        { 
            return hlaStringHashCode ^ Name.GetHashCode(); 
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

        public virtual bool IsMoreGeneralThan(Hla hla)
        {
            throw new Exception("The method or operation is not implemented."); 
        } 

    } 
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
