using Microsoft.EntityFrameworkCore;
using TryHardForum.Models;

namespace TryHardForum.Data
{
    public class AppDataDbContext : DbContext
    {
        public AppDataDbContext(DbContextOptions<AppDataDbContext> options) : base(options) {  }   

        public DbSet<Forum> Forums { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostReply> PostReplies { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.AppUser).HasForeignKey(p => p.AppUserId);

            modelBuilder.Entity<Forum>()
                .HasMany(f => f.Topics)
                .WithOne(t => t.Forum).HasForeignKey(t => t.ForumId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Topic>()
                .HasMany(t => t.Posts)
                .WithOne(p => p.Topic).HasForeignKey(p => p.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Replies)
                .WithOne(r => r.Post).HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }  
}