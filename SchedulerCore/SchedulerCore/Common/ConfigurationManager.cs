namespace SchedulerCore.Host.Common
{
    public static class ConfigurationManager
    {
        private static readonly IConfiguration _configuration;
        static ConfigurationManager()
        {
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true).Build();
        }

        public static T GetSection<T>(string key) where T : class, new()
        {
            return _configuration.GetSection(key).Get<T>();
        }

        public static string GetConfig(this string key, string defaultValue = "")
        { 
            string value = _configuration.GetValue(key, defaultValue);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                {
                    return defaultValue?.Trim() ?? string.Empty;
                }
                throw new Exception($"获取配置{key}异常");
            }
            return value?.Trim() ?? string.Empty;
        }

        public static string GetTryConfig(this string key, string defaultValue = "")
        {
            string value = _configuration.GetValue(key, defaultValue);
            if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(defaultValue))
            {
                return defaultValue.Trim();
            }

            return value?.Trim() ?? string.Empty;
        }
    }
}
