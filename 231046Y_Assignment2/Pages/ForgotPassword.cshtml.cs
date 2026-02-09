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
        private readonly EmailService _emailService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            ApplicationDbContext context,
            IConfiguration configuration,
            AuditLogService auditLogService,
            InputSanitizationService sanitizationService,
            EmailService emailService,
            ILogger<ForgotPasswordModel> logger)
        {
            _context = context;
            _configuration = configuration;
            _auditLogService = auditLogService;
            _sanitizationService = sanitizationService;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public ForgotPasswordViewModel ForgotPassword { get; set; } = new();

        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public string? ResetLink { get; set; } // For demo purposes - display reset link

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

            // Generate reset link
            var resetLink = $"{Request.Scheme}://{Request.Host}/ResetPassword?token={token}&email={Uri.EscapeDataString(ForgotPassword.Email)}";
            
            // Try to send email
            var emailSent = await _emailService.SendPasswordResetEmailAsync(ForgotPassword.Email, resetLink);
            
            // Log the reset link generation
            await _auditLogService.LogActivityAsync(member.MemberId, ForgotPassword.Email, "ForgotPassword", 
                emailSent ? "Password reset email sent" : $"Reset link generated (email not sent): {resetLink}", "Success");

            var emailEnabled = _configuration.GetValue<bool>("Email:Enabled", false);
            if (!emailEnabled)
            {
                // Email is disabled - show link on page for demo/testing
                ResetLink = resetLink;
                Message = "Password reset link has been generated! Use the link below to reset your password. (Email sending is disabled - see link below or visit '/viewemails' to see all sent emails)";
            }
            else if (emailSent)
            {
                // Email was sent successfully - don't show link
                Message = $"Password reset link has been sent to {ForgotPassword.Email}. Please check your email inbox (and spam folder) for the reset link.";
            }
            else
            {
                // Email sending failed - show link as fallback
                ResetLink = resetLink;
                Message = "Password reset link generated. (Email sending failed - use the link below to reset your password)";
            }
            
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
