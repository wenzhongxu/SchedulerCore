using Newtonsoft.Json;
using Quartz;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Common.Enum;
using SchedulerCore.Host.Helpers;
using SchedulerCore.Host.IJobs;
using SchedulerCore.Host.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Talk.Extensions;

namespace SchedulerCore.Host.Jobs
{
    public class HttpJob : JobBase<LogUrlModel>, IJob
    {
        public HttpJob() :base(new LogUrlModel())
        {

        }


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
                    responseMessage = await http.PutAsync(requestUrl, requestParameters, headers);
                    break;
                case RequestTypeEnum.Delete:
                    responseMessage = await http.DeleteAsync(requestUrl);
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

        public override async Task NextExecute(IJobExecutionContext context)
        {
            //获取相关参数
            var requestUrl = context.JobDetail.JobDataMap.GetString(Constant.RequestUrl)?.ToString();
            requestUrl = requestUrl?.IndexOf("http") == 0 ? requestUrl : "http://" + requestUrl;
            var requestParameters = context.JobDetail.JobDataMap.GetString(Constant.RequestParameters);
            var headerString = context.JobDetail.JobDataMap.GetString(Constant.Headers);
            var headers = headerString != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(headerString?.Trim()) : null;
            var requestType = (RequestTypeEnum)int.Parse(context.JobDetail.JobDataMap.GetString(Constant.RequestType));

            LogInfo.Url = requestUrl;
            LogInfo.RequestType = requestType.ToString();
            LogInfo.Parameters = requestParameters;

            HttpResponseMessage responseMessage = new();
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
                    responseMessage = await http.PutAsync(requestUrl, requestParameters, headers);
                    break;
                case RequestTypeEnum.Delete:
                    responseMessage = await http.DeleteAsync(requestUrl);
                    break;
                default:
                    break;
            }
            var result = HttpUtility.HtmlEncode(await responseMessage.Content.ReadAsStringAsync());
            LogInfo.Result = $"<span class='result'>{result.MaxLeft(1000)}</span>";
            if (!responseMessage.IsSuccessStatusCode)
            {
                LogInfo.ErrorMessage = $"<span class='error'>{result.MaxLeft(3000)}</span>";
                await ErrorAsync(LogInfo.JobName, new Exception(result.MaxLeft(3000)), JsonConvert.SerializeObject(LogInfo), MailLevel);
                context.JobDetail.JobDataMap[Constant.Exception] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
            }
            else
            {
                try
                {
                    var httpResult = JsonConvert.DeserializeObject<HttpResultDto>(HttpUtility.HtmlDecode(result));
                    if (!httpResult.IsSuccess)
                    {
                        LogInfo.ErrorMessage = $"<span class='error'>{httpResult.ErrorMsg}</span>";
                        await ErrorAsync(LogInfo.JobName, new Exception(httpResult.ErrorMsg), JsonConvert.SerializeObject(LogInfo), MailLevel);
                        context.JobDetail.JobDataMap[Constant.Exception] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
                    }
                    else
                    {
                        await InfoAsync(LogInfo.JobName, JsonConvert.SerializeObject(LogInfo), MailLevel);
                    }
                }
                catch (Exception)
                {
                    await InfoAsync(LogInfo.JobName, JsonConvert.SerializeObject(LogInfo), MailLevel);
                }
            }
        }
    }
}
