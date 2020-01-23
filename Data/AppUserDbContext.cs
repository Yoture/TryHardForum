using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TryHardForum.Models;

namespace TryHardForum.Data
{
    public class AppUserDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<AppUser> AppUsers { get; set; }
        
        public AppUserDbContext(DbContextOptions<AppUserDbContext> options) : base(options) {  }   
    }  
}