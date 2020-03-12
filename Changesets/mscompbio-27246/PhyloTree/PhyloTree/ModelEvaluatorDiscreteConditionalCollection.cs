using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;

namespace VirusCount.PhyloTree
{ 
    public class ModelEvaluatorDiscreteConditionalCollection : ModelEvaluatorCollection 
    {
        public const string BaseName = "DiscreteConditionalCollection"; 
        private string _collectionType;

        protected ModelEvaluatorDiscreteConditionalCollection(List<ModelEvaluator> modelsToEvaluate, string collectionType)
            :
            base(modelsToEvaluate)
        { 
            _collectionType = collectionType; 
        }
 
        new public static ModelEvaluatorDiscreteConditionalCollection GetInstance(string collectionType, ModelScorer scorer)
        {
            collectionType = collectionType.ToLower();
            SpecialFunctions.CheckCondition(collectionType.Equals("onedirection") || collectionType.Equals("bothdirections"), "ModelEvaluatorDiscreteConditionalCollection must be of type \"OneDirection\" or \"BothDirections\"");
            List<ModelEvaluator> models = new List<ModelEvaluator>();
 
            models.Add(ModelEvaluatorDiscreteConditional.GetInstance("Attraction", scorer, true)); 
            models.Add(ModelEvaluatorDiscreteConditional.GetInstance("Repulsion", scorer, true));
            models.Add(ModelEvaluatorDiscreteConditional.GetInstance("Escape", scorer, true)); 
            models.Add(ModelEvaluatorDiscreteConditional.GetInstance("Reversion", scorer, true));


            if (collectionType.Equals("bothdirections"))
            {
                collectionType = "BothDirections"; 
                models.Add(ModelEvaluatorReverse.GetInstance(ModelEvaluatorDiscreteConditional.GetInstance("Attraction", scorer, true))); 
                models.Add(ModelEvaluatorReverse.GetInstance(ModelEvaluatorDiscreteConditional.GetInstance("Repulsion", scorer, true)));
                models.Add(ModelEvaluatorReverse.GetInstance(ModelEvaluatorDiscreteConditional.GetInstance("Escape", scorer, true))); 
                models.Add(ModelEvaluatorReverse.GetInstance(ModelEvaluatorDiscreteConditional.GetInstance("Reversion", scorer, true)));
            }
            else
            {
                collectionType = "OneDirection";
            } 
            return new ModelEvaluatorDiscreteConditionalCollection(models, collectionType); 
        }
 
        public override string Name
        {
            get { return BaseName + _collectionType; }
        }
    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
