namespace TestWorkshop;

public class OrganizationUnitDto : ExtensibleAuditedEntityDto<Guid>
{
    public Guid? ParentId { get; set; }
    public string Code { get; set; }
    public string DisplayName { get; set; }
}
