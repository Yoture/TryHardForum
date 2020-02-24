using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TryHardForum.Data;
using TryHardForum.Models;
using TryHardForum.Services;
using TryHardForum.ViewModels.Reply;

namespace TryHardForum.Controllers
{
    public class ReplyController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAppUserRepository _userRepository;
        private readonly IForumRepository _forumRepository;
        private readonly IPostReplyRepository _replyRepository;
        private readonly ITopicRepository _topicRepository;

        public ReplyController(IPostRepository postRepository,
            UserManager<AppUser> userManager,
            IAppUserRepository userRepository,
            IForumRepository forumRepository,
            IPostReplyRepository replyRepository,
            ITopicRepository topicRepository)
        {
            _postRepository = postRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _replyRepository = replyRepository;
            _forumRepository = forumRepository;
            _topicRepository = topicRepository;
        }
        
        public async Task<IActionResult> Create(int id)
        {
            var post = _postRepository.GetPost(id);
            var topic = _topicRepository.GetTopic(post.Topic.Id);
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            
            var model = new PostReplyModel
            {
                PostContent = post.Content,
                PostId = post.Id,
                
                AuthorId = user.Id,
                AuthorName = User.Identity.Name,
                AuthorImageUrl = user.ProfileImageUrl,
                AuthorRating = user.Rating,
                IsAuthorAdmin = user.IsAdmin,
                
                TopicId = topic.Id,
                TopicTitle = topic.Title,

                Created = DateTime.Now
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostReplyModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            var reply = BuildReply(model, user);

            // Разобраться, что это за метод такой AddReply.
            await _replyRepository.CreateReply(reply);
            await _userRepository.UpdateUserRating(userId, typeof(PostReply));

            return RedirectToAction("Index", "Topic", new{ id = model.PostId });
        }

        // Используется при создании нового ответа.
        private PostReply BuildReply(PostReplyModel model, AppUser user)
        {
            var post = _postRepository.GetPost(model.PostId);

            return new PostReply
            {
                Post = post,
                Content = model.ReplyContent,
                Created = DateTime.Now,
                AppUser = user
            };
        }
    }
}