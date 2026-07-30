using Volo.Abp.AuditLogging;

namespace TestWorkshop;

[Authorize(Policy = TestWorkshopPermissions.AuditLog.Default)]
public class SystemAuditLogAppService : TestWorkshopAppService, ISystemAuditLogAppService
{

    protected IAuditLogRepository AuditLogRepository { get; }

    public SystemAuditLogAppService(IAuditLogRepository auditLogRepository)
    {
        AuditLogRepository = auditLogRepository;
    }


    /// <summary>
    /// 分页查询审计日志
    /// </summary>
    public virtual async Task<PagedResultDto<AuditLogOutput>> GetListAsync(AuditLogInput input)
    {
        var totalCount = await AuditLogRepository.GetCountAsync(
            input.StartTime,
            input.EndTime,
            input.HttpMethod,
            input.Url,
            null,
            input.UserId,
            input.UserName,
            input.ApplicationName,
            input.ClientIpAddress,
            input.CorrelationId,
            input.MaxExecutionDuration,
            input.MinExecutionDuration,
            input.HasException,
            input.HttpStatusCode);

        if (totalCount == 0)
        {
            return new PagedResultDto<AuditLogOutput>();
        }

        if (!input.IsPaged)
        {
            input.SkipCount = 0;
            input.MaxResultCount = int.MaxValue;
        }

        var list = await AuditLogRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.StartTime,
            input.EndTime,
            input.HttpMethod,
            input.Url,
            null,
            input.UserId,
            input.UserName,
            input.ApplicationName,
            input.ClientIpAddress,
            input.CorrelationId,
            input.MaxExecutionDuration,
            input.MinExecutionDuration,
            input.HasException,
            input.HttpStatusCode,
            true);

        var items = ObjectMapper.Map<List<AuditLog>, List<AuditLogOutput>>(list);

        return new PagedResultDto<AuditLogOutput>(totalCount, items);
    }


}
