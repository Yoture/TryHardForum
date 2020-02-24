using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TryHardForum.Models;
using TryHardForum.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using TryHardForum.Extensions;
using TryHardForum.Services;

namespace TryHardForum
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config) 
        {
            Configuration = config;
        }
      
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        
        public void ConfigureServices(IServiceCollection services)
        {
            // Строка подключения к БД с УЗ пользователей
            var identityConString = Configuration.GetConnectionString("IdentityConnection");

            // Строка подключения к БД с данными приложения (за исключением данных пользователей).
            var dataConString = Configuration.GetConnectionString("DataConnection");

            // Подключение к БД Identity через EFCore с использованием поставщика БД Npgsql.
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<AppUserDbContext>(options =>
                    options.UseNpgsql(identityConString));

            // Подключение БД с данными приложения (за исключением данных пользователей).
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<AppDataDbContext>(options =>
                    options.UseNpgsql(dataConString));

            // Установка проверки куки на валидность каждую секунду
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromSeconds(0);
            });

            // Подключение аутентификации (конфигурация подключения)
            // Передаётся значение CookieAuthenticationDefaults.AuthenticationScheme.
            // Далее с помощью метода AddCookie() настраивается аутентификация
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Если пользователь будет совершать попытки получить доступ к возможностям
                    // доступным только вошедшим пользователям - его программа перенаправит на "/Account/Login"
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                    // Указание длительности действия токена 30 минут. Что даёт первое свойство?
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });

            // Подключение identity с указанием места хранения данных. Последний метод я не помню что даёт. 
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghujklmnopqrstuvwxyz";
            }).AddEntityFrameworkStores<AppUserDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IForumRepository, ForumRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<ITopicRepository, TopicRepository>();
            services.AddScoped<IUploadService, UploadService>();
            services.AddSingleton(Configuration);
            
            // Как работает этот код?
            services.Configure<AuthMessageSenderOptions>(Configuration);
            
            // Внедрение SendGrid
            services.AddSendGridEmailSender();
            
            // Внедрение темплейтов SendGrid
            services.AddEmailTemplateSender();

            // Сид базы УЗ админа.
            services.AddTransient<DataSeeder>();

            // Используется вместо AddMvc().
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataSeeder dataSeeder)
        {
            app.UseStatusCodePages();
            app.UseDeveloperExceptionPage();

            dataSeeder.SeedSuperUser();
            
            app.UseStaticFiles();
            
            // Маршрутизация на основе конечных точек (Endpoints)
            // тут не требуется указывать метод UseMvc(), если используется эта система маршрутизации.
            app.UseRouting();

            // Методы UseAuthentication и UseAuthorization должны находиться ПОСЛЕ эндпоинтов для корректной работы
            // Задаёт вопрос пользователю "Кто ты?" (не буквально, но принцип работы соответствует этому вопросу)
            // Встраивает в конвейер мидлварь, которая управляет аутентификацией
            app.UseAuthentication();

            // Авторизация отвечает на вопрос, какие права в системе имеет пользователь
            // (если короче, то "are you allowed?")
            app.UseAuthorization();
            
            
            app.UseEndpoints(endpoints =>
            {
                // Стандартный путь.
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" }
                );
                
                // Маршрутизация по областям.
                // endpoints.MapAreaControllerRoute(
                //     name: "areas",
                //     areaName: "areas",
                //     pattern: "{area}/{controller}/{action}/{id?}",
                //     defaults: new { controller = "Home", action = "Index" }
                // );
            });
        }
    }
}
