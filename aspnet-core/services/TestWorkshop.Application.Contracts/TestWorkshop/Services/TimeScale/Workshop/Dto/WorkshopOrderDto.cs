namespace TestWorkshop;

/// <summary>
/// 车间测试订单 DTO
/// </summary>
public class WorkshopOrderDto : EntityDto<Guid>
{
    public string OrderNo { get; set; }

    public Guid DeviceId { get; set; }

    public string DeviceCode { get; set; }

    public string DeviceName { get; set; }

    /// <summary>
    /// 单船项目主管用户ID
    /// </summary>
    public Guid ProjectSupervisorUserId { get; set; }

    /// <summary>
    /// 单船项目主管姓名
    /// </summary>
    public string ProjectSupervisorName { get; set; }

    /// <summary>
    /// 要求车间完成测试时间
    /// </summary>
    public DateTime RequiredCompletionTime { get; set; }

    public WorkshopOrderStatus Status { get; set; }

    public string StatusName => Status switch
    {
        WorkshopOrderStatus.Draft => "已录入",
        WorkshopOrderStatus.Testing => "测试中",
        WorkshopOrderStatus.Completed => "已完成",
        WorkshopOrderStatus.Cancelled => "已取消",
        _ => "未知"
    };

    public DateTime? TestStartedAt { get; set; }

    public DateTime? TestEndedAt { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}
