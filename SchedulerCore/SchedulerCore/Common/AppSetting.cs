using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SchedulerCore.Host.Common
{
    public class AppSetting
    {
        /// <summary>
        /// 小驼峰命名
        /// </summary>
        public static JsonSerializerSettings SerializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }
}
