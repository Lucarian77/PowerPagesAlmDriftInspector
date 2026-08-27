using System.Windows.Forms;

namespace PowerPagesAlmDriftInspector.Forms
{
    partial class ComparisonResultDetailForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblSettingName = new System.Windows.Forms.Label();
            this.lblSettingNameValue = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.lblCategoryValue = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblStatusValue = new System.Windows.Forms.Label();
            this.lblReviewFocus = new System.Windows.Forms.Label();
            this.lblReviewFocusValue = new System.Windows.Forms.Label();
            this.lblEnvironmentSpecificReason = new System.Windows.Forms.Label();
            this.lblEnvironmentSpecificReasonValue = new System.Windows.Forms.Label();
            this.lblRecommendedAction = new System.Windows.Forms.Label();
            this.lblRecommendedActionValue = new System.Windows.Forms.Label();
            this.lblSourceContext = new System.Windows.Forms.Label();
            this.lblSourceContextValue = new System.Windows.Forms.Label();
            this.lblTargetContext = new System.Windows.Forms.Label();
            this.lblTargetContextValue = new System.Windows.Forms.Label();
            this.lblSourcePanel = new System.Windows.Forms.GroupBox();
            this.txtSourceValue = new System.Windows.Forms.TextBox();
            this.lblTargetPanel = new System.Windows.Forms.GroupBox();
            this.txtTargetValue = new System.Windows.Forms.TextBox();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.chkWrapText = new System.Windows.Forms.CheckBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnCopySummary = new System.Windows.Forms.Button();
            this.btnCopyBoth = new System.Windows.Forms.Button();
            this.btnCopyTarget = new System.Windows.Forms.Button();
            this.btnCopySource = new System.Windows.Forms.Button();
            this.lblSourcePanel.SuspendLayout();
            this.lblTargetPanel.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSettingName
            // 
            this.lblSettingName.AutoSize = true;
            this.lblSettingName.Location = new System.Drawing.Point(16, 18);
            this.lblSettingName.Name = "lblSettingName";
            this.lblSettingName.Size = new System.Drawing.Size(74, 13);
            this.lblSettingName.TabIndex = 0;
            this.lblSettingName.Text = "Setting Name:";
            // 
            // lblSettingNameValue
            // 
            this.lblSettingNameValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSettingNameValue.AutoEllipsis = true;
            this.lblSettingNameValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSettingNameValue.Location = new System.Drawing.Point(170, 12);
            this.lblSettingNameValue.Name = "lblSettingNameValue";
            this.lblSettingNameValue.Padding = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.lblSettingNameValue.Size = new System.Drawing.Size(1120, 34);
            this.lblSettingNameValue.TabIndex = 1;
            this.lblSettingNameValue.Text = "(not available)";
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(16, 61);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 2;
            this.lblCategory.Text = "Category:";
            // 
            // lblCategoryValue
            // 
            this.lblCategoryValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCategoryValue.Location = new System.Drawing.Point(170, 55);
            this.lblCategoryValue.Name = "lblCategoryValue";
            this.lblCategoryValue.Padding = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.lblCategoryValue.Size = new System.Drawing.Size(390, 34);
            this.lblCategoryValue.TabIndex = 3;
            this.lblCategoryValue.Text = "(not available)";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(585, 61);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 13);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status:";
            // 
            // lblStatusValue
            // 
            this.lblStatusValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatusValue.AutoEllipsis = true;
            this.lblStatusValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStatusValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblStatusValue.Location = new System.Drawing.Point(645, 56);
            this.lblStatusValue.Name = "lblStatusValue";
            this.lblStatusValue.Padding = new System.Windows.Forms.Padding(8, 5, 8, 0);
            this.lblStatusValue.Size = new System.Drawing.Size(645, 32);
            this.lblStatusValue.TabIndex = 5;
            this.lblStatusValue.Text = "(not available)";
            // 
            // lblReviewFocus
            // 
            this.lblReviewFocus.AutoSize = true;
            this.lblReviewFocus.Location = new System.Drawing.Point(16, 104);
            this.lblReviewFocus.Name = "lblReviewFocus";
            this.lblReviewFocus.Size = new System.Drawing.Size(78, 13);
            this.lblReviewFocus.TabIndex = 6;
            this.lblReviewFocus.Text = "Review Focus:";
            // 
            // lblReviewFocusValue
            // 
            this.lblReviewFocusValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblReviewFocusValue.AutoEllipsis = true;
            this.lblReviewFocusValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblReviewFocusValue.Location = new System.Drawing.Point(170, 98);
            this.lblReviewFocusValue.Name = "lblReviewFocusValue";
            this.lblReviewFocusValue.Padding = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.lblReviewFocusValue.Size = new System.Drawing.Size(1120, 34);
            this.lblReviewFocusValue.TabIndex = 7;
            this.lblReviewFocusValue.Text = "(not available)";
            // 
            // lblEnvironmentSpecificReason
            // 
            this.lblEnvironmentSpecificReason.AutoSize = true;
            this.lblEnvironmentSpecificReason.Location = new System.Drawing.Point(16, 148);
            this.lblEnvironmentSpecificReason.Name = "lblEnvironmentSpecificReason";
            this.lblEnvironmentSpecificReason.Size = new System.Drawing.Size(142, 13);
            this.lblEnvironmentSpecificReason.TabIndex = 8;
            this.lblEnvironmentSpecificReason.Text = "Environment-Specific Note:";
            // 
            // lblEnvironmentSpecificReasonValue
            // 
            this.lblEnvironmentSpecificReasonValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEnvironmentSpecificReasonValue.AutoEllipsis = true;
            this.lblEnvironmentSpecificReasonValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblEnvironmentSpecificReasonValue.Location = new System.Drawing.Point(170, 142);
            this.lblEnvironmentSpecificReasonValue.Name = "lblEnvironmentSpecificReasonValue";
            this.lblEnvironmentSpecificReasonValue.Padding = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.lblEnvironmentSpecificReasonValue.Size = new System.Drawing.Size(1120, 44);
            this.lblEnvironmentSpecificReasonValue.TabIndex = 9;
            this.lblEnvironmentSpecificReasonValue.Text = "(not available)";
            // 
            // lblRecommendedAction
            // 
            this.lblRecommendedAction.AutoSize = true;
            this.lblRecommendedAction.Location = new System.Drawing.Point(16, 202);
            this.lblRecommendedAction.Name = "lblRecommendedAction";
            this.lblRecommendedAction.Size = new System.Drawing.Size(116, 13);
            this.lblRecommendedAction.TabIndex = 10;
            this.lblRecommendedAction.Text = "Recommended Action:";
            // 
            // lblRecommendedActionValue
            // 
            this.lblRecommendedActionValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRecommendedActionValue.AutoEllipsis = true;
            this.lblRecommendedActionValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblRecommendedActionValue.Location = new System.Drawing.Point(170, 196);
            this.lblRecommendedActionValue.Name = "lblRecommendedActionValue";
            this.lblRecommendedActionValue.Padding = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.lblRecommendedActionValue.Size = new System.Drawing.Size(1120, 44);
            this.lblRecommendedActionValue.TabIndex = 11;
            this.lblRecommendedActionValue.Text = "(not available)";
            // 
            // lblSourceContext
            // 
            this.lblSourceContext.AutoSize = true;
            this.lblSourceContext.Location = new System.Drawing.Point(16, 255);
            this.lblSourceContext.Name = "lblSourceContext";
            this.lblSourceContext.Size = new System.Drawing.Size(44, 13);
            this.lblSourceContext.TabIndex = 12;
            this.lblSourceContext.Text = "Source:";
            // 
            // lblSourceContextValue
            // 
            this.lblSourceContextValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceContextValue.AutoEllipsis = true;
            this.lblSourceContextValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSourceContextValue.Location = new System.Drawing.Point(170, 249);
            this.lblSourceContextValue.Name = "lblSourceContextValue";
            this.lblSourceContextValue.Padding = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.lblSourceContextValue.Size = new System.Drawing.Size(1120, 34);
            this.lblSourceContextValue.TabIndex = 13;
            this.lblSourceContextValue.Text = "(not available)";
            // 
            // lblTargetContext
            // 
            this.lblTargetContext.AutoSize = true;
            this.lblTargetContext.Location = new System.Drawing.Point(16, 298);
            this.lblTargetContext.Name = "lblTargetContext";
            this.lblTargetContext.Size = new System.Drawing.Size(41, 13);
            this.lblTargetContext.TabIndex = 14;
            this.lblTargetContext.Text = "Target:";
            // 
            // lblTargetContextValue
            // 
            this.lblTargetContextValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTargetContextValue.AutoEllipsis = true;
            this.lblTargetContextValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTargetContextValue.Location = new System.Drawing.Point(170, 292);
            this.lblTargetContextValue.Name = "lblTargetContextValue";
            this.lblTargetContextValue.Padding = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.lblTargetContextValue.Size = new System.Drawing.Size(1120, 34);
            this.lblTargetContextValue.TabIndex = 15;
            this.lblTargetContextValue.Text = "(not available)";
            // 
            // lblSourcePanel
            // 
            this.lblSourcePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourcePanel.Controls.Add(this.txtSourceValue);
            this.lblSourcePanel.Location = new System.Drawing.Point(19, 344);
            this.lblSourcePanel.Name = "lblSourcePanel";
            this.lblSourcePanel.Size = new System.Drawing.Size(626, 314);
            this.lblSourcePanel.TabIndex = 16;
            this.lblSourcePanel.TabStop = false;
            this.lblSourcePanel.Text = "Source Value";
            // 
            // txtSourceValue
            // 
            this.txtSourceValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourceValue.Location = new System.Drawing.Point(12, 24);
            this.txtSourceValue.Multiline = true;
            this.txtSourceValue.Name = "txtSourceValue";
            this.txtSourceValue.ReadOnly = true;
            this.txtSourceValue.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSourceValue.Size = new System.Drawing.Size(602, 278);
            this.txtSourceValue.TabIndex = 0;
            this.txtSourceValue.WordWrap = false;
            // 
            // lblTargetPanel
            // 
            this.lblTargetPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTargetPanel.Controls.Add(this.txtTargetValue);
            this.lblTargetPanel.Location = new System.Drawing.Point(664, 344);
            this.lblTargetPanel.Name = "lblTargetPanel";
            this.lblTargetPanel.Size = new System.Drawing.Size(626, 314);
            this.lblTargetPanel.TabIndex = 17;
            this.lblTargetPanel.TabStop = false;
            this.lblTargetPanel.Text = "Target Value";
            // 
            // txtTargetValue
            // 
            this.txtTargetValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTargetValue.Location = new System.Drawing.Point(12, 24);
            this.txtTargetValue.Multiline = true;
            this.txtTargetValue.Name = "txtTargetValue";
            this.txtTargetValue.ReadOnly = true;
            this.txtTargetValue.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTargetValue.Size = new System.Drawing.Size(602, 278);
            this.txtTargetValue.TabIndex = 0;
            this.txtTargetValue.WordWrap = false;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlButtons.Controls.Add(this.chkWrapText);
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Controls.Add(this.btnCopySummary);
            this.pnlButtons.Controls.Add(this.btnCopyBoth);
            this.pnlButtons.Controls.Add(this.btnCopyTarget);
            this.pnlButtons.Controls.Add(this.btnCopySource);
            this.pnlButtons.Location = new System.Drawing.Point(19, 670);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(1271, 45);
            this.pnlButtons.TabIndex = 18;
            // 
            // chkWrapText
            // 
            this.chkWrapText.AutoSize = true;
            this.chkWrapText.Location = new System.Drawing.Point(610, 14);
            this.chkWrapText.Name = "chkWrapText";
            this.chkWrapText.Size = new System.Drawing.Size(76, 17);
            this.chkWrapText.TabIndex = 4;
            this.chkWrapText.Text = "Wrap Text";
            this.chkWrapText.UseVisualStyleBackColor = true;
            this.chkWrapText.CheckedChanged += new System.EventHandler(this.chkWrapText_CheckedChanged);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(1131, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(140, 34);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnCopySummary
            // 
            this.btnCopySummary.Location = new System.Drawing.Point(450, 5);
            this.btnCopySummary.Name = "btnCopySummary";
            this.btnCopySummary.Size = new System.Drawing.Size(140, 34);
            this.btnCopySummary.TabIndex = 3;
            this.btnCopySummary.Text = "Copy Summary";
            this.btnCopySummary.UseVisualStyleBackColor = true;
            this.btnCopySummary.Click += new System.EventHandler(this.btnCopySummary_Click);
            // 
            // btnCopyBoth
            // 
            this.btnCopyBoth.Location = new System.Drawing.Point(300, 5);
            this.btnCopyBoth.Name = "btnCopyBoth";
            this.btnCopyBoth.Size = new System.Drawing.Size(140, 34);
            this.btnCopyBoth.TabIndex = 2;
            this.btnCopyBoth.Text = "Copy Both";
            this.btnCopyBoth.UseVisualStyleBackColor = true;
            this.btnCopyBoth.Click += new System.EventHandler(this.btnCopyBoth_Click);
            // 
            // btnCopyTarget
            // 
            this.btnCopyTarget.Location = new System.Drawing.Point(150, 5);
            this.btnCopyTarget.Name = "btnCopyTarget";
            this.btnCopyTarget.Size = new System.Drawing.Size(140, 34);
            this.btnCopyTarget.TabIndex = 1;
            this.btnCopyTarget.Text = "Copy Target";
            this.btnCopyTarget.UseVisualStyleBackColor = true;
            this.btnCopyTarget.Click += new System.EventHandler(this.btnCopyTarget_Click);
            // 
            // btnCopySource
            // 
            this.btnCopySource.Location = new System.Drawing.Point(0, 5);
            this.btnCopySource.Name = "btnCopySource";
            this.btnCopySource.Size = new System.Drawing.Size(140, 34);
            this.btnCopySource.TabIndex = 0;
            this.btnCopySource.Text = "Copy Source";
            this.btnCopySource.UseVisualStyleBackColor = true;
            this.btnCopySource.Click += new System.EventHandler(this.btnCopySource_Click);
            // 
            // ComparisonResultDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1308, 740);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.lblTargetPanel);
            this.Controls.Add(this.lblSourcePanel);
            this.Controls.Add(this.lblTargetContextValue);
            this.Controls.Add(this.lblTargetContext);
            this.Controls.Add(this.lblSourceContextValue);
            this.Controls.Add(this.lblSourceContext);
            this.Controls.Add(this.lblRecommendedActionValue);
            this.Controls.Add(this.lblRecommendedAction);
            this.Controls.Add(this.lblEnvironmentSpecificReasonValue);
            this.Controls.Add(this.lblEnvironmentSpecificReason);
            this.Controls.Add(this.lblReviewFocusValue);
            this.Controls.Add(this.lblReviewFocus);
            this.Controls.Add(this.lblStatusValue);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblCategoryValue);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.lblSettingNameValue);
            this.Controls.Add(this.lblSettingName);
            this.MinimumSize = new System.Drawing.Size(1000, 780);
            this.Name = "ComparisonResultDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Comparison Detail";
            this.lblSourcePanel.ResumeLayout(false);
            this.lblSourcePanel.PerformLayout();
            this.lblTargetPanel.ResumeLayout(false);
            this.lblTargetPanel.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label lblSettingName;
        private Label lblSettingNameValue;
        private Label lblCategory;
        private Label lblCategoryValue;
        private Label lblStatus;
        private Label lblStatusValue;
        private Label lblReviewFocus;
        private Label lblReviewFocusValue;
        private Label lblEnvironmentSpecificReason;
        private Label lblEnvironmentSpecificReasonValue;
        private Label lblRecommendedAction;
        private Label lblRecommendedActionValue;
        private Label lblSourceContext;
        private Label lblSourceContextValue;
        private Label lblTargetContext;
        private Label lblTargetContextValue;
        private GroupBox lblSourcePanel;
        private TextBox txtSourceValue;
        private GroupBox lblTargetPanel;
        private TextBox txtTargetValue;
        private Panel pnlButtons;
        private CheckBox chkWrapText;
        private Button btnClose;
        private Button btnCopySummary;
        private Button btnCopyBoth;
        private Button btnCopyTarget;
        private Button btnCopySource;
    }
}
