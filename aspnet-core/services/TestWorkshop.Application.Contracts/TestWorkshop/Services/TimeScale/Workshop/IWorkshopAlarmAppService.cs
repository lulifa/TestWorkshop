namespace TestWorkshop;

public interface IWorkshopAlarmAppService : IApplicationService
{
    /// <summary>
    /// 分页查询全部报警（所有登录用户可见）
    /// </summary>
    Task<PagedResultDto<WorkshopAlarmDto>> GetListAsync(WorkshopAlarmListInput input);
}
