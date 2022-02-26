using AutoMapper;
using Newtonsoft.Json;
using Quartz;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Common.Enum;
using SchedulerCore.Host.Controllers;
using SchedulerCore.Host.Entities;
using SchedulerCore.Host.IJobs;
using SchedulerCore.Host.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.IJobs
{
    public abstract class JobBase<T> where T : LogModel, new()
    {
        protected readonly int maxLogCount = 20; // 最多保存日志数量
        protected readonly int warnTime = 20; // 接口请求超过多少秒记录警告日志
        protected Stopwatch stopwatch = new();
        protected T LogInfo { get; private set; }
        protected MailMessageEnum MailLevel = MailMessageEnum.None;


        public JobBase(T logInfo)
        {
            LogInfo = logInfo;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // 如果结束时间超过当前时间，暂停任务
            var endTime = context.JobDetail.JobDataMap.GetString("endAt");
            if (!string.IsNullOrEmpty(endTime) && DateTime.Parse(endTime) <= DateTime.Now)
            {
                await context.Scheduler.PauseJob(new JobKey(context.JobDetail.Key.Name, context.JobDetail.Key.Group));
                return;
            }

            MailLevel = (MailMessageEnum)int.Parse(context.JobDetail.JobDataMap.GetString(Constant.MailMessage) ?? "0");
            //记录执行次数
            var runNumber = context.JobDetail.JobDataMap.GetLong(Constant.RunNumber);
            context.JobDetail.JobDataMap[Constant.RunNumber] = ++runNumber;

            var logs = context.JobDetail.JobDataMap[Constant.LogList] as List<string> ?? new List<string>();
            if (logs.Count >= maxLogCount)
            {
                logs.RemoveRange(0, logs.Count - maxLogCount);
            }

            stopwatch.Restart(); // 开始监视代码运行时间
            try
            {
                LogInfo.BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                LogInfo.JobName = $"{context.JobDetail.Key.Group}.{context.JobDetail.Key.Name}";
                await NextExecute(context);
            }
            catch (Exception ex)
            {
                LogInfo.ErrorMessage = $"<span class='error'>{ex.Message}</span>";
                context.JobDetail.JobDataMap[Constant.Exception] = $"<div class='err-time'>{LogInfo.BeginTime}</div>{JsonConvert.SerializeObject(LogInfo)}";
                await ErrorAsync(LogInfo.JobName, ex, JsonConvert.SerializeObject(LogInfo), MailLevel);
            }
            finally
            {
                stopwatch.Stop(); //停止监视
                double seconds = stopwatch.Elapsed.TotalSeconds;
                if (seconds >= 1)
                {
                    LogInfo.ExecuteTime = seconds + "秒";
                }
                else
                {
                    LogInfo.ExecuteTime = stopwatch.Elapsed.TotalMilliseconds + "毫秒";
                }
                LogInfo.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            }
        }

        public abstract Task NextExecute(IJobExecutionContext context);

        public async Task InfoAsync(string title, string msg, MailMessageEnum mailMessage)
        {
            Log.Logger.Information(msg);
            if (mailMessage == MailMessageEnum.All)
            {
                SendMail sendMail = new()
                {
                    Title = $"任务调度-{title}消息",
                    Content = msg
                };
                await MailHelper.SendMail(sendMail.Title, sendMail.Content, sendMail.MailInfo);
            }
        }

        public async Task ErrorAsync(string title, Exception ex, string msg, MailMessageEnum mailMessage)
        {
            Log.Logger.Error(ex, msg);
            if (mailMessage == MailMessageEnum.Err || mailMessage == MailMessageEnum.All)
            {
                SendMail sendMail = new()
                {
                    Title = $"任务调度-{title}【异常】消息",
                    Content = msg
                };
                await MailHelper.SendMail(sendMail.Title, sendMail.Content, sendMail.MailInfo);
            }
        }
    }
}
