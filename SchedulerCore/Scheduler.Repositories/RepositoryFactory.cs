using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;

namespace Scheduler.Repositories
{
    public class RepositoryFactory
    {
        public static IRepository CreateRepository(string dbType, IDbProvider dbProvider)
        {
            if (dbType == typeof(SQLiteDelegate).AssemblyQualifiedName)
            {
                return new RepositorySQLite(dbProvider);
            }
            return null;
        }
    }
}