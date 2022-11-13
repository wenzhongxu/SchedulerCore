using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Common.Helpers;
using Scheduler.Common.Helpers.Models;

namespace SchedulerCore.Host.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SetingController : ControllerBase
    {
        private static string refreshIntervalPath = "File/refreshInterval.json";

    }
}
