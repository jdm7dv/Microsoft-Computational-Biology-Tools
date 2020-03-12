using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.Qmr;
using Msr.Mlas.SpecialFunctions;
 
//!!!replace TCause of string with Int16 
//!!!use arrays rather than foreach
//!!!in the inner loop only only look at effects of the current cause (may need arrays) 
//!!!pre-compute 1.0 - prior
//!!!distribute prior into condidtional probability
//!!!use multiple threads?
//!!!make arrays unsafe in innerloop?
//!!! remove .Length from for loops?
//!!! How about not just ushort, but ubyte? 
//!!!don't output leak 
//!!!sort each epitope by probability
//!!!instead of Int16 should it be Int16 to match arrays? (should this be a template type?) 
//!!!         Int16 sign = 1; //!!!could make the type a byte or event a double
//!!! increase numerical stablity by going to log space (use SpecialFunctions.LogSum())



namespace VirusCount 
{ 
 	public class Quickscore<TCause, TEffect> : Qmr<TCause, TEffect>
	{ 
		private static void AssertNear(double goal, double expression)
		{
			Debug.Assert(Math.Abs(expression - goal) < .0000000001);
		}
 		public static void Test()
 		{ 
			Quickscore<string, string> quickscoreSS = Quickscore<string, string>.GetInstance(""); 

 			quickscoreSS.SetCause("cold", .01); 
			quickscoreSS.SetLink("cold", "cough", .1);
			quickscoreSS.SetLink("cold", "runny nose", .3);
			AssertNear(.01, quickscoreSS.PosteriorOfCause("cold", new string[] { }, new string[] { }));
			AssertNear(1, quickscoreSS.PosteriorOfCause("cold", new string[] { "runny nose" }, new string[] { }));
			AssertNear(.9 * .01 / (1 - .01 * .1), quickscoreSS.PosteriorOfCause("cold", new string[] { }, new string[] { "cough" }));
 
 			quickscoreSS.SetCause("onion attack", .001); 
 			quickscoreSS.SetLink("onion attack", "watery eyes", .9);
			AssertNear(.001, quickscoreSS.PosteriorOfCause("onion attack", new string[] { }, new string[] { })); 
 			AssertNear(.001, quickscoreSS.PosteriorOfCause("onion attack", new string[] { "cough" }, new string[] { }));
			AssertNear(.001, quickscoreSS.PosteriorOfCause("onion attack", new string[] { }, new string[] { "cough" }));
			AssertNear(1, quickscoreSS.PosteriorOfCause("onion attack", new string[] { "watery eyes" }, new string[] { }));


			quickscoreSS.SetCause("allergy", .02); 
			quickscoreSS.SetLink("allergy", "watery eyes", .1); 
			quickscoreSS.SetLink("allergy", "runny nose", .6);
 
 			AssertNear(0.223055458667595, quickscoreSS.PosteriorOfCause("cold", new string[] { "runny nose" }, new string[] { "watery eyes" }));
 			AssertNear(0.786362050924305, quickscoreSS.PosteriorOfCause("allergy", new string[] { "runny nose" }, new string[] { "watery eyes" }));
			AssertNear(1.0, quickscoreSS.PosteriorOfCause("cold", new string[] { "runny nose", "cough" }, new string[] { "watery eyes" }));
 			AssertNear(0.042220484753702256, quickscoreSS.PosteriorOfCause("allergy", new string[] { "runny nose", "cough" }, new string[] { "watery eyes" }));

			Dictionary<string, double> posteriorOfEveryCauseSS = quickscoreSS.PosteriorOfEveryCause(new string[] { "runny nose", "cough" }, new string[] { "watery eyes" }); 
			AssertNear(1.0, posteriorOfEveryCauseSS["cold"]); 
			AssertNear(0.042220484753702256, posteriorOfEveryCauseSS["allergy"]);
 
			AssertNear(1, quickscoreSS.PosteriorOfCause("cold", new string[] { "cough" }, new string[] { }));
			quickscoreSS.SetLeak("cough", 1.0 / 300.0);
 			AssertNear(0.237875288683606, quickscoreSS.PosteriorOfCause("cold", new string[] { "cough" }, new string[] { }));


 			AssertNear(.01, quickscoreSS.PosteriorOfCause("cold", new string[] { }, new string[] { "watery eyes" })); 
 

			Quickscore<double, int> quickscoreDL = Quickscore<double, int>.GetInstance(double.NaN); 

 			quickscoreDL.SetCause(3.1, .01);
			quickscoreDL.SetLink(3.1, 0, .1);
			quickscoreDL.SetLink(3.1, 2, .3);
			AssertNear(.01, quickscoreDL.PosteriorOfCause(3.1, new int[] { }, new int[] { }));
			AssertNear(1, quickscoreDL.PosteriorOfCause(3.1, new int[] { 0 }, new int[] { })); 
			Dictionary<double, double> posteriorOfEveryCauseDL = quickscoreDL.PosteriorOfEveryCause(new int[] { }, new int[] { 2 }); 
 			quickscoreDL.SetLeak(0, .001);
 			AssertNear(0.50475237618810087, quickscoreDL.PosteriorOfCause(3.1, new int[] { 0 }, new int[] { })); 

			TestBigSize();


 		}
 
		static internal void SubsetTest() 
		{
			List<int> universe = new List<int>(); 


			while (true)
			{
 				Stopwatch stopwatch = new Stopwatch();
 				stopwatch.Start(); 
				Int16 total = 0; 
 				for (Int16 causei = 0; causei < Math.Min(universe.Count * 10, 600); ++causei)
				{ 
					for (Int16 subsetIndex = 0; subsetIndex < Math.Pow(2, universe.Count); ++subsetIndex)
					//foreach (IList<int> subset in Quickscore<int, int>.Subsets(universe, 0))
					{
						for (Int16 causej = 0; causej < 600; ++causej)
 						{
 							for (Int16 effectIndex = 0; effectIndex < subsetIndex; ++effectIndex) 
							{ 
 								++total;
							} 
						}
					}
				}
				stopwatch.Stop();
 				Console.WriteLine("{0}\t{1}\t{2}", universe.Count, stopwatch.Elapsed, total);
 				universe.Add(universe.Count); 
 
			}
 
 		}

		internal static void TestBigSize()
		{
			Random random = new Random(023029);
			Dictionary<int, bool> effectList; 
			Quickscore<int, int> quickscore = GetRandomInstance(random, 600, 10, 10, .0001, out effectList); 
 			int[] effectListX = new int[effectList.Count];
 			effectList.Keys.CopyTo(effectListX, 0); 
			for (int numberOfPositiveFindings = 0; numberOfPositiveFindings < 20; ++numberOfPositiveFindings)
 			{
				int[] positiveFinding = GetRandomEffects(random, effectListX, numberOfPositiveFindings);

				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start(); 
 
				quickscore.PosteriorOfEveryCause(positiveFinding, new int[] { });
 
				stopwatch.Stop();
 				Console.WriteLine("{0}\t{1}", numberOfPositiveFindings, stopwatch.Elapsed);
 			}
		}

 		private static int[] GetRandomEffects(Random random, int[] effectList, int numberOfPositiveFindings) 
		{ 
			Dictionary<int, bool> positiveFindingHashtable = new Dictionary<int, bool>();
			while (positiveFindingHashtable.Count < numberOfPositiveFindings) 
			{
				positiveFindingHashtable[effectList[random.Next(effectList.Length)]] = true;
 			}
 			int[] positiveFinding = new int[positiveFindingHashtable.Count];
			positiveFindingHashtable.Keys.CopyTo(positiveFinding, 0);
 			return positiveFinding; 
		} 

		internal static Quickscore<int, int> GetRandomInstance(Random random, int cCause, int cEffectsPerCause, int maxJumpBetweenEffects, double leakProbability, out Dictionary<int, bool> effectList) 
		{
			Quickscore<int, int> quickscore = Quickscore<int, int>.GetInstance(int.MinValue);
			effectList = new Dictionary<int, bool>();
 			for (int cause = 0; cause < cCause; ++cause)
 			{
				quickscore.SetCause(cause, random.NextDouble()); 
 				int effect = -1; 
				for (int iEffect = 0; iEffect < cEffectsPerCause; ++iEffect)
				{ 
					effect += 1 + random.Next(maxJumpBetweenEffects);
					quickscore.SetLink(cause, effect, random.NextDouble());
					if (!effectList.ContainsKey(effect))
 					{
 						quickscore.SetLeak(effect, leakProbability);
						effectList.Add(effect, true); 
 					} 
				}
			} 
			return quickscore;
		}

		override public void SetLeak(TEffect effect, double leakProbability)
 		{
 			SetLink(Leak, effect, leakProbability); 
		} 

 		private Dictionary<TCause, double> CauseCollection = new Dictionary<TCause, double>(); 


		override public void SetCause(TCause cause, double priorProbability)
		{
			SpecialFunctions.CheckCondition(0.0 <= priorProbability && priorProbability <= 1.0); //!!!raise error
 
			CauseCollection[cause] = priorProbability; 
			if (!CauseEffectCollection.ContainsKey(cause))
 			{ 
 				CauseEffectCollection.Add(cause, new Dictionary<TEffect, double>());
			}
 		}

		//From eq 12 but this has a correction that is missing from the text
		//!!!need to be able to do many causes at once for speed up 
		override public Dictionary<TCause, double> PosteriorOfEveryCause(IList<TEffect> presentEffectCollection, IList<TEffect> absentEffectCollection) 
		{
			CheckThatEffectsAppearAtMostOnce(presentEffectCollection, absentEffectCollection); 

 			//We do a copy instead of changing the member distribution to make this more thread-safe and cleaner.
 			Dictionary<TCause, double> causeCollectionWith0Or1Assignments = new Dictionary<TCause, double>(CauseCollection);

			Dictionary<TCause, double> posteriorOfEveryCause = new Dictionary<TCause, double>();
 			double bottom = Probability(presentEffectCollection, absentEffectCollection, CauseCollection); 
 
			foreach (KeyValuePair<TCause, double> causeAndPrior in CauseCollection)
			{ 
				TCause cause = causeAndPrior.Key;
				if (cause.Equals(Leak))
				{
 					continue;
 				}
 
				double prior = causeAndPrior.Value; 

 				if (CauseHasPresentOrAbsentEffects(cause, presentEffectCollection, absentEffectCollection)) 
				{
					causeCollectionWith0Or1Assignments[cause] = 1.0;
					double top = prior * Probability(presentEffectCollection, absentEffectCollection, causeCollectionWith0Or1Assignments);
					causeCollectionWith0Or1Assignments[cause] = prior;
					posteriorOfEveryCause[cause] = top / bottom;
 				} 
 				else 
				{
 					posteriorOfEveryCause[cause] = prior; 
				}

				Debug.Assert(0.0 <= posteriorOfEveryCause[cause] && posteriorOfEveryCause[cause] <= 1.0);
			}
			return posteriorOfEveryCause;
		} 
 

 		//!!!this could be made faster, but it's not in the inner loops, so it speed doesn't matter 
 		//!!!This test doesn't speed up anything if every cause as at least one effect and if all effects are either present or absent
		private bool CauseHasPresentOrAbsentEffects(TCause cause, IList<TEffect> presentEffectCollection, IList<TEffect> absentEffectCollection)
 		{
			Dictionary<TEffect, double> conditionalProb = CauseEffectCollection[cause];
			foreach (TEffect effect in absentEffectCollection)
			{ 
				if (conditionalProb.ContainsKey(effect)) 
				{
 					return true; 
 				}
			}
 			foreach (TEffect effect in presentEffectCollection)
			{
				if (conditionalProb.ContainsKey(effect))
				{ 
					return true; 
				}
 			} 
 			return false;

		}


 		//From eq 12 but this has a correction that is missing from the text 
		//!!!need to be able to do many causes at once for speed up 
		override public double PosteriorOfCause(TCause cause, IList<TEffect> presentEffectCollection, IList<TEffect> absentEffectCollection)
		{ 
			CheckThatEffectsAppearAtMostOnce(presentEffectCollection, absentEffectCollection);

			//We do a copy instead of changing the member distribution to make this more thread-safe and cleaner.
 			Dictionary<TCause, double> causeCollectionWith0Or1Assignments = new Dictionary<TCause, double>(CauseCollection);

 			double prior = CauseCollection[cause];//!!!Raise error 
			causeCollectionWith0Or1Assignments[cause] = 1.0; 
 			double top = prior * Probability(presentEffectCollection, absentEffectCollection, causeCollectionWith0Or1Assignments);
			double bottom = Probability(presentEffectCollection, absentEffectCollection, CauseCollection); 
			double probability = top / bottom;
			return probability;
		}

		private static void CheckThatEffectsAppearAtMostOnce(IList<TEffect> presentEffectCollection, IList<TEffect> absentEffectCollection)
 		{ 
 			//Will get error if any effects appear more than once 
			Dictionary<TEffect, bool> justChecking = new Dictionary<TEffect, bool>();
 			foreach (TEffect s in presentEffectCollection) 
			{
				justChecking.Add(s, true);
			}
			foreach (TEffect s in absentEffectCollection)
			{
 				justChecking.Add(s, true); 
 			} 
		}
 
 		// From equation 11 of "A Tractable Inference Algorithm for Diagnosing Multiple Diseases", Heckerman, 1989
		private double Probability(IList<TEffect> presentEffectCollection, IList<TEffect> absentEffectCollection, Dictionary<TCause, double> causeCollectionWith0Or1Assignments)
		{

			Dictionary<TCause, Int16> causeToCauseIndex = IndexCauses(causeCollectionWith0Or1Assignments);
 
			double[] causeIndexToOneLessPrior = CreateCauseIndexToOneLessPrior(causeCollectionWith0Or1Assignments, causeToCauseIndex); 

			double[] causeIndexToAbsentEffectProductTimePrior = CreateCauseIndexToAbsentEffectProductTimePrior(absentEffectCollection, causeCollectionWith0Or1Assignments, causeToCauseIndex); 

 			Dictionary<TEffect, Int16> presentEffectToPresentEffectIndex = CreatePresentEffectIndex(presentEffectCollection);

 			//!!!these two datastructures could be together in a struct
			Int16[][] causeIndexToListOfPresentEffects;
 			double[][] causeIndexToListOfCondProb; 
			CreateCauseIndexToEffectListInfo(presentEffectCollection, causeCollectionWith0Or1Assignments, 
				causeToCauseIndex, presentEffectToPresentEffectIndex, out causeIndexToListOfPresentEffects, out causeIndexToListOfCondProb);
 

			double sumProbability = 0.0;
			foreach (KeyValuePair<bool[], Int16> presentEffectSubsetAndSign in Subsets(presentEffectToPresentEffectIndex))
			{
 				double productAcrossCauses = presentEffectSubsetAndSign.Value
 					* ProductAcrossCauses( 
							presentEffectSubsetAndSign.Key, 
 							causeIndexToListOfPresentEffects,
							causeIndexToListOfCondProb, 
							causeIndexToAbsentEffectProductTimePrior,
							causeIndexToOneLessPrior);
				sumProbability += productAcrossCauses;
			}
 			return sumProbability;
 		} 
 
		private IEnumerable<KeyValuePair<bool[], Int16>> Subsets(Dictionary<TEffect, short> presentEffectToPresentEffectIndex)
 		{ 
			Int16 sign = 1; //!!!could make the type a byte or event a double
			bool[] presentEffectIndexToBool = new bool[presentEffectToPresentEffectIndex.Count]; // C# inits to all false
			while (true)
			{
				yield return new KeyValuePair<bool[], Int16>(presentEffectIndexToBool, sign);
 				for (Int16 i = 0; i < presentEffectIndexToBool.Length; ++i) 
 				{ 
					sign *= -1;
 					if (!presentEffectIndexToBool[i]) 
					{
						presentEffectIndexToBool[i] = true;
						goto good;
					}
					else
 					{ 
 						presentEffectIndexToBool[i] = false; 
					}
 				} 
				break; //while
			good: ;
			}
		}

		private void CreateCauseIndexToEffectListInfo(IList<TEffect> presentEffectCollection, 
 			Dictionary<TCause, double> causeCollectionWith0Or1Assignments, 
 			Dictionary<TCause, Int16> causeToCauseIndex,
			Dictionary<TEffect, Int16> presentEffectToPresentEffectIndex, 
 			out Int16[][] causeIndexToListOfPresentEffects,
			out double[][] causeIndexToListOfCondProb)
		{
			causeIndexToListOfPresentEffects = new Int16[causeCollectionWith0Or1Assignments.Count][];
			causeIndexToListOfCondProb = new double[causeCollectionWith0Or1Assignments.Count][];
			foreach (KeyValuePair<TCause, double> causeAndPrior in causeCollectionWith0Or1Assignments) 
 			{ 
 				TCause cause = causeAndPrior.Key;
				Int16 iCause = causeToCauseIndex[causeAndPrior.Key]; 

 				Dictionary<TEffect, double> effectToConditionalProb = CauseEffectCollection[cause];

				List<Int16> presentEffectIndexList = new List<Int16>();
				List<double> listOfCondProb = new List<double>();
				foreach (TEffect presentEffect in presentEffectCollection) 
				{ 
					if (effectToConditionalProb.ContainsKey(presentEffect))
 					{ 
 						Int16 presentEffectIndex = presentEffectToPresentEffectIndex[presentEffect];
						presentEffectIndexList.Add(presentEffectIndex);

 						double conditionalProb = 1.0 - effectToConditionalProb[presentEffect];
						listOfCondProb.Add(conditionalProb);
					} 
				} 
				causeIndexToListOfPresentEffects[iCause] = new Int16[presentEffectIndexList.Count];
				presentEffectIndexList.CopyTo(causeIndexToListOfPresentEffects[iCause], 0); 

 				causeIndexToListOfCondProb[iCause] = new double[listOfCondProb.Count];
 				listOfCondProb.CopyTo(causeIndexToListOfCondProb[iCause], 0);

			}
 		} 
 
		private double[] CreateCauseIndexToAbsentEffectProductTimePrior(IList<TEffect> absentEffectCollection, Dictionary<TCause, double> causeCollectionWith0Or1Assignments, Dictionary<TCause, Int16> causeToCauseIndex)
		{ 
			double[] causeIndexToAbsentEffectProductTimePrior = new double[causeCollectionWith0Or1Assignments.Count];
			foreach (KeyValuePair<TCause, double> causeAndPrior in causeCollectionWith0Or1Assignments)
			{
 				TCause cause = causeAndPrior.Key;
 				double prior = causeAndPrior.Value;
				Int16 iCause = causeToCauseIndex[causeAndPrior.Key]; 
 
 				Dictionary<TEffect, double> effectToConditionalProb = CauseEffectCollection[cause];
 
				causeIndexToAbsentEffectProductTimePrior[iCause] = prior;
				foreach (TEffect absentEffect in absentEffectCollection)
				{
					if (effectToConditionalProb.ContainsKey(absentEffect))
					{
 						causeIndexToAbsentEffectProductTimePrior[iCause] *= (1.0 - effectToConditionalProb[absentEffect]); 
 					} 
				}
 			} 
			return causeIndexToAbsentEffectProductTimePrior;
		}

		private static Dictionary<TEffect, Int16> CreatePresentEffectIndex(IList<TEffect> presentEffectCollection)
		{
			Dictionary<TEffect, Int16> presentEffectToPresentEffectIndex = new Dictionary<TEffect, Int16>(); 
 			Int16 iPresentEffect = 0; 
 			foreach (TEffect presentEffect in presentEffectCollection)
			{ 
 				presentEffectToPresentEffectIndex.Add(presentEffect, iPresentEffect);
				++iPresentEffect;
			}
			return presentEffectToPresentEffectIndex;
		}
 
		private static double[] CreateCauseIndexToOneLessPrior(Dictionary<TCause, double> causeCollectionWith0Or1Assignments, Dictionary<TCause, Int16> causeToCauseIndex) 
 		{
 			double[] causeIndexOneLessPrior = new double[causeCollectionWith0Or1Assignments.Count]; 
			foreach (KeyValuePair<TCause, double> causeAndPrior in causeCollectionWith0Or1Assignments)
 			{
				Int16 iCause = causeToCauseIndex[causeAndPrior.Key];
				causeIndexOneLessPrior[iCause] = 1.0 - causeAndPrior.Value;
			}
			return causeIndexOneLessPrior; 
		} 

 		private static Dictionary<TCause, Int16> IndexCauses(Dictionary<TCause, double> causeCollectionWith0Or1Assignments) 
 		{
			Dictionary<TCause, Int16> causeToCauseIndex = new Dictionary<TCause, Int16>();

 			Int16 iCause = 0;
			foreach (KeyValuePair<TCause, double> causeAndPrior in causeCollectionWith0Or1Assignments)
			{ 
				TCause cause = causeAndPrior.Key; 
				causeToCauseIndex.Add(cause, iCause);
				++iCause; 
 			}

 			return causeToCauseIndex;
		}

 		//!!!would "bitmap" method be faster? 
		static private IEnumerable<List<TEffect>> Subsets(IList<TEffect> set, int start) 
		{
			if (start == set.Count) 
			{
				yield return new List<TEffect>();
 			}
 			else
			{
 				foreach (List<TEffect> subsubset in Subsets(set, start + 1)) 
				{ 
					yield return subsubset;
					subsubset.Add(set[start]); 
					yield return subsubset;
				}
 			}
 		}

		//IList<TEffect> presentEffectSubset, IList<TEffect> absentEffectCollection, Dictionary<TCause, double> causeCollectionWith0Or1Assignment 
 		private double ProductAcrossCauses( 
					bool[] presentEffectSubset,
					Int16[][] causeIndexToListOfPresentEffects, 
					double[][] causeIndexToListOfCondProb,
					double[] causeIndexToAbsentEffectProductTimePrior,
					double[] causeIndexOneLessPrior)
 		{
 			double productProbabilityCauses = 1.0;
			for (Int16 causeIndex = 0; causeIndex < causeIndexToListOfPresentEffects.Length; ++causeIndex) 
 			{ 
				Int16[] listOfPresentEffects = causeIndexToListOfPresentEffects[causeIndex];
				double[] listCondProb = causeIndexToListOfCondProb[causeIndex]; 

				double productProbabilityEffects = causeIndexToAbsentEffectProductTimePrior[causeIndex];
				for (Int16 presentEffectIndexIndex = 0; presentEffectIndexIndex < listOfPresentEffects.Length; ++presentEffectIndexIndex)
				{
 					Int16 presentEffectIndex = listOfPresentEffects[presentEffectIndexIndex];
 					if (presentEffectSubset[presentEffectIndex]) 
					{ 
 						productProbabilityEffects *= listCondProb[presentEffectIndexIndex];
					} 
				}

				productProbabilityCauses *= productProbabilityEffects + causeIndexOneLessPrior[causeIndex];
			}
			return productProbabilityCauses;
 		} 
 
 		private double ProductAcrossOneSetOfEffects(Dictionary<TEffect, double> probabilitiesFromCause, IList<TEffect> effectCollection)
		{ 
 			double productProbability = 1.0;
			foreach (TEffect effect in effectCollection)
			{
				if (probabilitiesFromCause.ContainsKey(effect))
				{
					productProbability *= (1.0 - probabilitiesFromCause[effect]); //!!!if always used this direction, could reverse 
 				} 
 			}
			return productProbability; 
 		}



		override public void SetLink(TCause cause, TEffect effect, double conditionalProbability)
		{ 
            SpecialFunctions.CheckCondition(0.0 <= conditionalProbability && conditionalProbability <= 1.0); //!!!raise error 
            SpecialFunctions.CheckCondition(CauseEffectCollection.ContainsKey(cause), "cause not defined");
			CauseEffectCollection[cause][effect] = conditionalProbability; 
		}

		internal Dictionary<TCause, Dictionary<TEffect, double>> CauseEffectCollection = new Dictionary<TCause, Dictionary<TEffect, double>>();
 		public List<TEffect> EffectList()
 		{
			Dictionary<TEffect, bool> seenIt = new Dictionary<TEffect, bool>(); 
 			foreach (Dictionary<TEffect, double> effectToConditionalProb in CauseEffectCollection.Values) 
			{
				foreach (TEffect effect in effectToConditionalProb.Keys) 
				{
					seenIt[effect] = true;
				}
 			}

 			return new List<TEffect>(seenIt.Keys); 
		} 

 		private TCause Leak; 

		public static Quickscore<TCause, TEffect> GetInstance(TCause leak)
		{
			Quickscore<TCause, TEffect> quickscore = new Quickscore<TCause, TEffect>();
			quickscore.Leak = leak;
			quickscore.SetCause(leak, 1.0); //adding the "leak cause" 
 			return quickscore; 
 		}
 

		public double LogLikelihoodOfModelWithCompleteAssignments(List<TEffect> patientsWhoRespond, List<TEffect> patientsWhoDoNotRespond, Dictionary<TCause, bool> hlaAssignment, Dictionary<TEffect, double> patientWeightTableOrNull)
 		{
			double logLikelihood = 0;

			Dictionary<TEffect, double> effectToLogPiPOfNoEffectGivenCause; 
 
			TallyLogLikelihoodOfCauses(hlaAssignment, ref logLikelihood, out effectToLogPiPOfNoEffectGivenCause);
			TallyLogLikelihoodOfPresentEffects(patientsWhoRespond, ref logLikelihood, ref effectToLogPiPOfNoEffectGivenCause, patientWeightTableOrNull); 
			TallyLogLikelihoodOfAbsentEffects(patientsWhoDoNotRespond, ref logLikelihood, ref effectToLogPiPOfNoEffectGivenCause, patientWeightTableOrNull);

            SpecialFunctions.CheckCondition(effectToLogPiPOfNoEffectGivenCause.Count == 0); //!!!raise error
 			return logLikelihood;
 		}
 
		private static void TallyLogLikelihoodOfAbsentEffects(List<TEffect> patientsWhoDoNotRespond, ref double logLikelihood, ref Dictionary<TEffect, double> effectToLogPiPOfNoEffectGivenCause, Dictionary<TEffect, double> patientWeightTableOrNull) 
 		{
			foreach (TEffect effect in patientsWhoDoNotRespond) 
			{
				double logPiPOfNoEffectGivenCause = GetLogPiPOfNoEffectGivenCause(effectToLogPiPOfNoEffectGivenCause, effect);
				logLikelihood += logPiPOfNoEffectGivenCause * (patientWeightTableOrNull == null ? 1.0 : patientWeightTableOrNull[effect]);
			}

 		} 
 
 		private static void TallyLogLikelihoodOfPresentEffects(List<TEffect> patientsWhoRespond, ref double logLikelihood, ref Dictionary<TEffect, double> effectToLogPiPOfNoEffectGivenCause, Dictionary<TEffect, double> patientWeightTableOrNull)
		{ 
 			foreach (TEffect effect in patientsWhoRespond)
			{
				double logPiPOfNoEffectGivenCause = GetLogPiPOfNoEffectGivenCause(effectToLogPiPOfNoEffectGivenCause, effect);
				double probabilityOfOutcome = 1.0 - Math.Exp(logPiPOfNoEffectGivenCause);
				logLikelihood += Math.Log(probabilityOfOutcome) * (patientWeightTableOrNull == null ? 1.0 : patientWeightTableOrNull[effect]);
			} 
 		} 

 		private static double GetLogPiPOfNoEffectGivenCause(Dictionary<TEffect, double> effectToLogPiPOfNoEffectGivenCause, TEffect effect) 
		{
 			double logPiPOfNoEffectGivenCause;
			if (effectToLogPiPOfNoEffectGivenCause.ContainsKey(effect))
			{
				logPiPOfNoEffectGivenCause = effectToLogPiPOfNoEffectGivenCause[effect];
				effectToLogPiPOfNoEffectGivenCause.Remove(effect); 
			} 
 			else
 			{ 
				logPiPOfNoEffectGivenCause = 0.0;
 			}
			return logPiPOfNoEffectGivenCause;
		}

		private void TallyLogLikelihoodOfCauses(Dictionary<TCause, bool> hlaAssignment, ref double logLikelihood, out Dictionary<TEffect, double> effectToLogPiPOfNoEffectGivenCause) 
		{ 
			effectToLogPiPOfNoEffectGivenCause = new Dictionary<TEffect, double>();
 			foreach (KeyValuePair<TCause, double> causeAndPrior in CauseCollection) 
 			{
				TCause cause = causeAndPrior.Key;
 				double prior = causeAndPrior.Value;

				bool causeValue = (cause.Equals(Leak)) ? true : hlaAssignment[cause];
 
				logLikelihood += Math.Log(causeValue ? prior : (1.0 - prior)); 

				if (causeValue) 
				{
					foreach (KeyValuePair<TEffect, double> effectAndConditionalProbability in CauseEffectCollection[cause])
 					{
 						TEffect effect = effectAndConditionalProbability.Key;
						double conditionalProbability = effectAndConditionalProbability.Value;
 
 						double logPiPOfNoEffectGivenCause = (effectToLogPiPOfNoEffectGivenCause.ContainsKey(effect)) ? effectToLogPiPOfNoEffectGivenCause[effect] : 0.0; 
						effectToLogPiPOfNoEffectGivenCause[effect] = logPiPOfNoEffectGivenCause + Math.Log(1.0 - conditionalProbability);
					} 
				}
			}
		}


 
 		internal Dictionary<TEffect, double> PosteriorOfEveryEffect(Dictionary<TCause, bool> hlaAssignment) 
 		{
			double logLikelihood = 0; 
 			Dictionary<TEffect, double> effectToLogPiPOfNoEffectGivenCause;
			TallyLogLikelihoodOfCauses(hlaAssignment, ref logLikelihood, out effectToLogPiPOfNoEffectGivenCause);

			Dictionary<TEffect, double> posteriorOfEveryEffect = new Dictionary<TEffect, double>();
			foreach (KeyValuePair<TEffect, double> effectAndLogPiPOfNoEffectGivenCause in effectToLogPiPOfNoEffectGivenCause)
			{ 
				TEffect effect = effectAndLogPiPOfNoEffectGivenCause.Key; 
 				double posteriorOfEffect = 1.0 - Math.Exp(effectAndLogPiPOfNoEffectGivenCause.Value);
 				posteriorOfEveryEffect.Add(effect, posteriorOfEffect); 
			}
 			return posteriorOfEveryEffect;
		}

		public IEnumerable<TCause> CauseList()
		{ 
			foreach (TCause cause in CauseCollection.Keys) 
			{
 				if (!cause.Equals(Leak)) 
 				{
					yield return cause;
 				}
			}
		}
 
		//internal double LogLikelihoodOfModelWithCompleteAssignments(Dictionary<int, double> probabilityThatPatientResponds, Dictionary<string, bool> hlaAssignment) 
		//{
		//    double logLikelihood = 0; 

 		//    Dictionary<TEffect, double> effectToLogPiPOfNoEffectGivenCause;

 		//    TallyLogLikelihoodOfCauses(hlaAssignment, ref logLikelihood, out effectToLogPiPOfNoEffectGivenCause);
		//    TallyLogLikelihoodOfEffects(probabilityThatPatientResponds, ref logLikelihood, ref effectToLogPiPOfNoEffectGivenCause);
 
 		//    Study.CheckCondition(effectToLogPiPOfNoEffectGivenCause.Count == 0); //!!!raise error 
		//    return logLikelihood;
		//} 

		//private void TallyLogLikelihoodOfEffects(Dictionary<int, double> probabilityThatPatientResponds, ref double logLikelihood, ref Dictionary<TEffect, double> effectToLogPiPOfNoEffectGivenCause)
		//{
		//    foreach (KeyValuePair<TEffect,double> effectAndProbability in probabilityThatPatientResponds)
 		//    {
 		//        TEffect effect = effectAndProbability.Key; 
		//        double probability = effectAndProbability.Value; 

 		//        double logPiPOfNoEffectGivenCause = GetLogPiPOfNoEffectGivenCause(effectToLogPiPOfNoEffectGivenCause, effect); 
		//        double logProbabilityOfOutcome = probability * Math.Log(1.0 - Math.Exp(logPiPOfNoEffectGivenCause)) + (1.0 - probability) * logPiPOfNoEffectGivenCause;
		//        logLikelihood += logProbabilityOfOutcome;
		//    }
		//}

		//internal IEnumerable<TCause> CausesOfThisEffect(TEffect goalEffect) 
 		//{ 
 		//    Dictionary<TCause, bool> causeSet = new Dictionary<TCause, bool>();
		//    foreach (KeyValuePair<TCause, Dictionary<TEffect, double>> causeAndEffectToProbability in CauseEffectCollection) 
 		//    {
		//        foreach (TEffect anEffect in causeAndEffectToProbability.Value.Keys)
		//        {
		//            if (anEffect.Equals(goalEffect))
		//            {
		//                causeSet[causeAndEffectToProbability.Key] = true; 
 		//            } 
 		//        }
		//    } 
 		//    return causeSet.Keys;
		//}

		//If we already had a mapping from effect to cause, this could be faster for small subsets
		public Dictionary<TCause, List<TEffect>> CreateCauseToSubsetOfEffects(ICollection<TEffect> effectSubset)
		{ 
			Dictionary<TEffect, bool> effectsAsDictionary = new Dictionary<TEffect, bool>(); 
 			foreach (TEffect effect in effectSubset)
 			{ 
				effectsAsDictionary.Add(effect, true);
 			}

			Dictionary<TCause, List<TEffect>> causeToSubsetOfEffects = new Dictionary<TCause, List<TEffect>>();
			foreach(TCause cause in CauseList())
			{ 
				Dictionary<TEffect, double> effectToProbability = CauseEffectCollection[cause]; 
				List<TEffect> effectList = new List<TEffect>();
 				foreach (TEffect anEffect in effectToProbability.Keys) 
 				{
					if (effectsAsDictionary.ContainsKey(anEffect))
 					{
						effectList.Add(anEffect);
					}
				} 
				if (effectList.Count > 0) 
				{
 					causeToSubsetOfEffects.Add(cause, effectList); 
 				}
			}

 			return causeToSubsetOfEffects;
		}
 
		public IEnumerable<TEffect> EffectListForCause(TCause cause) 
		{
			foreach (TEffect effect in CauseEffectCollection[cause].Keys) 
			{
 				yield return effect;
 			}
		}

 	} 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
