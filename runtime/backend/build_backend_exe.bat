@echo off
setlocal EnableExtensions EnableDelayedExpansion
set "ROOT=%~dp0..\..\"
set "SPEC_DIR=%ROOT%runtime\backend"
set "BACKEND_ENTRY=%ROOT%backend\open_webui\main.py"

if not exist "%BACKEND_ENTRY%" (
  echo [ERROR] Missing backend entrypoint: %BACKEND_ENTRY%
  exit /b 1
)

py -m pip install pyinstaller >nul 2>&1
if errorlevel 1 (
  echo [ERROR] PyInstaller not available. Install Python build deps on the build machine.
  exit /b 1
)

py -m PyInstaller --noconfirm --onefile --name openwebui "%BACKEND_ENTRY%" --distpath "%SPEC_DIR%" --workpath "%SPEC_DIR%\build" --specpath "%SPEC_DIR%"
if errorlevel 1 (
  echo [ERROR] Backend executable build failed.
  exit /b 1
)

echo [OK] Built portable backend executable at %SPEC_DIR%\openwebui.exe
