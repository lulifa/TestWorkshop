namespace TestWorkshop;

/// <summary>
/// 遥测任务 DTO
/// </summary>
public class TelemetryTaskDto : EntityDto<long>
{
    /// <summary>
    /// 文件ID
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// 原始文件名
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Blob存储文件名
    /// </summary>
    public string BlobName { get; set; }

    /// <summary>
    /// 处理状态 (0=Pending 1=Processing 2=Success 3=Failed)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 下次重试时间
    /// </summary>
    public DateTime? NextRetryTime { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// 解析的记录数
    /// </summary>
    public int? RecordCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 处理完成时间
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 状态描述
    /// </summary>
    public string StatusName => GetStatusName(Status);

    private string GetStatusName(int status)
    {
        return status switch
        {
            0 => "Pending",
            1 => "Processing",
            2 => "Success",
            3 => "Failed",
            _ => "Unknown"
        };
    }
}