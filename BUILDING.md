# Building Power Pages ALM Drift Inspector 1.2026.1.3

## Requirements

- Visual Studio 2019 or later
- .NET Framework 4.8 Developer Pack
- NuGet package restore enabled
- XrmToolBox 1.2025.10.74 or later for runtime testing
- `nuget.exe` available on `PATH`
- Developer PowerShell for Visual Studio

## Restore and build from Developer PowerShell

Run these commands from the repository root:

```powershell
nuget restore .\PowerPagesAlmDriftInspector.sln -PackagesDirectory .\packages
msbuild .\PowerPagesAlmDriftInspector.sln /t:Rebuild /m /p:Configuration=Release /p:Platform="Any CPU"
```

The plugin assembly is written to:

```text
PowerPagesAlmDriftInspector\bin\Release\PowerPagesAlmDriftInspector.dll
```

## Create the package

Continue from the repository root:

```powershell
New-Item -ItemType Directory -Path .\artifacts -Force | Out-Null
nuget pack .\PowerPagesAlmDriftInspector\PowerPagesAlmDriftInspector.nuspec `
  -BasePath .\PowerPagesAlmDriftInspector `
  -OutputDirectory .\artifacts
```

Expected output:

```text
artifacts\PowerPagesAlmDriftInspector.1.2026.1.3.nupkg
```

Before publishing, inspect the package and confirm:

- `lib\net48\Plugins\PowerPagesAlmDriftInspector.dll` exists.
- `lib\net48\Plugins\DocumentFormat.OpenXml.dll` exists.
- Package version is `1.2026.1.3`.
- The XrmToolBox dependency is `1.2025.10.74` or later.
- The OpenXML dependency is exactly `2.13.1`.
- `README.md`, `RELEASE_NOTES_1.2026.1.3.md`, and `PowerPagesAlmDriftInspector_64.png` exist at the package root.

The NuGet specification sets `<readme>README.md</readme>`, so NuGet.org displays
the same README included at the package root.

## Checksum

```powershell
$package = Resolve-Path .\artifacts\PowerPagesAlmDriftInspector.1.2026.1.3.nupkg
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $package).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($package))" |
  Set-Content .\artifacts\PowerPagesAlmDriftInspector.1.2026.1.3.nupkg.sha256 -Encoding ascii
Get-Content .\artifacts\PowerPagesAlmDriftInspector.1.2026.1.3.nupkg.sha256
```

## Clean-install validation

1. Close XrmToolBox.
2. Remove only the previously installed Power Pages ALM Drift Inspector package through the supported Tool Library workflow.
3. Install the locally built `1.2026.1.3` package.
4. Restart XrmToolBox.
5. Confirm the plugin launches.
6. Select different source and target environments.
7. Click **Load and Compare**, select the Source and Target website pair, and run the comparison.
8. Validate duplicate, different, missing, and matching statuses.
9. Validate CSV, Excel, and HTML exports.
10. Close and reopen XrmToolBox, then repeat Excel export to confirm OpenXML loading remains stable.

For the complete GitHub, NuGet.org, and XrmToolBox release sequence, see
`PUBLISHING_GUIDE_1.2026.1.3.md` in the repository root.
