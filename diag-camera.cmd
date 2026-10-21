@echo off
setlocal
cd /d "%~dp0"
set OUT=%~dp0diag-camera.txt
if exist "%OUT%" del "%OUT%"

echo ================ 64-bit process ================>> "%OUT%"
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%~dp0diag-camera.ps1" %1 >> "%OUT%" 2>&1
echo.>> "%OUT%"
echo ================ 32-bit process ================>> "%OUT%"
"%SystemRoot%\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%~dp0diag-camera.ps1" %1 >> "%OUT%" 2>&1

type "%OUT%"
echo.
echo ---------------------------------------------------------------
echo Result saved to: %OUT%
echo Send this file back, it shows what VisionPro reports for both
echo 32-bit and 64-bit processes plus what AlphaRap itself sees.
echo ---------------------------------------------------------------
pause