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
- Determine startup mode before launch:
  - External CLI mode: `open-webui` command resolves in PATH.
  - Bundled runtime mode: `OPEN_WEBUI_START_CMD` points to packaged executable.
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


## 10) Task 11 Execution Flow (Clean Machine, Windows)
1. Capture baseline: Windows version, shell (CMD/PowerShell), AV/firewall posture.
2. Verify portable layout from launcher directory (`%~dp0`) after moving package folder.
3. Run preflight gates: startup mode, paths, model artifact, ports, and PowerShell readiness-probe capability.
4. Execute cold launch (`launch_z3r4h.bat`) and capture first-failure stage if unsuccessful.
5. Validate readiness endpoints and browser open behavior; confirm startup sequence in `z3r4h_launcher.log`.
6. Execute UI/runtime smoke checks: mode selector present + one successful local response.
7. Execute negative-path checks: missing binary/model, missing PATH command, and port conflicts.
8. Record blockers into ranked register and assign release-gate status (P0/P1/P2).

## 11) Highest-Risk Blockers (Packaging Readiness)
- **P0 candidate:** Default startup mode depends on `open-webui` in PATH (clean-machine failure risk).
- **P0/P1 candidate:** Readiness probes depend on PowerShell `Invoke-WebRequest` behavior/policy.
- **P1 candidate:** Port conflicts on `11434` (llama.cpp) or `8080` (Open WebUI) appear as startup timeout.
- **P1 candidate:** Default model path/file (`models\model.gguf`) may not match packaged artifact name.
- **P1/P2 candidate:** Fresh-profile localStorage state for mode routing may be absent/corrupt.

## 12) Blocker Prioritization Rules
- **P0:** Prevents clean-machine launch in the intended default package flow.
- **P1:** Launch possible, but core local usability/hardening expectation fails.
- **P2:** Non-blocking friction or documentation clarity issue with workaround.

## 13) Blocker Register (Task 11)
Capture each finding with:
- ID
- Severity (P0/P1/P2)
- Stage (preflight/startup/runtime)
- Repro reliability (always/intermittent)
- Expected vs actual
- Evidence (`z3r4h_launcher.log` timestamp + console symptom)
- Proposed next action (doc-only, launcher change, packaging change)
- Owner and status

## 14) Deferred Beyond Task 11
- Installer generation/signing
- CI/release automation
- Bundled Open WebUI runtime build/distribution workflow
- Product/backend behavior changes unless a verified P0 requires escalation


## 15) Task 12 Blocker Fix Verification (Windows Local)
- Confirm startup mode branch is explicit:
  - `OPEN_WEBUI_START_MODE=external` -> PATH check for `open-webui`
  - `OPEN_WEBUI_START_MODE=bundled` -> file existence check for `OPEN_WEBUI_BUNDLED_EXE`
- Confirm launcher fails fast on occupied ports before starting processes.
- Confirm missing model path error shows configured path and expected package default.
- Confirm readiness probe failures are distinguishable from service startup timeouts.

## 16) Task 12 Blockers Fixed Now
- Startup-mode ambiguity for Open WebUI command source.
- Readiness-probe fragility/diagnostic ambiguity.
- Model-path assumption clarity for packaged layout.
- Port-conflict detection clarity.

## 17) Deferred Beyond Task 12
- Installer generation/signing
- CI/release automation
- Bundled Open WebUI runtime build/distribution workflow
- Product/backend behavior changes outside launcher robustness


## 18) Task 13 Release-Candidate Checklist Structure
1. Scope & Intent
2. Package Manifest
3. Runtime Prerequisites
4. Preflight Validation
5. Launch Validation
6. Operator Smoke Checks
7. Blocker Gate (P0/P1)
8. Evidence & Signoff

## 19) Package Manifest (Portable Windows RC)
Required:
- `launch_z3r4h.bat`
- `runtime/llama.cpp/llama-server.exe` (or configured equivalent)
- `models/model.gguf` (or configured equivalent)
- `runtime/open-webui/open-webui.exe` if `OPEN_WEBUI_START_MODE=bundled`
- writable launcher directory for `z3r4h_launcher.log`

## 20) Runtime Prerequisites (Windows Local)
- CMD-based launcher execution supported
- localhost network access for configured ports
- Startup mode prerequisite:
  - `external` mode -> `open-webui` in PATH
  - `bundled` mode -> valid `OPEN_WEBUI_BUNDLED_EXE`
- Readiness probe capability via PowerShell and/or curl per launcher behavior

## 21) RC Signoff Criteria
Mark build RC-ready only when all are true:
- Package manifest complete for selected startup mode
- Preflight checks pass with no ambiguous failures
- Cold launch completes with both services ready
- UI opens and one local prompt returns a response
- No unresolved P0/P1 launch blockers
- Evidence set captured: checklist results + key log excerpts + signoff date/owner

## 22) Deferred Beyond Task 13
- Installer generation/signing
- Wrapper/bootstrapper work
- CI/release automation
- Bundled runtime build/distribution pipeline
- Backend/product behavior changes


## 23) Task 14 Execution Workflow (Operator Runbook)
Execute in order and record pass/fail per step:
1. **Session Header**
   - Operator name/initials
   - Date/time (local)
   - Machine profile (Windows version, shell, clean-profile confirmation)
   - Build/package identifier
2. **Manifest Check**
   - Verify required package contents for selected startup mode
3. **Prerequisite Check**
   - Verify runtime prerequisites and startup mode requirements
4. **Preflight Check**
   - Validate launcher configuration, ports, and log write path
5. **Cold Launch**
   - Run launcher and confirm readiness path completes
6. **Smoke Validation**
   - Confirm UI reachable and one local prompt returns response
7. **Blocker Gate**
   - Classify and log blockers as P0/P1/P2
8. **Signoff**
   - Mark RC-ready or blocked with owner/date

## 24) Task 14 Evidence Pack (Required)
Capture and store for each validation run:
- Session header metadata (operator/date/machine/build)
- Startup mode used (`external` or `bundled`)
- Step-by-step pass/fail outcomes from section 23
- `z3r4h_launcher.log` excerpts for preflight, readiness, and any errors
- One smoke-test prompt/response result
- Blocker entries with severity/repro/evidence references
- Final signoff record (RC-ready vs blocked, owner/date)

## 25) Blocker Reporting and Escalation Rules
For each blocker record:
- ID
- Severity (P0/P1/P2)
- Stage (manifest/prereq/preflight/launch/smoke/signoff)
- Reproducibility (always/intermittent)
- Expected vs actual behavior
- Evidence pointer (log timestamp, screenshot/notes path)
- Owner and status

Escalation policy:
- **P0**: Immediate stop, RC status = blocked, escalate immediately.
- **P1**: RC blocked unless explicit acceptance is documented by owner.
- **P2**: RC may proceed with follow-up task logged and owner assigned.

Status lifecycle:
- Open -> Triaged -> Mitigated -> Closed
- Open -> Triaged -> Accepted Risk (with owner/date)

## 26) Deferred Beyond Task 14
- Installer generation/signing
- Wrapper/bootstrapper implementation
- CI/release automation
- Bundled runtime build/distribution pipeline
- Product/backend behavior changes outside validation support


## 27) Task 15 RC Run Result Template
Complete this section for each clean-Windows RC run.

### Run Metadata
- Run ID:
- Operator:
- Date/Time (local):
- Machine Profile:
- Build/Package ID:
- Startup Mode (`external`/`bundled`):

### Step Results
| Step | Status (PASS/FAIL/BLOCKED) | Timestamp | Expected | Actual | Evidence Ref | Notes |
|---|---|---|---|---|---|---|
| Manifest Check |  |  |  |  |  |  |
| Prerequisite Check |  |  |  |  |  |  |
| Preflight Check |  |  |  |  |  |  |
| Cold Launch |  |  |  |  |  |  |
| Smoke Validation |  |  |  |  |  |  |
| Blocker Gate |  |  |  |  |  |  |
| Signoff |  |  |  |  |  |  |

## 28) Task 15 Pass/Fail Evidence Fields (Required)
Per-step required fields:
- Step name
- Status (`PASS`, `FAIL`, `BLOCKED`)
- Timestamp
- Expected result
- Actual result
- Evidence reference (log excerpt path/time, console note, screenshot path if captured)
- Operator notes

Run-level required evidence:
- `z3r4h_launcher.log` reference and key timestamps
- One smoke-test prompt/response record
- Environment notes affecting outcome (ports, mode, path overrides)

## 29) Task 15 Blocker Rollup and Release Recommendation
### Blocker Rollup Table
| Severity | Count Open | Count Closed | Decision Impact |
|---|---:|---:|---|
| P0 |  |  | Immediate NO-GO while open |
| P1 |  |  | NO-GO unless explicitly accepted by owner |
| P2 |  |  | GO allowed with follow-up tracking |

### Release Recommendation Rules
- Any open **P0** -> `NO-GO`
- Any open **P1** -> `NO-GO` unless acceptance is documented (owner/date/rationale)
- No open P0/P1 and only P2 or none -> `GO` or `GO-WITH-RISK`

### Recommendation Record
- Recommendation (`GO` / `GO-WITH-RISK` / `NO-GO`):
- Rationale:
- Approver/Owner:
- Date:

## 30) Deferred Beyond Task 15
- Installer generation/signing
- Wrapper/bootstrapper implementation
- CI/release automation
- Bundled runtime build/distribution pipeline
- Product/backend behavior changes outside validation results capture


## 31) Task 16 Shipping Format Comparison (v1 Decision Support)
### Options Compared
- **Option A:** Portable BAT-launched package (current baseline)
- **Option B:** Wrapped desktop `.exe` / shell approach (intended v1 target)

### Comparison Matrix
| Criterion | Option A: Portable BAT | Option B: Wrapped `.exe` |
|---|---|---|
| Operator first-run UX | Medium (manual launcher awareness) | Strong (single desktop entry path) |
| Time-to-ship from current state | Strong | Medium (requires wrapper delivery readiness) |
| Portability behavior | Strong | Medium/Strong (depends on wrapper packaging discipline) |
| Support burden profile | Medium (manual setup questions) | Medium (wrapper + runtime diagnostics) |
| Launch risk from extra layer | Lower | Higher initially (new shell layer) |
| v1 product presentation | Medium | Strong |

## 32) Criteria Evaluated
- Operator experience and first-run clarity on clean Windows machines
- Portability of runtime/model/package assets
- Support burden and diagnosability for field issues
- Incremental launch risk introduced by wrapper layer
- Alignment with v1 shipping intent and distribution expectations

## 33) Task 16 v1 Recommendation (Exe-First)
**Recommended v1 shipping format: Option B (Wrapped desktop `.exe` / shell approach).**

Rationale:
- Best matches intended v1 operator experience (single obvious launch path).
- Better presentation for non-technical operators in initial distribution.
- Enables BAT launcher to shift to fallback/support role rather than primary UX.

## 34) Exe-First Gating Conditions (Must Be True)
1. RC validation shows no unresolved P0 blockers in exe-first launch path.
2. P1 issues are either closed or explicitly accepted with owner/date/rationale.
3. Diagnostic visibility is preserved (launcher/log evidence still available to support).
4. Packaged portability assumptions remain valid after wrapper introduction.
5. Pilot support burden is acceptable relative to BAT baseline.

## 35) Deferred Beyond Task 16
- Wrapper/desktop `.exe` implementation
- Installer generation/signing
- CI/release automation
- Bundled runtime build/distribution pipeline changes
- Product/backend behavior changes


## 36) Task 17 Wrapper Technology Options
### Option A: Lightweight native launcher executable
- Small orchestration-focused wrapper
- Direct process start/monitor for local runtime services
- Minimal UI surface, strong control over startup states

### Option B: Desktop shell framework wrapper
- Richer UI opportunities
- More runtime/moving parts for v1

### Option C: Executable stub delegating to BAT
- Fastest to scaffold
- Keeps BAT-centric launch behavior too visible for primary v1 UX

## 37) Task 17 Recommended Wrapper Approach (v1)
**Recommended:** Option A (lightweight native launcher executable) as primary v1 entrypoint.

Rationale:
- Balances operator simplicity with low orchestration complexity.
- Avoids large framework overhead for v1 launch path.
- Preserves direct control over startup, readiness, and failure states.

## 38) Planned Runtime Orchestration Flow
1. Validate package structure and startup mode assumptions.
2. Validate key paths/ports and log file accessibility.
3. Launch local llama runtime process.
4. Poll llama readiness endpoint until success or timeout.
5. Launch Open WebUI process.
6. Poll Open WebUI readiness endpoint until success or timeout.
7. Open local UI URL for operator when both are ready.
8. Surface clear failure state with remediation hint if any stage fails.

## 39) Diagnostics, Logging, and BAT Fallback Strategy
- Preserve launcher log continuity (`z3r4h_launcher.log` evidence flow).
- Keep failure categories operator-visible (path/mode/port/probe/timeout).
- Preserve BAT launcher as documented fallback/support path.
- Add explicit support step: if wrapper launch fails, retry via BAT path and capture comparative evidence.

## 40) Deferred Beyond Task 17
- Wrapper executable implementation
- Wrapper build pipeline/toolchain setup
- Installer generation/signing
- CI/release automation
- Product/backend behavior changes


## 41) Task 18 Recommended Wrapper Technology (Build-Ready Plan)
**Recommendation:** Lightweight native Windows launcher executable as the wrapper core.

Rationale:
- Provides direct process orchestration control with minimal additional runtime surface.
- Keeps startup/readiness state handling explicit and supportable.
- Supports `.exe`-first operator experience without requiring heavy framework overhead for v1.

## 42) Task 18 Proposed Wrapper Project/File Structure
```text
wrapper/
├─ README.md
├─ src/
│  ├─ main/                  # entrypoint + single-instance behavior
│  ├─ config/                # package-root, mode, path, port resolution
│  ├─ orchestrator/          # process launch/monitor/stop sequencing
│  ├─ health/                # readiness checks + timeout policy
│  ├─ logging/               # wrapper events + launcher log bridging
│  └─ fallback/              # BAT fallback invocation support
├─ assets/                   # icon/version placeholders
└─ build/                    # output conventions and artifact layout
```

## 43) Task 18 Exact Startup Orchestration Flow
1. Resolve package root and effective configuration (mode/paths/ports).
2. Validate required files, ports, and log writeability.
3. Start llama runtime process.
4. Poll llama readiness endpoint until ready or timeout.
5. Start Open WebUI process.
6. Poll Open WebUI readiness endpoint until ready or timeout.
7. On success, open local UI URL in default browser (v1 default path).
8. On failure, emit stage-specific error and present fallback action.

## 44) Logging, Error Handling, and BAT Fallback Preservation
- Preserve `z3r4h_launcher.log` continuity for support evidence.
- Emit wrapper-level stage markers (preflight/startup/readiness/failure).
- Map failures to actionable classes:
  - config/path errors
  - startup mode errors
  - port conflicts
  - readiness timeout/failure
- Keep BAT launcher as fallback/support path with explicit handoff condition and operator guidance.

## 45) Task 18 Minimum Viable Wrapper Implementation Milestone
MVP is reached when all are true:
- `.exe` wrapper starts and monitors both runtime processes.
- Readiness checks gate success/failure with clear timeout behavior.
- Browser opens only after both services are ready.
- Failure diagnostics are operator-actionable and logged.
- BAT fallback can be triggered and documented for support use.

## 46) Deferred Beyond Task 18
- Wrapper code/build implementation
- Wrapper toolchain setup and artifact production
- Installer generation/signing
- CI/release automation
- Embedded-WebView-first approach for v1
- Product/backend behavior changes


## 47) Task 19 Wrapper Scaffold Structure (Implemented)
```text
wrapper/
├─ README.md
├─ .gitignore
├─ Z3R4H.Wrapper.sln
└─ src/Z3R4H.Wrapper/
   ├─ Z3R4H.Wrapper.csproj
   ├─ Program.cs
   ├─ App/
   │  ├─ WrapperApp.cs
   │  └─ StartupResult.cs
   ├─ Config/
   │  ├─ WrapperConfig.cs
   │  └─ ConfigResolver.cs
   ├─ Orchestration/RuntimeOrchestrator.cs
   ├─ Health/ReadinessProbe.cs
   ├─ Logging/WrapperLogger.cs
   └─ Fallback/BatFallback.cs
```

## 48) Task 19 Entrypoint vs Stubbed Behavior
Entrypoint (`Program.cs` + `WrapperApp.cs`) currently:
- initializes logger/config/orchestrator/fallback components
- resolves package-root and BAT/log paths
- executes orchestration scaffold and returns structured exit code

Still stubbed in Task 19:
- full process orchestration for llama/Open WebUI
- full readiness polling/timeout behavior
- browser open logic

## 49) Task 19 Logging/Error Categories and BAT Fallback Hook
- Logging scaffold via `WrapperLogger` with stage/error outputs
- Error categories scaffolded:
  - `ConfigError`, `PathError`, `ModeError`, `PortConflict`, `ProbeFailure`, `Timeout`, `StartupFailure`, `FallbackInvocation`
- BAT fallback represented via `BatFallback.ReportFallbackHint(...)` with explicit fallback path hint to `launch_z3r4h.bat`

## 50) Deferred Beyond Task 19
- Full orchestration and readiness implementation
- Browser-open behavior implementation
- Wrapper build/release pipeline and packaging automation
- Installer generation/signing
- Embedded UI path
- Product/backend behavior changes


## 51) Task 20 Wrapper Milestone Behavior (Implemented)
Wrapper now performs:
- package-root and key-path resolution
- config validation (`startup-mode`, `fallback`)
- structured stage/error logging
- structured exit code return
- BAT fallback invocation when configured

## 52) Task 20 Supported Success/Failure States and Exit Codes
- `0` (`Success`): milestone validation success
- `10` (`ConfigResolutionFailed`): config resolution error
- `11` (`ConfigValidationFailed`): config/path validation failed
- `12` (`UnsupportedMode`): startup mode unsupported in milestone
- `20` (`FallbackInvoked`): BAT fallback invoked and completed
- `21` (`FallbackInvocationFailed`): BAT fallback failed to invoke/complete
- `99` (`UnhandledError`): unhandled wrapper exception

## 53) Task 20 BAT Fallback Invocation/Logging Rules
- Fallback mode `always`: invoke BAT immediately
- Fallback mode `auto`: invoke BAT when validation/startup gate fails
- Fallback mode `never`: return failure without invoking BAT
- Every fallback attempt logs reason, target BAT path, and resulting exit code

## 54) Deferred Beyond Task 20
- Full llama/Open WebUI process orchestration
- Full readiness lifecycle and timeout policy
- Browser launch behavior
- Installer/signing/build automation
- Embedded UI path


## 55) Task 21 Live Llama Orchestration Behavior (Implemented)
Wrapper now performs:
- config/path validation including llama executable and model paths
- llama process launch via configured host/port/model args
- llama readiness polling against `/health` with timeout/poll intervals
- structured success/failure + optional BAT fallback

## 56) Task 21 Supported Exit Codes
- `0` Success (llama launched + readiness passed)
- `10` ConfigResolutionFailed
- `11` ConfigValidationFailed
- `12` UnsupportedMode
- `13` LlamaLaunchFailed
- `14` LlamaReadinessFailed
- `20` FallbackInvoked
- `21` FallbackInvocationFailed
- `99` UnhandledError

## 57) Task 21 BAT Fallback Interaction
- `always` -> invoke BAT immediately
- `auto` -> invoke BAT on llama launch/readiness failure
- `never` -> do not invoke BAT; return structured failure
- fallback logging includes reason, BAT path, and BAT exit result

## 58) Deferred Beyond Task 21
- Open WebUI startup
- Browser launch
- Dual-service orchestration lifecycle
- Installer/signing/build automation


## 59) Task 22 Dual-Service Startup Behavior (Implemented)
Wrapper now performs:
- llama launch + llama readiness
- Open WebUI launch after llama ready
- Open WebUI readiness polling
- stage-specific structured outcomes

## 60) Task 22 New/Updated Exit Codes
- `0` Success (dual-service ready)
- `10` ConfigResolutionFailed
- `11` ConfigValidationFailed
- `12` UnsupportedMode
- `13` LlamaLaunchFailed
- `14` LlamaReadinessFailed
- `15` WebUiLaunchFailed
- `16` WebUiReadinessFailed
- `20` FallbackInvoked
- `21` FallbackInvocationFailed
- `99` UnhandledError

## 61) Task 22 BAT Fallback Behavior for Dual-Service Startup
- `always`: invoke BAT immediately
- `auto`: invoke BAT on llama-stage or webui-stage failure
- `never`: return structured failure without BAT invocation
- fallback logs include failure stage, reason, BAT path, and BAT result

## 62) Deferred Beyond Task 22
- Browser launch
- Embedded UI
- Advanced process supervision/restart policies
- Installer/signing/build automation


## 63) Task 23 Browser Launch Behavior (Implemented)
After dual-service readiness succeeds, wrapper requests browser launch using the configured local Open WebUI URL.
Browser launch is not attempted before llama + Open WebUI readiness gates pass.

## 64) Task 23 Exit Code Update
- `17` BrowserLaunchFailed

## 65) Task 23 Fallback Behavior
- `always`: invoke BAT immediately (unchanged)
- `auto`: invoke BAT on earlier startup/readiness failures only (llama/webui stages)
- `never`: no fallback invocation
- browser-launch-only failure returns structured wrapper failure without BAT handoff

## 66) Deferred Beyond Task 23
- Embedded UI
- Advanced process supervision/restart policies
- Installer/signing/build automation


## 67) Task 24 Wrapper-First RC Workflow Adaptation (Implemented)
- Wrapper `.exe` execution is now the primary RC validation entrypoint on clean Windows machines.
- Required wrapper-stage run order for evidence: `config` -> `llama-*` -> `webui-*` -> `browser-launch`.
- BAT execution is a fallback/support branch only when wrapper failure triggers handoff.

## 68) Task 24 Wrapper-Specific Evidence Fields (Required)
- Wrapper executable path and command arguments used.
- Wrapper exit code and terminal stage.
- Stage-by-stage pass/fail notes for config, llama, webui, and browser-launch.
- Readiness evidence for llama and Open WebUI stages.
- Browser-launch outcome evidence (success/failure message).
- BAT fallback invocation details (reason, BAT exit code) when applicable.
- Final RC recommendation with owner/date.

## 69) Task 24 Wrapper-Specific Blocker Categories
- Wrapper config/path resolution failure.
- Wrapper llama launch/readiness failure.
- Wrapper Open WebUI launch/readiness failure.
- Wrapper browser-launch failure.
- Wrapper -> BAT fallback handoff failure.
- Wrapper exit-code/stage evidence mismatch.
- Wrapper-first clean-machine usability blocker (primary path fails even if BAT fallback runs).

## 70) Deferred Beyond Task 24
- Wrapper runtime behavior changes
- Product/backend changes
- Embedded UI
- Advanced supervision/restart policies
- Installer/signing/build automation
