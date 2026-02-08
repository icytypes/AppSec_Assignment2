using System.Text.RegularExpressions;
using System.Net;

namespace _231046Y_Assignment2.Services
{
    public class InputSanitizationService
    {
        public string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove potential SQL injection patterns
            input = Regex.Replace(input, @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT)\b)", "", RegexOptions.IgnoreCase);

            // Remove script tags and event handlers (XSS prevention)
            input = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            input = Regex.Replace(input, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            input = Regex.Replace(input, @"on\w+\s*=", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"javascript:", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"vbscript:", "", RegexOptions.IgnoreCase);

            // HTML encode special characters
            input = WebUtility.HtmlEncode(input);

            // Trim whitespace
            input = input.Trim();

            return input;
        }

        public string SanitizeForDatabase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove null bytes
            input = input.Replace("\0", "");

            // Remove control characters except newline and tab
            input = Regex.Replace(input, @"[\x00-\x08\x0B-\x0C\x0E-\x1F]", "");

            // Remove potential SQL injection patterns (parameterized queries protect, but defense in depth)
            input = Regex.Replace(input, @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT|--|/\*|\*/)\b)", "", RegexOptions.IgnoreCase);

            // HTML encode to prevent XSS when displaying
            input = WebUtility.HtmlEncode(input);

            return input.Trim();
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return regex.IsMatch(email) && email.Length <= 100;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidMobileNumber(string mobileNo)
        {
            if (string.IsNullOrEmpty(mobileNo))
                return false;

            var regex = new Regex(@"^[0-9]{8,15}$");
            return regex.IsMatch(mobileNo);
        }
    }
}
