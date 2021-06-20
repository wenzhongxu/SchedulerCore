using Quartz.Impl.AdoJobStore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Repositories
{
    public class RepositoryMySql : IRepository
    {
        public RepositoryMySql(IDbProvider dbProvider)
        {
            DbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }

        public IDbProvider DbProvider { get; }

        public Task<int> InitTable()
        {
            throw new NotImplementedException();
        }
    }
}
