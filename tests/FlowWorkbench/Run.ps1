$ErrorActionPreference = 'Stop'
$flowRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$flowOutput = Join-Path $flowRoot '.vp-validation/flow-workbench'
$flowLocator = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$flowMsbuild = & $flowLocator -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild/**/Bin/MSBuild.exe' | Select-Object -First 1
if (-not $flowMsbuild) { throw 'MSBuild not found.' }
& $flowMsbuild (Join-Path $PSScriptRoot 'FlowWorkbench.Regression.csproj') /t:Build /p:Configuration=Release "/p:OutputPath=$flowOutput/" /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Regression build failed.' }
& (Join-Path $flowOutput 'FlowWorkbench.Regression.exe') $flowOutput
if ($LASTEXITCODE -ne 0) { throw 'FlowWorkbench regressions failed.' }
