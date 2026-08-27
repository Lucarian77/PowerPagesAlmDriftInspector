# Power Pages ALM Drift Inspector 1.2026.1.4 Release Manifest

## Validated source replacements

- `PowerPagesAlmDriftInspectorPlugin.cs`
- `Models/WebsiteModel.cs`
- `Models/SiteSettingModel.cs`
- `Services/DataverseRetrievalService.cs`
- `Properties/AssemblyInfo.cs`

## Public release metadata

- `README.md`
- `PowerPagesAlmDriftInspector.nuspec`
- `RELEASE_NOTES_1.2026.1.4.md`

## Publishing instructions

- `PUBLISHING_GUIDE_1.2026.1.4.md`

## Important

This release bundle does not include a prebuilt `.nupkg` because the final package must be created from the locally validated **Release** build so that the actual `PowerPagesAlmDriftInspector.dll` and `DocumentFormat.OpenXml.dll` binaries are packaged.

Do not publish RC/test notes or environment-specific exported comparison evidence.
