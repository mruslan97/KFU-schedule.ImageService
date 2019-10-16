using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schedule.ImageService.Models;
using Schedule.ImageService.Services;
using Schedule.ImageService.Services.Impl;
using Serilog;

namespace Schedule.ImageService
{
    public class Startup
    {
        /// <summary>
        ///     Путь к файлу конфига
        /// </summary>
        private readonly string ConfigPath = "appsettings.json";
        
        /// <summary>
        ///     Путь к файлу настроек логгера
        /// </summary>
        private readonly string LogConfigPath = "logsettings.json";

        private readonly string CorsPolicyName = "CorsPolicy";

        /// <summary>
        ///     Информация об окружении
        /// </summary>
        private IHostingEnvironment Env { get; }

        /// <summary>
        ///     Конфигурация приложения
        /// </summary>
        private IConfiguration Configuration { get; }

        /// <summary>
        ///     Инициализирует новый объект <see cref = "Startup" /> class.
        ///     Конструктор класса
        /// </summary>
        /// <param name = "env" > Информация об окружении </param>
        public Startup(IHostingEnvironment env)
        {
            Env = env;
            var envConfigPath = Path.ChangeExtension(ConfigPath,
                $".{Env.EnvironmentName}{Path.GetExtension(ConfigPath)}");
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(ConfigPath, false, true)
                .AddJsonFile(envConfigPath, true, true)
                .AddJsonFile(envConfigPath, true, true)
                .AddJsonFile(LogConfigPath, true, true)
                .AddEnvironmentVariables();
            
            Configuration = builder.Build();
        }
        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.Configure<StorageOptions>(Configuration.GetSection(nameof(StorageOptions)));
            services.AddTransient<IHtmlParser, HtmlParser>();
            services.AddTransient<IConverterService, ConverterService>();
            services.AddHangfire(x => x.UseMemoryStorage());
            services.AddHttpClient();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(new LoggerConfiguration()
                    .ReadFrom.Configuration(Configuration).CreateLogger());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            RecurringJob.AddOrUpdate<IConverterService>(u => 
                u.UpdateStorage(), Cron.Daily(01, 00), TimeZoneInfo.Local);
        }
    }
}