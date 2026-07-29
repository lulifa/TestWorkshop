namespace TestWorkshop;

public interface ISystemSecurityLogAppService : IApplicationService
{

    /// <summary>
    /// 分页查询登录日志
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<IdentitySecurityLogOutput>> GetListAsync(IdentitySecurityLogInput input);

}
