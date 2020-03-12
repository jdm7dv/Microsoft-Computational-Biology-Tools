using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Msr.Mlas.SpecialFunctions;
 
namespace VirusCount 
{
 	public class Patch : Combinable 
	{
		static public Patch GetInstance(PatchPattern patchPattern, double weight)
		{
			Patch aPatch = new Patch();
			aPatch._patchPattern = patchPattern;
 			aPatch.Weight = weight; 
 			return aPatch; 
		}
 
 		override public void RemoveMeFromTablesAndUpdate(Greedy greedy, Component component)
		{
			greedy.VaccineAsString.SetComponents(greedy.Components);
			greedy.RemovePatchFromTables(this);
			greedy.UpdateComponentTables(component);
		} 
 

 		override public Move GetAppend() 
 		{
			Move bestMove = new AppendPatch(this);
 			return bestMove;
		}

		public void IncreaseWeight(double weightIncrease) 
		{ 
			Weight += weightIncrease;
		} 

 		PatchPattern _patchPattern;
 		override public PatchPattern PatchPattern()
		{
 			return _patchPattern;
		} 
 

		private Patch() 
		{
		}

	}

 	public class PatchTable 
 	{ 
		public int Count
 		{ 
			get
			{
				return PatchPatternToPatch.Count;
			}
		}
 		public Dictionary<PatchPattern, Patch> PatchPatternToPatch = new Dictionary<PatchPattern, Patch>(); 
 
        static public PatchTable GetInstance(Dictionary<string, double> patchToWeight, string protein, string vaccineCriteriaName)
        { 
            PatchTable patchTable = new PatchTable();
            patchTable.PatchTableScorer = PatchTableScorer.GetInstance("normal");

            PatchPatternFactory patchPatternFactory = PatchPatternFactory.GetFactory("strings");
            patchTable.PatchPatternToPatch = new Dictionary<PatchPattern, Patch>();
            foreach (KeyValuePair<string, double> patchAndWeight in patchToWeight) 
            { 
                string patchAsString = patchAndWeight.Key;
                double weight = patchAndWeight.Value; 
                PatchPattern patchPattern = patchPatternFactory.GetInstance(patchAsString);
                Patch aPatch = Patch.GetInstance(patchPattern, weight);
                patchTable.Add(aPatch);
            }

            patchTable.CreateSortedArrays(); 
 
            return patchTable;
        } 



 		private double WeightSoFar()
		{
 			double r = 0.0; 
			foreach (Patch aPatch in PatchPatternToPatch.Values) 
			{
				r += aPatch.Weight; 
			}
			return r;
 		}


 		private void SetMaxPatchLength(PatchPattern patchPattern) 
		{ 
 			MaxPatchLength = Math.Max(MaxPatchLength, patchPattern.MaxLength);
		} 

		private void AddPatchToCollection(double weightPerPatient, PatchPattern patchPattern, int iCount, int iProtectable)
		{
			double weightIncrease = weightPerPatient * (double)iCount / (double)iProtectable;
			Patch aPatch = GetPatchFromCollection(patchPattern);
 			aPatch.IncreaseWeight(weightIncrease); 
 		} 

 
		private Patch GetPatchFromCollection(PatchPattern patchPattern)
 		{
			if (PatchPatternToPatch.ContainsKey(patchPattern))
			{
				return (Patch)PatchPatternToPatch[patchPattern];
			} 
			else 
 			{
 				Patch aPatch = Patch.GetInstance(patchPattern, 0.0); 
				PatchPatternToPatch.Add(patchPattern, aPatch);
 				return aPatch;
			}

		}
 
        static public PatchTable GetInstanceFromFile(PatchPatternFactory patchPatternFactory, 
            TextReader patchTableTextReader, string scorerName)
		{ 
			PatchTable patchTable = new PatchTable();
            patchTable.PatchTableScorer = PatchTableScorer.GetInstance(scorerName);

            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(patchTableTextReader, "Patch\tWeight", false))
            {
                PatchPattern patchPattern = patchPatternFactory.GetInstance(row["Patch"]); 
                double weight = double.Parse(row["Weight"]); 
                patchTable.SetMaxPatchLength(patchPattern);
                Patch patch = Patch.GetInstance(patchPattern, weight); 
                patchTable.Add(patch);
            }

			patchTable.CreateSortedArrays();

 			return patchTable; 
 		} 

		private void Add(Patch aPatch) 
 		{
			PatchPattern patchPattern = aPatch.PatchPattern();
			if (PatchPatternToPatch.ContainsKey(patchPattern))
			{
				Patch previous = (Patch)PatchPatternToPatch[patchPattern];
				previous.Weight += aPatch.Weight; 
 			} 
 			else
			{ 
 				PatchPatternToPatch.Add(patchPattern, aPatch);
			}
		}

		public void Display(StreamWriter streamwriterOutputFile)
		{ 
			foreach (Patch aPatch in SortedPatchCollection) 
 			{
 				streamwriterOutputFile.WriteLine("{0}\t{1}", aPatch.PatchPattern(), aPatch.Weight); 
			}
 		}

		static private string HeaderString = string.Format("{0}\t{1}", "Patch", "Weight");

		static public void DisplayHeader(StreamWriter streamwriterOutputFile) 
		{ 
			streamwriterOutputFile.WriteLine(HeaderString);
		} 

 		public Patch[] SortedPatchCollection;
 		public int MaxPatchLength = int.MinValue;

		public void CreateSortedArrays()
 		{ 
			double[] rSortedWeightsCollection = new double[PatchPatternToPatch.Count]; 
			SortedPatchCollection = new Patch[PatchPatternToPatch.Count];
			int iPatch = -1; 
			foreach (Patch aPatch in PatchPatternToPatch.Values)
			{
 				++iPatch;
 				rSortedWeightsCollection[iPatch] = aPatch.Weight;
				SortedPatchCollection[iPatch] = aPatch;
 			} 
 
			Array.Sort(rSortedWeightsCollection, SortedPatchCollection);
			Array.Reverse(SortedPatchCollection); 
		}




		internal static PatchTable GetInstanceWithUniformWeights(VaccineMaker vaccineMaker) 
		{ 
 			SpecialFunctions.CheckCondition(vaccineMaker is Greedy);
 			Greedy greedy = (Greedy) vaccineMaker; 

			return GetInstanceWithUniformWeightByLength(PatchPatternCollection(greedy));
 		}

        internal static PatchTable GetInstanceWithUniformWeightByLength(IList<PatchPattern> patchPatternCollection)
        { 
            PatchTable patchTable = new PatchTable(); 
            patchTable.PatchTableScorer = PatchTableScorer.GetInstance("normal");
 
            int totalLength = 0;
            foreach (PatchPattern patchPattern in patchPatternCollection)
            {
                totalLength += patchPattern.FullLength;
            }
 
 
            foreach (PatchPattern patchPattern in patchPatternCollection)
            { 
                double weight = (double)patchPattern.FullLength / (double)totalLength;

                patchTable.SetMaxPatchLength(patchPattern);

                Patch aPatch = Patch.GetInstance(patchPattern, weight);
                patchTable.Add(aPatch); 
                patchTable.CreateSortedArrays(); 
            }
 
            return patchTable;
        }

		private static List<PatchPattern> PatchPatternCollection(Greedy greedy)
		{
			List<PatchPattern> rg = new List<PatchPattern>(); 
			foreach (Component component in greedy.Components) 
			{
 				PatchPattern patchPattern = component.PatchPattern(); 
 				rg.Add(patchPattern);
			}
 			return rg;
		}

		internal static PatchTable GetInstanceWithUniformWeightByLength(VaccineAsString vaccineUncompressed) 
		{ 
			return GetInstanceWithUniformWeightByLength(PatchPatternCollection(vaccineUncompressed));
		} 

 		private static List<PatchPattern> PatchPatternCollection(VaccineAsString vaccineUncompressed)
 		{
			List<PatchPattern> rg = new List<PatchPattern>();

 			PatchPatternFactory aPatchPatternFactory = PatchPatternFactory.GetFactory("strings"); 
			foreach(string componentAsString in vaccineUncompressed.NiceStringCollection()) 
			{
				PatchPattern patchPattern = aPatchPatternFactory.GetInstance(componentAsString); 
				rg.Add(patchPattern);
			}
 			return rg;
 		}

        private PatchTableScorer PatchTableScorer; 
        internal double DupScore(VaccineAsString vaccine) 
		{
            return PatchTableScorer.DupScore(PatchPatternToPatch, vaccine); 
 		}

        public double Score(VaccineAsString vaccineAsString)
        {
            return PatchTableScorer.Score(PatchPatternToPatch, vaccineAsString);
        } 
 

        internal void Normalize() 
        {
			double rTotal = 0.0;
			foreach(Patch patch in SortedPatchCollection)
			{
				rTotal += patch.Weight;
			} 
            foreach (Patch patch in SortedPatchCollection) 
            {
                patch.Weight /= rTotal; 
            }
        }

    }

    public abstract class PatchTableScorer
    { 
        protected PatchTableScorer() 
        {
        } 

        public static PatchTableScorer GetInstance(string name)
        {
            if (name == "normal")
            {
                PatchTableScorer aPatchTableScorer = new NormalScorer(); 
                return aPatchTableScorer; 
            }
            SpecialFunctions.CheckCondition(false, "patch table scorer unknown: " + name); 
            return null;
        }

        public abstract double Score(Dictionary<PatchPattern, Patch> patchPaternToPatch, VaccineAsString vaccineAsString);
        public abstract double DupScore(Dictionary<PatchPattern, Patch> patchPaternToPatch, VaccineAsString vaccine);
    } 
 
    public class NormalScorer : PatchTableScorer
    { 
        internal NormalScorer()
        {
        }

        public override double Score(Dictionary<PatchPattern, Patch> patchPaternToPatch, VaccineAsString vaccineAsString)
        { 
            double score = 0.0; 
            foreach (Patch aPatch in patchPaternToPatch.Values)
            { 
                if (aPatch.PatchPattern().IsMatch(vaccineAsString.String))
                {
                    score += aPatch.Weight;
                }
            }
 
            return score; 
        }
 
        public override double DupScore(Dictionary<PatchPattern, Patch> patchPaternToPatch, VaccineAsString vaccine)
        {
            double score = 0.0;
            foreach (string component in vaccine.NiceStringCollection())
            {
                foreach (Patch aPatch in patchPaternToPatch.Values) 
                { 
                    if (aPatch.PatchPattern().IsMatch(component))
                    { 
                        score += aPatch.Weight;
                    }
                }
            }

            return score; 
        } 
    }
 

}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
