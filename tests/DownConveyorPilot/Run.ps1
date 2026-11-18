param([string]$AssemblyDirectory = '.vp-validation/down-conveyor-build/bin')
$ErrorActionPreference = 'Stop'
$pilotAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$pilotWorkspace = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$pilotOutput = Join-Path $pilotWorkspace ('.vp-validation/down-conveyor-pilot/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $pilotOutput -Force | Out-Null
$pilotExe = Join-Path $pilotOutput 'DownConveyorPilot.exe'
$pilotCompilerArguments = @('/nologo', '/target:exe', '/platform:x64', "/out:$pilotExe", '/r:System.Core.dll', '/r:System.Windows.Forms.dll')
foreach ($pilotAssembly in @('AlphaRap.exe', 'AlphaRap.Framework.dll', 'NPSDK.dll')) {
    $pilotSource = Join-Path $pilotAssemblies $pilotAssembly
    Copy-Item -LiteralPath $pilotSource -Destination $pilotOutput
    $pilotCompilerArguments += '/r:' + $pilotSource
}
$pilotCompilerArguments += Join-Path $PSScriptRoot 'Regression.cs'
& "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" @pilotCompilerArguments
if ($LASTEXITCODE -ne 0) { throw 'Down conveyor pilot compilation failed.' }
Push-Location -LiteralPath $pilotOutput
try {
    & $pilotExe $pilotOutput
    if ($LASTEXITCODE -ne 0) { throw 'Down conveyor pilot failed.' }
}
finally { Pop-Location }
