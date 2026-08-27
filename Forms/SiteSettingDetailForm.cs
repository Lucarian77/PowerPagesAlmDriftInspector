using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PowerPagesAlmDriftInspector.Models;

namespace PowerPagesAlmDriftInspector.Forms
{
    public sealed class SiteSettingDetailForm : Form
    {
        private readonly SiteSettingModel _setting;
        private readonly string _environmentName;
        private readonly string _websiteName;

        private Label lblEnvironmentCaption;
        private Label lblEnvironmentValue;
        private Label lblWebsiteCaption;
        private Label lblWebsiteValue;
        private Label lblNameCaption;
        private Label lblNameValue;
        private Label lblCategoryCaption;
        private Label lblCategoryValue;
        private Label lblValueCaption;
        private TextBox txtValue;
        private Button btnCopyValue;
        private Button btnCopyAll;
        private Button btnClose;

        public SiteSettingDetailForm(
            SiteSettingModel setting,
            string environmentName,
            string websiteName)
        {
            _setting = setting ?? new SiteSettingModel();
            _environmentName = environmentName ?? string.Empty;
            _websiteName = websiteName ?? string.Empty;

            InitializeComponent();
            InitializeFormStyling();
            InitializeFormData();
        }

        private void InitializeComponent()
        {
            lblEnvironmentCaption = new Label();
            lblEnvironmentValue = new Label();
            lblWebsiteCaption = new Label();
            lblWebsiteValue = new Label();
            lblNameCaption = new Label();
            lblNameValue = new Label();
            lblCategoryCaption = new Label();
            lblCategoryValue = new Label();
            lblValueCaption = new Label();
            txtValue = new TextBox();
            btnCopyValue = new Button();
            btnCopyAll = new Button();
            btnClose = new Button();

            SuspendLayout();

            lblEnvironmentCaption.AutoSize = false;
            lblEnvironmentCaption.Location = new Point(16, 18);
            lblEnvironmentCaption.Name = "lblEnvironmentCaption";
            lblEnvironmentCaption.Size = new Size(120, 22);
            lblEnvironmentCaption.Text = "Environment:";
            lblEnvironmentCaption.TextAlign = ContentAlignment.MiddleLeft;

            lblEnvironmentValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblEnvironmentValue.AutoEllipsis = true;
            lblEnvironmentValue.Location = new Point(145, 16);
            lblEnvironmentValue.Name = "lblEnvironmentValue";
            lblEnvironmentValue.Size = new Size(705, 26);
            lblEnvironmentValue.TextAlign = ContentAlignment.MiddleLeft;

            lblWebsiteCaption.AutoSize = false;
            lblWebsiteCaption.Location = new Point(16, 54);
            lblWebsiteCaption.Name = "lblWebsiteCaption";
            lblWebsiteCaption.Size = new Size(120, 22);
            lblWebsiteCaption.Text = "Website:";
            lblWebsiteCaption.TextAlign = ContentAlignment.MiddleLeft;

            lblWebsiteValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblWebsiteValue.AutoEllipsis = true;
            lblWebsiteValue.Location = new Point(145, 52);
            lblWebsiteValue.Name = "lblWebsiteValue";
            lblWebsiteValue.Size = new Size(705, 26);
            lblWebsiteValue.TextAlign = ContentAlignment.MiddleLeft;

            lblNameCaption.AutoSize = false;
            lblNameCaption.Location = new Point(16, 90);
            lblNameCaption.Name = "lblNameCaption";
            lblNameCaption.Size = new Size(120, 22);
            lblNameCaption.Text = "Setting Name:";
            lblNameCaption.TextAlign = ContentAlignment.MiddleLeft;

            lblNameValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblNameValue.AutoEllipsis = true;
            lblNameValue.Location = new Point(145, 88);
            lblNameValue.Name = "lblNameValue";
            lblNameValue.Size = new Size(705, 26);
            lblNameValue.TextAlign = ContentAlignment.MiddleLeft;

            lblCategoryCaption.AutoSize = false;
            lblCategoryCaption.Location = new Point(16, 126);
            lblCategoryCaption.Name = "lblCategoryCaption";
            lblCategoryCaption.Size = new Size(120, 22);
            lblCategoryCaption.Text = "Category:";
            lblCategoryCaption.TextAlign = ContentAlignment.MiddleLeft;

            lblCategoryValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCategoryValue.AutoEllipsis = true;
            lblCategoryValue.Location = new Point(145, 124);
            lblCategoryValue.Name = "lblCategoryValue";
            lblCategoryValue.Size = new Size(705, 26);
            lblCategoryValue.TextAlign = ContentAlignment.MiddleLeft;

            lblValueCaption.AutoSize = false;
            lblValueCaption.Location = new Point(16, 166);
            lblValueCaption.Name = "lblValueCaption";
            lblValueCaption.Size = new Size(120, 22);
            lblValueCaption.Text = "Setting Value:";
            lblValueCaption.TextAlign = ContentAlignment.MiddleLeft;

            txtValue.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtValue.BackColor = Color.White;
            txtValue.BorderStyle = BorderStyle.FixedSingle;
            txtValue.Location = new Point(16, 194);
            txtValue.Multiline = true;
            txtValue.Name = "txtValue";
            txtValue.ReadOnly = true;
            txtValue.ScrollBars = ScrollBars.Both;
            txtValue.ShortcutsEnabled = true;
            txtValue.Size = new Size(834, 330);
            txtValue.WordWrap = false;

            btnCopyValue.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCopyValue.Location = new Point(514, 544);
            btnCopyValue.Name = "btnCopyValue";
            btnCopyValue.Size = new Size(110, 32);
            btnCopyValue.Text = "Copy Value";
            btnCopyValue.UseVisualStyleBackColor = true;
            btnCopyValue.Click += btnCopyValue_Click;

            btnCopyAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCopyAll.Location = new Point(630, 544);
            btnCopyAll.Name = "btnCopyAll";
            btnCopyAll.Size = new Size(110, 32);
            btnCopyAll.Text = "Copy All";
            btnCopyAll.UseVisualStyleBackColor = true;
            btnCopyAll.Click += btnCopyAll_Click;

            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Location = new Point(746, 544);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(104, 32);
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;

            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(866, 592);
            Controls.Add(lblEnvironmentCaption);
            Controls.Add(lblEnvironmentValue);
            Controls.Add(lblWebsiteCaption);
            Controls.Add(lblWebsiteValue);
            Controls.Add(lblNameCaption);
            Controls.Add(lblNameValue);
            Controls.Add(lblCategoryCaption);
            Controls.Add(lblCategoryValue);
            Controls.Add(lblValueCaption);
            Controls.Add(txtValue);
            Controls.Add(btnCopyValue);
            Controls.Add(btnCopyAll);
            Controls.Add(btnClose);
            MinimumSize = new Size(720, 520);
            Name = "SiteSettingDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Site Setting Detail";

            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializeFormStyling()
        {
            Font = SystemFonts.MessageBoxFont;
            BackColor = SystemColors.Control;

            ConfigureCaptionLabel(lblEnvironmentCaption);
            ConfigureCaptionLabel(lblWebsiteCaption);
            ConfigureCaptionLabel(lblNameCaption);
            ConfigureCaptionLabel(lblCategoryCaption);
            ConfigureCaptionLabel(lblValueCaption);

            ConfigureValueLabel(lblEnvironmentValue);
            ConfigureValueLabel(lblWebsiteValue);
            ConfigureValueLabel(lblNameValue);
            ConfigureValueLabel(lblCategoryValue);

            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        private static void ConfigureCaptionLabel(Label label)
        {
            if (label == null)
            {
                return;
            }

            label.Font = new Font(label.Font, FontStyle.Bold);
            label.ForeColor = SystemColors.ControlText;
        }

        private static void ConfigureValueLabel(Label label)
        {
            if (label == null)
            {
                return;
            }

            label.BackColor = Color.White;
            label.BorderStyle = BorderStyle.FixedSingle;
            label.ForeColor = SystemColors.ControlText;
            label.Padding = new Padding(6, 4, 6, 0);
        }

        private void InitializeFormData()
        {
            lblEnvironmentValue.Text = GetSafeDisplayValue(_environmentName, "Unknown Environment");
            lblWebsiteValue.Text = GetSafeDisplayValue(_websiteName, "Unknown Website");
            lblNameValue.Text = GetSafeDisplayValue(_setting.Name, "(not available)");
            lblCategoryValue.Text = GetSafeDisplayValue(_setting.Category, "(not categorized)");
            txtValue.Text = GetSafeMultilineValue(_setting.Value, "(empty)");
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (btnClose != null)
            {
                ActiveControl = btnClose;
            }
        }

        private static string GetSafeDisplayValue(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim();
        }

        private static string GetSafeMultilineValue(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value;
        }

        private void btnCopyValue_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtValue == null ? string.Empty : txtValue.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to copy the setting value.\r\n\r\n" + ex.Message,
                    "Copy Value",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCopyAll_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine("Environment: " + (lblEnvironmentValue == null ? string.Empty : lblEnvironmentValue.Text));
                builder.AppendLine("Website: " + (lblWebsiteValue == null ? string.Empty : lblWebsiteValue.Text));
                builder.AppendLine("Setting Name: " + (lblNameValue == null ? string.Empty : lblNameValue.Text));
                builder.AppendLine("Category: " + (lblCategoryValue == null ? string.Empty : lblCategoryValue.Text));
                builder.AppendLine();
                builder.AppendLine("Setting Value:");
                builder.AppendLine(txtValue == null ? string.Empty : txtValue.Text);

                Clipboard.SetText(builder.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to copy the site setting detail.\r\n\r\n" + ex.Message,
                    "Copy All",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
