namespace TestWorkshop;

public interface ISystemAuditLogAppService : IApplicationService
{

    /// <summary>
    /// 分页查询审计日志
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<AuditLogOutput>> GetListAsync(AuditLogInput input);

}
