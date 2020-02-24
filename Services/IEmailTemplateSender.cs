using System.Threading.Tasks;
using TryHardForum.ViewModels.Email;

namespace TryHardForum.Services
{
    // Интерфейс сервиса.
    public interface IEmailTemplateSender
    {
        // позволяет оборачивать письма в определённый макет.
        // Метод SendGeneralEmailAsync() является обёрткой для метода SendEmailAsync().
        // Позволяет не просто отправить данные, но и применить их к определённому шаблону письма.
        Task<SendEmailResponse> SendGeneralEmailAsync(SendEmailDetails details, string title,
            string content1, string content2,
            string buttonText, string buttonUrl);
    }
}