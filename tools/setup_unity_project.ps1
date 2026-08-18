# Sprint 0: auto-detect the installed Unity Editor and create the project
# skeleton in game/. Run this from PowerShell, from anywhere -- it locates
# the repo root relative to its own location.

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "game"
$logPath = Join-Path $repoRoot "unity_create_log.txt"

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

if ($LASTEXITCODE -eq 0) {
    Write-Host "Success. Project created at $projectPath."
    Write-Host "Log: $logPath"
} else {
    Write-Host "Unity exited with code $LASTEXITCODE. Check $logPath for details."
}
