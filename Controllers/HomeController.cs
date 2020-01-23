using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TryHardForum.Models;
using TryHardForum.Data;

namespace TryHardForum.Controllers
{
    public class HomeController : Controller
    {
        private AppUserDbContext _userContext;

        private UserManager<AppUser> _userManager;

        // Через объект UserManager<AppUser> осуществляется доступ к пользовательским данным
        public HomeController(AppUserDbContext usrCtx, UserManager<AppUser> usrMgr)
        {
            _userContext = usrCtx;
            _userManager = usrMgr;
        }

        public ViewResult Index() => View();

        public ViewResult About() => View();

        public ViewResult Contact() => View();
    }
}