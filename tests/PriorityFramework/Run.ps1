param([string] $AssemblyDirectory = '.vp-validation/five-build/bin', [string] $ValidationDirectory)
$ErrorActionPreference = 'Stop'
$priorityRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$priorityAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
if (-not $ValidationDirectory) { $ValidationDirectory = Join-Path $priorityRoot '.vp-validation/priority-framework' }
$priorityOutput = Join-Path ([IO.Path]::GetFullPath($ValidationDirectory)) ([Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $priorityOutput -Force | Out-Null
$priorityExecutable = Join-Path $priorityOutput 'PriorityFramework.Regression.exe'
$priorityArguments = @('/nologo','/target:exe','/platform:x64',"/out:$priorityExecutable",'/r:System.Data.dll','/r:System.Drawing.dll','/r:System.Windows.Forms.dll','/r:System.Xml.Linq.dll','/r:Microsoft.CSharp.dll')
foreach ($priorityAssembly in @('AlphaRap.exe','AlphaRap.Framework.dll','NPSDK.dll')) { $priorityArguments += '/r:' + (Join-Path $priorityAssemblies $priorityAssembly) }
foreach ($priorityAssembly in @('Cognex.VisionPro.dll','Cognex.VisionPro.Core.dll','Cognex.VisionPro.ToolGroup.dll')) { $priorityArguments += '/r:C:/Program Files/Cognex/VisionPro/ReferencedAssemblies/' + $priorityAssembly }
$priorityArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' @priorityArguments
if ($LASTEXITCODE -ne 0) { throw 'Priority framework regression compilation failed.' }
& $priorityExecutable $priorityAssemblies $priorityOutput
if ($LASTEXITCODE -ne 0) { throw "Priority framework regression failed: $priorityOutput" }
