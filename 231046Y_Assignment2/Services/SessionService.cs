using _231046Y_Assignment2.Data;
using _231046Y_Assignment2.Models;
using Microsoft.EntityFrameworkCore;

namespace _231046Y_Assignment2.Services
{
    public class SessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const int SessionTimeoutMinutes = 30;

        public SessionService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public void CreateSession(int memberId, string sessionId)
        {
            var member = _context.Members.Find(memberId);
            if (member != null)
            {
                member.SessionId = sessionId;
                member.LastLoginDate = DateTime.Now;
                _context.SaveChanges();
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.SetString("MemberId", memberId.ToString());
                httpContext.Session.SetString("SessionId", sessionId);
                httpContext.Session.SetString("LoginTime", DateTime.Now.ToString());
            }
        }

        public bool IsSessionValid()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || !httpContext.Session.IsAvailable)
                return false;

            var memberIdStr = httpContext.Session.GetString("MemberId");
            var sessionId = httpContext.Session.GetString("SessionId");
            var loginTimeStr = httpContext.Session.GetString("LoginTime");

            if (string.IsNullOrEmpty(memberIdStr) || string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(loginTimeStr))
                return false;

            if (!DateTime.TryParse(loginTimeStr, out DateTime loginTime))
                return false;

            if (DateTime.Now.Subtract(loginTime).TotalMinutes > SessionTimeoutMinutes)
            {
                ClearSession();
                return false;
            }

            int memberId = int.Parse(memberIdStr);
            var member = _context.Members.Find(memberId);
            if (member == null || member.SessionId != sessionId)
            {
                ClearSession();
                return false;
            }

            return true;
        }

        public int? GetCurrentMemberId()
        {
            if (!IsSessionValid())
                return null;

            var httpContext = _httpContextAccessor.HttpContext;
            var memberIdStr = httpContext?.Session.GetString("MemberId");
            return int.TryParse(memberIdStr, out int memberId) ? memberId : null;
        }

        public void ClearSession()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.Clear();
            }
        }

        public bool DetectMultipleLogins(int memberId, string currentSessionId)
        {
            var member = _context.Members.Find(memberId);
            if (member == null)
                return false;

            // Check if there's an existing active session
            if (!string.IsNullOrEmpty(member.SessionId) && member.SessionId != currentSessionId)
            {
                // Check if the old session is still valid (not timed out)
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    // This indicates a new login from a different device/tab
                    return true;
                }
            }

            return false;
        }

        public string? GetCurrentSessionId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Session.GetString("SessionId");
        }

        public string GenerateSessionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
