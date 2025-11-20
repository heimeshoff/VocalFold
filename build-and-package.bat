@echo off
REM Complete build and packaging script for VocalFold
REM Builds both WebUI and main application, then packages for deployment

setlocal enabledelayedexpansion

echo ========================================
echo VocalFold - Complete Build and Package
echo ========================================
echo.

REM Store the root directory
set ROOT_DIR=%~dp0
cd /d "%ROOT_DIR%"

REM Define output directory name with timestamp
for /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mydate=%%c%%a%%b)
for /f "tokens=1-2 delims=/:" %%a in ('time /t') do (set mytime=%%a%%b)
set TIMESTAMP=%mydate%_%mytime: =0%
set PACKAGE_NAME=VocalFold_%TIMESTAMP%
set DEPLOY_DIR=%ROOT_DIR%deploy\%PACKAGE_NAME%

echo Step 1: Building WebUI
echo ========================================
echo.

cd /d "%ROOT_DIR%VocalFold.WebUI"

REM Check if node_modules exists, if not install dependencies
if not exist "node_modules" (
    echo Installing npm dependencies...
    call npm install
    if %ERRORLEVEL% NEQ 0 (
        echo [ERROR] npm install failed
        pause
        exit /b %ERRORLEVEL%
    )
    echo.
)

echo Compiling F# code with Fable...

echo Cleaning previous Fable output...
if exist "dist\src" (
    rmdir /s /q "dist\src"
)
if exist "dist\fable_modules" (
    rmdir /s /q "dist\fable_modules"
)

call dotnet fable clean --yes
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Fable clean failed
    pause
    exit /b %ERRORLEVEL%
)

call dotnet fable --outDir dist --extension .js
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Fable compilation failed
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Building WebUI with Vite...
call npm run build
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] WebUI build failed
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo WebUI build completed successfully!
echo Output: %ROOT_DIR%VocalFold.WebUI\dist
echo.

echo Step 2: Converting logo to .ico format
echo ========================================
echo.

cd /d "%ROOT_DIR%"
powershell -ExecutionPolicy Bypass -File "%ROOT_DIR%convert-logo-to-ico.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo [WARNING] Could not convert logo to .ico format. Continuing anyway...
) else (
    echo Logo conversion completed!
)
echo.

echo Step 3: Building VocalFold application
echo ========================================
echo.

cd /d "%ROOT_DIR%VocalFold"

echo Cleaning previous builds...
dotnet clean -c Release

if exist "bin\Release\net9.0\win-x64\publish" (
    rmdir /s /q "bin\Release\net9.0\win-x64\publish"
)

echo.
echo Building and publishing VocalFold...
echo This may take a few minutes...
echo.

dotnet publish -c Release -r win-x64 --self-contained true

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] VocalFold build failed
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo VocalFold build completed successfully!
echo.

echo Step 4: Packaging for deployment
echo ========================================
echo.

cd /d "%ROOT_DIR%"

REM Create deploy directory
if exist "deploy" (
    echo Cleaning old deploy directory...
) else (
    mkdir "deploy"
)

if exist "%DEPLOY_DIR%" (
    rmdir /s /q "%DEPLOY_DIR%"
)

mkdir "%DEPLOY_DIR%"

echo Copying VocalFold executable and dependencies...
xcopy /E /I /Y "VocalFold\bin\Release\net9.0\win-x64\publish\*" "%DEPLOY_DIR%\"

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to copy VocalFold files
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Copying WebUI dist folder...
mkdir "%DEPLOY_DIR%\VocalFold.WebUI"
xcopy /E /I /Y "VocalFold.WebUI\dist\*" "%DEPLOY_DIR%\VocalFold.WebUI\dist\"

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to copy WebUI files
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Creating README for deployment...

(
echo VocalFold - Voice-Controlled Text Input
echo =========================================
echo.
echo This is a self-contained distribution of VocalFold.
echo All required runtime files are included.
echo.
echo HOW TO RUN:
echo -----------
echo 1. Double-click VocalFold.exe to start the application
echo 2. The application will run in the system tray
echo 3. Right-click the tray icon to access settings
echo 4. Use the configured hotkeys to start/stop dictation
echo.
echo SYSTEM REQUIREMENTS:
echo -------------------
echo - Windows 10/11 (64-bit)
echo - NVIDIA GPU with CUDA support
echo - NVIDIA Drivers version 12.1.0 or higher
echo - NO additional software installation needed!
echo.
echo IMPORTANT NOTES:
echo ---------------
echo - Keep ALL files in this folder together
echo - Do NOT delete or move any DLL files
echo - The WebUI folder is required for the settings interface
echo - You can copy this entire folder anywhere on your PC
echo.
echo FIRST RUN:
echo ----------
echo On first run, the application will download the required
echo Whisper AI model (approximately 150 MB). This is a one-time
echo download and will be cached for future use.
echo.
echo TROUBLESHOOTING:
echo ---------------
echo - If the app doesn't start, check that your NVIDIA drivers are up to date
echo - Check the logs in the application data folder for errors
echo - Make sure no antivirus is blocking the executable
echo.
echo For more information, visit: https://github.com/heimeshoffIT/VocalFold
echo.
echo Built: %DATE% %TIME%
) > "%DEPLOY_DIR%\README.txt"

echo.
echo Step 5: Creating ZIP archive
echo ========================================
echo.

set ZIP_FILE=%ROOT_DIR%deploy\%PACKAGE_NAME%.zip

REM Check if PowerShell's Compress-Archive is available
powershell -Command "Get-Command Compress-Archive" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Creating ZIP archive using PowerShell...
    powershell -Command "Compress-Archive -Path '%DEPLOY_DIR%\*' -DestinationPath '%ZIP_FILE%' -Force"

    if %ERRORLEVEL% EQU 0 (
        echo.
        echo ZIP archive created successfully!
        echo Location: %ZIP_FILE%
        echo.
    ) else (
        echo [WARNING] Failed to create ZIP archive
        echo You can manually zip the folder: %DEPLOY_DIR%
        echo.
    )
) else (
    echo [WARNING] PowerShell Compress-Archive not available
    echo Please manually zip the folder: %DEPLOY_DIR%
    echo.
)

echo ========================================
echo BUILD AND PACKAGING COMPLETED!
echo ========================================
echo.
echo Package location: %DEPLOY_DIR%
echo ZIP archive: %ZIP_FILE%
echo.
echo Package contents:
dir /B "%DEPLOY_DIR%"
echo.
echo FILE SIZES:
echo -----------
for %%F in ("%DEPLOY_DIR%\VocalFold.exe") do echo VocalFold.exe: %%~zF bytes
echo.
REM Calculate total folder size
for /f "tokens=3" %%a in ('dir "%DEPLOY_DIR%" /-c ^| findstr /C:"bytes" ^| findstr /v "free"') do set FOLDER_SIZE=%%a
echo Total package size: %FOLDER_SIZE% bytes
echo.
echo DEPLOYMENT INSTRUCTIONS:
echo -----------------------
echo 1. Share the ZIP file or the deploy folder with end users
echo 2. Users should extract/copy the entire folder to their PC
echo 3. Users can run VocalFold.exe directly - no installation needed!
echo 4. All dependencies and the web interface are included
echo.
echo The package is fully self-contained and portable.
echo Users only need Windows 10/11 and NVIDIA GPU with CUDA support.
echo.

pause
