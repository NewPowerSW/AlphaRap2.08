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
$flowArguments += Join-Path $PSScriptRoot 'ObjectMethods.cs'
$flowArguments += Join-Path $PSScriptRoot 'Resources.cs'
$flowArguments += Join-Path $PSScriptRoot 'Usability.cs'
foreach ($flowCognexAssembly in @('Cognex.VisionPro.dll','Cognex.VisionPro.Core.dll','Cognex.VisionPro.ToolGroup.dll')) { $flowArguments += '/r:C:/Program Files/Cognex/VisionPro/ReferencedAssemblies/' + $flowCognexAssembly }
& $flowCompiler @flowArguments
if ($LASTEXITCODE -ne 0) { throw 'Flow regression compilation failed.' }
$flowRunRoot = Join-Path $flowTestRoot ([Guid]::NewGuid().ToString('N'))
& $flowTestExecutable $flowAssemblyRoot $flowRunRoot $flowProjectRoot
if ($LASTEXITCODE -ne 0) { throw 'Flow regressions failed.' }
Write-Output "Flow UI screenshots: $flowRunRoot"
$flowStartupExecutable = Join-Path $flowTestRoot 'FlowProgramming.StartupSmoke.exe'
$flowStartupArguments = $flowArguments | Where-Object { $_ -notlike '/out:*' -and $_ -notlike '*Regression.cs' -and $_ -notlike '*ObjectMethods.cs' -and $_ -notlike '*Resources.cs' -and $_ -notlike '*Usability.cs' }
$flowStartupArguments += "/out:$flowStartupExecutable"
$flowStartupArguments += Join-Path $PSScriptRoot 'StartupSmoke.cs'
& $flowCompiler @flowStartupArguments
if ($LASTEXITCODE -ne 0) { throw 'Startup smoke compilation failed.' }
& $flowStartupExecutable $flowAssemblyRoot (Join-Path $flowRunRoot 'startup') $flowProjectRoot
if ($LASTEXITCODE -ne 0) { throw 'Startup smoke failed.' }
