using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Schedule.ImageService.Quartz
{
    public static class QuartzExtensions
    {
        public static void AddQuartz(this IServiceCollection services, Type jobType)
        {
            services.Add(new ServiceDescriptor(typeof(IJob), jobType, ServiceLifetime.Transient));
            services.AddSingleton<IJobFactory, ScheduledJobFactory>();
            services.AddSingleton(provider => JobBuilder.Create<UpdateJob>()
                .WithIdentity("Update.job", "group1")
                .Build());

            services.AddSingleton(provider => TriggerBuilder.Create()
                .WithIdentity($"Update.trigger", "group1")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(02, 00))
                .Build());

            services.AddSingleton(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                scheduler.Start();
                return scheduler;
            });
        }

        public static void UseQuartz(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<IScheduler>()
                .ScheduleJob(app.ApplicationServices.GetService<IJobDetail>(),
                    app.ApplicationServices.GetService<ITrigger>()
                );
        }
    }
}