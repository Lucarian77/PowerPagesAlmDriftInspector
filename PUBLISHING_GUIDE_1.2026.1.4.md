# Power Pages ALM Drift Inspector 1.2026.1.4 Publishing Guide

This package contains the validated 1.2026.1.4 source replacements and the public publishing metadata.

## 1. Apply the release files

Copy the supplied files into the existing working solution:

- `PowerPagesAlmDriftInspectorPlugin.cs`
- `Models\WebsiteModel.cs`
- `Models\SiteSettingModel.cs`
- `Services\DataverseRetrievalService.cs`
- `Properties\AssemblyInfo.cs`
- `README.md`
- `PowerPagesAlmDriftInspector.nuspec`
- `RELEASE_NOTES_1.2026.1.4.md`

Keep the existing control, forms, resources, project file, packages.config, app.config, and other validated source files.

## 2. Build Release

In Visual Studio:

1. Select **Release**.
2. Clean Solution.
3. Rebuild Solution.
4. Confirm zero build errors.

Confirm `bin\Release` contains at least:

- `PowerPagesAlmDriftInspector.dll`
- `DocumentFormat.OpenXml.dll`

## 3. Verify DLL version

Open:

`bin\Release\PowerPagesAlmDriftInspector.dll`

Windows Properties → Details should show:

- File version: `1.2026.1.4`
- Product version: `1.2026.1.4`

## 4. Create the NuGet package

From the solution/project root, using `nuget.exe`:

```powershell
nuget pack PowerPagesAlmDriftInspector.nuspec -OutputDirectory .\nupkg
```

Expected output:

`PowerPagesAlmDriftInspector.1.2026.1.4.nupkg`

## 5. Inspect the package before publishing

A `.nupkg` is a ZIP archive. Inspect it and confirm these paths exist:

```text
lib/net48/Plugins/PowerPagesAlmDriftInspector.dll
lib/net48/Plugins/DocumentFormat.OpenXml.dll
README.md
RELEASE_NOTES_1.2026.1.4.md
PowerPagesAlmDriftInspector_64.png
```

Also confirm the `.nuspec` embedded in the package reports:

`1.2026.1.4`

## 6. Final local XrmToolBox package test

Install the generated `.nupkg` using the same local/manual XrmToolBox package workflow used during prior release validation.

After restart, confirm:

- Tool Library/plugin version: `1.2026.1.4`
- Standard website selection shows `Standard (adx)`
- Enhanced website selection shows `Enhanced (mspp)`
- No arbitrary Target website is selected when no unique match exists
- Excel export works after restart
- HTML and Excel filenames remain clean
- HTML and Excel evidence still contains Data Model context

## 7. Publish to NuGet

After the local package test passes:

```powershell
nuget push .\nupkg\PowerPagesAlmDriftInspector.1.2026.1.4.nupkg `
  -Source https://api.nuget.org/v3/index.json `
  -ApiKey <YOUR_NUGET_API_KEY>
```

Do not store the API key in the repository, scripts, screenshots, or release notes.

## 8. XrmToolBox Tool Library

After the NuGet package is available, follow the existing Power Pages ALM Drift Inspector Tool Library update workflow and refresh the listing for version `1.2026.1.4`.

Confirm the listing shows:

- Name: Power Pages ALM Drift Inspector
- Version: 1.2026.1.4
- Author: Adrian Lucaci
- Description: Compare Power Pages site settings across Dataverse environments to identify ALM drift.
- Icon loads correctly

## 9. GitHub release

Recommended tag:

`v1.2026.1.4`

Recommended release title:

`Power Pages ALM Drift Inspector 1.2026.1.4`

Use `RELEASE_NOTES_1.2026.1.4.md` as the release description.

Recommended release assets:

- `PowerPagesAlmDriftInspector.1.2026.1.4.nupkg`
- Optional checksum file for the `.nupkg`

## 10. Repository update

Commit the public source and metadata updates after final package validation.

At minimum update:

- validated 1.2026.1.4 source files
- `Properties\AssemblyInfo.cs`
- `README.md`
- `PowerPagesAlmDriftInspector.nuspec`
- `RELEASE_NOTES_1.2026.1.4.md`

Do not commit internal test exports or environment-specific configuration evidence.
