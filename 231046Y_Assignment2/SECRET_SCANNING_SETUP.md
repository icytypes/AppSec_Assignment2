# Secret Scanning Setup Guide

## Where to Find Secret Scanning

Secret scanning in GitHub can be found in different locations depending on your repository type and GitHub plan.

---

## Method 1: Repository Settings (Standard Location)

1. Go to your repository: https://github.com/icytypes/AppSec_Assignment2
2. Click **Settings** (top right)
3. Click **Security** (left sidebar)
4. Scroll down to **Code security and analysis** section
5. Look for **Secret scanning** option

**Note**: If you don't see it, it might be because:
- Your repository is private and you don't have GitHub Advanced Security
- It's located elsewhere (see Method 2)

---

## Method 2: Security Tab (Alternative Location)

1. Go to your repository: https://github.com/icytypes/AppSec_Assignment2
2. Click **Security** tab (top navigation)
3. Look for **Secret scanning** section on the right side
4. Click **Set up** or **Enable** if available

---

## Method 3: GitHub Advanced Security (For Private Repos)

If your repository is **private**, secret scanning requires **GitHub Advanced Security**:

### Option A: Use GitHub Free (Public Repository)

1. Make your repository **public** (temporarily or permanently)
2. Secret scanning is **automatically enabled** for public repositories
3. No additional setup needed

**To make repository public**:
- Go to **Settings** → **General**
- Scroll to **Danger Zone**
- Click **Change visibility** → **Make public**

### Option B: Enable GitHub Advanced Security (For Private Repos)

1. Go to **Settings** → **Security**
2. Look for **GitHub Advanced Security** section
3. Click **Enable** (requires GitHub Pro, Team, or Enterprise plan)
4. Once enabled, secret scanning will be available

---

## What Secret Scanning Does

Secret scanning automatically scans your repository for:
- API keys
- Passwords
- Access tokens
- Private keys
- Database connection strings
- And other secrets

It checks against patterns from:
- AWS
- Azure
- Google Cloud
- GitHub
- And 100+ other service providers

---

## Verification

### Check if Secret Scanning is Active

1. Go to **Security** tab in your repository
2. Look for **Secret scanning** section
3. If enabled, you'll see:
   - Number of secrets found (hopefully 0!)
   - Last scan time
   - Option to view alerts

### Test Secret Scanning (Optional)

If you want to test that it's working:

1. **Create a test commit** with a fake secret:
   ```powershell
   # Create a test file (DO NOT commit real secrets!)
   echo "AWS_ACCESS_KEY_ID=AKIAIOSFODNN7EXAMPLE" > test-secret.txt
   git add test-secret.txt
   git commit -m "Test: Secret scanning detection"
   git push
   ```

2. **Wait a few minutes**
   - GitHub scans commits automatically
   - Usually takes 1-5 minutes

3. **Check Security tab**
   - Go to **Security** → **Secret scanning**
   - You should see an alert for the test secret

4. **Remove the test file**:
   ```powershell
   git rm test-secret.txt
   git commit -m "Remove test secret"
   git push
   ```

**⚠️ WARNING**: Only use fake/example secrets for testing! Never commit real secrets.

---

## If Secret Scanning is Not Available

### For Private Repositories (Free Plan)

If you're on GitHub Free with a private repository:

1. **Option 1**: Make repository public (secret scanning is free for public repos)
2. **Option 2**: Upgrade to GitHub Pro ($4/month) for Advanced Security
3. **Option 3**: Use manual secret scanning tools (see below)

### Manual Secret Scanning Alternatives

If GitHub secret scanning isn't available, you can use:

1. **GitGuardian** (free for public repos)
   - https://www.gitguardian.com/
   - Scans for secrets in your code

2. **TruffleHog** (open source)
   - https://github.com/trufflesecurity/trufflehog
   - Can be run locally or in CI/CD

3. **gitleaks** (open source)
   - https://github.com/gitleaks/gitleaks
   - Command-line tool for secret detection

---

## Current Status Check

### For Your Repository

Since your repository appears to be **private**, secret scanning might not be available unless:
- You have GitHub Advanced Security enabled, OR
- You make the repository public

### Recommended Approach

1. **Check if it's available**:
   - Go to Settings → Security
   - Look for "Secret scanning" option
   - If not visible, it's likely not available for private repos on free plan

2. **If not available, ensure secrets are excluded**:
   - ✅ `appsettings.json` is in `.gitignore` (already done)
   - ✅ `*.secrets.json` is in `.gitignore` (already done)
   - ✅ Database files are excluded (already done)

3. **Use manual verification**:
   - Review code before committing
   - Never commit real secrets
   - Use environment variables for production

---

## Best Practices (Even Without Secret Scanning)

1. **Never commit secrets**:
   - Use `.gitignore` (already configured ✅)
   - Use `appsettings.Example.json` as template
   - Store real secrets in environment variables

2. **Review before committing**:
   - Check `git diff` before committing
   - Look for hardcoded passwords, keys, tokens

3. **Use GitHub Secrets** (for CI/CD):
   - Store secrets in repository Settings → Secrets
   - Reference in workflows: `${{ secrets.SECRET_NAME }}`

4. **Rotate if exposed**:
   - If you accidentally commit a secret, rotate it immediately
   - Remove from Git history if possible

---

## Summary

**For your assignment requirements**:

✅ **CodeQL Analysis**: Enabled (default setup)
✅ **Dependabot**: Should be available
⏳ **Secret Scanning**: 
   - May not be available for private repos on free plan
   - Can make repo public to enable (free)
   - Or ensure secrets are properly excluded (already done ✅)

**Your code is already secure** because:
- `appsettings.json` is excluded from Git
- No hardcoded secrets in code
- Using configuration files properly

---

## Next Steps

1. **Verify CodeQL is working**:
   - Go to **Security** tab → **Code scanning alerts**
   - Should see results after scan completes

2. **Check Dependabot**:
   - Go to **Security** tab → **Dependabot alerts**
   - Should see dependency analysis

3. **For Secret Scanning**:
   - If available, enable it
   - If not available, document that secrets are excluded via `.gitignore`
   - This is acceptable for assignment purposes

---

**Last Updated**: 2026-02-05
