using System.Net;
using System.Net.Mail;

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
                    _logger.LogInformation("Email sending is disabled. Reset link for {Email}: {ResetLink}", toEmail, resetLink);
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
                    _logger.LogWarning("SMTP credentials not configured. Email not sent. Reset link: {ResetLink}", resetLink);
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
                    _logger.LogInformation("Password reset email sent to {Email}", toEmail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", toEmail);
                return false;
            }
        }
    }
}
