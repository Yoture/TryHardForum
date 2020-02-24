using System;
using System.Collections;
using System.Collections.Generic;

namespace TryHardForum.Models
{
    public class Topic : IEnumerable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public bool IsArchived { get; set; }
        
        public int ForumId { get; set; }
        public virtual Forum Forum { get; set; }
        
        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        
        public virtual IEnumerable<Post> Posts { get; set; }
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}