# Z3R4H Wrapper Scaffold (Task 19)

This folder contains the minimal C#/.NET scaffold for the planned `.exe`-first wrapper path.

## Scope in Task 19
- Project/file structure
- Entrypoint wiring
- Config/path resolution scaffold
- Orchestration stubs
- Logging stubs
- BAT fallback hook scaffold

## Not implemented in Task 19
- Full runtime process orchestration
- Full readiness probing behavior
- Installer/signing/build automation
- Embedded UI shell


## Task 20 Milestone Behavior
- Validates core wrapper configuration and path assumptions.
- Uses structured exit codes for success/failure categories.
- Supports fallback modes: `auto`, `always`, `never`.
- Invokes BAT fallback in `auto` or `always` mode based on milestone rules.

## Deferred After Task 20
- Full runtime orchestration (llama/Open WebUI process control)
- Full readiness lifecycle
- Browser launch flow


## Task 21 Live Llama Orchestration Milestone
- Wrapper now starts llama runtime process from configured executable/model paths.
- Wrapper waits for llama readiness via `/health` polling.
- Structured exit codes now include llama launch/readiness outcomes.
- BAT fallback remains support/recovery via `auto`/`always`/`never` policy.

## Deferred After Task 21
- Open WebUI startup
- Browser launch
- Dual-service orchestration lifecycle


## Task 22 Dual-Service Readiness Milestone
- Preserves existing llama launch/readiness behavior.
- Starts Open WebUI process after llama readiness succeeds.
- Polls Open WebUI readiness endpoint until success/timeout.
- Returns structured dual-service exit outcomes.

## Deferred After Task 22
- Browser launch
- Embedded UI
- Advanced process supervision


## Task 23 Browser Launch Milestone
- Preserves existing llama + Open WebUI startup/readiness flow.
- Launches configured local UI URL in default browser only after dual-service readiness succeeds.
- Adds structured browser-launch outcome handling and logging.
- BAT fallback behavior remains unchanged for earlier startup failures only.

## Deferred After Task 23
- Embedded UI
- Advanced process supervision
- Installer/signing/build automation


## Task 24 Wrapper-First RC Validation Support
- RC validation now treats wrapper `.exe` execution as the primary clean-machine path.
- Operator workflow records wrapper stages in order: config -> llama -> webui -> browser-launch.
- BAT is captured as fallback/support evidence only when wrapper failure triggers handoff.

## Wrapper-First Evidence Requirements
- Wrapper command/args, exit code, and terminal stage.
- Stage-specific console evidence for llama/webui/browser-launch outcomes.
- BAT handoff reason and BAT exit code when fallback occurs.

## Deferred After Task 24
- Embedded UI
- Advanced process supervision
- Installer/signing/build automation


## Task 25 Package-Root Resolution Blocker Fix
- Default package-root detection is now anchored to runtime package layout instead of fixed upward traversal.
- Wrapper now logs:
  - resolved package root and resolution source (`arg` vs `auto`)
  - resolved llama executable path
  - resolved BAT fallback path
- Fallback and runtime paths are derived from resolved package root.

### Quick verification (RC package)
Run:
- `Z3R4H.Wrapper.exe`
Expect config logs to show package root under your RC folder (not `C:\\`) and BAT path as `<package-root>\\launch_z3r4h.bat`.
