param([string] $AssemblyDirectory = 'AlphaRap/bin/x64/Debug', [switch] $CompileOnly)

$ErrorActionPreference = 'Stop'
$designerProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$designerAssemblyRoot = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$designerOutputRoot = Join-Path $designerProjectRoot '.vp-validation/designer-compatibility'
New-Item -ItemType Directory -Path $designerOutputRoot -Force | Out-Null
$designerExecutable = Join-Path $designerOutputRoot 'DesignerCompatibility.Regression.exe'
$designerArguments = @('/nologo', '/target:exe', '/platform:x64', "/out:$designerExecutable", '/r:System.Data.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Xml.Linq.dll')
foreach ($designerAssembly in @('AlphaRap.exe', 'NPSDK.dll', 'NewPowerLibrary.dll')) { $designerArguments += '/r:' + (Join-Path $designerAssemblyRoot $designerAssembly) }
$designerArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' @designerArguments
if ($LASTEXITCODE -ne 0) { throw 'Designer compatibility regression compilation failed.' }
if ($CompileOnly) { Write-Output $designerExecutable; return }
$designerRunDirectory = Join-Path $designerOutputRoot ([Guid]::NewGuid().ToString('N'))
& $designerExecutable $designerAssemblyRoot $designerProjectRoot $designerRunDirectory
if ($LASTEXITCODE -ne 0) { throw "Designer compatibility regressions failed: $designerRunDirectory" }
Write-Output "Designer compatibility screenshots: $designerRunDirectory"
