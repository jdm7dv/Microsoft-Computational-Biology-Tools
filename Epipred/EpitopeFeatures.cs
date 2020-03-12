//using System; 
//using System.Collections.Generic;
//using System.Text;
//using Msr.Adapt.LearningWorkbench;
//using System.Xml.Serialization;
//using System.Diagnostics;
//using Msr.Adapt.HighLevelFeatures; 
//using System.IO; 
//using System.Text.RegularExpressions;
//using Msr.Mlas.SpecialFunctions; 
//using EpipredLib;
////using VirusCount;

//namespace VirusCount
//{
 
//    //[Serializable] 
//    //[XmlRoot(Namespace = "", IsNullable = false)]
//    //public class Immunogenic : EntityFeature 
//    //{

//    //    public Immunogenic()
//    //    {
//    //    }
 
//    //    public bool FeatureFunction(MerAndHlaToLength merAndHlaToLength) 
//    //    {
//    //        bool b = MerAndHlaToLengthWithLabel.Value; 
//    //        return b;
//    //    }

//    //    //static public string ExtractValue(DataRow dataRow, string name)
//    //    //{
//    //    //    if (dataRow[name] == DBNull.Value) 
//    //    //    { 
//    //    //        return "--";
//    //    //    } 

//    //    //    object objValue = dataRow[name];
//    //    //    string sValue = objValue.ToString();
//    //    //    return sValue;

//    //    //} 
 
//    //    //static public IEnumerable Generate(object dataRow)
//    //    //{ 
//    //    //    Debug.Assert(dataRow is DataRow); //!!!raise error
//    //    //    DataRow aDataRow = (DataRow)dataRow;

//    //    //    ArrayList rgFeature = new ArrayList();
//    //    //    foreach (DataColumn aDataColumn in aDataRow.Table.Columns)
//    //    //    { 
//    //    //        string sName = aDataColumn.ColumnName; 
//    //    //        string sValue = Column.ExtractValue(aDataRow, sName); //!!!! what if value is not string
//    //    //        //!!!need to remove = 0 features if the variable is boolean 
//    //    //        rgFeature.Add(new Column(sName, sValue));
//    //    //    }
//    //    //    return rgFeature;
//    //    //}

 
//    //} 

//    public class AminoAcidInternal 
//    {
//        public string AminoAcidAsString;
//        public char AminoAcidAsChar;
//        //public Study Study;

//        internal static AminoAcidInternal GetInstance(char aminoAcid) 
//        { 
//            AminoAcidInternal aAminoAcidInternal = new AminoAcidInternal();
//            string sAminoAcid = Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[aminoAcid]; 
//            aAminoAcidInternal.AminoAcidAsChar = aminoAcid;
//            aAminoAcidInternal.AminoAcidAsString = sAminoAcid;
//            //aAminoAcidInternal.Study = study;
//            return aAminoAcidInternal;
//        }
 
//        public override int GetHashCode() 
//        {
//            return AminoAcidAsString.GetHashCode() ^ 2838483; //The constant string is to distinish this class from a string 
//        }

//        public override bool Equals(object obj)
//        {
//            AminoAcidInternal other = obj as AminoAcidInternal;
//            if (other == null) 
//            { 
//                return false;
//            } 
//            else
//            {
//                return AminoAcidAsString == other.AminoAcidAsString;
//            }
//        }
//    } 
 

 
//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class IsHla : EntityFeature, IHashableFeature
//    {

//        public IsHla() 
//            : this(null) 
//        {
//        } 

//        public IsHla(HlaToLength hlaToLength)
//        {
//            if (hlaToLength == null)
//            {
//                HlaToLength = ""; 
//            } 
//            else
//            { 

//                HlaToLength = hlaToLength.ToString();
//            }
//        }

//        [XmlAttribute("hla")] 
//        public string HlaToLength; 

 

//        public bool FeatureFunction(MerAndHlaToLength merAndHlaToLength)
//        {
//            bool b = HlaToLength == merAndHlaToLength.HlaToLength.ToString();
//            return b;
//        } 
 
//        public override int GetHashCode()
//        { 
//            return HlaToLength.GetHashCode() ^ 932838483; //The constant string is to distinish this class from a string
//        }

//        public override bool Equals(object obj)
//        {
//            IsHla other = obj as IsHla; 
//            if (other == null) 
//            {
//                return false; 
//            }
//            else
//            {
//                return HlaToLength.Equals(other.HlaToLength);
//            }
//        } 
 
//        public override string ToString()
//        { 
//            return HlaToLength.ToString();
//        }


//    }
 
 

//    [Serializable] 
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class CloseAminoAcids : EntityFeature, IHashableFeature
//    {
//        public static Regex RegexAminoAcid = new Regex("^[A-Z][a-z][a-z]$");

//        public CloseAminoAcids() 
//        { 
//            EpitopeAminoAcid = "";
//            MhcIAminoAcid = ""; 
//        }

//        public CloseAminoAcids(string epitopeAminoAcid, string mhcIAminoAcid)
//        {
//            SpecialFunctions.CheckCondition(RegexAminoAcid.IsMatch(epitopeAminoAcid) && RegexAminoAcid.IsMatch(mhcIAminoAcid));
//            EpitopeAminoAcid = epitopeAminoAcid; 
//            MhcIAminoAcid = mhcIAminoAcid; 
//        }
 
//        [XmlAttribute("epitope")]
//        public string EpitopeAminoAcid;

//        [XmlAttribute("mhci")]
//        public string MhcIAminoAcid;
 
 
//        public bool FeatureFunction(MerAndHlaToLength merAndHlaToLength)
//        { 
//            bool b = false;
//            SpecialFunctions.CheckCondition(false, "The evaluator for 'CloseAminoAcids' is not written because it should not be needed.");
//            return b;
//        }

//        public override int GetHashCode() 
//        { 
//            return EpitopeAminoAcid.GetHashCode() ^ MhcIAminoAcid.GetHashCode() ^ "CloseAminoAcids".GetHashCode();
//        } 

//        public override bool Equals(object obj)
//        {
//            CloseAminoAcids other = obj as CloseAminoAcids;
//            if (other == null)
//            { 
//                return false; 
//            }
//            else 
//            {
//                bool b = EpitopeAminoAcid == other.EpitopeAminoAcid && MhcIAminoAcid == other.MhcIAminoAcid;
//                //string s1 = ToString();
//                //string s2 = other.ToString();
//                //bool b2 = s1 == s2;
//                //Debug.Assert(b == b2); 
//                return b; 
//            }
//        } 

//        public override string ToString()
//        {
//            return string.Format("CloseAminoAcids({0},{1})", EpitopeAminoAcid, MhcIAminoAcid);
//        }
 
 
//    }
 

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class CloseAminoAcids2 : EntityFeature, IHashableFeature
//    {
//        public static Regex RegexAminoAcid = new Regex("^[A-Z][a-z][a-z]$"); 
 
//        public CloseAminoAcids2()
//        { 
//            EpitopeAminoAcid = "";
//            MhcIAminoAcid = "";
//            Pos = int.MinValue;
//        }

//        public CloseAminoAcids2(int pos, string epitopeAminoAcid, string mhcIAminoAcid) 
//        { 
//            SpecialFunctions.CheckCondition(RegexAminoAcid.IsMatch(epitopeAminoAcid) && RegexAminoAcid.IsMatch(mhcIAminoAcid));
//            EpitopeAminoAcid = epitopeAminoAcid; 
//            MhcIAminoAcid = mhcIAminoAcid;
//            Pos = pos;
//        }

//        [XmlAttribute("pos")]
//        public int Pos; 
 
//        [XmlAttribute("epitope")]
//        public string EpitopeAminoAcid; 

//        [XmlAttribute("mhci")]
//        public string MhcIAminoAcid;


//        public bool FeatureFunction(MerAndHlaToLength merAndHlaToLength) 
//        { 
//            bool b = false;
//            SpecialFunctions.CheckCondition(false, "The evaluator for 'CloseAminoAcids' is not written because it should not be needed."); 
//            return b;
//        }

//        public override int GetHashCode()
//        {
//            return Pos.GetHashCode() ^ EpitopeAminoAcid.GetHashCode() ^ MhcIAminoAcid.GetHashCode() ^ "CloseAminoAcids2".GetHashCode(); 
//        } 

//        public override bool Equals(object obj) 
//        {
//            CloseAminoAcids2 other = obj as CloseAminoAcids2;
//            if (other == null)
//            {
//                return false;
//            } 
//            else 
//            {
//                bool b = Pos == other.Pos && EpitopeAminoAcid == other.EpitopeAminoAcid && MhcIAminoAcid == other.MhcIAminoAcid; 
//                return b;
//            }
//        }

//        public override string ToString()
//        { 
//            return string.Format("CloseAminoAcids2({0},{1},{2})", Pos, EpitopeAminoAcid, MhcIAminoAcid); 
//        }
 

//    }

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class H : EntityFeature, IHashableFeature 
//    { 
//        public H()
//        { 
//        }

//        [XmlAttribute("pos")]
//        public int Pos;

//        public AminoAcidInternal FeatureFunction(MerAndHlaToLength aMerAndHlaToLength) 
//        { 
//            Debug.Fail("Not used for now");
//            return null; 
//        }

//        internal static H GetInstance(int pos)
//        {
//            H h = new H();
//            h.Pos = pos; 
//            return h; 
//        }
 
//        public override int GetHashCode()
//        {
//            return Pos.GetHashCode() ^ "H".GetHashCode();
//        }

//        public override bool Equals(object obj) 
//        { 
//            H other = obj as H;
//            if (other == null) 
//            {
//                return false;
//            }
//            else
//            {
//                return Pos == other.Pos; 
//            } 
//        }
 
//        public override string ToString()
//        {
//            return string.Format("H{0}", Pos);
//        }

//        private static List<int> hlaPositionsBase1OfInterestList = null; 
//        private static IEnumerable<int> HlaPositionsOfInterest() 
//        {
//            if (hlaPositionsBase1OfInterestList == null) 
//            {
//                hlaPositionsBase1OfInterestList = new List<int>();
//                foreach (Dictionary<string, string> row in Predictor.TabFileTableNoHeaderInFile("HlaPositionsBase1OfInterest.txt", "pos1", false))
//                {
//                    hlaPositionsBase1OfInterestList.Add(int.Parse(row["pos1"]));
//                } 
//            } 
//            return hlaPositionsBase1OfInterestList;
//        } 

//        private static Dictionary<string, string> hlaToAAString = new Dictionary<string, string>();
//        private static List<KeyValuePair<string, string>> hlaFileContents = null;
//        private static string AminoAcidForHla(string hla)
//        {
//            if (!hlaToAAString.ContainsKey(hla)) 
//            { 
//                if (hlaFileContents == null)
//                { 
//                    hlaFileContents = new List<KeyValuePair<string, string>>();
//                    using (StreamReader streamReader = Predictor.OpenResource("human_HLAs_strs.txt"))
//                    {
//                        while (true)
//                        {
//                            string hlaName = streamReader.ReadLine(); 
//                            if (hlaName == null) 
//                            {
//                                break; 
//                            }
//                            string aaSeq = streamReader.ReadLine();
//                            SpecialFunctions.CheckCondition(aaSeq != null);
//                            hlaFileContents.Add(new KeyValuePair<string, string>(hlaName.Replace("_", ""), aaSeq));
//                        }
//                    } 
//                } 

//                string foundAASeq = null; 
//                foreach (KeyValuePair<string, string> hlaNameAndAASeq in hlaFileContents)
//                {
//                    string hlaName = hlaNameAndAASeq.Key;
//                    if (hlaName.StartsWith(hla))
//                    {
//                        foundAASeq = hlaNameAndAASeq.Value; 
//                        break; 
//                    }
//                } 
//                SpecialFunctions.CheckCondition(foundAASeq != null);
//                hlaToAAString.Add(hla, foundAASeq);

//            }
//            return hlaToAAString[hla];
//        } 
 
//        internal static IEnumerable<KeyValuePair<int, string>> PosAndAACollection(string hla)
//        { 
//            string aaString = AminoAcidForHla(hla);
//            foreach (int posBase1 in HlaPositionsOfInterest())
//            {
//                if (aaString.Length < posBase1)
//                {
//                    break; 
//                } 

//                char aaChar = aaString[posBase1 - 1]; 
//                string aa = Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[aaChar];

//                yield return new KeyValuePair<int, string>(posBase1, aa);
//            }
//        }
 
 
//    }
 

//    //[Serializable]
//    //[XmlRoot(Namespace = "", IsNullable = false)]
//    //public class HlaTreading : EntityFeature, IHashableFeature
//    //{
//    //    public static Regex RegexAminoAcid = new Regex("^[A-Z][a-z][a-z]$"); 
 
//    //    public HlaTreading()
//    //    { 
//    //        MerAminoAcid = "";
//    //        MerPos = int.MinValue;
//    //        HlaAminoAcid = "";
//    //        HlaPos = int.MinValue;
//    //    }
 
//    //    public HlaTreading(string merAminoAcid, int merPos, string hlaAminoAcid, int hlaPos) 
//    //    {
//    //        SpecialFunctions.CheckCondition(RegexAminoAcid.IsMatch(merAminoAcid) && RegexAminoAcid.IsMatch(hlaAminoAcid)); 
//    //        MerAminoAcid = merAminoAcid;
//    //        MerPos = merPos;
//    //        HlaAminoAcid = hlaAminoAcid;
//    //        HlaPos = hlaPos;
//    //    }
 
//    //    [XmlAttribute("mer")] 
//    //    public string MerAminoAcid;
 
//    //    [XmlAttribute("merPos")]
//    //    public int MerPos;

//    //    [XmlAttribute("hla")]
//    //    public string HlaAminoAcid;
 
//    //    [XmlAttribute("hlaPos")] 
//    //    public int HlaPos;
 

//    //    public bool FeatureFunction(MerAndHlaToLength merAndHlaToLength)
//    //    {
//    //        bool b = false;
//    //        SpecialFunctions.CheckCondition(false, "The evaluator for 'CloseAminoAcids' is not written because it should not be needed.");
//    //        return b; 
//    //    } 

//    //    public override int GetHashCode() 
//    //    {
//    //        return (~HlaPos.GetHashCode()) ^ MerPos.GetHashCode() ^ MerAminoAcid.GetHashCode() ^ HlaAminoAcid.GetHashCode() ^ "HlaTreading".GetHashCode();
//    //    }

//    //    public override bool Equals(object obj)
//    //    { 
//    //        HlaTreading other = obj as HlaTreading; 
//    //        if (other == null)
//    //        { 
//    //            return false;
//    //        }
//    //        else
//    //        {
//    //            bool b = HlaPos == other.HlaPos && MerPos == other.MerPos && MerAminoAcid == other.MerAminoAcid && HlaAminoAcid == other.HlaAminoAcid;
//    //            return b; 
//    //        } 
//    //    }
 
//    //    public override string ToString()
//    //    {
//    //        return string.Format("HlaTreading(MerAA={0}@{1},HlaAA={2}@{3})",MerAminoAcid, MerPos, HlaAminoAcid, HlaPos);
//    //    }

 
 
//    //}
 
//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class IsAA : Feature, IHashableFeature
//    {

//        public IsAA() 
//            : this("", null) 
//        {
//        } 

//        public IsAA(string aminoAcid, IHashableFeature featureBea)
//        {
//            AminoAcid = aminoAcid;
//            if (featureBea != null)
//            { 
//                base.FeatureCollection = new Feature[] { (Feature)featureBea }; 
//            }
//        } 

//        [XmlAttribute("aa")]
//        public string AminoAcid;


 
//        public bool FeatureFunction(AminoAcidInternal aminoAcidInternal) 
//        {
//            bool b = aminoAcidInternal.AminoAcidAsString == AminoAcid; 
//            return b;
//        }

//        public override int GetHashCode()
//        {
//            return AminoAcid.GetHashCode() ^ (FeatureCollection.Length==0?0:FeatureCollection[0].GetHashCode()); 
//        } 

//        public override bool Equals(object obj) 
//        {
//            IsAA other = obj as IsAA;
//            if (other == null)
//            {
//                return false;
//            } 
//            else 
//            {
//                if (AminoAcid != other.AminoAcid || FeatureCollection.Length != other.FeatureCollection.Length) 
//                {
//                    return false;
//                }
//                if (FeatureCollection.Length > 0)
//                {
//                    return FeatureCollection[0].Equals(other.FeatureCollection[0]); 
//                } 
//                return true;
//            } 
//        }

//        public override string ToString()
//        {
//            if (FeatureCollection.Length == 0)
//            { 
//                return ""; 
//            }
//            return string.Format("{0}={1}", FeatureCollection[0], AminoAcid); 
//        }


//    }

 
//    //[Serializable] 
//    //[XmlRoot(Namespace = "", IsNullable = false)]
//    //public class And : Feature, IHashableFeature 
//    //{
//    //    public And()
//    //        : this(null, null)
//    //    {
//    //    }
 
//    //    //!!!make work for any number of arguments 
//    //    public And(IHashableFeature boolfeature1, IHashableFeature boolfeature2)
//    //    { 
//    //        if (boolfeature1 != null && boolfeature2 != null)
//    //        {
//    //            base.FeatureCollection = new Feature[] {(Feature) boolfeature1, (Feature) boolfeature2 };
//    //        }
//    //    }
 
//    //    public bool FeatureFunction(params object[] bools) 
//    //    {
//    //        foreach (bool aBool in bools) 
//    //        {
//    //            if (!aBool)
//    //            {
//    //                return false;
//    //            }
//    //        } 
 
//    //        return true;
//    //    } 

//    //    public override int GetHashCode()
//    //    {
//    //        int hashCode = 93823; // an arbitrary constant for "And"
//    //        foreach (Feature feature in FeatureCollection)
//    //        { 
//    //            hashCode ^= feature.GetHashCode(); 
//    //        }
//    //        return hashCode; 
//    //    }

//    //    public override bool Equals(object obj)
//    //    {
//    //        And other = obj as And;
//    //        if (other == null) 
//    //        { 
//    //            return false;
//    //        } 
//    //        else
//    //        {
//    //            if (FeatureCollection.Length != other.FeatureCollection.Length)
//    //            {
//    //                return false;
//    //            } 
//    //            for (int iFeature = 0; iFeature < FeatureCollection.Length; ++iFeature) 
//    //            {
//    //                if (!FeatureCollection[iFeature].Equals(other.FeatureCollection[iFeature])) 
//    //                {
//    //                    return false;
//    //                }
//    //            }
//    //            return true;
//    //        } 
//    //    } 

//    //    public override string ToString() 
//    //    {
//    //        StringBuilder sb = new StringBuilder();
//    //        foreach (Feature feature in FeatureCollection)
//    //        {
//    //            if (sb.Length != 0)
//    //            { 
//    //                sb.Append(" && "); 
//    //            }
//    //            sb.Append(feature.ToString()); 
//    //        }
//    //        return sb.ToString();
//    //    }
//    //}

 
 
//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)] 
//    public class E : EntityFeature, IHashableFeature
//    {
//        public E()
//        {
//        }
 
//        [XmlAttribute("pos")] 
//        public int Pos;
 
//        public AminoAcidInternal FeatureFunction(MerAndHlaToLength aMerAndHlaToLength)
//        {
//            KmerDefinition aKmerDefinition = aMerAndHlaToLength.KmerDefinition;
//            int iIndexInString = aKmerDefinition.BeforeMerCount + Pos - 1;
//            char chAminoAcid = aMerAndHlaToLength.Mer[(int)iIndexInString];
//            AminoAcidInternal aAminoAcidInternal = AminoAcidInternal.GetInstance(chAminoAcid); 
//            return aAminoAcidInternal; 
//        }
 
//        internal static E GetInstance(int pos)
//        {
//            E e = new E();
//            e.Pos = pos;
//            return e;
//        } 
 
//        public override int GetHashCode()
//        { 
//            return Pos.GetHashCode() ^ 23433;
//        }

//        public override bool Equals(object obj)
//        {
//            E other = obj as E; 
//            if (other == null) 
//            {
//                return false; 
//            }
//            else
//            {
//                return Pos == other.Pos;
//            }
//        } 
 
//        public override string ToString()
//        { 
//            return string.Format("E{0}", Pos);
//        }


//    }
 
//    [Serializable] 
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class B : EntityFeature, IHashableFeature 
//    {
//        public B()
//        {
//        }

//        [XmlAttribute("pos")] 
//        public int Pos; 

//        public AminoAcidInternal FeatureFunction(MerAndHlaToLength aMerAndHlaToLength) 
//        {
//            KmerDefinition aKmerDefinition = aMerAndHlaToLength.KmerDefinition;
//            int iIndexInString = aKmerDefinition.BeforeMerCount + Pos;
//            char chAminoAcid = aMerAndHlaToLength.Mer[(int)iIndexInString];
//            AminoAcidInternal aAminoAcidInternal = AminoAcidInternal.GetInstance(chAminoAcid);
//            return aAminoAcidInternal; 
//        } 

//        internal static B GetInstance(int pos) 
//        {
//            B b = new B();
//            b.Pos = pos;
//            return b;
//        }
 
//        public override int GetHashCode() 
//        {
//            return Pos.GetHashCode() ^ 23493; 
//        }

//        public override bool Equals(object obj)
//        {
//            B other = obj as B;
//            if (other == null) 
//            { 
//                return false;
//            } 
//            else
//            {
//                return Pos == other.Pos;
//            }
//        }
 
//        public override string ToString() 
//        {
//            return string.Format("B{0}", Pos); 
//        }
//    }

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class A : EntityFeature, IHashableFeature 
//    { 
//        public A()
//        { 
//        }

//        [XmlAttribute("pos")]
//        public int Pos;

//        public AminoAcidInternal FeatureFunction(MerAndHlaToLength aMerAndHlaToLength) 
//        { 
//            KmerDefinition aKmerDefinition = aMerAndHlaToLength.KmerDefinition;
//            int iIndexInString = aKmerDefinition.BeforeMerCount + aKmerDefinition.EpitopeMerCount + Pos - 1; 
//            char chAminoAcid = aMerAndHlaToLength.Mer[(int)iIndexInString];
//            AminoAcidInternal aAminoAcidInternal = AminoAcidInternal.GetInstance(chAminoAcid);
//            return aAminoAcidInternal;
//        }

//        internal static A GetInstance(int pos) 
//        { 
//            A a = new A();
//            a.Pos = pos; 
//            return a;
//        }

//        public override int GetHashCode()
//        {
//            return Pos.GetHashCode() ^ 237433; 
//        } 

//        public override bool Equals(object obj) 
//        {
//            A other = obj as A;
//            if (other == null)
//            {
//                return false;
//            } 
//            else 
//            {
//                return Pos == other.Pos; 
//            }
//        }

//        public override string ToString()
//        {
//            return string.Format("A{0}", Pos); 
//        } 

 
//    }

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class HasAAProp : Feature, IHashableFeature
//    { 
//        public HasAAProp(): this("",null) 
//        {
//        } 

//        public HasAAProp(string property, IHashableFeature featureBea)
//        {
//            Prop = property;
//            if (featureBea != null)
//            { 
//                base.FeatureCollection = new Feature[] { (Feature)featureBea }; 
//            }
//        } 

//        [XmlAttribute("prop")]
//        public string Prop;

//        public bool FeatureFunction(AminoAcidInternal aminoAcidInternal)
//        { 
//            bool b = KmerProperties.GetInstance().DoesAminoAcidHaveProperty(aminoAcidInternal.AminoAcidAsString, Prop); 
//            return b;
//        } 


//        public override int GetHashCode()
//        {
//            return Prop.GetHashCode() ^ 998 ^ (FeatureCollection.Length==0?0:FeatureCollection[0].GetHashCode());
//        } 
 
//        public override bool Equals(object obj)
//        { 
//            HasAAProp other = obj as HasAAProp;
//            if (other == null)
//            {
//                return false;
//            }
//            else 
//            { 
//                if (Prop != other.Prop || FeatureCollection.Length != other.FeatureCollection.Length)
//                { 
//                    return false;
//                }
//                if (FeatureCollection.Length > 0)
//                {
//                    return FeatureCollection[0].Equals(other.FeatureCollection[0]);
//                } 
//                return true; 
//            }
//        } 

//        public override string ToString()
//        {
//            if (FeatureCollection.Length == 0)
//            {
//                return ""; 
//            } 
//            return string.Format("{0}({1})", Prop, FeatureCollection[0]);
//        } 


//    }

//    //[Serializable]
//    //[XmlRoot(Namespace = "", IsNullable = false)] 
//    //public class Logistic : Feature 
//    //{
//    //    public Logistic() 
//    //        : this(null)
//    //    {
//    //    }

//    //    public Logistic(Feature[] FeatureCollectionToSum)
//    //    { 
//    //        base.FeatureCollection = FeatureCollectionToSum; 
//    //    }
 

//    //    [XmlAttribute("probability")]
//    //    public string Probability
//    //    {
//    //        get
//    //        { 
//    //            return string.Format("Exp(Total)/(1 + Exp(Total))"); 
//    //        }
//    //        set 
//    //        {
//    //            //Ignore, because you really can't set this
//    //        }

//    //    }
 
//    //    [XmlAnyAttribute()] 
//    //    public System.Xml.XmlAttribute[] AnyAttr;
 
//    //    public double FeatureFunction(params object[] products)
//    //    {
//    //        double rTotal = 0;
//    //        foreach (double rProduct in products)
//    //        {
//    //            rTotal += rProduct; 
//    //        } 

 
//    //        return TheFunction(rTotal);
//    //    }

//    //    protected double TheFunction(double rTotal)
//    //    {
//    //        double rExp = Math.Exp(rTotal); 
//    //        double rResult = rExp / (1.0 + rExp); // the logistic aka logistic function (see http://cerebro.xu.edu/math/math120/01f/logistic.pdf) 
//    //        return rResult;
//    //    } 

//    //    private Converter<object,Set<IHashableFeature>> _featureGenerator;
//    //    //[XmlElement("FeaturesFromEntity")]
//    //    [XmlIgnore]
//    //    public Converter<object, Set<IHashableFeature>> FeatureGenerator
//    //    { 
//    //        set 
//    //        {
//    //            _featureGenerator = value; 
//    //            SpecialFunctions.CheckCondition(_rgLogisticFeatures == null); //!!!raise error - should not set this twice
//    //            SpecialFunctions.CheckCondition(FeatureCollection != null); //!!!raise error the features should be here
//    //            _rgLogisticFeatures = new Dictionary<IHashableFeature, double>();
//    //            foreach (WeightIf aWeightIf in FeatureCollection) //!!!what if have non-boolean features
//    //            {
//    //                Debug.Assert(aWeightIf.FeatureCollection.Length == 1); //!!! 
//    //                IHashableFeature aFeature = (IHashableFeature) aWeightIf.FeatureCollection[0]; 
//    //                if (_rgLogisticFeatures.ContainsKey(aFeature))
//    //                { 
//    //                    Debug.Fail("don't expect two features that are the same");
//    //                    Debug.Assert(aFeature.Equals(Logistic.AlwaysTrue)); //real assert
//    //                    _rgLogisticFeatures[aFeature] = _rgLogisticFeatures[aFeature] + aWeightIf.Weight;
//    //                }
//    //                else
//    //                { 
//    //                    _rgLogisticFeatures.Add(aFeature, aWeightIf.Weight); 
//    //                }
//    //            } 
//    //        }
//    //        get
//    //        {
//    //            return _featureGenerator;
//    //        }
//    //    } 
//    //    private Dictionary<IHashableFeature, double> _rgLogisticFeatures = null; 

//    //    override public object Evaluate(object entity) 
//    //    {
//    //        double rProbability;
//    //        if (_featureGenerator == null)
//    //        {
//    //            rProbability = (double)base.Evaluate(entity);
//    //        } 
//    //        else 
//    //        {
//    //            SpecialFunctions.CheckCondition(_rgLogisticFeatures != null); //!!!raise error - this should have been set 

//    //            double rTotal = 0;
//    //            Set<IHashableFeature> entityFeatureSet = _featureGenerator(entity);
//    //            Debug.Assert(!entityFeatureSet.Contains(Logistic.AlwaysTrue)); //real assert
//    //            foreach (IHashableFeature feature in entityFeatureSet)
//    //            { 
//    //                Debug.Assert(feature != Logistic.AlwaysTrue); // real assert 
//    //                if (_rgLogisticFeatures.ContainsKey(feature))
//    //                { 
//    //                    rTotal += (double)_rgLogisticFeatures[feature];
//    //                }
//    //            }
//    //            rTotal += Offset;

//    //            rProbability = TheFunction(rTotal); 
//    //        } 
//    //        return rProbability;
 
//    //    }

//    //    [XmlIgnore]
//    //    public double Offset
//    //    {
//    //        get 
//    //        { 
//    //            return _rgLogisticFeatures[AlwaysTrue];
//    //        } 
//    //        set
//    //        {
//    //            _rgLogisticFeatures[AlwaysTrue] = value;
//    //        }
//    //    }
 
//    //    static public IHashableFeature AlwaysTrue = new And(); 
//    //    static string Header = SpecialFunctions.CreateTabString("Weight", "|Weight|", "Hla", "AA", "AAProp", "Expression");
//    //    static string ProbabilityExpressionWithoutOffset = "Exp(Total)/(1 + Exp(Total))"; 
//    //    public override string ToString()
//    //    {
//    //        StringBuilder sb = new StringBuilder();
//    //        sb.AppendFormat("{0}\n\n{1}\n\t\t\t\n", ProbabilityExpressionWithoutOffset, Header);
//    //        foreach (WeightIf weightIf in FeatureCollection)
//    //        { 
//    //            sb.AppendLine(WeightIfToString(weightIf)); 
//    //        }
//    //        return sb.ToString(); 
//    //    }

//    //    private string WeightIfToString(WeightIf weightIf)
//    //    {
//    //        string hla = FindFeatureOrEmpty(weightIf.FeatureCollection[0], typeof(IsHla));
//    //        string aa = FindFeatureOrEmpty(weightIf.FeatureCollection[0], typeof(IsAA)); 
//    //        string prop = FindFeatureOrEmpty(weightIf.FeatureCollection[0], typeof(HasAAProp)); 
//    //        string expression = weightIf.FeatureCollection[0].ToString();
//    //        return string.Format(SpecialFunctions.CreateTabString(weightIf.Weight, Math.Abs(weightIf.Weight), hla, aa, prop, expression)); 
//    //    }

//    //    //!!!should check that no-unknown types are here, too
//    //    private string FindFeatureOrEmpty(Feature feature, Type type)
//    //    {
//    //        //Debug.Assert(feature is And || feature is IsHla || feature is IsAA || feature is HasAAProp); //!!!raise error 
//    //        if (feature.GetType() == type) //!!!use subsumes?? 
//    //        {
//    //            return feature.ToString(); 
//    //        }
//    //        if (feature is And)
//    //        {
//    //            foreach (Feature featurex in feature.FeatureCollection)
//    //            {
//    //                string s = FindFeatureOrEmpty(featurex, type); 
//    //                if (s != "") 
//    //                {
//    //                    return s; 
//    //                }
//    //            }
//    //        }
//    //        return "";
//    //    }
 
//    //    static internal void ReportHeader(TextWriter modelStream) 
//    //    {
//    //        modelStream.WriteLine("{0}\t{1}", "iCrossVal", Header); 
//    //    }


//    //    public void Report(string fileName, int iCrossVal)
//    //    {
//    //        using (TextWriter modelStream = File.CreateText(fileName)) 
//    //        { 
//    //            Logistic.ReportHeader(modelStream);
 
//    //            foreach (WeightIf weightIf in FeatureCollection)
//    //            {
//    //                modelStream.WriteLine("{0}\t{1}", iCrossVal, WeightIfToString(weightIf));
//    //            }
//    //        }
//    //    } 
//    //} 

 

//    //!!!Change these to use the BindingEnergy feature and a "LessThan" feature
//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class BindingLessThan1 : BindingLessThan
//    { 
//        public BindingLessThan1() 
//            : this(double.NaN)
//        { 
//        }
//        public BindingLessThan1(double cutoff)
//            : base(1, cutoff)
//        {
//        }
//    } 
 
//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)] 
//    public class IsZero6Supertype : EntityFeature, IHashableFeature
//    {

//        public IsZero6Supertype()
//            : this(null)
//        { 
//        } 

//        public IsZero6Supertype(HlaToLength hlaToLength) 
//        {
//            if (hlaToLength == null)
//            {
//                Zero6Supertype = "";
//            }
//            else 
//            { 

//                Zero6Supertype = hlaToLength.ToZero6SupertypeString(); 
//            }
//        }

//        [XmlAttribute("zero6Supertype")]
//        public string Zero6Supertype;
 
 

//        public bool FeatureFunction(MerAndHlaToLength merAndHlaToLength) 
//        {
//            bool b = Zero6Supertype == merAndHlaToLength.HlaToLength.ToZero6SupertypeString();
//            return b;
//        }

//        public override int GetHashCode() 
//        { 
//            return Zero6Supertype.GetHashCode() ^ "IsZero6Supertype".GetHashCode();
//        } 

//        public override bool Equals(object obj)
//        {
//            IsZero6Supertype other = obj as IsZero6Supertype;
//            if (null == other)
//            { 
//                return false; 
//            }
//            else 
//            {
//                return Zero6Supertype.Equals(other.Zero6Supertype);
//            }
//        }

//        public override string ToString() 
//        { 
//            return string.Format("Zero6Supertype={0}", Zero6Supertype);
//        } 


//    }


//    [Serializable] 
//    [XmlRoot(Namespace = "", IsNullable = false)] 
//    public class BindingLessThan2 : BindingLessThan
//    { 
//        public BindingLessThan2()
//            : this(double.NaN)
//        {
//        }
//        public BindingLessThan2(double cutoff)
//            : base(2, cutoff) 
//        { 
//        }
//    } 

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class BindingLessThan3 : BindingLessThan
//    {
//        public BindingLessThan3() 
//            : this(double.NaN) 
//        {
//        } 
//        public BindingLessThan3(double cutoff)
//            : base(3, cutoff)
//        {
//        }
//    }
 
//    [Serializable] 
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class BindingLessThan4 : BindingLessThan 
//    {
//        public BindingLessThan4()
//            : this(double.NaN)
//        {
//        }
//        public BindingLessThan4(double cutoff) 
//            : base(4, cutoff) 
//        {
//        } 
//    }

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class BindingLessThan5 : BindingLessThan
//    { 
//        public BindingLessThan5() 
//            : this(double.NaN)
//        { 
//        }
//        public BindingLessThan5(double cutoff)
//            : base(5, cutoff)
//        {
//        }
//    } 
 

//    [Serializable] 
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class BindingLessThan : EntityFeature, IHashableFeature
//    {

//        public BindingLessThan()
//            : this(int.MinValue, double.NaN) 
//        { 
//        }
 
//        public BindingLessThan(int crossNumber, double cutoff)
//        {
//            CrossNumber = crossNumber;
//            Cutoff = cutoff;
//        }
 
//        [XmlAttribute("cutoff")] 
//        public double Cutoff;
 
//        [XmlIgnore]
//        public int CrossNumber;


//        public bool FeatureFunction(MerAndHlaToLength merAndHlaToLength)
//        { 
//            double energy = GetBindingEnergy(CrossNumber, merAndHlaToLength); 
//            bool b = energy < Cutoff;
//            return b; 
//        }

//        public override int GetHashCode()
//        {
//            return Cutoff.GetHashCode() ^ "BindingLessThan".GetHashCode();
//        } 
 
//        public override bool Equals(object obj)
//        { 
//            BindingLessThan other = obj as BindingLessThan;
//            if (other == null)
//            {
//                return false;
//            }
//            else 
//            { 
//                return Cutoff.Equals(other.Cutoff) && CrossNumber.Equals(other.CrossNumber);
//            } 
//        }

//        public override string ToString()
//        {
//            return string.Format("BindingEnergyLessThan {0}", Cutoff);
//        } 
 

//        static private Dictionary<int, Dictionary<string, double>> BindingEnergyTable = null; 
//        internal static double GetBindingEnergy(GeneratorType bindingType, MerAndHlaToLength merAndHlaToLength)
//        {
//            switch (bindingType)
//            {
//                case GeneratorType.Binding1:
//                    return GetBindingEnergy(1, merAndHlaToLength); 
//                case GeneratorType.Binding2: 
//                    return GetBindingEnergy(2, merAndHlaToLength);
//                case GeneratorType.Binding3: 
//                    return GetBindingEnergy(3, merAndHlaToLength);
//                case GeneratorType.Binding4:
//                    return GetBindingEnergy(4, merAndHlaToLength);
//                case GeneratorType.Binding5:
//                    return GetBindingEnergy(5, merAndHlaToLength);
//                default: 
//                    SpecialFunctions.CheckCondition(false); 
//                    return double.NaN;
//            } 

//        }
//        internal static double GetBindingEnergy(int crossNumber1To5, MerAndHlaToLength merAndHlaToLength)
//        {
//            return GetBindingEnergy(crossNumber1To5, merAndHlaToLength.Mer + "/" + merAndHlaToLength.HlaToLength);
//        } 
//        internal static double GetBindingEnergy(int crossNumber1To5, string merSlashHlaToLength) 
//        {
//            if (BindingEnergyTable == null) 
//            {
//                BindingEnergyTable = CreateBindingEnergyTable();
//            }
//            return BindingEnergyTable[crossNumber1To5][merSlashHlaToLength];
//        }
 
//        private static Dictionary<int, Dictionary<string, double>> CreateBindingEnergyTable() 
//        {
//            Dictionary<int, Dictionary<string, double>> table = new Dictionary<int, Dictionary<string, double>>(); 
//            for (int i = 1; i <= 5; ++i)
//            {
//                Dictionary<string, double> subTable = new Dictionary<string, double>();
//                table.Add(i, subTable);
//                foreach (string when in new string[] { "Train", "Test" })
//                { 
//                    string valueFile = string.Format("{0}_5_CrossVal_Bind_Est{1}", when, i); 
//                    string merFile = string.Format("ManuelFeb24{0}_5_CrossVal_{1}.txt", when, i);
//                    foreach (KeyValuePair<Dictionary<string, string>, Dictionary<string, string>> rowLeftAndRowRight 
//                        in SpecialFunctions.EnumerateTwo(
//                                Predictor.TabFileTableNoHeaderInFile(merFile, "IsEpitope	Sequence	Hla", false),
//                                Predictor.TabFileTableNoHeaderInFile(valueFile, "Energy", false)))
//                    {
//                        string sequence = rowLeftAndRowRight.Key["Sequence"];
//                        string hla = rowLeftAndRowRight.Key["Hla"]; 
//                        double energy = double.Parse(rowLeftAndRowRight.Value["Energy"]); 

//                        subTable.Add(sequence + "/" + hla, energy); 
//                    }

//                }
//            }
//            return table;
//        } 
//    } 

 
//    //!!!Change these to use the BindingEnergy feature and a "Energy" feature
//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class BindingEnergy1 : BindingEnergy
//    {
//        public BindingEnergy1() 
//            : base() 
//        {
//        } 
//    }

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class BindingEnergy2 : BindingEnergy
//    { 
//        public BindingEnergy2() 
//            : base(2)
//        { 
//        }
//    }

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class BindingEnergy3 : BindingEnergy 
//    { 
//        public BindingEnergy3()
//            : base(3) 
//        {
//        }
//    }

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)] 
//    public class BindingEnergy4 : BindingEnergy 
//    {
//        public BindingEnergy4() 
//            : base(4)
//        {
//        }
//    }

//    [Serializable] 
//    [XmlRoot(Namespace = "", IsNullable = false)] 
//    public class BindingEnergy5 : BindingEnergy
//    { 
//        public BindingEnergy5()
//            : base(5)
//        {
//        }
//    }
 
 
//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)] 
//    public class BindingEnergy : EntityFeature, IHashableFeature
//    {

//        public BindingEnergy()
//            : this(int.MinValue)
//        { 
//        } 

//        public BindingEnergy(int crossNumber) 
//        {
//            CrossNumber = crossNumber;
//        }

//        [XmlIgnore]
//        public int CrossNumber; 
 

//        public double FeatureFunction(MerAndHlaToLength merAndHlaToLength) 
//        {
//            double energy = BindingLessThan.GetBindingEnergy(CrossNumber, merAndHlaToLength);
//            return energy;
//        }

//        public override int GetHashCode() 
//        { 
//            return CrossNumber ^ "BindingEnergy".GetHashCode();
//        } 

//        public override bool Equals(object obj)
//        {
//            BindingEnergy other = obj as BindingEnergy;
//            if (other == null)
//            { 
//                return false; 
//            }
//            else 
//            {
//                return CrossNumber.Equals(other.CrossNumber);
//            }
//        }

//        public override string ToString() 
//        { 
//            return string.Format("BindingEnergy");
//        } 
//    }

//    [Serializable]
//    [XmlRoot(Namespace = "", IsNullable = false)]
//    public class SubtractConstant : Feature, IHashableFeature
//    { 
 
//        public SubtractConstant()
//        { 
//        }

//        public SubtractConstant(Feature subFeature, double constant)
//        {
//            FeatureCollection = new Feature[] { subFeature };
//            Constant = constant; 
//        } 

 
//        [XmlAttribute("constant")]
//        public double Constant;


//        public double FeatureFunction(double inputNumber)
//        { 
//            return inputNumber - Constant; 
//        }
 
//        public override int GetHashCode()
//        {
//            return Constant.GetHashCode() ^ "BindingEnergy".GetHashCode();
//        }

//        public override bool Equals(object obj) 
//        { 
//            SubtractConstant other = obj as SubtractConstant;
//            if (other == null) 
//            {
//                return false;
//            }
//            else
//            {
//                if (Constant != other.Constant || FeatureCollection.Length != other.FeatureCollection.Length) 
//                { 
//                    return false;
//                } 
//                if (FeatureCollection.Length > 0)
//                {
//                    return FeatureCollection[0].Equals(other.FeatureCollection[0]);
//                }
//                return true;
//            } 
//        } 

//        public override string ToString() 
//        {
//            if (FeatureCollection.Length == 0)
//            {
//                return "";
//            }
//            if (Constant <= 0) 
//            { 
//                return string.Format("{0}+{1}", FeatureCollection[0], -Constant);
//            } 
//            else
//            {
//                return string.Format("{0}-{1}", FeatureCollection[0], Constant);
//            }
//        }
 
 
//    }
 
//}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
