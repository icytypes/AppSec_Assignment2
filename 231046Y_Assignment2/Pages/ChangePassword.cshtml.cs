using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Models;
using _231046Y_Assignment2.Services;
using _231046Y_Assignment2.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace _231046Y_Assignment2.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly PasswordPolicyService _passwordPolicyService;
        private readonly SessionService _sessionService;
        private readonly AuditLogService _auditLogService;
        private readonly InputSanitizationService _sanitizationService;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            ApplicationDbContext context,
            PasswordService passwordService,
            PasswordPolicyService passwordPolicyService,
            SessionService sessionService,
            AuditLogService auditLogService,
            InputSanitizationService sanitizationService,
            ILogger<ChangePasswordModel> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _passwordPolicyService = passwordPolicyService;
            _sessionService = sessionService;
            _auditLogService = auditLogService;
            _sanitizationService = sanitizationService;
            _logger = logger;
        }

        [BindProperty]
        public ChangePasswordViewModel ChangePassword { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public bool MustChangePassword { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_sessionService.IsSessionValid())
            {
                return RedirectToPage("/Login");
            }

            var memberId = _sessionService.GetCurrentMemberId();
            if (memberId == null)
            {
                return RedirectToPage("/Login");
            }

            MustChangePassword = await _passwordPolicyService.MustChangePasswordAsync(memberId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!_sessionService.IsSessionValid())
            {
                return RedirectToPage("/Login");
            }

            var memberId = _sessionService.GetCurrentMemberId();
            if (memberId == null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                MustChangePassword = await _passwordPolicyService.MustChangePasswordAsync(memberId.Value);
                return Page();
            }

            // Note: Passwords should NOT be sanitized - they're hashed immediately and never displayed
            // Sanitization would change special characters and cause hash mismatches

            var member = await _context.Members.FindAsync(memberId.Value);
            if (member == null)
            {
                return RedirectToPage("/Login");
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(ChangePassword.CurrentPassword, member.PasswordHash))
            {
                ModelState.AddModelError("ChangePassword.CurrentPassword", "Current password is incorrect.");
                MustChangePassword = await _passwordPolicyService.MustChangePasswordAsync(memberId.Value);
                await _auditLogService.LogActivityAsync(memberId, member.Email, "ChangePassword", "Incorrect current password", "Failed");
                return Page();
            }

            // Check if can change password (min age)
            if (!await _passwordPolicyService.CanChangePasswordAsync(memberId.Value))
            {
                ModelState.AddModelError("", "You cannot change your password so soon after the last change. Please wait a moment.");
                MustChangePassword = await _passwordPolicyService.MustChangePasswordAsync(memberId.Value);
                return Page();
            }

            // Check password strength
            var passwordStrength = _passwordService.CheckPasswordStrength(ChangePassword.NewPassword);
            if (!passwordStrength.IsStrong)
            {
                foreach (var feedback in passwordStrength.Feedback)
                {
                    ModelState.AddModelError("ChangePassword.NewPassword", feedback);
                }
                MustChangePassword = await _passwordPolicyService.MustChangePasswordAsync(memberId.Value);
                return Page();
            }

            // Check password history
            if (await _passwordPolicyService.IsPasswordInHistoryAsync(memberId.Value, ChangePassword.NewPassword))
            {
                ModelState.AddModelError("ChangePassword.NewPassword", "You cannot reuse a recently used password.");
                MustChangePassword = await _passwordPolicyService.MustChangePasswordAsync(memberId.Value);
                return Page();
            }

            // Save old password to history
            await _passwordPolicyService.SavePasswordToHistoryAsync(memberId.Value, member.PasswordHash);

            // Update password
            member.PasswordHash = _passwordService.HashPassword(ChangePassword.NewPassword);
            member.PasswordChangedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            await _auditLogService.LogActivityAsync(memberId, member.Email, "ChangePassword", "Password changed successfully", "Success");

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToPage("/Index");
        }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

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
