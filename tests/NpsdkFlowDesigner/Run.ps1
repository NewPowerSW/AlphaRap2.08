param([string] $OutputDirectory = '.vp-validation/npsdk-designer-tests')
$ErrorActionPreference = 'Stop'
$designerRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$designerOutput = [IO.Path]::GetFullPath((Join-Path $designerRoot $OutputDirectory))
New-Item -ItemType Directory -Path $designerOutput -Force | Out-Null
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe' (Join-Path $designerRoot 'tools/NpsdkFlowDesigner/NpsdkFlowDesigner.csproj') /t:Build "/p:OutputPath=$designerOutput/" /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Designer build failed.' }
$designerCompiler = 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $designerCompiler /nologo /target:exe /platform:x64 "/out:$designerOutput/Regression.exe" /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Xml.Linq.dll "/r:$designerOutput/NPSDK.dll" "/r:$designerOutput/NPSDK.FlowDesigner.dll" (Join-Path $PSScriptRoot 'Regression.cs')
if ($LASTEXITCODE -ne 0) { throw 'Designer regression compilation failed.' }
& (Join-Path $designerOutput 'Regression.exe') $designerOutput
if ($LASTEXITCODE -ne 0) { throw 'Designer regressions failed.' }
