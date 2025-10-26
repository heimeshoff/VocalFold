@echo off
REM Quick start script for VocalFold Voice-to-Text application
REM This script builds and runs the application in development mode

echo ========================================
echo VocalFold - Voice to Text Application
echo ========================================
echo.

cd /d "%~dp0VocalFold"

echo Building application...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed. Please check the errors above.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Starting application...
echo Press Ctrl+Windows to activate voice recording
echo Press Ctrl+C to stop the application
echo.

dotnet run
