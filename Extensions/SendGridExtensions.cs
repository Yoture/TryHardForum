using Microsoft.Extensions.DependencyInjection;
using TryHardForum.Services;

namespace TryHardForum.Extensions
{
    // Расширяющие методы для любых SendGrid классов
    public static class SendGridExtensions
    {
        public static IServiceCollection AddSendGridEmailSender(this IServiceCollection services)
        {
            // Внедрение SendGridEmailSender
            services.AddTransient<IEmailSender, SendGridEmailSender>();
            
            // Возвращение коллекции для создания цепи (chaining)
            return services;
        }
    }
}