#!/usr/bin/env node
const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
const archiver = require('archiver');

const rootDir = path.join(__dirname, '..');
const deployDir = path.join(rootDir, 'deploy');
const deployAppDir = path.join(deployDir, 'VocalFold');

console.log('='.repeat(60));
console.log('VocalFold Deployment Script');
console.log('='.repeat(60));

// Clean deploy directory
console.log('\n[1/6] Cleaning deployment directory...');
if (fs.existsSync(deployDir)) {
  fs.rmSync(deployDir, { recursive: true, force: true });
}
fs.mkdirSync(deployDir, { recursive: true });
fs.mkdirSync(deployAppDir, { recursive: true });

// Publish .NET application
console.log('\n[2/6] Publishing .NET application...');
const publishDir = path.join(rootDir, 'VocalFold', 'bin', 'Release', 'net9.0', 'publish');
try {
  execSync('cd VocalFold && dotnet publish -c Release -o bin/Release/net9.0/publish', {
    cwd: rootDir,
    stdio: 'inherit'
  });
} catch (error) {
  console.error('Failed to publish .NET application');
  process.exit(1);
}

// Copy published files
console.log('\n[3/6] Copying application files...');
const publishedFiles = fs.readdirSync(publishDir);
for (const file of publishedFiles) {
  const srcPath = path.join(publishDir, file);
  const destPath = path.join(deployAppDir, file);

  if (fs.statSync(srcPath).isDirectory()) {
    fs.cpSync(srcPath, destPath, { recursive: true });
  } else {
    fs.copyFileSync(srcPath, destPath);
  }
}

// Copy WebUI dist folder
console.log('\n[4/6] Copying WebUI files...');
const webuiDistSrc = path.join(rootDir, 'VocalFold.WebUI', 'dist');
const webuiDistDest = path.join(deployAppDir, 'VocalFold.WebUI', 'dist');
if (fs.existsSync(webuiDistSrc)) {
  fs.mkdirSync(path.join(deployAppDir, 'VocalFold.WebUI'), { recursive: true });
  fs.cpSync(webuiDistSrc, webuiDistDest, { recursive: true });
  console.log(`  ✓ Copied WebUI dist folder`);
} else {
  console.warn('  ⚠ WebUI dist folder not found. Run npm run build:webui first!');
}

// Copy logo.png if it exists
const logoSrc = path.join(rootDir, 'VocalFold', 'logo.png');
if (fs.existsSync(logoSrc)) {
  fs.copyFileSync(logoSrc, path.join(deployAppDir, 'logo.png'));
  console.log(`  ✓ Copied logo.png`);
}

// Create README for deployment
const readmeContent = `# VocalFold - Deployment Package

## System Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in this package if self-contained)
- NVIDIA GPU with CUDA support (recommended for best performance)

## Installation Instructions

1. Extract this zip file to your desired location
2. Run VocalFold.exe to start the application
3. The application will create a system tray icon
4. Right-click the tray icon to access Settings
5. Configure your hotkey and start using voice-to-text!

## Features
- Voice-to-text transcription using Whisper AI
- Customizable global hotkey
- Keyword replacements
- Web-based settings interface
- System tray integration

## First Run
On first run, the application will:
- Download the Whisper AI model (approximately 140MB)
- Create settings folder in %APPDATA%\\VocalFold
- Set up the system tray icon

## Troubleshooting
- Check the logs in: %APPDATA%\\VocalFold\\logs
- Ensure your microphone is working and not muted
- Make sure no other application is using the same hotkey
- For GPU issues, ensure NVIDIA drivers are up to date

## Default Hotkey
Ctrl + Left Win (can be changed in Settings)

Version: 1.0.0
`;

fs.writeFileSync(path.join(deployAppDir, 'README.txt'), readmeContent);
console.log(`  ✓ Created README.txt`);

// Create zip file
console.log('\n[5/6] Creating deployment package...');
const timestamp = new Date().toISOString().replace(/[:.]/g, '-').split('T')[0];
const zipFileName = `VocalFold-${timestamp}.zip`;
const zipFilePath = path.join(deployDir, zipFileName);

const output = fs.createWriteStream(zipFilePath);
const archive = archiver('zip', {
  zlib: { level: 9 } // Maximum compression
});

output.on('close', () => {
  const sizeMB = (archive.pointer() / 1024 / 1024).toFixed(2);
  console.log(`\n${'='.repeat(60)}`);
  console.log('✅ Deployment package created successfully!');
  console.log(`${'='.repeat(60)}`);
  console.log(`\nPackage: ${zipFilePath}`);
  console.log(`Size: ${sizeMB} MB`);
  console.log(`\nYou can now distribute this file to other computers.`);
});

archive.on('error', (err) => {
  console.error('Error creating zip file:', err);
  process.exit(1);
});

archive.pipe(output);
archive.directory(deployAppDir, 'VocalFold');
archive.finalize();

// Try to compile Inno Setup installer if ISCC is available
console.log('\n[6/6] Checking for Inno Setup...');
try {
  // Common Inno Setup locations
  const userProfile = process.env.USERPROFILE || process.env.HOME;
  const isccPaths = [
    'C:\\Program Files (x86)\\Inno Setup 6\\ISCC.exe',
    'C:\\Program Files\\Inno Setup 6\\ISCC.exe',
    'C:\\Program Files (x86)\\Inno Setup 5\\ISCC.exe',
    'C:\\Program Files\\Inno Setup 5\\ISCC.exe',
    path.join(userProfile, 'AppData', 'Local', 'Programs', 'Inno Setup 6', 'ISCC.exe'),
    path.join(userProfile, 'AppData', 'Local', 'Programs', 'Inno Setup 5', 'ISCC.exe')
  ];

  let isccPath = null;
  for (const candidatePath of isccPaths) {
    if (fs.existsSync(candidatePath)) {
      isccPath = candidatePath;
      break;
    }
  }

  if (isccPath) {
    console.log(`  ✓ Found Inno Setup at: ${isccPath}`);
    console.log('  Compiling installer...');

    const installerScript = path.join(rootDir, 'installer.iss');
    execSync(`"${isccPath}" "${installerScript}"`, {
      cwd: rootDir,
      stdio: 'inherit'
    });

    console.log('\n✅ Installer executable created successfully!');
    console.log(`   Location: ${path.join(deployDir, 'VocalFold-Setup.exe')}`);
  } else {
    console.log('  ⚠ Inno Setup not found.');
    console.log('  To create an installer executable, please:');
    console.log('    1. Download Inno Setup from https://jrsoftware.org/isdl.php');
    console.log('    2. Install it to the default location');
    console.log('    3. Re-run npm run deploy');
    console.log('  OR manually compile: Right-click installer.iss → Compile');
  }
} catch (error) {
  console.error('  ⚠ Failed to compile installer:', error.message);
  console.log('  You can manually compile by right-clicking installer.iss → Compile');
}
