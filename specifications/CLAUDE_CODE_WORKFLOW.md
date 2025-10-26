# Claude Code Workflow Guide

## How to Use This Project Structure with Claude Code

### Quick Start

1. **Copy these files to your project directory**:
   ```
   SPECIFICATION.md
   ARCHITECTURE.md
   TASKS.md
   CONTEXT.md
   ```

2. **Start Claude Code** in your project directory:
   ```bash
   cd C:\path\to\VocalFold
   claude
   ```

3. **First prompt to Claude Code**:
   ```
   Hi! I want to implement the Voice-to-Text application defined in this project. 
   
   Please read:
   - SPECIFICATION.md (requirements)
   - ARCHITECTURE.md (design)
   - TASKS.md (implementation plan)
   - CONTEXT.md (current status)
   
   Then start with Task 1.1 from TASKS.md. Let me know when you're ready to begin.
   ```

---

## Project Structure Benefits

### Why This Structure Works Well with Claude Code

**SPECIFICATION.md** gives Claude:
- ✅ Clear requirements
- ✅ Success criteria
- ✅ Constraints to respect
- ✅ Out-of-scope items to avoid

**ARCHITECTURE.md** gives Claude:
- ✅ Technical decisions already made
- ✅ Module interfaces defined
- ✅ Design patterns to follow
- ✅ Technology stack locked in

**TASKS.md** gives Claude:
- ✅ Step-by-step roadmap
- ✅ Clear acceptance criteria
- ✅ Implementation order
- ✅ Checkpoints for validation

**CONTEXT.md** gives Claude:
- ✅ Session continuity
- ✅ Quick reference
- ✅ Common pitfalls to avoid
- ✅ Current progress tracking

---

## Recommended Claude Code Workflow

### Phase 1: Setup & Planning

**Prompt 1 - Initial Context**:
```
Read SPECIFICATION.md, ARCHITECTURE.md, TASKS.md, and CONTEXT.md.
Confirm you understand:
1. The project goal
2. Technology stack (F#, Whisper.NET library, NAudio, etc.)
3. Key constraints (Windows, CUDA, offline)
4. Implementation plan

Then list the first 3 tasks we'll tackle.
```

**What to expect**: Claude summarizes understanding and confirms next steps

---

### Phase 2: Task-by-Task Implementation

**For each task, use this pattern**:

**Prompt**:
```
Let's implement Task {N}: {Task Name}

Requirements:
- {Requirement 1}
- {Requirement 2}

Acceptance criteria:
- {Criteria 1}
- {Criteria 2}

Please implement this task and let me know when it's done so we can test it.
```

**What to expect**: 
- Claude reads relevant docs
- Implements the task
- Creates/modifies files
- Tells you how to test

**After implementation**:
```
Great! Let me test this. I'll run:
{test command}

[Test it yourself, then report back]

{Result}: ✅ Works / ❌ Error: {description}
```

---

### Phase 3: Integration & Testing

**Prompt pattern**:
```
All modules are complete. Let's integrate them in the main application.

According to TASKS.md Phase 5.2, we need to:
- Wire hotkey → audio → transcription → typing
- Add error handling
- Add startup logging
- Add cleanup on exit

Please implement the main application that ties everything together.
```

---

### Phase 4: Polish & Build

**Prompt**:
```
Let's create the deployment scripts and documentation.

Tasks:
1. Create run.bat for quick start
2. Create build-exe.bat for standalone build
3. Update README with actual test results
4. Verify all documentation is accurate

Please handle these one by one.
```

---

## Tips for Working with Claude Code

### ✅ DO:

1. **Be specific about files**:
   ```
   "Update Program.fs to add the AudioRecorder module"
   ```
   Not: "Add audio recording"

2. **Reference the docs**:
   ```
   "According to ARCHITECTURE.md, the AudioRecorder module should..."
   ```

3. **Test incrementally**:
   After each task, test it before moving on

4. **Update CONTEXT.md**:
   ```
   "Update CONTEXT.md to show we completed Task 2.1"
   ```

5. **Ask Claude to explain**:
   ```
   "Explain how the Windows message loop works in the WinAPI module"
   ```

### ❌ DON'T:

1. **Don't be vague**:
   ❌ "Make it work"
   ✅ "Implement Task 2.1 from TASKS.md"

2. **Don't skip testing**:
   ❌ Implement all tasks then test
   ✅ Test after each task

3. **Don't ignore the structure**:
   ❌ "Just build the whole app"
   ✅ "Follow the tasks in order"

4. **Don't forget context**:
   Remind Claude of constraints if it suggests wrong approach

---

## Handling Common Scenarios

### Scenario 1: Claude Suggests Wrong Technology

**If Claude suggests Python instead of F#**:
```
Stop. According to SPECIFICATION.md and ARCHITECTURE.md, we MUST use F#, 
not Python. The requirement is "Language: F# (functional-first .NET language)".

Please implement this in F# using the specified libraries.
```

### Scenario 2: Implementation Doesn't Match Spec

**If implementation deviates**:
```
This doesn't match the specification. According to SPECIFICATION.md section FR-2,
we must use Whisper.NET AI for transcription, and according to ARCHITECTURE.md,
we're using the Whisper.NET library.

Please revise to match the specification.
```

### Scenario 3: Need to Debug

**When something doesn't work**:
```
Task 2.2 isn't working. The audio recording returns 0 samples.

From ARCHITECTURE.md, we should be using NAudio WaveInEvent with:
- 16kHz sample rate
- Mono channel
- Float32 conversion

Can you check the implementation and fix the issue?
```

### Scenario 4: Want to Add Feature Not in Plan

**When you want something extra**:
```
I want to add voice activity detection to stop recording on silence.
This is listed as "Future Enhancements" in SPECIFICATION.md.

Can we add this feature? If yes, please:
1. Update SPECIFICATION.md to include it in scope
2. Add tasks to TASKS.md
3. Implement it
```

---

## Session Management

### Starting a New Session

**If you return to the project later**:
```
Hi! I'm continuing work on the Voice-to-Text project.

Please read CONTEXT.md to see where we left off.
Current status: {from CONTEXT.md}
Last completed: {from CONTEXT.md}

What should we work on next?
```

### Ending a Session

**Before you stop**:
```
Before we end, please:
1. Update CONTEXT.md with current progress
2. Note which task we completed
3. Suggest what to do next session
4. Commit any changes if we're using git
```

---

## Debugging with Claude Code

### Performance Issues

```
The transcription is taking 5 seconds instead of <1 second.

According to ARCHITECTURE.md, with RTX 3080 we should get ~0.5s for 
5s of speech with Base model.

Can you:
1. Check if CUDA is actually being used
2. Verify we're using the correct model (Base)
3. Add performance logging to identify bottleneck
```

### Build Errors

```
Getting error: "CS0246: The type or namespace name 'Whisper.NET' could not be found"

According to TASKS.md Task 1.2, we should have these packages:
- Whisper.net (1.7.1)
- Whisper.net.Runtime.Cuda (1.7.1)

Can you verify the .fsproj has the correct package references?
```

---

## Project Structure Philosophy

### Why This Approach?

**Problem with "Just build it"**:
- Claude might use wrong technology
- Implementation might not match needs
- Hard to track progress
- Difficult to resume later

**Solution with structured docs**:
- ✅ Clear requirements prevent wrong decisions
- ✅ Technical constraints are explicit
- ✅ Progress is trackable
- ✅ Easy to resume in new session
- ✅ Quality checkpoints built in

### Comparison with Other Methods

| Method | Pros | Cons |
|--------|------|------|
| **"Just build it"** | Fast start | Wrong tech, scope creep, hard to resume |
| **PRD only** | Clear goals | No technical guidance, Claude must decide everything |
| **Code-first** | Quick results | No documentation, hard to modify, unclear requirements |
| **This structure** | Clear path, good docs, resumable | Initial setup time |

---

## Success Indicators

### You'll know this structure works when:

✅ Claude implements correct technology (F#, not Python)  
✅ Claude follows module design from ARCHITECTURE.md  
✅ Claude tests after each task  
✅ You can resume work easily in new session  
✅ Implementation matches specification  
✅ Code quality is high (proper error handling, logging)  
✅ Documentation stays current  

---

## Advanced: Parallel Development

If working with multiple contributors:

1. **Assign phases to different people**
2. **Update CONTEXT.md with ownership**
3. **Use git branches per phase**
4. **Reference same SPECIFICATION.md**

Example CONTEXT.md addition:
```
### Current Assignments
- Phase 2 (Audio): Alice
- Phase 3 (Whisper.NET): Bob  
- Phase 4 (TextInput): Charlie
```

---

## Troubleshooting Claude Code

### If Claude ignores the structure:

```
STOP. Please read SPECIFICATION.md, ARCHITECTURE.md, and TASKS.md 
before continuing.

We're following a structured implementation plan. 
The next task is Task {N} in TASKS.md.

Please implement only that task, following the specifications.
```

### If Claude makes up new requirements:

```
That's not in the specification. According to SPECIFICATION.md,
the requirements are:

{paste relevant section}

Please implement according to the specification only.
```

---

**Remember**: 
- The structure guides Claude
- Be specific in requests
- Test incrementally
- Reference docs frequently
- Update CONTEXT.md

**This structure helps Claude Code give you exactly what you need!**
