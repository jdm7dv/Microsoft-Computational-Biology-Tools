namespace FalseDiscoveryRateUI
{
    partial class FalseDiscoveryRateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FalseDiscoveryRateForm));
            this.label1 = new System.Windows.Forms.Label();
            this.txtInputFile = new System.Windows.Forms.TextBox();
            this.btnInputFile = new System.Windows.Forms.Button();
            this.btnOutputFile = new System.Windows.Forms.Button();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.udNameColumns = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.chkColumnHeaders = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkFiltering = new System.Windows.Forms.CheckBox();
            this.chkPFDR = new System.Windows.Forms.CheckBox();
            this.lblPI = new System.Windows.Forms.Label();
            this.chkHuge = new System.Windows.Forms.CheckBox();
            this.cmbPIMethod = new System.Windows.Forms.ComboBox();
            this.udSampleSize = new System.Windows.Forms.NumericUpDown();
            this.chkAutomatedSampling = new System.Windows.Forms.CheckBox();
            this.lblSampleSize = new System.Windows.Forms.Label();
            this.chkSampling = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.udFDRCutoff = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.chkFullOutput = new System.Windows.Forms.CheckBox();
            this.chkReportProgress = new System.Windows.Forms.CheckBox();
            this.cmdCompute = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udNameColumns)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udSampleSize)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udFDRCutoff)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input file:";
            // 
            // txtInputFile
            // 
            this.txtInputFile.Location = new System.Drawing.Point(74, 17);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.Size = new System.Drawing.Size(214, 20);
            this.txtInputFile.TabIndex = 1;
            this.txtInputFile.TextChanged += new System.EventHandler(this.txtInputFile_TextChanged);
            // 
            // btnInputFile
            // 
            this.btnInputFile.Location = new System.Drawing.Point(294, 15);
            this.btnInputFile.Name = "btnInputFile";
            this.btnInputFile.Size = new System.Drawing.Size(25, 22);
            this.btnInputFile.TabIndex = 2;
            this.btnInputFile.Text = "...";
            this.btnInputFile.UseVisualStyleBackColor = true;
            this.btnInputFile.Click += new System.EventHandler(this.btnInputFile_Click);
            // 
            // btnOutputFile
            // 
            this.btnOutputFile.Location = new System.Drawing.Point(294, 53);
            this.btnOutputFile.Name = "btnOutputFile";
            this.btnOutputFile.Size = new System.Drawing.Size(25, 22);
            this.btnOutputFile.TabIndex = 5;
            this.btnOutputFile.Text = "...";
            this.btnOutputFile.UseVisualStyleBackColor = true;
            this.btnOutputFile.Click += new System.EventHandler(this.btnOutputFile_Click);
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Location = new System.Drawing.Point(74, 55);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.Size = new System.Drawing.Size(214, 20);
            this.txtOutputFile.TabIndex = 4;
            this.txtOutputFile.TextChanged += new System.EventHandler(this.txtOutputFile_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output file:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.udNameColumns);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.chkColumnHeaders);
            this.groupBox1.Location = new System.Drawing.Point(12, 107);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(323, 71);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data properties";
            // 
            // udNameColumns
            // 
            this.udNameColumns.Location = new System.Drawing.Point(174, 45);
            this.udNameColumns.Name = "udNameColumns";
            this.udNameColumns.Size = new System.Drawing.Size(33, 20);
            this.udNameColumns.TabIndex = 2;
            this.udNameColumns.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(144, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Table counts start on column";
            // 
            // chkColumnHeaders
            // 
            this.chkColumnHeaders.AutoSize = true;
            this.chkColumnHeaders.Checked = true;
            this.chkColumnHeaders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkColumnHeaders.Location = new System.Drawing.Point(17, 22);
            this.chkColumnHeaders.Name = "chkColumnHeaders";
            this.chkColumnHeaders.Size = new System.Drawing.Size(186, 17);
            this.chkColumnHeaders.TabIndex = 0;
            this.chkColumnHeaders.Text = "First row contains column headers";
            this.chkColumnHeaders.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkFiltering);
            this.groupBox2.Controls.Add(this.chkPFDR);
            this.groupBox2.Controls.Add(this.lblPI);
            this.groupBox2.Controls.Add(this.chkHuge);
            this.groupBox2.Controls.Add(this.cmbPIMethod);
            this.groupBox2.Controls.Add(this.udSampleSize);
            this.groupBox2.Controls.Add(this.chkAutomatedSampling);
            this.groupBox2.Controls.Add(this.lblSampleSize);
            this.groupBox2.Controls.Add(this.chkSampling);
            this.groupBox2.Location = new System.Drawing.Point(12, 189);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(323, 214);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Operation properties";
            // 
            // chkFiltering
            // 
            this.chkFiltering.AutoSize = true;
            this.chkFiltering.Location = new System.Drawing.Point(17, 31);
            this.chkFiltering.Name = "chkFiltering";
            this.chkFiltering.Size = new System.Drawing.Size(125, 17);
            this.chkFiltering.TabIndex = 8;
            this.chkFiltering.Text = "Filter irrelevant tables";
            this.chkFiltering.UseVisualStyleBackColor = true;
            this.chkFiltering.CheckedChanged += new System.EventHandler(this.chkFiltering_CheckedChanged);
            // 
            // chkPFDR
            // 
            this.chkPFDR.AutoSize = true;
            this.chkPFDR.Location = new System.Drawing.Point(17, 87);
            this.chkPFDR.Name = "chkPFDR";
            this.chkPFDR.Size = new System.Drawing.Size(170, 17);
            this.chkPFDR.TabIndex = 7;
            this.chkPFDR.Text = "Compute Positive FDR (pFDR)";
            this.chkPFDR.UseVisualStyleBackColor = true;
            // 
            // lblPI
            // 
            this.lblPI.AutoSize = true;
            this.lblPI.Location = new System.Drawing.Point(17, 59);
            this.lblPI.Name = "lblPI";
            this.lblPI.Size = new System.Drawing.Size(107, 13);
            this.lblPI.TabIndex = 6;
            this.lblPI.Text = "PI evaluation method";
            // 
            // chkHuge
            // 
            this.chkHuge.AutoSize = true;
            this.chkHuge.Location = new System.Drawing.Point(17, 184);
            this.chkHuge.Name = "chkHuge";
            this.chkHuge.Size = new System.Drawing.Size(119, 17);
            this.chkHuge.TabIndex = 4;
            this.chkHuge.Text = "Huge dataset mode";
            this.chkHuge.UseVisualStyleBackColor = true;
            // 
            // cmbPIMethod
            // 
            this.cmbPIMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPIMethod.FormattingEnabled = true;
            this.cmbPIMethod.Items.AddRange(new object[] {
            "PI = 1",
            "sum observed p-values / expected p-value",
            "2*avg(p-value)"});
            this.cmbPIMethod.Location = new System.Drawing.Point(142, 56);
            this.cmbPIMethod.Name = "cmbPIMethod";
            this.cmbPIMethod.Size = new System.Drawing.Size(175, 21);
            this.cmbPIMethod.TabIndex = 5;
            // 
            // udSampleSize
            // 
            this.udSampleSize.Enabled = false;
            this.udSampleSize.Location = new System.Drawing.Point(155, 132);
            this.udSampleSize.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.udSampleSize.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udSampleSize.Name = "udSampleSize";
            this.udSampleSize.Size = new System.Drawing.Size(95, 20);
            this.udSampleSize.TabIndex = 3;
            this.udSampleSize.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // chkAutomatedSampling
            // 
            this.chkAutomatedSampling.AutoSize = true;
            this.chkAutomatedSampling.Enabled = false;
            this.chkAutomatedSampling.Location = new System.Drawing.Point(36, 155);
            this.chkAutomatedSampling.Name = "chkAutomatedSampling";
            this.chkAutomatedSampling.Size = new System.Drawing.Size(121, 17);
            this.chkAutomatedSampling.TabIndex = 2;
            this.chkAutomatedSampling.Text = "Automated sampling";
            this.chkAutomatedSampling.UseVisualStyleBackColor = true;
            // 
            // lblSampleSize
            // 
            this.lblSampleSize.AutoSize = true;
            this.lblSampleSize.Enabled = false;
            this.lblSampleSize.Location = new System.Drawing.Point(36, 134);
            this.lblSampleSize.Name = "lblSampleSize";
            this.lblSampleSize.Size = new System.Drawing.Size(100, 13);
            this.lblSampleSize.TabIndex = 1;
            this.lblSampleSize.Text = "Sample size (tables)";
            // 
            // chkSampling
            // 
            this.chkSampling.AutoSize = true;
            this.chkSampling.Location = new System.Drawing.Point(17, 113);
            this.chkSampling.Name = "chkSampling";
            this.chkSampling.Size = new System.Drawing.Size(89, 17);
            this.chkSampling.TabIndex = 0;
            this.chkSampling.Text = "Use sampling";
            this.chkSampling.UseVisualStyleBackColor = true;
            this.chkSampling.CheckedChanged += new System.EventHandler(this.chkSampling_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.udFDRCutoff);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.chkFullOutput);
            this.groupBox3.Controls.Add(this.chkReportProgress);
            this.groupBox3.Location = new System.Drawing.Point(12, 409);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(323, 119);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Output properties";
            // 
            // udFDRCutoff
            // 
            this.udFDRCutoff.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.udFDRCutoff.Location = new System.Drawing.Point(217, 84);
            this.udFDRCutoff.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.udFDRCutoff.Name = "udFDRCutoff";
            this.udFDRCutoff.Size = new System.Drawing.Size(33, 20);
            this.udFDRCutoff.TabIndex = 3;
            this.udFDRCutoff.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 86);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(200, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Output only tables with q-value less than ";
            // 
            // chkFullOutput
            // 
            this.chkFullOutput.AutoSize = true;
            this.chkFullOutput.Location = new System.Drawing.Point(17, 51);
            this.chkFullOutput.Name = "chkFullOutput";
            this.chkFullOutput.Size = new System.Drawing.Size(182, 17);
            this.chkFullOutput.TabIndex = 1;
            this.chkFullOutput.Text = "Output all the computed statistics";
            this.chkFullOutput.UseVisualStyleBackColor = true;
            // 
            // chkReportProgress
            // 
            this.chkReportProgress.AutoSize = true;
            this.chkReportProgress.Checked = true;
            this.chkReportProgress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkReportProgress.Location = new System.Drawing.Point(17, 28);
            this.chkReportProgress.Name = "chkReportProgress";
            this.chkReportProgress.Size = new System.Drawing.Size(166, 17);
            this.chkReportProgress.TabIndex = 0;
            this.chkReportProgress.Text = "Report progress while running";
            this.chkReportProgress.UseVisualStyleBackColor = true;
            // 
            // cmdCompute
            // 
            this.cmdCompute.Enabled = false;
            this.cmdCompute.Location = new System.Drawing.Point(115, 534);
            this.cmdCompute.Name = "cmdCompute";
            this.cmdCompute.Size = new System.Drawing.Size(96, 24);
            this.cmdCompute.TabIndex = 9;
            this.cmdCompute.Text = "Compute";
            this.cmdCompute.UseVisualStyleBackColor = true;
            this.cmdCompute.Click += new System.EventHandler(this.btnCompute_Click);
            // 
            // FalseDiscoveryRateForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(348, 566);
            this.Controls.Add(this.cmdCompute);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOutputFile);
            this.Controls.Add(this.txtOutputFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnInputFile);
            this.Controls.Add(this.txtInputFile);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FalseDiscoveryRateForm";
            this.Text = "False Discovery Rate Tool";
            this.Load += new System.EventHandler(this.FalseDiscoveryRateForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udNameColumns)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udSampleSize)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udFDRCutoff)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInputFile;
        private System.Windows.Forms.Button btnInputFile;
        private System.Windows.Forms.Button btnOutputFile;
        private System.Windows.Forms.TextBox txtOutputFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown udNameColumns;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkColumnHeaders;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkAutomatedSampling;
        private System.Windows.Forms.Label lblSampleSize;
        private System.Windows.Forms.CheckBox chkSampling;
        private System.Windows.Forms.NumericUpDown udSampleSize;
        private System.Windows.Forms.CheckBox chkHuge;
        private System.Windows.Forms.Label lblPI;
        private System.Windows.Forms.ComboBox cmbPIMethod;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown udFDRCutoff;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkFullOutput;
        private System.Windows.Forms.CheckBox chkReportProgress;
        private System.Windows.Forms.Button cmdCompute;
        private System.Windows.Forms.CheckBox chkPFDR;
        private System.Windows.Forms.CheckBox chkFiltering;
    }
}

