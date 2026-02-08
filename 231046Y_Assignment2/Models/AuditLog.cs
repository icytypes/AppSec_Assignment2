using System.ComponentModel.DataAnnotations;

namespace _231046Y_Assignment2.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public int? MemberId { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Login, Logout, Register, ChangePassword, etc.

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? SessionId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Success"; // Success, Failed, Blocked
    }
}
