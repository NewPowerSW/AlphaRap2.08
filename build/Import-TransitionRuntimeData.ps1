param([Parameter(Mandatory = $true)][string]$SourceDirectory)

$ErrorActionPreference = 'Stop'
$source = (Resolve-Path -LiteralPath $SourceDirectory).Path
$project = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../AlphaRap')).Path
$seed = Join-Path $project 'RuntimeData.local'
foreach ($required in 'MachineSetup.ini', 'SystemData.mdb', 'ModuleData/RecipeData', 'ModuleData/SettingData') {
    if (!(Test-Path -LiteralPath (Join-Path $source $required))) { throw "Missing runtime data: $required" }
}
if (Test-Path -LiteralPath $seed) { throw "A local snapshot already exists: $seed" }

$staging = Join-Path $project ('RuntimeData.local.' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $staging | Out-Null
$files = @(Get-ChildItem -File -LiteralPath $source | Where-Object {
    $_.Extension -in '.ini', '.eni' -or $_.Name -in 'SystemData.mdb', 'SpareList.xml'
})
foreach ($folder in 'ModuleData', 'VisionData', 'ProductData', 'MotorPara', 'leishai', 'LanguageData', 'AlarmTable') {
    $path = Join-Path $source $folder
    if (Test-Path -LiteralPath $path) {
        $files += @(Get-ChildItem -LiteralPath $path -File -Recurse | Where-Object {
            $_.Extension -notin '.bak', '.tmp', '.ldb', '.laccdb' -and $_.Name -ne 'FlowSourceGeneration.xml'
        })
    }
}
foreach ($file in $files) {
    $relative = $file.FullName.Substring($source.TrimEnd('\').Length + 1)
    $destination = Join-Path $staging $relative
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination)) | Out-Null
    [IO.File]::Copy($file.FullName, $destination, $false)
    if ((Get-FileHash -LiteralPath $destination).Hash -ne (Get-FileHash -LiteralPath $file.FullName).Hash) {
        throw "Source changed during import: $relative"
    }
}
[IO.Directory]::Move($staging, $seed)
Write-Output ("Imported {0} runtime files to {1}" -f $files.Count, $seed)
