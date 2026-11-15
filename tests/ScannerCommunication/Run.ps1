param([string] $AssemblyDirectory = '.vp-validation/scanner-build/bin')
$ErrorActionPreference = 'Stop'
$scannerRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$scannerAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$scannerOutput = Join-Path $scannerRoot ('.vp-validation/scanner-tests/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scannerOutput -Force | Out-Null
$scannerExecutable = Join-Path $scannerOutput 'ScannerCommunication.Regression.exe'
$scannerArguments = @('/nologo', '/target:exe', '/platform:x64', "/out:$scannerExecutable", '/r:System.Core.dll', '/r:System.Data.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Xml.Linq.dll')
foreach ($scannerAssembly in @('AlphaRap.exe','AlphaRap.Framework.dll','NPSDK.dll')) { $scannerArguments += '/r:' + (Join-Path $scannerAssemblies $scannerAssembly) }
$scannerArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/Roslyn/csc.exe' @scannerArguments
if ($LASTEXITCODE -ne 0) { throw 'Scanner regression compilation failed.' }
& $scannerExecutable $scannerAssemblies $scannerOutput
if ($LASTEXITCODE -ne 0) { throw "Scanner communication regression failed: $scannerOutput" }
