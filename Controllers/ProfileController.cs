using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TryHardForum.Data;
using TryHardForum.Models;
using TryHardForum.Services;
using TryHardForum.ViewModels.Profile;

namespace TryHardForum.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAppUserRepository _userRepository;
        private readonly IUploadService _uploadService;
        private readonly IConfiguration  _configuration;

        public ProfileController(UserManager<AppUser> userManager,
            IAppUserRepository userRepository,
            IUploadService uploadService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _uploadService = uploadService;
            _configuration = configuration;
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            //var user = await _userManager.GetUserAsync(User);
            var user = await _userManager.FindByIdAsync(id);
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new ProfileModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserRating = user.Rating.ToString(),
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                MemberSince = user.MemberSince,
                IsActive = user.IsActive,
                IsAdmin = userRoles.Contains("Admin")
            };

            return View(model);
        }

        public IActionResult Index()
        {
            var profiles = _userRepository.GetAllUsers()
                .OrderByDescending(user => user.Rating)
                .Select(u => new ProfileModel
                {
                    Email = u.Email,
                    UserName = u.UserName,
                    ProfileImageUrl = u.ProfileImageUrl,
                    UserRating = u.Rating.ToString(),
                    MemberSince = u.MemberSince,
                    IsActive = u.IsActive
                });

            var model = new ProfileListModel
            {
                Profiles = profiles
            };
            return View(model);
        }

        public IActionResult Deactivate(string userId)
        {
            var user = _userRepository.GetUserById(userId);
            _userRepository.Deactivate(user);
            return RedirectToAction("Index", "Profile");
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            // Переписать логику выполнения метода.
            var userId = _userManager.GetUserId(User);

            // Соединение с Azure Storage Account Container.
            // В дальнейшем переделать на Amazon Web Services.
            var connectionString = _configuration.GetConnectionString("AzureStorageAccount");
            
            // Get Blob Container
            var container = _uploadService.GetBlobContainer(connectionString);

            // Parse the Content Disposition response header
            var parsedContentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
            // Grab the filename
            var filename = Path.Combine(parsedContentDisposition.FileName.Trim('"'));
            // Get a reference to a Block Blob
            var blockBlob = container.GetBlockBlobReference(filename);
            // On that block blob, Upload our file <-- file uploaded to the cloud
            await blockBlob.UploadFromStreamAsync(file.OpenReadStream());
            // Set the User's profile image to the URI
            await _userRepository.SetProfileImage(userId, blockBlob.Uri);
            // Redirect to the user's profile page.
            return RedirectToAction("Details", "Profile", new { id = userId });
        }
    }
}