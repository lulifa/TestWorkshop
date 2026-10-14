namespace TestWorkshop;

/// <summary>
/// 报警列表查询输入
/// </summary>
public class WorkshopAlarmListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 按报警类型过滤
    /// </summary>
    public WorkshopAlarmType? AlarmType { get; set; }

    /// <summary>
    /// 按状态过滤；不传表示不限
    /// </summary>
    public WorkshopAlarmStatus? Status { get; set; }

    /// <summary>
    /// 按测试台过滤
    /// </summary>
    public Guid? DeviceId { get; set; }

    /// <summary>
    /// 按车间过滤
    /// </summary>
    public Guid? OrganizationUnitId { get; set; }

    /// <summary>
    /// 开始时间（>=）
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间（&lt;=）
    /// </summary>
    public DateTime? EndTime { get; set; }
}
