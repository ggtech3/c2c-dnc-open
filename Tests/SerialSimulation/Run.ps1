$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$alias = 'using SerialPort = SerialSimulation.FakeSerialPort;' + [char]10
$manifest = @()
foreach ($name in @('SerialDncService.cs', 'DncTextProcessor.cs', 'CncProfile.cs')) {
    $source = Join-Path $root $name
    $target = Join-Path $PSScriptRoot $name
    if ($name -eq 'SerialDncService.cs') {
        [IO.File]::WriteAllText($target, $alias + [IO.File]::ReadAllText($source))
        if ([IO.File]::ReadAllText($target).Substring($alias.Length) -cne [IO.File]::ReadAllText($source)) { throw 'Transform integrity failed' }
    } else { Copy-Item -LiteralPath $source -Destination $target -Force }
    $manifest += [pscustomobject]@{file=$name; sourceSHA256=(Get-FileHash $source).Hash; testSHA256=(Get-FileHash $target).Hash; transform=$(if($name -eq 'SerialDncService.cs'){'Prepend exactly: using SerialPort = SerialSimulation.FakeSerialPort; followed by LF. No other changes.'}else{'Byte-identical copy'})}
}
$manifest | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'source-manifest.json')
dotnet restore (Join-Path $PSScriptRoot 'SerialSimulation.csproj') --locked-mode
if ($LASTEXITCODE -ne 0) { throw "Locked dependency restore failed: $LASTEXITCODE" }
dotnet run --no-restore --project (Join-Path $PSScriptRoot 'SerialSimulation.csproj') --configuration Release 2>&1 | Tee-Object (Join-Path $PSScriptRoot 'results.txt')
if ($LASTEXITCODE -ne 0) { throw "Tests failed: $LASTEXITCODE" }
