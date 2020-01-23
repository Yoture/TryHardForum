using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TryHardForum.Models;
using TryHardForum.Models.ViewModels;
using TryHardForum.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TryHardForum.Controllers
{
    public class AccountController : Controller
    {
        // Через этот объект можно запрашивать данные пользователей для дальнейшей работы с ними
        private UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        // Валидация пользователя?
        private readonly IUserValidator<AppUser> _userValidator;
        // Валидация пароля?
        private readonly IPasswordValidator<AppUser> _passwordValidator;
        // Переменная для хеширования пароля?
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        
        // Осуществляется взаимодействие с хранилищем через интерфейс
        // Хранилище же взаимодействует с классом контекста
        // Однако у меня пока что репозиторий не используется, т.е. новые пользователи в него не вносятся
        private AppUserDbContext _userContext;

        public AccountController(UserManager<AppUser> userManager,
                IUserValidator<AppUser> userValid,
                IPasswordValidator<AppUser> passValid,
                IPasswordHasher<AppUser> passwordHash,
                AppUserDbContext userContext,
                SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _userValidator = userValid;
            _passwordValidator = passValid;
            _passwordHasher = passwordHash;
            _userContext = userContext;
            _signInManager = signInManager;
        }
        
        // переписать
        // public ViewResult Index() => View(_userManager.Users);

        [HttpGet]
        public ViewResult Index() => View();


        [HttpGet]
        public ViewResult Register() => View();
    
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid) 
            {
                // Создание объекта AppUser с внесением данных в поля UserName и Email
                AppUser user = new AppUser { UserName = model.UserName, Email = model.Email };
                
                // Иртересуют детали работы метода CreateAsync
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // В случае успеха осуществляется вход в эту УЗ без создания куки.
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // В случае неудачи будут выведены все ошибки, которые были допущены.
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }    
                }
            }
            return View();
        }

        // Редактирует имя пользователя (за исключением пароля)
        [HttpGet]
        public async Task<IActionResult> Edit(string id) 
        {
            // Отображение почты пользователя через ViewBag мне кажется сейчас простым костылём.
            AppUser user = await _userManager.FindByIdAsync(id);

            if (user == null) 
            {
                return NotFound();
            }

            EditViewModel model = new EditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByIdAsync(model.Id);
        
                if (user != null)
                {
                    user.UserName = model.UserName;
            
                    // Какой тип данных?
                    IdentityResult result = await _userManager.UpdateAsync(user);
            
                    if (result.Succeeded)
                    {
                        // Перенаправление на страницу УЗ?
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

        // GET-метод
        public async Task<IActionResult> ChangePassword(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            ChangePasswordViewModel model = new ChangePasswordViewModel { Id = user.Id, Email = user.Email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.GetUserAsync(User);
        
                if (user == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                IdentityResult result = 
                    await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                
                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await _signInManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmation");
            }
            return View(model);
        }

        // Удаление УЗ
        [HttpPost]
        public async Task<IActionResult> Delete(string id) 
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null) 
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded) 
                {
                    return RedirectToAction("Index", "Home");
                } 
                else 
                {
                    AddErrorsFromResult(result);
                }
            } 
            else 
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View(nameof(Index));
        }

        // Вспомогательный метод для метода удаления УЗ
        private void AddErrorsFromResult(IdentityResult result) 
        {
            foreach (IdentityError error in result.Errors) 
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        // Вход
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null) 
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> Login(LoginViewModel model, string returnUrl) 
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await _signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result =
                        await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                    if (result.Succeeded)
                    {
                        return Redirect(returnUrl ?? "/");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(LoginViewModel.Email),
                        "Неверный пользователь или пароль");
                }
            }
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Удаление аутентификационных куки.
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}