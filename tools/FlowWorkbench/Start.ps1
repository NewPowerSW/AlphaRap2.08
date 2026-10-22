param([switch] $BuildOnly)
$ErrorActionPreference = 'Stop'
$flowRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$flowOutput = Join-Path $flowRoot '.vp-validation/flow-workbench'
$flowLocator = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
if (-not (Test-Path -LiteralPath $flowLocator)) { throw 'Visual Studio MSBuild / .NET Framework 4.8 targeting pack is required.' }
$flowMsbuild = & $flowLocator -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild/**/Bin/MSBuild.exe' | Select-Object -First 1
if (-not $flowMsbuild) { throw 'MSBuild not found.' }
& $flowMsbuild (Join-Path $PSScriptRoot 'FlowWorkbench.csproj') /t:Build /p:Configuration=Release "/p:OutputPath=$flowOutput/" /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'FlowWorkbench build failed.' }
Write-Output (Join-Path $flowOutput 'FlowWorkbench.exe')
if (-not $BuildOnly) { Start-Process -FilePath (Join-Path $flowOutput 'FlowWorkbench.exe') -WorkingDirectory $flowOutput -WindowStyle Normal }
