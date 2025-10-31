# Cross-Platform GPU Support - Technical Options

**Document Version:** 1.0
**Last Updated:** 2025-10-29
**Status:** Technical Analysis & Recommendations

---

## Executive Summary

VocalFold is currently a **Windows-only** application using **NVIDIA CUDA** for GPU acceleration. This document analyzes the feasibility and effort required to support:
1. **macOS** (with Apple Silicon or Intel)
2. **Windows with AMD GPUs** (Radeon RX series)

**TL;DR:** Both are **feasible** but require **moderate to significant effort**. The good news: Whisper.NET already has runtime packages for most scenarios.

---

## Current Architecture

### Technology Stack
| Component | Library | Version | Platform Support |
|-----------|---------|---------|------------------|
| Language | F# | .NET 9.0 | Cross-platform |
| AI Engine | Whisper.NET | 1.7.1 | Cross-platform |
| GPU Runtime | Whisper.net.Runtime.Cuda.Windows | 1.7.1 | **Windows + NVIDIA only** |
| Audio Capture | NAudio | 2.2.1 | **Windows only** |
| Keyboard Sim | InputSimulatorCore | 1.0.5 | **Windows only** |
| Hotkeys | Windows API (P/Invoke) | - | **Windows only** |

### GPU Dependencies
- **Current GPU**: NVIDIA RTX 3080
- **Required**: CUDA Toolkit 12.x on target machine
- **Runtime**: `Whisper.net.Runtime.Cuda.Windows` (bundled native libraries)
- **Code**: No explicit CUDA code - all GPU handling is automatic via Whisper.NET

---

## Option 1: macOS Support

### Overview
Add support for Apple computers running macOS (both Intel and Apple Silicon).

### Technical Approach

#### 1.1 GPU Acceleration - CoreML (Recommended)
**Package**: `Whisper.net.Runtime.CoreML` v1.8.1

**What it provides:**
- Apple Neural Engine acceleration on M1/M2/M3 chips
- GPU acceleration on Intel Macs with compatible GPUs
- Automatic hardware detection and optimization

**Changes required:**
```xml
<!-- Add to VocalFold.fsproj -->
<ItemGroup Condition="$([MSBuild]::IsOSPlatform('OSX'))">
    <PackageReference Include="Whisper.net.Runtime.CoreML" Version="1.8.1" />
</ItemGroup>

<ItemGroup Condition="$([MSBuild]::IsOSPlatform('Windows'))">
    <PackageReference Include="Whisper.net.Runtime.Cuda.Windows" Version="1.8.1" />
</ItemGroup>
```

**Code changes:**
- ✅ **None required** - Whisper.NET automatically selects the correct runtime
- Model loading and inference code remains identical
- `WhisperFactory.FromPath()` works unchanged

#### 1.2 Audio Capture - Replace NAudio

**Current**: NAudio (Windows only)

**Option A: OpenTK.Audio.OpenAL** (Recommended)
```xml
<PackageReference Include="OpenTK.Audio.OpenAL" Version="4.9.4" />
```

**Pros:**
- Mature, cross-platform OpenAL bindings
- Supports Windows, macOS, Linux
- Active maintenance (.NET 9 compatible)
- Similar API concepts to NAudio

**Cons:**
- Different API - requires rewriting `AudioRecorder.fs`
- Lower-level than NAudio (more manual buffer management)

**Option B: Silk.NET.OpenAL**
```xml
<PackageReference Include="Silk.NET.OpenAL" Version="2.22.0" />
```

**Pros:**
- Modern .NET Foundation project
- Excellent .NET 9 support
- More idiomatic C# API

**Cons:**
- Newer library (less community resources)
- Still requires AudioRecorder.fs rewrite

**Estimated effort:**
- Rewrite `AudioRecorder.fs` (260 lines) → **8-12 hours**
- Test on Mac hardware → **4 hours**
- Handle platform-specific audio device enumeration → **3-4 hours**

#### 1.3 Keyboard Simulation - Platform-Specific P/Invoke

**Current**: InputSimulatorCore (Windows SendInput API)

**macOS approach:**
- Use **CGEvent** API via P/Invoke
- Or use **Accessibility API** for keyboard input

**Example F# code:**
```fsharp
module MacKeyboardInput =
    open System.Runtime.InteropServices

    [<DllImport("ApplicationServices")>]
    extern IntPtr CGEventCreateKeyboardEvent(IntPtr source, uint16 virtualKey, bool keyDown)

    [<DllImport("ApplicationServices")>]
    extern void CGEventPost(int tap, IntPtr event)

    let typeText (text: string) =
        // Implementation using CGEvent
        ()
```

**Estimated effort:**
- Create macOS keyboard module → **6-8 hours**
- Handle Unicode properly → **2-3 hours**
- Platform detection and routing → **2 hours**

#### 1.4 Global Hotkeys - Platform-Specific

**Current**: Windows API (RegisterHotKey)

**macOS approach:**
- Use **Carbon HotKey API** (legacy but still works)
- Or use **NSEvent** global monitor (requires accessibility permissions)

**Security consideration:**
- macOS requires **Accessibility Permissions** for:
  - Global keyboard monitoring
  - Keyboard event injection
- App must request permissions at runtime

**Estimated effort:**
- Implement macOS hotkey registration → **8-10 hours**
- Handle permission requests gracefully → **3-4 hours**
- Test with different macOS versions → **4 hours**

### Summary: macOS Support

| Component | Change Type | Effort | Risk |
|-----------|-------------|--------|------|
| GPU Runtime | Package swap | 1 hour | **Low** |
| Audio Capture | Rewrite module | 12-16 hours | **Medium** |
| Keyboard Sim | New platform module | 10-12 hours | **Medium** |
| Global Hotkeys | New platform module | 15-18 hours | **High** |
| Testing & Polish | Cross-platform testing | 12-16 hours | **Medium** |
| **TOTAL** | | **50-63 hours** | |

**Feasibility:** ✅ **YES - Feasible**

**Challenges:**
- macOS security model requires explicit permissions
- Different audio device naming/enumeration
- Testing requires Mac hardware
- App distribution (code signing, notarization)

**Performance expectations:**
- **M1/M2/M3 Macs**: Excellent (Neural Engine is very fast)
- **Intel Macs**: Good (CoreML GPU acceleration)
- Similar or better than NVIDIA GPU performance on Apple Silicon

---

## Option 2: Windows + AMD GPU Support

### Overview
Enable GPU acceleration for AMD Radeon graphics cards on Windows.

### Technical Approach

#### 2.1 GPU Acceleration - Vulkan (Recommended)

**Package**: `Whisper.net.Runtime.Vulkan` v1.8.1

**What it provides:**
- Cross-vendor GPU support (AMD, NVIDIA, Intel)
- Vulkan API support (AMD has excellent Vulkan drivers)
- Works on Windows x64

**Changes required:**
```xml
<!-- Add Vulkan runtime alongside CUDA -->
<PackageReference Include="Whisper.net.Runtime.Vulkan" Version="1.8.1" />
<PackageReference Include="Whisper.net.Runtime.Cuda.Windows" Version="1.8.1" />
```

**Runtime selection logic:**
- Whisper.NET tries runtimes in order: CUDA → Vulkan → CPU
- Automatic fallback if CUDA not available
- **No code changes required** - works automatically

**Requirements:**
- User must have **Vulkan drivers** installed (usually included with AMD drivers)
- AMD Radeon RX 400 series or newer (Vulkan 1.0+ support)

**Code changes:**
- ✅ **None required** - Whisper.NET handles it automatically

**Estimated effort:**
- Add package reference → **15 minutes**
- Test on AMD hardware → **2 hours**
- Document requirements → **1 hour**

#### 2.2 Alternative: DirectML (Microsoft's Cross-GPU Solution)

**Status:** Not directly supported by Whisper.NET (as of v1.8.1)

**What is DirectML:**
- Microsoft's DirectX 12 ML acceleration layer
- Supports NVIDIA, AMD, Intel GPUs
- Used by Windows ML and ONNX Runtime

**Implementation approach:**
- Would require switching from Whisper.NET to **ONNX Runtime**
- Convert Whisper model to ONNX format
- Use ONNX Runtime with DirectML ExecutionProvider

**Estimated effort:**
- Full ONNX Runtime integration → **40-60 hours**
- Model conversion pipeline → **10-15 hours**
- Testing & optimization → **15-20 hours**
- **TOTAL**: **65-95 hours**

**Verdict:** ❌ **Not recommended** - Vulkan is much simpler and already integrated

#### 2.3 Alternative: ROCm (AMD's CUDA Equivalent)

**Status:** ROCm Windows support is **in preview** (Q3 2025 for PyTorch)

**Challenges:**
- Whisper.NET doesn't have ROCm runtime package yet
- ROCm Windows support is immature compared to Linux
- Limited to newer AMD GPUs (RDNA 2+)

**Verdict:** ❌ **Not recommended** - Too bleeding edge, use Vulkan instead

### Summary: AMD GPU Support

| Component | Change Type | Effort | Risk |
|-----------|-------------|--------|------|
| GPU Runtime | Add Vulkan package | 15 min | **Very Low** |
| Audio Capture | No change needed | 0 hours | **None** |
| Keyboard Sim | No change needed | 0 hours | **None** |
| Global Hotkeys | No change needed | 0 hours | **None** |
| Testing & Docs | AMD hardware testing | 3-4 hours | **Low** |
| **TOTAL** | | **3-4 hours** | |

**Feasibility:** ✅ **YES - Very Feasible**

**Challenges:**
- Minimal - this is the easiest option
- Requires AMD GPU hardware for testing
- User must have recent AMD drivers

**Performance expectations:**
- **AMD RX 6000/7000 series**: Good performance (comparable to NVIDIA equivalents)
- **AMD RX 5000 series**: Moderate performance
- **Older AMD GPUs**: May fall back to CPU (Vulkan support varies)

---

## Option 3: Full Cross-Platform (Windows + macOS + Linux)

### Overview
Support all three major platforms with unified codebase.

### Technical Approach

#### 3.1 GPU Acceleration Strategy

**Multi-runtime approach:**
```xml
<!-- Platform-specific runtime packages -->
<ItemGroup Condition="$([MSBuild]::IsOSPlatform('Windows'))">
    <PackageReference Include="Whisper.net.Runtime.Cuda.Windows" Version="1.8.1" />
    <PackageReference Include="Whisper.net.Runtime.Vulkan" Version="1.8.1" />
</ItemGroup>

<ItemGroup Condition="$([MSBuild]::IsOSPlatform('OSX'))">
    <PackageReference Include="Whisper.net.Runtime.CoreML" Version="1.8.1" />
</ItemGroup>

<ItemGroup Condition="$([MSBuild]::IsOSPlatform('Linux'))">
    <PackageReference Include="Whisper.net.Runtime.Cuda.Linux" Version="1.8.1" />
    <PackageReference Include="Whisper.net.Runtime.Vulkan" Version="1.8.1" />
</ItemGroup>
```

**Runtime priority per platform:**
- **Windows**: CUDA → Vulkan → CPU
- **macOS**: CoreML → CPU
- **Linux**: CUDA → Vulkan → CPU

#### 3.2 Platform Abstraction Layer

**Create platform-agnostic interfaces:**

```fsharp
// IAudioRecorder.fs
type IAudioRecorder =
    abstract member StartRecording: unit -> unit
    abstract member StopRecording: unit -> RecordingResult
    abstract member GetAvailableDevices: unit -> AudioDevice list

// IKeyboardSimulator.fs
type IKeyboardSimulator =
    abstract member TypeText: string -> float32 -> unit

// IHotkeyManager.fs
type IHotkeyManager =
    abstract member RegisterHotkey: ModifierKeys -> Keys -> (unit -> unit) -> unit
    abstract member UnregisterAll: unit -> unit
```

**Platform-specific implementations:**
- `WindowsAudioRecorder.fs` (NAudio)
- `MacAudioRecorder.fs` (OpenTK.Audio.OpenAL)
- `LinuxAudioRecorder.fs` (OpenTK.Audio.OpenAL)
- Similar for keyboard and hotkeys

#### 3.3 Dependency Injection

**Modify Program.fs:**
```fsharp
let createPlatformServices() =
    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        {| Audio = WindowsAudioRecorder()
           Keyboard = WindowsKeyboard()
           Hotkeys = WindowsHotkeys() |}
    elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
        {| Audio = MacAudioRecorder()
           Keyboard = MacKeyboard()
           Hotkeys = MacHotkeys() |}
    elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
        {| Audio = LinuxAudioRecorder()
           Keyboard = LinuxKeyboard()
           Hotkeys = LinuxHotkeys() |}
    else
        failwith "Unsupported platform"
```

### Summary: Full Cross-Platform

| Component | Change Type | Effort | Risk |
|-----------|-------------|--------|------|
| GPU Runtime | Multi-package config | 2 hours | **Low** |
| Platform Interfaces | New abstraction layer | 8-10 hours | **Medium** |
| Windows Impl | Adapt existing code | 4-6 hours | **Low** |
| macOS Impl | New implementation | 40-50 hours | **High** |
| Linux Impl | New implementation | 35-45 hours | **High** |
| Build System | Multi-target config | 6-8 hours | **Medium** |
| Testing & CI | Platform testing matrix | 20-30 hours | **High** |
| **TOTAL** | | **115-151 hours** | |

**Feasibility:** ✅ **YES - But High Effort**

**Challenges:**
- Maintaining 3 platform implementations
- Testing matrix complexity (3 OS × multiple GPU types)
- Platform-specific bugs and quirks
- Distribution complexity (3 different installers)

---

## Comparison Matrix

| Criteria | Windows + AMD GPU | macOS Only | Full Cross-Platform |
|----------|-------------------|------------|---------------------|
| **Effort** | 3-4 hours | 50-63 hours | 115-151 hours |
| **Code Changes** | Minimal | Moderate | Extensive |
| **New GPU Support** | AMD (Vulkan) | Apple (CoreML) | All vendors |
| **New OS Support** | None | macOS | macOS + Linux |
| **Testing Complexity** | Low | Medium | High |
| **Maintenance Burden** | Very Low | Medium | High |
| **User Benefit** | Windows AMD users | Mac users | Maximum reach |
| **Risk Level** | Very Low | Medium | High |
| **ROI** | High | Medium-High | Medium |

---

## Recommendations

### Recommendation 1: Quick Win - Add AMD Support (Vulkan)
**Priority:** 🟢 **HIGH**
**Effort:** 3-4 hours
**Impact:** Enables all Windows users with AMD GPUs

**Action items:**
1. Add `Whisper.net.Runtime.Vulkan` package reference
2. Update documentation with AMD GPU support notice
3. Test on AMD Radeon RX 6000+ series
4. Update system requirements documentation

**Why do this first:**
- Minimal effort, high reward
- No breaking changes
- Existing Windows users benefit immediately
- Natural fallback for NVIDIA users with driver issues

### Recommendation 2: Medium Term - Add macOS Support
**Priority:** 🟡 **MEDIUM**
**Effort:** 50-63 hours (2 weeks of dedicated work)
**Impact:** Opens Mac user market

**Action items:**
1. Create platform abstraction interfaces
2. Implement macOS audio recording (OpenTK)
3. Implement macOS keyboard simulation
4. Implement macOS hotkey registration
5. Set up macOS build pipeline
6. Test on both Intel and Apple Silicon Macs
7. Handle macOS security permissions properly

**When to do this:**
- After AMD support is stable
- If there's demand from Mac users
- If team has Mac hardware for testing

### Recommendation 3: Long Term - Linux Support
**Priority:** 🔴 **LOW**
**Effort:** 35-45 hours
**Impact:** Desktop Linux users (smaller market)

**Defer because:**
- Smaller user base for desktop voice-to-text
- Linux audio/input APIs are more fragmented
- Focus should be on Mac first (larger market)

---

## Technical Risks & Mitigations

### Risk 1: GPU Performance Variations
**Risk:** AMD Vulkan performance may differ from NVIDIA CUDA

**Mitigation:**
- Benchmark on actual AMD hardware (RX 6700 XT or better)
- Document expected performance by GPU tier
- Provide CPU fallback documentation
- Set user expectations in README

### Risk 2: macOS Security Permissions
**Risk:** Users may not grant Accessibility permissions

**Mitigation:**
- Clear onboarding flow explaining why permissions needed
- Graceful degradation if permissions denied
- Link to macOS security documentation
- Test permission prompts on multiple macOS versions

### Risk 3: Platform-Specific Audio Latency
**Risk:** OpenTK audio may have different latency characteristics

**Mitigation:**
- Tune buffer sizes per platform
- Add configurable latency settings
- Document platform differences
- Consider platform-specific optimizations

### Risk 4: Maintenance Overhead
**Risk:** Supporting 3+ platforms increases bug surface area

**Mitigation:**
- Strong abstraction layer with clear interfaces
- Comprehensive unit tests for each platform
- CI/CD with multi-platform builds
- Clear documentation for platform-specific code

---

## Alternative Architecture: ONNX Runtime

### Overview
Complete rewrite using ONNX Runtime instead of Whisper.NET

### When to consider:
- If Whisper.NET doesn't meet performance needs
- If need bleeding-edge GPU optimization
- If need NPU support (Copilot+ PCs with Snapdragon X Elite)

### Pros:
- DirectML support (Microsoft-backed, cross-GPU)
- Potentially better performance with optimization
- NPU acceleration on ARM-based Windows devices
- More control over model execution

### Cons:
- Complete rewrite of TranscriptionService.fs
- Must handle model conversion (PyTorch → ONNX)
- More complex API (lower level)
- Model quantization required for optimal performance
- Estimated effort: **80-120 hours**

### Verdict:
❌ **Not recommended** - Whisper.NET + Vulkan is sufficient for 95% of use cases

---

## Implementation Roadmap

### Phase 1: AMD GPU Support (Week 1)
- [ ] Add Vulkan runtime package
- [ ] Test on AMD RX 6700 XT
- [ ] Update documentation
- [ ] Release as v1.1

### Phase 2: macOS Foundation (Weeks 2-3)
- [ ] Create platform abstraction interfaces
- [ ] Implement macOS audio recording
- [ ] Test basic recording on Mac

### Phase 3: macOS Input & Hotkeys (Weeks 4-5)
- [ ] Implement macOS keyboard simulation
- [ ] Implement macOS hotkey registration
- [ ] Handle permission requests

### Phase 4: macOS Polish (Week 6)
- [ ] Cross-platform testing
- [ ] Performance optimization
- [ ] Documentation updates
- [ ] Release as v2.0 (cross-platform)

### Phase 5: Linux (Optional, Weeks 7-8)
- [ ] Linux audio implementation
- [ ] Linux input/hotkey implementation
- [ ] Package for common distros
- [ ] Release as v2.1

---

## Cost-Benefit Analysis

### Windows + AMD GPU Support
**Cost:** 1 day of development
**Benefit:** +15-20% potential users (AMD GPU owners)
**ROI:** 🟢 **Excellent**

### macOS Support
**Cost:** 2-3 weeks of development
**Benefit:** +25-30% potential users (Mac market share)
**ROI:** 🟢 **Good** (if Mac users are target audience)

### Linux Support
**Cost:** 1-2 weeks of development
**Benefit:** +2-5% potential users (desktop Linux market share)
**ROI:** 🟡 **Moderate** (niche but enthusiastic user base)

---

## Testing Requirements

### For AMD GPU Support
- [ ] Test on AMD Radeon RX 6700 XT or better
- [ ] Test on AMD Radeon RX 5000 series
- [ ] Benchmark transcription times vs. NVIDIA
- [ ] Test CPU fallback when Vulkan unavailable

### For macOS Support
- [ ] Test on M1/M2/M3 Mac (Apple Silicon)
- [ ] Test on Intel Mac (if supporting older hardware)
- [ ] Test with macOS 13 Ventura (minimum)
- [ ] Test with macOS 14 Sonoma
- [ ] Test with macOS 15 Sequoia
- [ ] Verify Accessibility permission flow
- [ ] Test audio device enumeration
- [ ] Benchmark CoreML performance

### For Full Cross-Platform
- All of the above, plus:
- [ ] Test on Ubuntu 22.04 LTS / 24.04 LTS
- [ ] Test on Fedora 40+
- [ ] Test on Arch Linux
- [ ] Verify consistent behavior across platforms

---

## Frequently Asked Questions

### Q: Can I run VocalFold on Mac right now?
**A:** No. The current version (v1.x) is Windows-only due to dependencies on Windows-specific APIs for audio, keyboard input, and hotkeys.

### Q: Will Vulkan work with my NVIDIA GPU too?
**A:** Yes! Vulkan supports NVIDIA, AMD, and Intel GPUs. However, the CUDA runtime is typically faster on NVIDIA hardware, so CUDA will be preferred if available.

### Q: What about older AMD GPUs (like RX 500 series)?
**A:** It may work, but performance will be slower. Vulkan support on older AMD GPUs (pre-RDNA) is less optimized. Recommend RX 6000+ for best experience.

### Q: Does macOS version support Intel Macs or just Apple Silicon?
**A:** CoreML works on both Intel and Apple Silicon Macs, but Apple Silicon (M1/M2/M3) will have significantly better performance due to the Neural Engine.

### Q: Can I use this on a Chromebook or iPad?
**A:** No. This is a desktop application targeting Windows, macOS, and potentially Linux. Mobile/web platforms would require a complete architectural redesign.

### Q: What about Windows on ARM (Snapdragon X Elite)?
**A:** Whisper.NET doesn't currently have ARM64 runtime packages. This would require additional investigation, possibly using ONNX Runtime with NPU support.

---

## Conclusion

**Is cross-platform support possible?** ✅ **Yes, absolutely.**

**Is it worth the effort?** It depends on your goals:

1. **AMD GPU support** → ✅ Do it now (4 hours)
2. **macOS support** → ✅ Do it if targeting Mac users (50-63 hours)
3. **Linux support** → ⚠️ Consider later (35-45 hours)

**Recommended approach:**
1. Start with Vulkan for AMD GPU support (quick win)
2. If successful and there's demand, pursue macOS support
3. Defer Linux until requested by users

The architecture of VocalFold is well-suited for cross-platform expansion thanks to Whisper.NET's excellent multi-runtime support. The main work is adapting platform-specific I/O (audio, keyboard, hotkeys) rather than the core AI functionality.

---

**Next Steps:**
1. Review this document with stakeholders
2. Decide on priority (AMD? Mac? Both?)
3. Allocate development resources
4. Begin with Phase 1 (AMD/Vulkan support)

**Questions?** Open an issue on GitHub or contact the development team.
