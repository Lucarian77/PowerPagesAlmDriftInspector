# Power Pages ALM Drift Inspector

Power Pages ALM Drift Inspector is an XrmToolBox plugin for comparing Power Pages site settings across Dataverse environments to identify ALM drift.

This tool helps solution teams quickly compare source and target environments, highlight missing or changed site settings, and export results for review.

---

## Features

- Compare Power Pages site settings between two Dataverse environments
- Capture separate Source and Target snapshots
- Compare site settings for selected websites
- Detect:
  - Different values
  - Missing in source
  - Missing in target
  - Exact matches
- Filter comparison results by:
  - Status
  - Category
  - Text search
- View detailed comparison popup for each setting
- Copy source value, target value, or both values directly from the popup
- Export comparison results to CSV
- View summary counts for compared settings, matches, and differences

---

## Requirements

- XrmToolBox
- Microsoft Dataverse / Dynamics 365 environment access
- Permissions to read:
  - Website records
  - Site Settings records

---

## Installation

Install directly from the XrmToolBox Tool Library once published.

---

## Usage

1. Open XrmToolBox and connect to your source Dataverse environment
2. Open Power Pages ALM Drift Inspector
3. Click Load Websites
4. Select a website
5. Click Load Site Settings
6. Click Capture Source
7. Connect to the target Dataverse environment
8. Repeat the process and click Capture Target
9. Click Compare
10. Filter or export the results as needed

---

## Example Use Cases

- Compare DEV vs UAT Power Pages configuration
- Compare UAT vs PROD site settings
- Validate deployment completeness after release
- Detect missing authentication, search, or portal configuration values
- Troubleshoot unexpected Power Pages behavior between environments


## License

MIT License

---

## Author

Created by Adrian Lucaci

GitHub:
https://github.com/Lucarian77

---

## Support

If you find this project useful, consider starring the repository or supporting future development.
