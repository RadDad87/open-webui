@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "EXE=%SCRIPT_DIR%dist\cbpp.exe"
set "PY_SCRIPT=%SCRIPT_DIR%generator.py"

REM Single-entry launcher:
REM 1) If packaged EXE exists, run it.
REM 2) Otherwise fall back to Python script.
if exist "%EXE%" (
  "%EXE%" %*
  exit /b %ERRORLEVEL%
)

if exist "%PY_SCRIPT%" (
  python "%PY_SCRIPT%" %*
  exit /b %ERRORLEVEL%
)

echo ERROR: Neither "%EXE%" nor "%PY_SCRIPT%" was found.
exit /b 1
