using System;

namespace TryHardForum.ViewModels.Post
{
    public class EditPostModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
    }
}