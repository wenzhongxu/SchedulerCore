using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Helpers
{
    public static class JsonHelper
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }

        /// <summary>
        /// 将Json数据格式的字符串反序列化为一个对象
        /// </summary>
        /// <returns></returns>
        public static T Deserialize<T>(string josnString)
        {
            try
            {
                if (string.IsNullOrEmpty(josnString))
                {
                    josnString = string.Empty;
                }
                var obj = JsonConvert.DeserializeObject<T>(josnString);
                return obj;
            }
            catch (Exception e)
            {
                string s = e.InnerException != null ? e.InnerException.ToString() : e.ToString();

                if (s.Contains("Could not cast or convert from"))
                {
                    s = "数据类型格式错误，请检查输入项。";
                }

                throw new Exception(s);
            }
        }
    }
}
