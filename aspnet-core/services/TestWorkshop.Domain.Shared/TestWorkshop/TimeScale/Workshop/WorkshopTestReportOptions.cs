namespace TestWorkshop.TimeScale;

/// <summary>
/// 测试报告外部接口获取配置。
/// </summary>
public class WorkshopTestReportOptions
{
    /// <summary>
    /// 是否启用自动获取。外部接口未配置时保持关闭。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 使用内置模拟适配器。仅用于接口未完成时联调页面和状态流转。
    /// </summary>
    public bool UseMock { get; set; }

    /// <summary>
    /// Worker 扫描间隔（秒）。
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// 首次重试等待时间（秒）。
    /// </summary>
    public int MinRetryDelaySeconds { get; set; } = 300;

    /// <summary>
    /// 最大重试等待时间（秒）。
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 3600;

    /// <summary>
    /// 单订单最大请求次数。
    /// </summary>
    public int MaxAttempts { get; set; } = 60;

    /// <summary>
    /// 测试结束后最长自动获取天数。
    /// </summary>
    public int MaxFetchDaysAfterTest { get; set; } = 30;

    /// <summary>
    /// 单次获取的最大报告文件大小（字节）。
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 200 * 1024 * 1024;

    /// <summary>
    /// 处理中记录的超时时间（分钟）。
    /// </summary>
    public int StaleProcessingMinutes { get; set; } = 10;

    /// <summary>
    /// 每轮最多处理的订单数量。
    /// </summary>
    public int BatchSize { get; set; } = 5;

    public void Validate()
    {
        if (PollingIntervalSeconds <= 0)
        {
            throw new ArgumentException("WorkshopTestReport:PollingIntervalSeconds 必须大于 0");
        }

        if (MinRetryDelaySeconds <= 0 || MaxRetryDelaySeconds < MinRetryDelaySeconds)
        {
            throw new ArgumentException("WorkshopTestReport 重试等待时间配置无效");
        }

        if (MaxAttempts <= 0 || MaxFetchDaysAfterTest <= 0 || BatchSize <= 0)
        {
            throw new ArgumentException("WorkshopTestReport 次数、天数和批量大小必须大于 0");
        }

        if (MaxFileSizeBytes <= 0 || StaleProcessingMinutes <= 0)
        {
            throw new ArgumentException("WorkshopTestReport 文件大小和超时时间必须大于 0");
        }
    }

    public TimeSpan GetRetryDelay(int attemptNo)
    {
        var safeAttempt = Math.Max(attemptNo, 1);
        var multiplier = Math.Pow(2, Math.Min(safeAttempt - 1, 10));
        var seconds = Math.Min(
            MinRetryDelaySeconds * multiplier,
            MaxRetryDelaySeconds);
        return TimeSpan.FromSeconds(seconds);
    }
}
