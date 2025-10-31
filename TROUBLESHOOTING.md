# VocalFold Troubleshooting Guide

## GPU Acceleration Issues

### Issue: "CPU mode (no GPU detected)" or Slow Transcription

**Symptoms:**
- Transcription is very slow (5-10 seconds for short audio)
- Console shows "CPU mode" or doesn't mention GPU acceleration
- Performance significantly worse than expected benchmarks

**Root Cause:**
GPU acceleration is not working. This can happen if:
- GPU drivers are not installed or outdated
- CUDA Toolkit not installed (NVIDIA users)
- Vulkan runtime not available (AMD/Intel users)
- GPU is too old or unsupported

---

### Solutions by GPU Vendor

#### For NVIDIA GPU Users (CUDA)

**Requirements:**
- NVIDIA RTX 20 series or newer (RTX 2060, 2070, 2080, 3060, 3070, 3080, 4060, 4070, 4080, 4090, etc.)
- NVIDIA CUDA Toolkit 12.x

**Steps to Fix:**

1. **Install NVIDIA CUDA Toolkit 12.x**
   - Download from: https://developer.nvidia.com/cuda-downloads
   - Choose: Windows → x86_64 → your Windows version
   - Run installer and follow instructions
   - Restart computer after installation

2. **Verify CUDA Installation**
   - Open Command Prompt
   - Run: `nvcc --version`
   - Should show CUDA version 12.x
   - If command not found, CUDA is not installed correctly

3. **Update NVIDIA Drivers**
   - Download latest Game Ready or Studio drivers from: https://www.nvidia.com/download/index.aspx
   - Install and restart

4. **Verify GPU is Detected**
   - Open Command Prompt
   - Run: `nvidia-smi`
   - Should show your GPU model and driver version

5. **Restart VocalFold**
   - Close VocalFold completely (exit from system tray)
   - Launch VocalFold again
   - Check console output for GPU detection messages

**Expected Performance (NVIDIA):**
- RTX 4090: <0.3s for 5s speech (Base model)
- RTX 4080: <0.4s
- RTX 3080: <0.5s
- RTX 3060: <1.0s
- RTX 2060: <1.5s

---

#### For AMD GPU Users (Vulkan)

**Requirements:**
- AMD Radeon RX 6000 series or newer (RX 6600, 6700 XT, 6800, 6900 XT, 7600, 7700 XT, 7800 XT, 7900 XTX, etc.)
- Latest AMD Adrenalin drivers with Vulkan support

**Steps to Fix:**

1. **Install Latest AMD Drivers**
   - Download from: https://www.amd.com/en/support
   - Select your GPU model
   - Download "Adrenalin Edition" driver
   - Install and restart

2. **Verify Vulkan Support**
   - Download Vulkan SDK from: https://vulkan.lunarg.com/
   - Install the SDK (or just the runtime)
   - Open Command Prompt
   - Navigate to Vulkan SDK bin folder (e.g., `C:\VulkanSDK\1.3.xxx\Bin`)
   - Run: `vulkaninfo.exe`
   - Should list your AMD GPU and Vulkan capabilities
   - If error or no GPU listed, Vulkan is not working

3. **Check GPU Compatibility**
   - Vulkan works best on RX 6000 series and newer
   - RX 5000 series may work but will be slower
   - Older GPUs (RX 500, Vega) may not support required Vulkan features

4. **Update Windows**
   - Ensure Windows is fully updated
   - Some Vulkan features require recent Windows updates

5. **Restart VocalFold**
   - Close VocalFold completely
   - Launch VocalFold again
   - Check console for "Vulkan Runtime" messages

**Expected Performance (AMD):**
- RX 7900 XTX: <1.0s for 5s speech (Base model)
- RX 6800 XT: <1.5s
- RX 6700 XT: <2.0s
- RX 5700 XT: <3.0s

**Note:** AMD Vulkan performance is generally 2-3x slower than NVIDIA CUDA, but still much faster than CPU.

---

#### For Intel GPU Users (Vulkan)

**Requirements:**
- Intel Arc series GPU (A750, A770, etc.)
- Latest Intel Graphics drivers

**Steps to Fix:**

1. **Install Latest Intel Graphics Drivers**
   - Download from: https://www.intel.com/content/www/us/en/download-center/home.html
   - Search for "Intel Arc Graphics Drivers"
   - Install latest version
   - Restart computer

2. **Verify Vulkan Support**
   - Download Vulkan SDK from: https://vulkan.lunarg.com/
   - Install the SDK
   - Run `vulkaninfo.exe` (see AMD section for details)
   - Should list Intel Arc GPU

3. **Check GPU Compatibility**
   - Intel Arc series is required
   - Older Intel integrated GPUs (UHD, Iris Xe) may not work well

4. **Restart VocalFold**
   - Close and relaunch VocalFold
   - Check console for Vulkan messages

**Expected Performance (Intel):**
- Intel Arc A770: <2.0s for 5s speech (Base model)
- Intel Arc A750: <2.5s

---

### Issue: Application Crashes During Transcription

**Symptoms:**
- VocalFold crashes when trying to transcribe
- Error messages mentioning GPU, CUDA, or Vulkan
- Application works in CPU mode but crashes with GPU

**Possible Causes & Solutions:**

1. **Insufficient VRAM**
   - Base model requires ~2GB VRAM
   - Small model requires ~4GB VRAM
   - Medium/Large models require 8GB+ VRAM
   - **Solution**: Use a smaller model (Tiny or Base) in settings

2. **Conflicting GPU Software**
   - Other applications using GPU heavily (games, video editing, mining)
   - **Solution**: Close other GPU-intensive applications

3. **Corrupted Drivers**
   - **Solution**: Use DDU (Display Driver Uninstaller) to completely remove drivers, then reinstall fresh

4. **Out-of-date Runtime**
   - **Solution**: Update VocalFold to latest version (includes latest runtimes)

---

### Issue: "Vulkan Runtime Not Found" Error

**Symptoms:**
- Error message about missing Vulkan runtime
- Application doesn't start or crashes immediately

**Solutions:**

1. **Install Vulkan Runtime**
   - Download Vulkan SDK: https://vulkan.lunarg.com/
   - Install the runtime components
   - Restart computer

2. **Update GPU Drivers**
   - Most modern GPU drivers include Vulkan runtime
   - Ensure drivers are up-to-date

3. **Check Windows Version**
   - Vulkan requires Windows 10 or newer
   - Ensure Windows is fully updated

---

## Performance Optimization

### Transcription is Slower Than Expected

**Even with GPU acceleration working, transcription may be slower than benchmarks if:**

1. **Wrong Model Size**
   - Larger models (Medium, Large) are slower
   - **Solution**: Use Base or Tiny model for faster transcription
   - Configure in Settings → General → Whisper Model

2. **GPU is Under Load**
   - Other applications using GPU
   - **Solution**: Close other GPU-intensive apps

3. **Thermal Throttling**
   - GPU overheating and reducing performance
   - **Solution**: Improve cooling, clean dust from GPU fans

4. **Power Settings**
   - Windows power plan set to "Power Saver"
   - **Solution**: Change to "High Performance" or "Balanced"

5. **Background Processes**
   - Windows Update, antivirus scans, etc.
   - **Solution**: Run VocalFold when system is idle

---

## Performance Benchmarks

Use these benchmarks to verify your setup is working correctly.

### Test Method
1. Record exactly 5 seconds of clear speech
2. Use Base model
3. Note transcription time shown in console
4. Compare to benchmarks below

### Expected Performance by Hardware

**NVIDIA GPUs (CUDA):**
```
GPU               | Base Model | Small Model | Tiny Model
------------------|------------|-------------|------------
RTX 4090          | 0.3s       | 0.5s        | 0.2s
RTX 4080          | 0.4s       | 0.6s        | 0.2s
RTX 3090          | 0.5s       | 0.8s        | 0.3s
RTX 3080          | 0.5s       | 1.0s        | 0.3s
RTX 3070          | 0.7s       | 1.2s        | 0.4s
RTX 3060          | 1.0s       | 1.5s        | 0.5s
RTX 2080 Ti       | 0.8s       | 1.3s        | 0.4s
RTX 2070          | 1.2s       | 2.0s        | 0.6s
```

**AMD GPUs (Vulkan):**
```
GPU               | Base Model | Small Model | Tiny Model
------------------|------------|-------------|------------
RX 7900 XTX       | 1.0s       | 1.8s        | 0.5s
RX 6900 XT        | 1.5s       | 2.5s        | 0.7s
RX 6800 XT        | 1.5s       | 2.5s        | 0.7s
RX 6700 XT        | 2.0s       | 3.0s        | 1.0s
RX 5700 XT        | 3.0s       | 5.0s        | 1.5s
```

**Intel GPUs (Vulkan):**
```
GPU               | Base Model | Small Model | Tiny Model
------------------|------------|-------------|------------
Arc A770          | 2.0s       | 3.5s        | 1.0s
Arc A750          | 2.5s       | 4.0s        | 1.2s
```

**CPU (Fallback):**
```
CPU               | Base Model | Small Model | Tiny Model
------------------|------------|-------------|------------
i9-12900K         | 5.0s       | 8.0s        | 3.0s
i7-10700K         | 6.0s       | 10.0s       | 4.0s
i5-9600K          | 8.0s       | 15.0s       | 5.0s
```

**If your performance is worse than expected:**
1. Verify GPU acceleration is working (check console logs)
2. Close other applications
3. Try a smaller model
4. Update drivers
5. Check for thermal throttling

---

## Other Common Issues

### Hotkey Not Working

**Symptoms:**
- Pressing Ctrl+Windows (or configured hotkey) does nothing
- No recording starts

**Solutions:**

1. **Hotkey Conflict**
   - Another application using the same hotkey
   - **Solution**: Change hotkey in Settings

2. **Administrator Privileges**
   - VocalFold cannot send keys to elevated applications
   - **Solution**: Run VocalFold as administrator (right-click → Run as administrator)

3. **Hotkey Not Registered**
   - Check console for hotkey registration messages
   - **Solution**: Restart VocalFold

---

### Microphone Not Working

**Symptoms:**
- Recording starts but no audio captured
- Transcription returns empty text

**Solutions:**

1. **Wrong Microphone Selected**
   - Windows may be using wrong microphone
   - **Solution**: Set default microphone in Windows Sound settings

2. **Microphone Muted**
   - Check microphone is not muted in Windows

3. **Microphone Permissions**
   - VocalFold doesn't have microphone permissions
   - **Solution**: Go to Windows Settings → Privacy → Microphone → Allow desktop apps

4. **Test Microphone**
   - Open Sound Recorder app and test recording
   - If Sound Recorder doesn't work, issue is with Windows/mic, not VocalFold

---

### Typing Not Working

**Symptoms:**
- Transcription completes but text doesn't appear
- Text appears in wrong location

**Solutions:**

1. **Focus Lost**
   - Cursor moved between recording and transcription
   - **Solution**: Ensure cursor stays in text field

2. **Application Blocking**
   - Some apps block keyboard simulation (games with anti-cheat)
   - **Solution**: Use in different application

3. **Elevated Application**
   - VocalFold cannot type into admin-level apps
   - **Solution**: Run VocalFold as administrator

---

### Settings Not Saving

**Symptoms:**
- Changes in settings UI don't persist after restart

**Solutions:**

1. **Permission Issues**
   - Cannot write to `%APPDATA%\VocalFold\`
   - **Solution**: Check folder permissions

2. **Corrupted Settings File**
   - `settings.json` is malformed
   - **Solution**: Delete `%APPDATA%\VocalFold\settings.json` (will reset to defaults)

---

## Getting Help

If you're still experiencing issues:

1. **Check Console Output**
   - VocalFold logs detailed information to console
   - Look for error messages

2. **Collect System Information**
   - GPU model and driver version
   - Windows version
   - VocalFold version

3. **Report Issue on GitHub**
   - Go to: https://github.com/yourusername/VocalFold/issues
   - Include:
     - Detailed description of problem
     - Steps to reproduce
     - System information
     - Relevant console output
     - Expected vs actual behavior

4. **Community Support**
   - Check existing GitHub issues for similar problems
   - Search for solutions in Discussions

---

## Advanced Troubleshooting

### Force CPU Mode (Disable GPU)

If GPU is causing crashes, you can force CPU mode:

1. Navigate to VocalFold installation folder
2. Delete or rename:
   - `Whisper.net.Runtime.Cuda.Windows.dll`
   - `Whisper.net.Runtime.Vulkan.dll`
3. Restart VocalFold
4. Will run in CPU mode (slower but stable)

### View Detailed Logs

VocalFold logs to console. To save logs:

1. Run VocalFold from command prompt:
   ```
   VocalFold.exe > vocalfold.log 2>&1
   ```
2. Logs will be saved to `vocalfold.log`
3. Review logs for detailed error information

### Reset All Settings

To reset VocalFold to defaults:

1. Close VocalFold
2. Delete: `%APPDATA%\VocalFold\`
3. Restart VocalFold
4. Reconfigure settings

---

**Last Updated**: 2025-10-31 (Phase 14: AMD GPU Support)
