    using System; 
    using System.Collections.Generic;
    using System.Text;
    using Msr.Mlas.Qmr;

namespace VirusCount.Qmr
    { 
        public class QmrJob<TCause, TEffect> 
        {
            private QmrJob() 
            {
            }

            public string Name;
            public List<TEffect> PresentEffectCollection;
            public List<TEffect> AbsentEffectCollection; 
            public Qmr<TCause, TEffect> Qmr; 

            public static QmrJob<TCause, TEffect> 
                GetInstance(string name, List<TEffect> presentEffectCollection,
                List<TEffect> absentEffectCollection, Qmr<TCause, TEffect> qmr)
            {
                QmrJob<TCause, TEffect> aQmrJob = new QmrJob<TCause, TEffect>();
                aQmrJob.Name = name;
                aQmrJob.PresentEffectCollection = presentEffectCollection; 
                aQmrJob.AbsentEffectCollection = absentEffectCollection; 
                aQmrJob.Qmr = qmr;
                return aQmrJob; 
            }

            public override string ToString()
            {
                return string.Format("{0}\t{1}\t{2}", Name, PresentEffectCollection.Count, AbsentEffectCollection.Count);
            } 
 
            public Dictionary<TCause,double> PosteriorOfEveryCause()
            { 
                return Qmr.PosteriorOfEveryCause(PresentEffectCollection, AbsentEffectCollection);
            }
        }
    }

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved. 
