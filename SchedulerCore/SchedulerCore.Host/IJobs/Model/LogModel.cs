using Newtonsoft.Json;

namespace SchedulerCore.Host.IJobs
{
    public abstract class LogModel
    {
        /// <summary>
        /// 开始执行时间
        /// </summary>
        [JsonIgnore]
        public string BeginTime { get; set; }

        /// <summary>
        /// 结束执行时间
        /// </summary>
        [JsonIgnore]
        public string EndTime { get; set; }

        /// <summary>
        /// 耗时(秒)
        /// </summary>
        [JsonIgnore]
        public string ExecuteTime { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 异常消息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
