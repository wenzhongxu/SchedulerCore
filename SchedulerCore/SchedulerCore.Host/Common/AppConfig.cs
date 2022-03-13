using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Common
{
    public static class AppConfig
    {
        public static string DbProviderName => ConfigurationManager.GetTryConfig("Quartz:dbProviderName");

        private static string Host => ConfigurationManager.GetTryConfig("Quartz:Oracle:Host");

        private static string Port => ConfigurationManager.GetTryConfig("Quartz:Oracle:Port");

        private static string ServiceName => ConfigurationManager.GetTryConfig("Quartz:Oracle:ServiceName");

        private static string User => ConfigurationManager.GetTryConfig("Quartz:Oracle:User");

        private static string Pwd => ConfigurationManager.GetTryConfig("Quartz:Oracle:Pwd");

        //要处理密码加密。。。
        //public static string ConnectionString = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={Host})(PORT={Port})))(CONNECT_DATA=(SERVICE_NAME={ServiceName})));User Id={User};Password={Pwd};";
        public static string ConnectionString => ConfigurationManager.GetTryConfig("Quartz:connectionString");
    }
}
