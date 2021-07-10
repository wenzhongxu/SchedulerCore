using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Simpl;
using Quartz.Util;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Common.Enum;
using SchedulerCore.Host.Entities;
using SchedulerCore.Host.Jobs;
using SchedulerCore.Host.Models;
using SchedulerCore.Host.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Managers
{
    /// <summary>
    /// 任务调度中心 单例 采用 DirectSchedulerFactory
    /// </summary>
    public class SchedulerCenter
    {
        private IDbProvider dbProvider;
        private string delegateType;
        private IScheduler scheduler;

        public SchedulerCenter()
        {
            InitDriverDelegateType();
            dbProvider = new DbProvider(AppConfig.DbProviderName, AppConfig.ConnectionString);
        }

        /// <summary>
        /// 初始化数据库类型
        /// </summary>
        private void InitDriverDelegateType()
        {
            var dbProdierName = AppConfig.DbProviderName;
            delegateType = dbProdierName switch
            {
                "OracleODPManaged" => typeof(OracleDelegate).AssemblyQualifiedName,
                "MySql" => typeof(MySQLDelegate).AssemblyQualifiedName,
                "SqlServer" => typeof(SqlServerDelegate).AssemblyQualifiedName,
                _ => typeof(OracleDelegate).AssemblyQualifiedName,
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
                DBConnectionManager.Instance.AddConnectionProvider("XCRMS", dbProvider);

                var serializer = new JsonObjectSerializer();
                serializer.Initialize();

                var jobStore = new JobStoreTX
                {
                    DataSource = "XCRMS",
                    TablePrefix = "QRTZ_",
                    InstanceId = "AUTO",
                    DriverDelegateType = delegateType,
                    ObjectSerializer = serializer
                };

                DirectSchedulerFactory.Instance.CreateScheduler("XCRMSScheduler", "AUTO", new DefaultThreadPool(), jobStore);
                scheduler = await SchedulerRepository.Instance.Lookup("XCRMSScheduler");
            }
        }

        /// <summary>
        /// 启动任务调度
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartScheduleAsync()
        {
            await InitSchedulerAsync();

            if (scheduler.InStandbyMode)
            {
                await scheduler.Start();
                Console.WriteLine("任务调度已启动");
            }
            return scheduler.InStandbyMode;
        }

        #region for webapi
        public async Task<List<SchedulerDto>> GetAllJobAsync()
        {
            List<JobKey> jobKeyList = new();
            List<JobInfoEntity> jobInfoList = new();
            List<SchedulerDto> jobInfoDtos = new();

            var groupNames = await scheduler.GetJobGroupNames();
            foreach (var groupName in groupNames.OrderBy(t => t))
            {
                jobKeyList.AddRange(await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
                jobInfoList.Add(new JobInfoEntity() { GroupName = groupName });
            }

            foreach (var jobKey in jobKeyList.OrderBy(t => t.Name))
            {
                var jobDetail = await scheduler.GetJobDetail(jobKey);
                var triggersList = await scheduler.GetTriggersOfJob(jobKey);
                var triggers = triggersList.AsEnumerable().FirstOrDefault();

                string interval;
                if (triggers is SimpleTriggerImpl)
                {
                    interval = (triggers as SimpleTriggerImpl)?.RepeatInterval.ToString();
                }
                else
                {
                    interval = (triggers as CronTriggerImpl)?.CronExpressionString;
                }

                foreach (var jobInfo in jobInfoList)
                {
                    if (jobInfo.GroupName == jobKey.Group)
                    {
                        var jobType = (JobTypeEnum)jobDetail.JobDataMap.GetLong(Constant.JobTypeEnum);
                        jobType = jobType == JobTypeEnum.None ? JobTypeEnum.Url : jobType;

                        var triggerAddress = string.Empty;
                        if (jobType == JobTypeEnum.Url)
                        {
                            triggerAddress = jobDetail.JobDataMap.GetString(Constant.RequestUrl);
                        }
                        else if (jobType == JobTypeEnum.Email)
                        {
                            triggerAddress = jobDetail.JobDataMap.GetString(Constant.MailTo);
                        }
                        else if (jobType == JobTypeEnum.Mqtt)
                        {
                            triggerAddress = jobDetail.JobDataMap.GetString(Constant.Topic);
                        }
                        else if (jobType == JobTypeEnum.RabbitMQ)
                        {
                            triggerAddress = jobDetail.JobDataMap.GetString(Constant.RabbitQueue);
                        }

                        jobInfo.JobInfoList.Add(new JobInfo()
                        {
                            Name = jobKey.Name,
                            LastErrMsg = jobDetail.JobDataMap.GetString(Constant.Exception),
                            TriggerAddress = triggerAddress,
                            TriggerState = await scheduler.GetTriggerState(triggers.Key),
                            PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                            BeginTime = triggers.StartTimeUtc.LocalDateTime,
                            Interval = interval,
                            EndTime = triggers.EndTimeUtc?.LocalDateTime,
                            Description = triggers.Description,
                            RequestType = triggers.JobDataMap.GetString(Constant.RequestType),
                            RunNumber = triggers.JobDataMap.GetLong(Constant.RunNumber),
                            JobType = (long)jobType
                        });

                        jobInfoDtos.Add(new SchedulerDto()
                        {
                            GroupName = jobInfo.GroupName,
                            Name = jobKey.Name,
                            LastErrMsg = jobDetail.JobDataMap.GetString(Constant.Exception),
                            TriggerAddress = triggerAddress,
                            TriggerState = await scheduler.GetTriggerState(triggers.Key),
                            PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                            BeginTime = triggers.StartTimeUtc.LocalDateTime,
                            Interval = interval,
                            EndTime = triggers.EndTimeUtc?.LocalDateTime,
                            Description = triggers.Description,
                            RequestType = triggers.JobDataMap.GetString(Constant.RequestType),
                            RunNumber = triggers.JobDataMap.GetLong(Constant.RunNumber),
                            JobType = (long)jobType
                        });
                    }
                }
            }

            return jobInfoDtos;
        }

        public async Task AddScheduleJobAsync(ScheduleAddDto entity, long? runNumber = null)
        {
            //请求配置
            var dataMapDir = new Dictionary<string, string>()
            {
                { Constant.EndAt, entity.EndTime.ToString() },
                { Constant.JobTypeEnum, ((int)entity.JobType).ToString()},
                { Constant.MailMessage, ((int)entity.MailMessage).ToString()}
            };

            if (runNumber.HasValue)
            {
                dataMapDir.Add(Constant.RunNumber, runNumber.ToString());
            }

            IJobConfigurator jobConfigurator = null;
            if (entity.JobType == JobTypeEnum.Url)
            {
                jobConfigurator = JobBuilder.Create<HttpJob>();
                dataMapDir.Add(Constant.RequestUrl, entity.RequestUrl);
                dataMapDir.Add(Constant.RequestType, ((int)entity.RequestType).ToString());
                dataMapDir.Add(Constant.Headers, entity.Headers);
                dataMapDir.Add(Constant.RequestParameters, entity.RequestParameters);
            }

            IJobDetail jobDetail = jobConfigurator
                .SetJobData(new JobDataMap(dataMapDir))
                .WithDescription(entity.Description)
                .WithIdentity(entity.JobName, entity.JobGroup)
                .Build();

            ITrigger trigger = CreateSimpleTrigger(entity);

            await scheduler.ScheduleJob(jobDetail, trigger);

        }

        private ITrigger CreateSimpleTrigger(ScheduleAddDto entity)
        {
            if (entity.RunTimes.HasValue && entity.RunTimes > 0)
            {
                return TriggerBuilder.Create()
                    .WithIdentity(entity.JobName, entity.JobGroup)
                    .StartAt(entity.BeginTime)
                    .WithSimpleSchedule(p =>
                    {
                        p.WithIntervalInSeconds(entity.IntervalSecond.Value)
                        .WithRepeatCount(entity.RunTimes.Value)
                        .WithMisfireHandlingInstructionFireNow();
                    })
                    .ForJob(entity.JobName, entity.JobGroup)
                    .Build();
            }
            else
            {
                return TriggerBuilder.Create()
                    .WithIdentity(entity.JobName, entity.JobGroup)
                    .StartAt(entity.BeginTime)
                    .WithSimpleSchedule(p =>
                    {
                        p.WithIntervalInSeconds(entity.IntervalSecond.Value)
                        .RepeatForever()
                        .WithMisfireHandlingInstructionFireNow();
                    })
                    .ForJob(entity.JobName, entity.JobGroup)
                    .Build();
            }
        }


        #endregion

    }
}
