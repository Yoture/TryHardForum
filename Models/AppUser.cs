using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TryHardForum.Models
{
    // С добавлением <int> можно перезаписать Id в int.
    // Аналогичным способом можно будет поступить с ролями (IdentityRole<int>).
    public class AppUser : IdentityUser
    {
        public override string Id { get; set; }
        [PersonalData]
        public override string UserName { get; set; }
        public override string Email { get; set; }
        public int Rating { get; set; }
        public string UserDescription { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateTime MemberSince { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsActive { get; set; }
        public bool RememberMe { get; set; }
        
        // 
        public virtual IEnumerable<Forum> Forums { get; set; }
        public virtual IEnumerable<Topic> Topics { get; set; }
        public virtual IEnumerable<Post> Posts { get; set; }
        public virtual IEnumerable<PostReply> Replies { get; set; }
    }
}