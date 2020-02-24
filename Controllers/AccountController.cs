using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TryHardForum.Models;
using TryHardForum.Services;
using TryHardForum.ViewModels.Account;
using TryHardForum.ViewModels.Email;

namespace TryHardForum.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        
        public AccountController(UserManager<AppUser> userManager,
                SignInManager<AppUser> signInManager,
                ILogger<AccountController> logger,
                IEmailSender emailSender,
                IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
        }
        
        // Вход в УЗ
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ViewBag.ReturnUrl = Request.Headers["Referer"];
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(model.Email);
                // Если пользователь найден, то он автоматически выходит.
                if (user != null)
                {
                    // Переменная, реализующая механизм входа пользователя.
                    var result = await _signInManager.PasswordSignInAsync(
                        user, 
                        model.Password, 
                        isPersistent: false,
                        lockoutOnFailure: false);
                    
                    // если result благополучно выполняется, значит пользователь входит
                    // Это действие отмечается в логах, и приложение перенаправляет пользователя.
                    if (result.Succeeded)
                    {
                        _logger.LogInformation(1, "User logged in.");
                        return Redirect(returnUrl);
                    }
                    
                    // В случае, если пользователь заблокирован
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning(2, "Учётная запись пользователя заблокирована.");
                        return View("Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Неудачная попытка входа.");
                        return View(model);
                    }
                }
            }
            return View(model);
        }

        // Выход из УЗ пользователя.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // Регистрация новых пользователей
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email.ToLower(),
                    MemberSince = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Генерация токена подтверждения электронной почты для пользователя.
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Генерация обратного URL при нажатии ссылки подтверждения почты.
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                        new { userId = user.Id, code = token }, protocol: Request.Scheme);

                    string content = "<p>Чтобы подтвердить свою УЗ пройди по ссылке далее: <a href=\"" 
                                     + confirmationLink + "\">link</a>";

                    // Создаю новый объект, который затем передаю на отправку.
                    var details = new SendEmailDetails
                    {
                        IsHtml = true,
                        FromName = _configuration["EmailSettings:EmailAuthor"],
                        FromEmail = _configuration["EmailSettings:EmailAddress"],
                        ToName = user.UserName,
                        ToEmail = user.Email,
                        Subject = "Подтверждение адреса электронной почты",
                        Content = content
                    };
                    
                    // Отправка письма пользователю со встроенной обратной ссылкой (URL)
                    await _emailSender.SendEmailAsync(details);

                    _logger.LogInformation(3, "User created a new account with password.");
                    // TODO: Поместить сообщение о необходимости подтверждения адреса электронной почты
                    
                    // После регистрации пользователя, происходит перенаправление на страницу Error.cshtml
                    TempData["Message"] = "Регистрация прошла успешно! " +
                                          "На указанный вами адрес электронной почты выслано письмо с подтверждением." +
                                          "Рекомендуем перед использованием учётной записи пройти процедуру подтверждения.";
                    return RedirectToAction("Index", "Home");
                }
                // 
                AddErrors(result);
            }
            return View(model);
        }

        // Подтверждение адреса электронной почты.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            // code генерируется при регистрации новой УЗ пользователя. Фактически это токен.
            if (userId == null || code == null)
            {
                return RedirectToPage("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("Error");
            }
            // Если подтверждение успешно, то пользователь перенаправляется на
            // страницу подтверждения.
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }


        // Восстановление забытого пароля.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                // Поиск пользователя по указанному почтовому адресу.
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Не отобразится, если пользователь не найден или его почта не подтверждена.
                    return View("ForgotPasswordConfirmation");
                }
                
                // Генерирует токен восстановления и обратный адрес.
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Кодировка токена перед помещением его в ссылку.
                token = HttpUtility.HtmlEncode(token);
                
                // Указывается метод, на который будет ссылка.
                // ранее был userId = user.Id. В этой модели отсутствует Id, так что решение должно быть верным.
                var passwordResetLink = 
                    Url.Action("ResetPassword", "Account", 
                        new { email = user.Email,  token }, protocol: Request.Scheme);
                
                var content = "<p>Чтобы сбросить пароль пройди по ссылке далее: <a href=\"" 
                              + passwordResetLink + "\">link</a>";

                // Создаю новый объект, который затем передаю на отправку.
                var details = new SendEmailDetails
                {
                    IsHtml = true,
                    FromName = _configuration["EmailSettings:EmailAuthor"],
                    FromEmail = _configuration["EmailSettings:EmailAddress"],
                    ToName = user.UserName,
                    ToEmail = user.Email,
                    Subject = "Восстановление пароля",
                    Content = content
                };
                
                // Осуществляет отправку письма с обратным адресом пользователю.
                await _emailSender.SendEmailAsync(details);
                return View("ForgotPasswordConfirmation");
            }
            return View(model);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // Сброс пароля
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (token == null || email == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // Декодировка токена (он был закодирован в ForgotPassword()).
            var token = HttpUtility.HtmlDecode(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, token: token, model.NewPassword);
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            if (user == null)
            {
                // По-хорошему тут должно быть перенаправление на Home.Error, или что-то в этом роде...
                return View("ResetPasswordConfirmation");
            }
            
            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }
            AddErrors(result);
            return View(model);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
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