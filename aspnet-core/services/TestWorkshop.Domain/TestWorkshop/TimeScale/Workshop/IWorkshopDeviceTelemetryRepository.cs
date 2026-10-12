namespace TestWorkshop.TimeScale;

/// <summary>
/// 设备遥测数据仓储（TimescaleDB 超级表，非 ABP 聚合根，无单主键）
/// </summary>
public interface IWorkshopDeviceTelemetryRepository
{
    /// <summary>
    /// 按任务ID集合获取遥测波形数据
    /// </summary>
    Task<List<WorkshopDeviceTelemetry>> GetListByTaskIdsAsync(List<long> taskIds);
}
