using System.Collections.Generic;
using TryHardForum.ViewModels.Post;

namespace TryHardForum.ViewModels.Home
{
    public class HomeIndexModel
    {
        public string SearchQuery { get; set; }
        public IEnumerable<PostListingModel> LatestPosts { get; set; }
    }
}