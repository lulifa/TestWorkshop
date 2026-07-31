namespace TestWorkshop;

/// <summary>
/// 审计日志
/// </summary>
[Route("api/system/auditlog")]
public class SystemAuditLogController : TestWorkshopController, ISystemAuditLogAppService
{
    protected ISystemAuditLogAppService Service { get; }

    public SystemAuditLogController(ISystemAuditLogAppService service)
    {
        Service = service;
    }

    [HttpGet]
    [Route("{id}")]
    public Task<AuditLogOutput> GetAsync(Guid id)
    {
        return Service.GetAsync(id);
    }

    [HttpGet]
    public Task<PagedResultDto<AuditLogOutput>> GetListAsync(AuditLogInput input)
    {
        return Service.GetListAsync(input);
    }
}
