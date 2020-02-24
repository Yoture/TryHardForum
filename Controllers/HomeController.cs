using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TryHardForum.Services;
using System.Linq;
using TryHardForum.Data;
using TryHardForum.ViewModels.Home;
using TryHardForum.ViewModels.Post;

namespace TryHardForum.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostRepository _postRepository;

        // Через объект UserManager<AppUser> осуществляется доступ к пользовательским данным
        public HomeController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public IActionResult Index()
        {
            var model = BuildHomeIndexModel();
            return View(model);
        } 

        public ViewResult About()
        {
            ViewData["Message"] = "Страница с описанием твоего приложения.";
            return View();
        }

        public ViewResult Contact()
        {
            ViewData["Message"] = "Твоя страница с контактами.";
            return View();
        }

        private HomeIndexModel BuildHomeIndexModel()
        {
            var latestPosts = _postRepository.GetLatestPosts(10);
            var posts = latestPosts.Select(post => new PostListingModel
            {
                Id = post.Id,
                AuthorName = post.AppUser.UserName,
                AuthorId = post.AppUserId,
                AuthorRating = post.AppUser.Rating,
                DatePosted = post.Created.ToString(CultureInfo.InvariantCulture),
                RepliesCount = _postRepository.GetReplyCount(post.Id),
                TopicTitle = post.Topic.Title,
                TopicId = post.Topic.Id
            });
            // Придумать механизм, в котором будет отображаться рядом с сообщениями форумы, к которым принадлежат
            // топики этих сообщений.

            return new HomeIndexModel
            {
                LatestPosts = posts
            };
        }

        public IActionResult Error()
        {
            return RedirectToPage("Error");
        }
    }
}