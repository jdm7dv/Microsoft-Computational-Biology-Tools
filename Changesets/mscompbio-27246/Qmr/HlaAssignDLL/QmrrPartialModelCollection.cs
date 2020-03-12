using System; 
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Msr.Mlas.SpecialFunctions;
using System.Diagnostics;
using Optimization; 
using EpipredLib; 

namespace VirusCount.Qmr 
{
 	public class QmrrPartialModelCollection : List<QmrrPartialModel>
	{
        private OptimizationParameterList OptimizationParameterList;
        private ModelLikelihoodFactories ModelLikelihoodFactories;
        private List<string> PeptideList; 
        public string DatasetName; 
        private string PatientFileName;
        private string PeptideFileName; 
        private string ReactFileName;
        private string KnownFileName;

        private QmrrPartialModelCollection()
        {
        } 
 
        static public QmrrPartialModelCollection GetInstance(ModelLikelihoodFactories modelLikelihoodFactories, string datasetName, OptimizationParameterList qmrrParams, string hlaFactoryName)
		{ 
            QmrrPartialModelCollection aQmrrPartialModelCollection = new QmrrPartialModelCollection();
            aQmrrPartialModelCollection.HlaFactory = Qmrr.HlaFactory.GetFactory(hlaFactoryName);

            aQmrrPartialModelCollection.OptimizationParameterList = qmrrParams;
            aQmrrPartialModelCollection.SetFileNames(datasetName);
            aQmrrPartialModelCollection.ReadTables(); 
            aQmrrPartialModelCollection.ModelLikelihoodFactories = modelLikelihoodFactories; 
            aQmrrPartialModelCollection.CreateList();
            return aQmrrPartialModelCollection; 
        }

        public static QmrrPartialModelCollection GetInstance(string peptide,
            ModelLikelihoodFactories modelLikelihoodFactories,
            OptimizationParameterList qmrrParamsStart,
            Dictionary<string, Set<Hla>> patientList, 
            Dictionary<string, Dictionary<string,double>> reactTable, 
            Dictionary<string, Set<Hla>> knownTable,
            string hlaFactoryName 
            )
        {
            QmrrPartialModelCollection aQmrrPartialModelCollection = new QmrrPartialModelCollection();
            aQmrrPartialModelCollection.HlaFactory = Qmrr.HlaFactory.GetFactory(hlaFactoryName);
            aQmrrPartialModelCollection.OptimizationParameterList = qmrrParamsStart;
            aQmrrPartialModelCollection.SetFileNamesToNull(); 
            aQmrrPartialModelCollection.PeptideList = new List<string>(new string[] { peptide }); 
            aQmrrPartialModelCollection.PatientList = patientList;
            aQmrrPartialModelCollection.ReactTable = reactTable; 
            aQmrrPartialModelCollection._knownTable = knownTable;
            aQmrrPartialModelCollection.ModelLikelihoodFactories = modelLikelihoodFactories;
            aQmrrPartialModelCollection.CreateList();
            return aQmrrPartialModelCollection;
        }
 
        private void SetFileNamesToNull() 
        {
            DatasetName = null; 
            PatientFileName = null;
            PeptideFileName = null;
            ReactFileName = null;
            KnownFileName = null;
        }
 
 

		private void CreateList() 
		{
			foreach (string peptide in PeptideList)
 			{
                Dictionary<string, double> patientToAnyReaction = ReactTable.ContainsKey(peptide)? ReactTable[peptide] : new Dictionary<string, double>();
                Set<Hla> knownHlaSet = KnownTable(peptide);
                QmrrPartialModel aQmrrPartialModel = QmrrPartialModel.GetInstance(ModelLikelihoodFactories, peptide, patientToAnyReaction, knownHlaSet, PatientList, OptimizationParameterList); 
                Add(aQmrrPartialModel); 
 			}
		} 

 		private void ReadTables()
		{
			ReadPeptideTable();
			ReadPatientTable();
			ReadReactTable(); 
            ReadKnownTable(); 
        }
 

		private Dictionary<string, Dictionary<string,double>> ReactTable;
 		private void ReadReactTable()
 		{
			ReactTable = new Dictionary<string,Dictionary<string,double>>();
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(ReactFileName, "pid	peptide	mag", false))//!!!const 
 			{ 
				string peptide = row["peptide"];//!!!const
				Dictionary<string, double> patientToAnyReaction = SpecialFunctions.GetValueOrDefault(ReactTable, peptide); 
				patientToAnyReaction.Add(row["pid"],double.Parse(row["mag"])); //!!!const
			}
		}

        private Dictionary<string, Set<Hla>> PatientList;
 		private void ReadPatientTable() 
 		{ 
            Qmrr.HlaFactory hlaFactory = Qmrr.HlaFactory.GetFactory("noConstraint");
 

            PatientList = new Dictionary<string, Set<Hla>>();
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(PatientFileName, "pid	a1	a2	b1	b2	c1	c2", false))//!!!const
			{
 				string patientId = row["pid"];
                Set<Hla> hlaList = new Set<Hla>(); 
				foreach (string columnName in new string[] { "a1", "a2", "b1", "b2", "c1", "c2" })//!!!const 
				{
                    hlaList.AddNewOrOld(hlaFactory.GetGroundInstance(row[columnName])); 
				}
				PatientList.Add(patientId, hlaList);
			}
 		}

 		private void ReadPeptideTable() 
		{ 
 			PeptideList = new List<string>();
            foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTableAsList(PeptideFileName, "peptide", false))//!!!const 
			{
				PeptideList.Add(row["peptide"]); //!!!const
			}
		}

        private Dictionary<string, Set<Hla>> _knownTable; 
        public Set<Hla> KnownTable(string peptide) 
        {
            if (_knownTable.ContainsKey(peptide)) 
            {
                return _knownTable[peptide];
            }
            else
            {
                return Set<Hla>.GetInstance(); 
            } 
        }
 
        Qmrr.HlaFactory HlaFactory;
        private void ReadKnownTable()
        {
            _knownTable = new Dictionary<string, Set<Hla>>();
            if (OptimizationParameterList["useKnownList"].Value == 1)
            { 
                foreach (Dictionary<string, string> row in SpecialFunctions.TabFileTable(KnownFileName, "peptide	knownHLA", false))//!!!const 
                {
                    string peptide = row["peptide"];//!!!const 
                    Set<Hla> knownHlaSet = SpecialFunctions.GetValueOrDefault(_knownTable, peptide);
                    knownHlaSet.AddNew(HlaFactory.GetGroundInstance(row["knownHLA"])); //!!!const
                }
            }
        }
 
		private void SetFileNames(string datasetName) 
 		{
 			DatasetName = datasetName; 
			PatientFileName = string.Format("{0}patient.txt", datasetName);//!!!const
 			PeptideFileName = string.Format("{0}peptide.txt", datasetName);//!!!const
			ReactFileName = string.Format("{0}react.txt", datasetName);//!!!const
            KnownFileName = string.Format("{0}known.txt", datasetName);//!!!const
        }
 
 
        internal Set<Hla> GetKnownHlaSet(string peptide)
        { 
            return KnownTable(peptide);
        }
    }


} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
