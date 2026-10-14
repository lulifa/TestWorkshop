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
}
