# Quick Security Testing Checklist

Use this quick checklist to verify all security features. For detailed instructions, see `SECURITY_TESTING_GUIDE.md`.

## Password Security

- [ ] **Client-side password validation**: Enter weak password in registration form → See real-time feedback
- [ ] **Server-side password validation**: Disable JavaScript, submit weak password → Server rejects it
- [ ] **Password hashing**: Check database → Password is hashed (not plain text)
- [ ] **Password strength**: Try `password123` → Rejected with specific error messages

## Data Encryption

- [ ] **Credit card encryption**: Register with credit card → Check database → Card number is encrypted
- [ ] **Credit card decryption**: Login → View homepage → Card number displayed (masked)

## Session Management

- [ ] **Session creation**: Login → Check browser cookies → Session cookie exists
- [ ] **Session timeout**: Login → Wait 30 min (or modify timeout) → Try accessing page → Redirected to login
- [ ] **Multiple logins**: Login from 2 browsers → Check audit logs → Both sessions logged

## reCAPTCHA

- [ ] **reCAPTCHA loading**: Open registration page → Check Network tab → reCAPTCHA script loaded
- [ ] **Token generation**: Submit form → Check form data → `recaptchaToken` is sent

## Injection Prevention

- [ ] **SQL injection**: Try `' OR '1'='1` in login → Login fails (not bypassed)
- [ ] **XSS prevention**: Enter `<script>alert('XSS')</script>` in name field → Check homepage → Script not executed

## CSRF Protection

- [ ] **Token presence**: View page source → Search for `RequestVerificationToken` → Token exists
- [ ] **Token validation**: Remove token, submit form → Request rejected

## Input Validation

- [ ] **Client validation**: Leave required field empty → Error appears immediately
- [ ] **Server validation**: Disable JavaScript, submit invalid form → Server rejects
- [ ] **Error messages**: Enter invalid email → See error message

## Error Handling

- [ ] **404 page**: Navigate to `/NonExistentPage` → Custom 404 page displayed
- [ ] **403 page**: Access protected resource without auth → Custom 403 page displayed
- [ ] **500 page**: Cause server error → Custom 500 page displayed

## Account Security

- [ ] **Account lockout**: Wrong password 3 times → Account locked
- [ ] **Auto recovery**: Wait 15 minutes → Account automatically unlocked
- [ ] **Password history**: Change password → Try reusing old password → Rejected
- [ ] **Min password age**: Change password → Try changing again immediately → Rejected
- [ ] **Max password age**: Set password date to 90+ days ago → Warning displayed

## Password Management

- [ ] **Change password**: Login → `/ChangePassword` → Change password → Success
- [ ] **Reset password**: `/ForgotPassword` → Enter email → Get reset link → Reset password → Success

## Two-Factor Authentication

- [ ] **Enable 2FA**: Login → `/Enable2FA` → Scan QR code → Enter code → Enabled
- [ ] **2FA login**: Logout → Login → Enter 2FA code → Success
- [ ] **Invalid 2FA**: Enter wrong code → Error message

## HTTPS & Security

- [ ] **HTTPS redirect**: Access via HTTP → Redirected to HTTPS
- [ ] **Access control**: Logout → Try accessing `/Index` → Redirected to login
- [ ] **Audit logging**: Perform actions → Check `AuditLogs` table → All events logged

## Database Checks

Run these SQL queries to verify:

```sql
-- Check password is hashed
SELECT Email, PasswordHash FROM Members;
-- Should see long hash strings, not plain text

-- Check credit card is encrypted
SELECT Email, CreditCardNo FROM Members;
-- Should see encrypted strings, not actual card numbers

-- Check audit logs
SELECT * FROM AuditLogs ORDER BY Timestamp DESC LIMIT 10;
-- Should see recent security events

-- Check password history
SELECT * FROM PasswordHistories;
-- Should see password hashes for recent changes

-- Check reset tokens
SELECT * FROM PasswordResetTokens;
-- Should see tokens with expiry dates
```

## Browser DevTools Checks

1. **F12 → Application → Cookies**: Check session cookie exists with HttpOnly flag
2. **F12 → Network**: Check requests include CSRF token and reCAPTCHA token
3. **F12 → Console**: Check for JavaScript errors or XSS execution
4. **F12 → Elements**: Check forms have hidden antiforgery token inputs

## Quick Test Script

1. Register new account → Check database (password hashed, card encrypted)
2. Login → Check session cookie → Access protected page
3. Try SQL injection in login → Should fail
4. Try XSS in registration → Should be sanitized
5. Change password → Try reusing old password → Should fail
6. Enable 2FA → Login with 2FA code → Should succeed
7. Wrong password 3 times → Account locked → Wait 15 min → Auto unlock
8. Access non-existent page → Custom 404 displayed
9. Check audit logs → All events logged

---

**Time Required**: ~30-60 minutes for full testing
**Priority**: Test critical features (password security, session, injection prevention) first
