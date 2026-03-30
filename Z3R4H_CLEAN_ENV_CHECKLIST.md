# Z3R4H Clean-Environment Validation Checklist (Windows Local)

## Scope
This checklist validates launch readiness on a clean Windows machine for the current Z3R4H build, without changing product behavior.

## 1) Machine Baseline
- Windows user profile is fresh or has no prior Open WebUI local config.
- Terminal used: Command Prompt or PowerShell.
- Local firewall/AV policies noted before test.

## 2) Preflight Requirements
- `launch_z3r4h.bat` exists at repo root.
- `LLAMA_SERVER_EXE` path is valid.
- `MODEL_PATH` path is valid.
- `open-webui` command resolves in PATH.
- Local ports configured for llama.cpp and Open WebUI are available.

## 2b) Portable Layout Preflight
- Confirm package root contains expected folders (`models/`, `runtime/llama.cpp/`).
- Confirm launcher defaults resolve correctly from `%~dp0` after moving package to a new directory.
- Confirm whether current build uses:
  - external `open-webui` CLI in PATH, or
  - bundled Open WebUI runtime command.

## 3) Launcher Configuration Validation
- Confirm top variables in `launch_z3r4h.bat`:
  - `LLAMA_SERVER_EXE`
  - `MODEL_PATH`
  - `LLAMA_HOST`
  - `LLAMA_PORT`
  - `OPEN_WEBUI_START_CMD`
  - `OPEN_WEBUI_URL`
  - `LOG_FILE`
- Confirm localhost-only settings remain unchanged.
- Confirm offline/local hardening defaults are active.

## 4) Startup Validation
1. Run `launch_z3r4h.bat`.
2. Verify llama.cpp server process starts.
3. Verify Open WebUI process starts.
4. Verify readiness polling completes for both services.
5. Verify browser opens the local UI URL.
6. Verify `z3r4h_launcher.log` contains startup sequence and no fatal errors.

## 5) UI/Runtime Validation
- Z3R4H branding appears in UI surfaces.
- Z3R4H mode selector is visible.
- One chat request returns a local response.
- Mode-to-model routing works; fallback warning appears if mapped model is missing.

## 6) Offline-Hardening Validation
- No login prompt in launcher path.
- Community sharing surfaces disabled.
- Web search features disabled.
- No cloud endpoint configuration required for baseline chat.

## 7) Negative/Failure Path Checks
- Missing `LLAMA_SERVER_EXE` -> clear error in console/log.
- Missing `MODEL_PATH` -> clear error in console/log.
- `open-webui` missing in PATH -> clear error in console/log.
- Port conflict -> timeout + likely conflict guidance.

## 8) Hidden Assumptions Audit
- Hardcoded/default local paths may not match machine.
- PATH inheritance for spawned shell may differ by setup.
- PowerShell availability is assumed for readiness probes.
- Localhost binding and firewall behavior can block readiness checks.
- Runtime model list may not include mode-mapped IDs on first run.
- localStorage state (`z3r4h_mode`, `z3r4h_mode_model_map`) may be absent/corrupt on fresh profiles.

## 9) Blocker Log Template
For each blocker, capture:
- Severity
- Repro steps
- Expected vs actual result
- Log snippet path/time
- Candidate fix (if known)
