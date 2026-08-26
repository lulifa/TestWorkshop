using TestWorkshop.TimeScale;

namespace TestWorkshop;

/// <summary>
/// 车间设备 DTO
/// </summary>
public class WorkshopDeviceDto : EntityDto<Guid>
{
    /// <summary>
    /// 设备编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceTypeEnum Type { get; set; }

    /// <summary>
    /// 设备类型名称
    /// </summary>
    public string TypeName => Type.ToString();

    /// <summary>
    /// 所属车间组织单元
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }
}
