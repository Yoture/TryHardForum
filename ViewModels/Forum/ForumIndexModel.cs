using System.Collections.Generic;

namespace TryHardForum.ViewModels.Forum
{
    public class ForumIndexModel
    {
        public int NumberOfForums { get; set; }
        public IEnumerable<ForumListingModel> ForumList { get; set; }
    }
}