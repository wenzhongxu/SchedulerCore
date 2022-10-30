namespace SchedulerCore.Host.Common
{
    public static class AppConfig
    {
        public static string DbProviderName { get; set; } = ConfigurationManager.GetTryConfig("Quartz:dbProviderName");

        public static string ConnectionString { get; set; } = ConfigurationManager.GetTryConfig("Quartz:connectionString");
    }
}
