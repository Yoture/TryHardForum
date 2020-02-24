using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TryHardForum.Models;

namespace TryHardForum.Data
{
    public interface IPostRepository
    {
        Task CreatePost(Post post);
        Task DeletePost(int id);
        Task ArchivePost(int id);
        Task EditPostContent(int id, string newContent);

        int GetReplyCount(int id);
        
        Post GetPost(int id);
        IEnumerable<Post> GetAllPosts();
        IEnumerable<Post> GetPostsByUserId(string id);
        IEnumerable<Post> GetPostsByTopicId(int id);
        IEnumerable<Post> GetPostsBetween(DateTime start, DateTime end);
        IEnumerable<Post> GetFilteredPosts(string searchQuery);
        IEnumerable<AppUser> GetAllUsers(IEnumerable<Post> posts);
        IEnumerable<Post> GetLatestPosts(int count);
        Post GetLatestPost(int topicId);
    }
    
    public class PostRepository : IPostRepository
    {
        private readonly AppDataDbContext _dataContext;

        public PostRepository(AppDataDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        // Создать сообщение.
        public async Task CreatePost(Post post)
        {
            _dataContext.Posts.Add(post);
            await _dataContext.SaveChangesAsync();
        }

        // Отправить выбранное сообщение в архив.
        // Нужен ли этот механизм?
        public async Task ArchivePost(int id)
        {
            var post = GetPost(id);
            post.IsArchived = true;
            _dataContext.Update(post);
            await _dataContext.SaveChangesAsync();
        }

        // Удалить сообщение.
        public async Task DeletePost(int id)
        {
            var post = GetPost(id);
            _dataContext.Remove(post);
            await _dataContext.SaveChangesAsync();
        }

        // Реактировать содержимое сообщения
        public async Task EditPostContent(int id, string content)
        {
            var post = GetPost(id);
            post.Content = content;
            _dataContext.Update(post);
            await _dataContext.SaveChangesAsync();
        }

        // Получить все сообщения.
        // В каком контексте используется этот метод?
        public IEnumerable<Post> GetAllPosts()
        {
            return _dataContext.Posts
                .Include(post => post.AppUser)
                .Include(post => post.Replies).ThenInclude(reply => reply.AppUser)
                .Include(post => post.Topic);
            // Потребуется также указать Форум?
        }
        
        public IEnumerable<Post> GetFilteredPosts(string searchQuery)
        {
            var query = searchQuery.ToLower();
            return _dataContext.Posts
                // .Include(post => post.Forum)
                .Include(post => post.Topic)
                .Include(post => post.AppUser)
                .Include(post => post.Replies)
                .Where(post => post.Content.ToLower().Contains(query));
        }

        // Получить всех пользователей сообщений (и ответов?).
        // Метод должен работать в топике (т.е. там, где располагаются сообщения пользователей)
        public IEnumerable<AppUser> GetAllUsers(IEnumerable<Post> posts)
        {
            // Создаётся список пользователей.
            var users = new List<AppUser>();
            // каждое сообщение в списке полученных методом сообщений...
            foreach(var post in posts)
            {
                // Добавляется пользователь сообщения в общий список пользователей.
                users.Add(post.AppUser);
                // Если сообщение не содержит ответов, то продолжить.
                // По идее логичней предположить, что если  ЕСТЬ ответы к сообщению,
                // то приплюсовать к основному
                // списку пользователей имена ответчиков.
                if (!post.Replies.Any()) continue;
                users.AddRange(post.Replies.Select(reply => reply.AppUser));
            }
            // Воврат списка пользователей по убыванию.
            return users.Distinct();
        }
        
        // Получить сообщение.
        public Post GetPost(int id)
        {
            // Выборка сообщений где совпадает сообщение...
            // Так понимаю, что с сообщением тянутся данные автора, ответов и их авторов, а также топика,
            // Внутри которого находится это сообщение?
            return _dataContext.Posts.Where(post => post.Id == id)
                .Include(post => post.AppUser)
                .Include(post => post.Replies).ThenInclude(reply => reply.AppUser)
                .Include(post => post.Topic)
                .FirstOrDefault();
        }

        // Извлекает последние сообщения, принадлежащие определённой теме
        public Post GetLatestPost(int topicId)
        {
            var allPosts = GetAllPosts()
                .Where(post => post.TopicId == topicId)
                .OrderByDescending(post => post.Created);
            // Из массы сортированных сообщений отбирается определённое их число (count).
            return allPosts.First();
        }
        
        // Извлекает все последние сообщения.
        public IEnumerable<Post> GetLatestPosts(int count)
        {
            // Выводятся все сообщение в порядке их создания по убыванию.
            var allPosts = GetAllPosts()
                .OrderByDescending(post => post.Created);
            return allPosts.Take(count);
        }

        // Выбор сообщений за определённый период времени.
        public IEnumerable<Post> GetPostsBetween(DateTime start, DateTime end)
        {
            return _dataContext.Posts.Where(post => post.Created >= start && post.Created <= end);
        }

        // Получить сообщения по ID темы.
        public IEnumerable<Post> GetPostsByTopicId(int id)
        {
            // Выбирается тема по её ID, а затем выводятся все сообщения этой темы (топика).
            return _dataContext.Topics.First(forum => forum.Id == id).Posts;
        }

        // Получить сообщения по ID пользователя.
        public IEnumerable<Post> GetPostsByUserId(string id)
        {
            // Отбор всех сообщений, оставленных определённым пользователем.
            return _dataContext.Posts.Where(post => post.AppUserId == id);
        }

        // Получить число ответов на определённое сообщение.
        public int GetReplyCount(int id)
        {
            // Выбор сообщения и подсчитывается количество ответов к нему.
            return GetPost(id).Replies.Count();
        }
    }
}