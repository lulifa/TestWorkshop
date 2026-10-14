namespace TestWorkshop;

/// <summary>
/// 报警管理
/// </summary>
[Route("api/workshop/alarm")]
public class WorkshopAlarmController : TestWorkshopController
{
    private readonly IWorkshopAlarmAppService Service;

    public WorkshopAlarmController(IWorkshopAlarmAppService alarmAppService)
    {
        Service = alarmAppService;
    }

    /// <summary>
    /// 分页查询报警（按当前用户可访问的车间过滤）
    /// </summary>
    [HttpGet]
    public virtual async Task<PagedResultDto<WorkshopAlarmDto>> GetListAsync(WorkshopAlarmListInput input)
    {
        return await Service.GetListAsync(input);
    }
}
