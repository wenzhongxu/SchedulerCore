using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchedulerCore.Host.Entities;
using SchedulerCore.Host.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Controllers
{
    [ApiController]
    [Route("api/job")]
    public class JobController : ControllerBase
    {
        private readonly ILogger<JobController> _logger;
        private SchedulerCenter _scheduler;

        public JobController(ILogger<JobController> logger, SchedulerCenter scheduler)
        {
            _logger = logger;
            _scheduler = scheduler;
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<JobInfoEntity>>> GetAllJob()
        {
            var jobs = await _scheduler.GetAllJobAsync();
            return Ok(jobs);
        }
    }
}
