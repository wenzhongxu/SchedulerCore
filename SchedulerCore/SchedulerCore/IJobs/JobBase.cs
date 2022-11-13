using Newtonsoft.Json;
using Quartz;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Common.Enums;
using SchedulerCore.Host.IJobs.Models;
using Serilog;
using System.Diagnostics;

namespace SchedulerCore.Host.IJobs
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public abstract class JobBase<T> where T : LogModel, new()
    {
        protected readonly int maxLogCount = 20;//最多保存日志数量  
        protected readonly int warnTime = 20;//接口请求超过多少秒记录警告日志 
        protected Stopwatch stopwatch = new();
        protected T _logInfo { get; private set; }
        protected MailMessageEnum MailLevel = MailMessageEnum.None;

        public JobBase(T logInfo)
        {
            _logInfo = logInfo;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //结束时间超过当前时间，暂停当前任务
            var endTime = context.JobDetail.JobDataMap.GetString(Constant.EndAt);
            if (!string.IsNullOrWhiteSpace(endTime) && DateTime.Parse(endTime) <= DateTime.Now)
            {
                await context.Scheduler.PauseJob(new JobKey(context.JobDetail.Key.Name, context.JobDetail.Key.Group));
                return;
            }

            MailLevel = (MailMessageEnum)int.Parse(context.JobDetail.JobDataMap.GetString(Constant.MAILMESSAGE) ?? "0");
            //记录执行次数
            var runNumber = context.JobDetail.JobDataMap.GetLong(Constant.RUNNUMBER);
            context.JobDetail.JobDataMap[Constant.RUNNUMBER] = ++runNumber;

            var logs = context.JobDetail.JobDataMap[Constant.LOGLIST] as List<string> ?? new List<string>();
            if (logs.Count >= maxLogCount)
            {
                logs.RemoveRange(0, logs.Count - maxLogCount);
            }

            stopwatch.Restart(); //监视代码运行时间
            try
            {
                _logInfo.BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _logInfo.JobName = $"{context.JobDetail.Key.Group}.{context.JobDetail.Key.Name}";
                await NextExecute(context);
            }
            catch (Exception ex)
            {
                _logInfo.ErrorMsg = $"<span class='error'>{ex.Message}</span>";
                context.JobDetail.JobDataMap[Constant.EXCEPTION] = $"<div class='err-time'>{_logInfo.BeginTime}</div>{JsonConvert.SerializeObject(_logInfo)}";
                await ErrorAsync(_logInfo.JobName, ex, JsonConvert.SerializeObject(_logInfo), MailLevel);
            }
            finally
            {
                stopwatch.Stop(); //  停止监视            
                double seconds = stopwatch.Elapsed.TotalSeconds;  //总秒数             
                _logInfo.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (seconds >= 1)
                    _logInfo.ExecuteTime = seconds + "秒";
                else
                    _logInfo.ExecuteTime = stopwatch.Elapsed.TotalMilliseconds + "毫秒";

                var classErr = string.IsNullOrWhiteSpace(_logInfo.ErrorMsg) ? "" : "error";
                logs.Add($"<p class='msgList {classErr}'><span class='time'>{_logInfo.BeginTime} 至 {_logInfo.EndTime}  【耗时】{_logInfo.ExecuteTime}</span>{JsonConvert.SerializeObject(_logInfo)}</p>");
                context.JobDetail.JobDataMap[Constant.LOGLIST] = logs;
                if (seconds >= warnTime)//如果请求超过20秒，记录警告日志    
                {
                    await WarningAsync(_logInfo.JobName, "耗时过长 - " + JsonConvert.SerializeObject(_logInfo), MailLevel);
                }
            }
        }

        public abstract Task NextExecute(IJobExecutionContext context);

        public async Task WarningAsync(string title, string msg, MailMessageEnum mailMessage)
        {
            Log.Logger.Warning(msg);
            if (mailMessage == MailMessageEnum.All)
            {
                //await new SetingController().SendMail(new SendMailModel()
                //{
                //    Title = $"任务调度-{title}【警告】消息",
                //    Content = msg
                //});
            }
        }

        public async Task InformationAsync(string title, string msg, MailMessageEnum mailMessage)
        {
            Log.Logger.Information(msg);
            if (mailMessage == MailMessageEnum.All)
            {
                //await new SetingController().SendMail(new SendMailModel()
                //{
                //    Title = $"任务调度-{title}消息",
                //    Content = msg
                //});
            }
        }

        public async Task ErrorAsync(string title, Exception ex, string msg, MailMessageEnum mailMessage)
        {
            Log.Logger.Error(ex, msg);
            if (mailMessage == MailMessageEnum.Err || mailMessage == MailMessageEnum.All)
            {
                //await new SetingController().SendMail(new SendMailModel()
                //{
                //    Title = $"任务调度-{title}【异常】消息",
                //    Content = msg
                //});
            }
        }
    }
}
