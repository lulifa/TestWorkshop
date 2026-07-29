namespace TestWorkshop;


/// <summary>
/// 登录日志
/// </summary>
[Route("api/system/securitylog")]
public class SystemSecurityLogController : TestWorkshopController, ISystemSecurityLogAppService
{

    protected ISystemSecurityLogAppService Service { get; }

    public SystemSecurityLogController(ISystemSecurityLogAppService service)
    {
        Service = service;
    }

    [HttpGet]
    public Task<PagedResultDto<IdentitySecurityLogOutput>> GetListAsync(IdentitySecurityLogInput input)
    {
        return Service.GetListAsync(input);
    }

}
