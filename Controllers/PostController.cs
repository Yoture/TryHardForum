using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TryHardForum.Data;
using TryHardForum.Models;
using TryHardForum.ViewModels.Post;
using TryHardForum.ViewModels.Reply;

namespace TryHardForum.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IForumRepository _forumRepository;
        private readonly IAppUserRepository _userRepository;
        private readonly ITopicRepository _topicRepository;
        private static UserManager<AppUser> _userManager;

        public PostController(IPostRepository postRepository, 
            IForumRepository forumRepository,
            UserManager<AppUser> userManager,
            ITopicRepository topicRepository,
            IAppUserRepository userRepository)
        {
            _postRepository = postRepository;
            _forumRepository = forumRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _topicRepository = topicRepository;
        }
        
        // Метод Index() позволяет просмотреть сообщение, а также список ответов, прикреплённых к нему.
        // Только вот нужен ли этот механизм?
        // Согласно моей задумке, список ответов на сообщение будет открываться в основном окне темы,
        // Без манипуляций с подгрузками отдельных страниц.
        
        /*
        public IActionResult Index(int id)
        {
            var post = _postRepository.GetPost(id);

            var replies = GetPostReplies(post)
                .OrderBy(reply => reply.Created);

            var model = new PostIndexModel
            {
                Id = post.Id,
                AuthorId = post.User.Id,
                AuthorName = post.User.UserName,
                AuthorImageUrl = post.User.ProfileImageUrl,
                AuthorRating = post.User.Rating,
                IsAuthorAdmin = IsAuthorAdmin(post.User),
                Created = post.Created,
                PostContent = post.Content,
                Replies = replies,
                TopicId = post.Topic.Id,
                TopicTitle = post.Topic.Title
            };
            return View(model);
        }
        */

        public IActionResult Create(int id)
        {
            // Выборка текущего топика по ID.
            var topic = _topicRepository.GetTopic(id);

            // Формирование новой модели представления
            var model = new CreatePostModel
            {
                TopicId = topic.Id,
                TopicTitle = topic.Title,
                AuthorName = User.Identity.Name
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(CreatePostModel model)
        {
            var userId = _userManager.GetUserId(User);
            // При необходимости кейворд await заменить на метод Result (или это не метод?) в конце цепочки
            var user = await _userManager.FindByIdAsync(userId);
            var post = BuildPost(model, user);

            // Чтобы приложение корректно работало использую метод Wait() для блокировки потока.
            // изначально вместо метода Wait() был кейворд await.
            await _postRepository.CreatePost(post);
            await _userRepository.UpdateUserRating(userId, typeof(Post));

            return RedirectToAction("Index", "Topic", new { id = post.Id });
        }

        public IActionResult Edit(int postId)
        {
            var post = _postRepository.GetPost(postId);

            var model = new EditPostModel
            {
                Content = post.Content,
                Created = post.Created
            };
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var post = _postRepository.GetPost(id);
            var model = new DeletePostModel
            {
                PostId = post.Id,
                PostAuthor = post.AppUser.UserName,
                PostContent = post.Content
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmDelete(int id)
        {
            var post = _postRepository.GetPost(id);
            _postRepository.DeletePost(id);

            return RedirectToAction("Index", "Topic", new { id = post.Topic.Id });
        }

        #region Helpers
        
        // Метод для создания нового сообщения на форуме.
        // Используется в методе AddPost().
        private Post BuildPost(CreatePostModel post, AppUser user)
        {
            var topic = _topicRepository.GetTopic(post.TopicId);

            return new Post
            {
                Content = post.Content,
                Created = DateTime.Now,
                AppUser = user,
                Topic = topic,
                IsArchived = false
            };
        }
        
        // Этот метод работает вместе с действием Index
        
        private IEnumerable<PostReplyModel> GetPostReplies(Post post)
        {
            return post.Replies.Select(reply => new PostReplyModel
            {
                Id = reply.Id,
                AuthorName = reply.AppUser.UserName,
                AuthorId = reply.AppUser.Id,
                AuthorImageUrl = reply.AppUser.ProfileImageUrl,
                AuthorRating = reply.AppUser.Rating,
                Created = reply.Created,
                ReplyContent = reply.Content,
                IsAuthorAdmin = IsAuthorAdmin(reply.AppUser)
            });
        }
        

        private static bool IsAuthorAdmin(AppUser user)
        {
            return _userManager.GetRolesAsync(user)
                .Result.Contains("Admin");
        }

        #endregion
    }
}