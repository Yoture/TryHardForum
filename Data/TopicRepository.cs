using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TryHardForum.Models;
using TryHardForum.ViewModels.Post;

namespace TryHardForum.Data
{
    public interface ITopicRepository
    {
        Task CreateTopic(Topic topic);
        Task DeleteTopic(int id);
        Task UpdateTopic(int id, string title);
        Topic GetTopic(int id);
        Topic GetTopicByTitle(string title);
        IEnumerable<Topic> GetAllTopics();
        IEnumerable<AppUser> GetActiveUsers(int topicId);
        Post GetLatestPost(int id);
        bool HasRecentPost(int id);
        IEnumerable<Post> GetFilteredPosts(string searchQuery);
        IEnumerable<Post> GetFilteredPosts(int topicId, string searchQuery);
        // string GetForumImageUrl(int id);
    }
    
    public class TopicRepository : ITopicRepository
    {
        private readonly AppDataDbContext _dataContext;
        private readonly IPostRepository _postRepository;

        public TopicRepository(AppDataDbContext dataContext,
            IPostRepository postRepository)
        {
            _dataContext = dataContext;
            _postRepository = postRepository;
        }
        
        // Создание топика
        public async Task CreateTopic(Topic topic)
        {
            // Сначала даю понять EFCore, что БД будет выбирать ID новому объекту.
            topic.Id = 0;
            // Указываю в какой DbSet<T> следует добавить новый объект.
            _dataContext.Topics.Add(topic);
            // Сохранить внесённые изменения.
            await _dataContext.SaveChangesAsync();
        }

        // Удаление топика
        public async Task DeleteTopic(int id)
        {
            // Механизм удаления существующего форума.
            var topic = GetTopic(id);
            _dataContext.Remove(topic);
            await _dataContext.SaveChangesAsync();
        }
        
        // Получить тему по ID.
        public Topic GetTopic(int id)
        {
            var topic = _dataContext.Topics
                .FirstOrDefault(t => t.Id == id);
            return topic;
        }
        
        public Topic GetTopicByTitle(string title)
        {
            var topic = _dataContext.Topics
                .FirstOrDefault(t => t.Title == title);
            return topic;
        }
        
        // Вывести все темы.
        public IEnumerable<Topic> GetAllTopics()
        {
            return _dataContext.Topics.Include(topic => topic.Posts);
        }

        // Получить список активных пользователей внутри топика.
        public IEnumerable<AppUser> GetActiveUsers(int topicId)
        {
            var posts = GetTopic(topicId).Posts;

            if (posts == null || !posts.Any())
            {
                return new List<AppUser>();
            }
            return _postRepository.GetAllUsers(posts);
        }
        
        // Получить самое последнее сообщение.
        public Post GetLatestPost(int id)
        {
            var posts = GetTopic(id).Posts;
            if (posts != null)
            {
                return GetTopic(id).Posts
                    .OrderByDescending(post => post.Created)
                    .FirstOrDefault();
            }
            return  new Post();
        }

        // Является ли сообщение недавним?
        public bool HasRecentPost(int id)
        {
            const int hoursAgo = 12;
            // Определение окна, в рамках которого пост считается недавним.
            var window = DateTime.Now.AddHours(-hoursAgo);
            // Возвращение true или false в зависимости от наличия/отсутствия недавних постов.
            return GetTopic(id).Posts.Any(post => post.Created >= window);
        }
        
        public IEnumerable<Post> GetFilteredPosts(string searchQuery)
        {
            // Отфильтровать посты.
            return _postRepository.GetFilteredPosts(searchQuery);
        }

        // Получить определённые сообщения.
        public IEnumerable<Post> GetFilteredPosts(int topicId, string searchQuery)
        {
            if (topicId  == 0) return _postRepository.GetFilteredPosts(searchQuery);
            var topic = GetTopic(topicId);

            return string.IsNullOrEmpty(searchQuery)
                ? topic.Posts
                : topic.Posts.Where(post => post.Content.Contains(searchQuery));
        }
        
        // Обновить заголовок темы.
        public async Task UpdateTopic(int id, string title)
        {
            var topic = GetTopic(id);
            topic.Title = title;

            _dataContext.Update(topic);
            await _dataContext.SaveChangesAsync();
        }
    }
}