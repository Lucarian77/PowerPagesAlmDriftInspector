using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using PowerPagesAlmDriftInspector.Controls;
using PowerPagesAlmDriftInspector.Models;
using PowerPagesAlmDriftInspector.Services;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace PowerPagesAlmDriftInspector
{
    [Export(typeof(IXrmToolBoxPlugin))]
    [ExportMetadata("Name", "Power Pages ALM Drift Inspector")]
    [ExportMetadata("Description", "Compare Power Pages site settings across Dataverse environments to identify ALM drift.")]
    [ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAABaElEQVR4nGNgGGDAiMxRqrr2n14W32vTYmRgYGBgQrf8bqvmXWw0tQHMPkZslmMDytXXlWnhECZkDswSXDQtACMDAwODQsyq/0xyOrSyAyf49+gKIgTO1WrS1XKYfSzYBOkJmAgroQ44VM+IVZwFXSCi9xzDimIjhojecwy2msJ4Dc32kifa8tlHGBjssMjRLQRgDkEHGCGwotgIhaaGpb5htgyzjxzGKk9xFPBvMmH46HeGKMf4htkybK5nZLBrRJT4NI0CmO/xAZpHweZVhzHYh5BCgaIoqPoTzMDAwMDAv8kERRwWJchBjS0HMDDQORdgA5RFwSYaOICkXOB3BiMX2C1kYDhEiQPIAXYLMfmH4sl0ALVzASFAcSLUf09cIYQLUBwF6EFNSvAzMAyCbEh1B5Die5o4gFQw6oDB4YAHS8IYDVJX09Vig9TVDA+WhDEyoQvSy3IYwGglKsSsonkP+cGSMLi9AA3+hLXqfUG7AAAAAElFTkSuQmCC")]
    [ExportMetadata("BigImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAACWklEQVR4nO2bu0oDQRSG/5X04hMoptJGCbbJAwhJZ7AQrKwEGztFgojpbAQrq4BFiNhE8AFiq6IgpoqoLxB8AbUIo5O9zM4OOzkz43zNJpPdzDn/ucxkwwIez78mEH04v/fyPSlDdPPaXIz1NXbQJcfDhIWYCp/gsvNA1L+IAP+NMQFE0R8cLwxUx0yD91MqA5hTvHOyY6YjJUBxv1/kj1nGTGesI7reAHnYajCWAV8fzzTWTBjeT78KhAdcz4Kwf4W4kx4OFgAApaO+fosmBPNpeUtCgPBFLuN7ALUB1HgBqA3Ii15DeGsjEWET5Fk/eYgdb++WcHbzrjQ5AGyvzipfy+g1AlTrZVw3AlQOs21mnckAVawXgEW/tnOLar2cuRSkS6C9W0r8LI80psLqDGDR58maBcYIMN1dyXR+nPMqGCNAnmTJAiOWQR3Rl10WncmA7qlaOVgnQFL0azu3kTGZUiBfBrOmf95ICzAJ0sS4vr8fHTvRaIvGe4JeYJQAaYgaWkXxO0l7AHX6A8TL4J7SVfli3SqQN6QCNAtXlNMDMGAZRPfv5WftLvaUSgvobapPIYK8BJKcDlNp6ZmfXABqjNoHpEVZRykY8WtwaXiHpxmaPYF1JZB3LzBGgKWhXDMkKwFXb4oa0QTTosrSXsdewJgSoMIaAZzdCVJjhQC6og9YIoBOvADUBlDjBaA2gBovAP/m7aIeAMDy1iWNNZphfjE/AZ8BUQFczYK46AOC5wbnNjq//0M9nq9pM0w3fCDDzgMpD07yIthOnPNAigAMm4VIctzjGfED8yHNczaWVVYAAAAASUVORK5CYII=")]
    [ExportMetadata("BackgroundColor", "White")]
    [ExportMetadata("PrimaryFontColor", "Black")]
    [ExportMetadata("SecondaryFontColor", "DimGray")]
    public class PowerPagesAlmDriftInspectorPlugin : PluginBase
    {
        private const string OpenXmlAssemblyName = "DocumentFormat.OpenXml";
        private const string OpenXmlAssemblyFileName = "DocumentFormat.OpenXml.dll";
        private const string PluginPackageId = "PowerPagesAlmDriftInspector";

        private static readonly object AssemblyResolverLock = new object();
        private static bool _assemblyResolverRegistered;

        public override IXrmToolBoxPluginControl GetControl()
        {
            EnsureAssemblyResolverRegistered();

            var control = new PowerPagesAlmDriftInspectorControl();

            // TC4 keeps the validated comparison/export control untouched, but unlike
            // TC2 the diagnostics now bind to an authoritative retrieval registry.
            // This removes the fragile dependency on finding WebsiteModel instances
            // in private fields on the host control.
            PowerPagesDataModelDiagnostics.Attach(control);

            return control;
        }

        private static void EnsureAssemblyResolverRegistered()
        {
            if (_assemblyResolverRegistered)
            {
                return;
            }

            lock (AssemblyResolverLock)
            {
                if (_assemblyResolverRegistered)
                {
                    return;
                }

                AppDomain.CurrentDomain.AssemblyResolve -= ResolvePluginAssembly;
                AppDomain.CurrentDomain.AssemblyResolve += ResolvePluginAssembly;

                _assemblyResolverRegistered = true;
            }
        }

        private static Assembly ResolvePluginAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyName requestedAssemblyName;

            try
            {
                requestedAssemblyName = new AssemblyName(args.Name);
            }
            catch
            {
                return null;
            }

            if (!string.Equals(
                    requestedAssemblyName.Name,
                    OpenXmlAssemblyName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Assembly alreadyLoadedAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly =>
                    string.Equals(
                        assembly.GetName().Name,
                        OpenXmlAssemblyName,
                        StringComparison.OrdinalIgnoreCase));

            if (alreadyLoadedAssembly != null)
            {
                return alreadyLoadedAssembly;
            }

            foreach (string candidateFile in GetOpenXmlCandidateFiles())
            {
                Assembly loadedAssembly = TryLoadAssembly(candidateFile);

                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }
            }

            return null;
        }

        private static IEnumerable<string> GetOpenXmlCandidateFiles()
        {
            List<string> candidates = new List<string>();

            string pluginAssemblyFolder =
                GetSafeDirectoryName(typeof(PowerPagesAlmDriftInspectorPlugin).Assembly.Location);
            string appBaseFolder = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;

            AddCandidateFilesForFolder(candidates, pluginAssemblyFolder);
            AddCandidateFilesForFolder(candidates, appBaseFolder);

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!string.IsNullOrWhiteSpace(appData))
            {
                string xrmToolBoxFolder = Path.Combine(appData, "MscrmTools", "XrmToolBox");

                AddCandidateFilesForFolder(candidates, xrmToolBoxFolder);
                AddCandidateFilesForFolder(candidates, Path.Combine(xrmToolBoxFolder, "Plugins"));
                AddCandidateFilesForFolder(candidates, Path.Combine(xrmToolBoxFolder, "NugetPlugins"));
                AddNugetPluginPackageCandidateFiles(
                    candidates,
                    Path.Combine(xrmToolBoxFolder, "NugetPlugins"));
            }

            return candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static void AddCandidateFilesForFolder(
            List<string> candidates,
            string folder)
        {
            if (candidates == null || string.IsNullOrWhiteSpace(folder))
            {
                return;
            }

            candidates.Add(Path.Combine(folder, OpenXmlAssemblyFileName));
            candidates.Add(Path.Combine(folder, "Plugins", OpenXmlAssemblyFileName));
            candidates.Add(Path.Combine(folder, "lib", "net48", OpenXmlAssemblyFileName));
            candidates.Add(Path.Combine(
                folder,
                "lib",
                "net48",
                "Plugins",
                OpenXmlAssemblyFileName));
        }

        private static void AddNugetPluginPackageCandidateFiles(
            List<string> candidates,
            string nugetPluginsFolder)
        {
            if (candidates == null ||
                string.IsNullOrWhiteSpace(nugetPluginsFolder) ||
                !Directory.Exists(nugetPluginsFolder))
            {
                return;
            }

            try
            {
                foreach (string pluginPackageFolder in Directory.GetDirectories(
                             nugetPluginsFolder,
                             PluginPackageId + "*",
                             SearchOption.TopDirectoryOnly))
                {
                    AddCandidateFilesForFolder(candidates, pluginPackageFolder);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(
                    "PowerPagesAlmDriftInspector assembly resolver could not search " +
                    "NugetPlugins folder: " + ex.Message);
            }
        }

        private static Assembly TryLoadAssembly(string candidateFile)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(candidateFile) &&
                    File.Exists(candidateFile))
                {
                    return Assembly.LoadFrom(candidateFile);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(
                    "PowerPagesAlmDriftInspector assembly resolver could not load '" +
                    candidateFile + "': " + ex.Message);
            }

            return null;
        }

        private static string GetSafeDirectoryName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            return Path.GetDirectoryName(filePath) ?? string.Empty;
        }
    }

    /// <summary>
    /// 1.2026.1.4 RC2 compatibility, selection-safety and evidence adapter.
    ///
    /// This class is deliberately located in the already-compiled plugin source file.
    /// That keeps RC2 isolated from the large validated main control and avoids any
    /// csproj or Designer changes.
    ///
    /// The adapter does not perform Dataverse writes and does not change comparison,
    /// duplicate or filtering semantics. RC2 also adds a guarded export-metadata overlay
    /// so HTML/Excel evidence carries the selected website data-model label.
    /// </summary>
    internal static class PowerPagesDataModelDiagnostics
    {
        private static readonly List<DiagnosticSession> Sessions =
            new List<DiagnosticSession>();

        public static void Attach(Control root)
        {
            if (root == null)
            {
                return;
            }

            lock (Sessions)
            {
                if (Sessions.Any(session => ReferenceEquals(session.Root, root)))
                {
                    return;
                }

                DiagnosticSession diagnosticSession = new DiagnosticSession(root);
                Sessions.Add(diagnosticSession);

                root.Disposed += delegate
                {
                    diagnosticSession.Dispose();

                    lock (Sessions)
                    {
                        Sessions.Remove(diagnosticSession);
                    }
                };

                diagnosticSession.Start();
            }
        }

        private sealed class DiagnosticSession : IDisposable
        {
            private const string DataModelColumnName = "PowerPagesDataModelRc2";

            private readonly System.Windows.Forms.Timer _timer;
            private bool _disposed;
            private bool _diagnosticsEnabledLogged;
            private string _lastLoggedPairKey;
            private readonly Dictionary<ComboBox, string> _websiteComboRoles =
                new Dictionary<ComboBox, string>();
            private readonly Dictionary<Form, WebsiteDialogState> _websiteDialogStates =
                new Dictionary<Form, WebsiteDialogState>();
            private readonly HashSet<Button> _hookedExportButtons =
                new HashSet<Button>();
            private ExportMetadataOverlay _activeExportOverlay;
            private System.Windows.Forms.Timer _exportOverlayDelayTimer;

            public DiagnosticSession(Control root)
            {
                Root = root;

                _timer = new System.Windows.Forms.Timer
                {
                    Interval = 500
                };

                _timer.Tick += Timer_Tick;
            }

            public Control Root { get; private set; }

            public void Start()
            {
                if (_disposed)
                {
                    return;
                }

                _timer.Start();
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                CancelPendingExportModelOverlay();
                RestoreExportModelOverlay();

                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer.Dispose();
            }

            private void Timer_Tick(object sender, EventArgs e)
            {
                if (_disposed || Root == null || Root.IsDisposed)
                {
                    Dispose();
                    return;
                }

                try
                {
                    List<WebsiteCandidate> candidates = ReadWebsiteCandidates(Root);

                    UpdateWebsiteSelectionDialogs();
                    RewriteLegacyEnhancedOnlyLogText();
                    UpdateSelectedWebsiteGrid(candidates);
                    UpdateEnvironmentCards(candidates);
                    UpdateExportButtons(candidates);
                    AppendConfirmedModelPairLog(candidates);
                    LogDiagnosticsEnabledOnce();
                }
                catch (Exception ex)
                {
                    // RC2 diagnostics must never interfere with the comparison tool.
                    Trace.WriteLine(
                        "PowerPagesAlmDriftInspector RC2 diagnostic/UI update skipped: " +
                        ex.Message);
                }
            }

            private void UpdateWebsiteSelectionDialogs()
            {
                foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
                {
                    if (form == null || form.IsDisposed)
                    {
                        continue;
                    }

                    string title = form.Text ?? string.Empty;
                    bool looksLikePairDialog =
                        title.IndexOf(
                            "Source and Target Websites",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        title.IndexOf(
                            "Website",
                            StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!looksLikePairDialog)
                    {
                        continue;
                    }

                    List<ComboBox> comboBoxes = GetDescendants<ComboBox>(form)
                        .OrderBy(GetScreenTop)
                        .ThenBy(GetScreenLeft)
                        .ToList();

                    for (int index = 0; index < comboBoxes.Count; index++)
                    {
                        ComboBox comboBox = comboBoxes[index];
                        string role = index == 0
                            ? "Source"
                            : index == 1
                                ? "Target"
                                : string.Empty;

                        if (string.IsNullOrWhiteSpace(role))
                        {
                            continue;
                        }

                        _websiteComboRoles[comboBox] = role;

                        comboBox.FormattingEnabled = true;
                        comboBox.Format -= WebsiteComboBox_Format;
                        comboBox.Format += WebsiteComboBox_Format;

                        comboBox.SelectionChangeCommitted -=
                            WebsiteComboBox_SelectionChangeCommitted;
                        comboBox.SelectionChangeCommitted +=
                            WebsiteComboBox_SelectionChangeCommitted;

                        comboBox.Refresh();
                    }

                    ComboBox sourceCombo = comboBoxes.Count > 0
                        ? comboBoxes[0]
                        : null;
                    ComboBox targetCombo = comboBoxes.Count > 1
                        ? comboBoxes[1]
                        : null;

                    if (sourceCombo != null && targetCombo != null)
                    {
                        WebsiteDialogState state;

                        if (!_websiteDialogStates.TryGetValue(
                                form,
                                out state))
                        {
                            state = new WebsiteDialogState();
                            _websiteDialogStates[form] = state;

                            form.FormClosed -=
                                WebsiteSelectionForm_FormClosed;
                            form.FormClosed +=
                                WebsiteSelectionForm_FormClosed;
                        }

                        state.SourceCombo = sourceCombo;
                        state.TargetCombo = targetCombo;

                        // Important RC2 behavior:
                        // Until the user explicitly chooses a Target, re-apply the
                        // unique-match rule on every adapter refresh. This prevents
                        // the underlying dialog's normal first-item selection from
                        // re-introducing an unrelated site such as "Blank Page".
                        if (!state.TargetWasManuallySelected)
                        {
                            ApplyUniqueTargetSuggestion(
                                sourceCombo,
                                targetCombo,
                                true);
                        }

                        UpdateWebsiteDialogConfirmButton(
                            form,
                            sourceCombo,
                            targetCombo);
                    }

                    foreach (Label label in GetDescendants<Label>(form))
                    {
                        if (label == null ||
                            string.IsNullOrWhiteSpace(label.Text))
                        {
                            continue;
                        }

                        if (label.Text.IndexOf(
                                "Power Pages Management website",
                                StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            label.Text = ReplaceOrdinalIgnoreCase(
                                label.Text,
                                "Power Pages Management website",
                                "Power Pages website");
                        }

                        if (label.Text.IndexOf(
                                "An exact Name or Partial URL match is suggested",
                                StringComparison.OrdinalIgnoreCase) >= 0 &&
                            label.Text.IndexOf(
                                "No arbitrary website is selected",
                                StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            label.Text =
                                label.Text.TrimEnd() +
                                " No arbitrary website is selected when a unique match is not available.";
                        }
                    }
                }
            }

            private void WebsiteSelectionForm_FormClosed(
                object sender,
                FormClosedEventArgs e)
            {
                Form form = sender as Form;

                if (form == null)
                {
                    return;
                }

                _websiteDialogStates.Remove(form);

                foreach (ComboBox comboBox in
                         GetDescendants<ComboBox>(form).ToList())
                {
                    _websiteComboRoles.Remove(comboBox);
                }
            }

            private void WebsiteComboBox_SelectionChangeCommitted(
                object sender,
                EventArgs e)
            {
                ComboBox comboBox = sender as ComboBox;

                if (comboBox == null)
                {
                    return;
                }

                string role;
                if (!_websiteComboRoles.TryGetValue(
                        comboBox,
                        out role))
                {
                    return;
                }

                Form form = comboBox.FindForm();

                if (form == null)
                {
                    return;
                }

                List<ComboBox> comboBoxes = GetDescendants<ComboBox>(form)
                    .OrderBy(GetScreenTop)
                    .ThenBy(GetScreenLeft)
                    .ToList();

                ComboBox sourceCombo = comboBoxes.Count > 0
                    ? comboBoxes[0]
                    : null;
                ComboBox targetCombo = comboBoxes.Count > 1
                    ? comboBoxes[1]
                    : null;

                WebsiteDialogState state;
                _websiteDialogStates.TryGetValue(
                    form,
                    out state);

                if (string.Equals(
                        role,
                        "Source",
                        StringComparison.OrdinalIgnoreCase) &&
                    sourceCombo != null &&
                    targetCombo != null)
                {
                    if (state != null)
                    {
                        state.TargetWasManuallySelected = false;
                    }

                    ApplyUniqueTargetSuggestion(
                        sourceCombo,
                        targetCombo,
                        true);
                }
                else if (string.Equals(
                             role,
                             "Target",
                             StringComparison.OrdinalIgnoreCase))
                {
                    if (state != null)
                    {
                        state.TargetWasManuallySelected =
                            targetCombo != null &&
                            targetCombo.SelectedIndex >= 0;
                    }
                }

                UpdateWebsiteDialogConfirmButton(
                    form,
                    sourceCombo,
                    targetCombo);
            }

            private void ApplyUniqueTargetSuggestion(
                ComboBox sourceCombo,
                ComboBox targetCombo,
                bool clearWhenAmbiguous)
            {
                if (sourceCombo == null || targetCombo == null)
                {
                    return;
                }

                WebsiteModel sourceWebsite = ResolveWebsiteFromComboItem(
                    sourceCombo,
                    sourceCombo.SelectedItem,
                    GetRoleWebsiteBatch("Source"));

                if (sourceWebsite == null)
                {
                    if (clearWhenAmbiguous)
                    {
                        targetCombo.SelectedIndex = -1;
                    }

                    return;
                }

                List<WebsiteModel> targetWebsites =
                    GetRoleWebsiteBatch("Target");

                WebsiteModel suggestedTarget =
                    FindUniqueSuggestedTargetWebsite(
                        sourceWebsite,
                        targetWebsites);

                if (suggestedTarget == null)
                {
                    if (clearWhenAmbiguous)
                    {
                        targetCombo.SelectedIndex = -1;
                    }

                    return;
                }

                int matchingIndex = FindWebsiteComboIndex(
                    targetCombo,
                    suggestedTarget,
                    targetWebsites);

                if (matchingIndex >= 0)
                {
                    targetCombo.SelectedIndex = matchingIndex;
                }
                else if (clearWhenAmbiguous)
                {
                    targetCombo.SelectedIndex = -1;
                }
            }

            private static WebsiteModel FindUniqueSuggestedTargetWebsite(
                WebsiteModel sourceWebsite,
                IEnumerable<WebsiteModel> targetWebsites)
            {
                if (sourceWebsite == null)
                {
                    return null;
                }

                List<WebsiteModel> targets =
                    (targetWebsites ?? Enumerable.Empty<WebsiteModel>())
                    .Where(website => website != null)
                    .ToList();

                if (targets.Count == 0)
                {
                    return null;
                }

                string sourceName =
                    (sourceWebsite.Name ?? string.Empty).Trim();
                string sourcePartialUrl =
                    (sourceWebsite.PartialUrl ?? string.Empty).Trim();

                List<WebsiteModel> matches = targets
                    .Where(target =>
                    {
                        bool nameMatch =
                            !string.IsNullOrWhiteSpace(sourceName) &&
                            string.Equals(
                                (target.Name ?? string.Empty).Trim(),
                                sourceName,
                                StringComparison.OrdinalIgnoreCase);

                        bool partialMatch =
                            !string.IsNullOrWhiteSpace(sourcePartialUrl) &&
                            string.Equals(
                                (target.PartialUrl ?? string.Empty).Trim(),
                                sourcePartialUrl,
                                StringComparison.OrdinalIgnoreCase);

                        return nameMatch || partialMatch;
                    })
                    .GroupBy(target => target.WebsiteId)
                    .Select(group => group.First())
                    .ToList();

                return matches.Count == 1
                    ? matches[0]
                    : null;
            }

            private static int FindWebsiteComboIndex(
                ComboBox comboBox,
                WebsiteModel expectedWebsite,
                IEnumerable<WebsiteModel> roleBatch)
            {
                if (comboBox == null || expectedWebsite == null)
                {
                    return -1;
                }

                for (int index = 0; index < comboBox.Items.Count; index++)
                {
                    WebsiteModel itemWebsite = ResolveWebsiteFromComboItem(
                        comboBox,
                        comboBox.Items[index],
                        roleBatch);

                    if (itemWebsite != null &&
                        itemWebsite.WebsiteId == expectedWebsite.WebsiteId)
                    {
                        return index;
                    }
                }

                return -1;
            }

            private void UpdateWebsiteDialogConfirmButton(
                Form form,
                ComboBox sourceCombo,
                ComboBox targetCombo)
            {
                if (form == null)
                {
                    return;
                }

                Button confirmButton = GetDescendants<Button>(form)
                    .FirstOrDefault(button =>
                        string.Equals(
                            (button.Text ?? string.Empty).Trim(),
                            "Load and Compare",
                            StringComparison.OrdinalIgnoreCase));

                if (confirmButton == null)
                {
                    return;
                }

                bool sourceValid =
                    sourceCombo != null &&
                    sourceCombo.SelectedIndex >= 0 &&
                    ResolveWebsiteFromComboItem(
                        sourceCombo,
                        sourceCombo.SelectedItem,
                        GetRoleWebsiteBatch("Source")) != null;

                bool targetValid =
                    targetCombo != null &&
                    targetCombo.SelectedIndex >= 0 &&
                    ResolveWebsiteFromComboItem(
                        targetCombo,
                        targetCombo.SelectedItem,
                        GetRoleWebsiteBatch("Target")) != null;

                confirmButton.Enabled = sourceValid && targetValid;
            }

            private void WebsiteComboBox_Format(
                object sender,
                ListControlConvertEventArgs e)
            {
                ComboBox comboBox = sender as ComboBox;

                if (comboBox == null || e == null)
                {
                    return;
                }

                string role;
                if (!_websiteComboRoles.TryGetValue(comboBox, out role))
                {
                    return;
                }

                List<WebsiteModel> roleBatch = GetRoleWebsiteBatch(role);
                WebsiteModel website = ResolveWebsiteFromComboItem(
                    comboBox,
                    e.ListItem,
                    roleBatch);

                if (website == null)
                {
                    return;
                }

                string displayText = GetComboItemDisplayText(comboBox, e.ListItem);

                if (string.IsNullOrWhiteSpace(displayText))
                {
                    displayText = website.DisplayName;
                }

                displayText = RemoveDataModelSuffix(displayText);
                e.Value = displayText + " [" + website.DataModelShortName + "]";
            }

            private static int GetScreenTop(Control control)
            {
                try
                {
                    return control.PointToScreen(Point.Empty).Y;
                }
                catch
                {
                    return control == null ? int.MaxValue : control.Top;
                }
            }

            private static int GetScreenLeft(Control control)
            {
                try
                {
                    return control.PointToScreen(Point.Empty).X;
                }
                catch
                {
                    return control == null ? int.MaxValue : control.Left;
                }
            }

            private static string RemoveDataModelSuffix(string value)
            {
                string text = (value ?? string.Empty).Trim();
                string[] suffixes =
                {
                    " [Enhanced (mspp)]",
                    " [Standard (adx)]",
                    " [Unknown]"
                };

                foreach (string suffix in suffixes)
                {
                    if (text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        return text.Substring(0, text.Length - suffix.Length).TrimEnd();
                    }
                }

                return text;
            }

            private static string GetComboItemDisplayText(
                ComboBox comboBox,
                object item)
            {
                if (item == null)
                {
                    return string.Empty;
                }

                WebsiteModel website = item as WebsiteModel;
                if (website != null)
                {
                    return website.DisplayName;
                }

                string displayMember = comboBox == null
                    ? string.Empty
                    : comboBox.DisplayMember ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(displayMember))
                {
                    object displayValue = TryReadMemberValue(item, displayMember);
                    if (displayValue != null)
                    {
                        string text = Convert.ToString(displayValue);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            return text;
                        }
                    }
                }

                return Convert.ToString(item) ?? string.Empty;
            }

            private static WebsiteModel ResolveWebsiteFromComboItem(
                ComboBox comboBox,
                object item,
                IEnumerable<WebsiteModel> roleBatch)
            {
                WebsiteModel direct = FindNestedWebsiteModel(item, 0);
                if (direct != null)
                {
                    return direct;
                }

                List<WebsiteModel> websites = (roleBatch ?? Enumerable.Empty<WebsiteModel>())
                    .Where(website => website != null)
                    .ToList();

                if (websites.Count == 0)
                {
                    return null;
                }

                Guid itemId = TryReadGuidFromObject(item);
                if (itemId != Guid.Empty)
                {
                    WebsiteModel idMatch = websites.FirstOrDefault(
                        website => website.WebsiteId == itemId);

                    if (idMatch != null)
                    {
                        return idMatch;
                    }
                }

                string displayText = RemoveDataModelSuffix(
                    GetComboItemDisplayText(comboBox, item));

                List<WebsiteModel> textMatches = websites
                    .Where(website =>
                        string.Equals(
                            (website.DisplayName ?? string.Empty).Trim(),
                            displayText,
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            (website.Name ?? string.Empty).Trim(),
                            displayText,
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            (website.PartialUrl ?? string.Empty).Trim(),
                            displayText,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return textMatches.Count == 1 ? textMatches[0] : null;
            }

            private static WebsiteModel FindNestedWebsiteModel(object value, int depth)
            {
                if (value == null || depth > 2)
                {
                    return null;
                }

                WebsiteModel direct = value as WebsiteModel;
                if (direct != null)
                {
                    return direct;
                }

                Type type = value.GetType();
                if (type.IsPrimitive || type.IsEnum || value is string)
                {
                    return null;
                }

                foreach (PropertyInfo property in type.GetProperties(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (property.GetIndexParameters().Length != 0)
                    {
                        continue;
                    }

                    object nested;
                    try
                    {
                        nested = property.GetValue(value, null);
                    }
                    catch
                    {
                        continue;
                    }

                    WebsiteModel found = nested as WebsiteModel;
                    if (found != null)
                    {
                        return found;
                    }

                    string propertyName = property.Name ?? string.Empty;
                    if (propertyName.IndexOf("website", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = FindNestedWebsiteModel(nested, depth + 1);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }

                foreach (FieldInfo field in type.GetFields(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    object nested;
                    try
                    {
                        nested = field.GetValue(value);
                    }
                    catch
                    {
                        continue;
                    }

                    WebsiteModel found = nested as WebsiteModel;
                    if (found != null)
                    {
                        return found;
                    }

                    string fieldName = field.Name ?? string.Empty;
                    if (fieldName.IndexOf("website", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = FindNestedWebsiteModel(nested, depth + 1);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }

                return null;
            }

            private static Guid TryReadGuidFromObject(object value)
            {
                if (value == null)
                {
                    return Guid.Empty;
                }

                if (value is Guid)
                {
                    return (Guid)value;
                }

                string[] names =
                {
                    "WebsiteId",
                    "WebsiteGuid",
                    "RecordId",
                    "Id"
                };

                foreach (string name in names)
                {
                    object memberValue = TryReadMemberValue(value, name);
                    if (memberValue is Guid && (Guid)memberValue != Guid.Empty)
                    {
                        return (Guid)memberValue;
                    }

                    Guid parsed;
                    if (memberValue != null &&
                        Guid.TryParse(Convert.ToString(memberValue), out parsed) &&
                        parsed != Guid.Empty)
                    {
                        return parsed;
                    }
                }

                return Guid.Empty;
            }

            private static object TryReadMemberValue(object value, string memberName)
            {
                if (value == null || string.IsNullOrWhiteSpace(memberName))
                {
                    return null;
                }

                Type type = value.GetType();

                PropertyInfo property = type.GetProperty(
                    memberName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    try
                    {
                        return property.GetValue(value, null);
                    }
                    catch
                    {
                        // Continue to field lookup.
                    }
                }

                FieldInfo field = type.GetField(
                    memberName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

                if (field != null)
                {
                    try
                    {
                        return field.GetValue(value);
                    }
                    catch
                    {
                        return null;
                    }
                }

                return null;
            }

            private static List<WebsiteModel> GetRoleWebsiteBatch(string role)
            {
                List<List<WebsiteModel>> batches =
                    DataverseRetrievalService.GetRecentWebsiteBatchesSnapshot();

                if (batches.Count == 0)
                {
                    return new List<WebsiteModel>();
                }

                if (batches.Count == 1)
                {
                    return batches[0];
                }

                if (string.Equals(role, "Source", StringComparison.OrdinalIgnoreCase))
                {
                    return batches[batches.Count - 2];
                }

                if (string.Equals(role, "Target", StringComparison.OrdinalIgnoreCase))
                {
                    return batches[batches.Count - 1];
                }

                return new List<WebsiteModel>();
            }

            private void UpdateExportButtons(
                List<WebsiteCandidate> candidates)
            {
                foreach (Button button in GetDescendants<Button>(Root))
                {
                    string text = (button.Text ?? string.Empty).Trim();

                    bool isEvidenceExport =
                        string.Equals(
                            text,
                            "Export HTML",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            text,
                            "Export Excel",
                            StringComparison.OrdinalIgnoreCase);

                    if (!isEvidenceExport)
                    {
                        continue;
                    }

                    if (!_hookedExportButtons.Contains(button))
                    {
                        button.MouseDown += ExportButton_MouseDown;
                        button.KeyDown += ExportButton_KeyDown;
                        button.Click += ExportButton_Click;
                        _hookedExportButtons.Add(button);
                    }
                }
            }

            private void ExportButton_MouseDown(
                object sender,
                MouseEventArgs e)
            {
                if (e != null && e.Button == MouseButtons.Left)
                {
                    ScheduleExportModelOverlay();
                }
            }

            private void ExportButton_KeyDown(
                object sender,
                KeyEventArgs e)
            {
                if (e != null &&
                    (e.KeyCode == Keys.Enter ||
                     e.KeyCode == Keys.Space))
                {
                    ScheduleExportModelOverlay();
                }
            }

            private void ExportButton_Click(
                object sender,
                EventArgs e)
            {
                // The existing export Click handler runs first. While its native
                // SaveFileDialog is open, RC2's delayed timer augments only the
                // report metadata fields. The dialog's default FileName has already
                // been calculated from the original clean website names.
                CancelPendingExportModelOverlay();
                RestoreExportModelOverlay();
            }

            private void ScheduleExportModelOverlay()
            {
                CancelPendingExportModelOverlay();

                _exportOverlayDelayTimer =
                    new System.Windows.Forms.Timer
                    {
                        // Deliberately longer than a normal mouse click so the
                        // existing export handler can create/show SaveFileDialog
                        // using the original website names first.
                        Interval = 350
                    };

                _exportOverlayDelayTimer.Tick +=
                    ExportOverlayDelayTimer_Tick;

                _exportOverlayDelayTimer.Start();
            }

            private void ExportOverlayDelayTimer_Tick(
                object sender,
                EventArgs e)
            {
                System.Windows.Forms.Timer timer =
                    _exportOverlayDelayTimer;

                _exportOverlayDelayTimer = null;

                if (timer != null)
                {
                    timer.Stop();
                    timer.Tick -=
                        ExportOverlayDelayTimer_Tick;
                    timer.Dispose();
                }

                BeginExportModelOverlay();
            }

            private void CancelPendingExportModelOverlay()
            {
                System.Windows.Forms.Timer timer =
                    _exportOverlayDelayTimer;

                _exportOverlayDelayTimer = null;

                if (timer == null)
                {
                    return;
                }

                timer.Stop();
                timer.Tick -=
                    ExportOverlayDelayTimer_Tick;
                timer.Dispose();
            }

            /// <summary>
            /// Temporarily augments the control's selected website metadata strings so
            /// HTML/Excel evidence includes the detected Power Pages data model.
            ///
            /// RC2 does not mutate WebsiteModel.DisplayName. In addition, the overlay
            /// is deliberately delayed until the existing SaveFileDialog is already
            /// open. This keeps the suggested filename clean while the report/workbook
            /// body still receives:
            ///   Data Model: Standard (adx)
            /// or
            ///   Data Model: Enhanced (mspp)
            /// </summary>
            private void BeginExportModelOverlay()
            {
                if (_activeExportOverlay != null)
                {
                    return;
                }

                List<WebsiteCandidate> candidates =
                    ReadWebsiteCandidates(Root);

                DataGridView grid = FindSelectedWebsiteGrid();

                if (grid == null)
                {
                    return;
                }

                DataGridViewRow sourceRow =
                    FindRoleRow(grid, "Source");
                DataGridViewRow targetRow =
                    FindRoleRow(grid, "Target");

                if (sourceRow == null || targetRow == null)
                {
                    return;
                }

                WebsiteModel sourceWebsite = ResolveWebsite(
                    candidates,
                    "Source",
                    GetCellTextByHeader(
                        grid,
                        sourceRow,
                        "Website Name"),
                    GetCellTextByHeader(
                        grid,
                        sourceRow,
                        "Partial URL"));

                WebsiteModel targetWebsite = ResolveWebsite(
                    candidates,
                    "Target",
                    GetCellTextByHeader(
                        grid,
                        targetRow,
                        "Website Name"),
                    GetCellTextByHeader(
                        grid,
                        targetRow,
                        "Partial URL"));

                if (sourceWebsite == null || targetWebsite == null)
                {
                    return;
                }

                var overlay = new ExportMetadataOverlay();

                ApplyStringFieldExportOverlay(
                    overlay,
                    "Source",
                    sourceWebsite);

                ApplyStringFieldExportOverlay(
                    overlay,
                    "Target",
                    targetWebsite);

                _activeExportOverlay = overlay;
            }

            private void ApplyStringFieldExportOverlay(
                ExportMetadataOverlay overlay,
                string role,
                WebsiteModel website)
            {
                if (overlay == null ||
                    website == null ||
                    string.IsNullOrWhiteSpace(role))
                {
                    return;
                }

                string expectedDisplay =
                    RemoveDataModelSuffix(
                        website.DisplayName);
                string expectedName =
                    (website.Name ?? string.Empty).Trim();

                foreach (FieldInfo field in
                         GetAllFields(Root.GetType()))
                {
                    if (field.FieldType != typeof(string))
                    {
                        continue;
                    }

                    string fieldName =
                        field.Name ?? string.Empty;

                    if (fieldName.IndexOf(
                            "website",
                            StringComparison.OrdinalIgnoreCase) < 0 ||
                        fieldName.IndexOf(
                            role,
                            StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    string currentValue;

                    try
                    {
                        currentValue =
                            Convert.ToString(
                                field.GetValue(Root)) ??
                            string.Empty;
                    }
                    catch
                    {
                        continue;
                    }

                    string normalized =
                        RemoveDataModelSuffix(
                            currentValue);

                    bool looksLikeSelectedWebsite =
                        (!string.IsNullOrWhiteSpace(
                             expectedDisplay) &&
                         string.Equals(
                             normalized,
                             expectedDisplay,
                             StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(
                             expectedName) &&
                         string.Equals(
                             normalized,
                             expectedName,
                             StringComparison.OrdinalIgnoreCase));

                    if (!looksLikeSelectedWebsite)
                    {
                        continue;
                    }

                    overlay.StringFields.Add(
                        new StringFieldMutation
                        {
                            Owner = Root,
                            Field = field,
                            OriginalValue = currentValue
                        });

                    try
                    {
                        field.SetValue(
                            Root,
                            AppendDataModelEvidence(
                                currentValue,
                                website.DataModelShortName));
                    }
                    catch
                    {
                        // Evidence overlay is best-effort and never blocks export.
                    }
                }
            }

            private void RestoreExportModelOverlay()
            {
                ExportMetadataOverlay overlay =
                    _activeExportOverlay;

                _activeExportOverlay = null;

                if (overlay == null)
                {
                    return;
                }

                foreach (StringFieldMutation mutation in
                         overlay.StringFields)
                {
                    if (mutation == null ||
                        mutation.Owner == null ||
                        mutation.Field == null)
                    {
                        continue;
                    }

                    try
                    {
                        mutation.Field.SetValue(
                            mutation.Owner,
                            mutation.OriginalValue);
                    }
                    catch
                    {
                        // Never interfere with host shutdown or an already disposed control.
                    }
                }

            }

            private static string AppendDataModelEvidence(
                string value,
                string dataModel)
            {
                string text =
                    (value ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(text) ||
                    string.IsNullOrWhiteSpace(dataModel))
                {
                    return value;
                }

                if (text.IndexOf(
                        "Data Model:",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return text;
                }

                return text +
                       " | Data Model: " +
                       dataModel;
            }

            private void UpdateSelectedWebsiteGrid(List<WebsiteCandidate> candidates)
            {
                DataGridView grid = FindSelectedWebsiteGrid();

                if (grid == null)
                {
                    return;
                }

                EnsureDataModelColumn(grid);

                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row == null || row.IsNewRow)
                    {
                        continue;
                    }

                    string role = GetCellTextByHeader(grid, row, "Role");
                    string websiteName = GetCellTextByHeader(grid, row, "Website Name");
                    string partialUrl = GetCellTextByHeader(grid, row, "Partial URL");

                    WebsiteModel website = ResolveWebsite(
                        candidates,
                        role,
                        websiteName,
                        partialUrl);

                    DataGridViewCell modelCell = row.Cells[DataModelColumnName];

                    if (modelCell == null)
                    {
                        continue;
                    }

                    if (website == null)
                    {
                        modelCell.Value = string.Empty;
                        modelCell.ToolTipText =
                            "The Power Pages data model has not been resolved yet.";
                        continue;
                    }

                    modelCell.Value = website.DataModelShortName;
                    modelCell.ToolTipText = website.DataModelDiagnosticName;
                }
            }

            private void UpdateEnvironmentCards(List<WebsiteCandidate> candidates)
            {
                DataGridView grid = FindSelectedWebsiteGrid();

                if (grid == null)
                {
                    return;
                }

                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row == null || row.IsNewRow)
                    {
                        continue;
                    }

                    string role = GetCellTextByHeader(grid, row, "Role");
                    string environment = GetCellTextByHeader(grid, row, "Environment");
                    string websiteName = GetCellTextByHeader(grid, row, "Website Name");
                    string partialUrl = GetCellTextByHeader(grid, row, "Partial URL");

                    WebsiteModel website = ResolveWebsite(
                        candidates,
                        role,
                        websiteName,
                        partialUrl);

                    if (website == null)
                    {
                        continue;
                    }

                    foreach (Label label in GetDescendants<Label>(Root))
                    {
                        string text = label.Text ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(text) ||
                            text.IndexOf("settings", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }

                        bool environmentMatches =
                            string.IsNullOrWhiteSpace(environment) ||
                            text.IndexOf(
                                environment,
                                StringComparison.OrdinalIgnoreCase) >= 0;

                        bool websiteMatches =
                            string.IsNullOrWhiteSpace(websiteName) ||
                            text.IndexOf(
                                websiteName,
                                StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!environmentMatches || !websiteMatches)
                        {
                            continue;
                        }

                        label.Text = InsertDataModelIntoSettingsSummary(
                            text,
                            website.DataModelShortName);
                    }
                }
            }

            private void AppendConfirmedModelPairLog(List<WebsiteCandidate> candidates)
            {
                DataGridView grid = FindSelectedWebsiteGrid();

                if (grid == null)
                {
                    return;
                }

                DataGridViewRow sourceRow = FindRoleRow(grid, "Source");
                DataGridViewRow targetRow = FindRoleRow(grid, "Target");

                if (sourceRow == null || targetRow == null)
                {
                    return;
                }

                WebsiteModel sourceWebsite = ResolveWebsite(
                    candidates,
                    "Source",
                    GetCellTextByHeader(grid, sourceRow, "Website Name"),
                    GetCellTextByHeader(grid, sourceRow, "Partial URL"));

                WebsiteModel targetWebsite = ResolveWebsite(
                    candidates,
                    "Target",
                    GetCellTextByHeader(grid, targetRow, "Website Name"),
                    GetCellTextByHeader(grid, targetRow, "Partial URL"));

                if (sourceWebsite == null || targetWebsite == null)
                {
                    return;
                }

                string pairKey =
                    sourceWebsite.WebsiteId + "|" +
                    sourceWebsite.DataModel + "|" +
                    targetWebsite.WebsiteId + "|" +
                    targetWebsite.DataModel;

                if (string.Equals(
                        _lastLoggedPairKey,
                        pairKey,
                        StringComparison.Ordinal))
                {
                    return;
                }

                string message =
                    "Website data models confirmed. Source: '" +
                    SafeWebsiteName(sourceWebsite) +
                    "' = " +
                    sourceWebsite.DataModelDiagnosticName +
                    "; Target: '" +
                    SafeWebsiteName(targetWebsite) +
                    "' = " +
                    targetWebsite.DataModelDiagnosticName +
                    ".";

                if (TryLogMessage(message))
                {
                    _lastLoggedPairKey = pairKey;
                }
            }

            private void LogDiagnosticsEnabledOnce()
            {
                if (_diagnosticsEnabledLogged)
                {
                    return;
                }

                if (FindLogControl() == null)
                {
                    return;
                }

                if (TryLogMessage(
                        "RC2 diagnostics enabled. Website model is bound to retrieval results; persistent website selection safety and clean-name export evidence are active."))
                {
                    _diagnosticsEnabledLogged = true;
                }
            }

            private void RewriteLegacyEnhancedOnlyLogText()
            {
                Control logControl = FindLogControl();

                if (logControl == null)
                {
                    return;
                }

                const string oldLoading =
                    "Loading enhanced-model websites from Power Pages Management.";
                const string newLoading =
                    "Loading Power Pages websites from supported data models.";

                const string oldDataSource =
                    "Data source: Power Pages Management enhanced model.";
                const string newDataSource =
                    "Data source: detected per selected website.";

                TextBoxBase textBoxBase = logControl as TextBoxBase;

                if (textBoxBase != null)
                {
                    string original = textBoxBase.Text ?? string.Empty;
                    string updated = original
                        .Replace(oldLoading, newLoading)
                        .Replace(oldDataSource, newDataSource);

                    if (!string.Equals(original, updated, StringComparison.Ordinal))
                    {
                        bool wasAtEnd =
                            textBoxBase.SelectionStart >=
                            Math.Max(0, original.Length - 1);

                        textBoxBase.Text = updated;

                        if (wasAtEnd)
                        {
                            textBoxBase.SelectionStart = textBoxBase.TextLength;
                            textBoxBase.ScrollToCaret();
                        }
                    }

                    return;
                }

                ListBox listBox = logControl as ListBox;

                if (listBox != null)
                {
                    for (int index = 0; index < listBox.Items.Count; index++)
                    {
                        string original =
                            Convert.ToString(listBox.Items[index]) ?? string.Empty;
                        string updated = original
                            .Replace(oldLoading, newLoading)
                            .Replace(oldDataSource, newDataSource);

                        if (!string.Equals(
                                original,
                                updated,
                                StringComparison.Ordinal))
                        {
                            listBox.Items[index] = updated;
                        }
                    }
                }
            }

            private bool TryLogMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return false;
                }

                try
                {
                    MethodInfo logMethod = GetAllMethods(Root.GetType())
                        .FirstOrDefault(method =>
                        {
                            if (!string.Equals(
                                    method.Name,
                                    "LogMessage",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }

                            ParameterInfo[] parameters = method.GetParameters();

                            return parameters.Length == 1 &&
                                   parameters[0].ParameterType == typeof(string);
                        });

                    if (logMethod != null)
                    {
                        logMethod.Invoke(Root, new object[] { message });
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(
                        "PowerPagesAlmDriftInspector TC3 could not invoke LogMessage: " +
                        ex.Message);
                }

                Control logControl = FindLogControl();

                TextBoxBase textBoxBase = logControl as TextBoxBase;

                if (textBoxBase != null)
                {
                    string line =
                        "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message;

                    if (textBoxBase.TextLength > 0)
                    {
                        textBoxBase.AppendText(Environment.NewLine);
                    }

                    textBoxBase.AppendText(line);
                    textBoxBase.SelectionStart = textBoxBase.TextLength;
                    textBoxBase.ScrollToCaret();
                    return true;
                }

                ListBox listBox = logControl as ListBox;

                if (listBox != null)
                {
                    listBox.Items.Add(
                        "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message);

                    if (listBox.Items.Count > 0)
                    {
                        listBox.TopIndex = listBox.Items.Count - 1;
                    }

                    return true;
                }

                return false;
            }

            private Control FindLogControl()
            {
                IEnumerable<Control> controls =
                    GetDescendants<Control>(Root)
                        .Where(control =>
                        {
                            string name = control.Name ?? string.Empty;
                            return name.IndexOf(
                                       "log",
                                       StringComparison.OrdinalIgnoreCase) >= 0;
                        });

                Control namedLog = controls.FirstOrDefault(control =>
                    control is TextBoxBase || control is ListBox);

                if (namedLog != null)
                {
                    return namedLog;
                }

                return GetDescendants<Control>(Root)
                    .FirstOrDefault(control =>
                    {
                        TextBoxBase textBoxBase = control as TextBoxBase;

                        if (textBoxBase != null)
                        {
                            return textBoxBase.Multiline &&
                                   (textBoxBase.Text ?? string.Empty)
                                       .IndexOf(
                                           "Comparison",
                                           StringComparison.OrdinalIgnoreCase) >= 0;
                        }

                        return false;
                    });
            }

            private DataGridView FindSelectedWebsiteGrid()
            {
                return GetDescendants<DataGridView>(Root)
                    .FirstOrDefault(grid =>
                        HasColumnHeader(grid, "Role") &&
                        HasColumnHeader(grid, "Environment") &&
                        HasColumnHeader(grid, "Website Name") &&
                        HasColumnHeader(grid, "Partial URL"));
            }

            private static DataGridViewRow FindRoleRow(
                DataGridView grid,
                string role)
            {
                if (grid == null)
                {
                    return null;
                }

                return grid.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(row =>
                        row != null &&
                        !row.IsNewRow &&
                        string.Equals(
                            GetCellTextByHeader(grid, row, "Role"),
                            role,
                            StringComparison.OrdinalIgnoreCase));
            }

            private static void EnsureDataModelColumn(DataGridView grid)
            {
                if (grid == null || grid.Columns.Contains(DataModelColumnName))
                {
                    return;
                }

                var column = new DataGridViewTextBoxColumn
                {
                    Name = DataModelColumnName,
                    HeaderText = "Data Model",
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    MinimumWidth = 120,
                    Width = 145,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };

                grid.Columns.Add(column);
            }

            private static WebsiteModel GetFirstWebsiteItem(ComboBox comboBox)
            {
                if (comboBox == null)
                {
                    return null;
                }

                foreach (object item in comboBox.Items)
                {
                    WebsiteModel website = item as WebsiteModel;

                    if (website != null)
                    {
                        return website;
                    }
                }

                return null;
            }

            private static List<WebsiteCandidate> ReadWebsiteCandidates(
                object owner)
            {
                var result = new List<WebsiteCandidate>();

                if (owner == null)
                {
                    return result;
                }

                // TC3 primary source: the exact batches returned by DataverseRetrievalService.
                // The last two retrieval starts correspond to Source and Target for the
                // paired Load and Compare workflow. Reflection below remains only as a
                // compatibility fallback for older control builds.
                List<List<WebsiteModel>> batches =
                    DataverseRetrievalService.GetRecentWebsiteBatchesSnapshot();

                if (batches.Count >= 2)
                {
                    foreach (WebsiteModel website in batches[batches.Count - 2])
                    {
                        AddCandidate(result, website, "Source", "RetrievalRegistry.Source");
                    }

                    foreach (WebsiteModel website in batches[batches.Count - 1])
                    {
                        AddCandidate(result, website, "Target", "RetrievalRegistry.Target");
                    }
                }
                else if (batches.Count == 1)
                {
                    foreach (WebsiteModel website in batches[0])
                    {
                        AddCandidate(result, website, string.Empty, "RetrievalRegistry");
                    }
                }

                foreach (FieldInfo field in GetAllFields(owner.GetType()))
                {
                    object value;

                    try
                    {
                        value = field.GetValue(owner);
                    }
                    catch
                    {
                        continue;
                    }

                    if (value == null)
                    {
                        continue;
                    }

                    string fieldName = field.Name ?? string.Empty;
                    string role = InferRole(fieldName);

                    WebsiteModel directWebsite = value as WebsiteModel;

                    if (directWebsite != null)
                    {
                        AddCandidate(result, directWebsite, role, fieldName);
                        continue;
                    }

                    bool fieldLooksLikeWebsiteCollection =
                        fieldName.IndexOf(
                            "website",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        FieldTypeLooksLikeWebsiteCollection(field.FieldType);

                    if (!fieldLooksLikeWebsiteCollection)
                    {
                        continue;
                    }

                    IEnumerable enumerable = value as IEnumerable;

                    if (enumerable == null || value is string)
                    {
                        continue;
                    }

                    int count = 0;

                    try
                    {
                        foreach (object item in enumerable)
                        {
                            if (++count > 250)
                            {
                                break;
                            }

                            WebsiteModel website = item as WebsiteModel;

                            if (website != null)
                            {
                                AddCandidate(result, website, role, fieldName);
                            }
                        }
                    }
                    catch
                    {
                        // A diagnostic scan should never affect the host control.
                    }
                }

                return result
                    .GroupBy(candidate =>
                        candidate.Website.WebsiteId + "|" +
                        candidate.Role + "|" +
                        candidate.MemberName)
                    .Select(group => group.First())
                    .ToList();
            }

            private static bool FieldTypeLooksLikeWebsiteCollection(Type type)
            {
                if (type == null || type == typeof(string))
                {
                    return false;
                }

                if (type.IsArray)
                {
                    return type.GetElementType() == typeof(WebsiteModel);
                }

                if (!type.IsGenericType)
                {
                    return false;
                }

                return type.GetGenericArguments()
                    .Any(argument => argument == typeof(WebsiteModel));
            }

            private static void AddCandidate(
                ICollection<WebsiteCandidate> result,
                WebsiteModel website,
                string role,
                string memberName)
            {
                if (result == null || website == null)
                {
                    return;
                }

                result.Add(new WebsiteCandidate
                {
                    Website = website,
                    Role = role,
                    MemberName = memberName ?? string.Empty
                });
            }

            private static WebsiteModel ResolveWebsite(
                IEnumerable<WebsiteCandidate> candidates,
                string role,
                string websiteName,
                string partialUrl)
            {
                List<WebsiteCandidate> list =
                    (candidates ?? Enumerable.Empty<WebsiteCandidate>())
                    .Where(candidate => candidate != null &&
                                        candidate.Website != null)
                    .ToList();

                if (list.Count == 0)
                {
                    return null;
                }

                List<WebsiteCandidate> matching = list
                    .Where(candidate =>
                        WebsiteTextMatches(
                            candidate.Website,
                            websiteName,
                            partialUrl))
                    .ToList();

                if (matching.Count == 0)
                {
                    return null;
                }

                List<WebsiteCandidate> roleMatches = matching
                    .Where(candidate =>
                        string.Equals(
                            candidate.Role,
                            role,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (roleMatches.Count > 0)
                {
                    return roleMatches
                        .OrderByDescending(candidate =>
                            IsSelectedLikeMember(candidate.MemberName))
                        .Select(candidate => candidate.Website)
                        .FirstOrDefault();
                }

                List<PowerPagesDataModel> distinctModels = matching
                    .Select(candidate => candidate.Website.DataModel)
                    .Distinct()
                    .ToList();

                if (distinctModels.Count == 1)
                {
                    return matching[0].Website;
                }

                List<WebsiteModel> distinctWebsiteIds = matching
                    .Select(candidate => candidate.Website)
                    .GroupBy(website => website.WebsiteId)
                    .Select(group => group.First())
                    .ToList();

                return distinctWebsiteIds.Count == 1
                    ? distinctWebsiteIds[0]
                    : null;
            }

            private static bool WebsiteTextMatches(
                WebsiteModel website,
                string websiteName,
                string partialUrl)
            {
                if (website == null)
                {
                    return false;
                }

                bool nameKnown = !string.IsNullOrWhiteSpace(websiteName);
                bool partialKnown = !string.IsNullOrWhiteSpace(partialUrl);

                bool nameMatches =
                    !nameKnown ||
                    string.Equals(
                        (website.Name ?? string.Empty).Trim(),
                        websiteName.Trim(),
                        StringComparison.OrdinalIgnoreCase);

                bool partialMatches =
                    !partialKnown ||
                    string.Equals(
                        (website.PartialUrl ?? string.Empty).Trim(),
                        partialUrl.Trim(),
                        StringComparison.OrdinalIgnoreCase);

                return nameMatches && partialMatches;
            }

            private static bool IsSelectedLikeMember(string memberName)
            {
                string name = memberName ?? string.Empty;

                return name.IndexOf(
                           "selected",
                           StringComparison.OrdinalIgnoreCase) >= 0 ||
                       name.IndexOf(
                           "current",
                           StringComparison.OrdinalIgnoreCase) >= 0 ||
                       name.IndexOf(
                           "confirmed",
                           StringComparison.OrdinalIgnoreCase) >= 0;
            }

            private static string InferRole(string memberName)
            {
                string name = memberName ?? string.Empty;

                if (name.IndexOf(
                        "source",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Source";
                }

                if (name.IndexOf(
                        "target",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Target";
                }

                return string.Empty;
            }

            private static string SafeWebsiteName(WebsiteModel website)
            {
                if (website == null)
                {
                    return "(unknown)";
                }

                if (!string.IsNullOrWhiteSpace(website.Name))
                {
                    return website.Name;
                }

                return website.DisplayName;
            }

            private static string InsertDataModelIntoSettingsSummary(
                string original,
                string modelText)
            {
                if (string.IsNullOrWhiteSpace(original) ||
                    string.IsNullOrWhiteSpace(modelText))
                {
                    return original;
                }

                string normalized = original
                    .Replace(" | Enhanced (mspp) | ", " | ")
                    .Replace(" | Standard (adx) | ", " | ")
                    .Replace(" | Unknown | ", " | ");

                int lastPipe = normalized.LastIndexOf('|');

                if (lastPipe < 0)
                {
                    return normalized;
                }

                string tail = normalized.Substring(lastPipe + 1).Trim();

                if (tail.IndexOf(
                        "settings",
                        StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return normalized;
                }

                string prefix = normalized.Substring(0, lastPipe).TrimEnd();

                return prefix + " | " + modelText + " | " + tail;
            }

            private static bool HasColumnHeader(
                DataGridView grid,
                string expectedHeader)
            {
                if (grid == null)
                {
                    return false;
                }

                return grid.Columns
                    .Cast<DataGridViewColumn>()
                    .Any(column =>
                        string.Equals(
                            (column.HeaderText ?? string.Empty).Trim(),
                            expectedHeader,
                            StringComparison.OrdinalIgnoreCase));
            }

            private static string GetCellTextByHeader(
                DataGridView grid,
                DataGridViewRow row,
                string headerText)
            {
                if (grid == null || row == null)
                {
                    return string.Empty;
                }

                DataGridViewColumn column = grid.Columns
                    .Cast<DataGridViewColumn>()
                    .FirstOrDefault(candidate =>
                        string.Equals(
                            (candidate.HeaderText ?? string.Empty).Trim(),
                            headerText,
                            StringComparison.OrdinalIgnoreCase));

                if (column == null || column.Index < 0 ||
                    column.Index >= row.Cells.Count)
                {
                    return string.Empty;
                }

                object value = row.Cells[column.Index].Value;

                return Convert.ToString(value) ?? string.Empty;
            }

            private static IEnumerable<FieldInfo> GetAllFields(Type type)
            {
                Type current = type;

                while (current != null && current != typeof(object))
                {
                    foreach (FieldInfo field in current.GetFields(
                                 BindingFlags.Instance |
                                 BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.DeclaredOnly))
                    {
                        yield return field;
                    }

                    current = current.BaseType;
                }
            }

            private static IEnumerable<MethodInfo> GetAllMethods(Type type)
            {
                Type current = type;

                while (current != null && current != typeof(object))
                {
                    foreach (MethodInfo method in current.GetMethods(
                                 BindingFlags.Instance |
                                 BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.DeclaredOnly))
                    {
                        yield return method;
                    }

                    current = current.BaseType;
                }
            }

            private static IEnumerable<T> GetDescendants<T>(Control root)
                where T : Control
            {
                if (root == null)
                {
                    yield break;
                }

                foreach (Control child in root.Controls)
                {
                    T typed = child as T;

                    if (typed != null)
                    {
                        yield return typed;
                    }

                    foreach (T descendant in GetDescendants<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }

            private static string ReplaceOrdinalIgnoreCase(
                string input,
                string oldValue,
                string newValue)
            {
                if (string.IsNullOrEmpty(input) ||
                    string.IsNullOrEmpty(oldValue))
                {
                    return input;
                }

                int index = input.IndexOf(
                    oldValue,
                    StringComparison.OrdinalIgnoreCase);

                if (index < 0)
                {
                    return input;
                }

                return input.Substring(0, index) +
                       newValue +
                       input.Substring(index + oldValue.Length);
            }

            private sealed class WebsiteDialogState
            {
                public ComboBox SourceCombo { get; set; }

                public ComboBox TargetCombo { get; set; }

                public bool TargetWasManuallySelected { get; set; }
            }

            private sealed class ExportMetadataOverlay
            {
                public ExportMetadataOverlay()
                {
                    StringFields =
                        new List<StringFieldMutation>();
                }

                public List<StringFieldMutation> StringFields
                {
                    get;
                    private set;
                }
            }

            private sealed class StringFieldMutation
            {
                public object Owner { get; set; }

                public FieldInfo Field { get; set; }

                public string OriginalValue { get; set; }
            }

            private sealed class WebsiteCandidate
            {
                public WebsiteModel Website { get; set; }

                public string Role { get; set; }

                public string MemberName { get; set; }
            }
        }
    }
}
