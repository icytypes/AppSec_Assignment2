# Fresh Farm Market - Security Features Checklist

This document verifies all security features implemented in the Fresh Farm Market web application.

## ✅ Input Validation and Sanitization

### SQL Injection Prevention
- ✅ **Entity Framework Core with Parameterized Queries**: All database operations use EF Core, which automatically parameterizes queries, preventing SQL injection.
- ✅ **InputSanitizationService**: Removes SQL injection patterns (SELECT, INSERT, UPDATE, DELETE, DROP, CREATE, ALTER, EXEC, UNION, SCRIPT, --, /*, */) from user inputs.
- ✅ **SanitizeForDatabase Method**: Applied to all user inputs before database operations in:
  - `Register.cshtml.cs` - All registration fields
  - `Login.cshtml.cs` - Email and password
  - `ChangePassword.cshtml.cs` - Password fields
  - `ForgotPassword.cshtml.cs` - Email field
  - `ResetPassword.cshtml.cs` - Password fields

### Cross-Site Scripting (XSS) Prevention
- ✅ **HTML Encoding**: `InputSanitizationService.SanitizeInput()` uses `WebUtility.HtmlEncode()` to encode special characters.
- ✅ **Script Tag Removal**: Removes `<script>`, `<iframe>`, event handlers (`onclick`, `onerror`, etc.), and `javascript:`/`vbscript:` protocols.
- ✅ **Database Encoding**: `SanitizeForDatabase()` applies HTML encoding before saving to database.
- ✅ **Razor Auto-Encoding**: Razor Pages automatically HTML-encodes all output by default (e.g., `@Model.Email`).

### Cross-Site Request Forgery (CSRF) Protection
- ✅ **Antiforgery Service**: Configured in `Program.cs` with secure cookie settings.
- ✅ **Automatic Token Generation**: Razor Pages automatically includes antiforgery tokens in all `<form method="post">` tags.
- ✅ **Token Validation**: All POST requests are automatically validated by ASP.NET Core middleware.
- ✅ **Secure Cookie Settings**: Antiforgery cookies are HttpOnly and use SecurePolicy.

### Input Validation
- ✅ **Client-Side Validation**: 
  - All forms use `asp-validation-for` and `asp-validation-summary` attributes.
  - JavaScript validation for password strength feedback.
  - Email format validation using HTML5 `type="email"` and regex patterns.
  - Password match validation (client-side JavaScript).
- ✅ **Server-Side Validation**:
  - Model validation attributes (`[Required]`, `[EmailAddress]`, `[MinLength]`, `[Compare]`).
  - Custom `StrongPasswordAttribute` for password complexity.
  - `InputSanitizationService.IsValidEmail()` and `IsValidMobileNumber()` methods.
  - Password strength checking via `PasswordService.CheckPasswordStrength()`.
- ✅ **Error Messages**: All validation errors are displayed using `asp-validation-for` and `ModelState.AddModelError()`.

### Proper Encoding Before Database Save
- ✅ **SanitizeForDatabase Method**: Applied to all inputs before saving:
  - Removes null bytes and control characters.
  - Removes SQL injection patterns.
  - Applies HTML encoding.
  - Trims whitespace.

---

## ✅ Error Handling

### Custom Error Pages
- ✅ **404 Not Found**: `Pages/Error404.cshtml` - Custom page for missing resources.
- ✅ **403 Forbidden**: `Pages/Error403.cshtml` - Custom page for unauthorized access.
- ✅ **500 Internal Server Error**: `Pages/Error500.cshtml` - Custom page for server errors.
- ✅ **Generic Error Page**: `Pages/Error.cshtml` - Fallback error page.

### Error Handling Middleware
- ✅ **Status Code Pages**: Configured in `Program.cs` to redirect to custom error pages:
  - 404 → `/404`
  - 403 → `/403`
  - 500+ → `/500`
- ✅ **Exception Handler**: `UseExceptionHandler("/Error")` in production mode.
- ✅ **HSTS**: HTTP Strict Transport Security enabled in production.
- ✅ **Try-Catch Blocks**: Critical operations wrapped in try-catch with proper error logging.

### Graceful Error Handling
- ✅ **ModelState Validation**: All forms validate input and display errors gracefully.
- ✅ **Database Error Handling**: EF Core exceptions are caught and handled.
- ✅ **File Upload Errors**: File validation errors are displayed to users.
- ✅ **Session Errors**: Session validation failures redirect to login page.

---

## ✅ Advanced Security Features

### Account Lockout and Recovery
- ✅ **AccountLockoutService**: Implements rate limiting:
  - Maximum 3 failed login attempts.
  - Account locked for 15 minutes after lockout.
  - Automatic unlock after lockout period expires.
- ✅ **Lockout Detection**: `IsAccountLockedAsync()` checks if account is currently locked.
- ✅ **Remaining Time**: `GetRemainingLockoutMinutesAsync()` provides remaining lockout time.
- ✅ **Automatic Recovery**: Lockout automatically expires after 15 minutes (configurable via `LockoutDurationMinutes`).

### Password History
- ✅ **PasswordPolicyService**: Enforces password history:
  - Maximum 2 previous passwords stored in `PasswordHistories` table.
  - `IsPasswordInHistoryAsync()` checks if new password was recently used.
  - `SavePasswordToHistoryAsync()` saves old password before updating.
- ✅ **History Enforcement**: Applied in:
  - `ChangePassword.cshtml.cs`
  - `ResetPassword.cshtml.cs`

### Password Age Policies
- ✅ **Minimum Password Age**: Cannot change password within 1 minute of last change.
  - Enforced via `CanChangePasswordAsync()`.
  - Prevents rapid password changes.
- ✅ **Maximum Password Age**: Must change password after 90 days.
  - Enforced via `MustChangePasswordAsync()`.
  - Displayed as warning on homepage and change password page.
- ✅ **Password Changed Date**: Tracked in `Member.PasswordChangedDate`.

### Change Password Functionality
- ✅ **ChangePassword Page**: `/ChangePassword`
- ✅ **Current Password Verification**: Validates current password before allowing change.
- ✅ **Password Strength Check**: Enforces strong password requirements.
- ✅ **Password History Check**: Prevents reusing recent passwords.
- ✅ **Password Age Check**: Enforces minimum password age.
- ✅ **Audit Logging**: Logs all password change attempts (success and failure).

### Reset Password Functionality
- ✅ **ForgotPassword Page**: `/ForgotPassword` - Request password reset.
- ✅ **ResetPassword Page**: `/ResetPassword` - Set new password using token.
- ✅ **Token Generation**: Secure GUID-based tokens stored in `PasswordResetTokens` table.
- ✅ **Token Expiry**: Tokens expire after 24 hours.
- ✅ **One-Time Use**: Tokens are marked as used after successful reset.
- ✅ **Email Link**: Reset link format: `/ResetPassword?token={token}&email={email}`
- ✅ **Password Strength**: Enforces strong password requirements.
- ✅ **Password History**: Prevents reusing recent passwords.
- ✅ **Audit Logging**: Logs password reset requests and completions.

### Two-Factor Authentication (2FA)
- ✅ **TwoFactorService**: Implements TOTP-based 2FA:
  - Secret key generation.
  - QR code generation for authenticator apps.
  - Code verification.
- ✅ **Enable2FA Page**: `/Enable2FA` - Setup 2FA for account.
- ✅ **QR Code Display**: Shows QR code for scanning with authenticator apps.
- ✅ **Verification Required**: Requires verification code to enable 2FA.
- ✅ **Login Integration**: 2FA verification during login (if enabled).
- ✅ **Database Storage**: 2FA secret stored in `Member.TwoFactorSecret`.
- ✅ **Status Tracking**: `Member.IsTwoFactorEnabled` tracks 2FA status.

---

## ✅ General Security Best Practices

### HTTPS Enforcement
- ✅ **HTTPS Redirection**: `UseHttpsRedirection()` redirects HTTP to HTTPS.
- ✅ **HSTS**: HTTP Strict Transport Security enabled in production (`UseHsts()`).
- ✅ **Secure Cookies**: Session and antiforgery cookies use `SecurePolicy.SameAsRequest`.

### Access Controls and Authorization
- ✅ **Session-Based Authentication**: All protected pages require valid session.
- ✅ **Session Validation Middleware**: Custom middleware checks session validity on all protected routes.
- ✅ **Login Required**: Unauthenticated users redirected to `/Login`.
- ✅ **Session Timeout**: 30-minute idle timeout configured.
- ✅ **Multiple Login Detection**: `SessionService.DetectMultipleLogins()` identifies concurrent sessions.

### Secure Coding Practices
- ✅ **Password Hashing**: SHA-256 hashing via `PasswordService.HashPassword()`.
- ✅ **Data Encryption**: AES-256 encryption for sensitive data (credit card numbers) via `EncryptionService`.
- ✅ **Parameterized Queries**: EF Core uses parameterized queries (no string concatenation).
- ✅ **Input Sanitization**: All user inputs sanitized before processing.
- ✅ **Output Encoding**: All output automatically HTML-encoded by Razor.

### Logging and Monitoring
- ✅ **AuditLogService**: Comprehensive audit logging:
  - Logs all user activities (login, logout, registration, password changes, etc.).
  - Records IP address, user agent, session ID, timestamp, and status.
  - Stored in `AuditLogs` table.
- ✅ **Failed Login Tracking**: Failed login attempts logged with status "Failed".
- ✅ **Account Lockout Logging**: Account lockouts logged with status "Blocked".
- ✅ **Security Event Logging**: All security-sensitive operations are logged.

### Data Protection
- ✅ **Password Storage**: Passwords are hashed (never stored in plain text).
- ✅ **Sensitive Data Encryption**: Credit card numbers encrypted before database storage.
- ✅ **Data Decryption**: Encrypted data decrypted when displaying to authorized users.
- ✅ **Database Security**: SQLite database file permissions should be restricted in production.

### Session Management
- ✅ **Secure Session Creation**: Sessions created only after successful login.
- ✅ **Session Timeout**: 30-minute idle timeout.
- ✅ **Session Validation**: All protected pages validate session before access.
- ✅ **Session Cleanup**: Sessions cleared on logout.
- ✅ **Multiple Login Detection**: Detects and logs concurrent logins from different devices/tabs.

### File Upload Security
- ✅ **File Type Restriction**: Only `.jpg` files allowed.
- ✅ **MIME Type Validation**: Validates `image/jpeg` and `image/jpg` MIME types.
- ✅ **File Size Limit**: Maximum 5MB file size.
- ✅ **Secure Storage**: Files stored in `wwwroot/uploads/photos/` with unique filenames.
- ✅ **Path Validation**: Prevents directory traversal attacks.

### Anti-Bot Protection
- ✅ **Google reCAPTCHA v3**: Integrated via `ReCaptchaService`.
- ✅ **Client-Side Integration**: reCAPTCHA script loaded on registration and login pages.
- ✅ **Server-Side Verification**: Token verified on server before processing.
- ✅ **Score-Based Validation**: Validates reCAPTCHA score (default threshold: 0.5).
- ✅ **Test Key Support**: Handles Google test keys for development.

---

## Implementation Details

### Services
- `EncryptionService`: AES-256 encryption/decryption
- `PasswordService`: SHA-256 hashing and password strength checking
- `SessionService`: Session management and validation
- `ReCaptchaService`: Google reCAPTCHA v3 integration
- `AccountLockoutService`: Rate limiting and account lockout
- `AuditLogService`: Activity logging
- `PasswordPolicyService`: Password history and age policies
- `InputSanitizationService`: Input sanitization and validation
- `TwoFactorService`: TOTP-based 2FA

### Database Models
- `Member`: User account information
- `AuditLog`: User activity logs
- `PasswordHistory`: Previous password hashes
- `PasswordResetToken`: Password reset tokens

### Security Configuration
- Session timeout: 30 minutes
- Max failed login attempts: 3
- Lockout duration: 15 minutes
- Minimum password age: 1 minute
- Maximum password age: 90 days
- Password history: 2 previous passwords
- Password reset token expiry: 24 hours

---

## Testing Recommendations

1. **SQL Injection**: Test with inputs like `' OR '1'='1`, `'; DROP TABLE Members; --`
2. **XSS**: Test with inputs like `<script>alert('XSS')</script>`, `<img src=x onerror=alert(1)>`
3. **CSRF**: Verify antiforgery tokens are present in all POST forms
4. **Session Timeout**: Wait 30 minutes and verify redirect to login
5. **Account Lockout**: Attempt 3 failed logins and verify lockout
6. **Password Strength**: Test weak passwords and verify rejection
7. **Password History**: Change password, then try to reuse it
8. **2FA**: Enable 2FA and verify login requires code
9. **File Upload**: Test with non-JPG files and verify rejection
10. **Error Pages**: Access non-existent pages and verify custom 404 page

---

## Notes

- All security features are implemented and tested.
- The application uses SQLite for development (can be switched to SQL Server for production).
- reCAPTCHA uses test keys by default (replace with production keys in `appsettings.json`).
- Password reset emails are logged but not sent (implement email service for production).
- HTTPS redirection works in production; for development, ensure HTTPS is configured.

---

**Last Updated**: 2026-02-05
**Status**: ✅ All Security Features Implemented
