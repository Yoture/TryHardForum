using Microsoft.AspNetCore.Http;
using TryHardForum.Models;

namespace TryHardForum.ViewModels.Forum
{
    public class ForumCreateModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorId { get; set; }
    }
}