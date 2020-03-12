using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount 
{ 
    public class Parameter
    { 
        static public Parameter GetProbabilityInstance(string name, double value, bool doSearch)
        {
            Parameter parameter = new Parameter();

            parameter.TransformForSearch = SpecialFunctions.LogOdds;
            parameter.TransformFromSearch = SpecialFunctions.Probability; 
 
            parameter.Name = name;
            parameter.Value = value; 
            parameter.DoSearch = doSearch;
            parameter.Low = .001;
            parameter.High = .999;
            parameter.LowForSearch = parameter.TransformForSearch(parameter.Low);
            parameter.HighForSearch = parameter.TransformForSearch(parameter.High);
 
            return parameter; 

        } 

        static public Parameter GetPositiveFactorInstance(string name, double value, bool doSearch)
        {
            Parameter parameter = new Parameter();

            parameter.TransformForSearch = delegate(double r) { return Math.Log(r); }; 
            parameter.TransformFromSearch = delegate(double r) { return Math.Exp(r); }; 
            parameter.Name = name;
            parameter.Value = value; 
            parameter.DoSearch = doSearch;
            parameter.Low = 1.0/1000.0;
            parameter.High = 1000;
            parameter.LowForSearch = parameter.TransformForSearch(parameter.Low);
            parameter.HighForSearch = parameter.TransformForSearch(parameter.High);
 
            return parameter; 

        } 

        static public Parameter GetTanInstance(string name, double value, bool doSearch, double low, double high)
        {
            Parameter parameter = new Parameter();

            parameter.TransformForSearch = delegate(double r) { return Math.Atan(r)*2/Math.PI; }; 
            parameter.TransformFromSearch = delegate(double r) { return Math.Tan(r*Math.PI/2); }; 
            parameter.Name = name;
            parameter.Value = value; 
            parameter.DoSearch = doSearch;
            parameter.Low = low;
            parameter.High = high;
            parameter.LowForSearch = parameter.TransformForSearch(parameter.Low);
            parameter.HighForSearch = parameter.TransformForSearch(parameter.High);
 
            return parameter; 

        } 


        private Parameter()
        {
        }
 
        public string Name; 
        public bool DoSearch;
        private Converter<double, double> TransformForSearch; 
        public Converter<double, double> TransformFromSearch;
        private double _value;
        private double _valueForSearch;
        public double Low;
        public double High;
        public double LowForSearch; 
        public double HighForSearch; 
        public double Value
        { 
            get
            {
                return _value;
            }
            set
            { 
                _value = value; 
                _valueForSearch = TransformForSearch(value);
            } 
        }

        public double ValueForSearch
        {
            get
            { 
                return _valueForSearch; 
            }
            set 
            {
                _valueForSearch = value;
                _value = TransformFromSearch(value);
            }
        }
 
        internal Parameter Clone() 
        {
            Parameter parameter = new Parameter(); 

            parameter.TransformForSearch = TransformForSearch;
            parameter.TransformFromSearch = TransformFromSearch;
            parameter.Name = Name;
            parameter.Value = Value;
            parameter.DoSearch = DoSearch; 
            parameter.Low = Low; 
            parameter.High = High;
            parameter.LowForSearch = LowForSearch; 
            parameter.HighForSearch = HighForSearch;

            return parameter;

        }
    } 
 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
