param([string] $AssemblyDirectory = '.vp-validation/plc-build/bin')
$ErrorActionPreference = 'Stop'
$plcRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$plcAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$plcOutput = Join-Path $plcRoot ('.vp-validation/plc-tests/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $plcOutput -Force | Out-Null
$plcExecutable = Join-Path $plcOutput 'PlcCommunication.Regression.exe'
$plcArguments = @('/nologo', '/target:exe', '/platform:x64', "/out:$plcExecutable", '/r:System.Core.dll', '/r:System.Data.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll')
foreach ($plcAssembly in @('AlphaRap.Framework.dll', 'NPSDK.dll')) { $plcArguments += '/r:' + (Join-Path $plcAssemblies $plcAssembly) }
foreach ($plcSource in @('PLC/INOVANCE.cs','PLC/PlcSession.cs','PLC/PlcResult.cs','PLC/PlcWorker.cs','Devices/DeviceText.cs')) {
    $plcArguments += Join-Path $plcRoot ('AlphaRap/' + $plcSource)
}
$plcArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' @plcArguments
if ($LASTEXITCODE -ne 0) { throw 'PLC communication regression compilation failed.' }
foreach ($plcDependency in @('AlphaRap.Framework.dll','NPSDK.dll','SunnyUI.dll')) {
    Copy-Item -LiteralPath (Join-Path $plcAssemblies $plcDependency) -Destination $plcOutput
}
foreach ($plcNativeLibrary in @('StandardModbusApi.dll','ModbusTcpAPI.dll')) {
    $plcNativeTarget = Join-Path $plcAssemblies $plcNativeLibrary
    if (-not (Test-Path -LiteralPath $plcNativeTarget)) {
        Copy-Item -LiteralPath (Join-Path $plcRoot ('AlphaRap/bin/x64/Debug/' + $plcNativeLibrary)) -Destination $plcNativeTarget
    }
}
& $plcExecutable $plcAssemblies $plcOutput
if ($LASTEXITCODE -ne 0) { throw "PLC communication regression failed: $plcOutput" }
