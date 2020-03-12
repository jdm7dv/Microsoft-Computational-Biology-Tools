using System; 
using System.Collections.Generic;
using System.Text;

namespace VirusCount
{
 
 	//public class BestSoFarClone<TScore, TItem> : BestSoFar<TScore, TItem> where TItem : ICloneable 
	//{
	//    private BestSoFarClone() 
	//        : base()
	//    {
	//    }

 	//    //!!!could like to merge this into BestSoFar.GetInstance, but can't get it to work
 	//    new public static BestSoFarClone<TScore, TItem> GetInstance(IsBetterDelgate isBetter) 
	//    { 
 	//        BestSoFarClone<TScore, TItem> bestSoFar = new BestSoFarClone<TScore, TItem>();
	//        bestSoFar.IsBetter = isBetter; 
	//        return bestSoFar;
	//    }

	//    override internal bool Compare(TScore scoreCallenger, TItem itemChallenger)
	//    {
 	//        if (ChangeCount == 0 || IsBetter(ChampsScore, scoreCallenger)) 
 	//        { 
	//            ChampsScore = scoreCallenger;
 	//            Champ = (TItem) itemChallenger.Clone(); 
	//            ++ChangeCount;
	//            return true;
	//        }
	//        return false;
	//    }
 
 	//} 

 
 	//Merge with BestItems
	public class BestSoFar<TScore, TItem>
 	{
		//public delegate bool IsBetterDelegate(TScore scoreChamp, TScore scoreChallenger);
		public TScore ChampsScore;
		public TItem Champ; 
		public int ChangeCount = 0; 

		private BestSoFar() 
 		{
 		}

        internal Comparison<TScore> IsBetter;

 
 
		public static BestSoFar<TScore, TItem> GetInstance(Comparison<TScore> isBetter)
 		{ 
			BestSoFar<TScore, TItem> bestSoFar = new BestSoFar<TScore, TItem>();
			bestSoFar.IsBetter = isBetter;
			return bestSoFar;
		}

		public virtual bool Compare(TScore scoreChallenger, TItem itemChallenger) 
 		{ 
 			if (ChangeCount == 0 || IsBetter(scoreChallenger, ChampsScore) > 0)
			{ 
 				ChampsScore = scoreChallenger;
				Champ = itemChallenger;
				++ChangeCount;
				return true;
			}
			return false; 
 		} 
 	}
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
