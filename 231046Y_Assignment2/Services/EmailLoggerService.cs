using System.Text;

namespace _231046Y_Assignment2.Services
{
    public class EmailLoggerService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EmailLoggerService> _logger;
        private readonly string _emailLogPath;

        public EmailLoggerService(IWebHostEnvironment environment, ILogger<EmailLoggerService> logger)
        {
            _environment = environment;
            _logger = logger;
            _emailLogPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "emails", "sent-emails.txt");
        }

        public async Task LogEmailAsync(string toEmail, string subject, string body, string resetLink)
        {
            try
            {
                // Ensure directory exists
                var emailDir = Path.GetDirectoryName(_emailLogPath);
                if (!Directory.Exists(emailDir))
                {
                    Directory.CreateDirectory(emailDir);
                }

                var emailLog = $@"
================================================================================
Email Sent: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
================================================================================
To: {toEmail}
Subject: {subject}

{body}

Reset Link: {resetLink}
================================================================================

";

                await File.AppendAllTextAsync(_emailLogPath, emailLog, Encoding.UTF8);
                // Log only the directory path, not the full path with filename to reduce information exposure
                _logger.LogInformation("Email logged to file in directory: {Directory}", emailDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging email to file");
            }
        }

        public async Task<string> GetEmailLogAsync()
        {
            try
            {
                if (File.Exists(_emailLogPath))
                {
                    return await File.ReadAllTextAsync(_emailLogPath, Encoding.UTF8);
                }
                return "No emails have been sent yet.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading email log");
                return $"Error reading email log: {ex.Message}";
            }
        }

        public void ClearEmailLog()
        {
            try
            {
                if (File.Exists(_emailLogPath))
                {
                    File.Delete(_emailLogPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing email log");
            }
        }
    }
}
