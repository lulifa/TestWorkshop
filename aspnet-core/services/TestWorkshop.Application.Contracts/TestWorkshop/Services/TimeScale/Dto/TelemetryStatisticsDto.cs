namespace TestWorkshop;

/// <summary>
/// 遥测统计信息 DTO
/// </summary>
public class TelemetryStatisticsDto
{
    /// <summary>
    /// 总文件数
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// 总文件大小（字节）
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// 待处理数
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// 处理中数
    /// </summary>
    public int ProcessingCount { get; set; }

    /// <summary>
    /// 成功数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 总解析记录数
    /// </summary>
    public long TotalRecords { get; set; }

    /// <summary>
    /// 总大小显示（MB）
    /// </summary>
    public decimal TotalSizeMB => Math.Round((decimal)TotalSize / (1024 * 1024), 2);
}