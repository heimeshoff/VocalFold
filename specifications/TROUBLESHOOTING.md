# Troubleshooting & FAQ

## Common Issues and Solutions

### Installation Issues

#### ❌ "dotnet: command not found" or ".NET SDK not installed"
**Solution**:
1. Download .NET 9.0 SDK from https://dotnet.microsoft.com/download
2. Run the installer
3. Restart your terminal/command prompt
4. Verify: `dotnet --version`

#### ❌ "CUDA not found" or GPU acceleration not working
**Solution**:
1. Install NVIDIA CUDA Toolkit 12.x from https://developer.nvidia.com/cuda-downloads
2. Choose "Express Installation"
3. Restart your computer
4. Verify: Open Command Prompt and run `nvcc --version`
5. If still not working, check Windows Environment Variables include:
   - `CUDA_PATH` pointing to CUDA installation
   - CUDA bin directory in `PATH`

#### ❌ "NuGet package restore failed"
**Solution**:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore again
dotnet restore
```

---

### Runtime Issues

#### ❌ Hotkey (Ctrl+Windows) doesn't work
**Possible Causes & Solutions**:

1. **Another app is using the same hotkey**
   - Try different key combination
   - Edit `Program.fs` line 151 to change hotkey

2. **Application needs administrator privileges**
   - Right-click exe → "Run as Administrator"

3. **Antivirus blocking global hotkeys**
   - Add exception for VocalFold.exe

4. **Application not running**
   - Check if process is running in Task Manager
   - Look for console window

#### ❌ "No microphone detected" or recording fails
**Solutions**:
1. Check Windows microphone permissions:
   - Settings → Privacy → Microphone
   - Enable for desktop apps

2. Verify microphone is default device:
   - Right-click speaker icon → Sounds
   - Recording tab → Set default microphone

3. Test microphone in Windows Voice Recorder app first

4. Check microphone levels:
   - Right-click speaker icon → Sounds
   - Recording tab → Properties → Levels
   - Set to 80-100%

#### ❌ Text doesn't appear / types in wrong location
**Solutions**:
1. **Cursor not focused in text field**
   - Click in the text field BEFORE pressing hotkey
   - Add delay: Edit line 236 for longer delay

2. **Some apps don't accept simulated input**
   - Known issues: Some games, elevated apps
   - Try "Run as Administrator"

3. **Text types too fast/slow**
   - Edit line 133: `interval=0.01` (adjust value)

#### ❌ Poor transcription quality
**Solutions**:
1. **Background noise**
   - Use in quiet environment
   - Use headset microphone
   - Enable noise cancellation in Windows

2. **Speaking too fast/unclear**
   - Speak at normal pace
   - Articulate clearly
   - Speak closer to microphone

3. **Wrong language setting**
   - Edit line 103: `.WithLanguage("en")` to your language

4. **Model too small**
   - Upgrade from Base to Small or Medium
   - Edit line 212: `GgmlType.Small`

5. **Technical terms/jargon**
   - Add custom prompt for context
   - Edit line 103: `.WithPrompt("Technical programming terms")`

#### ❌ Application crashes or freezes
**Solutions**:
1. **Out of memory**
   - Use smaller model (Tiny or Base)
   - Close other GPU applications
   - Check GPU memory: Task Manager → Performance → GPU

2. **CUDA error**
   - Update NVIDIA drivers
   - Reinstall CUDA Toolkit
   - Try CPU mode (remove CUDA package reference)

3. **Audio device error**
   - Restart application
   - Unplug/replug microphone
   - Update audio drivers

---

### Performance Issues

#### ⚠️ Slow transcription (takes >5 seconds)
**Causes & Solutions**:

1. **Not using GPU**
   - Verify CUDA installation
   - Check: Task Manager → Performance → GPU should show activity
   - Reinstall CUDA Toolkit if needed

2. **Model too large for GPU**
   - RTX 3080 can handle up to Medium comfortably
   - Large model may be slow
   - Use Small or Medium for best balance

3. **GPU memory full**
   - Close other GPU applications (games, video editing)
   - Check GPU memory in Task Manager

4. **CPU mode being used**
   - Ensure Whisper.net.Runtime.Cuda package is installed
   - Check build output for CUDA loading

#### ⚠️ High memory usage
**Solutions**:
1. Use smaller model: Tiny (75MB) or Base (150MB)
2. Close unused applications
3. Reduce recording duration
4. Check for memory leaks (restart app daily)

#### ⚠️ First transcription very slow
**This is normal!**
- Model loads on first use (~2-5 seconds)
- Subsequent transcriptions will be fast
- Model stays in memory while app runs

---

## Frequently Asked Questions

### General

**Q: Does this work offline?**  
A: Yes! Everything runs locally. No internet required after initial setup.

**Q: Is my voice data sent anywhere?**  
A: No. All processing happens on your machine. Complete privacy.

**Q: What microphone should I use?**  
A: Any microphone works. USB or headset mics generally give better results than laptop built-in mics.

**Q: Can I use this for other languages?**  
A: Yes! Whisper.NET supports 90+ languages. Edit the language setting in Program.fs.

**Q: How accurate is it?**  
A: With the Base model and clear speech: 90-95% accuracy for English. Upgrade to Small/Medium for better results.

### Configuration

**Q: How do I change the hotkey?**  
A: Edit `Program.fs` around line 151. See README.md "Customization" section.

**Q: Can I have multiple hotkeys?**  
A: Yes! See DEVELOPER_GUIDE.md for examples of multiple hotkey registration.

**Q: How do I use a different Whisper.NET model?**  
A: Edit line 212 in `Program.fs`: Change `GgmlType.Base` to Tiny, Small, Medium, or Large.

**Q: Can I adjust recording length?**  
A: Yes! Edit line 236: Change `recordAudio 10` to your desired seconds.

### Performance

**Q: Which model should I use?**  
A: 
- Start with **Base** (fast, good accuracy)
- Upgrade to **Small** for better accuracy
- Use **Medium** if you need high accuracy and have time
- Use **Tiny** only for very fast transcription of simple speech

**Q: How much VRAM does each model use?**  
A:
- Tiny: ~1GB
- Base: ~1GB  
- Small: ~2GB
- Medium: ~4-5GB
- Large: ~8-10GB

**Q: Will this work on laptop/mobile GPU?**  
A: Yes, if it's NVIDIA with CUDA support. Desktop GPUs are faster.

**Q: Can I use AMD GPU?**  
A: Currently no for GPU acceleration. Whisper.NET requires NVIDIA CUDA for GPU support. Use CPU mode instead.

### Advanced

**Q: Can I add voice commands like "period" or "new line"?**  
A: Yes! See DEVELOPER_GUIDE.md for voice command examples.

**Q: Can I make it stop on silence automatically?**  
A: This requires Voice Activity Detection (VAD). See DEVELOPER_GUIDE.md for implementation guide.

**Q: Can I save transcriptions to a file?**  
A: Not by default, but easy to add. See DEVELOPER_GUIDE.md for logging examples.

**Q: Will this work on Windows 10?**  
A: Yes! Requires .NET 9.0 SDK and CUDA Toolkit.

**Q: Can I run this on Linux or Mac?**  
A: The F# code will work, but global hotkeys and some Windows API calls need to be rewritten. The core Whisper.NET transcription will work fine.

---

## Performance Benchmarks

Typical transcription times on RTX 3080 (for 5 seconds of speech):

| Model | Time | Accuracy | RAM | VRAM |
|-------|------|----------|-----|------|
| Tiny | ~0.3s | Good | 500MB | ~1GB |
| Base | ~0.5s | Better | 800MB | ~1GB |
| Small | ~1.2s | Great | 1.5GB | ~2GB |
| Medium | ~2.5s | Excellent | 3GB | ~5GB |
| Large | ~5s | Best | 5GB | ~10GB |

*Times include audio processing and transcription. First run adds 2-3s for model loading.*

---

## Getting Help

1. **Check this FAQ first**
2. **Review README.md and DEVELOPER_GUIDE.md**
3. **Check application logs** (if logging enabled)
4. **Try with default settings** (Base model, Ctrl+Windows)
5. **Test microphone in other apps** (Windows Voice Recorder)

### Diagnostic Information to Collect

When reporting issues, include:
- Windows version
- .NET SDK version (`dotnet --version`)
- CUDA Toolkit version (`nvcc --version`)
- GPU model
- Whisper.NET model being used
- Error messages (exact text)
- Console output

---

## Known Limitations

1. **Elevated applications**: May not work with apps running as Administrator (unless this app also runs as Admin)
2. **Some games**: Anti-cheat may block simulated keyboard input
3. **Remote desktop**: Hotkeys may not work in RDP sessions
4. **Virtual machines**: GPU passthrough required for CUDA

---

## Tips for Best Results

✅ **DO:**
- Use a good quality microphone
- Speak clearly at normal pace
- Use in quiet environment
- Position microphone 6-12 inches from mouth
- Focus cursor in text field before recording
- Use Base or Small model for best balance
- Keep application running in background

❌ **DON'T:**
- Speak too fast or mumble
- Record in noisy environment
- Use when GPU is under heavy load
- Expect 100% accuracy with technical jargon
- Record very long passages (split into chunks)

---

**Need more help?** Check the code comments in Program.fs for detailed explanations!
