using System; 
using Msr.Adapt.LearningWorkbench;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions; 
using System.Collections.Specialized; 
//using System.Collections;
using System.Text; 
using Msr.Mlas.SpecialFunctions;
using System.Collections.Generic;
using System.IO;

//!!!get rid of all uses of "int", create constructors for everything, where it is needed have the construct take a feature(s) as input
namespace Msr.Adapt.HighLevelFeatures 
{ 
 	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)] 
	public class ConstantDate : Feature
	{
		[XmlAttribute]
		public DateTime DateTime = DateTime.MinValue;

 		public DateTime FeatureFunction() 
 		{ 
			return DateTime;
 		} 

		//TODO would it be possible to have the main method return the DateTime Field instead? or a property? it would make this a bit simplier.
	}


	[Serializable] 
	[XmlRoot(Namespace="", IsNullable=false)] 
	public class BeforeDate : Feature
 	{ 
 		public bool FeatureFunction(DateTime datetime1, DateTime datetime2)
		{
 			return (datetime1 < datetime2);
		}

	} 
 
	
    //[Serializable] 
    //[XmlRoot(Namespace="", IsNullable=false)]
    //public class Logistic : Feature
    //{
    //    public Logistic() : this(0.0, null)
    //    {
    //    } 
 
    //    public Logistic(double offset, Feature[] FeatureCollectionToSum)
    //    { 
    //        Offset = offset;
    //        base.FeatureCollection = FeatureCollectionToSum;
    //    }


    //    [XmlAttribute("probability")] 
    //    public string Probability 
    //    {
    //        get 
    //        {
    //            return string.Format("Exp(Total + {0})/(1 + Exp(Total + {0}))",Offset);
    //        }
    //        set
    //        {
    //            //Ignore, because you really can't set this 
    //        } 

    //    } 

    //    [XmlAttribute("offset")]
    //    public double Offset;


    //    [XmlAnyAttribute()] 
    //    public System.Xml.XmlAttribute[] AnyAttr; 

    //    public double FeatureFunction(params object[] products) 
    //    {
    //        double rTotal = 0;
    //        foreach (double rProduct in products)
    //        {
    //            rTotal += rProduct;
    //        } 
 

    //        return TheFunction(rTotal); 
    //    }

    //    protected double TheFunction(double rTotal)
    //    {
    //        double rExp = Math.Exp(rTotal + Offset);
    //        double rResult =  rExp / (1.0 + rExp); // the logistic aka logistic function (see http://cerebro.xu.edu/math/math120/01f/logistic.pdf) 
    //        return rResult; 
    //    }
 
    //    private FeaturesFromEntity _featuresFromEntity;
    //    //[XmlElement("FeaturesFromEntity")]
    //    [XmlIgnore]
    //    public FeaturesFromEntity FeaturesFromEntity
    //    {
    //        set 
    //        { 
    //            _featuresFromEntity = value;
    //            Debug.Assert(_rgLogisticFeatures == null); //!!!raise error - should not set this twice 
    //            Debug.Assert(FeatureCollection != null); //!!!raise error the features should be here
    //            _rgLogisticFeatures = new Hashtable();
    //            foreach (WeightIf aWeightIf in FeatureCollection) //!!!what if have non-boolean features
    //            {	
    //                Debug.Assert(aWeightIf.FeatureCollection.Length == 1); //!!!
    //                Feature aFeature = aWeightIf.FeatureCollection[0]; 
    //                string sXml = _featuresFromEntity.FeatureSerializer.ToXml(aFeature); 
    //                _rgLogisticFeatures.Add(sXml, aWeightIf.Weight);
    //            } 
    //        }
    //        get
    //        {
    //            return _featuresFromEntity;
    //        }
    //    } 
    //    private Hashtable _rgLogisticFeatures = null; 

    //    override public object Evaluate(object entity) 
    //    {
    //        double rProbability;
    //        if (_featuresFromEntity == null)
    //        {
    //            rProbability = (double) base.Evaluate(entity);
    //        } 
    //        else 
    //        {
    //            Debug.Assert(_rgLogisticFeatures != null); //!!!raise error - this should have been set 

    //            double rTotal = 0;
    //            IDictionary dictionaryEntityFeatures = FeaturesFromEntity.FeatureDictionary(entity);
    //            foreach (string sXml in dictionaryEntityFeatures.Keys)
    //            {
    //                if (_rgLogisticFeatures.Contains(sXml)) 
    //                { 
    //                    rTotal += (double) _rgLogisticFeatures[sXml];
    //                } 
    //            }
    //            rProbability = TheFunction(rTotal);
    //        }
    //        return rProbability;
    //    }
    //} 
 
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)] 
    public class Logistic : Feature
    {
        public Logistic()
            : this(null)
        {
        } 
 
        public Logistic(Feature[] FeatureCollectionToSum)
        { 
            base.FeatureCollection = FeatureCollectionToSum;
        }


        [XmlAttribute("probability")]
        public string Probability 
        { 
            get
            { 
                return string.Format("Exp(Total)/(1 + Exp(Total))");
            }
            set
            {
                //Ignore, because you really can't set this
            } 
 
        }
 
        [XmlAnyAttribute()]
        public System.Xml.XmlAttribute[] AnyAttr;

        public double FeatureFunction(params object[] products)
        {
            double rTotal = 0; 
            foreach (double rProduct in products) 
            {
                rTotal += rProduct; 
            }


            return TheFunction(rTotal);
        }
 
        protected double TheFunction(double rTotal) 
        {
            double rExp = Math.Exp(rTotal); 
            double rResult = rExp / (1.0 + rExp); // the logistic aka logistic function (see http://cerebro.xu.edu/math/math120/01f/logistic.pdf)
            return rResult;
        }

        private Converter<object, Set<IHashableFeature>> _featureGenerator;
        //[XmlElement("FeaturesFromEntity")] 
        [XmlIgnore] 
        public Converter<object, Set<IHashableFeature>> FeatureGenerator
        { 
            set
            {
                _featureGenerator = value;
                SpecialFunctions.CheckCondition(_rgLogisticFeatures == null); //!!!raise error - should not set this twice
                SpecialFunctions.CheckCondition(FeatureCollection != null); //!!!raise error the features should be here
                _rgLogisticFeatures = new Dictionary<IHashableFeature, double>(); 
                foreach (WeightIf aWeightIf in FeatureCollection) //!!!what if have non-boolean features 
                {
                    Debug.Assert(aWeightIf.FeatureCollection.Length == 1); //!!! 
                    IHashableFeature aFeature = (IHashableFeature)aWeightIf.FeatureCollection[0];
                    if (_rgLogisticFeatures.ContainsKey(aFeature))
                    {
                        Debug.Fail("don't expect two features that are the same");
                        Debug.Assert(aFeature.Equals(Logistic.AlwaysTrue)); //real assert
                        _rgLogisticFeatures[aFeature] = _rgLogisticFeatures[aFeature] + aWeightIf.Weight; 
                    } 
                    else
                    { 
                        _rgLogisticFeatures.Add(aFeature, aWeightIf.Weight);
                    }
                }
            }
            get
            { 
                return _featureGenerator; 
            }
        } 
        private Dictionary<IHashableFeature, double> _rgLogisticFeatures = null;

        private Dictionary<object, double> cache = new Dictionary<object, double>();

        public double EvaluateViaCache(object entity)
        {
            double probability;
            if (cache.TryGetValue(entity, out probability))
            {
                //double checkP = (double)Evaluate(entity);
                //SpecialFunctions.CheckCondition(checkP == probability);
                return probability;
            }
            probability = (double)Evaluate(entity);
            if (cache.Count > 10000)
            {
                cache.Clear();
            }
            cache.Add(entity, probability);
            return probability;
        }

        override public object Evaluate(object entity)
        {
            double rProbability;
            if (_featureGenerator == null) 
            { 
                rProbability = (double)base.Evaluate(entity);
            } 
            else
            {
                SpecialFunctions.CheckCondition(_rgLogisticFeatures != null); //!!!raise error - this should have been set
                Set<IHashableFeature> entityFeatureSet = _featureGenerator(entity);

                rProbability = EvaluateGivenFeatures(entityFeatureSet); 
            } 
            return rProbability;
 
        }

        public double EvaluateGivenFeatures(Set<IHashableFeature> entityFeatureSet)
        {
            double rProbability;
            double rTotal = 0; 
            Debug.Assert(!entityFeatureSet.Contains(Logistic.AlwaysTrue)); //real assert 
            foreach (IHashableFeature feature in entityFeatureSet)
            { 
                Debug.Assert(feature != Logistic.AlwaysTrue); // real assert
                if (_rgLogisticFeatures.ContainsKey(feature))
                {
                    rTotal += (double)_rgLogisticFeatures[feature];
                }
            } 
            rTotal += Offset; 

            rProbability = TheFunction(rTotal); 
            return rProbability;
        }

        [XmlIgnore]
        public double Offset
        { 
            get 
            {
                return _rgLogisticFeatures[AlwaysTrue]; 
            }
            set
            {
                _rgLogisticFeatures[AlwaysTrue] = value;
            }
        } 
 
        static public IHashableFeature AlwaysTrue = new And();
        static string Header = SpecialFunctions.CreateTabString("Weight", "AbsWeight", "Feature"); 
        static string ProbabilityExpressionWithoutOffset = "Exp(Total)/(1 + Exp(Total))";
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\n\n{1}\n\t\t\t\n", ProbabilityExpressionWithoutOffset, Header);
            foreach (WeightIf weightIf in FeatureCollection) 
            { 
                sb.AppendLine(WeightIfToString(weightIf));
            } 
            return sb.ToString();
        }

        private string WeightIfToString(WeightIf weightIf)
        {
            //string hla = FindFeatureOrEmpty(weightIf.FeatureCollection[0], typeof(IsHla)); 
            //string aa = FindFeatureOrEmpty(weightIf.FeatureCollection[0], typeof(IsAA)); 
            //string prop = FindFeatureOrEmpty(weightIf.FeatureCollection[0], typeof(HasAAProp));
            SpecialFunctions.CheckCondition(weightIf.FeatureCollection.Length == 1); 
            Feature innerFeature = weightIf.FeatureCollection[0];
            return string.Format(SpecialFunctions.CreateTabString(weightIf.Weight, Math.Abs(weightIf.Weight), innerFeature));
        }

        //!!!should check that no-unknown types are here, too
        private string FindFeatureOrEmpty(Feature feature, Type type) 
        { 
            //Debug.Assert(feature is And || feature is IsHla || feature is IsAA || feature is HasAAProp); //!!!raise error
            if (feature.GetType() == type) //!!!use subsumes?? 
            {
                return feature.ToString();
            }
            if (feature is And)
            {
                foreach (Feature featurex in feature.FeatureCollection) 
                { 
                    string s = FindFeatureOrEmpty(featurex, type);
                    if (s != "") 
                    {
                        return s;
                    }
                }
            }
            return ""; 
        } 

        static internal void ReportHeader(TextWriter modelStream) 
        {
            modelStream.WriteLine("{0}\t{1}", "iCrossVal", Header);
        }


        public void Report(string fileName, int iCrossVal) 
        { 
            using (TextWriter modelStream = File.CreateText(fileName))
            { 
                Logistic.ReportHeader(modelStream);

                foreach (WeightIf weightIf in FeatureCollection)
                {
                    modelStream.WriteLine("{0}\t{1}", iCrossVal, WeightIfToString(weightIf));
                } 
            } 
        }
    } 





 
	[Serializable] 
	[XmlRoot(Namespace="", IsNullable=false)]
 	public class WeightIf : Feature 
 	{
		public WeightIf() : this(0, null)
 		{
		}

		public WeightIf(double weight, Feature feature) 
		{ 
			Weight = weight;
			if (feature != null) 
 			{
 				base.FeatureCollection = new Feature[]{feature};
			}
 		}

		[XmlAttribute("weight")] 
		public double Weight; 

		//TODO think about allowing different signtures 

		public double FeatureFunction(object objValue)
		{
 			if (objValue is bool)
 			{
				if ((bool)objValue) 
 				{ 
					return Weight;
				} 
				else
				{
					return 0;
 				}
 			}
			else 
 			{ 
				throw new System.ArgumentException(string.Format("WeightIf requires a subfeature of type bool, but got one of type {0}",objValue.GetType().Name));
			} 
		}
	}

	
 	[Serializable]
 	[XmlRoot(Namespace="", IsNullable=false)] 
	public class Or : Feature 
 	{
		//TODO think about allowing different signtures 
		public bool FeatureFunction(params object[] bools)
		{
			foreach (bool aBool in bools)
			{
 				if (aBool)
 				{ 
					return true; 
 				}
			} 

			return false;
		}

	}
 
 
    //[Serializable]
    //[XmlRoot(Namespace="", IsNullable=false)] 
    //public class And : Feature
    //{
    //    public And() : this(null,null)
    //    {
    //    }
 
    //    !!!make work for any number of arguments 
    //    public And(Feature boolfeature1, Feature boolfeature2)
    //    { 
    //        if (boolfeature1 != null && boolfeature2 != null)
    //        {
    //            base.FeatureCollection = new Feature[]{boolfeature1, boolfeature2};
    //        }
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

    //}

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class And : Feature, IHashableFeature 
    { 
        public And()
            : this(null, null) 
        {
        }

        //!!!make work for any number of arguments
        public And(IHashableFeature boolfeature1, IHashableFeature boolfeature2)
        { 
            if (boolfeature1 != null && boolfeature2 != null) 
            {
                base.FeatureCollection = new Feature[] { (Feature)boolfeature1, (Feature)boolfeature2 }; 
            }
        }
        //!!!make work for any number of arguments
        public static And GetInstance(IHashableFeature boolfeature1, IHashableFeature boolfeature2)
        {
            return new And(boolfeature1, boolfeature2); 
        } 

 
        public bool FeatureFunction(params object[] bools)
        {
            foreach (bool aBool in bools)
            {
                if (!aBool)
                { 
                    return false; 
                }
            } 

            return true;
        }

        public override int GetHashCode()
        { 
            int hashCode = "And".GetHashCode(); // an arbitrary constant for "And" 
            foreach (Feature feature in FeatureCollection)
            { 
                hashCode ^= feature.GetHashCode();
            }
            return hashCode;
        }

        public override bool Equals(object obj) 
        { 
            And other = obj as And;
            if (other == null) 
            {
                return false;
            }
            else
            {
                if (FeatureCollection.Length != other.FeatureCollection.Length) 
                { 
                    return false;
                } 
                for (int iFeature = 0; iFeature < FeatureCollection.Length; ++iFeature)
                {
                    if (!FeatureCollection[iFeature].Equals(other.FeatureCollection[iFeature]))
                    {
                        return false;
                    } 
                } 
                return true;
            } 
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Feature feature in FeatureCollection) 
            { 
                if (sb.Length != 0)
                { 
                    sb.Append(" && ");
                }
                sb.Append(feature.ToString());
            }
            return sb.ToString();
        } 
    } 

	 
    //[Serializable]
    //[XmlRoot(Namespace="", IsNullable=false)]
    //public class Not : Feature
    //{
    //    public Not() : this(null)
    //    { 
    //    } 

    //    public Not(Feature featureBool) 
    //    {
    //        if (featureBool != null)
    //        {
    //            base.FeatureCollection = new Feature[]{featureBool};
    //        }
    //    } 
 
    //    public bool FeatureFunction(bool aBool)
    //    { 
    //        return !aBool;
    //    }

    //}

 
    [Serializable] 
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Not : Feature, IHashableFeature 
    {
        public Not()
            : this(null)
        {
        }
 
        private Not(Feature boolfeature) 
        {
            if (boolfeature != null) 
            {
                base.FeatureCollection = new Feature[] {boolfeature };
            }
        }

        //!!!make work for any number of arguments 
        public static Not GetInstance(Feature boolfeature) 
        {
            return new Not(boolfeature); 
        }

        public bool FeatureFunction(bool aBool)
        {
            return !aBool;
        } 
 
        public override int GetHashCode()
        { 
            if (FeatureCollection.Length == 0)
            {
                return "Not".GetHashCode();
            }

            //Will raise an error if you ask for the hashcode, but the inner feature is not IHashableFeature 
            return "Not".GetHashCode() ^ ((IHashableFeature)FeatureCollection[0]).GetHashCode(); 
        }
 
        public override bool Equals(object obj)
        {
            //Will raise an error if you use Equals, but the inner feature is not IHashableFeature
            Not other = obj as Not;
            if (other == null)
            { 
                return false; 
            }
            else 
            {
                if (!FeatureCollection.Length.Equals(other.FeatureCollection.Length))
                {
                    Debug.Assert(obj.ToString() != ToString());
                    return false;
                } 
                if (FeatureCollection.Length == 0) 
                {
                    Debug.Assert(obj.ToString() == ToString()); 
                    return true;
                }

                Debug.Assert(FeatureCollection[0] is IHashableFeature && other.FeatureCollection[0] is IHashableFeature);
                bool b = (FeatureCollection[0].Equals(other.FeatureCollection[0]));
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

            return string.Format("not {0}", FeatureCollection[0]); 
        }
    }

    [Serializable]
 	[XmlRoot(Namespace="", IsNullable=false)]
 	public class True : Feature 
	{ 
 		public bool FeatureFunction()
		{ 
			return true;
		}

	}

	[Serializable] 
 	[XmlRoot(Namespace="", IsNullable=false)] 
 	public class False : Feature
	{ 
 		public bool FeatureFunction()
		{
			return false;
		}

	} 
 
	
 	///!!!would be cool if the hiRange defaults to long.MaxValue and doesn't write that out 
 	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)]
 	public class IsInRange : Feature
	{
		public IsInRange() : this(0,1,null)
		{ 
		} 

		public IsInRange(long hiRange, Feature featureNumber) : this(0,hiRange,featureNumber) 
 		{
 		}

		public IsInRange(long loRange, long hiRange, Feature featureNumber)
 		{
			LoRange = loRange; 
			HiRange = hiRange; 
			if (featureNumber != null)
			{ 
				base.FeatureCollection = new Feature[]{featureNumber};
 			}

 		}

		[XmlAttribute("loRange")] 
 		[System.ComponentModel.DefaultValueAttribute(0)] 
		public long LoRange = 0;
 
		[XmlAttribute("hiRange")]
		public long HiRange;

		public bool FeatureFunction(long Value)
		{
 			return LoRange <= Value && Value <= HiRange; 
 		} 
	}
 
 	/// <summary>
	/// Inclusive
	/// </summary>
	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)]
	public class IsInDateRange : Feature 
 	{ 
 		public IsInDateRange() : this(DateTime.MinValue, true, DateTime.MaxValue, true, null)
		{ 
 		}

		//TODO add some defaults to the xml so that it doesn't have to be so long (?)
		public IsInDateRange(DateTime low, bool lowIsInclusive, DateTime high, bool highIsInclusive, Feature featureDateTime)
		{
			Low = low; 
			LowIsInclusive = lowIsInclusive; 
 			High = high;
 			HighIsInclusive = highIsInclusive; 

			if (featureDateTime != null)
 			{
				base.FeatureCollection = new Feature[]{featureDateTime};
			}
		} 
 
		[XmlAttribute("low")]
		public DateTime Low; 

 		[XmlAttribute("lowIsInclusive")]
 		public bool LowIsInclusive;


		[XmlAttribute("high")] 
 		public DateTime High; 

		[XmlAttribute("highIsInclusive")] 
		public bool HighIsInclusive;


		public bool FeatureFunction(DateTime Value)
		{
			return (Low < Value || (Low == Value && LowIsInclusive)) 
 				&& (Value < High || (Value == High && HighIsInclusive)); 
 		}
	} 

 	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)]
	public class HasLengthOf : Feature
	{
 
		[XmlAttribute("loRange")] 
		[System.ComponentModel.DefaultValueAttribute(0)]
 		public long LoRange = 0; 

 		[XmlAttribute("hiRange")]
		public long HiRange;

 		public bool FeatureFunction(string aString)
		{ 
			long c = aString.Length; 
			return LoRange <= c && c <= HiRange;
		} 

	}


 	[Serializable]
 	[XmlRoot(Namespace="", IsNullable=false)] 
	public class HasHighAlphaRatio : Feature 
 	{
		public bool FeatureFunction(string aString) 
		{
			Debug.Assert(false,"not implemented");
			//TODO!!!bugbug not implemented
			return false;
 		}
 	} 
 

	[Serializable] 
 	[XmlRoot(Namespace="", IsNullable=false)]
	public class HasHighNonLowerWordRatio : Feature
	{
		public bool FeatureFunction(string aString)
		{
			Debug.Assert(false,"not implemented"); 
 			//TODO!!!bugbug not implemented 
 			return false;
		} 
 	}


	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)]
	public class HasEntityPart : Feature 
	{ 
		public bool FeatureFunction(string aString)
 		{ 
 			Debug.Assert(false,"not implemented");
			//TODO!!!bugbug not implemented
 			return false;
		}
	}
 
 
	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)] 
	public class HasName : Feature
 	{
 		[XmlAttribute] //(DataType="HasNameName")]
		public HasNameName Name = HasNameName.UserFullName;

 		public bool FeatureFunction(string aString) 
		{ 
			Debug.Assert(false,"Not implemented");
			//TODO!!!BUGBUG this is not real Name code 
			return false;
		}

 	}

 
 	public enum HasNameName 
	{
 		[XmlEnumAttribute("HasNameName")] 
		UserFullName,
		UserFirstName,
		UserLastName,
		CompanyName
	}
 
 
 	[Serializable]
 	[XmlRoot(Namespace="", IsNullable=false)] 
	public class HasStringHash : Feature
 	{
		[XmlElement]
		public string Text ="";

		public bool FeatureFunction(StringCollection stringCollection) 
		{ 
			return stringCollection.Contains(Text);
 		} 
 	}

	//	[Serializable]
 	//	[XmlRoot(Namespace="", IsNullable=false)]
	//	public class HasWord : Feature
	//	{ 
	//		[XmlElement] 
	//		public string Text ="";
	// 
 	//		public bool FeatureFunction(string stringValue)
 	//		{
	//			return stringValue.xxxIndexOf(Text) >= 0;
 	//		}
	//	}
	 
 
	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)] 
	public class HasPattern : Feature
 	{

 		[XmlIgnore]
		string _pattern;
 
 		[XmlIgnore] 
		Regex _regex;
 

		public HasPattern() : this(".*", null)
		{
		}

		public HasPattern(string pattern, Feature featureBigString) 
 		{ 
 			Pattern = pattern;
			if (featureBigString != null) 
 			{
				base.FeatureCollection = new Feature[]{featureBigString};
			}
		}

		[XmlElement] 
		public string Pattern 
 		{
 			set 
			{
 				_pattern = value;
				_regex = new Regex(_pattern);
			}
			get
			{ 
				return _pattern; 
 			}
 		} 

		public bool FeatureFunction(string Big)
 		{
			return false;
		}
	 
	} 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
