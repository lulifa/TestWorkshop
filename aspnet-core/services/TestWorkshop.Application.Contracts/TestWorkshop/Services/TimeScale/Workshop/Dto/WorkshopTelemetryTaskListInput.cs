namespace TestWorkshop;

public class WorkshopTelemetryTaskListInput : PagedAndSortedResultRequestDto, IEnablePaging
{
    /// <summary>
    /// 订单ID过滤
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// 订单号过滤（模糊匹配）
    /// </summary>
    public string OrderNo { get; set; }

    /// <summary>
    /// 文件名过滤
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 被测设备编码过滤（匹配上传参数里的 TestInfo.TestedDeviceCode）
    /// </summary>
    public string TestedDeviceCode { get; set; }

    /// <summary>
    /// 是否关联订单过滤。true：只看已匹配订单；false：只看未匹配订单（非法数据）；null：全部
    /// </summary>
    public bool? HasOrder { get; set; }

    /// <summary>
    /// 状态过滤。0=待处理，1=处理中，2=成功（不含已替换），3=失败，4=已替换（界面虚拟状态）
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 创建时间开始
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 创建时间结束
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 是否启用分页；true：分页查询，false：查询全部数据
    /// </summary>
    public bool IsPaged { get; set; } = true;

}
