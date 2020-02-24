using System.Collections.Generic;

namespace TryHardForum.ViewModels.Topic
{
    public class TopicIndexModel
    {
        // TODO заполнить!
        public int NumberOfTopics { get; set; }
        public IEnumerable<TopicInformationModel> TopicList { get; set; }
    }
}