using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Services;
using _231046Y_Assignment2.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace _231046Y_Assignment2.Pages
{
    public class Enable2FAModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly SessionService _sessionService;
        private readonly TwoFactorService _twoFactorService;
        private readonly AuditLogService _auditLogService;

        public Enable2FAModel(
            ApplicationDbContext context,
            SessionService sessionService,
            TwoFactorService twoFactorService,
            AuditLogService auditLogService)
        {
            _context = context;
            _sessionService = sessionService;
            _twoFactorService = twoFactorService;
            _auditLogService = auditLogService;
        }

        [BindProperty]
        public Enable2FAViewModel Enable2FA { get; set; } = new();

        public string? Secret { get; set; }
        public string? QRCodeUrl { get; set; }
        public string? ErrorMessage { get; set; }

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

            var member = await _context.Members.FindAsync(memberId.Value);
            if (member == null)
            {
                return RedirectToPage("/Login");
            }

            if (member.IsTwoFactorEnabled)
            {
                return RedirectToPage("/Index");
            }

            Secret = _twoFactorService.GenerateSecret();
            QRCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=otpauth://totp/FreshFarmMarket:{member.Email}?secret={Secret}&issuer=FreshFarmMarket";
            
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

            var member = await _context.Members.FindAsync(memberId.Value);
            if (member == null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                Secret = Enable2FA.Secret;
                QRCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=otpauth://totp/FreshFarmMarket:{member.Email}?secret={Secret}&issuer=FreshFarmMarket";
                return Page();
            }

            if (!_twoFactorService.VerifyCode(Enable2FA.Secret, Enable2FA.VerificationCode))
            {
                ModelState.AddModelError("Enable2FA.VerificationCode", "Invalid verification code. Please try again.");
                Secret = Enable2FA.Secret;
                QRCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=otpauth://totp/FreshFarmMarket:{member.Email}?secret={Secret}&issuer=FreshFarmMarket";
                return Page();
            }

            member.IsTwoFactorEnabled = true;
            member.TwoFactorSecret = Enable2FA.Secret;
            await _context.SaveChangesAsync();

            await _auditLogService.LogActivityAsync(memberId.Value, member.Email, "Enable2FA", "2FA enabled successfully", "Success");

            TempData["SuccessMessage"] = "Two-Factor Authentication has been enabled successfully!";
            return RedirectToPage("/Index");
        }
    }

    public class Enable2FAViewModel
    {
        public string Secret { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        [Display(Name = "Verification Code")]
        public string VerificationCode { get; set; } = string.Empty;
    }
}
