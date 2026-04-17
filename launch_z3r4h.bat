@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM Z3R4H Windows launcher (localhost/offline path)

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
call :resolve_path LLAMA_SERVER_EXE
call :resolve_path LLAMA_MODEL_PATH

set "MODEL_PATH=%LLAMA_MODEL_PATH%"
set "LLAMA_HOST=127.0.0.1"
set "LLAMA_PORT=11434"

REM Open WebUI startup mode:
REM - external (default): uses `open-webui` from PATH
REM - bundled: uses OPEN_WEBUI_BUNDLED_EXE
if not defined OPEN_WEBUI_START_MODE set "OPEN_WEBUI_START_MODE=bundled"
if not defined OPEN_WEBUI_BUNDLED_EXE set "OPEN_WEBUI_BUNDLED_EXE=runtime\open-webui\open-webui.exe"
call :resolve_path OPEN_WEBUI_BUNDLED_EXE
if not defined BACKEND_CMD set "BACKEND_CMD=open-webui serve"
set "OPEN_WEBUI_START_CMD=%BACKEND_CMD%"

if not defined BACKEND_URL (
	set "OPEN_WEBUI_URL=http://localhost:8080"
) else (
	set "OPEN_WEBUI_URL=%BACKEND_URL%"
)
set "OPEN_WEBUI_PORT=8080"
if not defined RUNTIME_LOGS_DIR set "RUNTIME_LOGS_DIR=logs"
call :resolve_path RUNTIME_LOGS_DIR
if not exist "%RUNTIME_LOGS_DIR%" mkdir "%RUNTIME_LOGS_DIR%" >nul 2>&1
set "LOG_FILE=%RUNTIME_LOGS_DIR%\z3r4h_launcher.log"

REM Verified Open WebUI env vars in this repo/version:
REM - ENABLE_OPENAI_API
REM - OPENAI_API_BASE_URLS
REM - OPENAI_API_KEYS
set "ENABLE_OPENAI_API=True"
set "OPENAI_API_BASE_URLS=http://127.0.0.1:%LLAMA_PORT%/v1"
set "OPENAI_API_KEYS="

REM Z3R4H offline/local hardening defaults (launcher path only)
set "OFFLINE_MODE=True"
set "WEBUI_AUTH=False"
set "ENABLE_LOGIN_FORM=False"
set "ENABLE_SIGNUP=False"
set "ENABLE_COMMUNITY_SHARING=False"
set "ENABLE_WEB_SEARCH=False"

REM --- Timeouts ---
set "LLAMA_TIMEOUT=120"
set "WEBUI_TIMEOUT=180"
set "POLL_SECONDS=2"

call :log "========================================"
call :log "Z3R4H launcher start"
call :log "llama endpoint: http://%LLAMA_HOST%:%LLAMA_PORT%"
call :log "webui url: %OPEN_WEBUI_URL%"
call :log "========================================"

if /I not "%LLAMA_HOST%"=="127.0.0.1" (
	call :fail "LLAMA_HOST must be 127.0.0.1 (localhost-only)."
	exit /b 1
)

echo %OPEN_WEBUI_URL% | findstr /I /R "^http://localhost[:/]" >nul
if errorlevel 1 (
	call :fail "OPEN_WEBUI_URL must be localhost-only (http://localhost:...)."
	exit /b 1
)

if not exist "%LLAMA_SERVER_EXE%" (
	call :fail "Missing llama.cpp executable. Expected default: %PACKAGE_ROOT%runtime\llama.cpp\llama-server.exe | Configured: %LLAMA_SERVER_EXE%"
	exit /b 1
)
if not exist "%MODEL_PATH%" (
	call :fail "Missing model file. Expected default: %PACKAGE_ROOT%models\model.gguf | Configured: %MODEL_PATH%"
	exit /b 1
)

if /I "%OPEN_WEBUI_START_MODE%"=="bundled" (
	if not exist "%OPEN_WEBUI_BUNDLED_EXE%" (
		call :fail "Bundled Open WebUI executable not found: %OPEN_WEBUI_BUNDLED_EXE%"
		exit /b 1
	)
	set "OPEN_WEBUI_START_CMD=\"%OPEN_WEBUI_BUNDLED_EXE%\" serve"
	call :log "Open WebUI startup mode: bundled (%OPEN_WEBUI_BUNDLED_EXE%)"
) else (
	where open-webui >nul 2>&1
	if errorlevel 1 (
		call :fail "External Open WebUI mode selected but open-webui not found in PATH. Portable mode expects bundled runtime by default."
		exit /b 1
	)
	call :log "Open WebUI startup mode: external (open-webui in PATH)"
)

call :check_port_free "%LLAMA_PORT%" "llama.cpp"
if errorlevel 1 exit /b 1
call :check_port_free "%OPEN_WEBUI_PORT%" "Open WebUI"
if errorlevel 1 exit /b 1

call :log "Starting llama.cpp..."
start "Z3R4H llama.cpp" /MIN cmd /c ""%LLAMA_SERVER_EXE%" -m "%MODEL_PATH%" --host %LLAMA_HOST% --port %LLAMA_PORT% >> "%LOG_FILE%" 2>&1"

call :wait_llama
if errorlevel 2 (
	call :fail "Readiness probe unavailable/blocked (PowerShell and curl probes failed)."
	exit /b 1
)
if errorlevel 1 (
	call :fail "llama.cpp startup timeout. Verify port conflicts/firewall and review %LOG_FILE%."
	exit /b 1
)

call :log "Starting Open WebUI..."
start "Z3R4H Open WebUI" /MIN cmd /c "%OPEN_WEBUI_START_CMD% >> "%LOG_FILE%" 2>&1"

call :wait_webui
if errorlevel 2 (
	call :fail "Readiness probe unavailable/blocked (PowerShell and curl probes failed)."
	exit /b 1
)
if errorlevel 1 (
	call :fail "Open WebUI startup timeout. Verify port conflicts/firewall and review %LOG_FILE%."
	exit /b 1
)

call :log "Services ready. Opening browser..."
start "" "%OPEN_WEBUI_URL%"
call :log "Launcher completed successfully."
echo [OK] Z3R4H ready.
exit /b 0

:check_port_free
set "port=%~1"
set "service=%~2"
for /f "tokens=*" %%L in ('netstat -ano ^| findstr /R /C:":%port% .*LISTENING"') do (
	call :fail "Port %port% appears occupied before startup (%service%). Free the port or change launcher ports."
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

:wait_llama
set /a elapsed=0
:wait_llama_loop
call :probe_url "http://%LLAMA_HOST%:%LLAMA_PORT%/health"
if not errorlevel 1 (
    call :log "llama.cpp is ready."
    exit /b 0
)
if errorlevel 2 exit /b 2
if %elapsed% GEQ %LLAMA_TIMEOUT% exit /b 1
set /a elapsed+=POLL_SECONDS
call :log "Waiting for llama.cpp... %elapsed%s/%LLAMA_TIMEOUT%s"
timeout /t %POLL_SECONDS% /nobreak >nul
goto wait_llama_loop

:wait_webui
set /a elapsed=0
:wait_webui_loop
call :probe_url "%OPEN_WEBUI_URL%"
if not errorlevel 1 (
    call :log "Open WebUI is ready."
    exit /b 0
)
if errorlevel 2 exit /b 2
if %elapsed% GEQ %WEBUI_TIMEOUT% exit /b 1
set /a elapsed+=POLL_SECONDS
call :log "Waiting for Open WebUI... %elapsed%s/%WEBUI_TIMEOUT%s"
timeout /t %POLL_SECONDS% /nobreak >nul
goto wait_webui_loop

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
