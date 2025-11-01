# VocalFold
System-wide voice-to-text dictation for Windows using AI

---

### ☕ Support VocalFold

**Enjoying VocalFold?** Support the development of this free, open-source project!

[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support%20Me-FF5E5B?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/heimeshoff)

**[☕ Buy me a coffee on Ko-fi](https://ko-fi.com/heimeshoff)** — Your support helps keep this project alive and growing!

---

## Overview
VocalFold is a Windows desktop application that transcribes your voice to text using Whisper.NET AI. Press a global hotkey, speak, and your words appear as text at the cursor position in any application.

## Features
- 🎤 **Voice Recording**: Activate with global hotkey (Ctrl+Windows by default)
- 🤖 **AI Transcription**: Local processing using Whisper.NET (no cloud services)
- ⚡ **GPU Acceleration**: Support for NVIDIA (CUDA), AMD (Vulkan), and Intel (Vulkan) GPUs
- ⌨️ **Text Output**: Types text at cursor position (works in any app)
- 🔒 **Privacy-First**: All processing happens locally, no data leaves your machine
- 🎯 **Background Operation**: Runs in system tray, always available
- 🚀 **Windows Startup**: Optional auto-start with Windows
- 🎨 **Modern Web UI**: Configure settings via beautiful web interface
- 📝 **Keyword Replacement**: Create shortcuts for frequently used phrases
- 🗂️ **Category Organization**: Organize keywords into collapsible categories

## System Requirements

### Operating System
- **Windows 11** (recommended)
- **Windows 10** (supported)

### GPU Support (Optional, CPU fallback available)

VocalFold automatically selects the best available GPU runtime:

**NVIDIA GPUs** (CUDA):
- **Supported**: RTX 20 series or newer (RTX 2060, 3060, 3080, 4080, etc.)
- **Requires**: NVIDIA CUDA Toolkit 12.x ([Download](https://developer.nvidia.com/cuda-downloads))
- **Performance**: Excellent (~0.5s for 5s speech with Base model)
- **VRAM**: 4GB minimum, 8GB+ recommended

**AMD GPUs** (Vulkan):
- **Supported**: Radeon RX 6000 series or newer (RX 6700 XT, 6800 XT, 7900 XTX, etc.)
- **Requires**: Latest AMD Adrenalin drivers with Vulkan support ([Download](https://www.amd.com/en/support))
- **Performance**: Good (~1-2s for 5s speech with Base model)
- **VRAM**: 4GB minimum, 8GB+ recommended
- **Note**: Older GPUs (RX 5000 series) may have slower performance

**Intel GPUs** (Vulkan):
- **Supported**: Intel Arc series (A750, A770, etc.)
- **Requires**: Latest Intel Graphics drivers with Vulkan support ([Download](https://www.intel.com/content/www/us/en/download-center/home.html))
- **Performance**: Moderate (~2-3s for 5s speech with Base model)
- **VRAM**: 8GB+ recommended

**No GPU / Unsupported GPU** (CPU Fallback):
- **Performance**: Slow (~5-10s for 5s speech with Base model)
- **Recommended**: Use Tiny or Base model for acceptable speed
- **Works**: Fully functional, just slower

### Runtime Priority
VocalFold automatically detects and uses the best available runtime:
1. **CUDA** (NVIDIA GPUs) - Best performance
2. **Vulkan** (AMD/Intel GPUs) - Good performance
3. **CPU** (Fallback) - Slowest but functional

### Other Requirements
- **.NET 9.0 SDK** (for building from source)
- **16GB RAM** (recommended)
- **8GB RAM** (minimum)

## Installation

### Option 1: Download Pre-built Executable (Recommended)
1. Download `VocalFold.exe` from the [Releases](../../releases) page
2. Install GPU drivers (see GPU Support section above)
3. Run `VocalFold.exe`
4. First run will download the AI model (~150MB, one-time)
5. Application runs in system tray

### Option 2: Build from Source
```bash
# Clone repository
git clone https://github.com/yourusername/VocalFold.git
cd VocalFold

# Build project
dotnet restore
dotnet build

# Run application
dotnet run --project VocalFold

# Or build standalone executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Usage

### Basic Usage
1. Launch VocalFold (runs in system tray)
2. Click in any text field (Notepad, browser, Word, etc.)
3. Press **Ctrl+Windows** (or your configured hotkey)
4. Speak clearly into your microphone
5. Your words appear as text at the cursor position

### Configuring Settings
1. Right-click the VocalFold tray icon
2. Click **Settings**
3. Configure in the web UI:
   - Change global hotkey
   - Adjust typing speed
   - Manage keyword replacements
   - Organize keywords into categories
   - Enable/disable voice input

### Keyword Replacements
Create shortcuts for frequently used text:
- Say "comma" → types ","
- Say "period" → types "."
- Say "email signature" → types your full email signature
- Say "code snippet" → types your code template

Organize keywords into categories for better management.

## Troubleshooting

### GPU Not Detected
**Symptoms**: Slow transcription (5-10 seconds), console shows "CPU mode"

**Solutions**:

**For NVIDIA Users:**
1. Install NVIDIA CUDA Toolkit 12.x: https://developer.nvidia.com/cuda-downloads
2. Verify installation: Open command prompt, run `nvcc --version`
3. Restart VocalFold

**For AMD Users:**
1. Install latest AMD Adrenalin drivers: https://www.amd.com/en/support
2. Verify Vulkan support:
   - Download Vulkan SDK: https://vulkan.lunarg.com/
   - Run `vulkaninfo` to check availability
3. Ensure GPU is RX 6000 series or newer
4. Restart VocalFold

**For Intel Users:**
1. Install latest Intel Graphics drivers: https://www.intel.com/content/www/us/en/download-center/home.html
2. Verify Vulkan support (Arc series required)
3. Restart VocalFold

### Performance Benchmarks
Use these benchmarks to verify GPU is working:

**Test method:**
1. Record 5 seconds of clear speech
2. Use Base model
3. Measure transcription time (shown in console)

**Expected performance:**
- NVIDIA RTX 3080: <1s
- NVIDIA RTX 3060: <1.5s
- AMD RX 6800 XT: <1.5s
- AMD RX 6700 XT: <2s
- AMD RX 5700 XT: <3s
- Intel Arc A750: <2.5s
- CPU (i7-10700K): 5-8s

If your performance is significantly worse, GPU acceleration may not be working correctly.

### Other Issues
- **Hotkey not working**: Check for conflicts with other applications
- **Microphone not detected**: Check Windows sound settings
- **Application crashes**: Check logs, report issue on GitHub

## Technology Stack
- **Language**: F# (.NET 9.0)
- **AI Engine**: Whisper.NET with CUDA and Vulkan runtimes
- **Audio**: NAudio
- **Input Simulation**: InputSimulatorCore
- **Web UI**: F# Fable + React + TailwindCSS
- **Web Server**: Giraffe + ASP.NET Core

## Privacy & Security
- ✅ All processing happens locally on your machine
- ✅ No audio data is sent to external services
- ✅ No recordings stored to disk
- ✅ No telemetry or tracking
- ✅ Offline operation (after initial model download)

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## License
[Your License Here]

## Acknowledgments
- [Whisper.NET](https://github.com/sandrohanea/whisper.net) - .NET bindings for OpenAI's Whisper
- [NAudio](https://github.com/naudio/NAudio) - Audio library for .NET
- OpenAI - Original Whisper model

---

**Made with ❤️ using F#**
