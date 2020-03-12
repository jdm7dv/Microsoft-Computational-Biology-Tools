using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using EpipredLib;

 
namespace VirusCount.Qmr 
{
    public class TrueCollection : List<Hla> 
 	{
		private TrueCollection()
		{
		}

        //internal static TrueCollection GetInstanceX(IEnumerable<string> hlaCollection, Random random) 
        //{ 
        //    TrueCollection aTrueCollection = new TrueCollection();
        //    foreach (string hla in hlaCollection) 
        //    {
        //        if (random.Next(2) == 0)
        //        {
        //            aTrueCollection.Add(hla);
        //        }
        //    } 
        //    return aTrueCollection; 
        //}
 
        public Set<Hla> CreateHlaAssignmentAsSet()
        {
            Set<Hla> hlaAssignmentAsSet = Set<Hla>.GetInstance();
            foreach (Hla hla in this)
            {
                hlaAssignmentAsSet.AddNewOrOld(hla); 
            } 
            return hlaAssignmentAsSet;
        } 

        public Dictionary<Hla, bool> CreateHlaAssignmentAsDict(IEnumerable<Hla> hlaUniverse)
		{
            Dictionary<Hla, bool> hlaAssignmentAsDict = new Dictionary<Hla, bool>();
            foreach (Hla hla in hlaUniverse)
			{ 
 				hlaAssignmentAsDict.Add(hla, false); 
 			}
            foreach (Hla hla in this) 
			{
 				hlaAssignmentAsDict[hla] = true;
			}
			return hlaAssignmentAsDict;
		}
 
		public override string  ToString() 
		{
 			StringBuilder sb = new StringBuilder(); 
            foreach (Hla hla in this)
 			{
				if (sb.Length != 0)
 				{
					sb.Append(";");
				} 
				sb.Append(hla); 
			}
			return sb.ToString(); 
 		}

        internal static TrueCollection GetInstance(params IEnumerable<Hla>[] hlaCollectionCollection)
        {
            TrueCollection aTrueCollection = new TrueCollection();
            foreach (IEnumerable<Hla> hlaCollection in hlaCollectionCollection) 
            { 
                foreach (Hla hla in hlaCollection)
                { 
                    aTrueCollection.Add(hla);
                }
            }
            return aTrueCollection;
        }
 
 		internal static TrueCollection GetInstance() 
		{
 			TrueCollection aTrueCollection = new TrueCollection(); 
			return aTrueCollection;
		}

		//#region ICloneable Members

		//object ICloneable.Clone() 
		//{ 
 		//    return TrueCollection.GetInstance(this);
 		//} 

		//#endregion

 		//internal TrueCollection Clone()
		//{
		//    return TrueCollection.GetInstance(this); 
		//} 

    } 
	public class HlaAssignmentWithResponses
	{

        internal static HlaAssignmentWithResponses GetInstance(Quickscore<Hla, int> quickscore, List<Hla> hlaListFromRepondingPatients, List<int> indexCollection,
            Dictionary<Hla, List<int>> hlaToRespondingPatients)
 		{ 
            List<Hla> trueCollection = new List<Hla>(); 
 			foreach (int index in indexCollection)
			{ 
                Hla hla = hlaListFromRepondingPatients[index];
 				trueCollection.Add(hla);
			}
			return HlaAssignmentWithResponses.GetInstance(trueCollection, quickscore, hlaListFromRepondingPatients, hlaToRespondingPatients);
		}
 
        internal static HlaAssignmentWithResponses GetInstance(IEnumerable<Hla> trueCollection, Quickscore<Hla, int> quickscore, List<Hla> hlaListFromRepondingPatients, Dictionary<Hla, List<int>> hlaToRespondingPatients) 
		{
 

			HlaAssignmentWithResponses aHlaAssignment = new HlaAssignmentWithResponses();
 			aHlaAssignment.TrueCollection = TrueCollection.GetInstance(trueCollection);

            aHlaAssignment.AsDictionary = new Dictionary<Hla, bool>();
 			aHlaAssignment.UnexplainedPatients = new List<int>(); 
			Dictionary<int, bool> patientToIsExplained = new Dictionary<int, bool>(); 
 			aHlaAssignment.HlaToRespondingPatients = hlaToRespondingPatients;
 

            foreach (Hla hla in trueCollection)
			{
				aHlaAssignment.AsDictionary.Add(hla, true);
				foreach (int patient in hlaToRespondingPatients[hla])
				{ 
					patientToIsExplained[patient] = true; 
 				}
 			} 
            foreach (Hla hla in quickscore.CauseList())
			{
 				if (!aHlaAssignment.AsDictionary.ContainsKey(hla))
				{
					aHlaAssignment.AsDictionary.Add(hla, false);
					if (hlaToRespondingPatients.ContainsKey(hla)) 
					{ 
						foreach (int patient in hlaToRespondingPatients[hla])
 						{ 
 							if (!patientToIsExplained.ContainsKey(patient))
							{
 								patientToIsExplained.Add(patient, false);
								aHlaAssignment.UnexplainedPatients.Add(patient);
							}
						} 
					} 
				}
 			} 
 			return aHlaAssignment;
		}


        Dictionary<Hla, List<int>> HlaToRespondingPatients; //only use for a version of ToString()
        public Dictionary<Hla, bool> AsDictionary; 
 		internal TrueCollection TrueCollection; 
		public int TrueCount
		{ 
			get
			{
				return TrueCollection.Count;
 			}
 		}
		public List<int> UnexplainedPatients; 
 

 		public static bool BetterAtExplainingReactions(HlaAssignmentWithResponses champ, HlaAssignmentWithResponses challenger) 
		{
			if (challenger.UnexplainedPatients.Count < champ.UnexplainedPatients.Count)
			{
				return true;
			}
 			if (champ.UnexplainedPatients.Count < challenger.UnexplainedPatients.Count) 
 			{ 
				return false;
 			} 

			return challenger.TrueCount < champ.TrueCount;

		}

 
		internal string TrueToString() 
		{
			return TrueCollection.ToString(); 
 		}


 		internal object TrueToListString()
		{
 			StringBuilder sb = new StringBuilder(); 
            foreach (Hla hla in TrueCollection) 
			{
				if (sb.Length != 0) 
				{
					sb.Append(";");
				}
 				sb.AppendFormat("{0}(",hla);
 				foreach (int patient in HlaToRespondingPatients[hla])
				{ 
 					sb.AppendFormat("{0} ", patient); 
				}
				sb.Append(")"); 
			}
			return sb.ToString();
		}

 	}
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
