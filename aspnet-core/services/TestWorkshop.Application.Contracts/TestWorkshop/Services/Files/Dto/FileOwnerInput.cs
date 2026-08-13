namespace TestWorkshop;

/// <summary>
/// 文件业务对象
/// </summary>
public class FileOwnerInput
{
    /// <summary>
    /// 业务类型
    /// </summary>
    [Required]
    public string OwnerType { get; set; }

    /// <summary>
    /// 业务ID
    /// </summary>
    [Required]
    public string OwnerId { get; set; }
}