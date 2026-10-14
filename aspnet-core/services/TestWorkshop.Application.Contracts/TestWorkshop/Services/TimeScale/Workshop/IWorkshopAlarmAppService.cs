namespace TestWorkshop;

public interface IWorkshopAlarmAppService : IApplicationService
{
    /// <summary>
    /// 分页查询报警（按当前用户可访问的车间过滤）
    /// </summary>
    Task<PagedResultDto<WorkshopAlarmDto>> GetListAsync(WorkshopAlarmListInput input);
}
