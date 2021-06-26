using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Common
{
    public static class AppConfig
    {
        public static readonly IConfiguration Configuration;

        public static string DbProviderName = Configuration["Quartz:dbProviderName"];

        private static readonly string host = Configuration["Quartz:Oracle:Host"];
        private static readonly string port = Configuration["Quartz:Oracle:Port"];
        private static readonly string serviceName = Configuration["Quartz:Oracle:ServiceName"];
        private static readonly string user = Configuration["Quartz:Oracle:User"];
        private static readonly string pwd = Configuration["Quartz:Oracle:Pwd"];

        //要处理密码加密。。。
        public static string ConnectionString = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port})))(CONNECT_DATA=(SERVICE_NAME={serviceName})));User Id={user};Password={pwd};";
    }
}
