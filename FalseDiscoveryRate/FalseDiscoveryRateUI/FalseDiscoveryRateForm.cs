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
    public partial class FalseDiscoveryRateForm : Form
    {
        public FalseDiscoveryRateForm()
        {
            InitializeComponent();
        }

        private void btnInputFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = txtInputFile.Text;
            dlg.ShowDialog();
            txtInputFile.Text = dlg.FileName;
        }

        private void FalseDiscoveryRateForm_Load(object sender, EventArgs e)
        {
            cmbPIMethod.SelectedIndex = 0;
            this.MaximizeBox = false;
        }

        private void btnOutputFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = txtOutputFile.Text; 
            dlg.ShowDialog();
            txtOutputFile.Text = dlg.FileName;
        }

        private void chkSampling_CheckedChanged(object sender, EventArgs e)
        {
            bool bEnabled = chkSampling.Checked;
            chkAutomatedSampling.Enabled = bEnabled;
            lblSampleSize.Enabled = bEnabled;
            udSampleSize.Enabled = bEnabled;
        }

        private void btnCompute_Click(object sender, EventArgs e)
        {
            bool bHasColumnHeaders = chkColumnHeaders.Checked;
            bool bFullOutput = chkFullOutput.Checked;
            bool bReportProgress = chkReportProgress.Checked;
            bool bHuge = chkHuge.Checked;
            FalseDiscoveryRate.PiMethod mPi = FalseDiscoveryRate.PiMethod.One;
            if (cmbPIMethod.SelectedIndex == 0)
                mPi = FalseDiscoveryRate.PiMethod.One;
            else if (cmbPIMethod.SelectedIndex == 1)
                mPi = FalseDiscoveryRate.PiMethod.WeightedSum;
            if (cmbPIMethod.SelectedIndex == 2)
                mPi = FalseDiscoveryRate.PiMethod.DoubleAverage;
            if (chkFiltering.Checked)
                mPi = FalseDiscoveryRate.PiMethod.Filtering;

            double dCutoff = (double)udFDRCutoff.Value;

            int cTableNamesColumns = (int)udNameColumns.Value - 1;

            bool bPositiveFDR = chkPFDR.Checked;

            bool bSampling = chkSampling.Checked;
            bool bAutomatedSampling = false;
            int iSampleSize = 0;
            double dConvergenceEpsilon = 0.0;
            if (bSampling)
            {
                iSampleSize = (int)udSampleSize.Value;
                bAutomatedSampling = chkAutomatedSampling.Checked;
                if (bAutomatedSampling)
                    dConvergenceEpsilon = 0.001;
            }


            string sInputFile = txtInputFile.Text;
            string sOutputFile = txtOutputFile.Text;

            DateTime dtBefore = DateTime.Now;
            FalseDiscoveryRateComputationTask task = new FalseDiscoveryRateComputationTask(sInputFile, sOutputFile);
            FalseDiscoveryRate t = new FalseDiscoveryRate(cTableNamesColumns, bReportProgress, dCutoff, bHuge, iSampleSize, dConvergenceEpsilon, bHasColumnHeaders, mPi, bPositiveFDR, bFullOutput, task);
            task.setTask(t);
            task.run();
            DateTime dtAfter = DateTime.Now;
        }

        private void txtInputFile_TextChanged(object sender, EventArgs e)
        {
            if (txtInputFile.Text.Length > 0 && txtOutputFile.Text.Length > 0)
                cmdCompute.Enabled = true;
            else
                cmdCompute.Enabled = false;
        }

        private void txtOutputFile_TextChanged(object sender, EventArgs e)
        {
            if (txtInputFile.Text.Length > 0 && txtOutputFile.Text.Length > 0)
                cmdCompute.Enabled = true;
            else
                cmdCompute.Enabled = false;
        }

        private void chkFiltering_CheckedChanged(object sender, EventArgs e)
        {
            lblPI.Enabled = !chkFiltering.Checked;
            cmbPIMethod.Enabled = !chkFiltering.Checked;
        }
 
    }
}
