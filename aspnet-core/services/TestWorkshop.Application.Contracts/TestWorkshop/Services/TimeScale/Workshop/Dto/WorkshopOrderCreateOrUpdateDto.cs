namespace TestWorkshop;

/// <summary>
/// 车间测试订单新增/编辑 DTO
/// </summary>
public class WorkshopOrderCreateOrUpdateDto
{
    /// <summary>
    /// 订单号
    /// </summary>
    [Required]
    [StringLength(64)]
    public string OrderNo { get; set; }

    /// <summary>
    /// 关联设备
    /// </summary>
    [Required]
    public Guid DeviceId { get; set; }

    /// <summary>
    /// 单船项目主管用户ID（必须拥有“单船项目主管”角色）
    /// </summary>
    public Guid ProjectSupervisorUserId { get; set; }

    /// <summary>
    /// 要求车间完成测试时间
    /// </summary>
    public DateTime RequiredCompletionTime { get; set; }
}

public class WorkshopOrderCreateDto : WorkshopOrderCreateOrUpdateDto
{
}

public class WorkshopOrderUpdateDto : WorkshopOrderCreateOrUpdateDto
{
}
