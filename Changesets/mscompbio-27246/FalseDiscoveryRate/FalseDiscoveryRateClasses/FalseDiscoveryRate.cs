using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

namespace FalseDiscoveryRateClasses
{
    /**
     * Implements the False Discovery Rate algorithms
     * Author - Guy Shani 2008
     * */

    public class FalseDiscoveryRate
    {
        public const double EPSILON = 0.000000000001;
        private const int MAX_REPROT_POINT = 10000000;
        private int m_cTableNamesColumns; //The number of columns in the input file that contain information prior to the table counts
        private bool m_bReportProgress; //Should all processes report progress - allow for a silent execution
        private double m_dFDRCutoff; //for a large number of tables - avoid computing tables with large FDR
        private bool m_bHuge;// Huge mode - using only a sample and not storing the actual tables. Slower, but can handle more tables.
        private FileInfo m_fiInput;//Keeping this as a state variable in the case of multiple samples to avoid opening and closing the file multiple times.
        private ContingencyTableCache m_ctcTableCounts;//A contingency tables cache - used only when the Huge switch is on
        private bool m_bSampling;//A switch for sampling - start with a small sample and increase it until FDR does not change
        private int m_iSampleSize;//The size of the first sample. We double the sample in each iteration
        private double m_dMinimalChange;//Minimal change that still requires additional sampling iterations
        private int m_cTables;//The number of tables in the input file - used for sampling
        private bool m_bHasColumnHeaders;//A flag for column headers in the input file
        private string m_sColumnHeaders;//If the file contains column headers, store them here to be written to the output file too
        private PiMethod m_pmEvaluatePi;//The type of PI evaluation method
        private bool m_bFullOutput;//Output all information gathered through the computation (FDR, pooled p-values, filtering pi, ...).
        private double m_dPi; //the evaluated pi_0 - the proportion of nulls in the data
        private ProgressReport m_prReport; //callback channel for generic progress reports
        private bool m_bPositiveFDR;//compute pFDR (Storey) or regular FDR (Benjamini & Hochberg)
        private bool m_bContinue;

        public List<KeyValuePair<double, double>> PToQMapping { get; set; }

        public enum PiMethod { 
            One, //Use pi = 1 (no PI estimation)
            WeightedSum, //Use pi = (\sum_p rho(p)pr(P=p))/(\sum_p rho(p)pr(P=p|H=0))
            DoubleAverage, //Following Pounds & Cheng - use pi = 2*avg(p)
            Filtering //Compute weighted sum but only for relevant tables (tables that can possibly be significant)
        };

        //See above for paramter description
        public FalseDiscoveryRate(int cTableNamesColumns, bool bReportProgress, double dFDRCutoff, bool bHuge, int iInitialSampleSize, double dMinimalChange, bool bHasColumnHeaders, PiMethod pmEvaluatePi, bool bPositiveFDR, bool bFullOutput, ProgressReport prReport)
        {
            m_cTableNamesColumns = cTableNamesColumns;
            m_bReportProgress = bReportProgress;
            m_dFDRCutoff = dFDRCutoff;
            m_bHuge = bHuge;
            m_fiInput = null;
            m_ctcTableCounts = null;
            m_bSampling = ( iInitialSampleSize > 0 );
            m_iSampleSize = iInitialSampleSize;
            m_dMinimalChange = dMinimalChange;
            m_cTables = -1;
            m_bHasColumnHeaders = bHasColumnHeaders;
            m_sColumnHeaders = null;
            m_pmEvaluatePi = pmEvaluatePi;
            m_bFullOutput = bFullOutput;
            m_dPi = -1;
            m_prReport = prReport;
            m_bContinue = true;
            m_bPositiveFDR = bPositiveFDR;
        }

        private static Random m_rndGenerator = new Random();
        public static Random RandomGenerator
        {
            get
            {
                return m_rndGenerator;
            }
        }

        public double PI
        {
            get
            {
                return m_dPi;
            }
        }
        public FileInfo InputFile
        {
            set
            {
                m_fiInput = value;
            }
        }

        //Loading the contingency tables from the input file
        //Implements the sampling techniques
        private List<ContingencyTable> loadTables()
        {
            try
            {
                StreamReader sr = m_fiInput.OpenText();
                ContingencyTable ctCurrent = null;
                if (m_bReportProgress)
                {
                    m_bContinue = m_prReport.reportPhase("Loading data");
                    m_bContinue = m_prReport.reportMessage("Loading data from file " + m_fiInput.Name, true);
                }
                string sLine = "";
                List<ContingencyTable> actTables = new List<ContingencyTable>();
                int cTables = 0;
                long cCharacters = 0;
                bool bUseTable = true;
                double dSampleProbability = 0.0, dProb = 0.0;
                Random rnd = new Random();
                int iLineNumber = 0;
                
                if (m_bHuge)
                    m_ctcTableCounts = new ContingencyTableCache();
                else
                    m_ctcTableCounts = null;

                //On the first iteration go through the file to check the number of rows (tables)
                if (m_cTables == -1)
                {
                    m_cTables = 0;
                    sLine = sr.ReadLine();
                    initColumnHeaders(sLine);
                    while (!sr.EndOfStream)
                    {
                        sLine = sr.ReadLine();
                        m_cTables++;
                        if (m_bReportProgress)
                        {
                            if( m_cTables % MAX_REPROT_POINT == 0 )
                                m_bContinue = m_prReport.reportMessage(".", false);
                        }

                    }
                    if (m_bReportProgress)
                    {
                        m_bContinue = m_prReport.reportMessage("", true);
                        m_bContinue = m_prReport.reportMessage("Found " + m_cTables + " data rows.", true);
                    }
                }
                //Instead of enforcing a hard sample size, we sample the given the sample probability
                dSampleProbability = m_iSampleSize / (double)m_cTables;
                sr.Close();
                sr = m_fiInput.OpenText();
                if (m_bReportProgress)
                {
                    if (m_bSampling)
                    {
                        m_bContinue = m_prReport.reportPhase("Sampling tables");
                        m_bContinue = m_prReport.reportMessage("Sampling " + m_iSampleSize + " tables.", true);
                    }
                }

                if (m_bHasColumnHeaders)
                    sr.ReadLine();
                while (!sr.EndOfStream && m_bContinue)
                {
                    sLine = sr.ReadLine().Trim();
                    iLineNumber++;
                    if (sLine.Length > 0)
                    {
                        bUseTable = true;//general use flag - sampling, validation, ...
                        if (m_bSampling)
                        {
                            dProb = rnd.NextDouble();
                            if (dProb > dSampleProbability)
                                bUseTable = false;
                        } 
                        if (bUseTable)
                        {
                            ctCurrent = new ContingencyTable(sLine, m_cTableNamesColumns);
                            bUseTable = ctCurrent.validate();
                        }
                        if (bUseTable)
                        {
                            if (m_bHuge)//instead of maintaining all the tables try to see whether we already loaded a table with the same counts
                            {
                                double dCount = m_ctcTableCounts.getCachedValue(ctCurrent);
                                if (double.IsNaN(dCount))//First time table was observed
                                {
                                    dCount = 0;
                                    actTables.Add(ctCurrent);
                                }
                                m_ctcTableCounts.setCachedValue(ctCurrent, dCount + 1);//increment the table count
                            }
                            else//not huge - maintain all tables (including duplicates)
                            {
                                actTables.Add(ctCurrent);
                            }
                        }
                        cTables++;
                    }
                    if ((cTables > 0) && (cTables % MAX_REPROT_POINT == 0))
                    {
                        if (m_bReportProgress)
                        {
                            m_bContinue = m_prReport.reportProcessedTables(cTables, m_cTables);
                            m_bContinue = m_prReport.reportMessage("Loaded " + cTables + " tables.", false);
                            if( m_bHuge )
                                m_bContinue = m_prReport.reportMessage(" Found " + actTables.Count + " distinct tables.", false);
                            m_bContinue = m_prReport.reportMessage("", true);
                        }
                    }
                    cCharacters += sLine.Length + 2;
                }
                if (m_bReportProgress)
                {
                    m_bContinue = m_prReport.reportMessage("Done loading data. Found " + actTables.Count + " distinct tables.", true);
                }
                sr.Close();
                return actTables;
            }
            catch (Exception e)
            {
                m_bContinue = m_prReport.reportError("Could not load data : " + e.Message);
            }
            return null;
        }
        //If the data contains column headers initialize it to the first line.
        //Otherwise, just use Column x for all the headers.
        private void initColumnHeaders(string sLine)
        {
            if (m_bHasColumnHeaders)
                m_sColumnHeaders = sLine;
            else
            {
                int cColumns = 1, iColumn = 0;
                foreach (char c in sLine)
                    if (c == '\t')
                        cColumns++;
                m_sColumnHeaders = "Column 1";
                for (iColumn = 1; iColumn < cColumns; iColumn++)
                {
                    m_sColumnHeaders += "\tColumn " + (iColumn + 1);
                }
            }
        }

        //Here we force the computation of the Fisher test scores so that we can later sort the tables
        private void computeFisherScores(List<ContingencyTable> actTables)
        {
            int cTables = 0;

            if (m_bReportProgress)
            {
                m_bContinue = m_prReport.reportPhase("Computing Fisher scores.");
                m_bContinue = m_prReport.reportMessage("Computing Fisher Test Scores.", false);
            }
            foreach (ContingencyTable ct in actTables)
            {
                ct.getFisher2TailPermutationTest();
                cTables++;
                if (m_bReportProgress)
                {
                    if ((cTables > 0) && (cTables % MAX_REPROT_POINT == 0))
                    {
                        m_bContinue = m_prReport.reportProcessedTables(cTables, actTables.Count);
                        m_bContinue = m_prReport.reportMessage(".", false);
                        if (!m_bContinue)
                            return;
                    }
                }
            }
            if (m_bReportProgress)
                m_bContinue = m_prReport.reportMessage("", true);

            actTables.Sort();
            if (m_bReportProgress)
            {
                m_bContinue = m_prReport.reportMessage("", true);
                m_bContinue = m_prReport.reportMessage("Done computing Fisher Test Scores.", true);
            }
        }

        //1-(1-x)^m
        private double OneMinusOneMinuXToTheM(double x, long cTables)
        {
            if( x > 1E-10 ) //assuming that above this threshold (1-x)^m is reasonable computed. Math.Pow is much faster than the loops below.
                return 1 - Math.Pow(1 - x, cTables);
            long iPower = 0;
            double dSum = 0.0, dValue = 1.0;
            List<double> lValues = new List<double>();
            //dValue is 0 when the accuracy drops below double.Epsilon.
            //This is a reasoanbly efficient implementaiton as usually only a small number of values will be higher than double.Epsilon.
            for (iPower = 1; iPower <= cTables && dValue != 0.0; iPower++)
            {
                dValue *= (cTables - iPower) / iPower;
                dValue *= -x;
                if (dValue != 0.0)
                    lValues.Add(dValue);
            }
            lValues.Reverse();
            foreach (double d in lValues)
            {
                dSum -= d;
            }
            //(1-x)^m = 1 + \sum_i=1..m a_i -x^i
            //1-(1-x)^m = -\sum_i=1..m a_i -x^i
            return dSum;
        }


        /**
         * When computing we only add the probabilities of the permutations to the closest higher p-value table
         * Now, we need to go over all these tables and sum everything that has smaller p-value (more significant)
         * We take a mapping from p-value to single appearance probabilities
         * and teturn a mapping from p-value to sum of all more significant probabilities
         * */
        private void sumFDRs( List<ContingencyTable> actTables, Map<double, FDRData> slFDR, double dPiEstimation)
        {
            double dSum = 0;
            int iTable = 0;
            long cAllTables = actTables.Count;
            long cTables = 0;
            ContingencyTable ctCurrent = null, ctNext = null;
            double dFisherScore = 0.0, dNextFisherScore = 0.0;

            int iSample = 0;

            //First, sum all the pooled p-values that are lower than the current table
            foreach (FDRData data in slFDR.Values)
            {
                dSum += data.PooledPValue;
                data.PooledPValue = dSum;
                data.PooledPValue /= cAllTables;
                if( data.FilteringPi > 0.0 ) 
                    data.FDR = dSum * data.FilteringPi;
                else
                    data.FDR = dSum * dPiEstimation;
                if (m_bPositiveFDR)
                {
                    data.RejectionAreaProb = OneMinusOneMinuXToTheM(data.PooledPValue, cAllTables);
                    data.FDR /= data.RejectionAreaProb;
                }
                iSample++;
            }

            dSum = 0;
            //We now have to divide by the number of more significant tables to move from pooled p-values to FDR
            for (iTable = 0; iTable < actTables.Count; iTable++)
            {
                ctCurrent = (ContingencyTable)actTables[iTable];
                if (iTable < actTables.Count - 1)
                    ctNext = (ContingencyTable)actTables[iTable + 1];
                else
                    ctNext = null;
                dFisherScore = round( ctCurrent.getFisher2TailPermutationTest() );
                if (m_bHuge)//special case to huge datasets where the same table can appear multiple times
                    cTables += (long)m_ctcTableCounts.getCachedValue(ctCurrent);
                else
                    cTables++;
                if( ctNext != null )
                    dNextFisherScore = round( ctNext.getFisher2TailPermutationTest() );
                if ((ctNext == null) || (dFisherScore != dNextFisherScore))
                {
                    slFDR[dFisherScore].FDR /= cTables;
                }
            }
        }

        //returns the first key that is greater than dKey
        //implemented usign binary search
        private double getNextKey(IList<double> lKeys, double dKey)
        {
            return getNextKey(lKeys, dKey, 0, lKeys.Count - 1);
        }

        //returns the first key that is greater than dKey in the range [start,end]
        //implemented usign binary search
        private double getNextKey(IList<double> lKeys, double dKey, int iStart, int iEnd)
        {
            int idx = 0;
            if (lKeys[iStart] >= dKey)
                return lKeys[iStart];
            if (lKeys[iEnd] < dKey)
                return lKeys[iEnd + 1];
            if (iEnd - iStart <= 5)
            {
                for (idx = iStart; idx <= iEnd; idx++)
                {
                    if (lKeys[idx] >= dKey)
                        return lKeys[idx];
                }
                return double.NaN;
            }
            int iMedian = (iStart + iEnd) / 2;
            if (dKey == lKeys[iMedian])
                return lKeys[iMedian];
            if (dKey < lKeys[iMedian])
                return getNextKey(lKeys, dKey, iStart, iMedian - 1);
            return getNextKey(lKeys, dKey, iMedian + 1, iEnd);

        }
        
        //Given a mapping from Fisher score to FDR and an FDR value finds the appropriate Fisher score
        private double mapFDR2FisherScore(Map<double, FDRData> slFDR, double dFDR)
        {
            foreach (KeyValuePair<double, FDRData> p in slFDR)
            {
                if (p.Value.FDR >= dFDR)
                {
                    return p.Key;
                }
            }
            return 1.0;
        }

        /*
         * Writes the results of the computation to a file.
         * First line is the headers (if exist) with the new columns added.
         * Each following line is the contingency table with the Fisher scores, FDR and q-value
         * */
        private List<string> getResults( List<ContingencyTable> actTables
            , Map<double, FDRData> slFDR)
        {
            int iTable = 0;
            ContingencyTable ctCurrent = null;
            double dFisherTest = 0.0, dCurrentQValue = 0.0;
            double dNextKey = 0;
            string sHeader = "";
            FDRData fdCurrent = null;
            string sOutputLine = "";
            List<string> lResults = new List<string>();
            bool bFiltering = m_pmEvaluatePi == PiMethod.Filtering;

            if (m_bReportProgress)
            {
                m_bContinue = m_prReport.reportPhase("Writing results.");
            }

            sHeader = m_sColumnHeaders + "\tp-value";
            if (m_bFullOutput)
            {
                sHeader += "\tpooled p-value\t";
                if (bFiltering)
                    sHeader += "filtering pi\t";
                if (m_bPositiveFDR)
                    sHeader += "pr(R(p)>0)\tpFDR";
                else
                    sHeader += "FDR";
            }
            sHeader += "\tq-value";
            lResults.Add(sHeader);


            List<KeyValuePair<double, double>> lPToQMappings = new List<KeyValuePair<double, double>>();

            //When the huge flag is used, the tables are not kept. 
            //We now have to go over the entire input file, read each table,
            //compute p-value for it, and map it into FDR and q-value.
            if (m_bHuge)
            {
                StreamReader sr = m_fiInput.OpenText();
                string sLine = "";
                double dFisherScoreCutoff = 0.0;
                bool bUseTable = true;

                if (m_dFDRCutoff > 0.0)
                {
                    dFisherScoreCutoff = mapFDR2FisherScore(slFDR, m_dFDRCutoff);
                }

                iTable = 0;
                while (!sr.EndOfStream)
                {
                    sLine = sr.ReadLine();
                    if (sLine.Length > 0)
                    {
                        ctCurrent = new ContingencyTable(sLine, m_cTableNamesColumns);
                        bUseTable = ctCurrent.validate();
                        if (bUseTable)
                        {
                            dFisherTest = round( ctCurrent.getFisher2TailPermutationTest( dFisherScoreCutoff ) );
                            dNextKey = getNextKey( slFDR.KeyList, dFisherTest );
                            fdCurrent = slFDR[dNextKey];
                            dCurrentQValue = round(fdCurrent.QValue);
                            if (dCurrentQValue <= m_dFDRCutoff)
                            {
                                sOutputLine = ctCurrent.ToString() + "\t";
                                sOutputLine += fdCurrent.getData(m_bFullOutput, bFiltering, m_bPositiveFDR);
                                lResults.Add(sOutputLine);
                                lPToQMappings.Add(new KeyValuePair<double, double>(dNextKey, dCurrentQValue));//will not work for huge because multiple tables will be missed
                            }
                        }
                        iTable++;
                        if (m_bReportProgress && (iTable % MAX_REPROT_POINT == 0))
                        {
                            m_bContinue = m_prReport.reportProcessedTables(iTable, m_cTables);
                            m_bContinue = m_prReport.reportMessage("Written " + iTable + " tables.", true);
                        }
                    }
                }
                sr.Close();
            }
            else//Not huge - all data is already in memory - just write the tables.
            {
                for (iTable = 0; iTable < actTables.Count; iTable++)
                {
                    ctCurrent = (ContingencyTable)actTables[iTable];
                    dFisherTest = ctCurrent.getFisher2TailPermutationTest();
                    dNextKey = getNextKey(slFDR.KeyList, dFisherTest);
                    fdCurrent = slFDR[dNextKey];
                    dCurrentQValue = floor( fdCurrent.QValue );
                    if (dCurrentQValue <= m_dFDRCutoff)
                    {
                        sOutputLine = ctCurrent.ToString() + "\t";
                        sOutputLine += fdCurrent.getData(m_bFullOutput, bFiltering, m_bPositiveFDR);
                        lPToQMappings.Add(new KeyValuePair<double, double>(dNextKey, dCurrentQValue));
                        lResults.Add(sOutputLine);
                    }
                    if (m_bReportProgress && (iTable % MAX_REPROT_POINT == 0))
                    {
                        m_bContinue = m_prReport.reportProcessedTables(iTable, actTables.Count);
                    }


                    //swMarginalPValues.WriteLine(fdCurrent.PValue);



                }
            }
            PToQMapping = lPToQMappings;
            if (m_bReportProgress)
                m_bContinue = m_prReport.reportMessage("Done writing results", true);




            //swMarginalPValues.Close();


            return lResults;
        }

        public static double floor(double x)
        {
            string s = x.ToString("e5");
            string[] a = s.Split(new char[] { 'e' });
            double y = double.Parse(a[0]);
            s = y + "e" + a[1];
            double z = double.Parse(s);
            return z;
        }

        //Rounding 5 digits after the most significant digit
        public static double round(double x)
        {
            string s = x.ToString("e5");
            string[] a = s.Split(new char[] { 'e' });
            double y = double.Parse(a[0]);
            y += 0.00001;
            s = y + "e" + a[1];
            double z = double.Parse(s);
            return z;
        }
        /*
         * Main FDR computation function.
         * Takes as input an array of tables, already sorted by Fisher scores.
         * Outputs a map from p-value to FDR.
         * */
        private Map<double, FDRData> computeFDR(List<ContingencyTable> actTables)
        {
            int iTable = 0, cTables = actTables.Count;
            ContingencyTable ctCurrent = null;
            double dFirstLargerKey = 0.0;
            double dHyperProbability = 0.0, dFisherScore = 0.0;
            DateTime dtBefore = DateTime.Now, dtAfter = DateTime.Now;
            TimeSpan tsCurrent = TimeSpan.Zero, tsTotal = TimeSpan.Zero;
            int cTableCount = 1;
            int cReprotInterval = 0;
            Map<double, FDRData> slFDR = null;
            double dSumObservedPValues = 0.0, dCurrentTableFisherTestPValue = 0.0;
            double dSumNullPValues = 0.0, dExpectedNullPValue = 0.0;
            double dEPhiNull = 0.0, dEPhiObserved = 0.0;
            int cNullPValues = 0;
            int cObservedPValues = 0;

            if (m_bReportProgress)
            {
                m_bContinue = m_prReport.reportPhase("Computing pooled p-values.");
                m_bContinue = m_prReport.reportMessage("Started computing pooled p-values values.", true);
            }

            slFDR = initFDRMap(actTables);

            cReprotInterval = Math.Min(actTables.Count / 10, MAX_REPROT_POINT);

            for (iTable = 0; iTable < cTables && m_bContinue; iTable++)
            {
                ctCurrent = (ContingencyTable)actTables[iTable];

                dCurrentTableFisherTestPValue = ctCurrent.getFisher2TailPermutationTest();
                
                dSumObservedPValues += dCurrentTableFisherTestPValue;

                //dEPhiObserved += -Math.Log(1 - 0.99999999 * dCurrentTableFisherTestPValue);
                dEPhiObserved += Math.Sqrt( dCurrentTableFisherTestPValue );


                cObservedPValues++;

                double[,] adScores = ctCurrent.computeAllPermutationsScores();
                int iCurrent = 0;
                if (m_bHuge)
                    cTableCount = (int)m_ctcTableCounts.getCachedValue(ctCurrent);
                else
                    cTableCount = 1;

                for (iCurrent = 0; iCurrent < adScores.GetLength(0); iCurrent++)
                {
                    dHyperProbability = adScores[iCurrent, 0];
                    dFisherScore = adScores[iCurrent, 1];

                    dSumNullPValues += dHyperProbability;
                    dExpectedNullPValue += dFisherScore * dHyperProbability;
                    //dEPhiNull += -Math.Log(1 - 0.99999999 * dFisherScore) * dHyperProbability;
                    dEPhiNull += Math.Sqrt( dFisherScore ) * dHyperProbability;
                    cNullPValues++;

                    dFirstLargerKey = getNextKey(slFDR.KeyList, dFisherScore);

                    slFDR[dFirstLargerKey].PooledPValue += (dHyperProbability * cTableCount);
                }

                if ((iTable > 0) && (iTable % cReprotInterval == 0))
                {
                    if (m_bReportProgress)
                    {
                        dtAfter = DateTime.Now;
                        tsCurrent = dtAfter.Subtract(dtBefore);
                        tsTotal += tsCurrent;
                        m_bContinue = m_prReport.reportProcessedTables(iTable, cTables);
                        m_bContinue = m_prReport.reportMessage("Done " + iTable + " tables, avg time (ms) " + Math.Round(tsTotal.TotalMilliseconds / (iTable + 1)) +
                            ", total time " + tsTotal, true);
                    }
                }
            }

            double dPi = 1.0;
            if( (m_pmEvaluatePi == PiMethod.WeightedSum) || (m_pmEvaluatePi == PiMethod.DoubleAverage) )
            {
                if (m_pmEvaluatePi == PiMethod.WeightedSum) 
                    dPi = (dSumObservedPValues / cObservedPValues) / (dExpectedNullPValue / dSumNullPValues); // \pi_0 = (\sum_T p(T))/(\sum_T p(T)pr(T|H=0))
                else if (m_pmEvaluatePi == PiMethod.DoubleAverage)
                    dPi = 2.0 * (dSumObservedPValues / cObservedPValues); // \pi_0 = 2 * avg(p)

                double dPhiPi = dEPhiObserved / dEPhiNull;


                m_bContinue = m_prReport.reportMessage("Estimating PI = " + dPi, true);
            }
            else if (m_pmEvaluatePi == PiMethod.Filtering)
            {
                Map<double, double> slPi = computeFilteringPi(actTables, slFDR.KeyList);
                List<double> lKeys = new List<double>(slFDR.Keys);
                foreach (double dKey in lKeys)
                {
                    slFDR[dKey].FilteringPi = slPi[dKey];
                }
            }
            m_dPi = dPi;
            sumFDRs(actTables, slFDR, dPi);
            return slFDR;
        }

        /*
         * Computing pi in the filtering case.
         * In this case we compute a different pi for each p-value
         * A table is considered relevant for the pi computation of a p-value p only if its marginals support a p-value that is more extreme than p.
         * */
        private Map<double, double> computeFilteringPi( List<ContingencyTable> actTables, List<double> lPValues )
        {
            Map<double, List<ContingencyTable>> slRelevantTables = new Map<double, List<ContingencyTable>>();
            double dSumObservedPValuesInRange = 0.0, dCurrentTableFisherTestPValue = 0.0;
            int cObservedTablesInRange = 0;
            double dFisherScore = 0.0, dHyperProbability = 0.0, dMinimalPossiblePValue = 0.0, dFirstLargerKey = 0.0;
            double dSumExpectedNullsInRange = 0;
            double dSumNullProbsInRange = 0.0;
            int cNullsInRange = 0;
            int iTable = 0;
            Map<double, double> slPi = new Map<double,double>();
            ContingencyTable ctCurrent = null;

            if (m_bReportProgress)
            {
                m_bContinue = m_prReport.reportPhase("Computing relevant tables.");
                m_bContinue = m_prReport.reportMessage("Started computing relevant tables for PI computation.", true);
            }

            //We first compute the list of relevant tables.
            //For each table we compute its minimal achievable p-value and add it to the next p-value on the list.
            //Now, the relevant tables are all the tables that belong to a p-value that is more exterme than the current one.
            for (iTable = 0; iTable < actTables.Count && m_bContinue; iTable++)
            {
                ctCurrent = (ContingencyTable)actTables[iTable];
                dMinimalPossiblePValue = ctCurrent.getMinimalAchievablePValue();
                dFirstLargerKey = getNextKey(lPValues, dMinimalPossiblePValue);
                if (!slRelevantTables.ContainsKey(dFirstLargerKey))
                    slRelevantTables.Add(dFirstLargerKey, new List<ContingencyTable>());
                slRelevantTables[dFirstLargerKey].Add(ctCurrent);
                if( m_bReportProgress && ( iTable > 0 ) && ( iTable % 1000 == 0 ) )
                    m_bContinue = m_prReport.reportProcessedTables(iTable, actTables.Count);
            }

            //We iterate from smallest p-value to largest. The order is important because we want the relevant tables list to grow all the time.
            for (iTable = 0; iTable < actTables.Count && m_bContinue; iTable++)
            {
                ctCurrent = (ContingencyTable)actTables[iTable];

                dCurrentTableFisherTestPValue = round(ctCurrent.getFisher2TailPermutationTest());
                
                if (slRelevantTables.ContainsKey(dCurrentTableFisherTestPValue))
                {
                    //Now we iterate over the list of relevant tables
                    //Note - a table never becomes irrelevant. Therefore we always accumulate more observations and remove any.
                    foreach (ContingencyTable ctRelevant in slRelevantTables[dCurrentTableFisherTestPValue])
                    {
                        dFisherScore = ctRelevant.getFisher2TailPermutationTest();

                        dSumObservedPValuesInRange += dFisherScore;
                        cObservedTablesInRange++;
                        //TODO - calling computeAllPermutationsScores twice - inefficient
                        double[,] adScores = ctRelevant.computeAllPermutationsScores();

                        for (int iCurrent = 0; iCurrent < adScores.GetLength(0); iCurrent++)
                        {
                            dHyperProbability = adScores[iCurrent, 0];
                            dFisherScore = adScores[iCurrent, 1];

                            dSumNullProbsInRange += dHyperProbability;
                            dSumExpectedNullsInRange += dFisherScore * dHyperProbability;
                            cNullsInRange++;
                        }
                    }
                    slRelevantTables.Remove(dCurrentTableFisherTestPValue);
                }
                //After iterating over all the relevant tables we compute the PI for that p-value
                //using the weighted sum method
                slPi[dCurrentTableFisherTestPValue] = (dSumObservedPValuesInRange / cObservedTablesInRange) /
                                                                        (dSumExpectedNullsInRange / dSumNullProbsInRange);
                if (m_bReportProgress && (iTable > 0) && (iTable % 1000 == 0))
                    m_bContinue = m_prReport.reportProcessedTables(iTable, actTables.Count);
            }
            slPi[10.0] = 1.0;
            return slPi;
        }

        private Map<double, FDRData> initFDRMap(List<ContingencyTable> actTables)
        {
            Map<double, FDRData> slFDR = new Map<double, FDRData>();
            int iTable = 0;
            ContingencyTable ctCurrent = null;
            double dFisherScore = 0.0;
            for (iTable = 0; iTable < actTables.Count ; iTable++)
            {
                ctCurrent = (ContingencyTable)actTables[iTable];
                dFisherScore = round(ctCurrent.getFisher2TailPermutationTest());
                if (!slFDR.ContainsKey(dFisherScore))
                {
                    slFDR.Add(dFisherScore, new FDRData(dFisherScore));
                }
            }
            slFDR.Add(10.0, new FDRData(10.0)); // add a last entry with a huge fisher score
            return slFDR;
        }
 
        //Compute the maximal difference between two lists of FDR values.
        //We use this in the sampling scenario to determine convergence.
        private double getMaxDifference(Map<double, FDRData> slOld, Map<double, FDRData> slNew)
        {
            double dDiff = 0.0;
            double dKey = 0.0, dOldKey = 0.0, dValue = 0.0, dOldValue = 0.0;
            foreach (KeyValuePair<double, FDRData> pEntry in slNew)
            {
                dKey = pEntry.Key;
                dValue = pEntry.Value.FDR;
                dOldKey = getNextKey(slOld.KeyList, dKey);
                if (dKey < 1.0 && dOldKey < 1.0)
                {
                    dOldValue = slOld[dOldKey].FDR;
                    if (Math.Abs(dOldValue - dValue) > dDiff)
                    {
                        dDiff = Math.Abs(dOldValue - dValue);
                    }
                }
            }
            return dDiff;
        }
        
        //Writing the intermidiate p-value to FDR mapping
        private void writeFDRTable(string sFileName, Map<double, FDRData> slFDR)
        {
            StreamWriter sw = new StreamWriter(sFileName);
            foreach (FDRData data in slFDR.Values)
            {
                sw.WriteLine(data.PValue + "\t" + data.FDR);
            }
            sw.Close();
        }

        public void computeFDR(string sInputFile, string sOutputFile)
        {
            m_fiInput = new FileInfo(sInputFile);
            List<string> lResults = computeFDR();
            if( lResults != null )
                writeResults(lResults, sOutputFile);
        }

        private void writeResults(List<string> lResults, string sOutputFile)
        {
            try
            {
                StreamWriter sw = new StreamWriter(sOutputFile);
                foreach (string sLine in lResults)
                    sw.WriteLine(sLine);
                sw.Close();
            }
            catch (Exception e)
            {
                m_prReport.reportError("Could not load write the results: " + e);
            }
        }

        /*
         * The main function for computing FDR.
         * Implements the sampling iterations in the case of a huge dataset.
         * */
        public List<string> computeFDR()
        {
            Map<double, FDRData> slFDR = null, slFDRPrevious = null;
            List<ContingencyTable> actTables = null;
            double dMaxDiff = double.PositiveInfinity;
            int iIteration = 0;
            
            if (m_bReportProgress)
                m_bContinue = m_prReport.reportMessage("Started computing FDR", true);

            do //sampling iterations
            {
                if (( m_dMinimalChange > 0.0 ) && m_bReportProgress)
                    m_bContinue = m_prReport.reportMessage("Begining iteration " + iIteration + " - sampling " + m_iSampleSize + " tables.", true);

                slFDRPrevious = slFDR;

                //load the tables
                actTables = loadTables();
                if (!m_bContinue)
                    return null;
                //compute p-values and sort the tables
                computeFisherScores(actTables);
                if (!m_bContinue)
                    return null;

                //compute the FDR
                slFDR = computeFDR(actTables);
                if (!m_bContinue)
                    return null;

                //Compute the maximal change in FDR (convergence of sampling process)
                if (slFDRPrevious != null)
                    dMaxDiff = getMaxDifference(slFDRPrevious, slFDR);

                if ((m_dMinimalChange > 0.0) && m_bReportProgress)
                    m_bContinue = m_prReport.reportMessage("Done iteration " + iIteration + " - maximal change in FDR " + dMaxDiff, true);
                //Double the sample size
                m_iSampleSize *= 2;
                iIteration++;
            }//if we are in sampling mode, and the FDR has not yet converged, run another iteration
            while ((m_dMinimalChange > 0.0) && (dMaxDiff > m_dMinimalChange) && m_bContinue);
            //Compute the q-values - the minimal FDR over all the rejection areas
            computeQValues(slFDR);

            if (m_bReportProgress)
                m_bContinue = m_prReport.reportMessage("Done computing FDR", true);

            //Write the results to a file
            List<string> lResults = getResults(actTables, slFDR);
            return lResults;
        }

        //q-value is the minimal FDR over all the rejection areas that a table belongs to
        private void computeQValues(Map<double, FDRData> slFDR)
        {
            if (m_bReportProgress)
            {
                m_bContinue = m_prReport.reportPhase("Computing q-values.");
                m_bContinue = m_prReport.reportMessage("Computing q-values", true);
            }
            double dMin = double.PositiveInfinity;
            int iTable = slFDR.Count - 1;
            double dFisherScore = 0.0, dFDR = 0.0;
            List<double> lKeys = new List<double>(slFDR.Keys);
            while (iTable >= 0)
            {
                dFisherScore = lKeys[iTable];
                dFDR = slFDR[dFisherScore].FDR;
                if (dFDR < dMin)
                    dMin = dFDR;
                slFDR[dFisherScore].QValue = dMin;
                iTable--;
            }
            if (m_bReportProgress)
                m_bContinue = m_prReport.reportMessage("Done computing q-values", true);
        }

    }
}
