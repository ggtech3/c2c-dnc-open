param([ValidateSet("Release", "Debug")][string]$Configuration = "Release", [string]$OutputDirectory = "")
$ErrorActionPreference = "Stop"
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "Install the .NET 10 SDK to build." }
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $PSScriptRoot "publish" }
dotnet publish (Join-Path $PSScriptRoot "WindowsDnc.csproj") --configuration $Configuration --runtime win-x64 --self-contained true --output $OutputDirectory --configfile (Join-Path $PSScriptRoot "NuGet.Config") -p:RestoreLockedMode=true -p:DebugType=None -p:DebugSymbols=false -p:PublishTrimmed=false -p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { throw "Publish failed." }
Write-Host (Join-Path $OutputDirectory "C2CDncOpenPreview.exe")
# Preserve redistribution terms beside every self-contained portable build.
foreach ($name in @('LICENSE','LICENSE-STATUS.md','THIRD-PARTY-NOTICES.md','README.md','SECURITY.md')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination (Join-Path $OutputDirectory $name) -Force
}
$notices = Join-Path $OutputDirectory 'ThirdPartyNotices'
New-Item -ItemType Directory -Force $notices | Out-Null
$assets = Get-Content (Join-Path $PSScriptRoot 'obj\project.assets.json') -Raw | ConvertFrom-Json
$packageRoots = @($assets.packageFolders.PSObject.Properties.Name)
$deps = Get-Content (Join-Path $OutputDirectory 'C2CDncOpenPreview.deps.json') -Raw | ConvertFrom-Json
foreach ($library in $deps.libraries.PSObject.Properties.Name) {
    if ($library -notmatch '^(?:runtimepack\.)?(Microsoft\.(?:NETCore|WindowsDesktop)\.App\.Runtime\.win-x64|System\.IO\.Ports)/(.+)$') { continue }
    $id = $Matches[1]; $version = $Matches[2]
    $relative = $id.ToLowerInvariant() + '\' + $version
    $package = $packageRoots | ForEach-Object { Join-Path $_ $relative } | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    if (-not $package) { throw "License source package missing: $id/$version" }
    $target = Join-Path $notices "$id-$version"
    New-Item -ItemType Directory -Force $target | Out-Null
    $files = @(Get-ChildItem -LiteralPath $package -File | Where-Object { $_.Name -match '^(LICENSE(?:\.TXT)?|THIRD-PARTY-NOTICES\.TXT)$' -or $_.Extension -eq '.nuspec' })
    if ($files.Count -eq 0) { throw "No license metadata in $id/$version" }
    foreach ($file in $files) { Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $target $file.Name) -Force }
}
# System.IO.Ports declares MIT in its NuGet manifest and is part of dotnet/runtime.
# Preserve the .NET Foundation runtime license text as well as the package's exact
# third-party notice and original MIT-expression nuspec.
$coreLicense = Get-ChildItem -LiteralPath $notices -Filter LICENSE.TXT -Recurse | Select-Object -First 1
$portsDir = Get-ChildItem -LiteralPath $notices -Directory | Where-Object Name -like 'System.IO.Ports-*' | Select-Object -First 1
if (-not $coreLicense -or -not $portsDir) { throw 'Required .NET/Ports license material missing.' }
Copy-Item -LiteralPath $coreLicense.FullName -Destination (Join-Path $portsDir.FullName 'LICENSE-dotnet-runtime.txt') -Force
Write-Host 'Portable package includes project license and exact runtime/package notices.'
# Additional review-package instructions, separate from optional repository starters.
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'docs\RELEASE-0.9.8-open-preview.2.md') -Destination (Join-Path $OutputDirectory 'RELEASE-NOTES.md') -Force
@"
C2C DNC Open Preview 0.9.8-open-preview.2
Extract the entire ZIP; run C2CDncOpenPreview.exe beside its companion files.
First run: choose the visible Choose verified setup action, review one of the three offline setups, supply your own name and COM port, then Add.
Other machine starters: https://github.com/ggtech3/c2c-dnc-open/tree/main/profiles/starters
Download a JSON file, choose Community configurations > Import contribution file, review settings, enter name and COM port, then Add.
Public downloads need no GitHub account. GitHub issue submissions do.
Import does not connect, transfer, or execute a CNC program. Verify the complete setup before using Send or Receive.
See RELEASE-NOTES.md, LICENSE and ThirdPartyNotices for validation limits and licenses.
"@ | Set-Content (Join-Path $OutputDirectory 'START-HERE.txt')
