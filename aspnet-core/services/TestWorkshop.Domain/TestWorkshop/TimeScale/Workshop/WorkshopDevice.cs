namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备实体 - 用于存储下位机上传的设备基础信息
/// </summary>
public class WorkshopDevice : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 设备编码（如 FIVA-001，下位机传）
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 设备名称（如 FIVA #1）
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 设备类型（大类，如 FIVA / PUMP）
    /// </summary>
    public DeviceTypeEnum Type { get; set; }

    /// <summary>
    /// 测试台型号（如 NAB-II-60，系统初始化固定死，人工维护）
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// 关联车间
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public Guid? TenantId { get; set; }

    public WorkshopDevice()
    {
    }
}
