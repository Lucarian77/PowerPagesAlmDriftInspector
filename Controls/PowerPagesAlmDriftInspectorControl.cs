using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using PowerPagesAlmDriftInspector.Forms;
using PowerPagesAlmDriftInspector.Models;
using PowerPagesAlmDriftInspector.Services;
using XrmToolBox.Extensibility;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Spreadsheet = DocumentFormat.OpenXml.Spreadsheet;


namespace PowerPagesAlmDriftInspector.Controls
{
    public partial class PowerPagesAlmDriftInspectorControl : MultipleConnectionsPluginControlBase
    {
        private readonly DataverseRetrievalService _retrievalService;

        private List<SiteSettingModel> _allSiteSettings;
        private List<SiteSettingModel> _sourceSiteSettings;
        private List<SiteSettingModel> _targetSiteSettings;
        private List<ComparisonResultModel> _comparisonResults;

        private string _sourceEnvironmentName;
        private string _targetEnvironmentName;
        private string _sourceWebsiteName;
        private string _targetWebsiteName;
        private Guid _sourceWebsiteId;
        private Guid _targetWebsiteId;

        private bool _isComparisonView;
        private bool _showFindingsOnly;
        private bool _hasSourceSnapshot;
        private bool _hasTargetSnapshot;
        private bool _comparisonExecuted;
        private bool _workInProgress;

        private ConnectionDetail _sourceConnectionDetail;
        private IOrganizationService _sourceService;
        private ConnectionDetail _targetConnectionDetail;
        private IOrganizationService _targetService;

        private const int WebsiteGridMinHeight = 110;
        private const int WebsiteGridMaxHeight = 140;
        private const int WebsiteGridExtraPadding = 8;
        private const int WebsiteRowHeightEstimate = 26;

        private const string AllStatusesText = "All";
        private const string AllCategoriesText = "All Categories";

        private const string StatusDifferentValue = "Different Value";
        private const string StatusMatch = "Match";
        private const string StatusMissingInSource = "Missing in Source";
        private const string StatusMissingInTarget = "Missing in Target";
        private const string StatusDuplicate = "Duplicate";

        private const uint ExcelStyleDefault = 0U;
        private const uint ExcelStyleMetaLabel = 1U;
        private const uint ExcelStyleHeader = 2U;
        private const uint ExcelStyleText = 3U;
        private const uint ExcelStyleDifferent = 4U;
        private const uint ExcelStyleMissing = 5U;
        private const uint ExcelStyleMatch = 6U;

        public PowerPagesAlmDriftInspectorControl()
        {
            InitializeComponent();

            _retrievalService = new DataverseRetrievalService();

            _allSiteSettings = new List<SiteSettingModel>();
            _sourceSiteSettings = new List<SiteSettingModel>();
            _targetSiteSettings = new List<SiteSettingModel>();
            _comparisonResults = new List<ComparisonResultModel>();

            _sourceEnvironmentName = string.Empty;
            _targetEnvironmentName = string.Empty;
            _sourceWebsiteName = string.Empty;
            _targetWebsiteName = string.Empty;
            _sourceWebsiteId = Guid.Empty;
            _targetWebsiteId = Guid.Empty;

            _isComparisonView = false;
            _showFindingsOnly = false;
            _hasSourceSnapshot = false;
            _hasTargetSnapshot = false;
            _comparisonExecuted = false;
            _workInProgress = false;
            _sourceConnectionDetail = null;
            _sourceService = null;
            _targetConnectionDetail = null;
            _targetService = null;

            InitializeGridDefaults();
            WireEvents();
            InitializeStatusFilter();
            InitializeCategoryFilter();
            InitializeControlState();
        }

        private void InitializeGridDefaults()
        {
            if (dgvWebsites != null)
            {
                dgvWebsites.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                dgvWebsites.EnableHeadersVisualStyles = false;
            }

            if (dgvSiteSettings != null)
            {
                dgvSiteSettings.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                dgvSiteSettings.EnableHeadersVisualStyles = false;
            }

            if (btnClearFilter != null)
            {
                btnClearFilter.Text = "Clear Filters";
            }

            if (btnExportExcel != null)
            {
                btnExportExcel.Text = "Export Excel";
            }

            if (btnExportHtml != null)
            {
                btnExportHtml.Text = "Export HTML";
            }

            if (chkFindingsOnly != null)
            {
                chkFindingsOnly.Text = "Findings only";
                chkFindingsOnly.Checked = false;
            }

            ConfigureEmptyStateLabel(lblWebsitesEmptyState);
            ConfigureEmptyStateLabel(lblBottomGridEmptyState);
        }

        private static void ConfigureEmptyStateLabel(System.Windows.Forms.Label label)
        {
            if (label == null)
            {
                return;
            }

            label.BackColor = Color.FromArgb(245, 245, 245);
            label.ForeColor = Color.DimGray;
            label.BorderStyle = BorderStyle.FixedSingle;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Visible = false;
        }

        private void WireEvents()
        {
            if (dgvSiteSettings != null)
            {
                dgvSiteSettings.CellDoubleClick -= dgvSiteSettings_CellDoubleClick;
                dgvSiteSettings.CellDoubleClick += dgvSiteSettings_CellDoubleClick;
            }

            if (cboStatusFilter != null)
            {
                cboStatusFilter.SelectedIndexChanged -= cboStatusFilter_SelectedIndexChanged;
                cboStatusFilter.SelectedIndexChanged += cboStatusFilter_SelectedIndexChanged;
            }

            if (cboCategoryFilter != null)
            {
                cboCategoryFilter.SelectedIndexChanged -= cboCategoryFilter_SelectedIndexChanged;
                cboCategoryFilter.SelectedIndexChanged += cboCategoryFilter_SelectedIndexChanged;
            }

            if (chkFindingsOnly != null)
            {
                chkFindingsOnly.CheckedChanged -= chkFindingsOnly_CheckedChanged;
                chkFindingsOnly.CheckedChanged += chkFindingsOnly_CheckedChanged;
            }
        }

        private void InitializeStatusFilter()
        {
            if (cboStatusFilter == null)
            {
                return;
            }

            cboStatusFilter.Items.Clear();
            cboStatusFilter.Items.Add(AllStatusesText);
            cboStatusFilter.Items.Add(StatusDifferentValue);
            cboStatusFilter.Items.Add(StatusMatch);
            cboStatusFilter.Items.Add(StatusMissingInSource);
            cboStatusFilter.Items.Add(StatusMissingInTarget);
            cboStatusFilter.Items.Add(StatusDuplicate);
            cboStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatusFilter.SelectedIndex = 0;
        }

        private void InitializeCategoryFilter()
        {
            if (cboCategoryFilter == null)
            {
                return;
            }

            cboCategoryFilter.Items.Clear();
            cboCategoryFilter.Items.Add(AllCategoriesText);
            cboCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategoryFilter.SelectedIndex = 0;
        }

        private void InitializeControlState()
        {
            txtLog?.Clear();

            if (dgvWebsites != null)
            {
                dgvWebsites.DataSource = null;
            }

            if (dgvSiteSettings != null)
            {
                dgvSiteSettings.DataSource = null;
            }

            _allSiteSettings = new List<SiteSettingModel>();
            _sourceSiteSettings = new List<SiteSettingModel>();
            _targetSiteSettings = new List<SiteSettingModel>();
            _comparisonResults = new List<ComparisonResultModel>();

            _sourceEnvironmentName = string.Empty;
            _targetEnvironmentName = string.Empty;
            _sourceWebsiteName = string.Empty;
            _targetWebsiteName = string.Empty;
            _sourceWebsiteId = Guid.Empty;
            _targetWebsiteId = Guid.Empty;

            _isComparisonView = false;
            _showFindingsOnly = false;
            _hasSourceSnapshot = false;
            _hasTargetSnapshot = false;
            _comparisonExecuted = false;

            if (txtFilterSettings != null)
            {
                txtFilterSettings.Text = string.Empty;
            }

            if (cboStatusFilter != null)
            {
                cboStatusFilter.SelectedIndex = 0;
            }

            if (cboCategoryFilter != null)
            {
                cboCategoryFilter.SelectedIndex = 0;
            }

            if (chkFindingsOnly != null)
            {
                chkFindingsOnly.Checked = false;
            }

            SetWebsiteCount(0);
            SetSiteSettingsCount(0, 0);
            UpdateSnapshotLabels();
            UpdateSummaryLabel();
            UpdateCategoryFilterItems();
            UpdateWebsiteGridHeight();
            UpdateEmptyStateLabels();
            UpdateActionStates();
        }

        private void ResetComparisonState()
        {
            _comparisonResults = new List<ComparisonResultModel>();
            _isComparisonView = false;
            _comparisonExecuted = false;
            _showFindingsOnly = false;

            if (chkFindingsOnly != null)
            {
                chkFindingsOnly.Checked = false;
            }

            if (cboStatusFilter != null)
            {
                cboStatusFilter.SelectedIndex = 0;
            }

            if (cboCategoryFilter != null)
            {
                cboCategoryFilter.SelectedIndex = 0;
            }

            UpdateSummaryLabel();
            UpdateCategoryFilterItems();
            UpdateWebsiteGridHeight();
            UpdateEmptyStateLabels();
            UpdateActionStates();
        }

        private void ClearComparisonData()
        {
            _sourceSiteSettings = new List<SiteSettingModel>();
            _targetSiteSettings = new List<SiteSettingModel>();
            _sourceWebsiteName = string.Empty;
            _targetWebsiteName = string.Empty;
            _sourceWebsiteId = Guid.Empty;
            _targetWebsiteId = Guid.Empty;
            _hasSourceSnapshot = false;
            _hasTargetSnapshot = false;
            _allSiteSettings = new List<SiteSettingModel>();

            if (dgvWebsites != null)
            {
                dgvWebsites.DataSource = null;
            }

            SetWebsiteCount(0);
            ResetComparisonState();
            ApplyBottomGridFilter();
            UpdateSnapshotLabels();
            UpdateWebsiteGridHeight();
            UpdateEmptyStateLabels();
        }

        private void ClearLoadedData()
        {
            _allSiteSettings = new List<SiteSettingModel>();
            _sourceSiteSettings = new List<SiteSettingModel>();
            _targetSiteSettings = new List<SiteSettingModel>();
            _comparisonResults = new List<ComparisonResultModel>();
            _sourceWebsiteName = string.Empty;
            _targetWebsiteName = string.Empty;
            _sourceWebsiteId = Guid.Empty;
            _targetWebsiteId = Guid.Empty;
            _hasSourceSnapshot = false;
            _hasTargetSnapshot = false;
            _comparisonExecuted = false;
            _isComparisonView = false;
            _showFindingsOnly = false;

            if (txtFilterSettings != null)
            {
                txtFilterSettings.Text = string.Empty;
            }

            if (cboStatusFilter != null && cboStatusFilter.Items.Count > 0)
            {
                cboStatusFilter.SelectedIndex = 0;
            }

            if (cboCategoryFilter != null && cboCategoryFilter.Items.Count > 0)
            {
                cboCategoryFilter.SelectedIndex = 0;
            }

            if (chkFindingsOnly != null)
            {
                chkFindingsOnly.Checked = false;
            }

            if (dgvWebsites != null)
            {
                dgvWebsites.DataSource = null;
                SetWebsiteCount(0);
            }

            if (dgvSiteSettings != null)
            {
                dgvSiteSettings.DataSource = null;
            }

            SetSiteSettingsCount(0, 0);
            UpdateCategoryFilterItems();
            UpdateSummaryLabel();
            UpdateWebsiteGridHeight();
            UpdateEmptyStateLabels();
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            bool isSourceSelectionCallback = string.Equals(
                actionName,
                nameof(SourceConnectionSelected),
                StringComparison.Ordinal);

            base.UpdateConnection(newService, detail, actionName, parameter);

            bool isAdditionalConnection = string.Equals(
                actionName,
                "AdditionalOrganization",
                StringComparison.OrdinalIgnoreCase);

            // SourceConnectionSelected is invoked by base.UpdateConnection after the
            // host has assigned Service and ConnectionDetail. Other primary connection
            // changes (for example, the XrmToolBox Connect button) are applied here.
            if (!isAdditionalConnection && !isSourceSelectionCallback)
            {
                ApplySourceConnection(newService, detail, "Source connection changed to");
            }

            UpdateSnapshotLabels();
            UpdateActionStates();
        }

        private void SourceConnectionSelected()
        {
            ApplySourceConnection(Service, ConnectionDetail, "Source environment selected");
        }

        private void ApplySourceConnection(
            IOrganizationService sourceService,
            ConnectionDetail sourceConnectionDetail,
            string logPrefix)
        {
            _sourceService = sourceService;
            _sourceConnectionDetail = sourceConnectionDetail;
            _sourceEnvironmentName = GetConnectionName(sourceConnectionDetail, "Not connected");
            ClearLoadedData();

            LogMessage(logPrefix + ": " + _sourceEnvironmentName + ".");
            UpdateSnapshotLabels();
            UpdateActionStates();
        }

        protected override void ConnectionDetailsUpdated(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count > 0)
            {
                ConnectionDetail addedConnection = e.NewItems[0] as ConnectionDetail;

                if (addedConnection != null)
                {
                    ReplaceTargetConnection(addedConnection);

                    _targetService = addedConnection.GetCrmServiceClient();
                    _targetEnvironmentName = GetConnectionName(
                        addedConnection,
                        "Unknown Target Environment");

                    ClearComparisonData();
                    LogMessage("Target environment selected: " + _targetEnvironmentName + ".");

                    UpdateSnapshotLabels();
                    UpdateActionStates();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove ||
                     e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (_targetConnectionDetail != null &&
                    !AdditionalConnectionDetails.Contains(_targetConnectionDetail))
                {
                    LogMessage("Target connection removed: " + _targetEnvironmentName);

                    _targetConnectionDetail = null;
                    _targetService = null;
                    _targetEnvironmentName = string.Empty;
                    ClearComparisonData();

                    UpdateSnapshotLabels();
                    UpdateActionStates();
                }
            }
        }

        private void ReplaceTargetConnection(ConnectionDetail newConnection)
        {
            ConnectionDetail previousConnection = _targetConnectionDetail;
            _targetConnectionDetail = newConnection;

            if (previousConnection != null &&
                !ReferenceEquals(previousConnection, newConnection) &&
                AdditionalConnectionDetails.Contains(previousConnection))
            {
                BeginInvoke(new Action(() =>
                {
                    if (AdditionalConnectionDetails.Contains(previousConnection))
                    {
                        RemoveAdditionalOrganization(previousConnection);
                    }
                }));
            }
        }

        private void UpdateActionStates()
        {
            bool isSourceConnected = _sourceService != null;
            bool isTargetConnected = _targetService != null;
            bool hasComparisonResults = _comparisonResults != null && _comparisonResults.Count > 0;
            bool hasSiteSettings = _allSiteSettings != null && _allSiteSettings.Count > 0;
            bool hasBottomData = _isComparisonView ? hasComparisonResults : hasSiteSettings;

            if (btnSelectSource != null)
            {
                btnSelectSource.Enabled = !_workInProgress;
            }

            if (btnSelectTarget != null)
            {
                btnSelectTarget.Enabled = !_workInProgress && isSourceConnected;
            }

            if (btnCompare != null)
            {
                btnCompare.Enabled =
                    !_workInProgress &&
                    isSourceConnected &&
                    isTargetConnected;
            }

            if (cboStatusFilter != null)
            {
                cboStatusFilter.Enabled = !_workInProgress && _isComparisonView && hasComparisonResults;
            }

            if (lblStatusFilter != null)
            {
                lblStatusFilter.Enabled = _isComparisonView && hasComparisonResults;
            }

            if (cboCategoryFilter != null)
            {
                cboCategoryFilter.Enabled = !_workInProgress && hasBottomData;
            }

            if (lblCategoryFilter != null)
            {
                lblCategoryFilter.Enabled = hasBottomData;
            }

            if (txtFilterSettings != null)
            {
                txtFilterSettings.Enabled = !_workInProgress && hasBottomData;
            }

            if (lblFilterSettings != null)
            {
                lblFilterSettings.Enabled = hasBottomData;
            }

            if (chkFindingsOnly != null)
            {
                chkFindingsOnly.Enabled = !_workInProgress && _isComparisonView && hasComparisonResults;
            }

            if (btnClearFilter != null)
            {
                bool hasTextFilter = txtFilterSettings != null && !string.IsNullOrWhiteSpace(txtFilterSettings.Text);
                bool hasStatusFilter = !string.Equals(GetSelectedStatusFilter(), AllStatusesText, StringComparison.OrdinalIgnoreCase);
                bool hasCategoryFilter = !string.Equals(GetSelectedCategoryFilter(), AllCategoriesText, StringComparison.OrdinalIgnoreCase);
                bool hasFindingsOnlyFilter = _showFindingsOnly;

                btnClearFilter.Enabled =
                    !_workInProgress &&
                    hasBottomData &&
                    (hasTextFilter || hasStatusFilter || hasCategoryFilter || hasFindingsOnlyFilter);
            }

            bool hasExportableRows = _isComparisonView
                ? GetFilteredComparisonResults().Count > 0
                : GetFilteredSiteSettings().Count > 0;

            if (btnExportCsv != null)
            {
                btnExportCsv.Enabled = !_workInProgress && hasExportableRows;
            }

            if (btnExportExcel != null)
            {
                btnExportExcel.Enabled = !_workInProgress && hasExportableRows;
            }

            if (btnExportHtml != null)
            {
                btnExportHtml.Enabled = !_workInProgress && hasExportableRows;
            }

            if (dgvWebsites != null)
            {
                dgvWebsites.Enabled = !_workInProgress;
            }

            if (dgvSiteSettings != null)
            {
                dgvSiteSettings.Enabled = !_workInProgress;
            }
        }

        private void UpdateSnapshotLabels()
        {
            if (lblSourceSnapshot != null)
            {
                if (_hasSourceSnapshot)
                {
                    lblSourceSnapshot.Text =
                        (string.IsNullOrWhiteSpace(_sourceEnvironmentName) ? "Unknown" : _sourceEnvironmentName) +
                        " | " +
                        (string.IsNullOrWhiteSpace(_sourceWebsiteName) ? "Unknown Website" : _sourceWebsiteName) +
                        " | " +
                        _sourceSiteSettings.Count + " settings";
                }
                else
                {
                    lblSourceSnapshot.Text =
                        GetConnectionName(_sourceConnectionDetail, "Not connected") +
                        " | No website loaded";
                }

                lblSourceSnapshot.ForeColor = _sourceService == null
                    ? Color.Firebrick
                    : Color.FromArgb(0, 112, 60);
            }

            if (lblTargetSnapshot != null)
            {
                if (_hasTargetSnapshot)
                {
                    lblTargetSnapshot.Text =
                        (string.IsNullOrWhiteSpace(_targetEnvironmentName) ? "Unknown" : _targetEnvironmentName) +
                        " | " +
                        (string.IsNullOrWhiteSpace(_targetWebsiteName) ? "Unknown Website" : _targetWebsiteName) +
                        " | " +
                        _targetSiteSettings.Count + " settings";
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(_targetEnvironmentName))
                    {
                        lblTargetSnapshot.Text = _targetEnvironmentName + " | No website loaded";
                    }
                    else
                    {
                        lblTargetSnapshot.Text = "Not selected";
                    }
                }

                lblTargetSnapshot.ForeColor = _targetService == null
                    ? Color.Firebrick
                    : Color.FromArgb(0, 112, 60);
            }
        }

        private void GetComparisonSummaryCounts(
            out int total,
            out int different,
            out int matches,
            out int missingInSource,
            out int missingInTarget,
            out int duplicates)
        {
            total = _comparisonResults?.Count ?? 0;
            different = 0;
            matches = 0;
            missingInSource = 0;
            missingInTarget = 0;
            duplicates = 0;

            foreach (ComparisonResultModel result in _comparisonResults ?? new List<ComparisonResultModel>())
            {
                if (result == null)
                {
                    continue;
                }

                if (string.Equals(result.Status, StatusDifferentValue, StringComparison.OrdinalIgnoreCase))
                {
                    different++;
                }
                else if (string.Equals(result.Status, StatusMatch, StringComparison.OrdinalIgnoreCase))
                {
                    matches++;
                }
                else if (string.Equals(result.Status, StatusMissingInSource, StringComparison.OrdinalIgnoreCase))
                {
                    missingInSource++;
                }
                else if (string.Equals(result.Status, StatusMissingInTarget, StringComparison.OrdinalIgnoreCase))
                {
                    missingInTarget++;
                }
                else if (string.Equals(result.Status, StatusDuplicate, StringComparison.OrdinalIgnoreCase))
                {
                    duplicates++;
                }
            }
        }

        private void UpdateSummaryLabel()
        {
            if (lblSummary == null)
            {
                return;
            }

            GetComparisonSummaryCounts(
                out int total,
                out int different,
                out int matches,
                out int missingInSource,
                out int missingInTarget,
                out int duplicates);

            if (total == 0)
            {
                lblSummary.Text = _comparisonExecuted
                    ? "Summary: 0 compared | No site settings found"
                    : "Summary: Not compared";
                lblSummary.ForeColor = _comparisonExecuted
                    ? Color.DarkGreen
                    : SystemColors.ControlText;
                return;
            }

            lblSummary.Text =
                "Summary: " +
                total + " compared | " +
                different + " differences | " +
                matches + " matches | " +
                missingInSource + " missing in source | " +
                missingInTarget + " missing in target | " +
                duplicates + " duplicates";

            if (missingInSource > 0 || missingInTarget > 0 || duplicates > 0)
            {
                lblSummary.ForeColor = Color.DarkRed;
            }
            else if (different > 0)
            {
                lblSummary.ForeColor = Color.DarkOrange;
            }
            else
            {
                lblSummary.ForeColor = Color.DarkGreen;
            }
        }

        private void UpdateWebsiteGridHeight()
        {
            if (dgvWebsites == null)
            {
                return;
            }

            int rowCount = dgvWebsites.Rows?.Count ?? 0;
            int desiredHeight = dgvWebsites.ColumnHeadersHeight + WebsiteGridExtraPadding;

            if (rowCount > 0)
            {
                desiredHeight += rowCount * WebsiteRowHeightEstimate;
            }

            desiredHeight = Math.Max(WebsiteGridMinHeight, desiredHeight);
            desiredHeight = Math.Min(WebsiteGridMaxHeight, desiredHeight);

            dgvWebsites.Height = desiredHeight;
        }

        private void UpdateEmptyStateLabels()
        {
            if (lblWebsitesEmptyState != null)
            {
                bool hasWebsiteRows = dgvWebsites != null && dgvWebsites.Rows.Count > 0;
                lblWebsitesEmptyState.Text = hasWebsiteRows
                    ? string.Empty
                    : "Select Source and Target environments, then click Load and Compare.";
                lblWebsitesEmptyState.Visible = !hasWebsiteRows;

                if (lblWebsitesEmptyState.Visible)
                {
                    lblWebsitesEmptyState.BringToFront();
                }
            }

            if (lblBottomGridEmptyState != null)
            {
                bool hasBottomRows = dgvSiteSettings != null && dgvSiteSettings.Rows.Count > 0;

                if (_isComparisonView)
                {
                    lblBottomGridEmptyState.Text = HasActiveFilters()
                        ? "No comparison results match the current filters."
                        : (_comparisonExecuted
                            ? "The comparison completed, but no site settings were found in either environment."
                            : "Select Source and Target environments, then click Load and Compare.");
                }
                else if (_allSiteSettings != null && _allSiteSettings.Count > 0 && HasActiveFilters())
                {
                    lblBottomGridEmptyState.Text = "No site settings match the current filters.";
                }
                else
                {
                    lblBottomGridEmptyState.Text =
                        "Select Source and Target environments, then click Load and Compare.";
                }

                lblBottomGridEmptyState.Visible = !hasBottomRows;

                if (lblBottomGridEmptyState.Visible)
                {
                    lblBottomGridEmptyState.BringToFront();
                }
            }
        }

        private bool HasActiveFilters()
        {
            bool hasTextFilter = txtFilterSettings != null && !string.IsNullOrWhiteSpace(txtFilterSettings.Text);
            bool hasStatusFilter = !string.Equals(GetSelectedStatusFilter(), AllStatusesText, StringComparison.OrdinalIgnoreCase);
            bool hasCategoryFilter = !string.Equals(GetSelectedCategoryFilter(), AllCategoriesText, StringComparison.OrdinalIgnoreCase);
            bool hasFindingsOnlyFilter = _showFindingsOnly;

            return hasTextFilter || hasStatusFilter || hasCategoryFilter || hasFindingsOnlyFilter;
        }

        private void UpdateCategoryFilterItems()
        {
            if (cboCategoryFilter == null)
            {
                return;
            }

            string selectedCategory = GetSelectedCategoryFilter();
            List<string> categories;

            if (_isComparisonView)
            {
                categories = (_comparisonResults ?? new List<ComparisonResultModel>())
                    .Where(result => !string.IsNullOrWhiteSpace(result?.Category))
                    .Select(result => result.Category.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(category => category, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            else
            {
                categories = (_allSiteSettings ?? new List<SiteSettingModel>())
                    .Where(setting => !string.IsNullOrWhiteSpace(setting?.Category))
                    .Select(setting => setting.Category.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(category => category, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            cboCategoryFilter.SelectedIndexChanged -= cboCategoryFilter_SelectedIndexChanged;

            try
            {
                cboCategoryFilter.BeginUpdate();
                cboCategoryFilter.Items.Clear();
                cboCategoryFilter.Items.Add(AllCategoriesText);

                foreach (string category in categories)
                {
                    cboCategoryFilter.Items.Add(category);
                }

                if (!string.IsNullOrWhiteSpace(selectedCategory) &&
                    !string.Equals(selectedCategory, AllCategoriesText, StringComparison.OrdinalIgnoreCase) &&
                    cboCategoryFilter.Items.Contains(selectedCategory))
                {
                    cboCategoryFilter.SelectedItem = selectedCategory;
                }
                else
                {
                    cboCategoryFilter.SelectedIndex = 0;
                }
            }
            finally
            {
                cboCategoryFilter.EndUpdate();
                cboCategoryFilter.SelectedIndexChanged += cboCategoryFilter_SelectedIndexChanged;
            }
        }

        private void btnSelectSource_Click(object sender, EventArgs e)
        {
            if (_workInProgress)
            {
                return;
            }

            try
            {
                LogMessage("Opening source environment selector...");

                if (Service == null)
                {
                    // ExecuteMethod is the supported XrmToolBox path when a tool
                    // is opened before the host has a primary connection.
                    ExecuteMethod(SourceConnectionSelected);
                }
                else
                {
                    // Request a new primary connection even when one is already
                    // active, so Select Source remains a real connection picker.
                    RaiseRequestConnectionEvent(new RequestConnectionEventArgs
                    {
                        ActionName = nameof(SourceConnectionSelected),
                        Control = this
                    });
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to open the source environment selector: " + ex.Message);
                MessageBox.Show(
                    "The source environment selector could not be opened.\r\n\r\n" + ex.Message,
                    "Select Source",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnSelectTarget_Click(object sender, EventArgs e)
        {
            if (_workInProgress)
            {
                return;
            }

            if (_sourceService == null)
            {
                ShowConnectionRequired(
                    "Select a source environment before selecting a target.",
                    "Source Connection Required");
                return;
            }

            try
            {
                LogMessage("Opening target environment selector...");
                AddAdditionalOrganization();
            }
            catch (Exception ex)
            {
                LogMessage("Failed to open the target environment selector: " + ex.Message);
                MessageBox.Show(
                    "The target environment selector could not be opened.\r\n\r\n" + ex.Message,
                    "Select Target",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (_sourceService == null)
            {
                ShowConnectionRequired("Select a source environment first.", "Source Connection Required");
                return;
            }

            if (_targetService == null || _targetConnectionDetail == null)
            {
                ShowConnectionRequired("Select a target environment first.", "Target Connection Required");
                return;
            }

            string sourceUrl = NormalizeUrl(_sourceConnectionDetail?.WebApplicationUrl);
            string targetUrl = NormalizeUrl(_targetConnectionDetail?.WebApplicationUrl);

            if (!string.IsNullOrWhiteSpace(sourceUrl) &&
                string.Equals(sourceUrl, targetUrl, StringComparison.OrdinalIgnoreCase))
            {
                ShowOperationError(
                    "The selected source and target connections point to the same Dataverse environment.",
                    new InvalidOperationException("Select two different environments."));
                return;
            }

            IOrganizationService sourceService = _sourceService;
            IOrganizationService targetService = _targetService;
            string sourceName = GetConnectionName(_sourceConnectionDetail, "Source");
            string targetName = GetConnectionName(_targetConnectionDetail, "Target");
            Guid previousSourceWebsiteId = _sourceWebsiteId;
            Guid previousTargetWebsiteId = _targetWebsiteId;

            SetBusy(true, "Loading Power Pages Management websites...");
            LogMessage(
                "Loading enhanced-model websites from Power Pages Management. Source: '" +
                sourceName + "'; Target: '" + targetName + "'.");

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading Power Pages Management websites",
                Work = (worker, args) =>
                {
                    Guid sourceOrganizationId = TryGetOrganizationId(sourceService);
                    Guid targetOrganizationId = TryGetOrganizationId(targetService);

                    if (sourceOrganizationId != Guid.Empty &&
                        sourceOrganizationId == targetOrganizationId)
                    {
                        throw new InvalidOperationException(
                            "The selected source and target connections point to the same Dataverse environment.");
                    }

                    List<WebsiteModel> sourceWebsites =
                        _retrievalService.GetWebsites(sourceService);
                    List<WebsiteModel> targetWebsites =
                        _retrievalService.GetWebsites(targetService);

                    args.Result = new WebsiteListsLoadResult
                    {
                        SourceWebsites = sourceWebsites,
                        TargetWebsites = targetWebsites
                    };
                },
                PostWorkCallBack = args =>
                {
                    SetBusy(false, string.Empty);

                    if (args.Error != null)
                    {
                        ShowOperationError(
                            "Power Pages Management websites could not be loaded. " +
                            "Both environments must provide readable enhanced-model website data.",
                            args.Error);
                        return;
                    }

                    WebsiteListsLoadResult websiteLists =
                        args.Result as WebsiteListsLoadResult;

                    if (websiteLists == null)
                    {
                        ShowOperationError(
                            "The website load did not return a result.",
                            new InvalidOperationException("No website lists were returned."));
                        return;
                    }

                    websiteLists.SourceWebsites =
                        websiteLists.SourceWebsites ?? new List<WebsiteModel>();
                    websiteLists.TargetWebsites =
                        websiteLists.TargetWebsites ?? new List<WebsiteModel>();

                    LogMessage(
                        "Loaded " + websiteLists.SourceWebsites.Count +
                        " Source website(s) from '" + sourceName + "'.");
                    LogMessage(
                        "Loaded " + websiteLists.TargetWebsites.Count +
                        " Target website(s) from '" + targetName + "'.");

                    if (websiteLists.SourceWebsites.Count == 0)
                    {
                        ShowOperationError(
                            "No Power Pages Management websites were found in the Source environment.",
                            new InvalidOperationException(sourceName + " returned zero mspp_website records."));
                        return;
                    }

                    if (websiteLists.TargetWebsites.Count == 0)
                    {
                        ShowOperationError(
                            "No Power Pages Management websites were found in the Target environment.",
                            new InvalidOperationException(targetName + " returned zero mspp_website records."));
                        return;
                    }

                    using (WebsitePairSelectionForm selectionForm =
                           new WebsitePairSelectionForm(
                               sourceName,
                               targetName,
                               websiteLists.SourceWebsites,
                               websiteLists.TargetWebsites,
                               previousSourceWebsiteId,
                               previousTargetWebsiteId))
                    {
                        if (selectionForm.ShowDialog(this) != DialogResult.OK)
                        {
                            LogMessage(
                                "Load and Compare cancelled. Previous comparison data was preserved.");
                            UpdateActionStates();
                            return;
                        }

                        WebsiteModel sourceWebsite = selectionForm.SelectedSourceWebsite;
                        WebsiteModel targetWebsite = selectionForm.SelectedTargetWebsite;

                        LogMessage(
                            "Website selection confirmed. Source: '" +
                            sourceWebsite.Name + "'; Target: '" +
                            targetWebsite.Name + "'.");

                        StartComparison(
                            sourceService,
                            targetService,
                            sourceName,
                            targetName,
                            sourceWebsite,
                            targetWebsite);
                    }
                }
            });
        }

        private void StartComparison(
            IOrganizationService sourceService,
            IOrganizationService targetService,
            string sourceName,
            string targetName,
            WebsiteModel sourceWebsite,
            WebsiteModel targetWebsite)
        {
            if (sourceWebsite == null || targetWebsite == null)
            {
                ShowOperationError(
                    "Both a Source website and a Target website are required.",
                    new InvalidOperationException("The website selection was incomplete."));
                return;
            }

            SetBusy(true, "Loading and comparing Power Pages Management site settings...");
            LogMessage(
                "Comparison started. Source: '" + sourceName +
                "'; Source Website: '" + sourceWebsite.Name +
                "'; Target: '" + targetName +
                "'; Target Website: '" + targetWebsite.Name +
                "'; Data source: Power Pages Management enhanced model.");

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading and comparing Power Pages Management site settings",
                Work = (worker, args) =>
                {
                    List<SiteSettingModel> sourceSettings =
                        _retrievalService.GetSiteSettings(sourceService, sourceWebsite.Id);
                    List<SiteSettingModel> targetSettings =
                        _retrievalService.GetSiteSettings(targetService, targetWebsite.Id);

                    args.Result = new ComparisonLoadResult
                    {
                        SourceSettings = sourceSettings,
                        TargetSettings = targetSettings,
                        SourceWebsite = sourceWebsite,
                        TargetWebsite = targetWebsite,
                        Results = CompareSiteSettings(sourceSettings, targetSettings)
                    };
                },
                PostWorkCallBack = args =>
                {
                    SetBusy(false, string.Empty);

                    if (args.Error != null)
                    {
                        ShowOperationError(
                            "The comparison could not be completed using the selected " +
                            "Power Pages Management websites.",
                            args.Error);
                        return;
                    }

                    ComparisonLoadResult result = args.Result as ComparisonLoadResult;
                    if (result == null)
                    {
                        ShowOperationError(
                            "The comparison did not return a result.",
                            new InvalidOperationException("No comparison result was returned."));
                        return;
                    }

                    _sourceSiteSettings = result.SourceSettings ?? new List<SiteSettingModel>();
                    _targetSiteSettings = result.TargetSettings ?? new List<SiteSettingModel>();
                    _comparisonResults = result.Results ?? new List<ComparisonResultModel>();
                    _sourceEnvironmentName = sourceName;
                    _targetEnvironmentName = targetName;
                    _sourceWebsiteName = result.SourceWebsite?.Name ?? string.Empty;
                    _targetWebsiteName = result.TargetWebsite?.Name ?? string.Empty;
                    _sourceWebsiteId = result.SourceWebsite?.Id ?? Guid.Empty;
                    _targetWebsiteId = result.TargetWebsite?.Id ?? Guid.Empty;
                    _hasSourceSnapshot = true;
                    _hasTargetSnapshot = true;
                    _comparisonExecuted = true;
                    _isComparisonView = true;

                    BindWebsitePair(
                        sourceName,
                        targetName,
                        result.SourceWebsite,
                        result.TargetWebsite);
                    SetWebsiteCount(
                        (result.SourceWebsite == null ? 0 : 1) +
                        (result.TargetWebsite == null ? 0 : 1));
                    UpdateCategoryFilterItems();
                    ApplyBottomGridFilter();
                    UpdateSnapshotLabels();
                    UpdateSummaryLabel();
                    UpdateActionStates();

                    LogMessage(
                        "Comparison completed with " + _comparisonResults.Count +
                        " result(s). Source settings: " + _sourceSiteSettings.Count +
                        "; Target settings: " + _targetSiteSettings.Count + ".");
                }
            });
        }

        private void SetBusy(bool busy, string message)
        {
            _workInProgress = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;

            if (lblActivity != null)
            {
                lblActivity.Text = string.IsNullOrWhiteSpace(message) ? "Ready" : message;
            }

            if (progressBar != null)
            {
                progressBar.Visible = busy;
            }

            UpdateActionStates();
        }

        private void ShowConnectionRequired(string message, string title)
        {
            LogMessage(message);
            MessageBox.Show(
                this,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ShowOperationError(string message, Exception error)
        {
            string detail = error?.Message ?? "Unknown error.";
            LogMessage("Error: " + message + " " + detail);
            MessageBox.Show(
                this,
                message + "\r\n\r\n" + detail,
                "Power Pages ALM Drift Inspector",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static Guid TryGetOrganizationId(IOrganizationService service)
        {
            if (service == null)
            {
                return Guid.Empty;
            }

            try
            {
                WhoAmIResponse response =
                    (WhoAmIResponse)service.Execute(new WhoAmIRequest());
                return response.OrganizationId;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        private sealed class WebsiteListsLoadResult
        {
            public List<WebsiteModel> SourceWebsites { get; set; } = new List<WebsiteModel>();

            public List<WebsiteModel> TargetWebsites { get; set; } = new List<WebsiteModel>();
        }

        private sealed class WebsiteSelectionDisplayRow
        {
            public string Role { get; set; }

            public string Environment { get; set; }

            public string WebsiteName { get; set; }

            public string PartialUrl { get; set; }

            public Guid WebsiteId { get; set; }
        }

        private sealed class ComparisonLoadResult
        {
            public List<SiteSettingModel> SourceSettings { get; set; }

            public List<SiteSettingModel> TargetSettings { get; set; }

            public WebsiteModel SourceWebsite { get; set; }

            public WebsiteModel TargetWebsite { get; set; }

            public List<ComparisonResultModel> Results { get; set; }
        }

        private string GetCurrentEnvironmentName()
        {
            return GetConnectionName(_sourceConnectionDetail, "Unknown Environment");
        }

        private static string GetConnectionName(ConnectionDetail detail, string fallback)
        {
            if (detail == null)
            {
                return fallback;
            }

            if (!string.IsNullOrWhiteSpace(detail.ConnectionName))
            {
                return detail.ConnectionName;
            }

            return !string.IsNullOrWhiteSpace(detail.WebApplicationUrl)
                ? detail.WebApplicationUrl
                : fallback;
        }

        private static string NormalizeUrl(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().TrimEnd('/');
        }

        private string GetCurrentBottomGridWebsiteName()
        {
            if (_isComparisonView)
            {
                if (!string.IsNullOrWhiteSpace(_sourceWebsiteName) &&
                    string.Equals(_sourceWebsiteName, _targetWebsiteName, StringComparison.OrdinalIgnoreCase))
                {
                    return _sourceWebsiteName;
                }

                if (!string.IsNullOrWhiteSpace(_sourceWebsiteName))
                {
                    return _sourceWebsiteName;
                }

                if (!string.IsNullOrWhiteSpace(_targetWebsiteName))
                {
                    return _targetWebsiteName;
                }
            }

            if (!string.IsNullOrWhiteSpace(_targetWebsiteName) &&
                _targetSiteSettings != null &&
                _allSiteSettings != null &&
                _allSiteSettings.Count == _targetSiteSettings.Count)
            {
                return _targetWebsiteName;
            }

            if (!string.IsNullOrWhiteSpace(_sourceWebsiteName))
            {
                return _sourceWebsiteName;
            }

            return "Website";
        }

        private string GetCurrentBottomGridEnvironmentName()
        {
            if (!string.IsNullOrWhiteSpace(_targetEnvironmentName) &&
                _targetSiteSettings != null &&
                _allSiteSettings != null &&
                _allSiteSettings.Count == _targetSiteSettings.Count &&
                !_isComparisonView)
            {
                return _targetEnvironmentName;
            }

            if (!string.IsNullOrWhiteSpace(_sourceEnvironmentName) && !_isComparisonView)
            {
                return _sourceEnvironmentName;
            }

            return GetCurrentEnvironmentName();
        }

        private void BindWebsitePair(
            string sourceEnvironmentName,
            string targetEnvironmentName,
            WebsiteModel sourceWebsite,
            WebsiteModel targetWebsite)
        {
            List<WebsiteSelectionDisplayRow> rows = new List<WebsiteSelectionDisplayRow>();

            if (sourceWebsite != null)
            {
                rows.Add(new WebsiteSelectionDisplayRow
                {
                    Role = "Source",
                    Environment = sourceEnvironmentName ?? string.Empty,
                    WebsiteName = sourceWebsite.Name ?? string.Empty,
                    PartialUrl = sourceWebsite.PartialUrl ?? string.Empty,
                    WebsiteId = sourceWebsite.Id
                });
            }

            if (targetWebsite != null)
            {
                rows.Add(new WebsiteSelectionDisplayRow
                {
                    Role = "Target",
                    Environment = targetEnvironmentName ?? string.Empty,
                    WebsiteName = targetWebsite.Name ?? string.Empty,
                    PartialUrl = targetWebsite.PartialUrl ?? string.Empty,
                    WebsiteId = targetWebsite.Id
                });
            }

            dgvWebsites.AutoGenerateColumns = true;
            dgvWebsites.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvWebsites.DataSource = null;
            dgvWebsites.DataSource = rows;

            if (dgvWebsites.Columns["WebsiteId"] != null)
            {
                dgvWebsites.Columns["WebsiteId"].Visible = false;
            }

            if (dgvWebsites.Columns["Role"] != null)
            {
                dgvWebsites.Columns["Role"].HeaderText = "Role";
                dgvWebsites.Columns["Role"].FillWeight = 12;
                dgvWebsites.Columns["Role"].DisplayIndex = 0;
            }

            if (dgvWebsites.Columns["Environment"] != null)
            {
                dgvWebsites.Columns["Environment"].HeaderText = "Environment";
                dgvWebsites.Columns["Environment"].FillWeight = 25;
                dgvWebsites.Columns["Environment"].DisplayIndex = 1;
            }

            if (dgvWebsites.Columns["WebsiteName"] != null)
            {
                dgvWebsites.Columns["WebsiteName"].HeaderText = "Website Name";
                dgvWebsites.Columns["WebsiteName"].FillWeight = 31;
                dgvWebsites.Columns["WebsiteName"].DisplayIndex = 2;
            }

            if (dgvWebsites.Columns["PartialUrl"] != null)
            {
                dgvWebsites.Columns["PartialUrl"].HeaderText = "Partial URL";
                dgvWebsites.Columns["PartialUrl"].FillWeight = 32;
                dgvWebsites.Columns["PartialUrl"].DisplayIndex = 3;
            }

            UpdateWebsiteGridHeight();
            dgvWebsites.ClearSelection();
            UpdateEmptyStateLabels();
        }

        private void BindSiteSettings(List<SiteSettingModel> settings)
        {
            settings = settings ?? new List<SiteSettingModel>();

            dgvSiteSettings.AutoGenerateColumns = true;
            dgvSiteSettings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSiteSettings.DataSource = null;
            dgvSiteSettings.DataSource = settings;

            if (dgvSiteSettings.Columns["Id"] != null)
            {
                dgvSiteSettings.Columns["Id"].Visible = false;
            }

            if (dgvSiteSettings.Columns["WebsiteId"] != null)
            {
                dgvSiteSettings.Columns["WebsiteId"].Visible = false;
            }

            if (dgvSiteSettings.Columns["Name"] != null)
            {
                dgvSiteSettings.Columns["Name"].HeaderText = "Setting Name";
                dgvSiteSettings.Columns["Name"].FillWeight = 55;
                dgvSiteSettings.Columns["Name"].DisplayIndex = 0;
            }

            if (dgvSiteSettings.Columns["Value"] != null)
            {
                dgvSiteSettings.Columns["Value"].HeaderText = "Setting Value";
                dgvSiteSettings.Columns["Value"].FillWeight = 30;
                dgvSiteSettings.Columns["Value"].DisplayIndex = 1;
            }

            if (dgvSiteSettings.Columns["Category"] != null)
            {
                dgvSiteSettings.Columns["Category"].HeaderText = "Category";
                dgvSiteSettings.Columns["Category"].FillWeight = 15;
                dgvSiteSettings.Columns["Category"].DisplayIndex = 2;
            }

            ResetBottomGridRowStyling();
            ApplySiteSettingTooltips();
            SelectFirstRow(dgvSiteSettings);
            UpdateEmptyStateLabels();
        }

        private void BindComparisonResults(List<ComparisonResultModel> results)
        {
            results = results ?? new List<ComparisonResultModel>();

            dgvSiteSettings.AutoGenerateColumns = true;
            dgvSiteSettings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSiteSettings.DataSource = null;
            dgvSiteSettings.DataSource = results;

            if (dgvSiteSettings.Columns["IsMatch"] != null)
            {
                dgvSiteSettings.Columns["IsMatch"].Visible = false;
            }

            if (dgvSiteSettings.Columns["IsFinding"] != null)
            {
                dgvSiteSettings.Columns["IsFinding"].Visible = false;
            }

            if (dgvSiteSettings.Columns["IsEnvironmentSpecificCandidate"] != null)
            {
                dgvSiteSettings.Columns["IsEnvironmentSpecificCandidate"].Visible = false;
            }

            if (dgvSiteSettings.Columns["SourceRecordCount"] != null)
            {
                dgvSiteSettings.Columns["SourceRecordCount"].Visible = false;
            }

            if (dgvSiteSettings.Columns["TargetRecordCount"] != null)
            {
                dgvSiteSettings.Columns["TargetRecordCount"].Visible = false;
            }

            if (dgvSiteSettings.Columns["SourceDistinctValueCount"] != null)
            {
                dgvSiteSettings.Columns["SourceDistinctValueCount"].Visible = false;
            }

            if (dgvSiteSettings.Columns["TargetDistinctValueCount"] != null)
            {
                dgvSiteSettings.Columns["TargetDistinctValueCount"].Visible = false;
            }

            if (dgvSiteSettings.Columns["DuplicateClassification"] != null)
            {
                dgvSiteSettings.Columns["DuplicateClassification"].Visible = false;
            }

            if (dgvSiteSettings.Columns["SourceValue"] != null)
            {
                dgvSiteSettings.Columns["SourceValue"].Visible = false;
            }

            if (dgvSiteSettings.Columns["TargetValue"] != null)
            {
                dgvSiteSettings.Columns["TargetValue"].Visible = false;
            }

            if (dgvSiteSettings.Columns["ReviewFocus"] != null)
            {
                dgvSiteSettings.Columns["ReviewFocus"].Visible = false;
            }

            if (dgvSiteSettings.Columns["EnvironmentSpecificReason"] != null)
            {
                dgvSiteSettings.Columns["EnvironmentSpecificReason"].Visible = false;
            }

            if (dgvSiteSettings.Columns["RecommendedAction"] != null)
            {
                dgvSiteSettings.Columns["RecommendedAction"].Visible = false;
            }

            foreach (DataGridViewColumn column in dgvSiteSettings.Columns)
            {
                column.Frozen = false;
            }

            if (dgvSiteSettings.Columns["SettingName"] != null)
            {
                dgvSiteSettings.Columns["SettingName"].HeaderText = "Setting Name";
                dgvSiteSettings.Columns["SettingName"].FillWeight = 30;
                dgvSiteSettings.Columns["SettingName"].DisplayIndex = 0;
            }

            if (dgvSiteSettings.Columns["SourceDisplayValue"] != null)
            {
                dgvSiteSettings.Columns["SourceDisplayValue"].HeaderText = "Source Value";
                dgvSiteSettings.Columns["SourceDisplayValue"].FillWeight = 25;
                dgvSiteSettings.Columns["SourceDisplayValue"].DisplayIndex = 1;
            }

            if (dgvSiteSettings.Columns["TargetDisplayValue"] != null)
            {
                dgvSiteSettings.Columns["TargetDisplayValue"].HeaderText = "Target Value";
                dgvSiteSettings.Columns["TargetDisplayValue"].FillWeight = 25;
                dgvSiteSettings.Columns["TargetDisplayValue"].DisplayIndex = 2;
            }

            if (dgvSiteSettings.Columns["Category"] != null)
            {
                dgvSiteSettings.Columns["Category"].HeaderText = "Category";
                dgvSiteSettings.Columns["Category"].FillWeight = 11;
                dgvSiteSettings.Columns["Category"].DisplayIndex = 3;
            }

            if (dgvSiteSettings.Columns["Status"] != null)
            {
                dgvSiteSettings.Columns["Status"].HeaderText = "Status";
                dgvSiteSettings.Columns["Status"].FillWeight = 9;
                dgvSiteSettings.Columns["Status"].DisplayIndex = 4;
            }

            ApplyComparisonRowHighlighting();
            ApplyComparisonTooltips();
            SelectFirstRow(dgvSiteSettings);
            UpdateEmptyStateLabels();
        }

        private void ApplySiteSettingTooltips()
        {
            if (dgvSiteSettings == null || _isComparisonView)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvSiteSettings.Rows)
            {
                SiteSettingModel setting = row.DataBoundItem as SiteSettingModel;

                if (setting == null)
                {
                    continue;
                }

                if (dgvSiteSettings.Columns["Name"] != null)
                {
                    row.Cells["Name"].ToolTipText = setting.Name ?? string.Empty;
                }

                if (dgvSiteSettings.Columns["Value"] != null)
                {
                    row.Cells["Value"].ToolTipText = setting.Value ?? string.Empty;
                }

                if (dgvSiteSettings.Columns["Category"] != null)
                {
                    row.Cells["Category"].ToolTipText = setting.Category ?? string.Empty;
                }
            }
        }

        private void ApplyComparisonTooltips()
        {
            if (dgvSiteSettings == null || !_isComparisonView)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvSiteSettings.Rows)
            {
                ComparisonResultModel result = row.DataBoundItem as ComparisonResultModel;

                if (result == null)
                {
                    continue;
                }

                if (dgvSiteSettings.Columns["SettingName"] != null)
                {
                    row.Cells["SettingName"].ToolTipText = result.SettingName ?? string.Empty;
                }

                if (dgvSiteSettings.Columns["SourceDisplayValue"] != null)
                {
                    row.Cells["SourceDisplayValue"].ToolTipText =
                        BuildComparisonValueToolTip(
                            "Source",
                            result.SourceDisplayValue,
                            result.SourceValue,
                            result.SourceRecordCount,
                            result.SourceDistinctValueCount);
                }

                if (dgvSiteSettings.Columns["TargetDisplayValue"] != null)
                {
                    row.Cells["TargetDisplayValue"].ToolTipText =
                        BuildComparisonValueToolTip(
                            "Target",
                            result.TargetDisplayValue,
                            result.TargetValue,
                            result.TargetRecordCount,
                            result.TargetDistinctValueCount);
                }

                if (dgvSiteSettings.Columns["Category"] != null)
                {
                    row.Cells["Category"].ToolTipText = result.Category ?? string.Empty;
                }

                if (dgvSiteSettings.Columns["Status"] != null)
                {
                    row.Cells["Status"].ToolTipText = BuildComparisonStatusToolTip(result);
                }
            }
        }

        private static string BuildComparisonValueToolTip(
            string environmentRole,
            string displayValue,
            string fullValue,
            int recordCount,
            int distinctValueCount)
        {
            if (string.Equals(displayValue, fullValue, StringComparison.Ordinal))
            {
                return fullValue ?? string.Empty;
            }

            StringBuilder tooltip = new StringBuilder();
            tooltip.AppendLine(
                environmentRole +
                " record summary (" +
                recordCount +
                " record(s), " +
                distinctValueCount +
                " distinct value(s)):");
            tooltip.AppendLine(displayValue ?? string.Empty);
            tooltip.AppendLine();
            tooltip.AppendLine("Full record evidence:");
            tooltip.Append(fullValue ?? string.Empty);

            return tooltip.ToString();
        }

        private static string BuildComparisonStatusToolTip(ComparisonResultModel result)
        {
            if (result == null)
            {
                return string.Empty;
            }

            StringBuilder tooltip = new StringBuilder();
            tooltip.AppendLine("Status: " + (result.Status ?? string.Empty));

            if (!string.IsNullOrWhiteSpace(result.ReviewFocus))
            {
                tooltip.AppendLine();
                tooltip.AppendLine("Review Focus:");
                tooltip.AppendLine(result.ReviewFocus);
            }

            if (!string.IsNullOrWhiteSpace(result.EnvironmentSpecificReason))
            {
                tooltip.AppendLine();
                tooltip.AppendLine("Environment-Specific Note:");
                tooltip.AppendLine(result.EnvironmentSpecificReason);
            }

            if (!string.IsNullOrWhiteSpace(result.RecommendedAction))
            {
                tooltip.AppendLine();
                tooltip.AppendLine("Recommended Action:");
                tooltip.AppendLine(result.RecommendedAction);
            }

            tooltip.AppendLine();
            tooltip.Append("Double-click the row for complete comparison details.");

            return tooltip.ToString();
        }

        private void ResetBottomGridRowStyling()
        {
            if (dgvSiteSettings == null)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvSiteSettings.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.Empty;
                row.DefaultCellStyle.ForeColor = Color.Empty;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 90, 158);
                row.DefaultCellStyle.SelectionForeColor = Color.White;
                row.DefaultCellStyle.Font = dgvSiteSettings.Font;
            }
        }

        private void ApplyComparisonRowHighlighting()
        {
            if (dgvSiteSettings == null || !_isComparisonView)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvSiteSettings.Rows)
            {
                ComparisonResultModel result = row.DataBoundItem as ComparisonResultModel;

                row.DefaultCellStyle.Font = dgvSiteSettings.Font;
                row.DefaultCellStyle.BackColor = Color.Empty;
                row.DefaultCellStyle.ForeColor = Color.Empty;
                row.DefaultCellStyle.SelectionBackColor = Color.Empty;
                row.DefaultCellStyle.SelectionForeColor = Color.Empty;

                if (result == null || string.IsNullOrWhiteSpace(result.Status))
                {
                    continue;
                }

                if (string.Equals(result.Status, StatusDifferentValue, StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 242, 204);
                }
                else if (string.Equals(result.Status, StatusMissingInSource, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(result.Status, StatusMissingInTarget, StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 228, 225);
                    row.DefaultCellStyle.Font = new Font(dgvSiteSettings.Font, FontStyle.Bold);
                }
                else if (string.Equals(result.Status, StatusDuplicate, StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(248, 215, 218);
                    row.DefaultCellStyle.Font = new Font(dgvSiteSettings.Font, FontStyle.Bold);
                }
                else if (string.Equals(result.Status, StatusMatch, StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(226, 239, 218);
                }
            }
        }

        private void dgvSiteSettings_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e == null || e.RowIndex < 0 || dgvSiteSettings == null || e.RowIndex >= dgvSiteSettings.Rows.Count)
            {
                return;
            }

            if (_isComparisonView)
            {
                ComparisonResultModel selectedResult = dgvSiteSettings.Rows[e.RowIndex].DataBoundItem as ComparisonResultModel;

                if (selectedResult == null)
                {
                    return;
                }

                OpenComparisonResultDetail(selectedResult);
                return;
            }

            SiteSettingModel selectedSetting = dgvSiteSettings.Rows[e.RowIndex].DataBoundItem as SiteSettingModel;

            if (selectedSetting == null)
            {
                return;
            }

            OpenSiteSettingDetail(selectedSetting);
        }

        private void OpenSiteSettingDetail(SiteSettingModel setting)
        {
            if (setting == null)
            {
                return;
            }

            try
            {
                LogMessage("Opening site setting detail view for '" + (setting.Name ?? string.Empty) + "'.");

                using (SiteSettingDetailForm detailForm = new SiteSettingDetailForm(
                    setting,
                    GetCurrentBottomGridEnvironmentName(),
                    GetCurrentBottomGridWebsiteName()))
                {
                    detailForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error opening site setting detail view: " + ex.Message);

                MessageBox.Show(
                    ex.ToString(),
                    "Detail View Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OpenComparisonResultDetail(ComparisonResultModel result)
        {
            if (result == null)
            {
                return;
            }

            try
            {
                LogMessage("Opening comparison detail view for setting '" + (result.SettingName ?? string.Empty) + "'.");

                using (ComparisonResultDetailForm detailForm = new ComparisonResultDetailForm(
                    result,
                    _sourceEnvironmentName,
                    _targetEnvironmentName,
                    _sourceWebsiteName,
                    _targetWebsiteName))
                {
                    detailForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error opening comparison detail view: " + ex.Message);

                MessageBox.Show(
                    ex.ToString(),
                    "Detail View Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void cboStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isComparisonView)
            {
                ApplyBottomGridFilter();
            }

            UpdateActionStates();
        }

        private void cboCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyBottomGridFilter();
            UpdateActionStates();
        }

        private void chkFindingsOnly_CheckedChanged(object sender, EventArgs e)
        {
            _showFindingsOnly = chkFindingsOnly != null && chkFindingsOnly.Checked;

            if (_isComparisonView)
            {
                ApplyBottomGridFilter();
            }

            UpdateActionStates();
        }

        private void SelectFirstRow(DataGridView grid)
        {
            if (grid == null || grid.Rows.Count == 0)
            {
                return;
            }

            grid.ClearSelection();

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (!row.Visible)
                {
                    continue;
                }

                row.Selected = true;

                foreach (DataGridViewColumn column in grid.Columns)
                {
                    if (column.Visible)
                    {
                        grid.CurrentCell = row.Cells[column.Index];
                        return;
                    }
                }
            }
        }

        private void ApplyBottomGridFilter()
        {
            if (dgvSiteSettings == null)
            {
                return;
            }

            UpdateCategoryFilterItems();

            if (_isComparisonView)
            {
                List<ComparisonResultModel> filteredResults = GetFilteredComparisonResults();
                BindComparisonResults(filteredResults);
                SetComparisonResultsCount(filteredResults.Count, _comparisonResults.Count);
            }
            else
            {
                List<SiteSettingModel> filteredSettings = GetFilteredSiteSettings();
                BindSiteSettings(filteredSettings);
                SetSiteSettingsCount(filteredSettings.Count, _allSiteSettings.Count);
            }

            UpdateEmptyStateLabels();
            UpdateActionStates();
        }

        private List<SiteSettingModel> GetFilteredSiteSettings()
        {
            string filterText = txtFilterSettings != null
                ? txtFilterSettings.Text.Trim()
                : string.Empty;

            string selectedCategory = GetSelectedCategoryFilter();

            List<SiteSettingModel> filteredSettings = new List<SiteSettingModel>(_allSiteSettings ?? new List<SiteSettingModel>());

            if (!string.Equals(selectedCategory, AllCategoriesText, StringComparison.OrdinalIgnoreCase))
            {
                filteredSettings = filteredSettings.FindAll(setting =>
                    string.Equals(setting?.Category, selectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            if (string.IsNullOrWhiteSpace(filterText))
            {
                return filteredSettings;
            }

            return filteredSettings.FindAll(setting =>
                (!string.IsNullOrWhiteSpace(setting?.Name) &&
                 setting.Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(setting?.Value) &&
                 setting.Value.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(setting?.Category) &&
                 setting.Category.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private List<ComparisonResultModel> GetFilteredComparisonResults()
        {
            string filterText = txtFilterSettings != null
                ? txtFilterSettings.Text.Trim()
                : string.Empty;

            string selectedStatus = GetSelectedStatusFilter();
            string selectedCategory = GetSelectedCategoryFilter();

            List<ComparisonResultModel> filteredResults =
                new List<ComparisonResultModel>(_comparisonResults ?? new List<ComparisonResultModel>());

            if (!string.Equals(selectedStatus, AllStatusesText, StringComparison.OrdinalIgnoreCase))
            {
                filteredResults = filteredResults.FindAll(result =>
                    string.Equals(result?.Status, selectedStatus, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.Equals(selectedCategory, AllCategoriesText, StringComparison.OrdinalIgnoreCase))
            {
                filteredResults = filteredResults.FindAll(result =>
                    string.Equals(result?.Category, selectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            if (_showFindingsOnly)
            {
                filteredResults = filteredResults.FindAll(result => result != null && !result.IsMatch);
            }

            if (string.IsNullOrWhiteSpace(filterText))
            {
                return filteredResults;
            }

            return filteredResults.FindAll(result =>
                (!string.IsNullOrWhiteSpace(result?.SettingName) &&
                 result.SettingName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(result?.SourceValue) &&
                 result.SourceValue.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(result?.TargetValue) &&
                 result.TargetValue.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(result?.Category) &&
                 result.Category.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(result?.Status) &&
                 result.Status.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(result?.DuplicateClassification) &&
                 result.DuplicateClassification.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(result?.ReviewFocus) &&
                 result.ReviewFocus.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(result?.EnvironmentSpecificReason) &&
                 result.EnvironmentSpecificReason.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                ||
                (!string.IsNullOrWhiteSpace(result?.RecommendedAction) &&
                 result.RecommendedAction.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private string GetSelectedStatusFilter()
        {
            if (cboStatusFilter == null || cboStatusFilter.SelectedItem == null)
            {
                return AllStatusesText;
            }

            string selectedValue = cboStatusFilter.SelectedItem.ToString();

            return string.IsNullOrWhiteSpace(selectedValue)
                ? AllStatusesText
                : selectedValue;
        }

        private string GetSelectedCategoryFilter()
        {
            if (cboCategoryFilter == null || cboCategoryFilter.SelectedItem == null)
            {
                return AllCategoriesText;
            }

            string selectedValue = cboCategoryFilter.SelectedItem.ToString();

            return string.IsNullOrWhiteSpace(selectedValue)
                ? AllCategoriesText
                : selectedValue;
        }

        private List<ComparisonResultModel> CompareSiteSettings(
            List<SiteSettingModel> sourceSettings,
            List<SiteSettingModel> targetSettings)
        {
            List<ComparisonResultModel> results = new List<ComparisonResultModel>();

            Dictionary<string, List<SiteSettingModel>> sourceLookup =
                BuildSiteSettingLookup(sourceSettings);

            Dictionary<string, List<SiteSettingModel>> targetLookup =
                BuildSiteSettingLookup(targetSettings);

            HashSet<string> allSettingNames =
                new HashSet<string>(sourceLookup.Keys, StringComparer.OrdinalIgnoreCase);

            foreach (string key in targetLookup.Keys)
            {
                allSettingNames.Add(key);
            }

            foreach (string settingName in allSettingNames)
            {
                sourceLookup.TryGetValue(settingName, out List<SiteSettingModel> sourceMatches);
                targetLookup.TryGetValue(settingName, out List<SiteSettingModel> targetMatches);

                sourceMatches = sourceMatches ?? new List<SiteSettingModel>();
                targetMatches = targetMatches ?? new List<SiteSettingModel>();

                SiteSettingModel sourceSetting = sourceMatches.FirstOrDefault();
                SiteSettingModel targetSetting = targetMatches.FirstOrDefault();
                string sourceValue = FormatComparedValues(sourceMatches);
                string targetValue = FormatComparedValues(targetMatches);
                int sourceDistinctValueCount = GetDistinctValueCount(sourceMatches);
                int targetDistinctValueCount = GetDistinctValueCount(targetMatches);

                string status;

                if (sourceMatches.Count > 1 || targetMatches.Count > 1)
                {
                    status = StatusDuplicate;
                }
                else if (sourceSetting == null)
                {
                    status = StatusMissingInSource;
                }
                else if (targetSetting == null)
                {
                    status = StatusMissingInTarget;
                }
                else if (AreSiteSettingValuesEquivalent(sourceValue, targetValue))
                {
                    status = StatusMatch;
                }
                else
                {
                    status = StatusDifferentValue;
                }

                bool isDuplicate = string.Equals(
                    status,
                    StatusDuplicate,
                    StringComparison.OrdinalIgnoreCase);

                results.Add(new ComparisonResultModel
                {
                    SettingName = settingName,
                    SourceValue = sourceValue,
                    TargetValue = targetValue,
                    SourceDisplayValue = isDuplicate
                        ? FormatDuplicateValueSummary(sourceMatches)
                        : sourceValue,
                    TargetDisplayValue = isDuplicate
                        ? FormatDuplicateValueSummary(targetMatches)
                        : targetValue,
                    Category = sourceSetting?.Category ?? targetSetting?.Category ?? string.Empty,
                    Status = status,
                    SourceRecordCount = sourceMatches.Count,
                    TargetRecordCount = targetMatches.Count,
                    SourceDistinctValueCount = sourceDistinctValueCount,
                    TargetDistinctValueCount = targetDistinctValueCount,
                    DuplicateClassification = isDuplicate
                        ? GetDuplicateClassification(
                            sourceMatches.Count,
                            sourceDistinctValueCount,
                            targetMatches.Count,
                            targetDistinctValueCount)
                        : string.Empty
                });
            }

            results.Sort((a, b) =>
            {
                int statusCompare = GetStatusSortOrder(a.Status).CompareTo(GetStatusSortOrder(b.Status));

                if (statusCompare != 0)
                {
                    return statusCompare;
                }

                int categoryCompare = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);

                if (categoryCompare != 0)
                {
                    return categoryCompare;
                }

                return string.Compare(a.SettingName, b.SettingName, StringComparison.OrdinalIgnoreCase);
            });

            return results;
        }

        private static Dictionary<string, List<SiteSettingModel>> BuildSiteSettingLookup(
            List<SiteSettingModel> settings)
        {
            Dictionary<string, List<SiteSettingModel>> lookup =
                new Dictionary<string, List<SiteSettingModel>>(StringComparer.OrdinalIgnoreCase);

            foreach (SiteSettingModel setting in settings ?? new List<SiteSettingModel>())
            {
                if (setting == null)
                {
                    continue;
                }

                string key = string.IsNullOrWhiteSpace(setting.Name)
                    ? "(Unnamed site setting)"
                    : setting.Name.Trim();

                if (!lookup.TryGetValue(key, out List<SiteSettingModel> matches))
                {
                    matches = new List<SiteSettingModel>();
                    lookup.Add(key, matches);
                }

                matches.Add(setting);
            }

            return lookup;
        }

        private static string FormatComparedValues(List<SiteSettingModel> settings)
        {
            settings = settings ?? new List<SiteSettingModel>();

            if (settings.Count == 0)
            {
                return string.Empty;
            }

            if (settings.Count == 1)
            {
                return settings[0]?.Value ?? string.Empty;
            }

            return string.Join(
                Environment.NewLine,
                settings
                    .Select((setting, index) =>
                        "Record " + (index + 1) +
                        " (" + (setting?.Id ?? Guid.Empty) + "): " +
                        (setting?.Value ?? string.Empty))
                    .ToArray());
        }

        private static int GetDistinctValueCount(List<SiteSettingModel> settings)
        {
            return (settings ?? new List<SiteSettingModel>())
                .Select(setting => setting?.Value ?? string.Empty)
                .Distinct(SiteSettingValueComparer.Instance)
                .Count();
        }

        private static string FormatDuplicateValueSummary(List<SiteSettingModel> settings)
        {
            settings = settings ?? new List<SiteSettingModel>();

            if (settings.Count == 0)
            {
                return "0 records";
            }

            if (settings.Count == 1)
            {
                return "1 record: " +
                    FormatCompactDuplicateValue(settings[0]?.Value ?? string.Empty);
            }

            List<IGrouping<string, SiteSettingModel>> valueGroups = settings
                .GroupBy(
                    setting => setting?.Value ?? string.Empty,
                    SiteSettingValueComparer.Instance)
                .ToList();

            string valueLabel = valueGroups.Count == 1 ? "value" : "values";

            string[] visibleValueSummaries = valueGroups
                .Take(3)
                .Select(group =>
                    FormatCompactDuplicateValue(group.Key) +
                    " \u00d7" +
                    group.Count())
                .ToArray();

            string valueSummary = string.Join("; ", visibleValueSummaries);

            if (valueGroups.Count > visibleValueSummaries.Length)
            {
                valueSummary += "; +" +
                    (valueGroups.Count - visibleValueSummaries.Length) +
                    " more";
            }

            return settings.Count +
                " records / " +
                valueGroups.Count +
                " " +
                valueLabel +
                ": " +
                valueSummary;
        }

        private static bool AreSiteSettingValuesEquivalent(
            string sourceValue,
            string targetValue)
        {
            bool sourceBoolean;
            bool targetBoolean;

            if (TryParseBooleanSiteSettingValue(sourceValue, out sourceBoolean) &&
                TryParseBooleanSiteSettingValue(targetValue, out targetBoolean))
            {
                return sourceBoolean == targetBoolean;
            }

            return string.Equals(
                sourceValue ?? string.Empty,
                targetValue ?? string.Empty,
                StringComparison.Ordinal);
        }

        private static bool TryParseBooleanSiteSettingValue(
            string value,
            out bool parsedValue)
        {
            return bool.TryParse(
                (value ?? string.Empty).Trim(),
                out parsedValue);
        }

        private sealed class SiteSettingValueComparer : IEqualityComparer<string>
        {
            public static readonly SiteSettingValueComparer Instance =
                new SiteSettingValueComparer();

            private SiteSettingValueComparer()
            {
            }

            public bool Equals(string x, string y)
            {
                return AreSiteSettingValuesEquivalent(x, y);
            }

            public int GetHashCode(string value)
            {
                bool parsedBoolean;

                if (TryParseBooleanSiteSettingValue(value, out parsedBoolean))
                {
                    return parsedBoolean.GetHashCode();
                }

                return StringComparer.Ordinal.GetHashCode(value ?? string.Empty);
            }
        }

        private static string FormatCompactDuplicateValue(string value)
        {
            if (value == null)
            {
                return "(empty)";
            }

            string compactValue = value
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

            if (compactValue.Length == 0)
            {
                return "(empty)";
            }

            const int maximumLength = 72;

            return compactValue.Length <= maximumLength
                ? compactValue
                : compactValue.Substring(0, maximumLength - 1) + "\u2026";
        }

        private static string GetDuplicateClassification(
            int sourceRecordCount,
            int sourceDistinctValueCount,
            int targetRecordCount,
            int targetDistinctValueCount)
        {
            bool sourceHasDuplicates = sourceRecordCount > 1;
            bool targetHasDuplicates = targetRecordCount > 1;
            bool sourceHasConflictingValues =
                sourceHasDuplicates && sourceDistinctValueCount > 1;
            bool targetHasConflictingValues =
                targetHasDuplicates && targetDistinctValueCount > 1;

            if (sourceHasConflictingValues && targetHasConflictingValues)
            {
                return "Conflicting duplicate values in Source and Target";
            }

            if (sourceHasConflictingValues)
            {
                return targetHasDuplicates
                    ? "Conflicting duplicate values in Source; repeated duplicate records in Target"
                    : "Conflicting duplicate values in Source";
            }

            if (targetHasConflictingValues)
            {
                return sourceHasDuplicates
                    ? "Repeated duplicate records in Source; conflicting duplicate values in Target"
                    : "Conflicting duplicate values in Target";
            }

            if (sourceHasDuplicates && targetHasDuplicates)
            {
                return "Repeated duplicate records in Source and Target";
            }

            if (sourceHasDuplicates)
            {
                return "Repeated duplicate records in Source";
            }

            if (targetHasDuplicates)
            {
                return "Repeated duplicate records in Target";
            }

            return "Duplicate site-setting records";
        }

        private static int GetStatusSortOrder(string status)
        {
            if (string.Equals(status, StatusDuplicate, StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }

            if (string.Equals(status, StatusDifferentValue, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (string.Equals(status, StatusMissingInSource, StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (string.Equals(status, StatusMissingInTarget, StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            if (string.Equals(status, StatusMatch, StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            return 4;
        }

        private void ExportSiteSettingsToCsv(List<SiteSettingModel> settings, string websiteName, string filePath)
        {
            StringBuilder csvBuilder = new StringBuilder();

            csvBuilder.AppendLine("sep=;");
            csvBuilder.AppendLine("Report Type;Power Pages Site Settings Export");
            csvBuilder.AppendLine("Environment;" + EscapeCsv(GetCurrentBottomGridEnvironmentName()));
            csvBuilder.AppendLine("Website;" + EscapeCsv(websiteName));
            csvBuilder.AppendLine("Exported At;" + EscapeCsv(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            csvBuilder.AppendLine("Visible Settings;" + settings.Count);
            csvBuilder.AppendLine("Total Settings;" + (_allSiteSettings?.Count ?? 0));
            csvBuilder.AppendLine();
            csvBuilder.AppendLine("Website Name;Setting Name;Setting Value;Category");

            foreach (SiteSettingModel setting in settings)
            {
                csvBuilder.Append(EscapeCsv(websiteName));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(setting?.Name));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(setting?.Value));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(setting?.Category));
                csvBuilder.AppendLine();
            }

            File.WriteAllText(filePath, csvBuilder.ToString(), new UTF8Encoding(true));

            LogMessage("Exported " + settings.Count + " site setting(s).");
        }

        private void ExportComparisonResultsToCsv(List<ComparisonResultModel> results, string filePath)
        {
            StringBuilder csvBuilder = new StringBuilder();

            GetComparisonSummaryCounts(
                out int total,
                out int different,
                out int matches,
                out int missingInSource,
                out int missingInTarget,
                out int duplicates);

            csvBuilder.AppendLine("sep=;");
            csvBuilder.AppendLine("Report Type;Power Pages ALM Drift Comparison");
            csvBuilder.AppendLine("Source Environment;" + EscapeCsv(_sourceEnvironmentName));
            csvBuilder.AppendLine("Target Environment;" + EscapeCsv(_targetEnvironmentName));
            csvBuilder.AppendLine("Source Website;" + EscapeCsv(_sourceWebsiteName));
            csvBuilder.AppendLine("Target Website;" + EscapeCsv(_targetWebsiteName));
            csvBuilder.AppendLine("Exported At;" + EscapeCsv(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            csvBuilder.AppendLine("Filtered Results;" + results.Count);
            csvBuilder.AppendLine("Total Results;" + total);
            csvBuilder.AppendLine("Differences;" + different);
            csvBuilder.AppendLine("Matches;" + matches);
            csvBuilder.AppendLine("Missing in Source;" + missingInSource);
            csvBuilder.AppendLine("Missing in Target;" + missingInTarget);
            csvBuilder.AppendLine("Duplicates;" + duplicates);
            csvBuilder.AppendLine("Filters;" + EscapeCsv(GetActiveFilterSummary()));
            csvBuilder.AppendLine();
            csvBuilder.AppendLine("Status;Duplicate Classification;Category;Setting Name;Source Value;Target Value;Review Focus;Environment-Specific Candidate;Environment-Specific Note;Recommended Action");

            foreach (ComparisonResultModel result in results)
            {
                csvBuilder.Append(EscapeCsv(result?.Status));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result?.DuplicateClassification));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result?.Category));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result?.SettingName));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result?.SourceValue));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result?.TargetValue));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result?.ReviewFocus));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result != null && result.IsEnvironmentSpecificCandidate ? "Yes" : "No"));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result?.EnvironmentSpecificReason));
                csvBuilder.Append(";");
                csvBuilder.Append(EscapeCsv(result?.RecommendedAction));
                csvBuilder.AppendLine();
            }

            File.WriteAllText(filePath, csvBuilder.ToString(), new UTF8Encoding(true));

            LogMessage("Exported " + results.Count + " comparison result(s).");
        }

        private void ExportSiteSettingsToHtmlReport(List<SiteSettingModel> settings, string websiteName, string filePath)
        {
            settings = settings ?? new List<SiteSettingModel>();

            string html = BuildSiteSettingsHtmlReport(settings, websiteName);
            File.WriteAllText(filePath, html, new UTF8Encoding(true));

            LogMessage("Exported " + settings.Count + " site setting(s) to HTML report.");
        }

        private void ExportComparisonResultsToHtmlReport(List<ComparisonResultModel> results, string filePath)
        {
            results = results ?? new List<ComparisonResultModel>();

            string html = BuildComparisonHtmlReport(results);
            File.WriteAllText(filePath, html, new UTF8Encoding(true));

            LogMessage("Exported " + results.Count + " comparison result(s) to HTML report.");
        }

        private string BuildSiteSettingsHtmlReport(List<SiteSettingModel> settings, string websiteName)
        {
            settings = settings ?? new List<SiteSettingModel>();

            int totalSettings = _allSiteSettings?.Count ?? 0;
            int visibleSettings = settings.Count;
            int categoryCount = settings
                .Where(setting => !string.IsNullOrWhiteSpace(setting?.Category))
                .Select(setting => setting.Category.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            StringBuilder html = new StringBuilder();
            AppendHtmlDocumentStart(html, "Power Pages Site Settings Export");

            html.AppendLine("<div class=\"shell\">");
            html.AppendLine("<section class=\"hero\">");
            html.AppendLine("<div>");
            html.AppendLine("<div class=\"eyebrow\">POWER PAGES ALM DRIFT INSPECTOR</div>");
            html.AppendLine("<h1>Power Pages Site Settings Export</h1>");
            html.AppendLine("<p>Inventory evidence for Power Pages site settings from a Dataverse environment.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"hero-badge neutral\">Inventory</div>");
            html.AppendLine("</section>");

            AppendSensitiveInformationNotice(html);

            html.AppendLine("<section class=\"panel\">");
            html.AppendLine("<div class=\"section-title\">Export context</div>");
            html.AppendLine("<div class=\"meta-grid\">");
            AppendHtmlMetaCard(html, "Environment", GetCurrentBottomGridEnvironmentName());
            AppendHtmlMetaCard(html, "Website", websiteName);
            AppendHtmlMetaCard(html, "Exported At", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AppendHtmlMetaCard(html, "Filters", GetActiveFilterSummary());
            html.AppendLine("</div>");
            html.AppendLine("</section>");

            html.AppendLine("<section class=\"panel\">");
            html.AppendLine("<div class=\"section-title\">Summary</div>");
            html.AppendLine("<div class=\"card-grid\">");
            AppendHtmlMetricCard(html, "Visible Settings", visibleSettings.ToString(), "Rows included in this export");
            AppendHtmlMetricCard(html, "Total Settings", totalSettings.ToString(), "Total loaded in the current view");
            AppendHtmlMetricCard(html, "Categories", categoryCount.ToString(), "Distinct categories in exported rows");
            AppendHtmlMetricCard(html, "Report Type", "Inventory", "Site setting export");
            html.AppendLine("</div>");
            html.AppendLine("</section>");

            html.AppendLine("<section class=\"panel\">");
            html.AppendLine("<div class=\"section-title\">Site settings</div>");
            html.AppendLine("<div class=\"table-wrap\">");
            html.AppendLine("<table class=\"inventory-table\">");
            html.AppendLine("<colgroup>");
            html.AppendLine("<col style=\"width:220px\" />");
            html.AppendLine("<col style=\"width:180px\" />");
            html.AppendLine("<col style=\"width:420px\" />");
            html.AppendLine("<col style=\"width:500px\" />");
            html.AppendLine("</colgroup>");
            html.AppendLine("<thead><tr><th>Website Name</th><th>Category</th><th>Setting Name</th><th>Setting Value</th></tr></thead>");
            html.AppendLine("<tbody>");

            foreach (SiteSettingModel setting in settings)
            {
                html.AppendLine("<tr>");
                html.AppendLine("<td>" + HtmlEncode(websiteName) + "</td>");
                html.AppendLine("<td>" + HtmlEncode(setting?.Category) + "</td>");
                html.AppendLine("<td>" + HtmlEncode(setting?.Name) + "</td>");
                html.AppendLine("<td class=\"value-cell\">" + HtmlValue(setting?.Value) + "</td>");
                html.AppendLine("</tr>");
            }

            if (settings.Count == 0)
            {
                html.AppendLine("<tr><td colspan=\"4\" class=\"empty-row\">No site settings were available for this export.</td></tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");
            html.AppendLine("</section>");

            AppendHtmlFooter(html);
            html.AppendLine("</div>");
            AppendHtmlDocumentEnd(html);

            return html.ToString();
        }

        private string BuildComparisonHtmlReport(List<ComparisonResultModel> exportedResults)
        {
            exportedResults = exportedResults ?? new List<ComparisonResultModel>();

            List<ComparisonResultModel> allResults = _comparisonResults ?? new List<ComparisonResultModel>();

            GetComparisonSummaryCountsForResults(
                allResults,
                out int total,
                out int different,
                out int matches,
                out int missingInSource,
                out int missingInTarget,
                out int duplicates);

            GetComparisonSummaryCountsForResults(
                exportedResults,
                out int exportedTotal,
                out int exportedDifferent,
                out int exportedMatches,
                out int exportedMissingInSource,
                out int exportedMissingInTarget,
                out int exportedDuplicates);

            GetComparisonDecisionInfo(
                allResults,
                out string decisionLabel,
                out string decisionClass,
                out string decisionSummary,
                out string recommendedNextStep);

            StringBuilder html = new StringBuilder();
            AppendHtmlDocumentStart(html, "Power Pages ALM Drift Comparison Report");

            html.AppendLine("<div class=\"shell\">");
            html.AppendLine("<section class=\"hero\">");
            html.AppendLine("<div>");
            html.AppendLine("<div class=\"eyebrow\">POWER PAGES ALM DRIFT INSPECTOR</div>");
            html.AppendLine("<h1>Power Pages ALM Drift Report</h1>");
            html.AppendLine("<p>Configuration drift evidence for Power Pages site settings across Dataverse environments.</p>");
            html.AppendLine("</div>");
            html.AppendLine("<div class=\"hero-badge " + decisionClass + "\">" + HtmlEncode(decisionLabel) + "</div>");
            html.AppendLine("</section>");

            html.AppendLine("<section class=\"decision-panel " + decisionClass + "\">");
            html.AppendLine("<div class=\"decision-kicker\">ALM DRIFT DECISION</div>");
            html.AppendLine("<h2>" + HtmlEncode(decisionLabel) + "</h2>");
            html.AppendLine("<p>" + HtmlEncode(decisionSummary) + "</p>");
            html.AppendLine("<p><strong>Recommended next step:</strong> " + HtmlEncode(recommendedNextStep) + "</p>");
            html.AppendLine("</section>");

            AppendSensitiveInformationNotice(html);

            html.AppendLine("<section class=\"panel\">");
            html.AppendLine("<div class=\"section-title\">Comparison context</div>");
            html.AppendLine("<div class=\"meta-grid\">");
            AppendHtmlMetaCard(html, "Source Environment", _sourceEnvironmentName);
            AppendHtmlMetaCard(html, "Target Environment", _targetEnvironmentName);
            AppendHtmlMetaCard(html, "Source Website", _sourceWebsiteName);
            AppendHtmlMetaCard(html, "Target Website", _targetWebsiteName);
            AppendHtmlMetaCard(html, "Exported At", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AppendHtmlMetaCard(html, "Filters", GetActiveFilterSummary());
            html.AppendLine("</div>");
            html.AppendLine("</section>");

            html.AppendLine("<section class=\"panel\">");
            html.AppendLine("<div class=\"section-title\">Summary</div>");
            html.AppendLine("<div class=\"card-grid\">");
            AppendHtmlMetricCard(html, "Compared Settings", total.ToString(), "Full comparison result count");
            AppendHtmlMetricCard(
                html,
                "Exported Rows",
                exportedTotal.ToString(),
                (exportedDifferent + exportedMissingInSource + exportedMissingInTarget + exportedDuplicates) +
                " findings and " + exportedMatches + " matches after current filters");
            AppendHtmlMetricCard(html, "Findings", (different + missingInSource + missingInTarget + duplicates).ToString(), "Rows requiring review");
            AppendHtmlMetricCard(html, "Differences", different.ToString(), "Changed values across source and target");
            AppendHtmlMetricCard(html, "Matches", matches.ToString(), "Exact value matches");
            AppendHtmlMetricCard(html, "Missing in Source", missingInSource.ToString(), "Exists only in target");
            AppendHtmlMetricCard(html, "Missing in Target", missingInTarget.ToString(), "Exists only in source");
            AppendHtmlMetricCard(html, "Duplicates", duplicates.ToString(), "Duplicate setting names requiring cleanup");
            html.AppendLine("</div>");
            html.AppendLine("</section>");

            if (HasActiveFilters())
            {
                html.AppendLine("<section class=\"filter-note\">");
                html.AppendLine("This report table reflects the active grid filters. Full comparison counts and decision summary are still based on the complete loaded comparison.");
                html.AppendLine("</section>");
            }

            string tableTitle = HasActiveFilters()
                ? "Filtered Comparison Results"
                : "Comparison Results";

            html.AppendLine("<section class=\"panel\">");
            html.AppendLine("<div class=\"section-title\">" + HtmlEncode(tableTitle) + "</div>");
            html.AppendLine("<div class=\"table-wrap\">");
            html.AppendLine("<table class=\"comparison-table\">");
            html.AppendLine("<colgroup>");
            html.AppendLine("<col style=\"width:130px\" />");
            html.AppendLine("<col style=\"width:300px\" />");
            html.AppendLine("<col style=\"width:180px\" />");
            html.AppendLine("<col style=\"width:460px\" />");
            html.AppendLine("<col style=\"width:380px\" />");
            html.AppendLine("<col style=\"width:380px\" />");
            html.AppendLine("<col style=\"width:260px\" />");
            html.AppendLine("<col style=\"width:420px\" />");
            html.AppendLine("<col style=\"width:420px\" />");
            html.AppendLine("</colgroup>");
            html.AppendLine("<thead><tr><th>Status</th><th>Duplicate Classification</th><th>Category</th><th>Setting Name</th><th>Source Value</th><th>Target Value</th><th>Review Focus</th><th>Environment-Specific Note</th><th>Recommended Action</th></tr></thead>");
            html.AppendLine("<tbody>");

            foreach (ComparisonResultModel result in exportedResults)
            {
                string rowClass = GetHtmlStatusCssClass(result?.Status);

                html.AppendLine("<tr class=\"" + rowClass + "\">");
                html.AppendLine("<td><span class=\"status-pill " + rowClass + "\">" + HtmlEncode(result?.Status) + "</span></td>");
                html.AppendLine("<td>" + HtmlEncode(result?.DuplicateClassification) + "</td>");
                html.AppendLine("<td>" + HtmlEncode(result?.Category) + "</td>");
                html.AppendLine("<td>" + HtmlEncode(result?.SettingName) + "</td>");
                html.AppendLine("<td class=\"value-cell\">" + HtmlValue(result?.SourceValue) + "</td>");
                html.AppendLine("<td class=\"value-cell\">" + HtmlValue(result?.TargetValue) + "</td>");
                html.AppendLine("<td class=\"recommendation-cell\">" + HtmlEncode(result?.ReviewFocus) + "</td>");
                html.AppendLine("<td class=\"recommendation-cell\">" + HtmlEncode(result?.EnvironmentSpecificReason) + "</td>");
                html.AppendLine("<td class=\"recommendation-cell\">" + HtmlEncode(result?.RecommendedAction) + "</td>");
                html.AppendLine("</tr>");
            }

            if (exportedResults.Count == 0)
            {
                html.AppendLine("<tr><td colspan=\"9\" class=\"empty-row\">No comparison results were available for this export.</td></tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");
            html.AppendLine("</section>");

            AppendHtmlFooter(html);
            html.AppendLine("</div>");
            AppendHtmlDocumentEnd(html);

            return html.ToString();
        }

        private static void AppendHtmlDocumentStart(StringBuilder html, string title)
        {
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"utf-8\" />");
            html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            html.AppendLine("<title>" + HtmlEncode(title) + "</title>");
            html.AppendLine("<style>");
            html.AppendLine(":root{--bg:#f5f7fb;--panel:#ffffff;--ink:#17202a;--muted:#5f6b7a;--line:#d9e2ec;--navy:#132333;--navy2:#1d3348;--blue:#d9eaf7;--warn:#fff2cc;--warnText:#7a4a00;--danger:#fce4d6;--dangerText:#9c0006;--ok:#e2f0d9;--okText:#27632a;--neutral:#edf2f7;}");
            html.AppendLine("*{box-sizing:border-box;}");
            html.AppendLine("body{margin:0;background:var(--bg);color:var(--ink);font-family:'Segoe UI',Arial,sans-serif;font-size:14px;line-height:1.45;}");
            html.AppendLine(".shell{max-width:1640px;margin:32px auto;padding:0 24px 36px;}");
            html.AppendLine(".hero{display:flex;align-items:flex-start;justify-content:space-between;gap:24px;background:linear-gradient(135deg,var(--navy),var(--navy2));color:#fff;border-radius:18px;padding:30px 34px;box-shadow:0 16px 38px rgba(15,31,47,.18);}");
            html.AppendLine(".eyebrow{font-size:12px;letter-spacing:.16em;font-weight:700;color:#b8d3ea;text-transform:uppercase;margin-bottom:10px;}");
            html.AppendLine("h1{font-size:30px;line-height:1.1;margin:0 0 8px;}");
            html.AppendLine("h2{font-size:22px;margin:0 0 10px;}");
            html.AppendLine(".hero p,.decision-panel p{margin:0 0 10px;color:inherit;}");
            html.AppendLine(".hero-badge{white-space:nowrap;border-radius:999px;padding:10px 16px;font-weight:700;background:rgba(255,255,255,.12);border:1px solid rgba(255,255,255,.3);}");
            html.AppendLine(".hero-badge.success{background:#e2f0d9;color:#27632a;border-color:#b7d7a8;}");
            html.AppendLine(".hero-badge.warn{background:#fff2cc;color:#7a4a00;border-color:#f1d780;}");
            html.AppendLine(".hero-badge.danger{background:#fce4d6;color:#9c0006;border-color:#f4b183;}");
            html.AppendLine(".hero-badge.neutral{background:#edf2f7;color:#263746;border-color:#d9e2ec;}");
            html.AppendLine(".decision-panel,.panel,.filter-note,.sensitive-note{background:var(--panel);border:1px solid var(--line);border-radius:16px;margin-top:20px;padding:22px 24px;box-shadow:0 8px 22px rgba(15,31,47,.06);}");
            html.AppendLine(".decision-panel.success{border-left:6px solid #70ad47;}");
            html.AppendLine(".decision-panel.warn{border-left:6px solid #f1c232;}");
            html.AppendLine(".decision-panel.danger{border-left:6px solid #c00000;}");
            html.AppendLine(".decision-kicker,.section-title{font-size:12px;letter-spacing:.12em;font-weight:800;color:var(--muted);text-transform:uppercase;margin-bottom:12px;}");
            html.AppendLine(".meta-grid,.card-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(210px,1fr));gap:12px;}");
            html.AppendLine(".meta-card,.metric-card{border:1px solid var(--line);border-radius:12px;background:#fbfdff;padding:14px 16px;min-height:82px;}");
            html.AppendLine(".meta-card .label,.metric-card .label{font-size:12px;color:var(--muted);margin-bottom:6px;}");
            html.AppendLine(".meta-card .value{font-weight:700;word-break:normal;overflow-wrap:anywhere;}");
            html.AppendLine(".metric-card .number{font-size:24px;font-weight:800;margin-bottom:4px;}");
            html.AppendLine(".metric-card .hint{font-size:12px;color:var(--muted);}");
            html.AppendLine(".filter-note{background:#fffdf2;color:#5c4813;border-left:6px solid #f1c232;}");
            html.AppendLine(".sensitive-note{background:#fffaf0;color:#5f370e;border-left:6px solid #f59e0b;}");
            html.AppendLine(".sensitive-note .section-title{color:#7c2d12;}");
            html.AppendLine(".sensitive-note p{margin:0 0 8px;}");
            html.AppendLine(".sensitive-note p:last-child{margin-bottom:0;}");
            html.AppendLine(".table-wrap{overflow-x:auto;overflow-y:visible;border:1px solid var(--line);border-radius:12px;background:#fff;}");
            html.AppendLine("table{width:100%;border-collapse:collapse;background:#fff;}");
            html.AppendLine("table.inventory-table{min-width:1320px;table-layout:fixed;}");
            html.AppendLine("table.comparison-table{min-width:2930px;table-layout:fixed;}");
            html.AppendLine("th{position:sticky;top:0;background:#d9eaf7;color:#17202a;text-align:left;font-weight:800;border-bottom:1px solid var(--line);padding:10px 12px;z-index:2;}");
            html.AppendLine("td{vertical-align:top;border-bottom:1px solid #e6edf3;padding:9px 12px;word-break:normal;overflow-wrap:break-word;}");
            html.AppendLine("tr.row-different td{background:#fff2cc;}");
            html.AppendLine("tr.row-missing td{background:#fce4d6;font-weight:700;}");
            html.AppendLine("tr.row-duplicate td{background:#f8d7da;font-weight:700;}");
            html.AppendLine("tr.row-match td{background:#e2f0d9;}");
            html.AppendLine(".value-cell{white-space:pre-wrap;word-break:normal;overflow-wrap:anywhere;max-width:none;}");
            html.AppendLine(".recommendation-cell{white-space:normal;word-break:normal;overflow-wrap:break-word;max-width:none;}");
            html.AppendLine(".status-pill{display:inline-block;border-radius:999px;padding:4px 9px;font-size:12px;font-weight:800;line-height:1.25;}");
            html.AppendLine(".status-pill.row-different{background:#ffe699;color:#7a4a00;}");
            html.AppendLine(".status-pill.row-missing{background:#f4b183;color:#9c0006;}");
            html.AppendLine(".status-pill.row-duplicate{background:#dc3545;color:#ffffff;}");
            html.AppendLine(".status-pill.row-match{background:#c6e0b4;color:#27632a;}");
            html.AppendLine(".status-pill.row-default{background:#edf2f7;color:#263746;}");
            html.AppendLine(".empty-row{text-align:center;color:var(--muted);padding:28px;}");
            html.AppendLine(".footer{margin:22px 4px 0;color:var(--muted);font-size:12px;}");
            html.AppendLine("@media print{body{background:#fff;font-size:11px;}.shell{margin:0;max-width:none;padding:0;}.hero,.panel,.decision-panel,.filter-note,.sensitive-note{box-shadow:none;border-radius:0;}.hero{background:#132333;color:#fff;}.table-wrap{overflow:visible;border-radius:0;}table.inventory-table,table.comparison-table{min-width:0;width:100%;table-layout:auto;}th{position:static;}td,th{padding:6px 7px;}.status-pill{padding:2px 5px;font-size:10px;}}");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
        }

        private static void AppendHtmlDocumentEnd(StringBuilder html)
        {
            html.AppendLine("</body>");
            html.AppendLine("</html>");
        }

        private static void AppendHtmlMetaCard(StringBuilder html, string label, string value)
        {
            html.AppendLine("<div class=\"meta-card\">");
            html.AppendLine("<div class=\"label\">" + HtmlEncode(label) + "</div>");
            html.AppendLine("<div class=\"value\">" + HtmlEncode(value) + "</div>");
            html.AppendLine("</div>");
        }

        private static void AppendHtmlMetricCard(StringBuilder html, string label, string number, string hint)
        {
            html.AppendLine("<div class=\"metric-card\">");
            html.AppendLine("<div class=\"label\">" + HtmlEncode(label) + "</div>");
            html.AppendLine("<div class=\"number\">" + HtmlEncode(number) + "</div>");
            html.AppendLine("<div class=\"hint\">" + HtmlEncode(hint) + "</div>");
            html.AppendLine("</div>");
        }

        private static void AppendSensitiveInformationNotice(StringBuilder html)
        {
            html.AppendLine("<section class=\"sensitive-note\">");
            html.AppendLine("<div class=\"section-title\">Sensitive information notice</div>");
            html.AppendLine("<p>This report may show sensitive or environment-specific configuration information, including Dataverse environment names, Power Pages URLs, authentication settings, identity provider metadata, client identifiers, certificate thumbprints, tokens, connection strings, and other protected configuration values.</p>");
            html.AppendLine("<p><strong>Recommended handling:</strong> Review the report before sharing, remove values that are not required for the audience, store it only in approved locations, and avoid sending it through unsecured channels.</p>");
            html.AppendLine("</section>");
        }

        private static void AppendHtmlFooter(StringBuilder html)
        {
            html.AppendLine("<div class=\"footer\">Generated by Power Pages ALM Drift Inspector. Review exported configuration values before sharing outside approved project channels.</div>");
        }

        private string GetActiveFilterSummary()
        {
            List<string> filters = new List<string>();

            if (txtFilterSettings != null && !string.IsNullOrWhiteSpace(txtFilterSettings.Text))
            {
                filters.Add("Text contains '" + txtFilterSettings.Text.Trim() + "'");
            }

            string selectedStatus = GetSelectedStatusFilter();
            if (!string.Equals(selectedStatus, AllStatusesText, StringComparison.OrdinalIgnoreCase))
            {
                filters.Add("Status = " + selectedStatus);
            }

            string selectedCategory = GetSelectedCategoryFilter();
            if (!string.Equals(selectedCategory, AllCategoriesText, StringComparison.OrdinalIgnoreCase))
            {
                filters.Add("Category = " + selectedCategory);
            }

            if (_showFindingsOnly)
            {
                filters.Add("Findings only");
            }

            return filters.Count == 0
                ? "No filters applied"
                : string.Join("; ", filters.ToArray());
        }

        private static void GetComparisonSummaryCountsForResults(
            List<ComparisonResultModel> results,
            out int total,
            out int different,
            out int matches,
            out int missingInSource,
            out int missingInTarget,
            out int duplicates)
        {
            total = results?.Count ?? 0;
            different = 0;
            matches = 0;
            missingInSource = 0;
            missingInTarget = 0;
            duplicates = 0;

            foreach (ComparisonResultModel result in results ?? new List<ComparisonResultModel>())
            {
                if (result == null)
                {
                    continue;
                }

                if (string.Equals(result.Status, StatusDifferentValue, StringComparison.OrdinalIgnoreCase))
                {
                    different++;
                }
                else if (string.Equals(result.Status, StatusMatch, StringComparison.OrdinalIgnoreCase))
                {
                    matches++;
                }
                else if (string.Equals(result.Status, StatusMissingInSource, StringComparison.OrdinalIgnoreCase))
                {
                    missingInSource++;
                }
                else if (string.Equals(result.Status, StatusMissingInTarget, StringComparison.OrdinalIgnoreCase))
                {
                    missingInTarget++;
                }
                else if (string.Equals(result.Status, StatusDuplicate, StringComparison.OrdinalIgnoreCase))
                {
                    duplicates++;
                }
            }
        }

        private static void GetComparisonDecisionInfo(
            List<ComparisonResultModel> results,
            out string decisionLabel,
            out string decisionClass,
            out string decisionSummary,
            out string recommendedNextStep)
        {
            GetComparisonSummaryCountsForResults(
                results,
                out int total,
                out int different,
                out int matches,
                out int missingInSource,
                out int missingInTarget,
                out int duplicates);

            if (total == 0)
            {
                decisionLabel = "Not Compared";
                decisionClass = "neutral";
                decisionSummary = "No comparison results were available when this report was generated.";
                recommendedNextStep = "Select Source and Target environments, run Load and Compare, then generate the report again.";
                return;
            }

            if (missingInSource > 0 || missingInTarget > 0 || duplicates > 0)
            {
                decisionLabel = "Potential Deployment Risk";
                decisionClass = "danger";
                decisionSummary = "One or more Power Pages site settings are missing or duplicated in the compared environments.";
                recommendedNextStep = "Review missing settings and resolve duplicate records before deployment, then run the comparison again.";
                return;
            }

            if (different > 0)
            {
                decisionLabel = "Review Required";
                decisionClass = "warn";
                decisionSummary = "The compared websites contain site settings with different values.";
                recommendedNextStep = "Review changed values with the release or solution owner before promoting the site configuration.";
                return;
            }

            decisionLabel = "No Drift Detected";
            decisionClass = "success";
            decisionSummary = "All compared Power Pages site settings matched between the source and target environments.";
            recommendedNextStep = "Attach this report to the release evidence package or deployment validation record.";
        }

        private static string GetHtmlStatusCssClass(string status)
        {
            if (string.Equals(status, StatusDifferentValue, StringComparison.OrdinalIgnoreCase))
            {
                return "row-different";
            }

            if (string.Equals(status, StatusMissingInSource, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, StatusMissingInTarget, StringComparison.OrdinalIgnoreCase))
            {
                return "row-missing";
            }

            if (string.Equals(status, StatusDuplicate, StringComparison.OrdinalIgnoreCase))
            {
                return "row-duplicate";
            }

            if (string.Equals(status, StatusMatch, StringComparison.OrdinalIgnoreCase))
            {
                return "row-match";
            }

            return "row-default";
        }

        private static string HtmlEncode(string value)
        {
            return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string HtmlValue(string value)
        {
            string encodedValue = HtmlEncode(value);

            return encodedValue
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />")
                .Replace("\r", "<br />");
        }

        private void ExportSiteSettingsToExcelWorkbook(List<SiteSettingModel> settings, string websiteName, string filePath)
        {
            settings = settings ?? new List<SiteSettingModel>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Spreadsheet.Workbook();

                WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateExcelStylesheet();
                stylesPart.Stylesheet.Save();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = CreateSiteSettingsWorksheet(settings, websiteName);
                worksheetPart.Worksheet.Save();

                Spreadsheet.Sheets sheets = workbookPart.Workbook.AppendChild(new Spreadsheet.Sheets());
                sheets.Append(new Spreadsheet.Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1U,
                    Name = GetSafeWorksheetName("Site Settings")
                });

                workbookPart.Workbook.Save();
            }

            LogMessage("Exported " + settings.Count + " site setting(s) to Excel workbook.");
        }

        private void ExportComparisonResultsToExcelWorkbook(List<ComparisonResultModel> results, string filePath)
        {
            results = results ?? new List<ComparisonResultModel>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Spreadsheet.Workbook();

                WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateExcelStylesheet();
                stylesPart.Stylesheet.Save();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = CreateComparisonWorksheet(results);
                worksheetPart.Worksheet.Save();

                Spreadsheet.Sheets sheets = workbookPart.Workbook.AppendChild(new Spreadsheet.Sheets());
                sheets.Append(new Spreadsheet.Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1U,
                    Name = GetSafeWorksheetName("ALM Drift Comparison")
                });

                workbookPart.Workbook.Save();
            }

            LogMessage("Exported " + results.Count + " comparison result(s) to Excel workbook.");
        }

        private Spreadsheet.Worksheet CreateSiteSettingsWorksheet(List<SiteSettingModel> settings, string websiteName)
        {
            Spreadsheet.Worksheet worksheet = new Spreadsheet.Worksheet();

            worksheet.Append(CreateFrozenSheetViews(8U));
            worksheet.Append(new Spreadsheet.SheetFormatProperties { DefaultRowHeight = 15D });
            worksheet.Append(CreateColumns(26D, 70D, 95D, 26D));

            Spreadsheet.SheetData sheetData = new Spreadsheet.SheetData();
            uint rowIndex = 1U;

            AppendMetadataRow(sheetData, ref rowIndex, "Report Type", "Power Pages Site Settings Export");
            AppendMetadataRow(sheetData, ref rowIndex, "Environment", GetCurrentBottomGridEnvironmentName());
            AppendMetadataRow(sheetData, ref rowIndex, "Website", websiteName);
            AppendMetadataRow(sheetData, ref rowIndex, "Exported At", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AppendMetadataRow(sheetData, ref rowIndex, "Visible Settings", settings.Count.ToString());
            AppendMetadataRow(sheetData, ref rowIndex, "Total Settings", (_allSiteSettings?.Count ?? 0).ToString());
            AppendBlankExcelRow(sheetData, ref rowIndex);

            uint headerRowIndex = rowIndex;
            AppendExcelTextRow(sheetData, ref rowIndex, ExcelStyleHeader, "Website Name", "Setting Name", "Setting Value", "Category");

            foreach (SiteSettingModel setting in settings)
            {
                AppendExcelTextRow(
                    sheetData,
                    ref rowIndex,
                    ExcelStyleText,
                    websiteName,
                    setting?.Name,
                    setting?.Value,
                    setting?.Category);
            }

            worksheet.Append(sheetData);
            AppendAutoFilter(worksheet, headerRowIndex, Math.Max(headerRowIndex, rowIndex - 1U), 4);

            return worksheet;
        }

        private Spreadsheet.Worksheet CreateComparisonWorksheet(List<ComparisonResultModel> results)
        {
            Spreadsheet.Worksheet worksheet = new Spreadsheet.Worksheet();

            worksheet.Append(CreateFrozenSheetViews(15U));
            worksheet.Append(new Spreadsheet.SheetFormatProperties { DefaultRowHeight = 15D });
            worksheet.Append(CreateColumns(22D, 44D, 24D, 72D, 80D, 80D, 34D, 52D, 58D));

            Spreadsheet.SheetData sheetData = new Spreadsheet.SheetData();
            uint rowIndex = 1U;

            GetComparisonSummaryCounts(
                out int total,
                out int different,
                out int matches,
                out int missingInSource,
                out int missingInTarget,
                out int duplicates);

            AppendMetadataRow(sheetData, ref rowIndex, "Report Type", "Power Pages ALM Drift Comparison");
            AppendMetadataRow(sheetData, ref rowIndex, "Source Environment", _sourceEnvironmentName);
            AppendMetadataRow(sheetData, ref rowIndex, "Target Environment", _targetEnvironmentName);
            AppendMetadataRow(sheetData, ref rowIndex, "Source Website", _sourceWebsiteName);
            AppendMetadataRow(sheetData, ref rowIndex, "Target Website", _targetWebsiteName);
            AppendMetadataRow(sheetData, ref rowIndex, "Exported At", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AppendMetadataRow(sheetData, ref rowIndex, "Filtered Results", results.Count.ToString());
            AppendMetadataRow(sheetData, ref rowIndex, "Total Results", total.ToString());
            AppendMetadataRow(sheetData, ref rowIndex, "Differences", different.ToString());
            AppendMetadataRow(sheetData, ref rowIndex, "Matches", matches.ToString());
            AppendMetadataRow(sheetData, ref rowIndex, "Missing in Source", missingInSource.ToString());
            AppendMetadataRow(sheetData, ref rowIndex, "Missing in Target", missingInTarget.ToString());
            AppendMetadataRow(sheetData, ref rowIndex, "Duplicates", duplicates.ToString());
            AppendMetadataRow(sheetData, ref rowIndex, "Filters", GetActiveFilterSummary());
            AppendBlankExcelRow(sheetData, ref rowIndex);

            uint headerRowIndex = rowIndex;
            AppendExcelTextRow(sheetData, ref rowIndex, ExcelStyleHeader, "Status", "Duplicate Classification", "Category", "Setting Name", "Source Value", "Target Value", "Review Focus", "Environment-Specific Note", "Recommended Action");

            foreach (ComparisonResultModel result in results)
            {
                AppendExcelTextRow(
                    sheetData,
                    ref rowIndex,
                    GetExcelComparisonStyleIndex(result?.Status),
                    result?.Status,
                    result?.DuplicateClassification,
                    result?.Category,
                    result?.SettingName,
                    result?.SourceValue,
                    result?.TargetValue,
                    result?.ReviewFocus,
                    result?.EnvironmentSpecificReason,
                    result?.RecommendedAction);
            }

            worksheet.Append(sheetData);
            AppendAutoFilter(worksheet, headerRowIndex, Math.Max(headerRowIndex, rowIndex - 1U), 9);

            return worksheet;
        }

        private static Spreadsheet.SheetViews CreateFrozenSheetViews(uint freezeAfterRow)
        {
            Spreadsheet.SheetViews sheetViews = new Spreadsheet.SheetViews();
            Spreadsheet.SheetView sheetView = new Spreadsheet.SheetView { WorkbookViewId = 0U };

            if (freezeAfterRow > 0U)
            {
                string firstScrollableCell = "A" + (freezeAfterRow + 1U).ToString();

                sheetView.Append(new Spreadsheet.Pane
                {
                    VerticalSplit = (double)freezeAfterRow,
                    TopLeftCell = firstScrollableCell,
                    ActivePane = Spreadsheet.PaneValues.BottomLeft,
                    State = Spreadsheet.PaneStateValues.Frozen
                });
            }

            sheetViews.Append(sheetView);
            return sheetViews;
        }

        private static Spreadsheet.Columns CreateColumns(params double[] widths)
        {
            Spreadsheet.Columns columns = new Spreadsheet.Columns();

            for (int index = 0; index < (widths?.Length ?? 0); index++)
            {
                uint columnIndex = (uint)(index + 1);

                columns.Append(new Spreadsheet.Column
                {
                    Min = columnIndex,
                    Max = columnIndex,
                    Width = widths[index],
                    CustomWidth = true
                });
            }

            return columns;
        }

        private static void AppendMetadataRow(Spreadsheet.SheetData sheetData, ref uint rowIndex, string label, string value)
        {
            Spreadsheet.Row row = new Spreadsheet.Row { RowIndex = rowIndex };
            row.Append(CreateTextCell(1, rowIndex, label, ExcelStyleMetaLabel));
            row.Append(CreateTextCell(2, rowIndex, value, ExcelStyleText));
            sheetData.Append(row);
            rowIndex++;
        }

        private static void AppendBlankExcelRow(Spreadsheet.SheetData sheetData, ref uint rowIndex)
        {
            sheetData.Append(new Spreadsheet.Row { RowIndex = rowIndex });
            rowIndex++;
        }

        private static void AppendExcelTextRow(Spreadsheet.SheetData sheetData, ref uint rowIndex, uint styleIndex, params string[] values)
        {
            Spreadsheet.Row row = new Spreadsheet.Row { RowIndex = rowIndex };

            for (int index = 0; index < (values?.Length ?? 0); index++)
            {
                row.Append(CreateTextCell(index + 1, rowIndex, values[index], styleIndex));
            }

            sheetData.Append(row);
            rowIndex++;
        }

        private static Spreadsheet.Cell CreateTextCell(int columnIndex, uint rowIndex, string value, uint styleIndex)
        {
            return new Spreadsheet.Cell
            {
                CellReference = GetExcelColumnName(columnIndex) + rowIndex.ToString(),
                DataType = Spreadsheet.CellValues.InlineString,
                StyleIndex = styleIndex,
                InlineString = new Spreadsheet.InlineString(
                    new Spreadsheet.Text(NormalizeExcelText(value))
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    })
            };
        }

        private static void AppendAutoFilter(Spreadsheet.Worksheet worksheet, uint headerRowIndex, uint lastRowIndex, int columnCount)
        {
            if (worksheet == null || columnCount <= 0 || lastRowIndex < headerRowIndex)
            {
                return;
            }

            string reference = "A" + headerRowIndex.ToString() + ":" + GetExcelColumnName(columnCount) + lastRowIndex.ToString();
            worksheet.Append(new Spreadsheet.AutoFilter { Reference = reference });
        }

        private static Spreadsheet.Stylesheet CreateExcelStylesheet()
        {
            Spreadsheet.Fonts fonts = new Spreadsheet.Fonts { Count = 4U, KnownFonts = true };
            fonts.Append(CreateFont(false, null));
            fonts.Append(CreateFont(true, null));
            fonts.Append(CreateFont(true, "9C5700"));
            fonts.Append(CreateFont(true, "9C0006"));

            Spreadsheet.Fills fills = new Spreadsheet.Fills { Count = 6U };
            fills.Append(new Spreadsheet.Fill(new Spreadsheet.PatternFill { PatternType = Spreadsheet.PatternValues.None }));
            fills.Append(new Spreadsheet.Fill(new Spreadsheet.PatternFill { PatternType = Spreadsheet.PatternValues.Gray125 }));
            fills.Append(CreateSolidFill("D9EAF7"));
            fills.Append(CreateSolidFill("FFF2CC"));
            fills.Append(CreateSolidFill("FCE4D6"));
            fills.Append(CreateSolidFill("E2F0D9"));

            Spreadsheet.Borders borders = new Spreadsheet.Borders { Count = 2U };
            borders.Append(new Spreadsheet.Border());
            borders.Append(CreateThinBorder());

            Spreadsheet.CellStyleFormats cellStyleFormats = new Spreadsheet.CellStyleFormats { Count = 1U };
            cellStyleFormats.Append(new Spreadsheet.CellFormat { FontId = 0U, FillId = 0U, BorderId = 0U });

            Spreadsheet.CellFormats cellFormats = new Spreadsheet.CellFormats { Count = 7U };
            cellFormats.Append(new Spreadsheet.CellFormat { FontId = 0U, FillId = 0U, BorderId = 0U });
            cellFormats.Append(CreateCellFormat(1U, 0U, 0U));
            cellFormats.Append(CreateCellFormat(1U, 2U, 1U));
            cellFormats.Append(CreateCellFormat(0U, 0U, 0U));
            cellFormats.Append(CreateCellFormat(0U, 3U, 1U));
            cellFormats.Append(CreateCellFormat(3U, 4U, 1U));
            cellFormats.Append(CreateCellFormat(0U, 5U, 1U));

            Spreadsheet.CellStyles cellStyles = new Spreadsheet.CellStyles { Count = 1U };
            cellStyles.Append(new Spreadsheet.CellStyle { Name = "Normal", FormatId = 0U, BuiltinId = 0U });

            return new Spreadsheet.Stylesheet(fonts, fills, borders, cellStyleFormats, cellFormats, cellStyles);
        }

        private static Spreadsheet.Font CreateFont(bool bold, string rgb)
        {
            Spreadsheet.Font font = new Spreadsheet.Font();

            if (bold)
            {
                font.Append(new Spreadsheet.Bold());
            }

            if (!string.IsNullOrWhiteSpace(rgb))
            {
                font.Append(new Spreadsheet.Color { Rgb = rgb });
            }

            font.Append(new Spreadsheet.FontName { Val = "Calibri" });
            font.Append(new Spreadsheet.FontSize { Val = 11D });
            return font;
        }

        private static Spreadsheet.Fill CreateSolidFill(string rgb)
        {
            return new Spreadsheet.Fill(
                new Spreadsheet.PatternFill(
                    new Spreadsheet.ForegroundColor { Rgb = rgb },
                    new Spreadsheet.BackgroundColor { Indexed = 64U })
                {
                    PatternType = Spreadsheet.PatternValues.Solid
                });
        }

        private static Spreadsheet.Border CreateThinBorder()
        {
            return new Spreadsheet.Border(
                new Spreadsheet.LeftBorder { Style = Spreadsheet.BorderStyleValues.Thin, Color = new Spreadsheet.Color { Auto = true } },
                new Spreadsheet.RightBorder { Style = Spreadsheet.BorderStyleValues.Thin, Color = new Spreadsheet.Color { Auto = true } },
                new Spreadsheet.TopBorder { Style = Spreadsheet.BorderStyleValues.Thin, Color = new Spreadsheet.Color { Auto = true } },
                new Spreadsheet.BottomBorder { Style = Spreadsheet.BorderStyleValues.Thin, Color = new Spreadsheet.Color { Auto = true } },
                new Spreadsheet.DiagonalBorder());
        }

        private static Spreadsheet.CellFormat CreateCellFormat(uint fontId, uint fillId, uint borderId)
        {
            return new Spreadsheet.CellFormat
            {
                FontId = fontId,
                FillId = fillId,
                BorderId = borderId,
                ApplyFont = true,
                ApplyFill = fillId > 0U,
                ApplyBorder = borderId > 0U,
                NumberFormatId = 49U,
                ApplyNumberFormat = true,
                Alignment = new Spreadsheet.Alignment
                {
                    Vertical = Spreadsheet.VerticalAlignmentValues.Top,
                    WrapText = true
                }
            };
        }

        private static uint GetExcelComparisonStyleIndex(string status)
        {
            if (string.Equals(status, StatusDifferentValue, StringComparison.OrdinalIgnoreCase))
            {
                return ExcelStyleDifferent;
            }

            if (string.Equals(status, StatusMissingInSource, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, StatusMissingInTarget, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, StatusDuplicate, StringComparison.OrdinalIgnoreCase))
            {
                return ExcelStyleMissing;
            }

            if (string.Equals(status, StatusMatch, StringComparison.OrdinalIgnoreCase))
            {
                return ExcelStyleMatch;
            }

            return ExcelStyleText;
        }

        private static string GetExcelColumnName(int columnIndex)
        {
            if (columnIndex <= 0)
            {
                return "A";
            }

            string columnName = string.Empty;

            while (columnIndex > 0)
            {
                int modulo = (columnIndex - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnIndex = (columnIndex - modulo) / 26;
            }

            return columnName;
        }

        private static string GetSafeWorksheetName(string value)
        {
            string safeValue = string.IsNullOrWhiteSpace(value) ? "Sheet1" : value.Trim();
            char[] invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };

            foreach (char invalidChar in invalidChars)
            {
                safeValue = safeValue.Replace(invalidChar, '_');
            }

            if (safeValue.Length > 31)
            {
                safeValue = safeValue.Substring(0, 31);
            }

            return safeValue;
        }

        private static string NormalizeExcelText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);

            foreach (char character in value)
            {
                if (character == '\t' || character == '\n' || character == '\r' || character >= ' ')
                {
                    builder.Append(character);
                }
            }

            string normalizedValue = builder.ToString();

            if (normalizedValue.Length > 32760)
            {
                normalizedValue = normalizedValue.Substring(0, 32760) + "...";
            }

            return normalizedValue;
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;

            safeValue = safeValue.Replace("\"", "\"\"");

            bool mustQuote =
                safeValue.Contains(";") ||
                safeValue.Contains("\"") ||
                safeValue.Contains("\r") ||
                safeValue.Contains("\n");

            return mustQuote
                ? "\"" + safeValue + "\""
                : safeValue;
        }

        private static string GetSafeFileName(string value)
        {
            string safeValue = string.IsNullOrWhiteSpace(value) ? "Website" : value;

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                safeValue = safeValue.Replace(invalidChar, '_');
            }

            return safeValue;
        }

        private void txtFilterSettings_TextChanged(object sender, EventArgs e)
        {
            ApplyBottomGridFilter();
            UpdateActionStates();
        }

        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            bool hadTextFilter = txtFilterSettings != null && !string.IsNullOrWhiteSpace(txtFilterSettings.Text);
            bool hadStatusFilter = !string.Equals(GetSelectedStatusFilter(), AllStatusesText, StringComparison.OrdinalIgnoreCase);
            bool hadCategoryFilter = !string.Equals(GetSelectedCategoryFilter(), AllCategoriesText, StringComparison.OrdinalIgnoreCase);
            bool hadFindingsOnlyFilter = _showFindingsOnly;

            if (hadTextFilter && txtFilterSettings != null)
            {
                txtFilterSettings.Text = string.Empty;
            }

            if (hadStatusFilter && cboStatusFilter != null)
            {
                cboStatusFilter.SelectedIndex = 0;
            }

            if (hadCategoryFilter && cboCategoryFilter != null)
            {
                cboCategoryFilter.SelectedIndex = 0;
            }

            if (hadFindingsOnlyFilter)
            {
                _showFindingsOnly = false;

                if (chkFindingsOnly != null)
                {
                    chkFindingsOnly.Checked = false;
                }
            }

            ApplyBottomGridFilter();

            if (!hadTextFilter && !hadStatusFilter && !hadCategoryFilter && !hadFindingsOnlyFilter)
            {
                LogMessage("Clear requested, but no active filter was set.");
            }
            else
            {
                LogMessage("Filters cleared.");
            }
        }

        private void btnExportHtml_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isComparisonView)
                {
                    List<ComparisonResultModel> filteredResults = GetFilteredComparisonResults();

                    if (filteredResults.Count == 0)
                    {
                        LogMessage("HTML export canceled. No comparison results are available.");

                        MessageBox.Show(
                            "There are no comparison results to export.",
                            "Nothing to Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    string sourceName = GetSafeFileName(string.IsNullOrWhiteSpace(_sourceWebsiteName) ? "Source" : _sourceWebsiteName);
                    string targetName = GetSafeFileName(string.IsNullOrWhiteSpace(_targetWebsiteName) ? "Target" : _targetWebsiteName);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string defaultFileName = sourceName + "_vs_" + targetName + "_" + timestamp + ".html";

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Title = "Export Comparison Results to HTML";
                        saveDialog.Filter = "HTML Report (*.html)|*.html|All files (*.*)|*.*";
                        saveDialog.FileName = defaultFileName;
                        saveDialog.DefaultExt = "html";
                        saveDialog.AddExtension = true;
                        saveDialog.OverwritePrompt = true;

                        DialogResult result = saveDialog.ShowDialog();

                        if (result != DialogResult.OK || string.IsNullOrWhiteSpace(saveDialog.FileName))
                        {
                            LogMessage("HTML export canceled by user.");
                            return;
                        }

                        ExportComparisonResultsToHtmlReport(filteredResults, saveDialog.FileName);

                        LogMessage("HTML export completed: " + saveDialog.FileName);
                        ShowExportCompletedMessage("Comparison HTML report export completed successfully.", saveDialog.FileName);
                    }
                }
                else
                {
                    List<SiteSettingModel> filteredSettings = GetFilteredSiteSettings();

                    if (filteredSettings.Count == 0)
                    {
                        LogMessage("HTML export canceled. No filtered site settings are available.");

                        MessageBox.Show(
                            "There are no site settings to export.",
                            "Nothing to Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    string websiteName = GetCurrentBottomGridWebsiteName();
                    string safeWebsiteName = GetSafeFileName(websiteName);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string defaultFileName = safeWebsiteName + "_SiteSettings_" + timestamp + ".html";

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Title = "Export Site Settings to HTML";
                        saveDialog.Filter = "HTML Report (*.html)|*.html|All files (*.*)|*.*";
                        saveDialog.FileName = defaultFileName;
                        saveDialog.DefaultExt = "html";
                        saveDialog.AddExtension = true;
                        saveDialog.OverwritePrompt = true;

                        DialogResult result = saveDialog.ShowDialog();

                        if (result != DialogResult.OK || string.IsNullOrWhiteSpace(saveDialog.FileName))
                        {
                            LogMessage("HTML export canceled by user.");
                            return;
                        }

                        ExportSiteSettingsToHtmlReport(filteredSettings, websiteName, saveDialog.FileName);

                        LogMessage("HTML export completed: " + saveDialog.FileName);
                        ShowExportCompletedMessage("Site settings HTML report export completed successfully.", saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error exporting HTML report: " + ex.Message);

                MessageBox.Show(
                    "HTML export failed. See log for details.\r\n\r\n" + ex.Message,
                    "Export HTML Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ShowExportCompletedMessage(string message, string filePath)
        {
            DialogResult openResult = MessageBox.Show(
                message + "\r\n\r\nOpen the report now?",
                "Export Complete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (openResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LogMessage("Unable to open exported report: " + ex.Message);

                MessageBox.Show(
                    "The report was exported, but it could not be opened automatically.\r\n\r\n" + filePath,
                    "Open Report",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isComparisonView)
                {
                    List<ComparisonResultModel> filteredResults = GetFilteredComparisonResults();

                    if (filteredResults.Count == 0)
                    {
                        LogMessage("Export canceled. No comparison results are available.");

                        MessageBox.Show(
                            "There are no comparison results to export.",
                            "Nothing to Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    string sourceName = GetSafeFileName(string.IsNullOrWhiteSpace(_sourceWebsiteName) ? "Source" : _sourceWebsiteName);
                    string targetName = GetSafeFileName(string.IsNullOrWhiteSpace(_targetWebsiteName) ? "Target" : _targetWebsiteName);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string defaultFileName = sourceName + "_vs_" + targetName + "_" + timestamp + ".xlsx";

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Title = "Export Comparison Results to Excel";
                        saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                        saveDialog.FileName = defaultFileName;
                        saveDialog.DefaultExt = "xlsx";
                        saveDialog.AddExtension = true;
                        saveDialog.OverwritePrompt = true;

                        DialogResult result = saveDialog.ShowDialog();

                        if (result != DialogResult.OK || string.IsNullOrWhiteSpace(saveDialog.FileName))
                        {
                            LogMessage("Excel export canceled by user.");
                            return;
                        }

                        ExportComparisonResultsToExcelWorkbook(filteredResults, saveDialog.FileName);

                        LogMessage("Excel export completed: " + saveDialog.FileName);

                        MessageBox.Show(
                            "Comparison Excel export completed successfully.",
                            "Export Excel",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                else
                {
                    List<SiteSettingModel> filteredSettings = GetFilteredSiteSettings();

                    if (filteredSettings.Count == 0)
                    {
                        LogMessage("Excel export canceled. No filtered site settings are available.");

                        MessageBox.Show(
                            "There are no site settings to export.",
                            "Nothing to Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    string websiteName = GetCurrentBottomGridWebsiteName();
                    string safeWebsiteName = GetSafeFileName(websiteName);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string defaultFileName = safeWebsiteName + "_SiteSettings_" + timestamp + ".xlsx";

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Title = "Export Site Settings to Excel";
                        saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                        saveDialog.FileName = defaultFileName;
                        saveDialog.DefaultExt = "xlsx";
                        saveDialog.AddExtension = true;
                        saveDialog.OverwritePrompt = true;

                        DialogResult result = saveDialog.ShowDialog();

                        if (result != DialogResult.OK || string.IsNullOrWhiteSpace(saveDialog.FileName))
                        {
                            LogMessage("Excel export canceled by user.");
                            return;
                        }

                        ExportSiteSettingsToExcelWorkbook(filteredSettings, websiteName, saveDialog.FileName);

                        LogMessage("Excel export completed: " + saveDialog.FileName);

                        MessageBox.Show(
                            "Site settings Excel export completed successfully.",
                            "Export Excel",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error exporting Excel workbook: " + ex.Message);

                MessageBox.Show(
                    "Excel export failed. See log for details.\r\n\r\n" + ex.Message,
                    "Export Excel Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isComparisonView)
                {
                    List<ComparisonResultModel> filteredResults = GetFilteredComparisonResults();

                    if (filteredResults.Count == 0)
                    {
                        LogMessage("Export canceled. No comparison results are available.");

                        MessageBox.Show(
                            "There are no comparison results to export.",
                            "Nothing to Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    string sourceName = GetSafeFileName(string.IsNullOrWhiteSpace(_sourceWebsiteName) ? "Source" : _sourceWebsiteName);
                    string targetName = GetSafeFileName(string.IsNullOrWhiteSpace(_targetWebsiteName) ? "Target" : _targetWebsiteName);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string defaultFileName = sourceName + "_vs_" + targetName + "_" + timestamp + ".csv";

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Title = "Export Comparison Results to CSV";
                        saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                        saveDialog.FileName = defaultFileName;
                        saveDialog.DefaultExt = "csv";
                        saveDialog.AddExtension = true;
                        saveDialog.OverwritePrompt = true;

                        DialogResult result = saveDialog.ShowDialog();

                        if (result != DialogResult.OK || string.IsNullOrWhiteSpace(saveDialog.FileName))
                        {
                            LogMessage("Export canceled by user.");
                            return;
                        }

                        ExportComparisonResultsToCsv(filteredResults, saveDialog.FileName);

                        LogMessage("Export completed: " + saveDialog.FileName);

                        MessageBox.Show(
                            "Comparison export completed successfully.",
                            "Export CSV",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                else
                {
                    List<SiteSettingModel> filteredSettings = GetFilteredSiteSettings();

                    if (filteredSettings.Count == 0)
                    {
                        LogMessage("Export canceled. No filtered site settings are available.");

                        MessageBox.Show(
                            "There are no site settings to export.",
                            "Nothing to Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    string websiteName = GetCurrentBottomGridWebsiteName();
                    string safeWebsiteName = GetSafeFileName(websiteName);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string defaultFileName = safeWebsiteName + "_SiteSettings_" + timestamp + ".csv";

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Title = "Export Site Settings to CSV";
                        saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                        saveDialog.FileName = defaultFileName;
                        saveDialog.DefaultExt = "csv";
                        saveDialog.AddExtension = true;
                        saveDialog.OverwritePrompt = true;

                        DialogResult result = saveDialog.ShowDialog();

                        if (result != DialogResult.OK || string.IsNullOrWhiteSpace(saveDialog.FileName))
                        {
                            LogMessage("Export canceled by user.");
                            return;
                        }

                        ExportSiteSettingsToCsv(filteredSettings, websiteName, saveDialog.FileName);

                        LogMessage("Export completed: " + saveDialog.FileName);

                        MessageBox.Show(
                            "Export completed successfully.",
                            "Export CSV",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error exporting CSV: " + ex.Message);

                MessageBox.Show(
                    "Export failed. See log for details.\r\n\r\n" + ex.Message,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SetWebsiteCount(int count)
        {
            if (lblWebsiteCount != null)
            {
                lblWebsiteCount.Text = "Selected Websites: " + count;
            }
        }

        private void SetSiteSettingsCount(int visibleCount, int totalCount)
        {
            if (lblSettingsCount == null)
            {
                return;
            }

            if (totalCount <= 0)
            {
                lblSettingsCount.Text = "Site Settings: 0";
            }
            else if (visibleCount == totalCount)
            {
                lblSettingsCount.Text = "Site Settings: " + visibleCount + " of " + totalCount;
            }
            else
            {
                lblSettingsCount.Text = "Site Settings: " + visibleCount + " of " + totalCount + " shown";
            }
        }

        private void SetComparisonResultsCount(int visibleCount, int totalCount)
        {
            if (lblSettingsCount == null)
            {
                return;
            }

            if (totalCount <= 0)
            {
                lblSettingsCount.Text = "Comparison Results: 0";
            }
            else if (visibleCount == totalCount)
            {
                lblSettingsCount.Text = "Comparison Results: " + visibleCount + " of " + totalCount;
            }
            else
            {
                lblSettingsCount.Text = "Comparison Results: " + visibleCount + " of " + totalCount + " shown";
            }
        }

        private void LogMessage(string message)
        {
            if (txtLog == null)
            {
                return;
            }

            txtLog.AppendText(
                "[" + DateTime.Now.ToString("HH:mm:ss") + "] " +
                message +
                Environment.NewLine);
        }
    }
}
