namespace TestWorkshop;

public class TenantConnectionStringCheckResultDto
{
    public bool Connected { get; set; }
    public TenantConnectionStringCheckResultDto()
    {

    }

    public TenantConnectionStringCheckResultDto(bool connected)
    {
        Connected = connected;
    }
}
