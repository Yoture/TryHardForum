using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TryHardForum.Models
{
    public class AppUser : IdentityUser
    {
        public override string Id { get; set; }
        [PersonalData]
        public override string UserName { get; set; }
        public override string Email { get; set; }
        public string Password { get; set; }

        // Для простого проекта хватит и обычного наследования.
        // Тут есть почта, id, подтверждение почты, имя пользователя, хеш пароля и многое другое.
        // Больше мне пока что ничего не нужно.
    }
}