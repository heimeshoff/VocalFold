# Quick Start Guide - 5 Minutes to Voice-to-Text!

## ⚡ Prerequisites (One-Time Setup)

### Step 1: Install .NET 9.0 SDK
1. Go to: https://dotnet.microsoft.com/download/dotnet/9.0
2. Download ".NET 9.0 SDK" (not Runtime)
3. Run installer
4. Restart terminal

### Step 2: Install CUDA Toolkit (for GPU acceleration)
1. Go to: https://developer.nvidia.com/cuda-downloads
2. Select: Windows → x86_64 → 11 → exe (local)
3. Download and run installer (~3GB)
4. Choose "Express Installation"
5. Restart computer

**✓ Done!** These only need to be done once.

---

## 🚀 Running the Application

### Option A: Quick Run (Recommended for First Time)

**Double-click**: `run.bat`

That's it! The script will:
1. Install dependencies
2. Build the project
3. Start the application
4. Download Whisper.NET model on first run (2-5 minutes)

### Option B: Command Line

```bash
# Navigate to project folder
cd path\to\VoiceToText

# First time: restore dependencies
dotnet restore

# Run
dotnet run
```

---

## 🎯 Using Voice-to-Text

### Basic Workflow:

```
1. Start the app (run.bat or dotnet run)
   └→ You'll see: "✓ Hotkey registered: Ctrl+Windows"

2. Click in ANY text field (Word, browser, notepad, etc.)

3. Press: Ctrl+Windows

4. Speak clearly into microphone
   └→ Recording stops automatically after 10 seconds

5. Wait 1-2 seconds

6. Text appears where your cursor was! ✨
```

### Example:
```
You say: "Hello world, this is a test of voice transcription"
   ↓
Types: "Hello world, this is a test of voice transcription"
```

---

## ⚙️ First Run Setup

When you run the app for the first time:

```
[1/3] Downloading Whisper.NET model...
      ████████████████████ 100% (150MB)
      
[2/3] Loading model...
      ✓ Model loaded successfully
      
[3/3] Ready!
      Press Ctrl+Windows to start recording
```

**This takes 2-5 minutes, but only happens once!**

---

## 🎯 Quick Tips

### For Best Results:

| ✅ DO | ❌ DON'T |
|------|---------|
| Speak clearly | Mumble or speak too fast |
| Use quiet room | Record with loud background noise |
| Use headset mic | Use laptop mic if possible |
| Click in text field first | Start recording without focus |
| Wait 1-2 seconds | Expect instant typing |

### Performance:

- **RTX 3080**: Transcribes in ~0.5-1 seconds ⚡
- **First recording**: May take 3-5 seconds (model loading)
- **Subsequent recordings**: Very fast!

---

## 🔧 Common Adjustments

### Change Hotkey
**File**: `Program.fs` (line 151)

Current: `Ctrl+Windows`
```fsharp
// Change to Ctrl+Alt+R:
WinAPI.MOD_CONTROL ||| WinAPI.MOD_ALT, 0x52u
```

### Better Accuracy
**File**: `Program.fs` (line 212)

Current: `GgmlType.Base`
```fsharp
// Change to:
let modelName = GgmlType.Small  // Better accuracy
```

### Longer Recording
**File**: `Program.fs` (line 236)

Current: `recordAudio 10`
```fsharp
// Change to:
let recording = AudioRecorder.recordAudio 30  // 30 seconds
```

---

## 📦 Building Standalone .exe

Want a portable exe? Easy!

**Double-click**: `build-exe.bat`

Result: `bin\Release\net9.0\win-x64\publish\VoiceToText.exe`

- **Size**: ~60MB
- **Portable**: Copy anywhere
- **No .NET required**: Runs on any Windows 11 PC

---

## 🛠️ Troubleshooting

### "CUDA not found"
→ Install CUDA Toolkit, restart PC

### "Microphone not working"
→ Check Windows Settings → Privacy → Microphone

### "Hotkey doesn't work"
→ Try running as Administrator

### "Poor transcription"
→ Upgrade model to Small (see above)

### Need more help?
→ See `TROUBLESHOOTING.md`

---

## 📊 Model Comparison Chart

Choose your model based on needs:

```
SPEED        ←―――――――――→   ACCURACY

Tiny     Base    Small   Medium   Large
⚡⚡⚡     ⚡⚡      ⚡      🐢      🐢🐢
 ⭐⭐      ⭐⭐⭐    ⭐⭐⭐⭐   ⭐⭐⭐⭐⭐  ⭐⭐⭐⭐⭐
 75MB     150MB   500MB   1.5GB   3GB
```

**Recommendation**: Start with **Base**, upgrade if needed.

---

## ✨ What's Next?

### Want to customize?
- See `README.md` for detailed options
- See `DEVELOPER_GUIDE.md` for advanced features

### Want to add features?
- Voice commands ("period", "comma")
- Silence detection
- Multiple hotkeys
- System tray icon
- Configuration file

All examples in `DEVELOPER_GUIDE.md`!

---

## 🎉 You're Ready!

```bash
# Start the app
dotnet run

# Or just double-click
run.bat
```

**Press Ctrl+Windows anywhere and start talking!**

---

## 🆘 Still Stuck?

1. Check `TROUBLESHOOTING.md`
2. Verify .NET installed: `dotnet --version`
3. Verify CUDA installed: `nvcc --version`
4. Test microphone in Windows Voice Recorder first

**Most issues are solved by restarting after CUDA installation!**

---

**Happy transcribing! 🎤→📝**
