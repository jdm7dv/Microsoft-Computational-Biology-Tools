using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using EpipredLib;
 
namespace VirusCount.Qmr 
{
 	public class QmmrModelOnePeptide 
	{

		//public QmrrPartialModel QmrrPartialModel;
		public BestSoFar<double, TrueCollection> BestHlaAssignmentSoFar;
		//OptimizationParameterList OptimizationParameterList;
		public QmrrModelMissingAssignment QmrrModelMissingAssignment; // = QmrrModelMissingAssignment.GetInstance(OptimizationParameterList); 
 
 		internal List<HlaAssignmentsToConsider> HlaAssignmentsToConsiderCollection;
 
 		internal QmmrModelOnePeptide()
		{
 		}


 
		internal void CreateNoSwitchablesHlaAssignment() 
		{
            BestHlaAssignmentSoFar = BestSoFar<double, TrueCollection>.GetInstance(SpecialFunctions.DoubleGreaterThan); 
			TrueCollection trueCollection = TrueCollection.GetInstance(QmrrModelMissingAssignment.KnownHlaSet);
			double scoreOnNoSwitchables = QmrrModelMissingAssignment.LogLikelihoodOfCompleteModelConditionedOnKnownHlas(trueCollection);
			BestHlaAssignmentSoFar.Compare(scoreOnNoSwitchables, trueCollection);
 		}

 		public void FindBestHlaAssignment() 
		{ 

 			int iDelegate = 0; 

			while (true)
			{
				HlaAssignmentsToConsider hlaAssignmentsToConsider = HlaAssignmentsToConsiderCollection[iDelegate];
				int previousChangeCount = BestHlaAssignmentSoFar.ChangeCount;
				ScoreAssignments(hlaAssignmentsToConsider); 
 
 				if (Improvement(previousChangeCount))
 				{ 
					iDelegate = 0;
 				}
				else
				{
					if (LastDelegate(iDelegate))
					{ 
						break; 
 					}
 					else 
					{
 						++iDelegate;
					}
				}

			} 
 
		}
 
		private bool Improvement(int previousChangeCount)
 		{
 			return previousChangeCount != BestHlaAssignmentSoFar.ChangeCount;
		}

 		private bool LastDelegate(int iDelegate) 
		{ 
			return iDelegate == HlaAssignmentsToConsiderCollection.Count - 1;
		} 


		//public void FindBestHlaAssignment()
		//{
 		//    while (true)
 		//    { 
		//        int previousChangeCount = BestHlaAssignmentSoFar.ChangeCount; 
 		//        foreach(HlaAssignmentsToConsiderDelegate hlaAssignmentsToConsider in HlaAssignmentsToConsiderCollection)
		//        { 
		//            RepeatUntilToImprovement(hlaAssignmentsToConsider);
		//        }
		//        if (previousChangeCount == BestHlaAssignmentSoFar.ChangeCount)
		//        {
 		//            break;
 		//        } 
		//    } 

 		//} 

		//private void RepeatUntilToImprovement(HlaAssignmentsToConsiderDelegate hlaAssignmentsToConsider)
		//{
		//    while (true)
		//    {
		//        int previousChangeCount = BestHlaAssignmentSoFar.ChangeCount; 
 		//        ScoreAssignments(hlaAssignmentsToConsider); 
 		//        if (previousChangeCount == BestHlaAssignmentSoFar.ChangeCount)
		//        { 
 		//            break;
		//        }
		//    }
		//}

		private void ScoreAssignments(HlaAssignmentsToConsider hlaAssignmentsToConsider) 
		{ 
 			foreach (TrueCollection hlaAssignment in hlaAssignmentsToConsider.Collection())
 			{ 
				double score = QmrrModelMissingAssignment.LogLikelihoodOfCompleteModelConditionedOnKnownHlas(hlaAssignment);
 				BestHlaAssignmentSoFar.Compare(score, hlaAssignment);
			}
		}

		public void SetForBitFlipsAnd1Replacement() 
		{ 
			HlaAssignmentsToConsiderCollection = new List<HlaAssignmentsToConsider>();
 			HlaAssignmentsToConsiderCollection.Add(EveryHlaAssignmentToConsiderReplace1s.GetInstance(this)); 
 			HlaAssignmentsToConsiderCollection.Add(EveryHlaAssignmentToConsiderBitFlip.GetInstance(this));
		}

 		public void SetForDepthSearch(int depth)
		{
			SpecialFunctions.CheckCondition(depth > 0);//!!!raise error 
 
			HlaAssignmentsToConsiderCollection = new List<HlaAssignmentsToConsider>();
			for (int i = 1; i <= depth; ++i) 
			{
 				HlaAssignmentsToConsiderCollection.Add(EveryHlaAssignmentOfThisDepth.GetInstance(this,i));
 			}
		}

 	} 
 
	abstract public class HlaAssignmentsToConsider
	{ 
		abstract public IEnumerable<TrueCollection> Collection();
		public QmmrModelOnePeptide QmmrModelOnePeptide;

        internal static Set<Hla> CreateAssignmentAsSet(TrueCollection startingAssignment)
		{
            Set<Hla> assignmentAsSet = Set<Hla>.GetInstance(); 
            foreach (Hla hla in startingAssignment) 
 			{
                assignmentAsSet.AddNew(hla); 
 			}
			return assignmentAsSet;
 		}


	} 
 
	public class EveryHlaAssignmentOfThisDepth : HlaAssignmentsToConsider
	{ 
		private int Depth;
		public static HlaAssignmentsToConsider GetInstance(QmmrModelOnePeptide qmmrModelOnePeptide, int depth)
 		{
 			SpecialFunctions.CheckCondition(depth > 0);//!!!raise error
			EveryHlaAssignmentOfThisDepth aEveryHlaAssignmentOfThisDepth = new EveryHlaAssignmentOfThisDepth();
 			aEveryHlaAssignmentOfThisDepth.QmmrModelOnePeptide = qmmrModelOnePeptide; 
			aEveryHlaAssignmentOfThisDepth.Depth = depth; 
			return aEveryHlaAssignmentOfThisDepth;
		} 

		override public IEnumerable<TrueCollection> Collection()
		{
            Set<Hla> assignmentAsSet = CreateAssignmentAsSet(QmmrModelOnePeptide.BestHlaAssignmentSoFar.Champ);
            SubtractKnownHlas(ref assignmentAsSet);
 
            EverySubsetBySize aEverySubsetBySize = EverySubsetBySize.GetInstance(QmmrModelOnePeptide.QmrrModelMissingAssignment.SwitchableHlasOfRespondingPatients.Count, Depth, 1); 
 			foreach (List<int> indexList in aEverySubsetBySize.Collection())
 			{ 
				FlipListedBits(assignmentAsSet, indexList);
 				yield return TrueCollection.GetInstance(QmmrModelOnePeptide.QmrrModelMissingAssignment.KnownHlaSet, assignmentAsSet);
				FlipListedBits(assignmentAsSet, indexList);
			}
		}
 
        private void SubtractKnownHlas(ref Set<Hla> assignmentAsSet) 
        {
            foreach (Hla knownHla in QmmrModelOnePeptide.QmrrModelMissingAssignment.KnownHlaSet) 
            {
                assignmentAsSet.Remove(knownHla);
            }
        }

        private void FlipListedBits(Set<Hla> assignmentAsSet, List<int> indexList) 
		{ 
			foreach (int index in indexList)
 			{ 
                Hla hlaOfRepondingPatients = QmmrModelOnePeptide.QmrrModelMissingAssignment.SwitchableHlasOfRespondingPatients[index];
 				BitFlip(assignmentAsSet, hlaOfRepondingPatients);
			}

 		}
 
        private static void BitFlip(Set<Hla> assignmentAsSet, Hla hlaOfRepondingPatients) 
		{
			if (assignmentAsSet.Contains(hlaOfRepondingPatients)) 
			{
				assignmentAsSet.Remove(hlaOfRepondingPatients);
			}
 			else
 			{
                assignmentAsSet.AddNew(hlaOfRepondingPatients); 
			} 
 		}
 
	}


	public class EveryHlaAssignmentToConsiderReplace1s : HlaAssignmentsToConsider
	{
		public static HlaAssignmentsToConsider GetInstance(QmmrModelOnePeptide qmmrModelOnePeptide) 
		{ 
 			EveryHlaAssignmentToConsiderReplace1s aEveryHlaAssignmentToConsiderReplace1s = new EveryHlaAssignmentToConsiderReplace1s();
 			aEveryHlaAssignmentToConsiderReplace1s.QmmrModelOnePeptide = qmmrModelOnePeptide; 
			return aEveryHlaAssignmentToConsiderReplace1s;
 		}


		override public IEnumerable<TrueCollection> Collection()
		{ 
            Set<Hla> assignmentAsSet = CreateAssignmentAsSet(QmmrModelOnePeptide.BestHlaAssignmentSoFar.Champ); 

            foreach (Hla originalTrueHla in QmmrModelOnePeptide.BestHlaAssignmentSoFar.Champ) 
			{
                if (QmmrModelOnePeptide.QmrrModelMissingAssignment.KnownHlaSet.Contains(originalTrueHla))
                {
                    continue;
                }
				assignmentAsSet.Remove(originalTrueHla); 
                foreach (Hla originalFalseHla in QmmrModelOnePeptide.QmrrModelMissingAssignment.SwitchableHlasOfRespondingPatients) 
				{
 					if (!assignmentAsSet.Contains(originalFalseHla)) 
 					{
                        assignmentAsSet.AddNew(originalFalseHla);
						Debug.Assert(assignmentAsSet.Count == QmmrModelOnePeptide.BestHlaAssignmentSoFar.Champ.Count); // real assert
 						yield return TrueCollection.GetInstance(assignmentAsSet);
						assignmentAsSet.Remove(originalFalseHla);
					} 
				} 
                assignmentAsSet.AddNew(originalTrueHla);
				Debug.Assert(assignmentAsSet.Count == QmmrModelOnePeptide.BestHlaAssignmentSoFar.Champ.Count); // real assert 
			}
 		}
 	}


	public class EveryHlaAssignmentToConsiderBitFlip : HlaAssignmentsToConsider 
 	{ 
		public static HlaAssignmentsToConsider GetInstance(QmmrModelOnePeptide qmmrModelOnePeptide)
		{ 
			EveryHlaAssignmentToConsiderBitFlip aEveryHlaAssignmentToConsiderBitFlip = new EveryHlaAssignmentToConsiderBitFlip();
			aEveryHlaAssignmentToConsiderBitFlip.QmmrModelOnePeptide = qmmrModelOnePeptide;
			return aEveryHlaAssignmentToConsiderBitFlip;
 		}

 
 		override public IEnumerable<TrueCollection> Collection() 
		{
            Set<Hla> assignmentAsSet = CreateAssignmentAsSet(QmmrModelOnePeptide.BestHlaAssignmentSoFar.Champ); 

            foreach (Hla hla in QmmrModelOnePeptide.QmrrModelMissingAssignment.SwitchableHlasOfRespondingPatients)
 			{
				if (assignmentAsSet.Contains(hla))
				{
					assignmentAsSet.Remove(hla); 
					yield return TrueCollection.GetInstance(assignmentAsSet); 
                    assignmentAsSet.AddNew(hla);
 
				}
 				else
 				{
                    assignmentAsSet.AddNew(hla);
					yield return TrueCollection.GetInstance(assignmentAsSet);
 					assignmentAsSet.Remove(hla); 
				} 
			}
		} 
	}
}


// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
