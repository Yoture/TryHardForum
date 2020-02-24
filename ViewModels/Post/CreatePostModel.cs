using System;

namespace TryHardForum.ViewModels.Post
{
    public class CreatePostModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        
        public int TopicId { get; set; }
        public string TopicTitle { get; set; }
        // public string ForumImageUrl { get; set; }
        
        public string AuthorName { get; set; }
        public string UserId { get; set; }
    }
}