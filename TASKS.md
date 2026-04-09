# Z3R4H Tasks

## Task 1 — Windows Launcher
Create a Windows launcher that:
- starts the local llama.cpp server
- waits for initialization
- opens the UI in the default browser
- shows clear logs/errors
- stays minimal and offline-only

## Task 2 — Connect Open WebUI to local llama.cpp
Configure Open WebUI so it works cleanly with a local llama.cpp server and local models.

## Task 3 — Rebrand Open WebUI as Z3R4H
Update visible branding:
- Z3R4H name
- offline survival positioning
- remove generic cloud AI wording
- dark tactical/premium visual identity

## Task 4 — Add Z3R4H modes
Implement visible mode choices:
- General Advisor
- Medical Reference
- Engineering & Repair
- Navigation & Terrain
- Low Power Mode

## Task 5 — Prompt system
Implement or prepare a prompt-loading structure using:
- core_system.txt
- mode-specific prompt files
- failure_behavior.txt

## Task 6 — Offline hardening
Remove or disable:
- cloud dependencies
- telemetry
- login requirements for v1
- unnecessary external integrations

## Task 7 — Packaging and docs
Prepare:
- final folder structure
- run instructions
- troubleshooting notes
- offline usage notes

## Completed (Tasks 1–5)
- Task 1 — Windows Launcher: completed in `ca715c0b` (initial launcher and local startup flow)
- Task 2 — Local llama.cpp connection: completed in `ca715c0b` (local OpenAI-compatible routing setup)
- Task 3 — Z3R4H rebrand pass: completed in `5b381011`
- Task 4 — Z3R4H mode selector UI: completed in `6c0ed902`
- Task 5 — Prompt stack binding by mode: completed in `76d6bb8a`

## Completed (Task 7)
- Added concise Windows-local run, verification, and troubleshooting guidance in `README.md`.
- Updated progress tracking summary for packaging/readiness status in `Z3R4H_PROGRESS_SUMMARY.md`.


## Completed (Task 9)
- Added `Z3R4H_CLEAN_ENV_CHECKLIST.md` for clean Windows launch validation.
- Added focused pointers in `README.md` and summary status in `Z3R4H_PROGRESS_SUMMARY.md`.

## Completed (Task 10)
- Documented portable Windows package layout and runtime requirements in `README.md`.
- Updated launcher defaults to `%~dp0`-relative paths for portability.


## Completed (Task 11)
- Converted clean-environment checklist into an execution-ready Windows clean-machine validation flow.
- Added highest-risk blocker candidates and P0/P1/P2 prioritization rules.
- Added blocker register fields for triage evidence and release-gate decisions.
- Kept scope documentation/review-only with no product/backend behavior changes.

## Deferred (Beyond Task 11)
- Installer generation/signing
- CI/release automation
- Bundled Open WebUI runtime build/distribution workflow


## Completed (Task 8, Revised)
- Confirmed local mode-to-model routing resolves only against runtime `model.id` values.
- Kept routing deterministic with explicit fallback order: mode-mapped -> selected -> first-available -> none.
- Preserved existing prompt-stack behavior (no change to prompt composition).


## Completed (Task 12)
- Hardened launcher preflight for clean-machine startup (mode branch clarity, port checks, and clearer failure handling).
- Added readiness probe fallback/diagnostic differentiation in launcher behavior.
- Updated Windows-local checklist and README troubleshooting for Task 12 blocker fixes.
- Kept scope minimal: launcher + documentation/status only.

## Deferred (Beyond Task 12)
- Installer generation/signing
- CI/release automation
- Bundled Open WebUI runtime build/distribution workflow


## Completed (Task 13)
- Added a release-candidate packaging checklist structure for portable Windows Z3R4H readiness decisions.
- Documented package manifest requirements and runtime prerequisites for RC gating.
- Added explicit RC signoff criteria and evidence expectations.
- Kept scope documentation-only with no installer/wrapper/automation implementation.

## Deferred (Beyond Task 13)
- Installer generation/signing
- Wrapper/bootstrapper implementation
- CI/release automation
- Bundled runtime build/distribution pipeline


## Completed (Task 14)
- Added operator-focused execution runbook for Task 13 RC checklist on clean Windows machines.
- Added required evidence pack definition for validation runs.
- Added blocker reporting fields and escalation rules for RC decision support.
- Kept scope documentation/readiness-only with no product/backend behavior changes.

## Deferred (Beyond Task 14)
- Installer generation/signing
- Wrapper/bootstrapper implementation
- CI/release automation
- Bundled runtime build/distribution pipeline


## Completed (Task 15)
- Added RC run-result template for clean-Windows execution outcomes.
- Added pass/fail evidence field requirements for each validation step.
- Added blocker rollup and release recommendation rules (`GO`, `GO-WITH-RISK`, `NO-GO`).
- Kept scope documentation-only and operator-friendly.

## Deferred (Beyond Task 15)
- Installer generation/signing
- Wrapper/bootstrapper implementation
- CI/release automation
- Bundled runtime build/distribution pipeline


## Completed (Task 16)
- Added v1 shipping-format decision support comparing portable BAT baseline vs wrapped `.exe` target path.
- Documented comparison criteria: operator experience, portability, support burden, and launch risk.
- Recorded exe-first v1 recommendation with explicit gating conditions for release readiness.
- Kept scope decision-oriented and documentation-only (no wrapper implementation).

## Deferred (Beyond Task 16)
- Wrapper/desktop `.exe` implementation
- Installer generation/signing
- CI/release automation
- Bundled runtime build/distribution pipeline


## Completed (Task 17)
- Added .exe wrapper implementation planning docs for v1 (no code implementation).
- Documented wrapper technology options and selected lightweight native launcher approach.
- Added planned runtime orchestration flow, diagnostics/logging expectations, and BAT fallback strategy.
- Kept scope planning/documentation only.

## Deferred (Beyond Task 17)
- Wrapper executable implementation
- Wrapper build pipeline/toolchain setup
- Installer generation/signing
- CI/release automation


## Completed (Task 18)
- Added concrete build-ready planning docs for `.exe` wrapper implementation (no code changes).
- Documented recommended wrapper technology and project/file structure.
- Documented exact startup orchestration flow and readiness sequence.
- Documented logging/error model and BAT fallback preservation strategy.

## Deferred (Beyond Task 18)
- Wrapper code/build implementation
- Wrapper toolchain setup and artifact production
- Installer generation/signing
- CI/release automation


## Completed (Task 19)
- Added minimal native C#/.NET wrapper scaffold under `wrapper/` for `.exe`-first path.
- Added entrypoint/config/orchestration/logging/fallback scaffold files.
- Kept orchestration/readiness as stubs to avoid behavior changes in scaffold phase.

## Deferred (Beyond Task 19)
- Full runtime orchestration and readiness logic
- Browser-open behavior implementation
- Wrapper build/release pipeline and packaging automation
- Installer generation/signing


## Completed (Task 20)
- Upgraded wrapper scaffold to first working milestone with config/path validation and structured exit behavior.
- Added fallback-mode support and actual BAT fallback invocation path.
- Added structured logging/error categories for milestone failure states.

## Deferred (Beyond Task 20)
- Full runtime orchestration/readiness lifecycle
- Browser-open behavior
- Build/release pipeline and installer/signing


## Completed (Task 21)
- Extended wrapper to first live orchestration milestone: llama process launch + readiness polling.
- Added llama-specific structured exit codes and failure mapping.
- Preserved BAT fallback policies (`auto`/`always`/`never`) for support/recovery.

## Deferred (Beyond Task 21)
- Open WebUI startup/orchestration
- Browser launch
- Dual-service lifecycle management
- Installer/signing/build automation


## Completed (Task 22)
- Extended wrapper from single-service llama milestone to dual-service startup readiness milestone.
- Added Open WebUI launch + readiness polling after llama readiness success.
- Added stage-aware exit mapping for llama/webui launch/readiness failures.

## Deferred (Beyond Task 22)
- Browser launch
- Embedded UI
- Advanced process supervision/restart policy
- Installer/signing/build automation


## Completed (Task 23)
- Extended wrapper to launch the default browser after confirmed dual-service readiness.
- Added stage-aware browser launch success/failure handling and exit mapping.
- Preserved BAT fallback behavior for earlier startup failures only.

## Deferred (Beyond Task 23)
- Embedded UI
- Advanced process supervision/restart policy
- Installer/signing/build automation


## Completed (Task 24)
- Adapted RC validation docs for wrapper-first execution on clean Windows machines.
- Added wrapper-specific evidence capture requirements (stage outcomes, exit code, BAT handoff evidence).
- Added wrapper-specific blocker categories for RC triage and release recommendation support.

## Deferred (Beyond Task 24)
- Wrapper runtime behavior changes
- Product/backend changes
- Embedded UI
- Advanced process supervision/restart policy
- Installer/signing/build automation


## Completed (Task 25)
- Fixed wrapper package-root auto-detection for published RC layout.
- Fixed default llama and BAT path derivation from resolved package root.
- Added explicit startup logging for resolution source, package root, llama path, and BAT path.

## Deferred (Beyond Task 25)
- Embedded UI
- Advanced process supervision/restart policy
- Installer/signing/build automation
