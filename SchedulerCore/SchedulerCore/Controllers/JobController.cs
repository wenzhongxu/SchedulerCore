using Microsoft.AspNetCore.Mvc;
using Quartz;
using SchedulerCore.Host.Common.Enums;
using SchedulerCore.Host.Entities;
using SchedulerCore.Host.Managers;
using SchedulerCore.Host.Models;
using SchedulerCore.Host.Models.ParamDtos;

namespace SchedulerCore.Host.Controllers
{
    [Route("api/[controller]/[action]")]
    public class JobController : Controller
    {
        private readonly SchedulerCenter _schedulerCenter;

        public JobController(SchedulerCenter schedulerCenter)
        {
            _schedulerCenter = schedulerCenter;
        }

        /// <summary>
        /// 查询任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ScheduleEntity> QueryJob([FromBody] JobKey job)
        {
            return await _schedulerCenter.QueryJobAsync(job.Group, job.Name);
        }


        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddJob([FromBody] ScheduleEntity entity)
        {
            if (Common.ConfigurationManager.GetTryConfig("EnvironmentalRestrictions", "false") == "true")
            {
                if (entity.TriggerType == TriggerTypeEnum.Simple &&
                    entity.IntervalSecond.HasValue &&
                    entity.IntervalSecond <= 10)
                {
                    return BadRequest("当前环境不允许低于10秒内循环执行任务");
                }
                else if (entity.TriggerType == TriggerTypeEnum.Cron &&
                         entity.Cron == "* * * * * ?")
                {
                    return BadRequest("当前环境不允许过频繁执行任务");
                }

            }

            //检查任务是否已存在
            var jobKey = new JobKey(entity.JobName, entity.JobGroup);
            if (await _schedulerCenter.CheckExists(jobKey))
            {
               return NoContent();
            }
            
            await _schedulerCenter.AddScheduleJobAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> TriggerJob([FromBody] JobKey job)
        {
            await _schedulerCenter.TriggerJobAsync(job);
            return true;
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<JobInfoDto>> GetAllJob()
        {
            return await _schedulerCenter.GetAllJobAsync();
        }

        /// <summary>
        /// 获取所有Job信息（简要信息 - 刷新数据的时候使用）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<JobBriefInfoDto>> GetAllJobBriefInfo()
        {
            return await _schedulerCenter.GetAllJobBriefInfoAsync();
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<BaseResultDto> StopJob([FromBody] JobKey job)
        {
            return await _schedulerCenter.StopOrDelScheduleJobAsync(job.Group, job.Name);
        }

        /// <summary>
        /// 删除任务
        /// </summary> 
        /// <returns></returns>
        [HttpPost]
        public async Task<BaseResultDto> RemoveJob([FromBody] JobKey job)
        {
            return await _schedulerCenter.StopOrDelScheduleJobAsync(job.Group, job.Name, true);
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary> 
        /// <returns></returns>
        [HttpPost]
        public async Task<BaseResultDto> ResumeJob([FromBody] JobKey job)
        {
            return await _schedulerCenter.ResumeJobAsync(job.Group, job.Name);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<BaseResultDto> ModifyJob([FromBody] ModifyJobParamDto entity)
        {
            if (Common.ConfigurationManager.GetTryConfig("EnvironmentalRestrictions", "false") == "true")
            {

                if (entity.NewScheduleEntity.TriggerType == TriggerTypeEnum.Simple &&
                    entity.NewScheduleEntity.IntervalSecond.HasValue &&
                    entity.NewScheduleEntity.IntervalSecond <= 10)
                {
                    return new BaseResultDto()
                    {
                        Code = 403,
                        Msg = "当前环境不允许低于10秒内循环执行任务！"
                    };
                }
                else if (entity.NewScheduleEntity.TriggerType == TriggerTypeEnum.Cron &&
                    entity.NewScheduleEntity.Cron == "* * * * * ?")
                {
                    return new BaseResultDto()
                    {
                        Code = 403,
                        Msg = "当前环境不允许过频繁执行任务！"
                    };
                }
            }

            var jobKey = new JobKey(entity.OldScheduleEntity.JobName, entity.OldScheduleEntity.JobGroup);
            var runNumber = await _schedulerCenter.GetRunNumberAsync(jobKey);
            await _schedulerCenter.StopOrDelScheduleJobAsync(entity.OldScheduleEntity.JobGroup, entity.OldScheduleEntity.JobName, true);
            await _schedulerCenter.AddScheduleJobAsync(entity.NewScheduleEntity, runNumber);
            return new BaseResultDto() { Msg = "修改计划任务成功！" };
        }

        /// <summary>
        /// 获取job日志
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<List<string>> GetJobLogs([FromBody] JobKey jobKey)
        {
            var logs = await _schedulerCenter.GetJobLogsAsync(jobKey);
            logs?.Reverse();
            return logs;
        }

        /// <summary>
        /// 启动调度
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> StartSchedule()
        {
            return await _schedulerCenter.StartSchedulerAsync();
        }

        /// <summary>
        /// 停止调度
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> StopSchedule()
        {
            return await _schedulerCenter.StopSchedulerAsync();
        }

        /// <summary>
        /// 移除异常信息
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> RemoveErrLog([FromBody] JobKey jobKey)
        {
            return await _schedulerCenter.RemoveErrLog(jobKey.Group, jobKey.Name);
        }
    }
}
