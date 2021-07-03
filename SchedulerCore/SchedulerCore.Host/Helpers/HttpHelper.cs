using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Helpers
{
    /// <summary>
    /// HTTP请求
    /// </summary>
    public class HttpHelper
    {
        public static readonly HttpHelper Instance;

        static HttpHelper()
        {
            Instance = new HttpHelper();
        }

        public static ConcurrentDictionary<string, HttpClient> dictionary = new ConcurrentDictionary<string, HttpClient>();

        private HttpClient GetHttpClient(string url)
        {
            var uri = new Uri(url);
            var key = uri.Scheme + uri.Host;
            return dictionary.GetOrAdd(key, new HttpClient());
        }

        public async Task<HttpResponseMessage> PostAsync(string url, string jsonString, Dictionary<string, string> headers = null)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                jsonString = "{}";
            }

            StringContent content = new StringContent(jsonString);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            if (headers != null && headers.Any())
            {
                using HttpClient http = new HttpClient();
                foreach (var item in headers)
                {
                    http.DefaultRequestHeaders.Remove(item.Key);
                    http.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                }
                return await http.PostAsync(new Uri(url), content);
            }
            else
            {
                return await GetHttpClient(url).PostAsync(new Uri(url), content);
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headers = null)
        {
            if (headers != null && headers.Any())
            {
                using HttpClient http = new HttpClient();
                foreach (var item in headers)
                {
                    http.DefaultRequestHeaders.Remove(item.Key);
                    http.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                }
                return await http.GetAsync(url);
            }
            else
            {
                return await GetHttpClient(url).GetAsync(url);
            }
        }
    }
}
