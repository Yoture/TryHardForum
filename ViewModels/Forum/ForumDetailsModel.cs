using System.Collections.Generic;
using System.Linq;
using TryHardForum.ViewModels.Topic;

namespace TryHardForum.ViewModels.Forum
{
    public class ForumDetailsModel
    {
        public int ForumId { get; set; }
        public string ForumTitle { get; set; }
        public string ForumDescription { get; set; }
        
        public IEnumerable<TopicInformationModel> TopicList { get; set; }
        public int NumberOfTopics { get; set; }
        
    }
}