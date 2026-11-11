param([string] $AssemblyDirectory = 'AlphaRap/bin/x64/Debug')

$ErrorActionPreference = 'Stop'
$logRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$logAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$logOutput = Join-Path $logRoot ('.vp-validation/runtime-logs/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $logOutput -Force | Out-Null
$logExecutable = Join-Path $logOutput 'RuntimeLogs.Regression.exe'
$logArguments = @('/nologo', '/target:exe', '/platform:x64', "/out:$logExecutable", '/r:System.Data.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Core.dll', '/r:Microsoft.VisualBasic.dll')
foreach ($logAssembly in @('AlphaRap.exe', 'AlphaRap.Framework.dll', 'NPSDK.dll')) { $logArguments += '/r:' + (Join-Path $logAssemblies $logAssembly) }
$logArguments += Join-Path $PSScriptRoot 'Regression.cs'
$logArguments += Join-Path $PSScriptRoot 'Readability.cs'
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' @logArguments
if ($LASTEXITCODE -ne 0) { throw 'Runtime log regression compilation failed.' }
& $logExecutable $logAssemblies $logOutput
if ($LASTEXITCODE -ne 0) { throw "Runtime log checks failed (exit $LASTEXITCODE): $logOutput" }
Write-Output "Runtime log screenshots: $logOutput"
