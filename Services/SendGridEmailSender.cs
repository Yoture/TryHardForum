using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using TryHardForum.ViewModels.Email;

namespace TryHardForum.Services
{
    /*
     * Отправляет почту используя SendGrid WebAPI C#
     */
    public class SendGridEmailSender : IEmailSender
    {
        public IConfiguration Configuration { get; }

        public SendGridEmailSender(IConfiguration config)
        {
            Configuration = config;
        }

        // Получает данные из модели SendEmailDetails.
        public async Task<SendEmailResponse> SendEmailAsync(SendEmailDetails details)
        {
            // Получаю ключ SendGrid из appsettings.json
            var apiKey = Configuration["SendGridKey"];
            
            // Создаю нового клиента SendGrid.
            var client = new SendGridClient(apiKey);
            
            // From От кого (почта и имя)
            var from = new EmailAddress(details.FromEmail, details.FromName);
            
            // To Кому (почта и имя)
            var to = new EmailAddress(details.ToEmail, details.ToName);
            
            // Subject (тема сообщения)
            var subject = details.Subject;
            
            // Содержимое сообщения. Включает разметку HTML.
            var content = details.Content;
            
            // Метод CrtateSingleEmail() принимает два вида содержимого: обычный текст (четвёртый аргумент метода)
            // и с HTML разметкой (пятый аргумент). Чтобы обойти третий аргумент леплю тернарный оператор.
            var message = MailHelper.CreateSingleEmail(
                from, 
                to, 
                subject, 
                details.IsHtml ? null : content, 
                details.IsHtml ? content : null);

            // Можно создать темплейт, ID которого указать в коде.
            // message.TemplateId = "";
            // также можно указать конкретные поля в письме, воторые можно заполнить определённым текстом.
            // message.AddSubstitution("", "");
            
            // Отправка письма...
            var response = await client.SendEmailAsync(message);
            
            // Если успешно
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                // Возвращает успешный ответ.
                return new SendEmailResponse();
            }
            
            // Иначе...
            try
            {
                // Получить result в теле.
                var bodyResult = await response.Body.ReadAsStringAsync();

                // Десериализация ответа
                var sendGridResponse = JsonConvert.DeserializeObject<SendGridResponse>(bodyResult);

                // Добавление любых ошибок к ответу.
                var errorResponse = new SendEmailResponse
                {
                    Errors = sendGridResponse?.Errors.Select(f => f.Message).ToList()
                };

                // Удостоверяюсь, что имею хотя бы одну ошибку
                if (errorResponse.Errors == null || errorResponse.Errors.Count == 0)
                {
                    // Добавление неизвестной ошибки.
                    errorResponse.Errors = new List<string>(new [] { "Unknown error from email sending service" });
                }

                // ВОзврат ответа
                return errorResponse;
            }
            catch (Exception ex)
            {
                // Сбрасываю (break) если начинается дебаг
                if (Debugger.IsAttached)
                {
                    var error = ex;
                    Debugger.Break();
                }

                return new SendEmailResponse
                {
                    Errors =  new List<string>(new[] { "Произошла неизвестная ошибка." })
                };
            }
        }
    }
}
