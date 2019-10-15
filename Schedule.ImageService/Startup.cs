using System;
using System.Collections.Generic;
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

namespace Schedule.ImageService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.Configure<StorageOptions>(Configuration.GetSection(nameof(StorageOptions)));
            services.AddTransient<IHtmlParser, HtmlParser>();
            services.AddTransient<IConverterService, ConverterService>();
            services.AddHangfire(x => x.UseMemoryStorage());
            services.AddHttpClient();
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