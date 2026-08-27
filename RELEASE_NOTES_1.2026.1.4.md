# Power Pages ALM Drift Inspector 1.2026.1.4

Version 1.2026.1.4 expands Power Pages compatibility while preserving the comparison, duplicate detection, filtering, and export behavior validated in 1.2026.1.3.

## Power Pages data-model compatibility

The tool now supports both Power Pages storage models.

### Standard / older model

- `adx_website`
- `adx_sitesetting`
- Displayed as **Standard (adx)**

### Enhanced model

- `mspp_website`
- `mspp_sitesetting`
- Displayed as **Enhanced (mspp)**

Data-model detection is performed **per website**. This allows Standard and Enhanced websites to coexist in one Dataverse environment and supports:

- Standard → Standard
- Enhanced → Enhanced
- Standard → Enhanced
- Enhanced → Standard

## Improvements

- Loads active websites from both supported Power Pages table families.
- Tracks the data model independently for each website.
- Loads site settings only from the table family that owns the selected website.
- Avoids combining `adx_*` and `mspp_*` site-setting records for one selected website.
- Displays the detected Data Model in the website selector, Source/Target cards, website grid, and Activity Log.
- Adds explicit physical-table evidence to diagnostics.
- Adds Source and Target Data Model context to Excel and HTML exports.
- Keeps export filenames based on website names without diagnostic Data Model text.
- Suggests a Target website only when there is a unique Name or Partial URL match.
- Leaves Target blank when a unique match cannot be identified.
- Keeps **Load and Compare** disabled until a valid Target is explicitly selected when required.
- Preserves the existing independent Source/Target website workflow.
- Preserves all Dataverse paging behavior.
- Preserves duplicate record detection and full duplicate evidence.
- Preserves logical boolean comparison so `True` and `true` match.
- Preserves case-sensitive comparison for non-boolean values.
- Preserves the focused five-column live comparison grid.
- Preserves CSV, Excel, and HTML export behavior.
- Preserves the OpenXML runtime-loading hardening introduced in 1.2026.1.2.

## Export evidence

Excel and HTML exports include the detected Source and Target Power Pages data models.

Examples:

- `Standard (adx)`
- `Enhanced (mspp)`

The Data Model context is included inside the exported evidence without being added to the default export filename.

## Security and sensitive information

Power Pages site settings can contain sensitive or environment-specific values.

The tool intentionally does not redact site-setting values in CSV, Excel, or HTML exports. Review exported evidence before sharing and store it only in approved locations.

## Compatibility

- .NET Framework 4.8
- XrmToolBox 1.2025.10.74 or later
- DocumentFormat.OpenXml 2.13.1
- Power Pages Standard data model
- Power Pages Enhanced data model

## Upgrade notes

No configuration migration is required.

Existing Enhanced-model comparisons continue to work. Standard/older Power Pages websites can now be selected and compared using the same Source/Target workflow.
