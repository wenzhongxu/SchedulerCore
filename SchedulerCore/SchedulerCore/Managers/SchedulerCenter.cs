using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Simpl;
using Quartz.Util;
using Scheduler.Repositories;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Common.Enums;
using SchedulerCore.Host.Entities;
using SchedulerCore.Host.IJobs;
using SchedulerCore.Host.Models;
using Serilog;

namespace SchedulerCore.Host.Managers
{
    public class SchedulerCenter
    {
        private string delegateType;
        private IDbProvider dbProvider;
        private IScheduler scheduler;

        public SchedulerCenter()
        {
            InitDriverDelegateType();
            dbProvider = new DbProvider(AppConfig.DbProviderName, AppConfig.ConnectionString);
        }

        private void InitDriverDelegateType()
        {
            string dbProviderName = AppConfig.DbProviderName;
            delegateType = dbProviderName switch
            {
                "SQLite-Microsoft" or "SQLite" => typeof(SQLiteDelegate).AssemblyQualifiedName,
                "Mysql" => typeof(MySQLDelegate).AssemblyQualifiedName,
                "OracleODPManaged" => typeof(OracleDelegate).AssemblyQualifiedName,
                "SqlServer" or "SQLServerMOT" => typeof(SqlServerDelegate).AssemblyQualifiedName,
                _ => throw new Exception("dbProviderName unreasonable")
            };
        }

        /// <summary>
        /// 初始化scheduler
        /// </summary>
        /// <returns></returns>
        private async Task InitSchedulerAsync()
        {
            if (scheduler == null)
            {
                DBConnectionManager.Instance.AddConnectionProvider("SchedulerAuction", dbProvider);
                var serializer = new JsonObjectSerializer();
                serializer.Initialize();
                var jobStore = new JobStoreTX
                {
                    DataSource = "SchedulerAuction",
                    TablePrefix = "QRTZ_",
                    InstanceId = "AUTO",
                    DriverDelegateType = delegateType,
                    ObjectSerializer = serializer
                };
                DirectSchedulerFactory.Instance.CreateScheduler("AuctionCrawl", "AUTO", new DefaultThreadPool(), jobStore);
                scheduler = await SchedulerRepository.Instance.Lookup("AuctionCrawl");
            }
        }

        /// <summary>
        /// 开启调度器
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartSchedulerAsync()
        {
            await InitDbTablesAsync();
            await InitSchedulerAsync();

            if (scheduler.InStandbyMode)
            {
                await scheduler.Start();
                Console.WriteLine("任务调度已启动");
            }
            return scheduler.InStandbyMode;
        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        public async Task<bool> StopSchedulerAsync()
        {
            //判断调度是否已经关闭
            if (!scheduler.InStandbyMode)
            {
                //等待任务运行完成
                await scheduler.Standby(); //TODO  注意：Shutdown后Start会报错，所以这里使用暂停。
                Log.Information("任务调度暂停！");
            }
            return !scheduler.InStandbyMode;
        }

        /// <summary>
        /// 初始化数据库表
        /// </summary>
        /// <returns></returns>
        public async Task InitDbTablesAsync()
        {
            IRepository repository = RepositoryFactory.CreateRepository(delegateType, dbProvider);
            await repository?.InitTable();
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public async Task<ScheduleEntity> QueryJobAsync(string jobGroup, string jobName)
        {
            ScheduleEntity entity = new();
            var jobKey = new JobKey(jobGroup, jobName);
            var jobDetail = await scheduler.GetJobDetail(jobKey);
            var triggersList = await scheduler.GetTriggersOfJob(jobKey);
            var triggers = triggersList.AsEnumerable().FirstOrDefault();
            var intervalSeconds = (triggers as SimpleTriggerImpl)?.RepeatInterval.TotalSeconds;
            var endTime = jobDetail.JobDataMap.GetString(Constant.EndAt);
            entity.BeginTime = triggers.StartTimeUtc.LocalDateTime;
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                entity.EndTime = DateTime.Parse(endTime);
            }
            if (intervalSeconds.HasValue)
            {
                entity.IntervalSecond = Convert.ToInt32(intervalSeconds.Value);
            }
            entity.JobGroup = jobGroup;
            entity.JobName = jobName;
            entity.Cron = (triggers as CronTriggerImpl)?.CronExpressionString;
            entity.RunTimes = (triggers as SimpleTriggerImpl)?.RepeatCount;
            entity.TriggerType = triggers is SimpleTriggerImpl ? TriggerTypeEnum.Simple : TriggerTypeEnum.Cron;
            entity.MailMessage = (MailMessageEnum)int.Parse(jobDetail.JobDataMap.GetString(Constant.MAILMESSAGE) ?? "0");
            entity.Description = jobDetail.Description;
            entity.JobType = (JobTypeEnum)int.Parse(jobDetail.JobDataMap.GetString(Constant.JobTypeEnum) ?? "1");
            switch (entity.JobType)
            {
                case JobTypeEnum.None:
                    break;
                case JobTypeEnum.Url:
                    entity.RequestUrl = jobDetail.JobDataMap.GetString(Constant.REQUESTURL);
                    entity.RequestType = (RequestTypeEnum)int.Parse(jobDetail.JobDataMap.GetString(Constant.REQUESTTYPE));
                    entity.RequestParameters = jobDetail.JobDataMap.GetString(Constant.REQUESTPARAMETERS);
                    entity.Headers = jobDetail.JobDataMap.GetString(Constant.HEADERS);
                    break;
                case JobTypeEnum.Emial:
                    entity.MailTitle = jobDetail.JobDataMap.GetString(Constant.MailTitle);
                    entity.MailContent = jobDetail.JobDataMap.GetString(Constant.MailContent);
                    entity.MailTo = jobDetail.JobDataMap.GetString(Constant.MailTo);
                    break;
                case JobTypeEnum.Mqtt:
                    entity.Payload = jobDetail.JobDataMap.GetString(Constant.Payload);
                    entity.Topic = jobDetail.JobDataMap.GetString(Constant.Topic);
                    break;
                case JobTypeEnum.RabbitMQ:
                    entity.RabbitQueue = jobDetail.JobDataMap.GetString(Constant.RabbitQueue);
                    entity.RabbitBody = jobDetail.JobDataMap.GetString(Constant.RabbitBody);
                    break;
                case JobTypeEnum.Hotreload:
                    break;
                default:
                    break;
            }
            return entity;
        }

        /// <summary>
        /// 检查任务是否存在
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<bool> CheckExists(JobKey jobKey)
        {
            return await scheduler.CheckExists(jobKey);
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="runNumber"></param>
        /// <returns></returns>
        public async Task<int> AddScheduleJobAsync(ScheduleEntity entity, long? runNumber = null)
        {
            //http请求配置
            var httpDir = new Dictionary<string, string>()
                {
                    { Constant.EndAt, entity.EndTime.ToString()},
                    { Constant.JobTypeEnum, ((int)entity.JobType).ToString()},
                    { Constant.MAILMESSAGE, ((int)entity.MailMessage).ToString()},
                };
            if (runNumber.HasValue)
                httpDir.Add(Constant.RUNNUMBER, runNumber.ToString());

            IJobConfigurator jobConfigurator;
            switch (entity.JobType)
            {
                case JobTypeEnum.Url:
                    jobConfigurator = JobBuilder.Create<HttpJob>();
                    httpDir.Add(Constant.REQUESTURL, entity.RequestUrl);
                    httpDir.Add(Constant.HEADERS, entity.Headers);
                    httpDir.Add(Constant.REQUESTPARAMETERS, entity.RequestParameters);
                    httpDir.Add(Constant.REQUESTTYPE, ((int)entity.RequestType).ToString());
                    break;
                default:
                    jobConfigurator = JobBuilder.Create<HttpJob>(); 
                    break;
            }

            IJobDetail job = jobConfigurator.SetJobData(new JobDataMap(httpDir))
                .WithDescription(entity.Description)
                .WithIdentity(entity.JobName, entity.JobGroup)
                .Build();
            ITrigger trigger;
            if (entity.TriggerType == TriggerTypeEnum.Cron)
            {
                trigger = CreateCronTrigger(entity);
            }
            else
            {
                trigger = CreateSimpleTrigger(entity);
            }

            await scheduler.ScheduleJob(job, trigger);
            return 1;
        }

        /// <summary>
        /// 创建类型Simple的触发器
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ITrigger CreateSimpleTrigger(ScheduleEntity entity)
        {
            //作业触发器
            if (entity.RunTimes.HasValue && entity.RunTimes > 0)
            {
                return TriggerBuilder.Create()
               .WithIdentity(entity.JobName, entity.JobGroup)
               .StartAt(entity.BeginTime)//开始时间
                                         //.EndAt(entity.EndTime)//结束数据
               .WithSimpleSchedule(x =>
               {
                   x.WithIntervalInSeconds(entity.IntervalSecond.Value)//执行时间间隔，单位秒
                        .WithRepeatCount(entity.RunTimes.Value)//执行次数、默认从0开始
                        .WithMisfireHandlingInstructionFireNow();
               })
               .ForJob(entity.JobName, entity.JobGroup)//作业名称
               .Build();
            }
            else
            {
                return TriggerBuilder.Create()
               .WithIdentity(entity.JobName, entity.JobGroup)
               .StartAt(entity.BeginTime)//开始时间
                                         //.EndAt(entity.EndTime)//结束数据
               .WithSimpleSchedule(x =>
               {
                   x.WithIntervalInSeconds(entity.IntervalSecond.Value)//执行时间间隔，单位秒
                        .RepeatForever()//无限循环
                        .WithMisfireHandlingInstructionFireNow();
               })
               .ForJob(entity.JobName, entity.JobGroup)//作业名称
               .Build();
            }

        }

        /// <summary>
        /// 创建类型Cron的触发器
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ITrigger CreateCronTrigger(ScheduleEntity entity)
        {
            // 作业触发器
            return TriggerBuilder.Create()

                   .WithIdentity(entity.JobName, entity.JobGroup)
                   .StartAt(entity.BeginTime)//开始时间
                                             //.EndAt(entity.EndTime)//结束时间
                   .WithCronSchedule(entity.Cron, cronScheduleBuilder => cronScheduleBuilder.WithMisfireHandlingInstructionFireAndProceed())//指定cron表达式
                   .ForJob(entity.JobName, entity.JobGroup)//作业名称
                   .Build();
        }

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<bool> TriggerJobAsync(JobKey jobKey)
        {
            await scheduler.TriggerJob(jobKey);
            return true;
        }

        /// <summary>
        /// 获取所有Job（详情信息 - 初始化页面调用）
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobInfoDto>> GetAllJobAsync()
        {
            List<JobKey> jboKeyList = new();
            List<JobInfoDto> jobInfoList = new();
            var groupNames = await scheduler.GetJobGroupNames();
            foreach (var groupName in groupNames.OrderBy(t => t))
            {
                jboKeyList.AddRange(await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
                jobInfoList.Add(new JobInfoDto() { GroupName = groupName });
            }
            foreach (var jobKey in jboKeyList.OrderBy(t => t.Name))
            {
                var jobDetail = await scheduler.GetJobDetail(jobKey);
                var triggersList = await scheduler.GetTriggersOfJob(jobKey);
                var triggers = triggersList.AsEnumerable().FirstOrDefault();

                var interval = string.Empty;
                if (triggers is SimpleTriggerImpl)
                    interval = (triggers as SimpleTriggerImpl)?.RepeatInterval.ToString();
                else
                    interval = (triggers as CronTriggerImpl)?.CronExpressionString;

                foreach (var jobInfo in jobInfoList)
                {
                    if (jobInfo.GroupName == jobKey.Group)
                    {
                        //旧代码没有保存JobTypeEnum，所以None可以默认为Url。
                        var jobType = (JobTypeEnum)jobDetail.JobDataMap.GetLong(Constant.JobTypeEnum);
                        jobType = jobType == JobTypeEnum.None ? JobTypeEnum.Url : jobType;

                        var triggerAddress = string.Empty;
                        if (jobType == JobTypeEnum.Url)
                            triggerAddress = jobDetail.JobDataMap.GetString(Constant.REQUESTURL);
                        else if (jobType == JobTypeEnum.Emial)
                            triggerAddress = jobDetail.JobDataMap.GetString(Constant.MailTo);
                        else if (jobType == JobTypeEnum.Mqtt)
                            triggerAddress = jobDetail.JobDataMap.GetString(Constant.Topic);
                        else if (jobType == JobTypeEnum.RabbitMQ)
                            triggerAddress = jobDetail.JobDataMap.GetString(Constant.RabbitQueue);

                        //Constant.MailTo
                        jobInfo.JobInfoList.Add(new JobInfo()
                        {
                            Name = jobKey.Name,
                            LastErrMsg = jobDetail.JobDataMap.GetString(Constant.EXCEPTION),
                            TriggerAddress = triggerAddress,
                            TriggerState = await scheduler.GetTriggerState(triggers.Key),
                            PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                            BeginTime = triggers.StartTimeUtc.LocalDateTime,
                            Interval = interval,
                            EndTime = triggers.EndTimeUtc?.LocalDateTime,
                            Description = jobDetail.Description,
                            RequestType = jobDetail.JobDataMap.GetString(Constant.REQUESTTYPE),
                            RunNumber = jobDetail.JobDataMap.GetLong(Constant.RUNNUMBER),
                            JobType = (long)jobType
                            //(triggers as SimpleTriggerImpl)?.TimesTriggered
                            //CronTriggerImpl 中没有 TimesTriggered 所以自己RUNNUMBER记录
                        });
                        continue;
                    }
                }
            }
            return jobInfoList;
        }

        /// <summary>
        /// 获取所有Job信息（简要信息 - 刷新数据的时候使用）
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobBriefInfoDto>> GetAllJobBriefInfoAsync()
        {
            List<JobKey> jboKeyList = new();
            List<JobBriefInfoDto> jobInfoList = new();
            var groupNames = await scheduler.GetJobGroupNames();
            foreach (var groupName in groupNames.OrderBy(t => t))
            {
                jboKeyList.AddRange(await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
                jobInfoList.Add(new JobBriefInfoDto() { GroupName = groupName });
            }
            foreach (var jobKey in jboKeyList.OrderBy(t => t.Name))
            {
                var jobDetail = await scheduler.GetJobDetail(jobKey);
                var triggersList = await scheduler.GetTriggersOfJob(jobKey);
                var triggers = triggersList.AsEnumerable().FirstOrDefault();

                foreach (var jobInfo in jobInfoList)
                {
                    if (jobInfo.GroupName == jobKey.Group)
                    {
                        jobInfo.JobInfoList.Add(new JobBriefInfo()
                        {
                            Name = jobKey.Name,
                            LastErrMsg = jobDetail?.JobDataMap.GetString(Constant.EXCEPTION),
                            TriggerState = await scheduler.GetTriggerState(triggers.Key),
                            PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                            RunNumber = jobDetail?.JobDataMap.GetLong(Constant.RUNNUMBER) ?? 0
                        });
                        continue;
                    }
                }
            }
            return jobInfoList;
        }

        /// <summary>
        /// 暂停/删除 指定的计划
        /// </summary>
        /// <param name="jobGroup">任务分组</param>
        /// <param name="jobName">任务名称</param>
        /// <param name="isDelete">停止并删除任务</param>
        /// <returns></returns>
        public async Task<BaseResultDto> StopOrDelScheduleJobAsync(string jobGroup, string jobName, bool isDelete = false)
        {
            BaseResultDto result;
            try
            {
                await scheduler.PauseJob(new JobKey(jobName, jobGroup));
                if (isDelete)
                {
                    await scheduler.DeleteJob(new JobKey(jobName, jobGroup));
                    result = new BaseResultDto
                    {
                        Code = 200,
                        Msg = "删除任务计划成功！"
                    };
                }
                else
                {
                    result = new BaseResultDto
                    {
                        Code = 200,
                        Msg = "停止任务计划成功！"
                    };
                }

            }
            catch (Exception ex)
            {
                result = new BaseResultDto
                {
                    Code = 505,
                    Msg = "停止任务计划失败" + ex.Message
                };
            }
            return result;
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="jobGroup">任务分组</param>
        public async Task<BaseResultDto> ResumeJobAsync(string jobGroup, string jobName)
        {
            BaseResultDto result = new BaseResultDto();
            try
            {
                //检查任务是否存在
                var jobKey = new JobKey(jobName, jobGroup);
                if (await scheduler.CheckExists(jobKey))
                {
                    var jobDetail = await scheduler.GetJobDetail(jobKey);
                    var endTime = jobDetail.JobDataMap.GetString("EndAt");
                    if (!string.IsNullOrWhiteSpace(endTime) && DateTime.Parse(endTime) <= DateTime.Now)
                    {
                        result.Code = 500;
                        result.Msg = "Job的结束时间已过期。";
                    }
                    else
                    {
                        //任务已经存在则暂停任务
                        await scheduler.ResumeJob(jobKey);
                        result.Msg = "恢复任务计划成功！";
                        Log.Information(string.Format("任务“{0}”恢复运行", jobName));
                    }
                }
                else
                {
                    result.Code = 500;
                    result.Msg = "任务不存在";
                }
            }
            catch (Exception ex)
            {
                result.Msg = "恢复任务计划失败！";
                result.Code = 500;
                Log.Error(string.Format("恢复任务失败！{0}", ex));
            }
            return result;
        }

        /// <summary>
        /// 获取运行次数
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<long> GetRunNumberAsync(JobKey jobKey)
        {
            var jobDetail = await scheduler.GetJobDetail(jobKey);
            return jobDetail.JobDataMap.GetLong(Constant.RUNNUMBER);
        }

        /// <summary>
        /// 获取job日志
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<List<string>> GetJobLogsAsync(JobKey jobKey)
        {
            var jobDetail = await scheduler.GetJobDetail(jobKey);
            return jobDetail.JobDataMap[Constant.LOGLIST] as List<string>;
        }

        /// <summary>
        /// 移除异常信息
        /// 因为只能在IJob持久化操作JobDataMap，所有这里直接暴力操作数据库。
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>          
        public async Task<bool> RemoveErrLog(string jobGroup, string jobName)
        {
            //IRepository logRepositorie = RepositoryFactory.CreateRepository(driv, dbProvider);

            //if (logRepositorie == null) return false;

            //await logRepositorie.RemoveErrLogAsync(jobGroup, jobName);

            //var jobKey = new JobKey(jobName, jobGroup);
            //var jobDetail = await scheduler.GetJobDetail(jobKey);
            //jobDetail.JobDataMap[Constant.EXCEPTION] = string.Empty;

            return true;
        }
    }
}
