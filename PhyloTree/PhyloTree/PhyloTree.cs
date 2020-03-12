///Ideas for refactoring 
///  PhyloTree will represent only the tree and all the inference methods (e.g. RunDiscrete) will be
///  there own class and take a PhyloTree as input
using System;
using System.Collections.Generic;
using System.Text;
using System.IO; 
using System.Diagnostics; 
using Msr.Mlas.SpecialFunctions;
using Optimization; 
using Mlas.Tabulate;


namespace VirusCount.PhyloTree
{
    public class PhyloTree 
    { 

        private PhyloTree() 
        { 
        }
 
        BranchOrLeaf BranchOrLeaf;

        //private SpecialFunctions SpecialFunctions;
        private List<Leaf> _leaves;
        private string _treeName;
 
        public BranchOrLeaf Root 
        {
            get { return BranchOrLeaf; } 
            set { BranchOrLeaf = value; }
        }

        public IEnumerable<Leaf> LeafCollection
        {
            get 
            { 
                if (_leaves == null)
                { 
                    _leaves = new List<Leaf>();
                    foreach (Leaf leaf in Root.AllLeaves())
                    {
                        _leaves.Add(leaf);
                    }
                } 
                return _leaves; 
            }
        } 


        Dictionary<int, string> PidToCaseName = null;

        static public PhyloTree GetInstance(StreamReader streamreaderTree, string leafNameToPidOrNullFileName)
        { 
 
            //DebugStream(streamreader);
            PhyloTree phyloTree = new PhyloTree(); 
            phyloTree._treeName = (leafNameToPidOrNullFileName == null) ? "tree" : SpecialFunctions.BaseFileName(leafNameToPidOrNullFileName);

            Dictionary<string, int?> leafNameToPidOrNull;
            CreateLeafNameToPidAndVisaVersa(leafNameToPidOrNullFileName, out leafNameToPidOrNull, out phyloTree.PidToCaseName);
            //phyloTree.SpecialFunctions = SpecialFunctions.GetInstance();
            phyloTree.BranchOrLeaf = CreateBranchOrLeaf(streamreaderTree); 
 
            Set<string> leafNameSet = phyloTree.LeafNameSet();
            if (leafNameToPidOrNull != null) 
            {
                SpecialFunctions.CheckCondition(leafNameSet.Equals(Set<string>.GetInstance(leafNameToPidOrNull.Keys)));
            }

            phyloTree.NormalizeDistancesToOne(0.00001);
            return phyloTree; 
        } 

        public static PhyloTree GetRandomInstance(int depth, double maxBranchLength, ref Random random) 
        {
            SpecialFunctions.CheckCondition(depth > 0);
            PhyloTree aPhyloTree = new PhyloTree();

            Branch root = new Branch();
            root.IsRoot = true; 
            root.GenerateRandomBinarySubtree(depth, maxBranchLength, ref random); 

            aPhyloTree.Root = root; 
            int counter = 0;
            foreach (Leaf leaf in aPhyloTree.Root.AllLeaves())
            {
                leaf.CaseName = (counter++).ToString();
            }
            return aPhyloTree; 
        } 

        public Set<string> LeafNameSet() 
        {
            Set<string> leafNameSet = Set<string>.GetInstance();
            foreach (Leaf leaf in LeafCollection)
            {
                leafNameSet.AddNew(leaf.CaseName);
            } 
            return leafNameSet; 
        }
 
        private static BranchOrLeaf CreateBranchOrLeaf(StreamReader streamreaderTree)
        {
            BranchOrLeaf branchOrLeaf = BranchOrLeaf.GetInstance(streamreaderTree, true);
            if (streamreaderTree.Peek() != ';')
            {
                double length = BranchOrLeaf.ReadLength(streamreaderTree, /*changeZeroToEpislon*/ false); 
                if (streamreaderTree.Peek() == ':') 
                {
                    BranchOrLeaf.Read(streamreaderTree); 
                    int nodeId = (int)length;
                    Debug.WriteLine(SpecialFunctions.CreateTabString("NodeId", nodeId));
                    length = BranchOrLeaf.ReadLength(streamreaderTree, /*changeZeroToEpislon*/ false);
                }

                SpecialFunctions.CheckCondition(length == 0); 
            } 
            char semi = BranchOrLeaf.Read(streamreaderTree);
            SpecialFunctions.CheckCondition(semi == ';'); 
            SpecialFunctions.CheckCondition(streamreaderTree.ReadToEnd().Trim() == "");
            return branchOrLeaf;
        }

        //!!!would be better if the two dictionaries were a class that did the mapping either via the mapping in the file or with the identity function
        public static void CreateLeafNameToPidAndVisaVersa(string leafNameToPidOrNullFileName, /* Set<string> leafNameSet, */ out Dictionary<string, int?> leafNameToPidOrNull, out Dictionary<int, string> pidToLeafName) 
        { 
            leafNameToPidOrNull = new Dictionary<string, int?>();
            pidToLeafName = new Dictionary<int, string>(); 

            if (leafNameToPidOrNullFileName == null)
            {
                leafNameToPidOrNull = null;
                pidToLeafName = null;
                return; 
                //foreach (string leafName in LeafNameSet()) 
                //{
                //    int pidSameAsLeaf; 
                //    int.TryParse(leafName, pidSameAsLeaf);
                //    leafNameToPidOrNull.Add(leafName, pidSameAsLeaf);
                //    pidToLeafName.Add(pidSameAsLeaf, leafName);
                //}
                //return;
            } 
 
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(leafNameToPidOrNullFileName, "leafName\tpidOrNull", false)) //!!!these constants are repeated in this file
            { 
                string leafName = row["leafName"];
                int? pidOrNull = (row["pidOrNull"].ToLower() == "null") ? (int?)null : (int?)int.Parse(row["pidOrNull"]);
                leafNameToPidOrNull.Add(leafName, pidOrNull);
                if (pidOrNull != null)
                {
                    pidToLeafName.Add((int)pidOrNull, leafName); 
                } 
            }
        } 

        private static void DebugStream(StreamReader streamreader)
        {
            int leftMoreThanRight = 0;
            int paramId = 99;
            Stack<int> parensSeen = new Stack<int>(); 
            foreach (char c in streamreader.ReadToEnd()) 
            {
                if (c == '(') 
                {
                    ++paramId;
                    parensSeen.Push(paramId);
                    ++leftMoreThanRight;
                    string tabs = new string('\t', leftMoreThanRight - 1);
                    Debug.WriteLine(string.Format("\n{0}\t{1}\t{2}", tabs, paramId, leftMoreThanRight)); 
                    Debug.Write(string.Format("{0}\t{1}", tabs, c)); 
                }
                else if (c == ')') 
                {
                    int leftId = parensSeen.Pop();
                    --leftMoreThanRight;
                    string tabs = new string('\t', leftMoreThanRight);
                    Debug.WriteLine(string.Format("\n{0}\t{1}\t{2}", tabs, leftId, leftMoreThanRight));
                    Debug.Write(string.Format("{0}\t{1}", tabs, c)); 
                } 
                else if (c == '\f' || c == '\r')
                { 
                    //do nothing
                }
                else
                {
                    Debug.Write(string.Format("{0}", c));
                } 
            } 
        }
 
        public void ChangeLeafNames(string leafNameToPidOrNullFileName)
        {
            Dictionary<string, int?> leafNameToPidOrNull;
            CreateLeafNameToPidAndVisaVersa(leafNameToPidOrNullFileName, out leafNameToPidOrNull, out this.PidToCaseName);
            int nullCount = -1;
            foreach (Leaf leaf in LeafCollection) 
            { 
                string oldName = leaf.CaseName;
                string newName = SpecialFunctions.GetValueOrDefault(leafNameToPidOrNull, oldName).ToString(); 
                if (newName != "")
                {
                    leaf.CaseName = newName;
                }
                else
                { 
                    leaf.CaseName = (nullCount--).ToString(); 
                }
            } 
        }

        public string ToPhylipCompatableString()
        {
            StringBuilder stringBuilder = new StringBuilder();
 
            ToPhylipCompatableStringInternal(BranchOrLeaf, stringBuilder); 

            string result = stringBuilder.ToString(); 
            result = result.Substring(0, result.LastIndexOf(':'));
            result += ";";

            return result;
        }
 
        private void ToPhylipCompatableStringInternal(BranchOrLeaf node, StringBuilder stringBuilder) 
        {
            if (node is Leaf) 
            {
                stringBuilder.Append(((Leaf)node).PhylipFormattedName + ":" + node.Length);
            }
            else
            {
                stringBuilder.Append("("); 
                bool firstKid = true; 
                foreach (BranchOrLeaf child in ((Branch)node).BranchOrLeafCollection)
                { 
                    SpecialFunctions.CheckCondition(child != node);
                    if (!firstKid)
                    {
                        stringBuilder.Append(",");
                    }
                    ToPhylipCompatableStringInternal(child, stringBuilder); 
                    firstKid = false; 
                }
                stringBuilder.Append("):" + node.Length); 
            }
        }

        public void CleanTree(Set<string> leafNamesToKeep)
        {
            Set<string> leafNamesToDelete = new Set<string>(); 
            foreach (Leaf leaf in LeafCollection) 
            {
                if (!leafNamesToKeep.Contains(leaf.CaseName)) 
                {
                    leafNamesToDelete.AddNew(leaf.CaseName);
                }
            }
            DeleteLeavesFromTree(leafNamesToDelete);
        } 
 
        public void DeleteLeavesFromTree(Set<string> leafNamesToDelete)
        { 
            Root = DeleteRootOfSubtree(Root, leafNamesToDelete);
        }

        private BranchOrLeaf DeleteRootOfSubtree(BranchOrLeaf node, Set<string> leafNamesToDelete)
        {
            if (node is Leaf) 
            { 
                if (leafNamesToDelete.Contains(((Leaf)node).CaseName))
                    return null; 
                else
                    return node;
            }
            else
            {
                Branch nodeAsBranch = (Branch)node; 
                List<BranchOrLeaf> newBranchOrLeafCollection = new List<BranchOrLeaf>(); 
                foreach (BranchOrLeaf child in nodeAsBranch.BranchOrLeafCollection)
                { 
                    BranchOrLeaf newRoot = DeleteRootOfSubtree(child, leafNamesToDelete);
                    if (newRoot != null)
                    {
                        newBranchOrLeafCollection.Add(newRoot);
                    }
                } 
 
                nodeAsBranch.BranchOrLeafCollection = newBranchOrLeafCollection;
 
                switch (nodeAsBranch.BranchOrLeafCollection.Count)
                {
                    case 0: // a Branch node without children is useless.
                        return null;
                    case 1: // a Branch node with 1 child should be absorbed into the branch length of that child to its parent
                        BranchOrLeaf newRoot = nodeAsBranch.BranchOrLeafCollection[0]; 
                        newRoot.Length = newRoot.Length + node.Length; 
                        return newRoot;
                    default: 
                        return node;
                }
            }
        }

        public double[] MarginalOfLeaves(Converter<Leaf, SufficientStatistics> leafToDiscreteDistnFunction, DistributionDiscrete distributionDiscrete) 
        { 
            int[] counts = new int[distributionDiscrete.NonMissingClassCount];
            double[] marginals = new double[distributionDiscrete.NonMissingClassCount]; 

            int nonMissingCount = 0;
            foreach (Leaf leaf in LeafCollection)
            {
                SufficientStatistics classBin = leafToDiscreteDistnFunction(leaf);
                if (!classBin.IsMissing() && classBin is DiscreteStatistics) 
                { 
                    nonMissingCount++;
                    counts[(DiscreteStatistics)classBin]++; 
                }
            }
            for (int i = 0; i < counts.Length; i++)
                marginals[i] = (double)counts[i] / nonMissingCount;

            return marginals; 
        } 

        public int CountOfNonMissingLeaves<T>(Dictionary<string, T> caseIdToSomething) 
        {
            int count = 0;

            foreach (Leaf leaf in LeafCollection)
            {
                if (caseIdToSomething.ContainsKey(leaf.CaseName)) 
                { 
                    ++count;
                } 
            }
            return count;
        }

        //public int CountOfNonMissingLeaves(Converter<Leaf, SufficientStatistics> map)
        //{ 
        //    int count = 0; 

        //    foreach (Leaf leaf in LeafCollection) 
        //    {
        //        if (!map(leaf).IsMissing())
        //        {
        //            ++count;
        //        }
        //    } 
        //    return count; 
        //}
 
        public int CountOfNonMissingLeaves(params Converter<Leaf, SufficientStatistics>[] maps)
        {
            int count = 0;

            foreach (Leaf leaf in LeafCollection)
            { 
                bool isMissing = false; 
                foreach (Converter<Leaf, SufficientStatistics> map in maps)
                { 
                    if (map(leaf).IsMissing())
                    {
                        isMissing = true;
                    }
                }
                if (!isMissing) 
                { 
                    count++;
                } 
            }
            return count;
        }


        public int[] CountsOfLeaves(Converter<Leaf, SufficientStatistics> leafToDiscreteDistnFunction) 
        { 
            return CountsOfLeaves(leafToDiscreteDistnFunction, null);
        } 

        public int[] CountsOfLeaves(Converter<Leaf, SufficientStatistics> leafToDiscreteDistnFunction, DistributionDiscrete distribution)
        {
            int classCount = distribution == null ? 2 : distribution.NonMissingClassCount;
            int[] counts = new int[classCount];
 
            foreach (Leaf leaf in LeafCollection) 
            {
                SufficientStatistics classBin = leafToDiscreteDistnFunction(leaf); 
                if (!classBin.IsMissing() && classBin is DiscreteStatistics)
                {
                    counts[(DiscreteStatistics)classBin]++;
                }
            }
 
            return counts; 
        }
 
        public int[] FisherCounts(Converter<Leaf, SufficientStatistics> leafToBooleanStats1, Converter<Leaf, SufficientStatistics> leafToBooleanStats2)
        {
            int[] counts = new int[4];
            foreach (Leaf leaf in LeafCollection)
            {
                if (!leafToBooleanStats1(leaf).IsMissing() && !leafToBooleanStats2(leaf).IsMissing()) 
                { 
                    bool predTrue = (BooleanStatistics)leafToBooleanStats1(leaf);
                    bool targTrue = (BooleanStatistics)leafToBooleanStats2(leaf); 
                    if (predTrue)
                    {
                        if (targTrue)
                        {
                            counts[0]++;
                        } 
                        else 
                        {
                            counts[1]++; 
                        }
                    }
                    else
                    {
                        if (targTrue)
                        { 
                            counts[2]++; 
                        }
                        else 
                        {
                            counts[3]++;
                        }
                    }
                }
            } 
            return counts; 
        }
 
        public int GlobalNonMissingCount(params Converter<Leaf, SufficientStatistics>[] leafToDiscreteDistnFunctions)
        {
            int nonMissingCount = 0;
            foreach(Leaf leaf in LeafCollection){
                bool isMissing = false;
                foreach (Converter<Leaf, SufficientStatistics> map in leafToDiscreteDistnFunctions) 
                { 
                    if (map(leaf).IsMissing())
                    { 
                        isMissing = true;
                        break;
                    }
                }
                if (!isMissing)
                { 
                    nonMissingCount++; 
                }
            } 
            return nonMissingCount;
        }



        //!!! is the leafNameToPidOrNullFileName ever used? If not remove.
        public static PhyloTree GetInstance(string treeFileName, string leafNameToPidOrNullFileName) 
        { 
            using (StreamReader streamreaderTree = File.OpenText(treeFileName))
            { 
                return PhyloTree.GetInstance(streamreaderTree, leafNameToPidOrNullFileName);
            }
        }


 
        public Dictionary<string, BooleanStatistics> EvolveBinaryTree(double stationaryDistnOfTrue, double lambda, double pMissing, ref Random random) 
        {
            Dictionary<string, BooleanStatistics> caseNameToVal = new Dictionary<string, BooleanStatistics>(); 
            bool startingVal = random.NextDouble() < stationaryDistnOfTrue;
            Root.Evolve(startingVal, stationaryDistnOfTrue, lambda, pMissing, ref random, caseNameToVal);
            return caseNameToVal;
        }

        public void EvolveDiscreteTree(DistributionDiscrete distribution, OptimizationParameterList parameters, 
            ref Random random, 
            ref Dictionary<string, BooleanStatistics> predictorMapToCreate, out Dictionary<string, BooleanStatistics> targetMapToCreate)
        { 
            EvolveDiscreteTree(distribution, parameters, null, ref random, ref predictorMapToCreate, out targetMapToCreate);
        }

        public void EvolveDiscreteTree(DistributionDiscrete distribution, OptimizationParameterList parameters,
            Set<string> targetVariableToMissingLeaves, ref Random random,
            ref Dictionary<string, BooleanStatistics> predictorMapToCreate, 
            out Dictionary<string, BooleanStatistics> targetMapToCreate) 
        {
            int mutFromRootNoImmune, mutFromParentNoImmune, muteFromImmune, mutFromParent, mutFromParentWithHLA; 
            EvolveDiscreteTree(distribution, parameters, targetVariableToMissingLeaves, ref random,
                ref predictorMapToCreate, out targetMapToCreate,
                out mutFromRootNoImmune, out mutFromParentNoImmune, out muteFromImmune, out mutFromParent, out mutFromParentWithHLA);
        }

        public void EvolveDiscreteTree(DistributionDiscrete distribution, OptimizationParameterList parameters, 
            Set<string> targetVariableToMissingLeaves, ref Random random, 
            ref Dictionary<string, BooleanStatistics> predictorMapToCreate, out Dictionary<string, BooleanStatistics> targetMapToCreate,
            out int mutFromRootNoImmune, out int mutFromParentNoImmune, out int muteFromImmune, out int mutFromParent, out int mutFromParentWithHLA) 
        {
            mutFromRootNoImmune = mutFromParentNoImmune = muteFromImmune = mutFromParent = mutFromParentWithHLA = 0;

            targetMapToCreate = new Dictionary<string, BooleanStatistics>(100);

            SpecialFunctions.CheckCondition((predictorMapToCreate != null) == (distribution is DistributionDiscreteConditional), 
                "Conditional distribution must have the predictorFile specified."); 
            if (predictorMapToCreate == null)
            { 
                predictorMapToCreate = new Dictionary<string, BooleanStatistics>(100);
            }

            double[] priors = distribution.GetPriorProbabilities(parameters);
            int discreteClass = SpecialFunctions.PickRandomBin(priors, ref random);
 
            //EvolveDiscreteTreeInternal(Root, (DiscreteStatistics)discreteClass, distribution, parameters, predictorStationaryProbability, 
            //    ref rand, ref predictorMap, ref targetMap);
            Branch root = Root as Branch; 
            SpecialFunctions.CheckCondition(root != null);
            foreach (BranchOrLeaf child in root.BranchOrLeafCollection)
            {
                EvolveDiscreteTreeInternal(child, (DiscreteStatistics)discreteClass, (DiscreteStatistics)discreteClass, distribution, parameters,
                    targetVariableToMissingLeaves, ref random, ref predictorMapToCreate, ref targetMapToCreate,
                    ref  mutFromRootNoImmune, ref  mutFromParentNoImmune, ref  muteFromImmune, ref  mutFromParent, ref mutFromParentWithHLA); 
            } 
        }
 
        private void EvolveDiscreteTreeInternal(BranchOrLeaf branchOrLeaf, DiscreteStatistics rootClass, DiscreteStatistics parentClass,
            DistributionDiscrete distribution, OptimizationParameterList parameters,
            Set<string> targetVariableToMissingLeaves,
            ref Random random, ref Dictionary<string, BooleanStatistics> predictorMapToCreate, ref Dictionary<string, BooleanStatistics> targetMapToCreate,
            ref int mutFromRootNoImmune, ref int mutFromParentNoImmune, ref int muteFromImmune, ref int mutFromParent, ref int mutFromParentWithHLA)
        { 
            if (branchOrLeaf is Branch) 
            {
                Branch branch = branchOrLeaf as Branch; 
                double[][] dist = distribution.CreateDistribution(branch, parameters);
                double[] conditionedDist = dist[(int)parentClass];

                // temp. check my reasoning
                double sum = 0;
                foreach (double p in conditionedDist) 
                    sum += p; 
                SpecialFunctions.CheckCondition(ComplexNumber.ApproxEqual(sum, 1.0, 0.0000001), "Not a probability distribution");
                // end temp 

                int classIndex = SpecialFunctions.PickRandomBin(conditionedDist, ref random);

                foreach (BranchOrLeaf child in branch.BranchOrLeafCollection)
                {
                    EvolveDiscreteTreeInternal(child, rootClass, (DiscreteStatistics)classIndex, distribution, parameters, 
                        targetVariableToMissingLeaves, ref random, ref predictorMapToCreate, ref targetMapToCreate, 
                        ref  mutFromRootNoImmune, ref  mutFromParentNoImmune, ref  muteFromImmune, ref  mutFromParent, ref mutFromParentWithHLA);
                } 
            }
            else
            {
                Leaf leaf = branchOrLeaf as Leaf;
                BooleanStatistics leafHasPredictor;
                predictorMapToCreate.TryGetValue(leaf.CaseName, out leafHasPredictor); 
                if ((targetVariableToMissingLeaves != null && targetVariableToMissingLeaves.Contains(leaf.CaseName)) || 
                    (distribution is DistributionDiscreteConditional && leafHasPredictor == null))
                { 
                    return; // missing data in this one special case. Ignore.
                }
                Converter<Leaf, SufficientStatistics> predictorSufficientStatisticsMap = delegate(Leaf theLeaf)
                {
                    return leafHasPredictor;
                }; 
 
                double[][] dist = distribution.CreateDistribution(leaf, parameters, predictorSufficientStatisticsMap);
                double[] conditionedDist = dist[(int)parentClass]; 

                // temp. check my reasoning
                double sum = 0;
                foreach (double p in conditionedDist)
                    sum += p;
                SpecialFunctions.CheckCondition(ComplexNumber.ApproxEqual(sum, 1.0, 0.0000001), "Not a probability distribution"); 
                // end temp 

                int classIndex = SpecialFunctions.PickRandomBin(conditionedDist, ref random); 

                distribution.ReflectClassInDictionaries((DiscreteStatistics)classIndex, leaf, ref predictorMapToCreate, ref targetMapToCreate);

                //!! hack to get at immune versus ctmp pressure
                if (distribution is DistributionDiscreteConditional)
                { 
                    //DistributionDiscreteBinary.DistributionClass predictorClassification = 
                    //        (DistributionDiscreteBinary.DistributionClass)(int)(DiscreteStatistics)predictorSufficientStatisticsMap(leaf);
 
                    bool noImmune =
                        !((BooleanStatistics)predictorSufficientStatisticsMap(leaf)) ||
                        parameters[(int)DistributionDiscreteConditional.ParameterIndex.Predictor1].Value == 0;

                    int classIndexNoImmune;
                    if (noImmune) 
                    { 
                        classIndexNoImmune = classIndex;
                    } 
                    else
                    {
                        OptimizationParameterList parametersNoImmune = (OptimizationParameterList)parameters.Clone();
                        parametersNoImmune[(int)DistributionDiscreteConditional.ParameterIndex.Predictor1].Value = 0;
                        dist = distribution.CreateDistribution(leaf, parametersNoImmune, predictorSufficientStatisticsMap);
                        conditionedDist = dist[(int)parentClass]; 
                        classIndexNoImmune = SpecialFunctions.PickRandomBin(conditionedDist, ref random); 
                    }
 
                    mutFromRootNoImmune += rootClass != classIndexNoImmune ? 1 : 0;
                    mutFromParentNoImmune += parentClass != classIndexNoImmune ? 1 : 0;
                    mutFromParent += parentClass != classIndex ? 1 : 0;
                    muteFromImmune += classIndex != classIndexNoImmune ? 1 : 0;
                    mutFromParentWithHLA += parentClass != classIndex && (BooleanStatistics)predictorSufficientStatisticsMap(leaf) ? 1 : 0;
                } 
            } 
        }
 

        public List<Set<string>> PartitionIntoClades(int depth)
        {
            List<Set<string>> clades = new List<Set<string>>();
            PartitionIntoCladesInternal((Branch)Root, depth, clades);
            return clades; 
        } 

        private void PartitionIntoCladesInternal(Branch branch, int depth, List<Set<string>> clades) 
        {
            if (depth == 0)
            {
                Set<string> thisClade = new Set<string>();
                foreach (Leaf leaf in branch.AllLeaves())
                { 
                    thisClade.AddNew(leaf.CaseName); 
                }
                clades.Add(thisClade); 
            }
            else
            {
                foreach (BranchOrLeaf branchOrLeaf in branch.BranchOrLeafCollection)
                {
                    if (branchOrLeaf is Branch) 
                    { 
                        PartitionIntoCladesInternal((Branch)branchOrLeaf, depth - 1, clades);
                    } 
                }
            }
        }

        private Dictionary<int, string> CreatePidToCaseName()
        { 
            Dictionary<int, string> pidToCaseName = new Dictionary<int, string>(); 
            foreach (Leaf leaf in this.LeafCollection)
            { 
                int? pidOrNull = Leaf.PatientIdOrNullFromCaseName(leaf.CaseName);
                if (pidOrNull != null)
                {
                    pidToCaseName.Add((int)pidOrNull, leaf.CaseName);
                }
            } 
            return pidToCaseName; 
        }
 

        //!!!replace the keepTest and it's name with an object of the IKeepTest or abstract KeepTest class
        public static void TabulateResultsCondDiscrete(string niceName, string modelTesterNameAndParameters, string keepTestOldName, string keepTestNewName,
            int lastNullIndex, int rowCount, string pValueDirectoryName, string treeFileName,
            string predictorSparseFileName, string targetSparseFileName, string leafNameToPidOrNullFileName, EditRow editRowOrNull, bool oldStyle)
        { 
 

            string outputFilePattern = @"{0}\{1}.PhyloTree.qValues.{2}.all.{3}.{4}.new.txt"; 
            //ModelTesterDiscrete modelTester = ModelTesterDiscrete.GetInstance(modelTesterNameAndParameters);
            //string header = modelTester.Header;

            ModelEvaluator modelEvaluator = ModelEvaluator.GetInstance(modelTesterNameAndParameters, null);
            string header = PhyloDDriver.GetHeaderString(modelEvaluator);
 
            PhyloTree phyloTree = PhyloTree.GetInstance(treeFileName, leafNameToPidOrNullFileName); 

            KeepTest<Dictionary<string, string>> keepTestOld = KeepTest<Dictionary<string, string>>.GetInstance(pValueDirectoryName, keepTestOldName); 
            KeepTest<Dictionary<string, string>> keepTestNew = KeepTest<Dictionary<string, string>>.GetInstance(pValueDirectoryName, keepTestNewName);

            SpecialFunctions.CheckCondition(keepTestOld.IsCompatibleWithNewKeepTest(keepTestNew));

            int firstNullIndex = -1;
 
            int nullCount = lastNullIndex - Math.Max(firstNullIndex, 0) + 1; 

            string outputFile = string.Format(outputFilePattern, pValueDirectoryName, niceName, nullCount, modelEvaluator.Name, keepTestNew); 


            if (!File.Exists(outputFile))
            {
                string leafEtc;
                if (oldStyle) 
                { 
                    leafEtc = "Escape";
                    header = "LeafDistribution\trowIndex\trowCount\tpieceIndex\tNullIndex\tPredictorVariable\tPredictorFalseCount\tPredictorTrueCount\tPredictorNonMissingCount\tTargetVariable\tTargetFalseCount\tTargetTrueCount\tTargetNonMissingCount\tlambda0\tlogLambda0\tEscapeParameter0\tx0\tlogLikelihood0\tlambda1\tlogLambda1\tEscapeParameter1\tx1\tlogLikelihood1\tdiff\tPValue"; 
                }
                else
                {
                    leafEtc = modelEvaluator.ToString();
                }
 
 
                string filePattern = string.Format(@"{0}.PhyloTree.*-*.*.{1}-{2}.{3}.new.txt", niceName, firstNullIndex, lastNullIndex, leafEtc);
                //!!!too many params 
                TabulateInternalDiscrete(lastNullIndex, rowCount, pValueDirectoryName, keepTestOld, keepTestNew,
                    firstNullIndex, nullCount, outputFile, header, editRowOrNull, filePattern);
            }

            //Debug.Fail("Need to adjust for filtered header");
            int iAtLeastPoint2 = 0; 
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(outputFile, header + "\tqValue", false)) 
            {
                double qValue = double.Parse(row["qValue"]); 
                if (qValue <= .2)
                {
                    ++iAtLeastPoint2;
                }
                else
                { 
                    break; 
                }
            } 

            Debug.WriteLine(SpecialFunctions.CreateTabString(modelEvaluator.Name, nullCount, keepTestNew, iAtLeastPoint2, outputFile));
        }

        //!!!replace the keepTest and it's name with an object of the IKeepTest or abstract KeepTest class
        public static bool TryTabulateResultsCorrDiscrete(string modelTesterNameAndParameters, string keepTestOldName, string keepTestNewName, 
            RangeCollection rangeCollection, string dataFilesDirectoryName, string pValuesDirectoryName, string treeName, 
            string leafNameToPidOrNullFileName, double poorPValueThreshold)
        { 
            string outputFilePattern = @"{0}\PhyloTreeCorrDiscrete.qValues.{1}.{2}.{3}.txt";
            //ModelTesterDiscrete modelTester = ModelTesterDiscrete.GetInstance(modelTesterNameAndParameters);
            //string header = modelTester.Header;

            ModelEvaluator modelEvaluator = ModelEvaluator.GetInstance(modelTesterNameAndParameters, null);
            string header = PhyloDDriver.GetHeaderString(modelEvaluator); 
 
            PhyloTree phyloTree = PhyloTree.GetInstance(string.Format(@"{0}\{1}", dataFilesDirectoryName, treeName), string.Format(@"{0}\{1}", dataFilesDirectoryName, leafNameToPidOrNullFileName));
 
            KeepTest<Dictionary<string, string>> keepTestOld = KeepTest<Dictionary<string, string>>.GetInstance(dataFilesDirectoryName, keepTestOldName);
            KeepTest<Dictionary<string, string>> keepTestNew = KeepTest<Dictionary<string, string>>.GetInstance(dataFilesDirectoryName, keepTestNewName);

            SpecialFunctions.CheckCondition(keepTestOld.IsCompatibleWithNewKeepTest(keepTestNew));

            int nullCount = rangeCollection.Count(0, int.MaxValue); 
 
            string outputFile = string.Format(outputFilePattern, pValuesDirectoryName, nullCount, modelEvaluator.Name, keepTestNew);
            outputFile = outputFile.Replace(',', '-'); 
            outputFile = outputFile.Replace("\"", "");

            string filePattern = string.Format(@"PhyloTreeCorr.*.all.{0}.{1}.txt", modelEvaluator.Name, keepTestOld);


            if (!File.Exists(outputFile)) 
            { 
                //!!!too many params
                bool isOK = TryTabulateInternal(filePattern, modelEvaluator.Name, rangeCollection, 
                    pValuesDirectoryName, keepTestOld, keepTestNew, outputFile, header, poorPValueThreshold);

                if (!isOK)
                {
                    return false;
                }; 
 
            }
            else 
            {
                Console.WriteLine(outputFile + " already exists");
            }

            //Debug.WriteLine(SpecialFunctions.CreateTabString(distribution, nullCount, keepTestNew, iAtLeastPoint2, outputFile));
            return true; 
        } 

        private static bool TryTabulateInternal(string filePattern, string name, /*ModelTesterDiscrete modelTester,*/ RangeCollection masterRangeCollection, 
            string directoryName, KeepTest<Dictionary<string, string>> keepTestOld, KeepTest<Dictionary<string, string>> keepTestNew, string outputFile, string header, double minimumPValueOfInterest)
        {

            RangeCollectionCollection collectionOfNullRanges = RangeCollectionCollection.GetInstance(masterRangeCollection);
            Console.WriteLine("Using the following ranges: " + collectionOfNullRanges);
 
            List<Dictionary<string, string>> realRowCollectionToSort = new List<Dictionary<string, string>>(2000000); 
            List<double> nullPValueCollectionToBeSorted = new List<double>(1000000);
 
            Dictionary<RangeCollection, int> rangeToRowCount = new Dictionary<RangeCollection, int>();
            Dictionary<RangeCollection, RangeCollection> nullRangeToRowRange = new Dictionary<RangeCollection, RangeCollection>();

            // use this to save membory consumption. No point saving 2 million pValues of 1 or 500 million pValues greater than .5
            int numberOfNullsWorseThanThreshold = 0;
            int numberOfRealsWorseThanThreshold = 0; 
            int numberOfNullRuns = masterRangeCollection.Count(0, int.MaxValue); 

            foreach (RangeCollection rc in collectionOfNullRanges) 
            {
                nullRangeToRowRange[rc] = RangeCollection.GetInstance();
            }

            foreach (string fileName in Directory.GetFiles(directoryName, filePattern))
            { 
                Console.WriteLine("RealCount: {0} Significant Null Count: {1} Bad Null Count: {2}", 
                    realRowCollectionToSort.Count, nullPValueCollectionToBeSorted.Count, numberOfNullsWorseThanThreshold);
                Console.WriteLine(fileName); 
                int rangeEntryCount = -1; // for memory debugging

                foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(fileName, header, true))
                {
                    int nullIndex = int.Parse(row[Tabulate.NullIndexColumnName]);
 
                    RangeCollection nullRangeCollection = collectionOfNullRanges.GetContainingRangeCollection(nullIndex); 

                    int rowCount; 

                    if (!rangeToRowCount.ContainsKey(nullRangeCollection))
                    {
                        rowCount = int.Parse(row["rowCount"]);
                        rangeToRowCount.Add(nullRangeCollection, rowCount);
                    } 
                    else 
                    {
                        rowCount = rangeToRowCount[nullRangeCollection]; 
                        SpecialFunctions.CheckCondition(rowCount == int.Parse(row["rowCount"]));
                    }

                    RangeCollection rangeCollection = nullRangeToRowRange[nullRangeCollection];
                    SpecialFunctions.CheckCondition(rangeCollection != null, nullRangeCollection + " is not in nullRangeToRowRange");
 
                    rangeEntryCount = rangeCollection.EntryCount; 
                    int rowIndex = int.Parse(row["rowIndex"]);
                    SpecialFunctions.CheckCondition(0 <= rowIndex && rowIndex < rowCount); 

                    if (rangeCollection.TryAdd(rowIndex) && keepTestNew.Test(row))
                    {
                        double pValue = double.Parse(row["PValue"]);
                        if (nullIndex == -1)
                        { 
                            if (pValue > minimumPValueOfInterest) 
                            {
                                numberOfRealsWorseThanThreshold++; 
                            }
                            else
                            {
                                realRowCollectionToSort.Add(row);
                            }
                        } 
                        else 
                        {
                            // all of these scores are the same...they're bad. 
                            if (pValue > minimumPValueOfInterest)
                            {
                                numberOfNullsWorseThanThreshold++;
                            }
                            else
                            { 
                                nullPValueCollectionToBeSorted.Add(pValue); 
                            }
                        } 
                    }
                }
                Console.WriteLine("Range entry count: " + rangeEntryCount);
            }

            foreach (RangeCollection rangeCollection in collectionOfNullRanges) 
            { 
                RangeCollection rowRange = nullRangeToRowRange[rangeCollection];
                if (!rowRange.IsComplete(rangeToRowCount[rangeCollection])) 
                {
                    Console.WriteLine("Range collection is not complete. Still writing output, but be wary of results. Range: " + rangeCollection);
                    foreach (KeyValuePair<int, int> firstAndLast in rangeCollection.Collection)
                    {
                        Console.WriteLine(SpecialFunctions.CreateTabString(name, keepTestNew, firstAndLast.Key, firstAndLast.Value));
                    } 
                    //return false; 
                }
            } 

            Console.WriteLine("Computing q-values");
            Dictionary<Dictionary<string, string>, double> rowToQValue =
                SpecialFunctions.ComputeQValues(ref realRowCollectionToSort, AccessPValueFromPhylotreeRow,
                ref nullPValueCollectionToBeSorted, numberOfNullRuns);
 
            Console.WriteLine("Writing Results."); 
            using (StreamWriter outputStream = File.CreateText(outputFile))
            { 
                outputStream.WriteLine(SpecialFunctions.CreateTabString(header, "qValue"));
                foreach (Dictionary<string, string> row in realRowCollectionToSort)
                {
                    double qValue = rowToQValue[row];
                    outputStream.WriteLine(SpecialFunctions.CreateTabString(row[""], qValue));
                } 
                outputStream.WriteLine("Number of samples with p-value greater than " + minimumPValueOfInterest + 
                    ": " + numberOfRealsWorseThanThreshold);
            } 
            return true;
        }

        private static void TabulateInternalDiscrete(/*ModelTesterDiscrete modelTester,*/ int lastNullIndex, int rowCount, string directoryName, KeepTest<Dictionary<string, string>> keepTestOld, KeepTest<Dictionary<string, string>> keepTestNew, int firstNullIndex, int nullCount, string outputFile, string header, EditRow editRowOrNull, string filePattern)
        {
            //!!!This doesn't list unfiltered row index, so it is harder to restart 
            RangeCollection rangeCollection = RangeCollection.GetInstance(); 
            List<Dictionary<string, string>> realRowCollectionToSort = new List<Dictionary<string, string>>();
            List<double> nullPValueCollectionToBeSorted = new List<double>(); 
            foreach (string fileName in Directory.GetFiles(directoryName, filePattern))
            {
                Debug.WriteLine(fileName);
                foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(fileName, header, true))
                {
                    int rowIndex = int.Parse(row["rowIndex"]); 
                    SpecialFunctions.CheckCondition(rowCount == int.Parse(row["rowCount"])); 
                    SpecialFunctions.CheckCondition(0 <= rowIndex && rowIndex < rowCount);
 
                    if (rangeCollection.TryAdd(rowIndex) && keepTestNew.Test(row))
                    {
                        int nullIndex = int.Parse(row[Tabulate.NullIndexColumnName]);
                        double pValue = double.Parse(row["PValue"]);
                        //if (!double.IsNaN(pValue))  // NaN is a signal that this row should be ignored.
                        { 
                            if (nullIndex == -1) 
                            {
                                realRowCollectionToSort.Add(row); 
                            }
                            else
                            {
                                nullPValueCollectionToBeSorted.Add(pValue);
                            }
                        } 
                    } 
                }
            } 
            if (!rangeCollection.IsComplete(rowCount))
            {
                Console.WriteLine(rangeCollection);
                throw new Exception("Missing rows");
            }
 
            Dictionary<Dictionary<string, string>, double> 
                qValueList = SpecialFunctions.ComputeQValues(ref realRowCollectionToSort, AccessPValueFromPhylotreeRow, ref nullPValueCollectionToBeSorted, nullCount);
 
            using (StreamWriter outputStream = File.CreateText(outputFile))
            {
                if (editRowOrNull != null)
                {
                    editRowOrNull.EditHeader(ref header);
                } 
                outputStream.WriteLine(SpecialFunctions.CreateTabString(header, "qValue")); 
                foreach (Dictionary<string, string> row in realRowCollectionToSort)
                { 
                    if (editRowOrNull != null)
                    {
                        editRowOrNull.Edit(row);
                    }
                    double qValue = qValueList[row];
                    outputStream.WriteLine(SpecialFunctions.CreateTabString(row[""], qValue)); 
                } 
            }
        } 


        public void NormalizeDistancesToOne(double epsilon)
        {
            double longestLength = 0.0;
            //double shortestNonepsilonLength = double.PositiveInfinity; 
            foreach (BranchOrLeaf branchOrLeaf in BranchOrLeafCollectionWithoutRoot()) 
            {
                //if (double.Epsilon != branchOfLeaf.Length) 
                //{
                //    shortestNonepsilonLength = Math.Min(shortestNonepsilonLength, branchOfLeaf.Length);
                //}
                longestLength = Math.Max(longestLength, branchOrLeaf.Length);
            }
            SpecialFunctions.CheckCondition(longestLength > 0.0); 
            foreach (BranchOrLeaf branchOrLeaf in BranchOrLeafCollection()) 
            {
                if (double.Epsilon != branchOrLeaf.Length) 
                {
                    branchOrLeaf.Length = branchOrLeaf.Length / longestLength;
                }
                else
                {
                    branchOrLeaf.Length = epsilon / longestLength; 
                } 
            }
        } 

        private IEnumerable<BranchOrLeaf> BranchOrLeafCollection()
        {
            return BranchOrLeaf.AllBranchesOrLeaves();
        }
 
        private IEnumerable<BranchOrLeaf> BranchOrLeafCollectionWithoutRoot() 
        {
            return BranchOrLeaf.AllBranchesOrLeavesExceptRoot(); 
        }

        public double ComputeLogLikelihoodModelGivenDataGaussian(MessageInitializer messageInitializer, OptimizationParameterList gaussianParameters) 
        {
            throw new NotImplementedException("not yet ready");
        }
 

        public static void CreateStartingLeafNameToPidOrNullFileName(string treeFileName, string leafNameToPidOrNullFileName)
        {
            BranchOrLeaf branchOrLeaf;
            using (StreamReader streamreaderTree = File.OpenText(treeFileName)) 
            { 
                branchOrLeaf = CreateBranchOrLeaf(streamreaderTree);
            } 
            using (StreamWriter streamwriterLeafNameToPidOrNull = File.CreateText(leafNameToPidOrNullFileName))
            {
                streamwriterLeafNameToPidOrNull.WriteLine(SpecialFunctions.CreateTabString("leafName", "pidOrNull"));
                foreach (Leaf leaf in branchOrLeaf.AllLeaves())
                {
                    streamwriterLeafNameToPidOrNull.WriteLine(SpecialFunctions.CreateTabString(leaf.CaseName, leaf.CaseName)); 
                } 
            }
        } 

        //public double ComputeLogLikelihoodModelGivenDataDiscrete(
        //    DistributionDiscrete discreteDistribution,
        //    OptimizationParameterList discreteParameters,
        //    LeafToDiscreteDistributionClass predictorClassFunction,
        //    LeafToDiscreteDistributionClass targetClassFunction) 
        public double ComputeLogLikelihoodModelGivenDataDiscrete(
            MessageInitializer messageInitializer, OptimizationParameterList discreteParameters, bool useLogTransform) 
        {
            Branch root = BranchOrLeaf as Branch; 
            SpecialFunctions.CheckCondition(root != null);

            DistributionDiscrete discreteDistribution = (DistributionDiscrete)messageInitializer.PropogationDistribution;

            try
            {
                double[] logDist = discreteDistribution.GetPriorProbabilities(discreteParameters);
                if (useLogTransform)
                {
                    SpecialFunctions.ConvertToLog(logDist);
                }

                List<double[]> childDistList = new List<double[]>(); 
                foreach (BranchOrLeaf child in root.BranchOrLeafCollection)
                {
                    //double[] childDist = ComputeLogLikelihoodModelGivenDataInternalDiscrete(child, discreteDistribution, discreteParameters,
                    //    predictorClassFunction, targetClassFunction);
                    double[] childDist = ComputeLogLikelihoodModelGivenDataInternalDiscrete(child, messageInitializer, discreteParameters, useLogTransform);
                    childDistList.Add(childDist); 
                }

                double logP = useLogTransform ? double.NegativeInfinity : 0;

                double[] childStateLogProducts = new double[logDist.Length];
                for (int childState = 0; childState < logDist.Length; ++childState)
                {
                    double product = logDist[childState];
                    foreach (double[] childDist in childDistList)
                    {
                        if (useLogTransform)
                        {
                            product += childDist[childState];
                        }
                        else
                        {
                            product *= childDist[childState];
                        }
                    }
                    //logP += product;
                    //logP = SpecialFunctions.LogSum(logP, product);
                    childStateLogProducts[childState] = product;
                }

                logP = useLogTransform ?
                    SpecialFunctions.LogSumParams(childStateLogProducts) :
                    SpecialFunctions.Sum(childStateLogProducts);

                if (!useLogTransform)
                {
                    logP = Math.Log(logP);
                }
                //Debug.Assert(discreteDistribution is DistributionDiscreteBinary || p > 0);
                //if (0 > p && p > -1E-10)    // may happen if a probability is close to 0 and there are rounding errors
                //{
                //    p = 0;
                //}
                return logP;
            }
            catch (Msr.Mlas.LinearAlgebra.NotComputableException)    // if for some reason the parameters lead to unstable matrix operation, abort.
            {
                //return double.NegativeInfinity;
                return double.NaN;
            }
        }

        private double[] ComputeLogLikelihoodModelGivenDataInternalDiscrete(BranchOrLeaf branchOrLeaf, MessageInitializer messageInitializer,
            OptimizationParameterList discreteParameters, bool useLogTransform) 
        {
            DistributionDiscrete discreteDistribution = (DistributionDiscrete)messageInitializer.PropogationDistribution; 

            int stateCount = discreteDistribution.NonMissingClassCount;
            double[] logP = new double[stateCount]; //C# init's to zero
            if (useLogTransform)
            {
                for (int i = 0; i < logP.Length; i++)
                {
                    logP[i] = double.NegativeInfinity;
                }
            }

            if (branchOrLeaf is Branch)
            {
                Branch branch = branchOrLeaf as Branch; 
                List<double[]> childDistList = new List<double[]>(); 
                foreach (BranchOrLeaf child in branch.BranchOrLeafCollection)
                {
                    double[] childDist = ComputeLogLikelihoodModelGivenDataInternalDiscrete(child, messageInitializer, discreteParameters, useLogTransform);
                    childDistList.Add(childDist);
                }

                double[][] logDist = discreteDistribution.CreateDistribution(branch, discreteParameters);
                if (useLogTransform)
                {
                    SpecialFunctions.ConvertToLog(logDist);
                }

                for (int iParentState = 0; iParentState < stateCount; ++iParentState)
                {
                    double[] childStateLogProducts = new double[logDist.Length];
                    for (int iChildState = 0; iChildState < stateCount; ++iChildState)
                    {
                        double product = logDist[iParentState][iChildState];
                        foreach (double[] childDist in childDistList)
                        {
                            if (useLogTransform)
                            {
                                product += childDist[iChildState];
                            }
                            else
                            {
                                product *= childDist[iChildState];
                            }
                        }
                        //logP[iParentState] += product;
                        //logP[iParentState] = SpecialFunctions.LogSum(logP[iParentState], product);
                        childStateLogProducts[iChildState] = product;

                    }
                    logP[iParentState] = useLogTransform ?
                            SpecialFunctions.LogSumParams(childStateLogProducts) :
                            SpecialFunctions.Sum(childStateLogProducts);
                    //logP[iParentState] = SpecialFunctions.LogSumParams(childStateLogProducts);
                }
            } 
            else
            {
                MessageDiscrete discreteMessage = (MessageDiscrete)messageInitializer.InitializeMessage((Leaf)branchOrLeaf, discreteParameters);
                logP = discreteMessage.P;
                if (useLogTransform)
                {
                    SpecialFunctions.ConvertToLog(logP);
                }
                //#if DEBUG
                //                foreach(double d in p) 
                //                    Debug.Assert(!double.IsNaN(d) && d >= 0 && d <=1); 
                //#endif
            } 

            //Debug.Assert(!(p[0] == 0 && p[1] == 0));
            return logP;
        }

 
        public static void TestDiscrete(string p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, string p_8, KeepTest<Dictionary<string, string>> keepTest) 
        {
            throw new Exception("Old code"); 
        }

        public static void TestDiscrete(string p, int p_2, int p_3, int p_4, int p_5, int p_6, int p_7, string leafDistributionName, KeepTest<Dictionary<string, string>> keepTest, string treefileName, string binarySeqFileName, string hlaFileName, string leafNameToPidOrNullFileName, string p_14, string p_15)
        {
            throw new Exception("Old code");
        } 
 
        public void ExtractNames(string outputNameListName)
        { 

            using (TextWriter textWriter = File.CreateText(outputNameListName))
            {
                textWriter.WriteLine(Header);
                foreach (Leaf leaf in LeafCollection)
                { 
                    textWriter.WriteLine(SpecialFunctions.CreateTabString(leaf.CaseName, leaf.CaseName)); 
                }
            } 
        }

        static string OldLeafName = "oldLeafName";
        static string NewLeafName = "newLeafName";
        static string Header = SpecialFunctions.CreateTabString(OldLeafName, NewLeafName);
 
        public void ReplaceNames(string inputNameListName) 
        {
            Dictionary<string, string> oldLeafNameToNew = ReadOldLeafNameToNew(inputNameListName); 

            CheckTheInputNameList(oldLeafNameToNew);

            foreach (Leaf leaf in LeafCollection)
            {
                leaf.CaseName = oldLeafNameToNew[leaf.CaseName]; 
            } 

        } 

        private void CheckTheInputNameList(Dictionary<string, string> oldLeafNameToNew)
        {
            //check that every leaf is mentioned
            //check that every thing mentioned is a leaf
            //check that the resulting names are still a set (no dups) 
            Set<string> currentLeafNameSet = LeafNameSet(); 
            Set<string> oldLeafNameSet = Set<string>.GetInstance(oldLeafNameToNew.Keys);
            Set<string> inCurrentButNotOld = currentLeafNameSet.Subtract(oldLeafNameSet); 
            SpecialFunctions.CheckCondition(inCurrentButNotOld.Count == 0, string.Format("Some leaf names are missing from the input name list. {0}", inCurrentButNotOld));

            Set<string> inOldButNotCurrent = oldLeafNameSet.Subtract(currentLeafNameSet);
            SpecialFunctions.CheckCondition(inOldButNotCurrent.Count == 0, string.Format("The input name list has names that are not in the tree. {0}", inOldButNotCurrent));

            Set<string> newLeafNameSet = Set<string>.GetInstance(oldLeafNameToNew.Values); 
            SpecialFunctions.CheckCondition(newLeafNameSet.Count == oldLeafNameSet.Count, "Some new names in the input list are repeated."); 
        }
 
        private static Dictionary<string, string> ReadOldLeafNameToNew(string inputNameListName)
        {
            Dictionary<string, string> oldLeafNameToNew = new Dictionary<string, string>();
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(inputNameListName, Header, false))
            {
                string oldLeafName = row[OldLeafName]; 
                string newLeafName = row[NewLeafName]; 
                oldLeafNameToNew.Add(oldLeafName, newLeafName);
            } 
            return oldLeafNameToNew;
        }

        public void ToTreeFile(string outputPhyloTreeName)
        {
            using (TextWriter textWriter = File.CreateText(outputPhyloTreeName)) 
            { 
                textWriter.WriteLine(ToTreeString());
            } 
        }
        public string ToTreeString()
        {
            return ToString(delegate(Leaf leaf) { return leaf.CaseName; });
        }
 
        public string ToString(Converter<Leaf, string> nameExtractor) 
        {
            StringBuilder stringBuilder = new StringBuilder(); 

            ToStringInternal(BranchOrLeaf, nameExtractor, ref stringBuilder);

            string result = stringBuilder.ToString();
            result = result.Substring(0, result.LastIndexOf(':'));
            result += ";"; 
 
            return result;
        } 
        private void ToStringInternal(BranchOrLeaf node, Converter<Leaf, string> nameExtractor, ref StringBuilder stringBuilder)
        {
            if (node is Leaf)
            {
                stringBuilder.Append(nameExtractor((Leaf)node) + ":" + node.Length);
            } 
            else 
            {
                stringBuilder.Append("("); 
                bool firstKid = true;
                foreach (BranchOrLeaf child in ((Branch)node).BranchOrLeafCollection)
                {
                    SpecialFunctions.CheckCondition(child != node);
                    if (!firstKid)
                    { 
                        stringBuilder.Append(","); 
                    }
                    ToStringInternal(child, nameExtractor, ref stringBuilder); 
                    firstKid = false;
                }
                stringBuilder.Append("):" + node.Length);
            }
        }
 
        static double AccessPValueFromPhylotreeRow(Dictionary<string, string> row) 
        {
            try 
            {
                double pValue = double.Parse(row["PValue"]);
                return pValue;
            }
            catch (KeyNotFoundException)
            { 
                throw new Exception(@"The header must contain ""PValue"""); 
            }
        } 


    }


} 
 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
