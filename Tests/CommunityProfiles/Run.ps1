$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'CommunityProfiles.csproj'
dotnet restore $project --locked-mode
if ($LASTEXITCODE -ne 0) { throw "Locked dependency restore failed: $LASTEXITCODE" }
dotnet run --no-restore --project $project --configuration Release
if ($LASTEXITCODE -ne 0) { throw "Community profile checks failed: $LASTEXITCODE" }
