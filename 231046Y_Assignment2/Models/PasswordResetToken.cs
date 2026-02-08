using System.ComponentModel.DataAnnotations;

namespace _231046Y_Assignment2.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        [StringLength(255)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
