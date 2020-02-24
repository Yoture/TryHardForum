using System.Collections.Generic;
using TryHardForum.ViewModels.Post;
using TryHardForum.ViewModels.Topic;

namespace TryHardForum.ViewModels.Forum
{
    public class ForumListingModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int NumberOfTopics { get; set; }
        public int NumberOfUsers { get; set; }
        
        public IEnumerable<TopicInformationModel> AllTopics { get; set; }
        
        // public bool HasRecentPost { get; set; }
        // public PostListingModel Latest { get; set; }
        // public IEnumerable<PostListingModel> AllPosts { get; set; }
    }
}