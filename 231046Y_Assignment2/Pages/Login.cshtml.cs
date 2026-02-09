using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Models;
using _231046Y_Assignment2.Services;
using _231046Y_Assignment2.Data;
using Microsoft.EntityFrameworkCore;

namespace _231046Y_Assignment2.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly ReCaptchaService _reCaptchaService;
        private readonly SessionService _sessionService;
        private readonly AccountLockoutService _lockoutService;
        private readonly AuditLogService _auditLogService;
        private readonly InputSanitizationService _sanitizationService;
        private readonly TwoFactorService _twoFactorService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            ApplicationDbContext context,
            PasswordService passwordService,
            ReCaptchaService reCaptchaService,
            SessionService sessionService,
            AccountLockoutService lockoutService,
            AuditLogService auditLogService,
            InputSanitizationService sanitizationService,
            TwoFactorService twoFactorService,
            IConfiguration configuration,
            ILogger<LoginModel> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _reCaptchaService = reCaptchaService;
            _sessionService = sessionService;
            _lockoutService = lockoutService;
            _auditLogService = auditLogService;
            _sanitizationService = sanitizationService;
            _twoFactorService = twoFactorService;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public LoginViewModel Login { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }
        public int? LockoutMinutes { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            if (_sessionService.IsSessionValid())
            {
                Response.Redirect("/");
                return;
            }

            ReturnUrl = returnUrl;
            ViewData["ReCaptchaSiteKey"] = _configuration["ReCaptcha:SiteKey"] ?? "";
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null, string? recaptchaToken = null)
        {
            try
            {
                ReturnUrl = returnUrl;

                ViewData["ReCaptchaSiteKey"] = _configuration["ReCaptcha:SiteKey"] ?? "";

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Sanitize email (but NOT password - passwords should be hashed as-is)
                Login.Email = _sanitizationService.SanitizeForDatabase(Login.Email);
                // Password should NOT be sanitized - it's hashed immediately and never displayed

                if (!_sanitizationService.IsValidEmail(Login.Email))
                {
                    ModelState.AddModelError("Login.Email", "Invalid email format.");
                    await _auditLogService.LogActivityAsync(null, Login.Email, "Login", "Invalid email format", "Failed");
                    return Page();
                }

                // Check account lockout
                if (await _lockoutService.IsAccountLockedAsync(Login.Email))
                {
                    LockoutMinutes = await _lockoutService.GetRemainingLockoutMinutesAsync(Login.Email);
                    ModelState.AddModelError("", $"Account is locked. Please try again in {LockoutMinutes} minute(s).");
                    await _auditLogService.LogActivityAsync(null, Login.Email, "Login", "Account locked", "Blocked");
                    return Page();
                }

                // Verify reCAPTCHA (skip if token is "test-token" for development)
                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (recaptchaToken != "test-token" && (string.IsNullOrEmpty(recaptchaToken) || !await _reCaptchaService.VerifyTokenAsync(recaptchaToken, remoteIp)))
                {
                    ModelState.AddModelError("", "reCAPTCHA verification failed. Please try again.");
                    await _auditLogService.LogActivityAsync(null, Login.Email, "Login", "reCAPTCHA verification failed", "Failed");
                    return Page();
                }

                var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == Login.Email);
                if (member == null || !_passwordService.VerifyPassword(Login.Password, member.PasswordHash))
                {
                    if (member != null)
                    {
                        await _lockoutService.RecordFailedLoginAsync(Login.Email);
                    }
                    ModelState.AddModelError("", "Invalid email or password.");
                    await _auditLogService.LogActivityAsync(member?.MemberId, Login.Email, "Login", "Invalid credentials", "Failed");
                    return Page();
                }

                // Check 2FA if enabled
                if (member.IsTwoFactorEnabled && !string.IsNullOrEmpty(member.TwoFactorSecret))
                {
                    var twoFactorCode = Request.Form["TwoFactorCode"].ToString();
                    if (string.IsNullOrEmpty(twoFactorCode) || !_twoFactorService.VerifyCode(member.TwoFactorSecret, twoFactorCode))
                    {
                        ModelState.AddModelError("", "Invalid two-factor authentication code.");
                        await _auditLogService.LogActivityAsync(member.MemberId, Login.Email, "Login", "Invalid 2FA code", "Failed");
                        ViewData["Require2FA"] = true;
                        ViewData["MemberId"] = member.MemberId;
                        return Page();
                    }
                }

                // Reset failed attempts on successful login
                await _lockoutService.ResetFailedLoginAttemptsAsync(Login.Email);

                string sessionId = _sessionService.GenerateSessionId();

                if (_sessionService.DetectMultipleLogins(member.MemberId, sessionId))
                {
                    _logger.LogWarning($"Multiple login detected for member {member.MemberId}. Previous session: {member.SessionId}, New session: {sessionId}");
                    await _auditLogService.LogActivityAsync(member.MemberId, Login.Email, "Login", "Multiple login detected from different device/tab", "Success");
                }

                _sessionService.CreateSession(member.MemberId, sessionId);
                await _auditLogService.LogActivityAsync(member.MemberId, Login.Email, "Login", "Successful login", "Success");

                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                await _auditLogService.LogActivityAsync(null, Login.Email ?? "Unknown", "Login", $"Login error: {ex.Message}", "Failed");
                return RedirectToPage("/500");
            }
        }
    }
}
