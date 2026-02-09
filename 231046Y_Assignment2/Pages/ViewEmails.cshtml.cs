using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Services;

namespace _231046Y_Assignment2.Pages
{
    public class ViewEmailsModel : PageModel
    {
        private readonly EmailLoggerService _emailLogger;

        public ViewEmailsModel(EmailLoggerService emailLogger)
        {
            _emailLogger = emailLogger;
        }

        public string? EmailLog { get; set; }

        public async Task OnGetAsync()
        {
            EmailLog = await _emailLogger.GetEmailLogAsync();
        }

        public async Task<IActionResult> OnPostClearEmailsAsync()
        {
            _emailLogger.ClearEmailLog();
            EmailLog = await _emailLogger.GetEmailLogAsync();
            TempData["SuccessMessage"] = "Email logs cleared successfully.";
            return Page();
        }
    }
}
