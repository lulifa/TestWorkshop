namespace TestWorkshop.TimeScale;

/// <summary>
/// 测试台实时数据保留与清理配置。
/// </summary>
public class WorkshopRealtimeDataOptions
{
    public const string SectionName = "WorkshopRealtimeData";

    /// <summary>
    /// 实时数据保留月数。
    /// </summary>
    public int RetentionMonths { get; set; } = 3;

    /// <summary>
    /// 过期数据清理执行间隔（分钟）。
    /// </summary>
    public int CleanupIntervalMinutes { get; set; } = 60;

    public void Validate()
    {
        if (RetentionMonths <= 0)
        {
            throw new ArgumentException(
                "WorkshopRealtimeData:RetentionMonths 必须大于 0");
        }

        if (CleanupIntervalMinutes <= 0)
        {
            throw new ArgumentException(
                "WorkshopRealtimeData:CleanupIntervalMinutes 必须大于 0");
        }
    }
}
