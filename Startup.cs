using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TryHardForum.Models;
using TryHardForum.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TryHardForum
{
    public class Startup
    {
        public Startup(IConfiguration config) 
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        
        public void ConfigureServices(IServiceCollection services)
        {
            // Строка подключения к БД с УЗ пользователей
            string identityConString = Configuration.GetConnectionString("IdentityConnection");
            // Подключение к БД Identity через EFCore с использованием поставщика БД Npgsql.
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<AppUserDbContext>(options =>
                    options.UseNpgsql(identityConString));

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

            // Регистрация хранилища
            // services.AddTransient<IAppUserRepository, AppUserRepository>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStatusCodePages();
            app.UseDeveloperExceptionPage();
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
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );
            });

            
        }
    }
}
