namespace TestWorkshop.TimeScale;

/// <summary>
/// 遥测任务处理和文件保留配置
/// </summary>
public class WorkshopTelemetryOptions
{
    /// <summary>
    /// 遥测任务记录和对应原始 CSV 文件的统一保留天数。
    /// Task 与 FileObject 使用同一生命周期，到期后由清理 Worker 一起删除。
    /// </summary>
    public int RetentionDays { get; set; } = 180;

    /// <summary>
    /// 过期文件清理执行间隔（分钟）。
    /// </summary>
    public int FileCleanupIntervalMinutes { get; set; } = 60;

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
    }
}
