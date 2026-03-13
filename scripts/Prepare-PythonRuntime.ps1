param(
    [Parameter(Mandatory = $true)]
    [string]$RuntimeDir,

    [Parameter(Mandatory = $true)]
    [string]$RequirementsFile,

    [string]$PythonCommand = "python"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (-not (Test-Path -LiteralPath $RequirementsFile -PathType Leaf)) {
    throw "Requirements file was not found: $RequirementsFile"
}

Write-Host "Preparing publish Python runtime at $RuntimeDir"

if (Test-Path -LiteralPath $RuntimeDir) {
    Remove-Item -LiteralPath $RuntimeDir -Recurse -Force
}

$runtimeParent = Split-Path -Parent $RuntimeDir
if (-not [string]::IsNullOrWhiteSpace($runtimeParent)) {
    New-Item -ItemType Directory -Path $runtimeParent -Force | Out-Null
}

& $PythonCommand -m venv $RuntimeDir
if ($LASTEXITCODE -ne 0) {
    throw "Failed to create Python runtime with command '$PythonCommand'."
}

$runtimePython = Join-Path $RuntimeDir "Scripts\python.exe"
if (-not (Test-Path -LiteralPath $runtimePython -PathType Leaf)) {
    throw "Python executable was not created: $runtimePython"
}

& $runtimePython -m pip install -r $RequirementsFile
if ($LASTEXITCODE -ne 0) {
    throw "Failed to install Python dependencies from $RequirementsFile"
}

Write-Host "Python runtime is ready: $runtimePython"