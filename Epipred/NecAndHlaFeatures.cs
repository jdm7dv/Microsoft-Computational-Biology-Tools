using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using ProcessingPrediction;
using VirusCount; 
using EpipredLib; 
using VirusCount.Qmrr;
using System.IO; 
using Msr.Adapt.LearningWorkbench;
using System.Xml.Serialization;
using System.Reflection;
using Msr.Adapt.HighLevelFeatures;

namespace NecAndHlaFeatures 
{ 
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class IsHla : EntityFeature, IHashableFeature
    {

        public IsHla()
        {
        } 
 
        public static IsHla GetInstance(Hla hla)
        { 
            IsHla isHla = new IsHla();
            isHla.Hla = hla.ToString();
            return isHla;
        }

        [XmlAttribute("hla")] 
        public string Hla = ""; 

 

        public bool FeatureFunction(Pair<NEC, Hla> necAndHla)
        {
            bool b = Hla == necAndHla.Second.ToString();
            return b;
        } 
 
        public override int GetHashCode()
        { 
            return Hla.GetHashCode() ^ "IsHla".GetHashCode(); //The constant string is to distinish this class from a string
        }

        public override bool Equals(object obj)
        {
            IsHla other = obj as IsHla; 
            if (other == null) 
            {
                return false; 
            }
            else
            {
                return Hla.Equals(other.Hla);
            }
        } 
 
        public override string ToString()
        { 
            return Hla.ToString();
        }

    }

    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class IsHla24 : EntityFeature, IHashableFeature
    { 

        public IsHla24()
        {
        }

        public static IsHla24 GetInstance(Hla hla) 
        { 
            IsHla24 isHla24 = new IsHla24();
            isHla24.Hla24 = FindHla24(hla); 
            return isHla24;
        }

        private static string FindHla24(Hla hla)
        {
            string hlaName = hla.ToString(); 
            hlaName = hlaName.Replace("*", ""); 
            if (hlaName.StartsWith("HLA"))
            { 
                hlaName = hlaName.Substring(3);
            }
            if (hlaName.StartsWith(" ") || hlaName.StartsWith("-"))
            {
                hlaName = hlaName.Substring(1);
            } 
            SpecialFunctions.CheckCondition(hlaName.Length == 5, "Expect hla to contain a class letter and a 4-digit number. " + hla.ToString()); 
            SpecialFunctions.CheckCondition(hlaName[0] == 'A' || hlaName[0] == 'B' || hlaName[0] == 'C', "Expect hla to contain a class letter: A,B, or C. " + hla.ToString());
            int ignore; 
            SpecialFunctions.CheckCondition(int.TryParse(hlaName.Substring(1), out ignore), "Expect hla to contain a 4-digit number. " + hla.ToString());

            string hla24;
            if (hlaName.StartsWith("A68") || hlaName.StartsWith("B15"))
            {
                hla24 = hlaName; 
            } 
            else
            { 
                hla24 = hlaName.Substring(0, 3);
            }
            return hla24;
        }

        [XmlAttribute("hla24")] 
        public string Hla24 = ""; 

 

        public bool FeatureFunction(Pair<NEC, Hla> necAndHla)
        {
            bool b = Hla24 == FindHla24(necAndHla.Second);
            return b;
        } 
 
        public override int GetHashCode()
        { 
            return Hla24.GetHashCode() ^ "IsHla24".GetHashCode(); //The constant string is to distinish this class from a string
        }

        public override bool Equals(object obj)
        {
            IsHla24 other = obj as IsHla24; 
            if (other == null) 
            {
                return false; 
            }
            else
            {
                return Hla24.Equals(other.Hla24);
            }
        } 
 
        public override string ToString()
        { 
            return string.Format("Hla24={0}", Hla24);
        }

    }

    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class IsAA : Feature, IHashableFeature
    { 

        public IsAA()
        {
        }

        internal static IsAA GetInstance(string aminoAcid, IHashableFeature featureE) 
        { 
            IsAA isAA = new IsAA();
            isAA.AminoAcid = aminoAcid; 
            if (featureE != null)
            {
                isAA.FeatureCollection = new Feature[] { (Feature)featureE };
            }
            return isAA;
        } 
 
        [XmlAttribute("aa")]
        public string AminoAcid = ""; 



        public bool FeatureFunction(AminoAcidInternal aminoAcidInternal)
        {
            bool b = aminoAcidInternal.AminoAcidAsString == AminoAcid; 
            return b; 
        }
 
        public override int GetHashCode()
        {
            return "aa".GetHashCode() ^ AminoAcid.GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode());
        }

        public override bool Equals(object obj) 
        { 
            IsAA other = obj as IsAA;
            if (other == null) 
            {
                return false;
            }
            else
            {
                if (AminoAcid != other.AminoAcid || FeatureCollection.Length != other.FeatureCollection.Length) 
                { 
                    return false;
                } 
                if (FeatureCollection.Length > 0)
                {
                    return FeatureCollection[0].Equals(other.FeatureCollection[0]);
                }
                return true;
            } 
        } 

        public override string ToString() 
        {
            if (FeatureCollection.Length == 0)
            {
                return "";
            }
            return string.Format("{0}={1}", FeatureCollection[0], AminoAcid); 
        } 

 

    }
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class E : EntityFeature, IHashableFeature
    { 
        public E() 
        {
        } 

        [XmlAttribute("pos")]
        public int Pos;

        public AminoAcidInternal FeatureFunction(Pair<NEC, Hla> necAndHla)
        { 
            char chAminoAcid = necAndHla.First.E[Pos - 1]; 
            AminoAcidInternal aAminoAcidInternal = AminoAcidInternal.GetInstance(chAminoAcid);
            return aAminoAcidInternal; 
        }

        internal static E GetInstance(int pos)
        {
            SpecialFunctions.CheckCondition(pos > 0, "pos must be 1 or greater");
            E e = new E(); 
            e.Pos = pos; 
            return e;
        } 

        public override int GetHashCode()
        {
            return Pos.GetHashCode() ^ "E".GetHashCode();
        }
 
        public override bool Equals(object obj) 
        {
            E other = obj as E; 
            if (other == null)
            {
                return false;
            }
            else
            { 
                return Pos == other.Pos; 
            }
        } 

        public override string ToString()
        {
            return string.Format("E{0}", Pos);
        }
 
 
    }
 
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class HasAAProp : Feature, IHashableFeature
    {
        public HasAAProp()
        { 
        } 

        internal static HasAAProp GetInstance(string property, IHashableFeature featureE) 
        {
            HasAAProp hasAAProp = new HasAAProp();
            hasAAProp.Prop = property;
            if (featureE != null)
            {
                hasAAProp.FeatureCollection = new Feature[] { (Feature)featureE }; 
            } 
            return hasAAProp;
        } 

        [XmlAttribute("prop")]
        public string Prop = "";

        public bool FeatureFunction(AminoAcidInternal aminoAcidInternal)
        { 
            bool b = VirusCount.KmerProperties.GetInstance().DoesAminoAcidHaveProperty(aminoAcidInternal.AminoAcidAsString, Prop); 
            return b;
        } 


        public override int GetHashCode()
        {
            return Prop.GetHashCode() ^ "HasAAProp".GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode());
        } 
 
        public override bool Equals(object obj)
        { 
            HasAAProp other = obj as HasAAProp;
            if (other == null)
            {
                return false;
            }
            else 
            { 
                if (Prop != other.Prop || FeatureCollection.Length != other.FeatureCollection.Length)
                { 
                    return false;
                }
                if (FeatureCollection.Length > 0)
                {
                    return FeatureCollection[0].Equals(other.FeatureCollection[0]);
                } 
                return true; 
            }
        } 

        public override string ToString()
        {
            if (FeatureCollection.Length == 0)
            {
                return ""; 
            } 
            return string.Format("{0}({1})", Prop, FeatureCollection[0]);
        } 



    }

    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class IsZero6Supertype : EntityFeature, IHashableFeature
    { 

        public IsZero6Supertype()
        {
        }

        internal static IsZero6Supertype GetInstance(Hla hla) 
        { 
            IsZero6Supertype isZero6Supertype = new IsZero6Supertype();
            isZero6Supertype.Zero6Supertype = VirusCount.HlaToLength.ToZero6SupertypeBlanksString(hla.ToString()); 
            return isZero6Supertype;
        }

        [XmlAttribute("zero6Supertype")]
        public string Zero6Supertype = "";
 
        public bool FeatureFunction(Pair<NEC, Hla> necAndHla) 
        {
            bool b = Zero6Supertype == VirusCount.HlaToLength.ToZero6SupertypeBlanksString(necAndHla.Second.ToString()); 
            return b;
        }


        public override int GetHashCode()
        { 
            return Zero6Supertype.GetHashCode() ^ "IsZero6Supertype".GetHashCode(); 
        }
 
        public override bool Equals(object obj)
        {
            IsZero6Supertype other = obj as IsZero6Supertype;
            if (null == other)
            {
                return false; 
            } 
            else
            { 
                return Zero6Supertype.Equals(other.Zero6Supertype);
            }
        }

        public override string ToString()
        { 
            return string.Format("Supertype={0}", Zero6Supertype); 
        }
 


    }

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class IsZero6SupertypeNoBlanks : EntityFeature, IHashableFeature 
    {
 
        public IsZero6SupertypeNoBlanks()
        {
        }

        internal static IsZero6SupertypeNoBlanks GetInstance(Hla hla)
        { 
            IsZero6SupertypeNoBlanks isZero6SupertypeNoBlanks = new IsZero6SupertypeNoBlanks(); 
            isZero6SupertypeNoBlanks.Zero6SupertypeNoBlanks = VirusCount.HlaToLength.ToZero6SupertypeNoBlanksString(hla.ToString());
            return isZero6SupertypeNoBlanks; 
        }

        [XmlAttribute("zero6SupertypeNoBlanks")]
        public string Zero6SupertypeNoBlanks = "";

        public bool FeatureFunction(Pair<NEC, Hla> necAndHla) 
        { 
            bool b = Zero6SupertypeNoBlanks == VirusCount.HlaToLength.ToZero6SupertypeNoBlanksString(necAndHla.Second.ToString());
            return b; 
        }


        public override int GetHashCode()
        {
            return Zero6SupertypeNoBlanks.GetHashCode() ^ "IsZero6SupertypeNoBlanks".GetHashCode(); 
        } 

        public override bool Equals(object obj) 
        {
            IsZero6SupertypeNoBlanks other = obj as IsZero6SupertypeNoBlanks;
            if (null == other)
            {
                return false;
            } 
            else 
            {
                return Zero6SupertypeNoBlanks.Equals(other.Zero6SupertypeNoBlanks); 
            }
        }

        public override string ToString()
        {
            return string.Format("SupertypeNoBlanks={0}", Zero6SupertypeNoBlanks); 
        } 

 

    }


    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class IsSupertypeFromWorkingDirectory : EntityFeature, IHashableFeature 
    {
 
        public IsSupertypeFromWorkingDirectory()
        {
        }

        internal static IsSupertypeFromWorkingDirectory GetInstance(Hla hla)
        { 
            IsSupertypeFromWorkingDirectory isSupertypeFromWorkingDirectory = new IsSupertypeFromWorkingDirectory(); 
            isSupertypeFromWorkingDirectory.SupertypeFromWorkingDirectory = VirusCount.HlaToLength.ToSupertypeFromWorkingDirectoryString(hla.ToString());
            return isSupertypeFromWorkingDirectory; 
        }

        [XmlAttribute("supertypeFromWorkingDirectory")]
        public string SupertypeFromWorkingDirectory = "";

        public bool FeatureFunction(Pair<NEC, Hla> necAndHla) 
        { 
            bool b = SupertypeFromWorkingDirectory == VirusCount.HlaToLength.ToSupertypeFromWorkingDirectoryString(necAndHla.Second.ToString());
            return b; 
        }


        public override int GetHashCode()
        {
            return SupertypeFromWorkingDirectory.GetHashCode() ^ "IsSupertypeFromWorkingDirectory".GetHashCode(); 
        } 

        public override bool Equals(object obj) 
        {
            IsSupertypeFromWorkingDirectory other = obj as IsSupertypeFromWorkingDirectory;
            if (null == other)
            {
                return false;
            } 
            else 
            {
                return SupertypeFromWorkingDirectory.Equals(other.SupertypeFromWorkingDirectory); 
            }
        }

        public override string ToString()
        {
            return string.Format("SupertypeWorkingDir={0}", SupertypeFromWorkingDirectory); 
        } 

 

    }


    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class IsSupertypeFromFile : EntityFeature, IHashableFeature 
    {
        static Dictionary<string, Dictionary<string, string>> filePrefixToHlaToSt = new Dictionary<string, Dictionary<string, string>>(); 



        public IsSupertypeFromFile()
        {
        } 
 
        internal static IsSupertypeFromFile GetInstance(Hla hla, string filePrefix, Assembly assembly, string resourcePrefix)
        { 
            IsSupertypeFromFile isSupertypeFromFile = new IsSupertypeFromFile();
            isSupertypeFromFile.FilePrefix = filePrefix;
            isSupertypeFromFile.Supertype = LookupSupertypeFromFile(hla.ToString(), filePrefix, assembly, resourcePrefix);
            isSupertypeFromFile.Assembly = assembly;
            isSupertypeFromFile.ResourcePrefix = resourcePrefix;
            return isSupertypeFromFile; 
        } 

        [XmlAttribute("supertype")] 
        public string Supertype = "";

        [XmlAttribute("filePrefix")]
        public string FilePrefix = "";

        [XmlIgnore()] 
        public Assembly Assembly = null; 

        [XmlIgnore()] 
        public string ResourcePrefix = "";

        public bool FeatureFunction(Pair<NEC, Hla> necAndHla)
        {
            bool b = (Supertype == LookupSupertypeFromFile(necAndHla.Second.ToString(), FilePrefix, Assembly, ResourcePrefix));
            return b; 
        } 

 
        public override int GetHashCode()
        {
            return Supertype.GetHashCode() ^ "IsSupertypeFromFile".GetHashCode() ^ FilePrefix.GetHashCode();
        }

        public override bool Equals(object obj) 
        { 
            IsSupertypeFromFile other = obj as IsSupertypeFromFile;
            if (null == other) 
            {
                return false;
            }
            else
            {
                return Supertype.Equals(other.Supertype) && FilePrefix.Equals(other.FilePrefix); 
            } 
        }
 
        public override string ToString()
        {
            return string.Format("Supertype@{0}={1}", FilePrefix, Supertype);
        }

        public static string LookupSupertypeFromFile(string hlaName, string supertypeFilePrefix, Assembly assembly, string resourcePrefix) 
        { 
            Dictionary<string, string> hlaToSupertype;
            if (!filePrefixToHlaToSt.ContainsKey(supertypeFilePrefix)) 
            {
                string fileName = supertypeFilePrefix + ".txt";
                hlaToSupertype = new Dictionary<string, string>();
                //SpecialFunctions.CheckCondition(File.Exists(fileName), string.Format("The file '{0}' was not found", fileName));
                foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(assembly, resourcePrefix, fileName, "hla\tsupertype", false, '\t', true))
                { 
                    string hlax = row["hla"]; 
                    string supertypex = row["supertype"];
                    if (supertypex != "") 
                    {
                        hlaToSupertype.Add(hlax, supertypex);
                    }
                }
                filePrefixToHlaToSt.Add(supertypeFilePrefix, hlaToSupertype);
            } 
            else 
            {
                hlaToSupertype = filePrefixToHlaToSt[supertypeFilePrefix]; 
            }

            string supertypeName;
            if (!hlaToSupertype.TryGetValue(hlaName, out supertypeName))
            {
                supertypeName = "none"; 
            } 
            return supertypeName;
        } 



    }

 
 

    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class DoesBind : EntityFeature, IHashableFeature
    {

        public DoesBind()
        { 
        } 

        public static DoesBind GetInstance() 
        {
            DoesBind doesBind = new DoesBind();
            return doesBind;
        }

        public bool FeatureFunction(Pair<NEC, Hla> necAndHla) 
        { 
            bool b = BindingTable[necAndHla];
            return b; 
        }

        public override int GetHashCode()
        {
            return "DoesBind".GetHashCode();
        } 
 
        public override bool Equals(object obj)
        { 
            DoesBind other = obj as DoesBind;
            if (other == null)
            {
                return false;
            }
            else 
            { 
                return true;
            } 
        }

        public override string ToString()
        {
            return "DoesBind";
        } 
 

        static private Dictionary<Pair<NEC, Hla>, bool> _bindingTable = null; 
        static public Dictionary<Pair<NEC, Hla>, bool> BindingTable
        {
            get
            {
                if (null == _bindingTable)
                { 
                    _bindingTable = ReadTable(HlaFactory.GetFactory("JustSlash"), "Binding.txt", false); 
                }
                return _bindingTable; 
            }
        }

        //!!!very similar to other code
        public static Dictionary<Pair<NEC, Hla>, bool> ReadTable(HlaFactory hlaFactory, string fileName, bool dedup)
        { 
            Dictionary<Pair<NEC, Hla>, bool> table = new Dictionary<Pair<NEC, Hla>, bool>(); 
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(fileName, "N\tepitope\tC\thla\tlabel", false))
            { 
                string n = row["N"];
                string epitope = row["epitope"];
                SpecialFunctions.CheckCondition(Biology.GetInstance().LegalPeptide(epitope), string.Format("Peptide, '{0}', contains illegal char.", epitope));
                string c = row["C"];
                NEC nec = NEC.GetInstance(n, epitope, c);
                Hla hla = hlaFactory.GetGroundInstance(row["hla"]); 
                string labelString = row["label"]; 
                SpecialFunctions.CheckCondition(labelString == "0" || labelString == "1", "Expect label to be '0' or '1'");
                Pair<NEC, Hla> pair = new Pair<NEC, Hla>(nec, hla); 
                bool labelAsBool = (labelString == "1");
                if (dedup && table.ContainsKey(pair))
                {
                    SpecialFunctions.CheckCondition(table[pair] == labelAsBool, "The example " + pair.ToString() + " appears with contradictory labels.");
                    continue;
                } 
                table.Add(pair, labelAsBool); 
            }
            return table; 
        }
    }

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class CloseHuman : EntityFeature, IHashableFeature 
    { 

        public CloseHuman() 
        {
        }

        public static CloseHuman GetInstance(int maxMismatches)
        {
            CloseHuman closeHuman = new CloseHuman(); 
            closeHuman.MaxMismatches = maxMismatches; 
            return closeHuman;
        } 

        public bool FeatureFunction(Pair<NEC, Hla> necAndHla)
        {
            return IsCloseHuman(necAndHla, MaxMismatches);
        }
 
        static public bool IsCloseHuman(Pair<NEC, Hla> necAndHla, int maxMismatches) 
        {
            int differenceCount = CloseHumanTable()[necAndHla.First.E]; 
            bool b = (differenceCount <= maxMismatches);
            return b;
        }

        public override int GetHashCode()
        { 
            return MaxMismatches.GetHashCode() ^ "CloseHuman".GetHashCode(); 
        }
 
        public override bool Equals(object obj)
        {
            CloseHuman other = obj as CloseHuman;
            if (other == null)
            {
                return false; 
            } 
            else
            { 
                return MaxMismatches == other.MaxMismatches;
            }
        }

        public override string ToString()
        { 
            return string.Format("HumanDist<={0}", MaxMismatches); 
        }
 
        [XmlAttribute("maxMismatches")]
        public int MaxMismatches;


        static private Dictionary<string, int> _closeHumanTable = null;
        static int MaxMaxMismatches = int.MinValue; 
        static public Dictionary<string, int> CloseHumanTable() 
        {
                if (null == _closeHumanTable) 
                {
                    _closeHumanTable = ReadTable("HumanDistance.txt");
                }
                return _closeHumanTable;
        }
 
        //!!!very similar to other code 
        public static Dictionary<string, int> ReadTable(string fileName)
        { 
            Dictionary<string, int> table = new Dictionary<string, int>();
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(fileName, "epitope\tdifferenceCount\tid\tstartAABase1", false))
            {
                string epitope = row["epitope"];
                SpecialFunctions.CheckCondition(Biology.GetInstance().LegalPeptide(epitope), string.Format("Peptide, '{0}', contains illegal char.", epitope));
                int differenceCount = int.Parse(row["differenceCount"]); 
                MaxMaxMismatches = Math.Max(MaxMaxMismatches, differenceCount); 
                table.Add(epitope, differenceCount);
            } 
            return table;
        }

        internal static IEnumerable<IHashableFeature> CreateFeatures(Pair<NEC, Hla> necAndHla)
        {
            CloseHumanTable(); 
            Debug.Assert(MaxMaxMismatches != int.MinValue); // real assert 
            for (int maxMismatches = 0; maxMismatches <= MaxMaxMismatches; ++maxMismatches)
            { 
                if (CloseHuman.IsCloseHuman(necAndHla, maxMismatches))
                {
                    CloseHuman featureCloseHuman = CloseHuman.GetInstance(maxMismatches);
                    yield return featureCloseHuman;
                }
            } 
        } 
    }
 
    public class AminoAcidInternal
    {
        public string AminoAcidAsString;
        public char AminoAcidAsChar;
        //public Study Study;
 
        internal static AminoAcidInternal GetInstance(char aminoAcid) 
        {
            AminoAcidInternal aAminoAcidInternal = new AminoAcidInternal(); 
            string sAminoAcid = VirusCount.Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[aminoAcid];
            aAminoAcidInternal.AminoAcidAsChar = aminoAcid;
            aAminoAcidInternal.AminoAcidAsString = sAminoAcid;
            return aAminoAcidInternal;
        }
 
        internal static AminoAcidInternal GetInstance(string sAminoAcid) 
        {
            AminoAcidInternal aAminoAcidInternal = new AminoAcidInternal(); 
            char aminoAcid = VirusCount.Biology.GetInstance().ThreeLetterAminoAcidAbbrevTo1Letter[sAminoAcid];
            aAminoAcidInternal.AminoAcidAsChar = aminoAcid;
            aAminoAcidInternal.AminoAcidAsString = sAminoAcid;
            return aAminoAcidInternal;
        }
 
 
        public override int GetHashCode()
        { 
            return AminoAcidAsString.GetHashCode() ^ 2838483; //The constant string is to distinish this class from a string
        }

        public override bool Equals(object obj)
        {
            AminoAcidInternal other = obj as AminoAcidInternal; 
            if (other == null) 
            {
                return false; 
            }
            else
            {
                return AminoAcidAsString == other.AminoAcidAsString;
            }
        } 
 
    }
 
    //!!!!Need to somehow not have "AsPrefix" and "Exact" so interminggled.

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class HlaSeq : EntityFeature, IHashableFeature
    { 
        public HlaSeq() 
        {
        } 

        [XmlAttribute("pos")]
        public int Pos;

        [XmlAttribute("treatHlaAsPrefix")]
        public bool TreatHlaAsPrefix; 
 
        public AminoAcidInternal FeatureFunction(Pair<NEC, Hla> necAndHla)
        { 
            string aminoAcidAsString = GetAminoAcid(Pos, TreatHlaAsPrefix, necAndHla.Second);
            AminoAcidInternal aAminoAcidInternal = AminoAcidInternal.GetInstance(aminoAcidAsString);
            return aAminoAcidInternal;
        }

        internal static HlaSeq GetInstance(int pos, bool treatHlaAsPrefix) 
        { 
            SpecialFunctions.CheckCondition(pos > 0, "pos must be 1 or greater");
            HlaSeq hlaSeq = new HlaSeq(); 
            hlaSeq.Pos = pos;
            hlaSeq.TreatHlaAsPrefix = treatHlaAsPrefix;
            return hlaSeq;
        }

        public override int GetHashCode() 
        { 
            return Pos.GetHashCode() ^ "HlaSeq".GetHashCode() ^ TreatHlaAsPrefix.GetHashCode();
        } 

        public override bool Equals(object obj)
        {
            HlaSeq other = obj as HlaSeq;
            if (other == null)
            { 
                return false; 
            }
            else 
            {
                return Pos == other.Pos && TreatHlaAsPrefix == other.TreatHlaAsPrefix;
            }
        }

        public override string ToString() 
        { 
            if (TreatHlaAsPrefix)
            { 
                return string.Format("HlaPreSeq{0}", Pos);
            }
            else
            {
                return string.Format("HlaSeq{0}", Pos);
            } 
        } 

        internal static IEnumerable<HlaSeq> InterestingHlaPositionsForPropertyFeatures(bool treatHlaAsPrefix) 
        {
            if (treatHlaAsPrefix)
            {
                return InterestingHlaSeqToPropertiesAsPrefix.Keys;
            }
            else 
            { 
                return InterestingHlaSeqToPropertiesExact.Keys;
            } 
        }

        internal static Set<string> InterestingProperties(HlaSeq featureHlaSeq)
        {
            if (featureHlaSeq.TreatHlaAsPrefix)
            { 
                return InterestingHlaSeqToPropertiesAsPrefix[featureHlaSeq]; 
            }
            else 
            {
                return InterestingHlaSeqToPropertiesExact[featureHlaSeq];
            }
        }

        internal static string GetAminoAcid(int position, bool treatHlaAsPrefix, Hla hla) 
        { 
            string sequence;
            if (treatHlaAsPrefix) 
            {
                sequence = null;
                foreach (string hlaPrefix in EveryPrefixInclusiveDownToTwoDigits(hla.ToString()))
                {
                    if (HlaNameToSequenceAsPrefix.TryGetValue(hlaPrefix, out sequence)) // Will also get a hit because empty string is included.
                    { 
                        break; 
                    }
                } 
                SpecialFunctions.CheckCondition(null != sequence, "The hla, even trimmed two length 3 (class letter and two digits), not found in HlaToSequence file: '" + hla.ToString() + "'.");

            }
            else
            {
                sequence = HlaNameToSequenceExact[hla.ToString()]; 
            } 
            char aminoAcidChar = sequence[position - 1];
            string sAminoAcid = VirusCount.Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[aminoAcidChar]; 
            return sAminoAcid;
        }

        //!!!these have to be in this order or it doesn't work - could be nicer
        static Set<HlaSeq> InterestingToAAExact = null;
        static Set<HlaSeq> InterestingToAAAsPrefix = null; 
        static Dictionary<HlaSeq, Set<string>> InterestingHlaSeqToPropertiesExact = null; 
        static Dictionary<HlaSeq, Set<string>> InterestingHlaSeqToPropertiesAsPrefix = null;
        static Dictionary<string, string> HlaNameToSequenceAsPrefix = null; 
        static Dictionary<string, string> HlaNameToSequenceExact = null; //LoadHlaNameToSequence();

        private static Dictionary<string, string> LoadHlaNameToSequence()
        {
            Dictionary<string, string> hlaNameToSequence = new Dictionary<string, string>();
            foreach (Dictionary<string, string> row in Predictor.TabFileTable("HlaToSequence.txt", "Hla\tSequence", false)) 
            { 
                hlaNameToSequence.Add(row["Hla"], row["Sequence"]);
            } 

            CheckThatAllSameLengthAndAtLeastOne(hlaNameToSequence);
            SetUpInterestingHlaPositionsForAAFeaturesTable(hlaNameToSequence); //!!! would be nicer if didn't have side effects

            ComputeAsPrefix(hlaNameToSequence);
 
            return hlaNameToSequence; 
        }
 
        private static void ComputeAsPrefix(Dictionary<string, string> hlaNameToSequenceExact)
        {
            int? sequenceLength = null;
            Dictionary<string, List<string>> hlaPrefixToSequenceList = CreateHlaPrefixToSequenceList(hlaNameToSequenceExact, ref sequenceLength);

            Random random = new Random("ComputeAsPrefix".GetHashCode()); 
            HlaNameToSequenceAsPrefix = new Dictionary<string, string>(); 
            foreach (string hlaPrefix in hlaPrefixToSequenceList.Keys)
            { 
                List<string> sequenceList = hlaPrefixToSequenceList[hlaPrefix];
                StringBuilder sb = CreateConsensus(sequenceList, (int) sequenceLength, ref random);
                HlaNameToSequenceAsPrefix.Add(hlaPrefix, sb.ToString());
            }

        } 
 
        private static Dictionary<string, List<string>> CreateHlaPrefixToSequenceList(Dictionary<string, string> hlaNameToSequenceExact, ref int? sequenceLength)
        { 
            Dictionary<string, List<string>> hlaPrefixToSequenceList = new Dictionary<string, List<string>>();
            foreach (string hlaExact in hlaNameToSequenceExact.Keys)
            {
                string sequence = hlaNameToSequenceExact[hlaExact];
                sequenceLength = SetOrCheckSequenceLength(ref sequenceLength, sequence);
 
                foreach (string hlaPrefix in EveryPrefixInclusiveDownToTwoDigits(hlaExact)) 
                {
                    List<string> sequenceList = SpecialFunctions.GetValueOrDefault(hlaPrefixToSequenceList, hlaPrefix); 
                    sequenceList.Add(sequence);

                    if (hlaPrefix != hlaExact && hlaNameToSequenceExact.ContainsKey(hlaPrefix))
                    {
                        SpecialFunctions.CheckCondition(hlaNameToSequenceExact[hlaPrefix] != hlaNameToSequenceExact[hlaExact], "The input HlaToSequence file should not contains hla's HH and HHH such that HH is a prefix of HHH and such that their sequences are exactly the same. " + hlaPrefix + " " + hlaExact);
                    } 
                } 
            }
            return hlaPrefixToSequenceList; 
        }

        private static int? SetOrCheckSequenceLength(ref int? sequenceLength, string sequence)
        {
            if (null == sequenceLength)
            { 
                sequenceLength = sequence.Length; 
            }
            else 
            {
                SpecialFunctions.CheckCondition(sequenceLength == sequence.Length, "All sequences must be the same length");
            }
            return sequenceLength;
        }
 
        private static StringBuilder CreateConsensus(List<string> sequenceList, int sequenceLength, ref Random random) 
        {
            StringBuilder sb = new StringBuilder(sequenceLength); 
            for (int pos = 0; pos < sequenceLength; ++pos)
            {
                Dictionary<char, int> aaToCount = CreateAAToCount(sequenceList, pos);
                char bestAA = FindBestItemRandomlyBreakingTies(aaToCount, ref random);
                sb.Append(bestAA);
            } 
            return sb; 
        }
 
        private static Dictionary<char, int> CreateAAToCount(List<string> sequenceList, int pos)
        {
            Dictionary<char, int> aaToCount = new Dictionary<char, int>();
            foreach (string sequence in sequenceList)
            {
                char aa = sequence[pos]; 
                aaToCount[aa] = 1 + SpecialFunctions.GetValueOrDefault(aaToCount, aa); 
            }
            return aaToCount; 
        }

        private static char FindBestItemRandomlyBreakingTies(Dictionary<char, int> aaToCount, ref Random random)
        {
            Debug.Assert(aaToCount.Count > 0); // real assert
            int bestValue = 0; 
            char bestAA = char.MinValue; 
            int tieCount = 0;
            foreach (KeyValuePair<char, int> aaAndCount in aaToCount) 
            {
                int count = aaAndCount.Value;
                if (count >= bestValue)
                {
                    if (count > bestValue)
                    { 
                        bestValue = count; 
                        bestAA = aaAndCount.Key;
                        tieCount = 1; 
                    }
                    else
                    {
                        ++tieCount;
                        if (random.Next(tieCount) == 0)
                        { 
                            bestAA = aaAndCount.Key; 
                        }
                    } 
                }
            }
            Debug.Assert(tieCount != 0 && bestAA != char.MinValue); // real assert
            return bestAA;
        }
 
        private static IEnumerable<string> EveryPrefixInclusiveDownToTwoDigits(string hlaExact) 
        {
            SpecialFunctions.CheckCondition(hlaExact.Length >= 3, "Hla expected to have a least length 3 (a class letter and two digits): '" + hlaExact + "'."); 
            yield return hlaExact;
            for (int length = hlaExact.Length - 1; length >= 3; --length)
            {
                yield return hlaExact.Substring(0, length);
            }
        } 
 

        private static void SetUpInterestingHlaPositionsForAAFeaturesTable(Dictionary<string, string> hlaNameToSequence) 
        {
            int sumOfNonMaxMin = ReadSumOfNonMaxMin();

            string firstSequence = SpecialFunctions.First(hlaNameToSequence.Values);

            InterestingToAAExact = Set<HlaSeq>.GetInstance(); 
            InterestingToAAAsPrefix = Set<HlaSeq>.GetInstance(); 
            InterestingHlaSeqToPropertiesExact = new Dictionary<HlaSeq,Set<string>>();
            InterestingHlaSeqToPropertiesAsPrefix = new Dictionary<HlaSeq, Set<string>>(); 

            for (int posBase0 = 0; posBase0 < firstSequence.Length; ++posBase0)
            {
                Dictionary<char, int> aaToCount = new Dictionary<char, int>();
                Dictionary<string, Dictionary<bool, int>> propertyToValueToCount = new Dictionary<string, Dictionary<bool, int>>();
                foreach (string sequence in hlaNameToSequence.Values) 
                { 
                    char aa = sequence[posBase0];
                    aaToCount[aa] = SpecialFunctions.GetValueOrDefault(aaToCount, aa) + 1; 
                    //string aaAsString = Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[aa];

                    foreach (KeyValuePair<string, Set<char>> propertyAndAASet in VirusCount.KmerProperties.GetInstance().PropertyToAACharSet)
                    {
                        string property = propertyAndAASet.Key;
                        Set<char> aaSet = propertyAndAASet.Value; 
                        bool val = aaSet.Contains(aa); 
                        Dictionary<bool, int> valueToCount = SpecialFunctions.GetValueOrDefault(propertyToValueToCount, property);
                        valueToCount[val] = 1 + SpecialFunctions.GetValueOrDefault(valueToCount, val); 
                    }
                }

                if (SumOfNonMax(aaToCount.Values) >= sumOfNonMaxMin)
                {
                    HlaSeq hlaSeqExact = HlaSeq.GetInstance(posBase0 + 1, false); 
                    InterestingToAAExact.AddNew(hlaSeqExact); 

                    HlaSeq hlaSeqAsPrefix = HlaSeq.GetInstance(posBase0 + 1, true); 
                    InterestingToAAAsPrefix.AddNew(hlaSeqAsPrefix);

                    foreach (string property in propertyToValueToCount.Keys)
                    {
                        Dictionary<bool, int> valueToCount = propertyToValueToCount[property];
                        if (SumOfNonMax(valueToCount.Values) >= sumOfNonMaxMin) 
                        { 
                            Set<string> interestingPropertiesExact = SpecialFunctions.GetValueOrDefault(InterestingHlaSeqToPropertiesExact, hlaSeqExact);
                            interestingPropertiesExact.AddNew(property); 
                            Set<string> interestingPropertiesAsPrefix = SpecialFunctions.GetValueOrDefault(InterestingHlaSeqToPropertiesAsPrefix, hlaSeqAsPrefix);
                            interestingPropertiesAsPrefix.AddNew(property);
                        }
                    }
                }
            } 
        } 

        private static int ReadSumOfNonMaxMin() 
        {
            foreach (Dictionary<string, string> row in Predictor.TabFileTableNoHeaderInFile("HlaToSequenceCountFilter.txt", "CountFilter", false))
            {
                string asString = row["CountFilter"];
                int sumOfNonMaxMin = int.Parse(asString);
                return sumOfNonMaxMin; 
            } 
            SpecialFunctions.CheckCondition(false, "HlaToSequenceCountFilter.txt is empty");
            return int.MinValue; 
        }

        private static int SumOfNonMax(IEnumerable<int> valueCollection)
        {
            int max = int.MinValue;
            int total = 0; 
            foreach (int val in valueCollection) 
            {
                total += val; 
                max = Math.Max(max, val);
            }
            return total - max;
        }

 
        private static void CheckThatAllSameLengthAndAtLeastOne(Dictionary<string, string> hlaNameToSequence) 
        {
            SpecialFunctions.CheckCondition(hlaNameToSequence.Count > 0, "hlaToSequence table must not be empty"); 
            string firstSequence = SpecialFunctions.First(hlaNameToSequence.Values);
            foreach (string sequence in hlaNameToSequence.Values)
            {
                SpecialFunctions.CheckCondition(firstSequence.Length == sequence.Length, "All sequences in the hlaToSequence table must be the same length.");
            }
 
        } 

        internal static IEnumerable<HlaSeq> InterestingHlaPositionsForAAFeatures(bool treatHlaAsPrefix) 
        {
            if (treatHlaAsPrefix)
            {
                if (null == InterestingToAAAsPrefix)
                {
                    HlaNameToSequenceExact = LoadHlaNameToSequence(); 
                    Debug.Assert(null != InterestingToAAAsPrefix); // real assert 
                }
                return InterestingToAAAsPrefix; 
            }
            else
            {
                if (null == InterestingToAAExact)
                {
                    HlaNameToSequenceExact = LoadHlaNameToSequence(); 
                    Debug.Assert(null != HlaNameToSequenceExact); // real assert 
                }
                return InterestingToAAExact; 
            }
        }
    }


} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
