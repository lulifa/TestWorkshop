namespace TestWorkshop;

public class WorkshopTelemetryTaskListInput : PagedAndSortedResultRequestDto, IEnablePaging
{
    /// <summary>
    /// 文件名过滤
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 状态过滤 (0/1/2/3)
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 是否启用分页；true：分页查询，false：查询全部数据
    /// </summary>
    public bool IsPaged { get; set; } = true;

}
