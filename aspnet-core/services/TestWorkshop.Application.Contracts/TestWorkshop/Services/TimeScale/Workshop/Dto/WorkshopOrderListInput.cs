namespace TestWorkshop;

/// <summary>
/// 车间测试订单分页查询输入
/// </summary>
public class WorkshopOrderListInput : PagedAndSortedResultRequestDto, IEnablePaging
{
    public string OrderNo { get; set; }

    public Guid? DeviceId { get; set; }

    public WorkshopOrderStatus? Status { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsPaged { get; set; } = true;
}
