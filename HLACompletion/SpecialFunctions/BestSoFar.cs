using System; 
using System.Collections.Generic;
using System.Text;

namespace VirusCount
{
 
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

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
