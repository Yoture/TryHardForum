using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TryHardForum.Models;

namespace TryHardForum.Data
{
    public interface IPostReplyRepository
    {
        Task CreateReply(PostReply reply);
        PostReply GetPostReply(int id);
        Task EditReplyContent(int id, string message);
        Task DeleteReply(int id);
    }
    
    public class PostReplyRepository : IPostReplyRepository
    {
        private readonly AppDataDbContext _dataContext;

        public PostReplyRepository(AppDataDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        // Создать ответ на сообщение
        public async Task CreateReply(PostReply reply)
        {
            // По идее нужно найти сообщение и потом уже поместить новый ответ к нему в список таких же ответов.
            // Добавить ответ.
            reply.Id = 0;
            _dataContext.PostReplies.Add(reply);
            await _dataContext.SaveChangesAsync();
        }
        
        // Получить ответ на сообщение (один).
        public PostReply GetPostReply(int id)
        {
            // Выполняется два запроса. В первом получает название топика сообщения, а во втором - пользователя сообщения?
            return _dataContext.PostReplies
                .Include(r=>r.Post)
                .ThenInclude(post=>post.Topic)
                .Include(r=>r.Post)
                .ThenInclude(post => post.AppUser).First(r => r.Id == id);
        }

        // Удалить ответ
        public async Task DeleteReply(int id)
        {
            var reply = GetPostReply(id);
            _dataContext.Remove(reply);
            await _dataContext.SaveChangesAsync();
        }

        // Редактировать содержимое ответа.
        public async Task EditReplyContent(int id, string message)
        {
            var reply = GetPostReply(id);
            reply.Content = message;
            
            _dataContext.Update(reply);
            await _dataContext.SaveChangesAsync();
        }
    }
}