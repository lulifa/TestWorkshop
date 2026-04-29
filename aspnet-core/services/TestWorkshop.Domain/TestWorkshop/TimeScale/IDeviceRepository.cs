namespace TestWorkshop.TimeScale;

public interface IDeviceRepository : IBasicRepository<Device, Guid>
{

    Task<Device> FindByCodeAsync(string code);

    Task<List<Device>> GetByOrganizationUnitIdAsync(Guid orgId);

}
