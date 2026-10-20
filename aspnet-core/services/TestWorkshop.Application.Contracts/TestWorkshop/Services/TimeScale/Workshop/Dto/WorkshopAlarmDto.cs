namespace TestWorkshop;

/// <summary>
/// 报警 DTO
/// </summary>
public class WorkshopAlarmDto : EntityDto<Guid>
{
    /// <summary>
    /// 报警类型
    /// </summary>
    public WorkshopAlarmType AlarmType { get; set; }

    /// <summary>
    /// 报警类型名称
    /// </summary>
    public string AlarmTypeName => AlarmType.ToString();

    /// <summary>
    /// 测试台设备ID
    /// </summary>
    public Guid DeviceId { get; set; }

    /// <summary>
    /// 测试台设备编码
    /// </summary>
    public string DeviceCode { get; set; }

    /// <summary>
    /// 所属车间
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    /// 数据类型/物理量编码（通信报警为空）
    /// </summary>
    public string DataItemType { get; set; }

    /// <summary>
    /// 报警内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 报警成立时间
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public WorkshopAlarmStatus Status { get; set; }

    /// <summary>
    /// 恢复时间
    /// </summary>
    public DateTime? ClearedAt { get; set; }
}
