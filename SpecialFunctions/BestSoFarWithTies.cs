using System; 
using System.Collections.Generic;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{
    public class BestSoFarWithTies<TScore, TItem> 
    { 
        //public delegate bool IsBetterDelegate(TScore scoreChamp, TScore scoreChallenger);
        public TScore ChampsScore; 
        public List<TItem> ChampList;
        public int ChangeCount = 0;
        internal Comparison<TScore> IsBetter;

        private BestSoFarWithTies()
        { 
        } 

 


        public static BestSoFarWithTies<TScore, TItem> GetInstance(Comparison<TScore> isBetter)
        {
            BestSoFarWithTies<TScore, TItem> bestSoFarWithTies = new BestSoFarWithTies<TScore, TItem>();
            bestSoFarWithTies.IsBetter = isBetter; 
            return bestSoFarWithTies; 
        }
 
        public virtual bool Compare(TScore scoreChallenger, TItem itemChallenger)
        {
            int isBetterComparison = (ChangeCount == 0) ? 1 : IsBetter(scoreChallenger, ChampsScore);
            switch (isBetterComparison)
            {
                case -1: 
                    return false; 
                case 1:
                    ChampsScore = scoreChallenger; 
                    ChampList = new List<TItem>();
                    ChampList.Add(itemChallenger);
                    ++ChangeCount;
                    return true;
                case 0:
                    ChampList.Add(itemChallenger); 
                    ++ChangeCount; 
                    return true;
                default: 
                    SpecialFunctions.CheckCondition(false, "Comparison should return -1, 0, or 1");
                    return false;
            }
        }
    }
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
