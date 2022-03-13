using Dapper;
using Microsoft.Data.Sqlite;
using Quartz.Impl.AdoJobStore.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Repositories
{
    public class RepositorySQLite : IRepository
    {
        private readonly IDbProvider DbProvider;

        public RepositorySQLite(IDbProvider dbProvider)
        {
            DbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }

        public async Task<int> InitTable()
        {
            if (!Directory.Exists("File")) Directory.CreateDirectory("File");

            using (var connection = new SqliteConnection(DbProvider.ConnectionString))
            {
                var check_sql = @$"SELECT
	                                        count(1)
                                        FROM
	                                        sqlite_master
                                        WHERE
	                                        type = 'table'
                                        AND name IN (
	                                        'QRTZ_JOB_DETAILS',
	                                        'QRTZ_TRIGGERS',
	                                        'QRTZ_SIMPLE_TRIGGERS',
	                                        'QRTZ_SIMPROP_TRIGGERS',
	                                        'QRTZ_CRON_TRIGGERS',
	                                        'QRTZ_BLOB_TRIGGERS',
	                                        'QRTZ_CALENDARS',
	                                        'QRTZ_PAUSED_TRIGGER_GRPS',
	                                        'QRTZ_FIRED_TRIGGERS',
	                                        'QRTZ_SCHEDULER_STATE',
	                                        'QRTZ_LOCKS'
                                            );";
                var count = await connection.QueryFirstOrDefaultAsync<int>(check_sql);
                if (count == 0)
                {
                    string initSql = await File.ReadAllTextAsync("Tables/tables_sqlite.sql");
                    return await connection.ExecuteAsync(initSql);
                }
            }
            return 0;
        }
    }
}
