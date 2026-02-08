using System.ComponentModel.DataAnnotations;
using _231046Y_Assignment2.Attributes;

namespace _231046Y_Assignment2.Models
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Credit Card Number is required")]
        [Display(Name = "Credit Card Number")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Invalid credit card number format")]
        public string CreditCardNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required")]
        [Display(Name = "Mobile Number")]
        [RegularExpression(@"^[0-9]{8,15}$", ErrorMessage = "Invalid mobile number format")]
        public string MobileNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery Address is required")]
        [Display(Name = "Delivery Address")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [Display(Name = "Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long")]
        [StrongPassword(ErrorMessage = "Password must contain at least 12 characters with uppercase, lowercase, numbers, and special characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Photo is required")]
        [Display(Name = "Photo (.JPG only)")]
        public IFormFile? Photo { get; set; }

        [Display(Name = "About Me")]
        [StringLength(2000, ErrorMessage = "About Me cannot exceed 2000 characters")]
        public string? AboutMe { get; set; }
    }
}
