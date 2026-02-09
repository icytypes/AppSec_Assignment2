using _231046Y_Assignment2.Data;
using _231046Y_Assignment2.Models;
using Microsoft.EntityFrameworkCore;

namespace _231046Y_Assignment2.Services
{
    public class PasswordPolicyService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private const int MinPasswordAgeMinutes = 1; // Cannot change password within 1 minute
        private const int MaxPasswordAgeMinutes = 2; // Must change password after 2 minutes (for demo/testing)
        private const int MaxPasswordHistory = 2; // Remember last 2 passwords

        public PasswordPolicyService(ApplicationDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<bool> CanChangePasswordAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member == null) return false;

            if (member.PasswordChangedDate.HasValue)
            {
                var timeSinceLastChange = DateTime.Now - member.PasswordChangedDate.Value;
                if (timeSinceLastChange.TotalMinutes < MinPasswordAgeMinutes)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> MustChangePasswordAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member == null) return false;

            if (member.PasswordChangedDate.HasValue)
            {
                var timeSinceLastChange = DateTime.Now - member.PasswordChangedDate.Value;
                if (timeSinceLastChange.TotalMinutes > MaxPasswordAgeMinutes)
                {
                    return true;
                }
            }
            else if (member.CreatedDate.AddMinutes(MaxPasswordAgeMinutes) < DateTime.Now)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPasswordInHistoryAsync(int memberId, string newPassword)
        {
            var passwordHash = _passwordService.HashPassword(newPassword);
            
            // First, check if the new password matches the current password
            var member = await _context.Members.FindAsync(memberId);
            if (member != null && member.PasswordHash == passwordHash)
            {
                return true; // Cannot reuse current password
            }
            
            // Then check password history (last 2 passwords)
            var history = await _context.PasswordHistories
                .Where(ph => ph.MemberId == memberId)
                .OrderByDescending(ph => ph.CreatedDate)
                .Take(MaxPasswordHistory)
                .ToListAsync();

            return history.Any(ph => ph.PasswordHash == passwordHash);
        }

        public async Task SavePasswordToHistoryAsync(int memberId, string passwordHash)
        {
            var history = await _context.PasswordHistories
                .Where(ph => ph.MemberId == memberId)
                .OrderByDescending(ph => ph.CreatedDate)
                .ToListAsync();

            if (history.Count >= MaxPasswordHistory)
            {
                var oldest = history.OrderBy(ph => ph.CreatedDate).First();
                _context.PasswordHistories.Remove(oldest);
            }

            _context.PasswordHistories.Add(new PasswordHistory
            {
                MemberId = memberId,
                PasswordHash = passwordHash,
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }
}
