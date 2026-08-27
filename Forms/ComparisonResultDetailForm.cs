using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PowerPagesAlmDriftInspector.Models;

namespace PowerPagesAlmDriftInspector.Forms
{
    public partial class ComparisonResultDetailForm : Form
    {
        private readonly ComparisonResultModel _result;
        private readonly string _sourceEnvironmentName;
        private readonly string _targetEnvironmentName;
        private readonly string _sourceWebsiteName;
        private readonly string _targetWebsiteName;

        public ComparisonResultDetailForm(
            ComparisonResultModel result,
            string sourceEnvironmentName,
            string targetEnvironmentName,
            string sourceWebsiteName,
            string targetWebsiteName)
        {
            InitializeComponent();

            _result = result ?? new ComparisonResultModel();
            _sourceEnvironmentName = sourceEnvironmentName ?? string.Empty;
            _targetEnvironmentName = targetEnvironmentName ?? string.Empty;
            _sourceWebsiteName = sourceWebsiteName ?? string.Empty;
            _targetWebsiteName = targetWebsiteName ?? string.Empty;

            InitializeFormStyling();
            InitializeFormData();
        }

        private void InitializeFormStyling()
        {
            Text = "Comparison Detail";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1000, 780);
            Size = new Size(1320, 820);

            ConfigureValueTextBox(txtSourceValue);
            ConfigureValueTextBox(txtTargetValue);

            ConfigureReadOnlyValueLabel(lblSettingNameValue);
            ConfigureReadOnlyValueLabel(lblCategoryValue);
            ConfigureReadOnlyValueLabel(lblStatusValue);
            ConfigureReadOnlyValueLabel(lblRecommendedActionValue);
            ConfigureReadOnlyValueLabel(lblReviewFocusValue);
            ConfigureReadOnlyValueLabel(lblEnvironmentSpecificReasonValue);
            ConfigureReadOnlyValueLabel(lblSourceContextValue);
            ConfigureReadOnlyValueLabel(lblTargetContextValue);

            ConfigureActionButton(btnCopySource);
            ConfigureActionButton(btnCopyTarget);
            ConfigureActionButton(btnCopyBoth);
            ConfigureActionButton(btnCopySummary);
            ConfigureActionButton(btnClose);

            if (chkWrapText != null)
            {
                chkWrapText.Checked = false;
            }

            if (lblSourcePanel != null)
            {
                lblSourcePanel.Font = new Font(lblSourcePanel.Font, FontStyle.Bold);
            }

            if (lblTargetPanel != null)
            {
                lblTargetPanel.Font = new Font(lblTargetPanel.Font, FontStyle.Bold);
            }

            if (btnClose != null)
            {
                btnClose.DialogResult = DialogResult.OK;
            }

            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        private static void ConfigureValueTextBox(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.ReadOnly = true;
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.WordWrap = false;
            textBox.ShortcutsEnabled = true;
            textBox.BackColor = Color.White;
            textBox.ForeColor = SystemColors.ControlText;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void ConfigureReadOnlyValueLabel(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.BackColor = Color.White;
            control.ForeColor = SystemColors.ControlText;
            control.Padding = new Padding(8, 6, 8, 0);
        }

        private static void ConfigureActionButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.AutoSize = false;
            button.Width = 140;
            button.Height = 34;
        }

        private void InitializeFormData()
        {
            lblSettingNameValue.Text = GetSafeDisplayValue(_result.SettingName);
            lblCategoryValue.Text = GetSafeDisplayValue(_result.Category);
            lblStatusValue.Text = GetSafeDisplayValue(_result.Status);
            lblRecommendedActionValue.Text = GetSafeDisplayValue(_result.RecommendedAction);
            lblReviewFocusValue.Text = GetSafeDisplayValue(_result.ReviewFocus);
            lblEnvironmentSpecificReasonValue.Text = GetSafeDisplayValue(_result.EnvironmentSpecificReason);

            lblSourceContextValue.Text = BuildContextText(
                _sourceEnvironmentName,
                _sourceWebsiteName,
                _result.SourceRecordCount,
                _result.SourceDistinctValueCount,
                IsDuplicateResult());
            lblTargetContextValue.Text = BuildContextText(
                _targetEnvironmentName,
                _targetWebsiteName,
                _result.TargetRecordCount,
                _result.TargetDistinctValueCount,
                IsDuplicateResult());

            txtSourceValue.Text = GetSafeMultilineValue(_result.SourceValue, "missing");
            txtTargetValue.Text = GetSafeMultilineValue(_result.TargetValue, "missing");

            ApplyStatusStyling();
            ApplyReviewFocusStyling();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (btnClose != null)
            {
                ActiveControl = btnClose;
            }
        }

        private static string BuildContextText(
            string environmentName,
            string websiteName,
            int recordCount,
            int distinctValueCount,
            bool includeRecordSummary)
        {
            string safeEnvironment = string.IsNullOrWhiteSpace(environmentName)
                ? "Unknown Environment"
                : environmentName;

            string safeWebsite = string.IsNullOrWhiteSpace(websiteName)
                ? "Unknown Website"
                : websiteName;

            string context = safeEnvironment + " | " + safeWebsite;

            if (!includeRecordSummary)
            {
                return context;
            }

            string recordLabel = recordCount == 1 ? "record" : "records";
            string valueLabel = distinctValueCount == 1 ? "value" : "values";

            return context +
                " | " +
                recordCount +
                " " +
                recordLabel +
                " / " +
                distinctValueCount +
                " " +
                valueLabel;
        }

        private bool IsDuplicateResult()
        {
            return string.Equals(
                _result.Status,
                "Duplicate",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSafeDisplayValue(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "(not available)"
                : value.Trim();
        }

        private static string GetSafeMultilineValue(string value, string emptyDisplayText)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "(" + emptyDisplayText + ")";
            }

            return value;
        }

        private void ApplyReviewFocusStyling()
        {
            if (lblReviewFocusValue == null || lblEnvironmentSpecificReasonValue == null)
            {
                return;
            }

            if (_result.IsEnvironmentSpecificCandidate && !_result.IsMatch)
            {
                lblReviewFocusValue.BackColor = Color.FromArgb(255, 242, 204);
                lblReviewFocusValue.ForeColor = Color.FromArgb(122, 74, 0);
                lblReviewFocusValue.Font = new Font(lblReviewFocusValue.Font, FontStyle.Bold);

                lblEnvironmentSpecificReasonValue.BackColor = Color.FromArgb(255, 250, 240);
                lblEnvironmentSpecificReasonValue.ForeColor = Color.FromArgb(95, 55, 14);
            }
            else
            {
                lblReviewFocusValue.BackColor = Color.White;
                lblReviewFocusValue.ForeColor = SystemColors.ControlText;
                lblEnvironmentSpecificReasonValue.BackColor = Color.White;
                lblEnvironmentSpecificReasonValue.ForeColor = SystemColors.ControlText;
            }
        }

        private void ApplyStatusStyling()
        {
            string status = _result.Status ?? string.Empty;

            if (lblStatusValue == null)
            {
                return;
            }

            lblStatusValue.Font = new Font(lblStatusValue.Font, FontStyle.Bold);

            if (string.Equals(status, "Different Value", StringComparison.OrdinalIgnoreCase))
            {
                lblStatusValue.ForeColor = Color.DarkOrange;
                lblStatusValue.BackColor = Color.FromArgb(255, 242, 204);
            }
            else if (string.Equals(status, "Missing in Source", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(status, "Missing in Target", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(status, "Duplicate", StringComparison.OrdinalIgnoreCase))
            {
                lblStatusValue.ForeColor = Color.DarkRed;
                lblStatusValue.BackColor = Color.FromArgb(255, 228, 225);
            }
            else if (string.Equals(status, "Match", StringComparison.OrdinalIgnoreCase))
            {
                lblStatusValue.ForeColor = Color.DarkGreen;
                lblStatusValue.BackColor = Color.FromArgb(226, 239, 218);
            }
            else
            {
                lblStatusValue.ForeColor = SystemColors.ControlText;
                lblStatusValue.BackColor = Color.White;
            }
        }

        private void btnCopySource_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtSourceValue?.Text ?? string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to copy source value.\r\n\r\n" + ex.Message,
                    "Copy Source",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCopyTarget_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtTargetValue?.Text ?? string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to copy target value.\r\n\r\n" + ex.Message,
                    "Copy Target",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCopyBoth_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine("Setting Name: " + (lblSettingNameValue?.Text ?? string.Empty));
                builder.AppendLine("Category: " + (lblCategoryValue?.Text ?? string.Empty));
                builder.AppendLine("Status: " + (lblStatusValue?.Text ?? string.Empty));
                AppendDuplicateEvidenceSummary(builder);
                builder.AppendLine("Review Focus: " + (lblReviewFocusValue?.Text ?? string.Empty));
                builder.AppendLine("Environment-Specific Note: " + (lblEnvironmentSpecificReasonValue?.Text ?? string.Empty));
                builder.AppendLine("Recommended Action: " + (lblRecommendedActionValue?.Text ?? string.Empty));
                builder.AppendLine();
                builder.AppendLine("Source: " + (lblSourceContextValue?.Text ?? string.Empty));
                builder.AppendLine(txtSourceValue?.Text ?? string.Empty);
                builder.AppendLine();
                builder.AppendLine("Target: " + (lblTargetContextValue?.Text ?? string.Empty));
                builder.AppendLine(txtTargetValue?.Text ?? string.Empty);

                Clipboard.SetText(builder.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to copy both values.\r\n\r\n" + ex.Message,
                    "Copy Both",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCopySummary_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(BuildSummaryText());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to copy setting summary.\r\n\r\n" + ex.Message,
                    "Copy Setting Summary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string BuildSummaryText()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Setting Name: " + (lblSettingNameValue?.Text ?? string.Empty));
            builder.AppendLine("Category: " + (lblCategoryValue?.Text ?? string.Empty));
            builder.AppendLine("Status: " + (lblStatusValue?.Text ?? string.Empty));
            AppendDuplicateEvidenceSummary(builder);
            builder.AppendLine("Review Focus: " + (lblReviewFocusValue?.Text ?? string.Empty));
            builder.AppendLine("Environment-Specific Note: " + (lblEnvironmentSpecificReasonValue?.Text ?? string.Empty));
            builder.AppendLine("Recommended Action: " + (lblRecommendedActionValue?.Text ?? string.Empty));
            builder.AppendLine("Source: " + (lblSourceContextValue?.Text ?? string.Empty));
            builder.AppendLine("Target: " + (lblTargetContextValue?.Text ?? string.Empty));

            return builder.ToString();
        }

        private void AppendDuplicateEvidenceSummary(StringBuilder builder)
        {
            if (builder == null || !IsDuplicateResult())
            {
                return;
            }

            builder.AppendLine(
                "Duplicate Classification: " +
                GetSafeDisplayValue(_result.DuplicateClassification));
            builder.AppendLine(
                "Source Records: " +
                _result.SourceRecordCount +
                " (" +
                _result.SourceDistinctValueCount +
                " distinct value(s))");
            builder.AppendLine(
                "Target Records: " +
                _result.TargetRecordCount +
                " (" +
                _result.TargetDistinctValueCount +
                " distinct value(s))");
        }

        private void chkWrapText_CheckedChanged(object sender, EventArgs e)
        {
            SetValueWrapMode(chkWrapText != null && chkWrapText.Checked);
        }

        private void SetValueWrapMode(bool wrapText)
        {
            if (txtSourceValue != null)
            {
                txtSourceValue.WordWrap = wrapText;
            }

            if (txtTargetValue != null)
            {
                txtTargetValue.WordWrap = wrapText;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
