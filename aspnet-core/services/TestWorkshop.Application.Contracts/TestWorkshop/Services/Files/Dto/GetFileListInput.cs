namespace TestWorkshop;

/// <summary>
/// 文件列表查询输入
/// </summary>
public class GetFileListInput : PagedResultRequestDto, IEnablePaging
{
    /// <summary>
    /// 关键字
    /// </summary>
    public string Keyword { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public string OwnerType { get; set; }

    /// <summary>
    /// 业务ID
    /// </summary>
    public string OwnerId { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    public bool IsPaged { get; set; } = true;

}
