using System; 
using System.Collections;
using System.Diagnostics;
//using Msr.Adapt.Tabulate;
using System.Collections.Specialized;
using Msr.Adapt.LearningWorkbench;
 
namespace Msr.Adapt.HighLevelFeatures 
{
//	/// <summary> 
//	///
//	/// </summary>
//	public class LookForAllWords: IFeatureGenerator
//	{
//		Feature[][] _rgTextExtractorAndRunTimes;
//		int _iHowManyWordsToUse; 
//		FPCreateHasWord _fpCreateHasWord; 
//
// 
//		public LookForAllWords(FPCreateHasWord fpCreateHasWord, Feature[][] textExtractorAndRunTimeCollection) : this(int.MaxValue, fpCreateHasWord, textExtractorAndRunTimeCollection)
//		{
//		}
//
//		/// <summary>
//		/// The count for a word is the number of entitys in which it appears 
//		/// </summary> 
//		/// <param name="maxNumberOfFeatureToReturn"></param>
//		/// <param name="textExtractorCollection"></param> 
//		public LookForAllWords(int maxNumberOfFeatureToReturn, FPCreateHasWord fpCreateHasWord, Feature[][] textExtractorAndRunTimeCollection)
//		{
//			_rgTextExtractorAndRunTimes = textExtractorAndRunTimeCollection;
//			_fpCreateHasWord = fpCreateHasWord;
//			_iHowManyWordsToUse = maxNumberOfFeatureToReturn;
//			//TODO test that these return strings 
//		} 
//
//		public Feature[] Generate(IEnumerable[] trainCollection) 
//		{
//			Hashtable rgFeaturesToReturn = new Hashtable();
//
//			// For each email set
//			foreach(IEnumerable entityCollection in trainCollection)
//			{ 
//				// for each entity 
//				foreach(object entity in entityCollection)
//				{ 
//					Hashtable rgSeenInEntity = new Hashtable();
//					// for each text extractor
//					foreach(Feature[] textExtractorAndRunTime in _rgTextExtractorAndRunTimes)
//					{
//						//for every word
// 
//						StringCollection aStringCollection; 
//						Debug.Assert(textExtractorAndRunTime.Length == 2); //TODO raise error
//						Feature textExtractor = textExtractorAndRunTime[0]; 
//						Feature textRunTime = textExtractorAndRunTime[1];
//						object aFeatureValue = textExtractor.Evaluate(entity);
//						if (aFeatureValue is string)
//						{
//							//TODO should we move ExtractWords somewere more central? or require entity types to provide one?
//							aStringCollection = Msr.Adapt.ExMAPIMessage.NPMessage.ExtractWords((string) aFeatureValue); 
//						} 
//						else if (aFeatureValue is StringCollection)
//						{ 
//							aStringCollection = (StringCollection) aFeatureValue;
//						}
//						else
//						{
//							aStringCollection = new StringCollection();
//							Debug.Assert(false); //TODO raise error 
//						} 
//						
//						foreach(string sWord in aStringCollection) 
//						{												
//							// tally every RunTime/word
//							KeyPair aKeyPair = new KeyPair(textRunTime,sWord);
//
//							if (!rgSeenInEntity.ContainsKey(aKeyPair))
//							{ 
//								rgSeenInEntity.Add(aKeyPair,null); 
//
//								if (!rgFeaturesToReturn.ContainsKey(aKeyPair)) 
//								{
//									rgFeaturesToReturn.Add(aKeyPair,1);
//								}
//								else
//								{
//									rgFeaturesToReturn[aKeyPair] = (int)rgFeaturesToReturn[aKeyPair] + 1; 
//								} 
//							}
//							else 
//							{
//								//Debug.WriteLine("Seen " + sWord);
//							}
//						}
//					}
//				} 
//			} 
//
//			// Either get all the items, or just the top ones 
//			ICollection rgKeyPair;
//			if (_iHowManyWordsToUse == int.MaxValue)
//			{
//				rgKeyPair = rgFeaturesToReturn.Keys;
//			}
//			else 
//			{ 
//				// Figure out which features to return
//				BestItems aBestItems = new BestItems(_iHowManyWordsToUse); 
//				foreach(KeyPair aKeyPair in rgFeaturesToReturn.Keys)
//				{
//					aBestItems.Add(aKeyPair,(int) rgFeaturesToReturn[aKeyPair]);
//				}
//				rgKeyPair = aBestItems.Items;
//			} 
// 
//
//			// Create the output array of features 
//			Feature[] rg = new Feature[rgKeyPair.Count];
//			int iFeature = 0;
//			foreach(KeyPair aKeyPair in rgKeyPair)
//			{
//				Feature aHasWord = _fpCreateHasWord(aKeyPair.Word, aKeyPair.TextRunTime);
//				rg[iFeature++] = aHasWord; 
//			} 
//			Debug.Assert(iFeature == rg.Length); // real assert
// 
//			return rg;
//		}
//	}

 	internal class KeyPair
	{ 
		public Feature TextRunTime; 
		public string Word;
 
		public KeyPair(Feature textRunTime, string sWord)
		{
 			TextRunTime = textRunTime;
 			Word = sWord;
			
 		} 
 
		public override int GetHashCode()
		{ 
			return TextRunTime.GetHashCode() ^ Word.GetHashCode();
		}

		public override bool Equals(Object obj)
 		{
 			return obj is KeyPair && this == (KeyPair)obj; 
		} 
 		public static bool operator ==(KeyPair x, KeyPair y)
		{ 
			return  x.TextRunTime == y.TextRunTime && x.Word == y.Word;
		}

		public static bool operator !=(KeyPair x, KeyPair y)
		{
 			return !(x == y); 
 		} 

 
	}

 	public delegate Feature FPCreateHasWord(string text, Feature inputFeature);

}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
