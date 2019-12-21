using System;
using Microsoft.AspNetCore.Mvc;

namespace TryHardForum.Controllers
{
    public class HomeController : Controller
    {
        public ViewResult Index() => View();
    }
}