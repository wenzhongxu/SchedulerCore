using Newtonsoft.Json;
using Quartz;
using Scheduler.Common.Helpers;
using Scheduler.Common.Helpers.Models;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Common.Enums;
using SchedulerCore.Host.IJobs.Models;
using System.Web;

namespace SchedulerCore.Host.IJobs
{
    public class HttpJob : JobBase<LogUrlModel>, IJob
    {
        public HttpJob():base(new LogUrlModel())
        {

        }

        public override async Task NextExecute(IJobExecutionContext context)
        {
            //获取相关参数
            var requestUrl = context.JobDetail.JobDataMap.GetString(Constant.REQUESTURL)?.Trim();
            requestUrl = requestUrl?.IndexOf("http") == 0 ? requestUrl : "http://" + requestUrl;
            var requestParameters = context.JobDetail.JobDataMap.GetString(Constant.REQUESTPARAMETERS);
            var headersString = context.JobDetail.JobDataMap.GetString(Constant.HEADERS);
            var headers = headersString != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(headersString?.Trim()) : null;
            var requestType = (RequestTypeEnum)int.Parse(context.JobDetail.JobDataMap.GetString(Constant.REQUESTTYPE));


            _logInfo.Url = requestUrl;
            _logInfo.RequestType = requestType.ToString();
            _logInfo.Parameters = requestParameters;

            HttpResponseMessage response = new();
            var http = HttpHelper.Instance;
            switch (requestType)
            {
                case RequestTypeEnum.Get:
                    response = await http.GetAsync(requestUrl, headers);
                    break;
                case RequestTypeEnum.Post:
                    response = await http.PostAsync(requestUrl, requestParameters, headers);
                    break;
                case RequestTypeEnum.Put:
                    response = await http.PutAsync(requestUrl, requestParameters, headers);
                    break;
                case RequestTypeEnum.Delete:
                    response = await http.DeleteAsync(requestUrl, headers);
                    break;
            }
            var result = HttpUtility.HtmlEncode(await response.Content.ReadAsStringAsync());
            _logInfo.Result = $"<span class='result'>{result}</span>";
            if (!response.IsSuccessStatusCode)
            {
                _logInfo.ErrorMsg = $"<span class='error'>{result}</span>";
                await ErrorAsync(_logInfo.JobName, new Exception(result), JsonConvert.SerializeObject(_logInfo), MailLevel);
                context.JobDetail.JobDataMap[Constant.EXCEPTION] = $"<div class='err-time'>{_logInfo.BeginTime}</div>{JsonConvert.SerializeObject(_logInfo)}";
            }
            else
            {
                try
                {
                    //这里需要和请求方约定好返回结果约定为HttpResultModel模型
                    var httpResult = JsonConvert.DeserializeObject<HttpResultModel>(HttpUtility.HtmlDecode(result));
                    if (!httpResult.IsSuccess)
                    {
                        _logInfo.ErrorMsg = $"<span class='error'>{httpResult.ErrorMsg}</span>";
                        await ErrorAsync(_logInfo.JobName, new Exception(httpResult.ErrorMsg), JsonConvert.SerializeObject(_logInfo), MailLevel);
                        context.JobDetail.JobDataMap[Constant.EXCEPTION] = $"<div class='err-time'>{_logInfo.BeginTime}</div>{JsonConvert.SerializeObject(_logInfo)}";
                    }
                    else
                        await InformationAsync(_logInfo.JobName, JsonConvert.SerializeObject(_logInfo), MailLevel);
                }
                catch (Exception)
                {
                    await InformationAsync(_logInfo.JobName, JsonConvert.SerializeObject(_logInfo), MailLevel);
                }
            }
        }
    }
}
