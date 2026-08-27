namespace PowerPagesAlmDriftInspector.Controls
{
    partial class PowerPagesAlmDriftInspectorControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources are being disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.btnSelectSource = new System.Windows.Forms.Button();
            this.btnSelectTarget = new System.Windows.Forms.Button();
            this.btnCompare = new System.Windows.Forms.Button();
            this.dgvWebsites = new System.Windows.Forms.DataGridView();
            this.dgvSiteSettings = new System.Windows.Forms.DataGridView();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblWebsiteCount = new System.Windows.Forms.Label();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblSourceSnapshot = new System.Windows.Forms.Label();
            this.lblTargetSnapshot = new System.Windows.Forms.Label();
            this.lblSettingsCount = new System.Windows.Forms.Label();
            this.lblFilterSettings = new System.Windows.Forms.Label();
            this.txtFilterSettings = new System.Windows.Forms.TextBox();
            this.lblStatusFilter = new System.Windows.Forms.Label();
            this.cboStatusFilter = new System.Windows.Forms.ComboBox();
            this.chkFindingsOnly = new System.Windows.Forms.CheckBox();
            this.lblCategoryFilter = new System.Windows.Forms.Label();
            this.cboCategoryFilter = new System.Windows.Forms.ComboBox();
            this.btnClearFilter = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnExportHtml = new System.Windows.Forms.Button();
            this.lblWebsitesEmptyState = new System.Windows.Forms.Label();
            this.lblBottomGridEmptyState = new System.Windows.Forms.Label();
            this.sourcePanel = new System.Windows.Forms.Panel();
            this.targetPanel = new System.Windows.Forms.Panel();
            this.lblSourceCaption = new System.Windows.Forms.Label();
            this.lblTargetCaption = new System.Windows.Forms.Label();
            this.lblActivity = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWebsites)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSiteSettings)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectSource
            // 
            this.btnSelectSource.Location = new System.Drawing.Point(8, 8);
            this.btnSelectSource.Name = "btnSelectSource";
            this.btnSelectSource.Size = new System.Drawing.Size(120, 28);
            this.btnSelectSource.TabIndex = 0;
            this.btnSelectSource.Text = "Select Source";
            this.btnSelectSource.UseVisualStyleBackColor = true;
            this.btnSelectSource.Click += new System.EventHandler(this.btnSelectSource_Click);
            // 
            // btnSelectTarget
            // 
            this.btnSelectTarget.Location = new System.Drawing.Point(136, 8);
            this.btnSelectTarget.Name = "btnSelectTarget";
            this.btnSelectTarget.Size = new System.Drawing.Size(120, 28);
            this.btnSelectTarget.TabIndex = 1;
            this.btnSelectTarget.Text = "Select Target";
            this.btnSelectTarget.UseVisualStyleBackColor = true;
            this.btnSelectTarget.Click += new System.EventHandler(this.btnSelectTarget_Click);
            // 
            // btnCompare
            // 
            this.btnCompare.Location = new System.Drawing.Point(264, 8);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(150, 28);
            this.btnCompare.TabIndex = 2;
            this.btnCompare.Text = "Load and Compare";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // lblWebsiteCount
            // 
            this.lblWebsiteCount.AutoSize = true;
            this.lblWebsiteCount.Location = new System.Drawing.Point(430, 14);
            this.lblWebsiteCount.Name = "lblWebsiteCount";
            this.lblWebsiteCount.Size = new System.Drawing.Size(117, 13);
            this.lblWebsiteCount.TabIndex = 3;
            this.lblWebsiteCount.Text = "Selected Websites: 0";
            // 
            // lblSummary
            // 
            this.lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSummary.AutoEllipsis = true;
            this.lblSummary.Location = new System.Drawing.Point(570, 14);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(756, 16);
            this.lblSummary.TabIndex = 4;
            this.lblSummary.Text = "Summary: Not compared";
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSourceSnapshot
            // 
            this.lblSourceSnapshot.AutoEllipsis = true;
            this.lblSourceSnapshot.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSourceSnapshot.ForeColor = System.Drawing.Color.Firebrick;
            this.lblSourceSnapshot.Location = new System.Drawing.Point(12, 27);
            this.lblSourceSnapshot.Name = "lblSourceSnapshot";
            this.lblSourceSnapshot.Size = new System.Drawing.Size(618, 19);
            this.lblSourceSnapshot.TabIndex = 7;
            this.lblSourceSnapshot.Text = "Not connected";
            // 
            // lblTargetSnapshot
            // 
            this.lblTargetSnapshot.AutoEllipsis = true;
            this.lblTargetSnapshot.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblTargetSnapshot.ForeColor = System.Drawing.Color.Firebrick;
            this.lblTargetSnapshot.Location = new System.Drawing.Point(12, 27);
            this.lblTargetSnapshot.Name = "lblTargetSnapshot";
            this.lblTargetSnapshot.Size = new System.Drawing.Size(626, 19);
            this.lblTargetSnapshot.TabIndex = 8;
            this.lblTargetSnapshot.Text = "Not selected";
            // 
            // sourcePanel
            // 
            this.sourcePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
            this.sourcePanel.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.sourcePanel.Controls.Add(this.lblSourceCaption);
            this.sourcePanel.Controls.Add(this.lblSourceSnapshot);
            this.sourcePanel.Location = new System.Drawing.Point(8, 44);
            this.sourcePanel.Name = "sourcePanel";
            this.sourcePanel.Size = new System.Drawing.Size(650, 54);
            this.sourcePanel.TabIndex = 26;
            // 
            // targetPanel
            // 
            this.targetPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.targetPanel.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.targetPanel.Controls.Add(this.lblTargetCaption);
            this.targetPanel.Controls.Add(this.lblTargetSnapshot);
            this.targetPanel.Location = new System.Drawing.Point(670, 44);
            this.targetPanel.Name = "targetPanel";
            this.targetPanel.Size = new System.Drawing.Size(658, 54);
            this.targetPanel.TabIndex = 27;
            // 
            // lblSourceCaption
            // 
            this.lblSourceCaption.AutoSize = true;
            this.lblSourceCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblSourceCaption.Location = new System.Drawing.Point(12, 7);
            this.lblSourceCaption.Name = "lblSourceCaption";
            this.lblSourceCaption.Size = new System.Drawing.Size(142, 15);
            this.lblSourceCaption.TabIndex = 0;
            this.lblSourceCaption.Text = "SOURCE ENVIRONMENT";
            // 
            // lblTargetCaption
            // 
            this.lblTargetCaption.AutoSize = true;
            this.lblTargetCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTargetCaption.Location = new System.Drawing.Point(12, 7);
            this.lblTargetCaption.Name = "lblTargetCaption";
            this.lblTargetCaption.Size = new System.Drawing.Size(138, 15);
            this.lblTargetCaption.TabIndex = 0;
            this.lblTargetCaption.Text = "TARGET ENVIRONMENT";
            // 
            // dgvWebsites
            // 
            this.dgvWebsites.AllowUserToAddRows = false;
            this.dgvWebsites.AllowUserToDeleteRows = false;
            this.dgvWebsites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvWebsites.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvWebsites.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWebsites.Location = new System.Drawing.Point(8, 106);
            this.dgvWebsites.MultiSelect = false;
            this.dgvWebsites.Name = "dgvWebsites";
            this.dgvWebsites.ReadOnly = true;
            this.dgvWebsites.RowHeadersVisible = false;
            this.dgvWebsites.RowHeadersWidth = 25;
            this.dgvWebsites.RowTemplate.Height = 24;
            this.dgvWebsites.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvWebsites.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWebsites.Size = new System.Drawing.Size(1320, 140);
            this.dgvWebsites.TabIndex = 9;
            // 
            // lblWebsitesEmptyState
            // 
            this.lblWebsitesEmptyState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWebsitesEmptyState.Location = new System.Drawing.Point(8, 106);
            this.lblWebsitesEmptyState.Name = "lblWebsitesEmptyState";
            this.lblWebsitesEmptyState.Size = new System.Drawing.Size(1320, 140);
            this.lblWebsitesEmptyState.TabIndex = 23;
            this.lblWebsitesEmptyState.Text = "Select Source and Target environments, then click Load and Compare.";
            this.lblWebsitesEmptyState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblWebsitesEmptyState.Visible = false;
            // 
            // lblBottomGridEmptyState
            // 
            this.lblBottomGridEmptyState.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBottomGridEmptyState.Location = new System.Drawing.Point(8, 258);
            this.lblBottomGridEmptyState.Name = "lblBottomGridEmptyState";
            this.lblBottomGridEmptyState.Size = new System.Drawing.Size(1320, 342);
            this.lblBottomGridEmptyState.TabIndex = 24;
            this.lblBottomGridEmptyState.Text = "Select Source and Target environments, then click Load and Compare.";
            this.lblBottomGridEmptyState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBottomGridEmptyState.Visible = false;
            // 
            // dgvSiteSettings
            // 
            this.dgvSiteSettings.AllowUserToAddRows = false;
            this.dgvSiteSettings.AllowUserToDeleteRows = false;
            this.dgvSiteSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSiteSettings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSiteSettings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSiteSettings.Location = new System.Drawing.Point(8, 258);
            this.dgvSiteSettings.MultiSelect = false;
            this.dgvSiteSettings.Name = "dgvSiteSettings";
            this.dgvSiteSettings.ReadOnly = true;
            this.dgvSiteSettings.RowHeadersVisible = false;
            this.dgvSiteSettings.RowHeadersWidth = 25;
            this.dgvSiteSettings.RowTemplate.Height = 24;
            this.dgvSiteSettings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvSiteSettings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSiteSettings.Size = new System.Drawing.Size(1320, 342);
            this.dgvSiteSettings.TabIndex = 10;
            // 
            // lblSettingsCount
            // 
            this.lblSettingsCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSettingsCount.AutoSize = false;
            this.lblSettingsCount.Location = new System.Drawing.Point(8, 616);
            this.lblSettingsCount.Name = "lblSettingsCount";
            this.lblSettingsCount.Size = new System.Drawing.Size(170, 17);
            this.lblSettingsCount.TabIndex = 11;
            this.lblSettingsCount.Text = "Site Settings: 0";
            // 
            // lblFilterSettings
            // 
            this.lblFilterSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblFilterSettings.AutoSize = true;
            this.lblFilterSettings.Location = new System.Drawing.Point(190, 616);
            this.lblFilterSettings.Name = "lblFilterSettings";
            this.lblFilterSettings.Size = new System.Drawing.Size(32, 13);
            this.lblFilterSettings.TabIndex = 12;
            this.lblFilterSettings.Text = "Filter:";
            // 
            // txtFilterSettings
            // 
            this.txtFilterSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtFilterSettings.Location = new System.Drawing.Point(225, 612);
            this.txtFilterSettings.Name = "txtFilterSettings";
            this.txtFilterSettings.Size = new System.Drawing.Size(200, 20);
            this.txtFilterSettings.TabIndex = 13;
            this.txtFilterSettings.TextChanged += new System.EventHandler(this.txtFilterSettings_TextChanged);
            // 
            // lblStatusFilter
            // 
            this.lblStatusFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatusFilter.AutoSize = true;
            this.lblStatusFilter.Location = new System.Drawing.Point(435, 616);
            this.lblStatusFilter.Name = "lblStatusFilter";
            this.lblStatusFilter.Size = new System.Drawing.Size(40, 13);
            this.lblStatusFilter.TabIndex = 14;
            this.lblStatusFilter.Text = "Status:";
            // 
            // cboStatusFilter
            // 
            this.cboStatusFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cboStatusFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatusFilter.FormattingEnabled = true;
            this.cboStatusFilter.Location = new System.Drawing.Point(480, 612);
            this.cboStatusFilter.Name = "cboStatusFilter";
            this.cboStatusFilter.Size = new System.Drawing.Size(120, 21);
            this.cboStatusFilter.TabIndex = 15;
            // 
            // chkFindingsOnly
            // 
            this.chkFindingsOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkFindingsOnly.AutoSize = true;
            this.chkFindingsOnly.Location = new System.Drawing.Point(610, 614);
            this.chkFindingsOnly.Name = "chkFindingsOnly";
            this.chkFindingsOnly.Size = new System.Drawing.Size(87, 17);
            this.chkFindingsOnly.TabIndex = 16;
            this.chkFindingsOnly.Text = "Findings only";
            this.chkFindingsOnly.UseVisualStyleBackColor = true;
            // 
            // lblCategoryFilter
            // 
            this.lblCategoryFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCategoryFilter.AutoSize = true;
            this.lblCategoryFilter.Location = new System.Drawing.Point(710, 616);
            this.lblCategoryFilter.Name = "lblCategoryFilter";
            this.lblCategoryFilter.Size = new System.Drawing.Size(52, 13);
            this.lblCategoryFilter.TabIndex = 17;
            this.lblCategoryFilter.Text = "Category:";
            // 
            // cboCategoryFilter
            // 
            this.cboCategoryFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cboCategoryFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategoryFilter.FormattingEnabled = true;
            this.cboCategoryFilter.Location = new System.Drawing.Point(768, 612);
            this.cboCategoryFilter.Name = "cboCategoryFilter";
            this.cboCategoryFilter.Size = new System.Drawing.Size(145, 21);
            this.cboCategoryFilter.TabIndex = 18;
            // 
            // btnClearFilter
            // 
            this.btnClearFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClearFilter.Location = new System.Drawing.Point(920, 610);
            this.btnClearFilter.Name = "btnClearFilter";
            this.btnClearFilter.Size = new System.Drawing.Size(95, 23);
            this.btnClearFilter.TabIndex = 19;
            this.btnClearFilter.Text = "Clear Filters";
            this.btnClearFilter.UseVisualStyleBackColor = true;
            this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExportCsv.Location = new System.Drawing.Point(1020, 610);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(95, 23);
            this.btnExportCsv.TabIndex = 20;
            this.btnExportCsv.Text = "Export CSV";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExportExcel.Location = new System.Drawing.Point(1120, 610);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(95, 23);
            this.btnExportExcel.TabIndex = 21;
            this.btnExportExcel.Text = "Export Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // btnExportHtml
            // 
            this.btnExportHtml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExportHtml.Location = new System.Drawing.Point(1220, 610);
            this.btnExportHtml.Name = "btnExportHtml";
            this.btnExportHtml.Size = new System.Drawing.Size(95, 23);
            this.btnExportHtml.TabIndex = 22;
            this.btnExportHtml.Text = "Export HTML";
            this.btnExportHtml.UseVisualStyleBackColor = true;
            this.btnExportHtml.Click += new System.EventHandler(this.btnExportHtml_Click);
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(8, 646);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(1320, 82);
            this.txtLog.TabIndex = 25;
            // 
            // lblActivity
            // 
            this.lblActivity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblActivity.AutoEllipsis = true;
            this.lblActivity.Location = new System.Drawing.Point(8, 738);
            this.lblActivity.Name = "lblActivity";
            this.lblActivity.Size = new System.Drawing.Size(1140, 17);
            this.lblActivity.TabIndex = 28;
            this.lblActivity.Text = "Ready";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(1160, 738);
            this.progressBar.MarqueeAnimationSpeed = 25;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(168, 14);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 29;
            this.progressBar.Visible = false;
            // 
            // PowerPagesAlmDriftInspectorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblActivity);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnExportHtml);
            this.Controls.Add(this.btnExportExcel);
            this.Controls.Add(this.btnExportCsv);
            this.Controls.Add(this.btnClearFilter);
            this.Controls.Add(this.cboCategoryFilter);
            this.Controls.Add(this.lblCategoryFilter);
            this.Controls.Add(this.chkFindingsOnly);
            this.Controls.Add(this.cboStatusFilter);
            this.Controls.Add(this.lblStatusFilter);
            this.Controls.Add(this.txtFilterSettings);
            this.Controls.Add(this.lblFilterSettings);
            this.Controls.Add(this.lblSettingsCount);
            this.Controls.Add(this.dgvSiteSettings);
            this.Controls.Add(this.dgvWebsites);
            this.Controls.Add(this.lblBottomGridEmptyState);
            this.Controls.Add(this.lblWebsitesEmptyState);
            this.Controls.Add(this.targetPanel);
            this.Controls.Add(this.sourcePanel);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.lblWebsiteCount);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.btnSelectTarget);
            this.Controls.Add(this.btnSelectSource);
            this.Name = "PowerPagesAlmDriftInspectorControl";
            this.Size = new System.Drawing.Size(1340, 760);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWebsites)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSiteSettings)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnSelectSource;
        private System.Windows.Forms.Button btnSelectTarget;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.DataGridView dgvWebsites;
        private System.Windows.Forms.DataGridView dgvSiteSettings;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblWebsiteCount;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Label lblSourceSnapshot;
        private System.Windows.Forms.Label lblTargetSnapshot;
        private System.Windows.Forms.Label lblSettingsCount;
        private System.Windows.Forms.Label lblFilterSettings;
        private System.Windows.Forms.TextBox txtFilterSettings;
        private System.Windows.Forms.Label lblStatusFilter;
        private System.Windows.Forms.ComboBox cboStatusFilter;
        private System.Windows.Forms.CheckBox chkFindingsOnly;
        private System.Windows.Forms.Label lblCategoryFilter;
        private System.Windows.Forms.ComboBox cboCategoryFilter;
        private System.Windows.Forms.Button btnClearFilter;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnExportHtml;
        private System.Windows.Forms.Label lblWebsitesEmptyState;
        private System.Windows.Forms.Label lblBottomGridEmptyState;
        private System.Windows.Forms.Panel sourcePanel;
        private System.Windows.Forms.Panel targetPanel;
        private System.Windows.Forms.Label lblSourceCaption;
        private System.Windows.Forms.Label lblTargetCaption;
        private System.Windows.Forms.Label lblActivity;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}
