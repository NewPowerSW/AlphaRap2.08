param(
    [Parameter(Mandatory = $true)] [string] $AssemblyDirectory,
    [switch] $BuiltInOnly
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
    (Join-Path $vpBuildDirectory 'AlphaRap.Framework.dll'),
    (Join-Path $vpBuildDirectory 'NPSDK.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.Core.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.Controls.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.CalibFix.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.Caliper.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.Dimensioning.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.Display.Controls.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.PMAlign.dll'),
    (Join-Path $vpCognexDirectory 'Cognex.VisionPro.ToolGroup.dll')
)
$vpCompilerArgs = @('/nologo', '/platform:x64', '/target:exe', "/out:$vpExecutable", '/r:System.Data.dll', '/r:System.Xml.Linq.dll', '/r:System.Windows.Forms.dll', '/r:System.Drawing.dll')
foreach ($vpReference in $vpReferences) { $vpCompilerArgs += "/r:$vpReference" }
$vpCompilerArgs += Join-Path $PSScriptRoot 'Regression.cs'
$vpCompilerArgs += Join-Path $PSScriptRoot 'BuiltInAlgorithms.cs'
$vpCompilerArgs += Join-Path $PSScriptRoot 'PrecisionCalibration.cs'
$vpCompilerArgs += Join-Path $PSScriptRoot 'RotationWorkflow.cs'
$vpCompilerArgs += Join-Path $PSScriptRoot 'Localization.cs'
$vpCompilerArgs += Join-Path $PSScriptRoot 'MiddleLayer.cs'
foreach ($vpFixture in @('CalibrationV1', 'PositioningV1', 'CalibrationV2', 'CalibrationV3')) {
    $vpCompilerArgs += "/resource:$(Join-Path $PSScriptRoot "Fixtures/$vpFixture.vpp"),Fixtures.$vpFixture.vpp"
}
& $vpCompiler @vpCompilerArgs
if ($LASTEXITCODE -ne 0) { throw 'Regression compilation failed.' }
$vpRuntimeArguments = @($vpBuildDirectory, $vpLibraryDirectory, $vpCognexDirectory)
if ($BuiltInOnly) { $vpRuntimeArguments += '--built-in-only' }
$vpWorkingDirectory = if ($BuiltInOnly) { Join-Path $vpBuildDirectory 'vision-test-work' } else { (Get-Location).Path }
New-Item -ItemType Directory -Path $vpWorkingDirectory -Force | Out-Null
Push-Location -LiteralPath $vpWorkingDirectory
try {
    & $vpExecutable @vpRuntimeArguments
    if ($LASTEXITCODE -ne 0) { throw 'Vision runtime regressions failed.' }
}
finally { Pop-Location }
