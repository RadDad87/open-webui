@echo off
setlocal EnableExtensions EnableDelayedExpansion
set "ROOT=%~dp0..\..\"
set "BACKEND_EXE=%ROOT%runtime\backend\openwebui.exe"
if not exist "%BACKEND_EXE%" (
  echo [ERROR] Missing backend executable: %BACKEND_EXE%
  exit /b 1
)
"%BACKEND_EXE%" serve
