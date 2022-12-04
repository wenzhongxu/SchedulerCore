using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Common.Helpers.Enums
{
    public enum ConnectionMethod
    {
        None = 0,
        TCP = 1,
        TCP_SSL = 2,
        WS = 3,
        WSS = 4,
    }
}
