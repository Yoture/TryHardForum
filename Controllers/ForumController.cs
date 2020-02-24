using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TryHardForum.Data;
using TryHardForum.Models;
using TryHardForum.Services;
using TryHardForum.ViewModels.Forum;
using TryHardForum.ViewModels.Topic;

namespace TryHardForum.Controllers
{
    public class ForumController : Controller
    {
        private readonly IForumRepository _forumRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IPostRepository _postRepository;
        private readonly IAppUserRepository _userRepository;
        private readonly IUploadService _uploadService;

        private static UserManager<AppUser> _userManager;
        private static IConfiguration _configuration;

        public ForumController(ITopicRepository topicRepository,
            IForumRepository forumRepository,
            IPostRepository postRepository,
            IAppUserRepository userRepository,
            IUploadService uploadService,
            UserManager<AppUser> userManager,
            IConfiguration configuration)
        {
            _forumRepository = forumRepository;
            _topicRepository = topicRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
            _uploadService = uploadService;
            _userManager = userManager;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // Помещение в переменную списка форумов.
            var forums = _forumRepository.GetAllForums().Select(forum => 
                new ForumListingModel
                {
                    Id = forum.Id,
                    Title = forum.Title,
                    Description = forum.Description
                    // NumberOfUsers = _forumRepository.GetActiveUsersInForum(forum.Id).Count(),
                });

            // Преобразование списка форумов в тип IList<T>, если форумы есть, и если нет в  обычный список.
            var forumListingModels = forums as IList<ForumListingModel> ?? forums.ToList();
            
            // Формируется единая модель ForumIndexModel, в которую помещается список форумов, а также проводится
            // их подсчёт.
            var model = new ForumIndexModel
            {
                ForumList = forumListingModels.OrderBy(forum => forum.Title),
                NumberOfForums = forumListingModels.Count()
            };
            return View(model);
        }

        // Отображение списка тем внутри форума.
        public IActionResult Details(int id)
        {
            // Вытягиваю все остальные данные по конкретному форуму (они пригодятся далее).
            var forum = _forumRepository.GetForum(id);

            // Выделяю темы, относящиеся к конкретному форуму
            var topics = _topicRepository.GetAllTopics()
                .Where(topic => topic.Forum?.Id == id).ToList();
                
            // Создаю новый список
            IList<TopicInformationModel> topicListing = new List<TopicInformationModel>();

            // Помещаю в список новые объекты TopicInformationModel, сформированные из списка объектов Topic.
            foreach (var topic in topics)
            {
                var item = new TopicInformationModel
                {
                    Id = topic.Id,
                    Title = topic.Title,
                    // Переписать описание топика, это должен быть первый пост
                    Description = topic.Description,
                    // Правильно написать логику определения количества сообщений внутри темы.
                    NumberOfPosts = topic.Posts.Count(),
                    AuthorId = topic.AppUserId,
                    Created = topic.Created,
                    AuthorName = _userRepository.GetUserById(topic.AppUserId).UserName,
                    // Получить последнее сообщение. Из него можно будет вытащить данные.
                    LatestPost =  _postRepository.GetLatestPost(topic.Id)
                };
                topicListing.Add(item);
            }
            
            // Формирую список 
            var model = new ForumDetailsModel
            {
                TopicList = topicListing.OrderBy(topic => topic.Title),
                NumberOfTopics = topicListing.Count(),
                ForumId = forum.Id,
                ForumTitle = forum.Title,
                ForumDescription = forum.Description
            };
            
            // Отправляю список в представление
            return View(model);
        }
        
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.ReturnUrl = Request.Headers["Referer"];
            var user = await _userManager.GetUserAsync(User);
            var model = new ForumCreateModel
            {
                AuthorId = user.Id
            };
            
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(ForumCreateModel model, string returnUrl)
        {
            var forum = new Forum
            {
                Title = model.Title,
                Description = model.Description,
                Created = DateTime.Now,
                AppUserId = model.AuthorId
            };
            await _forumRepository.CreateForum(forum);
            return Redirect(returnUrl);
        }
        

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            ViewBag.ReturnUrl = Request.Headers["Referer"];
            var forum = _forumRepository.GetForum(id);

            var model = new ForumEditModel
            {
                Id = forum.Id,
                Title = forum.Title,
                Description = forum.Description
            };
            
            return View(model);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(ForumEditModel model,string returnUrl)
        {
            // TODO Множество проверок
            
            // Когда поля будут заполнены и проверены, они вносятся в метод UpdateForum(), который и вносит изменения.
            await _forumRepository.UpdateForum(model.Id, model.Title, model.Description);
            // Чтобы перезагрузить страницу заново, мне потребовалось перенаправить на неё же! 
            return Redirect(returnUrl);
        }
        
        
        // TODO Delete forum.
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _forumRepository.DeleteForum(id);
            return RedirectToAction("Index", "Forum");
        }

        #region Helpers
        
        private static ForumListingModel BuildForumListing(Forum forum)
        {
            return new ForumListingModel
            {
                Id = forum.Id,
                Title = forum.Title,
                Description = forum.Description
            };
        }

        private static ForumListingModel BuildForumListing(Topic topic)
        {
            var forum = topic.Forum;
            return BuildForumListing(forum);
        }

        #endregion

        // Что делает метод BuildForumListing()? Он никак не взаимодействует с методом Index().
        // GET-версия метода вызывает POST-версию для формирования списка форумов.
        
    }
}