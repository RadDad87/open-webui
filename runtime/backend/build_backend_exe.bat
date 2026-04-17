@echo off
setlocal EnableExtensions EnableDelayedExpansion
set "SCRIPT_DIR=%~dp0"
set "ROOT=%SCRIPT_DIR%..\..\"
set "SPEC_DIR=%ROOT%runtime\backend"
set "BACKEND_ENTRY=%ROOT%backend\open_webui\main.py"

if not exist "%BACKEND_ENTRY%" (
  echo [ERROR] Missing backend entrypoint: %BACKEND_ENTRY%
  exit /b 1
)

set "PY_CMD="
where py >nul 2>&1
if not errorlevel 1 set "PY_CMD=py"
if not defined PY_CMD (
  where python >nul 2>&1
  if not errorlevel 1 set "PY_CMD=python"
)
if not defined PY_CMD (
  echo [ERROR] Python launcher not found (py/python).
  exit /b 1
)

%PY_CMD% -m pip install pyinstaller >nul 2>&1
if errorlevel 1 (
  echo [ERROR] PyInstaller not available. Install Python build deps on the build machine.
  exit /b 1
)

%PY_CMD% -m PyInstaller --noconfirm --onefile --name openwebui "%BACKEND_ENTRY%" --distpath "%SPEC_DIR%" --workpath "%SPEC_DIR%\build" --specpath "%SPEC_DIR%"
if errorlevel 1 (
  echo [ERROR] Backend executable build failed.
  exit /b 1
)

echo [OK] Built portable backend executable at %SPEC_DIR%\openwebui.exe
