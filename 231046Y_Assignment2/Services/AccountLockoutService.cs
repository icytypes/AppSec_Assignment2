using _231046Y_Assignment2.Data;
using _231046Y_Assignment2.Models;
using Microsoft.EntityFrameworkCore;

namespace _231046Y_Assignment2.Services
{
    public class AccountLockoutService
    {
        private readonly ApplicationDbContext _context;
        private const int MaxFailedAttempts = 3;
        private const int LockoutDurationMinutes = 1; // 1 minute for demo/testing

        public AccountLockoutService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
            if (member == null) return false;

            if (member.AccountLockedUntil.HasValue && member.AccountLockedUntil.Value > DateTime.Now)
            {
                return true;
            }

            if (member.AccountLockedUntil.HasValue && member.AccountLockedUntil.Value <= DateTime.Now)
            {
                member.AccountLockedUntil = null;
                member.FailedLoginAttempts = 0;
                await _context.SaveChangesAsync();
            }

            return false;
        }

        public async Task<int> GetRemainingLockoutMinutesAsync(string email)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
            if (member == null || !member.AccountLockedUntil.HasValue) return 0;

            if (member.AccountLockedUntil.Value > DateTime.Now)
            {
                return (int)Math.Ceiling((member.AccountLockedUntil.Value - DateTime.Now).TotalMinutes);
            }

            return 0;
        }

        public async Task RecordFailedLoginAsync(string email)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
            if (member == null) return;

            member.FailedLoginAttempts++;

            if (member.FailedLoginAttempts >= MaxFailedAttempts)
            {
                member.AccountLockedUntil = DateTime.Now.AddMinutes(LockoutDurationMinutes);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ResetFailedLoginAttemptsAsync(string email)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
            if (member == null) return;

            member.FailedLoginAttempts = 0;
            member.AccountLockedUntil = null;
            await _context.SaveChangesAsync();
        }
    }
}
