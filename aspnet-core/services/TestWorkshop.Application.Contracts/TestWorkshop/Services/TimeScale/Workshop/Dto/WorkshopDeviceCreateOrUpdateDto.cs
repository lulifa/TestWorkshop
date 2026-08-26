using TestWorkshop.TimeScale;

namespace TestWorkshop;

/// <summary>
/// 车间设备新增/编辑 DTO
/// </summary>
public class WorkshopDeviceCreateOrUpdateDto
{
    /// <summary>
    /// 设备编码
    /// </summary>
    [Required]
    [StringLength(64)]
    public string Code { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceTypeEnum Type { get; set; }

    /// <summary>
    /// 所属车间组织单元
    /// </summary>
    [Required]
    public Guid OrganizationUnitId { get; set; }
}

/// <summary>
/// 车间设备新增 DTO
/// </summary>
public class WorkshopDeviceCreateDto : WorkshopDeviceCreateOrUpdateDto
{
}

/// <summary>
/// 车间设备编辑 DTO
/// </summary>
public class WorkshopDeviceUpdateDto : WorkshopDeviceCreateOrUpdateDto
{
}
