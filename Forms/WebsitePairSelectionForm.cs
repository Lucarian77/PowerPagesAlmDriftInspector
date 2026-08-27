using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PowerPagesAlmDriftInspector.Models;

namespace PowerPagesAlmDriftInspector.Forms
{
    public sealed class WebsitePairSelectionForm : Form
    {
        private readonly List<WebsiteModel> _sourceWebsites;
        private readonly List<WebsiteModel> _targetWebsites;
        private readonly Guid _previousSourceWebsiteId;
        private readonly Guid _previousTargetWebsiteId;

        private Label lblInstructions;
        private Label lblSourceEnvironmentCaption;
        private Label lblSourceEnvironmentValue;
        private Label lblSourceWebsiteCaption;
        private ComboBox cboSourceWebsite;
        private Label lblTargetEnvironmentCaption;
        private Label lblTargetEnvironmentValue;
        private Label lblTargetWebsiteCaption;
        private ComboBox cboTargetWebsite;
        private Label lblMatchGuidance;
        private Button btnLoadAndCompare;
        private Button btnCancel;

        private bool _initializing;

        public WebsitePairSelectionForm(
            string sourceEnvironmentName,
            string targetEnvironmentName,
            IEnumerable<WebsiteModel> sourceWebsites,
            IEnumerable<WebsiteModel> targetWebsites,
            Guid previousSourceWebsiteId,
            Guid previousTargetWebsiteId)
        {
            SourceEnvironmentName = sourceEnvironmentName ?? string.Empty;
            TargetEnvironmentName = targetEnvironmentName ?? string.Empty;
            _sourceWebsites = OrderWebsites(sourceWebsites);
            _targetWebsites = OrderWebsites(targetWebsites);
            _previousSourceWebsiteId = previousSourceWebsiteId;
            _previousTargetWebsiteId = previousTargetWebsiteId;

            InitializeComponent();
            InitializeData();
        }

        public string SourceEnvironmentName { get; }

        public string TargetEnvironmentName { get; }

        public WebsiteModel SelectedSourceWebsite
        {
            get
            {
                WebsiteChoiceItem item = cboSourceWebsite?.SelectedItem as WebsiteChoiceItem;
                return item?.Website;
            }
        }

        public WebsiteModel SelectedTargetWebsite
        {
            get
            {
                WebsiteChoiceItem item = cboTargetWebsite?.SelectedItem as WebsiteChoiceItem;
                return item?.Website;
            }
        }

        private void InitializeComponent()
        {
            lblInstructions = new Label();
            lblSourceEnvironmentCaption = new Label();
            lblSourceEnvironmentValue = new Label();
            lblSourceWebsiteCaption = new Label();
            cboSourceWebsite = new ComboBox();
            lblTargetEnvironmentCaption = new Label();
            lblTargetEnvironmentValue = new Label();
            lblTargetWebsiteCaption = new Label();
            cboTargetWebsite = new ComboBox();
            lblMatchGuidance = new Label();
            btnLoadAndCompare = new Button();
            btnCancel = new Button();

            SuspendLayout();

            lblInstructions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblInstructions.Location = new Point(16, 14);
            lblInstructions.Name = "lblInstructions";
            lblInstructions.Size = new Size(688, 42);
            lblInstructions.Text =
                "Select the corresponding Power Pages Management website in each environment. " +
                "The selections are kept independent because environment-specific names and Partial URLs may differ.";

            lblSourceEnvironmentCaption.AutoSize = true;
            lblSourceEnvironmentCaption.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblSourceEnvironmentCaption.Location = new Point(16, 69);
            lblSourceEnvironmentCaption.Name = "lblSourceEnvironmentCaption";
            lblSourceEnvironmentCaption.Text = "SOURCE ENVIRONMENT";

            lblSourceEnvironmentValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSourceEnvironmentValue.AutoEllipsis = true;
            lblSourceEnvironmentValue.BackColor = Color.FromArgb(245, 247, 250);
            lblSourceEnvironmentValue.BorderStyle = BorderStyle.FixedSingle;
            lblSourceEnvironmentValue.Location = new Point(16, 92);
            lblSourceEnvironmentValue.Name = "lblSourceEnvironmentValue";
            lblSourceEnvironmentValue.Padding = new Padding(7, 5, 7, 0);
            lblSourceEnvironmentValue.Size = new Size(688, 30);

            lblSourceWebsiteCaption.AutoSize = true;
            lblSourceWebsiteCaption.Location = new Point(16, 136);
            lblSourceWebsiteCaption.Name = "lblSourceWebsiteCaption";
            lblSourceWebsiteCaption.Text = "Source Website:";

            cboSourceWebsite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboSourceWebsite.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSourceWebsite.FormattingEnabled = true;
            cboSourceWebsite.Location = new Point(16, 158);
            cboSourceWebsite.Name = "cboSourceWebsite";
            cboSourceWebsite.Size = new Size(688, 23);
            cboSourceWebsite.SelectedIndexChanged += cboSourceWebsite_SelectedIndexChanged;

            lblTargetEnvironmentCaption.AutoSize = true;
            lblTargetEnvironmentCaption.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblTargetEnvironmentCaption.Location = new Point(16, 205);
            lblTargetEnvironmentCaption.Name = "lblTargetEnvironmentCaption";
            lblTargetEnvironmentCaption.Text = "TARGET ENVIRONMENT";

            lblTargetEnvironmentValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTargetEnvironmentValue.AutoEllipsis = true;
            lblTargetEnvironmentValue.BackColor = Color.FromArgb(245, 247, 250);
            lblTargetEnvironmentValue.BorderStyle = BorderStyle.FixedSingle;
            lblTargetEnvironmentValue.Location = new Point(16, 228);
            lblTargetEnvironmentValue.Name = "lblTargetEnvironmentValue";
            lblTargetEnvironmentValue.Padding = new Padding(7, 5, 7, 0);
            lblTargetEnvironmentValue.Size = new Size(688, 30);

            lblTargetWebsiteCaption.AutoSize = true;
            lblTargetWebsiteCaption.Location = new Point(16, 272);
            lblTargetWebsiteCaption.Name = "lblTargetWebsiteCaption";
            lblTargetWebsiteCaption.Text = "Target Website:";

            cboTargetWebsite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboTargetWebsite.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTargetWebsite.FormattingEnabled = true;
            cboTargetWebsite.Location = new Point(16, 294);
            cboTargetWebsite.Name = "cboTargetWebsite";
            cboTargetWebsite.Size = new Size(688, 23);
            cboTargetWebsite.SelectedIndexChanged += cboTargetWebsite_SelectedIndexChanged;

            lblMatchGuidance.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblMatchGuidance.ForeColor = Color.DimGray;
            lblMatchGuidance.Location = new Point(16, 329);
            lblMatchGuidance.Name = "lblMatchGuidance";
            lblMatchGuidance.Size = new Size(688, 38);
            lblMatchGuidance.Text =
                "An exact Name or Partial URL match is suggested when it is unique. " +
                "Review both selections before continuing.";

            btnLoadAndCompare.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLoadAndCompare.Location = new Point(450, 384);
            btnLoadAndCompare.Name = "btnLoadAndCompare";
            btnLoadAndCompare.Size = new Size(142, 32);
            btnLoadAndCompare.Text = "Load and Compare";
            btnLoadAndCompare.UseVisualStyleBackColor = true;
            btnLoadAndCompare.Click += btnLoadAndCompare_Click;

            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(598, 384);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(106, 32);
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;

            AcceptButton = btnLoadAndCompare;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(720, 432);
            Controls.Add(lblInstructions);
            Controls.Add(lblSourceEnvironmentCaption);
            Controls.Add(lblSourceEnvironmentValue);
            Controls.Add(lblSourceWebsiteCaption);
            Controls.Add(cboSourceWebsite);
            Controls.Add(lblTargetEnvironmentCaption);
            Controls.Add(lblTargetEnvironmentValue);
            Controls.Add(lblTargetWebsiteCaption);
            Controls.Add(cboTargetWebsite);
            Controls.Add(lblMatchGuidance);
            Controls.Add(btnLoadAndCompare);
            Controls.Add(btnCancel);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "WebsitePairSelectionForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select Source and Target Websites";

            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializeData()
        {
            _initializing = true;

            try
            {
                lblSourceEnvironmentValue.Text =
                    GetSafeDisplayValue(SourceEnvironmentName, "Unknown Source Environment");
                lblTargetEnvironmentValue.Text =
                    GetSafeDisplayValue(TargetEnvironmentName, "Unknown Target Environment");

                cboSourceWebsite.DisplayMember = nameof(WebsiteChoiceItem.DisplayText);
                cboSourceWebsite.DataSource = _sourceWebsites
                    .Select(website => new WebsiteChoiceItem(website))
                    .ToList();
                cboSourceWebsite.SelectedIndex = -1;

                cboTargetWebsite.DisplayMember = nameof(WebsiteChoiceItem.DisplayText);
                cboTargetWebsite.DataSource = _targetWebsites
                    .Select(website => new WebsiteChoiceItem(website))
                    .ToList();
                cboTargetWebsite.SelectedIndex = -1;

                SelectWebsiteById(cboSourceWebsite, _previousSourceWebsiteId);

                if (cboSourceWebsite.SelectedIndex < 0 && _sourceWebsites.Count == 1)
                {
                    cboSourceWebsite.SelectedIndex = 0;
                }

                SelectWebsiteById(cboTargetWebsite, _previousTargetWebsiteId);

                if (cboTargetWebsite.SelectedIndex < 0)
                {
                    SelectSuggestedTargetWebsite();
                }
            }
            finally
            {
                _initializing = false;
            }

            UpdateCompareButtonState();
        }

        private void cboSourceWebsite_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_initializing)
            {
                SelectSuggestedTargetWebsite();
            }

            UpdateCompareButtonState();
        }

        private void cboTargetWebsite_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCompareButtonState();
        }

        private void SelectSuggestedTargetWebsite()
        {
            WebsiteModel sourceWebsite = SelectedSourceWebsite;
            WebsiteModel suggestedTarget = FindUniqueTargetWebsiteMatch(sourceWebsite, _targetWebsites);

            if (suggestedTarget != null)
            {
                SelectWebsiteById(cboTargetWebsite, suggestedTarget.Id);
            }
            else if (_targetWebsites.Count == 1)
            {
                cboTargetWebsite.SelectedIndex = 0;
            }
            else
            {
                cboTargetWebsite.SelectedIndex = -1;
            }
        }

        private void UpdateCompareButtonState()
        {
            if (btnLoadAndCompare != null)
            {
                btnLoadAndCompare.Enabled =
                    SelectedSourceWebsite != null &&
                    SelectedTargetWebsite != null;
            }
        }

        private void btnLoadAndCompare_Click(object sender, EventArgs e)
        {
            if (SelectedSourceWebsite == null || SelectedTargetWebsite == null)
            {
                MessageBox.Show(
                    this,
                    "Select both a Source website and a Target website.",
                    "Website Selection Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static void SelectWebsiteById(ComboBox comboBox, Guid websiteId)
        {
            if (comboBox == null || websiteId == Guid.Empty)
            {
                return;
            }

            for (int index = 0; index < comboBox.Items.Count; index++)
            {
                WebsiteChoiceItem item = comboBox.Items[index] as WebsiteChoiceItem;

                if (item?.Website != null && item.Website.Id == websiteId)
                {
                    comboBox.SelectedIndex = index;
                    return;
                }
            }
        }

        private static WebsiteModel FindUniqueTargetWebsiteMatch(
            WebsiteModel sourceWebsite,
            IEnumerable<WebsiteModel> targetWebsites)
        {
            if (sourceWebsite == null)
            {
                return null;
            }

            List<WebsiteModel> targets = (targetWebsites ?? Enumerable.Empty<WebsiteModel>())
                .Where(website => website != null)
                .ToList();
            string sourceName = (sourceWebsite.Name ?? string.Empty).Trim();
            string sourcePartialUrl = (sourceWebsite.PartialUrl ?? string.Empty).Trim();

            List<WebsiteModel> nameMatches = targets
                .Where(website =>
                    string.Equals(
                        (website.Name ?? string.Empty).Trim(),
                        sourceName,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (nameMatches.Count == 1)
            {
                return nameMatches[0];
            }

            if (string.IsNullOrWhiteSpace(sourcePartialUrl))
            {
                return null;
            }

            List<WebsiteModel> partialUrlMatches = targets
                .Where(website =>
                    string.Equals(
                        (website.PartialUrl ?? string.Empty).Trim(),
                        sourcePartialUrl,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            return partialUrlMatches.Count == 1
                ? partialUrlMatches[0]
                : null;
        }

        private static List<WebsiteModel> OrderWebsites(IEnumerable<WebsiteModel> websites)
        {
            return (websites ?? Enumerable.Empty<WebsiteModel>())
                .Where(website => website != null)
                .OrderBy(website => website.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(website => website.PartialUrl ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string GetSafeDisplayValue(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim();
        }

        private sealed class WebsiteChoiceItem
        {
            public WebsiteChoiceItem(WebsiteModel website)
            {
                Website = website;
                DisplayText = BuildDisplayText(website);
            }

            public WebsiteModel Website { get; }

            public string DisplayText { get; }

            private static string BuildDisplayText(WebsiteModel website)
            {
                if (website == null)
                {
                    return "(unknown website)";
                }

                string name = string.IsNullOrWhiteSpace(website.Name)
                    ? "(unnamed website)"
                    : website.Name.Trim();
                string partialUrl = (website.PartialUrl ?? string.Empty).Trim();

                return string.IsNullOrWhiteSpace(partialUrl)
                    ? name
                    : name + "  |  Partial URL: " + partialUrl;
            }
        }
    }
}
