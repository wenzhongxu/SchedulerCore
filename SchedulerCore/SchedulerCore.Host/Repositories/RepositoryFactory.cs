using Quartz.Impl.AdoJobStore;
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
            if (dbType == typeof(SQLiteDelegate).AssemblyQualifiedName)
            {
                return new RepositorySQLite(dbProvider);
            }
            else if (dbType == typeof(OracleDelegate).AssemblyQualifiedName)
            {
                return new RepositoryOracle(dbProvider);
            }
            else
            {
                return null;
            }
        }
    }
}
