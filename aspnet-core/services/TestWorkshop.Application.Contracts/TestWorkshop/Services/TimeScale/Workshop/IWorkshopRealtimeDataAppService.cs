namespace TestWorkshop;

public interface IWorkshopRealtimeDataAppService : IApplicationService
{
    /// <summary>
    /// 下位机推送一帧测试台实时数据
    /// </summary>
    Task PushAsync(WorkshopRealtimeDataPushInput input);
}
