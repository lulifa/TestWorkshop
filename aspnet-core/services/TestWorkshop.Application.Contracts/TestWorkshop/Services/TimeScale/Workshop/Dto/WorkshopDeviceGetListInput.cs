using TestWorkshop.TimeScale;

namespace TestWorkshop;

/// <summary>
/// 车间设备分页查询输入
/// </summary>
public class WorkshopDeviceGetListInput : PagedAndSortedResultRequestDto, IEnablePaging
{
    /// <summary>
    /// 按编码/名称过滤
    /// </summary>
    public string Filter { get; set; }

    /// <summary>
    /// 按车间组织单元过滤
    /// </summary>
    public Guid? OrganizationUnitId { get; set; }

    /// <summary>
    /// 按设备类型过滤
    /// </summary>
    public DeviceTypeEnum? Type { get; set; }

    /// <summary>
    /// 是否启用分页；true：分页查询，false：查询全部数据
    /// </summary>
    public bool IsPaged { get; set; } = true;
}
