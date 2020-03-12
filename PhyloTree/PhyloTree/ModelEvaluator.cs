using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree 
{ 
    public abstract class ModelEvaluator
    { 
        private readonly ModelScorer _scorer;
        private List<IDistributionSingleVariable> _nullDistns;
        private IDistribution _altDistn;

        public List<IDistributionSingleVariable> NullDistns
        { 
            get { return _nullDistns; } 
        }
 
        public IDistribution AltDistn
        {
            get { return _altDistn; }
        }

        protected int ChiSquareDegreesOfFreedom 
        { 
            get
            { 
                int nullParams = 0;
                foreach (IDistributionSingleVariable nullDistn in NullDistns)
                {
                    nullParams += nullDistn.FreeParameterCount;
                }
                return AltDistn.FreeParameterCount - nullParams; 
            } 
        }
 
        protected ModelEvaluator(List<IDistributionSingleVariable> nullDistns, IDistribution altDistn, ModelScorer scorer)
        {
            _scorer = scorer;
            _nullDistns = nullDistns;
            _altDistn = altDistn;
        } 
 

 
        public static ModelEvaluator GetInstance(string nameAndParameters, ModelScorer scorer)
        {
            nameAndParameters = nameAndParameters.ToLower();
            if (nameAndParameters.StartsWith(ModelEvaluatorCrossValidate.BaseName.ToLower()))
            {
                return ModelEvaluatorCrossValidate.GetInstance(nameAndParameters.Substring(ModelEvaluatorCrossValidate.BaseName.Length), scorer); 
            } 
            else if(nameAndParameters.StartsWith(ModelEvaluatorDiscreteConditionalCollection.BaseName.ToLower()))
            { 
                return ModelEvaluatorDiscreteConditionalCollection.GetInstance(nameAndParameters.Substring(ModelEvaluatorDiscreteConditionalCollection.BaseName.Length), scorer);
            }
            else if (nameAndParameters.StartsWith(ModelEvaluatorDiscrete.BaseName.ToLower()))
            {
                return ModelEvaluatorDiscrete.GetInstance(nameAndParameters.Substring(ModelEvaluatorDiscrete.BaseName.Length), scorer);
            } 
            else if (nameAndParameters.StartsWith(ModelEvaluatorGaussian.BaseName.ToLower())) 
            {
                return ModelEvaluatorGaussian.GetInstance(nameAndParameters.Substring(ModelEvaluatorGaussian.BaseName.Length), scorer); 
            }
            else
            {
                throw new ArgumentException("ModelEvaluator cannot parse " + nameAndParameters);
            }
        } 
 
        public ModelScorer ModelScorer
        { 
            get { return _scorer; }
        }

        public abstract string Name { get;}
        public abstract EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2);
        // public abstract EvaluationResults EvaluateModelOnData(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2, bool ignoreSimpleCases); 
        public abstract EvaluationResults EvaluateModelOnDataGivenParams(Converter<Leaf, SufficientStatistics> v1, Converter<Leaf, SufficientStatistics> v2, EvaluationResults previousResults); 
        public abstract string ToHeaderString();
 
        public NullDataGenerator CreateNullDataGenerator(string nullDataGeneratorName)
        {
            return NullDataGenerator.GetInstance(nullDataGeneratorName, ModelScorer, NullDistns[0]);
        }
    }
 
    public class EvaluationResults 
    {
        protected readonly SpecialFunctions SpecialFunctions = SpecialFunctions.GetInstance(); 
        public readonly List<Score> NullScores;
        public readonly Score AltScore;
        public readonly int GlobalNonMissingCount;
        public readonly int ChiSquareDegreesOfFreedom;
        public ModelEvaluator ModelEvaluator;
        private readonly double _nullScore; 
 
        protected EvaluationResults(ModelEvaluator modelEval, List<Score> nullScores, Score altScore, int chiSquareDegreesOfFreedom, int globalNonMissingCount)
        { 
            ModelEvaluator = modelEval;
            NullScores = nullScores;
            AltScore = altScore;
            ChiSquareDegreesOfFreedom = chiSquareDegreesOfFreedom;
            GlobalNonMissingCount = globalNonMissingCount;
 
            _nullScore = 0; 
            foreach (Score score in NullScores)
            { 
                _nullScore += score.Loglikelihood;
            }
        }

        public static EvaluationResults GetInstance(ModelEvaluator modelEval, List<Score> nullScores, Score altScore, int chiSquareDegreesOfFreedom, int globalNonMissingCount)
        { 
            return new EvaluationResults(modelEval, nullScores, altScore, chiSquareDegreesOfFreedom, globalNonMissingCount); 
        }
 
        public double NullLL
        {
            get
            {
                //double LL = 0;
                //foreach (Score score in NullScores) 
                //{ 
                //    LL += score.Loglikelihood;
                //} 
                //return LL;
                return _nullScore;
            }
        }

        public double AltLL 
        { 
            get { return AltScore.Loglikelihood; }
        } 

        public virtual string IidStatsHeaderString()
        {
            return Mlas.Tabulate.Tabulate.GlobalNonMissingCountColumnName;
        }
 
        public virtual string IidStatsString() 
        {
            return GlobalNonMissingCount.ToString(); 
        }

        public virtual double ComputePValue()
        {
            double diff = AltLL - NullLL;
            double pValue = SpecialFunctions.LogLikelihoodRatioTest(Math.Max(diff, 0), ChiSquareDegreesOfFreedom);
            if (double.IsNaN(pValue) && diff > 0)
            {
                pValue = 0;
            }
            return pValue;
        }
 
        public string ToHeaderString()
        {
            StringBuilder result = new StringBuilder(IidStatsHeaderString() + "\t");
            int counter = 0;
            foreach (Score score in NullScores)
            { 
                string counterString = (NullScores.Count > 1 ? counter.ToString() : ""); 
                if (score.Distribution != null)
                { 
                    result.Append(score.Distribution.GetParameterHeaderString("_Null" + counterString) + "\t");
                }
                else
                {
                    result.Append((score.OptimizationParameters.ToStringHeader() + "\t").Replace("\t", "_Null" + counterString + "\t"));
                } 
 
                result.Append("Loglikelihood_Null" + counterString + "\t");
                counter++; 
            }

            if (NullScores.Count > 1)
            {
                result.Append("Loglikelihood_Null\t");
            } 
 
            if (AltScore.Distribution != null)
            { 
                result.Append(AltScore.Distribution.GetParameterHeaderString("_Alt") + "\t");
            }
            else
            {
                result.Append(AltScore.OptimizationParameters.ToStringHeader() + "\t");
            } 
 
            result.Append("Loglikelihood_Alt\t");
 
            result.Append("diff\tPValue");
            return result.ToString();
        }

        public override string ToString()
        { 
            StringBuilder result = new StringBuilder(IidStatsString() + "\t"); 
            foreach (Score score in NullScores)
            { 
                if (score.Distribution != null)
                {
                    result.Append(score.Distribution.GetParameterValueString(score.OptimizationParameters) + "\t");
                }
                else
                { 
                    result.Append(score.OptimizationParameters.ToString() + "\t"); 
                }
                result.Append(score.Loglikelihood + "\t"); 
            }

            if (NullScores.Count > 1)
            {
                result.Append(NullLL + "\t");
            } 
 
            if (AltScore.Distribution != null)
            { 
                result.Append(AltScore.Distribution.GetParameterValueString(AltScore.OptimizationParameters) + "\t");
            }
            else
            {
                result.Append(AltScore.OptimizationParameters.ToString() + "\t");
            } 
 
            result.Append(AltLL + "\t");
            double diff = AltLL - NullLL; 
            result.Append(diff + "\t");
            result.Append(ComputePValue());

            return result.ToString();
        }
 
        public override bool Equals(object obj) 
        {
            return ToString() == obj.ToString(); 
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        } 
    } 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
