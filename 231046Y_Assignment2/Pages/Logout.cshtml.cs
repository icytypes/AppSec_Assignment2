using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Services;
using _231046Y_Assignment2.Data;
using Microsoft.EntityFrameworkCore;

namespace _231046Y_Assignment2.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SessionService _sessionService;
        private readonly AuditLogService _auditLogService;
        private readonly ApplicationDbContext _context;

        public LogoutModel(
            SessionService sessionService,
            AuditLogService auditLogService,
            ApplicationDbContext context)
        {
            _sessionService = sessionService;
            _auditLogService = auditLogService;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var memberId = _sessionService.GetCurrentMemberId();
            if (memberId.HasValue)
            {
                var member = await _context.Members.FindAsync(memberId.Value);
                if (member != null)
                {
                    await _auditLogService.LogActivityAsync(memberId.Value, member.Email, "Logout", "User logged out", "Success");
                }
            }

            _sessionService.ClearSession();
            
            // Clear any TempData messages to prevent them from showing on login page
            TempData.Clear();
            
            return RedirectToPage("/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var memberId = _sessionService.GetCurrentMemberId();
            if (memberId.HasValue)
            {
                var member = await _context.Members.FindAsync(memberId.Value);
                if (member != null)
                {
                    await _auditLogService.LogActivityAsync(memberId.Value, member.Email, "Logout", "User logged out", "Success");
                }
            }

            _sessionService.ClearSession();
            
            // Clear any TempData messages to prevent them from showing on login page
            TempData.Clear();
            
            return RedirectToPage("/Login");
        }
    }
}
