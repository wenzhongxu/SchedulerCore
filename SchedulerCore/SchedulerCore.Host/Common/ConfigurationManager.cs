using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Common
{
    public static class ConfigurationManager
    {
        private static readonly IConfiguration Configuration;

        static ConfigurationManager()
        {
            Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true).Build();
        }

        public static T GetSection<T>(string key) where T : class, new()
        {
            return Configuration.GetSection(key).Get<T>();
        }

        public static string GetConfig(this string key, string defaultValue = "")
        {
            string value = Configuration.GetValue(key, defaultValue);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                {
                    return defaultValue?.Trim();
                }

                throw new Exception($"获取配置{key}异常");
            }

            return value?.Trim();
        }

        public static string GetTryConfig(this string key, string defaultValue = "")
        {
            string value = Configuration.GetValue(key, defaultValue);
            if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(defaultValue))
            {
                return defaultValue?.Trim();
            }

            return value?.Trim();
        }
    }
}
