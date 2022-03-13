using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Helpers
{
    public class BasicResult
    {
        public int Code { get; set; } = 200;
        public string msg { get; set; }
    }
}
