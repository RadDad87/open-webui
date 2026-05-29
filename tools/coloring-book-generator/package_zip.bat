@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "DIST_DIR=%SCRIPT_DIR%dist"
set "ZIP_PATH=%DIST_DIR%\cbpp-windows.zip"

if not exist "%DIST_DIR%\cbpp.exe" (
  echo cbpp.exe not found. Building first...
  call "%SCRIPT_DIR%build_exe.bat"
  if errorlevel 1 exit /b 1
)

copy /Y "%SCRIPT_DIR%cbpp.bat" "%DIST_DIR%\cbpp.bat" >nul
copy /Y "%SCRIPT_DIR%README.md" "%DIST_DIR%\README.md" >nul
copy /Y "%SCRIPT_DIR%.env.example" "%DIST_DIR%\.env.example" >nul

if exist "%ZIP_PATH%" del /Q "%ZIP_PATH%"

powershell -NoProfile -Command "Compress-Archive -Path '%DIST_DIR%\cbpp.exe','%DIST_DIR%\cbpp.bat','%DIST_DIR%\README.md','%DIST_DIR%\.env.example' -DestinationPath '%ZIP_PATH%'"

if errorlevel 1 (
  echo Failed to create zip package.
  exit /b 1
)

echo.
echo Package created: %ZIP_PATH%
endlocal
