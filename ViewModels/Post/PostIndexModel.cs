using System;
using System.Collections.Generic;
using TryHardForum.ViewModels.Reply;

namespace TryHardForum.ViewModels.Post
{
    public class PostIndexModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorImageUrl { get; set; }
        public int AuthorRating { get; set; }
        public DateTime Created { get; set; }
        public string PostContent { get; set; }
        public bool IsAuthorAdmin { get; set; }

        public int TopicId { get; set; }
        public string TopicTitle { get; set; }

        public IEnumerable<PostReplyModel> Replies { get; set; } 
    }
}