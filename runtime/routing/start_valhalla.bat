@echo off
setlocal EnableExtensions EnableDelayedExpansion
set "ROOT=%~dp0..\..\"
set "VALHALLA_EXE=%ROOT%runtime\routing\valhalla_service.exe"
set "VALHALLA_CFG=%ROOT%runtime\routing\valhalla.json"
if not exist "%VALHALLA_EXE%" (
  echo [ERROR] Missing Valhalla executable: %VALHALLA_EXE%
  exit /b 1
)
if not exist "%VALHALLA_CFG%" (
  echo [ERROR] Missing Valhalla config: %VALHALLA_CFG%
  exit /b 1
)
"%VALHALLA_EXE%" "%VALHALLA_CFG%" 1
