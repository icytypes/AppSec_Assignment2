using System.Text.Json;

namespace _231046Y_Assignment2.Services
{
    public class ReCaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;
        private readonly bool _isTestKey;

        public ReCaptchaService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _secretKey = _configuration["ReCaptcha:SecretKey"] ?? "";
            // Check if using Google's test keys
            _isTestKey = _secretKey == "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";
        }

        public async Task<bool> VerifyTokenAsync(string token, string? remoteIp = null)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(_secretKey))
                return false;

            // For test keys, accept any non-empty token
            if (_isTestKey && !string.IsNullOrEmpty(token))
            {
                return true;
            }

            try
            {
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("secret", _secretKey),
                    new KeyValuePair<string, string>("response", token)
                };

                if (!string.IsNullOrEmpty(remoteIp))
                {
                    formData.Add(new KeyValuePair<string, string>("remoteip", remoteIp));
                }

                var content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonResponse);

                if (result?.Success == true)
                {
                    // For reCAPTCHA v3, check score (0.0 to 1.0)
                    // Score >= 0.5 is considered human
                    // For test keys or if score is 0 (might be v2), accept it
                    if (result.Score == 0 || result.Score >= 0.5)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log error but don't expose to user
                System.Diagnostics.Debug.WriteLine($"reCAPTCHA verification error: {ex.Message}");
                return false;
            }
        }

        private class ReCaptchaResponse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
            public string? ChallengeTs { get; set; }
            public string? Hostname { get; set; }
            public List<string>? ErrorCodes { get; set; }
        }
    }
}
