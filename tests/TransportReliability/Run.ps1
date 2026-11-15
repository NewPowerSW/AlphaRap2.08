param([string] $AssemblyDirectory = '.vp-validation/priority-build/bin')
$ErrorActionPreference = 'Stop'
$reliabilityRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$reliabilityAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$reliabilityOutput = Join-Path $reliabilityRoot ('.vp-validation/transport-tests/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $reliabilityOutput -Force | Out-Null
$reliabilityExe = Join-Path $reliabilityOutput 'TransportReliability.Regression.exe'
$reliabilityArguments = @('/nologo','/target:exe','/platform:x64','/main:ReliabilityProbe',"/out:$reliabilityExe",'/r:System.Core.dll','/r:System.Data.dll','/r:System.Drawing.dll','/r:System.Windows.Forms.dll','/r:System.Xml.Linq.dll')
foreach($reliabilityLibrary in @('AlphaRap.exe','AlphaRap.Framework.dll','NPSDK.dll')) { $reliabilityArguments += '/r:' + (Join-Path $reliabilityAssemblies $reliabilityLibrary) }
$reliabilityArguments += Join-Path $reliabilityRoot 'tests/ScannerCommunication/Regression.cs'
$reliabilityArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/Roslyn/csc.exe' @reliabilityArguments
if($LASTEXITCODE -ne 0) { throw 'Transport regression compilation failed.' }
& $reliabilityExe $reliabilityAssemblies $reliabilityOutput
if($LASTEXITCODE -ne 0) { throw "Transport regression failed: $reliabilityOutput" }
