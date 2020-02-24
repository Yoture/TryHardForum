using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TryHardForum.Models;

namespace TryHardForum.Data
{
    public class DataSeeder
    {
        private AppUserDbContext _userContext;

        public DataSeeder(AppUserDbContext userContext)
        {
            _userContext = userContext;
        }

        public Task SeedSuperUser()
        {
            // Создаю хранилища, которые буду использовать дальше, при
            // присвоении админки новому пользователю ForumAdmin.
            var roleStore = new RoleStore<IdentityRole>(_userContext);
            var userStore = new UserStore<AppUser>(_userContext);

            var user = new AppUser
            {
                UserName = "ForumAdmin",
                NormalizedUserName = "FORUMADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                IsAdmin = true,
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var hasher = new PasswordHasher<AppUser>();
            var hashedPassword = hasher.HashPassword(user, "Admin");

            /* 
            Этот кусок кода появился тут из за того, что переменная 
             hashedPassword не может быть объявлена раньше, чем объявлена переменная user.
             В силу их закольцованности, требуется указать так, как есть в проекте
            но можно поискать и другие варианты решения...
            */ 
            user.PasswordHash = hashedPassword;

            // Переменная, позволяющая найти Админскую роль.
            var hasAdminRole = _userContext.Roles.Any(roles => roles.Name == "Admin");
            
            // В случае, если роль Администратора ещё не создана, выполняется это ветвление.
            if(!hasAdminRole)
            {
                roleStore.CreateAsync(new IdentityRole
                {
                    Name="Admin",
                    NormalizedName="admin"
                });
            }

            // Лучше следует присмотреться к этой переменной
            var hasSuperUser = 
                _userContext.Users.Any(u => u.NormalizedUserName == u.UserName);
            
            // Создание суперпользователя.
            if(!hasSuperUser)
            {
                userStore.CreateAsync(user);
                userStore.AddToRoleAsync(user, "Admin");
            }

            // Сохраняю все изменения в БД.
            _userContext.SaveChangesAsync();

            return Task.CompletedTask;
        }
    }
}