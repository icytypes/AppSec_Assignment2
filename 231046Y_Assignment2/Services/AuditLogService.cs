using _231046Y_Assignment2.Data;
using _231046Y_Assignment2.Models;
using Microsoft.EntityFrameworkCore;

namespace _231046Y_Assignment2.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActivityAsync(int? memberId, string email, string action, string description, string status = "Success")
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var auditLog = new AuditLog
            {
                MemberId = memberId,
                Email = email,
                Action = action,
                Description = description,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
                SessionId = httpContext.Session.GetString("SessionId"),
                Timestamp = DateTime.Now,
                Status = status
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetUserAuditLogsAsync(int memberId, int limit = 50)
        {
            return await _context.AuditLogs
                .Where(a => a.MemberId == memberId)
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
    }
}
