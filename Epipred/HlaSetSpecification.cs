using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using System.IO;
using VirusCount.Qmrr; 
 
namespace EpipredLib
{ 
    abstract public class HlaSetSpecification
    {


        internal HlaSetSpecification()
        { 
        } 

 
        static HlaSetSpecification _all = null;
        static public HlaSetSpecification All
        {
            get
            {
                if (null == _all) 
                { 
                    _all = new AllSpecification();
                } 
                return _all;
            }
        }

        static HlaSetSpecification _supertype = null;
        static public HlaSetSpecification Supertype 
        { 
            get
            { 
                if (null == _supertype)
                {
                    _supertype = new SupertypeSpecification();
                }
                return _supertype;
            } 
        } 

        static HlaSetSpecification _singleton = null; 
        static public HlaSetSpecification Singleton
        {
            get
            {
                if (null == _singleton)
                { 
                    _singleton = new SingletonSpecification(); 
                }
                return _singleton; 
            }
        }

        public static HlaSetSpecification GetInstance(string hlaSetName)
        {
            switch (hlaSetName) 
            { 
                case "all":
                    return All; 
                case "supertype":
                    return Supertype;
                case "singleton":
                    return Singleton;
                default:
                    SpecialFunctions.CheckCondition(false, "Don't understand HlaSetname " + hlaSetName); 
                    return null; 
            }
        } 

        public string Header()
        {
            return SpecialFunctions.CreateTabString(
                SpecialFunctions.CreateTabString2(InputHeaderCollection()),
                Prediction.ExtraHeader(IncludeHlaInOutput())); 
 
        }
 
        public abstract Set<Hla> HlaSet(string parameter, Set<Hla> hlaSet, Dictionary<string, Set<Hla>> supertypeMap);
        //public abstract string ExtraHeader();


        public abstract string[] InputHeaderCollection();
        public abstract bool IncludeHlaInOutput(); 
 
    }
 
    internal class AllSpecification : HlaSetSpecification
    {
        public override Set<Hla> HlaSet(string parameter, Set<Hla> hlaSet, Dictionary<string, Set<Hla>> supertypeMap)
        {
            SpecialFunctions.CheckCondition(null == parameter, "Parameter to HlaSetSpecification.All must be null");
            return hlaSet; 
        } 

 
        public override string[] InputHeaderCollection()
        {
            return new string[] { "InputPeptide" };
        }

        public override bool IncludeHlaInOutput() 
        { 
            return true;
        } 
    }
    class SupertypeSpecification : HlaSetSpecification
    {
        public override Set<Hla> HlaSet(string parameter, Set<Hla> hlaSet, Dictionary<string, Set<Hla>> supertypeMap)
        {
            if (supertypeMap.ContainsKey(parameter)) 
            { 
                return supertypeMap[parameter];
            } 
            return SingletonSpecification.HlaSetInternal(parameter, hlaSet, supertypeMap);

        }

        public override string ToString()
        { 
            return null; 
            //string s = SpecialFunctions.CreateTabString(InputPeptide, Parameter, Hla, PosteriorProbability, WeightOfEvidence, Peptide, K, StartPosition, LastPosition, Source);
            //return s; 
        }


        public override string[] InputHeaderCollection()
        {
            return new string[] { "InputPeptide", "Supertype" }; 
        } 

        public override bool IncludeHlaInOutput() 
        {
            return true;
        }
    }
    internal class SingletonSpecification : HlaSetSpecification
    { 
        static public HlaFactory HlaFactoryNoConstraints = HlaFactory.GetFactory("noConstraints"); 

        public override Set<Hla> HlaSet(string parameter, Set<Hla> hlaSet, Dictionary<string, Set<Hla>> supertypeMap) 
        {
            return HlaSetInternal(parameter, hlaSet, supertypeMap);
        }
        static public Set<Hla> HlaSetInternal(string parameter, Set<Hla> hlaSet, Dictionary<string, Set<Hla>> supertypeMap)
        {
            Hla hla = HlaFactoryNoConstraints.GetGroundInstance(parameter); 
            //SpecialFunctions.CheckCondition(hlaSet.Contains(hla), string.Format("Hla value of {0} is unknown", parameter)); 
            return Set<Hla>.GetInstance(hla);
        } 


        public override string[] InputHeaderCollection()
        {
            return new string[] { "InputPeptide", "Hla" };
        } 
 
        public override bool IncludeHlaInOutput()
        { 
            return false;
        }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
