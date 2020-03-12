using System; 
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Text; 
 
using Msr.Mlas.SpecialFunctions;
using VirusCount.Qmrr; 

namespace HlaAssign
{
 	public class HlaAssignMain
	{
		static void Main(string[] args) 
		{ 
			try
			{ 
 				HlaAssignMain	prog = HlaAssignMain.CreateProgramFromArguments(args);

 				if (prog == null)
					return;

 				prog.Run(); 
			} 
			catch (Exception exception)
			{ 
				Console.WriteLine("");
				Console.WriteLine(exception.Message);
 				if (exception.InnerException != null)
 				{
					Console.WriteLine(exception.InnerException.Message);
 				} 
 
				Console.WriteLine("");
				Console.WriteLine(UsageMessage); 
				Environment.Exit(-1);
			}
		}

 		static HlaAssignMain CreateProgramFromArguments(string[] args)
 		{ 
			ArgCollection argCollection = ArgCollection.GetInstance(args); 

 			if (argCollection.ExtractOptionalFlag("help")) 
			{
				Console.WriteLine();
				Console.WriteLine(UsageMessage);
				Console.WriteLine(HelpMessage);
				return null;
 			} 
 
 			bool	bExcel = argCollection.ExtractOptionalFlag("EXCEL");
            argCollection.CheckNoMoreOptions();
 
            double? leakProbabilityOrNull = argCollection.ExtractNext<double?>("leakProbabilityOrNull");
            double	pValue = argCollection.ExtractNext<double>("pValue");

            string	directoryName = argCollection.ExtractNext<string>("directory");
			string	caseName = argCollection.ExtractNext<string>("caseName");
 
            string hlaFactoryName = argCollection.ExtractNext<string>("hlaFactory"); 
            RangeCollection pieceIndexRange = argCollection.ExtractNext<RangeCollection>("pieceIndexRange");
            int pieceCount = argCollection.ExtractNext<int>("pieceCount"); 
            SpecialFunctions.CheckCondition(0 <= pieceIndexRange.FirstElement && pieceIndexRange.LastElement < pieceCount, "The pieceIndexRange must be a subrange of " + RangeCollection.GetInstance(0, pieceCount - 1).ToString());
            RangeCollection nullIndexRange = argCollection.ExtractNext<RangeCollection>("nullIndexRange");
            argCollection.CheckThatEmpty();

 			return new HlaAssignMain(bExcel, leakProbabilityOrNull, pValue, directoryName, caseName, hlaFactoryName, pieceIndexRange, pieceCount, nullIndexRange);
		} 
 
		public HlaAssignMain(bool excel, double? leakProbabilityOrNull, double pValue,
											string directoryName, string caseName, string hlaFactoryName, 
											RangeCollection pieceIndexRange, int pieceCount, RangeCollection nullIndexRange)
		{
 			m_bExcel          = excel;
 			m_leakProbability = leakProbabilityOrNull;
			m_pValue          = pValue;
 			m_directoryName   = directoryName; 
			m_caseName        = caseName; 
			m_hlaFactoryName  = hlaFactoryName;
			m_pieceIndexRange = pieceIndexRange; 
			m_pieceCount      = pieceCount;
			m_nullIndexRange  = nullIndexRange;
 		}

 		public string OutputFileName
		{ 
 			get { return string.Format(@"{0}.{1}.{2}.{3}.{4}.pValues.new", m_strSelection, m_caseName, m_nullIndexRange, m_pieceCount, m_pieceIndexRange); } 
		}
 
		String				m_strSelection = "ForwardSelection";

		bool				m_bExcel;
		double?				m_leakProbability;
		double				m_pValue;
 		string				m_directoryName; 
 		string				m_caseName; 
		string				m_hlaFactoryName;
 		RangeCollection		m_pieceIndexRange; 
		int					m_pieceCount;
		RangeCollection		m_nullIndexRange;

		public void Run()
		{
			LrtForHla	aLrtForHla; 
 
 			using (DataConnection connection = new DataConnection(m_directoryName, m_caseName, m_bExcel))
 			using (DbDataReader datareaderHLA   = CreateDataReader(connection, "HLA")) 
			using (DbDataReader datareaderReact = CreateDataReader(connection, "React"))
 			using (DbDataReader datareaderKnown = CreateDataReader(connection, "Known"))
			{
				aLrtForHla = LrtForHla.GetInstance(m_strSelection, datareaderHLA, datareaderReact, datareaderKnown, m_caseName, m_hlaFactoryName, m_leakProbability, m_pValue);

				datareaderKnown.Close(); 
				datareaderReact.Close(); 
				datareaderHLA.Close();
 				connection.Close(); 
 			}

            aLrtForHla.Run(m_pieceIndexRange, m_pieceCount, m_nullIndexRange, m_directoryName, OutputFileName);
		}

 		public DbDataReader CreateDataReader(DataConnection connection, string tablename) 
		{ 
			try
			{ 
				if (connection.Excel)
				{
 					tablename += "$";
 					OleDbCommand	command = new OleDbCommand("SELECT * FROM [" + tablename + "]", connection.Connection);
					return command.ExecuteReader();
 				} 
				else 
				{
					tablename = connection.Directory + "\\" + connection.CaseName + tablename + ".txt"; 
					return new StreamDataReader(tablename);
				}
 			}
 			catch (Exception excep)
			{
 				throw new ApplicationException(string.Format("{0}: failed to query table", connection), excep); 
			} 
		}
 
		public class DataConnection : IDisposable
		{
			public DataConnection(string dir, string casename, bool excel)
 			{
 				OleDbConnectionStringBuilder	builder = new OleDbConnectionStringBuilder();
 
				m_strDir      = dir; 
 				m_strCaseName = casename;
				m_bExcel      = excel; 

				builder.Provider = "Microsoft.Jet.OLEDB.4.0";

				if (m_bExcel)
				{
					builder.DataSource = dir + "\\" + casename + ".xls"; 
 					builder["Extended Properties"] = "Excel 8.0"; 

 					if (!File.Exists(builder.DataSource)) 
						throw new ApplicationException(string.Format("{0}: file not found", builder.DataSource));
 				}
				else
				{
					builder.DataSource = dir;
					builder["Extended Properties"] = "text"; 
 
					if (!System.IO.Directory.Exists(builder.DataSource))
 						throw new ApplicationException(string.Format("{0}: directory not found", builder.DataSource));					 
 				}

				m_connection = new OleDbConnection(builder.ConnectionString);
 				m_connection.Open();
			}
 
			public void Dispose() 
			{
				Close(); 
			}

 			public OleDbConnection Connection
 			{
				get { return m_connection; }
 			} 
 
			public bool Excel
			{ 
				get { return m_bExcel; }
			}

			public string Directory
 			{
 				get { return m_strDir; } 
			} 

 			public string CaseName 
			{
				get { return m_strCaseName; }
			}

			public void Close()
			{ 
 				m_connection.Close(); 
 			}
 
			public override string ToString()
 			{
				return m_connection.DataSource;
			}

			bool				m_bExcel; 
			string				m_strDir; 
			string				m_strCaseName;
 			OleDbConnection		m_connection; 
 		}

        static string UsageMessage = @"Usage:
HlaAssign {-EXCEL} leakProbabilityOrNull pValue directory caseName hlaFactory pieceIndexRange pieceCount nullIndexRange
HlaAssign -help
"; 
		static string HelpMessage = @" 
where
    ""leakProbabilityOrNull"" is either ""null"" (to have it learned) or given 
    ""pValue"" is the max pValue.
    ""directory"" is the location of the input files
    ""caseName"" is the prefix of the input files
        The input files themselves are
            CASENAMEKnown.txt (header: ""peptide knownHLA"")
            CASENAMEHla.txt (header: ""hla    cid   present"") 
            CASENAMEReact.txt (header: ""peptide   cid   magnitude"") 
    ""hlaFactoryName"" is either ""MixedWithB15AndA68"" or ""FourDigit"" or ""JustSlash"" or ""noConstraints""
    pieceIndexSet pieceCount nullIndexSet (e.g. 0-0 1 -1-9) 

If -Excel is given, the input is an Excel file with the name 'casename'.xls
that must contain three worksheets named: HLA, React, and Known, respectively (order is not important).

Each worksheet needs to start with a header line, very much like the input text files.
As with the text files, the order of the columns in worksheets is immaterial.

The header line of the HLA file should read:

hla<tab>cid<tab>present

and each entry should, for now, have a 'present' value of '1'.

The header line of the react file should read:

peptide<tab>cid<tab>magnitude

Leading and trailing spaces are trimmed for each input field in the input files.


";
    }
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
