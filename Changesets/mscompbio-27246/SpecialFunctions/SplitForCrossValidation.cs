using System; 
using System.Collections.Generic;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{
    public class SplitForCrossValidation<T> 
    { 
        private SplitForCrossValidation()
        { 
        }

        List<T>[] TestingListCollection;

        public static SplitForCrossValidation<T> GetInstance(IEnumerable<T> enumeration, int foldCount, ref Random random)
        { 
            SplitForCrossValidation<T> splitForCrossValidation = new SplitForCrossValidation<T>(); 

            List<T> shuffledItemCollection = SpecialFunctions.Shuffle(enumeration, ref random); 
            splitForCrossValidation.DealOutShuffledItems(foldCount, shuffledItemCollection);

            return splitForCrossValidation;
        }

        //!!! instead of copying the shuffled items, could just index into a single array 
        private void DealOutShuffledItems(int foldCount, List<T> shuffledItemCollection) 
        {
            InitTestingListCollection(foldCount); 

            for (int iItem = 0; iItem < shuffledItemCollection.Count; ++iItem)
            {
                TestingListCollection[iItem % foldCount].Add(shuffledItemCollection[iItem]);
            }
        } 
 
        private void InitTestingListCollection(int foldCount)
        { 
            TestingListCollection = new List<T>[foldCount];
            for (int iFold = 0; iFold < foldCount; ++iFold)
            {
                TestingListCollection[iFold] = new List<T>();
            }
        } 
 
        //!!!instead of having this method, could just enumerate the whole class
        public IEnumerable<KeyValuePair<IEnumerable<T>, IEnumerable<T>>> TrainAndTestCollection() 
        {
            for (int iFold = 0; iFold < TestingListCollection.Length; ++iFold)
            {
                //!!! instead of creating List's here (which takes memory) could just enumerate the train and test items
                List<T> trainingList = CreateTrainingList(iFold);
                List<T> testingList = TestingListCollection[iFold]; 
                KeyValuePair<IEnumerable<T>, IEnumerable<T>> trainAndTest = new KeyValuePair<IEnumerable<T>, IEnumerable<T>>(trainingList, testingList); 
                yield return trainAndTest;
            } 
        }

        private List<T> CreateTrainingList(int iFold)
        {
            List<T> trainingList = new List<T>();
            for (int jFold = 0; jFold < TestingListCollection.Length; ++jFold) 
            { 
                if (jFold != iFold)
                { 
                    trainingList.AddRange(TestingListCollection[jFold]);
                }
            }
            return trainingList;
        }
    } 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
