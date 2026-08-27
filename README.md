# Power Pages ALM Drift Inspector

Power Pages ALM Drift Inspector is an XrmToolBox plugin for comparing Power Pages site settings across Microsoft Dataverse environments to identify ALM drift.

Version **1.2026.1.4** supports both the standard/older Power Pages data model and the enhanced Power Pages data model, with the model detected independently for each selected website.

## Overview

Power Pages site settings can contain environment-specific configuration such as authentication settings, redirect URLs, identity provider values, feature flags, search settings, HTTP/security headers, analytics tracking values, certificates, storage configuration, and portal behavior settings.

The tool compares a selected Source website with a selected Target website and highlights:

- Matching settings
- Different values
- Settings missing from Source
- Settings missing from Target
- Duplicate setting names
- Likely environment-specific configuration
- Review guidance and recommended actions

## Supported Power Pages Data Models

Power Pages ALM Drift Inspector supports both current Power Pages storage models.

### Standard / older model

- Website table: `adx_website`
- Site Setting table: `adx_sitesetting`
- UI label: **Standard (adx)**

### Enhanced model

- Website table: `mspp_website`
- Site Setting table: `mspp_sitesetting`
- UI label: **Enhanced (mspp)**

The data model is detected **per website**, not per Dataverse environment. Standard and enhanced websites can therefore be compared independently, including mixed-model comparisons.

Supported comparison combinations include:

- Standard → Standard
- Enhanced → Enhanced
- Standard → Enhanced
- Enhanced → Standard

## Features

- Explicit **Select Source** and **Select Target** Dataverse connection workflow
- Load both Source and Target website lists in one operation
- Paired Source/Target website selection dialog
- Automatic Target suggestion only when a unique Name or Partial URL match exists
- No arbitrary Target selection when a unique match is unavailable
- Independent Source and Target website IDs
- Per-website Power Pages data-model detection
- Visible **Data Model** context in:
  - Website selector
  - Source/Target environment cards
  - Selected website grid
  - Activity Log
  - Excel and HTML export metadata
- Retrieves all Dataverse result pages
- Detects:
  - Match
  - Different Value
  - Missing in Source
  - Missing in Target
  - Duplicate
- Duplicate evidence includes record IDs, values, classifications, and value counts
- Boolean-equivalent comparison for values such as `True` and `true`
- Non-boolean values remain case-sensitive
- Focused live grid:
  - Setting Name
  - Source Value
  - Target Value
  - Category
  - Status
- Filters:
  - Text search
  - Status
  - Category
  - Findings only
- Detailed comparison popup
- Copy source, target, both values, or comparison details
- CSV export
- Formatted Excel `.xlsx` export
- Browser-friendly HTML report
- Review Focus, environment-specific guidance, and Recommended Action in exported evidence
- Sensitive Information notice in HTML reports
- Clean export filenames based on website names rather than diagnostic metadata

## Requirements

- .NET Framework 4.8
- XrmToolBox 1.2025.10.74 or later
- Microsoft Dataverse / Dynamics 365 access
- Read permissions for the Power Pages tables used by the selected website

For Standard websites:

- `adx_website`
- `adx_sitesetting`

For Enhanced websites:

- `mspp_website`
- `mspp_sitesetting`

Excel export uses **DocumentFormat.OpenXml 2.13.1**. The runtime DLL is included in the XrmToolBox NuGet package.

## Installation

Install **Power Pages ALM Drift Inspector** from the XrmToolBox Tool Library.

### Manual package testing

For local release validation:

1. Build the solution in **Release** mode.
2. Confirm the output contains:
   - `PowerPagesAlmDriftInspector.dll`
   - `DocumentFormat.OpenXml.dll`
3. Create the `.nupkg` from `PowerPagesAlmDriftInspector.nuspec`.
4. Install the package through the XrmToolBox local/manual package workflow.
5. Restart XrmToolBox.
6. Confirm the tool reports version **1.2026.1.4**.

## Usage

1. Open XrmToolBox.
2. Open **Power Pages ALM Drift Inspector**.
3. Click **Select Source** and choose the Source Dataverse environment.
4. Click **Select Target** and choose the Target Dataverse environment.
5. Click **Load and Compare**.
6. Review the Source and Target website selections.
7. Confirm the displayed **Standard (adx)** or **Enhanced (mspp)** data-model labels.
8. If there is no unique Target website match, select the intended Target explicitly.
9. Click **Load and Compare** in the website-selection dialog.
10. Review the summary and comparison results.
11. Use text, Status, Category, and **Findings only** filters as needed.
12. Double-click a row for detailed evidence.
13. Export the current view to CSV, Excel, or HTML as required.

## Result Statuses

### Match

The setting exists in both selected websites and the compared values are equivalent.

Boolean values are compared logically, so capitalization differences such as `True` and `true` are treated as a match.

### Different Value

The setting exists in both websites but the compared values differ.

### Missing in Source

The setting exists only in the Target website.

### Missing in Target

The setting exists only in the Source website.

### Duplicate

One or both websites contain multiple records with the same site-setting name.

Duplicate evidence preserves the record IDs and original values so the configuration can be reviewed before deployment.

## Export Options

### CSV

Exports the currently visible comparison result set after filters are applied.

### Excel

Creates a formatted `.xlsx` workbook with:

- Source and Target environment context
- Source and Target website and data-model context
- Summary counts
- Active filter information
- Color-coded result rows
- Duplicate evidence
- Review Focus
- Environment-Specific Note
- Recommended Action

### HTML

Creates a browser-friendly evidence report with:

- Comparison context
- Source and Target website data models
- ALM drift decision
- Summary metrics
- Sensitive Information notice
- Color-coded comparison results
- Duplicate evidence
- Review guidance and recommended actions

## Sensitive Information Notice

Power Pages site settings may contain sensitive or environment-specific configuration.

Exports can include URLs, identity-provider information, client identifiers, certificate references, tokens, keys, connection information, security policy values, and other protected configuration.

The tool intentionally does not redact comparison values. Review and protect exported CSV, Excel, and HTML reports before sharing them.

## Version 1.2026.1.4 Highlights

Version 1.2026.1.4 adds:

- Support for Standard / older Power Pages websites through `adx_website` and `adx_sitesetting`
- Continued Enhanced Power Pages support through `mspp_website` and `mspp_sitesetting`
- Per-website data-model detection
- Mixed-model comparison support
- Data Model labels and diagnostics throughout the UI
- Safe Target website selection when no unique match exists
- Data-model evidence in Excel and HTML exports
- Clean export filenames
- Version metadata aligned to 1.2026.1.4
- All comparison, duplicate, boolean-equivalence, filtering, and export improvements from 1.2026.1.3

## Compatibility

- .NET Framework 4.8
- XrmToolBox 1.2025.10.74 or later
- DocumentFormat.OpenXml 2.13.1
- Power Pages Standard data model
- Power Pages Enhanced data model

## License

MIT License

## Author

Created by Adrian Lucaci

Project repository:  
https://github.com/Lucarian77/PowerPagesAlmDriftInspector
