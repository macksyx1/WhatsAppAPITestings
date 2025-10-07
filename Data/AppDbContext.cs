using System.Collections.Generic;
using System.Reflection.Emit;
using WhatsAppTestLog.Models;
using Microsoft.EntityFrameworkCore;

namespace WhatsAppTestLog.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<OTPCode> OTPCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
            });

            modelBuilder.Entity<OTPCode>(entity =>
            {
                entity.HasIndex(o => o.PhoneNumber);
                entity.HasIndex(o => o.CreatedAt);
            });
        }
    }
}
