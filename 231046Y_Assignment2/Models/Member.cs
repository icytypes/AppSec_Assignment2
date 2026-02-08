using System.ComponentModel.DataAnnotations;

namespace _231046Y_Assignment2.Models
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string CreditCardNo { get; set; } = string.Empty; // Encrypted

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string MobileNo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(255)]
        public string? PhotoPath { get; set; }

        [StringLength(2000)]
        public string? AboutMe { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        [StringLength(100)]
        public string? SessionId { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? AccountLockedUntil { get; set; }

        public DateTime? PasswordChangedDate { get; set; }

        public bool IsTwoFactorEnabled { get; set; } = false;

        [StringLength(100)]
        public string? TwoFactorSecret { get; set; }

        [StringLength(50)]
        public string? TwoFactorBackupCodes { get; set; }
    }
}
