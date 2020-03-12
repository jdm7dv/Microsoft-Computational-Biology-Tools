using System;
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using System.IO;
using EpipredLib;
using VirusCount.Qmrr;
//using ExamplesWithFlanking;
using System.Xml.Serialization;
using System.Reflection;
using Msr.Adapt.HighLevelFeatures;
using NecAndHlaFeatures;
using Msr.Adapt.LearningWorkbench;
using VirusCount;

namespace ProcessingPrediction
{
    public static class FeatureLib
    {
        public static FeatureSerializer FeatureSerializer = GetFeatureSerializer();
        private static FeatureSerializer GetFeatureSerializer()
        {
            FeatureSerializer featureSerializer;

            // The static CreateTypeCollection function takes a collection of .Net assemblies and a collection of types
            // and creates a collection of contained types that inherit from class Feature.
            //!!!why is this Type[] and not Feature[] ???

            //Type sampleType = typeof(NecAndHlaFeatures.IsAA);
            //List<Type> typeCollection = new List<Type>();
            //foreach(Type type in Assembly.GetAssembly(sampleType).GetTypes())
            //{
            //    if (type.IsSubclassOf(typeof(Feature)) && type.Namespace == sampleType.Namespace)
            //    {
            //        typeCollection.Add(type);
            //    }
            //}
            //typeCollection.Add(typeof(WeightIf));
            //typeCollection.Add(typeof(And));


            Type[] rgFeatureVocabulary = FeatureSerializer.CreateTypeCollection(
                new string[] { Assembly.GetAssembly(typeof(NecAndHlaFeatures.IsAA)).GetName().Name,
                               //Assembly.GetAssembly(typeof(ProcessingPrediction.NFlank)).GetName().Name,
                               Assembly.GetAssembly(typeof(Logistic)).GetName().Name },
                new Type[] { }
                );
            // The FeatureSerializer is constructed with this vocabulary.
            // Test this if there are problems
            FeatureSerializer.WillSerialize(rgFeatureVocabulary, true);

            featureSerializer = new FeatureSerializer(rgFeatureVocabulary);

            return featureSerializer;
        }
        //!!!switch to using a class
        public static Converter<object, Set<IHashableFeature>> CreateFeaturizer(string featurerizerName)
        {
            string eacpSTGiven = "+ea+cpST@";

            //!!! make so doesn't care about case

            int flankSize;
            string given;

            if (ExtractFlankSize(out flankSize, out given, featurerizerName, eacpSTGiven))
            {
                return delegate(object entity)
                {
                    return GenerateFeatureSet(entity, given, flankSize, true, true, true, true);
                };
            }

            else
            {
                SpecialFunctions.CheckCondition(false, "Don't know featurizer name " + featurerizerName);
                return null;
            }
        }



        private static bool ExtractFlankSize(out int flankSize, out string given, string featurerizerName, params string[] mainNameCollection)
        {
            //!!!should use regex
            int posGiven = featurerizerName.IndexOf('@');
            if (posGiven < 0)
            {
                given = null;
                flankSize = int.MinValue;
                return false;
            }

            string prefix = featurerizerName.Substring(0, posGiven + 1);
            int posDot = featurerizerName.IndexOf('.');
            SpecialFunctions.CheckCondition(posDot >= 0 && posDot > posGiven, "'Given' features must have an explicit '.FlankSize'");

            given = featurerizerName.Substring(posGiven + 1, posDot - posGiven - 1);

            string flankSizeAsString = featurerizerName.Substring(posDot + 1);
            flankSize = int.Parse(flankSizeAsString);

            foreach (string mainName in mainNameCollection)
            {
                if (prefix.Equals(mainName))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ExtractFlankSize(out int flankSize, string featurerizerName, params string[] mainNameCollection)
        {
            //!!!should use Regex
            foreach (string mainName in mainNameCollection)
            {
                if (featurerizerName.Equals(mainName))
                {
                    flankSize = 0;
                    return true;
                }
                if (featurerizerName.StartsWith(mainName))
                {
                    string rest = featurerizerName.Substring(mainName.Length);
                    if (!rest.StartsWith("."))
                    {
                        continue;
                    }
                    SpecialFunctions.CheckCondition(rest.Length >= 2, "Expect '.NUMBER' after main name of featurizer");
                    flankSize = int.Parse(rest.Substring(1));
                    return true;
                }
            }
            flankSize = int.MinValue;
            return false;
        }

        //static private Dictionary<Pair<NEC, Hla>, bool> CloseHuman = null;

        //GeneratorType.Hla | GeneratorType.Position | GeneratorType.Property | GeneratorType.AndHla | GeneratorType.Zero6Supertype | GeneratorType.AndZero6Supertype
        private static Set<IHashableFeature> GenerateFeatureSet(
            object entity, string supertypeTableSource,
            int? flankSizeOrNull,
            bool includeFlankNECFeatures,
            bool includeChemicalProperties, bool includeAAFeatures,
            bool addEiFeatures
            )
        {
            bool includeAndHlaAndSTWithEpitopeAdjFeatures = false;
            bool subtractSupertypeFeatures = false;
            bool subtractHlaFeatures = false;
            bool substractChemAACrissCrossFeatures = false;


            SpecialFunctions.CheckCondition(!includeAndHlaAndSTWithEpitopeAdjFeatures || includeFlankNECFeatures);

            Pair<NEC, Hla> necAndHlaX = (Pair<NEC, Hla>)entity;
            NEC nec = (null == flankSizeOrNull) ? necAndHlaX.First : NEC.GetInstance(necAndHlaX.First, (int)flankSizeOrNull);
            Hla hla = necAndHlaX.Second;
            Debug.Assert(nec.N.Length == nec.C.Length); // real assert
            Pair<NEC, Hla> necAndHla = new Pair<NEC, Hla>(nec, hla);

            Set<IHashableFeature> hlaishFeatureSet = new Set<IHashableFeature>();
            CreateAndAddHlaFeature(subtractHlaFeatures, hla, necAndHla, ref hlaishFeatureSet);
            CreateAndAddFeatureSupertype(supertypeTableSource, subtractSupertypeFeatures, hla, necAndHla, ref hlaishFeatureSet, Assembly.GetExecutingAssembly(), Predictor.ResourceString);

            Set<IHashableFeature> featureSet = Set<IHashableFeature>.GetInstance(hlaishFeatureSet);

            if (addEiFeatures)
            {
                AddEiFeatures(includeChemicalProperties, includeAAFeatures, substractChemAACrissCrossFeatures, nec, necAndHla, hlaishFeatureSet, featureSet);
            }



            if (includeFlankNECFeatures)
            {
                List<IHashableFeature> aaInNFlankFeatureList = new List<IHashableFeature>(In.GetAASeqInRegionInstance(1, necAndHla, NFlank.GetInstance()));
                DebugCheckThatEvaluatesToTrue(necAndHla, aaInNFlankFeatureList);



                if (includeAAFeatures)
                {
                    featureSet.AddNewRange(aaInNFlankFeatureList); //AA in N flank
                    featureSet.AddNewRange(In.GetAASeqInRegionInstance(2, necAndHla, NFlank.GetInstance())); //AA1-AA2 in Nflank
                    featureSet.AddNewRange(SubSeq.GetInSubSeqEnumeration(NFlank.GetInstance(), false, 1, necAndHla)); //AA@x in N flank (numbering is 5 4 3 2 1)
                    featureSet.AddNewRange(SubSeq.GetInSubSeqEnumeration(NFlank.GetInstance(), false, 2, necAndHla)); //AA1-AA2@x in Nflank (x is position of AA2, i.e., the smaller number)


                    featureSet.AddNewRange(In.GetAASeqInRegionInstance(1, necAndHla, CFlank.GetInstance())); //AA in Cflank
                    featureSet.AddNewRange(In.GetAASeqInRegionInstance(2, necAndHla, CFlank.GetInstance()));  //AA1-AA2 in Cflank
                    featureSet.AddNewRange(SubSeq.GetInSubSeqEnumeration(CFlank.GetInstance(), true, 1, necAndHla)); //AA@x in C flank (numbering is 1 2 3 4 5)
                    featureSet.AddNewRange(SubSeq.GetInSubSeqEnumeration(CFlank.GetInstance(), true, 2, necAndHla)); //AA1-AA2@x in Cflank (x is position of AA1, i.e., the smaller number)
                }

                if (includeChemicalProperties)
                {
                    featureSet.AddNewOrOldRange(InProperty.GetPropertySeqInRegionInstance(1, necAndHla, NFlank.GetInstance()));
                    featureSet.AddNewOrOldRange(SubSeq.GetInPropertySubSeqEnumeration(NFlank.GetInstance(), false, 1, necAndHla));
                    featureSet.AddNewOrOldRange(InProperty.GetPropertySeqInRegionInstance(1, necAndHla, CFlank.GetInstance()));
                    featureSet.AddNewOrOldRange(SubSeq.GetInPropertySubSeqEnumeration(CFlank.GetInstance(), true, 1, necAndHla));
                    featureSet.AddNewOrOldRange(InProperty.GetPropertySeqInRegionInstance(2, necAndHla, NFlank.GetInstance()));
                    featureSet.AddNewOrOldRange(SubSeq.GetInPropertySubSeqEnumeration(NFlank.GetInstance(), false, 2, necAndHla));
                    featureSet.AddNewOrOldRange(InProperty.GetPropertySeqInRegionInstance(2, necAndHla, CFlank.GetInstance()));
                    featureSet.AddNewOrOldRange(SubSeq.GetInPropertySubSeqEnumeration(CFlank.GetInstance(), true, 2, necAndHla));
                }
            }
            if (includeFlankNECFeatures)
            {
                if (includeAAFeatures)
                {
                    //EV in Epitope
                    AddFeatureWithOptionalAndHlaAndST(In.GetAASeqInRegionInstance(2, necAndHla, Epitope.GetInstance()), includeAndHlaAndSTWithEpitopeAdjFeatures, hlaishFeatureSet, false, ref featureSet);//AA1-AA2 in Epitope

                    //RR in Epitope[@1-2]
                    AddFeatureWithOptionalAndHlaAndST(SubSeq.GetInSubSeqEnumeration(Epitope.GetInstance(), true, 2, necAndHla), includeAndHlaAndSTWithEpitopeAdjFeatures, hlaishFeatureSet, false, ref featureSet);//AA1-AA2@x in Epitope (x is position of AA1, i.e., the smaller number)
                }
                if (includeChemicalProperties)
                {
                    //polar,cyclic in Epitope
                    AddFeatureWithOptionalAndHlaAndST(InProperty.GetPropertySeqInRegionInstance(2, necAndHla, Epitope.GetInstance()), includeAndHlaAndSTWithEpitopeAdjFeatures, hlaishFeatureSet, false, ref featureSet);
                    //polar,large in Epitope[@8-9]
                    AddFeatureWithOptionalAndHlaAndST(SubSeq.GetInPropertySubSeqEnumeration(Epitope.GetInstance(), true, 2, necAndHla), includeAndHlaAndSTWithEpitopeAdjFeatures, hlaishFeatureSet, false, ref featureSet);
                }

                //AA1-AA2 in Nflank,Epitope, etc
                if (null != flankSizeOrNull && (int)flankSizeOrNull > 0)
                {
                    string epitope = (string)Epitope.GetInstance().Evaluate(entity);

                    SubSeq lastNAAFeature = SubSeq.GetInstance(1, 1, false, NFlank.GetInstance());
                    string lastNAA = (string)lastNAAFeature.Evaluate(entity);
                    In inLastNAA = In.GetInstance(lastNAA, lastNAAFeature);

                    SubSeq firstEAAFeature = SubSeq.GetInstance(1, 1, true, Epitope.GetInstance());
                    string firstEAA = (string)firstEAAFeature.Evaluate(entity);
                    Debug.Assert(firstEAA == epitope.Substring(0, 1));// real assert
                    In inFirstEAA = In.GetInstance(firstEAA, firstEAAFeature);

                    SubSeq lastEAAFeature = SubSeq.GetInstance(epitope.Length, epitope.Length, true, Epitope.GetInstance());
                    string lastEAA = (string)lastEAAFeature.Evaluate(entity);
                    In inLastEAA = In.GetInstance(lastEAA, lastEAAFeature);

                    SubSeq firstCAAFeature = SubSeq.GetInstance(1, 1, true, CFlank.GetInstance());
                    string firstCAA = (string)firstCAAFeature.Evaluate(entity);
                    In inFirstCAA = In.GetInstance(firstCAA, firstCAAFeature);

                    if (includeAAFeatures)
                    {
                        And andLastNNAAFirstEAA = And.GetInstance(inLastNAA, inFirstEAA);
                        AddFeatureWithOptionalAndHlaAndST(andLastNNAAFirstEAA, includeAndHlaAndSTWithEpitopeAdjFeatures, hlaishFeatureSet, /*checkThatNew*/ true, ref featureSet);

                        And andLastEAAFirstCAA = And.GetInstance(inLastEAA, inFirstCAA);
                        AddFeatureWithOptionalAndHlaAndST(andLastEAAFirstCAA, includeAndHlaAndSTWithEpitopeAdjFeatures, hlaishFeatureSet, /*checkThatNew*/ true, ref featureSet);
                    }

                    if (includeChemicalProperties)
                    {
                        foreach (string lastNProperty in KmerProperties.AaToPropList[Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[lastNAA[0]]])
                        {
                            InProperty inLastNProperty = InProperty.GetInstance(lastNProperty, lastNAAFeature);

                            foreach (string firstEProperty in KmerProperties.AaToPropList[Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[firstEAA[0]]])
                            {
                                InProperty inFirstEProperty = InProperty.GetInstance(firstEProperty, firstEAAFeature); //!!!get this out of the loop?
                                And andLastNPropertyFirstEProperty = And.GetInstance(inLastNProperty, inFirstEProperty);
                                AddFeatureWithOptionalAndHlaAndST(andLastNPropertyFirstEProperty, includeAndHlaAndSTWithEpitopeAdjFeatures, hlaishFeatureSet, /*checkThatNew*/ false, ref featureSet);
                                Debug.Assert((bool)andLastNPropertyFirstEProperty.Evaluate(necAndHla));
                            }
                        }
                        foreach (string lastEProperty in KmerProperties.AaToPropList[Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[lastEAA[0]]])
                        {
                            InProperty inlastEProperty = InProperty.GetInstance(lastEProperty, lastEAAFeature);

                            foreach (string firstCProperty in KmerProperties.AaToPropList[Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[firstCAA[0]]])
                            {
                                InProperty infirstCProperty = InProperty.GetInstance(firstCProperty, firstCAAFeature); //!!!get this out of the loop?
                                And andlastEPropertyfirstCProperty = And.GetInstance(inlastEProperty, infirstCProperty);
                                AddFeatureWithOptionalAndHlaAndST(andlastEPropertyfirstCProperty, includeAndHlaAndSTWithEpitopeAdjFeatures, hlaishFeatureSet, /*checkThatNew*/ false, ref featureSet);
                                Debug.Assert((bool)andlastEPropertyfirstCProperty.Evaluate(necAndHla));
                            }
                        }
                    }
                }
            }

            return featureSet;
        }

        private static IEnumerable<IHashableFeature> BegMiddleEndOfPeptide(IHashableFeature regionFeature)
        {
            yield return Begin.GetInstance(regionFeature);
            yield return Middle.GetInstance(regionFeature);
            yield return End.GetInstance(regionFeature);
        }

        private static void AddEiFeatures(bool includeChemicalProperties, bool includeAAFeatures, bool substractChemAACrissCrossFeatures, NEC nec, Pair<NEC, Hla> necAndHla, Set<IHashableFeature> hlaishFeatureSet, Set<IHashableFeature> featureSet)
        {

            for (int i = 0; i < nec.E.Length; ++i)
            {
                IHashableFeature featureE = E.GetInstance(i + 1);
                string aminoAcid = GetAminoAcidFromEpitopePosition(nec, i);

                if (includeAAFeatures)
                {
                    IsAA featureAA = IsAA.GetInstance(aminoAcid, featureE);
                    featureSet.AddNew(featureAA);
                    Debug.Assert((bool)featureAA.Evaluate(necAndHla)); // real assert - must only generate true features

                    foreach (IHashableFeature hlaishFeature in hlaishFeatureSet)
                    {
                        if (substractChemAACrissCrossFeatures && hlaishFeature is HasAAProp)
                        {
                            continue;
                        }

                        And featureHlaishAndAA = And.GetInstance(hlaishFeature, featureAA);
                        featureSet.AddNew(featureHlaishAndAA);
                        Debug.Assert((bool)featureHlaishAndAA.Evaluate(necAndHla)); // real assert - must only generate true features
                    }
                }

                if (includeChemicalProperties)
                {
                    foreach (string property in VirusCount.KmerProperties.AaToPropList[aminoAcid])
                    {
                        HasAAProp featureAAProp = HasAAProp.GetInstance(property, featureE);
                        featureSet.AddNew(featureAAProp);
                        Debug.Assert((bool)featureAAProp.Evaluate(necAndHla)); // real assert - must only generate true features

                        foreach (IHashableFeature hlaishFeature in hlaishFeatureSet)
                        {
                            if (substractChemAACrissCrossFeatures && hlaishFeature is IsAA)
                            {
                                continue;
                            }

                            And featureHlaishAndAAProb = And.GetInstance(hlaishFeature, featureAAProp);
                            featureSet.AddNew(featureHlaishAndAAProb);
                            Debug.Assert((bool)featureHlaishAndAAProb.Evaluate(necAndHla)); // real assert - must only generate true features
                        }
                    }
                }
            }

            //All of the above with AA replaced by chemical property of AA
        }


        private static string GetAminoAcidFromEpitopePosition(NEC nec, int i)
        {
            char chAminoAcid = nec.E[i];
            string aminoAcid = VirusCount.Biology.GetInstance().OneLetterAminoAcidAbbrevTo3Letter[chAminoAcid];
            SpecialFunctions.CheckCondition(aminoAcid != null);
            return aminoAcid;
        }

        private static void CreateAndAddHlaFeature(bool subtractHlaFeatures, Hla hla, Pair<NEC, Hla> necAndHla, ref Set<IHashableFeature> hlaishEnumeration)
        {
            if (subtractHlaFeatures)
            {
                return;
            }

            IsHla featureHla = IsHla.GetInstance(hla);
            hlaishEnumeration.AddNew(featureHla);
            Debug.Assert((bool)featureHla.Evaluate(necAndHla)); // real assert - must only generate true features
        }

        private static void CreateAndAddFeatureSupertype(string supertypeTableSource, bool subtractSupertypeFeatures, Hla hla, Pair<NEC, Hla> necAndHla, ref Set<IHashableFeature> hlaishEnumeration, Assembly assembly, string resourcePrefix)
        {
            if (subtractSupertypeFeatures)
            {
                return;
            }

            IHashableFeature featureSupertype;
            switch (supertypeTableSource)
            {
                case SupertypeTableSource.None:
                    featureSupertype = null;
                    break;
                default:
                    featureSupertype = (IHashableFeature)IsSupertypeFromFile.GetInstance(hla, supertypeTableSource, assembly, resourcePrefix);
                    break;
            }
            hlaishEnumeration.AddNew(featureSupertype);
            Debug.Assert((bool)((Feature)featureSupertype).Evaluate(necAndHla)); // real assert - must only generate true features
        }

        private static void AddFeatureWithOptionalAndHlaAndST(IEnumerable<IHashableFeature> featureEnum, bool includeAndHlaAndST, IEnumerable<IHashableFeature> hlaishEnumeration, bool checkThatNew, ref Set<IHashableFeature> featureSet)
        {
            foreach (IHashableFeature feature in featureEnum)
            {
                AddFeatureWithOptionalAndHlaAndST(feature, includeAndHlaAndST, hlaishEnumeration, checkThatNew, ref featureSet);
            }
        }
        private static void AddFeatureWithOptionalAndHlaAndST(IHashableFeature feature, bool includeAndHlaAndST, IEnumerable<IHashableFeature> hlaishEnumeration, bool checkThatNew, ref Set<IHashableFeature> featureSet)
        {
            featureSet.Add(feature, checkThatNew);
            if (includeAndHlaAndST)
            {
                foreach (IHashableFeature hlaishFeature in hlaishEnumeration)
                {
                    And featureAndHlaish = And.GetInstance(hlaishFeature, feature);
                    featureSet.Add(featureAndHlaish, checkThatNew);
                }
            }
        }


        [Conditional("DEBUG")]
        private static void DebugCheckThatEvaluatesToTrue(Pair<NEC, Hla> necAndHla, List<IHashableFeature> aaInNFlankFeatureList)
        {
            foreach (In inFeature in aaInNFlankFeatureList)
            {
                Debug.Assert((bool)inFeature.Evaluate(necAndHla));  // real assert - must only generate true features
            }
        }

    }

    public static class SupertypeTableSource
    {
        //We put a ":" in these strings values becase file names can not contain ":"
        public const string None = ":None";
    }


}
