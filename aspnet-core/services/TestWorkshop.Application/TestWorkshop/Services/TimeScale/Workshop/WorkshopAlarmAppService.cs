namespace TestWorkshop;

/// <summary>
/// 报警列表应用服务。所有登录用户都可以查看全部报警。
/// </summary>
[Authorize]
public class WorkshopAlarmAppService : TestWorkshopAppService, IWorkshopAlarmAppService
{
    private readonly IWorkshopAlarmRepository _alarmRepository;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;

    public WorkshopAlarmAppService(
        IWorkshopAlarmRepository alarmRepository,
        IAsyncQueryableExecuter asyncQueryableExecuter)
    {
        _alarmRepository = alarmRepository;
        _asyncQueryableExecuter = asyncQueryableExecuter;
    }

    public virtual async Task<PagedResultDto<WorkshopAlarmDto>> GetListAsync(WorkshopAlarmListInput input)
    {
        if (input.MaxResultCount <= 0)
        {
            input.MaxResultCount = 20;
        }

        var query = await _alarmRepository.GetQueryableAsync();

        if (input.OrganizationUnitId.HasValue)
        {
            query = query.Where(x => x.OrganizationUnitId == input.OrganizationUnitId.Value);
        }

        if (input.AlarmType.HasValue)
        {
            query = query.Where(x => x.AlarmType == input.AlarmType.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        if (input.DeviceId.HasValue)
        {
            query = query.Where(x => x.DeviceId == input.DeviceId.Value);
        }

        if (input.StartTime.HasValue)
        {
            query = query.Where(x => x.StartedAt >= input.StartTime.Value);
        }

        if (input.EndTime.HasValue)
        {
            query = query.Where(x => x.StartedAt <= input.EndTime.Value);
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(query);
        var items = await _asyncQueryableExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.StartedAt)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<WorkshopAlarmDto>(
            totalCount,
            ObjectMapper.Map<List<WorkshopAlarm>, List<WorkshopAlarmDto>>(items));
    }
}
