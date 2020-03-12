using System; 
using System.Diagnostics;
using Msr.Adapt.LearningWorkbench;
using Msr.Adapt.HighLevelFeatures;
using System.Collections;
using System.Reflection;
using System.Xml; 
using System.Xml.Serialization; 
using System.IO;
 
//!!!move this file into one of the main feature projects

namespace Msr.Adapt.HighLevelFeatures
 	
{
 
	/// <summary> 
	/// A feature collection,
	///    list the xml name of all features 
	///    give each feature object, which can then be used to evaluate an entity.
	///    Given an entity, tell EVERY feature that is true (or nonzero)
 	/// </summary>
 	public abstract class FeaturesFromEntity
	{
 		abstract public FeaturesFromEntity Clone(); 
		abstract public void Remove(string xmlKey); 
		abstract public void Add(string xmlKey, Feature feature);
		abstract public FeatureSerializer FeatureSerializer {get; set;} 
		abstract public IDictionary FeatureDictionary ();
		//!!!this is nice for boolean features, but for numeric wouldn't you want it to return not just the nonzero features, but also their value?
 		abstract public IDictionary FeatureDictionary (object entity);

 		//!!!might be better to have this function work on just one file at a time and just be called 3 times
 
		/// <summary> 
 		///
		/// </summary> 
		/// <param name="filenamesTrainTestOutputSuggest">The names of the three files (test, train, output). Give a name of "", to have a name generated. Give a name of NULL if no file is wanted.</param>
		/// <param name="FeaturesFromEntity">The set of features to apply to the entities.</param>
		/// <param name="trainingCollection">A collection of two collection: the positive entities and the entities examples</param>
		/// <param name="mapAttributeIdToFeature"></param>
 		/// <returns>The names of the three files. The name will be NULL if no file was wanted.</returns>
 		public string[] CreateDstFiles(string[] filenamesTrainTestOutputSuggest, IEnumerable[] trainingCollection, ref Hashtable mapFeatureXmlToAttributeId, out Hashtable mapAttributeIdToFeature) 
		{ 
 			Debug.Assert(trainingCollection.Length == 2); //!!!raise error: only works on two classes
			Debug.Assert(filenamesTrainTestOutputSuggest.Length == 3); //!!!raise error 

			mapAttributeIdToFeature = new Hashtable();

			bool bNewIds = (mapFeatureXmlToAttributeId == null);
			if (bNewIds)
			{ 
 				mapFeatureXmlToAttributeId = new Hashtable(); 
 			}
 

			//!!!need to do something to be sure this gets deleted
 			string[] filenamesTrainTestOutputActual = (string[])filenamesTrainTestOutputSuggest.Clone();

			for (long iFile = 0; iFile < filenamesTrainTestOutputActual.Length; ++iFile)
			{ 
				if (filenamesTrainTestOutputSuggest[iFile] == null) 
				{
					// no file name is wanted, so skip to next one 
 					continue;
 				}
				else if (filenamesTrainTestOutputSuggest[iFile] == "")
 				{
					filenamesTrainTestOutputActual[iFile] =  Path.GetTempFileName();
				} 
				else 
				{
					// a file name was given, so don't to generate it. 
 				}
 				FileStream aFileStream = null;
				StreamWriter aStreamWriter = null;

 				if (iFile == 0 || iFile == 1)
				{ 
 
					//!!!need using or try to be sure this gets closed?
					//!!!what if it's not there or can't be opened? 
					aFileStream = new FileStream(filenamesTrainTestOutputActual[iFile], FileMode.Create, FileAccess.Write);
					aStreamWriter = new StreamWriter(aFileStream);

 					aStreamWriter.WriteLine(@"I,4,""A Dst"",""Created by C# SVM Wrap""");
 					aStreamWriter.WriteLine(@"T,1,""Sparse 0"",0,0,""sparse.txt""");
					aStreamWriter.WriteLine(@"N,0,""0"""); 
 					aStreamWriter.WriteLine(@"N,1,""1"""); 
					aStreamWriter.WriteLine(@"T,101,""Class"",0,0,""Class""");
					aStreamWriter.WriteLine(@"N,0,""0"""); 
					aStreamWriter.WriteLine(@"N,1,""1""");

					aStreamWriter.WriteLine(@"A,1101,101,""Class"",""Class""");

					// Find the name of the features
 					IDictionary dictionaryFeature = FeatureDictionary(); 
 					long iACode = 1101; 
					foreach (DictionaryEntry aDictionaryEntry in dictionaryFeature)
 					{ 
						Feature aFeature = (Feature) aDictionaryEntry.Value;
						string sXml = (string) aDictionaryEntry.Key;
						string sSafeFeature = sXml.Replace(@"""",@"''"); //Make the string safe

						if (bNewIds)
						{ 
 							++iACode; 
 							mapFeatureXmlToAttributeId[sXml] = iACode;
						} 
 						else
						{
							iACode = (long) mapFeatureXmlToAttributeId[sXml];
						}

						aStreamWriter.WriteLine(@"A,{0},1,""{0}"",""{1}""", iACode, sSafeFeature); 
 
						if (iFile == 0)
 						{ 
 							mapAttributeIdToFeature.Add(iACode, aFeature);
						}
 					}
				}

				if (iFile == 0) 
				{ 

					// For every class, for every entity, find the values of all the features 
					long iCCode = 10000;
 					long iCCode2 = 101100;
 					long iEntity = -1;

					for(long iClass = 0; iClass < trainingCollection.Length; ++iClass)
 					{ 
						IEnumerable entityCollection = trainingCollection[iClass]; 
						foreach (object entity in entityCollection)
						{ 
							++iCCode;
							++iCCode2;
 							++iEntity;

 							aStreamWriter.WriteLine(@"C,""{0}"",""{1}""", iCCode, iCCode2);
							aStreamWriter.WriteLine(@"V,1101,{0}",iClass); 
 
 							IDictionary dictionaryFeature = FeatureDictionary(entity);
							foreach (DictionaryEntry aDictionaryEntry in dictionaryFeature) 
							{
								Feature aFeature = (Feature) aDictionaryEntry.Value;
								string sXml = (string) aDictionaryEntry.Key;
								long iA = (long) mapFeatureXmlToAttributeId[sXml];
 								aStreamWriter.WriteLine(@"V,{0},1",iA);
 							} 
						} 
 					}
 
				}

				if (iFile == 0 || iFile == 1)
				{
					aStreamWriter.Close();
					aFileStream.Close(); 
 				} 
 				else
				{ 
 					Debug.Assert(aStreamWriter == null); // real assert
					Debug.Assert(aFileStream == null); // real assert
				}
			}
			return filenamesTrainTestOutputActual;
		} 
 

 	} 


 	public class DenseFeaturesFromEntity : FeaturesFromEntity
	{
 		IDictionary _rgFeatureDictionary;
		private FeatureSerializer _featureSerializer; 
 
		public override FeatureSerializer FeatureSerializer
		{ 
			get
			{
 				return _featureSerializer;
 			}
			set
 			{ 
				_featureSerializer = value; 
			}
		} 

		public DenseFeaturesFromEntity(IDictionary featureDictionary, FeatureSerializer featureSerializer)
		{
 			_rgFeatureDictionary = featureDictionary; //!!!should we copy the dictionary so that outside changes in won't change us?
 			FeatureSerializer = featureSerializer;
		} 
 
 		public DenseFeaturesFromEntity(IEnumerable rgFeature, FeatureSerializer featureSerializer)
		{ 
			FeatureSerializer = featureSerializer;
			_rgFeatureDictionary = new Hashtable();
			foreach (Feature aFeature in rgFeature)
			{
 				string sXml = FeatureSerializer.ToXml(aFeature);
 				_rgFeatureDictionary.Add(sXml, aFeature); 
			} 
 		}
 

		public override FeaturesFromEntity Clone()
		{
			FeaturesFromEntity aFeaturesFromEntity = new DenseFeaturesFromEntity(new Hashtable(_rgFeatureDictionary),_featureSerializer);
			return aFeaturesFromEntity;
		} 
 		public override void Remove(string xmlKey) 
 		{
			_rgFeatureDictionary.Remove(xmlKey); 
 		}
		public override void Add(string xmlKey, Feature feature)
		{
			_rgFeatureDictionary.Add(xmlKey, feature);
		}
 
 
		public override IDictionary FeatureDictionary()
 		{ 
 			return _rgFeatureDictionary; //!!!do we need to copy so that the user of this can't change us?
		}

 		public override IDictionary FeatureDictionary (object entity)
		{
			Hashtable rgForEntity = new Hashtable(); 
			foreach (DictionaryEntry aDictionaryEntry in _rgFeatureDictionary) 
			{
				string sXml = (string) aDictionaryEntry.Key; //!!!what if dictionary is of wrong type 
 				Feature aFeature = (Feature) aDictionaryEntry.Value; //!!!what if dictionary is of wrong type
 				MethodInfo aMethodInfo = aFeature.MainMethod;
				if (aMethodInfo.ReturnType == typeof(bool))
 				{
					bool b = (bool) aFeature.Evaluate(entity);
					if (b) 
					{ 
						rgForEntity.Add(sXml, aFeature);
					} 
 				}
 					//				else if (aMethodInfo.ReturnType == typeof(double)) //!!!should this be generalized to types that can be converted to double??
					//				{
 					//					double r = aFeature.Evaluate(entity);
					//					if (r != 0)
					//					{ 
					//						rgForEntity.Add(sXml, aFeature); 
					//					}
					//} 
 				else
 				{
					Debug.Assert(false); //!!!unsupported type
 				}
			}
			return rgForEntity; 
		} 
	}
 
	[Serializable]
 	[XmlRoot(Namespace="", IsNullable=false)]
 	public class SparseFeaturesFromEntity : FeaturesFromEntity
	{
 		IDictionary _rgFeatureDictionary;
		Converter<object, IEnumerable>[] _rgFeaturesFromEntityFunction; 
		private FeatureSerializer _featureSerializer; 

		public override FeatureSerializer FeatureSerializer 
		{
			get
 			{
 				return _featureSerializer;
			}
 			set 
			{ 
				_featureSerializer = value;
			} 
		}

		public SparseFeaturesFromEntity(IDictionary featureDictionary, Converter<object, IEnumerable>[] featuresFromEntityFunctionCollection, FeatureSerializer featureSerializer)
 		{
 			_rgFeatureDictionary = featureDictionary; //!!!should we copy the dictionary so that outside changes in won't change us?
			_rgFeaturesFromEntityFunction = featuresFromEntityFunctionCollection; 
 			FeatureSerializer = featureSerializer; 
		}
 
		public override FeaturesFromEntity Clone()
		{
			Debug.Assert(false); //!!!not implemented
			FeaturesFromEntity aFeaturesFromEntity = null;
 			return aFeaturesFromEntity;
 		} 
		public override void Remove(string xmlKey) 
 		{
			Debug.Assert(false); //!!!not implemented 
		}
		public override void Add(string xmlKey, Feature feature)
		{
			Debug.Assert(false); //!!!not implemented
 		}
 
 
 		public override IDictionary FeatureDictionary()
		{ 
 			return _rgFeatureDictionary; //!!!do we need to copy so that the user of this can't change us?
		}

		public override IDictionary FeatureDictionary (object entity)
		{
			Hashtable rgForEntity = new Hashtable(); 
			foreach(Converter<object, IEnumerable> aFeaturesFromEntityFunction in _rgFeaturesFromEntityFunction) 
 			{
 				IEnumerable rgEntitySomeFeatures = aFeaturesFromEntityFunction(entity); 
				foreach (Feature aFeature in rgEntitySomeFeatures)
 				{
					//!!!if nested features, implemented a good HashCode and Equal, then wouldn't need to hash on Xml, instead could has on the feature itself - but this would make defining features more work.
					string sXml = _featureSerializer.ToXml(aFeature);
					if (_rgFeatureDictionary.Contains(sXml))
					{ 
						rgForEntity.Add(sXml, aFeature); 
 					}
 				 
				}
 			}
			return rgForEntity;
		}

	} 
 

	public class UnionFeaturesFromEntity : FeaturesFromEntity 
	{
 		IDictionary _rgFeatureDictionary;
 		FeaturesFromEntity[] _rgFeaturesFromEntity;
		private FeatureSerializer _featureSerializer;

 		public override FeatureSerializer FeatureSerializer 
		{ 
			get
			{ 
				return _featureSerializer;
			}
 			set
 			{
				_featureSerializer = value;
 			} 
		} 

 
		public UnionFeaturesFromEntity(FeaturesFromEntity[] FeaturesFromEntityCollection,	FeatureSerializer featureSerializer)
		{
			FeatureSerializer = featureSerializer;
			_rgFeaturesFromEntity = FeaturesFromEntityCollection;

 			//Create the feature collection 
 			_rgFeatureDictionary = new Hashtable(); 
			foreach (FeaturesFromEntity aFeaturesFromEntity in _rgFeaturesFromEntity)
 			{ 
				foreach (DictionaryEntry aDictonaryEntry in aFeaturesFromEntity.FeatureDictionary())
				{
					object entity = aDictonaryEntry.Value;
					string sXml = (string) aDictonaryEntry.Key;
					Debug.Assert(!_rgFeatureDictionary.Contains(sXml)); //!!!raise error: new of the feature collections generate the same feature
 					_rgFeatureDictionary.Add(sXml, entity); 
 				} 
			}
 		} 

		public override IDictionary FeatureDictionary()
		{
			return _rgFeatureDictionary; //!!!do we need to copy so that the user of this can't change us?
		}
 
		public override IDictionary FeatureDictionary (object entity) 
 		{
 			Hashtable rgForEntity = new Hashtable(); 

			foreach (FeaturesFromEntity aFeaturesFromEntity in _rgFeaturesFromEntity)
 			{
				IDictionary featureDictionary = aFeaturesFromEntity.FeatureDictionary(entity);
				foreach (DictionaryEntry aDictionaryEntry in featureDictionary)
				{ 
					Feature aFeature = (Feature) aDictionaryEntry.Value; 
					string sXml = (string) aDictionaryEntry.Key;
 					Debug.Assert(_rgFeatureDictionary.Contains(sXml)); //!!!real assert 
 					rgForEntity.Add(sXml, aFeature);
				}
 			}
			return rgForEntity;
		}
 
 
		public override FeaturesFromEntity Clone()
		{ 
			Debug.Assert(false); //!!!not implemented
 			FeaturesFromEntity aFeaturesFromEntity = null;
 			return aFeaturesFromEntity;
		}
 		public override void Remove(string xmlKey)
		{ 
			Debug.Assert(false); //!!!not implemented 
		}
		public override void Add(string xmlKey, Feature feature) 
		{
 			Debug.Assert(false); //!!!not implemented
 		}

	}
 
 
 	//!!!does this have to be "Wrapped"? couldn't other learners be there? How different from IAgentOfChange?
	public interface IWrappedLearner 
	{
		Logistic Train(FeaturesFromEntity featuresFromEntity, IEnumerable[] trainingCollection, double rParameter, string[] filenamesTrainTestOutputSuggest, ref Hashtable MapFeatureXmlToAttributeId);
	}


} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
