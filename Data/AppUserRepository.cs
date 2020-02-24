using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TryHardForum.Models;

namespace TryHardForum.Data
{
    public interface IAppUserRepository
    {
        AppUser GetUserById(string id);
        IEnumerable<AppUser> GetAllUsers();
        AppUser GetUserByName(string name);
        Task Deactivate(AppUser user);
        Task SetProfileImage(string id, Uri uri);
        Task UpdateUserRating(string id, Type type);
    }
    
    class AppUserRepository : IAppUserRepository
    {
        private readonly AppUserDbContext _userContext;

        public AppUserRepository(AppUserDbContext userContext)
        {
            _userContext = userContext;
        }

        
        // Деактивация пользователя.
        public async Task Deactivate(AppUser user)
        {
            user.IsActive = false;
            _userContext.Update(user);
            await _userContext.SaveChangesAsync();
        }
        
        // Взять всех пользователей.
        public IEnumerable<AppUser> GetAllUsers()
        {
            return _userContext.AppUsers;
        }

        // Получить пользователя по имени.
        public AppUser GetUserByName(string name)
        {
            return _userContext.AppUsers.FirstOrDefault(user => user.UserName == name);
        }
        
        // Получить пользователя по ID.
        public AppUser GetUserById(string id)
        {
            // Вытаскивает из общей кучи пользователей, пользователя с нужным ID.
            return _userContext.AppUsers.FirstOrDefault(user => user.Id == id);
        }

        // Обновление (увеличение) рейтинга пользователя
        public async Task UpdateUserRating(string id, Type type)
        {
            var user = GetUserById(id);
            var increment = GetIncrement(type);
            user.Rating += increment;
            await _userContext.SaveChangesAsync();
        }
        
        // Установление аватара пользователя. Пока что метод не работает.
        public async Task SetProfileImage(string id, Uri uri)
        {
            // Метод позволяет установить аватар профиля.
            var user = GetUserById(id);
            user.ProfileImageUrl = uri.AbsoluteUri;
            _userContext.Update(user);
            await _userContext.SaveChangesAsync();
        }
        
        // Механизм добавления рейтинга пользователя.
        private int GetIncrement(Type type)
        {
            var bump = 0;

            if (type == typeof(Post))
                bump = 1;

            if (type == typeof(PostReply))
                bump = 2;

            return bump;
        }
    }
}