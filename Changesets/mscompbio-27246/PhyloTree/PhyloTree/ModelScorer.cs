using System; 
using System.Collections.Generic;
using System.Text;
using Optimization;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
 
namespace VirusCount.PhyloTree 
{
    public abstract class ModelScorer 
    {
        GridSearch GridSearch;
        const int MAX_CACHE_SIZE = 10000;

        public int FuncCalls = 0, CacheHits = 0, CacheMisses = 0, CacheClears = 0;
 
        private readonly Dictionary<MessageInitializer, Score> _cache = new Dictionary<MessageInitializer, Score>(MAX_CACHE_SIZE); 
        public readonly PhyloTree PhyloTree;
 
        public abstract string Name { get;}

        protected ModelScorer(PhyloTree tree, GridSearch optimizer)
        {
            PhyloTree = tree;
            GridSearch = optimizer; 
        } 

        public static ModelScorer GetInstance(PhyloTree aPhyloTree, string leafDistributionName, string optimizerName) 
        {
            leafDistributionName = leafDistributionName.ToLower();
            ModelScorer modelScorer;
            GridSearch optimizer = GridSearch.GetInstance(optimizerName);
            if (leafDistributionName.StartsWith(ModelEvaluatorCrossValidate.BaseName.ToLower()))
            { 
                leafDistributionName = leafDistributionName.Substring(ModelEvaluatorCrossValidate.BaseName.Length); 
            }
            else if (leafDistributionName.StartsWith(ModelEvaluatorReverse.BaseName.ToLower())) 
            {
                leafDistributionName = leafDistributionName.Substring(ModelEvaluatorReverse.BaseName.Length);
            }

            if (leafDistributionName.StartsWith(ModelEvaluatorDiscrete.BaseName.ToLower()))
            { 
                modelScorer = new ModelScorerDiscrete(aPhyloTree, optimizer); 
            }
            //else if (leafDistributionName.StartsWith(ModelEvaluatorGaussian.BaseName.ToLower())) 
            //{
            //    modelScorer = new ModelScorerGaussian(aPhyloTree, optimizer);
            //}
            else
            {
                modelScorer = null; 
                throw new ArgumentException("Cannot parse " + leafDistributionName + " into a valid ModelScorer."); 
            }
 
            //modelScorer.GridSearch = GridSearch.GetInstance(optimizerName);
            return modelScorer;


        }
 
        /// <summary> 
        /// Learns the optimal parameters for the data contained in the messageInitializer and returns the corresponding Score.
        /// </summary> 
        public Score MaximizeLikelihood(MessageInitializer messageInitializer)
        {
            Score score;

            #region Caching details
            //Key aKey = Key.GetInstance(Tree, messageInitializer); 
 
            if (_cache.ContainsKey(messageInitializer))
            { 
                CacheHits++;
                score = _cache[messageInitializer];
#if (DEBUG)
                Score scoreLive = MaximizeLikelihoodInternal(messageInitializer);

                if (Math.Abs(score.Loglikelihood - scoreLive.Loglikelihood) >= 10e-7) 
                { 
                    double diff = scoreLive.Loglikelihood - score.Loglikelihood;
                    Debug.WriteLine("Cache differs from computed score by " + diff); 
                }
                // note: minute (10E-14) differences sometimes arise. The original explanation was that these were differences in rounding
                // errors caused when missing data was caught in different places. I have tried to localize the catch of missing data and
                // throw errors elsewhere but still have the same rounding errors. Not sure what else the cause could be.
                SpecialFunctions.CheckCondition(//score.Loglikelihood == scoreLive.Loglikelihood,
                    Math.Abs(score.Loglikelihood - scoreLive.Loglikelihood) < 10e-7, 
                    "Cached score " + score.Loglikelihood + " doesn't match live score " + scoreLive.Loglikelihood); 

#endif 
            }
            else
            {
                CacheMisses++;
                score = MaximizeLikelihoodInternal(messageInitializer);
 
                if (_cache.Count > MAX_CACHE_SIZE) 
                {
                    //_cache.Clear(); 
                    ClearCache();
                }
                _cache.Add(messageInitializer, score);
            }
            #endregion
 
            return score; 
        }
 
        protected Score MaximizeLikelihoodInternal(MessageInitializer messageInitializer)
        {
            OptimizationParameterList paramsToOptimize = messageInitializer.GetOptimizationParameters();
            int functionEvaluationCount = 0;

            bool useLogMethod = false;

            Converter<OptimizationParameterList, double> functionToOptimize = 
                                delegate(OptimizationParameterList paramList) 
                                {
                                    FuncCalls++; 
                                    ++functionEvaluationCount;
                                    //Debug.WriteLine("EvalCount " + functionEvaluationCount.ToString());
                                    double loglikelihood = ComputeLogLikelihoodModelGivenData(messageInitializer, paramList, useLogMethod);

                                    if (!useLogMethod && double.IsNegativeInfinity(loglikelihood))
                                    {
                                        useLogMethod = true;
                                        loglikelihood = ComputeLogLikelihoodModelGivenData(messageInitializer, paramList, useLogMethod);
                                    }
                                    //SpecialFunctions.CheckCondition(!double.IsNaN(loglikelihood), "for debugging: got a NaN from ComputeLogLikelihoodModelGivenData");
                                    //if (double.IsNaN(loglikelihood)) 
                                    //{ 
                                    //    return double.NegativeInfinity;
                                    //} 
                                    return loglikelihood;
                                };

            double loglikelihoodExternal = GridSearch.Optimize(functionToOptimize, paramsToOptimize, 10, 5);

            Score score = Score.GetInstance(loglikelihoodExternal, paramsToOptimize, messageInitializer.PropogationDistribution); 
            Debug.WriteLine(SpecialFunctions.CreateTabString(GridSearch.DebugCount, score, functionEvaluationCount)); 

            return score; 
        }

        public double ComputeLogLikelihoodModelGivenData(MessageInitializer messageInitializer, OptimizationParameterList paramList)
        {
            double loglikelihood = ComputeLogLikelihoodModelGivenData(messageInitializer, paramList, false);
            if (double.IsNegativeInfinity(loglikelihood))
            {
                loglikelihood = ComputeLogLikelihoodModelGivenData(messageInitializer, paramList, true);
            }
            return loglikelihood;
        }

        public abstract double ComputeLogLikelihoodModelGivenData(MessageInitializer messageInitializer, OptimizationParameterList paramList, bool useLogMethod);

        internal void ClearCache()
        { 
            CacheClears++; 
            _cache.Clear();
        } 
    }

    public class ModelScorerDiscrete : ModelScorer
    {
        public ModelScorerDiscrete(PhyloTree tree, GridSearch optimizer) : base(tree, optimizer) { }
 
        public override string Name 
        {
            get { return "Discrete"; } 
        }
        public override double ComputeLogLikelihoodModelGivenData(MessageInitializer messageInitializer, OptimizationParameterList paramList, bool useLogMethod)
        {
            return PhyloTree.ComputeLogLikelihoodModelGivenDataDiscrete(messageInitializer, paramList, useLogMethod);
        }
    }

    /// <summary>
    /// Not yet implemented
    /// </summary>
    public class ModelScorerGaussian : ModelScorer
    {
        public ModelScorerGaussian(PhyloTree tree, GridSearch optimizer) : base(tree, optimizer) { } 
 
        public override string Name
        { 
            get { return "Gaussian"; }
        }

        public override double ComputeLogLikelihoodModelGivenData(MessageInitializer messageInitializer, OptimizationParameterList paramList, bool useLogMethod)
        {
            return PhyloTree.ComputeLogLikelihoodModelGivenDataGaussian(messageInitializer, paramList); 
        } 

    }


}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
