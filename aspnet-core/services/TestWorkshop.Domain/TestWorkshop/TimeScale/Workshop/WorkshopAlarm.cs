namespace TestWorkshop.TimeScale;

/// <summary>
/// 报警记录（一条进行中/已恢复的报警）。
/// 报警主体是测试台（WorkshopDevice）；被测设备只是上下文，不参与归属。
/// </summary>
public class WorkshopAlarm : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// 报警类型
    /// </summary>
    public WorkshopAlarmType AlarmType { get; private set; }

    /// <summary>
    /// 测试台设备ID
    /// </summary>
    public Guid DeviceId { get; private set; }

    /// <summary>
    /// 测试台设备编码（冗余，便于展示）
    /// </summary>
    public string DeviceCode { get; private set; }

    /// <summary>
    /// 测试台所属车间（报警可见性按此过滤）
    /// </summary>
    public Guid OrganizationUnitId { get; private set; }

    /// <summary>
    /// 数据类型/物理量编码；不区分时存空字符串（唯一键需要非空）
    /// </summary>
    public string DataItemType { get; private set; }

    /// <summary>
    /// 报警内容
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// 报警成立时间（条件开始满足的时刻，升级时长从这里算）
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// 已通知级别：0=未通知，1/2/3 对应 5/10/20 分钟
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// 最近一次通知时间
    /// </summary>
    public DateTime? LastNotifiedAt { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public WorkshopAlarmStatus Status { get; private set; }

    /// <summary>
    /// 恢复时间
    /// </summary>
    public DateTime? ClearedAt { get; private set; }

    protected WorkshopAlarm()
    {
    }

    public WorkshopAlarm(
        Guid id,
        WorkshopAlarmType alarmType,
        Guid deviceId,
        string deviceCode,
        Guid organizationUnitId,
        string dataItemType,
        string content,
        DateTime startedAt)
        : base(id)
    {
        AlarmType = alarmType;
        DeviceId = deviceId;
        DeviceCode = deviceCode;
        OrganizationUnitId = organizationUnitId;
        DataItemType = dataItemType ?? string.Empty;
        Content = content;
        StartedAt = startedAt;
        Level = 0;
        Status = WorkshopAlarmStatus.Active;
    }

    /// <summary>
    /// 刷新报警内容，返回是否真的发生了变化。
    /// 没变化就不要落库——否则每次无意义的 UPDATE 都会刷新并发戳，跟升级流程打架。
    /// </summary>
    public bool RefreshContent(string content)
    {
        if (!content.IsNullOrWhiteSpace() && Content != content)
        {
            Content = content;
            return true;
        }

        return false;
    }
}
