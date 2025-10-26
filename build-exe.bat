@echo off
REM Standalone executable build script for VocalFold
REM Creates a self-contained single-file executable for Windows x64

echo ========================================
echo VocalFold - Standalone Build
echo ========================================
echo.

cd /d "%~dp0VoiceToText"

echo Cleaning previous builds...
dotnet clean -c Release

if exist "bin\Release\net9.0\win-x64\publish" (
    rmdir /s /q "bin\Release\net9.0\win-x64\publish"
)

echo.
echo Building standalone executable...
echo This may take a few minutes...
echo.

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true

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
echo   VoiceToText\bin\Release\net9.0\win-x64\publish\VoiceToText.exe
echo.
echo File size:
dir "bin\Release\net9.0\win-x64\publish\VoiceToText.exe" | find "VoiceToText.exe"
echo.
echo DEPLOYMENT NOTES:
echo ================
echo You can now copy VoiceToText.exe to any Windows x64 machine.
echo.
echo Requirements on target machine:
echo   - NVIDIA GPU with CUDA support
echo   - NVIDIA Drivers version 12.1.0 or higher
echo   - NO CUDA Toolkit installation needed!
echo.
echo The executable is fully self-contained and includes:
echo   - .NET 9.0 runtime
echo   - CUDA runtime libraries
echo   - All dependencies
echo.
pause
