using System; 
using System.Collections.Generic;
using System.Text;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
 
namespace VirusCount.PhyloTree 
{
    public abstract class ModelTesterDiscrete : ModelTester 
    {
        public const string Name = "Discrete";


        //protected override NullDataCollection CreateNullDataGenerator(
        //    ModelScorer modelScorer, 
        //    PhyloTree phyloTree, 
        //    RangeCollection nullIndexRangeCollection,
        //    IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> predictorNameAndCaseIdToNonMissingValueEnumeration, 
        //    IEnumerable<Pair<string, Dictionary<string, SufficientStatistics>>> targetNameAndCaseIdToNonMissingValueEnumeration)
        //{
        //    //NullDataGenerator nullDataGenerator = new NullDataGeneratorTargetAlongTree(modelScorer, phyloTree, this);
        //    //NullDataGenerator nullDataGenerator = new NullDataGeneratorPredictorAlongTree(modelScorer, phyloTree, this);
        //    //NullDataGenerator nullDataGenerator = new NullDataGeneratorPredictorPermutation();
        //    NullDataGenerator nullDataGenerator = new NullDataGeneratorTargetPermutation(); 
 
        //    return NullDataCollection.GetInstance(
        //        nullDataGenerator, 
        //        nullIndexRangeCollection,
        //        predictorNameAndCaseIdToNonMissingValueEnumeration,
        //        targetNameAndCaseIdToNonMissingValueEnumeration);

        //}
 
 
        protected ModelTesterDiscrete(DistributionDiscrete nullModel, DistributionDiscrete altModel) :
            base(nullModel, altModel) 
        { }

        new public static ModelTesterDiscrete GetInstance(string modelTesterNameAndArguments)
        {
            if (modelTesterNameAndArguments.StartsWith(ModelTesterDiscreteFisher.Name))
            { 
                return ModelTesterDiscreteFisher.GetInstance(); 
            }
            else if (modelTesterNameAndArguments.StartsWith(ModelTesterDiscreteConditional.Name)) 
            {
                string leafDistnName = modelTesterNameAndArguments.Substring(ModelTesterDiscreteConditional.Name.Length);
                DistributionDiscreteBinary distn = DistributionDiscreteBinary.GetInstance(leafDistnName);
                return ModelTesterDiscreteConditional.GetInstance(distn);
            }
            else if (modelTesterNameAndArguments.StartsWith(ModelTesterDiscreteJoint.Name)) 
            { 
                string altDistnName = modelTesterNameAndArguments.Substring(ModelTesterDiscreteJoint.Name.Length);
                DistributionDiscreteBinary nullDistn = DistributionDiscreteBinary.GetInstance("Repulsion"); // leafDistn doesn't matter 
                DistributionDiscreteJointBinary altDistn = DistributionDiscreteJointBinary.GetInstance(altDistnName);
                return ModelTesterDiscreteJoint.GetInstance(nullDistn, altDistn);
            }
            else
            {
                throw new ArgumentException(String.Format("{0} does not start with a valid ModelTesterDiscrete name.", modelTesterNameAndArguments)); 
            } 
        }
 
        new public DistributionDiscrete NullModelDistribution
        {
            get { return (DistributionDiscrete)base.NullModelDistribution; }
        }
        public DistributionDiscrete AlternativeModelDistribution
        { 
            get { return (DistributionDiscrete)base.AltModelDistribution; } 
        }
 
        protected abstract string NullModelParametersAndLikelihoodHeaderString
        {
            get;
        }

        protected abstract string AlternativeModelParametersAndLikelihoodHeaderString 
        { 
            get;
        } 

        public override string Header
        {
            get
            {
                return SpecialFunctions.CreateTabString( 
                        "LeafDistribution", 
                        "rowIndex",
                        "rowCount", 
                        "pieceIndex",
                        PhyloTree.NullIndexColumnName,
                        PhyloTree.PredictorVariableColumnName,
                        PhyloTree.PredictorFalseNameCountColumnName,
                        PhyloTree.PredictorTrueNameCountColumnName,
                        PhyloTree.PredictorNonMissingCountColumnName, 
                        PhyloTree.TargetVariableColumnName, 
                        PhyloTree.TargetFalseNameCountColumnName,
                        PhyloTree.TargetTrueNameCountColumnName, 
                        PhyloTree.TargetNonMissingCountColumnName,
                        SparseToFisher.TwoByTwo.CountsHeader,
                        PhyloTree.GlobalNonMissingCountColumnName,
                        NullModelParametersAndLikelihoodHeaderString,
                        AlternativeModelParametersAndLikelihoodHeaderString,
                        "diff", 
                        "PValue"); 
            }
        } 

        //protected override NullDataCollection CreateNullDataGenerator(ModelScorer modelScorer, PhyloTree phyloTree, RangeCollection nullIndexRangeCollection, Dictionary<string, Dictionary<string, BooleanStatistics>> predictorVariableToCaseIdToRealNonMissingValue)
        //{
        //    if (DateTime.Now.Date == new DateTime(2006, 6, 28).Date)  // for testing, force it to use the parametric bootstrap
        //    {
        //        return NullDataCollection.GetInstance( 
        //            new NullDataGeneratorAlongTree(modelScorer, phyloTree, (ModelTesterDiscrete)this), 
        //            nullIndexRangeCollection,
        //            predictorVariableToCaseIdToRealNonMissingValue); 
        //    }


        //    return base.CreateNullDataGenerator(modelScorer, phyloTree, nullIndexRangeCollection, predictorVariableToCaseIdToRealNonMissingValue);
        //}
 
        //public override Converter<Leaf, SufficientStatistics> CreateTargetSufficientStatisticsMap(Dictionary<string, ISufficientStatistics> caseIdToNonMissingValue) 
        //{
        //    return ISufficientStatistics.DictionaryToLeafMap(caseIdToNonMissingValue); 
        //}

        //public override Converter<Leaf, SufficientStatistics> CreatePredictorSufficientStatisticsMap(Dictionary<string, BooleanStatistics> caseIdToNonMissingValue)
        //{
        //    return CreateTargetSufficientStatisticsMap(caseIdToNonMissingValue);
        //} 
 
        protected override string CreateReportLine(
            ModelScorer modelScorer, 
            PhyloTree phyloTree,
            RowData rowAndTargetData,
            UniversalWorkList workList,
            int rowIndex, int workListCount, int workIndex)
        {
            //!!!there is very similar code in ModelTesterGaussian.cs 
 
            // we're iterating over each predictor (e.g. hla), each target (e.g. position in the sequence,
            // and each possible substring at that position). 
            // Then we ask the question, Does the presence of predictor (e.g. hla)
            // influence the probability that target (e.g. mer in position n1pos) will show up?
            // nullIndex specifies whether this is the true data or randomized data.
            Dictionary<string, string> row = rowAndTargetData.Row;
            string predictorVariable = row[PhyloTree.PredictorVariableColumnName]; // e.g. hla
            string targetVariable = row[PhyloTree.TargetVariableColumnName]; // e.g. A@182 (amino acid "A" at position 182) 
            int nullIndex = int.Parse(row[PhyloTree.NullIndexColumnName]); 

            //Dictionary<string, bool> caseIdToNonMissingPredictorValue = workList.NullIndexToPredictorToCaseIdToNonMissingValue[nullIndex][predictorVariable]; 
            Dictionary<string, SufficientStatistics> caseIdToNonMissingPredictorValue = rowAndTargetData.PredictorData; //workList.GetCaseIdToNonMissingValueForNullIndexAndPredictorVariable(nullIndex, predictorVariable);
            Dictionary<string, SufficientStatistics> caseIdToNonMissingTargetValue = rowAndTargetData.TargetData;

            IEnumerator<SufficientStatistics> enumerator = caseIdToNonMissingPredictorValue.Values.GetEnumerator();
            enumerator.MoveNext();
            SufficientStatistics representative = enumerator.Current; 
            bool predictorIsBoolean = representative is BooleanStatistics; 

            Converter<Leaf, SufficientStatistics> targetDistributionClassFunction = CreateSufficientStatisticsMap(caseIdToNonMissingTargetValue); 
            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction = CreateSufficientStatisticsMap(caseIdToNonMissingPredictorValue);

            int[] predictorCounts = predictorIsBoolean ?
                phyloTree.CountsOfLeaves(predictorDistributionClassFunction, NullModelDistribution) : new int[2];
            int[] targetCounts = phyloTree.CountsOfLeaves(targetDistributionClassFunction, NullModelDistribution);
 
 
            int predictorFalseNameCount = predictorCounts[(int)DistributionDiscreteBinary.DistributionClass.False];
            int predictorTrueNameCount = predictorCounts[(int)DistributionDiscreteBinary.DistributionClass.True]; 
            int targetFalseNameCount = targetCounts[(int)DistributionDiscreteBinary.DistributionClass.False];
            int targetTrueNameCount = targetCounts[(int)DistributionDiscreteBinary.DistributionClass.True];

            int[] fisherCounts = predictorIsBoolean ?
                phyloTree.FisherCounts(predictorDistributionClassFunction, targetDistributionClassFunction) : new int[4];
 
            int globalNonMissingCount = predictorIsBoolean ? 
                fisherCounts[0] + fisherCounts[1] + fisherCounts[2] + fisherCounts[3] :
                phyloTree.GlobalNonMissingCount(predictorDistributionClassFunction, targetDistributionClassFunction); 

            StringBuilder stringBuilder = new StringBuilder(
                SpecialFunctions.CreateTabString(this, rowIndex, workListCount, workIndex, nullIndex, predictorVariable,
                    predictorFalseNameCount,
                    predictorTrueNameCount,
                    predictorTrueNameCount + predictorFalseNameCount, 
                    targetVariable, 
                    targetFalseNameCount,
                    targetTrueNameCount, 
                    targetTrueNameCount + targetFalseNameCount,
                    fisherCounts[0], fisherCounts[1], fisherCounts[2], fisherCounts[3],
                    globalNonMissingCount,
                    ""));

            bool ignoreRow = false; 
            foreach (int[] counts in new int[][] { predictorIsBoolean ? predictorCounts : new int[] { 1, 1 }, targetCounts }) 
            {
                foreach (int count in counts) 
                {
                    if (count == 0)
                        ignoreRow = true;
                }
            }
 
            if (ignoreRow) 
            {
                CompleteRowWithNaN(stringBuilder); 
            }
            else
            {
                double targetMarginal = (double)targetTrueNameCount / (double)(targetTrueNameCount + targetFalseNameCount);
                double predictorMarginal = (double)predictorTrueNameCount / (double)(predictorTrueNameCount + predictorFalseNameCount);
 
                double diff = ComputeLLR(modelScorer, phyloTree, stringBuilder, targetMarginal, predictorMarginal, predictorDistributionClassFunction, targetDistributionClassFunction); 

 
                double pValue = SpecialFunctions.LogLikelihoodRatioTest(Math.Max(diff, 0), ChiSquareDegreesOfFreedom);

                stringBuilder.Append(SpecialFunctions.CreateTabString(diff, pValue));
            }

            return stringBuilder.ToString(); 
        } 

        /// <summary> 
        /// Computes the Log Likelihood ratio. Called from CreateReportLineDiscrete. Currently, a pValue is computed from this diff, assuming
        /// the LLR has a difference of 1 DF. !!!May need to change this in future versions to allow us to specific DF.
        /// ComputeLLR must print the value of any parameters that are specific to it (it should not print the return value).
        /// </summary>
        protected abstract double ComputeLLR(ModelScorer modelScorer, PhyloTree phyloTree, StringBuilder stringBuilder, double targetMarginal, double predictorMarginal,
            Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction, Converter<Leaf, SufficientStatistics> targetDistributionClassFunction); 
 

    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
