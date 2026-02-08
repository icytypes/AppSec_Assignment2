# Quick Start: GitHub Security Setup

Follow these steps to set up GitHub security analysis for your project.

---

## Step 1: Create GitHub Repository

1. Go to https://github.com
2. Click **New repository**
3. Name: `231046Y-Assignment2` (or your preferred name)
4. Description: "Fresh Farm Market - Secure Web Application"
5. Choose **Public** or **Private**
6. **Do NOT** initialize with README (we already have files)
7. Click **Create repository**

---

## Step 2: Push Your Code to GitHub

Open PowerShell in your project directory and run:

```powershell
# Navigate to project directory
cd "C:\Users\yiter\source\repos\231046Y_Assignment2"

# Initialize Git (if not already done)
git init

# Add all files
git add .

# Create initial commit
git commit -m "Initial commit: Fresh Farm Market security application"

# Add GitHub remote (replace with your repository URL)
git remote add origin https://github.com/YOUR-USERNAME/YOUR-REPO-NAME.git

# Push to GitHub
git branch -M main
git push -u origin main
```

**Note**: Replace `YOUR-USERNAME` and `YOUR-REPO-NAME` with your actual GitHub details.

---

## Step 3: Enable GitHub Security Features

1. Go to your repository on GitHub
2. Click **Settings** (top right)
3. Click **Security** (left sidebar)
4. Under **Code security and analysis**, enable:
   - ✅ **Dependabot alerts**
   - ✅ **Dependabot security updates**
   - ✅ **Code scanning**
   - ✅ **Secret scanning**

---

## Step 4: Verify Workflows Are Active

1. Go to **Actions** tab in your repository
2. You should see:
   - **CodeQL Security Analysis** workflow
   - **Security Vulnerability Scan** workflow
3. These will run automatically on:
   - Every push to main/master/develop
   - Every pull request
   - Weekly schedule (CodeQL on Mondays, Security Scan daily)

---

## Step 5: Review Initial Security Scan

1. Wait 5-10 minutes after pushing
2. Go to **Security** tab
3. Check for:
   - **Dependabot alerts**: Dependency vulnerabilities
   - **Code scanning alerts**: Code-level issues
   - **Secret scanning**: Exposed secrets

---

## Step 6: Address Any Findings

### If Dependencies Have Vulnerabilities:

1. Go to **Security** → **Dependabot alerts**
2. Review each alert
3. Dependabot will create PRs automatically
4. Review and merge the PRs

### If Code Issues Are Found:

1. Go to **Security** → **Code scanning alerts**
2. Review each finding
3. See `VULNERABILITY_REMEDIATION_GUIDE.md` for how to fix
4. Fix the issue and push changes
5. Alert will automatically close

### If Secrets Are Found:

1. **IMMEDIATELY** rotate the exposed secret
2. Remove from Git history (or create new repo)
3. Update `.gitignore` to prevent future exposure
4. Use environment variables instead

---

## Step 7: Set Up Notifications

1. Go to repository **Settings** → **Notifications**
2. Enable email notifications for:
   - Security alerts
   - Dependabot alerts
   - Code scanning alerts

Or configure in your GitHub profile settings.

---

## Verification Checklist

After setup, verify:

- [ ] Code pushed to GitHub successfully
- [ ] Security features enabled in repository settings
- [ ] Workflows visible in Actions tab
- [ ] Initial security scan completed
- [ ] No critical/high severity alerts (or they're being addressed)
- [ ] `.gitignore` excludes sensitive files
- [ ] `appsettings.json` is NOT in repository (only `appsettings.Example.json`)

---

## Troubleshooting

### Issue: "Repository not found" when pushing

**Solution**: 
- Check repository URL is correct
- Verify you have push access
- Try: `git remote set-url origin https://github.com/YOUR-USERNAME/YOUR-REPO.git`

### Issue: Workflows not running

**Solution**:
- Check GitHub Actions are enabled in repository settings
- Verify workflow files are in `.github/workflows/` directory
- Check workflow file syntax (YAML)

### Issue: Too many false positives

**Solution**:
- Review each alert carefully
- Dismiss false positives with explanation
- Add code comments explaining why code is safe

### Issue: Can't see Security tab

**Solution**:
- Ensure you have admin access to repository
- Security tab only visible to repository admins
- Check repository visibility settings

---

## Next Steps

1. **Weekly Review**: Check security alerts every Monday
2. **Update Dependencies**: Review and merge Dependabot PRs
3. **Fix Code Issues**: Address high/critical code scanning alerts
4. **Monitor**: Set up notifications for new alerts

---

## Need Help?

- See `GITHUB_SECURITY_SETUP.md` for detailed guide
- See `VULNERABILITY_REMEDIATION_GUIDE.md` for fixing issues
- GitHub Documentation: https://docs.github.com/en/code-security

---

**Last Updated**: 2026-02-05
