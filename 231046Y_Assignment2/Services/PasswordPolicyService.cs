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
        private const int MaxPasswordAgeDays = 90; // Must change password after 90 days
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
                if (timeSinceLastChange.TotalDays > MaxPasswordAgeDays)
                {
                    return true;
                }
            }
            else if (member.CreatedDate.AddDays(MaxPasswordAgeDays) < DateTime.Now)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPasswordInHistoryAsync(int memberId, string newPassword)
        {
            var passwordHash = _passwordService.HashPassword(newPassword);
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
