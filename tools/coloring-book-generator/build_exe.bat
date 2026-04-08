@echo off
setlocal

REM Build single-file Windows executable
pyinstaller --noconfirm --onefile --name cbpp generator.py

echo.
echo Build complete. EXE should be in dist\cbpp.exe
endlocal
