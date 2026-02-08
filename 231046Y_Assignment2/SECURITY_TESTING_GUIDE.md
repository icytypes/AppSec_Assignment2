# Security Features Testing Guide

This guide provides step-by-step instructions to manually test and verify each security feature in the Fresh Farm Market application.

---

## 1. Client-Side and Server-Side Password Checks

### Testing Client-Side Password Validation

1. **Navigate to Registration Page**: Go to `/Register`
2. **Test Password Strength Feedback**:
   - Enter a weak password (e.g., `password123`)
   - **Expected**: Real-time feedback showing password is weak
   - **Expected**: Visual indicators (red/yellow/green) showing password strength
   - **Expected**: Specific feedback messages (e.g., "Needs uppercase", "Needs special characters")
3. **Test Password Match Validation**:
   - Enter password: `StrongPass123!@#`
   - Enter different confirm password: `DifferentPass123!@#`
   - **Expected**: Error message "Passwords do not match" appears immediately
4. **Test Minimum Length**:
   - Enter password with less than 12 characters: `Short123!`
   - **Expected**: Error message about minimum 12 characters

### Testing Server-Side Password Validation

1. **Bypass Client-Side Validation** (using browser DevTools):
   - Open browser DevTools (F12)
   - Go to Console tab
   - Run: `document.querySelector('input[type="password"]').removeAttribute('minlength')`
   - Try submitting form with weak password
   - **Expected**: Server rejects the password with validation errors
2. **Test Password Strength Requirements**:
   - Try submitting passwords missing requirements:
     - No uppercase: `lowercase123!@#`
     - No lowercase: `UPPERCASE123!@#`
     - No numbers: `NoNumbers!@#`
     - No special chars: `NoSpecial123`
     - Less than 12 chars: `Short123!`
   - **Expected**: Server returns specific error messages for each missing requirement

---

## 2. Encrypt Sensitive User Data in Database

### Testing Credit Card Encryption

1. **Register a New Account**:
   - Go to `/Register`
   - Enter credit card number: `1234-5678-9012-3456`
   - Complete registration
2. **Check Database**:
   - Open SQLite database: `FreshFarmMarketDb.db`
   - Query: `SELECT CreditCardNo FROM Members WHERE Email = 'your-email@example.com'`
   - **Expected**: Credit card number is encrypted (not readable, different from input)
3. **Verify Decryption on Homepage**:
   - Login to the account
   - Go to homepage (`/Index`)
   - **Expected**: Credit card number is displayed (decrypted) but masked (e.g., `****-****-****-3456`)
4. **Test with Different Cards**:
   - Register another account with different credit card
   - **Expected**: Each card is encrypted differently (different encrypted values in database)

---

## 3. Proper Password Hashing and Storage

### Testing Password Hashing

1. **Register an Account**:
   - Go to `/Register`
   - Use password: `TestPassword123!@#`
   - Complete registration
2. **Check Database**:
   - Open database: `FreshFarmMarketDb.db`
   - Query: `SELECT PasswordHash FROM Members WHERE Email = 'your-email@example.com'`
   - **Expected**: Password is hashed (long string, not readable)
   - **Expected**: Password hash is different from original password
   - **Expected**: Same password produces same hash (verify by checking hash format)
3. **Verify Password Verification**:
   - Login with correct password: `TestPassword123!@#`
   - **Expected**: Login succeeds
   - Try login with wrong password: `WrongPassword123!@#`
   - **Expected**: Login fails
4. **Test Hash Consistency**:
   - Note the password hash from database
   - Delete the account and re-register with same password
   - **Expected**: New hash is different (if salt is used) or same (if deterministic)

---

## 4. Create Secure Session Upon Successful Login

### Testing Session Creation

1. **Login to Account**:
   - Go to `/Login`
   - Enter credentials and login
2. **Check Session Cookie**:
   - Open browser DevTools (F12)
   - Go to Application/Storage tab → Cookies
   - Look for cookie: `FreshFarmMarket.Session`
   - **Expected**: Session cookie exists
   - **Expected**: Cookie has `HttpOnly` flag set
   - **Expected**: Cookie has `Secure` flag (if HTTPS)
3. **Verify Session Data**:
   - After login, try accessing `/Index` (homepage)
   - **Expected**: Homepage loads with user information
   - **Expected**: User is authenticated (no redirect to login)

---

## 5. Implement Session Timeout

### Testing Session Timeout

1. **Login to Account**:
   - Login successfully
2. **Wait for Timeout** (or manually expire session):
   - Session timeout is 30 minutes
   - **Option A**: Wait 30 minutes of inactivity
   - **Option B**: Modify session timeout in `Program.cs` to 1 minute for testing
3. **Test Timeout Behavior**:
   - After timeout, try accessing `/Index` or `/ChangePassword`
   - **Expected**: Redirected to `/Login` page
   - **Expected**: Error message or return URL parameter in login page
4. **Test Session Expiry Message**:
   - After timeout, check if any message is displayed
   - **Expected**: User is informed session expired

---

## 6. Route to Homepage/Login After Session Timeout

### Testing Session Timeout Redirect

1. **Login and Navigate**:
   - Login successfully
   - Navigate to `/ChangePassword` or any protected page
2. **Expire Session**:
   - Wait for timeout or manually clear session
3. **Try Accessing Protected Page**:
   - Try to access `/ChangePassword` or `/Index`
   - **Expected**: Automatically redirected to `/Login`
   - **Expected**: `returnUrl` parameter contains original page path
4. **Test Return URL**:
   - After redirect, login again
   - **Expected**: Redirected back to original page (if returnUrl is implemented)

---

## 7. Detect and Handle Multiple Logins

### Testing Multiple Login Detection

1. **Login from First Browser/Device**:
   - Open browser (e.g., Chrome)
   - Login to account
2. **Login from Second Browser/Tab**:
   - Open different browser (e.g., Firefox) or incognito window
   - Login to same account
3. **Check Detection**:
   - Check audit logs in database: `SELECT * FROM AuditLogs WHERE Action = 'Login'`
   - **Expected**: Multiple login entries with different session IDs
   - **Expected**: Different IP addresses or user agents logged
4. **Test Session Handling**:
   - Try accessing protected pages from both sessions
   - **Expected**: Both sessions work (or one is invalidated, depending on implementation)
   - Check if warning message is displayed about multiple logins

---

## 8. Google reCAPTCHA v3 Service

### Testing reCAPTCHA Integration

1. **Check reCAPTCHA Script Loading**:
   - Go to `/Register` or `/Login`
   - Open browser DevTools (F12) → Network tab
   - **Expected**: Request to `google.com/recaptcha/api.js` is loaded
2. **Test reCAPTCHA Token Generation**:
   - Open DevTools → Console tab
   - Type: `grecaptcha`
   - **Expected**: reCAPTCHA object is available
3. **Test Form Submission**:
   - Fill registration form
   - Before submitting, check Network tab
   - Submit form
   - **Expected**: `recaptchaToken` is sent in form data
4. **Test Server-Side Verification**:
   - Submit form with valid reCAPTCHA token
   - **Expected**: Form processes successfully
   - **Expected**: If token is invalid/missing, form is rejected with error

---

## 9. Prevent SQL Injection Attacks

### Testing SQL Injection Prevention

1. **Test in Registration Form**:
   - Go to `/Register`
   - Try SQL injection in email field: `test@test.com' OR '1'='1`
   - Try SQL injection in name field: `'; DROP TABLE Members; --`
   - Submit form
   - **Expected**: Input is sanitized (SQL keywords removed or escaped)
   - **Expected**: Form either rejects or sanitizes the input
   - **Expected**: No SQL error messages displayed
2. **Test in Login Form**:
   - Go to `/Login`
   - Try SQL injection in email: `admin' OR '1'='1' --`
   - Try SQL injection in password: `' OR '1'='1`
   - **Expected**: Login fails (not bypassed)
   - **Expected**: No database errors
3. **Check Database**:
   - After registration with SQL injection attempt
   - Check database: `SELECT * FROM Members WHERE Email LIKE '%OR%'`
   - **Expected**: SQL keywords are removed or escaped in stored data
4. **Test with Parameterized Queries**:
   - All database operations use EF Core (parameterized queries)
   - **Expected**: SQL injection attempts are automatically prevented

---

## 10. Cross-Site Request Forgery (CSRF) Protection

### Testing CSRF Protection

1. **Check Antiforgery Token in Forms**:
   - Go to `/Register` or `/Login`
   - Right-click → View Page Source
   - Search for: `__RequestVerificationToken` or `RequestVerificationToken`
   - **Expected**: Hidden input field with antiforgery token exists
2. **Test Token Validation**:
   - Open browser DevTools (F12) → Network tab
   - Submit a form
   - Check request headers
   - **Expected**: Antiforgery token is sent in request
3. **Test Without Token**:
   - Use browser extension or script to remove token
   - Try submitting form
   - **Expected**: Request is rejected with 400 Bad Request or validation error
4. **Test Cross-Origin Request**:
   - Create HTML file with form pointing to your site
   - Try submitting from different origin
   - **Expected**: Request is blocked (CORS/CSRF protection)

---

## 11. Prevent Cross-Site Scripting (XSS) Attacks

### Testing XSS Prevention

1. **Test in Registration Form**:
   - Go to `/Register`
   - Try XSS in name field: `<script>alert('XSS')</script>`
   - Try XSS in About Me: `<img src=x onerror=alert(1)>`
   - Try XSS in email: `test<script>alert(1)</script>@test.com`
   - Submit form
2. **Check Database**:
   - Query database: `SELECT FullName, AboutMe FROM Members WHERE Email = 'test@test.com'`
   - **Expected**: Script tags are removed or HTML-encoded
3. **Check Display on Homepage**:
   - Login and view homepage
   - **Expected**: XSS code is displayed as text (not executed)
   - **Expected**: No alert boxes appear
   - **Expected**: HTML is escaped (e.g., `<script>` shown as text)
4. **Test Various XSS Payloads**:
   - `<script>alert('XSS')</script>`
   - `<img src=x onerror=alert(1)>`
   - `javascript:alert('XSS')`
   - `<iframe src=javascript:alert(1)>`
   - **Expected**: All are sanitized/encoded

---

## 12. Input Sanitization, Validation, and Verification

### Testing Input Sanitization

1. **Test All Input Fields**:
   - Go to `/Register`
   - Test each field with malicious input:
     - **Email**: `test<script>@test.com`
     - **Mobile**: `1234567890<script>`
     - **Address**: `123 Main St'; DROP TABLE--`
     - **Name**: `<img src=x onerror=alert(1)>`
   - **Expected**: All inputs are sanitized before saving
2. **Check Sanitization Service**:
   - Review `InputSanitizationService.cs`
   - **Expected**: `SanitizeForDatabase()` is called on all inputs
3. **Test Validation**:
   - Try invalid email: `notanemail`
   - Try invalid mobile: `abc123`
   - **Expected**: Validation errors displayed
   - **Expected**: Form is not submitted

---

## 13. Client-Side and Server-Side Input Validation

### Testing Dual Validation

1. **Test Client-Side Validation**:
   - Go to `/Register`
   - Leave required fields empty
   - **Expected**: Red borders/error messages appear immediately
   - **Expected**: Form cannot be submitted
2. **Bypass Client-Side Validation**:
   - Open DevTools → Console
   - Run: `document.querySelector('form').removeAttribute('novalidate')`
   - Or disable JavaScript
   - Try submitting invalid form
   - **Expected**: Server still validates and rejects
3. **Test Server-Side Validation**:
   - Use API testing tool (Postman) or curl to send POST request
   - Send invalid data (bypassing client validation)
   - **Expected**: Server returns validation errors
   - **Expected**: Status code 400 or validation error response

---

## 14. Display Error/Warning Messages for Improper Input

### Testing Error Messages

1. **Test Required Field Errors**:
   - Go to `/Register`
   - Leave email field empty
   - Try to submit
   - **Expected**: Error message: "Email address is required"
2. **Test Format Errors**:
   - Enter invalid email: `notanemail`
   - **Expected**: Error message: "Invalid email address format"
3. **Test Password Strength Errors**:
   - Enter weak password: `weak123`
   - **Expected**: Multiple error messages about password requirements
4. **Test Duplicate Email Error**:
   - Register with email: `test@test.com`
   - Try registering again with same email
   - **Expected**: Error message: "Email already exists" or similar

---

## 15. Proper Encoding Before Saving to Database

### Testing Database Encoding

1. **Register with Special Characters**:
   - Go to `/Register`
   - Enter name: `O'Brien & Sons <script>`
   - Enter address: `123 Main St, "Apt 5"`
   - Submit form
2. **Check Database**:
   - Query: `SELECT FullName, DeliveryAddress FROM Members`
   - **Expected**: Special characters are properly encoded/stored
   - **Expected**: No SQL errors or injection issues
3. **Check Display**:
   - View homepage after login
   - **Expected**: Special characters display correctly
   - **Expected**: No broken HTML or script execution

---

## 16. Graceful Error Handling on All Pages

### Testing Error Handling

1. **Test Database Errors**:
   - Temporarily rename database file
   - Try to access any page
   - **Expected**: Custom error page displayed (not raw exception)
2. **Test File Upload Errors**:
   - Go to `/Register`
   - Try uploading non-JPG file
   - **Expected**: Error message displayed gracefully
3. **Test Null Reference Errors**:
   - Access pages with invalid session
   - **Expected**: Redirected to login (not exception page)
4. **Test Network Errors**:
   - Disconnect from network
   - Try accessing pages
   - **Expected**: Appropriate error message

---

## 17. Custom Error Pages (404, 403)

### Testing Custom Error Pages

1. **Test 404 Page**:
   - Navigate to non-existent page: `/ThisPageDoesNotExist`
   - **Expected**: Custom 404 page displayed (`/404`)
   - **Expected**: Message: "Page Not Found"
   - **Expected**: Link to homepage
2. **Test 403 Page**:
   - Try accessing protected resource without authentication
   - Or manually trigger 403 error
   - **Expected**: Custom 403 page displayed (`/403`)
   - **Expected**: Message: "Forbidden"
3. **Test 500 Page**:
   - Cause server error (e.g., database connection failure)
   - **Expected**: Custom 500 page displayed (`/500`)
   - **Expected**: Message: "Internal Server Error"
4. **Verify Error Page Design**:
   - Check that error pages match application theme
   - **Expected**: Consistent branding and navigation

---

## 18. Automatic Account Recovery After Lockout

### Testing Account Lockout Recovery

1. **Trigger Account Lockout**:
   - Go to `/Login`
   - Enter wrong password 3 times
   - **Expected**: Account is locked
   - **Expected**: Message: "Account locked. Try again in X minutes"
2. **Wait for Recovery**:
   - Wait 15 minutes (or check `AccountLockedUntil` in database)
   - **Expected**: Lockout expires automatically
3. **Test Automatic Unlock**:
   - After lockout period, try logging in
   - **Expected**: Login succeeds (no manual unlock needed)
4. **Check Database**:
   - Query: `SELECT AccountLockedUntil, FailedLoginAttempts FROM Members WHERE Email = 'test@test.com'`
   - **Expected**: `AccountLockedUntil` is NULL after expiry
   - **Expected**: `FailedLoginAttempts` is reset to 0

---

## 19. Password History (Max 2 Password History)

### Testing Password History

1. **Change Password First Time**:
   - Login to account
   - Go to `/ChangePassword`
   - Change password from `OldPass123!@#` to `NewPass123!@#`
   - **Expected**: Password changed successfully
2. **Check Password History**:
   - Query database: `SELECT * FROM PasswordHistories WHERE MemberId = X`
   - **Expected**: Old password hash is saved in history
3. **Try Reusing Recent Password**:
   - Go to `/ChangePassword` again
   - Try changing password back to `OldPass123!@#`
   - **Expected**: Error: "You cannot reuse a recently used password"
4. **Test History Limit**:
   - Change password 3 times
   - Check database
   - **Expected**: Only last 2 passwords are in history (oldest removed)

---

## 20. Change Password Functionality

### Testing Change Password

1. **Access Change Password Page**:
   - Login to account
   - Navigate to `/ChangePassword`
   - **Expected**: Page loads with form
2. **Test Current Password Verification**:
   - Enter wrong current password
   - Enter new password
   - **Expected**: Error: "Current password is incorrect"
3. **Test Password Strength**:
   - Enter correct current password
   - Enter weak new password: `weak123`
   - **Expected**: Password strength errors displayed
4. **Test Successful Change**:
   - Enter correct current password
   - Enter strong new password: `NewStrongPass123!@#`
   - **Expected**: Password changed successfully
   - **Expected**: Redirected to homepage
   - **Expected**: Can login with new password

---

## 21. Reset Password Functionality

### Testing Password Reset

1. **Request Password Reset**:
   - Go to `/ForgotPassword`
   - Enter registered email
   - Submit form
   - **Expected**: Message: "If an account exists, reset link sent"
2. **Check Reset Token**:
   - Query database: `SELECT * FROM PasswordResetTokens WHERE MemberId = X`
   - **Expected**: Token is generated and stored
   - **Expected**: Expiry date is 24 hours from now
3. **Access Reset Link**:
   - Copy reset link from audit log or database
   - Navigate to reset link: `/ResetPassword?token=XXX&email=test@test.com`
   - **Expected**: Reset password form displayed
4. **Test Token Expiry**:
   - Wait 24+ hours or manually expire token in database
   - Try accessing reset link
   - **Expected**: Error: "Invalid or expired reset token"
5. **Test Successful Reset**:
   - Use valid reset token
   - Enter new strong password
   - **Expected**: Password reset successfully
   - **Expected**: Token marked as used
   - **Expected**: Can login with new password

---

## 22. Minimum and Maximum Password Age Policies

### Testing Password Age Policies

1. **Test Minimum Password Age**:
   - Change password
   - Immediately try to change password again
   - **Expected**: Error: "You cannot change password so soon after last change"
2. **Check Minimum Age**:
   - Wait 1+ minute (minimum age)
   - Try changing password again
   - **Expected**: Password change allowed
3. **Test Maximum Password Age**:
   - Check `PasswordChangedDate` in database
   - Manually set date to 90+ days ago: `UPDATE Members SET PasswordChangedDate = '2023-01-01' WHERE Email = 'test@test.com'`
   - Login and access homepage or change password page
   - **Expected**: Warning: "Your password has expired. Please change it now"
4. **Test Password Expiry Enforcement**:
   - With expired password, try accessing protected pages
   - **Expected**: Forced to change password before access

---

## 23. Two-Factor Authentication (2FA)

### Testing 2FA

1. **Enable 2FA**:
   - Login to account
   - Navigate to `/Enable2FA`
   - **Expected**: QR code displayed
   - **Expected**: Secret key shown
2. **Scan QR Code**:
   - Use authenticator app (Google Authenticator, Authy) to scan QR code
   - **Expected**: Account added to authenticator app
3. **Verify 2FA Setup**:
   - Enter 6-digit code from authenticator app
   - Submit form
   - **Expected**: 2FA enabled successfully
   - **Expected**: Redirected to homepage
4. **Test 2FA on Login**:
   - Logout
   - Login with email and password
   - **Expected**: 2FA code input field appears
   - Enter code from authenticator app
   - **Expected**: Login succeeds
5. **Test Invalid 2FA Code**:
   - Enter wrong 2FA code
   - **Expected**: Error: "Invalid verification code"
6. **Check Database**:
   - Query: `SELECT IsTwoFactorEnabled, TwoFactorSecret FROM Members WHERE Email = 'test@test.com'`
   - **Expected**: `IsTwoFactorEnabled = 1`
   - **Expected**: `TwoFactorSecret` is stored

---

## 24. HTTPS for All Communications

### Testing HTTPS Enforcement

1. **Check HTTPS Redirection**:
   - Access site via HTTP: `http://localhost:5000`
   - **Expected**: Automatically redirected to HTTPS: `https://localhost:5001`
2. **Test in Production**:
   - Deploy to production
   - Access via HTTP
   - **Expected**: Redirected to HTTPS
3. **Check HSTS Header**:
   - Open DevTools → Network tab
   - Check response headers
   - **Expected**: `Strict-Transport-Security` header present (in production)
4. **Test Secure Cookies**:
   - Check session cookie
   - **Expected**: `Secure` flag set (when using HTTPS)

---

## 25. Proper Access Controls and Authorization

### Testing Access Controls

1. **Test Unauthenticated Access**:
   - Logout or clear session
   - Try accessing `/Index` or `/ChangePassword`
   - **Expected**: Redirected to `/Login`
2. **Test Authenticated Access**:
   - Login successfully
   - Access protected pages
   - **Expected**: Pages load successfully
3. **Test Session Validation**:
   - Login and note session ID
   - Manually modify session in database
   - Try accessing protected page
   - **Expected**: Session invalidated, redirected to login
4. **Test Authorization Levels**:
   - Check if different user roles exist
   - Test access to admin/user-specific pages
   - **Expected**: Users can only access authorized resources

---

## 26. Keep Software and Dependencies Up to Date

### Testing Dependency Updates

1. **Check Package Versions**:
   - Open `231046Y_Assignment2.csproj`
   - Review package versions
   - **Expected**: Latest stable versions used
2. **Check for Vulnerabilities**:
   - Run: `dotnet list package --vulnerable`
   - **Expected**: No known vulnerabilities
3. **Review .NET Version**:
   - Check `TargetFramework` in `.csproj`
   - **Expected**: Latest LTS version (e.g., .NET 8.0)

---

## 27. Secure Coding Practices

### Testing Secure Coding

1. **Review Code for Common Issues**:
   - Check for hardcoded passwords/secrets
   - **Expected**: All secrets in `appsettings.json` or environment variables
2. **Check Password Handling**:
   - Verify passwords are never logged
   - **Expected**: No password in logs or console
3. **Check Error Messages**:
   - Trigger errors
   - **Expected**: No sensitive information in error messages
4. **Review SQL Queries**:
   - Check all database operations
   - **Expected**: All use EF Core (parameterized queries)
   - **Expected**: No string concatenation for SQL

---

## 28. Regular Backups and Secure Data Storage

### Testing Data Backup

1. **Check Database Backup Strategy**:
   - Review backup procedures
   - **Expected**: Regular backup schedule documented
2. **Test Database Backup**:
   - Create backup of SQLite database
   - **Expected**: Backup file created successfully
3. **Test Data Restoration**:
   - Restore from backup
   - **Expected**: Data restored correctly
4. **Check Secure Storage**:
   - Verify encrypted data in database
   - **Expected**: Sensitive data is encrypted

---

## 29. Logging and Monitoring for Security Events

### Testing Audit Logging

1. **Test Login Logging**:
   - Login to account
   - Query database: `SELECT * FROM AuditLogs WHERE Action = 'Login' ORDER BY Timestamp DESC`
   - **Expected**: Login event logged with:
     - Member ID
     - Email
     - IP address
     - User agent
     - Session ID
     - Timestamp
     - Status (Success/Failed)
2. **Test Failed Login Logging**:
   - Try logging in with wrong password
   - Check audit logs
   - **Expected**: Failed login logged with status "Failed"
3. **Test Password Change Logging**:
   - Change password
   - Check audit logs: `SELECT * FROM AuditLogs WHERE Action = 'ChangePassword'`
   - **Expected**: Password change logged
4. **Test Registration Logging**:
   - Register new account
   - Check audit logs: `SELECT * FROM AuditLogs WHERE Action = 'Register'`
   - **Expected**: Registration logged
5. **Test Account Lockout Logging**:
   - Trigger account lockout
   - Check audit logs: `SELECT * FROM AuditLogs WHERE Status = 'Blocked'`
   - **Expected**: Lockout event logged
6. **Test All Security Events**:
   - Perform various security-sensitive actions
   - **Expected**: All are logged in `AuditLogs` table

---

## Quick Testing Checklist

Use this checklist for quick verification:

- [ ] Password strength feedback works (client-side)
- [ ] Weak passwords rejected (server-side)
- [ ] Credit card encrypted in database
- [ ] Password hashed in database
- [ ] Session created after login
- [ ] Session expires after 30 minutes
- [ ] Redirected to login after session timeout
- [ ] Multiple logins detected and logged
- [ ] reCAPTCHA token sent with forms
- [ ] SQL injection attempts blocked
- [ ] CSRF token present in forms
- [ ] XSS attempts sanitized
- [ ] Input validation on client and server
- [ ] Error messages displayed for invalid input
- [ ] Special characters encoded in database
- [ ] Custom 404 page displayed
- [ ] Custom 403 page displayed
- [ ] Custom 500 page displayed
- [ ] Account unlocks automatically after 15 minutes
- [ ] Cannot reuse recent passwords
- [ ] Change password works
- [ ] Reset password works with token
- [ ] Cannot change password within 1 minute
- [ ] Password expires after 90 days
- [ ] 2FA can be enabled
- [ ] 2FA required on login (if enabled)
- [ ] HTTPS redirect works
- [ ] Protected pages require authentication
- [ ] All security events logged

---

## Tools for Testing

1. **Browser DevTools**: F12 for inspecting requests, cookies, console
2. **SQLite Browser**: DB Browser for SQLite to inspect database
3. **Postman/curl**: For API testing and bypassing client validation
4. **Authenticator App**: Google Authenticator or Authy for 2FA testing
5. **Network Monitor**: Wireshark or browser network tab for HTTPS verification

---

## Notes

- Some tests require modifying code temporarily (e.g., reducing timeout periods)
- Always test in a development environment first
- Restore original settings after testing
- Document any issues found during testing

---

**Last Updated**: 2026-02-05
