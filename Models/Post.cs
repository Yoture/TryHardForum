using System;
using System.Collections.Generic;

namespace TryHardForum.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public bool IsArchived { get; set; }
        
        public int TopicId { get; set; }
        public virtual Topic Topic { get; set; }
        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        public virtual IEnumerable<PostReply> Replies { get; set; }
    }
}