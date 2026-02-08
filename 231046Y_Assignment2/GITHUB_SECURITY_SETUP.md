# GitHub Security Analysis Setup Guide

This guide explains how to set up and use GitHub's security features to analyze and secure your source code.

---

## Prerequisites

1. **GitHub Account**: Create a free GitHub account at https://github.com
2. **Git Installed**: Ensure Git is installed on your local machine
3. **GitHub Repository**: Create a new repository on GitHub for this project

---

## Step 1: Initialize Git Repository

### 1.1 Initialize Git in Your Project

```bash
cd "C:\Users\yiter\source\repos\231046Y_Assignment2"
git init
git add .
git commit -m "Initial commit: Fresh Farm Market security application"
```

### 1.2 Connect to GitHub Repository

```bash
# Replace 'your-username' and 'your-repo-name' with your actual GitHub details
git remote add origin https://github.com/your-username/your-repo-name.git
git branch -M main
git push -u origin main
```

---

## Step 2: Enable GitHub Security Features

### 2.1 Enable Dependabot Alerts

1. Go to your GitHub repository
2. Click **Settings** → **Security**
3. Under **Code security and analysis**, enable:
   - ✅ **Dependabot alerts** - Automatically detects vulnerable dependencies
   - ✅ **Dependabot security updates** - Automatically creates PRs for security updates
   - ✅ **Code scanning** - Enables CodeQL analysis

### 2.2 Enable Secret Scanning

1. In **Settings** → **Security**
2. Enable **Secret scanning** - Detects accidentally committed secrets (API keys, passwords, etc.)

---

## Step 3: Configure Security Workflows

The following workflows have been created in `.github/workflows/`:

### 3.1 CodeQL Analysis (`codeql-analysis.yml`)

**What it does:**
- Performs static code analysis to find security vulnerabilities
- Scans for common security issues (SQL injection, XSS, etc.)
- Runs automatically on push, PR, and weekly schedule

**How to use:**
1. The workflow is already configured in `.github/workflows/codeql-analysis.yml`
2. Push your code to GitHub
3. Go to **Security** tab → **Code scanning alerts**
4. Review any findings

### 3.2 Security Vulnerability Scan (`security-scan.yml`)

**What it does:**
- Checks for known vulnerabilities in NuGet packages
- Identifies outdated packages
- Generates dependency reports

**How to use:**
1. The workflow runs automatically
2. Check **Actions** tab for results
3. Review dependency reports

### 3.3 Dependabot Configuration (`dependabot.yml`)

**What it does:**
- Automatically checks for package updates
- Creates pull requests for security updates
- Groups related updates together

**How to use:**
1. Dependabot runs automatically based on schedule
2. Check **Dependabot** tab in repository
3. Review and merge security update PRs

---

## Step 4: Review Security Alerts

### 4.1 View Dependabot Alerts

1. Go to **Security** tab → **Dependabot alerts**
2. Review list of vulnerabilities
3. Click on an alert to see:
   - Severity (Critical, High, Medium, Low)
   - Affected packages
   - Recommended fix
   - CVE details

### 4.2 View Code Scanning Alerts

1. Go to **Security** tab → **Code scanning alerts**
2. Review findings from CodeQL analysis
3. Each alert shows:
   - Severity level
   - Location in code
   - Description of the issue
   - Suggested fix

### 4.3 View Secret Scanning Alerts

1. Go to **Security** tab → **Secret scanning**
2. Review any detected secrets
3. **IMPORTANT**: If secrets are found, rotate them immediately

---

## Step 5: Address Security Vulnerabilities

### 5.1 Fix Dependency Vulnerabilities

**Method 1: Use Dependabot PRs**
1. Dependabot creates PRs automatically for security updates
2. Review the PR
3. Test the changes
4. Merge the PR

**Method 2: Manual Update**
```bash
# Check for vulnerable packages
dotnet list package --vulnerable

# Update a specific package
dotnet add package PackageName --version LatestVersion

# Update all packages
dotnet list package --outdated
# Then update each package individually
```

### 5.2 Fix CodeQL Findings

1. Review CodeQL alerts in **Security** tab
2. Click on an alert to see:
   - Exact location in code
   - Problem description
   - Example fix
3. Apply the fix in your code
4. Commit and push changes
5. CodeQL will re-scan automatically

### 5.3 Common Security Issues and Fixes

#### SQL Injection Prevention
- ✅ **Already implemented**: Using Entity Framework Core (parameterized queries)
- **If CodeQL flags raw SQL**: Ensure all queries use EF Core

#### XSS Prevention
- ✅ **Already implemented**: HTML encoding in `InputSanitizationService`
- **If CodeQL flags user input**: Ensure all output is HTML-encoded

#### Sensitive Data Exposure
- ✅ **Already implemented**: Passwords hashed, credit cards encrypted
- **If CodeQL flags secrets**: Move to `appsettings.json` or environment variables

#### Weak Cryptography
- ✅ **Already implemented**: SHA-256 for passwords, AES-256 for encryption
- **If CodeQL flags**: Ensure using strong algorithms

---

## Step 6: Continuous Security Monitoring

### 6.1 Weekly Security Review

1. **Check Dependabot Alerts** (every Monday)
   - Review new vulnerabilities
   - Update affected packages

2. **Review CodeQL Findings** (weekly)
   - Check for new code issues
   - Fix high/critical severity issues immediately

3. **Update Dependencies** (monthly)
   - Review outdated packages
   - Update to latest stable versions

### 6.2 Security Best Practices

1. **Never commit secrets**:
   - Use `.gitignore` to exclude `appsettings.Development.json`
   - Use environment variables for production secrets
   - Use GitHub Secrets for CI/CD

2. **Review PRs carefully**:
   - Check for security implications
   - Run security scans before merging

3. **Keep dependencies updated**:
   - Review Dependabot PRs promptly
   - Test updates before merging

4. **Monitor security alerts**:
   - Set up email notifications for critical alerts
   - Address high/critical issues within 24 hours

---

## Step 7: Using GitHub Security Features

### 7.1 Security Overview Dashboard

1. Go to **Security** tab in your repository
2. View **Security overview** dashboard showing:
   - Number of open alerts
   - Vulnerable dependencies
   - Code scanning findings
   - Secret scanning alerts

### 7.2 Security Policies

Create a `SECURITY.md` file to:
- Define security reporting process
- Set expectations for security updates
- Provide contact information for security issues

### 7.3 Security Advisories

For critical vulnerabilities:
1. Go to **Security** → **Security advisories**
2. Create a new advisory
3. Coordinate disclosure with security team
4. Publish when ready

---

## Step 8: Automated Security Workflows

### 8.1 Manual Workflow Trigger

You can manually trigger security scans:

1. Go to **Actions** tab
2. Select **CodeQL Security Analysis** or **Security Vulnerability Scan**
3. Click **Run workflow**
4. Select branch and click **Run workflow**

### 8.2 Schedule Customization

Edit `.github/workflows/*.yml` files to customize:
- Schedule frequency
- Branches to scan
- Notification settings

---

## Troubleshooting

### Issue: Dependabot not creating PRs

**Solution:**
1. Check `.github/dependabot.yml` exists
2. Verify package ecosystem is correct (`nuget` for .NET)
3. Ensure repository has dependency files committed

### Issue: CodeQL not running

**Solution:**
1. Check workflow file syntax
2. Verify GitHub Actions are enabled
3. Check workflow permissions in repository settings

### Issue: False positive alerts

**Solution:**
1. Review alert details carefully
2. If confirmed false positive, dismiss with explanation
3. Add code comments explaining why it's safe

### Issue: Too many alerts

**Solution:**
1. Prioritize by severity (Critical → High → Medium → Low)
2. Fix high/critical issues first
3. Create issues for medium/low priority items
4. Set up alert filters in GitHub

---

## Security Checklist

Use this checklist to ensure all security features are properly configured:

- [ ] Git repository initialized and connected to GitHub
- [ ] `.gitignore` file created (excludes sensitive files)
- [ ] Dependabot alerts enabled
- [ ] Code scanning enabled
- [ ] Secret scanning enabled
- [ ] CodeQL workflow file created (`.github/workflows/codeql-analysis.yml`)
- [ ] Security scan workflow created (`.github/workflows/security-scan.yml`)
- [ ] Dependabot configuration created (`.github/dependabot.yml`)
- [ ] First security scan completed
- [ ] All critical/high vulnerabilities addressed
- [ ] Security monitoring schedule established

---

## Next Steps

1. **Push code to GitHub**: Follow Step 1 to initialize and push
2. **Enable security features**: Follow Step 2 to enable GitHub security
3. **Review initial scan**: Wait for first scan to complete (usually within minutes)
4. **Address findings**: Fix any critical/high severity issues
5. **Set up monitoring**: Configure email notifications for security alerts

---

## Additional Resources

- [GitHub Security Documentation](https://docs.github.com/en/code-security)
- [CodeQL Documentation](https://codeql.github.com/docs/)
- [Dependabot Documentation](https://docs.github.com/en/code-security/dependabot)
- [.NET Security Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/security/)

---

**Last Updated**: 2026-02-05
