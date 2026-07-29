namespace TestWorkshop;

public class SystemSecurityLogAppService : TestWorkshopAppService, ISystemSecurityLogAppService
{

    protected IIdentitySecurityLogRepository IdentitySecurityLogRepository { get; }

    public SystemSecurityLogAppService(IIdentitySecurityLogRepository identitySecurityLogRepository)
    {
        IdentitySecurityLogRepository = identitySecurityLogRepository;
    }

    /// <summary>
    /// 分页查询安全日志
    /// </summary>
    [Authorize(Policy = TestWorkshopPermissions.SecurityLog.Default)]
    public virtual async Task<PagedResultDto<IdentitySecurityLogOutput>> GetListAsync(IdentitySecurityLogInput input)
    {
        var totalCount = await IdentitySecurityLogRepository.GetCountAsync(
            input.StartTime,
            input.EndTime,
            input.ApplicationName,
            input.Identity,
            input.ActionName,
            input.UserId,
            input.UserName,
            input.ClientId,
            input.CorrelationId,
            input.ClientIpAddress);
        if (totalCount == 0)
        {
            return new PagedResultDto<IdentitySecurityLogOutput>();
        }

        var list = await IdentitySecurityLogRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.StartTime,
            input.EndTime,
            input.ApplicationName,
            input.Identity,
            input.ActionName,
            input.UserId,
            input.UserName,
            input.ClientId,
            input.CorrelationId,
            input.ClientIpAddress);

        var items = ObjectMapper.Map<List<IdentitySecurityLog>, List<IdentitySecurityLogOutput>>(list);

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.ClientIpAddress))
            {
                item.ClientIpAddress = IpLocationHelper.GetIpWithLocation(item.ClientIpAddress);
                item.BrowserInfo = IpLocationHelper.GetBrowserInfo(item.BrowserInfo);
            }
        }

        return new PagedResultDto<IdentitySecurityLogOutput>(totalCount, items);
    }

}
