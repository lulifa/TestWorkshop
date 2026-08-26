namespace TestWorkshop.TimeScale;

public interface IWorkshopDeviceRepository : IBasicRepository<WorkshopDevice, Guid>
{
    Task<WorkshopDevice> FindByCodeAsync(string code);

    Task<IQueryable<WorkshopDevice>> GetQueryableAsync();

}
