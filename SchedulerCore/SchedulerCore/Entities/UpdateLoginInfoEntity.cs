namespace SchedulerCore.Host.Entities
{
    public class UpdateLoginInfoEntity
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
