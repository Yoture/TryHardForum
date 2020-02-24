using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TryHardForum.ViewModels.Email;

namespace TryHardForum.Services
{
    // Это простой сервис.
    public class EmailTemplateSender : IEmailTemplateSender
    {
        private readonly IEmailSender _emailSender;

        public EmailTemplateSender(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        
        // Если есть проблемы с этой реализацией, то лучше просто закомментить и нигде не указывать
        // этот класс. Сейчас мне не до шаблонов писем.
        public async Task<SendEmailResponse> SendGeneralEmailAsync(SendEmailDetails details, string title,
            string content1, string content2,
            string buttonText, string buttonUrl)
        {
            var templateText = default(string);
            // Читает основной темплейт из файла.
            // Указывается путь до темплейта (html файла).
            using (var reader = new StreamReader(Assembly.GetEntryAssembly().GetManifestResourceStream("TryHardForum.Services.Email.Templates.TemplateName.html"),
                Encoding.UTF8))
            {
                templateText = await reader.ReadToEndAsync();
            }
            // Заменить специальные значения следующими в темплейте.
            // Специальные значения должны располагаться в темплейте письма.
            templateText = templateText.Replace("--Title--", title)
                .Replace("--Content1--", content1)
                .Replace("--Content2--", content1)
                .Replace("--ButtonText--", content1)
                .Replace("--ButtonUrl--", content1);

            details.Content = templateText;
            // По-хорошему тут надо DI использовать.
            return await _emailSender.SendEmailAsync(details);
        }
    }
}