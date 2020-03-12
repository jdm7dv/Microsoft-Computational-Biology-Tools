using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Collections.Generic; 
using System.Linq; 

namespace VirusCount 
{

 	abstract public class VaccineMaker
	{

		static public VaccineMaker GetInstance(string name) 
		{ 
            SpecialFunctions.CheckCondition(name == "Greedy", "VaccineMaker must be 'Greedy'");
            VaccineMaker aVaccineMaker = new Greedy(); 
            aVaccineMaker.Name = name;
            return aVaccineMaker;
		}

		public string Name;
 
 		protected PatchTable PatchTable; 
 		public VaccineAsString VaccineAsString;
 
		abstract public void FirstVaccine(PatchTable patchTable);


 		public void DisplayHeader(TextWriter streamwriterOutputFile)
		{
            streamwriterOutputFile.WriteLine(DisplayHeaderString); 
		}
        static public string DisplayHeaderString = SpecialFunctions.CreateTabString("Method", "AminoAcidLength", "numComponents", "coverage", "Vaccine");

		public void Display(TextWriter streamwriterOutputFile, double rScoreOpt) 
		{
            streamwriterOutputFile.WriteLine(DisplayString(rScoreOpt));
		}

        public string DisplayString(double scoreOpt)
        {
            return SpecialFunctions.CreateTabString(Name, VaccineAsString.TotalNumberOfAminoAcids, VaccineAsString.NumberOfComponents, scoreOpt, VaccineAsString.NiceString());
        }

 		abstract public void ChangeToNext();
 
 		protected VaccineMaker() 
		{
 		} 

    }



	public class Greedy : VaccineMaker 
	{ 
		public double PointsInPatchTable = double.NaN;
		override public void FirstVaccine(PatchTable patchTable) 
		{
 			PatchTable = patchTable;
 			VaccineAsString = VaccineAsString.GetInstance(patchTable.MaxPatchLength, Name);

			PointsInPatchTable = 0.0;
 			foreach(Patch aPatch in patchTable.SortedPatchCollection) 
			{ 
				PointsInPatchTable += aPatch.Weight;
				UnusedPatches.AddNew(aPatch); 
			}

			Patch patchBest = patchTable.SortedPatchCollection[0];
 			MakePatchAComponent(patchBest);
 		}
 
		override public void ChangeToNext() 
 		{
			Move bestMove = FindBestPatchMoveOrNull(); 
			Move bestComponentMove = FindBestComponentMoveOrNull();
			bestMove = Move.MaxOrNull(bestMove, bestComponentMove);


			if (bestMove == null)
			{ 
 				EndItAll(); 
 			}
			else 
 			{
				MakeCheck(bestMove);
				MakeMove(bestMove);
			}
		}
 
		private double RealScore() 
 		{
 			double r = PatchTable.Score(VaccineAsString); 
			return r;
 		}

		private double SelfScore()
		{
			//Debug.WriteLine(string.Format("\tSelf Scoring")); 
			double r = 0.0; 
			foreach(Component aComponent in Components)
 			{ 
 				//Debug.WriteLine(string.Format("\t\t{0}\t{1}", aComponent.PatchPattern().ToString(), aComponent.Weight));
				r += aComponent.Weight;
 			}
			return r;
		}
 
		private double UnusedScore() 
		{
			//Debug.WriteLine("\tUnused patches"); 
 			double r = 0.0;
 			foreach(Patch aPatch in UnusedPatches)
			{
 				//Debug.WriteLine(string.Format("\t\t{0}\t{1}", aPatch.PatchPattern().ToString(), aPatch.Weight));
				r += aPatch.Weight;
			} 
			return r; 
		}
 
		private void MakeCheck(Move bestMove)
 		{
 			//Debug.WriteLine(string.Format("\tVaccine\t{0}", VaccineAsString.String));

			double rSelfScore = SelfScore();
 			double rRealScore = RealScore(); 
 
			CompareZone zone = Move.Zone(bestMove);
 
			Debug.Assert(rSelfScore <= rRealScore + 1e-10);
			if (zone == CompareZone.LengthIncreases)
			{
				double rMissing = rRealScore - rSelfScore;
 				if (rMissing > 1e-10)
 				{ 
					Debug.WriteLine(string.Format("It looks like {0} free points are being missed. It may not be a problem if false edge unification is the cause", rMissing)); 
 				}
			} 

			double rTotalPoints = rSelfScore + UnusedScore();
			Debug.Assert(double.IsNaN(PointsInPatchTable) || Math.Abs(rTotalPoints - PointsInPatchTable) < 1e-10);
		}

 
		public Greedy() //!!!private or protected??? 
 		{
 		} 



        public HashSet<Component> Components = new HashSet<Component>();
        public HashSet<Patch> UnusedPatches = new HashSet<Patch>();
        Dictionary<Component, Dictionary<Patch, Move>> TableFromComponentToPatchToMove = new Dictionary<Component, Dictionary<Patch, Move>>(); 
        Dictionary<Component, Dictionary<Component, Move>> TableFromComponentToShorterComponentToMove = new Dictionary<Component, Dictionary<Component, Move>>(); 

		public Component MakePatchAComponent(Patch aPatch) 
 		{
			RemovePatchFromTables(aPatch);
			VaccineAsString.AddComponent(aPatch.PatchPattern());
			Component aComponent = Component.GetInstance(aPatch);
			Components.AddNew(aComponent);
			UpdateComponentTables(aComponent); 
 
 			return aComponent;
 		} 



		public void RemovePatchFromTables(Patch patch)
 		{
			UnusedPatches.Remove(patch); 
            foreach (Dictionary<Patch, Move> tableFromPatchToMove in TableFromComponentToPatchToMove.Values) 
			{
				tableFromPatchToMove.Remove(patch); 
			}
		}

 		public void RemoveComponentFromTables(Component aComponent)
 		{
			Components.Remove(aComponent); 
 			TableFromComponentToPatchToMove.Remove(aComponent); 
			TableFromComponentToShorterComponentToMove.Remove(aComponent);
			foreach(var tableFromShorterComponentToMove in TableFromComponentToShorterComponentToMove.Values) 
			{
				tableFromShorterComponentToMove.Remove(aComponent);
			}
 		}

 		private bool Shorter(Component sbShort, Component sbLong) 
		{ 
 			if (sbShort.CoreLength < sbLong.CoreLength)
			{ 
				return true;
			}

			if (sbShort.CoreLength > sbLong.CoreLength)
			{
 				return false; 
 			} 

			bool b = (sbShort.PatchPattern().GetHashCode().CompareTo(sbLong.PatchPattern().GetHashCode()) > 0); 
 			return b;
		}

		public void UpdateComponentTables(Component componentThatChanged)
		{
            Dictionary<Patch, Move> tableFromPatchToMove = 
                ( 
                
                    from patch in UnusedPatches
#if (!SILVERLIGHT)
                        .AsParallel()
#endif
                    let move = ScoreOfAddingPatchToComponent(componentThatChanged, patch) 
                    select new { aPatch = patch, move }
                ).ToDictionary(patchAndMove => patchAndMove.aPatch, patchAndMove => patchAndMove.move);

			TableFromComponentToPatchToMove[componentThatChanged] = tableFromPatchToMove;

 
            Dictionary<Component, Move> tableFromShorterComponentToMove = new Dictionary<Component, Move>(); 
			foreach(Component aShorterComponent in Components)
 			{ 
 				if (Shorter(aShorterComponent, componentThatChanged))
				{
 					Debug.Assert(aShorterComponent != componentThatChanged);
					Move aMove = ScoreOfAddingShorterComponentToComponent(componentThatChanged, aShorterComponent);
					if (aMove != null)
					{ 
						tableFromShorterComponentToMove.Add(aShorterComponent, aMove); 
					}
 
                    Dictionary<Component, Move> tableFromComponentThatChangedToMove = TableFromComponentToShorterComponentToMove[aShorterComponent];
 					if (tableFromComponentThatChangedToMove.ContainsKey(componentThatChanged))
 					{
						tableFromComponentThatChangedToMove.Remove(componentThatChanged);
 					}
 
				} 
				else if (aShorterComponent != componentThatChanged)
				{ 
                    Dictionary<Component, Move> tableFromComponentThatChangedToMove = TableFromComponentToShorterComponentToMove[aShorterComponent];
					Move aMove = ScoreOfAddingShorterComponentToComponent(aShorterComponent, componentThatChanged);
					if (aMove == null)
 					{
 						tableFromComponentThatChangedToMove.Remove(componentThatChanged);
					} 
 					else 
					{
						tableFromComponentThatChangedToMove[componentThatChanged] = aMove; 
					}

				}
			}
 			TableFromComponentToShorterComponentToMove[componentThatChanged] = tableFromShorterComponentToMove;
 		} 
 
		virtual internal Move ScoreOfAddingPatchToComponent(Component component, Patch patch)
 		{ 
			Move bestMove = Move.GetInstance(component, patch);
			return bestMove;
		}

		virtual internal Move ScoreOfAddingShorterComponentToComponent(Component component, Component shorterComponent)
		{ 
 			Move bestMove = Move.GetInstance(component, shorterComponent); 
 			return bestMove;
		} 

 		private Move FindBestComponentMoveOrNull()
		{
			Move bestMove = null;

			foreach(var tableFromShorterComponentToMove in TableFromComponentToShorterComponentToMove.Values) 
			{ 
				foreach(Move aMove in tableFromShorterComponentToMove.Values)
 				{ 
 					bestMove = Move.MaxOrNull(aMove, bestMove);
				}
 			}
			return bestMove;

		} 
 
		private Move FindBestPatchMoveOrNull()
		{ 
			Move bestMove = null;

            foreach (Dictionary<Patch, Move> tableFromPatchToMove in TableFromComponentToPatchToMove.Values)
 			{
 				foreach(Move aMove in tableFromPatchToMove.Values)
				{ 
 					bestMove = Move.MaxOrNull(aMove, bestMove); 
				}
			} 
			return bestMove;
		}

		private void MakeMove(Move aMove)
 		{
 			aMove.DoIt(this); 
		} 

 		private void EndItAll() 
		{
			VaccineAsString = null;
		}

	}
 
	abstract public class Move 
 	{
 		public abstract void DoIt(Greedy greedy); 
		abstract public double ScoreImprovement {get;}
 		abstract public int LengthIncrease {get;}
		abstract public int Length { get;}

		public static Move GetInstance(Component component, Combinable combinable)
		{ 
			Move bestMove = BestAppend(combinable); 

			if (component.CoreLength >= combinable.CoreLength) 
 			{
 				Move absorbsMoveOrNull = BestAbsorbs(component, combinable);
				bestMove = Move.MaxOrNull(absorbsMoveOrNull, bestMove);
 			}
			else
			{ 
				Move absorbedByMoveOrNull = BestAbsorbedBy(component, combinable); 
				bestMove = Move.MaxOrNull(absorbedByMoveOrNull, bestMove);
			} 

 			Move leftMoveOrNull = BestLeftOrNull(component, combinable);
 			bestMove = Move.MaxOrNull(leftMoveOrNull, bestMove);

			Move rightMoveOrNull = BestRightOrNull(component, combinable);
 			bestMove = Move.MaxOrNull(rightMoveOrNull, bestMove); 
 
			return bestMove;
		} 

		static public CompareZone Zone(Move moveOrNull)
		{
			if (moveOrNull == null)
 			{
 				return CompareZone.Null; 
			} 

 			CompareZone compareZone = (CompareZone) 2 - Math.Sign(moveOrNull.LengthIncrease); 
			return compareZone;
			
		}

		public static Move MaxOrNull(Move moveOrNull, Move bestMoveOrNull)
		{ 
 			CompareZone zone = Zone(moveOrNull); 
 			CompareZone bestZone = Zone(bestMoveOrNull);
 
			if (zone > bestZone)
 			{
				return moveOrNull;
			}

			if (bestZone > zone) 
			{ 
				return bestMoveOrNull;
 			} 

 			Debug.Assert(bestZone == zone);
			
 			switch(zone)
			{
				case CompareZone.Null: 
					return null; 
				case CompareZone.LengthIncreases:
				{ 
 					double scorePerLength = moveOrNull.ScoreImprovement / (double) moveOrNull.LengthIncrease;
 					double scorePerLengthBest = bestMoveOrNull.ScoreImprovement / (double) bestMoveOrNull.LengthIncrease;
					if (scorePerLength > scorePerLengthBest)
 					{
						return moveOrNull;
					} 
					else 
					{
						return bestMoveOrNull; 
 					}
 				}
				case CompareZone.LengthUnchanged:
 				{
					if (moveOrNull.ScoreImprovement > bestMoveOrNull.ScoreImprovement)
					{ 
						return moveOrNull; 
					}
					else 
 					{
 						return bestMoveOrNull;
					}
 				}
				case CompareZone.LengthDecreases:
				{ 
					Debug.Assert(moveOrNull.ScoreImprovement == 0 && bestMoveOrNull.ScoreImprovement == 0); // real assert 
					if (moveOrNull.LengthIncrease < bestMoveOrNull.LengthIncrease)
					{ 
 						return moveOrNull;
 					}
					else
 					{
						return bestMoveOrNull;
					} 
				} 
				default:
				{
                    SpecialFunctions.CheckCondition(false, "Unknown zone (shouldn't get here");
 					return null;
				}
 			}
		}
 
 
		static private Move BestAppend(Combinable combinable)
		{ 
			Move bestMove = combinable.GetAppend();
			return bestMove;
 		}
 		static private Move BestAbsorbs(Component component, Combinable combinable)
		{
 
 			PatchPattern unification = combinable.PatchPattern().UnifyOrNull(component.PatchPattern()); 
			if (unification != null)
			{ 
				Move bestMove = new MergeInPatch(component, combinable, unification);
				return bestMove;
			}
 			else
 			{
				return null; 
 			} 
		}
		static private Move BestAbsorbedBy(Component component, Combinable combinable) 
		{
			PatchPattern unification = component.PatchPattern().UnifyOrNull(combinable.PatchPattern());
			if (unification != null)
 			{
 				Move bestMove = new MergeInPatch(component, combinable, unification);
				return bestMove; 
 			} 
			else
			{ 
				return null;
			}
		}

 		static private Move BestLeftOrNull(Component component, Combinable combinable)
 		{ 
			Move aMove = BestRightLeftOrNull(component, combinable, component.PatchPattern(), combinable.PatchPattern()); 
 			return aMove;
		} 

		static private Move BestRightOrNull(Component component, Combinable combinable)
		{
			Move aMove = BestRightLeftOrNull(component, combinable, combinable.PatchPattern(), component.PatchPattern());
			return aMove;
 		} 
 

 		static PatchPatternFactory PatchStringFactory = PatchPatternFactory.GetFactory("strings"); 

		static private Move BestRightLeftOrNull(Component component, Combinable combinable, PatchPattern main, PatchPattern other)
 		{
			int lengthOfShorter = Math.Min(main.CoreLength, other.CoreLength);

 
			for(int overlap = lengthOfShorter - 1; overlap > 0; --overlap) 
			{
				PatchPattern unification = other.UnifyOnRightOrNull(overlap, main); 
				if (unification != null)
 				{
 					Move aMove = new MergeInPatch(component, combinable, unification);
					return aMove;
 				}
			} 
			 
			return null;		
		} 



		protected Move()
 		{
 		} 
 

    } 


	abstract public class Combinable
 	{
		public int CoreLength
		{ 
			get 
			{
				return PatchPattern().CoreLength; 
 			}
 		}

		public int FullLength
 		{
			get 
			{ 
				return PatchPattern().FullLength;
			} 
		}
 		public double Weight;
 		abstract public PatchPattern PatchPattern();
		public abstract void RemoveMeFromTablesAndUpdate(Greedy greedy, Component component);
 		public abstract Move GetAppend();
	} 
 

	public class Component : Combinable 
	{
		private Component()
		{
 		}

 
 		override public void RemoveMeFromTablesAndUpdate(Greedy greedy, Component component) 
		{
 			greedy.RemoveComponentFromTables(this); 
			greedy.UpdateComponentTables(component);
			greedy.VaccineAsString.SetComponents(greedy.Components);
		}


		override public Move GetAppend() 
		{ 
 			return null; //Doesn't make any sense to append a componet to the vaccine as a component because it is already there
 		} 

		public int CreationOrder;
 		static int NumberCreated = 0;

		static public Component GetInstance(Patch aPatch)
		{ 
			Component aComponent = new Component(); 
			aComponent._patchPattern = aPatch.PatchPattern();
			aComponent.Weight = aPatch.Weight; 
 			aComponent.CreationOrder = NumberCreated++;
 			return aComponent;
		}

 		PatchPattern _patchPattern;
		override public PatchPattern PatchPattern() 
		{ 
			return _patchPattern;
		} 


		public void ReplaceWithUnification(Combinable combinable, PatchPattern unification)
 		{
 			_patchPattern = unification;
			Weight +=  combinable.Weight; 
 		} 
	}
 
	class MergeInPatch : Move
	{
		public MergeInPatch(Component component, Combinable combinable, PatchPattern unification)
		{
 			string u = unification.ToString();
 			Debug.Assert(u[0] != '.' && u[u.Length-1] != '.'); // real assert should have AASet.OptionalAny at start or end 
 
			Component = component;
 			Combinable = combinable; 
			Unification = unification;
			if (combinable is Patch)
			{
				_LengthIncrease = unification.FullLength - component.FullLength;
				Debug.Assert(_LengthIncrease >= 0); // real assert
 				_ScoreImprovement = combinable.Weight; 
 			} 
			else
 			{ 
				//!!!we don't distinish between a merge that reduces the length more or less
				Debug.WriteLine("A merge of two components has been found");
				_LengthIncrease = Unification.FullLength - (Component.FullLength + Combinable.FullLength);
				Debug.Assert(_LengthIncrease < 0);
				_ScoreImprovement = 0;
 			} 
 		} 

		public Component Component; 
 		public Combinable Combinable;
		public PatchPattern Unification;	

		public double _ScoreImprovement;
		override public double ScoreImprovement
		{ 
			get 
 			{
 				return _ScoreImprovement; 
			}
 		}
		public int _LengthIncrease;
		override public int LengthIncrease
		{
			get 
			{ 
 				return _LengthIncrease;
 			} 
		}

 		override public void DoIt(Greedy greedy)
		{
			string u = Unification.ToString();
			Debug.Assert(u[0] != '.' && u[u.Length-1] != '.'); // real assert should have AASet.OptionalAny at start or end 
			//Debug.Assert(Combinable is Component || greedy.UnusedPatches.Contains(Combinable)); // real assert 
			Component.ReplaceWithUnification(Combinable, Unification);
 			Combinable.RemoveMeFromTablesAndUpdate(greedy, Component); 
 		}



		public override int Length
 		{ 
			get { 
				return Unification.CoreLength;
			} 
		}
}

	class AppendPatch : Move
 	{
 		public AppendPatch(Patch patch) 
		{ 
 			Patch = patch;
		} 
		public Patch Patch;

		override public double ScoreImprovement
		{
			get
 			{ 
 				return Patch.Weight; 
			}
 		} 

		override public int LengthIncrease
		{
			get
			{
				return Patch.FullLength; 
 			} 
 		}
 
		override public void DoIt(Greedy greedy)
 		{
			Debug.Assert(greedy.UnusedPatches.Contains(Patch)); // real assert
			Debug.WriteLine(string.Format("Making patch {0} a component, causing weight to increase by {1}", Patch.PatchPattern(), Patch.Weight));
			greedy.MakePatchAComponent(Patch);
		} 
 
		public override int Length
 		{ 
 			get
			{
 				return Patch.CoreLength;
			}
		}
} 
 
	public enum CompareZone
	{ 
		Null = 0,
 		LengthIncreases = 1,
 		LengthUnchanged = 2,
		LengthDecreases = 3,
 	}
 
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
