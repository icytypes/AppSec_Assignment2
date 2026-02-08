# Repository Verification Guide

## ⚠️ Important: Correct Repository URL

Your code was pushed to a **different repository** than the one you're checking!

### ❌ Wrong Repository (Empty)
- https://github.com/icytypes/231046Y-Assignment2.git
- This repository appears to be empty or different

### ✅ Correct Repository (Your Code is Here)
- https://github.com/icytypes/Appsec_Assignment2.git
- OR: https://github.com/icytypes/AppSec_Assignment2.git
- This is where your code was actually pushed

---

## How to Verify Your Code is Pushed

### Method 1: Check the Correct Repository

1. Go to: **https://github.com/icytypes/Appsec_Assignment2** (or AppSec_Assignment2)
2. You should see:
   - Source code files
   - `.github` folder with workflows
   - Documentation files
   - All your project files

### Method 2: Verify via Git Command

Run this command to see what's in the remote repository:

```powershell
git ls-remote --heads origin
```

This shows the commit hash that's on GitHub.

### Method 3: Check Repository Contents

The correct repository should have:
- ✅ `231046Y_Assignment2/` folder with all source code
- ✅ `.github/workflows/` with CodeQL and security scan workflows
- ✅ `.github/dependabot.yml`
- ✅ `.gitignore`
- ✅ Documentation files (`.md` files)
- ✅ `231046Y_Assignment2.sln`

---

## Why Two Different Repositories?

GitHub showed this message when you pushed:
```
remote: This repository moved. Please use the new location:
remote:   https://github.com/icytypes/AppSec_Assignment2.git
```

This means:
- The repository URL changed/redirected
- Your code is in the **new location** (AppSec_Assignment2)
- The old URL (231046Y-Assignment2) might be empty or a different repo

---

## How to Check CodeQL is Scanning

### Step 1: Go to Correct Repository

Visit: **https://github.com/icytypes/Appsec_Assignment2** (or AppSec_Assignment2)

### Step 2: Check Actions Tab

1. Click **Actions** tab (top navigation)
2. You should see:
   - **CodeQL Security Analysis** workflow runs
   - **Security Vulnerability Scan** workflow runs
3. Click on a workflow run to see:
   - Status (running, completed, failed)
   - Logs and results

### Step 3: Check Security Tab

1. Click **Security** tab (top navigation)
2. Look for:
   - **Code scanning alerts** (CodeQL results)
   - **Dependabot alerts** (dependency vulnerabilities)
   - Workflow status

### Step 4: Verify CodeQL is Enabled

1. Go to **Settings** → **Security**
2. Under **Code security and analysis**
3. Check that **Code scanning** shows:
   - ✅ Enabled
   - Last scan time
   - Number of alerts (if any)

---

## Troubleshooting

### Issue: Repository Appears Empty

**Solution**: You're looking at the wrong repository!
- Check: https://github.com/icytypes/Appsec_Assignment2
- Or: https://github.com/icytypes/AppSec_Assignment2

### Issue: Can't Find CodeQL Results

**Possible reasons**:
1. **Scan hasn't completed yet** (takes 5-10 minutes)
   - Wait a bit longer
   - Check Actions tab for running workflows

2. **CodeQL not enabled**
   - Go to Settings → Security
   - Enable "Code scanning"

3. **Workflow failed**
   - Check Actions tab
   - Click on failed workflow
   - Review error logs

### Issue: No Workflows Running

**Solution**:
1. Check that workflow files exist:
   - `.github/workflows/codeql-analysis.yml`
   - `.github/workflows/security-scan.yml`
2. Verify they're in the repository (check GitHub)
3. Push again if missing:
   ```powershell
   git add .github/
   git commit -m "Add security workflows"
   git push
   ```

---

## Quick Verification Checklist

- [ ] Visited correct repository: https://github.com/icytypes/Appsec_Assignment2
- [ ] Can see source code files in repository
- [ ] Can see `.github` folder with workflows
- [ ] Actions tab shows workflow runs
- [ ] Security tab shows CodeQL scanning enabled
- [ ] Code scanning alerts visible (or "No alerts" if clean)

---

## Next Steps

1. **Visit the correct repository**: https://github.com/icytypes/Appsec_Assignment2
2. **Verify files are there**: Check that you can see your source code
3. **Check Actions tab**: See if workflows are running/completed
4. **Check Security tab**: See CodeQL results

If you still don't see your code, we may need to push again or check the repository settings.

---

**Last Updated**: 2026-02-05
