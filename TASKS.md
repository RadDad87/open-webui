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
