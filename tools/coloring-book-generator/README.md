# Coloring Book Prompt Generator

This generator uses **OpenAI by default**.

It only uses Claude if you explicitly toggle it with `--provider claude`.

## 1) Install

```bash
pip install -r requirements.txt
```

## 2) Add API keys

You can use either:
- environment variables
- a local `.env` file
- direct CLI flags

Copy `.env.example` to `.env` and fill in your keys:

```bash
copy .env.example .env
```

> Security note: keep `.env` private and never commit real API keys.

## 3) Run

Use the single launcher file:

```bat
cbpp.bat --theme "friendly forest animals" --age-range "6-10" --complexity medium
```

`cbpp.bat` will run `dist\cbpp.exe` if present, otherwise it falls back to `python generator.py`.

### OpenAI (default)

```bash
python generator.py --theme "friendly forest animals" --age-range "6-10" --complexity medium
```

### Claude (optional toggle)

```bash
python generator.py --theme "space dinosaurs" --provider claude
```

### Optional: pass keys directly

```bash
python generator.py --theme "castle cats" --openai-api-key your_openai_key_here
python generator.py --theme "ocean robots" --provider claude --anthropic-api-key your_anthropic_key_here
```

## 4) Build `.exe` (Windows)

```bat
build_exe.bat
```

The executable will be created at:

`dist\cbpp.exe`

## 5) Create distributable ZIP (Windows)

```bat
package_zip.bat
```

This creates:

`dist\cbpp-windows.zip`

## 6) Install & launch from ZIP (for end users)

1. Unzip `cbpp-windows.zip` to any folder (for example `C:\cbpp`).
2. Add API keys:
   - Copy `.env.example` to `.env`
   - Edit `.env` and set `OPENAI_API_KEY` (and `ANTHROPIC_API_KEY` only if needed).
3. Launch from Command Prompt:

```bat
cd C:\cbpp
cbpp.bat --theme "cute jungle animals"
```

The launcher uses `cbpp.exe` automatically when present.
