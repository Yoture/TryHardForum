using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TryHardForum.Models;

namespace TryHardForum.Data
{
    public interface IForumRepository
    {
        Task CreateForum(Forum forum);
        Task DeleteForum(int id);
        Task UpdateForum(int Id, string title, string description);
        Forum GetForum(int id);
        IEnumerable<Forum> GetAllForums();
        // IEnumerable<AppUser> GetActiveUsersInForum(int forumId);
    }
    
    public class ForumRepository : IForumRepository
    {
        private readonly AppDataDbContext _dataContext;

        public ForumRepository(AppDataDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        // Создание нового форума.
        public async Task CreateForum(Forum forum)
        {
            // Сначала даю понять EFCore, что БД будет выбирать ID новому объекту.
            forum.Id = 0;
            // Указываю в какой DbSet<T> следует добавить новый объект.
            _dataContext.Forums.Add(forum);
            // Сохранить внесённые изменения.
            await _dataContext.SaveChangesAsync();
        }
        
        // Обновление описания форума.
        public async Task UpdateForum(int id, string title, string description)
        {
            var forum = GetForum(id);
            forum.Title = title;
            forum.Description = description;

            _dataContext.Update(forum);
            await _dataContext.SaveChangesAsync();
        }


        // Удаление форума.
        public async Task DeleteForum(int id)
        {
            var forum = GetForum(id);
            _dataContext.Forums.Remove(forum);
            await _dataContext.SaveChangesAsync();
        }

        // Получить форум по его ID.
        public Forum GetForum(int id)
        {
            var forum = _dataContext.Forums
                .FirstOrDefault(f => f.Id == id);
            return forum;
        }
        
        // Получить список всех форумов.
        public IEnumerable<Forum> GetAllForums()
        {
            return _dataContext.Forums;
        }
    }
}