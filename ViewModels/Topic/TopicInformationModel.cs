using System;
using System.Collections.Generic;
using TryHardForum.ViewModels.Post;

namespace TryHardForum.ViewModels.Topic
{
    public class TopicInformationModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public int NumberOfPosts { get; set; }
        public int NumberOfUsers { get; set; }
        public bool HasRecentPost { get; set; }
        
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        
        public Models.Post LatestPost { get; set; }
        
        public IEnumerable<PostListingModel> AllPosts { get; set; }
        
    }
}