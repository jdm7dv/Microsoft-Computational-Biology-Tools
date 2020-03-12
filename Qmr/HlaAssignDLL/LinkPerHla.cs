using System; 
using System.Collections.Generic;
using System.Text;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using Optimization;
using EpipredLib; 
 
namespace VirusCount.Qmr
{ 
    public class LinkPerHla : ThreeParamSlow
    {
        internal LinkPerHla()
        {
        }
 
        override protected double LogLikelihoodOfEffects(QmrrPartialModel qmrrPartialModel, TrueCollection trueCollection, OptimizationParameterList qmrrParams) 
        {
            Set<Hla> trueCollectionAsSet = trueCollection.CreateHlaAssignmentAsSet(); 

            double logOneLessLeakProbability = Math.Log(1.0 - qmrrParams["leakProbability"].Value);

            Dictionary<Hla, double> hlaToLogOneLessLink = new Dictionary<Hla, double>();
            foreach (Hla hla in trueCollectionAsSet)
            { 
                double logOneLessLink = Math.Log(1.0 - qmrrParams["link" + hla].Value); 
                hlaToLogOneLessLink.Add(hla, logOneLessLink);
            } 


            double logLikelihood = 0.0;
            foreach (KeyValuePair<string, Set<Hla>> patientAndHlaList in qmrrPartialModel.PatientList)
            {
                double logLikelihoodNoReactionInThisPatient = logOneLessLeakProbability; 
                foreach (Hla hla in patientAndHlaList.Value) 
                {
                    if (trueCollectionAsSet.Contains(hla)) 
                    {
                        double logOneLessLink = hlaToLogOneLessLink[hla];
                        logLikelihoodNoReactionInThisPatient += logOneLessLink;
                    }
                }
 
                bool didReact = qmrrPartialModel.PatientToAnyReaction.ContainsKey(patientAndHlaList.Key); 
                logLikelihood += LogLikelihoodOfThisPatient(logLikelihoodNoReactionInThisPatient, didReact);
            } 

            return logLikelihood;

        }

    } 
 

} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
