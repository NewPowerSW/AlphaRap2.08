param([string]$AssemblyDirectory = 'SDK/NPSDK/bin/x64/Debug', [int]$Repeat = 1)
$ErrorActionPreference = 'Stop'
$assemblyRoot = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$output = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../.vp-validation/flow-legacy-execution'))
New-Item -ItemType Directory -Path $output -Force | Out-Null
Copy-Item (Join-Path $assemblyRoot 'NPSDK.dll') $output -Force
& "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /target:exe /platform:x64 "/out:$output/FlowLegacyExecution.exe" "/r:$output/NPSDK.dll" /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'Regression.cs')
if ($LASTEXITCODE -ne 0) { throw 'Legacy execution regression compilation failed.' }
for ($iteration = 0; $iteration -lt $Repeat; $iteration++) {
    & (Join-Path $output 'FlowLegacyExecution.exe')
    if ($LASTEXITCODE -ne 0) { throw "Legacy execution regression failed at iteration $iteration." }
}
