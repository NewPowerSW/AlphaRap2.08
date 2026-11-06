param(
    [string] $OutputDirectory,
    [string] $MSBuildPath
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$framework = Join-Path $projectRoot 'AlphaRap.Framework'
[xml]$versionFile = Get-Content -LiteralPath (Join-Path $framework 'FrameworkVersion.props') -Raw
$version = $versionFile.Project.PropertyGroup.FrameworkVersion
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'FrameworkVersion must have the form major.minor.patch.' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot 'artifacts/framework' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$packagePath = Join-Path $OutputDirectory "AlphaRap.Framework.$version.nupkg"
if (Test-Path -LiteralPath $packagePath) { throw "Framework version already exists at $packagePath." }

if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -find 'MSBuild\**\Bin\amd64\MSBuild.exe' | Select-Object -First 1
}
if (-not $MSBuildPath) { throw '64-bit Visual Studio MSBuild is required.' }

& $MSBuildPath (Join-Path $framework 'AlphaRap.Framework.csproj') /t:Build /m:1 /p:Configuration=Release /p:Platform=x64 /p:UseSharedCompilation=false /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Framework build failed.' }

$frameworkOutput = Join-Path $framework 'bin/x64/Release'
$assembly = Join-Path $frameworkOutput 'AlphaRap.Framework.dll'
if ((Get-Item -LiteralPath $assembly).VersionInfo.ProductVersion -ne $version) {
    throw "Framework assembly version does not match package version $version."
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$packProject = Join-Path $PSScriptRoot 'FrameworkPackage.csproj'
& dotnet restore $packProject --source $OutputDirectory '/p:NuGetAudit=false'
if ($LASTEXITCODE -ne 0) { throw 'Packaging project restore failed.' }
& dotnet pack (Join-Path $PSScriptRoot 'FrameworkPackage.csproj') --no-build --configuration Release --output $OutputDirectory "/p:FrameworkOutput=$frameworkOutput" '/p:NuGetAudit=false' "/p:RestoreSources=$OutputDirectory"
if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $packagePath)) { throw 'Framework packaging failed.' }

$hash = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash
Set-Content -LiteralPath "$packagePath.sha256" -Value "$hash  $([IO.Path]::GetFileName($packagePath))" -Encoding ascii
Get-Item -LiteralPath $packagePath
