@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Build-Mod.ps1"
if errorlevel 1 (
  echo.
  echo Build failed. Send the error text above back to ChatGPT.
  pause
)