using System; 
using System.Collections.Generic;
using System.Text;
using System.IO;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using SparseToFisher; 
 
namespace VirusCount.PhyloTree
{ 
    public class ModelTesterDiscreteFisher : ModelTesterDiscrete
    {
        new public const string Name = "Fisher";


        protected ModelTesterDiscreteFisher() 
            : // need a distribution that can model a variable independently working it's way down the tree. 
            base(DistributionDiscreteBinary.GetInstance("Escape"), DistributionDiscreteBinary.GetInstance("Escape"))
        { } 

        public static ModelTesterDiscreteFisher GetInstance()
        {
            return new ModelTesterDiscreteFisher();
        }
 
        public override string ToString() 
        {
            return "FishersExactTest"; 
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
                        PhyloTree.TargetVariableColumnName,
                        TwoByTwo.CountsHeader, 
                        "PValue"); 
            }
        } 

        protected override string CreateReportLine(
            ModelScorer modelScorer,
            PhyloTree phyloTree,
            RowData rowAndTargetData,
            UniversalWorkList workList, 
            int rowIndex, int workListCount, int workIndex) 
        {
            Dictionary<string, string> row = rowAndTargetData.Row; 
            string predictorVariable = row[PhyloTree.PredictorVariableColumnName]; // e.g. hla
            string targetVariable = row[PhyloTree.TargetVariableColumnName]; // e.g. A@182 (amino acid "A" at position 182)
            int nullIndex = int.Parse(row[PhyloTree.NullIndexColumnName]);

            //Dictionary<string, bool> caseIdToNonMissingPredictorValue = workList.NullIndexToPredictorToCaseIdToNonMissingValue[nullIndex][predictorVariable];
            Dictionary<string, SufficientStatistics> caseIdToNonMissingPredictorValue = rowAndTargetData.PredictorData; //workList.GetCaseIdToNonMissingValueForNullIndexAndPredictorVariable(nullIndex, predictorVariable); 
            Dictionary<string, SufficientStatistics> caseIdToNonMissingTargetValue = rowAndTargetData.TargetData; 

            TwoByTwo fishers2by2 = TwoByTwo.GetInstance( 
                SufficientStatisticsMapToIntMap(caseIdToNonMissingPredictorValue),
                SufficientStatisticsMapToIntMap(caseIdToNonMissingTargetValue));

            double pValue = fishers2by2.FisherExactTest;

            string reportLine = SpecialFunctions.CreateTabString(this, rowIndex, workListCount, workIndex, nullIndex, 
                predictorVariable, 
                targetVariable,
                fishers2by2.CountsString(), 
                fishers2by2.FisherExactTest);

            return reportLine;
        }

        private Dictionary<string, int> SufficientStatisticsMapToIntMap(Dictionary<string, SufficientStatistics> caseIdToNonMissingValue) 
        { 
            Dictionary<string, int> result = new Dictionary<string, int>(caseIdToNonMissingValue.Count);
            foreach (KeyValuePair<string, SufficientStatistics> pair in caseIdToNonMissingValue) 
            {
                result.Add(pair.Key, (int)(BooleanStatistics)pair.Value);
            }
            return result;
        }
 
        public override Converter<Leaf, SufficientStatistics> CreateAlternativeSufficientStatisticsMap(Converter<Leaf, SufficientStatistics> predictorDistnClassFunction, Converter<Leaf, SufficientStatistics> targetDistnClassFunction) 
        {
            throw new NotSupportedException("This method does not make sense for this class."); 
        }

        protected override string NullModelParametersAndLikelihoodHeaderString
        {
            get { throw new Exception("The method or operation is not implemented."); }
        } 
 
        protected override string AlternativeModelParametersAndLikelihoodHeaderString
        { 
            get { throw new Exception("The method or operation is not implemented."); }
        }

        protected override double ComputeLLR(ModelScorer modelScorer, PhyloTree phyloTree, StringBuilder stringBuilder, double targetMarginal, double predictorMarginal, Converter<Leaf, SufficientStatistics> predictorDistributionClassFunction, Converter<Leaf, SufficientStatistics> targetDistributionClassFunction)
        {
            throw new Exception("The method or operation is not implemented."); 
        } 
    }
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
