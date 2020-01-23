using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TryHardForum.Models;

namespace TryHardForum.Models.ViewModels
{
    public class LoginViewModel
    {
        // Нужно ли это свойство?
        public string Id { get; set; }
        [Required(ErrorMessage = "Не указан адрес электронной почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }

        // За что отвечает это свойство?
        public string ReturnUrl { get; set; }
    }
}