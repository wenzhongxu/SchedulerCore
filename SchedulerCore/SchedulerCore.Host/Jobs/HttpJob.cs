using Newtonsoft.Json;
using Quartz;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Common.Enum;
using SchedulerCore.Host.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SchedulerCore.Host.Jobs
{
    public class HttpJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("开始执行http任务");
            //获取相关参数
            var requestUrl = context.JobDetail.JobDataMap.GetString(Constant.RequestUrl)?.ToString();
            requestUrl = requestUrl?.IndexOf("http") == 0 ? requestUrl : "http://" + requestUrl;
            var requestParameters = context.JobDetail.JobDataMap.GetString(Constant.RequestParameters);
            var headerString = context.JobDetail.JobDataMap.GetString(Constant.Headers);
            var headers = headerString != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(headerString?.Trim()) : null;
            var requestType = (RequestTypeEnum)int.Parse(context.JobDetail.JobDataMap.GetString(Constant.RequestType));

            HttpResponseMessage responseMessage = new HttpResponseMessage();
            var http = HttpHelper.Instance;
            switch (requestType)
            {
                case RequestTypeEnum.Get:
                    responseMessage = await http.GetAsync(requestUrl, headers);
                    break;
                case RequestTypeEnum.Post:
                    responseMessage = await http.PostAsync(requestUrl, requestParameters, headers);
                    break;
                case RequestTypeEnum.Put:
                    break;
                case RequestTypeEnum.Delete:
                    break;
                default:
                    break;
            }

            var result = HttpUtility.HtmlEncode(await responseMessage.Content.ReadAsStringAsync());

            if (responseMessage.IsSuccessStatusCode)
            {
                context.JobDetail.JobDataMap[Constant.Exception] = result;
            }
        }
    }
}
