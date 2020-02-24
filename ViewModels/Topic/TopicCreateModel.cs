using System;

namespace TryHardForum.ViewModels.Topic
{
    public class TopicCreateModel
    {
        public string Title { get; set; }
        // Переименовать на FirstPostContent
        public string Description { get; set; }
        public DateTime Created { get; set; }

        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        
        public int ForumId { get; set; }
    }
}