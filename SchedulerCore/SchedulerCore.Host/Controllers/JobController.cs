using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using SchedulerCore.Host.Common.Enum;
using SchedulerCore.Host.Entities;
using SchedulerCore.Host.Helpers;
using SchedulerCore.Host.Managers;
using SchedulerCore.Host.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Controllers
{
    [ApiController]
    [Route("api/[controller]/[Action]")]
    [EnableCors("AllowSameDomain")] //允许跨域 
    public class JobController : ControllerBase
    {
        private readonly ILogger<JobController> _logger;
        //private readonly IScheduler _scheduler;
        private SchedulerCenter _schedulerCenter;

        public JobController(ILogger<JobController> logger, SchedulerCenter schedulerCenter)
        {
            _logger = logger;
            _schedulerCenter = schedulerCenter;
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<SchedulerDto>>> GetAllJob()
        {
            var jobs = await _schedulerCenter.GetAllJobAsync();
            return Ok(jobs);
        }

        [HttpPost]
        public async Task<IActionResult> AddJob(string scheduleData)
        {
            ScheduleAddDto schedule = new ScheduleAddDto();
            string scheduleDataStr = "";
            if (Request.Form.ContainsKey("scheduleData"))
            {
                scheduleDataStr = Request.Form["scheduleData"];
            }
            if (Request.Query.ContainsKey("scheduleData"))
            {
                scheduleDataStr = Request.Query["scheduleData"];
            }
            schedule = JsonHelper.Deserialize<ScheduleAddDto>(scheduleDataStr);

            var jobKey = new JobKey(schedule.JobName, schedule.JobGroup);

            if (schedule.TriggerType == TriggerTypeEnum.Simple &&
                    schedule.IntervalSecond.HasValue &&
                    schedule.IntervalSecond <= 10)
            {
                return BadRequest("当前环境不允许低于10秒内循环执行任务！");
            }
            else if (schedule.TriggerType == TriggerTypeEnum.Cron &&
                     schedule.Cron == "* * * * * ?")
            {
                return BadRequest("当前环境不允许过频繁执行任务！");
            }

            //if (await _scheduler.CheckExists(jobKey))
            //{
            //    return BadRequest("任务已存在！");
            //}

            await _schedulerCenter.AddScheduleJobAsync(schedule);
            return Ok();
        }
    }
}
