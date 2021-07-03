using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Common
{
    public class Constant
    {
        /// <summary>
        /// 请求url requesturl
        /// </summary>
        public const string RequestUrl = "RequestUrl";

        /// <summary>
        /// 请求参数
        /// </summary>
        public const string RequestParameters = "RequestParameters";

        /// <summary>
        /// 请求头
        /// </summary>
        public const string Headers = "Headers";

        /// <summary>
        /// 是否发邮件
        /// </summary>
        public const string MailMessage = "MailMessage";

        /// <summary>
        /// 请求类型
        /// </summary>
        public const string RequestType = "RequestType";

        /// <summary>
        /// 日志
        /// </summary>
        public const string LogList = "LogList";

        /// <summary>
        /// 异常
        /// </summary>
        public const string Exception = "Exception";

        public const string EndAt = "EndAt";

        /// <summary>
        /// 执行次数
        /// </summary>
        public const string RunNumber = "RunNumber";

        public const string JobTypeEnum = "JobTypeEnum";

        public const string MailTitle = "MailTitle";

        public const string MailContent = "MailContent";

        public const string MailTo = "MailTo";

        public const string Topic = "Topic";

        public const string Payload = "payload";

        public const string RabbitQueue = "RabbitQueue";

        public const string RabbitBody = "RabbitBody";
    }
}
