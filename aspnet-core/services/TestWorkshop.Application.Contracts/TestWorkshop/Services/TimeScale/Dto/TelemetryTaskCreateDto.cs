namespace TestWorkshop;

/// <summary>
/// 创建遥测任务 DTO
/// </summary>
public class TelemetryTaskCreateDto
{
    /// <summary>
    /// 原始文件名
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string FileName { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 过期天数（默认7天）
    /// </summary>
    public int ExpirationDays { get; set; } = 7;
}