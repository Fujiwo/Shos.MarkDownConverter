param(
    [string]$ProjectRoot = (Join-Path $PSScriptRoot '..\src\Shos.MarkDownConverter.Web')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$resolvedProjectRoot = [System.IO.Path]::GetFullPath($ProjectRoot)

if (-not (Test-Path -LiteralPath $resolvedProjectRoot -PathType Container)) {
    throw "Project root was not found: $resolvedProjectRoot"
}

$targets = @(
    (Join-Path $resolvedProjectRoot '.python-runtime'),
    (Join-Path $resolvedProjectRoot 'python-runtime'),
    (Join-Path $resolvedProjectRoot 'obj\python-runtime')
)

foreach ($target in $targets) {
    if (-not (Test-Path -LiteralPath $target)) {
        Write-Host "Skip: $target"
        continue
    }

    Write-Host "Remove: $target"
    Remove-Item -LiteralPath $target -Recurse -Force
}

Write-Host 'Python runtime cleanup completed.'