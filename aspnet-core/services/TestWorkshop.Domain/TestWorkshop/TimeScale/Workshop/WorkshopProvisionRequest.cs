namespace TestWorkshop.TimeScale;

/// <summary>
/// 车间初始化请求。种子数据可传固定ID，页面初始化时ID留空自动生成。
/// 未指定父组织单元时，车间将直接创建为根组织单元。
/// </summary>
public class WorkshopProvisionRequest
{
    public Guid? OrganizationUnitId { get; set; }

    public Guid? ParentOrganizationUnitId { get; set; }

    public Guid? SupervisorUserId { get; set; }

    public Guid? RegionalManagerUserId { get; set; }

    public Guid? TesterUserId { get; set; }

    public string DisplayName { get; set; }

    public string BusinessCode { get; set; }

    public string AccountPrefix { get; set; }

    public string InitialPassword { get; set; }
}
