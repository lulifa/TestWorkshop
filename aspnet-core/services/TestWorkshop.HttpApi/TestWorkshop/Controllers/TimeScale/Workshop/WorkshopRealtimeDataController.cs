namespace TestWorkshop;

/// <summary>
/// 测试台实时数据接收
/// </summary>
[Route("api/workshop/realtime-data")]
public class WorkshopRealtimeDataController : TestWorkshopController
{
    private readonly IWorkshopRealtimeDataAppService Service;

    public WorkshopRealtimeDataController(IWorkshopRealtimeDataAppService realtimeDataAppService)
    {
        Service = realtimeDataAppService;
    }

    /// <summary>
    /// 下位机推送一帧测试台实时数据
    /// </summary>
    [HttpPost]
    public virtual async Task PushAsync(WorkshopRealtimeDataPushInput input)
    {
        await Service.PushAsync(input);
    }

    /// <summary>
    /// 获取实时数据分析页所需的在线设备指标。
    /// </summary>
    [HttpGet("dashboard")]
    public virtual async Task<WorkshopRealtimeDashboardDto> GetDashboardAsync()
    {
        return await Service.GetDashboardAsync();
    }
}
