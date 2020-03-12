using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Adapt.LearningWorkbench;
using System.Xml.Serialization;
using System.Diagnostics;
using Msr.Adapt.HighLevelFeatures; 
using Msr.Mlas.SpecialFunctions; 
using VirusCount;
using EpipredLib; 
using System.Text.RegularExpressions;

namespace ProcessingPrediction
{

 
    ////!!!If this is the same as in epitopeFeatures.cs why not just have it once? 
    //[Serializable]
    //[XmlRoot(Namespace = "", IsNullable = false)] 
    //public class And : Feature, IHashableFeature
    //{
    //    public And()
    //        : this(null, null)
    //    {
    //    } 
 
    //    //!!!make work for any number of arguments
    //    private And(IHashableFeature boolfeature1, IHashableFeature boolfeature2) 
    //    {
    //        if (boolfeature1 != null && boolfeature2 != null)
    //        {
    //            base.FeatureCollection = new Feature[] { (Feature)boolfeature1, (Feature)boolfeature2 };
    //        }
    //    } 
 
    //    //!!!make work for any number of arguments
    //    public static And GetInstance(IHashableFeature boolfeature1, IHashableFeature boolfeature2) 
    //    {
    //        return new And(boolfeature1, boolfeature2);
    //    }

    //    public bool FeatureFunction(params object[] bools)
    //    { 
    //        foreach (bool aBool in bools) 
    //        {
    //            if (!aBool) 
    //            {
    //                return false;
    //            }
    //        }

    //        return true; 
    //    } 

    //    public override int GetHashCode() 
    //    {
    //        int hashCode = "And".GetHashCode();
    //        foreach (IHashableFeature feature in FeatureCollection)
    //        {
    //            hashCode ^= feature.GetHashCode();
    //        } 
    //        return hashCode; 
    //    }
 
    //    public override bool Equals(object obj)
    //    {
    //        And other = obj as And;
    //        if (other == null)
    //        {
    //            return false; 
    //        } 
    //        else
    //        { 
    //            if (FeatureCollection.Length != other.FeatureCollection.Length)
    //            {
    //                Debug.Assert(obj.ToString() != ToString()); // real assert
    //                return false;
    //            }
    //            for (int iFeature = 0; iFeature < FeatureCollection.Length; ++iFeature) 
    //            { 
    //                if (!FeatureCollection[iFeature].Equals(other.FeatureCollection[iFeature]))
    //                { 
    //                    Debug.Assert(obj.ToString() != ToString()); // real assert
    //                    return false;
    //                }
    //            }
    //            Debug.Assert(obj.ToString() == ToString()); // real assert
    //            return true; 
    //        } 
    //    }
 
    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        foreach (Feature feature in FeatureCollection)
    //        {
    //            if (sb.Length != 0) 
    //            { 
    //                sb.Append(" && ");
    //            } 
    //            sb.Append(feature.ToString());
    //        }
    //        return sb.ToString();
    //    }
    //}
 
 
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class In : Feature, IHashableFeature
    {

        public In()
        {
            FeatureCollection = null; 
        } 

        static public In GetInstance(string aaSeq, IHashableFeature feature) 
        {
            In inFeature = new In();
            inFeature.AASeq = aaSeq;
            inFeature.FeatureCollection = new Feature[] { (Feature)feature };
            return inFeature;
        } 
 
        [XmlAttribute("aaSeq")]
        public string AASeq = ""; 

        public bool FeatureFunction(string region)
        {
            bool b = (region.Contains(AASeq));
            return b;
        } 
 
        public override int GetHashCode()
        { 
            return "AASeqIn".GetHashCode() ^ AASeq.GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode());
        }

        public override bool Equals(object obj)
        {
            In other = obj as In; 
            if (other == null) 
            {
                return false; 
            }
            else
            {
                if (AASeq != other.AASeq || FeatureCollection.Length != other.FeatureCollection.Length)
                {
                    Debug.Assert(obj.ToString() != ToString()); // real assert 
                    return false; 
                }
                if (FeatureCollection.Length > 0) 
                {
                    bool b = FeatureCollection[0].Equals(other.FeatureCollection[0]);
                    Debug.Assert(b == (obj.ToString() == ToString())); // real assert
                    return b;

                } 
                Debug.Assert(obj.ToString() == ToString()); // real assert 
                return true;
            } 
        }

        public override string ToString()
        {
            if (FeatureCollection.Length == 0)
            { 
                return ""; 
            }
            return string.Format("{0} in {1}", AASeq, FeatureCollection[0]); 
        }

        public static IEnumerable<IHashableFeature> GetAASeqInRegionInstance(int k, object entity, IHashableFeature regionFeature)
        {
            string region = (string)((Feature)regionFeature).Evaluate(entity);
            Set<string> aaSeqSet = Set<string>.GetInstance(SpecialFunctions.SubstringEnumeration(region, k)); 
            foreach (string aaSeq in aaSeqSet) 
            {
                In inFeature = In.GetInstance(aaSeq, regionFeature); 
                Debug.Assert((bool)inFeature.Evaluate(entity)); // real assert - must only generate true features
                yield return inFeature;
            }
        }

    } 
 

    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class NFlank : EntityFeature, IHashableFeature
    {
        public NFlank()
        {
        } 
 
        //Dropping strong typing so that can handle both NEC and Pair<NEC,HLA>
        public string FeatureFunction(object entity) 
        {
            NEC nec = entity as NEC;
            if (null == nec)
            {
                nec = ((Pair<NEC, Hla>)entity).First;
            } 
            return nec.N; 
        }
 
        public static NFlank GetInstance()
        {
            NFlank region = new NFlank();
            return region;
        }
 
        public override int GetHashCode() 
        {
            return "NFlank".GetHashCode(); 
        }

        public override bool Equals(object obj)
        {
            NFlank other = obj as NFlank;
            if (other == null) 
            { 
                return false;
            } 
            else
            {
                Debug.Assert(obj.ToString() == ToString()); // real assert
                return true;
            }
        } 
 
        public override string ToString()
        { 
            return string.Format("NFlank");
        }


    }
 
    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class CFlank : EntityFeature, IHashableFeature 
    {
        public CFlank()
        {
        }

        //Dropping strong typing so that can handle both NEC and Pair<NEC,HLA> 
        public string FeatureFunction(object entity) 
        {
            NEC nec = entity as NEC; 
            if (null == nec)
            {
                nec = ((Pair<NEC, Hla>)entity).First;
            }
            return nec.C;
        } 
 

        public static CFlank GetInstance() 
        {
            CFlank region = new CFlank();
            return region;
        }

        public override int GetHashCode() 
        { 
            return "CFlank".GetHashCode();
        } 

        public override bool Equals(object obj)
        {
            CFlank other = obj as CFlank;
            if (other == null)
            { 
                return false; 
            }
            else 
            {
                Debug.Assert(obj.ToString() == ToString()); // real assert
                return true;
            }
        }
 
        public override string ToString() 
        {
            return string.Format("CFlank"); 
        }
    }

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Epitope : EntityFeature, IHashableFeature 
    { 
        public Epitope()
        { 
        }

        //Dropping strong typing so that can handle both NEC and Pair<NEC,HLA>
        public string FeatureFunction(object entity)
        {
            NEC nec = entity as NEC; 
            if (null == nec) 
            {
                nec = ((Pair<NEC, Hla>)entity).First; 
            }
            return nec.E;
        }


        public static Epitope GetInstance() 
        { 
            Epitope region = new Epitope();
            return region; 
        }

        public override int GetHashCode()
        {
            return "Epitope".GetHashCode();
        } 
 
        public override bool Equals(object obj)
        { 
            Epitope other = obj as Epitope;
            if (other == null)
            {
                return false;
            }
            else 
            { 
                Debug.Assert(obj.ToString() == ToString()); // real assert
                return true; 
            }
        }

        public override string ToString()
        {
            return string.Format("Epitope"); 
        } 
    }
 

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class CTermE : EntityFeature, IHashableFeature
    {
        public CTermE() 
        { 
        }
 
        public string FeatureFunction(NEC nec)
        {
            string aaSeq = nec.E.Substring(nec.E.Length - 1);
            return aaSeq;
        }
 
        public static CTermE GetInstance() 
        {
            CTermE region = new CTermE(); 
            return region;
        }

        public override int GetHashCode()
        {
            return "CTermE".GetHashCode(); 
        } 

        public override bool Equals(object obj) 
        {
            CTermE other = obj as CTermE;
            if (other == null)
            {
                return false;
            } 
            else 
            {
                Debug.Assert(obj.ToString() == ToString()); // real assert 
                return true;
            }
        }

        public override string ToString()
        { 
            return string.Format("CTermE"); 
        }
 
    }

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class NTermE : EntityFeature, IHashableFeature
    { 
        public NTermE() 
        {
        } 

        public string FeatureFunction(NEC nec)
        {
            string aaSeq = nec.E.Substring(0, 2);
            return aaSeq;
        } 
 
        internal static NTermE GetInstance()
        { 
            NTermE region = new NTermE();
            return region;
        }

        public override int GetHashCode()
        { 
            return "NTermE".GetHashCode(); 
        }
 
        public override bool Equals(object obj)
        {
            NTermE other = obj as NTermE;
            if (other == null)
            {
                return false; 
            } 
            else
            { 
                Debug.Assert(obj.ToString() == ToString()); // real assert
                return true;
            }
        }

        public override string ToString() 
        { 
            return string.Format("NTermE");
        } 


        public static IEnumerable<IHashableFeature> GetAndNTermENotNFlankEnumeration(NEC nec)
        {
            NTermE nTermEFeature = NTermE.GetInstance();
            string nTermERegion = nTermEFeature.FeatureFunction(nec); 
            Set<string> aaSeqSet = Set<string>.GetInstance(SpecialFunctions.SubstringEnumeration(nTermERegion, 1)); 
            foreach (string aaSeq in aaSeqSet)
            { 
                Not notFeature = Not.GetInstance(In.GetInstance(aaSeq, NFlank.GetInstance()));
                //Only generate the feature when the amino acid is not in NFlnak
                if ((bool)notFeature.Evaluate(nec))
                {
                    In inFeature = In.GetInstance(aaSeq, nTermEFeature);
                    And andFeature = And.GetInstance(inFeature, notFeature); 
                    Debug.Assert((bool)andFeature.Evaluate(nec)); // real assert - must only generate true features 
                    yield return andFeature;
                } 
            }
        }


        public static IEnumerable<IHashableFeature> GetAndNFlankNotNTermEEnumeration(NEC nec)
        { 
            NFlank nFlankFeature = NFlank.GetInstance(); 
            string nFlankRegion = nFlankFeature.FeatureFunction(nec);
            Set<string> aaSeqSet = Set<string>.GetInstance(SpecialFunctions.SubstringEnumeration(nFlankRegion, 1)); 
            foreach (string aaSeq in aaSeqSet)
            {
                Not notFeature = Not.GetInstance(In.GetInstance(aaSeq, NTermE.GetInstance()));
                //Only generate the feature when the amino acid is not in NTermE
                if ((bool)notFeature.Evaluate(nec))
                { 
                    In inFeature = In.GetInstance(aaSeq, nFlankFeature); 
                    And andFeature = And.GetInstance(inFeature, notFeature);
                    Debug.Assert((bool)andFeature.Evaluate(nec)); // real assert - must only generate true features 
                    yield return andFeature;
                }
            }
        }

    } 
 
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class AnyIn : Feature, IHashableFeature
    {

        public AnyIn()
        {
            FeatureCollection = null; 
        } 

        static public AnyIn GetInstance(string aaString, Feature feature) 
        {
            AnyIn anyInFeature = new AnyIn();
            anyInFeature.AAString = aaString;
            anyInFeature.FeatureCollection = new Feature[] { (Feature)feature };
            return anyInFeature;
        } 
 
        [XmlAttribute("aaString")]
        public string AAString 
        {
            get
            {
                return _aaString;
            }
            set 
            { 
                _aaString = value;
                _aaStringAsCharArray = value.ToCharArray(); 
            }
        }

        private string _aaString = "";
        private char[] _aaStringAsCharArray = "".ToCharArray();
 
        public bool FeatureFunction(string region) 
        {
            bool b = (-1 < region.IndexOfAny(_aaStringAsCharArray)); 
            return b;
        }

        public override int GetHashCode()
        {
            return "AAStringAnyIn".GetHashCode() ^ AAString.GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode()); 
        } 

        public override bool Equals(object obj) 
        {
            AnyIn other = obj as AnyIn;
            if (other == null)
            {
                return false;
            } 
            else 
            {
                if (AAString != other.AAString || FeatureCollection.Length != other.FeatureCollection.Length) 
                {
                    Debug.Assert(obj.ToString() != ToString()); // real assert
                    return false;
                }
                if (FeatureCollection.Length > 0)
                { 
                    bool b = FeatureCollection[0].Equals(other.FeatureCollection[0]); 
                    Debug.Assert(b == (obj.ToString() == ToString())); // real assert
                    return b; 

                }
                Debug.Assert(obj.ToString() == ToString()); // real assert
                return true;
            }
        } 
 
        public override string ToString()
        { 
            if (FeatureCollection.Length == 0)
            {
                return "";
            }
            return string.Format("{0} anyIn {1}", AAString, FeatureCollection[0]);
        } 
 
        public static IEnumerable<IHashableFeature> GetNFlankAny1AndNTermEAny2(NEC nec, string nflankAny, string nTermEAny)
        { 
            NFlank nFlankFeature = NFlank.GetInstance();
            string nFlankRegion = nFlankFeature.FeatureFunction(nec);

            if (-1 == nFlankRegion.IndexOfAny(nflankAny.ToCharArray()))
            {
                yield break; 
            } 

            NTermE nTermEFeature = NTermE.GetInstance(); 
            string nTermERegion = nTermEFeature.FeatureFunction(nec);

            if (-1 == nTermERegion.IndexOfAny(nTermEAny.ToCharArray()))
            {
                yield break;
            } 
 
            AnyIn anyFeature1 = AnyIn.GetInstance(nflankAny, nFlankFeature);
            AnyIn anyFeature2 = AnyIn.GetInstance(nTermEAny, nTermEFeature); 
            And andFeature = And.GetInstance(anyFeature1, anyFeature2);
            Debug.Assert((bool)andFeature.Evaluate(nec)); // real assert - must only generate true features
            yield return andFeature;
        }

    } 
 

    //[Serializable] 
    //[XmlRoot(Namespace = "", IsNullable = false)]
    //public class Not : Feature, IHashableFeature
    //{
    //    public Not()
    //        : this(null)
    //    { 
    //    } 

    //    private Not(IHashableFeature boolfeature) 
    //    {
    //        if (boolfeature != null)
    //        {
    //            base.FeatureCollection = new Feature[] { (Feature)boolfeature };
    //        }
    //    } 
 
    //    //!!!make work for any number of arguments
    //    public static Not GetInstance(IHashableFeature boolfeature) 
    //    {
    //        return new Not(boolfeature);
    //    }

    //    public bool FeatureFunction(bool aBool)
    //    { 
    //        return !aBool; 
    //    }
 
    //    public override int GetHashCode()
    //    {
    //        return "Not".GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode());
    //    }

    //    public override bool Equals(object obj) 
    //    { 
    //        Not other = obj as Not;
    //        if (other == null) 
    //        {
    //            return false;
    //        }
    //        else
    //        {
    //            if (!FeatureCollection.Length.Equals(other.FeatureCollection.Length)) 
    //            { 
    //                Debug.Assert(obj.ToString() != ToString());
    //                return false; 
    //            }
    //            if (FeatureCollection.Length == 0)
    //            {
    //                Debug.Assert(obj.ToString() == ToString());
    //                return true;
    //            } 
 
    //            bool b = FeatureCollection[0].Equals(other.FeatureCollection[0]);
    //            Debug.Assert(b == (obj.ToString() == ToString())); // real assert 
    //            return b;
    //        }
    //    }

    //    public override string ToString()
    //    { 
    //        if (FeatureCollection.Length == 0) 
    //        {
    //            return ""; 
    //        }

    //        return string.Format("not {0}", FeatureCollection[0]);
    //    }
    //}
 
    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class SubSeq : Feature, IHashableFeature 
    {

        public SubSeq()
        {
            FeatureCollection = null;
        } 
 
        static public SubSeq GetInstance(int posLo, int posHi, bool fromLeft, IHashableFeature feature)
        { 
            SubSeq subSeq = new SubSeq();
            subSeq.PosLo = posLo;
            subSeq.PosHi = posHi;
            subSeq.FromLeft = fromLeft;
            subSeq.FeatureCollection = new Feature[] { (Feature)feature };
            return subSeq; 
        } 

        [XmlAttribute("posLo")] 
        public int PosLo = int.MinValue;

        [XmlAttribute("posHi")]
        public int PosHi = int.MinValue;

        [XmlAttribute("fromLeft")] 
        public bool FromLeft = true; 

        public string FeatureFunction(string region) 
        {
            if (FromLeft)
            {
                return region.Substring(PosLo - 1, PosHi - PosLo + 1);
            }
            else 
            { 
                return region.Substring(region.Length - PosHi, PosHi - PosLo + 1);
            } 
        }

        public override int GetHashCode()
        {
            int hashCode = "SubSeq".GetHashCode() ^ PosLo.GetHashCode() ^ (~PosHi.GetHashCode()) ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode());
            if (FromLeft) 
            { 
                hashCode ^= "FromLeft".GetHashCode();
            } 
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            SubSeq other = obj as SubSeq; 
            if (other == null) 
            {
                return false; 
            }
            else
            {
                if (!PosLo.Equals(other.PosLo)
                    || !PosHi.Equals(other.PosHi)
                    || !FromLeft.Equals(other.FromLeft) 
                    || !FeatureCollection.Length.Equals(other.FeatureCollection.Length)) 
                {
                    Debug.Assert(obj.ToString() != ToString()); 
                    return false;
                }
                if (FeatureCollection.Length == 0)
                {
                    Debug.Assert(obj.ToString() == ToString());
                    return true; 
                } 
                bool b = FeatureCollection[0].Equals(other.FeatureCollection[0]);
                Debug.Assert(b == (obj.ToString() == ToString())); // real assert 
                return b;
            }
        }

        public override string ToString()
        { 
            if (FeatureCollection.Length == 0) 
            {
                return ""; 
            }
            if (FromLeft)
            {
                if (PosLo == PosHi)
                {
                    return string.Format("{0}[@{1}]", FeatureCollection[0], PosLo); 
                } 
                else
                { 
                    return string.Format("{0}[@{1}-{2}]", FeatureCollection[0], PosLo, PosHi);
                }
            }
            else
                if (PosLo == PosHi)
                { 
                    return string.Format("{0}[{1}@]", FeatureCollection[0], PosLo); 
                }
                else 
                {
                    return string.Format("{0}[{1}-{2}@]", FeatureCollection[0], PosHi, PosLo);
                }
        }

        public static IEnumerable<IHashableFeature> GetInSubSeqEnumeration(IHashableFeature regionFeature, bool fromLeft, int length, object entity) 
        { 
            return GetInSubSeqEnumeration(regionFeature, fromLeft,
                delegate(IHashableFeature regionFeatureX) { return FromOneByOne(regionFeatureX, fromLeft, length, entity); }, 
                entity, length);
        }

        private static IEnumerable<IHashableFeature> FromOneByOne(IHashableFeature regionFeature, bool fromLeft, int length, object entity)
        {
            string region = (string)((Feature)regionFeature).Evaluate(entity); 
            for (int aa1Lo = 1; aa1Lo <= region.Length - length + 1; ++aa1Lo) 
            {
                SubSeq subSeq = SubSeq.GetInstance(aa1Lo, aa1Lo + length - 1, fromLeft, regionFeature); 
                yield return subSeq;
            }
        }


        //public static IEnumerable<IHashableFeature> GetInSubSeqEnumeration(IHashableFeature regionFeature, 
        //    bool fromLeft, IEnumerable<KeyValuePair<int, int>> startAndEndEnumeration, object entity, int littleLength) 
        //{
        //    foreach (KeyValuePair<int, int> startAndEnd in startAndEndEnumeration) 
        //    {
        //        SubSeq subSeq = SubSeq.GetInstance(startAndEnd.Key, startAndEnd.Value, fromLeft, regionFeature);
        //        string aaSeqFull = (string)subSeq.Evaluate(entity);
        //        foreach (string aaSeqLittle in SpecialFunctions.SubstringEnumeration(aaSeqFull, littleLength))
        //        {
        //            In inFeature = In.GetInstance(aaSeqLittle, subSeq); 
        //            Debug.Assert((bool)inFeature.Evaluate(entity)); // real assert - must only generate true features 
        //            yield return inFeature;
        //        } 
        //    }
        //}


        public static IEnumerable<IHashableFeature> GetInSubSeqEnumeration(IHashableFeature regionFeature,
            bool fromLeft, Converter<IHashableFeature, IEnumerable<IHashableFeature>> regionFeatureToSubSeqEnumeration, object entity, int littleLength) 
        { 
            foreach (IHashableFeature epitopeSubSeq in regionFeatureToSubSeqEnumeration(regionFeature))
            { 
                string aaSeqFull = (string)((Feature)epitopeSubSeq).Evaluate(entity);
                foreach (string aaSeqLittle in SpecialFunctions.SubstringEnumeration(aaSeqFull, littleLength))
                {
                    In inFeature = In.GetInstance(aaSeqLittle, epitopeSubSeq);
                    Debug.Assert((bool)inFeature.Evaluate(entity)); // real assert - must only generate true features
                    yield return inFeature; 
                } 
            }
        } 


        public static IEnumerable<IHashableFeature> GetInPropertySubSeqEnumeration(IHashableFeature regionFeature, bool fromLeft, int length, Pair<NEC, Hla> entity)
        {
            return GetInPropertySubSeqEnumeration(regionFeature, fromLeft, FromOneByOne(regionFeature, fromLeft, length, entity), entity, length);
        } 
 

        public static IEnumerable<IHashableFeature> GetInPropertySubSeqEnumeration(IHashableFeature regionFeature, bool fromLeft, IEnumerable<IHashableFeature> epitopeSubSeqEnumeration, Pair<NEC, Hla> entity, int littleLength) 
        {
            foreach (IHashableFeature epitopeSubSeq in epitopeSubSeqEnumeration)
            {
                string aaSeqFull = (string)((Feature)epitopeSubSeq).Evaluate(entity);
                foreach (string aaSeqLittle in SpecialFunctions.SubstringEnumeration(aaSeqFull, littleLength))
                { 
                    foreach (List<string> propertyCombination in KmerProperties.EveryPropertyCombination(aaSeqLittle)) 
                    {
                        string propertySeq = SpecialFunctions.Join(",", propertyCombination); 
                        InProperty feature = InProperty.GetInstance(propertySeq, epitopeSubSeq);
                        Debug.Assert((bool)feature.Evaluate(entity)); // real assert - must only generate true features
                        yield return feature;
                    }
                }
            } 
        } 

        //public static IEnumerable<IHashableFeature> GetInPropertySubSeqEnumeration(IHashableFeature regionFeature, bool fromLeft, int length, Pair<NEC, Hla> entity) 
        //{
        //    NEC nec = entity.First;

        //    string region = (string)((Feature)regionFeature).Evaluate(entity);
        //    for (int aa1Lo = 1; aa1Lo <= region.Length - length + 1; ++aa1Lo)
        //    { 
        //        SubSeq subSeq = SubSeq.GetInstance(aa1Lo, aa1Lo + length - 1, fromLeft, regionFeature); 
        //        string aaSeq = (string)subSeq.Evaluate(entity);
        //        foreach (List<string> propertyCombination in KmerProperties.EveryPropertyCombination(aaSeq)) 
        //        {
        //            string propertySeq = SpecialFunctions.Join(",", propertyCombination);
        //            InProperty feature = InProperty.GetInstance(propertySeq, subSeq);
        //            Debug.Assert((bool)feature.Evaluate(entity)); // real assert - must only generate true features
        //            yield return feature;
        //        } 
        //    } 
        //}
    } 

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class InProperty : Feature, IHashableFeature
    {
        public InProperty() 
        { 
            PropertySeq = "";
            FeatureCollection = null; 
        }


        static public InProperty GetInstance(string propertySeq, IHashableFeature subFeature)
        {
            InProperty feature = new InProperty(); 
            feature.PropertySeq = propertySeq; 
            feature.FeatureCollection = new Feature[] { (Feature)subFeature };
            return feature; 
        }

        [XmlAttribute("propertySeq")]
        public string PropertySeq
        {
            set 
            { 
                _propertySeq = value;
                if (value == "") 
                {
                    Regex = new Regex(".");
                }
                else
                {
                    StringBuilder sb = new StringBuilder(); 
                    foreach (string propertyName in value.Split(',')) 
                    {
                        Set<char> aaSet = VirusCount.KmerProperties.PropertyNameToAASet(propertyName); 
                        sb.AppendFormat("[{0}]", SpecialFunctions.Join("", aaSet));
                    }
                    Regex = new Regex(sb.ToString());
                }
            }
            get 
            { 
                return _propertySeq;
            } 
        }

        private string _propertySeq = "";
        private Regex Regex = null;

        public bool FeatureFunction(string region) 
        { 
            bool b = (Regex.IsMatch(region));
            return b; 
        }

        public override int GetHashCode()
        {
            return "InProperty".GetHashCode() ^ PropertySeq.GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode());
        } 
 
        public override bool Equals(object obj)
        { 
            InProperty other = obj as InProperty;
            if (other == null)
            {
                return false;
            }
            else 
            { 
                if (PropertySeq != other.PropertySeq || FeatureCollection.Length != other.FeatureCollection.Length)
                { 
                    Debug.Assert(obj.ToString() != ToString()); // real assert
                    return false;
                }
                if (FeatureCollection.Length > 0)
                {
                    bool b = FeatureCollection[0].Equals(other.FeatureCollection[0]); 
                    Debug.Assert(b == (obj.ToString() == ToString())); // real assert 
                    return b;
 
                }
                Debug.Assert(obj.ToString() == ToString()); // real assert
                return true;
            }
        }
 
        public override string ToString() 
        {
            if (FeatureCollection.Length == 0) 
            {
                return "";
            }
            return string.Format("{0} in {1}", PropertySeq, FeatureCollection[0]);
        }
 
        public static IEnumerable<IHashableFeature> GetPropertySeqInRegionInstance(int k, Pair<NEC, Hla> necAndHla, IHashableFeature regionFeature) 
        {
            NEC nec = necAndHla.First; 

            string region = (string)((Feature)regionFeature).Evaluate(necAndHla);
            Set<string> aaSeqSet = Set<string>.GetInstance(SpecialFunctions.SubstringEnumeration(region, k));
            foreach (string aaSeq in aaSeqSet)
            {
                foreach (List<string> propertyCombination in KmerProperties.EveryPropertyCombination(aaSeq)) 
                { 
                    string propertySeq = SpecialFunctions.Join(",", propertyCombination);
                    InProperty feature = InProperty.GetInstance(propertySeq, regionFeature); 
                    Debug.Assert((bool)feature.Evaluate(necAndHla)); // real assert - must only generate true features
                    yield return feature;
                }
            }
        }
 
    } 

    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Begin : Feature, IHashableFeature
    {

        public Begin()
        { 
            FeatureCollection = null; 
        }
 
        static public Begin GetInstance(IHashableFeature feature)
        {
            Begin begin = new Begin();
            begin.FeatureCollection = new Feature[] { (Feature)feature };
            return begin;
        } 
 
        public string FeatureFunction(string region)
        { 
            return region.Substring(0, 2);
        }

        public override int GetHashCode()
        {
            int hashCode = "Begin".GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode()); 
            return hashCode; 
        }
 
        public override bool Equals(object obj)
        {
            Begin other = obj as Begin;
            if (other == null)
            {
                return false; 
            } 
            if (FeatureCollection.Length == 0)
            { 
                Debug.Assert(obj.ToString() == ToString());
                return true;
            }
            bool b = FeatureCollection[0].Equals(other.FeatureCollection[0]);
            Debug.Assert(b == (obj.ToString() == ToString())); // real assert
            return b; 
        } 

        public override string ToString() 
        {
            if (FeatureCollection.Length == 0)
            {
                return "";
            }
            return string.Format("Begin({0})", FeatureCollection[0]); 
        } 
    }
 
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Middle : Feature, IHashableFeature
    {

        public Middle() 
        { 
            FeatureCollection = null;
        } 

        static public Middle GetInstance(IHashableFeature feature)
        {
            Middle middle = new Middle();
            middle.FeatureCollection = new Feature[] { (Feature)feature };
            return middle; 
        } 

        public string FeatureFunction(string region) 
        {
            return region.Substring(2, region.Length - 4);
        }

        public override int GetHashCode()
        { 
            int hashCode = "Middle".GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode()); 
            return hashCode;
        } 

        public override bool Equals(object obj)
        {
            Middle other = obj as Middle;
            if (other == null)
            { 
                return false; 
            }
            if (FeatureCollection.Length == 0) 
            {
                Debug.Assert(obj.ToString() == ToString());
                return true;
            }
            bool b = FeatureCollection[0].Equals(other.FeatureCollection[0]);
            Debug.Assert(b == (obj.ToString() == ToString())); // real assert 
            return b; 
        }
 
        public override string ToString()
        {
            if (FeatureCollection.Length == 0)
            {
                return "";
            } 
            return string.Format("Middle({0})", FeatureCollection[0]); 
        }
    } 

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class End : Feature, IHashableFeature
    {
 
        public End() 
        {
            FeatureCollection = null; 
        }

        static public End GetInstance(IHashableFeature feature)
        {
            End end = new End();
            end.FeatureCollection = new Feature[] { (Feature)feature }; 
            return end; 
        }
 
        public string FeatureFunction(string region)
        {
            return region.Substring(region.Length-2);
        }

        public override int GetHashCode() 
        { 
            int hashCode = "End".GetHashCode() ^ (FeatureCollection.Length == 0 ? 0 : FeatureCollection[0].GetHashCode());
            return hashCode; 
        }

        public override bool Equals(object obj)
        {
            End other = obj as End;
            if (other == null) 
            { 
                return false;
            } 
            if (FeatureCollection.Length == 0)
            {
                Debug.Assert(obj.ToString() == ToString());
                return true;
            }
            bool b = FeatureCollection[0].Equals(other.FeatureCollection[0]); 
            Debug.Assert(b == (obj.ToString() == ToString())); // real assert 
            return b;
        } 

        public override string ToString()
        {
            if (FeatureCollection.Length == 0)
            {
                return ""; 
            } 
            return string.Format("End({0})", FeatureCollection[0]);
        } 
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
