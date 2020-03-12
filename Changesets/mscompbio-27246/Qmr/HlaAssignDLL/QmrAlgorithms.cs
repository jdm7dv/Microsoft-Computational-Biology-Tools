using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Msr.Mlas.SpecialFunctions; 
using EpipredLib; 

namespace VirusCount.Qmr 
{
 	public class QmrAlgorithms
	{

        static public bool? HasHla(Hla hlaGoal, Dictionary<string, string> row, HlaResolution hlaResolution)
		{ 
			bool hasNull = false; 
			for (int i = 1; i <= 2; ++i)
			{ 
 				string column = hlaGoal.ToString().Substring(0, 1) + i.ToString();
 				HlaToLength hlaToLengthOrNull = HlaAssignmentParams.GetHlaToLengthValueOrNull(row, column, hlaResolution);
				if (hlaToLengthOrNull == null || hlaToLengthOrNull.HlaNumberToLength >= 9000)
 				{
					hasNull = true;
					continue; 
				} 

				if (hlaGoal.ToString() == hlaToLengthOrNull.ToString()) 
				{
 					return true;
 				}
			}
 			if (hasNull)
			{ 
				return null; 
			}
			else 
			{
 				return false;
 			}

		}
        static public IEnumerable<Hla> FindAllHla(List<Dictionary<string, string>> expandedTable, HlaResolution hlaResolution, string header) 
 		{ 
            Qmrr.HlaFactory hlaFactory = Qmrr.HlaFactory.GetFactory("noConstraint");
 
            Dictionary<Hla, bool> seenIt = new Dictionary<Hla, bool>();
            foreach (Dictionary<string, string> row in expandedTable)
			{
				foreach (string column in HlaAssignmentParams.CreateHlaColumns(header))
				{
					HlaToLength hlaToLengthOrNull = HlaAssignmentParams.GetHlaToLengthValueOrNull(row, column, hlaResolution); 
					if (hlaToLengthOrNull == null || hlaToLengthOrNull.HlaNumberToLength >= 9000) 
 					{
 						continue; 
					}
                    Hla hla = hlaFactory.GetGroundInstance(hlaToLengthOrNull.ToString());
 					if (!seenIt.ContainsKey(hla))
					{
						seenIt.Add(hla, true);
						yield return hla; 
					} 
				}
 			} 
 		}




		static public Dictionary<string, List<Dictionary<string, string>>> CreateCauseAssignmentTable(string filename, string header) 
 		{ 
			Dictionary<string, List<Dictionary<string, string>>> causeAssignmentTable = new Dictionary<string, List<Dictionary<string, string>>>();
 
			string peptideColumn = "peptide";
			string hlaColumn = "HLA";


            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(filename, header, true))
			{ 
				string peptide = row[peptideColumn]; 
 				string hla = row[hlaColumn];
 				if (!nineXs.IsMatch(hla)) 
				{
 					List<Dictionary<string, string>> listOfRowForOnePeptide = SpecialFunctions.GetValueOrDefault(causeAssignmentTable, peptide);
					listOfRowForOnePeptide.Add(row);
				}
			}
 
			return causeAssignmentTable; 
		}
 
 		//private Dictionary<string, bool> GetAList(Dictionary<string, string> row)
 		//{
		//    Dictionary<string, bool> alist = new Dictionary<string, bool>();
 		//    string alistValue = row["HLA In Fixed List"];
		//    foreach (string hla in alistValue.Split(' '))
		//    { 
		//        Debug.Assert((hla.Length == 3 && !hla.StartsWith("B15")) || (hla.Length == 5 && hla.StartsWith("B15"))); 
		//        alist.Add(hla, true);
		//    } 
 		//    return alist;
 		//}

        public static HlaAssignmentWithResponses CreateHlaAssignment(List<Dictionary<string, string>> rowsOfThisPeptide, IEnumerable<Hla> hlaList, HlaResolution hlaResolution, Quickscore<Hla, int> quickscore, List<int> patientsWhoRespond)
		{
 			//!!!this code is repeated elsewhere 
            Dictionary<Hla, List<int>> hlaToRespondingPatientsUnfiltered = quickscore.CreateCauseToSubsetOfEffects(patientsWhoRespond); 
            Dictionary<Hla, List<int>> hlaToRespondingPatients = Filter9xs(hlaToRespondingPatientsUnfiltered);
            List<Hla> hlaListFromRepondingPatients = new List<Hla>(hlaToRespondingPatients.Keys); 

            Dictionary<Hla, bool> hlaAssignmentDicationary = new Dictionary<Hla, bool>();
			foreach (Dictionary<string, string> causeAssignmentRow in rowsOfThisPeptide)
			{
                Hla hla = GetHlaFromRow(causeAssignmentRow, hlaResolution);
				hlaAssignmentDicationary[hla] = true; 
			} 

			HlaAssignmentWithResponses hlaAssignment = HlaAssignmentWithResponses.GetInstance(hlaAssignmentDicationary.Keys, quickscore, hlaListFromRepondingPatients, 
 				hlaToRespondingPatients);
 			return hlaAssignment;
		}

 		//!!!Yikes tons of repeated code and stuff re-run
        internal static HlaAssignmentWithResponses CreateHlaAssignment(Quickscore<Hla, int> quickscore, string hlaListAsString, ICollection<int> patientsWhoRespond) 
		{ 
            Qmrr.HlaFactory hlaFactory = Qmrr.HlaFactory.GetFactory("noConstraint");
 
			//!!!this code is repeated elsewhere
			Dictionary<Hla, List<int>> hlaToRespondingPatientsUnfiltered = quickscore.CreateCauseToSubsetOfEffects(patientsWhoRespond);
			Dictionary<Hla, List<int>> hlaToRespondingPatients = Filter9xs(hlaToRespondingPatientsUnfiltered);
			List<Hla> hlaListFromRepondingPatients = new List<Hla>(hlaToRespondingPatients.Keys);

 			Dictionary<Hla, bool> hlaAssignmentDicationary = new Dictionary<Hla, bool>(); 
 			foreach(string hlaName in hlaListAsString.Split(';')) 
			{
                Hla hla = hlaFactory.GetGroundInstance(hlaName); 
 				hlaAssignmentDicationary[hla] = true;
			}

			List<int> indexCollection = new List<int>();
			for (int hlaIndex = 0; hlaIndex < hlaListFromRepondingPatients.Count; ++hlaIndex)
			{ 
				Hla hla = hlaListFromRepondingPatients[hlaIndex]; 
 				if (hlaAssignmentDicationary.ContainsKey(hla))
 				{ 
					indexCollection.Add(hlaIndex);
 				}
			}

			HlaAssignmentWithResponses hlaAssignment = HlaAssignmentWithResponses.GetInstance(quickscore, hlaListFromRepondingPatients,
				indexCollection, hlaToRespondingPatients); 
			return hlaAssignment; 
		}
 


 		//!!!Yikes tons of repeated code and stuff re-run
        public static HlaAssignmentWithResponses CreateHlaAssignment(List<Dictionary<string, string>> rowsOfThisPeptide, IEnumerable<Hla> hlaList, HlaResolution hlaResolution, Quickscore<Hla, int> quickscore, List<int> patientsWhoRespond, Hla hlaToReplace, Hla hlaWithWhichToReplaceItOrNull)
 		{
			//!!!this code is repeated elsewhere 
 			Dictionary<Hla, List<int>> hlaToRespondingPatientsUnfiltered = quickscore.CreateCauseToSubsetOfEffects(patientsWhoRespond); 
            Dictionary<Hla, List<int>> hlaToRespondingPatients = Filter9xs(hlaToRespondingPatientsUnfiltered);
            List<Hla> hlaListFromRepondingPatients = new List<Hla>(hlaToRespondingPatients.Keys); 

            Dictionary<Hla, bool> hlaAssignmentDicationary = new Dictionary<Hla, bool>();
            foreach (Dictionary<string, string> causeAssignmentRow in rowsOfThisPeptide)
			{
                Hla hla = GetHlaFromRow(causeAssignmentRow, hlaResolution);
				hlaAssignmentDicationary[hla] = true; 
			} 
			hlaAssignmentDicationary.Remove(hlaToReplace);
			if (hlaWithWhichToReplaceItOrNull != null) 
 			{
 				hlaAssignmentDicationary.Add(hlaWithWhichToReplaceItOrNull, true);
			}

 			List<int> indexCollection = new List<int>();
			for (int hlaIndex = 0; hlaIndex < hlaListFromRepondingPatients.Count; ++hlaIndex) 
			{ 
                Hla hla = hlaListFromRepondingPatients[hlaIndex];
				if (hlaAssignmentDicationary.ContainsKey(hla)) 
				{
					indexCollection.Add(hlaIndex);
 				}
 			}

			HlaAssignmentWithResponses hlaAssignment = HlaAssignmentWithResponses.GetInstance(quickscore, hlaListFromRepondingPatients, 
 				indexCollection, hlaToRespondingPatients); 
			return hlaAssignment;
		} 


		private static Hla GetHlaFromRow(Dictionary<string, string> causeAssignmentRow, HlaResolution hlaResolution)
		{
            Qmrr.HlaFactory hlaFactory = Qmrr.HlaFactory.GetFactory("noConstraint");
 
			string hla = causeAssignmentRow["HLA"]; 
 			if (hla.Length == 4 || hla.Length == 2) //!!!should this be moved to GetHlaLengthInstance so doesn't appear twice?
 			{ 
				hla = hla.Substring(0, 1) + "0" + hla.Substring(1);
 			}

			HlaToLength hlaToLength = hlaResolution.GetHlaLengthInstance(hla);
			SpecialFunctions.CheckCondition(hlaToLength != null);
            return hlaFactory.GetGroundInstance(hlaToLength.ToString()); 
		} 

		//private static bool IsTurnedOn(Dictionary<string, string> causeAssignmentRow) 
		//{
 		//    string isTurnedOnColumn = "isKnown";
 		//    if (!causeAssignmentRow.ContainsKey(isTurnedOnColumn))
		//    {
 		//        isTurnedOnColumn = "Turned On";
		//    } 
		//    string turnedOnString = causeAssignmentRow[isTurnedOnColumn]; 
		//    Debug.Assert(turnedOnString == "0" || turnedOnString == "1");
		//    bool isTurnedOn = (turnedOnString == "1"); 
		//    return isTurnedOn;
 		//}

 		//!!!combine with IsTurnedOn
		private static bool IsKnown(Dictionary<string, string> causeAssignmentRow)
 		{ 
			string s = causeAssignmentRow["isKnown"]; 
			Debug.Assert(s == "0" || s == "1");
			bool b = (s == "1"); 
			return b;
		}

 		public static void FindPatientsWhoRespondAndWhoDoNot(IList<int> patientList, List<Dictionary<string, string>> tableByPeptide, out List<int> patientsWhoRespond, out List<int> patientsWhoDoNotRespond)
 		{
			Dictionary<int, bool> patientsWhoRespondDictionary = new Dictionary<int, bool>(); 
 			foreach (Dictionary<string, string> row in tableByPeptide) 
			{
				int patient = HlaAssignmentParams.GetPatient(row); 
				patientsWhoRespondDictionary[patient] = true;
			}

			patientsWhoDoNotRespond = new List<int>();
 			foreach (int patient in patientList)
 			{ 
				if (!patientsWhoRespondDictionary.ContainsKey(patient)) 
 				{
					patientsWhoDoNotRespond.Add(patient); 
				}
			}
			patientsWhoRespond = new List<int>(patientsWhoRespondDictionary.Keys);
		}

        public static void BioQuickTestInternal(Quickscore<Hla, int> quickscore, string fileName, string header) 
 		{ 
 			List<int> patientList = quickscore.EffectList();
 
            List<QmrJob<Hla, int>> jobList = new List<QmrJob<Hla, int>>();

			foreach (List<Dictionary<string, string>> tableByPeptide in HlaAssignmentParams.QuickScoreOptimalsGroupByPeptide(fileName, header))
 			{
				Debug.Assert(tableByPeptide.Count > 0); // real assert
				string peptide = tableByPeptide[0]["peptide"]; 
 

 
				List<int> patientsWhoDoNotRespond;
				List<int> patientsWhoRespond;
				QmrAlgorithms.FindPatientsWhoRespondAndWhoDoNot(patientList, tableByPeptide, out patientsWhoRespond, out patientsWhoDoNotRespond);

                QmrJob<Hla, int> aQuickScoreJob = QmrJob<Hla, int>.GetInstance(peptide, patientsWhoRespond, patientsWhoDoNotRespond, quickscore);
 				jobList.Add(aQuickScoreJob); 
 			} 

 
            jobList.Sort(delegate(QmrJob<Hla, int> x, QmrJob<Hla, int> y) { return x.PresentEffectCollection.Count.CompareTo(y.PresentEffectCollection.Count); });
			foreach (QmrJob<Hla, int> job in jobList)
 			{
				Debug.WriteLine(job);
			}
 
 
            foreach (QmrJob<Hla, int> job in jobList)
			{ 
				//if (job.Name != "RIRTWKSLVK")
				//{
 				//    continue;
 				//}
				Console.WriteLine(job);
 
 				//job.Quickscore.Probability("B58", job.PresentEffectCollection, job.AbsentEffectCollection); 

				Stopwatch stopwatch = new Stopwatch(); 
				stopwatch.Start();
                Dictionary<Hla, double> posteriorOfEveryCause = job.PosteriorOfEveryCause();
				stopwatch.Stop();

				Console.WriteLine("{0}\t{1}\t{2}\t{3}", job.Name, job.PresentEffectCollection.Count, job.AbsentEffectCollection.Count, stopwatch.Elapsed);
 
				foreach (KeyValuePair<Hla, double> causeAndPosterior in posteriorOfEveryCause) 
 				{
 					Debug.Assert(0 <= causeAndPosterior.Value && causeAndPosterior.Value <= 1); 
					Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", job.Name, job.PresentEffectCollection.Count, job.AbsentEffectCollection.Count, stopwatch.Elapsed
 						, causeAndPosterior.Key, causeAndPosterior.Value);
				}
			}
		}
 
        public static Dictionary<bool, List<KeyValuePair<double, double>>> CreateListsWithWeights(HlaResolution hlaResolution, List<Dictionary<string, string>> expandedTable, Dictionary<string, double> logViralLoadTable, Hla hla) 
		{
			Dictionary<bool, List<KeyValuePair<double, double>>> patientListList = new Dictionary<bool, List<KeyValuePair<double, double>>>(); 
 			patientListList[false] = new List<KeyValuePair<double, double>>();
 			patientListList[true] = new List<KeyValuePair<double, double>>();

            foreach (Dictionary<string, string> row in expandedTable)
			{
 				bool? hasHlaOrNull = QmrAlgorithms.HasHla(hla, row, hlaResolution); 
				if (hasHlaOrNull != null) 
				{
					double weight = row.ContainsKey("weight")?double.Parse(row["weight"]):1.0; 
					string patient = row["patient"];
					if (logViralLoadTable.ContainsKey(patient))
 					{
 						double logViralLoad = logViralLoadTable[patient];
						KeyValuePair<double, double> weightAndLogViralLoad = new KeyValuePair<double, double>(weight, logViralLoad);
 						patientListList[(bool)hasHlaOrNull].Add(weightAndLogViralLoad); 
					} 
					else
					{ 
						Debug.WriteLine(string.Format("Can't find patient '{0}' in viralload list", patient));
					}
 				}
 			}
			return patientListList;
 		} 
 
        public static Dictionary<string, Dictionary<Hla, bool>> CreatePatientToHlaToYesNoDontKnow(HlaResolution hlaResolution, List<Dictionary<string, string>> expandedTable, string header, IEnumerable<Hla> hlaList)
		{ 
            Dictionary<string, Dictionary<Hla, bool>> patientToHlaToYesNoDontKnow = new Dictionary<string, Dictionary<Hla, bool>>();

            foreach (Hla hla in hlaList)
			{
                Dictionary<string, Set<bool>> patientToSetOfHasHlaValues = new Dictionary<string, Set<bool>>();
 
                foreach (Dictionary<string, string> row in expandedTable) 
				{
					bool? hasHlaOrNull = HasHla(hla, row, hlaResolution); 
					if (hasHlaOrNull != null)
 					{
 						string patient = row["patient"];
                        Set<bool> setOfHasHlaValues = SpecialFunctions.GetValueOrDefault(patientToSetOfHasHlaValues, patient);
                        setOfHasHlaValues.AddNewOrOld((bool)hasHlaOrNull);
					} 
 					else 
					{
						SpecialFunctions.CheckCondition(!row.ContainsKey("weight") || double.Parse(row["weight"]) == 1); 
					}
				}

				foreach (string patient in patientToSetOfHasHlaValues.Keys)
 				{
                    Set<bool> setOfHasHlaValues = patientToSetOfHasHlaValues[patient]; 
 					if (setOfHasHlaValues.Count == 1) 
					{
 						foreach (bool hasHlaOrNull in setOfHasHlaValues) 
						{
                            Dictionary<Hla, bool> hlaToYesNoDontKnow = SpecialFunctions.GetValueOrDefault(patientToHlaToYesNoDontKnow, patient);
							hlaToYesNoDontKnow.Add(hla, (bool)hasHlaOrNull);
						}
					}
					else 
 					{ 
 						//Debug.WriteLine(string.Format("For patient {0} and hla {1}, skipping because of ambiguious data", patient, hla));
					} 
 				}
			}
			return patientToHlaToYesNoDontKnow;
		}

 
		//!!!similar code to CreateListsWithWeights 
        public static Dictionary<bool, List<double>> CreateListsWithoutWeights(Dictionary<string, Dictionary<Hla, bool>> patientToHlaToYesNoDontKnow, Dictionary<string, double> logViralLoadTable, Hla hla)
		{ 

 			Dictionary<bool, List<double>> patientListList = new Dictionary<bool, List<double>>();
 			patientListList[false] = new List<double>();
			patientListList[true] = new List<double>();
 			foreach (string patient in patientToHlaToYesNoDontKnow.Keys)
			{ 
                Dictionary<Hla, bool> hlaToYesNoDontKnow = patientToHlaToYesNoDontKnow[patient]; 
				if (hlaToYesNoDontKnow.ContainsKey(hla))
				{ 
					patientListList[hlaToYesNoDontKnow[hla]].Add(logViralLoadTable[patient]);
				}
 			}

 			return patientListList;
		} 
 
 		public static double GetViralLoad(Dictionary<string, double> logViralLoadTable, Dictionary<string, string> row)
		{ 
			string patient = row["patient"];
			SpecialFunctions.CheckCondition(logViralLoadTable.ContainsKey(patient));
			double logViralLoad = logViralLoadTable[patient];

			return logViralLoad;
 		} 
 
 		static private void GenerateSyntheticResponses(Dictionary<int, double> effectDistribution, Random random,
			out List<int> patientsWhoRespond, out List<int> patientsWhoDoNotRespond) 
 		{
			patientsWhoRespond = new List<int>();
			patientsWhoDoNotRespond = new List<int>();

			foreach (KeyValuePair<int, double> effectAndProbability in effectDistribution)
			{ 
				if (random.NextDouble() < effectAndProbability.Value) 
 				{
 					patientsWhoRespond.Add(effectAndProbability.Key); 
				}
 				else
				{
					patientsWhoDoNotRespond.Add(effectAndProbability.Key);

				} 
			} 
		}
 
 		static private List<string> HlasWithGoalSetting(Dictionary<string, bool> hlaAssignment, bool goalSetting)
 		{
			List<string> hlasWithGoalSetting = new List<string>();
 			foreach (KeyValuePair<string, bool> hlaAndSetting in hlaAssignment)
			{
				if (hlaAndSetting.Value == goalSetting) 
				{ 
					hlasWithGoalSetting.Add(hlaAndSetting.Key);
				} 
 			}

 			return hlasWithGoalSetting;
		}

 
 		public static void SearchHlaAssignmentsRelaxation(string fileName, string header, string solutionFileName, string solutionHeader, double causePrior, double linkProbability, double leakProbability, int howManyBest, string outputFilename, HlaResolution hlaResolution) 
		{
			Debug.Assert(Math.Exp(double.NegativeInfinity) == 0); 

			//Create the structure of patients and their HLAs
            Quickscore<Hla, int> quickscore = HlaAssignmentParams.CreateQuickscore(fileName, header, causePrior, linkProbability, leakProbability, hlaResolution);

			// Get the list of patients, Hlas, and patient weights
			List<int> patientList = quickscore.EffectList(); 
            IEnumerable<Hla> hlaList = quickscore.CauseList(); 
 			//The structure file can contain a "weight" column. If it does, weight the patients, otherwise give them all weight 1.0
 			Dictionary<int, double> patientWeightTable = HlaAssignmentParams.CreatePatientWeightTable(fileName, header); 

			//The cause assignment table is a mapping from
 			//    peptide to HLA assignments for that peptide
			Dictionary<string, List<Dictionary<string, string>>> causeAssignmentTable = CreateCauseAssignmentTable(solutionFileName, solutionHeader);

			//We start the report 
			using (StreamWriter streamwriterOutputFile = File.CreateText(outputFilename)) 
			{
 
				//The report is the same as the input "solution" except that we now list new HLA, a note, and a p(assignment)
 				streamwriterOutputFile.WriteLine(SpecialFunctions.CreateTabString(solutionHeader,
 					"newHLA", "rank", "p(assignment)", "note", "Peptide", "LogLikelihood", "TrueHlas.Count", "TrueHlas", "TrueHlasAndRespondingPatients", "UnexplainedPatients.Count"));

				//Now we go through the model file, one peptide at a time. For each peptide, we know the patientID (did) of
 				// every patient who responded. (It also tells the HLAs of the patient, but we already have that info in the structure) 
				/* 
				peptide	did	a1	a2	b1	b2	c1	c2
				 
				ACQGVGGPGHK	10	2	11	38	44	12	16
				ACQGVGGPGHK	14	2	24	1517	58	3	7
 				ACQGVGGPGHK	41	11	33	35	40	6	7
 				ACQGVGGPGHK	102	2	11	35	44	4	5
				
 				AENLWVTVY	25	24	66	35	39	4	12 
				AENLWVTVY	36	23	32	8	44	7	7 
				AENLWVTVY	45	3	31	39	44	5	12
				AENLWVTVY	46	2	29	41	52	16	16 
				AENLWVTVY	59	2	33	7	44	3	15
				
 				 [...]
 				 */

				foreach (List<Dictionary<string, string>> tableByPeptide in HlaAssignmentParams.QuickScoreOptimalsGroupByPeptide(fileName, header)) 
 				{ 
					Debug.Assert(tableByPeptide.Count > 0); // real assert
					string peptide = tableByPeptide[0]["peptide"]; 


					//Create lists of patients who responded and assume everyone else didn't.
					List<int> patientsWhoRespond;
					List<int> patientsWhoDoNotRespond;
 					FindPatientsWhoRespondAndWhoDoNot(patientList, tableByPeptide, out patientsWhoRespond, out patientsWhoDoNotRespond); 
 
 					//If we get a peptide with no row in the solution file, skip it.
					if (!causeAssignmentTable.ContainsKey(peptide)) 
 					{
						streamwriterOutputFile.WriteLine("{0}\t\t{1}", peptide, "explained by noise");
						continue;
					}

					//We assign every HLA mentioned by the solution file with this peptide to TRUE and all others to FALSE 
					//An assignment of TRUE means that this HLA is a cause of this response. 
 					List<Dictionary<string, string>> rowsOfThisPeptide = causeAssignmentTable[peptide];
 					HlaAssignmentWithResponses hlaAssignmentBase = CreateHlaAssignment(rowsOfThisPeptide, hlaList, hlaResolution, quickscore, patientsWhoRespond); 

					//Find the likelihood of this structure, with these patient responses, and the solution's HLA assignment.
 					double baseLogLikelihood = quickscore.LogLikelihoodOfModelWithCompleteAssignments(patientsWhoRespond, patientsWhoDoNotRespond, hlaAssignmentBase.AsDictionary, patientWeightTable);


					//Consider each HLA assigned to TRUE (for this peptide) by the solution 
					foreach (Dictionary<string, string> row in rowsOfThisPeptide) 
					{
                        Hla hla = GetHlaFromRow(row, hlaResolution); 

						//If we already really know that this HLA is a cause of this reponse, just report that.
						if (IsKnown(row))
 						{
 							streamwriterOutputFile.WriteLine("{0}\t{1}\t{2}", row[""], hla, "known");
							continue; 
 						} 

						//Set it to false as a way to see measure the probability that it is true. 
						HlaAssignmentWithResponses hlaAssignmentL0 = CreateHlaAssignment(rowsOfThisPeptide, hlaList, hlaResolution, quickscore, patientsWhoRespond, hla, null);
						double logL0 = quickscore.LogLikelihoodOfModelWithCompleteAssignments(patientsWhoRespond, patientsWhoDoNotRespond, hlaAssignmentL0.AsDictionary, patientWeightTable);
						Debug.Assert(hlaAssignmentL0.TrueCount + 1 == hlaAssignmentBase.TrueCount); // real assert
						string noteNote = "";
 						if (logL0 > baseLogLikelihood)
 						{ 
							noteNote = string.Format("\tlogL0 > baseLogLikelihood ({0}>{1})", logL0, baseLogLikelihood); 
 						}
						double probability = Math.Exp(baseLogLikelihood - SpecialFunctions.LogSum(baseLogLikelihood, logL0)); 
						streamwriterOutputFile.WriteLine(SpecialFunctions.CreateTabString(
							row[""], hla, "1 best", probability, noteNote,
							peptide, baseLogLikelihood, hlaAssignmentBase.TrueCount, hlaAssignmentBase.TrueToString(), hlaAssignmentBase.TrueToListString(), hlaAssignmentBase.UnexplainedPatients.Count));
						streamwriterOutputFile.WriteLine(SpecialFunctions.CreateTabString(
 							row[""], hla, "remove 1 best", "", noteNote,
 							peptide, logL0, hlaAssignmentL0.TrueCount, hlaAssignmentL0.TrueToString(), hlaAssignmentL0.TrueToListString(), hlaAssignmentL0.UnexplainedPatients.Count)); 
 
						//Also, while we're in the world were it is false, let's measure how well each of the currently-set-to-false HLAs would do instead.
 						//!!!should this be one dictionary to a new class instead of three dictionaries? 
                        List<KeyValuePair<Hla, double>> listOfhla1AndProbability1 = new List<KeyValuePair<Hla, double>>();
                        Dictionary<Hla, double> loglikelihoodCollection = new Dictionary<Hla, double>();
                        Dictionary<Hla, HlaAssignmentWithResponses> hla1ToHlaAssignment = new Dictionary<Hla, HlaAssignmentWithResponses>();
						foreach (Hla hla1 in hlaList)
						{
							if (hla1 == hla || hlaAssignmentBase.AsDictionary[hla1]) 
							{ 
								continue;
 							} 

 							HlaAssignmentWithResponses hlaAssignmentL1 = CreateHlaAssignment(rowsOfThisPeptide, hlaList, hlaResolution, quickscore, patientsWhoRespond, hla, hla1);
							if (hlaAssignmentL1.TrueCount == hlaAssignmentBase.TrueCount)
 							{
								double logL1 = quickscore.LogLikelihoodOfModelWithCompleteAssignments(patientsWhoRespond, patientsWhoDoNotRespond, hlaAssignmentL1.AsDictionary, patientWeightTable);
 
								double probability1 = Math.Exp(logL1 - SpecialFunctions.LogSum(logL1, logL0)); 
								loglikelihoodCollection[hla1] = logL1;
								hla1ToHlaAssignment.Add(hla1, hlaAssignmentL1); 

                                listOfhla1AndProbability1.Add(new KeyValuePair<Hla, double>(hla1, probability1));
							}

 						}
 
 						//Report on the best of these 
                        listOfhla1AndProbability1.Sort(delegate(KeyValuePair<Hla, double> x, KeyValuePair<Hla, double> y) { return y.Value.CompareTo(x.Value); });
						for (int i = 0; i < howManyBest - 1; ++i) 
 						{
                            Hla hla1 = listOfhla1AndProbability1[i].Key;
							streamwriterOutputFile.WriteLine(SpecialFunctions.CreateTabString(
								row[""], hla1, string.Format("{0} best", i + 2), listOfhla1AndProbability1[i].Value,
								"", peptide, loglikelihoodCollection[hla1], hla1ToHlaAssignment[hla1].TrueCount, hla1ToHlaAssignment[hla1].TrueToString(), hla1ToHlaAssignment[hla1].TrueToListString(), hla1ToHlaAssignment[hla1].UnexplainedPatients.Count));
						} 
					} 
 				}
 			} 
		}



 		static Regex nineXs = new Regex("(^[B][0-9][0-9]9[0-9]$)|(^[ABC]9[0-9]$)");
        public static Dictionary<Hla, List<int>> Filter9xs(Dictionary<Hla, List<int>> hlaToRespondingPatientsUnfiltered) 
		{ 
            Debug.Fail("Confirm that is function is still useful after the 'string'->Qmrr.String conversion");
            Dictionary<Hla, List<int>> filtered = new Dictionary<Hla, List<int>>(); 
            foreach (KeyValuePair<Hla, List<int>> hlaAndPatientList in hlaToRespondingPatientsUnfiltered)
			{
                Hla hla = hlaAndPatientList.Key;
				if (!nineXs.IsMatch(hla.ToString()))
				{
					filtered.Add(hla, hlaAndPatientList.Value); 
 				} 
 			}
			return filtered; 
 		}
	}
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
