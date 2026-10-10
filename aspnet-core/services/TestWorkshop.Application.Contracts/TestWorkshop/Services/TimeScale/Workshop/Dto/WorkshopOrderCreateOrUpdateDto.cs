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
}

public class WorkshopOrderCreateDto : WorkshopOrderCreateOrUpdateDto
{
}

public class WorkshopOrderUpdateDto : WorkshopOrderCreateOrUpdateDto
{
}
