namespace Scheduler.Repositories
{
    public interface IRepository
    {
        Task<int> InitTable();
    }
}
