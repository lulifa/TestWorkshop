namespace TestWorkshop.TimeScale;

public interface IWorkshopDeviceHeartbeatRepository : IBasicRepository<WorkshopDeviceHeartbeat, Guid>
{
    /// <summary>
    /// 按设备编码查询心跳状态，不存在返回 null
    /// </summary>
    Task<WorkshopDeviceHeartbeat> FindByDeviceCodeAsync(string deviceCode);
}
