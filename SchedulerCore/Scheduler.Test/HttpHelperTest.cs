using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using SchedulerCore.Host.Common.Enum;
using SchedulerCore.Host.Controllers;
using SchedulerCore.Host.Helpers;
using SchedulerCore.Host.Managers;
using SchedulerCore.Host.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Scheduler.Test
{
    public class HttpHlperTest
    {
        private string httpBase = "http://localhost:5002";
        private readonly ILogger<JobController> _logger;
        //private readonly IScheduler _scheduler;
        private SchedulerCenter _schedulerCenter;


        [Fact]
        public async Task TestAddJobAsync()
        {
            var scheduleAddDto = new ScheduleAddDto();
            scheduleAddDto.TriggerType = TriggerTypeEnum.Simple;
            scheduleAddDto.JobName = "JobNameBenny";
            scheduleAddDto.JobGroup = "JobGroupBenny";
            scheduleAddDto.IntervalSecond = 12;
            var addUrl = httpBase + "/api/Job/AddJob";
            //添加测试数据
            var resultStr = await HttpHelper.Instance.PostAsync(addUrl, JsonConvert.SerializeObject(scheduleAddDto));
            var addResult = JsonConvert.DeserializeObject<BasicResult>(resultStr.Content.ReadAsStringAsync().Result);

            //验证
            Assert.True(addResult.Code == 200);

            //删除测试数据
            var key = new JobKey(scheduleAddDto.JobName, scheduleAddDto.JobGroup);
            var delUrl = httpBase + "/api/Job/RemoveJob";
            var delResultStr = await HttpHelper.Instance.PostAsync(delUrl, JsonConvert.SerializeObject(key));
            var delResult = JsonConvert.DeserializeObject<BasicResult>(delResultStr.Content.ReadAsStringAsync().Result);
            Assert.True(delResult.Code == 200);
        }

        [Fact]
        public async Task TestAddJobControllerAsync()
        {
            var controller = new JobController(_logger, _schedulerCenter);
            var scheduleAddDto = new ScheduleAddDto();
            scheduleAddDto.TriggerType = TriggerTypeEnum.Simple;
            scheduleAddDto.JobName = "JobNameBenny";
            scheduleAddDto.JobGroup = "JobGroupBenny";
            scheduleAddDto.IntervalSecond = 12;
            var result = controller.AddJob(scheduleAddDto.ToJson());
            
        }
    }
}
