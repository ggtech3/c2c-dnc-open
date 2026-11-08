param([ValidateSet("Release", "Debug")][string]$Configuration = "Release", [string]$OutputDirectory = "")
$ErrorActionPreference = "Stop"
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "Install the .NET 10 SDK to build." }
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $PSScriptRoot "publish" }
dotnet publish (Join-Path $PSScriptRoot "WindowsDnc.csproj") --configuration $Configuration --runtime win-x64 --self-contained true --output $OutputDirectory --configfile (Join-Path $PSScriptRoot "NuGet.Config") -p:RestoreLockedMode=true -p:DebugType=None -p:DebugSymbols=false -p:PublishTrimmed=false -p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { throw "Publish failed." }
Write-Host (Join-Path $OutputDirectory "C2CDncOpenPreview.exe")
