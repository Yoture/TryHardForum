using System;

namespace TryHardForum.Models
{
    // Не понимаю, зачем эта модель, если есть почти с таким же названием, но в директории ViewModels???
    // Срочно перекопать весь код!
    public class PostReply
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }

        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}