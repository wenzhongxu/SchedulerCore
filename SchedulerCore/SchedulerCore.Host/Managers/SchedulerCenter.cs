using Quartz.Impl.AdoJobStore.Common;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Managers
{
    /// <summary>
    /// 任务调度中心 单例 采用 DirectSchedulerFactory
    /// </summary>
    public class SchedulerCenter
    {
        private IDbProvider dbProvider;

        public SchedulerCenter()
        {
            dbProvider = new DbProvider(AppConfig.DbProviderName, AppConfig.ConnectionString);
        }

        /// <summary>
        /// 初始化数据表
        /// </summary>
        /// <returns></returns>
        private async Task InitDbTableAsync()
        {
            IRepository repositorie = RepositoryFactory.CreateRepository(AppConfig.DbProviderName, dbProvider);
            await repositorie?.InitTable();
        }

    }
}
