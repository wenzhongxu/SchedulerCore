using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Entities
{
    public class JobInfoEntity
    {
        /// <summary>
        /// 任务组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 任务信息
        /// </summary>
        public List<JobInfo> JobInfoList { get; set; } = new List<JobInfo>();
    }

    public class JobInfo
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? PreviousFireTime { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 上次执行的异常信息
        /// </summary>
        public string LastErrMsg { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TriggerState TriggerState { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }

        public string DisplayState 
        {
            get
            {
                string state;
                switch (TriggerState)
                {
                    case TriggerState.Normal:
                        state = "正常";
                        break;
                    case TriggerState.Paused:
                        state = "暂停";
                        break;
                    case TriggerState.Complete:
                        state = "完成";
                        break;
                    case TriggerState.Error:
                        state = "异常";
                        break;
                    case TriggerState.Blocked:
                        state = "阻塞";
                        break;
                    case TriggerState.None:
                        state = "不存在";
                        break;
                    default:
                        state = "未知";
                        break;
                }
                return state;
            }
        }

        /// <summary>
        /// 时间间隔
        /// </summary>
        public string Interval { get; set; }

        /// <summary>
        /// 触发地址
        /// </summary>
        public string TriggerAddress { get; set; }

        /// <summary>
        /// 请求类型
        /// </summary>
        public string RequestType { get; set; }

        /// <summary>
        /// 已执行次数
        /// </summary>
        public long RunNumber { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public long JobType { get; set; }
    }
}
