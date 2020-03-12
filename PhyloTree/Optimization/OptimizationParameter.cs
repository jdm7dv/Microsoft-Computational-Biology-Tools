using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;

namespace Optimization 
{ 
    public class OptimizationParameter
    { 
        static public OptimizationParameter GetProbabilityInstance(string name, double value, bool doSearch)
        {
            OptimizationParameter parameter = new OptimizationParameter();

            //parameter.TransformForSearch = SpecialFunctions.LogOdds;
            //parameter.TransformFromSearch = SpecialFunctions.Probability; 
            //parameter.Low = .001; 
            //parameter.High = .999;
            //parameter.LowForSearch = parameter.TransformForSearch(parameter.Low); 
            //parameter.HighForSearch = parameter.TransformForSearch(parameter.High);

            parameter.Name = name;
            parameter.Value = value;
            parameter.DoSearch = doSearch;
 
            parameter.ConvertToProbabilityInstance(); 

            return parameter; 

        }

        static public OptimizationParameter GetPositiveFactorInstance(string name, double value, bool doSearch)
        {
            OptimizationParameter parameter = new OptimizationParameter(); 
 
            //parameter.TransformForSearch = delegate(double r) { return Math.Log(r); };
            //parameter.TransformFromSearch = delegate(double r) { return Math.Exp(r); }; 
            //parameter.Low = 1.0 / 1000.0;
            //parameter.High = 1000;
            //parameter.LowForSearch = parameter.TransformForSearch(parameter.Low);
            //parameter.HighForSearch = parameter.TransformForSearch(parameter.High);

            parameter.Name = name; 
            parameter.Value = value; 
            parameter.DoSearch = doSearch;
            parameter.ConvertToPositiveFactorInstance(); 

            return parameter;

        }

        static public OptimizationParameter GetTanInstance(string name, double value, bool doSearch, double low, double high) 
        { 
            OptimizationParameter parameter = new OptimizationParameter();
 
            parameter.Name = name;
            parameter.Value = value;
            parameter.DoSearch = doSearch;
            //parameter.LowForSearch = parameter.TransformForSearch(parameter.Low);
            //parameter.HighForSearch = parameter.TransformForSearch(parameter.High);
            parameter.ConvertToTanInstance(low, high); 
 
            return parameter;
 
        }


        private OptimizationParameter()
        {
        } 
 
        public string Name;
        public bool DoSearch; 
        private Converter<double, double> TransformForSearch;
        public Converter<double, double> TransformFromSearch;
        private double _value;
        //private double _valueForSearch;
        private double _low;
        private double _high; 
        //private double _lowForSearch; 
        //private double _highForSearch;
 
        public double Value
        {
            get
            {
                return _value;
            } 
            set 
            {
                _value = value; 
                //_valueForSearch = TransformForSearch(value);
            }
        }

        internal double ValueForSearch
        { 
            get 
            {
                //return _valueForSearch; 
                return TransformForSearch(Value);
            }
            set
            {
                //_valueForSearch = value;
                _value = TransformFromSearch(value); 
            } 
        }
 
        public double Low
        {
            get
            {
                return _low;
            } 
            private set 
            {
                _low = value; 
                //_lowForSearch = TransformForSearch(value);
            }
        }

        internal double LowForSearch
        { 
            get 
            {
                //return _lowForSearch; 
                return TransformForSearch(Low);
            }
            private set
            {
                //_lowForSearch = value;
                _low = TransformFromSearch(value); 
            } 
        }
 
        public double High
        {
            get
            {
                return _high;
            } 
            private set 
            {
                _high = value; 
                //_highForSearch = TransformForSearch(value);
            }
        }

        internal double HighForSearch
        { 
            get 
            {
                //return _highForSearch; 
                return TransformForSearch(High);
            }
            private set
            {
                //_highForSearch = value;
                _high = TransformFromSearch(value); 
            } 
        }
 
        public void ConvertToProbabilityInstance()
        {
            TransformForSearch = SpecialFunctions.LogOdds;
            TransformFromSearch = SpecialFunctions.InverseLogOdds;
            Low = .00001;
            High = .99999; 
            //Value = Math.Max(Low, Math.Min(High, Value)); 
        }
 
        public void ConvertToPositiveFactorInstance()
        {
            TransformForSearch = delegate(double r) { return Math.Log(r); };
            TransformFromSearch = delegate(double r) { return Math.Exp(r); };
            Low = 1.0 / 1000.0;
            High = 10000; 
            //Value = Math.Max(Low, Math.Min(High, Value)); 
        }
 
        public void ConvertToTanInstance(double low, double high)
        {
            TransformForSearch = delegate(double r) { return Math.Atan(r) * 2 / Math.PI; };
            TransformFromSearch = delegate(double r) { return Math.Tan(r * Math.PI / 2); };
            Low = low;
            High = high; 
        } 

        public OptimizationParameter Clone() 
        {
            OptimizationParameter parameter = new OptimizationParameter();

            parameter.TransformForSearch = TransformForSearch;
            parameter.TransformFromSearch = TransformFromSearch;
            parameter.Name = Name; 
            parameter.Value = Value; 
            parameter.DoSearch = DoSearch;
            parameter.Low = Low; 
            parameter.High = High;

            return parameter;
        }

        public override string ToString() 
        { 
            return Name + "=" + Value;
        } 

    }

}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
