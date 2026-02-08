using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _231046Y_Assignment2.Models;
using _231046Y_Assignment2.Services;
using _231046Y_Assignment2.Data;
using Microsoft.EntityFrameworkCore;

namespace _231046Y_Assignment2.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EncryptionService _encryptionService;
        private readonly PasswordService _passwordService;
        private readonly ReCaptchaService _reCaptchaService;
        private readonly AuditLogService _auditLogService;
        private readonly InputSanitizationService _sanitizationService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            ApplicationDbContext context,
            EncryptionService encryptionService,
            PasswordService passwordService,
            ReCaptchaService reCaptchaService,
            AuditLogService auditLogService,
            InputSanitizationService sanitizationService,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<RegisterModel> logger)
        {
            _context = context;
            _encryptionService = encryptionService;
            _passwordService = passwordService;
            _reCaptchaService = reCaptchaService;
            _auditLogService = auditLogService;
            _sanitizationService = sanitizationService;
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public RegistrationViewModel Registration { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            ViewData["ReCaptchaSiteKey"] = _configuration["ReCaptcha:SiteKey"] ?? "";
        }

        public async Task<IActionResult> OnPostAsync(string? recaptchaToken = null)
        {
            ViewData["ReCaptchaSiteKey"] = _configuration["ReCaptcha:SiteKey"] ?? "";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Sanitize all inputs
            Registration.FullName = _sanitizationService.SanitizeForDatabase(Registration.FullName);
            Registration.CreditCardNo = _sanitizationService.SanitizeForDatabase(Registration.CreditCardNo);
            Registration.Gender = _sanitizationService.SanitizeForDatabase(Registration.Gender);
            Registration.MobileNo = _sanitizationService.SanitizeForDatabase(Registration.MobileNo);
            Registration.DeliveryAddress = _sanitizationService.SanitizeForDatabase(Registration.DeliveryAddress);
            Registration.Email = _sanitizationService.SanitizeForDatabase(Registration.Email);
            if (!string.IsNullOrEmpty(Registration.AboutMe))
            {
                Registration.AboutMe = _sanitizationService.SanitizeForDatabase(Registration.AboutMe);
            }

            // Validate email and mobile
            if (!_sanitizationService.IsValidEmail(Registration.Email))
            {
                ModelState.AddModelError("Registration.Email", "Invalid email format.");
                return Page();
            }

            if (!_sanitizationService.IsValidMobileNumber(Registration.MobileNo))
            {
                ModelState.AddModelError("Registration.MobileNo", "Invalid mobile number format.");
                return Page();
            }

            // Verify reCAPTCHA (skip if token is "test-token" for development)
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (recaptchaToken != "test-token" && (string.IsNullOrEmpty(recaptchaToken) || !await _reCaptchaService.VerifyTokenAsync(recaptchaToken, remoteIp)))
            {
                ModelState.AddModelError("", "reCAPTCHA verification failed. Please try again.");
                await _auditLogService.LogActivityAsync(null, Registration.Email, "Register", "reCAPTCHA verification failed", "Failed");
                return Page();
            }

            var passwordStrength = _passwordService.CheckPasswordStrength(Registration.Password);
            if (!passwordStrength.IsStrong)
            {
                foreach (var feedback in passwordStrength.Feedback)
                {
                    ModelState.AddModelError("Registration.Password", feedback);
                }
                return Page();
            }

            if (await _context.Members.AnyAsync(m => m.Email == Registration.Email))
            {
                ModelState.AddModelError("Registration.Email", "This email address is already registered.");
                await _auditLogService.LogActivityAsync(null, Registration.Email, "Register", "Duplicate email attempt", "Failed");
                return Page();
            }

            if (Registration.Photo == null || Registration.Photo.Length == 0)
            {
                ModelState.AddModelError("Registration.Photo", "Photo is required.");
                return Page();
            }

            // File upload restrictions - Only JPG files allowed
            var allowedExtensions = new[] { ".jpg", ".jpeg" };
            var fileExtension = Path.GetExtension(Registration.Photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("Registration.Photo", "Only JPG files are allowed.");
                return Page();
            }

            // Additional file validation - check MIME type
            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg" };
            if (!allowedMimeTypes.Contains(Registration.Photo.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError("Registration.Photo", "Invalid file type. Only JPG images are allowed.");
                return Page();
            }

            // Check file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (Registration.Photo.Length > maxFileSize)
            {
                ModelState.AddModelError("Registration.Photo", "File size exceeds 5MB limit.");
                return Page();
            }

            string photoPath = string.Empty;
            if (Registration.Photo != null && Registration.Photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "photos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                photoPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(photoPath, FileMode.Create))
                {
                    await Registration.Photo.CopyToAsync(fileStream);
                }

                photoPath = $"/uploads/photos/{uniqueFileName}";
            }

            var member = new Member
            {
                FullName = Registration.FullName,
                CreditCardNo = _encryptionService.Encrypt(Registration.CreditCardNo),
                Gender = Registration.Gender,
                MobileNo = Registration.MobileNo,
                DeliveryAddress = Registration.DeliveryAddress,
                Email = Registration.Email,
                PasswordHash = _passwordService.HashPassword(Registration.Password),
                PhotoPath = photoPath,
                AboutMe = Registration.AboutMe,
                CreatedDate = DateTime.Now,
                PasswordChangedDate = DateTime.Now
            };

            try
            {
                _context.Members.Add(member);
                await _context.SaveChangesAsync();
                
                await _auditLogService.LogActivityAsync(member.MemberId, Registration.Email, "Register", "Successful registration", "Success");
                
                TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                await _auditLogService.LogActivityAsync(null, Registration.Email, "Register", $"Registration error: {ex.Message}", "Failed");
                return Page();
            }
        }
    }
}
