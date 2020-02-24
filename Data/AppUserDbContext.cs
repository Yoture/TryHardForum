using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TryHardForum.Models;

namespace TryHardForum.Data
{
    public class AppUserDbContext : IdentityDbContext<AppUser>
    {
        public AppUserDbContext(DbContextOptions<AppUserDbContext> options) : base(options) {  }
        public DbSet<AppUser> AppUsers { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Forums)
                .WithOne(f => f.AppUser).HasForeignKey(f => f.AppUserId);
            
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Topics)
                .WithOne(t => t.AppUser).HasForeignKey(t => t.AppUserId);
            
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.AppUser).HasForeignKey(p => p.AppUserId);
            
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Replies)
                .WithOne(r => r.AppUser).HasForeignKey(r => r.AppUserId);
        }
           
    }  
}