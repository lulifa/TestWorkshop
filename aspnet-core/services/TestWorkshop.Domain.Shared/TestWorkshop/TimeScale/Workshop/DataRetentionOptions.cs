namespace TestWorkshop.TimeScale;

/// <summary>
/// 审计日志、安全日志和已恢复报警的保留与清理配置。
/// </summary>
public class DataRetentionOptions
{
    public const string SectionName = "DataRetention";

    /// <summary>
    /// 清理任务执行间隔（小时）
    /// </summary>
    public int CleanupIntervalHours { get; set; } = 24;

    /// <summary>
    /// 每批查询和删除的记录数量
    /// </summary>
    public int CleanupBatchSize { get; set; } = 1000;

    /// <summary>
    /// 审计日志保留天数
    /// </summary>
    public int AuditLogDays { get; set; } = 365;

    /// <summary>
    /// 安全日志保留天数
    /// </summary>
    public int SecurityLogDays { get; set; } = 365;

    /// <summary>
    /// 已恢复报警保留天数；活动报警不受影响
    /// </summary>
    public int ClearedAlarmDays { get; set; } = 180;

    public void Validate()
    {
        if (CleanupIntervalHours <= 0)
        {
            throw new ArgumentException(
                "DataRetention:CleanupIntervalHours 必须大于 0");
        }

        if (CleanupBatchSize <= 0)
        {
            throw new ArgumentException(
                "DataRetention:CleanupBatchSize 必须大于 0");
        }

        if (AuditLogDays <= 0
            || SecurityLogDays <= 0
            || ClearedAlarmDays <= 0)
        {
            throw new ArgumentException(
                "DataRetention 的保留天数必须大于 0");
        }
    }
}
