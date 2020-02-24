using System;
using System.Collections.Generic;

namespace TryHardForum.Models
{
    public class Forum
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public string ImageUrl { get; set; }

        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        public virtual IEnumerable<Topic> Topics { get; set; }
    }
}