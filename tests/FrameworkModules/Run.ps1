param([string] $AssemblyDirectory = '.vp-validation/module-build/bin')
$ErrorActionPreference = 'Stop'
$moduleRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$moduleAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$moduleOutput = Join-Path $moduleRoot ('.vp-validation/framework-modules/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $moduleOutput -Force | Out-Null
$moduleExecutable = Join-Path $moduleOutput 'FrameworkModules.Regression.exe'
$moduleArguments = @('/nologo','/target:exe','/platform:x64',"/out:$moduleExecutable",'/r:System.Data.dll','/r:System.Drawing.dll','/r:System.Windows.Forms.dll','/r:System.Xml.Linq.dll')
foreach ($moduleAssembly in @('AlphaRap.exe','AlphaRap.Framework.dll','NPSDK.dll')) { $moduleArguments += '/r:' + (Join-Path $moduleAssemblies $moduleAssembly) }
$moduleArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' @moduleArguments
if ($LASTEXITCODE -ne 0) { throw 'Framework module regression compilation failed.' }
& $moduleExecutable $moduleAssemblies $moduleOutput
if ($LASTEXITCODE -ne 0) { throw "Framework module regression failed: $moduleOutput" }
