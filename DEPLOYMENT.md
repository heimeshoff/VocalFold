# VocalFold Deployment Guide

This guide explains how to build and deploy VocalFold, including creating a Windows installer.

## Prerequisites

### For Development
- .NET 9.0 SDK
- Node.js and npm
- F# compiler (included with .NET SDK)

### For Creating Installer
- **Inno Setup 6** (or 5)
  - Download from: https://jrsoftware.org/isdl.php
  - Can install to either:
    - `C:\Program Files (x86)\Inno Setup 6\` (system-wide)
    - `C:\Users\<username>\AppData\Local\Programs\Inno Setup 6\` (current user)
  - The deploy script will automatically detect both locations

## Building VocalFold

### Quick Build
```bash
npm run deploy
```

This command will:
1. Clean the deployment directory
2. Build the WebUI (F# → JavaScript with Fable)
3. Publish the .NET application
4. Copy all necessary files to `deploy/VocalFold/`
5. Create a ZIP archive: `deploy/VocalFold-YYYY-MM-DD.zip`
6. **Generate Windows installer**: `deploy/VocalFold-Setup.exe` (if Inno Setup is installed)

### Manual Build Steps

If you need to build components separately:

```bash
# Build WebUI only
npm run build:webui

# Build backend only
npm run build:backend

# Full rebuild
npm run rebuild
```

## Creating the Installer

### Automatic (Recommended)
```bash
npm run deploy
```

If Inno Setup is installed, the installer will be automatically generated at:
- `deploy/VocalFold-Setup.exe`

### Manual Compilation
If you prefer to compile the installer manually:

1. Ensure the deployment folder exists with all files:
   ```bash
   npm run deploy
   ```

2. Right-click on `installer.iss` in Windows Explorer
3. Select **"Compile"** from the context menu

OR use the command line:
```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

## Installer Features

The VocalFold installer provides:

### Installation
- Installs to `C:\Program Files\VocalFold\` by default
- Creates Start Menu shortcut
- **Optional**: Desktop shortcut (unchecked by default)
- Launches VocalFold after installation (optional)

### Version Upgrades
- Automatically detects previous installations
- Replaces old version with new version
- Preserves user settings in `%APPDATA%\VocalFold\`

### Running Instance Detection
- Checks if VocalFold is currently running before installation
- Prompts user to close the application
- Prevents installation while the app is running
- Same protection during uninstallation

### Uninstallation
- Clean removal of all program files
- Preserves user data in `%APPDATA%\VocalFold\` (logs, settings)
- Removes Start Menu and Desktop shortcuts
- Removes from Windows Programs list

## Distribution

After running `npm run deploy`, you'll have two distribution options:

### 1. Installer Executable (Recommended for End Users)
- **File**: `deploy/VocalFold-Setup.exe`
- **Size**: ~50-80 MB (depends on .NET runtime inclusion)
- **Best for**: End users, simple installation
- **Features**:
  - Guided installation wizard
  - Desktop shortcut option
  - Automatic upgrades
  - Clean uninstallation

### 2. ZIP Archive (For Advanced Users)
- **File**: `deploy/VocalFold-YYYY-MM-DD.zip`
- **Size**: ~100-150 MB
- **Best for**: Portable installation, advanced users
- **Usage**: Extract and run `VocalFold.exe`

## Installer Configuration

The installer is configured in `installer.iss`. Key settings:

### Application Information
```pascal
#define MyAppName "VocalFold"
#define MyAppVersion "1.0.0"
```

### Installation Directory
Default: `C:\Program Files\VocalFold`

Users can change this during installation.

### Application ID
```pascal
AppId={{8F9A7B6C-5D4E-3A2B-1C9F-8E7D6C5B4A3F}
```

This GUID uniquely identifies VocalFold for upgrade detection.

### Single Instance Detection
VocalFold uses a global mutex: `Global\VocalFoldMutex`

This ensures:
- Only one instance runs at a time
- Installer can detect if the app is running
- Clean upgrades without file conflicts

## Customizing the Installer

To modify the installer:

1. Edit `installer.iss`
2. Common customizations:
   - Change default directory: Modify `DefaultDirName`
   - Add/remove shortcuts: Edit `[Icons]` section
   - Change installer appearance: Modify `[Setup]` section
   - Add license agreement: Add `LicenseFile` in `[Setup]`

3. Recompile:
   ```bash
   npm run deploy
   ```

## Troubleshooting

### Installer Not Generated
**Problem**: `npm run deploy` completes but no installer is created.

**Solution**:
1. Check if Inno Setup is installed:
   ```
   C:\Program Files (x86)\Inno Setup 6\ISCC.exe
   ```
2. If not installed, download from: https://jrsoftware.org/isdl.php
3. Install to default location
4. Re-run `npm run deploy`

### "VocalFold is already running" Error
**Problem**: Installer refuses to proceed.

**Solution**:
1. Close VocalFold from the system tray (right-click tray icon → Exit)
2. If that doesn't work, open Task Manager (Ctrl+Shift+Esc)
3. Look for `VocalFold.exe` under Processes
4. End the task
5. Retry installation

### Upgrade Fails
**Problem**: Cannot upgrade over existing installation.

**Solution**:
1. Close VocalFold completely
2. Uninstall the old version via Windows Settings
3. Install the new version
4. Your settings will be preserved in `%APPDATA%\VocalFold\`

### Missing Dependencies
**Problem**: Application won't start after installation.

**Solution**:
The installer includes all dependencies. If issues persist:
1. Ensure Windows is up to date
2. Install Visual C++ Redistributable:
   - https://aka.ms/vs/17/release/vc_redist.x64.exe
3. For GPU support, install NVIDIA drivers

## Signing the Installer (Optional)

For production releases, you should sign the installer with a code signing certificate:

```bash
signtool sign /f "certificate.pfx" /p "password" /tr http://timestamp.digicert.com /td sha256 /fd sha256 "deploy\VocalFold-Setup.exe"
```

Benefits:
- Removes Windows SmartScreen warnings
- Builds user trust
- Required for some enterprise environments

## Advanced: Self-Contained vs Framework-Dependent

### Current: Framework-Dependent
- Requires .NET 9.0 Runtime on target machine
- Smaller installer size
- Shares .NET runtime with other apps

### Alternative: Self-Contained
To create a self-contained installer:

1. Edit `scripts/deploy.js`, change the dotnet publish command:
   ```javascript
   execSync('cd VocalFold && dotnet publish -c Release -r win-x64 --self-contained true -o bin/Release/net9.0/publish', {
   ```

2. Edit `VocalFold\VocalFold.fsproj`, add:
   ```xml
   <PropertyGroup>
     <SelfContained>true</SelfContained>
     <RuntimeIdentifier>win-x64</RuntimeIdentifier>
   </PropertyGroup>
   ```

3. Rebuild:
   ```bash
   npm run deploy
   ```

**Trade-offs**:
- ✅ Works without .NET runtime installed
- ✅ Easier for end users
- ❌ Much larger installer (~150-200 MB)
- ❌ No shared runtime optimization

## Version Management

To release a new version:

1. Update version in `installer.iss`:
   ```pascal
   #define MyAppVersion "1.1.0"
   ```

2. Update version in `package.json`:
   ```json
   "version": "1.1.0"
   ```

3. Build and test:
   ```bash
   npm run deploy
   ```

4. Test the upgrade:
   - Install the old version
   - Run the new installer
   - Verify upgrade works smoothly

## CI/CD Integration

For automated builds, you can integrate the deploy script into your CI/CD pipeline:

```yaml
# Example GitHub Actions workflow
- name: Build and Deploy
  run: |
    npm install
    npm run deploy

- name: Upload Installer
  uses: actions/upload-artifact@v3
  with:
    name: VocalFold-Setup
    path: deploy/VocalFold-Setup.exe
```

## Support

For issues with deployment:
1. Check the build logs
2. Verify all prerequisites are installed
3. Try a clean build: Delete `deploy/`, `node_modules/`, then rebuild
4. Check the issue tracker on GitHub
