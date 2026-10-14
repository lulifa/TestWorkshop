namespace TestWorkshop.TimeScale;

/// <summary>
/// 测试台实时数据仓储（TimescaleDB 超级表，复合主键、非 ABP 聚合根）。
/// 调用方需要处于工作单元中：应用服务天然有；后台 Worker 请自行用 IUnitOfWorkManager 开启。
/// </summary>
public interface IWorkshopRealtimeDataRepository
{
    /// <summary>
    /// 写入一帧实时数据（按 设备+时间 冲突时覆盖）
    /// </summary>
    Task UpsertAsync(WorkshopRealtimeData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取每台测试台的最新一帧（报警判定用）
    /// </summary>
    Task<List<WorkshopRealtimeData>> GetLatestPerDeviceAsync(CancellationToken cancellationToken = default);
}
