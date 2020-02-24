using Microsoft.Extensions.DependencyInjection;
using TryHardForum.Services;

namespace TryHardForum.Extensions
{
    public static class EmailTemplateSenderExtensions
    {
        // Расширение и должно находиться в папке Extensions.
        public static IServiceCollection AddEmailTemplateSender(this IServiceCollection services)
        {
            services.AddTransient<IEmailTemplateSender, EmailTemplateSender>();
            return services;
        }
    }
}