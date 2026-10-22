param([string] $AssemblyDirectory = 'AlphaRap/bin/x64/Debug')
$ErrorActionPreference = 'Stop'
$flowProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$flowAssemblyRoot = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$flowTestRoot = Join-Path $flowProjectRoot '.vp-validation/flow-program-tests'
New-Item -ItemType Directory -Path $flowTestRoot -Force | Out-Null
$flowTestExecutable = Join-Path $flowTestRoot 'FlowProgramming.Regression.exe'
$flowCompiler = 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$flowArguments = @('/nologo', '/target:exe', '/platform:x64', "/out:$flowTestExecutable", '/r:System.Data.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Xml.Linq.dll')
foreach ($flowAssembly in @('AlphaRap.exe', 'NPSDK.dll', 'NewPowerLibrary.dll')) { $flowArguments += '/r:' + (Join-Path $flowAssemblyRoot $flowAssembly) }
$flowArguments += Join-Path $PSScriptRoot 'Regression.cs'
& $flowCompiler @flowArguments
if ($LASTEXITCODE -ne 0) { throw 'Flow regression compilation failed.' }
$flowRunRoot = Join-Path $flowTestRoot ([Guid]::NewGuid().ToString('N'))
& $flowTestExecutable $flowAssemblyRoot $flowRunRoot $flowProjectRoot
if ($LASTEXITCODE -ne 0) { throw 'Flow regressions failed.' }
Write-Output "Flow UI screenshots: $flowRunRoot"
