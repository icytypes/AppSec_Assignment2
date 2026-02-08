namespace _231046Y_Assignment2.Services
{
    public class CaptchaService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CaptchaService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateCaptcha()
        {
            Random random = new Random();
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            string captcha = new string(Enumerable.Range(0, 5)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.SetString("CaptchaCode", captcha);
            }

            return captcha;
        }

        public bool ValidateCaptcha(string userInput)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return false;

            var storedCaptcha = httpContext.Session.GetString("CaptchaCode");
            if (string.IsNullOrEmpty(storedCaptcha))
                return false;

            bool isValid = storedCaptcha.Equals(userInput, StringComparison.OrdinalIgnoreCase);
            
            httpContext.Session.Remove("CaptchaCode");
            
            return isValid;
        }
    }
}
