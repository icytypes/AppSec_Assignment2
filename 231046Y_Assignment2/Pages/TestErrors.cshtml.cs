using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _231046Y_Assignment2.Pages
{
    public class TestErrorsModel : PageModel
    {
        private readonly ILogger<TestErrorsModel> _logger;

        public TestErrorsModel(ILogger<TestErrorsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostTrigger500()
        {
            // Set 500 status code and redirect to custom error page
            _logger.LogWarning("Test 500 error triggered by user");
            Response.StatusCode = 500;
            return Redirect("/500");
        }

        public IActionResult OnPostTrigger403()
        {
            // Set 403 status code
            Response.StatusCode = 403;
            return Redirect("/403");
        }
    }
}
