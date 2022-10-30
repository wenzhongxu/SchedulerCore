using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Simpl;
using Quartz.Util;
using Scheduler.Repositories;
using SchedulerCore.Host.Common;

namespace SchedulerCore.Host.Managers
{
    public class SchedulerCenter
    {
        private string delegateType;
        private IDbProvider dbProvider;
        private IScheduler scheduler;

        public SchedulerCenter()
        {
            InitDriverDelegateType();
            dbProvider = new DbProvider(AppConfig.DbProviderName, AppConfig.ConnectionString);
        }

        private void InitDriverDelegateType()
        {
            string dbProviderName = AppConfig.DbProviderName;
            delegateType = dbProviderName switch
            {
                "SQLite-Microsoft" or "SQLite" => typeof(SQLiteDelegate).AssemblyQualifiedName,
                "Mysql" => typeof(MySQLDelegate).AssemblyQualifiedName,
                "OracleODPManaged" => typeof(OracleDelegate).AssemblyQualifiedName,
                "SqlServer" or "SQLServerMOT" => typeof(SqlServerDelegate).AssemblyQualifiedName,
                _ => throw new Exception("dbProviderName unreasonable")
            };
        }

        /// <summary>
        /// 初始化scheduler
        /// </summary>
        /// <returns></returns>
        private async Task InitSchedulerAsync()
        {
            if (scheduler != null)
            {
                DBConnectionManager.Instance.AddConnectionProvider("SchedulerAuction", dbProvider);
                var serializer = new JsonObjectSerializer();
                serializer.Initialize();
                var jobStore = new JobStoreTX
                {
                    DataSource = "SchedulerAuction",
                    TablePrefix = "QRTZ_",
                    InstanceId = "AUTO",
                    DriverDelegateType = delegateType,
                    ObjectSerializer = serializer
                };
                DirectSchedulerFactory.Instance.CreateScheduler("AuctionCrawl", "AUTO", new DefaultThreadPool(), jobStore);
                scheduler = await SchedulerRepository.Instance.Lookup("AuctionCrawl");
            }
        }

        public async Task<bool> StartSchedulerAsync()
        {
            await InitDbTablesAsync();
            await InitSchedulerAsync();

            if (scheduler.InStandbyMode)
            {
                await scheduler.Start();
                Console.WriteLine("任务调度已启动");
            }
            return scheduler.InStandbyMode;
        }

        public async Task InitDbTablesAsync()
        {
            IRepository repository = RepositoryFactory.CreateRepository(delegateType, dbProvider);
            await repository?.InitTable();
        }
    }
}
