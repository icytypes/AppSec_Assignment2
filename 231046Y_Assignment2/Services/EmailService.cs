using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace _231046Y_Assignment2.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailLoggerService _emailLogger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, EmailLoggerService emailLogger)
        {
            _configuration = configuration;
            _logger = logger;
            _emailLogger = emailLogger;
        }

        /// <summary>
        /// Masks email address for logging purposes to prevent exposure of private information
        /// Example: user@example.com -> u***@e***.com
        /// </summary>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return "***";

            var parts = email.Split('@');
            if (parts.Length != 2)
                return "***";

            var localPart = parts[0];
            var domain = parts[1];
            var domainParts = domain.Split('.');

            // Mask local part: show first char, mask the rest
            var maskedLocal = localPart.Length > 0 
                ? localPart[0] + new string('*', Math.Max(0, localPart.Length - 1))
                : "***";

            // Mask domain: show first char of domain and TLD
            if (domainParts.Length >= 2)
            {
                var domainName = domainParts[0];
                var tld = domainParts[domainParts.Length - 1];
                var maskedDomain = (domainName.Length > 0 ? domainName[0].ToString() : "") + 
                                   new string('*', Math.Max(0, domainName.Length - 1)) + 
                                   "." + tld;
                return $"{maskedLocal}@{maskedDomain}";
            }

            return $"{maskedLocal}@{domain[0]}***";
        }

        /// <summary>
        /// Sanitizes user input for logging to prevent log injection/forging attacks
        /// Removes newlines and other control characters that could be used to forge log entries
        /// </summary>
        private static string SanitizeForLogging(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove newlines and carriage returns to prevent log forging
            var sanitized = input.Replace("\r", "").Replace("\n", "").Replace("\r\n", "");
            
            // Remove other control characters (except space)
            sanitized = Regex.Replace(sanitized, @"[\x00-\x08\x0B-\x0C\x0E-\x1F]", "");
            
            // Limit length to prevent log flooding
            const int maxLogLength = 200;
            if (sanitized.Length > maxLogLength)
            {
                sanitized = sanitized.Substring(0, maxLogLength) + "... (truncated)";
            }

            return sanitized;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            try
            {
                // Check if email is enabled in configuration
                var emailEnabled = _configuration.GetValue<bool>("Email:Enabled", false);
                
                // Email body template
                var emailBody = $@"
Hello,

You have requested to reset your password for your Fresh Farm Market account.

Click the link below to reset your password:
{resetLink}

This link will expire in 24 hours.

If you did not request this password reset, please ignore this email.

Best regards,
Fresh Farm Market Team
";
                
                // Always log the email for demo/testing purposes
                await _emailLogger.LogEmailAsync(toEmail, "Password Reset Request - Fresh Farm Market", emailBody, resetLink);

                if (!emailEnabled)
                {
                    // For demo/development: log the email instead of sending
                    // Mask email address in logs to prevent exposure of private information
                    // Note: resetLink is not logged to prevent log forging attacks
                    _logger.LogInformation("Email sending is disabled. Reset link generated for masked email: {MaskedEmail}", MaskEmail(toEmail));
                    return true; // Return true so the user sees success message
                }

                // Email configuration from appsettings.json
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@freshfarmmarket.com";
                var fromName = _configuration["Email:FromName"] ?? "Fresh Farm Market";

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    // Sanitize reset link before logging to prevent log forging
                    var sanitizedResetLink = SanitizeForLogging(resetLink);
                    _logger.LogWarning("SMTP credentials not configured. Email not sent. Reset link: {ResetLink}", sanitizedResetLink);
                    return false;
                }

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, fromName),
                        Subject = "Password Reset Request - Fresh Farm Market",
                        Body = emailBody,
                        IsBodyHtml = false
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    // Mask email address in logs to prevent exposure of private information
                    _logger.LogInformation("Password reset email sent to masked email: {MaskedEmail}", MaskEmail(toEmail));
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Mask email address in logs to prevent exposure of private information
                _logger.LogError(ex, "Error sending password reset email to masked email: {MaskedEmail}", MaskEmail(toEmail));
                return false;
            }
        }
    }
}
