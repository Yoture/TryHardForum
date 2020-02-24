using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TryHardForum.Data;
using TryHardForum.Models;
using TryHardForum.ViewModels.Forum;
using TryHardForum.ViewModels.Post;
using TryHardForum.ViewModels.Topic;

namespace TryHardForum.Controllers
{
    [Authorize]
    public class TopicController : Controller
    {
        private readonly IForumRepository _forumRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IPostRepository _postRepository;
        private static UserManager<AppUser> _userManager;

        public TopicController(IForumRepository forumRepository,
            ITopicRepository topicRepository,
            IPostRepository postRepository,
            UserManager<AppUser> userManager)
        {
            _forumRepository = forumRepository;
            _topicRepository = topicRepository;
            _postRepository = postRepository;
            _userManager = userManager;
        }
        
        // Отображает Тему изнутри (т.е. сообщения внутри темы). Данные отображаются в соответствии с id топика.
        [AllowAnonymous]
        public IActionResult Index(int id)
        {
            // Получение данных по теме
            var topic = _topicRepository.GetTopic(id);
            
            // Извлечение всех тем, помещение данных каждой темы в новую отдельную модель представления.
            // Все темы таким образом помещаются в перечисление.
            var topics = _topicRepository.GetAllTopics().Select(topic => 
                new TopicInformationModel
                {
                    Id = topic.Id,
                    Title = topic.Title,
                    // Выбрать самое первое сообщение в качестве описания.
                    // Должен отображаться только контент сообщения.
                    Description = topic.Description,
                    AuthorId = topic.AppUserId,
                    // AuthorName = topic.AuthorName,
                    NumberOfPosts = topic.Posts?.Count() ??  0,
                    NumberOfUsers = _topicRepository.GetActiveUsers(topic.Id).Count(),
                    // Выбрать последнее сообщение, чтобы его отобразить в списке.
                    // LatestPost = _topicRepository.GetLatestPost(topic.Id),
                    // Нужно ли?
                    HasRecentPost = _topicRepository.HasRecentPost(topic.Id)
                });

            // Преобразование списка тем из перечисления в тип IList<T>, если темы есть, и если нет в
            // - обычный список.
            var topicListingModels = topics as IList<TopicInformationModel> ?? topics.ToList();

            // Формирование новой модели представления, в которую помещаются все темы по алфавитному порядку
            // а также подсчитывается их количество.
            var model = new TopicIndexModel
            {
                TopicList = topicListingModels.OrderBy(topic => topic.Title),
                NumberOfTopics = topicListingModels.Count()
            };
            // В итоге отображение всего списка 
            // return View(model);
            return RedirectToAction("Index", "Forum");
        }
        
        // Создание новой темы.
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            TempData["ReturnUrl"] = Request.Headers["Referer"];
            var forum = _forumRepository.GetForum(id);
            var user = await _userManager.GetUserAsync(User);

            // Заранее происходит привязка темы к форуму и автору.
            var model = new TopicCreateModel
            {
                ForumId = forum.Id,
                AuthorId = user.Id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TopicCreateModel model, string returnUrl)
        {
            var user = await _userManager.FindByIdAsync(model.AuthorId);
            
            // Формирование новой темы.
            var topic = new Topic
            {
                Title = model.Title,
                Description = model.Description,
                Created = DateTime.Now,
                AppUserId = model.AuthorId,
                ForumId = model.ForumId,
                IsArchived = false
            };
            // TODO Нужна проверка на уникальность названия Темы.
            
            // Создание новой темы.
            await _topicRepository.CreateTopic(topic);
            
            // Поиск созданной темы по названию.
            var newTopic = _topicRepository.GetTopicByTitle(model.Title);
            
            // Формирование первого сообщения темы.
            var firstPost = new Post
            {
                Content = model.Description,
                Created = DateTime.Now,
                AppUserId = model.AuthorId,
                // Указание ID найденного топика
                TopicId = newTopic.Id
            };
            
            // Создание сообщения
            await _postRepository.CreatePost(firstPost);
            // После того, как сообщение создано - нужно создать следом первое сообщение.
            return Redirect(returnUrl);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            TempData["ReturnUrl"] = Request.Headers["Referer"];
            var topic = _topicRepository.GetTopic(id);

            var model = new EditTopicModel
            {
                TopicId = topic.Id,
                TopicTitle = topic.Title
            };
            return View(model);
        }
        
        // TODO Edit topic title.
        [HttpPost]
        public async Task<IActionResult> Edit(EditTopicModel model, string returnUrl)
        {
            // TODO множество проверок введённых данных.
            // Когда поля будут заполнены и проверены, они вносятся в метод UpdateTopic().
            await _topicRepository.UpdateTopic(model.TopicId, model.TopicTitle);
            return Redirect(returnUrl);
        }
        
        // TODO Delete topic (Admin role only).
        public async Task<IActionResult> Delete(int id)
        {
            // TempData["ReturnUrl"] = Request.Headers["Referer"];
            await _topicRepository.DeleteTopic(id);
            return RedirectToAction("Details", "Forum");
        }

        #region Helpers
        
        // TODO topic listing
        // GET-версия метода вызывает POST-версию для формирования списка форумов.
        private static TopicInformationModel BuildTopicListing(Post post)
        {
            var topic = post.Topic;
            return BuildTopicListing(topic);
        }

        private static TopicInformationModel BuildTopicListing(Topic topic)
        {
            // Формирует список форумов путём создания объектов ForumListingModel.
            return new TopicInformationModel
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
            };
        }

        #endregion
    }
}