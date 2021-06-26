using Dapper;
using Oracle.ManagedDataAccess.Client;
using Quartz.Impl.AdoJobStore.Common;
using System;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Repositories
{
    public class RepositoryOracle : IRepository
    {
        private IDbProvider DbProvider { get; }

        public RepositoryOracle(IDbProvider dbProvider)
        {
            DbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }

    }
}

     
