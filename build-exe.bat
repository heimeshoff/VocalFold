@echo off
REM Standalone executable build script for VocalFold
REM Creates a self-contained single-file executable for Windows x64

echo ========================================
echo VocalFold - Standalone Build
echo ========================================
echo.

echo Converting logo.png to logo.ico...
powershell -ExecutionPolicy Bypass -File "%~dp0convert-logo-to-ico.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [WARNING] Could not convert logo to .ico format. Continuing anyway...
)

cd /d "%~dp0VocalFold"

echo Cleaning previous builds...
dotnet clean -c Release

if exist "bin\Release\net9.0\win-x64\publish" (
    rmdir /s /q "bin\Release\net9.0\win-x64\publish"
)

echo.
echo Building standalone executable...
echo This may take a few minutes...
echo.

REM Note: We don't use PublishSingleFile because native CUDA libraries need to be alongside the exe
dotnet publish -c Release -r win-x64 --self-contained true

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed. Please check the errors above.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
echo Output location:
echo   VocalFold\bin\Release\net9.0\win-x64\publish\VocalFold.exe
echo.
echo File size:
dir "bin\Release\net9.0\win-x64\publish\VocalFold.exe" | find "VocalFold.exe"
echo.
echo DEPLOYMENT NOTES:
echo ================
echo Copy the ENTIRE publish folder to deploy the application.
echo All DLL files must be kept together with VocalFold.exe.
echo.
echo Requirements on target machine:
echo   - NVIDIA GPU with CUDA support
echo   - NVIDIA Drivers version 12.1.0 or higher
echo   - NO CUDA Toolkit installation needed!
echo.
echo The application is fully self-contained and includes:
echo   - .NET 9.0 runtime
echo   - CUDA runtime libraries
echo   - All dependencies
echo.
echo To create a shortcut, right-click VocalFold.exe and select "Create shortcut"
echo.
pause
