namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备实体 - 用于存储下位机上传的设备基础信息
/// </summary>
public class WorkshopDevice : Entity<Guid>, IMultiTenant
{
    public string Code { get; set; }      // FIVA-001，下位机传
    public string Name { get; set; }      // FIVA #1
    public DeviceTypeEnum Type { get; set; }      // FIVA / PUMP
    public Guid OrganizationUnitId { get; protected set; } // 关联车间
    public Guid? TenantId { get; set; }

    public WorkshopDevice()
    {

    }

}
