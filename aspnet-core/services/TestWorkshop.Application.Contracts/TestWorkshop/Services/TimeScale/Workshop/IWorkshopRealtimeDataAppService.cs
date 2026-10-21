namespace TestWorkshop;

public interface IWorkshopRealtimeDataAppService : IApplicationService
{
    /// <summary>
    /// 下位机推送一帧测试台实时数据
    /// </summary>
    Task PushAsync(WorkshopRealtimeDataPushInput input);

    /// <summary>
    /// 获取实时数据分析页所需的在线设备指标。
    /// </summary>
    Task<WorkshopRealtimeDashboardDto> GetDashboardAsync();
}
