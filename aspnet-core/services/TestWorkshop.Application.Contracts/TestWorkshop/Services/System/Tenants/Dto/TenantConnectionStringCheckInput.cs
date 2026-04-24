namespace TestWorkshop;

public class TenantConnectionStringCheckInput
{
    [Required]
    public string Provider { get; set; }

    public string Name { get; set; }

    [Required]
    [DisableAuditing]
    [DynamicStringLength(typeof(TenantConnectionStringConsts), nameof(TenantConnectionStringConsts.MaxValueLength))]
    public string ConnectionString { get; set; }
}
