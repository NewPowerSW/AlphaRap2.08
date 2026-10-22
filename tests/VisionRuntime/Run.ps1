param(
    [Parameter(Mandatory = $true)] [string] $AssemblyDirectory
)

$ErrorActionPreference = 'Stop'
$vpProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$vpBuildDirectory = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$vpLibraryDirectory = Join-Path $vpProjectRoot 'AlphaRap/bin/x64/Debug'
$vpCognexDirectory = 'C:/Program Files/Cognex/VisionPro/ReferencedAssemblies'
$vpCompiler = 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$vpExecutable = Join-Path $vpBuildDirectory 'VisionRuntime.Regression.exe'
$vpReferences = @(
    (Join-Path $vpBuildDirectory 'AlphaRap.exe'),
    (Join-Path $vpBuildDirectory 'NPSDK.dll'),
    (Join-Path $vpBuildDirectory 'NewPowerLibrary.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.Core.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.CalibFix.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.ToolGroup.dll')
)
$vpCompilerArgs = @('/nologo', '/platform:x64', '/target:exe', "/out:$vpExecutable", '/r:System.Data.dll', '/r:System.Xml.Linq.dll', '/r:System.Windows.Forms.dll', '/r:System.Drawing.dll')
foreach ($vpReference in $vpReferences) { $vpCompilerArgs += "/r:$vpReference" }
$vpCompilerArgs += Join-Path $PSScriptRoot 'Regression.cs'
$vpCompilerArgs += Join-Path $PSScriptRoot 'Localization.cs'
$vpCompilerArgs += Join-Path $PSScriptRoot 'MiddleLayer.cs'
& $vpCompiler @vpCompilerArgs
if ($LASTEXITCODE -ne 0) { throw 'Regression compilation failed.' }
& $vpExecutable $vpBuildDirectory $vpLibraryDirectory $vpCognexDirectory
if ($LASTEXITCODE -ne 0) { throw 'Vision runtime regressions failed.' }
