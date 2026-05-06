namespace TestWorkshop.TimeScale;

public class Device : Entity<Guid>, IMultiTenant
{
    public string Code { get; set; }      // NB-FIVA-001，下位机传
    public string Name { get; set; }      // FIVA #1
    public DeviceTypeEnum Type { get; set; }      // FIVA / PUMP
    public Guid OrganizationUnitId { get; set; } // 关联车间

    public Guid? TenantId { get; set; }
}
