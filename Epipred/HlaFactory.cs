using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using EpipredLib;
 
namespace VirusCount.Qmrr 
{
    abstract public class HlaFactory 
    {
        protected HlaFactory()
        {
        }

        public string Name; 
 
        static public HlaFactory GetFactory(string name)
        { 
            HlaFactory hlaFactory;
            //!!!switch to switch
            if (name == "noConstraints")
            {
                hlaFactory = new NoConstraints();
            } else if (name == "MixedWithB15AndA68") 
            { 
                hlaFactory = new MixedWithB15AndA68();
            } 
            else if (name == "FourDigit")
            {
                hlaFactory = new FourDigit();
            }
            else if (name == "JustSlash")
            { 
                hlaFactory = new JustSlash(); 
            }
            else 
            {
                SpecialFunctions.CheckCondition(false, "Don't know how to create HlaFactory " + name);
                hlaFactory = null;
            }
            hlaFactory.Name = name;
            return hlaFactory; 
        } 

        public abstract Hla GetGroundInstance(string name); 
        public abstract Hla GetGroundOrAbstractInstance(string name);
        public abstract bool IsGroundOrAbstractInstance(string name);
    }

    internal class NoConstraints : HlaFactory
    { 
        internal NoConstraints() 
        {
        } 

        override public Hla GetGroundInstance(string name)
        {
            return GetGroundOrAbstractInstance(name);
        }
 
 
        public override Hla GetGroundOrAbstractInstance(string name)
        { 
            Hla hla = new Hla();
            hla.Name = name;
            return hla;
        }

        public override bool IsGroundOrAbstractInstance(string name) 
        { 
            return true;
        } 


    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
