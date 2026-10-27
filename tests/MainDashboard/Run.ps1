param([string] $AssemblyDirectory = '.vp-validation/dashboard-redesign-build/bin')

$ErrorActionPreference = 'Stop'
$dashboardRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$dashboardAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$dashboardOutput = Join-Path $dashboardRoot ('.vp-validation/main-dashboard/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $dashboardOutput -Force | Out-Null
$dashboardExecutable = Join-Path $dashboardOutput 'MainDashboard.Regression.exe'
$dashboardArguments = @('/nologo', '/target:exe', '/platform:x64', "/out:$dashboardExecutable", '/r:System.Data.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Xml.Linq.dll')
foreach ($dashboardAssembly in @('AlphaRap.exe', 'NPSDK.dll', 'NewPowerLibrary.dll')) { $dashboardArguments += '/r:' + (Join-Path $dashboardAssemblies $dashboardAssembly) }
$dashboardArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' @dashboardArguments
if ($LASTEXITCODE -ne 0) { throw 'Dashboard probe compilation failed.' }
& $dashboardExecutable $dashboardAssemblies $dashboardRoot $dashboardOutput
if ($LASTEXITCODE -ne 0) { throw "Dashboard checks failed: $dashboardOutput" }
Write-Output "Dashboard screenshots: $dashboardOutput"
