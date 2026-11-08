param([switch]$IncludeUntracked)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$argsList = @('-C', $root, 'ls-files')
if ($IncludeUntracked) { $argsList += @('--cached', '--others', '--exclude-standard') }
$paths = @(& git @argsList | Sort-Object -Unique)
if ($LASTEXITCODE -ne 0) { throw 'Cannot list Git source files.' }
if ($paths.Count -eq 0) { throw 'No tracked source files found; use -IncludeUntracked before initial staging.' }
$badPaths = '(^|/)(bin|obj|publish|releases|Developer support|LicenseBuilder|\.venv-specify|__pycache__)(/|$)|(^|/)\.env($|\.)|\.(pfx|p12|pem|key|log)$'
$patterns = @(
    '(?i)-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----',
    '(?i)\bgh[pousr]_[A-Za-z0-9]{20,}\b',
    '\b(?:AKIA|ASIA)[A-Z0-9]{16}\b',
    '(?i)\bxox[baprs]-[A-Za-z0-9-]{20,}\b',
    '(?i)https?://[^/\s:@]+:[^/\s@]+@',
    '(?i)[A-Z]:[\\/]Users[\\/][^\\/\s]+',
    '(?i)(?:api[_-]?key|client[_-]?secret|password)\s*[:=]\s*.?[A-Za-z0-9_+/=-]{24,}'
)
$problems = @()
foreach ($rel in $paths) {
    if ($rel -match $badPaths) { $problems += "$rel : prohibited path"; continue }
    $p = Join-Path $root $rel
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { continue }
    if ([IO.Path]::GetExtension($p) -in @('.png','.ico')) { continue }
    $text = [IO.File]::ReadAllText($p)
    foreach ($pattern in $patterns) {
        if ($text -match $pattern) { $problems += "$rel : sensitive-content pattern match"; break }
    }
}
if ($problems.Count) {
    $problems | ForEach-Object { Write-Output $_ }
    throw "Source hygiene failed for $($problems.Count) files. Review locally; do not print matching secrets."
}
Write-Output "Source hygiene passed for $($paths.Count) files (limited path/pattern checks, not a complete security audit)."
