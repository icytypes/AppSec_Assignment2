using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace _231046Y_Assignment2.Services
{
    public class PasswordService
    {
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            string passwordHash = HashPassword(password);
            return passwordHash == hash;
        }

        public PasswordStrength CheckPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return new PasswordStrength { Score = 0, IsStrong = false, Feedback = new List<string> { "Password cannot be empty" } };

            int score = 0;
            var feedback = new List<string>();

            if (password.Length >= 12)
                score += 2;
            else
                feedback.Add("Password must be at least 12 characters long");

            if (Regex.IsMatch(password, @"[a-z]"))
                score += 1;
            else
                feedback.Add("Password must contain at least one lowercase letter");

            if (Regex.IsMatch(password, @"[A-Z]"))
                score += 1;
            else
                feedback.Add("Password must contain at least one uppercase letter");

            if (Regex.IsMatch(password, @"[0-9]"))
                score += 1;
            else
                feedback.Add("Password must contain at least one number");

            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                score += 1;
            else
                feedback.Add("Password must contain at least one special character");

            return new PasswordStrength
            {
                Score = score,
                IsStrong = score >= 6,
                Feedback = feedback
            };
        }
    }

    public class PasswordStrength
    {
        public int Score { get; set; }
        public bool IsStrong { get; set; }
        public List<string> Feedback { get; set; } = new List<string>();
    }
}
