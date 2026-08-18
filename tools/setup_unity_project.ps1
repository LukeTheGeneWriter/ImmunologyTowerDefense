# Sprint 0: auto-detect the installed Unity Editor and create the project
# skeleton in game/. Run this from PowerShell, from anywhere -- it locates
# the repo root relative to its own location. Safe to re-run: skips
# creation if the project already exists.

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "game"
$logPath = Join-Path $repoRoot "unity_create_log.txt"
$versionMarker = Join-Path $projectPath "ProjectSettings\ProjectVersion.txt"

if (Test-Path $versionMarker) {
    Write-Host "Project already exists at $projectPath (found ProjectVersion.txt) -- nothing to do."
    exit 0
}

$unityExe = Get-ChildItem "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe" -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending | Select-Object -First 1

if (-not $unityExe) {
    Write-Host "Could not find Unity.exe under C:\Program Files\Unity\Hub\Editor\*."
    Write-Host "Open Unity Hub, confirm your installed version and install path, then edit the search path above if it's installed elsewhere."
    exit 1
}

Write-Host "Using Unity Editor: $($unityExe.FullName)"
Write-Host "Creating project at: $projectPath"
Write-Host "This can take a few minutes on first run (Unity imports default packages)..."

& $unityExe.FullName -batchmode -createProject $projectPath -quit -logFile $logPath

# NOTE: $LASTEXITCODE has been unreliable for this Unity version when
# launched this way -- it can come back $null even on a clean run. Check
# the actual marker file instead of trusting the exit code.
if (Test-Path $versionMarker) {
    Write-Host "Success. Project created at $projectPath."
} else {
    Write-Host "ProjectVersion.txt not found -- something went wrong. Check $logPath for details."
}
