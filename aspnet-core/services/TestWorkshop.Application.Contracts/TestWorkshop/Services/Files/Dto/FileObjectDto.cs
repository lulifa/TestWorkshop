namespace TestWorkshop;

/// <summary>
/// 文件对象 DTO
/// </summary>
public class FileObjectDto
{
    /// <summary>
    /// 文件ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件类型（MIME）
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// 业务ID
    /// </summary>
    public string OwnerId { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public string OwnerType { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 文件大小（格式化显示）
    /// </summary>
    public string FileSizeText => FormatFileSize(FileSize);

    private string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
    }
}
