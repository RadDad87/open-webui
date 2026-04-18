@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM Z3R4H portable launcher (offline, runtime-folder only)

REM --- Config ---
set "PACKAGE_ROOT=%~dp0"
set "RUNTIME_CONFIG_FILE=%PACKAGE_ROOT%runtime\runtime_config.env"

if exist "%RUNTIME_CONFIG_FILE%" (
	for /f "usebackq tokens=1,* delims==" %%A in ("%RUNTIME_CONFIG_FILE%") do (
		set "cfg_key=%%~A"
		set "cfg_val=%%~B"
		if defined cfg_key if not "!cfg_key:~0,1!"=="#" if not "!cfg_key!"=="" set "!cfg_key!=!cfg_val!"
	)
)

if not defined LLAMA_SERVER_EXE set "LLAMA_SERVER_EXE=runtime\llama.cpp\llama-server.exe"
if not defined LLAMA_MODEL_PATH set "LLAMA_MODEL_PATH=models\model.gguf"
if not defined BACKEND_CMD set "BACKEND_CMD=runtime\backend\start_backend.bat"
if not defined VALHALLA_CMD set "VALHALLA_CMD=runtime\routing\start_valhalla.bat"

if not defined BACKEND_URL set "BACKEND_URL=http://localhost:8080"
if not defined BACKEND_HEALTH_URL set "BACKEND_HEALTH_URL=%BACKEND_URL%/health"
if not defined VALHALLA_HEALTH_URL set "VALHALLA_HEALTH_URL=http://127.0.0.1:8002/status"
if not defined GEO_HEALTH_URL set "GEO_HEALTH_URL=%BACKEND_URL%/api/geo/health"
if not defined BROWSER_URL set "BROWSER_URL=%BACKEND_URL%/maps"

if not defined RUNTIME_LOGS_DIR set "RUNTIME_LOGS_DIR=logs"
if not defined RUNTIME_CACHE_DIR set "RUNTIME_CACHE_DIR=cache"
if not defined RUNTIME_DATA_DIR set "RUNTIME_DATA_DIR=data\geo"
if not defined RUNTIME_DEM_DIR set "RUNTIME_DEM_DIR=data\geo\dem"

call :resolve_path LLAMA_SERVER_EXE
call :resolve_path LLAMA_MODEL_PATH
call :resolve_path BACKEND_CMD
call :resolve_path VALHALLA_CMD
call :resolve_path RUNTIME_LOGS_DIR
call :resolve_path RUNTIME_CACHE_DIR
call :resolve_path RUNTIME_DATA_DIR
call :resolve_path RUNTIME_DEM_DIR

if not exist "%RUNTIME_LOGS_DIR%" mkdir "%RUNTIME_LOGS_DIR%" >nul 2>&1
if not exist "%RUNTIME_CACHE_DIR%" mkdir "%RUNTIME_CACHE_DIR%" >nul 2>&1
if not exist "%RUNTIME_DATA_DIR%" mkdir "%RUNTIME_DATA_DIR%" >nul 2>&1
if not exist "%RUNTIME_DEM_DIR%" mkdir "%RUNTIME_DEM_DIR%" >nul 2>&1

set "LOG_FILE=%RUNTIME_LOGS_DIR%\z3r4h_launcher.log"
set "LLAMA_HOST=127.0.0.1"
set "LLAMA_PORT=11434"
set "OPENAI_API_BASE_URLS=http://127.0.0.1:%LLAMA_PORT%/v1"

REM Offline hardening
set "ENABLE_OPENAI_API=True"
set "OPENAI_API_KEYS="
set "OFFLINE_MODE=True"
set "WEBUI_AUTH=False"
set "ENABLE_LOGIN_FORM=False"
set "ENABLE_SIGNUP=False"
set "ENABLE_COMMUNITY_SHARING=False"
set "ENABLE_WEB_SEARCH=False"

REM --- Timeouts ---
set "LLAMA_TIMEOUT=120"
set "VALHALLA_TIMEOUT=120"
set "BACKEND_TIMEOUT=180"
set "GEO_TIMEOUT=180"
set "POLL_SECONDS=2"

call :log "========================================"
call :log "Z3R4H portable launcher start"
call :log "package root: %PACKAGE_ROOT%"
call :log "runtime config: %RUNTIME_CONFIG_FILE%"
call :log "llama: %LLAMA_SERVER_EXE%"
call :log "backend cmd: %BACKEND_CMD%"
call :log "routing cmd: %VALHALLA_CMD%"
call :log "browser: %BROWSER_URL%"
call :log "========================================"

if /I not "%LLAMA_HOST%"=="127.0.0.1" (
	call :fail "LLAMA_HOST must be 127.0.0.1 (localhost-only)."
	exit /b 1
)

echo %BROWSER_URL% | findstr /I /R "^http://localhost[:/]" >nul
if errorlevel 1 (
	call :fail "BROWSER_URL must be localhost-only (http://localhost:...)."
	exit /b 1
)

if not exist "%LLAMA_SERVER_EXE%" (
	call :fail "Missing bundled llama executable: %LLAMA_SERVER_EXE%"
	exit /b 1
)
if not exist "%LLAMA_MODEL_PATH%" (
	call :fail "Missing bundled model file: %LLAMA_MODEL_PATH%"
	exit /b 1
)
if not exist "%BACKEND_CMD%" (
	call :fail "Missing bundled backend launcher: %BACKEND_CMD%"
	exit /b 1
)
set "ROUTING_ENABLED=1"
if not exist "%VALHALLA_CMD%" (
	call :log "WARN: Missing bundled routing launcher: %VALHALLA_CMD%"
	call :log "WARN: Continuing startup in degraded/basic mode."
	set "ROUTING_ENABLED=0"
)

call :check_port_free "%LLAMA_PORT%" "llama.cpp"
if errorlevel 1 exit /b 1

call :log "Starting llama.cpp..."
start "Z3R4H llama.cpp" /MIN cmd /c ""%LLAMA_SERVER_EXE%" -m "%LLAMA_MODEL_PATH%" --host %LLAMA_HOST% --port %LLAMA_PORT% >> "%LOG_FILE%" 2>&1"
call :wait_url "http://%LLAMA_HOST%:%LLAMA_PORT%/health" "%LLAMA_TIMEOUT%" "llama.cpp"
if errorlevel 1 exit /b 1

if "%ROUTING_ENABLED%"=="1" (
	call :log "Starting Valhalla routing..."
	start "Z3R4H Valhalla" /MIN cmd /c ""%VALHALLA_CMD%" >> "%LOG_FILE%" 2>&1"
	call :wait_url "%VALHALLA_HEALTH_URL%" "%VALHALLA_TIMEOUT%" "valhalla"
	if errorlevel 1 (
		call :log "WARN: Valhalla not ready. Continuing startup in degraded/basic mode."
		set "ROUTING_ENABLED=0"
	)
)

call :log "Starting Open WebUI backend executable..."
start "Z3R4H Backend" /MIN cmd /c ""%BACKEND_CMD%" >> "%LOG_FILE%" 2>&1"
call :wait_url "%BACKEND_HEALTH_URL%" "%BACKEND_TIMEOUT%" "backend"
if errorlevel 1 exit /b 1
call :wait_url "%GEO_HEALTH_URL%" "%GEO_TIMEOUT%" "geo"
if errorlevel 1 exit /b 1

call :log "All services healthy. Opening browser..."
start "" "%BROWSER_URL%"
call :log "Launcher completed successfully."
echo [OK] Z3R4H ready.
exit /b 0

:wait_url
set "wait_url=%~1"
set "wait_timeout=%~2"
set "wait_label=%~3"
set /a elapsed=0
:wait_loop
call :probe_url "%wait_url%"
if not errorlevel 1 (
	call :log "%wait_label% is ready."
	exit /b 0
)
if errorlevel 2 (
	call :fail "Readiness probe unavailable/blocked for %wait_label%."
	exit /b 1
)
if %elapsed% GEQ %wait_timeout% (
	call :fail "%wait_label% startup timeout (%wait_timeout%s)."
	exit /b 1
)
set /a elapsed+=POLL_SECONDS
call :log "Waiting for %wait_label%... %elapsed%s/%wait_timeout%s"
timeout /t %POLL_SECONDS% /nobreak >nul
goto wait_loop

:check_port_free
set "port=%~1"
set "service=%~2"
for /f "tokens=*" %%L in ('netstat -ano ^| findstr /R /C:":%port% .*LISTENING"') do (
	call :fail "Port %port% appears occupied before startup (%service%)."
	exit /b 1
)
exit /b 0

:probe_url
set "probe_url=%~1"
powershell -NoProfile -ExecutionPolicy Bypass -Command "$u='%probe_url%'; try { $r=Invoke-WebRequest -UseBasicParsing -Uri $u -TimeoutSec 2; if($r.StatusCode -ge 200 -and $r.StatusCode -lt 500){exit 0}else{exit 1} } catch { exit 1 }" >nul 2>&1
if not errorlevel 1 exit /b 0

where curl.exe >nul 2>&1
if errorlevel 1 exit /b 2
curl.exe --max-time 2 --silent --output nul "%probe_url%" >nul 2>&1
if errorlevel 1 exit /b 1
exit /b 0

:log
set "ts=%date% %time%"
echo [%ts%] %~1
>> "%LOG_FILE%" echo [%ts%] %~1
exit /b 0

:resolve_path
set "_rp_name=%~1"
call set "_rp_value=%%%_rp_name%%%"
if not defined _rp_value exit /b 0
if "%_rp_value:~1,1%"==":" exit /b 0
if "%_rp_value:~0,2%"=="\\" exit /b 0
set "_rp_value=%PACKAGE_ROOT%%_rp_value%"
call set "%_rp_name%=%_rp_value%"
exit /b 0

:fail
call :log "ERROR: %~1"
echo [ERROR] %~1
echo See log: "%LOG_FILE%"
exit /b 1
