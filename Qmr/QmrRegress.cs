//using System; 
//using System.Collections.Generic;
//using System.Text;
//using System.Diagnostics;
//using Msr.Mlas.Qmr;
//using Msr.Mlas.SpecialFunctions;
 
//namespace VirusCount 
//{
//    public class QmrRegress<TCause, TEffect> : Quickscore<TCause, TEffect> 
//    {
//        public override void SetLink(TCause cause, TEffect effect, double conditionalProbability)
//        {
//            Study.CheckCondition(double.IsNaN(conditionalProbability));
//            base.SetLink(cause,effect,double.NaN);
//        } 
 
//        private double LinkA = double.NaN;
//        private double LinkB = double.NaN; 

//        public void SetLinkAAndB(double linkA, double linkB)
//        {
//            LinkA = linkA;
//            LinkB = linkB;
//            RecomputeLinkProbabilities(); 
 
//        }
 
//        Dictionary<string, Dictionary<TEffect, double>> PeptideToReactingEffectToMagnitude = new Dictionary<string,Dictionary<TEffect,double>>();
//        public void SetPeptideToReactingEffectToMagnitute(string peptide, TEffect effect, double magnitute)
//        {
//            Dictionary<TEffect,double> reactingEffectToMagnitude = Study.GetValueOrDefault(PeptideToReactingEffectToMagnitude, peptide);
//            reactingEffectToMagnitude[effect] = magnitute;
//        } 
 
//        private string _peptide = null;
 
//        public string Peptide
//        {
//            set
//            {
//                _peptide = value;
//                RecomputeLinkProbabilities(); 
//            } 
//        }
 
//        private void RecomputeLinkProbabilities()
//        {
//            Dictionary<TEffect, double> reactingEffectToMagnitude = PeptideToReactingEffectToMagnitude[_peptide];
//            Dictionary<TCause, Dictionary<TEffect, double>> newCauseToEffectToCondProb = new Dictionary<TCause, Dictionary<TEffect, double>>();
//            foreach (KeyValuePair<TCause, Dictionary<TEffect, double>> causeAndEffectToCondProb in CauseEffectCollection)
//            { 
//                TCause cause = causeAndEffectToCondProb.Key; 
//                Dictionary<TEffect, double> effectToCondProb = causeAndEffectToCondProb.Value;
 
//                double logMedianMagnituteOfReactionForEffectsWithThisCause =
//                    LogMedianMagnituteOfReactionForEffectsWithThisCause(effectToCondProb, reactingEffectToMagnitude);


//                Dictionary<TEffect, double> newEffectToCondProb = new Dictionary<TEffect, double>();
//                newCauseToEffectToCondProb.Add(cause, newEffectToCondProb); 
//                foreach (TEffect effect in effectToCondProb.Keys) 
//                {
//                    if (reactingEffectToMagnitude.ContainsKey(effect)) 
//                    {
//                        newEffectToCondProb.Add(effect, LinkA + LinkB * logMedianMagnituteOfReactionForEffectsWithThisCause);
//                    }
//                    else
//                    {
//                        newEffectToCondProb.Add(effect, double.NaN); 
//                    } 
//                }
//            } 
//            CauseEffectCollection = newCauseToEffectToCondProb;

//        }

//        private double LogMedianMagnituteOfReactionForEffectsWithThisCause(Dictionary<TEffect, double> effectToCondProb, Dictionary<TEffect, double> reactingEffectToMagnitude)
//        { 
//            List<double> magnituteOfReactionsForEffectsWithThisCause = new List<double>(); 
//            foreach (TEffect effect in effectToCondProb.Keys)
//            { 
//                if (reactingEffectToMagnitude.ContainsKey(effect))
//                {
//                    magnituteOfReactionsForEffectsWithThisCause.Add(reactingEffectToMagnitude[effect]);
//                }
//            }
//            Study.CheckCondition(magnituteOfReactionsForEffectsWithThisCause.Count > 0); //Need code if this condition is not true 
//            double medianReactionMagnitute = SpecialFunctions.Median(ref magnituteOfReactionsForEffectsWithThisCause); 
//            return Math.Log(medianReactionMagnitute);
//        } 


//    }
//}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
