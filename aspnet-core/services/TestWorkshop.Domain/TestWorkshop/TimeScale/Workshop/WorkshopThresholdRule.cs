namespace TestWorkshop.TimeScale;

/// <summary>
/// 阈值规则 - 按设备类型配置，每个数据类型最多一条。
/// 上限/下限均可空（有的物理量只有上限或只有下限，但不能都为空）；
/// 值大于上限视为偏高、低于下限视为偏低。
/// </summary>
public class WorkshopThresholdRule : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceTypeEnum DeviceType { get; private set; }

    /// <summary>
    /// 数据类型（物理量编码，见 <see cref="TelemetryQuantity"/>）
    /// </summary>
    public string DataItemType { get; private set; }

    /// <summary>
    /// 上限阈值（可空）
    /// </summary>
    public decimal? UpperLimit { get; private set; }

    /// <summary>
    /// 下限阈值（可空）
    /// </summary>
    public decimal? LowerLimit { get; private set; }

    /// <summary>
    /// 报警内容（人工填写的说明，如"偏高"）
    /// </summary>
    public string AlarmContent { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; private set; }

    protected WorkshopThresholdRule()
    {
    }

    public WorkshopThresholdRule(
        Guid id,
        DeviceTypeEnum deviceType,
        string dataItemType,
        decimal? upperLimit,
        decimal? lowerLimit,
        string alarmContent,
        bool enabled = true)
        : base(id)
    {
        DeviceType = deviceType;
        DataItemType = dataItemType;
        SetLimits(upperLimit, lowerLimit);
        AlarmContent = alarmContent;
        Enabled = enabled;
    }

    public void Update(decimal? upperLimit, decimal? lowerLimit, string alarmContent, bool enabled)
    {
        SetLimits(upperLimit, lowerLimit);
        AlarmContent = alarmContent;
        Enabled = enabled;
    }

    private void SetLimits(decimal? upperLimit, decimal? lowerLimit)
    {
        if (!upperLimit.HasValue && !lowerLimit.HasValue)
        {
            throw new UserFriendlyException("上限阈值和下限阈值至少填写一个");
        }

        if (upperLimit.HasValue && lowerLimit.HasValue && lowerLimit.Value > upperLimit.Value)
        {
            throw new UserFriendlyException("下限阈值不能大于上限阈值");
        }

        UpperLimit = upperLimit;
        LowerLimit = lowerLimit;
    }
}
