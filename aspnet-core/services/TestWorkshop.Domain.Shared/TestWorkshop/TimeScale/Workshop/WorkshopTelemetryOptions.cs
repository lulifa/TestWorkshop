namespace TestWorkshop.TimeScale;

/// <summary>
/// 遥测任务处理和文件保留配置
/// </summary>
public class WorkshopTelemetryOptions
{
    /// <summary>
    /// 遥测任务记录、对应原始 CSV 文件和设备遥测超级表数据的统一保留天数。
    /// Task 与 FileObject 到期后由清理 Worker 一起删除；
    /// 超级表数据按时间分块删除。
    /// </summary>
    public int RetentionDays { get; set; } = 1095;

    /// <summary>
    /// 过期文件清理执行间隔（分钟）。
    /// </summary>
    public int FileCleanupIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// 订单匹配时间容差（秒）。默认 0，表示严格按订单开始和结束时间匹配。
    /// </summary>
    public int OrderMatchToleranceSeconds { get; set; } = 0;

    public void Validate()
    {
        if (RetentionDays <= 0)
        {
            throw new ArgumentException("WorkshopTelemetry:RetentionDays 必须大于 0");
        }

        if (FileCleanupIntervalMinutes <= 0)
        {
            throw new ArgumentException("WorkshopTelemetry:FileCleanupIntervalMinutes 必须大于 0");
        }

        if (OrderMatchToleranceSeconds < 0)
        {
            throw new ArgumentException("WorkshopTelemetry:OrderMatchToleranceSeconds 不能小于 0");
        }
    }
}
