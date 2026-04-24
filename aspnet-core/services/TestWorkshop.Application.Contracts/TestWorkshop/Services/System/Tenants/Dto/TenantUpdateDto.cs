namespace TestWorkshop;
public class TenantUpdateDto : TenantCreateOrUpdateBase, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}