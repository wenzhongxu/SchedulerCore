using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Entities
{
    public class SendMail
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public MailEntity MailInfo { get; set; } = null;
    }
}
