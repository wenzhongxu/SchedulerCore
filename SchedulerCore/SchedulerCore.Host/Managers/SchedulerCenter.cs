using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Simpl;
using Quartz.Util;
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
        private string delegateType;
        private IScheduler scheduler;

        public SchedulerCenter()
        {
            InitDriverDelegateType();
            dbProvider = new DbProvider(AppConfig.DbProviderName, AppConfig.ConnectionString);
        }

        /// <summary>
        /// 初始化数据库类型
        /// </summary>
        private void InitDriverDelegateType()
        {
            var dbProdierName = AppConfig.DbProviderName.ToUpper();
            switch (dbProdierName)
            {
                case "ORACLE":
                    delegateType = typeof(OracleDelegate).AssemblyQualifiedName;
                    break;
                case "MYSQL":
                    delegateType = typeof(MySQLDelegate).AssemblyQualifiedName;
                    break;
                case "SQLSERVER":
                    delegateType = typeof(SqlServerDelegate).AssemblyQualifiedName;
                    break;
                default:
                    delegateType = typeof(OracleDelegate).AssemblyQualifiedName;
                    break;
            }
        }

        /// <summary>
        /// 初始化scheduler
        /// </summary>
        /// <returns></returns>
        private async Task InitSchedulerAsync()
        {
            if (scheduler == null)
            {
                DBConnectionManager.Instance.AddConnectionProvider("XCRMS", dbProvider);

                var serializer = new JsonObjectSerializer();
                serializer.Initialize();

                var jobStore = new JobStoreTX
                {
                    DataSource = "XCRMS",
                    TablePrefix = "QRTZ_",
                    InstanceId = "AUTO",
                    DriverDelegateType = delegateType,
                    ObjectSerializer = serializer
                };

                DirectSchedulerFactory.Instance.CreateScheduler("XCRMSScheduler", "AUTO", new DefaultThreadPool(), jobStore);
                scheduler = await SchedulerRepository.Instance.Lookup("XCRMSScheduler");
            }
        }

        /// <summary>
        /// 启动任务调度
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartScheduleAsync()
        {
            if (scheduler.InStandbyMode)
            {
                await scheduler.Start();
                Console.WriteLine("任务调度已启动");
            }
            return scheduler.InStandbyMode;
        }

    }
}
