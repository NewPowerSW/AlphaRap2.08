param([string] $AssemblyDirectory = '.vp-validation/config-build/bin', [string] $ValidationDirectory)
$ErrorActionPreference = 'Stop'
$cfgRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$cfgAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
if (-not $ValidationDirectory) { $ValidationDirectory = Join-Path $cfgRoot '.vp-validation/config-participants' }
$cfgOutput = Join-Path ([IO.Path]::GetFullPath($ValidationDirectory)) ([Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $cfgOutput -Force | Out-Null
$cfgExecutable = Join-Path $cfgOutput 'ConfigurationParticipants.Regression.exe'
$cfgArguments = @('/nologo','/target:exe','/platform:x64',"/out:$cfgExecutable",'/r:System.Data.dll','/r:System.Drawing.dll','/r:System.Windows.Forms.dll','/r:System.Xml.Linq.dll')
foreach ($cfgAssembly in @('AlphaRap.exe','AlphaRap.Framework.dll','NPSDK.dll')) { $cfgArguments += '/r:' + (Join-Path $cfgAssemblies $cfgAssembly) }
$cfgArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' @cfgArguments
if ($LASTEXITCODE -ne 0) { throw 'Configuration participant regression compilation failed.' }
& $cfgExecutable $cfgAssemblies $cfgOutput
if ($LASTEXITCODE -ne 0) { throw "Configuration participant regression failed: $cfgOutput" }
