$ErrorActionPreference = 'Stop'
$flowProject = Join-Path $PSScriptRoot 'NpsdkFlowDesigner.csproj'
$flowBuildTool = 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe'
if (!(Test-Path -LiteralPath $flowBuildTool)) {
    $flowVswhere = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
    if (Test-Path -LiteralPath $flowVswhere) { $flowBuildTool = & $flowVswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild/**/Bin/MSBuild.exe' | Select-Object -First 1 }
}
if (!$flowBuildTool -or !(Test-Path -LiteralPath $flowBuildTool)) { throw 'Install Visual Studio MSBuild and the .NET Framework 4.8 targeting pack.' }
& $flowBuildTool $flowProject /t:Build /p:Configuration=Debug /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Flow_Chart designer build failed.' }
# 打开交互式离线设计器，由使用者拖拽、配置和试运行示例。
Start-Process -FilePath (Join-Path $PSScriptRoot 'bin/Debug/NpsdkFlowDesigner.exe') -WorkingDirectory (Join-Path $PSScriptRoot 'bin/Debug') -WindowStyle Normal
