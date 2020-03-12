//using System; 
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using System.Diagnostics;
//using Msr.Adapt.LearningWorkbench;
//using Msr.Adapt.HighLevelFeatures; 
//using Msr.Mlas.SpecialFunctions; 

//namespace VirusCount 
//{
//    public class EpitopeFeatureGenerator
//    {
//        private EpitopeFeatureGenerator()
//        {
//        } 
 
//        List<IHashableFeature> BeaCollection;
//        //Dictionary<string, List<string>> AaToPropList; 
//        //Study Study;
//        FeatureSerializer FeatureSerializer;
//        //OldThreeD.OldBindingCollection BindingCollection;

//        public Set<IHashableFeature> GenerateFeatureSet(object entity)
//        { 
//            Debug.Fail("Don't use this featurizer any more. Instead use SetupTrain.CreateFeaturizer"); 
//            MerAndHlaToLength merAndHlaToLength = (MerAndHlaToLength) entity;
//            //Debug.Assert(BindingCollection != null); 
//            Set<IHashableFeature> featureSet = Set<IHashableFeature>.GetInstance();

//            //featureSet.AddNew(Logistic.AlwaysTrue);

//            IHashableFeature featureHla = new IsHla(merAndHlaToLength.HlaToLength);
//            IHashableFeature featureZero6Supertype = new IsZero6Supertype(merAndHlaToLength.HlaToLength); 
 
//            if (Contains(GeneratorType.Hla))
//            { 
//                featureSet.AddNew(featureHla);
//            }

//            AddNeededAnyBindingFeatures(merAndHlaToLength, featureSet);

//            AddAnyNeededHlaThreadingFeatures(merAndHlaToLength, featureSet); 
 
//            if (Contains(GeneratorType.Zero6Supertype))
//            { 
//                featureSet.AddNew(featureZero6Supertype);
//            }
//            for (int stringIndex = 0; stringIndex < BeaCollection.Count; ++stringIndex)
//            {
//                IHashableFeature featureBea = BeaCollection[stringIndex];
//                char chAminoAcid = merAndHlaToLength.Mer[stringIndex]; 
//                string sAminoAcid = Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[chAminoAcid]; 
//                SpecialFunctions.CheckCondition(sAminoAcid != null);
 
//                IHashableFeature featureAA = new IsAA(sAminoAcid, featureBea);
//                if (Contains(GeneratorType.Position))
//                {
//                    featureSet.AddNew(featureAA);

//                    if (Contains(GeneratorType.AndHla)) 
//                    { 
//                        IHashableFeature featureHlaAndAA = new And(featureHla, featureAA);
//                        featureSet.AddNew(featureHlaAndAA); 
//                    }

//                    if (Contains(GeneratorType.AndZero6Supertype))
//                    {
//                        IHashableFeature featureZero6SupertypeAndAA = new And(featureZero6Supertype, featureAA);
//                        featureSet.AddNew(featureZero6SupertypeAndAA); 
//                    } 

//                } 

//                //if (Contains(GeneratorType.Distance))
//                //{
//                //    SpecialFunctions.CheckCondition(featureBea is E, "Don't have code for other cases");
//                //    int pos = (int)((E)featureBea).Pos;
//                //    foreach (string mhcIAminoAcid in BindingCollection.CloseMhcAminoAcids(merAndHlaToLength.HlaToLength, pos, sAminoAcid)) 
//                //    { 
//                //        IHashableFeature featureCloseAminoAcids = new CloseAminoAcids(sAminoAcid, mhcIAminoAcid);
//                //        featureSet.AddNewOrOld(featureCloseAminoAcids); 

//                //        if (Contains(GeneratorType.AndHla))
//                //        {
//                //            IHashableFeature featureHlaAndCloseAminoAcids = new And(featureHla, featureCloseAminoAcids);
//                //            featureSet.AddNewOrOld(featureHlaAndCloseAminoAcids);
//                //        } 
 
//                //        if (Contains(GeneratorType.AndSupertype))
//                //        { 
//                //            IHashableFeature featureSupertypeAndCloseAminoAcids = new And(featureSupertype, featureCloseAminoAcids);
//                //            featureSet.AddNewOrOld(featureSupertypeAndCloseAminoAcids);
//                //        }
//                //    }
//                //}
 
//                //if (Contains(GeneratorType.Distance2)) 
//                //{
//                //    SpecialFunctions.CheckCondition(featureBea is E, "Don't have code for other cases"); 
//                //    int pos = (int)((E)featureBea).Pos;
//                //    foreach (string mhcIAminoAcid in BindingCollection.CloseMhcAminoAcids(merAndHlaToLength.HlaToLength, pos, sAminoAcid))
//                //    {
//                //        IHashableFeature featureCloseAminoAcids2 = new CloseAminoAcids2(pos, sAminoAcid, mhcIAminoAcid);
//                //        featureSet.AddNewOrOld(featureCloseAminoAcids2);
//                //    } 
//                //} 

 

//                if (Contains(GeneratorType.Property))
//                {
//                    foreach (string property in KmerProperties.AaToPropList[sAminoAcid])
//                    {
//                        HasAAProp featureAAProp = new HasAAProp(property, featureBea); 
//                        featureSet.AddNew(featureAAProp); 

//                        if (Contains(GeneratorType.AndHla)) 
//                        {
//                            IHashableFeature featureHlaAndAAProb = new And(featureHla, featureAAProp);
//                            featureSet.AddNew(featureHlaAndAAProb);
//                        }
//                        if (Contains(GeneratorType.AndZero6Supertype))
//                        { 
//                            IHashableFeature featureSupertypeAndAAProb = new And(featureZero6Supertype, featureAAProp); 
//                            featureSet.AddNew(featureSupertypeAndAAProb);
//                        } 
//                    }
//                }


//            }
//            return featureSet; 
//        } 

//        private void AddAnyNeededHlaThreadingFeatures(MerAndHlaToLength merAndHlaToLength, Set<IHashableFeature> featureSet) 
//        {
//            if (Contains(GeneratorType.HlaThreadingAA) || Contains(GeneratorType.HlaThreadingChemicalProperties))
//            {
//                string hla = merAndHlaToLength.HlaToLength.ToString();
//                foreach (KeyValuePair<int, string> hlaPosBase1AndAA in H.PosAndAACollection(hla))
//                { 
//                    int hlaPosBase1 = hlaPosBase1AndAA.Key; 
//                    H aH = H.GetInstance(hlaPosBase1);
//                    string hlaAA = hlaPosBase1AndAA.Value; 
//                    IsAA hlaIsAA = new IsAA(hlaAA, aH);

//                    for (int merPosBase1 = 1; merPosBase1 <= merAndHlaToLength.Mer.Length; ++merPosBase1)
//                    {
//                        E aE = E.GetInstance(merPosBase1);
//                        char merAAChar = merAndHlaToLength.Mer[merPosBase1 - 1]; 
//                        string merAA = Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[merAAChar]; 
//                        IsAA merIsAA = new IsAA(merAA, aE);
 
//                        if (Contains(GeneratorType.HlaThreadingAA))
//                        {
//                            And anAndAA = new And(hlaIsAA, merIsAA);
//                            featureSet.AddNewOrOld(anAndAA);
//                        }
 
//                        if (Contains(GeneratorType.HlaThreadingChemicalProperties)) 
//                        {
//                            foreach (string hlaProperty in KmerProperties.AaToPropList[hlaAA]) 
//                            {
//                                HasAAProp hlaHasProp = new HasAAProp(hlaProperty, aH);

//                                foreach (string merProperty in KmerProperties.AaToPropList[merAA])
//                                {
//                                    HasAAProp merHasProp = new HasAAProp(merProperty, aE); 
//                                    And anAndProperty = new And(hlaHasProp, merHasProp); 
//                                    featureSet.AddNewOrOld(anAndProperty);
 
//                                }
//                            }
//                        }
//                    }
//                }
//            } 
//        } 

//        private void AddNeededAnyBindingFeatures(MerAndHlaToLength merAndHlaToLength, Set<IHashableFeature> featureSet) 
//        {
//            if ((GeneratorType.BindingX & GeneratorType) != GeneratorType.Empty)
//            {

//                double energy = BindingLessThan.GetBindingEnergy(GeneratorType.BindingX & GeneratorType, merAndHlaToLength);
//                //!!!move this to it's own code 
//                double middle = 6.27; 
//                for (int iRange = 9; iRange >= 0; --iRange)
//                { 
//                    double energyCutoff = (iRange - 5) * 2 + middle;
//                    if (energy >= energyCutoff)
//                    {
//                        break;
//                    }
//                    switch (GeneratorType.BindingX & GeneratorType) 
//                    { 
//                        case GeneratorType.Binding1:
//                            featureSet.AddNew(new BindingLessThan1(energyCutoff)); 
//                            break;
//                        case GeneratorType.Binding2:
//                            featureSet.AddNew(new BindingLessThan2(energyCutoff));
//                            break;
//                        case GeneratorType.Binding3:
//                            featureSet.AddNew(new BindingLessThan3(energyCutoff)); 
//                            break; 
//                        case GeneratorType.Binding4:
//                            featureSet.AddNew(new BindingLessThan4(energyCutoff)); 
//                            break;
//                        case GeneratorType.Binding5:
//                            featureSet.AddNew(new BindingLessThan5(energyCutoff));
//                            break;
//                        default:
//                            SpecialFunctions.CheckCondition(false); 
//                            break; 
//                    }
 
//                }
//            }
//        }


//        private bool Contains(GeneratorType element) 
//        { 
//            return (GeneratorType & element) != GeneratorType.Empty;
//        } 



//        private static List<IHashableFeature> CreateBeaCollection(KmerDefinition kmerDefinition)
//        {
//            List<IHashableFeature> beaCollection = new List<IHashableFeature>(); 
//            foreach (IHashableFeature bea in AminoAcidCollection(kmerDefinition)) 
//            {
//                beaCollection.Add(bea); 
//            }
//            return beaCollection;
//        }

//        static public IEnumerable<EntityFeature> AminoAcidCollection(KmerDefinition kmerDefinition)
//        { 
//            foreach (EntityFeature aminoAcid in BCollection(kmerDefinition)) 
//            {
//                yield return aminoAcid; 
//            }
//            foreach (EntityFeature aminoAcid in ECollection(kmerDefinition))
//            {
//                yield return aminoAcid;
//            }
//            foreach (EntityFeature aminoAcid in ACollection(kmerDefinition)) 
//            { 
//                yield return aminoAcid;
//            } 
//        }

//        static private IEnumerable<EntityFeature> BCollection(KmerDefinition kmerDefinition)
//        {
//            for (int i = -kmerDefinition.BeforeMerCount; i <= -1; ++i)
//            { 
//                yield return B.GetInstance(i); 
//            }
//        } 

//        static private IEnumerable<EntityFeature> ECollection(KmerDefinition kmerDefinition)
//        {
//            for (int i = 1; i <= kmerDefinition.EpitopeMerCount; ++i)
//            {
//                yield return E.GetInstance(i); 
//            } 
//        }
 
//        static private IEnumerable<EntityFeature> ACollection(KmerDefinition kmerDefinition)
//        {
//            for (int i = 1; i <= kmerDefinition.AfterMerCount; ++i)
//            {
//                yield return A.GetInstance(i);
//            } 
//        } 

 



//        private GeneratorType GeneratorType;
//        private KmerDefinition KmerDefinition;
 
//        public string Name 
//        {
//            get 
//            {
//                return string.Format("{0}{1}", KmerDefinition, GeneratorType.ToString("g"));
//            }
//        }

//        public static EpitopeFeatureGenerator GetInstance(KmerDefinition kmerDefinition, GeneratorType generatorType, FeatureSerializer featureSerializer) 
//        { 
//            EpitopeFeatureGenerator aEpitopeFeatureGenerator = new EpitopeFeatureGenerator();
//            //aEpitopeFeatureGenerator.Study = study; 
//            aEpitopeFeatureGenerator.BeaCollection = CreateBeaCollection(kmerDefinition);
//            //aEpitopeFeatureGenerator.AaToPropList = KmerProperties.CreateAAToPropList();
//            aEpitopeFeatureGenerator.GeneratorType = generatorType;
//            aEpitopeFeatureGenerator.KmerDefinition = kmerDefinition;
//            aEpitopeFeatureGenerator.FeatureSerializer = featureSerializer;
//            //aEpitopeFeatureGenerator.BindingCollection = bindingCollection; 
//            return aEpitopeFeatureGenerator; 
//        }
 

//        public Dictionary<IHashableFeature, int> GenerateFeaturesAndMebFile(EpitopeLearningData posAndNeg, string mebFile, string featureFile)
//        {
//            Set<IHashableFeature> featuresToKillOrNull =
//                (Contains(GeneratorType.KillRedundantFeatures)) ? FindFeaturesToKill(posAndNeg) : null;
 
//            Dictionary<IHashableFeature, int> featureTable = new Dictionary<IHashableFeature, int>(); 
//            Dictionary<IHashableFeature, int> featureToCount = new Dictionary<IHashableFeature, int>();
 
//            string countFile = mebFile.Replace("meb.", "count.");
//            using (TextWriter
//                    streamWriterMeb = File.CreateText(mebFile),
//                    streamWriterFeature = File.CreateText(featureFile),
//                    streamWriterCount = File.CreateText(countFile))
//            { 
//                int iExample = 0; 
//                foreach (MerAndHlaToLength merAndHlaToLength in posAndNeg.Keys)
//                { 
//                    ++iExample;
//                    if (iExample % 1000 == 0)
//                    {
//                        Debug.WriteLine(iExample);
//                    }
 
//                    //Target 
//                    streamWriterMeb.Write(posAndNeg[merAndHlaToLength] ? "1" : "0");
 
//                    Set<IHashableFeature> featureSet = GenerateFeatureSet(merAndHlaToLength);
//                    foreach (IHashableFeature feature in featureSet)
//                    {
//                        if (featuresToKillOrNull != null && featuresToKillOrNull.Contains(feature))
//                        {
//                            continue; 
//                        } 

//                        streamWriterMeb.Write(" {0}", GetFeatureId(feature, ref featureTable, streamWriterFeature)); 
//                        featureToCount[feature] = 1 + SpecialFunctions.GetValueOrDefault(featureToCount, feature);
//                    }
//                    streamWriterMeb.WriteLine("");
//                }

 
//                streamWriterCount.WriteLine(SpecialFunctions.CreateTabString("Feature", "Count")); 
//                foreach (IHashableFeature feature in featureToCount.Keys)
//                { 
//                    streamWriterCount.WriteLine(SpecialFunctions.CreateTabString(feature, featureToCount[feature]));
//                }
//            }

//            return featureTable;
//        } 
 
//        public Set<IHashableFeature> FindFeaturesToKill(EpitopeLearningData posAndNeg)
//        { 
//            //SpecialFunctions.CheckCondition(DateTime.Now - new DateTime(2006, 3, 1, 19, 15, 0) < new TimeSpan(6, 0, 0), "Check that still want to cutoff so only top 10,000 features");
//            //int maxFeatures = 10000;

//            Dictionary<IHashableFeature, int> featureToCount = CreateFeatureToCount(posAndNeg);
//            Dictionary<int, Set<IHashableFeature>> countToFeature = CreateCountToFeature(featureToCount);
//            Set<IHashableFeature> featuresToKill = CreateFeaturesToKillList(countToFeature); 
 
////            RemoveFeaturesUntilGoal(posAndNeg.Count, maxFeatures, featureToCount.Count, countToFeature, ref featuresToKill);
 
//            Debug.WriteLine(featuresToKill.Count.ToString() + " features to be killed");
//            return featuresToKill;
//        }

//        private static void RemoveFeaturesUntilGoal(int posAndNegCount, int maxFeatures, int featureCount, Dictionary<int, Dictionary<IHashableFeature, bool>> countToFeature, ref Dictionary<IHashableFeature, bool> featuresToKill)
//        { 
//            if (featureCount - featuresToKill.Count <= maxFeatures) 
//            {
//                return; 
//            }
//            for (int countToKill = 1; ; ++countToKill)
//            {
//                SpecialFunctions.CheckCondition(countToKill <= posAndNegCount);
//                if (countToFeature.ContainsKey(countToKill))
//                { 
//                    foreach (IHashableFeature feature in countToFeature[countToKill].Keys) 
//                    {
//                        featuresToKill[feature] = true; //OK to add twice, once in CreateFeaturesToKillList and once here 
//                        if (featureCount - featuresToKill.Count <= maxFeatures)
//                        {
//                            return;
//                        }
//                    }
//                } 
//            } 
//        }
 
//        private Dictionary<IHashableFeature, int> CreateFeatureToCount(EpitopeLearningData posAndNeg)
//        {
//            int iExample = 0;
//            Dictionary<IHashableFeature, int> featureToCount = new Dictionary<IHashableFeature, int>();
//            foreach (MerAndHlaToLength merAndHlaToLength in posAndNeg.Keys)
//            { 
//                ++iExample; 
//                if (iExample % 1000 == 0)
//                { 
//                    Debug.WriteLine("looking to kill features " + iExample.ToString());
//                }


//                Set<IHashableFeature> featureSet = GenerateFeatureSet(merAndHlaToLength);
//                foreach (IHashableFeature feature in featureSet) 
//                { 
//                    featureToCount[feature] = 1 + SpecialFunctions.GetValueOrDefault(featureToCount, feature);
//                } 
//            }
//            return featureToCount;
//        }

//        private static Set<IHashableFeature> CreateFeaturesToKillList(Dictionary<int, Set<IHashableFeature>> countToFeature)
//        { 
//            Set<IHashableFeature> featuresToKill = Set<IHashableFeature>.GetInstance(); 
//            foreach (int count in countToFeature.Keys)
//            { 
//                Set<IHashableFeature> featureSet = countToFeature[count];
//                foreach (IHashableFeature feature in featureSet)
//                {
//                    if (feature is And)
//                    {
//                        And anAnd = (And)feature; 
//                        SpecialFunctions.CheckCondition(anAnd.FeatureCollection.Length == 2); 
//                        if (featureSet.Contains((IHashableFeature)anAnd.FeatureCollection[0])
//                        || featureSet.Contains((IHashableFeature)anAnd.FeatureCollection[1])) 
//                        {
//                            featuresToKill.AddNew(feature);
//                        }
//                    }
//                }
//            } 
//            return featuresToKill; 
//        }
 
//        private static Dictionary<int, Set<IHashableFeature>> CreateCountToFeature(Dictionary<IHashableFeature, int> featureToCount)
//        {
//            Dictionary<int, Set<IHashableFeature>> countToFeature = new Dictionary<int, Set<IHashableFeature>>();
//            foreach (IHashableFeature feature in featureToCount.Keys)
//            {
//                int count = featureToCount[feature]; 
//                Set<IHashableFeature> featureList = SpecialFunctions.GetValueOrDefault(countToFeature, count); 
//                featureList.AddNew(feature);
//            } 
//            return countToFeature;
//        }

//        private int GetFeatureId(IHashableFeature feature, ref Dictionary<IHashableFeature, int> featureTable, TextWriter streamWriterFeature)
//        {
//            if (featureTable.ContainsKey(feature)) 
//            { 
//                return featureTable[feature];
//            } 
//            int id = featureTable.Count;
//            featureTable.Add(feature, id);
//            string sFeatureXml = FeatureSerializer.ToXml(feature);
//            streamWriterFeature.WriteLine("{0}\t{1}", id, sFeatureXml);
//            return id;
//        } 
 
//    }
 	 
//    //!!!should use subclassing ???
//    [Flags]
//    public enum GeneratorType
//    {
//        Empty = 0,
//        Position = 1, 
//        Hla = 2, 
//        //Distance = 4,
//        AndHla = 8, 
//        //Distance2 = 16,
//        //Supertype = 32,
//        //AndSupertype = 64,
//        Property = 128,
//        Binding1 = 256,
//        Binding2 = 512, 
//        Binding3 = 1024, 
//        Binding4 = 2048,
//        Binding5 = 2048 + 2048, 
//        BindingX = GeneratorType.Binding1 | GeneratorType.Binding2 | GeneratorType.Binding3 | GeneratorType.Binding4 | GeneratorType.Binding5,
//        HlaThreadingAA = Binding5 + Binding5,
//        HlaThreadingChemicalProperties = HlaThreadingAA + HlaThreadingAA,
//        KillRedundantFeatures = HlaThreadingChemicalProperties + HlaThreadingChemicalProperties,
//        Zero6Supertype = KillRedundantFeatures + KillRedundantFeatures,
//        AndZero6Supertype = Zero6Supertype + Zero6Supertype, 
//        ComboAndZero6SuperType = GeneratorType.Hla | GeneratorType.Position | GeneratorType.Property | GeneratorType.AndHla | GeneratorType.Zero6Supertype | GeneratorType.AndZero6Supertype, 
//    }
 
//}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
