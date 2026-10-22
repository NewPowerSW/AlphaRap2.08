$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Start.ps1') -BuildOnly
$flowRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$flowOutput = Join-Path $flowRoot '.vp-validation/flow-workbench'
$flowPackage = Join-Path $flowRoot '.vp-validation/FlowWorkbench-Test.zip'
$flowFiles = @(
    (Join-Path $flowOutput 'FlowWorkbench.exe'),
    (Join-Path $flowOutput 'AlphaRap.FlowRuntime.dll'),
    (Join-Path $flowOutput 'LanguageData'),
    (Join-Path $flowOutput 'README.md')
)
Compress-Archive -LiteralPath $flowFiles -DestinationPath $flowPackage -Force
Write-Output $flowPackage
