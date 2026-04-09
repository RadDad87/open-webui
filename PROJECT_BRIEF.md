# Z3R4H Project Brief

## Product
Z3R4H is a custom-branded offline survival AI interface built on Open WebUI and powered by local GGUF models through llama.cpp.

## Goal
Create a Windows-first, fully offline survival AI product that looks polished, launches simply, and uses custom Z3R4H modes and prompts.

## Core Rules
- Fully offline operation for v1
- No cloud APIs
- No OpenAI API dependency in product runtime
- No telemetry
- No login requirement for v1
- No unnecessary frameworks or overengineering
- Keep implementation simple, stable, and portable
- Preserve Open WebUI where useful, but remove generic/cloud-first assumptions
- Brand name must appear as Z3R4H
- Interface language should emphasize:
  - Offline Survival AI
  - Local processing
  - Knowledge when the grid falls

## UI Direction
- Dark tactical / premium theme
- Custom Z3R4H branding
- Remove generic assistant wording
- Replace generic model language with Z3R4H mode language where appropriate

## Z3R4H Modes
- General Advisor
- Medical Reference
- Engineering & Repair
- Navigation & Terrain
- Low Power Mode

## Prompt Architecture
Each mode should eventually load:
- core_system.txt
- one mode-specific prompt
- failure_behavior.txt

## Runtime Architecture
User
→ Z3R4H-branded Open WebUI
→ local llama.cpp server
→ local GGUF model

## Priority Order
1. Reliable Windows launcher
2. Open WebUI connected to local llama.cpp
3. Z3R4H branding pass
4. Z3R4H modes
5. Prompt loading system
6. Packaging and documentation

## Constraints
- Windows-first
- Offline-first
- Localhost-only for inference
- No unrelated refactors
- Keep edits narrowly scoped to the current task
