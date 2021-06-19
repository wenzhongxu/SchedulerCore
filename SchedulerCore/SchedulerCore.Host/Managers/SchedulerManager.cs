using Quartz;
using Quartz.Impl;
using SchedulerCore.Host.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Managers
{
    /// <summary>
    /// 任务调度中心 单例模式
    /// </summary>
    public class SchedulerManager
    {
        /// <summary>
        /// 开启调度器
        /// </summary>
        /// <returns></returns>
        public async Task StartScheduleAsync()
        {
            // grab instance
            StdSchedulerFactory factory = new();
            IScheduler scheduler = await factory.GetScheduler();

            // start it
            await scheduler.Start();

            // define the job and tie it to HelloJob
            IJobDetail job = JobBuilder.Create<HelloJob>()
                .WithIdentity("job1", "group1")
                .Build();

            // trigger the job to run now
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(10)
                .RepeatForever())
                .Build();

            // schedule the job using this trigger
            await scheduler.ScheduleJob(job, trigger);

            await Task.Delay(TimeSpan.FromSeconds(60));

            Console.Write("shutdown...");
            await scheduler.Shutdown();

            Console.WriteLine("Press any key to close the application");
            Console.ReadKey();
        }

    }
}
