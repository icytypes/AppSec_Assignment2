using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Services;
using _231046Y_Assignment2.Data;
using _231046Y_Assignment2.Models;
using Microsoft.EntityFrameworkCore;

namespace _231046Y_Assignment2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly SessionService _sessionService;
        private readonly EncryptionService _encryptionService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ApplicationDbContext context,
            SessionService sessionService,
            EncryptionService encryptionService,
            ILogger<IndexModel> logger)
        {
            _context = context;
            _sessionService = sessionService;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public MemberDisplayInfo? MemberInfo { get; set; }
        public bool IsLoggedIn { get; set; }

        public class MemberDisplayInfo
        {
            public string FullName { get; set; } = string.Empty;
            public string CreditCardNo { get; set; } = string.Empty;
            public string Gender { get; set; } = string.Empty;
            public string MobileNo { get; set; } = string.Empty;
            public string DeliveryAddress { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? PhotoPath { get; set; }
            public string? AboutMe { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? LastLoginDate { get; set; }
            public bool IsTwoFactorEnabled { get; set; }
        }

        public IActionResult OnGet()
        {
            try
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

                var member = _context.Members.Find(memberId.Value);
                if (member == null)
                {
                    _sessionService.ClearSession();
                    return RedirectToPage("/Login");
                }

                IsLoggedIn = true;
                MemberInfo = new MemberDisplayInfo
                {
                    FullName = member.FullName,
                    CreditCardNo = _encryptionService.Decrypt(member.CreditCardNo),
                    Gender = member.Gender,
                    MobileNo = member.MobileNo,
                    DeliveryAddress = member.DeliveryAddress,
                    Email = member.Email,
                    PhotoPath = member.PhotoPath,
                    AboutMe = member.AboutMe,
                    CreatedDate = member.CreatedDate,
                    LastLoginDate = member.LastLoginDate,
                    IsTwoFactorEnabled = member.IsTwoFactorEnabled
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage for member");
                return RedirectToPage("/500");
            }
        }
    }
}
