@echo off
setlocal EnableExtensions EnableDelayedExpansion
set "SCRIPT_DIR=%~dp0"
set "ROOT=%SCRIPT_DIR%..\..\"

set "BACKEND_EXE=%ROOT%runtime\backend\openwebui.exe"
set "BACKEND_MAIN=%ROOT%backend\open_webui\main.py"

if exist "%BACKEND_EXE%" (
  "%BACKEND_EXE%" serve
  exit /b %errorlevel%
)

echo [WARN] Bundled backend executable not found: %BACKEND_EXE%
echo [INFO] Falling back to Python backend startup.

if not exist "%BACKEND_MAIN%" (
  echo [ERROR] Missing Python backend entrypoint: %BACKEND_MAIN%
  exit /b 1
)

pushd "%ROOT%backend" >nul 2>&1
if errorlevel 1 (
  echo [ERROR] Unable to enter backend directory: %ROOT%backend
  exit /b 1
)

where py >nul 2>&1
if not errorlevel 1 (
  py -m uvicorn open_webui.main:app --host 0.0.0.0 --port 8080 --forwarded-allow-ips "*"
  set "RC=%errorlevel%"
  popd >nul 2>&1
  exit /b %RC%
)

where python >nul 2>&1
if not errorlevel 1 (
  python -m uvicorn open_webui.main:app --host 0.0.0.0 --port 8080 --forwarded-allow-ips "*"
  set "RC=%errorlevel%"
  popd >nul 2>&1
  exit /b %RC%
)

popd >nul 2>&1
echo [ERROR] Python launcher not available (py/python not found in PATH).
exit /b 1
