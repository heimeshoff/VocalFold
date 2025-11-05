# VocalFold Release Guide

This guide explains how to create a new release for VocalFold.

## Release Process Overview

1. **Manually update CHANGELOG.md**
2. **Commit and push changes**
3. **Create and push a version tag**
4. **GitHub Actions automatically builds and publishes the release**

---

## Step-by-Step Instructions

### 1. Update CHANGELOG.md

Edit `VocalFold.WebUI/src/CHANGELOG.md` and add a new version section at the top:

```markdown
# Changelog

## [1.2.0] - 2025-11-05
### Added
- New feature description here
- Another new feature

### Changed
- Description of changes

### Fixed
- Bug fix description

## [1.1.0] - 2025-11-03
...
```

**Important:**
- Use the [Keep a Changelog](https://keepachangelog.com/) format
- The version number in brackets `[x.y.z]` must match the git tag you'll create
- Use today's date in `YYYY-MM-DD` format

### 2. Commit and Push Changes

```bash
git add VocalFold.WebUI/src/CHANGELOG.md
git commit -m "docs: update changelog for v1.2.0"
git push origin main
```

### 3. Create and Push a Version Tag

```bash
# Create the tag (use the same version as in CHANGELOG.md)
git tag v1.2.0

# Push the tag to trigger the release workflow
git push origin v1.2.0
```

### 4. Monitor the Release Workflow

1. Go to: https://github.com/heimeshoffIT/VocalFold/actions
2. Watch the "Build & Release" workflow
3. When complete, the release will appear at: https://github.com/heimeshoffIT/VocalFold/releases

---

## What Happens Automatically

When you push a version tag, GitHub Actions will:

✅ **Verify** CHANGELOG.md contains the version entry
✅ **Generate** Changelog.fs component from CHANGELOG.md (syncs UI)
✅ **Update** version numbers in package.json, installer.iss, Directory.Build.props
✅ **Build** WebUI (Fable/React)
✅ **Publish** .NET application (self-contained, single-file)
✅ **Create** Windows installer with Inno Setup
✅ **Create** ZIP archive of the application
✅ **Publish** GitHub Release with installer + ZIP
✅ **Commit** back the generated Changelog.fs and updated version files

---

## Release Artifacts

Each release includes:

- **VocalFold-Setup-X.X.X.exe** - Windows installer
- **VocalFold-X.X.X.zip** - Portable version (no installation required)

---

## Pre-releases

To create a pre-release (marked as "pre-release" on GitHub), use a tag suffix:

```bash
git tag v1.2.0-rc1    # Release Candidate
git tag v1.2.0-beta1  # Beta version
git tag v1.2.0-alpha1 # Alpha version
```

---

## Troubleshooting

### "Version [x.y.z] not found in CHANGELOG.md"

**Solution:** Make sure the version in your git tag matches exactly with the version in CHANGELOG.md:

```markdown
## [1.2.0] - 2025-11-05  ← Must match tag v1.2.0
```

### Workflow fails during build

1. Check the Actions tab for detailed error logs
2. Fix the issue in your code
3. Create a new patch version tag (e.g., v1.2.1)

### Need to delete/redo a release

```bash
# Delete local tag
git tag -d v1.2.0

# Delete remote tag
git push origin :refs/tags/v1.2.0

# Delete the GitHub release manually from the Releases page

# Then recreate the tag
git tag v1.2.0
git push origin v1.2.0
```

---

## Version Numbering

VocalFold follows [Semantic Versioning](https://semver.org/):

- **Major** (1.0.0): Breaking changes
- **Minor** (1.1.0): New features, backwards compatible
- **Patch** (1.1.1): Bug fixes, backwards compatible

---

## Optional: Helper Script for Changelog

If you want to see commits since last release to help write the changelog:

```bash
# Get the last tag
$lastTag = git describe --tags --abbrev=0

# Show commits since last tag
git log $lastTag..HEAD --oneline --no-merges
```

This helps you remember what changed, but you still write the changelog entries manually.
