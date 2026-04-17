@echo off
setlocal EnableExtensions EnableDelayedExpansion
set "SCRIPT_DIR=%~dp0"
set "ROOT=%SCRIPT_DIR%..\..\"
set "VALHALLA_EXE=%ROOT%runtime\routing\valhalla_service.exe"
set "VALHALLA_CFG=%ROOT%runtime\routing\valhalla.json"
set "VALHALLA_TILES_DIR=%ROOT%data\geo\tiles"
if not exist "%VALHALLA_EXE%" (
  echo [WARN] Missing Valhalla executable: %VALHALLA_EXE%
  echo [INFO] Routing is disabled; continuing in degraded mode.
  exit /b 0
)
if not exist "%VALHALLA_CFG%" (
  echo [WARN] Missing Valhalla config: %VALHALLA_CFG%
  echo [INFO] Routing is disabled; continuing in degraded mode.
  exit /b 0
)
if not exist "%VALHALLA_TILES_DIR%" (
  echo [WARN] Missing Valhalla tiles directory: %VALHALLA_TILES_DIR%
  echo [INFO] Routing is disabled; continuing in degraded mode.
  exit /b 0
)
"%VALHALLA_EXE%" "%VALHALLA_CFG%" 1
