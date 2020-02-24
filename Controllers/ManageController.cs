using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TryHardForum.Models;
using TryHardForum.Services;
using TryHardForum.ViewModels.Email;
using TryHardForum.ViewModels.Manage;

namespace TryHardForum.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public ManageController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = loggerFactory.CreateLogger<ManageController>();
        }
        
        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Страница подгрузки данных пользователя.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ManageIndexModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = StatusMessage
            };
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ManageIndexModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }
            StatusMessage = "Ваш профиль был обновлён.";
            return RedirectToAction(nameof(Index));
        }

        // Подтверждение УЗ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(ManageIndexModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException(
                    $"Невозможно загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            // Механизм, аналогичный при регистрации нового пользователя.
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Генерация обратной ссылки
            var callbackUrl = Url.Action(nameof(AccountController.ConfirmEmail), 
                "Account", new { userId = user.Id, code }, protocol: HttpContext.Request.Scheme);
            
            string content = "<p>Подтвердите адрес электронной почты пройдя по этой ссылке: <a href=\"" 
                             + callbackUrl + "\">link</a>";

            // Создаю новый объект, который затем передаю на отправку.
            var details = new SendEmailDetails
            {
                IsHtml = true,
                FromName = "TryHardForum Admin",
                FromEmail = "admin@tryhard.com",
                ToName = user.UserName,
                ToEmail = user.Email,
                Subject = "Подтверждение адреса электронной почты",
                Content = content
            };
                
            // Осуществляет отправку письма с обратным адресом пользователю.
            await _emailSender.SendEmailAsync(details);

            // Выпилить его?
            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToAction(nameof(Index));
        }
        
        // Изменение пароля пользователя
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Невозможно загрузить пользователя с ID " +
                                               $"'{_userManager.GetUserId(User)}'.");
            }

            var model = new ChangePasswordModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                throw new ApplicationException($"Невозможно загрузить пользователя с ID " +
                                               $"'{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult =
                await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("Пользователь успешно изменил свой пароль.");
            StatusMessage = "Ваш пароль был успешно изменён.";

            return RedirectToAction("Index", "Manage");
        }

        // Удаление пользователя 
        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            // Пока что пускай этот код будет тут...
            /*
            var user = await _userManager.GetUserAsync(User);

            var model = new DeleteUserModel
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email
            };
            return View(model);
            */
            return View();
        }
        
        // Метод, работающий фактически для того, чтобы удалить УЗ
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmation()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user != null)
            {
                // Удаление УЗ найденного пользователя
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    // Если удаление успешно, то выход из УЗ пользователя и перенаправление на главную страницу.
                    await _signInManager.SignOutAsync();
                    // TODO прописать уведомление в логах о совершённой операции.
                    ViewBag.Message = "Учётная запись пользователя успешно удалена.";
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            // Если всё совсем плохо, но этот вариант
            return RedirectToAction("Index", "Home");
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        
        #endregion
    }
}