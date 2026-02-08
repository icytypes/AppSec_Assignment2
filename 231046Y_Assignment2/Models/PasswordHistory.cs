using System.ComponentModel.DataAnnotations;

namespace _231046Y_Assignment2.Models
{
    public class PasswordHistory
    {
        [Key]
        public int PasswordHistoryId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
