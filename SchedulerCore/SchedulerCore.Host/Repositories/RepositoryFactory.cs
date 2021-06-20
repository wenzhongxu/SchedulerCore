using Quartz.Impl.AdoJobStore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Repositories
{
    public class RepositoryFactory
    {
        public static IRepository CreateRepository(string dbType, IDbProvider dbProvider)
        {
            return dbType switch
            {
                "Oracle" => new RepositoryOracle(dbProvider),
                "MySql" => new RepositoryMySql(dbProvider),
                _ => null,
            };
        }
    }
}
