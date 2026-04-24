namespace TestWorkshop;

public class TenantDto : ExtensibleAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; }

    public string NormalizedName { get; set; }

    public string ConcurrencyStamp { get; set; }

}