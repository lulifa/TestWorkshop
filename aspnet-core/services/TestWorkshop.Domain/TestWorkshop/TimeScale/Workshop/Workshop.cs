namespace TestWorkshop.TimeScale;

public class Workshop : FullAuditedAggregateRoot<Guid>, IMultiTenant
{

    /// <summary>
    /// 车间编码（下位机上传使用）
    /// 例如：NB-A01
    /// </summary>
    public string Code { get; protected set; }

    /// <summary>
    /// 车间名称
    /// 例如：宁波一号车间
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// 所属组织机构（公司）
    /// 用于权限归属
    /// </summary>
    public Guid? OrganizationUnitId { get; protected set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public Guid? TenantId { get; protected set; }

    public Workshop()
    {

    }

}
