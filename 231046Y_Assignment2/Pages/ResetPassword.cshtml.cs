using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Services;
using _231046Y_Assignment2.Data;
using _231046Y_Assignment2.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace _231046Y_Assignment2.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly PasswordPolicyService _passwordPolicyService;
        private readonly AuditLogService _auditLogService;
        private readonly InputSanitizationService _sanitizationService;

        public ResetPasswordModel(
            ApplicationDbContext context,
            PasswordService passwordService,
            PasswordPolicyService passwordPolicyService,
            AuditLogService auditLogService,
            InputSanitizationService sanitizationService)
        {
            _context = context;
            _passwordService = passwordService;
            _passwordPolicyService = passwordPolicyService;
            _auditLogService = auditLogService;
            _sanitizationService = sanitizationService;
        }

        [BindProperty]
        public ResetPasswordViewModel ResetPassword { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public bool IsValidToken { get; set; }

        public async Task<IActionResult> OnGetAsync(string? token, string? email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ErrorMessage = "Invalid reset link.";
                return Page();
            }

            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
            
            if (resetToken == null)
            {
                ErrorMessage = "Invalid or expired reset token.";
                return Page();
            }

            var member = await _context.Members.FindAsync(resetToken.MemberId);
            if (member == null || member.Email != email)
            {
                ErrorMessage = "Invalid or expired reset token.";
                return Page();
            }

            if (resetToken == null || resetToken.IsUsed || resetToken.ExpiryDate < DateTime.Now)
            {
                ErrorMessage = "Invalid or expired reset token.";
                return Page();
            }

            IsValidToken = true;
            ResetPassword.Email = email;
            ResetPassword.Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                IsValidToken = true;
                return Page();
            }

            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(rt => rt.Token == ResetPassword.Token);
            
            if (resetToken == null)
            {
                ErrorMessage = "Invalid or expired reset token.";
                IsValidToken = false;
                return Page();
            }

            var member = await _context.Members.FindAsync(resetToken.MemberId);
            if (member == null || member.Email != ResetPassword.Email)
            {
                ErrorMessage = "Invalid or expired reset token.";
                IsValidToken = false;
                return Page();
            }

            if (resetToken.IsUsed || resetToken.ExpiryDate < DateTime.Now)
            {
                ErrorMessage = "Invalid or expired reset token.";
                IsValidToken = false;
                return Page();
            }

            // Check password strength
            var passwordStrength = _passwordService.CheckPasswordStrength(ResetPassword.NewPassword);
            if (!passwordStrength.IsStrong)
            {
                foreach (var feedback in passwordStrength.Feedback)
                {
                    ModelState.AddModelError("ResetPassword.NewPassword", feedback);
                }
                IsValidToken = true;
                return Page();
            }

            // Check password history
            if (await _passwordPolicyService.IsPasswordInHistoryAsync(member.MemberId, ResetPassword.NewPassword))
            {
                ModelState.AddModelError("ResetPassword.NewPassword", "You cannot reuse a recently used password.");
                IsValidToken = true;
                return Page();
            }

            // Save old password to history
            await _passwordPolicyService.SavePasswordToHistoryAsync(member.MemberId, member.PasswordHash);

            // Update password
            member.PasswordHash = _passwordService.HashPassword(ResetPassword.NewPassword);
            member.PasswordChangedDate = DateTime.Now;
            resetToken.IsUsed = true;
            await _context.SaveChangesAsync();

            await _auditLogService.LogActivityAsync(member.MemberId, member.Email, "ResetPassword", "Password reset successfully", "Success");

            TempData["SuccessMessage"] = "Password reset successfully! Please login with your new password.";
            return RedirectToPage("/Login");
        }
    }

    public class ResetPasswordViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
