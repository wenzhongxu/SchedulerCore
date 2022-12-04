using SchedulerCore.Host.Entities;

namespace SchedulerCore.Host.Models.ParamDtos
{
    public class ModifyJobParamDto
    {
        public ScheduleEntity NewScheduleEntity { get; set; }
        public ScheduleEntity OldScheduleEntity { get; set; }
    }
}
