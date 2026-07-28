namespace TestWorkshop;

public class WorkshopTelemetryTaskListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 文件名过滤
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 状态过滤 (0/1/2/3)
    /// </summary>
    public int? Status { get; set; }

}
