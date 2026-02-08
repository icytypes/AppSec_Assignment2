using Microsoft.EntityFrameworkCore;
using _231046Y_Assignment2.Models;

namespace _231046Y_Assignment2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(e => e.MemberId);
                entity.HasIndex(e => e.Timestamp);
            });

            modelBuilder.Entity<PasswordHistory>(entity =>
            {
                entity.HasIndex(e => e.MemberId);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasIndex(e => e.MemberId);
                entity.HasIndex(e => e.Token);
            });
        }
    }
}
