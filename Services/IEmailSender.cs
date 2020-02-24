using System.Threading.Tasks;
using TryHardForum.ViewModels.Email;

namespace TryHardForum.Services
{
    // Интерфейс сервиса
    public interface IEmailSender
    {
        Task<SendEmailResponse> SendEmailAsync(SendEmailDetails details);
    }
}