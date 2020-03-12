using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using System.IO;
 
namespace VirusCount.PhyloTree 
{
    public abstract class SufficientStatistics 
    {
        public abstract bool IsMissing();

        public static SufficientStatistics Parse(string val)
        {
            SufficientStatistics result; 
            if ( 
                MissingStatistics.Parse(val, out result) ||
                GaussianStatistics.Parse(val, out result) || 
                BooleanStatistics.Parse(val, out result) ||
                DiscreteStatistics.Parse(val, out result) ||
                ContinuousStatistics.Parse(val, out result))
            {
                return result;
            } 
            throw new ArgumentException(string.Format("Unable to parse {0} into an instance of ISufficientStatistics", val)); 
        }
 
        public static Converter<Leaf, SufficientStatistics> DictionaryToLeafMap(Dictionary<string, SufficientStatistics> caseIdToNonMissingValue)
        {
            return delegate(Leaf leaf)
                {
                    string name = leaf.CaseName;
                    if (name == null || !caseIdToNonMissingValue.ContainsKey(name)) 
                        return MissingStatistics.Singleton; 
                    else
                        return caseIdToNonMissingValue[name]; 
                };
        }


        public static void WriteDataToFile(string filename, Dictionary<string, Dictionary<string, SufficientStatistics>> variableToCidToValue)
        { 
            using (TextWriter writer = File.CreateText(filename)) 
            {
                writer.WriteLine("var\tcid\tval"); 
                foreach (KeyValuePair<string, Dictionary<string, SufficientStatistics>> variableAndCidToValue in variableToCidToValue)
                {
                    foreach (KeyValuePair<string, SufficientStatistics> cidAndValue in variableAndCidToValue.Value)
                    {
                        string valueToWrite =
                            cidAndValue.Value is BooleanStatistics ? 
                            ((int)(BooleanStatistics)cidAndValue.Value).ToString() 
                            : cidAndValue.Value.ToString();
                        writer.WriteLine(SpecialFunctions.CreateTabString(variableAndCidToValue.Key, cidAndValue.Key, valueToWrite)); 
                    }
                }
            }
        }

 
 
        public static bool operator !=(SufficientStatistics stats1, SufficientStatistics stats2)
        { 
            return !(stats1 == stats2);
        }
        public static bool operator ==(SufficientStatistics stats1, SufficientStatistics stats2)
        {
            object o1 = stats1 as Object;
            object o2 = stats2 as Object; 
            if (o1 != null && o2 != null) 
            {
                if (stats1.IsMissing() && stats2.IsMissing()) 
                    return true;
                return stats1.Equals(stats2);
            }
            else
            {
                return o1 == o2;    // at this point, only equal if both are null. 
            } 
        }
        public override int GetHashCode() 
        {
            throw new NotSupportedException("Derived class must override GetHashCode()");
        }
        public override bool Equals(object obj)
        {
            throw new NotSupportedException("Derived class must override Equals()"); 
        } 
    }
 
    public class MissingStatistics : SufficientStatistics
    {
        public readonly static MissingStatistics Singleton = new MissingStatistics();

        private MissingStatistics() { }
 
 
        public static bool Parse(string val, out SufficientStatistics result)
        { 
            result = null;
            if (val.Equals("null", StringComparison.CurrentCultureIgnoreCase) || val.Equals("missing", StringComparison.CurrentCultureIgnoreCase))
            {
                result = Singleton;
            }
            return result != null; 
        } 

        public override bool IsMissing() 
        {
            return true;
        }

        public override bool Equals(object obj)
        { 
            if (obj is SufficientStatistics) 
            {
                return ((SufficientStatistics)obj).IsMissing(); 
            }
            else
            {
                return false;
            }
        } 
 
        public override int GetHashCode()
        { 
            return "Missing".GetHashCode();
        }
        public override string ToString()
        {
            return "Missing";
        } 
    } 

    public class GaussianStatistics : SufficientStatistics 
    {
        private readonly double _mean;
        private readonly double _variance;
        private readonly int _sampleSize;
        private readonly bool _isMissing;
 
        private GaussianStatistics() 
        {
            _isMissing = true; 
        }

        private GaussianStatistics(double mean, double var, int sampleSize)
        {
            _mean = mean;
            _variance = var; 
            _sampleSize = sampleSize; 
            _isMissing = false;
        } 

        static public GaussianStatistics GetMissingInstance()
        {
            return new GaussianStatistics();
        }
 
        static public GaussianStatistics GetInstance(double mean, double variance, int sampleSize) 
        {
            return new GaussianStatistics(mean, variance, sampleSize); 
        }


        public static GaussianStatistics GetInstance(IEnumerable<double> observations)
        {
            int n = 0; 
            double sum = 0; 
            foreach (double d in observations)
            { 
                sum += d;
                n++;
            }
            double mean = sum / n;
            double variance = 0;
            foreach (double d in observations) 
            { 
                variance += (d - mean) * (d - mean);
            } 
            variance /= n - 1;
            return GetInstance(mean, variance, n);
        }

        public double Mean
        { 
            get 
            {
                SpecialFunctions.CheckCondition(!_isMissing); 
                return _mean;
            }
        }
        public double Variance
        {
            get 
            { 
                SpecialFunctions.CheckCondition(!_isMissing);
                return _variance; 
            }
        }
        public int SampleSize
        {
            get
            { 
                SpecialFunctions.CheckCondition(!_isMissing); 
                return _sampleSize;
            } 
        }

        internal static bool Parse(string val, out SufficientStatistics result)
        {
            result = null;
            if (val.Equals("null", StringComparison.InvariantCultureIgnoreCase)) 
            { 
                result = GetMissingInstance();
                return false; 
            }
            else
            {
                string[] fields = val.Split(',');
                if (!(fields.Length == 3))
                { 
                    return false; 
                }
                double mean, variance; 
                int sampleSize;

                if (double.TryParse(fields[0], out mean) &&
                    double.TryParse(fields[1], out variance) &&
                    int.TryParse(fields[2], out sampleSize))
                { 
                    result = GaussianStatistics.GetInstance(mean, variance, sampleSize); 
                    return true;
                } 

                return false;
            }
        }

        public override string ToString() 
        { 
            return IsMissing() ? "Missing" : string.Format("m={0},v={1},ss={2}", Mean, Variance, SampleSize);
        } 

        public override bool IsMissing()
        {
            return _isMissing;
        }
 
        public override bool Equals(object obj) 
        {
            if (IsMissing() && obj is SufficientStatistics && ((SufficientStatistics)obj).IsMissing()) 
            {
                return true;
            }
            GaussianStatistics other = obj as GaussianStatistics;
            if (other == null)
                return false; 
 
            return _mean == other._mean && _variance == other._variance && _sampleSize == other._sampleSize;
        } 

        public override int GetHashCode()
        {
            return IsMissing() ? MissingStatistics.Singleton.GetHashCode() : ToString().GetHashCode();
        }
 
        public static implicit operator GaussianStatistics(MissingStatistics missing) 
        {
            return GaussianStatistics.GetMissingInstance(); 
        }


        public static Converter<Leaf, SufficientStatistics> DictionaryToLeafMap(Dictionary<string, GaussianStatistics> caseIdToNonMissingValue)
        {
            return delegate(Leaf leaf) 
                { 
                    string name = leaf.CaseName;
                    if (name == null || !caseIdToNonMissingValue.ContainsKey(name)) 
                        return GaussianStatistics.GetMissingInstance();
                    else
                        return caseIdToNonMissingValue[name];
                };
        }
 
    } 

    public class DiscreteStatistics : SufficientStatistics 
    {
        private readonly int _class;

        public int Class
        {
            get 
            { 
                return _class < 0 ? -1 : _class;
            } 
        }

        protected DiscreteStatistics(int discreteClassification)
        {
            _class = discreteClassification;
        } 
 
        public static DiscreteStatistics GetMissingInstance()
        { 
            return new DiscreteStatistics(-1);
        }

        public static DiscreteStatistics GetInstance(int discreteteClassification)
        {
            return new DiscreteStatistics(discreteteClassification); 
        } 

        internal static bool Parse(string val, out SufficientStatistics result) 
        {
            int valAsInt;
            if (int.TryParse(val, out valAsInt))
            {
                result = DiscreteStatistics.GetInstance(valAsInt);
                return true; 
            } 
            else
            { 
                result = null;
                return false;
            }
        }

        public override bool IsMissing() 
        { 
            return Class < 0;
        } 

        public static implicit operator DiscreteStatistics(int classification)
        {
            return new DiscreteStatistics(classification);
        }
 
        public static implicit operator int(DiscreteStatistics stats) 
        {
            return stats.Class; 
        }

        public static implicit operator DiscreteStatistics(MissingStatistics missing)
        {
            return DiscreteStatistics.GetMissingInstance();
        } 
 
        //public static bool operator !=(DiscreteStatistics stats1, DiscreteStatistics stats2)
        //{ 
        //    return !(stats1 == stats2);
        //}
        //public static bool operator ==(DiscreteStatistics discreteStatistics1, DiscreteStatistics discreteStatistics2)
        //{
        //    object o1 = discreteStatistics1 as Object;
        //    object o2 = discreteStatistics2 as Object; 
        //    if (o1 != null && o2 != null) 
        //        return discreteStatistics1.Class == discreteStatistics2.Class;
        //    else 
        //        return o1 == o2;    // at this point, only equal if both are null.
        //}
        public override bool Equals(object obj)
        {
            if (IsMissing() && obj is SufficientStatistics && ((SufficientStatistics)obj).IsMissing())
            { 
                return true; 
            }
            DiscreteStatistics asDiscStat = obj as DiscreteStatistics; 
            if (asDiscStat == null)
            {
                ContinuousStatistics asContStat = obj as ContinuousStatistics;
                if (asContStat == null)
                {
                    return false; 
                } 
                else
                { 
                    return asContStat.Value == this.Class;
                }
            }

            return this.Class == asDiscStat.Class;
            //return this == asDiscStat; 
        } 

        public override int GetHashCode() 
        {
            return IsMissing() ? MissingStatistics.Singleton.GetHashCode() : Class.GetHashCode();
        }
        public override string ToString()
        {
            return IsMissing() ? "Missing" : Class.ToString(); 
        } 

 
    }

    public class BooleanStatistics : DiscreteStatistics
    {
        public const int Missing = -1;
        public const int False = 0; 
        public const int True = 1; 

        protected BooleanStatistics() : base(-1){}  // get missing instance 
        protected BooleanStatistics(bool classification) : base(classification ? 1 : 0){}

        new public static BooleanStatistics GetMissingInstance()
        {
            return new BooleanStatistics();
        } 
 
        public static BooleanStatistics GetInstance(bool classification)
        { 
            return new BooleanStatistics(classification);
        }

        public static void WriteDataToFile(string filename, Dictionary<string, Dictionary<string, BooleanStatistics>> variableToCidToValue)
        {
            using (TextWriter writer = File.CreateText(filename)) 
            { 
                writer.WriteLine("var\tcid\tval");
                foreach (KeyValuePair<string, Dictionary<string, BooleanStatistics>> variableAndCidToValue in variableToCidToValue) 
                {
                    foreach (KeyValuePair<string, BooleanStatistics> cidAndValue in variableAndCidToValue.Value)
                    {
                        writer.WriteLine(SpecialFunctions.CreateTabString(variableAndCidToValue.Key, cidAndValue.Key, (int)cidAndValue.Value));
                    }
                } 
            } 
        }
 
        public static Dictionary<string, BooleanStatistics> ConvertToBooleanStatistics(Dictionary<string, SufficientStatistics> dictionary)
        {
            Dictionary<string, BooleanStatistics> result = new Dictionary<string, BooleanStatistics>();

            foreach (KeyValuePair<string, SufficientStatistics> stringAndSuff in dictionary)
            { 
                result.Add(stringAndSuff.Key, (BooleanStatistics)stringAndSuff.Value); 
            }
            return result; 
        }

        public override string ToString()
        {
            return IsMissing() ? "Missing" : (Class > 0).ToString();
        } 
 
        public static Converter<Leaf, SufficientStatistics> DictionaryToLeafMap(Dictionary<string, BooleanStatistics> caseIdToNonMissingValue)
        { 
            return delegate(Leaf leaf)
                {
                    string name = leaf.CaseName;
                    if (name == null || !caseIdToNonMissingValue.ContainsKey(name))
                        return BooleanStatistics.GetMissingInstance();
                    else 
                        return (BooleanStatistics)caseIdToNonMissingValue[name]; 
                };
        } 

        public static implicit operator bool(BooleanStatistics stats)
        {
            if (stats.IsMissing())
            {
                throw new InvalidCastException("Cannot cast missing statistics to a bool."); 
            } 
            return stats.Class > 0;
        } 

        public static implicit operator BooleanStatistics(bool classification)
        {
            return new BooleanStatistics(classification);
        }
 
        public static implicit operator BooleanStatistics(MissingStatistics missing) 
        {
            return BooleanStatistics.GetMissingInstance(); 
        }

        //public static bool operator !=(BooleanStatistics stats1, BooleanStatistics stats2)
        //{
        //    return !(stats1 == stats2);
        //} 
        //public static bool operator ==(BooleanStatistics boolStats1, BooleanStatistics boolStats2) 
        //{
        //    DiscreteStatistics d1 = boolStats1 as DiscreteStatistics; 
        //    DiscreteStatistics d2 = boolStats2 as DiscreteStatistics;
        //    return d1 == d2;
        //}

        new internal static bool Parse(string val, out SufficientStatistics result)
        { 
            result = null; 
            if (val.Equals("true", StringComparison.CurrentCultureIgnoreCase) || val == "1")
            { 
                result = BooleanStatistics.GetInstance(true);
            }
            else if (val.Equals("false", StringComparison.CurrentCultureIgnoreCase) || val == "0")
            {
                result = BooleanStatistics.GetInstance(false);
            } 
            else if (val.Equals("null", StringComparison.CurrentCultureIgnoreCase) || val == "-1") 
            {
                result = BooleanStatistics.GetMissingInstance(); 
            }
            return result != null;
        }


    } 
 
    public class ContinuousStatistics : SufficientStatistics
    { 
        private readonly double _value;
        private readonly bool _isMissing;

        public double Value
        {
            get 
            { 
                if (_isMissing) throw new ArgumentException("Attempting to retrieve value from missing statistics");
                return _value; 
            }
        }

        protected ContinuousStatistics()
        {
            _isMissing = true; 
        } 

        protected ContinuousStatistics(double value) 
        {
            _value = value;
            _isMissing = false;
        }

        public static ContinuousStatistics GetMissingInstance() 
        { 
            return new ContinuousStatistics();
        } 

        public static ContinuousStatistics GetInstance(double value)
        {
            return new ContinuousStatistics(value);
        }
 
        internal static bool Parse(string val, out SufficientStatistics result) 
        {
            double valAsDouble; 
            if (double.TryParse(val, out valAsDouble))
            {
                result = ContinuousStatistics.GetInstance(valAsDouble);
                return true;
            }
            else 
            { 
                result = null;
                return false; 
            }
        }

        public override bool IsMissing()
        {
            return _isMissing; 
        } 

        public static implicit operator ContinuousStatistics(double value) 
        {
            return new ContinuousStatistics(value);
        }

        public static implicit operator double(ContinuousStatistics stats)
        { 
            return stats.Value; 
        }
 
        public static implicit operator ContinuousStatistics(MissingStatistics missing)
        {
            return ContinuousStatistics.GetMissingInstance();
        }

        public static implicit operator ContinuousStatistics(DiscreteStatistics discreteStats) 
        { 
            return discreteStats.IsMissing() ? ContinuousStatistics.GetMissingInstance() : ContinuousStatistics.GetInstance(discreteStats.Class);
        } 

        public static explicit operator DiscreteStatistics(ContinuousStatistics continuousStats)
        {
            return continuousStats.IsMissing() ? DiscreteStatistics.GetMissingInstance() : DiscreteStatistics.GetInstance((int)continuousStats.Value);
        }
 
        public override bool Equals(object obj) 
        {
            if (IsMissing() && obj is SufficientStatistics && ((SufficientStatistics)obj).IsMissing()) 
            {
                return true;
            }
            ContinuousStatistics asContStat = obj as ContinuousStatistics;
            if (asContStat == null)
            { 
                DiscreteStatistics asDiscStat = obj as DiscreteStatistics; 
                if (asDiscStat == null)
                { 
                    return false;
                }
                else
                {
                    return asDiscStat.Class == this.Value;
                } 
            } 

            return this.Value == asContStat.Value; 
        }

        public override int GetHashCode()
        {
            return IsMissing() ? MissingStatistics.Singleton.GetHashCode() : Value.GetHashCode();
        } 
        public override string ToString() 
        {
            return IsMissing() ? "Missing" : Value.ToString(); 
        }
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
