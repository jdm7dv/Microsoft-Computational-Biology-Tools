using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FalseDiscoveryRateClasses;

namespace FalseDiscoveryRateUI
{
    public partial class FalseDiscoveryRateComputationTask : Form, ProgressReport
    {
        FalseDiscoveryRate m_fdrTask;
        string m_sInputFile, m_sOutputFile;
        bool m_bCancel;
        bool m_bError;

        public FalseDiscoveryRateComputationTask( string sInputFile, string sOutputFile )
        {
            InitializeComponent();
            m_fdrTask = null;
            m_bCancel = false;
            m_sInputFile = sInputFile;
            m_sOutputFile = sOutputFile;
        }

        private void FalseDiscoveryRateComputationTask_Load(object sender, EventArgs e)
        {
        }

        public void run()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(runFDR);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(endFDR);
            this.Show();
            bw.RunWorkerAsync();
        }


        private void runFDR(object sender, DoWorkEventArgs e)
        {
            m_fdrTask.computeFDR(m_sInputFile, m_sOutputFile);
            //m_fdrTask.release();
            this.Invoke((MethodInvoker)delegate()
            {
                pbProgress.Value = pbProgress.Maximum;
            });
        }

        private void endFDR(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_bError)
                lblPhase.Text = "Operation failed!";
            else if (m_bCancel)
                lblPhase.Text = "Operation canceled!";
            else
                lblPhase.Text = "Done!";
            btnOk.Enabled = true;
        }

        #region ProgressReport Members

        public bool reportProcessedTables(int cProccessedTables, int cAllTables)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                pbProgress.Maximum = cAllTables;
                pbProgress.Value = cProccessedTables;
            });
            return !m_bCancel;
        }

        public bool reportPhase(string sPhase)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                lblPhase.Text = sPhase;
            });
            return !m_bCancel;
        }

        public bool reportMessage(string sMessage, bool bNewLine)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                txtMessages.Text += sMessage;
                if (bNewLine)
                    txtMessages.Text += Environment.NewLine;
                txtMessages.Select(txtMessages.Text.Length, 0);
                txtMessages.ScrollToCaret();
            });
            return !m_bCancel;
        }

        public bool reportError(string sError)
        {
            MessageBox.Show(sError, "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            reportMessage(sError, true);
            return false;
        }

        #endregion

        private void btnCancel_Click(object sender, EventArgs e)
        {
            m_bCancel = true;
            btnOk.Enabled = true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        internal void setTask(FalseDiscoveryRate t)
        {
            m_fdrTask = t;
        }
    }
}
