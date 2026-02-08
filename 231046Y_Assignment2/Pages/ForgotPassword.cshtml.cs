using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Services;
using _231046Y_Assignment2.Data;
using _231046Y_Assignment2.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace _231046Y_Assignment2.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AuditLogService _auditLogService;
        private readonly InputSanitizationService _sanitizationService;

        public ForgotPasswordModel(
            ApplicationDbContext context,
            IConfiguration configuration,
            AuditLogService auditLogService,
            InputSanitizationService sanitizationService)
        {
            _context = context;
            _configuration = configuration;
            _auditLogService = auditLogService;
            _sanitizationService = sanitizationService;
        }

        [BindProperty]
        public ForgotPasswordViewModel ForgotPassword { get; set; } = new();

        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            ForgotPassword.Email = _sanitizationService.SanitizeForDatabase(ForgotPassword.Email);

            if (!_sanitizationService.IsValidEmail(ForgotPassword.Email))
            {
                ModelState.AddModelError("ForgotPassword.Email", "Invalid email format.");
                return Page();
            }

            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == ForgotPassword.Email);
            if (member == null)
            {
                // Don't reveal if email exists
                Message = "If an account with that email exists, a password reset link has been sent.";
                IsSuccess = true;
                await _auditLogService.LogActivityAsync(null, ForgotPassword.Email, "ForgotPassword", "Password reset requested", "Success");
                return Page();
            }

            // Generate reset token
            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                MemberId = member.MemberId,
                Token = token,
                ExpiryDate = DateTime.Now.AddHours(24),
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // In production, send email with reset link
            // For now, we'll just log it
            var resetLink = $"{Request.Scheme}://{Request.Host}/ResetPassword?token={token}&email={ForgotPassword.Email}";
            await _auditLogService.LogActivityAsync(member.MemberId, ForgotPassword.Email, "ForgotPassword", $"Reset link generated: {resetLink}", "Success");

            Message = "If an account with that email exists, a password reset link has been sent.";
            IsSuccess = true;

            return Page();
        }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}
