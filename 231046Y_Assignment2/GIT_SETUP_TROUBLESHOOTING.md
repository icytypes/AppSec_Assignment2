# Git Setup Troubleshooting Guide

## Issue: Permission Denied on .vs/ Files

### Problem
```
error: open(".vs/231046Y_Assignment2/FileContentIndex/...vsidx"): Permission denied
error: unable to index file '.vs/...'
fatal: adding files failed
```

### Solution 1: Close Visual Studio (Recommended)

1. **Close Visual Studio completely**
   - Close all Visual Studio windows
   - Check Task Manager to ensure no `devenv.exe` processes are running
   - Kill any remaining processes if needed

2. **Then try again**:
   ```powershell
   git add .
   ```

### Solution 2: Add Files Selectively

If you can't close Visual Studio, add files selectively:

```powershell
# Add .gitignore first (so .vs/ is ignored)
git add .gitignore

# Add project files (this will skip locked .vs/ files)
git add 231046Y_Assignment2/ --ignore-errors

# Add solution file
git add 231046Y_Assignment2.sln

# Check what's staged
git status
```

### Solution 3: Force Ignore .vs/ Directory

If `.vs/` is still being tracked:

```powershell
# Remove .vs/ from Git tracking (if it was previously added)
git rm -r --cached .vs/ 2>$null

# Ensure .gitignore has .vs/
# (It should already be there, but verify)

# Add everything except .vs/
git add . --ignore-errors
```

### Solution 4: Use Git Update-Index

```powershell
# Skip worktree for .vs/ directory
git update-index --skip-worktree .vs/

# Or assume unchanged
git update-index --assume-unchanged .vs/**/*
```

---

## Verify .gitignore is Working

Check that `.vs/` is properly ignored:

```powershell
# Check if .vs/ is in .gitignore
Select-String -Path .gitignore -Pattern "\.vs"

# Should output: .vs/

# Check git status (should not show .vs/)
git status --ignored | Select-String "\.vs"
```

---

## Complete Setup Steps (After Fixing Permission Issue)

1. **Close Visual Studio**

2. **Initialize Git** (if not done):
   ```powershell
   git init
   ```

3. **Add all files**:
   ```powershell
   git add .
   ```

4. **Verify what's staged**:
   ```powershell
   git status
   ```
   - Should NOT show `.vs/` directory
   - Should NOT show `bin/`, `obj/`, `*.db` files
   - Should show source code, documentation, etc.

5. **Create initial commit**:
   ```powershell
   git commit -m "Initial commit: Fresh Farm Market security application"
   ```

6. **Add remote and push**:
   ```powershell
   git remote add origin https://github.com/YOUR-USERNAME/YOUR-REPO.git
   git branch -M main
   git push -u origin main
   ```

---

## Common Issues and Solutions

### Issue: "fatal: pathspec '.gitignore' did not match any files"

**Solution**: The `.gitignore` file might not exist in the root. Check:
```powershell
Test-Path .gitignore
# Should return True

# If False, the .gitignore is in the wrong location
# It should be at: C:\Users\yiter\source\repos\231046Y_Assignment2\.gitignore
```

### Issue: "LF will be replaced by CRLF" warnings

**Solution**: These are just warnings about line endings. They're harmless. To suppress:
```powershell
git config core.autocrlf true
```

### Issue: Still seeing .vs/ in git status

**Solution**: 
1. Ensure `.vs/` is in `.gitignore`
2. Remove from cache:
   ```powershell
   git rm -r --cached .vs/ 2>$null
   ```
3. Close Visual Studio
4. Try again

### Issue: Files locked by another process

**Solution**:
1. Close Visual Studio
2. Close any file explorers with the folder open
3. Check Task Manager for locked processes
4. Restart if necessary

---

## Quick Fix Script

Run this PowerShell script to fix common issues:

```powershell
# Navigate to project directory
cd "C:\Users\yiter\source\repos\231046Y_Assignment2"

# Ensure .gitignore exists and has .vs/
if (Test-Path .gitignore) {
    if (-not (Select-String -Path .gitignore -Pattern "\.vs/" -Quiet)) {
        Add-Content .gitignore "`n## Visual Studio`n.vs/"
    }
}

# Remove .vs/ from cache if it was added
git rm -r --cached .vs/ 2>$null

# Add files (will skip locked files)
git add .gitignore
git add 231046Y_Assignment2/ --ignore-errors
git add 231046Y_Assignment2.sln

# Show status
git status --short | Select-Object -First 20
```

---

## Verification Checklist

After fixing, verify:

- [ ] `.vs/` directory is NOT in `git status`
- [ ] `bin/`, `obj/` directories are NOT in `git status`
- [ ] `*.db` files are NOT in `git status`
- [ ] `appsettings.json` is NOT in `git status` (should be ignored)
- [ ] Source code files ARE in `git status`
- [ ] Documentation files ARE in `git status`
- [ ] `.gitignore` file IS in `git status`

---

## Still Having Issues?

1. **Close ALL applications** that might have files open:
   - Visual Studio
   - VS Code
   - File Explorer windows
   - Any text editors

2. **Restart your computer** (if necessary)

3. **Try the selective add method** (Solution 2 above)

4. **Check file permissions**:
   ```powershell
   Get-Acl .vs/ | Format-List
   ```

---

**Last Updated**: 2026-02-05
