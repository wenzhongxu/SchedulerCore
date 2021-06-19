using Quartz;
using Quartz.Impl;
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
        private IScheduler _scheduler;

        /// <summary>
        /// 开启调度器
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartScheduleAsync()
        {
            await InitSchedulerAsync();

            if (_scheduler.InStandbyMode)
            {
                await _scheduler.Start();
                Console.WriteLine("任务调度已启动。");
            }

            return _scheduler.InStandbyMode;
        }

        private async Task InitSchedulerAsync()
        {
            if (_scheduler == null)
            {
                // 初始化任务调度器
            }
        }

    }
}
